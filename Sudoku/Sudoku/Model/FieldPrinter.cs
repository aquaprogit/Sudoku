using System.Windows;
using System.Windows.Media;

namespace Sudoku.Model
{
    internal static class FieldPrinter
    {
        public static Brush WhiteBrush { get; private set; }
        public static Brush BlackBrush { get; private set; }
        public static Brush PrintedBrush { get; private set; }
        static FieldPrinter()
        {
            WhiteBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            BlackBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            PrintedBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(Sudoku.Properties.Settings.Default.PrintedBrush);
        }
    }
}
