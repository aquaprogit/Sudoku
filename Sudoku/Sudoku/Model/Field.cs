using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sudoku.Model
{
    internal class Field
    {
        private Dictionary<Cell, Grid> _cellToGrids = new Dictionary<Cell, Grid>(81);
        private FieldSelector _selector = new FieldSelector();
        private Stack<ICommand> _commandLog = new Stack<ICommand>();
        private int _hintsLeft = 3;

        private Random _random = new Random();

        private List<int> _solution;
        private List<Grid> _grids;
        private bool _autoCheck;

        private List<Cell> Cells => _cellToGrids.Keys.ToList();
        public Field(List<Grid> grids)
        {
            _grids = grids;
            GenerateNewField();
        }
        public bool AutoCheck {
            get => _autoCheck;
            set {
                _autoCheck = value;
                FocusGridCell(_cellToGrids[_selector.SelectedCell]);
            }
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
            _solution = FieldGenerator.GenerateMap(Cells);
            FieldGenerator.MakePlayable(Cells);
            foreach (Grid grid in _cellToGrids.Values)
            {
                grid.MouseLeftButtonUp += Grid_MouseButtonUp;
            }
        }
        public void GiveHint()
        {
            if (Cells.Count(c => c.Value == 0) != 0 && _hintsLeft-- > 0)
            {
                var withoutValue = Cells.Where(c => c.Value == 0 || c.Value != _solution[Cells.IndexOf(c)]).ToList());
                Cell toShow = withoutValue[_random.Next(withoutValue.Count)];
                toShow.Value = _solution[Cells.IndexOf(toShow)]];
            }
            else
            {
                throw new InvalidOperationException("No hints left");
            }
        }
        public void TypeValue(int value, bool isSurmise)
        {
            TypeValueCommand _command = new TypeValueCommand(_selector.SelectedCell);
            _command.Execute(value, isSurmise);
            _commandLog.Push(_command);
            FocusGridCell(_cellToGrids[_selector.SelectedCell]);
        }
        public void Undo()
        {
            if (_commandLog.Count == 0)
                throw new InvalidOperationException("Nothing to undo");
            ICommand command = _commandLog.Pop();
            var previousValue = command.Undo();
            FocusGridCell(_cellToGrids[Cells.First(c => c.Coordinate == previousValue.Coordinate)]);
            _selector.SelectedCell.Set(previousValue);
            FocusGridCell(_cellToGrids[_selector.SelectedCell]);
        }
        public void MoveSelection(Direction dir)
        {
            _selector.MoveSelection(dir, Cells);
            if (_selector.SelectedCell != null)
                FocusGridCell(_cellToGrids[_selector.SelectedCell]);
        }

        private void Cell_PropertyChanged(Cell sender)
        {
            Cell cell = sender;
            TextBlock tb = _cellToGrids[cell].FindVisualChildren<TextBlock>().First();
            tb.Text = cell.GetCellContent();
            tb.FontSize = cell.Value == 0 ? 13 : 24;
            tb.Opacity = cell.Value == 0 ? 0.8 : 1;
            tb.Foreground = cell.IsGenerated ? FieldPrinter.BlackBrush : FieldPrinter.NonGeneratedBrush;
        }
        private void Grid_MouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            FocusGridCell((Grid)sender);
        }

        private void FocusGridCell(Grid grid)
        {
            FieldPrinter.PrintCells(_grids, FieldPrinter.WhiteBrush);
            _selector.SelectedCell = _cellToGrids.First(pair => pair.Value == grid).Key;
            BrushLinked();
            if (AutoCheck)
                BrushSolved();
            _cellToGrids[_selector.SelectedCell].Background = FieldPrinter.SelectedCellBrush;
            if (AutoCheck)
                BrushIncorrect();
        }

        private void BrushLinked()
        {
            var linked = _selector.GetAllLinked(Cells).Select(cell => _cellToGrids[cell]);
            var sameToSelected = _selector.GetSameValues(Cells).Select(cell => _cellToGrids[cell]);
            FieldPrinter.PrintCells(linked, FieldPrinter.PrintedBrush);
            FieldPrinter.PrintCells(sameToSelected, FieldPrinter.SameNumberBrush);
        }

        private void BrushSolved()
        {
            var correctParts = _selector.GetCorrectAreas(Cells, _solution).Select(cell => _cellToGrids[cell]);
            FieldPrinter.PrintCells(correctParts, FieldPrinter.SolvedPartBrush);
        }

        private void BrushIncorrect()
        {
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i].Value != _solution[i] && Cells[i].Value != 0)
                    _cellToGrids[Cells[i]].Background = FieldPrinter.IncorrectNumberBrush;
            }
        }
    }
}
