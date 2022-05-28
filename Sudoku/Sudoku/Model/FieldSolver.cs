using System;
using System.Collections.Generic;

namespace Sudoku.Model
{
    internal class FieldSolver
    {
        private Random _random;
        public bool Solve(List<Cell> _cells, bool clearField = true)
        {
            _random = new Random();
            int[,] values = new int[9, 9];
            FieldSelector selector = new FieldSelector();
            var columns = selector.GetAreas(Area.Column, _cells);

            for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                for (int rowIndex = 0; rowIndex < columns[columnIndex].Count; rowIndex++)
                    values[rowIndex, columnIndex] = columns[rowIndex][columnIndex].Value;

            DancingLinksX.Sudoku sudoku = new DancingLinksX.Sudoku(values);
            bool result = sudoku.Solve();
            if (clearField == false)
            {
                for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                    for (int rowIndex = 0; rowIndex < columns[columnIndex].Count; rowIndex++)
                        if (columns[rowIndex][columnIndex].IsGenerated == false)
                            columns[rowIndex][columnIndex].Value = values[rowIndex, columnIndex];
            }
            return result;
        }
    }
}