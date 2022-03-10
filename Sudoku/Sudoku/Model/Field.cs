using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sudoku.Model
{
    internal class Field
    {
        private Dictionary<Cell, Grid> _cellToGrids = new Dictionary<Cell, Grid>(81);

        public Field(List<Grid> grids)
        {
            int cubeIndex = -1;
            for (int iter = 0; iter < 81; iter++)
            {
                int innerIndex = iter % 9;
                if (innerIndex == 0)
                    cubeIndex++;
                Cell cell = new Cell((cubeIndex, innerIndex), 0);
                cell.PropertyChanged += Cell_PropertyChanged;
                _cellToGrids.Add(cell, grids[iter]);

            }
        }

        private void Cell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Cell cell = (Cell)sender;
            TextBlock tb = _cellToGrids[cell].FindVisualChildren<TextBlock>().First();
            tb.Text = cell.GetCellContent();
            tb.FontSize = cell.Value == 0 ? 15 : 24;
            tb.Foreground = new SolidColorBrush(Color.FromArgb((byte)(cell.Value == 0 ? 80 : 100), 0, 0, 0));
        }
    }
}
