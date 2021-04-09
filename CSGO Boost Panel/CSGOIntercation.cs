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
            String ldr1Title = ActiveTeam.Player[0].WindowTitle, ldr2Title = ActiveTeam.Player[5].WindowTitle;
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
            if (ActiveTeam.Player[0].WindowTitle.Contains("LEADER"))
                ldrTitles.Add(ActiveTeam.Player[0].WindowTitle);
            if (ActiveTeam.Player[5].WindowTitle.Contains("LEADER"))
                ldrTitles.Add(ActiveTeam.Player[5].WindowTitle);
            for (short i = 0; i < 10; i++)
                if (ActiveTeam.Player[i].Toggled)
                    WindowHelper.EnableWindow(ActiveTeam.Player[i].WindowTitle, false);
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
                if (ActiveTeam.Player[i].Toggled)
                    WindowHelper.EnableWindow(ActiveTeam.Player[i].WindowTitle, true);
            return;
        }

       public static Task RestartCSGO(short WinNum)
       {
            if (!ActiveTeam.Player[WinNum - 1].Toggled || string.IsNullOrEmpty(ActiveTeam.Player[WinNum - 1].Login))
                return Task.CompletedTask;
            string res;
            foreach (Process proc in  Process.GetProcessesByName("steam"))
            {
                String parameters = CommandLineUtilities.getCommandLines(proc);
                if (parameters.Contains(ActiveTeam.Player[WinNum-1].Login))
                {
                    proc.Kill();
                    break;
                }
            }
            short x, y;
            if (Convert.ToInt16(WinNum) == 1 || Convert.ToInt16(WinNum) == 6)
            {
                    x = ProgramSettings.LeaderResX;
                    y = ProgramSettings.LeaderResY;
            }
            else
            {
                    x = ProgramSettings.BotResX;
                    y = ProgramSettings.BotResY;
            }
            res = x + " " + y;
            Process.Start("Launcher.exe", "true \"" + ProgramSettings.SteamFolder + "\" " + ActiveTeam.Player[WinNum-1].Login + " " + ActiveTeam.Player[WinNum-1].Password +
                " " + ActiveTeam.Player[WinNum-1].Pos + " " + res);
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
