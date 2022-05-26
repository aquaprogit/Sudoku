using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Model
{
    internal class FieldSolver
    {
        private List<Cell> _cells;
        private FieldSelector _selector;

        public FieldSolver(List<Cell> cells)
        {
            _cells = cells;
            _selector = new FieldSelector();
        }
        public bool Solve(bool clearSolution = false)
        {
            SetSurmises();
            List<int> beforeSolving, afterSolving;
            do
            {
                beforeSolving = _cells.Select(cell => cell.Value).ToList();
                RemoveOddSurmises();
                SetOnlySurmise();
                SetSingleInArea();
                afterSolving = _cells.Select(cell => cell.Value).ToList();
            } while (!beforeSolving.SequenceEqual(afterSolving));
            if (clearSolution)
                _cells.Where(cell => cell.IsGenerated == false).ToList().ForEach(cell => cell.Value = 0);
            return afterSolving.Count(c => c == 0) == 0;
        }

        private void SetSurmises()
        {
            _cells.Where(cell => cell.Value == 0).ToList().ForEach(cell => cell.Surmises.Add(Enumerable.Range(1, 9).Except(cell.Surmises)));
        }
        private void RemoveOddSurmises()
        {
            foreach (Cell cell in _cells.Where(c => c.IsGenerated == false))
            {
                foreach (Area area in Enum.GetValues(typeof(Area)))
                {
                    List<Cell> areaWithCell = _selector.GetAreas(area, _cells).First(l => l.Contains(cell));
                    cell.Surmises.Remove(areaWithCell.Where(c => c.Value != 0).Select(c => c.Value));
                }
            }
        }
        private void SetOnlySurmise()
        {
            _cells.Where(c => c.IsGenerated == false && c.Surmises.Count == 1)
                  .ToList()
                  .ForEach(c => c.Value = c.Surmises.First());
            RemoveOddSurmises();
        }
        private void SetSingleInArea()
        {
            foreach (Area area in Enum.GetValues(typeof(Area)))
            {
                foreach (List<Cell> region in _selector.GetAreas(area, _cells))
                {
                    var surmisesInArea = region.Where(c => c.IsGenerated == false)
                        .Select(cell => cell.Surmises)
                        .SelectMany(l => l)
                        .GroupBy(i => i)
                        .Select(group => new { Surmise = group.Key, Count = group.Count() });
                    if (surmisesInArea.Count(a => a.Count == 1) == 1)
                    {
                        Cell cell = region.Where(c => c.Value == 0).First(c => c.Surmises.Contains(surmisesInArea.First(a => a.Count == 1).Surmise));
                        cell.Value = surmisesInArea.First(a => a.Count == 1).Surmise;
                    }
                }
                RemoveOddSurmises();
            }
        }
    }
}