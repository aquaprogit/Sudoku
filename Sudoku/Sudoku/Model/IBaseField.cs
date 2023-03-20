using Sudoku.DancingLinksX;

namespace Sudoku.Model;

internal interface IBaseField
{
    void MoveSelection(Direction dir);
    void MoveSelection((int, int) coordinate);
    void TypeValue(int value, bool isSurmise);
    SudokuResultState Solve();
    void Clear();
    event FieldContentChangedHandler OnFieldContentChanged;
    event CellContentChangedHandler CellContentChanged;
}
