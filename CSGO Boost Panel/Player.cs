
using System.ComponentModel;

namespace CSGO_Boost_Panel
{
    public class Player : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Login { get; set; }
        public string nickname { get { return rank; } set { Nickname = value; OnPropertyChanged("Nickname"); } }
        public string Nickname { get; set; }
        public short Level { get; set; }
        public string XP { get; set; }
        private string rank;
        public string Rank { get { return rank; } set { rank = value; OnPropertyChanged("Rank"); OnPropertyChanged("XP"); OnPropertyChanged("Level"); } }
        public string PresetName { get; set; }
        public string Visibility { get; set; }

        public Player(string Login, string Nickname, short Level, string XP, string Rank, string PresetName, string Visibility)
        {
            this.Login = Login;
            this.Nickname = Nickname;
            this.Level = Level;
            this.XP = XP;
            this.Rank = Rank;
            this.PresetName = PresetName;
            this.Visibility = Visibility;
        }
    }
}
