using CSGSI;
using CSGSI.Events;
using MahApps.Metro.Controls;
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
using System.Runtime.InteropServices;
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
        public string loadedPreset;
        public static short WinTeamNum, GamesPlayerForAppSession = 0, GamesPlayerForGameSession = 0;
        public static int LobbyCount = 0, RoundNumber = 0, DisNumber = 15, ZIndex = 0;
        public static bool on = false, freezetime = true, loaded = false, choosed = false, newRound = true, onemeth = true, connected = false;
        public static bool AutoAcceptRestartS = false, WarmUp = false, DisconnectActive = false;
        public bool AutoBoost { get; set; }
        public static string AutoAcceptStatusCircle = "🔴", PlayerStatusCircle = "🔴";
        public List<string>[] TWinTitle = { T2WinTitle, T1WinTitle };
        public Button choosedObj;
        public static Brush Red = (Brush)new BrushConverter().ConvertFrom("#FFA20404");
        static public LobbyPlayer[] PArray = new LobbyPlayer[10];

        public MainWindow()
        {
            InitializeComponent();
            Loaded += LoadSettings;

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
            if (TgBot.BotIsOn)
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

        private readonly IList<Item> _items = new ObservableCollection<Item>();
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
            if (TgBot.BotIsOn)
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
                        _player.Add(new Player(property.Value.Value<JToken>("Acc" + (i + 1)).Value<string>("Login"), property.Value.Value<JToken>("Acc" + (i + 1)).Value<string>("Nickname"), property.Value.Value<JToken>("Acc" + (i + 1)).Value<short>("Level"), property.Value.Value<JToken>("Acc" + (i + 1)).Value<string>("XP"), "Images/" + (property.Value.Value<JToken>("Acc" + (i + 1)).Value<string>("Rank") ?? "0") + ".png", property.Name, "Collapsed"));
                    }
                    _player[_player.Count - 1].Visibility = "Visible";
                }
            }
            LobbyCount = _items.Count;
            if (!File.Exists("Settings.json"))
                File.WriteAllText("Settings.json", "{}");
            settingsObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Settings.json"));
            if (settingsObj.Property("SteamFolder") != null && settingsObj.Value<dynamic>("SteamFolder") != null)
            {
                SteamFolder.Text = settingsObj.Value<string>("SteamFolder");
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
            String[] BoolOptions = new string[] { "AutoAccept", "AutoDisconnect", "Sounds", "MatchFoundSound", "MatchEndedSound", "RoundLastsSound", "LongDisconnect" };

            for (int i = 0; i < BoolOptions.Length; i++)
            {
                if (settingsObj.Property(BoolOptions[i]) != null)
                    ((ToggleSwitch)this.GetType().GetField(BoolOptions[i], BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).IsOn = settingsObj.Value<bool>("AutoAccept");
                else
                    settingsObj[BoolOptions[i]] = ((ToggleSwitch)this.GetType().GetField(BoolOptions[i], BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).IsOn;
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

        public void StartTg()
        {
            Start(null, null);
        }

        private async void Start(object sender, RoutedEventArgs e)
        {
            if (on)
                return;
            if (!File.Exists("Settings.json"))
            {
                MessageBox.Show("Please specify Steam and CSGO folders");
                return;
            }
            else
            {
                if (string.IsNullOrEmpty(File.ReadAllText("Settings.json")))
                {
                    MessageBox.Show("Please specify Steam and CSGO folders");
                    return;
                }
            }
            if (settingsObj["CSGOFolder"] == null || settingsObj["SteamFolder"] == null)
            {
                MessageBox.Show("Please specify Steam and CSGO folders");
                return;
            }
            List<String> Logins = new List<String>();
            T1WinTitle.Clear();
            T2WinTitle.Clear();
            PasswordBox[] Password = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
            string[] Names = { "LEADER", "BOT" }, Res = { LeaderResX.Text + " " + LeaderResY.Text, BotResX.Text + " " + BotResY.Text };
            for (short i = 0, n = 0, l = 0; i < 10; i++)
            {
                if (PArray[i].State)
                {
                    if (string.IsNullOrEmpty(PArray[i].Login) || string.IsNullOrEmpty(Password[i].Password))
                    {
                        MessageBox.Show("Please type login or password");
                        return;
                    }
                    if (accInfo[PArray[i].Login.ToLower()] == null)
                    {
                        LoadSteamAccs();
                        if (accInfo[PArray[i].Login.ToLower()] == null)
                        {
                            MessageBox.Show("First login to this account: \"" + PArray[i].Login + "\" and then try again");
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
                MessageBox.Show("Please turn on at least one account");
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
                MessageBox.Show("Cannot start GameStateListener #1. AutoDisconnect won't work! Try reboot your PC");
            if (!gslT2.Start())
                MessageBox.Show("Cannot start GameStateListener #2. AutoDisconnect won't work! Try reboot your PC");
            if (AutoAccept.IsOn)
            {
                _ = Task.Run(() => AutoAcceptFunc());
            }
            CSGSILogic(true, true);
            AccountChecker();
            StatsUpdate();
            exChange.IsEnabled = true;
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            if (!controlContainer.IsEnabled)
            {
                ToggleSwitch[] ToggleButton = { ToggleButton1, ToggleButton2, ToggleButton3, ToggleButton4, ToggleButton5, ToggleButton6, ToggleButton7, ToggleButton8, ToggleButton9, ToggleButton10 };
                JObject lobbiesObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Lobbies.json"));
                for (int i = 0; i < 10; i++)
                {
                    if (ToggleButton[i].IsOn && FindWindow(null, PArray[i].WindowTitle).ToInt32() != 0)
                    {
                        int x;
                        Rect WindowRect = new Rect();
                        if (IsIconic(FindWindow(null, PArray[i].WindowTitle)))
                            ShowWindow(FindWindow(null, PArray[i].WindowTitle), 4);
                        GetWindowRect(FindWindow(null, PArray[i].WindowTitle), ref WindowRect);
                        if (WindowRect.Left < 0)
                            x = 0;
                        else
                            x = WindowRect.Left;
                        if (!string.IsNullOrEmpty(loadedPreset))
                            lobbiesObj[loadedPreset]["Acc" + (i + 1)]["Pos"] = x + " " + WindowRect.Top;
                        PArray[i].Position = x + " " + WindowRect.Top;
                    }
                    PArray[i].WindowTitle = "";
                }
                if (!string.IsNullOrEmpty(loadedPreset))
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
                PArray[i].Status = Red;
            on = false;
            WarmUp = false;
            connected = false;
            if (choosedObj != null)
                choosedObj.BorderBrush = null;
            choosed = false;
            controlContainer.IsEnabled = true;
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
                    lobbiesObj[PresetName.Text]["Acc" + (i + 1)]["Toggled"] = PArray[i].State;
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
                loadedPreset = PresetName.Text;
                for (short i = 0; i < 10; i++)
                {
                    _player.Add(new Player(lobbiesObj[loadedPreset].Value<JToken>("Acc" + (i + 1)).Value<string>("Login"), lobbiesObj[loadedPreset].Value<JToken>("Acc" + (i + 1)).Value<string>("Nickname"), lobbiesObj[loadedPreset].Value<JToken>("Acc" + (i + 1)).Value<short>("Level"), lobbiesObj[loadedPreset].Value<JToken>("Acc" + (i + 1)).Value<string>("XP"), "Images/" + (lobbiesObj[loadedPreset].Value<JToken>("Acc" + (i + 1)).Value<string>("Rank") ?? "0") + ".png", loadedPreset, "Collapsed"));
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
            loadedPreset = "";
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
        private void LoadPreset(object sender, MouseButtonEventArgs e)
        {
            if (lobbiesList.SelectedItem == null || on)
                return;
            JObject lobbiesObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Lobbies.json"));
            JObject AccObj = lobbiesObj.Property(_items[lobbiesList.SelectedIndex].Name).Value.ToObject<JObject>();
            PasswordBox[] Password = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
            loadedPreset = _items[lobbiesList.SelectedIndex].Name;
            for (int i = 0; i < 10; i++)
            {
                PArray[i].State = bool.Parse(AccObj["Acc" + (i + 1)].Value<string>("Toggled"));
                PArray[i].Login = AccObj["Acc" + (i + 1)].Value<string>("Login");
                Password[i].Password = AccObj["Acc" + (i + 1)].Value<string>("Password");
                PArray[i].Position = lobbiesObj[loadedPreset]["Acc" + (i + 1)]["Pos"].ToString();
            }
            PresetName.Text = loadedPreset;
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

            if (!enabled || !settingsObj.Value<bool>("AutoDisconnect"))
                return;

            if (settingsObj.Value<short>("WinTeam") == 2 && RoundNumber >= 15)
            {
                WinTeamNum = 1;
                WinTeam = gslT2; TeamConnectedChk = gslT1;
            }

            if (settingsObj.Value<bool>("LongDisconnect"))
                WinTeam.NewGameState += RoundLong;
            else
                WinTeam.NewGameState += Round;

            async void Round(GameState a)
            {
                if ((RoundNumber == a.Map.Round && roundPhase == a.Round.Phase) || a.Map.Phase == CSGSI.Nodes.MapPhase.Undefined || a.Map.Phase == CSGSI.Nodes.MapPhase.GameOver)
                    return;
                else
                {
                    RoundNumber = a.Map.Round;
                    roundPhase = a.Round.Phase;
                }

                if (a.Map.Round == (DisNumber - 1))
                    DisconnectActive = false;
                if (a.Round.Phase == CSGSI.Nodes.RoundPhase.Live && (a.Map.Round == 0 || (a.Map.Round == 15 && settingsObj.Value<short>("WinTeam") == 2)))
                {
                    for (int i = 0; i < TWinTitle[WinTeamNum].Count; i++)
                    {
                        if (IsIconic(FindWindow(null, TWinTitle[WinTeamNum][i])))
                            ShowWindow(FindWindow(null, TWinTitle[WinTeamNum][i]), 9);
                        SetForegroundWindow(FindWindow(null, TWinTitle[WinTeamNum][i]));
                        await Task.Delay(250);
                        SendKeyPress(0x44);
                        await Task.Delay(250);
                    }
                    return;
                }
                if (a.Map.Round == DisNumber && a.Round.Phase == CSGSI.Nodes.RoundPhase.FreezeTime)
                {
                    await Task.Delay(11000);
                    if (settingsObj.Value<bool>("LongDisconnect"))
                        return;
                    for (int i = 0; i < TWinTitle[WinTeamNum].Count; i++)
                    {
                        if (IsIconic(FindWindow(null, TWinTitle[WinTeamNum][i])))
                            ShowWindow(FindWindow(null, TWinTitle[WinTeamNum][i]), 9);
                        SetForegroundWindow(FindWindow(null, TWinTitle[WinTeamNum][i]));
                        Rect WindowRect = new Rect();
                        Coords CSGO = new Coords();
                        GetWindowRect(FindWindow(null, TWinTitle[WinTeamNum][i]), ref WindowRect);
                        ClientToScreen(FindWindow(null, TWinTitle[WinTeamNum][i]), ref CSGO);
                        await Task.Delay(250);
                        LeftClick(Convert.ToInt16((WindowRect.Right - WindowRect.Left - 6) / 1.348) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 30 + (WindowRect.Bottom - WindowRect.Top - 29) / 22 / 2 + CSGO.y));
                        await Task.Delay(250);
                    }
                    if (settingsObj.Value<short>("WinTeam") == 2)
                    {
                        gslT1.NewGameState -= Round;
                        gslT2.NewGameState -= Round;
                        WinTeamNum = 1;
                        WinTeam = gslT2; TeamConnectedChk = gslT1;
                        WinTeam.NewGameState += Round;
                    }
                    return;
                }

                if (!DisconnectActive && a.Round.Phase == CSGSI.Nodes.RoundPhase.Over && a.Map.Round != (DisNumber + 1) && a.Map.Round != DisNumber)
                {
                    DisconnectActive = true;
                    while (DisconnectActive)
                    {
                        short WinTeamNumTemp = WinTeamNum;
                        await Task.Delay(750);
                        if (IsIconic(FindWindow(null, TWinTitle[WinTeamNumTemp][0])))
                            ShowWindow(FindWindow(null, TWinTitle[WinTeamNumTemp][0]), 9);
                        SetForegroundWindow(FindWindow(null, TWinTitle[WinTeamNumTemp][0]));
                        Rect WindowRect = new Rect();
                        Coords CSGO = new Coords();
                        GetWindowRect(FindWindow(null, TWinTitle[WinTeamNumTemp][0]), ref WindowRect);
                        ClientToScreen(FindWindow(null, TWinTitle[WinTeamNumTemp][0]), ref CSGO);
                        await Task.Delay(250);
                        LeftClick(Convert.ToInt16((WindowRect.Right - WindowRect.Left - 6) / 1.348) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 30 + (WindowRect.Bottom - WindowRect.Top - 29) / 22 / 2 + CSGO.y));
                        await Task.Delay(250);
                        while (TeamConnectedChk.CurrentGameState.Map.Phase == CSGSI.Nodes.MapPhase.Undefined)
                            await Task.Delay(250);
                        if (IsIconic(FindWindow(null, TWinTitle[WinTeamNumTemp][0])))
                            ShowWindow(FindWindow(null, TWinTitle[WinTeamNumTemp][0]), 9);
                        SetForegroundWindow(FindWindow(null, TWinTitle[WinTeamNumTemp][0]));
                        await Task.Delay(250);
                        SendKeyPress(0x44);
                    }
                }
            }
            async void RoundLong(GameState a)
            {
                if (RoundNumber == a.Map.Round && roundPhase == a.Round.Phase || a.Map.Phase == CSGSI.Nodes.MapPhase.GameOver)
                    return;
                else
                {
                    RoundNumber = a.Map.Round;
                    roundPhase = a.Round.Phase;
                }

                if (a.Map.Round == 5 || a.Map.Round == (DisNumber - 2) || a.Map.Round == 20)
                    DisconnectActive = false;

                if ((a.Map.Round == 0 || a.Map.Round == 7 || a.Map.Round == DisNumber || a.Map.Round == 22) && a.Round.Phase == CSGSI.Nodes.RoundPhase.Live)
                {
                    for (int i = 0; i < TWinTitle[WinTeamNum].Count; i++)
                    {
                        if (IsIconic(FindWindow(null, TWinTitle[WinTeamNum][i])))
                            ShowWindow(FindWindow(null, TWinTitle[WinTeamNum][i]), 9);
                        SetForegroundWindow(FindWindow(null, TWinTitle[WinTeamNum][i]));
                        await Task.Delay(250);
                        SendKeyPress(0x44);
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
                        if (IsIconic(FindWindow(null, TWinTitle[WinTeamNum][i])))
                            ShowWindow(FindWindow(null, TWinTitle[WinTeamNum][i]), 9);
                        SetForegroundWindow(FindWindow(null, TWinTitle[WinTeamNum][i]));
                        Rect WindowRect = new Rect();
                        Coords CSGO = new Coords();
                        GetWindowRect(FindWindow(null, TWinTitle[WinTeamNum][i]), ref WindowRect);
                        ClientToScreen(FindWindow(null, TWinTitle[WinTeamNum][i]), ref CSGO);
                        await Task.Delay(250);
                        LeftClick(Convert.ToInt16((WindowRect.Right - WindowRect.Left - 6) / 1.348) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 30 + (WindowRect.Bottom - WindowRect.Top - 29) / 22 / 2 + CSGO.y));
                        await Task.Delay(250);
                    }
                    if (settingsObj.Value<short>("WinTeam") == 2)
                    {
                        if (a.Map.Round == 6 || a.Map.Round == 21)
                        {
                            gslT1.NewGameState -= RoundLong;
                            gslT2.NewGameState -= RoundLong;
                            WinTeamNum = 1;
                            WinTeam = gslT2; TeamConnectedChk = gslT1;
                            WinTeam.NewGameState += RoundLong;
                        }
                        else
                        {
                            gslT1.NewGameState -= RoundLong;
                            gslT2.NewGameState -= RoundLong;
                            WinTeamNum = 0;
                            WinTeam = gslT1; TeamConnectedChk = gslT2;
                            WinTeam.NewGameState += RoundLong;
                        }
                    }
                    return;
                }

                if (!DisconnectActive && a.Round.Phase == CSGSI.Nodes.RoundPhase.Over && a.Map.Round != 6 && a.Map.Round != 7 && a.Map.Round != (DisNumber - 1) && a.Map.Round != DisNumber && a.Map.Round != 21 && a.Map.Round != 22)
                {
                    DisconnectActive = true;
                    while (DisconnectActive)
                    {
                        short WinTeamNumTemp = WinTeamNum;
                        await Task.Delay(750);
                        if (IsIconic(FindWindow(null, TWinTitle[WinTeamNumTemp][0])))
                            ShowWindow(FindWindow(null, TWinTitle[WinTeamNumTemp][0]), 9);
                        SetForegroundWindow(FindWindow(null, TWinTitle[WinTeamNumTemp][0]));
                        Rect WindowRect = new Rect();
                        Coords CSGO = new Coords();
                        GetWindowRect(FindWindow(null, TWinTitle[WinTeamNumTemp][0]), ref WindowRect);
                        ClientToScreen(FindWindow(null, TWinTitle[WinTeamNumTemp][0]), ref CSGO);
                        await Task.Delay(250);
                        LeftClick(Convert.ToInt16((WindowRect.Right - WindowRect.Left - 6) / 1.348) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 30 + (WindowRect.Bottom - WindowRect.Top - 29) / 22 / 2 + CSGO.y));
                        await Task.Delay(250);
                        while (TeamConnectedChk.CurrentGameState.Map.Phase == CSGSI.Nodes.MapPhase.Undefined)
                            await Task.Delay(250);
                        if (IsIconic(FindWindow(null, TWinTitle[WinTeamNumTemp][0])))
                            ShowWindow(FindWindow(null, TWinTitle[WinTeamNumTemp][0]), 9);
                        SetForegroundWindow(FindWindow(null, TWinTitle[WinTeamNumTemp][0]));
                        await Task.Delay(250);
                        SendKeyPress(0x44);
                    }
                }
            }

            void Indicators(GameState a)
            {
                if (a.Map.Phase == CSGSI.Nodes.MapPhase.GameOver & !WarmUp)
                {
                    WarmUp = true;
                    DisconnectActive = false;
                    newRound = false;
                    connected = false;
                    GamesPlayerForAppSession++; GamesPlayerForGameSession++;
                    if (settingsObj.Value<bool>("AutoAccept"))
                    {
                        _ = Task.Run(() => AutoAcceptFunc());
                    }
                    InvokeUI(() =>
                    {
                        StatsUpdate();
                    });
                    if (settingsObj.Value<bool>("Sounds") && settingsObj.Value<bool>("MatchEndedSound"))
                    {
                        newRound = false;
                        mediaPlayer.Stream = Properties.Resources.MatchEnded;
                        mediaPlayer.Load();
                        mediaPlayer.Play();
                    }
                    if (TgBot.BotIsOn && MainWindow.settingsObj.Value<bool>("notifies"))
                        TgBot.SendNotify("Match ended (" + GamesPlayerForGameSession + "|" + GamesPlayerForAppSession + ")");
                    if (settingsObj.Value<short>("WinTeam") == 2 && settingsObj.Value<bool>("AutoDisconnect"))
                    {
                        gslT1.NewGameState -= Round;
                        gslT2.NewGameState -= Round;
                        gslT1.NewGameState -= RoundLong;
                        gslT2.NewGameState -= RoundLong;
                        WinTeam = gslT1; TeamConnectedChk = gslT2;
                        WinTeamNum = 0;
                        if (settingsObj.Value<bool>("LongDisconnect"))
                            WinTeam.NewGameState += RoundLong;
                        else
                            WinTeam.NewGameState += Round;
                    }
                }

                if (a.Map.Phase == CSGSI.Nodes.MapPhase.Warmup && WarmUp)
                {
                    WarmUp = false;
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
                            if (TgBot.BotIsOn && MainWindow.settingsObj.Value<bool>("notifies"))
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
                if (IsIconic(FindWindow(null, ldrTitles[i])))
                    ShowWindow(FindWindow(null, ldrTitles[i]), 9);
                SetForegroundWindow(FindWindow(null, ldrTitles[i]));
                Rect WindowRect = new Rect();
                Coords CSGO = new Coords();
                GetWindowRect(FindWindow(null, ldrTitles[i]), ref WindowRect);
                ClientToScreen(FindWindow(null, ldrTitles[i]), ref CSGO);
                await Task.Delay(250);
                LeftClick(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 3.36) + CSGO.x, Convert.ToInt16(WindowRect.Bottom - WindowRect.Top - 29 - (WindowRect.Bottom - WindowRect.Top - 29) / 22.85) + CSGO.y);
                await Task.Delay(250);
            }
            await Task.Delay(55000);
            for (int i = 0; i < ldrTitles.Count; i++)
            {
                if (IsIconic(FindWindow(null, ldrTitles[i])))
                    ShowWindow(FindWindow(null, ldrTitles[i]), 9);
                SetForegroundWindow(FindWindow(null, ldrTitles[i]));
                Rect WindowRect = new Rect();
                Coords CSGO = new Coords();
                GetWindowRect(FindWindow(null, ldrTitles[i]), ref WindowRect);
                ClientToScreen(FindWindow(null, ldrTitles[i]), ref CSGO);
                await Task.Delay(250);
                LeftClick(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 3.36) + CSGO.x, Convert.ToInt16(WindowRect.Bottom - WindowRect.Top - 29 - (WindowRect.Bottom - WindowRect.Top - 29) / 22.85) + CSGO.y);
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
            if (TgBot.BotIsOn && MainWindow.settingsObj.Value<bool>("notifies"))
                TgBot.SendNotify("Match found");
            for (int i = 0; i < 10; i++)
            {
                if (string.IsNullOrEmpty(PArray[i].WindowTitle))
                    continue;
                if (IsIconic(FindWindow(null, PArray[i].WindowTitle)))
                    ShowWindow(FindWindow(null, PArray[i].WindowTitle), 9);
                SetForegroundWindow(FindWindow(null, PArray[i].WindowTitle));
                Rect WindowRect = new Rect();
                Coords CSGO = new Coords();
                GetWindowRect(FindWindow(null, PArray[i].WindowTitle), ref WindowRect);
                ClientToScreen(FindWindow(null, PArray[i].WindowTitle), ref CSGO);
                Thread.Sleep(250);
                LeftClick(Convert.ToInt16((WindowRect.Right - WindowRect.Left - 6) / 2.35 + (WindowRect.Right - WindowRect.Left - 6) / 6.6 / 2) + CSGO.x, Convert.ToInt16(WindowRect.Bottom - WindowRect.Top - 29 - (WindowRect.Bottom - WindowRect.Top - 29) / 2.52 - (WindowRect.Bottom - WindowRect.Top - 29) / 12.5 / 2) + CSGO.y);
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
            WinTeam = gslT1; TeamConnectedChk = gslT2;
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
            WinTeam = gslT2; TeamConnectedChk = gslT1;
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
            WinTeam = gslT1; TeamConnectedChk = gslT2;
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
                if (Convert.ToInt16(choosedObj.Tag) < 6 && Convert.ToInt16(((Button)sender).Tag) < 6 || Convert.ToInt16(choosedObj.Tag) > 5 && Convert.ToInt16(((Button)sender).Tag) > 5)
                    return;
                if (!((ToggleSwitch)this.GetType().GetField("ToggleButton" + choosedObj.Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).IsOn || !((ToggleSwitch)this.GetType().GetField("ToggleButton" + ((Button)sender).Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).IsOn)
                    return;
                for (int i = 0, n = 0; i < 1 && n < 5; n++)
                {
                    if (((ToggleSwitch)this.GetType().GetField("ToggleButton" + (n + 1), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).IsOn)
                    {
                        if (Convert.ToInt16(choosedObj.Tag) < 6)
                        {
                            if (Convert.ToInt16(choosedObj.Tag) == (n + 1))
                                return;
                        }
                        else
                        {
                            if (Convert.ToInt16(((Button)sender).Tag) == (n + 1))
                                return;
                        }
                        i++;
                    }
                }

                for (int i = 0, n = 5; i < 1 && n < 10; n++)
                {
                    if (((ToggleSwitch)this.GetType().GetField("ToggleButton" + (n + 1), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).IsOn)
                    {
                        if (Convert.ToInt16(choosedObj.Tag) > 5)
                        {
                            if (Convert.ToInt16(choosedObj.Tag) == (n + 1))
                                return;
                        }
                        else
                        {
                            if (Convert.ToInt16(((Button)sender).Tag) == (n + 1))
                                return;
                        }
                        i++;
                    }
                }
                if (Convert.ToInt16(choosedObj.Tag) < 6)
                {
                    string changetitle = T1WinTitle[Convert.ToInt16(choosedObj.Tag) - 1];
                    T1WinTitle[Convert.ToInt16(choosedObj.Tag) - 1] = T2WinTitle[Convert.ToInt16(((Button)sender).Tag) - 6];
                    T2WinTitle[Convert.ToInt16(((Button)sender).Tag) - 6] = changetitle;
                }
                else
                {
                    string changetitle = T2WinTitle[Convert.ToInt16(choosedObj.Tag) - 6];
                    T2WinTitle[Convert.ToInt16(choosedObj.Tag) - 6] = T1WinTitle[Convert.ToInt16(((Button)sender).Tag) - 1];
                    T1WinTitle[Convert.ToInt16(((Button)sender).Tag) - 1] = changetitle;
                }
                string changeLog = ((TextBox)this.GetType().GetField("Login" + choosedObj.Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Text;
                ((TextBox)this.GetType().GetField("Login" + choosedObj.Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Text = ((TextBox)this.GetType().GetField("Login" + ((Button)sender).Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Text;
                ((TextBox)this.GetType().GetField("Login" + ((Button)sender).Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Text = changeLog;
                string changePass = ((PasswordBox)this.GetType().GetField("Password" + choosedObj.Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Password;
                ((PasswordBox)this.GetType().GetField("Password" + choosedObj.Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Password = ((PasswordBox)this.GetType().GetField("Password" + ((Button)sender).Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Password;
                ((PasswordBox)this.GetType().GetField("Password" + ((Button)sender).Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Password = changePass;
            }
            else
            {
                ((Button)sender).BorderBrush = Brushes.Red;
                choosedObj = (Button)sender;
                choosed = true;
            }
        }

        private async void AutomationTgl(object sender, RoutedEventArgs e)
        {
            await CSGOIntercation.GatherLobby();
        }

        private void RankBoostTgl(object sender, RoutedEventArgs e)
        {
            /*if (!loaded)
                return;
            MessageBox.Show(AutoBoost.ToString());
            settingsObj[((ToggleSwitch)sender).Name] = ((ToggleSwitch)sender).IsOn;
            //AutoBoost = ((ToggleSwitch)sender).IsOn;*/
        }

        private void CPUReducer(object sender, RoutedEventArgs e)
        {
            Process.Start(@"BES\BES.exe");
        }

        private async void AccountChecker()
        {
            ToggleSwitch[] ToggleButton = { ToggleButton1, ToggleButton2, ToggleButton3, ToggleButton4, ToggleButton5, ToggleButton6, ToggleButton7, ToggleButton8, ToggleButton9, ToggleButton10 };
            while (on)
            {
                for (short i = 0; i < 10; i++)
                {
                    short index = i;
                    if (ToggleButton[index].IsOn && FindWindow(null, PArray[i].WindowTitle).ToInt32() != 0)
                    {
                        PArray[index].Status = Brushes.Green;
                    }
                    else
                    {
                        PArray[index].Status = Red;
                    }
                }
                await Task.Delay(15000);
            }
        }

        private async void StatsUpdate()
        {
            if (string.IsNullOrEmpty(loadedPreset))
                return;
            JObject lobbiesObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Lobbies.json"));
            ToggleSwitch[] ToggleButton = { ToggleButton1, ToggleButton2, ToggleButton3, ToggleButton4, ToggleButton5, ToggleButton6, ToggleButton7, ToggleButton8, ToggleButton9, ToggleButton10 };
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
                string TeamString = Team1String;
                for (short i = 1; i < 11; i++)
                {
                    if (i == 6)
                        TeamString = Team2String;
                    if (!accB[i - 1])
                    {
                        if (!String.IsNullOrEmpty(lobbiesObj[loadedPreset]["Acc" + i].Value<string>("Login")) && lobbiesObj[loadedPreset]["Acc" + i].Value<string>("SteamID64") == "Unknown")
                        {
                            foreach (JToken tkn in lobbiesObj.SelectTokens("$..[?(@.Login == '" + lobbiesObj[loadedPreset]["Acc" + i].Value<string>("Login") + "')]"))
                            {
                                tkn["SteamID64"] = accInfo[tkn["Login"].ToString().ToLower()]["SteamID"];
                                tkn["Nickname"] = accInfo[tkn["Login"].ToString().ToLower()]["Nickname"];
                            }
                            foreach (Player plr in _player.Where(c => c.Login == lobbiesObj[loadedPreset]["Acc" + i].Value<string>("Login")))
                                plr.nickname = accInfo[plr.Login.ToLower()].Value<string>("Nickname");
                        }
                        string RegExStr = Regex.Match(TeamString, @"(xuid u64[(] " + lobbiesObj[loadedPreset]["Acc" + i]["SteamID64"] + " .*? )prime", RegexOptions.Singleline).Value;
                        string accRank = Regex.Match(RegExStr, @"(?<=ranking int[(] )\d+", RegexOptions.Singleline).Value;
                        if ((!string.IsNullOrEmpty(accRank) && Regex.Match(RegExStr, @"(?<=ranktype int[(] )\d+", RegexOptions.Singleline).Value != "0") || !ToggleButton[i - 1].IsOn)
                        {
                            if (ToggleButton[i - 1].IsOn)
                            {
                                short accLevel = Int16.Parse(Regex.Match(RegExStr, @"(?<=level int[(] )\d+", RegexOptions.Singleline).Value);
                                string accXP = Regex.Match(RegExStr, @"(?<=327680{0,3})(0{1}|[1-9]\d*)", RegexOptions.Singleline | RegexOptions.RightToLeft).Value;
                                foreach (JToken tkn in lobbiesObj.SelectTokens("$..[?(@.Login == '" + lobbiesObj[loadedPreset]["Acc" + i].Value<string>("Login") + "')]"))
                                {
                                    tkn["Level"] = accLevel;
                                    tkn["XP"] = accXP;
                                    tkn["Rank"] = accRank;
                                }
                                foreach (Player plr in _player.Where(c => c.Login == lobbiesObj[loadedPreset]["Acc" + i].Value<string>("Login")))
                                {
                                    plr.Level = accLevel;
                                    plr.XP = accXP;
                                    plr.Rank = "Images/" + accRank + ".png";
                                }
                            }
                            accB[i - 1] = true;
                        }
                    }
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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);

        /// <summary>
        /// simulate key press
        /// </summary>
        /// <param name="keyCode"></param>
        public static void SendKeyPress(ushort keyCode)
        {
            INPUT input = new INPUT
            {
                Type = 1
            };
            input.Data.Keyboard = new KEYBDINPUT()
            {
                Vk = (ushort)keyCode,
                Scan = (ushort)keyCode,
                Flags = 8,
                Time = 0,
                ExtraInfo = IntPtr.Zero,
            };

            INPUT input2 = new INPUT
            {
                Type = 1
            };
            input2.Data.Keyboard = new KEYBDINPUT()
            {
                Vk = (ushort)keyCode,
                Scan = (ushort)keyCode,
                Flags = 2 | 8,
                Time = 0,
                ExtraInfo = IntPtr.Zero
            };
            INPUT[] inputs = new INPUT[] { input, input2 };
            if (SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT))) == 0)
                throw new Exception();
        }

        public static void SendKeyPress(ushort keyCodeModifier, ushort keyCode)
        {
            INPUT input = new INPUT
            {
                Type = 1
            };
            input.Data.Keyboard = new KEYBDINPUT()
            {
                Vk = (ushort)keyCodeModifier,
                Scan = (ushort)keyCodeModifier,
                Flags = 8,
                Time = 0,
                ExtraInfo = IntPtr.Zero,
            };

            INPUT input2 = new INPUT
            {
                Type = 1
            };
            input2.Data.Keyboard = new KEYBDINPUT()
            {
                Vk = (ushort)keyCode,
                Scan = (ushort)keyCode,
                Flags = 8,
                Time = 0,
                ExtraInfo = IntPtr.Zero,
            };

            INPUT input3 = new INPUT
            {
                Type = 1
            };
            input3.Data.Keyboard = new KEYBDINPUT()
            {
                Vk = (ushort)keyCode,
                Scan = (ushort)keyCode,
                Flags = 2 | 8,
                Time = 0,
                ExtraInfo = IntPtr.Zero
            };

            INPUT input4 = new INPUT
            {
                Type = 1
            };
            input4.Data.Keyboard = new KEYBDINPUT()
            {
                Vk = (ushort)keyCodeModifier,
                Scan = (ushort)keyCodeModifier,
                Flags = 2 | 8,
                Time = 0,
                ExtraInfo = IntPtr.Zero
            };

            INPUT[] inputs = new INPUT[] { input, input2 };
            INPUT[] inputs2 = new INPUT[] { input3, input4 };
            if (SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT))) == 0)
                throw new Exception();
            Thread.Sleep(50);
            if (SendInput(2, inputs2, Marshal.SizeOf(typeof(INPUT))) == 0)
                throw new Exception();
        }

        public static void LeftClick(int x, int y)
        {
            INPUT input = new INPUT
            {
                Type = 0
            };
            input.Data.Mouse = new MOUSEINPUT()
            {
                X = (x + 1) * 65536 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                Y = (y + 1) * 65536 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height,
                MouseData = 0,
                Flags = 2 | 0x8000 | 1,
                Time = 0,
                ExtraInfo = IntPtr.Zero
            };
            INPUT input2 = new INPUT
            {
                Type = 0
            };
            input2.Data.Mouse = new MOUSEINPUT()
            {
                X = (x + 1) * 65536 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                Y = (y + 1) * 65536 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height,
                MouseData = 0,
                Flags = 4 | 0x8000 | 1,
                Time = 0,
                ExtraInfo = IntPtr.Zero
            };
            INPUT[] inputs = new INPUT[] { input, input2 };
            if (SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT))) == 0)
                throw new Exception();
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646270(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public uint Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        /// <summary>
        /// http://social.msdn.microsoft.com/Forums/en/csharplanguage/thread/f0e82d6e-4999-4d22-b3d3-32b25f61fb2a
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public HARDWAREINPUT Hardware;
            [FieldOffset(0)]
            public KEYBDINPUT Keyboard;
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            public uint Msg;
            public ushort ParamL;
            public ushort ParamH;
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            public ushort Vk;
            public ushort Scan;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        /// <summary>
        /// http://social.msdn.microsoft.com/forums/en-US/netfxbcl/thread/2abc6be8-c593-4686-93d2-89785232dacd
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int X;
            public int Y;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);
        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hwnd, ref Coords cordinates);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        public struct Coords
        {
            public int x;
            public int y;
        }

        public GameStateListener gslT1 = new GameStateListener(3001)
        {
            EnableRaisingIntricateEvents = true
        };
        public GameStateListener gslT2 = new GameStateListener(3002)
        {
            EnableRaisingIntricateEvents = true
        };

        public GameStateListener WinTeam, TeamConnectedChk;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr handle);
    }
}
