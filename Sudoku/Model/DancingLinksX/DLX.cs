using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Model.DancingLinksX
{
    public class DLX : ICSPSolver
    {
        public IReadOnlyCollection<ISet<int>> Solve(ExactCover problem, SolverOptions options)
        {
            Node root = Parse(problem);
            List<ISet<int>> solutions = new List<ISet<int>>();
            Stack<int> currentSolution = new Stack<int>();
            int recursionLevel = 0;

            Explore(root, solutions, currentSolution, problem.Clues, recursionLevel, options);

            return solutions.AsReadOnly();
        }

        internal bool CheckForSolution(Node root, IList<ISet<int>> solutions, Stack<int> currentSolution, ISet<int> clues, int recursionLevel, SolverOptions options)
        {
            if (root.IsLast || options.HasRecursionLevelExceeded(recursionLevel) || options.HasSolutionsExceeded(solutions))
            {
                if (root.IsLast)
                {
                    HashSet<int> solution = new HashSet<int>(currentSolution);
                    if (options.IncludeCluesInSolution)
                    {
                        foreach (int clue in clues)
                        {
                            solution.Add(clue);
                        }
                    }
                    solutions.Add(solution);
                }

                return true;
            }

            return false;
        }
        internal Node GetHeaderWithMinimumRowCount(Node root)
        {
            Node next = null;

            foreach (Node header in root.Iterate(n => n.right).Skip(1))
            {
                if (next == null || header.rowCount < next.rowCount)
                {
                    next = header;
                }
            }

            return next;
        }

        internal void Explore(Node root, IList<ISet<int>> solutions, Stack<int> currentSolution, ISet<int> clues, int recursionLevel, SolverOptions options)
        {
            if (CheckForSolution(root, solutions, currentSolution, clues, recursionLevel, options))
                return;

            Node header = GetHeaderWithMinimumRowCount(root);

            if (header.rowCount <= 0)
                return;

            Cover(header);

            foreach (Node row in header.Iterate(n => n.down).Skip(1))
            {
                currentSolution.Push(row.row.set);
                foreach (Node rightNode in row.Iterate(n => n.right).Skip(1))
                {
                    Cover(rightNode);
                }
                Explore(root, solutions, currentSolution, clues, recursionLevel + 1, options);
                foreach (Node leftNode in row.Iterate(n => n.left).Skip(1))
                {
                    Uncover(leftNode);
                }
                currentSolution.Pop();
            }

            Uncover(header);
        }

        internal void Cover(Node node)
        {
            if (node.row == node) return;

            Node header = node.header;
            header.right.left = header.left;
            header.left.right = header.right;

            foreach (Node row in header.Iterate(n => n.down).Skip(1))
            {
                foreach (Node rightNode in row.Iterate(n => n.right).Skip(1))
                {
                    rightNode.up.down = rightNode.down;
                    rightNode.down.up = rightNode.up;
                    rightNode.header.rowCount--;
                }
            }
        }

        internal void Uncover(Node node)
        {
            if (node.row == node) return;

            Node header = node.header;

            foreach (Node row in header.Iterate(n => n.up).Skip(1))
            {
                foreach (Node leftNode in row.Iterate(n => n.left).Skip(1))
                {
                    leftNode.up.down = leftNode;
                    leftNode.down.up = leftNode;
                    leftNode.header.rowCount++;
                }
            }

            header.right.left = header;
            header.left.right = header;
        }

        internal Node Parse(ExactCover problem)
        {
            Node root = new Node();
            Dictionary<int, Node> headerLookup = new Dictionary<int, Node>();
            Dictionary<int, Node> rowLookup = new Dictionary<int, Node>();
            HashSet<int> givens = new HashSet<int>(problem.Clues
                                                        .SelectMany(x => problem.Sets[x])
                                                        .Distinct());

            foreach (int constraint in problem.Constraints.Where(x => givens.Contains(x) == false))
            {
                Node header = new Node { constraint = constraint, row = root };
                headerLookup.Add(constraint, header);
                root.AddLast(header);
            }

            foreach (var set in problem.Sets.Where(x => x.Value.Any(y => givens.Contains(y)) == false))
            {
                Node row = new Node { set = set.Key, header = root };
                rowLookup.Add(set.Key, row);
                root.AddLastDown(row);

                foreach (int element in set.Value)
                {
                    if (headerLookup.TryGetValue(element, out Node header))
                    {
                        Node cell = new Node { row = row, header = header };
                        row.AddLast(cell);
                        header.AddLastDown(cell);
                    }
                }
            }

            return root;
        }
    }
}
