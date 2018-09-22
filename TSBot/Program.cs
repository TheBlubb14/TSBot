using System;
using System.Linq;
using TS3Client;
using TS3Client.Full;

namespace TSBot
{
    class Program
    {
        private static Ts3FullClient client;
        private static ConnectionDataFull connectionData;
        private static readonly Config config = new Config(@"..\..\..\secret.json");

        static void Main(string[] args)
        {
            while (!config.Load())
            {
                config.Save();
                Console.WriteLine($"Please edit the config under: {config.Path}");
                Console.WriteLine("Press any key to continiue..");
                Console.ReadKey();
            }

            Console.WriteLine("Config successfully loaded");

            var identityData = Ts3Crypt.DeobfuscateAndImportTs3Identity(config.Secret.TSIdentity);

            if (!string.IsNullOrEmpty(identityData.Error))
            {
                Console.WriteLine(identityData.Error);
            }
            else
            {
                connectionData = new ConnectionDataFull()
                {
                    Identity = identityData.Unwrap()
                };

                if (!string.IsNullOrEmpty(config.Secret.TSAddress))
                    connectionData.Address = config.Secret.TSAddress;

                if (!string.IsNullOrEmpty(config.Secret.TSServerPassword))
                    connectionData.ServerPassword = config.Secret.TSServerPassword;

                if (!string.IsNullOrEmpty(config.Secret.TSUsername))
                    connectionData.Username = config.Secret.TSUsername;

                if (!string.IsNullOrEmpty(config.Secret.TSDefaultChannel))
                    connectionData.DefaultChannel = config.Secret.TSDefaultChannel;

                if (!string.IsNullOrEmpty(config.Secret.TSDefaultChannelPassword))
                    connectionData.DefaultChannelPassword = config.Secret.TSDefaultChannelPassword;

                client = new Ts3FullClient(EventDispatchType.AutoThreadPooled);
                client.OnConnected += client_OnConnected;
                client.OnDisconnected += client_OnDisconnected;
                client.OnErrorEvent += client_OnErrorEvent;
                client.OnEachTextMessage += client_OnEachTextMessage;
                client.OnClientEnterView += Client_OnClientEnterView;
                client.Connect(connectionData);
            }

            Console.ReadLine();
            ListUsers();

            Console.ReadLine();
            client.Disconnect();
            client.Dispose();
        }

        private static void Client_OnClientEnterView(object sender, System.Collections.Generic.IEnumerable<TS3Client.Messages.ClientEnterView> e)
        {
            e.Where(x => x.ClientType != ClientType.Query)
                 .OrderBy(x => x.TargetChannelId)
                 .ForEach(x => Console.WriteLine($"[{x.TargetChannelId}] {x.Name}"));
        }


        private static void client_OnConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected");
        }

        private static void client_OnDisconnected(object sender, TS3Client.DisconnectEventArgs e)
        {
            Console.WriteLine("Disconnected");
        }

        private static void client_OnErrorEvent(object sender, TS3Client.Messages.CommandError e)
        {
            Console.WriteLine($"[Error] {e.Message}");
        }

        private static void client_OnEachTextMessage(object sender, TS3Client.Messages.TextMessage e)
        {
            Console.WriteLine($"[Message] {e.InvokerName}: {e.Message}");
        }

        private static void ListUsers()
        {
            var clientList = client.ClientList();

            if (clientList.Ok)
            {
                clientList
                    .Unwrap()
                    .Where(x => x.ClientType != ClientType.Query)
                    .OrderBy(x => x.ChannelId)
                    .ForEach(x => Console.WriteLine($"[{x.ChannelId}] {x.Name}"));
            }
        }
    }
}
