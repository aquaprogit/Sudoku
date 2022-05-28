using System;
using System.Collections.Generic;

namespace Sudoku.Model.DancingLinksX
{
    internal class Node
    {
        internal Node header, row;
        internal Node up, down, left, right;
        internal int constraint, set, rowCount;

        internal Node()
        {
            up = down = left = right = header = row = this;
        }

        internal bool IsLast => right == this;

        internal void AddLast(Node node)
        {
            row.left.Append(node);
        }

        internal void AddLastDown(Node node)
        {
            header.up.AppendDown(node);
        }

        internal void Append(Node node)
        {
            right.left = node;
            node.right = right;
            node.left = this;
            right = node;
        }

        internal void AppendDown(Node node)
        {
            down.up = node;
            node.down = down;
            node.up = this;
            down = node;
            header.rowCount++;
        }

        internal IEnumerable<Node> Iterate(Func<Node, Node> direction)
        {
            var node = this;
            do
            {
                yield return node;
                node = direction(node);

            } while (node != this);
        }

        public override string ToString()
        {
            bool isHeader = header == this;
            bool isRow = row == this;
            bool isRoot = isHeader && isRow;

            return isRoot ? "R"
                : isHeader ? $"H{header.constraint}"
                : isRow ? $"R{row.set}"
                : $"C({header.constraint},{row.set})";
        }
    }
}
