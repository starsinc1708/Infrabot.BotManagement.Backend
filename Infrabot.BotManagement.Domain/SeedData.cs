using Infrabot.BotManagement.Domain.Enums;
using Infrabot.BotManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;

namespace Infrabot.BotManagement.Domain;

public static class SeedData
{
    public static async Task InitializeDatabase(this BotManagementDbContext context)
        {
            if (!await context.UpdateSettings.AnyAsync())
            {
                var defaultSettings = new List<UpdateSettings>
                {
                    new() { UpdateSource = 0, UpdateType = (UpdateType)1 },
                    new() { UpdateSource = 0, UpdateType = (UpdateType)4 },
                    new() { UpdateSource = 0, UpdateType = (UpdateType)5 },
                    new() { UpdateSource = 0, UpdateType = (UpdateType)12 },
                    
                    new() { UpdateSource = (UpdateSource)1, UpdateType = (UpdateType)4 },
                    new() { UpdateSource = (UpdateSource)1, UpdateType = (UpdateType)6 },
                    new() { UpdateSource = (UpdateSource)1, UpdateType = (UpdateType)7 },
                    new() { UpdateSource = (UpdateSource)1, UpdateType = (UpdateType)12 },
                    new() { UpdateSource = (UpdateSource)1, UpdateType = (UpdateType)13 },
                    new() { UpdateSource = (UpdateSource)1, UpdateType = (UpdateType)14 },
                    new() { UpdateSource = (UpdateSource)1, UpdateType = (UpdateType)16 },
                    new() { UpdateSource = (UpdateSource)1, UpdateType = (UpdateType)17 },
                    new() { UpdateSource = (UpdateSource)1, UpdateType = (UpdateType)18 },
                    
                    new() { UpdateSource = (UpdateSource)2, UpdateType = (UpdateType)4 },
                    new() { UpdateSource = (UpdateSource)2, UpdateType = (UpdateType)6 },
                    new() { UpdateSource = (UpdateSource)2, UpdateType = (UpdateType)7 },
                    new() { UpdateSource = (UpdateSource)2, UpdateType = (UpdateType)12 },
                    new() { UpdateSource = (UpdateSource)2, UpdateType = (UpdateType)13 },
                    new() { UpdateSource = (UpdateSource)2, UpdateType = (UpdateType)14 },
                    new() { UpdateSource = (UpdateSource)2, UpdateType = (UpdateType)16 },
                    new() { UpdateSource = (UpdateSource)2, UpdateType = (UpdateType)17 },
                    new() { UpdateSource = (UpdateSource)2, UpdateType = (UpdateType)18 },
                    
                    new() { UpdateSource = (UpdateSource)3, UpdateType = (UpdateType)4 },
                    new() { UpdateSource = (UpdateSource)3, UpdateType = (UpdateType)6 },
                    new() { UpdateSource = (UpdateSource)3, UpdateType = (UpdateType)7 },
                    new() { UpdateSource = (UpdateSource)3, UpdateType = (UpdateType)12 },
                    new() { UpdateSource = (UpdateSource)3, UpdateType = (UpdateType)13 },
                    new() { UpdateSource = (UpdateSource)3, UpdateType = (UpdateType)14 },
                    new() { UpdateSource = (UpdateSource)3, UpdateType = (UpdateType)16 },
                    new() { UpdateSource = (UpdateSource)3, UpdateType = (UpdateType)17 },
                    new() { UpdateSource = (UpdateSource)3, UpdateType = (UpdateType)18 },
                    
                    new() { UpdateSource = (UpdateSource)4, UpdateType = (UpdateType)19 },
                    new() { UpdateSource = (UpdateSource)4, UpdateType = (UpdateType)20 },
                    new() { UpdateSource = (UpdateSource)4, UpdateType = (UpdateType)21 },
                    new() { UpdateSource = (UpdateSource)4, UpdateType = (UpdateType)22 },
                    
                    new() { UpdateSource = (UpdateSource)5, UpdateType = (UpdateType)2,},
                    new() { UpdateSource = (UpdateSource)5, UpdateType = (UpdateType)3,},
                    
                    new() { UpdateSource = (UpdateSource)6, UpdateType = (UpdateType)8,},
                    new() { UpdateSource = (UpdateSource)6, UpdateType = (UpdateType)9 },
                    new() { UpdateSource = (UpdateSource)6, UpdateType = (UpdateType)23 },
                    
                    new() { UpdateSource = (UpdateSource)7, UpdateType = (UpdateType)10 },
                    new() { UpdateSource = (UpdateSource)7, UpdateType = (UpdateType)11 },
                    
                    new() { UpdateSource = (UpdateSource)8, UpdateType = 0 }
                };

                await context.UpdateSettings.AddRangeAsync(defaultSettings);
                await context.SaveChangesAsync();
            }
        }
}