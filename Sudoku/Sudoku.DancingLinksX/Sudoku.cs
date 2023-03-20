namespace Sudoku.DancingLinksX;

public class Sudoku
{
    public int[,] Board { get; private set; }
    public Sudoku(int[,] board)
    {
        Board = board;
    }
    public SudokuResultState Solve(bool isSinglePossible = true)
    {
        var problem = Reduce(Board);
        var readOnlyCollection = problem.Solve(
                            new DLX(), new SolverOptions { MaxSolutions = isSinglePossible ? 2 : 1 });

        if (readOnlyCollection.Count > 1)
            return SudokuResultState.HasTooManySolutions;
        if (readOnlyCollection.Count == 0)
            return SudokuResultState.HasNoSolution;

        var solution = readOnlyCollection.Single();
        Augment(solution);

        return SudokuResultState.Solved;

    }

    internal void Augment(ISet<int> solution)
    {
        int n2 = Board.Length;
        int n = (int)Math.Sqrt(n2);

        foreach (int match in solution)
        {
            int row = match / (n * n);
            int column = match / n % n;
            int number = match % n;

            Board[row, column] = number + 1;
        }
    }

    internal ExactCover Reduce(int[,] board)
    {
        int n2 = board.Length;
        int n = (int)Math.Sqrt(n2);
        int m = (int)Math.Sqrt(n);

        // The constraints for any regular Sudoku puzzle are:
        //  - For each row, a number can appear only once.
        //  - For each column, a number can appear only once.
        //  - For each region(small square), a number can appear only once.
        //  - Each cell can only have one number.

        // For 9x9 Sudoku, the binary matrix will have 4 x 9² columns.

        HashSet<int> constraints = new HashSet<int>(Enumerable.Range(0, 4 * n * n));

        // The sets for any regular Sudoku puzzle are all permutations of:
        //  - Each row, each column, each number

        // For 9x9 Sudoku, the binary matrix will have 9³ rows.

        Dictionary<int, ISet<int>> sets = new Dictionary<int, ISet<int>>();
        HashSet<int> clues = new HashSet<int>();

        foreach (int row in Enumerable.Range(0, n))
        {
            foreach (int column in Enumerable.Range(0, n))
            {
                int region = m * (row / m) + column / m;

                foreach (int number in Enumerable.Range(0, n))
                {
                    sets.Add((row * n + column) * n + number, new HashSet<int>
                    {
                    row * n + number,
                    (n + column) * n + number,
                    (2 * n + region) * n + number,
                    (3 * n + row) * n + column
                });
                }
                if (board[row, column] != 0)
                    clues.Add((row * n + column) * n + board[row, column] - 1);
            }
        }

        ExactCover problem = new ExactCover(constraints, sets, clues);

        return problem;
    }
}
public enum SudokuResultState
{
    Solved,
    HasTooManySolutions,
    HasNoSolution
}
