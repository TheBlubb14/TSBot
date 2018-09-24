using System;
using System.Linq;
using System.Text;
using Telegram.Bot;
using TS3Client;
using TS3Client.Full;
using TS3Client.Messages;
using TSBot.DB;

namespace TSBot
{
    class Program
    {
        private static Ts3FullClient client;
        private static ConnectionDataFull connectionData;
        private static TelegramBotClient bot;
        private static DatabaseContext database;
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

            InitDatabase();
            Console.WriteLine("Loaded DB");

            InitTelegram();
            Console.WriteLine("Loaded Telegram Bot");

            InitTS();
            Console.WriteLine("Loaded TS");


            Console.ReadLine();
            //ListUsers();
            DisposeTelegram();
            DisposeTS();
            Console.ReadLine();

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
            var msg = e.Message.Text.Trim();
            var onlineCommand = "/online";
            var onlineCommandWithPassword = $"/online {config.Secret.BotPassword}".Trim();

            if (e.Message.Text.StartsWith(onlineCommand, StringComparison.OrdinalIgnoreCase))
            {
                if (e.Message.Text.Equals(onlineCommandWithPassword, StringComparison.OrdinalIgnoreCase))
                {
                    var builder = new StringBuilder();
                    foreach (var item in ListUsers().GroupBy(x => client.ChannelInfo(x.ChannelId).Unwrap().Name))
                    {
                        builder.AppendLine($"{item.Key}:");
                        item.ForEach(x => builder.AppendLine(x.Name));
                        builder.AppendLine("");
                    }

                    bot.SendTextMessageAsync(e.Message.Chat.Id, builder.ToString().TrimEnd('\r', '\n'));
                }
                else
                {
                    bot.SendTextMessageAsync(e.Message.Chat.Id, "Falsches Passwort übergeben.");
                }
            }
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

            if (e.Message.Equals("!show"))
            {
                var user = database.TSUser.FirstOrDefault(x => x.UID == e.InvokerUid);

                if (user == null)
                    user = database.TSUser.Add(new TSUser(e.InvokerUid, e.InvokerName)).Entity;

                if (user.Accepted)
                {
                    client.SendMessage("Du schon hast akzeptiert. Mit !no kannst du wiedersprechen.", TextMessageTargetMode.Private, e.InvokerId);
                }
                else if (!user.Accepted)
                {
                    client.SendMessage("Du wirst von dem BlubbBot aufgezeichnet. Du trittst alle deine Rechte an den Bot ab." + Environment.NewLine +
                        "Bestätige dies mit !yes oder verneine es mit !no", TextMessageTargetMode.Private, e.InvokerId);
                }

                database.SaveChanges();
            }
            else if (e.Message.Equals("!yes"))
            {
                var user = database.TSUser.FirstOrDefault(x => x.UID == e.InvokerUid);

                if (user == null)
                    return;

                if (user.Accepted)
                {
                    client.SendMessage("Du schon hast akzeptiert. Mit !no kannst du wiedersprechen.", TextMessageTargetMode.Private, e.InvokerId);
                }
                else
                {
                    user.Accepted = true;
                    client.SendMessage("Du hast akzeptiert. Mit !no kannst du wiedersprechen.", TextMessageTargetMode.Private, e.InvokerId);
                }

                database.SaveChanges();
            }
            else if (e.Message.Equals("!no"))
            {
                var user = database.TSUser.FirstOrDefault(x => x.UID == e.InvokerUid);

                if (user == null)
                    return;

                if (user.Accepted)
                {
                    
                client.SendMessage("Du dein Einverständnis widersprochen.", TextMessageTargetMode.Private, e.InvokerId);
                }
                else
                {
                    client.SendMessage("Du hast wiedersprochen.", TextMessageTargetMode.Private, e.InvokerId);
                }

                database.TSUser.Remove(user);
                database.SaveChanges();
            }

            Console.WriteLine($"[Message] {e.InvokerName}: {e.Message}");
        }

        private static IOrderedEnumerable<ClientData> ListUsers()
        {
            var clientList = client.ClientList();

            if (clientList.Ok)
            {
                return clientList
                    .Unwrap()
                    .Where(x => x.ClientType != ClientType.Query)
                    //.Where(x => database.TSUser.Any(y => y.Accepted && y.UID == x.Uid))
                    .OrderBy(x => x.ChannelId);
            }
            else
            {
                return default;
            }
        }
    }
}
