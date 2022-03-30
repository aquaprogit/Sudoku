using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sudoku.Model
{
    internal static class FieldPrinter
    {
        public static Brush WhiteBrush { get; private set; }
        public static Brush BlackBrush { get; private set; }
        public static Brush NonGeneratedBrush { get; private set; }
        public static Brush PrintedBrush { get; private set; }
        public static Brush SelectedCellBrush { get; private set; }
        public static Brush SameNumberBrush { get; private set; }
        public static Brush IncorrectNumberBrush { get; private set; }
        public static Brush SolvedPartBrush { get; private set; }
        static FieldPrinter()
        {
            WhiteBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            BlackBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            NonGeneratedBrush = HexToBrush(Properties.Settings.Default.NonGeneratedBrush);
            PrintedBrush = HexToBrush(Properties.Settings.Default.PrintedBrush);
            SelectedCellBrush = HexToBrush(Properties.Settings.Default.SelectedCellBrush);
            SameNumberBrush = HexToBrush(Properties.Settings.Default.SameNumberBrush);
            IncorrectNumberBrush = HexToBrush(Properties.Settings.Default.IncorrectNumberBrush);
            SolvedPartBrush = HexToBrush(Properties.Settings.Default.SolvedPartBrush);
        }

        static Brush HexToBrush(string hex)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFrom(hex);
        }

        public static void PrintCells(IEnumerable<Grid> grids, Brush brush)
        {
            grids.ToList().ForEach(grid => grid.Background = brush);
        }
    }
}
