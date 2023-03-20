namespace Sudoku.DancingLinksX;

public class SolverOptions
{
    public int MaxRecursion { get; set; } = -1;
    public int MaxSolutions { get; set; } = -1;
    public bool IncludeCluesInSolution = false;

    public bool HasRecursionLevelExceeded(int recursionLevel)
    {
        return MaxRecursion > -1 && recursionLevel > MaxRecursion;
    }

    public bool HasSolutionsExceeded(IEnumerable<ISet<int>> solutions)
    {
        return MaxSolutions > -1 && solutions.Count() >= MaxSolutions;
    }
}
