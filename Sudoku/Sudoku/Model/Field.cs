using System;
using System.Collections.Generic;
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

        private Random _random = new Random();

        private List<int> _solution;
        private List<Grid> _grids;
        private List<Cell> Cells => _cellToGrids.Keys.ToList();
        public Field(List<Grid> grids)
        {
            _grids = grids;
            GenerateNewField();
        }
        public bool AutoCheck { get; set; }

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
                grid.MouseLeftButtonUp += GridCell_Focus;
            }
        }
        public void TypeValue(int value, bool isSurmise)
        {
            TypeValueCommand _command = new TypeValueCommand(_selector.SelectedCell);
            _command.Execute(value, isSurmise);
            _commandLog.Push(_command);
            GridCell_Focus(_cellToGrids[_selector.SelectedCell], null);
        }
        public void Undo()
        {
            if (_commandLog.Count == 0)
                throw new InvalidOperationException("Nothing to undo");
            ICommand command = _commandLog.Pop();
            var res = command.Undo();
            GridCell_Focus(_cellToGrids[Cells.First(c => c.Coordinate == res.Coordinate)], null);
            _selector.SelectedCell.Set(res); 
            GridCell_Focus(_cellToGrids[_selector.SelectedCell], null);
        }
        public void MoveSelection(Direction dir)
        {
            _selector.MoveSelection(dir, Cells);
            if (_selector.SelectedCell != null)
                GridCell_Focus(_cellToGrids[_selector.SelectedCell], null);
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
        public void GridCell_Focus(object sender, MouseButtonEventArgs e)
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
            if (AutoCheck)
            {
                for (int i = 0; i < Cells.Count; i++)
                {
                    if (Cells[i].Value != _solution[i] && Cells[i].Value != 0)
                        _cellToGrids[Cells[i]].Background = FieldPrinter.IncorrectNumberBrush;
                }
            }
        }
    }
}
