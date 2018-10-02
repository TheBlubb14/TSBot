using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using TS3Client.Full;
using TSBot.DB;

namespace TSBot.Command.Telegram
{
    public interface  ITelegramCommand : ICommand
    {
        Action<DatabaseContext, TelegramBotClient, Message, Ts3FullClient> Execute();
    }
}
