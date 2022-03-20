using System.Collections.Generic;
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
        static FieldPrinter()
        {
            WhiteBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            BlackBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            NonGeneratedBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(Properties.Settings.Default.NonGeneratedBrush);
            PrintedBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(Properties.Settings.Default.PrintedBrush);
            SelectedCellBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(Properties.Settings.Default.SelectedCellBrush);
            SameNumberBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(Properties.Settings.Default.SameNumberBrush);
            IncorrectNumberBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(Properties.Settings.Default.IncorrectNumberBrush);
        }

    }
}
