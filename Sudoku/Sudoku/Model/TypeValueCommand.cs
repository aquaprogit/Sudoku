using System;

namespace Sudoku.Model;

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
        if (value is < 0 or > 9) 
            throw new ArgumentOutOfRangeException(nameof(value));

        if (_cell.IsGenerated) 
            return;

        if (isSurmise == false)
        {
            _cell.Value = value;
        }
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
