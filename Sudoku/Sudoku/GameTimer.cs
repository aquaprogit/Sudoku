using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

namespace Sudoku
{
    public class GameTimer : INotifyPropertyChanged
    {
        private Timer _timer;
        private TimeSpan _current;
        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            private set {
                _isEnabled = value;
                if (_isEnabled)
                    _timer?.Start();
                else
                    _timer?.Stop();
                OnPropertyChanged();
            }
        }

        public string Time => $"{_current.Hours.ToString().PadLeft(2, '0')}:" +
                       $"{_current.Minutes.ToString().PadLeft(2, '0')}:" +
                       $"{_current.Seconds.ToString().PadLeft(2, '0')}";

        public GameTimer()
        {
            _timer = new Timer(1000) {
                Enabled = true,
                AutoReset = true
            };
            _timer.Elapsed += (object a, ElapsedEventArgs e) => {
                if (IsEnabled)
                {
                    _current += TimeSpan.FromSeconds(1);
                    OnPropertyChanged(nameof(Time));
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Start()
        {
            IsEnabled = true;
            _current = TimeSpan.Zero;
        }

        public TimeSpan Stop()
        {
            IsEnabled = false;
            return _current;
        }

        public void Pause()
        {
            IsEnabled = false;
        }

        public void Resume()
        {
            IsEnabled = true;
        }
    }
}