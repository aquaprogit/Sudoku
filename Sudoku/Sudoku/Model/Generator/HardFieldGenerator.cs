using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Model.Generator
{
    internal class HardFieldGenerator : FieldGenerator
    {
        public HardFieldGenerator(List<Cell> cells, int cluesCount) : base(cells, cluesCount) { }

        protected override void CreatePattern()
        {
            FillCenter();
            FillOtherCells();
        }

        private void FillCenter()
        {
            var center = _selector.GetAreas(Area.Square, _cells)[4];
            List<int> set = Enumerable.Range(1, 9).OrderBy(x => _random.Next()).Take(9).ToList();
            for (int i = 0; i < set.Count; i++)
            {
                center[i].Value = set[i];
            }
        }
        private void FillOtherCells()
        {
            _solver.Solve(_cells, false, false);
        }
    }
}
