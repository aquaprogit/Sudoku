using Sudoku.Common.Helper;
using Sudoku.Common.Models;
using Sudoku.DancingLinksX;

namespace Sudoku.Common.Generators;

internal abstract class FieldGenerator
{
    private int _cluesCount;
    protected FieldSelector _selector;
    protected FieldSolver _solver;
    protected Random _random;
    protected List<Cell> _cells;
    protected List<int> _solution = null!;
    public FieldGenerator(List<Cell> cells, int cluesCount)
    {
        _cells = cells;
        _cluesCount = cluesCount;
        _selector = new FieldSelector(cells);
        _solver = new FieldSolver(_selector);
        _random = new Random();
    }

    protected abstract void CreatePattern();

    protected void LeaveCluesOnly()
    {
        int countOfRemoves = 81 - _cluesCount % 81;
        Dictionary<Cell, bool> checkList = new Dictionary<Cell, bool>();
        _cells.ForEach(c => checkList.Add(c, false));
        for (int i = 0; i < countOfRemoves; i++)
        {
            List<Cell> allToRemove = checkList.Where(pair => pair.Key.Value != 0 && pair.Value == false).Select(p => p.Key).ToList();
            var toRemove = allToRemove[_random.Next(allToRemove.Count)];
            checkList[toRemove] = true;
            int previousValue = toRemove.Value;

            toRemove.UnlockValue();
            if (_solver.Solve() == SudokuResultState.HasTooManySolutions)
            {
                toRemove.Value = previousValue;
                toRemove.LockValue();
            }
        }
    }

    public List<int> GenerateMap()
    {
        _cells.ForEach(c => c.UnlockValue());
        CreatePattern();
        _cells.ForEach(c => c.LockValue());
        _solution = _cells.Select(c => c.Value).ToList();
        LeaveCluesOnly();
        return _solution;
    }
}