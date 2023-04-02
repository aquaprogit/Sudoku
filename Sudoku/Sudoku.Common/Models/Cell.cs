using System.Collections.Specialized;

using Newtonsoft.Json;

using Sudoku.Common.Helper.Extensions;

namespace Sudoku.Common.Models;

public delegate void CellContentChangedHandler(Cell obj);
public class Cell
{
    private int _value = 0;
    private SurmiseList _surmises;
    private bool _isGenerated = false;
    [JsonRequired] public (int CubeIndex, int CellIndex) Coordinate { get; private set; }
    public int Value
    {
        get => _value;
        set {
            if (value is < 0 or >= 10)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (IsGenerated)
                throw new InvalidOperationException(nameof(IsGenerated));
            _value = value;
            _surmises.Clear();
            OnPropertyChanged();
        }
    }

    [JsonRequired]
    public bool IsGenerated
    {
        get => _isGenerated;
        private set {
            _isGenerated = value;
            OnPropertyChanged();
        }
    }

    public SurmiseList? Surmises => IsGenerated ? null : _surmises;
    public Cell((int y, int x) coord, int value)
    {
        Coordinate = coord;
        IsGenerated = false;
        _value = value;
        _surmises = new SurmiseList();
        _surmises.OnCollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) => {
            OnPropertyChanged();
        };
    }

    public event CellContentChangedHandler? ContentChanged;

    private void OnPropertyChanged()
    {
        ContentChanged?.Invoke(this);
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
        UnlockValue();
        Value = obj.Value;
        LockValue();
        IsGenerated = obj.IsGenerated;
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

    public override string ToString()
    {
        return $"{this.GetCellContent()?.Replace("\n", " ")} at ({Coordinate.CubeIndex}, {Coordinate.CellIndex})";
    }

    public override bool Equals(object? obj)
    {
        return obj is Cell cell
            && cell.Coordinate == Coordinate
            && cell.IsGenerated == IsGenerated
            && cell._surmises.Equals(_surmises)
            && cell.Value == Value;
    }
}