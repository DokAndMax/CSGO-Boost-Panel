using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static CSGO_Boost_Panel.CSGOIntercation;
using static CSGO_Boost_Panel.MainWindow;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Media;
using System.Diagnostics;

namespace CSGO_Boost_Panel
{
    class TgBot
    {
        private static TelegramBotClient botClient;
        public static bool BotIsOn = false;

        static readonly IEnumerable<IEnumerable<KeyboardButton>> keyboardRow = new KeyboardButton[][] {
            new KeyboardButton[] { "/screenshot", "/gather", "/playone" },
            new KeyboardButton[] { "/startsearch T1", "/startsearch T2", "/startsearch BOTH" },
            new KeyboardButton[] { "/notify", "/info", "/shutdown" } };
        static readonly IReplyMarkup rmu = new ReplyKeyboardMarkup(keyboardRow, true, false);

        public static bool TestApiKey(string key)
        {
            try
            {
                botClient = new TelegramBotClient(key);
                var me = botClient.GetMeAsync().Result;
                BotIsOn = true;
                return true;
            }
            catch (Exception ex) when (ex is AggregateException || ex is ArgumentException)
            {
                return false;
            }
        }

        public static void WaitingForCommand(string key)
        {
            botClient = new TelegramBotClient(key);
            botClient.OnMessage += BotOnMessageReceivedCatch;
            botClient.OnMessageEdited += BotOnMessageReceivedCatch;
            botClient.OnReceiveError += BotOnReceiveError;

            botClient.StartReceiving();

            SendNotify("Connected");
        }

        public static async void RemoveKeyboard()
        {
            await botClient.SendTextMessageAsync(
                chatId: MainWindow.settingsObj.Value<String>("chatID"),
                text: "Disconnected",
                replyMarkup: new ReplyKeyboardRemove()
                );
        }

        private static async void BotOnMessageReceivedCatch(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                await BotOnMessageReceived(sender, messageEventArgs);
            }
            catch(Exception ex)
            {
                try
                {
                    log.LogWrite("Received error: " + ex.HResult + " — " + ex.Message);
                    SendNotify("Received error: " + ex.HResult + " — " + ex.Message);
                }
                catch(Exception) { }
            }
        }   

        public async static void SendNotify(string message)
        {
            if (MainWindow.settingsObj.Value<String>("chatID") == null)
                return;
            await botClient.SendTextMessageAsync(
              chatId: MainWindow.settingsObj.Value<String>("chatID"),
              text: message,
              replyMarkup: rmu
              );
        }

        private static async Task BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            Message message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text || message.Date < DateTime.UtcNow.AddSeconds(-15))
                return;
            if(MainWindow.settingsObj.Value<String>("chatID") == null)
                MainWindow.settingsObj["chatID"] = message.Chat.Id;
            switch (message.Text.Split(' ').First())
            {
                // change notify settings
                case "/notify":
                    ChangeNotify(message);
                    break;

                // send a screenshot
                case "/screenshot":
                    await SendScreenshot(message);
                    break;

                // soon
                case "/startsearch":
                    if (!Check()) return;
                    await StartSearch(message);
                    SendNotify("ok");
                    break;

                // soon
                case "/gather":
                    if (!Check()) return;
                    await GatherLobby();
                    await Task.Delay(2000);
                    SendNotify("Lobby gathered");
                    await Task.Delay(1000);
                    await SendScreenshot(message);
                    break;

                //
                case "/playone":
                    if (!Check()) return;
                    botClient.OnMessage -= BotOnMessageReceivedCatch;
                    botClient.OnMessageEdited -= BotOnMessageReceivedCatch;
                    botClient.OnMessage += PlayOne;
                    botClient.OnMessageEdited += PlayOne;
                    IEnumerable<IEnumerable<KeyboardButton>> OneToTen = new KeyboardButton[][] {
                    new KeyboardButton[] { "1", "2", "3", "4", "5" },
                    new KeyboardButton[] { "6", "7", "8", "9", "10" },
                    new KeyboardButton[] { "All red" }};
                    await botClient.SendTextMessageAsync(
                        chatId: MainWindow.settingsObj.Value<String>("chatID"),
                        text: "Choose 1 - 10",
                        replyMarkup: new ReplyKeyboardMarkup(OneToTen, true, false)
                    );
                    break;

                // soon
                case "/info":
                    if (!Check()) return;
                    SendInfo(message);
                    break;

                //
                case "/shutdown":
                    var psi = new ProcessStartInfo("shutdown", "/sg /t 0")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(psi);
                    break;

                default:
                    await Usage(message);
                    break;
            }
        }

        private static bool Check()
        {
            if (!on)
            {
                SendNotify("Please, start boost first");
                return false;
            }
            return true;
        }

        private static async Task SendScreenshot(Message message)
        {
            Stream img = new MemoryStream();
            using (Bitmap bmpScreenCapture = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                {
                    g.CopyFromScreen(System.Windows.Forms.Screen.PrimaryScreen.Bounds.X,
                                     System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y,
                                     0, 0,
                                     bmpScreenCapture.Size,
                                     CopyPixelOperation.SourceCopy);

                    bmpScreenCapture.Save(img, ImageFormat.Png);
                    img.Position = 0;
                }
            }
            await botClient.SendDocumentAsync(
                chatId: message.Chat.Id,
                document: new InputOnlineFile(img, "screenshot.png")
            );
        }

        private static void ChangeNotify(Message message)
        {
            if (!MainWindow.settingsObj.Value<bool>("notifies"))
            {
                SendNotify("notify turns on");
                MainWindow.settingsObj["notifies"] = true;
                MainWindow.settingsObj["chatID"] = message.Chat.Id;
            }
            else
            {
                SendNotify("notify turns off");
                MainWindow.settingsObj["notifies"] = false;
            }
        }


        private static void SendInfo(Message message)
        {
            string info = "";
            string status;
            for (short i = 0; i < 10; i++)
            {
                if (string.IsNullOrEmpty(PArray[i].WindowTitle))
                    continue;
                if (PArray[i].Status == Red)
                    status = " 🔴 ";
                else
                    status = " 🟢 ";
                info += (i+1) + "\t" + status + "\t-\t" + PArray[i].WindowTitle + "\n";
            }
            info += "\n" + "AutoAccept:   " + AutoAcceptStatusCircle + "    Player Statistics:   " + PlayerStatusCircle;
            info += "\n" + "Games played for: game session - " + GamesPlayerForGameSession + "; app session - " + GamesPlayerForAppSession;
            SendNotify(info);
        }

        static async Task StartSearch(Message message)
        {
            if (message.Text.Split(' ').Length != 2)
            {
                SendNotify("/startsearch (T1, T2, BOTH)");
                return;
            }
            switch (message.Text.Split(' ')[1].ToLower())
            {
                case "t1":
                    await StartSearching(1);
                    break;
                case "t2":
                    await StartSearching(2);
                    break;
                case "both":
                    await StartSearching(3);
                    break;
            }
        }

        private static async void PlayOne(object sender, MessageEventArgs messageEventArgs)
        {
            Message message = messageEventArgs.Message;
            botClient.OnMessage -= PlayOne;
            botClient.OnMessageEdited -= PlayOne;
            botClient.OnMessage += BotOnMessageReceivedCatch;
            botClient.OnMessageEdited += BotOnMessageReceivedCatch;
            if (Int16.TryParse(message.Text.Split(' ')[0], out short result) && result < 11 && result > 0)
            {
                await RestartCSGO(result);
                SendNotify("ok");
            }
            else if(message.Text.ToLower() == "all red")
            {
                for (short i = 1; i < 11; i++)
                {
                    if (string.IsNullOrEmpty(PArray[i-1].WindowTitle))
                        continue;
                    if (PArray[i-1].Status == Red)
                        await RestartCSGO(i);
                }
                SendNotify("ok");
            }
            else
                await Usage(message);
        }

        static async Task Usage(Message message)
        {
            const string usage = "Використання:\n" +
                                    "/screenshot - зробити скріншот робочого столу\n" +
                                    "/gather - зібрати всіх ботів у два лобі в грі\n" +
                                    "/playone - перезапуск клієнта Steam і відповідно СSGO, якщо виникли проблеми\n" +
                                    "/startsearch (T1, T2, BOTH) - почати пошук\n" +
                                    "/notify - вимкнути / вимкнути сповіщення\n" +
                                    "/info - інформація про активну сесію бусту\n" +
                                    "/shutdown - вимнкути ПК";
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: usage,
                replyMarkup: rmu
            );
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            try
            {
                log.LogWrite("Received error: " + receiveErrorEventArgs.ApiRequestException.ErrorCode +
    " — " + receiveErrorEventArgs.ApiRequestException.Message);
                SendNotify("Received error: " + receiveErrorEventArgs.ApiRequestException.ErrorCode +
    " — " + receiveErrorEventArgs.ApiRequestException.Message);
            }
            catch (Exception) { }
        }
    }
}