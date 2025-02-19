using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrabot.BotManagement.Domain.Models
{
	public class TelegramBot : BaseModel
	{
		public required string BotToken { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public bool CanJoinGroups { get; set; }
		public bool CanReadAllGroupMessages { get; set; }
		public bool SupportsInlineQueries { get; set; }
		public bool CanConnectToBusiness { get; set; }
		public bool HasMainWebApp { get; set; }
		
		public bool Enabled { get; set; }
		public List<BotModule> Modules { get; set; } = [];
	}

	public class TgBotInfoConfiguration : IEntityTypeConfiguration<TelegramBot>
	{
		public void Configure(EntityTypeBuilder<TelegramBot> builder)
		{
			builder.Property(x => x.Id).ValueGeneratedNever();
			
			builder.HasMany(p => p.Modules)
				.WithOne(m => m.Bot)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
