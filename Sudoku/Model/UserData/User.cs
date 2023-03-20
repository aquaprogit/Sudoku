using System;
using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;

namespace Sudoku.Model.UserData
{
    public class User : INotifyPropertyChanged
    {
        private static User _instance;
        [JsonRequired]
        private List<Info> _info;
        private User()
        {
            _info = new List<Info>();
        }
        /// <summary>
        /// Returns unique instance of <see cref="User"/> if it alreany initialized, othewise creates new one and returns it
        /// </summary>
        [JsonIgnore]
        public static User Instance => _instance ?? (_instance = new User());
        /// <summary>
        /// List of statistics of user with difficulties and it's time, taken to solve
        /// </summary>
        public List<Info> Info {
            get {
                return _info;
            }
        }
        /// <summary>
        /// Property changed event for INPC
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Records info about difficulty and it's time taken to solve
        /// </summary>
        /// <param name="difficulty">Current field difficulty</param>
        /// <param name="seconds">Seconds, taken to solve that field</param>
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
        /// <summary>
        /// Returns new instance of <see cref="Info"/> that is has specified difficulty and it's time taken to solve in seconds
        /// </summary>
        /// <param name="difficulty">Current difficulty</param>
        /// <param name="seconds">Seconds, taken to solve that field</param>
        public Info(Difficulty difficulty, int seconds)
        {
            _difficulty = difficulty;
            _seconds = new TimeSpan(0, 0, seconds);
        }

        /// <summary>
        /// Difficulty of current <see cref="Info"/>
        /// </summary>
        [JsonIgnore]
        public Difficulty Difficulty { get => _difficulty; }
        /// <summary>
        /// Time in seconds, taken to solve this record of <see cref="Info"/>
        /// </summary>
        [JsonIgnore]
        public TimeSpan Time { get => _seconds; }
    }
}
