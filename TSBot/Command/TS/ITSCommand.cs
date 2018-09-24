using System;
using TS3Client.Full;
using TS3Client.Messages;
using TSBot.DB;

namespace TSBot.Command.TS
{
    public interface ITSCommand : ICommand
    {
        Action<DatabaseContext, Ts3FullClient, TextMessage> Execute();
    }
}
