using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CSGO_Boost_Panel
{
    class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public short WaitSecondsAutomation { get { return waitSecondsAutomation; } set { waitSecondsAutomation = value; OnPropertyChanged("WaitSecondsAutomation"); } }
        private short waitSecondsAutomation;

        public short DelayInRound14 { get { return delayInRound14; } set { delayInRound14 = value; OnPropertyChanged("DelayInRound14"); } }
        private short delayInRound14;

        public short LeaderResX { get { return leaderResX; } set { leaderResX = value; OnPropertyChanged("LeaderResX"); } }
        private short leaderResX;

        public short LeaderResY { get { return leaderResY; } set { leaderResY = value; OnPropertyChanged("LeaderResY"); } }
        private short leaderResY;

        public short BotResX { get { return botResX; } set { botResX = value; OnPropertyChanged("BotResX"); } }
        private short botResX;

        public short BotResY { get { return botResY; } set { botResY = value; OnPropertyChanged("BotResY"); } }
        private short botResY;

        public Settings(short WaitSecondsAutomation, short DelayInRound14, short LeaderResX, short LeaderResY, short BotResX, short BotResY)
        {
            this.WaitSecondsAutomation = WaitSecondsAutomation;
            this.DelayInRound14 = DelayInRound14;
            this.LeaderResX = LeaderResX;
            this.LeaderResY = LeaderResY;
            this.BotResX = BotResX;
            this.BotResY = BotResY;
        }
    }
}
