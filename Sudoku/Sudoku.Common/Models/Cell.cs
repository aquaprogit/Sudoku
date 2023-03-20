using System.Collections.Specialized;

using Sudoku.Common.Helper.Extensions;

using Newtonsoft.Json;

namespace Sudoku.Common.Models;

public delegate void CellContentChangedHandler(Cell obj);
public class Cell
{
    private int _value = 0;
    private SurmiseList _surmises;
    private bool _isGenerated = false;
    /// <summary>
    /// Returns new instance of <see cref="Cell"/> with set value and coordinates
    /// </summary>
    /// <param name="coord">Coordinates: (Cube index; Cell index)</param>
    /// <param name="value">Cell value in range from 0 to 9</param>
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
    /// <summary>
    /// Notifies all subscribers that cell view content changed and have to be updated
    /// </summary>
    public event CellContentChangedHandler? ContentChanged;
    /// <summary>
    /// Returns tuple of <see cref="Cell"/> coordinates
    /// </summary>
    [JsonRequired] public (int CubeIndex, int CellIndex) Coordinate { get; private set; }
    /// <summary>
    /// Major <see cref="Cell"/> value in range from 0 to 9. Default value - 0
    /// </summary>
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
    /// <summary>
    /// Returns whether <see cref="Cell"/> <see cref="Value"/> is editable.
    /// </summary>
    [JsonRequired]
    public bool IsGenerated
    {
        get => _isGenerated;
        private set {
            _isGenerated = value;
            OnPropertyChanged();
        }
    }
    /// <summary>
    /// List of <see cref="Cell"/> surmises, <see langword="null"/> is <see cref="IsGenerated"/>
    /// </summary>
    public SurmiseList? Surmises => IsGenerated ? null : _surmises;
    /// <summary>
    /// Clones values from current instance of <see cref="Cell"/> into new one
    /// </summary>
    /// <returns>Deep copy of current <see cref="Cell"/> instance</returns>
    public Cell Clone()
    {
        Cell copy = new Cell(Coordinate, Value);
        copy._surmises.Clear();
        copy._surmises.Add(_surmises);
        return copy;
    }
    /// <summary>
    /// Sets values from specified <see cref="Cell"/> to current instance
    /// </summary>
    /// <param name="obj"><see cref="Cell"/> to get values from</param>
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
    /// <summary>
    /// Locks <see cref="Cell"/> preventing <see cref="Value"/> from editing.
    /// </summary>
    public void LockValue()
    {
        IsGenerated = true;
    }
    /// <summary>
    /// Unlocks <see cref="Cell"/> allowing to edit <see cref="Value"/> and sets it to default value
    /// </summary>
    public void UnlockValue()
    {
        IsGenerated = false;
        Value = 0;
    }

    private void OnPropertyChanged()
    {
        ContentChanged?.Invoke(this);
    }
    /// <summary>
    /// Returns <see cref="Cell"/> content with coordinates
    /// </summary>
    /// <returns><see cref="Cell"/> in string format</returns>
    public override string ToString()
    {
        return $"{this.GetCellContent()?.Replace("\n", " ")} at ({Coordinate.CubeIndex}, {Coordinate.CellIndex})";
    }
    /// <summary>
    /// Determines whether specified object of <see cref="Cell"/> type equals to current instance.
    /// </summary>
    /// <param name="obj"><see cref="Cell"/> to compare with</param>
    /// <returns><see langword="true"/> if instances are equal, otherwise <see langword="false"/></returns>
    public override bool Equals(object? obj)
    {
        return obj is Cell cell
            && cell.Coordinate == Coordinate
            && cell.IsGenerated == IsGenerated
            && cell._surmises.Equals(_surmises)
            && cell.Value == Value;
    }
}
