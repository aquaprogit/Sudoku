using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sudoku.Model.UserData
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private Difficulty _currentDifficulty;
        /// <summary>
        /// Returns new instance of <see cref="UserViewModel"/> with default initializion and <see cref="User"/> event subscribtion
        /// </summary>
        public UserViewModel()
        {
            _currentDifficulty = Difficulty.Easy;
            User.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        /// <summary>
        /// Notifies subscribes that <see cref="UserViewModel"/>'s property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Current selected difficulty
        /// </summary>
        public Difficulty CurrentDifficulty {
            get { return _currentDifficulty; }
            set {
                _currentDifficulty = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AvarageTime));
            }
        }
        /// <summary>
        /// Avarage time taken by user to solve <see cref="CurrentDifficulty"/> field
        /// </summary>
        public TimeSpan AvarageTime {
            get {
                var result = User.Instance.Info.GroupBy(i => i.Difficulty)
                            .Select(g => new {
                                difficulty = g.Key,
                                avarage = g.Sum(a => (int)a.Time.TotalSeconds) / g.Count()
                            });
                int seconds = result.Count(a => a.difficulty == _currentDifficulty) > 0 ? result.FirstOrDefault(a => a.difficulty == _currentDifficulty).avarage : 0;
                return TimeSpan.FromSeconds(seconds);
            }
        }
        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Info")
                OnPropertyChanged(nameof(AvarageTime));
        }
        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
