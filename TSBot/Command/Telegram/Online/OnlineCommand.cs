using System;
using System.Linq;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using TS3Client.Full;
using TSBot.DB;
using TSBot.Extension;
using TSBot.Extension.TSExtention;

namespace TSBot.Command.Telegram.Online
{
    public class OnlineCommand : ITelegramCommand
    {
        public string Command => "online";

        private readonly StringBuilder builder = new StringBuilder();

        public Action<DatabaseContext, TelegramBotClient, Message, Ts3FullClient> Execute()
        {
            return (db, bot, msg, client) =>
            {
                this.builder.Clear();

                var users = client
                .ListUsers()
                .Where(x => db.TSUser.Any(y => y.UID == x.Uid))
                .Select(x => (client: x, channel: client.ChannelInfo(x.ChannelId)))
                .Where(x => x.channel.Ok)
                .Select(x => (x.client, channelname: x.channel.Unwrap().Name))
                .GroupBy(x => x.channelname ?? "")
                .OrderBy(x => x.Key);

                if (users is null || users.Count() == 0)
                    this.builder.Append("Keiner online.");

                users.ForEach(x =>
                {
                    this.builder.AppendLine($"{x.Key}:");
                    x.ForEach(y => this.builder.AppendLine(y.client.Name));
                    this.builder.AppendLine("");
                });

                bot.SendTextMessageAsync(msg.Chat.Id, this.builder.ToString().TrimEnd('\r', '\n'));
            };
        }
    }
}
