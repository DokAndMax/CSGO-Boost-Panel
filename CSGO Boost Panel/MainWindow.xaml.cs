using CSGSI;
using CSGSI.Events;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace CSGO_Boost_Panel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class Item
    {
        public string Name { get; set; }
        public Item(string name)
        {
            this.Name = name;
        }
    }

    public partial class MainWindow : MetroWindow
    {
        public Version AssemblyVersion => Assembly.GetEntryAssembly().GetName().Version;
        public static readonly LogWriter log = new LogWriter("Start");
        public static string TgAPIKey;
        private readonly SoundPlayer mediaPlayer = new SoundPlayer();
        public static List<string> T1WinTitle = new List<string>(), T2WinTitle = new List<string>();
        public static JObject settingsObj, accInfo;
        public static string ActivePreset;
        public static short WinTeamNum, GamesPlayerForAppSession = 0, GamesPlayerForGameSession = 0;
        public static int LobbyCount = 0, RoundNumber = 0, DisNumber = 15, ZIndex = 0;
        public static bool on = false, freezetime = true, loaded = false, choosed = false, newRound = true, onemeth = true, connected = false;
        public static bool AutoAcceptRestartS = false, WarmUp = false, DisconnectActive = false;
        public static string AutoAcceptStatusCircle = "🔴", PlayerStatusCircle = "🔴";
        public List<string>[] TWinTitle = { T2WinTitle, T1WinTitle };
        public Button choosedObj;
        public static Brush Red = (Brush)new BrushConverter().ConvertFrom("#FFA20404");
        static public LobbyPlayer[] PArray = new LobbyPlayer[10];
        public MainWindow()
        {
            InitializeComponent();
            Loaded += LoadSettings;

            #if DEBUG
            TestBtn.Visibility = Visibility.Visible;
            #endif
            for (short i = 0; i < 10; i++)
                PArray[i] = new LobbyPlayer();
            TextBox[] Login = { Login1, Login2, Login3, Login4, Login5, Login6, Login7, Login8, Login9, Login10 };
            ToggleSwitch[] ToggleButton = { ToggleButton1, ToggleButton2, ToggleButton3, ToggleButton4, ToggleButton5, ToggleButton6, ToggleButton7, ToggleButton8, ToggleButton9, ToggleButton10 };
            System.Windows.Shapes.Ellipse[] Status = { Status1, Status2, Status3, Status4, Status5, Status6, Status7, Status8, Status9, Status10 };
            for (short i = 0; i < PArray.Length; i++)
            {
                Status[i].DataContext = PArray[i];
                Login[i].DataContext = PArray[i];
                ToggleButton[i].DataContext = PArray[i];
            }
            lobbiesList.DisplayMemberPath = "Name";
            lobbiesList.ItemsSource = _items;
            playersList.ItemsSource = _player;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (on)
                settingsObj["CSGOsRunning"] = true;
            File.WriteAllText("Settings.json", JsonConvert.SerializeObject(settingsObj, Formatting.Indented));
            TgBot.RemoveKeyboard();
            log.LogWrite("Exit");
            log.LogWrite(e.ExceptionObject.ToString() + "\n Crash");
        }

        private Point _dragStartPoint;
        private T FindVisualParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;
            if (parentObject is T parent)
                return parent;
            return FindVisualParent<T>(parentObject);
        }

        public static readonly IList<Item> _items = new ObservableCollection<Item>();
        private readonly IList<Player> _player = new ObservableCollection<Player>();

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(null);
            Vector diff = _dragStartPoint - point;
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var lbi = FindVisualParent<ListBoxItem>(((DependencyObject)e.OriginalSource));
                if (lbi != null)
                {
                    DragDrop.DoDragDrop(lbi, lbi.DataContext, DragDropEffects.Move);
                }
            }
        }
        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void ListBoxItem_Drop(object sender, DragEventArgs e)
        {
            if (sender is ListBoxItem item)
            {
                var source = e.Data.GetData(typeof(Item)) as Item;
                var target = item.DataContext as Item;

                int sourceIndex = lobbiesList.Items.IndexOf(source);
                int targetIndex = lobbiesList.Items.IndexOf(target);

                Move(source, sourceIndex, targetIndex);
            }
        }

        private void Move(Item source, int sourceIndex, int targetIndex)
        {
            if (sourceIndex < targetIndex)
            {
                _items.Insert(targetIndex + 1, source);
                _items.RemoveAt(sourceIndex);
            }
            else
            {
                int removeIndex = sourceIndex + 1;
                if (_items.Count + 1 > removeIndex)
                {
                    _items.Insert(targetIndex, source);
                    _items.RemoveAt(removeIndex);
                }
            }
            JObject lobbiesObj = new JObject();
            if ((JObject)JsonConvert.DeserializeObject(File.ReadAllText("Lobbies.json")) != null)
                lobbiesObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Lobbies.json"));
            JObject PresetNameObj = new JObject();
            for (int i = 0; i < _items.Count; i++)
                PresetNameObj.Add(_items[i].Name, lobbiesObj[_items[i].Name]);
            File.WriteAllText("Lobbies.json", JsonConvert.SerializeObject(PresetNameObj, Formatting.Indented));
        }

        private void Application_Exit(object sender, EventArgs e)
        {
            File.WriteAllText("Settings.json", JsonConvert.SerializeObject(settingsObj, Formatting.Indented));
            TgBot.RemoveKeyboard();
            log.LogWrite("Exit");
            Environment.Exit(0);
        }

        private void LoadSettings(object sender, RoutedEventArgs e)
        {
            if (File.Exists("Lobbies.json"))
            {
                JObject lobbiesObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Lobbies.json"));
                foreach (JProperty property in lobbiesObj.Properties())
                {
                    _items.Add(new Item(property.Name));
                    for (short i = 0; i < 10; i++)
                    {
                        _player.Add(new Player(property.Value.Value<JToken>("Acc" + (i + 1)).Value<string>("Login"), 
                            property.Value.Value<JToken>("Acc" + (i + 1)).Value<string>("Nickname"), 
                            property.Value.Value<JToken>("Acc" + (i + 1)).Value<short>("Level"), 
                            property.Value.Value<JToken>("Acc" + (i + 1)).Value<string>("XP"), 
                            "Images/" + (property.Value.Value<JToken>("Acc" + (i + 1)).Value<string>("Rank") ?? "0") + ".png", 
                            property.Name, "Collapsed"));
                    }
                    _player[_player.Count - 1].Visibility = "Visible";
                }
            }
            LobbyCount = _items.Count;
            if (!File.Exists("Settings.json"))
                File.WriteAllText("Settings.json", "{}");
            settingsObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Settings.json"));
            if (settingsObj.Property("SteamFolder") != null && !String.IsNullOrEmpty(settingsObj.Value<string>("SteamFolder")))
            {
                SteamFolder.Text = settingsObj.Value<string>("SteamFolder");
                LoadSteamAccs();
            }
            else if (GetSteamInstallPath() is string a)
            {
                SteamFolder.Text = a;
                settingsObj["SteamFolder"] = a;
                LoadSteamAccs();
            }

            if (settingsObj.Value<bool>("CSGOsRunning") == true)
            {
                CSGOsRunning.IsChecked = true;
                settingsObj["CSGOsRunning"] = false;
            }
            if (settingsObj.Property("CSGOFolder") != null)
                CSGOFolder.Text = settingsObj.Value<string>("CSGOFolder");
            if (settingsObj.Property("LeaderResX") != null)
                LeaderResX.Text = settingsObj.Value<string>("LeaderResX");
            if (settingsObj.Property("LeaderResY") != null)
                LeaderResY.Text = settingsObj.Value<string>("LeaderResY");
            if (settingsObj.Property("BotResX") != null)
                BotResX.Text = settingsObj.Value<string>("BotResX");
            if (settingsObj.Property("BotResY") != null)
                BotResY.Text = settingsObj.Value<string>("BotResY");
            if (settingsObj.Property("TgApi") != null)
                TgApi.Text = settingsObj.Value<string>("TgApi");
            ToggleSwitch[] BoolOptions = new ToggleSwitch[] { AutoAccept, AutoDisconnect, Sounds, MatchFoundSound, 
                MatchEndedSound, RoundLastsSound, LongDisconnect, FocusWindows, Automation };

            for (int i = 0; i < BoolOptions.Length; i++)
            {
                if (settingsObj.Property(BoolOptions[i].Name) != null)
                    BoolOptions[i].IsOn = settingsObj.Value<bool>(BoolOptions[i].Name);
                else
                    settingsObj[BoolOptions[i].Name] = BoolOptions[i].IsOn;
            }
            if (settingsObj.Property("WinTeam") != null)
            {
                switch (settingsObj.Value<short>("WinTeam"))
                {
                    case 0:
                        WinTeam1.IsChecked = true;
                        WinTeamNum = 0;
                        break;
                    case 1:
                        WinTeam2.IsChecked = true;
                        WinTeamNum = 1;
                        break;
                    case 2:
                        WinTeamTie.IsChecked = true;
                        WinTeamNum = 0;
                        break;
                }
            }
            loaded = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            grid1.Focus();
            lobbiesList.SelectedIndex = -1;
            AcceptGrid.Visibility = Visibility.Collapsed;
            SoundSettings.Visibility = Visibility.Collapsed;
            DisconnectSettings.Visibility = Visibility.Collapsed;
            SaveButton.IsEnabled = true;
            ZIndex = 0;
        }

        private void SelectHome(object sender, RoutedEventArgs e)
        {
            tab.SelectedIndex = 0;
        }

        private void SelectLobbiesList(object sender, RoutedEventArgs e)
        {
            tab.SelectedIndex = 1;
        }

        private void SelectPlayersStats(object sender, RoutedEventArgs e)
        {
            tab.SelectedIndex = 2;
        }

        private void SelectSettings(object sender, RoutedEventArgs e)
        {
            tab.SelectedIndex = 3;
        }

        public async void Start(object sender, RoutedEventArgs e)
        {
            if (on)
                return;
            if (!File.Exists("Settings.json"))
            {
                InfoMessage(sender, "Please specify Steam and CSGO folders", MessageBoxImage.Information);
                return;
            }
            else
            {
                if (string.IsNullOrEmpty(File.ReadAllText("Settings.json")))
                {
                    InfoMessage(sender, "Please specify Steam and CSGO folders",  MessageBoxImage.Information);
                    return;
                }
            }
            if (string.IsNullOrEmpty(settingsObj.Value<string>("CSGOFolder")) || string.IsNullOrEmpty(settingsObj.Value<string>("SteamFolder")))
            {
                InfoMessage(sender, "Please specify Steam and CSGO folders",  MessageBoxImage.Information);
                return;
            }
            List<String> Logins = new List<String>();
            T1WinTitle.Clear();
            T2WinTitle.Clear();
            PasswordBox[] Password = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
            string[] Names = { "LEADER", "BOT" }, Res = { LeaderResX.Text + " " + LeaderResY.Text, BotResX.Text + " " + BotResY.Text };
            for (short i = 0, n = 0, l = 0; i < 10; i++)
            {
                if (PArray[i].IsOn)
                {
                    if (string.IsNullOrEmpty(PArray[i].Login) || string.IsNullOrEmpty(Password[i].Password))
                    {
                        InfoMessage(sender, "Please type login or password",  MessageBoxImage.Information);
                        return;
                    }
                    if (accInfo[PArray[i].Login.ToLower()] == null)
                    {
                        LoadSteamAccs();
                        if (accInfo[PArray[i].Login.ToLower()] == null)
                        {
                            InfoMessage(sender, "First login to this account: \"" + PArray[i].Login + "\" and then try again",  MessageBoxImage.Information);
                            return;
                        }
                    }
                    if (i < 5)
                    {
                        Logins.Add(PArray[i].Login + " " + Password[i].Password + " " + PArray[i].Position + " " + Res[l] + " \"" + Names[l] + " #1\" " + n);
                        PArray[i].WindowTitle = "LOGIN: " + PArray[i].Login.ToLower() + " | " + Names[l] + " #1";
                        T1WinTitle.Add("LOGIN: " + PArray[i].Login.ToLower() + " | " + Names[l] + " #1");
                    }
                    else
                    {
                        Logins.Add(PArray[i].Login + " " + Password[i].Password + " " + PArray[i].Position + " " + Res[l] + " \"" + Names[l] + " #2\" " + (n + 2));
                        PArray[i].WindowTitle = "LOGIN: " + PArray[i].Login.ToLower() + " | " + Names[l] + " #2";
                        T2WinTitle.Add("LOGIN: " + PArray[i].Login.ToLower() + " | " + Names[l] + " #2");
                    }
                    if (n == 0)
                        n++;
                }
                if (l == 0)
                    l++;
                if (i == 4)
                {
                    n = 0;
                    l = 0;
                }
                PArray[i].Password = Password[i].Password;
            }
            if (Logins.Count < 1)
            {
                InfoMessage(sender, "Please turn on at least one account", MessageBoxImage.Information);
                return;
            }
            controlContainer.IsEnabled = false;
            ClearButton.IsEnabled = false;
            on = true;
            for (int i = 0; i < Logins.Count; i++)
            {
                if (CSGOsRunning.IsChecked == true)
                    continue;
                if (!on)
                    return;
                Process.Start("Launcher.exe", "false \"" + settingsObj["SteamFolder"].ToString() + "\" " + Logins[i] + " \"" + settingsObj["CSGOFolder"].ToString() + "\" ");
                await Task.Delay(4000);
            }
            if (!(bool)WinTeam1.IsChecked && !(bool)WinTeam2.IsChecked && !(bool)WinTeamTie.IsChecked)
                WinTeam1.IsChecked = true;
            if (!gslT1.Start())
                InfoMessage(sender, "Cannot start GameStateListener #1. AutoDisconnect won't work! Try reboot your PC", MessageBoxImage.Warning);
            if (!gslT2.Start())
                InfoMessage(sender, "Cannot start GameStateListener #2. AutoDisconnect won't work! Try reboot your PC", MessageBoxImage.Warning);
            _ = Task.Run(() => AutoAcceptFunc());
            CSGSILogic(true, true);
            AccountChecker();
            StatsUpdate();
            if (CSGOsRunning.IsChecked == true)
                CSGOsRunning.IsChecked = false;
            exChange.IsEnabled = true;
            AdditionOptions.IsEnabled = true;
        }
        private void InfoMessage(object sender, string message, MessageBoxImage type)
        {
            if (sender != null)
                MessageBox.Show(message, "", MessageBoxButton.OK, type);
            else
            {
                string emoji = "";
                if (type == MessageBoxImage.Information)
                {
                    emoji = "ℹ️  ";
                    TgBot.StartResult = false;
                }
                else if (type == MessageBoxImage.Warning)
                    emoji = "⚠️  ";
                TgBot.SendNotify(emoji + message);
            }
        }

        public void Stop(object sender, RoutedEventArgs e)
        {
            if (!controlContainer.IsEnabled)
            {
                JObject lobbiesObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Lobbies.json"));
                for (int i = 0; i < 10; i++)
                {
                    if (PArray[i].IsOn && WindowHelper.IsExist(PArray[i].WindowTitle))
                    {
                        int x;
                        WindowHelper.Rect WindowRect = new WindowHelper.Rect();
                        WindowHelper.GetRect(PArray[i].WindowTitle, ref WindowRect);
                        if (WindowRect.Left < 0)
                            x = 0;
                        else
                            x = WindowRect.Left;
                        if (!string.IsNullOrEmpty(ActivePreset))
                            lobbiesObj[ActivePreset]["Acc" + (i + 1)]["Pos"] = x + " " + WindowRect.Top;
                        PArray[i].Position = x + " " + WindowRect.Top;
                    }
                    PArray[i].WindowTitle = "";
                }
                if (!string.IsNullOrEmpty(ActivePreset))
                    File.WriteAllText("Lobbies.json", JsonConvert.SerializeObject(lobbiesObj, Formatting.Indented));
            }

            try
            {
                foreach (Process proc in Process.GetProcessesByName("csgo"))
                {
                    proc.Kill();
                }
                foreach (Process proc in Process.GetProcessesByName("steam"))
                {
                    proc.Kill();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            CSGSILogic(false, false);
            if (gslT1.Running)
                gslT1.Stop();
            if (gslT2.Running)
                gslT2.Stop();
            System.Windows.Shapes.Ellipse[] Status = { AutoAcceptStatus, PlayerStatus };
            PlayerStatusCircle = "🔴";
            AutoAcceptStatusCircle = "🔴";
            GamesPlayerForGameSession = 0;
            for (short i = 0; i < 2; i++)
                Status[i].Fill = Red;
            for (short i = 0; i < 10; i++)
                PArray[i].IsStarted = Red;
            on = false;
            WarmUp = false;
            connected = false;
            if (choosedObj != null)
                choosedObj.BorderBrush = null;
            choosed = false;
            controlContainer.IsEnabled = true;
            AdditionOptions.IsEnabled = false;
            exChange.IsEnabled = false;
            ClearButton.IsEnabled = true;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (!AcceptGrid.IsVisible)
                AcceptGrid.Visibility = Visibility.Visible;
            else
                AcceptGrid.Visibility = Visibility.Collapsed;
        }
        private void Accept(object sender, KeyEventArgs e)
        {
            if (settingsObj["CSGOFolder"] == null || settingsObj["SteamFolder"] == null)
            {
                MessageBox.Show("Please specify Steam and CSGO folders");
                return;
            }
            if (e.Key != Key.Enter) return;
            if (string.IsNullOrEmpty(PresetName.Text)) return;
            AcceptGrid.Visibility = Visibility.Collapsed;
            e.Handled = true;
            TextBox[] Login = { Login1, Login2, Login3, Login4, Login5, Login6, Login7, Login8, Login9, Login10 };
            PasswordBox[] Password = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
            ToggleSwitch[] ToggleButton = { ToggleButton1, ToggleButton2, ToggleButton3, ToggleButton4, ToggleButton5, ToggleButton6, ToggleButton7, ToggleButton8, ToggleButton9, ToggleButton10 };
            JObject lobbiesObj = new JObject();
            if (!File.Exists("Lobbies.json"))
                File.Create("Lobbies.json").Close();
            if ((JObject)JsonConvert.DeserializeObject(File.ReadAllText("Lobbies.json")) != null)
                lobbiesObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Lobbies.json"));
            LoadSteamAccs();

            if (lobbiesObj.Property(PresetName.Text) != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    lobbiesObj[PresetName.Text]["Acc" + (i + 1)]["Toggled"] = PArray[i].IsOn;
                    lobbiesObj[PresetName.Text]["Acc" + (i + 1)]["Login"] = PArray[i].Login.ToLower();
                    lobbiesObj[PresetName.Text]["Acc" + (i + 1)]["Password"] = Password[i].Password;
                    lobbiesObj[PresetName.Text]["Acc" + (i + 1)]["SteamID64"] = accInfo[Login[i].Text.ToLower()]?["SteamID"] ?? "Unknown";
                    lobbiesObj[PresetName.Text]["Acc" + (i + 1)]["Nickname"] = accInfo[Login[i].Text.ToLower()]?["Nickname"] ?? "Unknown";
                }
                MessageBox.Show("Preset successfully updated");
            }
            else
            {
                JObject PresetNameObj = new JObject();
                lobbiesObj.Add(PresetName.Text, PresetNameObj);
                for (int i = 0; i < 10; i++)
                {
                    PresetNameObj.Add("Acc" + (i + 1), new JObject(new JProperty("Toggled", ToggleButton[i].IsOn), new JProperty("Login", Login[i].Text.ToLower()), new JProperty("Password", Password[i].Password), new JProperty("Pos", PArray[i].Position), new JProperty("SteamID64", accInfo[Login[i].Text.ToLower()]?["SteamID"] ?? "Unknown"), new JProperty("Nickname", accInfo[Login[i].Text.ToLower()]?["Nickname"] ?? "Unknown"), new JProperty("Level", 0), new JProperty("XP", ""), new JProperty("Rank", "0")));
                }
                ActivePreset = PresetName.Text;
                for (short i = 0; i < 10; i++)
                {
                    _player.Add(new Player(lobbiesObj[ActivePreset].Value<JToken>("Acc" + (i + 1)).Value<string>("Login"), lobbiesObj[ActivePreset].Value<JToken>("Acc" + (i + 1)).Value<string>("Nickname"), lobbiesObj[ActivePreset].Value<JToken>("Acc" + (i + 1)).Value<short>("Level"), lobbiesObj[ActivePreset].Value<JToken>("Acc" + (i + 1)).Value<string>("XP"), "Images/" + (lobbiesObj[ActivePreset].Value<JToken>("Acc" + (i + 1)).Value<string>("Rank") ?? "0") + ".png", ActivePreset, "Collapsed"));
                }
                _player[_player.Count - 1].Visibility = "Visible";
                MessageBox.Show("Preset successfully saved");
            }
            LobbyCount = _items.Count;
            File.WriteAllText("Lobbies.json", JsonConvert.SerializeObject(lobbiesObj, Formatting.Indented));
            if (_items.Any(x => x.Name == PresetName.Text) == false)
                _items.Add(new Item(PresetName.Text));
        }

        private void ClearText(object sender, RoutedEventArgs e)
        {
            TextBox[] Login = { Login1, Login2, Login3, Login4, Login5, Login6, Login7, Login8, Login9, Login10 };
            PasswordBox[] Password = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
            for (short i = 0; i < 10; i++)
            {
                Login[i].Text = "";
                Password[i].Password = "";
            }
            ActivePreset = "";
            PresetName.Text = "";
        }
        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            _items.RemoveAt(lobbiesList.SelectedIndex);
            JObject lobbiesObj = new JObject();
            if ((JObject)JsonConvert.DeserializeObject(File.ReadAllText("Lobbies.json")) != null)
                lobbiesObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Lobbies.json"));
            JObject PresetNameObj = new JObject();
            for (int i = 0; i < _items.Count; i++)
                PresetNameObj.Add(_items[i].Name, lobbiesObj[_items[i].Name]);
            File.WriteAllText("Lobbies.json", JsonConvert.SerializeObject(PresetNameObj, Formatting.Indented));
            LobbyCount = _items.Count;
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Cancel)
                return;
            if (((Button)sender).Tag.ToString() == "SteamFolder")
            {
                if (!File.Exists(dialog.FileName + @"/config/loginusers.vdf"))
                {
                    MessageBox.Show("Wrong Steam Directory");
                    return;
                }
                SteamFolder.Text = dialog.FileName;
            }
            else
                CSGOFolder.Text = dialog.FileName;
            settingsObj[((Button)sender).Tag] = dialog.FileName;
        }

        private void SaveSettings(object sender, TextChangedEventArgs e)
        {
            if (!loaded)
                return;
            settingsObj[((TextBox)sender).Name] = ((TextBox)sender).Text;
        }

        private void SettingsTgl(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                return;
            settingsObj[((ToggleSwitch)sender).Name] = ((ToggleSwitch)sender).IsOn;
            if (((ToggleSwitch)sender).Name == "AutoDisconnect" || ((ToggleSwitch)sender).Name == "LongDisconnect")
            {
                if (AutoDisconnect.IsOn && on)
                {
                    Task.Run(() => CSGSILogic(true, false));
                }
                if (!AutoDisconnect.IsOn && on)
                {
                    Task.Run(() => CSGSILogic(false, false));
                }
            }
        }

        public void LoadPreset(int num)
        {
            LoadPreset(null, num);
        }
        private void LoadPreset(object sender, MouseButtonEventArgs e)
        {
            LoadPreset(sender, -1);
        }
        private void LoadPreset(object sender, int num)
        {
            int index;
            if (num == -1)
                index = lobbiesList.SelectedIndex;
            else
                index = num;
            if ((sender != null && lobbiesList.SelectedItem == null) || on)
                return;
            JObject lobbiesObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Lobbies.json"));
            JObject AccObj = lobbiesObj.Property(_items[index].Name).Value.ToObject<JObject>();
            PasswordBox[] Password = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
            ActivePreset = _items[index].Name;
            for (int i = 0; i < 10; i++)
            {
                PArray[i].IsOn = bool.Parse(AccObj["Acc" + (i + 1)].Value<string>("Toggled"));
                PArray[i].Login = AccObj["Acc" + (i + 1)].Value<string>("Login");
                Password[i].Password = AccObj["Acc" + (i + 1)].Value<string>("Password");
                PArray[i].Position = lobbiesObj[ActivePreset]["Acc" + (i + 1)]["Pos"].ToString();
            }
            PresetName.Text = ActivePreset;
            tab.SelectedIndex = 0;
        }

        private void LoadSteamAccs()
        {
            if (!File.Exists(settingsObj.Value<String>("SteamFolder") + @"/config/loginusers.vdf"))
            {
                return;
            }
            accInfo = new JObject();
            string info = File.ReadAllText(settingsObj.Value<String>("SteamFolder") + @"/config/loginusers.vdf");
            string[] Delimiters = { @"""0""", @"""1""", "\"", "\n", "\t", "{", "}", "},", "\",", "users", "AccountName", "PersonaName", "RememberPassword", "MostRecent", "mostrecent", "Timestamp", "WantsOfflineMode" };
            string[] b = info.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
            for (int a = 1; a <= (b.Length / 4); a++)
            {
                accInfo.Add(b[a * 4 - 3], new JObject(new JProperty("SteamID", b[a * 4 - 4]), new JProperty("Nickname", b[a * 4 - 2])));
            }
            File.WriteAllText("Loginusers.json", JsonConvert.SerializeObject(accInfo, Formatting.Indented));
        }

        private void OpenSteam(object sender, RoutedEventArgs e)
        {
            if (((TextBox)this.GetType().GetField(((Button)sender).Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Text == "")
            {
                MessageBox.Show("Type login");
                return;
            }
            if (accInfo[((TextBox)this.GetType().GetField(((Button)sender).Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Text.ToLower()] == null)
            {
                LoadSteamAccs();
                if (accInfo[((TextBox)this.GetType().GetField(((Button)sender).Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Text.ToLower()] == null)
                {
                    MessageBox.Show("Please, login to this account and try again");
                    return;
                }
            }
            Process.Start("https://steamcommunity.com/profiles/" + accInfo[((TextBox)this.GetType().GetField(((Button)sender).Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Text.ToLower()]["SteamID"] + "/");
        }

        private void AutoAcceptFunc()
        {
            if (!settingsObj.Value<bool>("AutoAccept") || !PArray[0].IsOn || !PArray[5].IsOn)
                return;
            Directory.CreateDirectory(settingsObj["CSGOFolder"].ToString() + @"\csgo\log\");
            Stream Team1logStream = File.Open(settingsObj["CSGOFolder"].ToString() + @"\csgo\log\0.log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            Stream Team2logStream = File.Open(settingsObj["CSGOFolder"].ToString() + @"\csgo\log\1.log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            StreamReader Team1log = new StreamReader(Team1logStream);
            StreamReader Team2log = new StreamReader(Team2logStream);
            InvokeUI(() =>
            {
                AutoAcceptStatus.Fill = Brushes.Green;
            });
            AutoAcceptStatusCircle = "🟢";
            while (on)
            {
                string Team1String = Regex.Match(Team1log.ReadToEnd(), @"match_id=.*$", RegexOptions.Multiline).Value;
                string Team2String = Regex.Match(Team2log.ReadToEnd(), @"match_id=.*$", RegexOptions.Multiline).Value;
                if (!string.IsNullOrEmpty(Team1String) && string.IsNullOrEmpty(Team2String))
                {
                    Thread.Sleep(250);
                    Team2String = Regex.Match(Team2log.ReadToEnd(), @"match_id=.*$", RegexOptions.Multiline).Value;
                }
                if (!string.IsNullOrEmpty(Team2String) && string.IsNullOrEmpty(Team1String))
                {
                    Thread.Sleep(250);
                    Team1String = Regex.Match(Team2log.ReadToEnd(), @"match_id=.*$", RegexOptions.Multiline).Value;
                }
                if (!string.IsNullOrEmpty(Team1String) && Team1String == Team2String)
                {
                    Thread.Sleep(3000);
                    AcceptGame();
                    Team1logStream.Close();
                    Team2logStream.Close();
                    Team1log.Close();
                    Team2log.Close();
                    break;
                }
                if (Team1String != Team2String && string.IsNullOrEmpty(Team1String) && !AutoAcceptRestartS)
                {
                    AutoAcceptRestartS = true;
                    RestartSearch(true);
                }
                if (Team1String != Team2String && string.IsNullOrEmpty(Team2String) && !AutoAcceptRestartS)
                {
                    AutoAcceptRestartS = true;
                    RestartSearch(false);
                }
                Thread.Sleep(650);
            }
        }
        private void CSGSILogic(bool enabled, bool indicators)
        {
            CSGSI.Nodes.RoundPhase roundPhase = CSGSI.Nodes.RoundPhase.Undefined;

            if (indicators)
            {
                gslT1.NewGameState -= Indicators;
                gslT2.NewGameState -= Indicators;
                gslT1.NewGameState += Indicators;
                gslT2.NewGameState += Indicators;
                gslT1.RoundPhaseChanged -= RoundLasts;
                gslT2.RoundPhaseChanged -= RoundLasts;
                gslT1.RoundPhaseChanged += RoundLasts;
                gslT2.RoundPhaseChanged += RoundLasts;
            }

            gslT1.NewGameState -= Round;
            gslT2.NewGameState -= Round;
            gslT1.NewGameState -= RoundLong;
            gslT2.NewGameState -= RoundLong;
            DisconnectActive = false;

            if (!enabled || !settingsObj.Value<bool>("AutoDisconnect") || !PArray[0].IsOn || !PArray[5].IsOn)
                return;

            if (settingsObj.Value<short>("WinTeam") == 2 && RoundNumber >= 15)
            {
                WinTeamNum = 1;
                WinTeam = gslT2;
            }

            if (settingsObj.Value<bool>("LongDisconnect"))
                WinTeam.NewGameState += RoundLong;
            else
                WinTeam.NewGameState += Round;

            async void Round(GameState a)
            {
                if ((RoundNumber == a.Map.Round && roundPhase == a.Round.Phase)
                    || a.Map.Phase == CSGSI.Nodes.MapPhase.Undefined
                    || a.Map.Phase == CSGSI.Nodes.MapPhase.GameOver)
                    return;
                else
                {
                    RoundNumber = a.Map.Round;
                    roundPhase = a.Round.Phase;
                }
                if (a.Map.Round == DisNumber)
                    DisconnectActive = false;

                if (a.Round.Phase == CSGSI.Nodes.RoundPhase.FreezeTime && (a.Map.Round == 0 || (a.Map.Round == 15 && settingsObj.Value<short>("WinTeam") == 2)))
                {
                    for (int i = 0; i < TWinTitle[WinTeamNum].Count; i++)
                    {
                        await Task.Delay(250);
                        WindowHelper.SendKey(TWinTitle[WinTeamNum][i], WindowHelper.VK_F10);
                        await Task.Delay(250);
                    }
                    return;
                }
                if (a.Map.Round == 14 && a.Round.Phase == CSGSI.Nodes.RoundPhase.FreezeTime && settingsObj.Value<short>("WinTeam") == 2)
                {
                    await Task.Delay(11000);
                    if (settingsObj.Value<bool>("LongDisconnect"))
                        return;
                    for (int i = 0; i < TWinTitle[WinTeamNum].Count; i++)
                    {
                        await Task.Delay(250);
                        WindowHelper.Click(TWinTitle[WinTeamNum][i], CSGOCoefficients.Reconnect);
                        await Task.Delay(250);
                    }
                    gslT1.NewGameState -= Round;
                    gslT2.NewGameState -= Round;
                    WinTeamNum = 1;
                    WinTeam = gslT2;
                    WinTeam.NewGameState += Round;
                    return;
                }

                if (!DisconnectActive && a.Round.Phase == CSGSI.Nodes.RoundPhase.FreezeTime
                && ((a.Map.Round != DisNumber
                && a.Map.Round != (DisNumber + 1)
                && a.Map.Round != 29)
                || (a.Map.Round == 15 && a.Map.Phase == CSGSI.Nodes.MapPhase.Live && settingsObj.Value<short>("WinTeam") != 2)))
                {
                    short num;
                    if (WinTeam == gslT1)
                        num = 1;
                    else
                        num = 0;
                    Stream TeamlogStream = File.Open(settingsObj["CSGOFolder"].ToString() + @"\csgo\log\" + num + ".log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    StreamReader Teamlog = new StreamReader(TeamlogStream);
                    DisconnectActive = true;
                    while (DisconnectActive)
                    {
                        short WinTeamNumTemp = WinTeamNum;
                        WindowHelper.Click(TWinTitle[WinTeamNumTemp][0], CSGOCoefficients.Reconnect);
                        while (true)
                        {
                            string TeamString = Teamlog.ReadToEnd();
                            if (!string.IsNullOrEmpty(Regex.Match(TeamString, @"ChangeGameUIState: CSGO_GAME_UI_STATE_LOADINGSCREEN -> CSGO_GAME_UI_STATE_INGAME", RegexOptions.Singleline).Value))
                                break;
                            await Task.Delay(250);
                        }
                        await Task.Delay(250);
                        WindowHelper.SendKey(TWinTitle[WinTeamNumTemp][0], WindowHelper.VK_F10);
                        await Task.Delay(1250);
                    }
                    TeamlogStream.Close();
                    Teamlog.Close();
                }
            }
            async void RoundLong(GameState a)
            {
                if ((RoundNumber == a.Map.Round && roundPhase == a.Round.Phase)
                   || a.Map.Phase == CSGSI.Nodes.MapPhase.Undefined
                   || a.Map.Phase == CSGSI.Nodes.MapPhase.GameOver
                   || a.Map.Phase == CSGSI.Nodes.MapPhase.Warmup)
                    return;
                else
                {
                    RoundNumber = a.Map.Round;
                    roundPhase = a.Round.Phase;
                }

                if (a.Map.Round == 5 || a.Map.Round == (DisNumber - 2) || a.Map.Round == 20)
                    DisconnectActive = false;

                if (a.Round.Phase == CSGSI.Nodes.RoundPhase.FreezeTime && (a.Map.Round == 0 || a.Map.Round == 7 || a.Map.Round == DisNumber || a.Map.Round == 22))
                {
                    for (int i = 0; i < TWinTitle[WinTeamNum].Count; i++)
                    {
                        await Task.Delay(250);
                        WindowHelper.SendKey(TWinTitle[WinTeamNum][i], WindowHelper.VK_F10);
                        await Task.Delay(250);
                    }
                    return;
                }
                if ((a.Map.Round == 6 || a.Map.Round == (DisNumber - 1) || a.Map.Round == 21) && a.Round.Phase == CSGSI.Nodes.RoundPhase.FreezeTime)
                {
                    await Task.Delay(10000);
                    if (!settingsObj.Value<bool>("LongDisconnect"))
                        return;
                    for (int i = 0; i < TWinTitle[WinTeamNum].Count; i++)
                    {
                        await Task.Delay(250);
                        WindowHelper.Click(TWinTitle[WinTeamNum][i], CSGOCoefficients.Reconnect);
                        await Task.Delay(250);
                    }
                    if (settingsObj.Value<short>("WinTeam") == 2)
                    {
                        if (a.Map.Round == 6 || a.Map.Round == 21)
                        {
                            gslT1.NewGameState -= RoundLong;
                            gslT2.NewGameState -= RoundLong;
                            WinTeamNum = 1;
                            WinTeam = gslT2;
                            WinTeam.NewGameState += RoundLong;
                        }
                        else
                        {
                            gslT1.NewGameState -= RoundLong;
                            gslT2.NewGameState -= RoundLong;
                            WinTeamNum = 0;
                            WinTeam = gslT1;
                            WinTeam.NewGameState += RoundLong;
                        }
                    }
                    return;
                }

                if (!DisconnectActive && a.Round.Phase == CSGSI.Nodes.RoundPhase.FreezeTime && a.Map.Round != 6 && a.Map.Round != 7 && a.Map.Round != (DisNumber - 1) && a.Map.Round != DisNumber && a.Map.Round != 21 && a.Map.Round != 22)
                {
                    short num;
                    if (WinTeam == gslT1)
                        num = 1;
                    else
                        num = 0;
                    Stream TeamlogStream = File.Open(settingsObj["CSGOFolder"].ToString() + @"\csgo\log\" + num + ".log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    StreamReader Teamlog = new StreamReader(TeamlogStream);
                    DisconnectActive = true;
                    while (DisconnectActive)
                    {
                        short WinTeamNumTemp = WinTeamNum;
                        await Task.Delay(1000);
                        WindowHelper.Click(TWinTitle[WinTeamNumTemp][0], CSGOCoefficients.Reconnect);
                        await Task.Delay(250);
                        while (true)
                        {
                            string TeamString = Teamlog.ReadToEnd();
                            if (!String.IsNullOrEmpty(Regex.Match(TeamString, @"ChangeGameUIState: CSGO_GAME_UI_STATE_LOADINGSCREEN -> CSGO_GAME_UI_STATE_INGAME", RegexOptions.Singleline).Value))
                                break;
                            await Task.Delay(250);
                        }
                        await Task.Delay(250);
                        WindowHelper.SendKey(TWinTitle[WinTeamNumTemp][0], WindowHelper.VK_F10);
                    }
                    TeamlogStream.Close();
                    Teamlog.Close();
                }
            }

            async void Indicators(GameState a)
            {
                if (a.Map.Phase == CSGSI.Nodes.MapPhase.GameOver & !WarmUp)
                {
                    WarmUp = true;
                    DisconnectActive = false;
                    newRound = false;
                    connected = false;
                    GamesPlayerForAppSession++; GamesPlayerForGameSession++;
                    _ = Task.Run(() => AutoAcceptFunc());
                    InvokeUI(() =>
                    {
                        StatsUpdate();
                    });


                    if (settingsObj.Value<bool>("AutoDisconnect"))
                    {
                        for (short b = 0; b < 2; b++)
                        {
                            for (short i = 0; i < TWinTitle[b].Count; i++)
                            {
                                await Task.Delay(200);
                                WindowHelper.SendKey(TWinTitle[b][i], WindowHelper.VK_F10);
                            }
                        }
                    }

                    if (settingsObj.Value<bool>("Sounds") && settingsObj.Value<bool>("MatchEndedSound"))
                    {
                        newRound = false;
                        mediaPlayer.Stream = Properties.Resources.MatchEnded;
                        mediaPlayer.Load();
                        mediaPlayer.Play();
                    }
                    if (MainWindow.settingsObj.Value<bool>("notifies"))
                        TgBot.SendNotify("Match ended (" + GamesPlayerForGameSession + "|" + GamesPlayerForAppSession + ")");
                    if (settingsObj.Value<short>("WinTeam") == 2 && settingsObj.Value<bool>("AutoDisconnect"))
                    {
                        gslT1.NewGameState -= Round;
                        gslT2.NewGameState -= Round;
                        gslT1.NewGameState -= RoundLong;
                        gslT2.NewGameState -= RoundLong;
                        WinTeam = gslT1;
                        WinTeamNum = 0;
                        if (settingsObj.Value<bool>("LongDisconnect"))
                            WinTeam.NewGameState += RoundLong;
                        else
                            WinTeam.NewGameState += Round;
                    }
                    if(settingsObj.Value<bool>("Automation"))
                    {
                        await CSGOIntercation.GatherLobby();
                        await CSGOIntercation.StartSearching(3);
                        await Task.Delay(10000);
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            if (PlayerStatus.Fill != Red)
                                TgBot.SendNotify("Check you lobbies, maybe not all bots entered the lobby");
                        });
                    }
                }

                if (a.Map.Phase == CSGSI.Nodes.MapPhase.Warmup && WarmUp)
                {
                    WarmUp = false;
                    if (settingsObj.Value<short>("WinTeam") == 2 && settingsObj.Value<bool>("AutoDisconnect"))
                    {
                        gslT1.NewGameState -= Round;
                        gslT2.NewGameState -= Round;
                        gslT1.NewGameState -= RoundLong;
                        gslT2.NewGameState -= RoundLong;
                        WinTeam = gslT1;
                        WinTeamNum = 0;
                        if (settingsObj.Value<bool>("LongDisconnect"))
                            WinTeam.NewGameState += RoundLong;
                        else
                            WinTeam.NewGameState += Round;
                    }
                }
            }

            async void RoundLasts(RoundPhaseChangedEventArgs a)
            {
                if (a.CurrentPhase == CSGSI.Nodes.RoundPhase.Live && onemeth)
                {
                    onemeth = false;
                    Stopwatch at = new Stopwatch();
                    newRound = true;
                    at.Start();
                    while (newRound)
                    {
                        if (at.Elapsed.TotalSeconds >= 35)
                        {
                            if (settingsObj.Value<bool>("Sounds") && settingsObj.Value<bool>("RoundLastsSound"))
                            {
                                mediaPlayer.Stream = Properties.Resources.RoundLasts;
                                mediaPlayer.Load();
                                mediaPlayer.Play();
                            }
                            if (MainWindow.settingsObj.Value<bool>("notifies"))
                                TgBot.SendNotify("Check your game! Round lasts more than 35 seconds");
                            break;
                        }
                        await Task.Delay(5000);
                    }
                    onemeth = true;
                    return;
                }
                if (a.CurrentPhase == CSGSI.Nodes.RoundPhase.FreezeTime)
                {
                    newRound = false;
                }
            }
        }

        private async void RestartSearch(bool t2)
        {
            InvokeUI(() =>
            {
                AutoAcceptStatus.Fill = Brushes.Yellow;
            });
            AutoAcceptStatusCircle = "🟡";
            List<String> ldrTitles = new List<String>();
            for (int i = 0; i < 10; i++)
            {
                if (string.IsNullOrEmpty(PArray[i].WindowTitle))
                    continue;
                if (PArray[i].WindowTitle.Contains("LEADER"))
                    ldrTitles.Add(PArray[i].WindowTitle);
            }
            if (t2)
                ldrTitles.Reverse();
            for (int i = 0; i < ldrTitles.Count; i++)
            {
                await Task.Delay(250);
                WindowHelper.Click(ldrTitles[i], CSGOCoefficients.GO);
                await Task.Delay(250);
            }
            await Task.Delay(55000);
            for (int i = 0; i < ldrTitles.Count; i++)
            {
                await Task.Delay(250);
                WindowHelper.Click(ldrTitles[i], CSGOCoefficients.GO);
                await Task.Delay(250);
            }
            AutoAcceptRestartS = false;
            InvokeUI(() =>
            {
                AutoAcceptStatus.Fill = Brushes.Green;
            });
            AutoAcceptStatusCircle = "🟢";
        }

        private void AcceptGame()
        {
            if (settingsObj.Value<bool>("Sounds") && settingsObj.Value<bool>("MatchFoundSound"))
            {
                mediaPlayer.Stream = Properties.Resources.MatchFound;
                mediaPlayer.Load();
                mediaPlayer.Play();
            }
            if (MainWindow.settingsObj.Value<bool>("notifies"))
                TgBot.SendNotify("Match found");
            for (int i = 0; i < 10; i++)
            {
                if (string.IsNullOrEmpty(PArray[i].WindowTitle))
                    continue;
                Thread.Sleep(250);
                WindowHelper.Click(PArray[i].WindowTitle, CSGOCoefficients.Accept);
                Thread.Sleep(250);
            }
            InvokeUI(() =>
            {
                AutoAcceptStatus.Fill = Red;
            });
            AutoAcceptStatusCircle = "🔴";
        }

        private void WinTeam1_Checked(object sender, RoutedEventArgs e)
        {
            settingsObj["WinTeam"] = 0;
            WinTeamNum = 0;
            WinTeam = gslT1;
            DisNumber = 15;

            if (on && AutoDisconnect.IsOn)
            {
                _ = Task.Run(() => CSGSILogic(true, false));
            }
        }

        private void WinTeam2_Checked(object sender, RoutedEventArgs e)
        {
            settingsObj["WinTeam"] = 1;
            WinTeamNum = 1;
            WinTeam = gslT2;
            DisNumber = 15;

            if (on && AutoDisconnect.IsOn)
            {
                _ = Task.Run(() => CSGSILogic(true, false));
            }
        }

        private void WinTeamTie_Checked(object sender, RoutedEventArgs e)
        {
            settingsObj["WinTeam"] = 2;
            WinTeamNum = 0;
            WinTeam = gslT1;
            DisNumber = 14;

            if (on && AutoDisconnect.IsOn)
            {
                _ = Task.Run(() => CSGSILogic(true, false));
            }
        }

        private async void PlayOneFunc(object sender, RoutedEventArgs e)
        {
            await CSGOIntercation.RestartCSGO(Int16.Parse(((Button)sender).Tag.ToString()));
        }

        private void ExChangeBot(object sender, RoutedEventArgs e)
        {
            if (choosed)
            {
                choosedObj.BorderBrush = null;
                choosed = false;
                short ChoosedObjNum = Convert.ToInt16(choosedObj.Tag);
                short ButtonSenderNum = Convert.ToInt16(((Button)sender).Tag);
                if (ChoosedObjNum < 5 && ButtonSenderNum < 5 || (ChoosedObjNum > 4 && ButtonSenderNum > 4))
                    return;
                if (!PArray[ChoosedObjNum].IsOn || !PArray[ButtonSenderNum].IsOn)
                    return;
                for (int i = 0, n = 0; i < 1 && n < 5; n++)
                {
                    if (PArray[n].IsOn)
                    {
                        if (ChoosedObjNum < 5)
                        {
                            if (ChoosedObjNum == n)
                                return;
                        }
                        else
                        {
                            if (ButtonSenderNum == n)
                                return;
                        }
                        i++;
                    }
                }

                for (int i = 0, n = 5; i < 1 && n < 10; n++)
                {
                    if (PArray[n].IsOn)
                    {
                        if (ChoosedObjNum > 4)
                        {
                            if (ChoosedObjNum == n)
                                return;
                        }
                        else
                        {
                            if (ButtonSenderNum == n)
                                return;
                        }
                        i++;
                    }
                }
                if (ChoosedObjNum < 5)
                {
                    string changetitle = T1WinTitle[ChoosedObjNum];
                    T1WinTitle[ChoosedObjNum] = T2WinTitle[ButtonSenderNum - 5];
                    T2WinTitle[ButtonSenderNum - 5] = changetitle;
                }
                else
                {
                    string changetitle = T2WinTitle[ChoosedObjNum - 5];
                    T2WinTitle[ChoosedObjNum - 5] = T1WinTitle[ButtonSenderNum];
                    T1WinTitle[ButtonSenderNum] = changetitle;
                }
                string changeLog = PArray[ChoosedObjNum].Login;
                PArray[ChoosedObjNum].Login = PArray[ButtonSenderNum].Login;
                PArray[ButtonSenderNum].Login = changeLog;
                PasswordBox[] passwordBox = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
                string changePass = passwordBox[ChoosedObjNum].Password;
                passwordBox[ChoosedObjNum].Password = passwordBox[ButtonSenderNum].Password;
                passwordBox[ButtonSenderNum].Password = changePass;
                PArray[ChoosedObjNum].Password = passwordBox[ChoosedObjNum].Password;
                PArray[ButtonSenderNum].Password = passwordBox[ButtonSenderNum].Password;

            }
            else
            {
                ((Button)sender).BorderBrush = Brushes.Red;
                choosedObj = (Button)sender;
                choosed = true;
            }
        }

        private async void GatherLobbies(object sender, RoutedEventArgs e)
        {
            if (!on)
                return;
            await CSGOIntercation.GatherLobby();
        }

        private void TestButton(object sender, RoutedEventArgs e)
        {
        }

        private static string GetSteamInstallPath()
        {
            RegistryKey subkey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
            if (subkey == null)
                return null;
            return subkey.GetValue("InstallPath") as string;
        }

        private void CPUReducer(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(@"BES\BES.exe");
            }
            catch(Exception)
            {
                MessageBox.Show("File not found", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AccountChecker()
        {
            while (on)
            {
                for (short i = 0; i < 10; i++)
                {
                    short index = i;
                    if (PArray[index].IsOn && WindowHelper.IsExist(PArray[i].WindowTitle))
                    {
                        PArray[index].IsStarted = Brushes.Green;
                    }
                    else
                    {
                        PArray[index].IsStarted = Red;
                    }
                }
                await Task.Delay(15000);
            }
        }

        private async void StatsUpdate()
        {
            if (string.IsNullOrEmpty(ActivePreset) || !PArray[0].IsOn || !PArray[5].IsOn)
                return;
            JObject lobbiesObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Lobbies.json"));
            Directory.CreateDirectory(settingsObj["CSGOFolder"].ToString() + @"\csgo\log\");
            Stream Team1logStream = File.Open(settingsObj["CSGOFolder"].ToString() + @"\csgo\log\0.log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            Stream Team2logStream = File.Open(settingsObj["CSGOFolder"].ToString() + @"\csgo\log\1.log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            StreamReader Team1log = new StreamReader(Team1logStream);
            StreamReader Team2log = new StreamReader(Team2logStream);
            bool[] accB = { false, false, false, false, false, false, false, false, false, false };
            bool done = false;
            PlayerStatus.Fill = Brushes.Green;
            PlayerStatusCircle = "🟢";
            while (!done && on)
            {
                string Team1String = Team1log.ReadToEnd();
                string Team2String = Team2log.ReadToEnd();
                string TeamString = Team1String + "\n" + Team2String;
                for (short i = 1; i < 11; i++)
                {
                    if (!accB[i - 1] && PArray[i - 1].IsOn)
                    {
                        if (!string.IsNullOrEmpty(lobbiesObj[ActivePreset]["Acc" + i].Value<string>("Login")) && lobbiesObj[ActivePreset]["Acc" + i].Value<string>("SteamID64") == "Unknown")
                        {
                            foreach (JToken tkn in lobbiesObj.SelectTokens("$..[?(@.Login == '" + lobbiesObj[ActivePreset]["Acc" + i].Value<string>("Login") + "')]"))
                            {
                                tkn["SteamID64"] = accInfo[tkn["Login"].ToString().ToLower()]["SteamID"];
                                tkn["Nickname"] = accInfo[tkn["Login"].ToString().ToLower()]["Nickname"];
                            }
                            foreach (Player plr in _player.Where(c => c.Login == lobbiesObj[ActivePreset]["Acc" + i].Value<string>("Login")))
                                plr.nickname = accInfo[plr.Login.ToLower()].Value<string>("Nickname");
                        }
                        string RegExStr = Regex.Match(TeamString, @"(xuid u64[(] " + lobbiesObj[ActivePreset]["Acc" + i]["SteamID64"] + " .*? )prime", RegexOptions.Singleline).Value;
                        string accRank = Regex.Match(RegExStr, @"(?<=ranking int[(] )\d+", RegexOptions.Singleline).Value;
                        if (!string.IsNullOrEmpty(accRank) && Regex.Match(RegExStr, @"(?<=ranktype int[(] )\d+", RegexOptions.Singleline).Value != "0")
                        {
                            short accLevel = Int16.Parse(Regex.Match(RegExStr, @"(?<=level int[(] )\d+", RegexOptions.Singleline).Value);
                            string accXP = Regex.Match(RegExStr, @"(?<=327680{0,3})(0{1}|[1-9]\d*)", RegexOptions.Singleline | RegexOptions.RightToLeft).Value;
                            foreach (JToken tkn in lobbiesObj.SelectTokens("$..[?(@.Login == '" + lobbiesObj[ActivePreset]["Acc" + i].Value<string>("Login") + "')]"))
                            {
                                tkn["Level"] = accLevel;
                                tkn["XP"] = accXP;
                                tkn["Rank"] = accRank;
                            }
                            foreach (Player plr in _player.Where(c => c.Login == lobbiesObj[ActivePreset]["Acc" + i].Value<string>("Login")))
                            {
                                plr.Level = accLevel;
                                plr.XP = accXP;
                                plr.Rank = "Images/" + accRank + ".png";
                            }
                            accB[i - 1] = true;
                        }
                    }
                    else
                        accB[i - 1] = true;
                }
                if (accB[0] && accB[1] && accB[2] && accB[3] && accB[4] && accB[5] && accB[6] && accB[7] && accB[8] && accB[9])
                {
                    done = true;
                    File.WriteAllText("Lobbies.json", JsonConvert.SerializeObject(lobbiesObj, Formatting.Indented));
                }
                await Task.Delay(2000);
            }
            PlayerStatus.Fill = Red;
            PlayerStatusCircle = "🔴";
            Team1logStream.Close();
            Team2logStream.Close();
            Team1log.Close();
            Team2log.Close();
        }

        private void AdvanceSettingsOn(object sender, RoutedEventArgs e)
        {
            Grid TargetGrid = null;
            if (((Button)sender).Name == "DisconnectSettingsOn")
            {
                TargetGrid = DisconnectSettings;
                SoundSettings.Visibility = Visibility.Collapsed;
            }
            else if (((Button)sender).Name == "SoundSettingsOn")
            {
                TargetGrid = SoundSettings;
                DisconnectSettings.Visibility = Visibility.Collapsed;
            }
            if (!TargetGrid.IsVisible)
                TargetGrid.Visibility = Visibility.Visible;
            else
                TargetGrid.Visibility = Visibility.Collapsed;
        }

        private void APIKeySave(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text.Length < 44)
            {
                TgCheckApiButton.IsEnabled = false;
                return;
            }
            TgCheckApiButton.IsEnabled = true;
            settingsObj[((TextBox)sender).Name] = ((TextBox)sender).Text;
            TgAPIKey = ((TextBox)sender).Text;
        }

        private void TgCheckApi(object sender, RoutedEventArgs e)
        {
            if (TgBot.BotIsOn)
            {
                MessageBox.Show("Tg bot already running");
                return;
            }
            if(!TgBot.TestApiKey(TgAPIKey))
            { 
                MessageBox.Show("Check your key");
                return;
            }
            TgBotStatus.Fill = Brushes.Green;
            TgBot.WaitingForCommand(TgAPIKey);
        }

        private void CheckUpdates(object sender, RoutedEventArgs e)
        {
            try
            {
                JObject version = CheckForNewVersion();
                if (version.Value<String>("actualversion") == Assembly.GetEntryAssembly().GetName().Version.ToString())
                {
                    MessageBox.Show("UpToDate");
                    return;
                }
                var client = new WebClient();
                client.DownloadFile(new Uri(@"https://hippocratic-fishes.000webhostapp.com/IziBoost.zip"), "temp_myprogram");
                if (Directory.Exists("temp_myprogram_folder")) Directory.Delete("temp_myprogram_folder", true);
                ZipFile.ExtractToDirectory("temp_myprogram", "temp_myprogram_folder");
                if (File.Exists("updater.exe")) { File.Delete("updater.exe"); }
                File.Move("temp_myprogram_folder\\updater.exe", "updater.exe");
                if (on)
                    settingsObj["CSGOsRunning"] = true;
                Process.Start("updater.exe", "IziBoost.exe");
                Application_Exit(null, null);
            }
            catch (Exception) { }
        }

        private static JObject CheckForNewVersion()
        {
            using (var webClient = new WebClient())
            {
                var jsonData = string.Empty;
                try
                {
                    jsonData = webClient.DownloadString("https://hippocratic-fishes.000webhostapp.com/version.json");
                }
                catch (Exception) { }
                JObject version = JsonConvert.DeserializeObject<JObject>(jsonData);
                return version;
            }
        }

        private void InvokeUI(Action a)
        {
            this.BeginInvoke(a);
        }

        public GameStateListener gslT1 = new GameStateListener(3001)
        {
            EnableRaisingIntricateEvents = true
        };
        public GameStateListener gslT2 = new GameStateListener(3002)
        {
            EnableRaisingIntricateEvents = true
        };

        public GameStateListener WinTeam;
    }
}
