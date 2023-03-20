using System;
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
    internal static class Extensions
    {
        /// <summary>
        /// Measures method's time, taken to execute
        /// </summary>
        /// <param name="handler">Delegate of method which time should be measured</param>
        public static void Time(Action handler)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            handler.Invoke();
            sw.Stop();
            Debug.WriteLine(handler.Method.Name + " time is: " + Math.Round(sw.ElapsedMilliseconds / 1000D, 2) + " s.");
        }
        /// <summary>
        /// Converts <see cref="List{Cell}"/> of <see cref="Cell"/> to 2D array according to their placemnt on the field
        /// </summary>
        /// <param name="self">List of <see cref="Cell"/></param>
        /// <returns>Matrix of <see cref="Cell"/> with their proper indexes</returns>
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
        /// <summary>
        /// Finds all visual children of element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj">Parent element to find children in</param>
        /// <returns>Sequence of children of specified element</returns>
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is not null and T t)
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
        /// <summary>
        /// Gets major <see cref="Cell"/> content depending on it's <see cref="Cell.Value"/>
        /// </summary>
        /// <param name="cell"><see cref="Cell"/> to get content from</param>
        /// <returns><see cref="Cell.Value"/> if it is not zero, otherwise elements of <see cref="Cell.Surmises"/> in matrix format</returns>
        public static string GetCellContent(this Cell cell)
        {
            if (cell == null) 
                return null;
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
        /// <summary>
        /// Sets to string accellerator if it has not already applied to string
        /// </summary>
        /// <param name="input">String to add accellerator to</param>
        /// <returns>String with keyboard accellerator</returns>
        internal static string TryAddKeyboardAccellerator(this string input)
        {
            const string ACCELLERATOR = "_";
            return input.Contains(ACCELLERATOR) ? input : ACCELLERATOR + input;
        }
    }
}
