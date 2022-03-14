using System.Collections.Generic;
using UnityEngine.Events;

public class DelegateLinkedSet<T> : LinkedList<T> {

    public UnityEvent<T> OnInsert;
    public UnityEvent<T> OnRemove;

    Dictionary<T, LinkedListNode<T>> map;

    public DelegateLinkedSet() {
        if (OnInsert == null) OnInsert = new UnityEvent<T>();
        if (OnRemove == null) OnRemove = new UnityEvent<T>();
        map = new Dictionary<T, LinkedListNode<T>>();
    }

    public bool Add(T elem) => AddLast(elem);

    public new bool AddLast(T elem) {
        if (elem == null || map.ContainsKey(elem)) {
            return false;
        } else {
            var node = base.AddLast(elem);
            map[elem] = node;
            OnInsert.Invoke(elem);
            return true;
        }
    }

    public new bool AddFirst(T elem) {
        if (elem == null || map.ContainsKey(elem)) {
            return false;
        } else {
            var node = base.AddFirst(elem);
            map[elem] = node;
            OnInsert.Invoke(elem);
            return true;
        }
    }

    public new bool Remove(T elem) {
        if (elem != null && map.ContainsKey(elem)) {
            base.Remove(map[elem]);
            map.Remove(elem);
            OnRemove.Invoke(elem);
            return true;
        } else {
            return false;
        }
    }

    public new void RemoveFirst() {
        if (map.Count > 0) {
            map.Remove(base.First.Value);
            OnRemove.Invoke(base.First.Value);
            base.RemoveFirst();
        }
    }

    public new void RemoveLast() {
        if (map.Count > 0) {
            map.Remove(base.Last.Value);
            OnRemove.Invoke(base.Last.Value);
            base.RemoveLast();
        }
    }

    // Returns true if elem was inserted
    public bool Toggle(T elem) {
        if (elem == null) {
            return false;
        } else if (map.ContainsKey(elem)) {
            Remove(elem);
            return false;
        } else {
            AddLast(elem);
            return true;
        }
    }

    public new void Clear() {
        foreach (T elem in this) OnRemove.Invoke(elem);
        base.Clear();
        map.Clear();
    }

    public new bool Contains(T elem) {
        return elem != null && map.ContainsKey(elem);
    }

}
