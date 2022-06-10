using System;
using System.Collections.Generic;
using System.Linq;

using Sudoku.Model.DancingLinksX;
using Sudoku.Model.Generator;

namespace Sudoku.Model
{
    public delegate void SolvingFinishedHandler(bool user);
    public delegate void FieldContentChangedHandler();

    internal class Field : IBaseField
    {
        private List<Cell> _cells;
        private List<int> _solution;
        private Stack<ICommand> _commandLog;
        private FieldGenerator _generator;
        private FieldSolver _solver;
        private int _hintsMaxCount;
        private FieldSelector _selector;
        private bool IsSolved => _cells.All(cell => {
            int index = _cells.IndexOf(cell);
            return cell.Value == _solution[index];
        });

        public int HintsLeft { get; private set; }
        public Cell SelectedCell {
            get => _selector.SelectedCell;
            private set => _selector.SelectedCell = value;
        }

        public event SolvingFinishedHandler OnSolvingFinished;
        public event FieldContentChangedHandler OnFieldContentChanged;
        public event CellContentChangedHandler CellContentChanged;
        public Field(int hintsCount)
        {
            _cells = new List<Cell>(81);
            _solution = new List<int>();
            _solver = new FieldSolver();
            _commandLog = new Stack<ICommand>();
            _hintsMaxCount = hintsCount;
            HintsLeft = hintsCount;
            _selector = new FieldSelector();
            BaseCellInit();
            SelectedCell = _cells.Find(c => c.Coordinate == (4, 4));
        }

        public void GenerateNewField(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    _generator = new EasyFieldGenerator(_cells, 35);
                    break;
                case Difficulty.Normal:
                    _generator = new HardFieldGenerator(_cells, 20);
                    break;
                case Difficulty.Hard:
                    _generator = new HardFieldGenerator(_cells, 15);
                    break;
            }
            _solution = _generator.GenerateMap();
            HintsLeft = _hintsMaxCount;
            OnFieldContentChanged?.Invoke();
        }

        public void MoveSelection(Direction dir)
        {
            _selector.MoveSelection(dir, _cells);
            OnFieldContentChanged?.Invoke();
        }
        public void MoveSelection((int, int) coordinate)
        {
            SelectedCell = _cells.First(cell => cell.Coordinate == coordinate);
            OnFieldContentChanged?.Invoke();
        }
        public void TypeValue(int value, bool isSurmise = false)
        {
            TypeValueCommand command = new TypeValueCommand(SelectedCell);
            command.Execute(value, isSurmise);
            _commandLog.Push(command);
            Cell_ContentChanged(SelectedCell);
            if (_solution.Count != 0 && IsSolved)
                OnSolvingFinished?.Invoke(true);
        }
        public bool Undo()
        {
            if (_commandLog.Count == 0)
                return false;
            ICommand command = _commandLog.Pop();
            Cell previousCell = command.Undo();
            Cell_ContentChanged(previousCell);
            SelectedCell.Set(previousCell);
            Cell_ContentChanged(SelectedCell);
            return true;
        }
        public void GiveHint()
        {
            if (IsSolved)
                throw new InvalidOperationException("Field is already solved. Can not apply hint");
            if (IsSolved == false && HintsLeft > 0)
            {
                Cell toShow = _selector.CellForHint(_cells, _solution);
                int index = _cells.IndexOf(toShow);
                toShow.Value = _solution[index];
                HintsLeft--;
            }
        }
        public void FinishSolving()
        {
            for (int index = 0; index < _cells.Count; index++)
            {
                if (_cells[index].IsGenerated == false)
                    _cells[index].Value = _solution[index];
            }
            OnSolvingFinished?.Invoke(false);
        }
        SudokuResultState IBaseField.Solve()
        {
            _cells.Where(c => c.Value != 0).ToList().ForEach(cell => cell.LockValue());
            return _solver.Solve(_cells, false, true);
        }
        void IBaseField.Clear()
        {
            _cells.ForEach(cell => cell.UnlockValue());
        }

        public List<Cell> GetSameValues()
        {
            return _selector.GetSameValues(_cells);
        }
        public List<Cell> GetAllLinked()
        {
            return _selector.GetAllLinked(_cells);
        }
        public List<Cell> GetCorrectAreas()
        {
            return _selector.GetCorrectAreas(_cells, _solution);
        }
        public List<Cell> GetIncorrectCells()
        {
            List<Cell> cells = new List<Cell>();
            for (int index = 0; index < _cells.Count; index++)
            {
                if (_cells[index].Value != 0 && _cells[index].Value != _solution[index])
                    cells.Add(_cells[index]);
            }
            return cells;
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
            CellContentChanged?.Invoke(obj);
        }
    }
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }
}
