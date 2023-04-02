using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Sudoku;

internal static class Extensions
{
    internal static string TryAddKeyboardAccellerator(this string input)
    {
        const string ACCELLERATOR = "_";
        return input.Contains(ACCELLERATOR) ? input : ACCELLERATOR + input;
    }

    public static void Time(Action handler)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        handler.Invoke();
        sw.Stop();
        Debug.WriteLine(handler.Method.Name + " time is: " + Math.Round(sw.ElapsedMilliseconds / 1000D, 2) + " s.");
    }

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
}