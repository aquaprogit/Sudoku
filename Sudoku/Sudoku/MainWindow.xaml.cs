using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Sudoku.Model;
using Sudoku.Model.DancingLinksX;
using Sudoku.Model.UserData;

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
        private readonly string _userPath = @"user_stats.json";

        private Field _field;
        private IBaseField _toSolveField;

        private bool _isSurmiseMode;
        private bool _autoCheck;

        private BitmapImage _surmiseImageEnable;
        private BitmapImage _surmiseImageDisable;
        private BitmapImage _automodeImageEnable;
        private BitmapImage _automodeImageDisable;

        private Dictionary<(int, int), Grid> _gameGrids;
        private Dictionary<(int, int), Grid> _solveGrids;
        public GameTimer Timer { get; set; }
        internal UserViewModel UserViewModel { get; set; }
        internal Difficulty CurrentDifficulty {
            get => UserViewModel.CurrentDifficulty;
            set {
                UserViewModel.CurrentDifficulty = value;
            }
        }
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
                OnFieldContentChanged();
                (AutoMode_Button.Content as Image).Source = _autoCheck ? _automodeImageEnable : _automodeImageDisable;
            }
        }

        public MainWindow()
        {
            Timer = new GameTimer();
            InitBitmapImage(ref _automodeImageDisable, "/Assets/search_u.png");
            InitBitmapImage(ref _automodeImageEnable, "/Assets/search_f.png");
            InitBitmapImage(ref _surmiseImageEnable, "/Assets/edit_f.png");
            InitBitmapImage(ref _surmiseImageDisable, "/Assets/edit_u.png");

            InitializeComponent();
            InitDictionary(GetSmallGrids(Playground), out _gameGrids, GameGrid_MouseLeftButtonUp);
            InitDictionary(GetSmallGrids(SolveField), out _solveGrids, SolveGrid_MouseLeftButtonUp);

            UserViewModel = new UserViewModel();
            DataContext = UserViewModel;
            LoadUser().ForEach(i => User.Instance.RecordInfo(i.Difficulty, (int)i.Time.TotalSeconds));

            _field = new Field(3);
            _field.OnSolvingFinished += OnSolvingFinished;
            _field.OnFieldContentChanged += OnFieldContentChanged;
            _field.CellContentChanged += OnCellContentChanged;
            GenerateNewField();

            _toSolveField = new Field(0);
            _toSolveField.OnFieldContentChanged += OnSolveFieldContentChanged;
            _toSolveField.CellContentChanged += OnSolveCellContentChanged;

            GameMode_Grid.Visibility = Visibility.Visible;
            SolveMode_Grid.Visibility = Visibility.Hidden;
            Playground.Focus();
            OnSolveFieldContentChanged();

        }
        #region Help methods
        private void GenerateNewField()
        {
            _field.GenerateNewField(CurrentDifficulty);
            Timer.Start();
        }
        private void InitDictionary(List<Grid> grids, out Dictionary<(int, int), Grid> dict, MouseButtonEventHandler handler)
        {
            dict = new Dictionary<(int, int), Grid>();

            for (int cubeIndex = 0; cubeIndex < 9; cubeIndex++)
            {
                for (int innerIndex = 0; innerIndex < 9; innerIndex++)
                {
                    int listIndex = cubeIndex * 9 + innerIndex;
                    dict.Add((cubeIndex, innerIndex), grids[listIndex]);
                    grids[listIndex].MouseLeftButtonUp += handler;
                }
            }
        }
        private List<Grid> GetSmallGrids(Grid generalGrid)
        {
            return generalGrid.FindVisualChildren<Grid>().Where(g => g.Height == 50).ToList();
        }
        private void InitBitmapImage(ref BitmapImage image, string source)
        {
            if (image == null)
                image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(source, UriKind.Relative);
            image.EndInit();
        }
        private void UpdateCell(Field field, Cell cell, Dictionary<(int, int), Grid> grids, FieldContentChangedHandler handler)
        {
            var tb = grids[cell.Coordinate].FindVisualChildren<TextBlock>().First();
            tb.Text = cell.GetCellContent();
            tb.FontSize = cell.Value == 0 ? 13 : 24;
            tb.Opacity = cell.Value == 0 ? 0.8 : 1;
            tb.Foreground = cell.IsGenerated ? FieldPrinter.BlackBrush : FieldPrinter.NonGeneratedBrush;
            if (field.SelectedCell != null)
                handler();
        }
        private void SwitchTabs()
        {
            if (GameMode_Grid.Visibility != Visibility.Visible)
            {
                GameMode_Grid.Visibility = Visibility.Visible;
                Stats_StackPanel.Visibility = Visibility.Visible;
                SolveMode_Grid.Visibility = Visibility.Hidden;
                Playground.Focus();
                Timer.Resume();
            }
            else
            {
                SolveMode_Grid.Visibility = Visibility.Visible;
                GameMode_Grid.Visibility = Visibility.Hidden;
                Stats_StackPanel.Visibility = Visibility.Hidden;
                SolveField.Focus();
                Timer.Pause();
            }
        }
        #endregion
        #region User Responsibility
        private List<Info> LoadUser()
        {
            if (File.Exists(_userPath) == false)
                File.WriteAllText(_userPath, "[]");
            string data = File.ReadAllText(_userPath);
            if (ValidateJSON(data))
            {
                return JsonConvert.DeserializeObject<List<Info>>(data) ?? new List<Info>();
            }
            else
            {
                File.WriteAllText(_userPath, "[]");
                return new List<Info>();
            }
            bool ValidateJSON(string s)
            {
                try
                {
                    JToken.Parse(s);
                    return true;
                }
                catch (JsonReaderException ex)
                {
                    Trace.WriteLine(ex);
                    return false;
                }
            }
        }
        private void UnloadUser()
        {
            string data = JsonConvert.SerializeObject(User.Instance.Info, Formatting.Indented);
            File.WriteAllText(_userPath, data);
        }
        #endregion
        #region Brush Responsibility
        private void BrushLinked(Field field, Dictionary<(int, int), Grid> grids)
        {
            var linked = field.GetAllLinked().Select(cell => grids[cell.Coordinate]);
            FieldPrinter.PrintCells(linked, FieldPrinter.PrintedBrush);
        }
        private void BrushSameValue()
        {
            var sameToSelected = _field.GetSameValues().Select(cell => _gameGrids[cell.Coordinate]);
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
        #endregion
        #region Common Control Methods
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                SwitchTabs();
            }
            else if (e.Key == Key.E)
            {
                IsSurmiseMode = !IsSurmiseMode;
            }
            else if (e.Key == Key.C)
            {
                AutoCheck = !AutoCheck;
            }
            else
            {
                if (GameMode_Grid.Visibility == Visibility.Visible)
                    SolveField.Focus();
                else
                    Playground.Focus();
            }
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SwitchTabs();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            UnloadUser();
        }
        private void DifficultyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            CurrentDifficulty = (Difficulty)int.Parse(item.Tag.ToString());
            GenerateNewField();
        }
        #endregion

        #region GamePart
        private void OnSolvingFinished(bool user)
        {
            TimeSpan result = Timer.Stop();
            var messageBoxResult = MyMessageBox.Show("Well done!\nWant to create new field?", "Finished solving", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (messageBoxResult == MessageBoxResult.OK)
            {
                GenerateNewField();
            }
            if (user)
                User.Instance.RecordInfo(CurrentDifficulty, (int)result.TotalSeconds);
        }
        private void GameGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Grid grid = (Grid)sender;
            _field.MoveSelection(_gameGrids.First(p => p.Value == grid).Key);
        }
        private void OnCellContentChanged(Cell cell)
        {
            UpdateCell(_field, cell, _gameGrids, OnFieldContentChanged);
        }
        private void OnSolveCellContentChanged(Cell cell)
        {
            UpdateCell((Field)_toSolveField, cell, _solveGrids, OnSolveFieldContentChanged);
        }
        private void OnFieldContentChanged()
        {
            FieldPrinter.PrintCells(_gameGrids.Values, FieldPrinter.WhiteBrush);
            BrushLinked(_field, _gameGrids);
            if (AutoCheck)
                BrushSolved();
            BrushSameValue();
            _gameGrids[_field.SelectedCell.Coordinate].Background = FieldPrinter.SelectedCellBrush;
            if (AutoCheck)
                BrushIncorrect();
        }
        #region Game Control Methods
        private void Playground_KeyUp(object sender, KeyEventArgs e)
        {
            if (_keysValues.Keys.Contains(e.Key))
                _field.TypeValue(_keysValues[e.Key], IsSurmiseMode);
            else if (_navigationKeys.Keys.Contains(e.Key))
                _field.MoveSelection(_navigationKeys[e.Key]);
        }
        private void SurmiseModeButton_Click(object sender, RoutedEventArgs e)
        {
            IsSurmiseMode = !IsSurmiseMode;
        }
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (_field.Undo() == false)
                MyMessageBox.Show("Nothing to undo.", "Invalid operation error.", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void AutoModeButton_Click(object sender, RoutedEventArgs e)
        {
            AutoCheck = !AutoCheck;
        }
        private void HintButton_Click(object sender, RoutedEventArgs e)
        {
            if (_field.HintsLeft > 0)
            {
                try
                {
                    _field.GiveHint();
                }
                catch (InvalidOperationException ex)
                {
                    MyMessageBox.Show(ex.Message, "Invalid operation error.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MyMessageBox.Show("No hints left.", "Invalid operation error.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            if (MyMessageBox.Show("Are you sure you want to finish solving?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                _field.FinishSolving();
        }
        private void RegenButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateNewField();
        }
        #endregion
        #endregion

        #region SolvePart
        private void OnSolveFieldContentChanged()
        {
            FieldPrinter.PrintCells(_solveGrids.Values, FieldPrinter.WhiteBrush);
            BrushLinked((Field)_toSolveField, _solveGrids);
            _solveGrids[((Field)_toSolveField).SelectedCell.Coordinate].Background = FieldPrinter.SelectedCellBrush;
        }
        private void SolveGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Grid grid = (Grid)sender;
            _toSolveField.MoveSelection(_solveGrids.First(p => p.Value == grid).Key);
        }
        #region Solve Control Methods
        private void SolveField_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (_keysValues.Keys.Contains(e.Key))
                _toSolveField.TypeValue(_keysValues[e.Key], false);
            else if (_navigationKeys.Keys.Contains(e.Key))
                _toSolveField.MoveSelection(_navigationKeys[e.Key]);
        }
        private void CustomSolve_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sudokuResultState = _toSolveField.Solve();
                if (sudokuResultState == SudokuResultState.HasTooManySolutions)
                    MyMessageBox.Show("This field has more than one solution", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else if (sudokuResultState == SudokuResultState.HasNoSolution)
                    MyMessageBox.Show("This field has no solutions", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ArgumentException ex)
            {
                MyMessageBox.Show(ex.Message);
            }
        }
        private void CustomClear_Button_Click(object sender, RoutedEventArgs e)
        {
            _toSolveField.Clear();
        }
        #endregion
        #endregion
    }
}
