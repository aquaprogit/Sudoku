﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

namespace Sudoku
{
    public class GameTimer : INotifyPropertyChanged
    {
        private readonly DateTime _start;
        private DateTime _current;
        private string _time;

        public string Time
        {
            get
            {
                TimeSpan time = _current - _start;
                return $"{time.Hours}:{time.Minutes}:{time.Seconds}";
            }
        }

        public GameTimer()
        {
            _start = DateTime.Now;
            _current = _start;
        }

        public void UpdateCurrent(Object unused, ElapsedEventArgs args)
        {
            _current = DateTime.Now;
            OnPropertyChanged(nameof(Time));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}