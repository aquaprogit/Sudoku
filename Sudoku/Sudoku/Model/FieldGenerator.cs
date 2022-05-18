using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Model
{
    internal static class FieldGenerator
    {
        private static FieldSelector _selector;
        private static Random _random;
        private static List<Cell> _cells;
        private static List<int> _solution;
        static FieldGenerator()
        {
            _selector = new FieldSelector();
            _random = new Random();
        }
        public static List<int> GenerateMap(List<Cell> cells)
        {
            _cells = cells;
            FillBase();
            Shuffle();
            _cells = null;
            _solution = cells.Select(c => c.Value).ToList();
            return _solution;
        }
        public static void MakePlayable(List<Cell> cells)
        {
            _cells = cells;
            UnlockCells();
            SetSurmises();
            MakeSolvable();
            ClearSurmises();
            _cells = null;
        }
        private static void ClearSurmises()
        {
            _cells.Where(c => c.IsGenerated == false).ToList().ForEach(c => c.Value = 0);
        }
        private static void MakeSolvable()
        {
            if (TrySolve() == false)
            {
                List<Cell> withoutValue = _cells.Where(c => c.Value == 0).ToList();
                int index = _cells.IndexOf(withoutValue[_random.Next(withoutValue.Count)]);
                _cells[index].Value = _solution[index];
                _cells[index].LockValue();
                if (TrySolve() == false)
                    MakeSolvable();
            }
            bool TrySolve()
            {
                List<int> beforeSolving, afterSolving;
                do
                {
                    beforeSolving = _cells.Select(cell => cell.Value).ToList();
                    RemoveOddSurmises();
                    SetOnlySurmise();
                    SetSingleInArea();
                    afterSolving = _cells.Select(cell => cell.Value).ToList();
                } while (!beforeSolving.SequenceEqual(afterSolving));
                return afterSolving.Count(c => c == 0) == 0;
            }
        }
        private static void SetSurmises()
        {
            foreach (Cell cell in _cells.Where(c => c.Value == 0))
            {
                cell.Surmises.Add(Enumerable.Range(1, 9));
            }
        }
        private static void RemoveOddSurmises()
        {
            foreach (Cell cell in _cells.Where(c => c.IsGenerated == false))
            {
                foreach (Area area in Enum.GetValues(typeof(Area)))
                {
                    var areaWithCell = _selector.GetAreas(area, _cells).First(l => l.Contains(cell));
                    cell.Surmises.Remove(areaWithCell.Where(c => c.Value != 0).Select(c => c.Value));
                }
            }
        }
        private static void SetOnlySurmise()
        {
            _cells.Where(c => c.IsGenerated == false && c.Surmises.Count == 1)
                  .ToList()
                  .ForEach(c => c.Value = c.Surmises[0]);
            RemoveOddSurmises();
        }
        private static void SetSingleInArea()
        {
            foreach (Area area in Enum.GetValues(typeof(Area)))
            {
                foreach (List<Cell> region in _selector.GetAreas(area, _cells))
                {
                    var surmisesInArea = region.Where(c => c.IsGenerated == false)
                        .Select(c => c.Surmises)
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
        private static void UnlockCells()
        {
            int countOfRemoves = 81 - 30;
            for (int i = 0; i < countOfRemoves; i++)
            {
                List<Cell> withValue = _cells.Where(c => c.Value != 0).ToList();
                Cell toRemove = withValue[_random.Next(withValue.Count)];
                toRemove.UnlockValue();
            }
        }
        private static void Shuffle()
        {
            for (int i = 0; i < 55; i++)
            {
                ShuffleSmallArea((Area)_random.Next(0, 2));
                if (_random.Next(0, 4) == 0)
                    Transpose();
                ShuffleBigArea((Area)_random.Next(0, 2));
            }
            _cells.ForEach(c => c.LockValue());
        }

        private static void FillBase()
        {
            _cells.ForEach(c => c.UnlockValue());
            List<List<Cell>> rows = _selector.GetAreas(Area.Square, _cells);
            for (int rowIndex = 0; rowIndex < 9; rowIndex++)
            {
                int i = rowIndex;
                foreach (var item in rows[3 * rowIndex - 8 * (rowIndex / 3)])
                {
                    item.Value = i++ % 9 + 1;
                }
            }
        }
        private static void ShuffleSmallArea(Area area)
        {
            if (area == Area.Square) return;

            List<List<Cell>> areas = _selector.GetAreas(area, _cells);
            int sector = _random.Next(3);
            int len = 3;
            while (len > 1)
            {
                int t = _random.Next(len--);
                for (int i = 0; i < 9; i++)
                {
                    Cell c1 = _cells.First(c => c == areas[t + sector * 3][i]);
                    Cell c2 = _cells.First(c => c == areas[len + sector * 3][i]);
                    (c1.Value, c2.Value) = (c2.Value, c1.Value);
                }
            }
        }
        private static void ShuffleBigArea(Area area)
        {

        }
        private static void Transpose()
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
