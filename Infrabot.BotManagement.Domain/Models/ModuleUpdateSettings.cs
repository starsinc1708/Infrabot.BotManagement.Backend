using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrabot.BotManagement.Domain.Models
{
	public class ModuleUpdateSettings : BaseModel
	{
		public required long UpdateSettingsId { get; set; }
		[JsonIgnore] public UpdateSettings? UpdateSettings { get; set; }

		public required long ModuleId { get; set; }
		[JsonIgnore] public ProcessingModule? Module { get; set; }
	}

	public class ModuleUpdateSettingsConfiguration : IEntityTypeConfiguration<ModuleUpdateSettings>
	{
		public void Configure(EntityTypeBuilder<ModuleUpdateSettings> builder)
		{
			builder.HasKey(x => x.Id);

			builder.HasOne(x => x.Module)
				.WithMany(m => m.UpdateSettings)
				.HasForeignKey(x => x.ModuleId)
				.OnDelete(DeleteBehavior.Cascade);
        
			builder.HasOne(x => x.UpdateSettings)
				.WithMany(m => m.ModuleSettings)
				.HasForeignKey(x => x.UpdateSettingsId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
