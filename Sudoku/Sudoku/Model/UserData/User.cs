using System;
using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;

using Sudoku.Common.Models;

namespace Sudoku.Model.UserData
{
    public class User : INotifyPropertyChanged
    {
        private static User _instance;
        [JsonRequired]
        private List<Info> _info;
        [JsonIgnore]
        public static User Instance => _instance ??= new User();

        public List<Info> Info => _info;
        private User()
        {
            _info = new List<Info>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RecordInfo(Difficulty difficulty, int seconds)
        {
            _info.Add(new Info(difficulty, seconds));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Info"));
        }
    }

    public class Info
    {
        [JsonRequired]
        private Difficulty _difficulty;
        [JsonRequired]
        private TimeSpan _seconds;
        [JsonIgnore]
        public Difficulty Difficulty => _difficulty;

        [JsonIgnore]
        public TimeSpan Time => _seconds;

        public Info(Difficulty difficulty, int seconds)
        {
            _difficulty = difficulty;
            _seconds = new TimeSpan(0, 0, seconds);
        }
    }
}