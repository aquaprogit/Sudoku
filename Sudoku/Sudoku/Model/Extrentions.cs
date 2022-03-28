using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Sudoku.Model
{
    internal static class Extrentions
    {
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
                foreach (int i in Enumerable.Range(1, 9).Except(cell.Surmises))
                {
                    result = result.Replace(i.ToString(), " ");
                }
                return result;
            }
            else
                return cell.Value.ToString();
        }
    }
}
