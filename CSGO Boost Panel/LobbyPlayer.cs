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

        private bool state;
        public bool State { get { return state; } set { state = value; OnPropertyChanged("State"); } }

        private string login;
        public string Login { get { return login; } set { login = value; OnPropertyChanged("Login"); } }

        public string Password { get; set; }
        public string Position { get; set; }
        private Brush status;
        public Brush Status { get { return status; } set { status = value; OnPropertyChanged("Status"); } }
        public string WindowTitle { get; set; }

        public LobbyPlayer()
        {
            this.Status = MainWindow.Red;
            this.Position = "50 50";

        }
    }
}
