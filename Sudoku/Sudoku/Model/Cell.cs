using System;
using System.Collections.Generic;

namespace Sudoku.Model
{
    internal class Cell
    {
        private int _value = 0;
        private readonly List<int> _surmises = new List<int>(9);

        public Cell((int y, int x) coord, int value)
        {
            Coordinate = coord;
            IsGenerated = value != 0;
            _value = value;
        }

        public (int CubeIndex, int CellIndex) Coordinate { get; private set; }
        public int Value {
            get => _value;
            set {
                if (value < 0 || value >= 10) throw new ArgumentOutOfRangeException(nameof(value));
                
                _value = value;
            }
        }
        public bool IsGenerated { get; private set; }
        public List<int> Surmises => IsGenerated ? null : new List<int>(_surmises);
    }
}
