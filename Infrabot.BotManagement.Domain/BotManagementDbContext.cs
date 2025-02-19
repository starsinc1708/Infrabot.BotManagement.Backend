using Infrabot.BotManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.BotManagement.Domain
{
	public class BotManagementDbContext(DbContextOptions<BotManagementDbContext> options) : DbContext(options)
	{
		public DbSet<TelegramBot> Bots { get; set; }
		public DbSet<ProcessingModule> Modules { get; set; }
		public DbSet<UpdateSettings> UpdateSettings { get; set; }
		
		public DbSet<ModuleUpdateSettings> TgModuleUpdateSettings { get; set; }
		public DbSet<BotModule> TgBotModules { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(BotManagementDbContext).Assembly);
		}
	}
}
