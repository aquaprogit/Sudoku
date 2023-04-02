using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using Sudoku.Common.Models;

namespace Sudoku.Model.UserData
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private Difficulty _currentDifficulty;
        public Difficulty CurrentDifficulty
        {
            get => _currentDifficulty;
            set {
                _currentDifficulty = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AvarageTime));
            }
        }

        public TimeSpan AvarageTime
        {
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

        public UserViewModel()
        {
            _currentDifficulty = Difficulty.Easy;
            User.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

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