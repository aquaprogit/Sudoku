using Sudoku.Common.Models;

namespace Sudoku.Common.Commands;

public interface ICommand
{
    void Execute(int value, bool isSurmise);
    Cell Undo();
}
