using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sudoku.Model
{
    internal delegate void ActHandler();
    internal static class Extensions
    {
        public static void Time(ActHandler handler)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            handler.Invoke();
            sw.Stop();
            Debug.WriteLine(handler.Method.Name + " time is: " + sw.ElapsedMilliseconds);
        }
        public static Cell[,] To2DArray(this List<Cell> self)
        {
            Cell[,] cells = new Cell[9, 9];
            FieldSelector selector = new FieldSelector();
            var columns = selector.GetAreas(Area.Column, self);
            for (int columnIndex = 0; columnIndex < 9; columnIndex++)
            {
                for (int rowIndex = 0; rowIndex < 9; rowIndex++)
                {
                    cells[columnIndex, rowIndex] = columns[columnIndex][rowIndex];
                }
            }
            return cells;

        }
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }
                    foreach (var childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        public static string GetCellContent(this Cell cell)
        {
            if (cell == null) return null;
            string result = "1 2 3\n4 5 6\n7 8 9";

            if (cell.Value == 0)
            {
                foreach (int i in Enumerable.Range(1, 9).Except(cell.Surmises ?? new SurmiseList()))
                {
                    result = result.Replace(i.ToString(), " ");
                }
                return result;
            }
            else
            {
                return cell.Value.ToString();
            }
        }
        internal static ImageSource ToImageSource(this Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }
        internal static string TryAddKeyboardAccellerator(this string input)
        {
            const string ACCELLERATOR = "_";
            return input.Contains(ACCELLERATOR) ? input : ACCELLERATOR + input;
        }
    }
}
