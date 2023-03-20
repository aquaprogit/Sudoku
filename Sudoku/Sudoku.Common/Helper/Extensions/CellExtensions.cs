using Sudoku.Common.Models;

namespace Sudoku.Common.Helper.Extensions;
public static class CellExtensions
{
    /// <summary>
    /// Gets major <see cref="Cell"/> content depending on it's <see cref="Cell.Value"/>
    /// </summary>
    /// <param name="cell"><see cref="Cell"/> to get content from</param>
    /// <returns><see cref="Cell.Value"/> if it is not zero, otherwise elements of <see cref="Cell.Surmises"/> in matrix format</returns>
    public static string? GetCellContent(this Cell cell)
    {
        if (cell == null)
            return null;
        string result = "1 2 3\n4 5 6\n7 8 9";

        if (cell.Value == 0)
        {
            foreach (int i in Enumerable.Range(1, 9).Except(cell.Surmises ?? new SurmiseList()))
            {
                result = result.Replace(i.ToString(), " ");
            }
            return result;
        }
        else
        {
            return cell.Value.ToString();
        }
    }
    /// <summary>
    /// Converts <see cref="List{Cell}"/> of <see cref="Cell"/> to 2D array according to their placemnt on the field
    /// </summary>
    /// <param name="self">List of <see cref="Cell"/></param>
    /// <returns>Matrix of <see cref="Cell"/> with their proper indexes</returns>
    public static Cell[,] To2DArray(this List<Cell> self)
    {
        Cell[,] cells = new Cell[9, 9];
        FieldSelector selector = new FieldSelector();
        var columns = selector.GetAreas(Area.Column, self);
        for (int columnIndex = 0; columnIndex < 9; columnIndex++)
        {
            for (int rowIndex = 0; rowIndex < 9; rowIndex++)
            {
                cells[columnIndex, rowIndex] = columns[columnIndex][rowIndex];
            }
        }
        return cells;

    }
}
