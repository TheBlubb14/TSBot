using System;
using TS3Client.Full;
using TS3Client.Messages;
using TSBot.DB;
using TSBot.Properties;

namespace TSBot.Command.TS.Tracker
{
    public class ShowCommand : ITSCommand
    {
        public string Command => "show";

        private NoCommand noCommand = new NoCommand();
        private YesCommand yesCommand = new YesCommand();
        private string NoCommand => $"!{noCommand.Command}";
        private string YesCommand => $"!{yesCommand.Command}";

        public Action<DatabaseContext, Ts3FullClient, TextMessage> Execute()
        {
            return (db, client, msg) =>
            {
                var user = db.TSUser.Find(msg.InvokerUid);

                if (user == null)
                    user = db.TSUser.Add(new TSUser(msg.InvokerUid, msg.InvokerName)).Entity;

                if (user.Accepted)
                {
                    client.SendPrivateMessage(string.Format(Resources.TS_TRACKER_UserAlreadyAccepted, NoCommand), msg.InvokerId);
                }
                else
                {
                    client.SendPrivateMessage(string.Format(Resources.TS_TRACKER_Message, YesCommand, NoCommand), msg.InvokerId);
                }

                db.SaveChanges();
            };
        }
    }
}
