using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Sudoku.Model;

namespace Sudoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            List<Grid> allGrid = Playground.FindVisualChildren<Grid>().Where(g => g.Height == 50).ToList();
            Field field = new Field(allGrid);
        }
    }
}
    