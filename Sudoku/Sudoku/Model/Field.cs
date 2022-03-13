using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sudoku.Model
{
    internal class Field
    {
        private Dictionary<Cell, Grid> _cellToGrids = new Dictionary<Cell, Grid>(81);
        private Random _random = new Random();
        private readonly Brush _whiteBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        private readonly Brush _blackBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        private readonly Brush _printedBrush = new SolidColorBrush(Color.FromRgb(37, 84, 194));
        private delegate void ShuffleHandler();
        private delegate IEnumerable<List<Cell>> GetAreaHandler(List<Cell> cells);
        private List<Grid> _grids;
        public Field(List<Grid> grids)
        {
            _grids = grids;
            GenerateNewField();
        }
        public void GenerateNewField()
        {
            _cellToGrids.Clear();
            int cubeIndex = 0;
            for (int iteration = 0; iteration < 81; iteration++)
            {
                int innerIndex = iteration % 9;
                if (innerIndex == 0 && iteration != 0)
                    cubeIndex++;
                Cell cell = new Cell((cubeIndex, innerIndex), 0);
                cell.PropertyChanged += Cell_PropertyChanged;
                _cellToGrids.Add(cell, _grids[iteration]);
            }
            FillBase();
            Shuffle();
            LeaveOnlyHints(Difficulty.Hard);
            List<int> beforeSolving, afterSolving;
            do
            {
                beforeSolving = _cellToGrids.Keys.Select(cell => cell.Value).ToList();
                Solve();
                afterSolving = _cellToGrids.Keys.Select(cell => cell.Value).ToList();
            } while (!beforeSolving.SequenceEqual(afterSolving));
            if (afterSolving.Contains(0))
                GenerateNewField();
            _cellToGrids.Keys.Where(c => c.IsGenerated == false).ToList().ForEach(c => c.Value = 0);
        }
        public void Solve()
        {
            ClearExtraHint(GetSquares);
            ClearExtraHint(GetRows);
            ClearExtraHint(GetColumns);
            SetOnlyValues();
        }
        public void SetOnlyValues()
        {
            _cellToGrids.Keys.Where(c => c.Surmises?.Count == 1).ToList().ForEach(c => c.Value = c.Surmises[0]);
        }
        public void OnlyPossible()
        {
            foreach (var GetArea in new GetAreaHandler[] { GetSquares, GetRows, GetColumns })
            {
                ClearExtraHint(GetArea);
                var areas = GetArea.Invoke(_cellToGrids.Keys.ToList());
                foreach (var area in areas)
                {
                    List<int> counts = new List<int>(Enumerable.Repeat(0, 9));
                    foreach (var surmises in area.Where(cell => cell.Value == 0).Select(cell => cell.Surmises))
                    {
                        foreach (var surmise in surmises)
                        {
                            counts[surmise - 1]++;
                        }
                    }
                    if (counts.Count(i => i == 1) == 1)
                    {
                        int value = counts.IndexOf(1) + 1;
                        Cell toSet = area.First(cell => cell.Surmises != null && cell.Surmises.Contains(value));
                        toSet.Value = value;
                        toSet.RemoveSurmise(toSet.Surmises);
                        foreach (var ga in new GetAreaHandler[] { GetSquares, GetRows, GetColumns })
                            ClearExtraHint(ga);
                    }
                }
            }
        }

        private void ClearExtraHint(GetAreaHandler getArea)
        {
            List<IEnumerable<Cell>> withHints = getArea(_cellToGrids.Keys.ToList())
                                       .Select(area => area.Where(c => c.Value == 0))
                                       .ToList();
            List<IEnumerable<int>> constants = getArea(_cellToGrids.Keys.ToList())
                                       .Select(area => area.Where(c => c.Value != 0)
                                       .Select(c => c.Value))
                                       .ToList();
            int i = 0;
            foreach (var square in withHints.Select(area => area.ToList()))
            {
                foreach (var cell in square)
                {
                    cell.RemoveSurmise(constants[i]);
                }
                i++;
            }

        }
        #region Generating field
        private void LeaveOnlyHints(Difficulty difficulty)
        {
            int countOfRemoves = 0;
            switch (difficulty)
            {
                case Difficulty.Easy:
                    countOfRemoves = 40;
                    break;
                case Difficulty.Hard:
                    countOfRemoves = 46;
                    break;
            }
            for (int i = 0; i < countOfRemoves; i++)
            {
                int cbIndex = _random.Next(9);
                int innerIndex = _random.Next(9);
                Cell toRemove = _cellToGrids.Keys.First(cell => cell.Coordinate == (cbIndex, innerIndex));
                if (toRemove.Value == 0)
                {
                    i--;
                    continue;
                }
                toRemove.UnlockValue();
                toRemove.AddSurmise(Enumerable.Range(1, 9));
            }
            _cellToGrids.Keys.Where(cell => cell.Value != 0).ToList().ForEach(cell => cell.LockValue());
        }

        private void Shuffle()
        {
            ShuffleColumns();
            ShuffleRows();
            ShuffleColumns();
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
        private void FillBase()
        {
            List<List<Cell>> rows = GetSquares(_cellToGrids.Keys.ToList()).ToList();
            for (int rowIndex = 0; rowIndex < 9; rowIndex++)
            {
                int i = rowIndex;
                foreach (var item in rows[3 * rowIndex - 8 * (rowIndex / 3)])
                {
                    item.Value = i++ % 9 + 1;
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
            tb.FontSize = cell.Value == 0 ? 13 : 24;
            tb.Opacity = cell.Value == 0 ? 0.8 : 1;
            tb.Foreground = cell.IsGenerated ? _blackBrush : _printedBrush;
        }
    }
    enum Difficulty
    {
        Easy,
        Hard
    }
}
