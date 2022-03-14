using System.Collections.Generic;
using UnityEngine.Events;

public class DelegateLinkedMultiset<T> : LinkedList<T> {

    public UnityEvent<T> OnInsert;
    public UnityEvent<T> OnRemove;

    class MultiNode {
        public int count; public LinkedListNode<T> node;
        public MultiNode(LinkedListNode<T> node) { this.count = 1; this.node = node; }
    }
    Dictionary<T, MultiNode> map;

    public DelegateLinkedMultiset() {
        if (OnInsert == null) OnInsert = new UnityEvent<T>();
        if (OnRemove == null) OnRemove = new UnityEvent<T>();
        map = new Dictionary<T, MultiNode>();
    }

    public bool Add(T elem) => AddLast(elem);

    public new bool AddLast(T elem) {
        if (elem == null) {
            return false;
        } else if (map.ContainsKey(elem)) {
            map[elem].count++;
            return false;
        } else {
            var node = base.AddLast(elem);
            map[elem] = new MultiNode(node);
            OnInsert.Invoke(elem);
            return true;
        }
    }

    public new bool AddFirst(T elem) {
        if (elem == null) {
            return false;
        } else if (map.ContainsKey(elem)) {
            map[elem].count++;
            return false;
        } else {
            var node = base.AddFirst(elem);
            map[elem] = new MultiNode(node);
            OnInsert.Invoke(elem);
            return true;
        }
    }

    public new bool Remove(T elem) {
        if (elem == null || !map.ContainsKey(elem) || --map[elem].count > 0) {
            return false;
        } else {
            base.Remove(map[elem].node);
            map.Remove(elem);
            OnRemove.Invoke(elem);
            return true;
        }
    }

    public bool RemoveAll(T elem) {
        if (elem == null || !map.ContainsKey(elem)) {
            return false;
        } else {
            base.Remove(map[elem].node);
            map.Remove(elem);
            OnRemove.Invoke(elem);
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
