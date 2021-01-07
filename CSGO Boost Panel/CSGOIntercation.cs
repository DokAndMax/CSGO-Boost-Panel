using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static CSGO_Boost_Panel.MainWindow;

namespace CSGO_Boost_Panel
{
    class CSGOIntercation
    {
        static bool first = true;
        public async static Task StartSearching(int num)
        {
            String ldr1Title = PArray[0].WindowTitle, ldr2Title = PArray[5].WindowTitle;
            switch (num)
            {
                case 1:
                    {
                        if (IsIconic(FindWindow(null, ldr1Title)))
                            ShowWindow(FindWindow(null, ldr1Title), 9);
                        SetForegroundWindow(FindWindow(null, ldr1Title));
                        Rect WindowRect = new Rect();
                        Coords CSGO = new Coords();
                        GetWindowRect(FindWindow(null, ldr1Title), ref WindowRect);
                        ClientToScreen(FindWindow(null, ldr1Title), ref CSGO);
                        await Task.Delay(600);
                        LeftClick(Convert.ToInt16((WindowRect.Right - WindowRect.Left - 6) / 29.52) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 7.35) + CSGO.y);
                        await Task.Delay(600);
                        LeftClick(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 3.36) + CSGO.x, Convert.ToInt16(WindowRect.Bottom - WindowRect.Top - 29 - (WindowRect.Bottom - WindowRect.Top - 29) / 22.85) + CSGO.y);
                        await Task.Delay(600);
                    }
                    break;
                case 2:
                    {
                        if (IsIconic(FindWindow(null, ldr2Title)))
                            ShowWindow(FindWindow(null, ldr2Title), 9);
                        SetForegroundWindow(FindWindow(null, ldr2Title));
                        Rect WindowRect = new Rect();
                        Coords CSGO = new Coords();
                        GetWindowRect(FindWindow(null, ldr2Title), ref WindowRect);
                        ClientToScreen(FindWindow(null, ldr2Title), ref CSGO);
                        await Task.Delay(600);
                        LeftClick(Convert.ToInt16((WindowRect.Right - WindowRect.Left - 6) / 29.52) + CSGO.x, Convert.ToInt16( (WindowRect.Bottom - WindowRect.Top - 29) / 7.35) + CSGO.y);
                        await Task.Delay(600);
                        LeftClick(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 3.36) + CSGO.x, Convert.ToInt16(WindowRect.Bottom - WindowRect.Top - 29 - (WindowRect.Bottom - WindowRect.Top - 29) / 22.85) + CSGO.y);
                        await Task.Delay(600);
                    }
                    break;
                case 3:
                    {
                        string[] ldrTitles = new string[] { ldr1Title, ldr2Title };
                        for (int i = 0; i < ldrTitles.Length; i++)
                        {
                            if (IsIconic(FindWindow(null, ldrTitles[i])))
                                ShowWindow(FindWindow(null, ldrTitles[i]), 9);
                            SetForegroundWindow(FindWindow(null, ldrTitles[i]));
                            Rect WindowRect = new Rect();
                            Coords CSGO = new Coords();
                            GetWindowRect(FindWindow(null, ldrTitles[i]), ref WindowRect);
                            ClientToScreen(FindWindow(null, ldrTitles[i]), ref CSGO);
                            await Task.Delay(600);
                            LeftClick(Convert.ToInt16((WindowRect.Right - WindowRect.Left - 6) / 29.52) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 7.35) + CSGO.y);
                            await Task.Delay(600);
                            LeftClick(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 3.36) + CSGO.x, Convert.ToInt16(WindowRect.Bottom - WindowRect.Top - 29 - (WindowRect.Bottom - WindowRect.Top - 29) / 22.85) + CSGO.y);
                            await Task.Delay(600);
                        }
                    }
                    break;
            }
        }

        public async static Task GatherLobby()
        {
            if (first)
            {
                for(int i = 9; i > -1; i--)
                {
                    ShowWindow(FindWindow(null, PArray[i].WindowTitle), 9);
                    await Task.Delay(250);
                }
                first = false;
            }
            List<String> ldrTitles = new List<String>();
            if (PArray[0].WindowTitle.Contains("LEADER"))
                ldrTitles.Add(PArray[0].WindowTitle);
            if (PArray[5].WindowTitle.Contains("LEADER"))
                ldrTitles.Add(PArray[5].WindowTitle);
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
                await Task.Delay(500);
                SetCursorPos(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 30.476) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 4.285) + CSGO.y);
                await Task.Delay(500);
                LeftClick(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 30.476) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 4.285) + CSGO.y);
                await Task.Delay(500);
                LeftClick(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 4.353) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 3.404) + CSGO.y);
                await Task.Delay(500);
                LeftClick(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 2.154) + CSGO.x, Convert.ToInt16(WindowRect.Bottom - WindowRect.Top - 29 - (WindowRect.Bottom - WindowRect.Top - 29) / 2.364) + CSGO.y);
                await Task.Delay(500);
                SendKeyPress(0x1);

                Rect WindowRect2 = new Rect();
                Coords CSGO2 = new Coords();
                if (IsIconic(FindWindow(null, ldrTitles[n])))
                    ShowWindow(FindWindow(null, ldrTitles[n]), 9);
                SetForegroundWindow(FindWindow(null, ldrTitles[n]));
                GetWindowRect(FindWindow(null, ldrTitles[n]), ref WindowRect2);
                ClientToScreen(FindWindow(null, ldrTitles[n]), ref CSGO2);
                await Task.Delay(500);
                SetCursorPos(Convert.ToInt16(WindowRect2.Right - WindowRect2.Left - 6 - (WindowRect2.Right - WindowRect2.Left - 6) / 30.476) + CSGO2.x, Convert.ToInt16((WindowRect2.Bottom - WindowRect2.Top - 29) / 4.285) + CSGO2.y);
                await Task.Delay(500);
                LeftClick(Convert.ToInt16(WindowRect2.Right - WindowRect2.Left - 6 - (WindowRect2.Right - WindowRect2.Left - 6) / 30.476) + CSGO2.x, Convert.ToInt16((WindowRect2.Bottom - WindowRect2.Top - 29) / 4.285) + CSGO2.y);
                await Task.Delay(500);
                LeftClick(Convert.ToInt16(WindowRect2.Right - WindowRect2.Left - 6 - (WindowRect2.Right - WindowRect2.Left - 6) / 4.353) + CSGO2.x, Convert.ToInt16((WindowRect2.Bottom - WindowRect2.Top - 29) / 3.404) + CSGO2.y);
                await Task.Delay(500);
                SetCursorPos(Convert.ToInt16((WindowRect2.Right - WindowRect2.Left - 6) / 2.310) + CSGO2.x, Convert.ToInt16((WindowRect2.Bottom - WindowRect2.Top - 29) / 2.060) + CSGO2.y);
                await Task.Delay(500);
                LeftClick(Convert.ToInt16((WindowRect2.Right - WindowRect2.Left - 6) / 2.310) + CSGO2.x, Convert.ToInt16((WindowRect2.Bottom - WindowRect2.Top - 29) / 2.060) + CSGO2.y);
                await Task.Delay(500);
                SendKeyPress(0x1D, 0x2F);
                await Task.Delay(500);
                LeftClick(Convert.ToInt16(WindowRect2.Right - WindowRect2.Left - 6 - (WindowRect2.Right - WindowRect2.Left - 6) / 2.245) + CSGO2.x, Convert.ToInt16((WindowRect2.Bottom - WindowRect2.Top - 29) / 2.060) + CSGO2.y);
                await Task.Delay(500);
                LeftClick(Convert.ToInt16((WindowRect2.Right - WindowRect2.Left - 6) / 2.452) + CSGO2.x, Convert.ToInt16(WindowRect2.Bottom - WindowRect2.Top - 29 - (WindowRect2.Bottom - WindowRect2.Top - 29) / 2.096) + CSGO2.y);
                await Task.Delay(500);
                LeftClick(Convert.ToInt16(WindowRect2.Right - WindowRect2.Left - 6 - (WindowRect2.Right - WindowRect2.Left - 6) / 3.855) + CSGO2.x, Convert.ToInt16(WindowRect2.Bottom - WindowRect2.Top - 29 - (WindowRect2.Bottom - WindowRect2.Top - 29) / 2.096) + CSGO2.y);
                await Task.Delay(500);
                LeftClick(Convert.ToInt16(WindowRect2.Right - WindowRect2.Left - 6 - (WindowRect2.Right - WindowRect2.Left - 6) / 2.7) + CSGO2.x, Convert.ToInt16(WindowRect2.Bottom - WindowRect2.Top - 29 - (WindowRect2.Bottom - WindowRect2.Top - 29) / 2.594) + CSGO2.y);
                await Task.Delay(500);

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
                await Task.Delay(250);
                SetCursorPos(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 30.476) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 3.779) + CSGO.y);
                await Task.Delay(250);
                LeftClick(Convert.ToInt16(WindowRect.Right - WindowRect.Left - 6 - (WindowRect.Right - WindowRect.Left - 6) / 5.423) + CSGO.x, Convert.ToInt16((WindowRect.Bottom - WindowRect.Top - 29) / 3.809) + CSGO.y);
                await Task.Delay(250);
                if (i == (T1WinTitle.Count - 1) && n == 0)
                {
                    n++;
                    TeamWinTitle = T2WinTitle;
                    i = 0;
                }
                if (i == (T2WinTitle.Count - 1) && n == 1)
                    n++;
            }
            return;
        }

       public static void RestartCSGO(short WinNum)
       {
            if (!PArray[WinNum-1].State || string.IsNullOrEmpty(PArray[WinNum-1].Login))
                return;
            string res;
            foreach (Process proc in  Process.GetProcessesByName("steam"))
            {
                String parameters = CommandLineUtilities.getCommandLines(proc);
                if (parameters.Contains(PArray[WinNum-1].Login))
                {
                    proc.Kill();
                    break;
                }
            }
            string x, y;
            if (Convert.ToInt16(WinNum) == 1 || Convert.ToInt16(WinNum) == 6)
            {
                if (string.IsNullOrEmpty(settingsObj.Value<string>("LeaderResX")))
                    x = "640";
                else
                    x = settingsObj.Value<string>("LeaderResX");
                if (string.IsNullOrEmpty(settingsObj.Value<string>("LeaderResY")))
                    y = "480";
                else
                    y = settingsObj.Value<string>("LeaderResY");
            }
            else
            {
                if (string.IsNullOrEmpty(settingsObj.Value<string>("BotResX")))
                    x = "400";
                else
                    x = settingsObj.Value<string>("BotResX");
                if (string.IsNullOrEmpty(settingsObj.Value<string>("BotResY")))
                    y = "300";
                else
                    y = settingsObj.Value<string>("BotResY");
            }
            res = x + " " + y;
            Process.Start("Launcher.exe", "true \"" + settingsObj["SteamFolder"].ToString() + "\" " + PArray[WinNum-1].Login + " " + PArray[WinNum-1].Password +
                " " + PArray[WinNum-1].Position + " " + res);
       }

       [DllImport("user32.dll")]
       private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
