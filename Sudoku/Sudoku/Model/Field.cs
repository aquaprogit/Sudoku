using System;
using System.Collections.Generic;
using System.Linq;

using Sudoku.Model.Generator;

namespace Sudoku.Model
{
    public delegate void SolvingFinishedHandler();
    public delegate void FieldContentChangedHandler();

    internal class Field : IBaseField
    {
        private List<Cell> _cells;
        private List<int> _solution;
        private Stack<ICommand> _commandLog;
        private FieldGenerator _generator;
        private FieldSolver _solver;
        private int _hintsMaxCount;

        private bool IsSolved => _cells.All(cell => {
            int index = _cells.IndexOf(cell);
            return cell.Value == _solution[index];
        });

        public int HintsLeft { get; private set; }
        public FieldSelector Selector { get; private set; }
        public event SolvingFinishedHandler OnSolvingFinished;
        public event FieldContentChangedHandler OnFieldContentChanged;

        public Field(int hintsCount)
        {
            _cells = new List<Cell>(81);
            _solution = new List<int>();
            _solver = new FieldSolver();
            _commandLog = new Stack<ICommand>();
            _hintsMaxCount = hintsCount;
            HintsLeft = hintsCount;
            Selector = new FieldSelector();
            BaseCellInit();
        }

        public void GenerateNewField(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    _generator = new EasyFieldGenerator(_cells);
                    break;
                case Difficulty.Normal:
                    _generator = new HardFieldGenerator(_cells, 30);
                    break;
                case Difficulty.Hard:
                    _generator = new HardFieldGenerator(_cells, 25);
                    break;
            }
            _solution = _generator.GenerateMap();
            Selector.SelectedCell = _cells.Find(c => c.Coordinate == (4, 4));
            HintsLeft = _hintsMaxCount;
        }

        public void MoveSelection(Direction dir)
        {
            Selector.MoveSelection(dir, _cells);
            OnFieldContentChanged?.Invoke();
        }

        public void TypeValue(int value, bool isSurmise = false)
        {
            TypeValueCommand command = new TypeValueCommand(Selector.SelectedCell);
            command.Execute(value, isSurmise);
            _commandLog.Push(command);
            OnFieldContentChanged?.Invoke();
            if (IsSolved)
                OnSolvingFinished?.Invoke();
        }
        public void Undo()
        {
            if (_commandLog.Count == 0)
                throw new InvalidOperationException("Nothing to undo. Log is empty");
            ICommand command = _commandLog.Pop();
            Cell previousCell = command.Undo();
            Cell_ContentChanged(previousCell);
            Selector.SelectedCell.Set(previousCell);
            Cell_ContentChanged(Selector.SelectedCell);
        }
        public void GiveHint()
        {
            if (IsSolved == false && HintsLeft > 0)
            {
                Cell toShow = Selector.CellForHint(_cells);
                int index = _cells.IndexOf(toShow);
                toShow.Value = _solution[index];
            }
        }
        public void FinishSolving()
        {
            for (int index = 0; index < _cells.Count; index++)
            {
                if (_cells[index].IsGenerated == false)
                    _cells[index].Value = _solution[index];
            }
        }
        public void Solve()
        {
            _cells.ForEach(cell => cell.LockValue());
            if (_solver.Solve(_cells, false, false) == false)
                throw new ArgumentException("The field has more than one solution");
        }
        public void Clear()
        {
            _cells.ForEach(cell => cell.UnlockValue());
        }
        private void BaseCellInit()
        {
            int cubeIndex = 0;
            for (int iteration = 0; iteration < 81; iteration++)
            {
                int innerIndex = iteration % 9;
                if (innerIndex == 0 && iteration != 0)
                    cubeIndex++;
                Cell cell = new Cell((cubeIndex, innerIndex), 0);
                cell.ContentChanged += Cell_ContentChanged;
                _cells.Add(cell);
            }
        }

        private void Cell_ContentChanged(Cell obj)
        {
            OnFieldContentChanged?.Invoke();
        }
    }
    enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }
}
