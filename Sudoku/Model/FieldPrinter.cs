using System.Collections.Generic;
using System.Linq;
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
            NonGeneratedBrush = HexToBrush("#003ed4");
            PrintedBrush = HexToBrush("#d4e3ff");
            SelectedCellBrush = HexToBrush("#7195eb");
            SameNumberBrush = HexToBrush("#efff73");
            IncorrectNumberBrush = HexToBrush("#ff7a69");
            SolvedPartBrush = HexToBrush("#6bfdff");
        }

        private static Brush? HexToBrush(string hex)
        {
            return new BrushConverter().ConvertFrom(hex) as SolidColorBrush;
        }

        public static void PrintCells(IEnumerable<Grid> grids, Brush brush)
        {
            grids.ToList().ForEach(grid => grid.Background = brush);
        }
    }
}
