using System.Linq;
using TS3Client;
using TS3Client.Full;
using TS3Client.Messages;

namespace TSBot.Extension.TSExtention
{
    public static class TSExtension
    {
        public static IOrderedEnumerable<ClientInfo> ListUsers(this Ts3FullClient client)
        {
            var clientList = client.ClientList();

            if (clientList.Ok)
            {
                return clientList
                   .Unwrap()
                   .Where(x => x.ClientType != ClientType.Query)
                   .Select(x => client.ClientInfo(x.ClientId))
                   .Where(x => x.Ok)
                   .Select(x => x.Unwrap())
                   .OrderBy(x => x.ChannelId);
            }
            else
            {
                return Enumerable.Empty<ClientInfo>()
                    .OrderBy(x => x);
            }
        }
    }
}
