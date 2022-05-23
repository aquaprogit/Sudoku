using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Sudoku.Model
{
    internal delegate void CellContentChangedHandler(Cell obj);
    internal class Cell
    {
        private int _value = 0;
        private SurmiseList _surmises;
        private bool _isGenerated = false;

        public Cell((int y, int x) coord, int value)
        {
            Coordinate = coord;
            IsGenerated = false;
            _value = value;
            _surmises = new SurmiseList();
            _surmises.OnCollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => {
                OnPropertyChanged();
            };
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
        public SurmiseList Surmises {
            get => IsGenerated ? null : _surmises;
        }

        public Cell Clone()
        {
            Cell copy = new Cell(Coordinate, Value);
            copy._surmises.Clear();
            copy._surmises.Add(_surmises);
            return copy;
        }
        public void Set(Cell obj)
        {
            Value = obj.Value;
            _surmises.Clear();
            _surmises.Add(obj._surmises);
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

        private void OnPropertyChanged()
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
                && cell._surmises.Equals(_surmises)
                && cell.Value == Value;
        }
    }
}
