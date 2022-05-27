using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sudoku.Model
{
    internal class FieldSolver
    {
        private readonly Header _root;
        private readonly List<Header> _columns = new List<Header>();
        private readonly Dictionary<string, int> _names = new Dictionary<string, int>();
        private readonly List<Node> _rows = new List<Node>();
        private readonly List<Node> _stack = new List<Node>();
        private int _iterStack;
        private Node _iterRow;
        private Header _column;
        private Node _row;
        private bool _more;
        private void Search()
        {
            if (_root.Right == _root)
            {
                NumSolutions++;
                _iterStack = 0;
                _iterRow = (_iterStack != _stack.Count ? _stack[_iterStack] : null);
                _more = Record();
                return;
            }
            Choose();
            Cover(_column);
            _stack.Add(null);
            for (_row = _column.Down; _row != _column; _row = _row.Down)
            {
                for (var i = _row.Right; i != _row; i = i.Right)
                {
                    Cover(i.Head);
                }
                _stack[_stack.Count - 1] = _row;
                Search();
                _row = _stack[_stack.Count - 1];
                _column = _row.Head;
                for (var i = _row.Left; i != _row; i = i.Left)
                {
                    Uncover(i.Head);
                }
                if (!_more) break;
            }
            _stack.RemoveAt(_stack.Count - 1);
            Uncover(_column);
        }
        private void Cover(Header col)
        {
            col.Right.Left = col.Left;
            col.Left.Right = col.Right;
            DequeRemovals++;
            for (var i = col.Down; i != col; i = i.Down)
            {
                for (var j = i.Right; j != i; j = j.Right)
                {
                    j.Down.Up = j.Up;
                    j.Up.Down = j.Down;
                    j.Head.Size--;
                    DequeRemovals++;
                }
            }
        }
        private void Uncover(Header col)
        {
            for (var i = col.Up; i != col; i = i.Up)
            {
                for (var j = i.Left; j != i; j = j.Left)
                {
                    j.Head.Size++;
                    j.Down.Up = j;
                    j.Up.Down = j;
                }
            }
            col.Right.Left = col;
            col.Left.Right = col;
        }
        private void Choose()
        {
            if (UseGolumbHeuristic)
            {
                int best = int.MaxValue;
                for (Header i = (Header)_root.Right; i != _root; i = (Header)i.Right)
                {
                    if (i.Size >= best) continue;
                    _column = i;
                    best = i.Size;
                }
            }
            else
            {
                _column = (Header)_root.Right;
            }
        }
        private class Node
        {
            public Node Left, Right, Up, Down;
            public Header Head;
        }
        private class Header : Node
        {
            public int Size;
            public string Name;
        }
        protected virtual string GetSolution()
        {
            if (_iterRow != null)
            {
                string ret = _iterRow.Head.Name;
                _iterRow = _iterRow.Right;
                if (_iterRow == _stack[_iterStack])
                    _iterRow = null;
                return ret;
            }

            if (_iterStack != _stack.Count && ++_iterStack != _stack.Count)
            {
                _iterRow = _stack[_iterStack];
            }
            return null;
        }
        protected virtual bool Record()
        {
            var solutionEventHandler = _solutionListener;
            if (solutionEventHandler == null)
                return true;
            List<List<string>> solutionNodes = new List<List<string>>();
            for (string s = GetSolution(); s != null; s = GetSolution())
            {
                List<string> lineNodes = new List<string>();
                for (; s != null; s = GetSolution())
                    lineNodes.Add(s);
                if (lineNodes.Any())
                    solutionNodes.Add(lineNodes);
            }
            bool retval = true;
            if (solutionEventHandler != null)
                retval = solutionEventHandler(NumSolutions, DequeRemovals, solutionNodes);
            return retval;
        }
        private SolutionRecorderDelegateHandler _solutionListener;
        public FieldSolver()
        {
            _root = new Header();
            _root.Left = _root.Right = _root;
            _row = null;
            UseGolumbHeuristic = true;
        }

        private long NumSolutions { get; set; }
        private long DequeRemovals { get; set; }
        private bool UseGolumbHeuristic { get; set; }
        private void AddColumn(string name, bool mandatory = true)
        {
            Header col = new Header { Name = name, Size = 0 };
            col.Up = col.Down = col.Left = col.Right = col.Head = col;
            if (mandatory)
            {
                col.Right = _root;
                col.Left = _root.Left;
                _root.Left.Right = col;
                _root.Left = col;
            }
            _names[name] = _columns.Count;
            _columns.Add(col);
        }

        private void NewRow()
        {
            _row = null;
        }

        private void SetColumn(string name)
        {
            if (!_names.ContainsKey(name))
            {
                throw new Exception($"unknown column name {name}");
            }

            SetColumn(_names[name]);
        }

        private void SetColumn(int number)
        {
            if (number >= _columns.Count) return;
            var header = _columns[number];
            Node node = new Node();
            if (_row == null)
            {
                _row = node;
                _rows.Add(node);
                node.Left = node;
                node.Right = node;
            }
            else
            {
                node.Left = _row;
                node.Right = _row.Right;
                _row.Right.Left = node;
                _row.Right = node;
            }
            node.Head = header;
            node.Up = header;
            node.Down = header.Down;
            header.Down.Up = node;
            header.Down = node;
            header.Size++;
        }

        public void Solve(Cell[,] fieldArray)
        {
            for (int i = 1; i <= 9; ++i)
            {
                for (int d = 1; d <= 9; ++d)
                    AddColumn($"C{i}{d}");
            }
            for (int j = 1; j <= 9; ++j)
            {
                for (int d = 1; d <= 9; ++d)
                    AddColumn($"R{j}{d}");
            }
            for (int i = 1; i <= 3; ++i)
            {
                for (int j = 1; j <= 3; ++j)
                {
                    for (int d = 1; d <= 9; ++d)
                        AddColumn($"G{i}{j}{d}");
                }
            }
            for (int i = 1; i <= 9; ++i)
            {
                for (int j = 1; j <= 9; ++j)
                    AddColumn($"F{i}{j}");
            }

            for (int i = 1; i <= 9; ++i)
            {
                for (int j = 1; j <= 9; ++j)
                {
                    int d = fieldArray[i - 1, j - 1].Value;
                    if (d != 0)
                        AddColumn($"H{i}{j}{d}");
                }
            }

            for (int d = 1; d <= 9; ++d)
            {
                for (int i = 1; i <= 9; ++i)
                {
                    for (int j = 1; j <= 9; ++j)
                    {
                        NewRow();
                        SetColumn($"C{i}{d}");
                        SetColumn($"R{j}{d}");
                        SetColumn($"G{1 + (i - 1) / 3}{1 + (j - 1) / 3}{d}");
                        SetColumn($"F{i}{j}");
                        if (fieldArray[i - 1, j - 1].Value == d)
                            SetColumn($"H{i}{j}{d}");
                    }
                }
            }

            _solutionListener += (number, dequeueNumber, solution) => {
                foreach (var triple in solution)
                {
                    string col = triple.FirstOrDefault(s => s[0] == 'C');
                    string row = triple.FirstOrDefault(s => s[0] == 'R');
                    Debug.Assert(col != null && row != null && row[2] == col[2]);
                    int c = col[1] - '0';
                    int r = row[1] - '0';
                    int d = col[2] - '0';
                    if (fieldArray[c - 1, r - 1].Value == 0)
                        fieldArray[c - 1, r - 1].Value = d;
                    else
                        Debug.Assert(fieldArray[c - 1, r - 1].Value == d);
                }
                return true;
            };
            _more = true;
            _stack.Clear();
            DequeRemovals = 0;
            NumSolutions = 0;
            Search();
        }
        private delegate bool SolutionRecorderDelegateHandler(long solutionNumber, long dequeueNumber, List<List<string>> solution);

    }
}