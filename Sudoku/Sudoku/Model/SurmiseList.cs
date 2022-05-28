using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Sudoku.Model
{

    internal class SurmiseList : IEnumerable<int>, IEquatable<SurmiseList>
    {
        private List<int> _surmises;

        public int this[int index] {
            get => _surmises[index];
        }
        public SurmiseList()
        {
            _surmises = new List<int>();
        }
        public SurmiseList(List<int> surmises)
        {
            _surmises = new List<int>();
            Add(surmises);
        }

        public event NotifyCollectionChangedEventHandler OnCollectionChanged;
        public int Count { get => _surmises.Count; }

        public void Add(int surmise)
        {
            if (surmise == 0)
            {
                Clear();
                return;
            }
            if (surmise < 1 || surmise > 9)
                throw new ArgumentOutOfRangeException(nameof(surmise));
            if (Contains(surmise))
                throw new Exception("Item already in collection");
            _surmises.Add(surmise);
            OnCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<int>() { surmise }));
        }
        public void Add(IEnumerable<int> surmises)
        {
            foreach (int surmise in surmises)
                Add(surmise);
        }
        public bool Remove(int surmise)
        {
            bool result = _surmises.Remove(surmise);
            if (result)
                OnCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<int>() { surmise }));
            return result;
        }
        public void Remove(IEnumerable<int> surmises)
        {
            foreach (int surmise in surmises)
                Remove(surmise);
        }

        public void Clear()
        {
            _surmises.Clear();
            OnCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(int surmise)
        {
            return _surmises.Contains(surmise);
        }

        public IEnumerator<int> GetEnumerator()
        {
            return _surmises.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(SurmiseList other)
        {
            return other._surmises.SequenceEqual(_surmises);
        }

        public override int GetHashCode()
        {
            int source = 0;
            foreach (int i in _surmises)
            {
                source ^= (int)Math.Pow(2, i - 1);
            }
            return source;
        }
    }
}
