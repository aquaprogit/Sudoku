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
            result.AddRange(GetAreas(Area.Row, cells).First(list => list.Contains(SelectedCell)));
            result.AddRange(GetAreas(Area.Column, cells).First(list => list.Contains(SelectedCell)));
            result.AddRange(GetAreas(Area.Square, cells).First(list => list.Contains(SelectedCell)));
            result.Remove(SelectedCell);
            return result;
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
            _cells = new List<Cell>(cells);
            int y = 0, x = 0;
            int iterator = 0;
            int xStart = 0, yStart = 0;
            List<Cell> column = new List<Cell>();
            do
            {
                column.Add(_cells.First(c => c.Coordinate == (y, x)));
                x += 3;
                iterator++;
                if (iterator % 27 != 0)
                {
                    if (iterator % 9 != 0)
                    {
                        if (iterator % 3 == 0 && iterator != 0)
                        {
                            y += 3;
                            x = xStart;
                        }
                    }
                    else
                    {
                        _columns.Add(column.Select(c => c).ToList());
                        column.Clear();
                        y = yStart;
                        xStart++;
                        x = xStart;
                    }
                }
                else
                {
                    _columns.Add(column.Select(c => c).ToList());
                    column.Clear();
                    yStart++;
                    xStart = 0;
                    x = xStart;
                    y = yStart;
                }
            } while (iterator != 81);
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
