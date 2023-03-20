namespace Sudoku.Model
{
    internal interface ICommand
    {
        void Execute(int value, bool isSurmise);
        Cell Undo();
    }
}
