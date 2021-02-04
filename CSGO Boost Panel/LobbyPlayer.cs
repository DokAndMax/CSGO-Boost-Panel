using System.ComponentModel;
using System.Windows.Media;

namespace CSGO_Boost_Panel
{
    public class LobbyPlayer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool isOn;
        public bool IsOn { get { return isOn; } set { isOn = value; OnPropertyChanged("IsOn"); } }

        private string login;
        public string Login { get { return login; } set { login = value; OnPropertyChanged("Login"); } }

        public string Password { get; set; }
        public string Position { get; set; }
        private Brush isStarted;
        public Brush IsStarted { get { return isStarted; } set { isStarted = value; OnPropertyChanged("IsStarted"); } }
        public string WindowTitle { get; set; }

        public LobbyPlayer()
        {
            this.IsStarted = MainWindow.Red;
            this.Position = "50 50";

        }
    }
}
