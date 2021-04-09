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

    public partial class MainWindow : MetroWindow
    {
        public Version AssemblyVersion => Assembly.GetEntryAssembly().GetName().Version;
        public static readonly LogWriter log = new LogWriter("Start");
        public static string TgAPIKey;
        private readonly SoundPlayer mediaPlayer = new SoundPlayer();
        public static List<string> T1WinTitle = new List<string>(), T2WinTitle = new List<string>();
        public static JObject accInfo;
        public static short WinTeamNum, GamesPlayerForAppSession = 0, GamesPlayerForGameSession = 0;
        public static int LobbyCount = 0, RoundNumber = 0, DisNumber = 15, ZIndex = 0, RenameTeamIndex;
        public static bool on = false, freezetime = true, loaded = false, choosed = false, newRound = true, onemeth = true, connected = false;
        public static bool AutoAcceptRestartS = false, WarmUp = false, DisconnectActive = false;
        public static string AutoAcceptStatusCircle = "🔴", PlayerStatusCircle = "🔴";
        public List<string>[] TWinTitle = { T2WinTitle, T1WinTitle };
        public Button choosedObj;
        public static Brush Red = (Brush)new BrushConverter().ConvertFrom("#FFA20404");
        public static Team ActiveTeam = new Team().New();

        public static Settings ProgramSettings;
        public static IList<Team> TeamsCollection;

        public MainWindow()
        {
            if (File.Exists("LobbiesNew.json") && !string.IsNullOrEmpty(File.ReadAllText("LobbiesNew.json")))
                TeamsCollection = JsonConvert.DeserializeObject<ObservableCollection<Team>>(File.ReadAllText("LobbiesNew.json"));
            else
                TeamsCollection = new ObservableCollection<Team>();

            if (File.Exists("Settings.json") && !string.IsNullOrEmpty(File.ReadAllText("Settings.json")))
                ProgramSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("Settings.json"));
            else
                ProgramSettings = new Settings()
                {
                    WaitSecondsAutomation = 0,
                    DelayInRound14 = 5,
                    LeaderResX = 640,
                    LeaderResY = 480,
                    BotResX = 400,
                    BotResY = 300,
                    MatchFoundSound = true,
                    MatchEndedSound = true,
                    RoundLastsSound = true,
                    LobbyNotGatheredSound = true,
                };

            //UpdateProgram(true);
            InitializeComponent();

            Loaded += LoadSettings;

            #if DEBUG
            TestBtn.Visibility = Visibility.Visible;
            #endif
            controlContainer.DataContext = ActiveTeam;
            SettingsGrid.DataContext = ProgramSettings;
            lobbiesList.ItemsSource = TeamsCollection;
            playersList.ItemsSource = TeamsCollection;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (on)
                ProgramSettings.CSGOsRunning = true;
            File.WriteAllText("Settings.json", JsonConvert.SerializeObject(ProgramSettings, Formatting.Indented));
            File.WriteAllText("LobbiesNew.json", JsonConvert.SerializeObject(TeamsCollection, Formatting.Indented));
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
                var source = e.Data.GetData(typeof(Team)) as Team;
                var target = item.DataContext as Team;

                int sourceIndex = lobbiesList.Items.IndexOf(source);
                int targetIndex = lobbiesList.Items.IndexOf(target);

                Move(source, sourceIndex, targetIndex);
            }
        }

        private void Move(Team source, int sourceIndex, int targetIndex)
        {
            if (sourceIndex < targetIndex)
            {
                TeamsCollection.Insert(targetIndex + 1, source);
                TeamsCollection.RemoveAt(sourceIndex);
            }
            else
            {
                int removeIndex = sourceIndex + 1;
                if (TeamsCollection.Count + 1 > removeIndex)
                {
                    TeamsCollection.Insert(targetIndex, source);
                    TeamsCollection.RemoveAt(removeIndex);
                }
            }
            File.WriteAllText("LobbiesNew.json", JsonConvert.SerializeObject(TeamsCollection, Formatting.Indented));
        }

        private void Application_Exit(object sender, EventArgs e)
        {
            File.WriteAllText("Settings.json", JsonConvert.SerializeObject(ProgramSettings, Formatting.Indented));
            File.WriteAllText("LobbiesNew.json", JsonConvert.SerializeObject(TeamsCollection, Formatting.Indented));
            TgBot.RemoveKeyboard();
            log.LogWrite("Exit");
            Environment.Exit(0);
        }

        private void LoadSettings(object sender, RoutedEventArgs e)
        {
            LobbyCount = TeamsCollection.Count;
            if (string.IsNullOrEmpty(ProgramSettings.SteamFolder) && GetSteamInstallPath() is string a)
            {
                ProgramSettings.SteamFolder = a;
                LoadSteamAccs();
            }
            loaded = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            grid1.Focus();
            lobbiesList.SelectedIndex = -1;
            NewLobbyGrid.Visibility = Visibility.Collapsed;
            RenameLobbyGrid.Visibility = Visibility.Collapsed;
            SoundSettings.Visibility = Visibility.Collapsed;
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
            if (string.IsNullOrEmpty(ProgramSettings.CSGOFolder) || string.IsNullOrEmpty(ProgramSettings.SteamFolder))
            {
                InfoMessage(sender, "Please specify Steam and CSGO folders",  MessageBoxImage.Information);
                return;
            }
            List<String> Logins = new List<String>();
            T1WinTitle.Clear();
            T2WinTitle.Clear();
            string[] Names = { "LEADER", "BOT" }, Res = { ProgramSettings.LeaderResX + " " + ProgramSettings.LeaderResY, ProgramSettings.BotResX + " " + ProgramSettings.BotResY};
            SavePasswords();
            for (short i = 0, n = 0, l = 0; i < 10; i++)
            {
                if (ActiveTeam.Player[i].Toggled)
                {
                    if (string.IsNullOrEmpty(ActiveTeam.Player[i].Login) || string.IsNullOrEmpty(ActiveTeam.Player[i].Password))
                    {
                        InfoMessage(sender, "Please type login or password",  MessageBoxImage.Information);
                        return;
                    }
                    if (accInfo[ActiveTeam.Player[i].Login.ToLower()] == null)
                    {
                        LoadSteamAccs();
                        if (accInfo[ActiveTeam.Player[i].Login.ToLower()] == null)
                        {
                            InfoMessage(sender, "First login to this account: \"" + ActiveTeam.Player[i].Login + "\" and then try again",  MessageBoxImage.Information);
                            return;
                        }
                    }
                    if (i < 5)
                    {
                        Logins.Add(ActiveTeam.Player[i].Login + " " + ActiveTeam.Player[i].Password + " " + ActiveTeam.Player[i].Pos + " " + Res[l] + " \"" + Names[l] + " #1\" " + n);
                        ActiveTeam.Player[i].WindowTitle = "LOGIN: " + ActiveTeam.Player[i].Login.ToLower() + " | " + Names[l] + " #1";
                        T1WinTitle.Add("LOGIN: " + ActiveTeam.Player[i].Login.ToLower() + " | " + Names[l] + " #1");
                    }
                    else
                    {
                        Logins.Add(ActiveTeam.Player[i].Login + " " + ActiveTeam.Player[i].Password + " " + ActiveTeam.Player[i].Pos + " " + Res[l] + " \"" + Names[l] + " #2\" " + (n + 2));
                        ActiveTeam.Player[i].WindowTitle = "LOGIN: " + ActiveTeam.Player[i].Login.ToLower() + " | " + Names[l] + " #2";
                        T2WinTitle.Add("LOGIN: " + ActiveTeam.Player[i].Login.ToLower() + " | " + Names[l] + " #2");
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
            }
            if (Logins.Count < 1)
            {
                InfoMessage(sender, "Please turn on at least one account", MessageBoxImage.Information);
                return;
            }
            if(!string.IsNullOrEmpty(ActiveTeam.TeamName))
                File.WriteAllText("LobbiesNew.json", JsonConvert.SerializeObject(TeamsCollection, Formatting.Indented));
            controlContainer.IsEnabled = false;
            AddButton.IsEnabled = false;
                        on = true;
            for (int i = 0; i < Logins.Count; i++)
            {
                if (CSGOsRunning.IsChecked == true)
                    continue;
                if (!on)
                    return;
                Process.Start("Launcher.exe", "false \"" + ProgramSettings.SteamFolder + "\" " + Logins[i] + " \"" + ProgramSettings.CSGOFolder + "\" ");
                await Task.Delay(4000);
            }
            if (!(bool)WinTeam1.IsChecked && !(bool)WinTeam2.IsChecked && !(bool)WinTeamTie.IsChecked)
                WinTeam1.IsChecked = true;
            if (!gslT1.Start())
                InfoMessage(sender, "Cannot start GameStateListener #1. AutoDisconnect won't work! Try reboot your PC", MessageBoxImage.Warning);
            if (!gslT2.Start())
                InfoMessage(sender, "Cannot start GameStateListener #2. AutoDisconnect won't work! Try reboot your PC", MessageBoxImage.Warning);
            _ = Task.Run(() => AutoAcceptFunc());
            CSGSILogic(true);
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

        private void SavePasswords()
        {
            PasswordBox[] Password = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
            for (short i = 0; i < 10; i++)
            {
                ActiveTeam.Player[i].Password = Password[i].Password;
            }
        }

        public void Stop(object sender, RoutedEventArgs e)
        {
            if (!controlContainer.IsEnabled)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (ActiveTeam.Player[i].Toggled && WindowHelper.IsExist(ActiveTeam.Player[i].WindowTitle))
                    {
                        int x;
                        WindowHelper.Rect WindowRect = new WindowHelper.Rect();
                        WindowHelper.GetRect(ActiveTeam.Player[i].WindowTitle, ref WindowRect);
                        if (WindowRect.Left < 0)
                            x = 0;
                        else
                            x = WindowRect.Left;
                        ActiveTeam.Player[i].Pos = x + " " + WindowRect.Top;
                    }
                    ActiveTeam.Player[i].WindowTitle = "";
                }
                if (!string.IsNullOrEmpty(ActiveTeam.TeamName))
                    File.WriteAllText("LobbiesNew.json", JsonConvert.SerializeObject(TeamsCollection, Formatting.Indented));
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

            CSGSILogic(false);
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
                ActiveTeam.Player[i].IsStarted = Red;
            on = false;
            WarmUp = false;
            connected = false;
            if (choosedObj != null)
                choosedObj.BorderBrush = null;
            choosed = false;
            controlContainer.IsEnabled = true;
            AdditionOptions.IsEnabled = false;
            exChange.IsEnabled = false;
            AddButton.IsEnabled = true;
        }

        private void NewLobbyGridOpen(object sender, RoutedEventArgs e)
        {
            if (!NewLobbyGrid.IsVisible)
            {
                NewLobbyGrid.Visibility = Visibility.Visible;
                PresetName.Focus();
            }
            else
                NewLobbyGrid.Visibility = Visibility.Collapsed;
        }

        private void NewLobby(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (ProgramSettings.CSGOFolder == null || ProgramSettings.SteamFolder == null)
            {
                MessageBox.Show("Please specify Steam and CSGO folders");
                return;
            }
            if (string.IsNullOrEmpty(PresetName.Text) || on) return;
            NewLobbyGrid.Visibility = Visibility.Collapsed;
            grid1.Focus();
            e.Handled = true;
            PasswordBox[] Password = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
            if (!string.IsNullOrEmpty(ActiveTeam.TeamName))
            {
                ActiveTeam = new Team().New();
                controlContainer.DataContext = ActiveTeam;
                for (short i = 0; i < 10; i++)
                    Password[i].Password = "";
            }
            TeamsCollection.Add(ActiveTeam);
            ActiveTeam.TeamName = PresetName.Text;
            LobbyCount = TeamsCollection.Count;
            File.WriteAllText("LobbiesNew.json", JsonConvert.SerializeObject(TeamsCollection, Formatting.Indented));
        }

        private void SaveLobby(object sender, RoutedEventArgs e)
        {
            LoadSteamAccs();
            SavePasswords();
            if (!string.IsNullOrEmpty(ActiveTeam.TeamName))
            {
                foreach (Player player in ActiveTeam.Player)
                {
                    player.SteamID = accInfo.Value<JToken>(player.Login)?.Value<string>("SteamID") ?? "Unknown";
                    player.Nickname = accInfo.Value<JToken>(player.Login)?.Value<string>("Nickname") ?? "Unknown";
                }
                File.WriteAllText("LobbiesNew.json", JsonConvert.SerializeObject(TeamsCollection, Formatting.Indented));
                MessageBox.Show("Preset successfully updated");
            }
            else
            {
                NewLobbyGrid.Visibility = Visibility.Visible;
                PresetName.Focus();
            }
        }

        private void LoadFromFile(object sendder, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory()
            };
            PasswordBox[] Password = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
            if (dialog.ShowDialog() == CommonFileDialogResult.Cancel)
                return;
            string[] Delimiters = { " ", "\t", Environment.NewLine };
            string[] accounts = File.ReadAllText(dialog.FileName).Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
            if (accounts[0] == "1")
            {
                for (int i = 1, a = 0, b = 5; a < 5; i += 4, a++, b++)
                {
                    ActiveTeam.Player[a].Login = accounts[i];
                    Password[a].Password = accounts[i + 1];
                    ActiveTeam.Player[b].Login = accounts[i + 2];
                    Password[b].Password = accounts[i + 3];
                }
            } if (accounts[0] == "2")
            {
                for (int i = 1, a = 0; a < 10; i += 2, a++)
                {
                    ActiveTeam.Player[a].Login = accounts[i];
                    Password[a].Password = accounts[i + 1];
                }
            }
            SavePasswords();
        }

        private void RenameLobby(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (ProgramSettings.CSGOFolder == null || ProgramSettings.SteamFolder == null)
            {
                MessageBox.Show("Please specify Steam and CSGO folders");
                return;
            }
            if (string.IsNullOrEmpty(NewLobbyName.Text)) return;
            RenameLobbyGrid.Visibility = Visibility.Collapsed;
            grid1.Focus();
            TeamsCollection[RenameTeamIndex].TeamName = NewLobbyName.Text;
            File.WriteAllText("LobbiesNew.json", JsonConvert.SerializeObject(TeamsCollection, Formatting.Indented));
        }

        private void MenuItem_Delete(object sender, RoutedEventArgs e)
        {
            TeamsCollection[lobbiesList.SelectedIndex].TeamName = "";
            TeamsCollection.RemoveAt(lobbiesList.SelectedIndex);
            File.WriteAllText("LobbiesNew.json", JsonConvert.SerializeObject(TeamsCollection, Formatting.Indented));
            LobbyCount = TeamsCollection.Count;
        }
        private void MenuItem_Duplicate(object sender, RoutedEventArgs e)
        {
            TeamsCollection.Insert(lobbiesList.SelectedIndex+1, TeamsCollection[lobbiesList.SelectedIndex].Duplicate());
            File.WriteAllText("LobbiesNew.json", JsonConvert.SerializeObject(TeamsCollection, Formatting.Indented));
        }
        private void MenuItem_Rename(object sender, RoutedEventArgs e)
        {
            RenameLobbyGrid.Visibility = Visibility.Visible;
            RenameTeamIndex = lobbiesList.SelectedIndex;
            NewLobbyName.Text = TeamsCollection[RenameTeamIndex].TeamName;
            NewLobbyName.Focus();
        }
        private void MenuItem_CopyPos(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("Can not copy windows positions while boost is active!");
                return;
            }
            for(short i = 0; i < 10; i++)
                ActiveTeam.Player[i].Pos = TeamsCollection[lobbiesList.SelectedIndex].Player[i].Pos;
            MessageBox.Show("Succesfully! Save your active lobby preset to save windows positions.");
        }

        private void SaveFolder(object sender, RoutedEventArgs e)
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
                ProgramSettings.SteamFolder = dialog.FileName;
            }
            else
                ProgramSettings.CSGOFolder = dialog.FileName;
        }

        private void NoSpacesTextBox(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }

        private void CheckPastedText(object sender, DataObjectPastingEventArgs e)
        {
            if (Regex.IsMatch(e.DataObject.GetData(typeof(string)).ToString(), "[^0-9]+"))
            {
                e.CancelCommand();
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SettingsTgl(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                return;
            if (ProgramSettings.AutoDisconnect && on)
            {
                Task.Run(() => CSGSILogic(false));
            }
            if (!ProgramSettings.AutoDisconnect && on)
            {
                Task.Run(() => CSGSILogic(false));
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
            ActiveTeam = TeamsCollection[index];
            controlContainer.DataContext = ActiveTeam;
            PasswordBox[] Password = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
            for (short i = 0; i < 10; i++)
                Password[i].Password = ActiveTeam.Player[i].Password;
            tab.SelectedIndex = 0;
        }

        private void LoadSteamAccs()
        {
            if (!File.Exists(ProgramSettings.SteamFolder + @"/config/loginusers.vdf"))
            {
                return;
            }
            accInfo = new JObject();
            string info = File.ReadAllText(ProgramSettings.SteamFolder + @"/config/loginusers.vdf");
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
            if (!ProgramSettings.AutoAccept || !ActiveTeam.Player[0].Toggled || !ActiveTeam.Player[5].Toggled)
                return;
            Directory.CreateDirectory(ProgramSettings.CSGOFolder + @"\csgo\log\");
            Stream Team1logStream = File.Open(ProgramSettings.CSGOFolder + @"\csgo\log\0.log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            Stream Team2logStream = File.Open(ProgramSettings.CSGOFolder + @"\csgo\log\1.log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
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
                if (Team1String != Team2String && !string.IsNullOrEmpty(Team1String) && !string.IsNullOrEmpty(Team2String) && !AutoAcceptRestartS)
                {
                    AutoAcceptRestartS = true;
                    RestartSearch(false);
                }
                Thread.Sleep(650);
            }
        }

        private void CSGSILogic(bool indicators)
        {
            CSGSI.Nodes.RoundPhase roundPhase = CSGSI.Nodes.RoundPhase.Undefined;

            gslT1.NewGameState -= Round;
            gslT2.NewGameState -= Round;
            DisconnectActive = false;

            if (!ProgramSettings.AutoDisconnect || !ActiveTeam.Player[0].Toggled || !ActiveTeam.Player[5].Toggled)
                return;

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

            if (ProgramSettings.WinTeamTie && RoundNumber >= 15)
            {
                WinTeamNum = 1;
                WinTeam = gslT2;
            }

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

                if (a.Round.Phase == CSGSI.Nodes.RoundPhase.FreezeTime && (a.Map.Round == 0 || (a.Map.Round == 15 && ProgramSettings.WinTeamTie)))
                {
                    for (int i = 0; i < TWinTitle[WinTeamNum].Count; i++)
                    {
                        await Task.Delay(250);
                        WindowHelper.SendKey(TWinTitle[WinTeamNum][i], WindowHelper.VK_F10);
                        await Task.Delay(250);
                    }
                    return;
                }
                if (a.Map.Round == 14 && a.Round.Phase == CSGSI.Nodes.RoundPhase.FreezeTime && ProgramSettings.WinTeamTie)
                {
                    await Task.Delay(7000 + ProgramSettings.DelayInRound14*1000);
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
                || (a.Map.Round == 15 && a.Map.Phase == CSGSI.Nodes.MapPhase.Live && !ProgramSettings.WinTeamTie)))
                {
                    short num;
                    if (WinTeam == gslT1)
                        num = 1;
                    else
                        num = 0;
                    Stream TeamlogStream = File.Open(ProgramSettings.CSGOFolder + @"\csgo\log\" + num + ".log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
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

            async void Indicators(GameState a)
            {
                if (a.Map.Phase == CSGSI.Nodes.MapPhase.GameOver && !WarmUp)
                {
                    WarmUp = true;
                    DisconnectActive = false;
                    newRound = false;
                    connected = false;
                    RoundNumber = 0;
                    GamesPlayerForAppSession++; GamesPlayerForGameSession++;
                    _ = Task.Run(() => AutoAcceptFunc());
                    InvokeUI(() =>
                    {
                        StatsUpdate();
                    });


                    if (ProgramSettings.AutoDisconnect)
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

                    if (ProgramSettings.Sounds && ProgramSettings.MatchEndedSound)
                    {
                        newRound = false;
                        mediaPlayer.Stream = Properties.Resources.MatchEnded;
                        mediaPlayer.Load();
                        mediaPlayer.Play();
                    }
                    TgBot.SendNotify("Match ended (" + GamesPlayerForGameSession + "|" + GamesPlayerForAppSession + ")");
                    if (ProgramSettings.WinTeamTie && ProgramSettings.AutoDisconnect)
                    {
                        gslT1.NewGameState -= Round;
                        gslT2.NewGameState -= Round;
                        WinTeam = gslT1;
                        WinTeamNum = 0;
                        WinTeam.NewGameState += Round;
                    }
                    if(ProgramSettings.Automation && ActiveTeam.Player[0].Toggled && ActiveTeam.Player[5].Toggled)
                    {
                        if (ProgramSettings.WinTeam1)
                            ProgramSettings.WinTeam2 = true;
                        else if (ProgramSettings.WinTeam2)
                            ProgramSettings.WinTeam1 = true;
                        await Task.Delay(ProgramSettings.WaitSecondsAutomation * 1000);
                        await CSGOIntercation.GatherLobby();
                        await CSGOIntercation.StartSearching(3);
                        await Task.Delay(10000);
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            if (PlayerStatus.Fill != Red)
                            {
                                if (ProgramSettings.Sounds && ProgramSettings.LobbyNotGatheredSound)
                                {
                                    mediaPlayer.Stream = Properties.Resources.RoundLasts;
                                    mediaPlayer.Load();
                                    mediaPlayer.Play();
                                }
                                TgBot.SendNotify("Check you lobbies, maybe not all bots entered the lobby");
                            }
                        });
                    }
                }

                if (a.Map.Phase == CSGSI.Nodes.MapPhase.Warmup && WarmUp)
                {
                    WarmUp = false;
                    if (ProgramSettings.WinTeamTie && ProgramSettings.AutoDisconnect)
                    {
                        gslT1.NewGameState -= Round;
                        gslT2.NewGameState -= Round;
                        WinTeam = gslT1;
                        WinTeamNum = 0;
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
                            if (ProgramSettings.Sounds && ProgramSettings.RoundLastsSound)
                            {
                                mediaPlayer.Stream = Properties.Resources.RoundLasts;
                                mediaPlayer.Load();
                                mediaPlayer.Play();
                            }
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
                if (string.IsNullOrEmpty(ActiveTeam.Player[i].WindowTitle))
                    continue;
                if (ActiveTeam.Player[i].WindowTitle.Contains("LEADER"))
                    ldrTitles.Add(ActiveTeam.Player[i].WindowTitle);
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
            if (ProgramSettings.Sounds && ProgramSettings.MatchFoundSound)
            {
                mediaPlayer.Stream = Properties.Resources.MatchFound;
                mediaPlayer.Load();
                mediaPlayer.Play();
            }
            TgBot.SendNotify("Match found");
            for (int i = 0; i < 10; i++)
            {
                if (string.IsNullOrEmpty(ActiveTeam.Player[i].WindowTitle))
                    continue;
                Thread.Sleep(250);
                WindowHelper.Click(ActiveTeam.Player[i].WindowTitle, CSGOCoefficients.Accept);
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
            WinTeamNum = 0;
            WinTeam = gslT1;
            DisNumber = 15;

            if (on && ProgramSettings.AutoDisconnect)
                _ = Task.Run(() => CSGSILogic(false));
        }

        private void WinTeam2_Checked(object sender, RoutedEventArgs e)
        {
            WinTeamNum = 1;
            WinTeam = gslT2;
            DisNumber = 15;

            if (on && ProgramSettings.AutoDisconnect)
                _ = Task.Run(() => CSGSILogic(false));
        }

        private void WinTeamTie_Checked(object sender, RoutedEventArgs e)
        {
            WinTeamNum = 0;
            WinTeam = gslT1;
            DisNumber = 14;

            if (on && ProgramSettings.AutoDisconnect)
                _ = Task.Run(() => CSGSILogic(false));
        }

        private void PlayOneFunc(object sender, RoutedEventArgs e)
        {
             CSGOIntercation.RestartCSGO(Int16.Parse(((Button)sender).Tag.ToString()));
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
                if (!ActiveTeam.Player[ChoosedObjNum].Toggled || !ActiveTeam.Player[ButtonSenderNum].Toggled)
                    return;
                for (int i = 0, n = 0; i < 1 && n < 5; n++)
                {
                    if (ActiveTeam.Player[n].Toggled)
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
                    if (ActiveTeam.Player[n].Toggled)
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
                string changeLog = ActiveTeam.Player[ChoosedObjNum].Login;
                ActiveTeam.Player[ChoosedObjNum].Login = ActiveTeam.Player[ButtonSenderNum].Login;
                ActiveTeam.Player[ButtonSenderNum].Login = changeLog;
                PasswordBox[] passwordBox = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
                string changePass = passwordBox[ChoosedObjNum].Password;
                passwordBox[ChoosedObjNum].Password = passwordBox[ButtonSenderNum].Password;
                passwordBox[ButtonSenderNum].Password = changePass;
                ActiveTeam.Player[ChoosedObjNum].Password = passwordBox[ChoosedObjNum].Password;
                ActiveTeam.Player[ButtonSenderNum].Password = passwordBox[ButtonSenderNum].Password;

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
                    if (ActiveTeam.Player[index].Toggled && WindowHelper.IsExist(ActiveTeam.Player[i].WindowTitle))
                        ActiveTeam.Player[index].IsStarted = Brushes.Green;
                    else
                        ActiveTeam.Player[index].IsStarted = Red;
                }
                await Task.Delay(15000);
            }
        }

        private async void StatsUpdate()
        {
            if (string.IsNullOrEmpty(ActiveTeam.TeamName) || !ActiveTeam.Player[0].Toggled || !ActiveTeam.Player[5].Toggled)
                return;
            Directory.CreateDirectory(ProgramSettings.CSGOFolder + @"\csgo\log\");
            Stream Team1logStream = File.Open(ProgramSettings.CSGOFolder + @"\csgo\log\0.log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            Stream Team2logStream = File.Open(ProgramSettings.CSGOFolder + @"\csgo\log\1.log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
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
                for (short i = 0; i < 10; i++)
                {
                    if (!accB[i] && ActiveTeam.Player[i].Toggled)
                    {
                        if (!string.IsNullOrEmpty(ActiveTeam.Player[i].Login) && ActiveTeam.Player[i].SteamID == "Unknown")
                        {
                            foreach (Team Team in TeamsCollection)
                                foreach (Player plr in Team.Player.Where(c => c.Login == ActiveTeam.Player[i].Login))
                                {
                                    plr.Nickname = accInfo[plr.Login.ToLower()].Value<string>("Nickname");
                                    plr.SteamID = accInfo[plr.Login.ToLower()].Value<string>("SteamID");
                                }
                        }
                        string RegExStr = Regex.Match(TeamString, @"(xuid u64[(] " + ActiveTeam.Player[i].SteamID + " .*? )prime", RegexOptions.Singleline).Value;
                        string accRank = Regex.Match(RegExStr, @"(?<=ranking int[(] )\d+", RegexOptions.Singleline).Value;
                        if (!string.IsNullOrEmpty(accRank) && Regex.Match(RegExStr, @"(?<=ranktype int[(] )\d+", RegexOptions.Singleline).Value != "0")
                        {
                            short accLevel = Int16.Parse(Regex.Match(RegExStr, @"(?<=level int[(] )\d+", RegexOptions.Singleline).Value);
                            string accXP = Regex.Match(RegExStr, @"(?<=327680{0,3})(0{1}|[1-9]\d*)", RegexOptions.Singleline | RegexOptions.RightToLeft).Value;
                            foreach (Team Team in TeamsCollection)
                                foreach (Player plr in Team.Player.Where(c => c.Login == ActiveTeam.Player[i].Login))
                                {
                                    plr.Level = accLevel;
                                    plr.XP = accXP;
                                    plr.Rank = "Images/" + accRank + ".png";
                                }
                            accB[i] = true;
                        }
                    }
                    else
                        accB[i] = true;
                }
                if (accB[0] && accB[1] && accB[2] && accB[3] && accB[4] && accB[5] && accB[6] && accB[7] && accB[8] && accB[9])
                {
                    done = true;
                    File.WriteAllText("LobbiesNew.json", JsonConvert.SerializeObject(TeamsCollection, Formatting.Indented));
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
            if (((Button)sender).Name == "SoundSettingsOn")
                TargetGrid = SoundSettings;
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

        private void CheckUpdatesButton(object sender, RoutedEventArgs e)
        {
            UpdateProgram(false);
        }

        private void UpdateProgram(bool silentUpdate)
        {
            string appVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
            string newVersion = CheckForNewVersion();

            if (newVersion == appVersion)
            {
                if(!silentUpdate)
                    MessageBox.Show("You have the latest version.", "", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!silentUpdate && MessageBox.Show("Update " + newVersion + " is available! You have installed: " + appVersion + ". Do you want to update ?", "Update is available", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
                return;

            try
            {
                var client = new WebClient();
                client.DownloadFile(new Uri(@"https://hippocratic-fishes.000webhostapp.com/IziBoost.zip"), "temp_myprogram");
                if (Directory.Exists("temp_myprogram_folder")) Directory.Delete("temp_myprogram_folder", true);
                ZipFile.ExtractToDirectory("temp_myprogram", "temp_myprogram_folder");
                if (File.Exists("updater.exe")) { File.Delete("updater.exe"); }
                File.Move("temp_myprogram_folder\\updater.exe", "updater.exe");
                if (on)
                    ProgramSettings.CSGOsRunning = true;
                Process.Start("updater.exe", "IziBoost.exe");
                Application_Exit(null, null);
            }
            catch (Exception) { }
        }

        private static String CheckForNewVersion()
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
                return version.Value<String>("actualversion");
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
