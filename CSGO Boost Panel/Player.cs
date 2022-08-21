using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using static CSGO_Boost_Panel.MainWindow;

namespace CSGO_Boost_Panel
{
    public class Team : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string TeamName { get { return teamName; } set { teamName = value; OnPropertyChanged("TeamName"); } }
        private string teamName;
        public IList<Player> Player { get; set; }

        public Team()
        {
            Player = new ObservableCollection<Player>();
        }

        public Team New()
        {
            for(short i = 0; i < 10; i++)
            {
                Player.Add(new Player(false, "", "", "", "Unknown", "50 50"));
            }
            return this;
        }

        public Team Duplicate()
        {
            Team duplicatedTeam = new Team();
            for (short i = 0; i < 10; i++)
            {
                duplicatedTeam.Player.Add(new Player(false, "", "", "", "Unknown", "50 50"));
            }
            for(short i = 0; i < 10; i++)
            {
                duplicatedTeam.Player[i].Login = Player[i].Login;
                duplicatedTeam.Player[i].Password = Player[i].Password;
                duplicatedTeam.Player[i].Nickname = Player[i].Nickname;
                duplicatedTeam.Player[i].Level = Player[i].Level;
                duplicatedTeam.Player[i].XP = Player[i].XP;
                duplicatedTeam.Player[i].Pos = Player[i].Pos;
                duplicatedTeam.Player[i].SteamID = Player[i].SteamID;
                duplicatedTeam.Player[i].Rank = Player[i].Rank;
                duplicatedTeam.Player[i].Toggled = Player[i].Toggled;
            }
            duplicatedTeam.TeamName = this.TeamName + " " + "copy";
            return duplicatedTeam;
        }
    }

    public class Player : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Toggled { get { return toggled; } set { toggled = value; OnPropertyChanged("Toggled"); } }
        private bool toggled;
        public string Login { get; set; }

        public string Password { get { return password; } set { password = value; OnPropertyChanged("Login"); } }
        private string password;

        public string Pos { get; set; }
        public string SteamID { get; set; }

        public string Nickname { get { return nickname; } set { nickname = value; OnPropertyChanged("Nickname"); OnPropertyChanged("Login"); } }
        private string nickname;

        public short Level { get; set; }

        public string XP { get; set; }

        public string Rank { get { return rank; } set { rank = value; OnPropertyChanged("Rank"); OnPropertyChanged("XP"); OnPropertyChanged("Level"); } }
        private string rank;

        private Brush isStarted;
        [JsonIgnore]
        public Brush IsStarted { get { return isStarted; } set { isStarted = value; OnPropertyChanged("IsStarted"); } }

        [JsonIgnore]
        public string WindowTitle { get; set; }

        public Player(bool Toggled, string Login, string Password, string SteamID, string Nickname, string Pos)
        {
            this.Toggled = Toggled;
            this.Login = Login;
            this.Password = Password;
            this.SteamID = SteamID;
            this.Nickname = Nickname;
            Level = 0;
            XP = "";
            Rank = "Resources/RankImages/0.png";
            IsStarted = Red;
            this.Pos = Pos;
        }
    }
}
