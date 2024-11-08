﻿using System.Collections;
using System.Collections.Specialized;

namespace Sudoku.Common.Models;

public class SurmiseList : IEquatable<SurmiseList>, ICollection<int>
{
    private List<int> _surmises;

    public int this[int index] => _surmises[index];
    public int Count => _surmises.Count;
    public bool IsReadOnly => ((ICollection<int>)_surmises).IsReadOnly;
    public SurmiseList()
    {
        _surmises = new List<int>();
    }

    public SurmiseList(List<int> surmises)
    {
        _surmises = new List<int>();
        Add(surmises);
    }

    public event NotifyCollectionChangedEventHandler? OnCollectionChanged;

    public void Add(int surmise)
    {
        if (surmise == 0)
        {
            Clear();
            return;
        }
        if (surmise is < 1 or > 9)
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

    public bool Equals(SurmiseList? other)
    {
        return other != null && other._surmises.SequenceEqual(_surmises);
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

    public void CopyTo(int[] array, int arrayIndex)
    {
        ((ICollection<int>)_surmises).CopyTo(array, arrayIndex);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as SurmiseList);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}