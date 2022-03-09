using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sudoku.Model
{
    internal class Cell : INotifyPropertyChanged
    {
        private int _value = 0;
        private readonly List<int> _surmises = new List<int>(9);

        public Cell((int y, int x) coord, int value)
        {
            Coordinate = coord;
            IsGenerated = value != 0;
            _value = value;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public (int CubeIndex, int CellIndex) Coordinate { get; private set; }
        public int Value {
            get => _value;
            set {
                if (value < 0 || value >= 10) throw new ArgumentOutOfRangeException(nameof(value));

                _value = value;
                OnPropertyChanged();
            }
        }
        public bool IsGenerated { get; private set; }
        public List<int> Surmises => IsGenerated ? null : new List<int>(_surmises);

        public void AddSurmise(int value)
        {
            if (IsGenerated) return;
            if (value < 1 || value > 9) throw new ArgumentOutOfRangeException(nameof(value));
            if (Surmises.Contains(value)) throw new Exception("Item already in collection");

            Surmises.Add(value);
        }
        public void RemoveSurmise(int value)
        {
            if (IsGenerated) return;

            Surmises.Remove(value);
        }

        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
