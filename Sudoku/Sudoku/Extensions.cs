using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Windows.Media;

using System.Windows;

namespace Sudoku;

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
                    yield return t;
                foreach (var childOfChild in child.FindVisualChildren<T>())
                {
                    yield return childOfChild;
                }
            }
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
