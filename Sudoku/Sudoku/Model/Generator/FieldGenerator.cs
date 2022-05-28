using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Model.Generator
{
    internal abstract class FieldGenerator
    {
        private FieldSolver _solver;
        private int _cluesCount;

        protected FieldSelector _selector;
        protected Random _random;
        protected List<Cell> _cells;
        protected List<int> _solution;
        public FieldGenerator(List<Cell> cells, int cluesCount)
        {
            _cells = cells;
            _cluesCount = cluesCount;
            _selector = new FieldSelector();
            _solver = new FieldSolver();
            _random = new Random();
        }
        public abstract List<int> GenerateMap();

        protected void LeaveCluesOnly()
        {
            int countOfRemoves = 81 - _cluesCount % 81;
            Dictionary<Cell, bool> checkList = new Dictionary<Cell, bool>();
            _cells.ForEach(c => checkList.Add(c, false));
            for (int i = 0; i < countOfRemoves; i++)
            {
                var allToRemove = checkList.Where(pair => pair.Key.Value != 0 && pair.Value == false).Select(p => p.Key).ToList();
                Cell toRemove = allToRemove[_random.Next(allToRemove.Count)];
                checkList[toRemove] = true;
                int previousValue = toRemove.Value;

                toRemove.UnlockValue();
                if (_solver.Solve(_cells) == false)
                {
                    toRemove.Value = previousValue;
                    toRemove.LockValue();
                }
            }
        }

    }
}
