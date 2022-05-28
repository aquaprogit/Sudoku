using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Model.Generator
{
    internal class EasyFieldGenerator : FieldGenerator
    {
        public EasyFieldGenerator(List<Cell> cells, int cluesCount) : base(cells, cluesCount) { }

        protected override void CreatePattern()
        {
            FillBase();
            Shuffle();
        }
        private void Shuffle()
        {
            for (int i = 0; i < 10; i++)
            {
                ShuffleSmallArea((Area)_random.Next(0, 2));
            }
                if (_random.Next(0, 4) == 0)
                    Transpose();
            _cells.ForEach(c => c.LockValue());
        }
        private void FillBase()
        {
            _cells.ForEach(c => c.UnlockValue());
            var rows = _selector.GetAreas(Area.Square, _cells);
            for (int rowIndex = 0; rowIndex < 9; rowIndex++)
            {
                int i = rowIndex;
                foreach (var item in rows[3 * rowIndex - 8 * (rowIndex / 3)])
                {
                    item.Value = i++ % 9 + 1;
                }
            }
        }
        private void ShuffleSmallArea(Area area)
        {
            if (area == Area.Square) return;

            var areas = _selector.GetAreas(area, _cells);
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
        private void Transpose()
        {
            var transposed = _selector.Transpose(_cells);
            for (int i = 0; i < 81; i++)
            {
                var curr = _cells[i];
                _cells[i].Value = transposed.First(cell => cell.Coordinate == curr.Coordinate).Value;
            }
        }
    }
}
