﻿using CSGSI;
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
using System.Linq;
using System.Media;
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
        private readonly SoundPlayer mediaPlayer = new SoundPlayer();
        public static List<string> T1WinTitle = new List<string>(), T2WinTitle = new List<string>();
        public JObject settingsObj, accInfo;
        public string loadedPreset;
        public short WinTeamNum, score;
        public List<string> accWindowsTitle = new List<string>();
        public List<string> accPos = new List<string> { "50 50", "50 50", "50 50", "50 50", "50 50", "50 50", "50 50", "50 50", "50 50", "50 50" };
        public static bool on = false, live = true, freezetime = true, loaded = false, choosed = false, sounds = false, MatchFoundSnd = true, MatchEndedSnd = true, RoundLastsSnd = true, newRound = true, onemeth = true;
        public static bool AutoAcceptS = false, PlayerStatusS = false, SoundS = false, AutoAcceptRestartS = false;
        public List<string>[] TWinTitle = { T2WinTitle, T1WinTitle };

        public Button choosedObj;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += LoadSettings;

            lobbiesList.DisplayMemberPath = "Name";
            lobbiesList.ItemsSource = _items;
            playersList.ItemsSource = _player;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString());
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

        public void Application_Exit(object sender, EventArgs e)
        {
            File.WriteAllText("Settings.json", JsonConvert.SerializeObject(settingsObj, Formatting.Indented));
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
            if (!File.Exists("Settings.json"))
                File.WriteAllText("Settings.json", "{}");
            settingsObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Settings.json"));
            if (settingsObj.Property("SteamFolder") != null)
            {
                SteamFolder.Text = settingsObj.Property("SteamFolder").Value.ToString();
                LoadSteamAccs();
            }
            if (settingsObj.Property("CSGOFolder") != null)
                CSGOFolder.Text = settingsObj.Property("CSGOFolder").Value.ToString();
            if (settingsObj.Property("LeaderResX") != null)
                LeaderResX.Text = settingsObj.Property("LeaderResX").Value.ToString();
            if (settingsObj.Property("LeaderResY") != null)
                LeaderResY.Text = settingsObj.Property("LeaderResY").Value.ToString();
            if (settingsObj.Property("BotResX") != null)
                BotResX.Text = settingsObj.Property("BotResX").Value.ToString();
            if (settingsObj.Property("BotResY") != null)
                BotResY.Text = settingsObj.Property("BotResY").Value.ToString();
            if (settingsObj.Property("AutoAccept") != null)
                AutoAccept.IsOn = settingsObj.Property("AutoAccept").Value.ToObject<bool>();
            if (settingsObj.Property("AutoDisconnect") != null)
                AutoDisconnect.IsOn = settingsObj.Property("AutoDisconnect").Value.ToObject<bool>();
            if (settingsObj.Property("Sounds") != null)
            {
                Sounds.IsOn = settingsObj.Property("Sounds").Value.ToObject<bool>();
                sounds = settingsObj.Property("Sounds").Value.ToObject<bool>();
            }
            if (settingsObj.Property("MatchFoundSound") != null)
            {
                MatchFoundSound.IsOn = settingsObj.Property("Sounds").Value.ToObject<bool>();
                MatchFoundSnd = settingsObj.Property("Sounds").Value.ToObject<bool>();
            }
            if (settingsObj.Property("MatchEndedSound") != null)
            {
                MatchEndedSound.IsOn = settingsObj.Property("Sounds").Value.ToObject<bool>();
                MatchEndedSnd = settingsObj.Property("Sounds").Value.ToObject<bool>();
            }
            if (settingsObj.Property("RoundLastsSound") != null)
            {
                RoundLastsSound.IsOn = settingsObj.Property("Sounds").Value.ToObject<bool>();
                RoundLastsSnd = settingsObj.Property("Sounds").Value.ToObject<bool>();
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
            SaveButton.IsEnabled = true;
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

        private async void Start(object sender, RoutedEventArgs e)
        {
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
            accWindowsTitle.Clear();
            T1WinTitle.Clear();
            T2WinTitle.Clear();
            TextBox[] Login = { Login1, Login2, Login3, Login4, Login5, Login6, Login7, Login8, Login9, Login10 };
            PasswordBox[] Password = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
            ToggleSwitch[] ToggleButton = { ToggleButton1, ToggleButton2, ToggleButton3, ToggleButton4, ToggleButton5, ToggleButton6, ToggleButton7, ToggleButton8, ToggleButton9, ToggleButton10 };
            string[] Names = { "LEADER", "BOT" }, Res = { LeaderResX.Text + " " + LeaderResY.Text, BotResX.Text + " " + BotResY.Text };
            for (short i = 0, n = 0, l = 0; i < 10; i++)
            {
                if (ToggleButton[i].IsOn)
                {
                    if (string.IsNullOrEmpty(Login[i].Text) || string.IsNullOrEmpty(Password[i].Password))
                    {
                        MessageBox.Show("Please type login or password");
                        return;
                    }
                    if (accInfo[Login[i].Text.ToLower()] == null)
                    {
                        LoadSteamAccs();
                        if (accInfo[Login[i].Text.ToLower()] == null)
                        {
                            MessageBox.Show("First login to this account: \"" + Login[i].Text + "\" and then try again");
                            return;
                        }
                    }
                    if (i < 5)
                    {
                        Logins.Add(Login[i].Text + " " + Password[i].Password + " " + accPos[i] + " " + Res[l] + " \"" + Names[l] + " #1\" " + n);
                        accWindowsTitle.Add("LOGIN: " + Login[i].Text.ToLower() + " | " + Names[l] + " #1");
                        T1WinTitle.Add("LOGIN: " + Login[i].Text.ToLower() + " | " + Names[l] + " #1");
                    }
                    else
                    {
                        Logins.Add(Login[i].Text + " " + Password[i].Password + " " + accPos[i] + " " + Res[l] + " \"" + Names[l] + " #2\" " + (n + 2));
                        accWindowsTitle.Add("LOGIN: " + Login[i].Text.ToLower() + " | " + Names[l] + " #2");
                        T2WinTitle.Add("LOGIN: " + Login[i].Text.ToLower() + " | " + Names[l] + " #2");
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
                MessageBox.Show("Please turn on at least one account");
                return;
            }
            controlContainer.IsEnabled = false;
            ClearButton.IsEnabled = false;
            for (int i = 0; i < Logins.Count; i++)
            {
                if (controlContainer.IsEnabled)
                {
                    return;
                }
                Process.Start("Launcher.exe", "false \"" + settingsObj["SteamFolder"].ToString() + "\" " + Logins[i] + " \"" + settingsObj["CSGOFolder"].ToString() + "\" ");
                if (!on)
                    return;
                await Task.Delay(4000);
            }
            if (!(bool)WinTeam1.IsChecked && !(bool)WinTeam2.IsChecked && !(bool)WinTeamTie.IsChecked)
                WinTeam1.IsChecked = true;
            on = true;
            if (!gslT1.Start())
                MessageBox.Show("Cannot start GameStateListener #1. AutoDisconnect won't work! Try reboot your PC");
            if (!gslT2.Start())
                MessageBox.Show("Cannot start GameStateListener #2. AutoDisconnect won't work! Try reboot your PC");
            if (AutoAccept.IsOn)
            {
                _ = Task.Run(() => AutoAcceptFunc());
            }
            if (AutoDisconnect.IsOn)
            {
                _ = Task.Run(() => AutoDisconnectFunc(false));
            }
            _ = Task.Run(() => IndicatorsOn());
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
                for (int i = 0, n = 0; i < 10; i++)
                {
                    if (ToggleButton[i].IsOn)
                    {
                        if (FindWindow(null, accWindowsTitle[n]).ToInt32() != 0)
                        {
                            int x;
                            Rect WindowRect = new Rect();
                            if (IsIconic(FindWindow(null, accWindowsTitle[n])))
                                ShowWindow(FindWindow(null, accWindowsTitle[n]), 4);
                            GetWindowRect(FindWindow(null, accWindowsTitle[n]), ref WindowRect);
                            if (WindowRect.Left < 0)
                                x = 0;
                            else
                                x = WindowRect.Left;
                            if (!string.IsNullOrEmpty(loadedPreset))
                                lobbiesObj[loadedPreset]["Acc" + (i + 1)]["Pos"] = x + " " + WindowRect.Top;
                            accPos[i] = x + " " + WindowRect.Top;
                        }
                        n++;
                    }
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

            AutoDisconnectFunc(true);
            if (gslT1.Running)
                gslT1.Stop();
            if (gslT2.Running)
                gslT2.Stop();
            System.Windows.Shapes.Ellipse[] Status = { Status1, Status2, Status3, Status4, Status5, Status6, Status7, Status8, Status9, Status10, AutoAcceptStatus, PlayerStatus };
            for (short i = 0; i < 12; i++)
                Status[i].Fill = (Brush)new BrushConverter().ConvertFrom("#FFA20404");
            on = false;
            AutoAcceptS = false; PlayerStatusS = false; SoundS = false; AutoAcceptRestartS = false;
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
                    lobbiesObj[PresetName.Text]["Acc" + (i + 1)]["Toggled"] = ToggleButton[i].IsOn;
                    lobbiesObj[PresetName.Text]["Acc" + (i + 1)]["Login"] = Login[i].Text.ToLower();
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
                    PresetNameObj.Add("Acc" + (i + 1), new JObject(new JProperty("Toggled", ToggleButton[i].IsOn), new JProperty("Login", Login[i].Text.ToLower()), new JProperty("Password", Password[i].Password), new JProperty("Pos", accPos[i]), new JProperty("SteamID64", accInfo[Login[i].Text.ToLower()]?["SteamID"] ?? "Unknown"), new JProperty("Nickname", accInfo[Login[i].Text.ToLower()]?["Nickname"] ?? "Unknown"), new JProperty("Level", 0), new JProperty("XP", ""), new JProperty("Rank", "0")));
                }
                loadedPreset = PresetName.Text;
                for (short i = 0; i < 10; i++)
                {
                    _player.Add(new Player(lobbiesObj[loadedPreset].Value<JToken>("Acc" + (i + 1)).Value<string>("Login"), lobbiesObj[loadedPreset].Value<JToken>("Acc" + (i + 1)).Value<string>("Nickname"), lobbiesObj[loadedPreset].Value<JToken>("Acc" + (i + 1)).Value<short>("Level"), lobbiesObj[loadedPreset].Value<JToken>("Acc" + (i + 1)).Value<string>("XP"), "Images/" + (lobbiesObj[loadedPreset].Value<JToken>("Acc" + (i + 1)).Value<string>("Rank") ?? "0") + ".png", loadedPreset, "Collapsed"));
                }
                _player[_player.Count - 1].Visibility = "Visible";
                MessageBox.Show("Preset successfully saved");
            }
            File.WriteAllText("Lobbies.json", JsonConvert.SerializeObject(lobbiesObj, Formatting.Indented));
            if (_items.Any(x => x.Name == PresetName.Text) == false)
                _items.Add(new Item(PresetName.Text));
        }

        private void ClearText(object sender, RoutedEventArgs e)
        {
            TextBox[] Login = { Login1, Login2, Login3, Login4, Login5, Login6, Login7, Login8, Login9, Login10 };
            PasswordBox[] Password = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
            for(short i = 0; i < 10; i++)
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
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Cancel)
                return;
            settingsObj[((Button)sender).Tag] = dialog.FileName;
            if (((Button)sender).Tag.ToString() == "SteamFolder")
            {
                SteamFolder.Text = dialog.FileName;
                LoadSteamAccs();
            }
            else
                CSGOFolder.Text = dialog.FileName;
        }

        private void SaveSettings(object sender, TextChangedEventArgs e)
        {
            if (!loaded)
                return;
            settingsObj[((TextBox)sender).Name] = ((TextBox)sender).Text;
        }

        private void AutoAcceptTgl(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                return;
            settingsObj[((ToggleSwitch)sender).Name] = ((ToggleSwitch)sender).IsOn;
        }

        private void AutoDisconnectTgl(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                return;
            settingsObj[((ToggleSwitch)sender).Name] = ((ToggleSwitch)sender).IsOn;
            if (((ToggleSwitch)sender).IsOn && on)
            {
                Task.Run(() => AutoDisconnectFunc(false));
            }
            if (!((ToggleSwitch)sender).IsOn && on)
            {
                Task.Run(() => AutoDisconnectFunc(true));
            }
        }

        private void SoundsTgl(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                return;
            settingsObj[((ToggleSwitch)sender).Name] = ((ToggleSwitch)sender).IsOn;
            sounds = ((ToggleSwitch)sender).IsOn;
        }
        private void MatchFoundTgl(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                return;
            settingsObj[((ToggleSwitch)sender).Name] = ((ToggleSwitch)sender).IsOn;
            MatchFoundSnd = ((ToggleSwitch)sender).IsOn;
        }
        private void MatchEndedTgl(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                return;
            settingsObj[((ToggleSwitch)sender).Name] = ((ToggleSwitch)sender).IsOn;
            MatchEndedSnd = ((ToggleSwitch)sender).IsOn;
        }
        private void RoundLastsTgl(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                return;
            settingsObj[((ToggleSwitch)sender).Name] = ((ToggleSwitch)sender).IsOn;
            RoundLastsSnd = ((ToggleSwitch)sender).IsOn;
        }


        private void LoadPreset(object sender, MouseButtonEventArgs e)
        {
            if (lobbiesList.SelectedItem == null)
                return;
            JObject lobbiesObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Lobbies.json"));
            JObject AccObj = lobbiesObj.Property(_items[lobbiesList.SelectedIndex].Name).Value.ToObject<JObject>();
            TextBox[] Login = { Login1, Login2, Login3, Login4, Login5, Login6, Login7, Login8, Login9, Login10 };
            PasswordBox[] Password = { Password1, Password2, Password3, Password4, Password5, Password6, Password7, Password8, Password9, Password10 };
            ToggleSwitch[] ToggleButton = { ToggleButton1, ToggleButton2, ToggleButton3, ToggleButton4, ToggleButton5, ToggleButton6, ToggleButton7, ToggleButton8, ToggleButton9, ToggleButton10 };
            loadedPreset = _items[lobbiesList.SelectedIndex].Name;
            accPos.Clear();
            for (int i = 0; i < 10; i++)
            {
                ToggleButton[i].IsOn = bool.Parse(AccObj["Acc" + (i + 1)].Value<string>("Toggled"));
                Login[i].Text = AccObj["Acc" + (i + 1)].Value<string>("Login");
                Password[i].Password = AccObj["Acc" + (i + 1)].Value<string>("Password");
                accPos.Add(lobbiesObj[loadedPreset]["Acc" + (i + 1)]["Pos"].ToString());
            }
            PresetName.Text = loadedPreset;
            MessageBox.Show("Successfully loaded");
        }

        private void LoadSteamAccs()
        {
            accInfo = new JObject();
            string info = File.ReadAllText(settingsObj["SteamFolder"].ToString() + @"/config/loginusers.vdf");
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
                AutoAcceptStatus.Fill = Brushes.Green; AutoAcceptS = true;
            });
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

        private void IndicatorsOn()
        {
            gslT1.NewGameState -= Indicators;
            gslT2.NewGameState -= Indicators;
            gslT1.NewGameState += Indicators;
            gslT2.NewGameState += Indicators;
            gslT1.RoundPhaseChanged -= RoundLasts;
            gslT2.RoundPhaseChanged -= RoundLasts;
            gslT1.RoundPhaseChanged += RoundLasts;
            gslT2.RoundPhaseChanged += RoundLasts;

            void Indicators(GameState a)
            {

                if (a.Map.Phase.ToString() == "GameOver")
                {
                    if (settingsObj.Value<bool>("AutoAccept") && !AutoAcceptS)
                    {
                        AutoAcceptS = true;
                        _ = Task.Run(() => AutoAcceptFunc());
                    }
                    if (!PlayerStatusS)
                    {
                        PlayerStatusS = true;
                        InvokeUI(() =>
                        {
                            StatsUpdate();
                        });
                    }
                    if (sounds && MatchEndedSnd && !SoundS)
                    {
                        SoundS = true;
                        newRound = false;
                        mediaPlayer.Stream = Properties.Resources.MatchEnded;
                        mediaPlayer.Load();
                        mediaPlayer.Play();
                    }
                }
                if (a.Map.Phase.ToString() == "Warmup" && SoundS)
                {
                    SoundS = false;
                }
            }
            async void RoundLasts(RoundPhaseChangedEventArgs a)
            {
                if (a.CurrentPhase.ToString() == "Live" && onemeth)
                {
                    onemeth = false;
                    Stopwatch at = new Stopwatch();
                    newRound = true;
                    at.Start();
                    while (newRound)
                    {
                        if (at.Elapsed.TotalSeconds >= 35)
                        {
                            if (sounds && RoundLastsSnd)
                            {
                                mediaPlayer.Stream = Properties.Resources.RoundLasts;
                                mediaPlayer.Load();
                                mediaPlayer.Play();
                            }
                            break;
                        }
                        await Task.Delay(5000);
                    }
                    onemeth = true;
                    return;
                }
                if (a.CurrentPhase.ToString() == "FreezeTime")
                {
                    newRound = false;
                }
            }
        }
        private void AutoDisconnectFunc(bool disable)
        {
            gslT1.RoundPhaseChanged -= RoundFast;
            gslT2.RoundPhaseChanged -= RoundFast;
            gslT1.RoundPhaseChanged -= Round;
            gslT2.RoundPhaseChanged -= Round;
            gslT1.NewGameState -= RoundHalf;
            gslT2.NewGameState -= RoundHalf;
            gslT1.NewGameState -= RoundHalfScore;
            gslT2.NewGameState -= RoundHalfScore;
            gslT1.NewGameState -= RoundGameOver;
            gslT2.NewGameState -= RoundGameOver;
            if (disable)
            {
                gslT1.NewGameState -= RoundWarmup;
                gslT2.NewGameState -= RoundWarmup;
                return;
            }
            WinTeam.RoundPhaseChanged += Round;
            WinTeam.NewGameState += RoundGameOver;

            if (settingsObj.Value<short>("WinTeam") == 2)
            {
                WinTeam.NewGameState += RoundHalfScore;
            }

            async void Round(RoundPhaseChangedEventArgs a)
            {
                if (a.CurrentPhase.ToString() == "Live")
                {
                    if (live)
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
                    }
                }
                if (a.CurrentPhase.ToString() == "FreezeTime")
                {
                    if (live)
                    {
                        await Task.Delay(5500);
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
                    }
                    live = !live;
                }
            }

            async void RoundFast(RoundPhaseChangedEventArgs a)
            {
                if (a.CurrentPhase.ToString() == "FreezeTime")
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
                }
            }

            void RoundGameOver(GameState a)
            {
                if (a.Map.Phase.ToString() == "GameOver")
                {
                    live = true;
                    score = 0;
                    if (settingsObj.Value<short>("WinTeam") == 2)
                    {
                        WinTeam = gslT1;
                        WinTeamNum = 0;
                        gslT1.RoundPhaseChanged -= Round;
                        gslT2.RoundPhaseChanged -= Round;
                        WinTeam.RoundPhaseChanged += Round;
                        gslT1.NewGameState -= RoundHalf;
                        gslT2.NewGameState -= RoundHalf;
                        WinTeam.NewGameState += RoundHalf;
                    }
                    gslT1.NewGameState -= RoundGameOver;
                    gslT2.NewGameState -= RoundGameOver;
                    WinTeam.NewGameState += RoundWarmup;
                }
            }
            void RoundWarmup(GameState a)
            {
                if (a.Map.Phase.ToString() == "Warmup")
                {
                    gslT1.NewGameState -= RoundWarmup;
                    gslT2.NewGameState -= RoundWarmup;
                    WinTeam.NewGameState += RoundGameOver;
                }
            }

            void RoundHalfScore(GameState a)
            {
                if (Convert.ToInt16(a.Map.Round) < 15 && Convert.ToInt16(a.Map.Round) > -1)
                {
                    if (WinTeam == gslT2 || WinTeamNum == 1)
                    {
                        WinTeam = gslT1;
                        WinTeamNum = 0;
                    }
                    score = Convert.ToInt16(a.Map.Round);
                    gslT1.NewGameState -= RoundHalfScore;
                    gslT2.NewGameState -= RoundHalfScore;
                    WinTeam.NewGameState += RoundHalf;
                }
                if (Convert.ToInt16(a.Map.Round) > 15)
                {
                    if (WinTeam == gslT1 || WinTeamNum == 0)
                    {
                        WinTeam = gslT2;
                        WinTeamNum = 1;
                    }
                    score = Convert.ToInt16(a.Map.Round);
                    gslT1.NewGameState -= RoundHalfScore;
                    gslT2.NewGameState -= RoundHalfScore;
                    WinTeam.NewGameState += RoundHalf;
                }
            }

            void RoundHalf(GameState a)
            {
                if (Convert.ToInt16(a.Map.Round) == 12 && score == 12)
                {
                    live = !live;
                    gslT1.RoundPhaseChanged -= RoundFast;
                    gslT2.RoundPhaseChanged -= RoundFast;
                    WinTeam.RoundPhaseChanged += RoundFast;
                }
                if (Convert.ToInt16(a.Map.Round) == 15 && score == 15)
                {
                    live = !live;
                    WinTeam = gslT2;
                    WinTeamNum = 1;
                    gslT1.RoundPhaseChanged -= Round;
                    gslT2.RoundPhaseChanged -= Round;
                    WinTeam.RoundPhaseChanged += Round;
                    gslT1.NewGameState -= RoundHalf;
                    gslT2.NewGameState -= RoundHalf;
                    WinTeam.NewGameState += RoundHalf;
                    gslT1.RoundPhaseChanged -= RoundFast;
                    gslT2.RoundPhaseChanged -= RoundFast;
                    WinTeam.RoundPhaseChanged += RoundFast;
                }
                if (Convert.ToInt16(a.Map.Round) == 13 && score == 13 || Convert.ToInt16(a.Map.Round) == 16 && score == 16)
                {
                    gslT1.RoundPhaseChanged -= RoundFast;
                    gslT2.RoundPhaseChanged -= RoundFast;
                }

                if (Convert.ToInt16(a.Map.Round) == score) score++;
            }
        }

        private async void RestartSearch(bool t2)
        {
            InvokeUI(() =>
            {
                AutoAcceptStatus.Fill = Brushes.Yellow;
            });
            List<String> ldrTitles = new List<String>();
            for (int i = 0; i < accWindowsTitle.Count; i++)
            {
                if (accWindowsTitle[i].Contains("LEADER"))
                    ldrTitles.Add(accWindowsTitle[i]);
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
        }

        private void AcceptGame()
        {
            if (sounds && MatchEndedSnd)
            {
                mediaPlayer.Stream = Properties.Resources.MatchFound;
                mediaPlayer.Load();
                mediaPlayer.Play();
            }
            for (int i = 0; i < accWindowsTitle.Count; i++)
            {
                if (IsIconic(FindWindow(null, accWindowsTitle[i])))
                    ShowWindow(FindWindow(null, accWindowsTitle[i]), 9);
                SetForegroundWindow(FindWindow(null, accWindowsTitle[i]));
                Rect WindowRect = new Rect();
                Coords CSGO = new Coords();
                GetWindowRect(FindWindow(null, accWindowsTitle[i]), ref WindowRect);
                ClientToScreen(FindWindow(null, accWindowsTitle[i]), ref CSGO);
                Thread.Sleep(250);
                LeftClick(Convert.ToInt16((WindowRect.Right - WindowRect.Left - 6) / 2.35 + (WindowRect.Right - WindowRect.Left - 6) / 6.6 / 2) + CSGO.x, Convert.ToInt16(WindowRect.Bottom - WindowRect.Top - 29 - (WindowRect.Bottom - WindowRect.Top - 29) / 2.52 - (WindowRect.Bottom - WindowRect.Top - 29) / 12.5 / 2) + CSGO.y);
                Thread.Sleep(250);
            }
            InvokeUI(() =>
            {
                AutoAcceptStatus.Fill = (Brush)new BrushConverter().ConvertFrom("#FFA20404"); AutoAcceptS = false;
            });
        }

        private void WinTeam1_Checked(object sender, RoutedEventArgs e)
        {
            settingsObj["WinTeam"] = 0;
            WinTeamNum = 0;
            WinTeam = gslT1;
            if (on && AutoDisconnect.IsOn)
            {
                _ = Task.Run(() => AutoDisconnectFunc(false));
            }
        }

        private void WinTeam2_Checked(object sender, RoutedEventArgs e)
        {
            settingsObj["WinTeam"] = 1;
            WinTeamNum = 1;
            WinTeam = gslT2;
            if (on && AutoDisconnect.IsOn)
            {
                _ = Task.Run(() => AutoDisconnectFunc(false));
            }
        }

        private void WinTeamTie_Checked(object sender, RoutedEventArgs e)
        {
            settingsObj["WinTeam"] = 2;
            WinTeamNum = 0;
            WinTeam = gslT1;

            if (on && AutoDisconnect.IsOn)
            {
                _ = Task.Run(() => AutoDisconnectFunc(false));
            }
        }

        private void PlayOneFunc(object sender, RoutedEventArgs e)
        {
            if (!((ToggleSwitch)this.GetType().GetField("ToggleButton" + ((Button)sender).Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).IsOn ||
                ((TextBox)this.GetType().GetField("Login" + ((Button)sender).Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Text == "" ||
                ((PasswordBox)this.GetType().GetField("Password" + ((Button)sender).Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Password == "")
                return;
            string res;
            if (Convert.ToInt16(((Button)sender).Tag) == 1 || Convert.ToInt16(((Button)sender).Tag) == 6)
                res = LeaderResX.Text + " " + LeaderResY.Text;
            else
                res = BotResX.Text + " " + BotResY.Text;
            Process.Start("Launcher.exe", "true \"" + settingsObj["SteamFolder"].ToString() + "\" " +
                ((TextBox)this.GetType().GetField("Login" + ((Button)sender).Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Text +
                " " + ((PasswordBox)this.GetType().GetField("Password" + ((Button)sender).Tag.ToString(), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this)).Password +
                " " + accPos[Convert.ToInt16(((Button)sender).Tag)-1] + " " + res);
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

        private void AutomationTgl(object sender, RoutedEventArgs e)
        {
            List<String> ldrTitles = new List<String>();
            for (int i = 0; i < accWindowsTitle.Count; i++)
            {
                if (accWindowsTitle[i].Contains("LEADER"))
                    ldrTitles.Add(accWindowsTitle[i]);
            }
            List<string> TeamWinTitle = T1WinTitle;
            for (int i = 1, n = 0; n < 2; i++)
            {
                Rect WindowRect = new Rect();
                Coords CSGO = new Coords();
                if (IsIconic(FindWindow(null, TeamWinTitle[i])))
                    ShowWindow(FindWindow(null, TeamWinTitle[i]), 9);
                SetForegroundWindow(FindWindow(null, TeamWinTitle[i]));
                GetWindowRect(FindWindow(null, TeamWinTitle[i]), ref WindowRect);
                ClientToScreen(FindWindow(null, TeamWinTitle[i]), ref CSGO);
                Thread.Sleep(500);
                SetCursorPos(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 30.476) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 4.285) + CSGO.y);
                Thread.Sleep(500);
                LeftClick(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 30.476) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 4.285) + CSGO.y);
                Thread.Sleep(500);
                LeftClick(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 4.353) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 3.404) + CSGO.y);
                Thread.Sleep(500);
                LeftClick(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 2.154) + CSGO.x, Convert.ToInt16(WindowRect.Bottom - WindowRect.Top - 29 - (WindowRect.Bottom - WindowRect.Top - 29) / 2.364) + CSGO.y);
                Thread.Sleep(500);
                SendKeyPress(0x1);

                Rect WindowRect2 = new Rect();
                Coords CSGO2 = new Coords();
                if (IsIconic(FindWindow(null, ldrTitles[n])))
                    ShowWindow(FindWindow(null, ldrTitles[n]), 9);
                SetForegroundWindow(FindWindow(null, ldrTitles[n]));
                GetWindowRect(FindWindow(null, ldrTitles[n]), ref WindowRect2);
                ClientToScreen(FindWindow(null, ldrTitles[n]), ref CSGO2);
                Thread.Sleep(500);
                SetCursorPos(Convert.ToInt16(WindowRect2.Right - WindowRect2.Left - 6 - (WindowRect2.Right - WindowRect2.Left - 6) / 30.476) + CSGO2.x, Convert.ToInt16((WindowRect2.Bottom - WindowRect2.Top - 29) / 4.285) + CSGO2.y);
                Thread.Sleep(500);
                LeftClick(Convert.ToInt16(WindowRect2.Right - WindowRect2.Left - 6 - (WindowRect2.Right - WindowRect2.Left - 6) / 30.476) + CSGO2.x, Convert.ToInt16((WindowRect2.Bottom - WindowRect2.Top - 29) / 4.285) + CSGO2.y);
                Thread.Sleep(500);
                LeftClick(Convert.ToInt16(WindowRect2.Right - WindowRect2.Left - 6 - (WindowRect2.Right - WindowRect2.Left - 6) / 4.353) + CSGO2.x, Convert.ToInt16((WindowRect2.Bottom - WindowRect2.Top - 29) / 3.404) + CSGO2.y);
                Thread.Sleep(500);
                SetCursorPos(Convert.ToInt16((WindowRect2.Right - WindowRect2.Left - 6) / 2.310) + CSGO2.x, Convert.ToInt16((WindowRect2.Bottom - WindowRect2.Top - 29) / 2.060) + CSGO2.y);
                Thread.Sleep(500);
                LeftClick(Convert.ToInt16((WindowRect2.Right - WindowRect2.Left - 6) / 2.310) + CSGO2.x, Convert.ToInt16((WindowRect2.Bottom - WindowRect2.Top - 29) / 2.060) + CSGO2.y);
                Thread.Sleep(500);
                SendKeyPress(0x1D, 0x2F);
                Thread.Sleep(500);
                LeftClick(Convert.ToInt16(WindowRect2.Right - WindowRect2.Left - 6 - (WindowRect2.Right - WindowRect2.Left - 6) / 2.245) + CSGO2.x, Convert.ToInt16((WindowRect2.Bottom - WindowRect2.Top - 29) / 2.060) + CSGO2.y);
                Thread.Sleep(500);
                LeftClick(Convert.ToInt16((WindowRect2.Right - WindowRect2.Left - 6) / 2.452) + CSGO2.x, Convert.ToInt16(WindowRect2.Bottom - WindowRect2.Top - 29 - (WindowRect2.Bottom - WindowRect2.Top - 29) / 2.096) + CSGO2.y);
                Thread.Sleep(500);
                LeftClick(Convert.ToInt16(WindowRect2.Right - WindowRect2.Left - 6 - (WindowRect2.Right - WindowRect2.Left - 6) / 3.855) + CSGO2.x, Convert.ToInt16(WindowRect2.Bottom - WindowRect2.Top - 29 - (WindowRect2.Bottom - WindowRect2.Top - 29) / 2.096) + CSGO2.y);
                Thread.Sleep(500);
                LeftClick(Convert.ToInt16(WindowRect2.Right - WindowRect2.Left - 6 - (WindowRect2.Right - WindowRect2.Left - 6) / 2.7) + CSGO2.x, Convert.ToInt16(WindowRect2.Bottom - WindowRect2.Top - 29 - (WindowRect2.Bottom - WindowRect2.Top - 29) / 2.594) + CSGO2.y);
                Thread.Sleep(500);

                if (i == (T1WinTitle.Count - 1) && n == 0)
                {
                    n++;
                    TeamWinTitle = T2WinTitle;
                    i = 0;
                }
                if (i == (T2WinTitle.Count - 1) && n == 1)
                    n++;
            }

            TeamWinTitle = T1WinTitle;
            for (int i = 1, n = 0; n < 2; i++)
            {
                Rect WindowRect = new Rect();
                Coords CSGO = new Coords();
                if (IsIconic(FindWindow(null, TeamWinTitle[i])))
                    ShowWindow(FindWindow(null, TeamWinTitle[i]), 9);
                SetForegroundWindow(FindWindow(null, TeamWinTitle[i]));
                GetWindowRect(FindWindow(null, TeamWinTitle[i]), ref WindowRect);
                ClientToScreen(FindWindow(null, TeamWinTitle[i]), ref CSGO);
                Thread.Sleep(250);
                SetCursorPos(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 30.476) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 3.779) + CSGO.y);
                Thread.Sleep(250);
                LeftClick(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 5.423) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 3.809) + CSGO.y);
                Thread.Sleep(250);
                if (i == (T1WinTitle.Count - 1) && n == 0)
                {
                    n++;
                    TeamWinTitle = T2WinTitle;
                    i = 0;
                }
                if (i == (T2WinTitle.Count - 1) && n == 1)
                    n++;
            }
        }

        private void CPUReducer(object sender, RoutedEventArgs e)
        {
            Process.Start(@"BES\BES.exe");
        }

        private async void AccountChecker()
        {
            System.Windows.Shapes.Ellipse[] Status = { Status1, Status2, Status3, Status4, Status5, Status6, Status7, Status8, Status9, Status10 };
            ToggleSwitch[] ToggleButton = { ToggleButton1, ToggleButton2, ToggleButton3, ToggleButton4, ToggleButton5, ToggleButton6, ToggleButton7, ToggleButton8, ToggleButton9, ToggleButton10 };
            while (on)
            {
                for (short i = 0, n = 0; i < 10; i++)
                {
                    short index = i;
                    if (ToggleButton[index].IsOn && FindWindow(null, accWindowsTitle[n++]).ToInt32() != 0)
                    {
                        Status[index].Fill = Brushes.Green;
                    }
                    else
                    {
                        Status[index].Fill = (Brush)new BrushConverter().ConvertFrom("#FFA20404");
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
            PlayerStatus.Fill = Brushes.Green; PlayerStatusS = true;
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
            PlayerStatus.Fill = (Brush)new BrushConverter().ConvertFrom("#FFA20404"); PlayerStatusS = false;
            Team1logStream.Close();
            Team2logStream.Close();
            Team1log.Close();
            Team2log.Close();
        }

        private void Test(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Testing Button");
        }

        private void SoundSettingsOn(object sender, RoutedEventArgs e)
        {
            if (!SoundSettings.IsVisible)
                SoundSettings.Visibility = Visibility.Visible;
            else
                SoundSettings.Visibility = Visibility.Collapsed;
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

        public GameStateListener WinTeam;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr handle);
    }
}
