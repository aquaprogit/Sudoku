using System.Text;

using Sudoku.Common.Models;

namespace Sudoku.Common.Helper.Extensions;
public static class CellExtensions
{
    public static string? GetCellContent(this Cell cell)
    {
        if (cell == null)
            return null;

        if (cell.Value != 0)
            return cell.Value.ToString();

        var surmises = cell.Surmises ?? new SurmiseList();

        var result = new StringBuilder();
        for (int i = 1; i <= 9; i++)
        {
            var value = surmises.Contains(i) ? i.ToString() : " ";
            result.Append(value);
            if (i % 3 == 0)
                result.Append('\n');
            else
                result.Append(' ');
        }

        return result.ToString().TrimEnd('\n');
    }

    public static Cell[,] To2DArray(this List<Cell> self)
    {
        Cell[,] cells = new Cell[9, 9];
        FieldSelector selector = new FieldSelector(self);
        var columns = selector.GetAreas(Area.Column);
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