using Sudoku.Common.Models;
using Sudoku.DancingLinksX;

namespace Sudoku.Common.Helper;

internal class FieldSolver
{
    private Random _random = new Random();
    public SudokuResultState Solve(List<Cell> _cells, bool clearField = true, bool isSinglePossible = true)
    {
        _random = new Random();
        int[,] values = new int[9, 9];
        FieldSelector selector = new FieldSelector();
        var columns = selector.GetAreas(Area.Column, _cells);

        for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
        {
            for (int rowIndex = 0; rowIndex < columns[columnIndex].Count; rowIndex++)
                values[columnIndex, rowIndex] = columns[columnIndex][rowIndex].Value;
        }

        DancingLinksX.Sudoku sudoku = new DancingLinksX.Sudoku(values);
        var result = sudoku.Solve(isSinglePossible);
        if (clearField == false)
        {
            for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                for (int rowIndex = 0; rowIndex < columns[columnIndex].Count; rowIndex++)
                {
                    if (columns[columnIndex][rowIndex].IsGenerated == false)
                        columns[columnIndex][rowIndex].Value = values[columnIndex, rowIndex];
                }
            }
        }
        return result;
    }
}