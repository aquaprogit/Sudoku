using System;
using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;

namespace Sudoku.Model.UserData
{
    public class User : INotifyPropertyChanged
    {
        private static User _instance;
        [JsonRequired] private List<Info> _info;

        public event PropertyChangedEventHandler PropertyChanged;

        private User()
        {
            _info = new List<Info>();
        }
        [JsonIgnore] public static User Instance => _instance ?? (_instance = new User());
        public void RecordInfo(Difficulty difficulty, int seconds)
        {
            _info.Add(new Info(difficulty, seconds));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Info"));
        }
        public List<Info> Info => _info;
    }

    public class Info
    {
        [JsonRequired] private Difficulty _difficulty;
        [JsonRequired] private TimeSpan _seconds;

        public Info(Difficulty difficulty, int seconds)
        {
            _difficulty = difficulty;
            _seconds = new TimeSpan(0, 0, seconds);
        }

        [JsonIgnore] public Difficulty Difficulty { get => _difficulty; }
        [JsonIgnore] public TimeSpan Time { get => _seconds; }
    }
}
