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
            string result = "";

            if (cell.Value == 0)
            {
                for (int i = 1; i <= 9; i++)
                {
                    if (i % 3 == 1)
                        result += "\n";
                    if (cell.Surmises.Contains(i))
                        result += $"{i} ";
                    else
                        result += "  ";
                }
                return result.Substring(1, result.Length - 1);
            }
            else
                return cell.Value.ToString();
        }
    }
}
