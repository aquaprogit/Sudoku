using System.Collections.Generic;

namespace Sudoku.Model.DancingLinksX
{
    public interface ICSPSolver
    {
        IReadOnlyCollection<ISet<int>> Solve(ExactCover problem, SolverOptions options);
    }
}
