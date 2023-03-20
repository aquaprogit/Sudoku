using Sudoku.Common.Models;

namespace Sudoku.Common.Helper;

public class FieldSelector
{
    private List<Cell> _cells;
    private List<List<Cell>>? _rows;
    private List<List<Cell>>? _columns;
    private List<List<Cell>>? _squares;

    public Cell SelectedCell { get; set; } = null!;

    public FieldSelector(List<Cell> cells)
    {
        _cells = cells;
    }

    public void MoveSelection(Direction dir)
    {
        if (SelectedCell == null)
            return;
        var column = GetAreas(Area.Column).First(list => list.Contains(SelectedCell));
        var row = GetAreas(Area.Row).First(list => list.Contains(SelectedCell));
        int index = -1;

        if (dir is Direction.Up or Direction.Down)
        {
            index = column.IndexOf(SelectedCell);
            if (index > 0 && dir == Direction.Up)
                SelectedCell = column[index - 1];
            else if (index < 8 && dir == Direction.Down)
                SelectedCell = column[index + 1];
        }
        else
        {
            index = row.IndexOf(SelectedCell);
            if (index > 0 && dir == Direction.Left)
                SelectedCell = row[index - 1];
            else if (index < 8 && dir == Direction.Right)
                SelectedCell = row[index + 1];
        }
    }
    public List<Cell> GetSameValues()
    {
        return SelectedCell.Value == 0
            ? new List<Cell>()
            : _cells.Where(cell => cell.Value == SelectedCell.Value && cell != SelectedCell).ToList();
    }
    public List<Cell> GetAllLinked(Cell? cell = null)
    {
        cell ??= SelectedCell;
        List<Cell> result = new List<Cell>();
        foreach (Area area in Enum.GetValues(typeof(Area)))
        {
            var list = GetAreas(area);
            var collection = list.First(list => list.Contains(cell));
            result.AddRange(collection);
        }
        result.Remove(cell);
        return result;
    }
    public List<Cell> GetCorrectAreas(List<int> key)
    {
        List<Cell> solvedParts = new List<Cell>();
        foreach (Area area in Enum.GetValues(typeof(Area)))
        {
            solvedParts.AddRange(GetAreas(area)
                .Where(list => list.All(cell => cell.Value != 0
                                 && _cells[_cells.IndexOf(cell)].Value == key[_cells.IndexOf(cell)]))
                .SelectMany(l => l));
        }
        return solvedParts;
    }
    public List<List<Cell>> GetAreas(Area area)
    {
        return area switch {
            Area.Row => _rows ??= GetRows(),
            Area.Column => _columns ??= GetColumns(),
            Area.Square => _squares ??= GetSquares(),
            _ => throw new ArgumentOutOfRangeException(nameof(area)),
        };
    }
    public List<Cell> Transpose()
    {
        List<Cell> transposed = _cells.Select(c => new Cell(c.Coordinate, c.Value)).ToList();
        var rows = GetAreas(Area.Row).Select(c => c).ToList();
        var columns = GetAreas(Area.Column).Select(c => c).ToList();
        foreach (var col in columns)
        {
            foreach (var cell in col.Take(columns.IndexOf(col)))
            {
                var inRow = rows.First(l => l.Contains(cell));
                var inCol = columns.First(l => l.Contains(cell));
                (int row, int col) coord = (rows.IndexOf(inRow), columns.IndexOf(inCol));
                var toSwapWith = rows[coord.col][coord.row];
                transposed[_cells.IndexOf(cell)].Value = toSwapWith.Value;
                transposed[_cells.IndexOf(toSwapWith)].Value = cell.Value;
            }
        }
        return transposed;
    }
    public Cell CellForHint(List<int> key)
    {
        //With incorrect value
        List<Cell> incorrect = _cells.Where(c => c.Value != key[_cells.IndexOf(c)] && c.Value != 0).ToList();
        if (incorrect.Count > 0)
            return incorrect[new Random().Next(incorrect.Count)];

        //With less surmise count
        if (_cells.Any(c => c.Surmises?.Count > 0))
            return _cells.Where(c => c.Surmises?.Count is not null and not 0).OrderByDescending(c => c.Surmises?.Count).First();

        //With less neighbors
        var allPosible = _cells.Where(c => c.Value == 0);
        var withLessNeighbors = allPosible.First();
        int minCount = 81;
        foreach (var cell in allPosible)
        {
            int curCount = GetAllLinked(cell).Count(c => c.Value != 0 && c.Value == key[_cells.IndexOf(c)]);
            if (curCount < minCount)
            {
                minCount = curCount;
                withLessNeighbors = cell;
            }
        }
        return withLessNeighbors;
    }
    private List<List<Cell>> GetRows()
    {
        _rows = new List<List<Cell>>();
        _cells = new List<Cell>(_cells);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
                _rows.Add(_cells.Where(c => c.Coordinate.CellIndex / 3 == j && c.Coordinate.CubeIndex / 3 == i).ToList());
        }

        return _rows;
    }
    private List<List<Cell>> GetColumns()
    {
        _columns = new List<List<Cell>>();
        var rows = GetAreas(Area.Row);
        foreach (var row in rows)
        {
            for (int i = 0; i < row.Count; i++)
            {
                if (_columns.Count == i)
                    _columns.Add(new List<Cell>());
                _columns[i].Add(row[i]);
            }
        }
        return _columns;
    }
    private List<List<Cell>> GetSquares()
    {
        _squares = new List<List<Cell>>();
        _cells = new List<Cell>(_cells);
        foreach (var item in _cells.GroupBy(c => c.Coordinate.CubeIndex))
            _squares.Add(item.Select(c => c).ToList());

        return _squares;
    }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public enum Area
{
    Row,
    Column,
    Square
}
