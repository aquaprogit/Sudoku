using Sudoku.Common.Helper;
using Sudoku.Common.Models;

namespace Sudoku.Common.Generators;

internal class HardFieldGenerator : FieldGenerator
{
    public HardFieldGenerator(List<Cell> cells, int cluesCount) : base(cells, cluesCount)
    {
    }

    private void FillCenter()
    {
        var center = _selector.GetAreas(Area.Square)[4];
        List<int> set = Enumerable.Range(1, 9).OrderBy(x => _random.Next()).ToList();
        for (int i = 0; i < set.Count; i++)
        {
            center[i].Value = set[i];
        }
    }

    private void FillOtherCells()
    {
        _solver.Solve(false, false);
    }

    protected override void CreatePattern()
    {
        FillCenter();
        FillOtherCells();
    }
}