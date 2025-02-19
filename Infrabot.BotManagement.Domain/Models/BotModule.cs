using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrabot.BotManagement.Domain.Models;

public class BotModule : BaseModel
{
    public required long BotId { get; set; }
    [JsonIgnore] public TelegramBot Bot { get; set; } = null!;
    public required long ModuleId { get; set; }
    public ProcessingModule Module { get; set; } = null!;
    
    public bool Enabled { get; set; }
}

public class BotModulesConfiguration : IEntityTypeConfiguration<BotModule>
{
    public void Configure(EntityTypeBuilder<BotModule> builder)
    {
        builder.HasOne(x => x.Module)
            .WithMany(m => m.TgBots)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);
			
        builder.HasOne(x => x.Bot)
            .WithMany(m => m.Modules)
            .HasForeignKey(x => x.BotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}