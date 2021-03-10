using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using static CSGO_Boost_Panel.MainWindow;
using System.Threading;

namespace CSGO_Boost_Panel
{
    class CSGOIntercation
    {
        public async static Task StartSearching(int type)
        {
            String ldr1Title = PArray[0].WindowTitle, ldr2Title = PArray[5].WindowTitle;
            switch (type)
            {
                case 1:
                    {
                        await Task.Delay(600);
                        WindowHelper.Click(ldr1Title, CSGOCoefficients.Play);
                        await Task.Delay(600);
                        WindowHelper.Click(ldr1Title, CSGOCoefficients.GO);
                        await Task.Delay(600);
                    }
                    break;
                case 2:
                    {
                         await Task.Delay(600);
                         WindowHelper.Click(ldr2Title, CSGOCoefficients.Play);
                         await Task.Delay(600);
                         WindowHelper.Click(ldr2Title, CSGOCoefficients.GO);
                         await Task.Delay(600);
                    }
                    break;
                case 3:
                    {
                        string[] ldrTitles = new string[] { ldr1Title, ldr2Title };
                        for (int i = 0; i < ldrTitles.Length; i++)
                        {
                            await Task.Delay(600);
                            WindowHelper.Click(ldrTitles[i], CSGOCoefficients.Play);
                            await Task.Delay(600);
                            WindowHelper.Click(ldrTitles[i], CSGOCoefficients.GO);
                            await Task.Delay(600);
                        }
                    }
                    break;
            }
        }

        public async static Task GatherLobby()
        {
            List<String> ldrTitles = new List<String>();
            if (PArray[0].WindowTitle.Contains("LEADER"))
                ldrTitles.Add(PArray[0].WindowTitle);
            if (PArray[5].WindowTitle.Contains("LEADER"))
                ldrTitles.Add(PArray[5].WindowTitle);
            for (short i = 0; i < 10; i++)
                if (PArray[i].IsOn)
                    WindowHelper.EnableWindow(PArray[i].WindowTitle, false);
            List<string> TeamWinTitle = T1WinTitle;
            for (int i = 1, n = 0; n < 2; i++)
            {
                await Task.Delay(250);
                WindowHelper.Click(TeamWinTitle[i], CSGOCoefficients.RightMenuButtons, true);
                await Task.Delay(1000);
                WindowHelper.Click(TeamWinTitle[i], CSGOCoefficients.RightMenuButtons);
                await Task.Delay(500);
                WindowHelper.Click(TeamWinTitle[i], CSGOCoefficients.AddFriend);
                await Task.Delay(250);
                WindowHelper.Click(TeamWinTitle[i], CSGOCoefficients.CopyCode);
                await Task.Delay(250);
                WindowHelper.SendKey(TeamWinTitle[i], WindowHelper.VK_ESCAPE);

                await Task.Delay(250);
                WindowHelper.Click(ldrTitles[n], CSGOCoefficients.RightMenuButtons, true);
                await Task.Delay(1000);
                WindowHelper.Click(ldrTitles[n], CSGOCoefficients.RightMenuButtons);
                await Task.Delay(500);
                WindowHelper.Click(ldrTitles[n], CSGOCoefficients.AddFriend);
                await Task.Delay(250);
                WindowHelper.Click(ldrTitles[n], CSGOCoefficients.AddFriend);
                await Task.Delay(250);
                WindowHelper.Click(ldrTitles[n], CSGOCoefficients.FriendCodeField);
                await Task.Delay(250);
                string ClipboardText = "unkown";
                RunAsSTAThread(() =>
                {
                    ClipboardText = Clipboard.GetText();
                });
                WindowHelper.SendText(ldrTitles[n], ClipboardText);
                await Task.Delay(250);
                //WindowHelper.Click("Counter-Strike: Global Offensive", CSGOCoefficients.СheckMark);
                //await Task.Delay(500);
                WindowHelper.Click(ldrTitles[n], CSGOCoefficients.PlayerField);
                await Task.Delay(250);
                WindowHelper.Click(ldrTitles[n], CSGOCoefficients.Invite);
                await Task.Delay(250);
                WindowHelper.Click(ldrTitles[n], CSGOCoefficients.Cancel);
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

            TeamWinTitle = T1WinTitle;
            for (int i = 1, n = 0; n < 2; i++)
            {
                await Task.Delay(250);
                WindowHelper.Click(TeamWinTitle[i], CSGOCoefficients.RightInvitesMenu, true);
                await Task.Delay(1000);
                WindowHelper.Click(TeamWinTitle[i], CSGOCoefficients.Invitation);
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
            for (short i = 0; i < 10; i++)
                if (PArray[i].IsOn)
                    WindowHelper.EnableWindow(PArray[i].WindowTitle, true);
            return;
        }

       public static Task RestartCSGO(short WinNum)
       {
            if (!PArray[WinNum - 1].IsOn || string.IsNullOrEmpty(PArray[WinNum - 1].Login))
                return Task.CompletedTask;
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
            return Task.CompletedTask;
       }
        static void RunAsSTAThread(Action goForIt)
        {
            AutoResetEvent @event = new AutoResetEvent(false);
            Thread thread = new Thread(
                () =>
                {
                    goForIt();
                    @event.Set();
                });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            @event.WaitOne();
        }
    }
}
