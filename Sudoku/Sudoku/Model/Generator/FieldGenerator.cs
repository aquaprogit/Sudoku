using System;
using System.Collections.Generic;
using System.Linq;

using Sudoku.Model.DancingLinksX;

namespace Sudoku.Model.Generator
{
    internal abstract class FieldGenerator
    {
        private int _cluesCount;
        protected FieldSelector _selector;
        protected Random _random;
        protected List<Cell> _cells;
        protected List<int> _solution;
        /// <summary>
        /// Initialize new instance of <see cref="FieldGenerator"/>
        /// </summary>
        /// <param name="cells">Cells to fill</param>
        /// <param name="cluesCount">Count of clues that has to be left</param>
        public FieldGenerator(List<Cell> cells, int cluesCount)
        {
            _cells = cells;
            _cluesCount = cluesCount;
            _selector = new FieldSelector();
            _random = new Random();
        }
        /// <summary>
        /// Generates new values for cells
        /// </summary>
        /// <returns>Solution key of generated map</returns>
        public List<int> GenerateMap()
        {
            _cells.ForEach(c => c.UnlockValue());
            CreatePattern();
            _cells.ForEach(c => c.LockValue());
            _solution = _cells.Select(c => c.Value).ToList();
            LeaveCluesOnly();
            return _solution;
        }
        /// <summary>
        /// Creates base pattern of field
        /// </summary>
        protected abstract void CreatePattern();
        /// <summary>
        /// Removes all cells' values except specified count of clues
        /// </summary>
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
                if (_solver.Solve(_cells) == SudokuResultState.HasTooManySolutions)
                {
                    toRemove.Value = previousValue;
                    toRemove.LockValue();
                }
            }
        }

    }
}
