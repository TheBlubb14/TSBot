using System;
using TS3Client.Full;
using TS3Client.Messages;
using TSBot.DB;
using TSBot.Properties;

namespace TSBot.Command.TS.Tracker
{
    public class YesCommand : ITSCommand
    {
        public string Command => "yes";

        private NoCommand noCommand = new NoCommand();
        private string NoCommand => $"!{noCommand.Command}";

        public Action<DatabaseContext, Ts3FullClient, TextMessage> Execute()
        {
            return (db, client, msg) =>
            {
                var user = db.TSUser.Find(msg.InvokerUid);

                if (user is null)
                    return;

                if (user.Accepted)
                {
                    client.SendPrivateMessage(string.Format(Resources.TS_TRACKER_UserAlreadyAccepted, NoCommand), msg.InvokerId);
                }
                else
                {
                    user.Accepted = true;
                    client.SendPrivateMessage(string.Format(Resources.TS_TRACKER_UserAccepted, NoCommand), msg.InvokerId);
                }

                db.SaveChanges();
            };
        }
    }
}
