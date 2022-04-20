using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sudoku.Model
{
    internal class Field
    {
        private Dictionary<Cell, Grid> _cellToGrids = new Dictionary<Cell, Grid>(81);
        private FieldSelector _selector = new FieldSelector();

        private Random _random = new Random();

        private List<int> _solution;
        private List<Grid> _grids;
        private List<Cell> Cells => _cellToGrids.Keys.ToList();
        public Field(List<Grid> grids)
        {
            _grids = grids;
            GenerateNewField();
            SaveSolution();
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
                cell.ContentChanged += Cell_PropertyChanged;
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
            } while (beforeSolving.SequenceEqual(afterSolving) == false);
            if (afterSolving.Contains(0))
                GenerateNewField();
            foreach (Grid grid in _cellToGrids.Values)
            {
                grid.MouseLeftButtonUp += Cell_Focus;
            }
        }

        private void SaveSolution()
        {
            _solution = Cells.Select(cell => cell.Value).ToList();
            _cellToGrids.Keys.Where(c => c.IsGenerated == false).ToList().ForEach(c => c.Value = 0);
        }

        public void MoveSelection(Direction dir)
        {
            _selector.MoveSelection(dir, Cells);
            if (_selector.SelectedCell != null)
                Cell_Focus(_cellToGrids[_selector.SelectedCell], null);
        }
        public void TypeValue(int value, bool isSurmise = false)
        {
            if (value < 0 || value > 9) throw new ArgumentOutOfRangeException("value");
            Cell selected = _selector.SelectedCell;
            if (selected.IsGenerated) return;

            if (isSurmise == false)
                selected.Value = value;
            else
            {
                if (selected.Surmises.Contains(value))
                    selected.RemoveSurmise(value);
                else
                    selected.AddSurmise(value);
            }
            Cell_Focus(_cellToGrids[selected], null);
        }

        public void Solve()
        {
            OnlyPossible();
            SetOnlyValues();
        }
        public void SetOnlyValues()
        {
            _cellToGrids.Keys.Where(c => c.Surmises?.Count == 1).ToList().ForEach(c => c.Value = c.Surmises[0]);
        }
        public void OnlyPossible()
        {
            foreach (var areaToSelect in new Area[] { Area.Row, Area.Column, Area.Square })
            {
                ClearExtraHint(areaToSelect);
                var areas = _selector.GetAreas(areaToSelect, Cells);
                foreach (var area in areas)
                {
                    var groups = area.Where(cell => cell.Value == 0)
                        .Select(cell => cell.Surmises)
                        .SelectMany(list => list)
                        .GroupBy(group => group)
                        .Select(group => new { Value = group.Key, Count = group.Count() });
                    if (groups.Count(group => group.Count == 1) == 1)
                    {
                        int value = groups.First(group => group.Count == 1).Value;
                        Cell toSet = area.First(cell => cell.Surmises != null && cell.Surmises.Contains(value));
                        toSet.Value = value;
                        toSet.RemoveSurmise(toSet.Surmises);
                        foreach (var toSelect in new Area[] { Area.Row, Area.Column, Area.Square })
                            ClearExtraHint(toSelect);
                    }
                }
            }
        }
        private void ClearExtraHint(Area area)
        {
            var areas = _selector.GetAreas(area, Cells);
            List<IEnumerable<Cell>> withHints = areas
                                       .Select(a => a.Where(c => c.Value == 0))
                                       .ToList();
            List<IEnumerable<int>> constants = areas
                                       .Select(a => a.Where(c => c.Value != 0)
                                       .Select(c => c.Value))
                                       .ToList();
            int i = 0;
            foreach (var square in withHints.Select(a => a.ToList()))
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
            int countOfRemoves = 1;
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
        private void Transpose()
        {
            var tr = _selector.Transpose(Cells);
            for (int i = 0; i < 81; i++)
            {
                Cells[i].Value = tr[i].Value;
            }
        }
        private void Shuffle()
        {
            for (int i = 0; i < 21; i++)
            {
                ShuffleArea((Area)_random.Next(0, 2));
                if (_random.Next(0, 3) == 0)
                    Transpose();
            }
            #region jff
            Dictionary<string, int> data = new Dictionary<string, int>();
            if (File.Exists("data.txt") == false)
                File.Create("data.txt");
            foreach (string line in File.ReadAllLines("data.txt"))
            {
                string k = line.Split(' ')[0];
                int count = Convert.ToInt32(line.Split(' ')[1]);
                data.Add(k, count);
            }
            string key = "";
            foreach (Cell cell in Cells)
            {
                key += cell.Value;
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
        private void ShuffleArea(Area area)
        {
            if (area == Area.Square) return;

            List<List<Cell>> areas = _selector.GetAreas(area, Cells);
            int sector = _random.Next(3);
            int len = 3;
            while (len > 1)
            {
                int t = _random.Next(len--);
                for (int i = 0; i < 9; i++)
                {
                    Cell c1 = _cellToGrids.Keys.First(c => c == areas[t + sector * 3][i]);
                    Cell c2 = _cellToGrids.Keys.First(c => c == areas[len + sector * 3][i]);
                    (c1.Value, c2.Value) = (c2.Value, c1.Value);
                }
            }
        }
        private void FillBase()
        {
            List<List<Cell>> rows = _selector.GetAreas(Area.Square, Cells);
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
        private void Cell_PropertyChanged(Cell sender)
        {
            Cell cell = sender;
            TextBlock tb = _cellToGrids[cell].FindVisualChildren<TextBlock>().First();
            tb.Text = cell.GetCellContent();
            tb.FontSize = cell.Value == 0 ? 13 : 24;
            tb.Opacity = cell.Value == 0 ? 0.8 : 1;
            tb.Foreground = cell.IsGenerated ? FieldPrinter.BlackBrush : FieldPrinter.NonGeneratedBrush;
        }
        private void Cell_Focus(object sender, MouseButtonEventArgs e)
        {
            FieldPrinter.PrintCells(_grids, FieldPrinter.WhiteBrush);
            Grid grid = (Grid)sender;
            _selector.SelectedCell = _cellToGrids.First(pair => pair.Value == grid).Key;
            var linked = _selector.GetAllLinked(Cells).Select(cell => _cellToGrids[cell]);
            var correctParts = _selector.GetCorrectAreas(Cells, _solution).Select(cell => _cellToGrids[cell]);
            var sameToSelected = _selector.GetSameValues(Cells).Select(cell => _cellToGrids[cell]);
            FieldPrinter.PrintCells(linked, FieldPrinter.PrintedBrush);
            FieldPrinter.PrintCells(correctParts, FieldPrinter.SolvedPartBrush);
            FieldPrinter.PrintCells(sameToSelected, FieldPrinter.SameNumberBrush);
            _cellToGrids[_selector.SelectedCell].Background = FieldPrinter.SelectedCellBrush;
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i].Value != _solution[i] && Cells[i].Value != 0)
                    _cellToGrids[Cells[i]].Background = FieldPrinter.IncorrectNumberBrush;
            }
        }
    }
    enum Difficulty
    {
        Easy,
        Hard
    }
}
