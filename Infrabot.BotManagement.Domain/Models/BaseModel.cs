using System.ComponentModel.DataAnnotations;

namespace Infrabot.BotManagement.Domain.Models
{
	public abstract class BaseModel
	{
		[Key]
		public long Id { get; set; }
	}
}
