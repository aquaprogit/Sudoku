using System;
using System.Collections.Generic;

namespace Sudoku.Model
{
    internal delegate void CellContentChangedHandler(Cell obj);
    internal class Cell
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
        public event CellContentChangedHandler ContentChanged;
        public (int CubeIndex, int CellIndex) Coordinate { get; private set; }
        public int Value {
            get => _value;
            set {
                if (value < 0 || value >= 10) throw new ArgumentOutOfRangeException(nameof(value));
                if (IsGenerated) throw new InvalidOperationException(nameof(IsGenerated));
                _value = value;
                _surmises.Clear();
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

        public Cell Copy()
        {
            Cell copy = new Cell(Coordinate, Value);
            copy._surmises.Clear();
            copy._surmises.AddRange(Surmises);
            return copy;
        }
        public void Set(Cell obj)
        {
            Value = obj.Value;
            _surmises.Clear();
            _surmises.AddRange(obj._surmises);
            Coordinate = obj.Coordinate;
        }
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
            if (value == 0)
            {
                _surmises.Clear();
                OnPropertyChanged();
                return;
            }
            if (value < 1 || value > 9) throw new ArgumentOutOfRangeException(nameof(value));
            if (_surmises.Contains(value)) throw new Exception("Item already in collection");
            _surmises.Add(value);
            OnPropertyChanged();
        }
        public void RemoveSurmise(int value)
        {
            if (IsGenerated) return;

            _surmises.Remove(value);
            OnPropertyChanged();
        }
        public void AddSurmise(IEnumerable<int> values)
        {
            foreach (int value in values)
                AddSurmise(value);
        }
        public void RemoveSurmise(IEnumerable<int> values)
        {
            foreach (int value in values)
                RemoveSurmise(value);
        }

        public void OnPropertyChanged()
        {
            ContentChanged?.Invoke(this);
        }

        public override string ToString()
        {
            return $"{this.GetCellContent().Replace("\n", " ")} at ({Coordinate.CubeIndex}, {Coordinate.CellIndex})";
        }
        public override bool Equals(object obj)
        {
            return obj is Cell cell
                && cell.Coordinate == Coordinate
                && cell.IsGenerated == IsGenerated
                && cell._surmises == _surmises
                && cell.Value == Value;
        }
    }
}
