using Google.Protobuf.WellKnownTypes;
using Infrabot.BotManagement.Domain.DTOs.BotRequests;
using Infrabot.BotManagement.Domain.Grpc;
using Infrabot.BotManagement.Domain.Mappings;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Enum = System.Enum;

namespace Infrabot.BotManagement.ApiGateway.Endpoints;

public static class TelegramApiEndpoints
{
	public static void MapTelegramApiEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("/tgbot").WithTags("TelegramAPI");
		
		group.MapGet("/{botId:long}/getWebhookInfo", async (
			long botId,
			[FromServices] TelegramBotService.TelegramBotServiceClient botClient,
			CancellationToken cancellationToken) =>
		{
			try
			{
				var botsFromDB = await botClient.GetAllAsync(new Empty(), cancellationToken: cancellationToken);
				var botToken = botsFromDB.Bots.FirstOrDefault(b => b.Id == botId)?.Token ?? string.Empty;
				var bot = new TelegramBotClient(botToken, cancellationToken: cancellationToken);
				
				var webhookInfo = await bot.GetWebhookInfo(cancellationToken);
				return Results.Ok(webhookInfo);
			}
			catch (Exception ex)
			{
				return HandleTelegramError(ex);
			}
		});
		
		group.MapPost("/{botId:long}/setWebhook", async (
			long botId,
			[FromBody] SetWebhook.Request request,
			[FromServices] TelegramBotService.TelegramBotServiceClient botClient,
			CancellationToken cancellationToken) =>
		{
			try
			{
				var botsFromDB = await botClient.GetAllAsync(new Empty(), cancellationToken: cancellationToken);
				var botToken = botsFromDB.Bots.FirstOrDefault(b => b.Id == botId)?.Token ?? string.Empty;
				var bot = new TelegramBotClient(botToken, cancellationToken: cancellationToken);
				
				if (request.AllowedUpdates != null)
				{
					await bot.SetWebhook(
						url: request.Url,
						maxConnections: request.MaxConnections,
						allowedUpdates: request.AllowedUpdates.Select(au =>
						{
							Enum.TryParse(au, true, out UpdateType updateType);
							return updateType;
						}),
						dropPendingUpdates: request.DropPendingUpdates,
						cancellationToken: cancellationToken);
				}
				else
				{
					var webhookInfo = await bot.GetWebhookInfo(cancellationToken);
					await bot.SetWebhook(
						url: request.Url,
						maxConnections: request.MaxConnections,
						allowedUpdates: webhookInfo.AllowedUpdates,
						dropPendingUpdates: request.DropPendingUpdates,
						cancellationToken: cancellationToken);
				}

				return Results.Ok(new { status = "Webhook set successfully" });
			}
			catch (Exception ex)
			{
				return HandleTelegramError(ex);
			}
		});

		group.MapPost("/{botId:long}/getMe", async (
			long botId,
			[FromServices] TelegramBotService.TelegramBotServiceClient botClient,
			IConfiguration configuration,
			CancellationToken cancellationToken) =>
		{
			try
			{
				var botsFromDB = await botClient.GetAllAsync(new Empty(), cancellationToken: cancellationToken);
				var botToken = botsFromDB.Bots.FirstOrDefault(b => b.Id == botId)?.Token ?? string.Empty;
				var bot = new TelegramBotClient(botToken, cancellationToken: cancellationToken);
				
				var webhookSettings = await bot.GetWebhookInfo(cancellationToken);
				await bot.DeleteWebhook(true, cancellationToken: cancellationToken);
				await bot.DropPendingUpdates(cancellationToken: cancellationToken);
				
				var botInfo = await bot.GetMe(cancellationToken: cancellationToken);
				
				await bot.SetWebhook(
					url: webhookSettings.Url,
					maxConnections: webhookSettings.MaxConnections,
					allowedUpdates: webhookSettings.AllowedUpdates,
					ipAddress: webhookSettings.IpAddress,
					cancellationToken: cancellationToken);
				
				var tgBotInfoModel = botInfo.MapToModel(botToken);
				
				ArgumentNullException.ThrowIfNull(tgBotInfoModel);

				var dbBot = await botClient.GetByIdAsync(new BotIdRequest { BotId = botInfo.Id }, 
					cancellationToken: cancellationToken);

				if (dbBot.Bot == null)
				{
					await botClient.AddAsync(new CreateBotRequest { Bot = MapBot(tgBotInfoModel) }, 
						cancellationToken: cancellationToken);
				}
				else
				{
					await botClient.UpdateAsync(new CreateBotRequest { Bot = MapBot(tgBotInfoModel) },
						cancellationToken: cancellationToken);
				}
				
				return Results.Ok(tgBotInfoModel);
			}
			catch (Exception ex)
			{
				return Results.Problem(
					detail: ex.Message,
					statusCode: StatusCodes.Status500InternalServerError,
					title: "Failed to process getMe"
				);
			}
		});
	}
	
	private static IResult HandleTelegramError(Exception ex)
	{
		return Results.Problem(
			detail: ex.Message,
			statusCode: StatusCodes.Status500InternalServerError,
			title: "Telegram API Error"
		);
	}
	
	private static TelegramBot MapBot(Domain.Models.TelegramBot bot) => new TelegramBot
	{
		Id = bot.Id,
		Token = bot.BotToken,
		FirstName = bot.FirstName ?? string.Empty,
		LastName = bot.LastName ?? string.Empty,
		CanJoinGroups = bot.CanJoinGroups,
		CanReadAllGroupMessages = bot.CanReadAllGroupMessages,
		SupportsInlineQueries = bot.SupportsInlineQueries,
		CanConnectToBusiness = bot.CanConnectToBusiness,
		HasMainWebApp = bot.HasMainWebApp,
		Enabled = bot.Enabled
	};
}