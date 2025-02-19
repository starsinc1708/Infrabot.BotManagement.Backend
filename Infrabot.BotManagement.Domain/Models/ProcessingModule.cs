using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrabot.BotManagement.Domain.Models
{
	public class ProcessingModule : BaseModel
	{
		public string? Name { get; set; }
		public string? IpAddress { get; set; }
		public int Port { get; set; }
		public string? HealthCheckEndpoint { get; set; }
		
		public List<ModuleUpdateSettings> UpdateSettings { get; set; } = [];
		
		[JsonIgnore]
		public List<BotModule> TgBots { get; set; } = [];
	}
	
	public class ProcessingModuleConfiguration : IEntityTypeConfiguration<ProcessingModule>
	{
		public void Configure(EntityTypeBuilder<ProcessingModule> builder)
		{
			builder.HasKey(x => x.Id);

			builder.HasMany(x => x.TgBots)
				.WithOne(m => m.Module)
				.OnDelete(DeleteBehavior.Cascade);
        
			builder.HasMany(x => x.UpdateSettings)
				.WithOne(m => m.Module)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
