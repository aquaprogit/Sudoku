using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sudoku.Model.DancingLinksX;

namespace Sudoku.Model
{
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
}
