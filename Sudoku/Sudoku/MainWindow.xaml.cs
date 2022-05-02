using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Sudoku.Model;

namespace Sudoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Field _field;
        private bool _isSurmiseMode;
        private readonly Dictionary<Key, int> _keysValues = new Dictionary<Key, int>() {
            {Key.D0, 0},
            {Key.D1, 1},
            {Key.D2, 2},
            {Key.D3, 3},
            {Key.D4, 4},
            {Key.D5, 5},
            {Key.D6, 6},
            {Key.D7, 7},
            {Key.D8, 8},
            {Key.D9, 9}
        };
        private readonly Dictionary<Key, Direction> _navigationKeys = new Dictionary<Key, Direction>() {
            { Key.Up,    Direction.Up    },
            { Key.Down,  Direction.Down  },
            { Key.Left,  Direction.Left  },
            { Key.Right, Direction.Right },
            { Key.W,     Direction.Up    },
            { Key.S,     Direction.Down  },
            { Key.A,     Direction.Left  },
            { Key.D,     Direction.Right }
        };
        public bool IsSurmiseMode {
            get => _isSurmiseMode;
            set {
                _isSurmiseMode = value;
                SurmiseMode_Button.Content = "Surmise Mode | " + (IsSurmiseMode ? "On" : "Off");
            }
        }
        public bool AutoCheck {
            get => _field.AutoCheck;
            set {
                _field.AutoCheck = value;
                AutoMode_Button.Content = "Auto-check | " + (_field.AutoCheck ? "On" : "Off");
            }
        }
        public MainWindow()
        {
            InitializeComponent();

            List<Grid> allGrid = Playground.FindVisualChildren<Grid>().Where(g => g.Height == 50).ToList();
            _field = new Field(allGrid);
            IsSurmiseMode = false;

        }
        private void Playground_KeyUp(object sender, KeyEventArgs e)
        {
            if (_keysValues.Keys.Contains(e.Key))
                _field.TypeValue(_keysValues[e.Key], IsSurmiseMode);
            else if (_navigationKeys.Keys.Contains(e.Key))
                _field.MoveSelection(_navigationKeys[e.Key]);
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            Playground.Focus();
        }

        private void SurmiseModeButton_Click(object sender, RoutedEventArgs e)
        {
            IsSurmiseMode = !IsSurmiseMode;
        }

        private void Undo_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _field.Undo();
            }
            catch (InvalidOperationException)
            { 
                MessageBox.Show("Nothing to undo.", "Invalid operation error.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AutoCheck = !AutoCheck;
        }

        private void Hint_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _field.GiveHint();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("No hints left.", "Invalid operation error.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
