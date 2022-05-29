using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            {Key.D9, 9},
            {Key.NumPad0, 0},
            {Key.NumPad1, 1},
            {Key.NumPad2, 2},
            {Key.NumPad3, 3},
            {Key.NumPad4, 4},
            {Key.NumPad5, 5},
            {Key.NumPad6, 6},
            {Key.NumPad7, 7},
            {Key.NumPad8, 8},
            {Key.NumPad9, 9},
            {Key.Back, 0}
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

        private Field _field;
        private IBaseField _toSolveField;
        private bool _isSurmiseMode;
        private bool _autoCheck;
        private Difficulty _currentDifficulty;

        private BitmapImage _surmiseImageEnable;
        private BitmapImage _surmiseImageDisable;
        private BitmapImage _automodeImageEnable;
        private BitmapImage _automodeImageDisable;

        private Dictionary<(int, int), Grid> _gameGrids;
        private Dictionary<(int, int), Grid> _solveGrids;

        public bool IsSurmiseMode {
            get => _isSurmiseMode;
            set {
                _isSurmiseMode = value;
                (SurmiseMode_Button.Content as Image).Source = _isSurmiseMode ? _surmiseImageEnable : _surmiseImageDisable;
            }
        }
        public bool AutoCheck {
            get => _autoCheck;
            set {
                _autoCheck = value;
                (AutoMode_Button.Content as Image).Source = _autoCheck ? _automodeImageEnable : _automodeImageDisable;
            }
        }

        public MainWindow()
        {
            InitBitmapImage(ref _automodeImageDisable, "/Assets/search_u.png");
            InitBitmapImage(ref _automodeImageEnable, "/Assets/search_f.png");
            InitBitmapImage(ref _surmiseImageEnable, "/Assets/edit_f.png");
            InitBitmapImage(ref _surmiseImageDisable, "/Assets/edit_u.png");

            InitializeComponent();
            var game = Playground.FindVisualChildren<Grid>().Where(g => g.Height == 50).ToList();
            var solve = SolveField.FindVisualChildren<Grid>().Where(g => g.Height == 50).ToList();
            _gameGrids = new Dictionary<(int, int), Grid>();
            _solveGrids = new Dictionary<(int, int), Grid>();
            for (int cubeIndex = 0; cubeIndex < 9; cubeIndex++)
            {
                for (int innerIndex = 0; innerIndex < 9; innerIndex++)
                {
                    int listIndex = cubeIndex * 9 + innerIndex;
                    _gameGrids.Add((cubeIndex, innerIndex), game[listIndex]);
                    game[listIndex].MouseLeftButtonUp += GameGrid_MouseLeftButtonUp;
                    _solveGrids.Add((cubeIndex, innerIndex), solve[listIndex]);
                }
            }
            _currentDifficulty = Difficulty.Hard;
            _field = new Field(3);
            _field.OnSolvingFinished += OnSolvingFinished;
            _field.OnFieldContentChanged += OnFieldContentChanged;
            _field.CellContentChanged += OnCellContentChanged;
            _toSolveField = new Field(0);
            _field.GenerateNewField(_currentDifficulty);
        }

        private void GameGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Grid grid = (Grid)sender;
            _field.MoveSelection(_gameGrids.First(p => p.Value == grid).Key);
        }

        private void OnCellContentChanged(Cell cell)
        {
            var tb = _gameGrids[cell.Coordinate].FindVisualChildren<TextBlock>().First();
            tb.Text = cell.GetCellContent();
            tb.FontSize = cell.Value == 0 ? 13 : 24;
            tb.Opacity = cell.Value == 0 ? 0.8 : 1;
            tb.Foreground = cell.IsGenerated ? FieldPrinter.BlackBrush : FieldPrinter.NonGeneratedBrush;
        }

        private void InitBitmapImage(ref BitmapImage image, string source)
        {
            if (image == null)
                image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(source, UriKind.Relative);
            image.EndInit();
        }

        private void OnFieldContentChanged()
        {
            FieldPrinter.PrintCells(_gameGrids.Values, FieldPrinter.WhiteBrush);
            BrushLinked();
            if (AutoCheck)
                BrushSolved();
            _gameGrids[_field.Selector.SelectedCell.Coordinate].Background = FieldPrinter.SelectedCellBrush;
            if (AutoCheck)
                BrushIncorrect();
        }
        private void BrushLinked()
        {
            var linked = _field.GetAllLinked().Select(cell => _gameGrids[cell.Coordinate]);
            var sameToSelected = _field.GetSameValues().Select(cell => _gameGrids[cell.Coordinate]);
            FieldPrinter.PrintCells(linked, FieldPrinter.PrintedBrush);
            FieldPrinter.PrintCells(sameToSelected, FieldPrinter.SameNumberBrush);
        }

        private void BrushSolved()
        {
            var correctParts = _field.GetCorrectAreas().Select(cell => _gameGrids[cell.Coordinate]);
            FieldPrinter.PrintCells(correctParts, FieldPrinter.SolvedPartBrush);
        }

        private void BrushIncorrect()
        {
            _field.GetIncorrectCells().ForEach(cell => _gameGrids[cell.Coordinate].Background = FieldPrinter.IncorrectNumberBrush);
        }
        private void OnSolvingFinished()
        {
            if (MyMessageBox.Show("Well done!\nWant to create new field?", "Finished solving", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                _field.GenerateNewField(_currentDifficulty);
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
            _field.GenerateNewField(Difficulty.Hard);
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
            try
            {
                _toSolveField.Solve();
            }
            catch(ArgumentException ex)
            {
                MyMessageBox.Show(ex.Message);
            }
        }
        private void CustomClear_Button_Click(object sender, RoutedEventArgs e)
        {
            _toSolveField.Clear();
        }

    }
}
