using System;
using TS3Client.Full;
using TS3Client.Messages;
using TSBot.DB;
using TSBot.Properties;

namespace TSBot.Command.TS.Tracker
{
    public class NoCommand : ITSCommand
    {
        public string Command => "no";

        public Action<DatabaseContext, Ts3FullClient, TextMessage> Execute()
        {
            return (db, client, msg) =>
            {
                var user = db.TSUser.Find(msg.InvokerUid);

                if (user == null)
                    return;

                if (user.Accepted)
                {
                    client.SendPrivateMessage(Resources.TS_TRACKER_UserDeclined, msg.InvokerId);
                }
                else
                {
                    client.SendPrivateMessage(Resources.TS_TRACKER_UserAlreadyDeclined, msg.InvokerId);
                }

                db.TSUser.Remove(user);
                db.SaveChanges();
            };
        }
    }
}
