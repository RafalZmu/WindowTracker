using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WindowTracker.MVVM.Model
{
    class UserModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string UID { get; set; }

        private string window;
        public string WindowInFocus 
        {
            get { return window; }

            set
            {
                if(window != value)
                {
                    window = value;
                    OnPropertyChanged(nameof(WindowInFocus));
                }
            } 
        }
        private string time;
        public string LastMessageReceived
        {
            get { return time; }
            set
            {
                if(time != value)
                {
                    time = value;
                    OnPropertyChanged(nameof(LastMessageReceived));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
