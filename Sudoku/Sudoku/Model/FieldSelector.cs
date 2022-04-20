using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Model
{
    internal class FieldSelector
    {
        private List<Cell> _cells;
        private List<List<Cell>> _rows;
        private List<List<Cell>> _columns;
        private List<List<Cell>> _squares;

        public Cell SelectedCell { get; set; }

        public void MoveSelection(Direction dir, List<Cell> cells)
        {
            if (SelectedCell == null) return;
            var column = GetAreas(Area.Column, cells).First(list => list.Contains(SelectedCell));
            var row = GetAreas(Area.Row, cells).First(list => list.Contains(SelectedCell));
            int index = -1;

            if (dir == Direction.Up || dir == Direction.Down)
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
        public List<Cell> GetSameValues(List<Cell> cells)
        {
            return SelectedCell.Value == 0
                ? new List<Cell>()
                : cells.Where(cell => cell.Value == SelectedCell.Value && cell != SelectedCell).ToList();
        }
        public List<Cell> GetAllLinked(List<Cell> cells)
        {
            List<Cell> result = new List<Cell>();
            foreach (Area area in Enum.GetValues(typeof(Area)))
            {
                result.AddRange(GetAreas(area, cells).First(list => list.Contains(SelectedCell)));
            }
            result.Remove(SelectedCell);
            return result;
        }
        public List<Cell> GetCorrectAreas(List<Cell> cells, List<int> key)
        {
            List<Cell> solvedParts = new List<Cell>();
            foreach (Area area in Enum.GetValues(typeof(Area)))
            {
                solvedParts.AddRange(GetAreas(area, cells)
                    .Where(list => list.All(cell => cell.Value != 0
                                     && cells[cells.IndexOf(cell)].Value == key[cells.IndexOf(cell)]))
                    .SelectMany(l => l));
            }
            return solvedParts;
        }
        public List<List<Cell>> GetAreas(Area area, List<Cell> cells)
        {
            if (cells == null)
                throw new ArgumentNullException(nameof(cells));
            bool equal = cells.SequenceEqual(_cells ?? new List<Cell>());
            if (!equal)
            {
                GetRows(cells);
                GetColumns(cells);
                GetSquares(cells);
            }
            List<List<Cell>> result;
            switch (area)
            {
                case Area.Row:
                    result = _rows;
                    break;
                case Area.Column:
                    result = _columns;
                    break;
                case Area.Square:
                    result = _squares;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(area));
            }

            return result;
        }
        public List<Cell> Transpose(List<Cell> cells)
        {
            List<Cell> transposed = cells.Select(c => new Cell(c.Coordinate, c.Value)).ToList();
            var rows = GetAreas(Area.Row, cells).Select(c => c).ToList();
            var columns = GetAreas(Area.Column, cells).Select(c => c).ToList();
            foreach (var col in columns)
            {
                foreach (Cell cell in col.Take(columns.IndexOf(col)))
                {
                    var inRow = rows.First(l => l.Contains(cell));
                    var inCol = columns.First(l => l.Contains(cell));
                    (int row, int col) coord = (rows.IndexOf(inRow), columns.IndexOf(inCol));
                    Cell toSwapWith = rows[coord.col][coord.row];
                    transposed[cells.IndexOf(cell)].Value = toSwapWith.Value;
                    transposed[cells.IndexOf(toSwapWith)].Value = cell.Value;
                }
            }
            return transposed;
        }
        private List<List<Cell>> GetRows(List<Cell> cells)
        {
            _rows = new List<List<Cell>>();
            _cells = new List<Cell>(cells);
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    _rows.Add(_cells.Where(c => c.Coordinate.CellIndex / 3 == j && c.Coordinate.CubeIndex / 3 == i).ToList());
            return _rows;
        }
        private List<List<Cell>> GetColumns(List<Cell> cells)
        {
            _columns = new List<List<Cell>>();
            var rows = GetAreas(Area.Row, cells);
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
        private List<List<Cell>> GetSquares(List<Cell> cells)
        {
            _squares = new List<List<Cell>>();
            _cells = new List<Cell>(cells);
            foreach (var item in _cells.GroupBy(c => c.Coordinate.CubeIndex))
                _squares.Add(item.Select(c => c).ToList());

            return _squares;
        }
    }
    enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    enum Area
    {
        Row,
        Column,
        Square
    }
}
