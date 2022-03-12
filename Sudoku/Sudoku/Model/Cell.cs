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
        private bool _isGenerated = false;

        public Cell((int y, int x) coord, int value)
        {
            Coordinate = coord;
            IsGenerated = false;
            _value = value;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public (int CubeIndex, int CellIndex) Coordinate { get; private set; }
        public int Value {
            get => _value;
            set {
                if (value < 0 || value >= 10) throw new ArgumentOutOfRangeException(nameof(value));
                if (IsGenerated) throw new InvalidOperationException(nameof(IsGenerated)); 
                _value = value; 
                OnPropertyChanged();
            }
        }
        public bool IsGenerated {
            get => _isGenerated;
            private set {
                _isGenerated = value;
                OnPropertyChanged();
            }
        }
        public List<int> Surmises => IsGenerated ? null : new List<int>(_surmises);

        public void LockValue()
        {
            IsGenerated = true;
        }
        public void UnlockValue()
        {
            IsGenerated = false;
            Value = 0;
        }

        public void AddSurmise(int value)
        {
            if (IsGenerated) return;
            if (value < 1 || value > 9) throw new ArgumentOutOfRangeException(nameof(value));
            if (Surmises.Contains(value)) throw new Exception("Item already in collection");

            _surmises.Add(value);
            OnPropertyChanged("Surmises");
        }
        public void RemoveSurmise(int value)
        {
            if (IsGenerated) return;

            _surmises.Remove(value);
            OnPropertyChanged("Surmises");
        }
        public void AddSurmise(IEnumerable<int> values)
        {
            foreach (int value in values)
            {
                AddSurmise(value);
            }
        }
        public void RemoveSurmise(IEnumerable<int> values)
        {
            foreach (var value in values)
            {
                RemoveSurmise(value);
            }
        }

        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            return $"{Value} at ({Coordinate.CubeIndex}, {Coordinate.CellIndex})";
        }
    }
}
