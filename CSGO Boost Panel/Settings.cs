using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CSGO_Boost_Panel
{
    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public short WaitSecondsAutomation { get; set; }

        public short DelayInRound14 { get; set; }

        public short LeaderResX { get; set; }

        public short LeaderResY { get; set; }

        public short BotResX { get; set; }

        public short BotResY { get; set; }

        public bool Notifies { get; set; }

        public bool AutoAccept { get; set; }

        public bool AutoDisconnect { get; set; }

        public bool Sounds { get; set; }

        public bool MatchFoundSound { get; set; }

        public bool MatchEndedSound { get; set; }

        public bool LobbyNotGatheredSound { get; set; }

        public bool RoundLastsSound { get; set; }

        public bool FocusWindows { get; set; }

        public bool Automation { get; set; }

        public bool WinTeam1 { get { return winTeam1; } set { winTeam1 = value; OnPropertyChanged("WinTeam1"); } }
        private bool winTeam1;
        public bool WinTeam2 { get { return winTeam2; } set { winTeam2 = value; OnPropertyChanged("WinTeam2"); } }
        private bool winTeam2;
        public bool WinTeamTie { get; set; }

        public bool CSGOsRunning { get { return csgosRunning; } set { csgosRunning = value; OnPropertyChanged("CSGOsRunning"); } }
        private bool csgosRunning;

        public string TgApi { get; set; }

        public string SteamFolder { get { return steamFolder; } set { steamFolder = value; OnPropertyChanged("SteamFolder"); } }
        private string steamFolder;

        public string CSGOFolder { get { return csgoFolder; } set { csgoFolder = value; OnPropertyChanged("CSGOFolder"); } }
        private string csgoFolder;

        public string chatID { get; set; }
    }
}
