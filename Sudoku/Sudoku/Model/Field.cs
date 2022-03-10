using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace Sudoku.Model
{
    internal class Field
    {
        private Dictionary<Cell, Grid> _cellToGrids = new Dictionary<Cell, Grid>(81);

        public Field(List<Grid> grids)
        {
            int cubeIndex = -1;
            for (int iter = 0; iter < 81; iter++)
            {
                int innerIndex = iter % 9;
                if (innerIndex == 0)
                    cubeIndex++;
                Cell cell = new Cell((cubeIndex, innerIndex), 0);
                cell.PropertyChanged += Cell_PropertyChanged;
                _cellToGrids.Add(cell, grids[iter]);
            }
            FillBase();
        }
        public void FillBase()
        {
            List<List<Cell>> rows = GetSquares(_cellToGrids.Keys.ToList()).ToList();
            for (int rowIndex = 0; rowIndex < 9; rowIndex++)
            {
                int i = rowIndex;
                foreach (var item in rows[3 * rowIndex - 8 * (rowIndex / 3)])
                {
                    int v = i++ % 9 + 1;
                    item.Value = v;
                }
            }
        }
        public IEnumerable<List<Cell>> GetSquares(List<Cell> field)
        {
            foreach (var item in field.GroupBy(c => c.Coordinate.CubeIndex))
            {
                yield return item.Select(c => c).ToList();
            }
        }
        public IEnumerable<List<Cell>> GetRows(List<Cell> field)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    yield return field.Where(c => c.Coordinate.CellIndex / 3 == j && c.Coordinate.CubeIndex / 3 == i).ToList();
                }
            }
        }
        public IEnumerable<List<Cell>> GetColumns(List<Cell> field)
        {
            int y = 0, x = 0;
            int iterator = 0;
            int xStart = 0, yStart = 0;
            List<Cell> cells = new List<Cell>();
            do
            {
                cells.Add(field.First(c => c.Coordinate == (y, x)));
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
                        yield return cells.Select(c => c).ToList();
                        cells.Clear();
                        y = yStart;
                        xStart++;
                        x = xStart;
                    }
                }
                else
                {
                    yield return cells.Select(c => c).ToList();
                    cells.Clear();
                    yStart++;
                    xStart = 0;
                    x = xStart;
                    y = yStart;
                }
            } while (iterator != 81);
        }

        private void Cell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Cell cell = (Cell)sender;
            TextBlock tb = _cellToGrids[cell].FindVisualChildren<TextBlock>().First();
            tb.Text = cell.GetCellContent();
            tb.FontSize = cell.Value == 0 ? 15 : 24;
            tb.Opacity = cell.Value == 0 ? 0.8 : 1;
        }
    }
}
