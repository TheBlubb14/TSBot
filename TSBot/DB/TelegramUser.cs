using System.ComponentModel.DataAnnotations;

namespace TSBot.DB
{
    public class TelegramUser
    {
        [Key]
        public long ChatID { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
