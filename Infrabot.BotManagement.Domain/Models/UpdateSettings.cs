using Infrabot.BotManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Telegram.Bot.Types.Enums;

namespace Infrabot.BotManagement.Domain.Models;

public class UpdateSettings : BaseModel
{
    public required UpdateSource UpdateSource { get; set; }
    public required UpdateType UpdateType { get; set; }

    public List<ModuleUpdateSettings> ModuleSettings { get; set; } = [];
}

internal class UpdateSettingsConfiguration : IEntityTypeConfiguration<UpdateSettings>
{
    public void Configure(EntityTypeBuilder<UpdateSettings> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UpdateSource, x.UpdateType }).IsUnique();
    }
}