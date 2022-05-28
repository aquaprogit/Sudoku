using System;
using System.Collections.Generic;

namespace Sudoku.Model
{
    internal abstract class FieldGenerator
    {
        protected FieldSelector _selector;
        protected FieldSolver _solver;
        protected Random _random;
        protected List<Cell> _cells;
        protected List<int> _solution;
        public FieldGenerator(List<Cell> cells)
        {
            _cells = cells;
            _selector = new FieldSelector();
            _random = new Random();
        }
        public abstract List<int> GenerateMap();
        public abstract void MakePlayable();

    }
}
