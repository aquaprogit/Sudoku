using System;

namespace Sudoku.Model
{
    internal class TypeValueCommand : ICommand
    {
        private Cell _cell;
        private Cell _previousCell;
        public TypeValueCommand(Cell cell)
        {
            _cell = cell;
        }
        public void Execute(int value, bool isSurmise)
        {
            _previousCell = _cell.Clone();
            if (value < 0 || value > 9) throw new ArgumentOutOfRangeException("value");
            if (_cell.IsGenerated) return;

            if (isSurmise == false)
                _cell.Value = value;
            else
            {
                if (_cell.Surmises.Contains(value))
                    _cell.Surmises.Remove(value);
                else
                    _cell.Surmises.Add(value);
            }
        }

        public Cell Undo()
        {
            return _previousCell;
        }
    }
}
