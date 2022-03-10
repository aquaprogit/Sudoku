using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace Sudoku.Model
{
    internal class Field
    {
        private Dictionary<Cell, Grid> _cellToGrids = new Dictionary<Cell, Grid>(81);
        private Random _random = new Random();
        private delegate void ShuffleHandler();

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
            Shuffle();
        }

        #region Generating field
        public void Shuffle()
        {
            ShuffleColumns();
            ShuffleRows();
            #region jff
            Dictionary<string, int> data = new Dictionary<string, int>();
            foreach (var line in File.ReadAllLines("data.txt"))
            {
                string k = line.Split(' ')[0];
                string count = line.Split(' ')[1];
                data.Add(k, Convert.ToInt32(count));
            }

            string key = "";
            foreach (var item in _cellToGrids.Keys)
            {
                key += item.Value;
            }
            if (data.ContainsKey(key))
                data[key]++;
            else
                data.Add(key, 1);
            List<string> output = new List<string>();
            foreach (var item in data)
            {
                output.Add($"{item.Key} {item.Value}");

            }
            File.WriteAllLines("data.txt", output.ToArray()); 
            #endregion
        }

        private void ShuffleColumns()
        {
            List<List<Cell>> columns = GetColumns(_cellToGrids.Keys.ToList()).ToList();
            for (int area = 0; area < 3; area++)
            {
                int len = 3;
                while (len > 1)
                {
                    int k = _random.Next(len--);
                    for (int i = 0; i < 9; i++)
                    {
                        Cell c1 = columns[k + area * 3][i], c2 = columns[len + area * 3][i];
                        (_cellToGrids.Keys.Last(c => c == c2).Value, _cellToGrids.Keys.Last(c => c == c1).Value) =
                            (_cellToGrids.Keys.Last(c => c == c1).Value, _cellToGrids.Keys.Last(c => c == c2).Value);
                    }
                }
            }
        }
        private void ShuffleRows()
        {
            List<List<Cell>> columns = GetRows(_cellToGrids.Keys.ToList()).ToList();
            for (int area = 0; area < 3; area++)
            {
                int len = 3;
                while (len > 1)
                {
                    int k = _random.Next(len--);
                    for (int i = 0; i < 9; i++)
                    {
                        Cell c1 = columns[k + area * 3][i], c2 = columns[len + area * 3][i];
                        (_cellToGrids.Keys.Last(c => c == c2).Value, _cellToGrids.Keys.Last(c => c == c1).Value) =
                            (_cellToGrids.Keys.Last(c => c == c1).Value, _cellToGrids.Keys.Last(c => c == c2).Value);
                    }
                }
            }
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
        #endregion
        #region Orginize
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
        #endregion

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
