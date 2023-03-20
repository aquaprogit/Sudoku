using Sudoku.Common.Commands;
using Sudoku.Common.Generators;
using Sudoku.Common.Helper;
using Sudoku.DancingLinksX;

namespace Sudoku.Common.Models;

public delegate void SolvingFinishedHandler(bool user);
public delegate void FieldContentChangedHandler();

public class Field : IBaseField
{
    private List<Cell> _cells;
    private List<int> _solution;
    private Stack<ICommand> _commandLog;
    private FieldGenerator _generator = null!;
    private FieldSolver _solver;
    private int _hintsMaxCount;
    private FieldSelector _selector;
    private bool IsSolved => _cells.All(cell => {
        int index = _cells.IndexOf(cell);
        return cell.Value == _solution[index];
    });
    /// <summary>
    /// Amount of hints that user still can use.
    /// </summary>
    public int HintsLeft { get; private set; }
    /// <summary>
    /// <see cref="Cell"/> with current user selection on.
    /// </summary>
    public Cell SelectedCell
    {
        get => _selector.SelectedCell;
        private set => _selector.SelectedCell = value;
    }
    /// <summary>
    /// Event to notify when <see cref="Field"/> solving is finished.
    /// </summary>
    public event SolvingFinishedHandler? OnSolvingFinished;
    /// <summary>
    /// Event to notify when <see cref="Field"/> content is changed.
    /// </summary>
    public event FieldContentChangedHandler? OnFieldContentChanged;
    /// <summary>
    /// Event to notify when any <see cref="Cell"/> on <see cref="Field"/> content is changed.
    /// </summary>
    public event CellContentChangedHandler? CellContentChanged;
    /// <summary>
    /// Returns new instance of <see cref="Field"/> with specified limit of hints for user
    /// </summary>
    /// <param name="hintsCount">Maximum amount of hints for user</param>
    public Field(int hintsCount)
    {
        _cells = new List<Cell>(81);
        _solution = new List<int>();
        _commandLog = new Stack<ICommand>();
        _hintsMaxCount = hintsCount;
        HintsLeft = hintsCount;
        InitCellsBase();
        _selector = new FieldSelector(_cells);
        _solver = new FieldSolver(_selector);
        SelectedCell = _cells.Find(c => c.Coordinate == (4, 4))!;
    }
    /// <summary>
    /// Generates new <see cref="Field"/> content depending on specified <see cref="Difficulty"/>.
    /// </summary>
    /// <param name="difficulty"><see cref="Difficulty"/> of new generated content</param>
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
    /// <summary>
    /// Moves current selection from <see cref="SelectedCell"/> to specified <see cref="Direction"/>.
    /// </summary>
    /// <param name="dir"><see cref="Direction"/> to move selection to</param>
    public void MoveSelection(Direction dir)
    {
        _selector.MoveSelection(dir);
        OnFieldContentChanged?.Invoke();
    }
    /// <summary>
    /// Sets current selection at specified coordinates.
    /// </summary>
    /// <param name="coordinate">Coordinates to set selection at</param>
    public void MoveSelection((int, int) coordinate)
    {
        SelectedCell = _cells.First(cell => cell.Coordinate == coordinate);
        OnFieldContentChanged?.Invoke();
    }
    /// <summary>
    /// Types value to current <see cref="SelectedCell"/>.
    /// </summary>
    /// <param name="value">Value to apply on <see cref="SelectedCell"/></param>
    /// <param name="isSurmise">Whether enter value to <see cref="Cell.Surmises"/> or <see cref="Cell.Value"/></param>
    public void TypeValue(int value, bool isSurmise = false)
    {
        TypeValueCommand command = new TypeValueCommand(SelectedCell);
        command.Execute(value, isSurmise);
        _commandLog.Push(command);
        Cell_ContentChanged(SelectedCell);
        if (_solution.Count != 0 && IsSolved)
            OnSolvingFinished?.Invoke(true);
    }
    /// <summary>
    /// Undoes previous action
    /// </summary>
    /// <returns><see langword="true"/> if action list wasn't empty, otherwise <see langword="false"/></returns>
    public bool Undo()
    {
        if (_commandLog.Count == 0)
            return false;
        var command = _commandLog.Pop();
        var previousCell = command.Undo();
        Cell_ContentChanged(previousCell);
        MoveSelection(previousCell.Coordinate);
        SelectedCell.Set(previousCell);
        Cell_ContentChanged(SelectedCell);
        return true;
    }
    /// <summary>
    /// Uncovers value from <see cref="Cell"/> if there any hints left.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void GiveHint()
    {
        if (IsSolved)
            throw new InvalidOperationException("Field is already solved. Can not apply hint");
        if (HintsLeft > 0)
        {
            var toShow = _selector.CellForHint(_solution);
            int index = _cells.IndexOf(toShow);
            toShow.Value = _solution[index];
            HintsLeft--;
        }
    }
    /// <summary>
    /// Finishes solving current <see cref="Field"/> using solution key and invokes <see cref="OnSolvingFinished"/>
    /// </summary>
    public void FinishSolving()
    {
        for (int index = 0; index < _cells.Count; index++)
        {
            if (_cells[index].IsGenerated == false)
                _cells[index].Value = _solution[index];
        }
        OnSolvingFinished?.Invoke(false);
    }
    /// <summary>
    /// Solves entered content using solver
    /// </summary>
    /// <returns>Sudoku solving result</returns>
    public SudokuResultState Solve()
    {
        _cells.Where(c => c.Value != 0).ToList().ForEach(cell => cell.LockValue());
        return _solver.Solve(false, true);
    }
    /// <summary>
    /// Clears content for new enterings
    /// </summary>
    public void Clear()
    {
        _cells.ForEach(cell => cell.UnlockValue());
    }
    /// <summary>
    /// Gets <see cref="Cell"/>s with same value to <see cref="SelectedCell"/>
    /// </summary>
    /// <returns>Cells with same value to <see cref="SelectedCell"/></returns>
    public List<Cell> GetSameValues()
    {
        return _selector.GetSameValues();
    }
    /// <summary>
    /// Gets <see cref="Cell"/>s within one area with <see cref="SelectedCell"/>
    /// </summary>
    /// <returns>All <see cref="Cell"/>s in same area with <see cref="SelectedCell"/></returns>
    public List<Cell> GetAllLinked()
    {
        return _selector.GetAllLinked();
    }
    /// <summary>
    /// Gets <see cref="Cell"/>s where values same to solution keys
    /// </summary>
    /// <returns><see cref="Cell"/>s where values same to solution keys</returns>
    public List<Cell> GetCorrectAreas()
    {
        return _selector.GetCorrectAreas(_solution);
    }
    /// <summary>
    /// Gets <see cref="Cell"/>s where values are not same to solution keys
    /// </summary>
    /// <returns><see cref="Cell"/>s where values are not same to solution keys</returns>
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

    private void InitCellsBase()
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
