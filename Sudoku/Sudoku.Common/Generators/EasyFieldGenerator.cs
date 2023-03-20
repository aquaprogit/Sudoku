using Sudoku.Common.Helper;
using Sudoku.Common.Models;

namespace Sudoku.Common.Generators;

internal class EasyFieldGenerator : FieldGenerator
{
    /// <summary>
    /// Initialize new instance of <see cref="EasyFieldGenerator"/>
    /// </summary>
    /// <param name="cells">Cells to fill</param>
    /// <param name="cluesCount">Count of clues that has to be left</param>
    public EasyFieldGenerator(List<Cell> cells, int cluesCount = 30) : base(cells, cluesCount) { }

    /// <summary>
    /// Creates base pattern of field
    /// </summary>
    protected override void CreatePattern()
    {
        FillBase();
        Shuffle();
    }
    /// <summary>
    /// Shuffles field area 
    /// </summary>
    private void Shuffle()
    {
        for (int i = 0; i < 10; i++)
        {
            ShuffleSmallArea((Area)_random.Next(0, 2));
        }
        if (_random.Next(0, 4) == 0)
            Transpose();
    }
    /// <summary>
    /// Fills field with base pattern from 1 to 9 with offset in each row
    /// </summary>
    private void FillBase()
    {
        var rows = _selector.GetAreas(Area.Square);
        for (int rowIndex = 0; rowIndex < 9; rowIndex++)
        {
            int i = rowIndex;
            foreach (var item in rows[3 * rowIndex - 8 * (rowIndex / 3)])
            {
                item.Value = i++ % 9 + 1;
            }
        }
    }
    /// <summary>
    /// Shuffles between each others rows and columns in their section 3*9 and 9*3 accordingly
    /// </summary>
    /// <param name="area"></param>
    private void ShuffleSmallArea(Area area)
    {
        if (area == Area.Square)
            return;

        var areas = _selector.GetAreas(area);
        int sector = _random.Next(3);
        int len = 3;
        while (len > 1)
        {
            int t = _random.Next(len--);
            for (int i = 0; i < 9; i++)
            {
                var c1 = _cells.First(c => c == areas[t + sector * 3][i]);
                var c2 = _cells.First(c => c == areas[len + sector * 3][i]);
                (c1.Value, c2.Value) = (c2.Value, c1.Value);
            }
        }
    }
    /// <summary>
    /// Raplces rows with columns and columns with rows.
    /// </summary>
    private void Transpose()
    {
        var transposed = _selector.Transpose();
        for (int i = 0; i < 81; i++)
        {
            var curr = _cells[i];
            _cells[i].Value = transposed.First(cell => cell.Coordinate == curr.Coordinate).Value;
        }
    }
}
