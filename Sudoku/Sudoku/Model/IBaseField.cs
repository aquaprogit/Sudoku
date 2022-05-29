using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Model
{
    internal interface IBaseField
    {
        void MoveSelection(Direction dir);
        void TypeValue(int value, bool isSurmise);
        void Solve();
        void Clear();
    }
}
