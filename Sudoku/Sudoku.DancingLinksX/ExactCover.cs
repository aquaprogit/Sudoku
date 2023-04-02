namespace Sudoku.DancingLinksX;

public class ExactCover
{
    public ISet<int> Constraints { get; }
    public IDictionary<int, ISet<int>> Sets { get; }
    public ISet<int> Clues { get; }
    public ExactCover(ISet<int> constraints, IDictionary<int, ISet<int>> sets, ISet<int> clues)
    {
        Constraints = constraints;
        Sets = sets;
        Clues = clues;
    }

    public IReadOnlyCollection<ISet<int>> Solve(ICSPSolver solver, SolverOptions options)
    {
        return solver.Solve(this, options);
    }
}