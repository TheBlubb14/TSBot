using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using TS3Client;
using TS3Client.Full;
using TS3Client.Messages;
using TSBot.Command;
using TSBot.Command.Telegram;
using TSBot.Command.TS;
using TSBot.DB;
using TSBot.Extension;

namespace TSBot
{
    class Program
    {
        private static Ts3FullClient client;
        private static ConnectionDataFull connectionData;
        private static TelegramBotClient bot;
        private static DatabaseContext database;
        private static readonly Config config = new Config(@"..\..\..\secret.json");

        private static IEnumerable<ITSCommand> TSCommands;
        private static IEnumerable<ITelegramCommand> TelegramCommands;

        private static bool BotPasswordUsed = false;

        static void Main(string[] args)
        {
            while (!config.Load())
            {
                config.Save();
                Console.WriteLine($"Please edit the config under: {config.Path}");
                Console.WriteLine("Press any key to continiue..");
                Console.ReadKey();
            }

            BotPasswordUsed = !string.IsNullOrEmpty(config.Secret.BotPassword);
            Console.WriteLine("Config successfully loaded");

            InitDatabase();
            Console.WriteLine("Loaded DB");

            TelegramCommands = CommandHelper.GetCommands<ITelegramCommand>();
            TSCommands = CommandHelper.GetCommands<ITSCommand>();
            Console.WriteLine("Loaded Commands");

            InitTelegram();
            Console.WriteLine("Loaded Telegram Bot");

            InitTS();
            Console.WriteLine("Loaded TS");

            Console.ReadLine();
            
            DisposeTelegram();
            DisposeTS();
        }

        private static void InitDatabase()
        {
            if (string.IsNullOrEmpty(config.Secret.DatabasePath))
            {
                Console.WriteLine("DatabasePath is empty");
                return;
            }

            database = new DatabaseContext(config.Secret.DatabasePath);
            SQLitePCL.Batteries.Init();
            database.Database.EnsureCreated();
        }

        private static void InitTelegram()
        {
            bot = new TelegramBotClient(config.Secret.TelegramAPIKey);
            bot.OnMessage += Bot_OnMessage;

            bot.StartReceiving();
        }

        private static void DisposeTelegram()
        {
            bot.StopReceiving();
        }

        private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (!client.Connected)
                return;

            TelegramCommands.ForEach(x =>
            {
                if (e.Message.Text.Trim().StartsWith(x.BuildCommand() + (BotPasswordUsed ? " " + config.Secret.BotPassword : ""), StringComparison.OrdinalIgnoreCase))
                    x.Execute().Invoke(database, bot, e.Message, client);
            });
        }

        private static void InitTS()
        {
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

                client.OnEachClientMoved += Client_OnEachClientMoved;

                client.Connect(connectionData);
            }
        }

        private static void Client_OnEachClientMoved(object sender, ClientMoved e)
        {
        }

        public static void DisposeTS()
        {
            client.Disconnect();
            client.Dispose();
        }

        private static void Client_OnClientEnterView(object sender, System.Collections.Generic.IEnumerable<ClientEnterView> e)
        {
            e.Where(x => x.ClientType != ClientType.Query)
                 .OrderBy(x => x.TargetChannelId)
                 .ForEach(x => Console.WriteLine($"[{x.TargetChannelId}] {x.Name}"));
        }


        private static void client_OnConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected");
        }

        private static void client_OnDisconnected(object sender, DisconnectEventArgs e)
        {
            Console.WriteLine("Disconnected");
        }

        private static void client_OnErrorEvent(object sender, CommandError e)
        {
            Console.WriteLine($"[Error] {e.Message}");
        }

        private static void client_OnEachTextMessage(object sender, TextMessage e)
        {
            if (!(e.NotifyType == NotificationType.TextMessage && e.Target == TextMessageTargetMode.Private) || e.InvokerId == client.ClientId)
                return;

            TSCommands.ForEach(x =>
            {
                if (e.Message.Trim().StartsWith(x.BuildCommand(), StringComparison.OrdinalIgnoreCase))
                    x.Execute().Invoke(database, client, e);
            });

            Console.WriteLine($"[Message] {e.InvokerName}: {e.Message}");
        }
    }
}
