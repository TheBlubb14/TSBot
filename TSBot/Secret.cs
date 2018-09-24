namespace TSBot
{
    public sealed class Secret
    {
        public string TSIdentity { get; set; }

        public string TelegramAPIKey { get; set; }


        public string TSAddress { get; set; }

        public string TSServerPassword { get; set; }

        public string TSUsername { get; set; }

        public string TSDefaultChannel { get; set; }

        public string TSDefaultChannelPassword { get; set; }

        public string BotPassword { get; set; }

        public string DatabasePath { get; set; }
    }
}
