using System.Collections.Generic;

namespace Sudoku.DancingLinksX;

public interface ICSPSolver
{
    IReadOnlyCollection<ISet<int>> Solve(ExactCover problem, SolverOptions options);
}
