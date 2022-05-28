using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Sudoku.Model;

namespace Sudoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Field _field;
        private Field _toSolveField;
        private bool _isSurmiseMode;
        private BitmapImage _surmiseImageEnable;
        private BitmapImage _surmiseImageDisable;
        private BitmapImage _automodeImageEnable;
        private BitmapImage _automodeImageDisable;
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
                (SurmiseMode_Button.Content as Image).Source = _isSurmiseMode ? _surmiseImageEnable : _surmiseImageDisable;
            }
        }
        public bool AutoCheck {
            get => _field.AutoCheck;
            set {
                _field.AutoCheck = value;
                (AutoMode_Button.Content as Image).Source = AutoCheck ? _automodeImageEnable : _automodeImageDisable;
            }
        }
        public MainWindow()
        {
            InitBitmapImage(ref _automodeImageDisable, "/Assets/search_u.png");
            InitBitmapImage(ref _automodeImageEnable, "/Assets/search_f.png");
            InitBitmapImage(ref _surmiseImageEnable, "/Assets/edit_f.png");
            InitBitmapImage(ref _surmiseImageDisable, "/Assets/edit_u.png");

            InitializeComponent();
            List<Grid> allGrid = Playground.FindVisualChildren<Grid>().Where(g => g.Height == 50).ToList();
            _field = new Field(allGrid);
            _field.GenerateNewField();
            _field.OnSolvingFinished += OnSolvingFinished;
            IsSurmiseMode = false;

            List<Grid> toSolveGrids = SolveField.FindVisualChildren<Grid>().Where(g => g.Height == 50).ToList();
            _toSolveField = new Field(toSolveGrids);
            _toSolveField.BaseCells();
            Test();
        }

        private void Test()
        {

        }



        private void InitBitmapImage(ref BitmapImage image, string source)
        {
            if (image == null)
                image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(source, UriKind.Relative);
            image.EndInit();
        }
        private void OnSolvingFinished()
        {
            if (MyMessageBox.Show("Well done!\nWant to create new field?", "Finished solving", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                _field.GenerateNewField();
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
            if (e.Key == Key.Tab)
            {
                SwitchTabs();
            }
            else
            {
                if (GameMode_Grid.Visibility == Visibility.Visible)
                    SolveField.Focus();
                else
                    Playground.Focus();
            }
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
                MyMessageBox.Show("Nothing to undo.", "Invalid operation error.", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MyMessageBox.Show("No hints left.", "Invalid operation error.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Solve_Button_Click(object sender, RoutedEventArgs e)
        {
            _field.FinishSolving();
        }
        private void Regen_Button_Click(object sender, RoutedEventArgs e)
        {
            _field.GenerateNewField();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchTabs();
        }

        private void SwitchTabs()
        {
            if (GameMode_Grid.Visibility == Visibility.Visible)
            {
                GameMode_Grid.Visibility = Visibility.Hidden;
                SolveMode_Grid.Visibility = Visibility.Visible;
                SolveField.Focus();
            }
            else
            {
                GameMode_Grid.Visibility = Visibility.Visible;
                SolveMode_Grid.Visibility = Visibility.Hidden;
                Playground.Focus();
            }
        }

        private void SolveField_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (_keysValues.Keys.Contains(e.Key))
                _toSolveField.TypeValue(_keysValues[e.Key], IsSurmiseMode);
            else if (_navigationKeys.Keys.Contains(e.Key))
                _toSolveField.MoveSelection(_navigationKeys[e.Key]);
        }

        private void CustomSolve_Button_Click(object sender, RoutedEventArgs e)
        {
            _toSolveField.LockEntered();

            try
            {
                _toSolveField.SolveEntered();

            }
            catch (Exception)
            {
                MyMessageBox.Show("2 or more solutions for this field");
            }
        }

        private void CustomClear_Button_Click(object sender, RoutedEventArgs e)
        {
            _toSolveField.UnlockEntered();
            _toSolveField.ClearField();
        }
    }
}
