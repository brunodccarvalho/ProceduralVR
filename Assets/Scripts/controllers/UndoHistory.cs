using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UndoHistory {

    public static UndoHistory current, independent = new UndoHistory();
    public void Activate() => current = this;

    LinkedList<UndoableAction> history;
    LinkedListNode<UndoableAction> currentNode, headNode;
    MultiUndoableAction transaction;

    // The action specified in 'currentNode' has already been applied.

    public UndoHistory() {
        history = new LinkedList<UndoableAction>();
        headNode = currentNode = history.AddLast((UndoableAction)null);
        transaction = new MultiUndoableAction();
    }

    public LinkedListNode<UndoableAction> CurrentPointer() {
        return currentNode;
    }

    public bool Undo() {
        if (currentNode != headNode) {
            currentNode.Value.Undo();
            currentNode = currentNode.Previous;
            return true;
        } else {
            return false;
        }
    }

    public bool UndoAfter(LinkedListNode<UndoableAction> a) {
        return currentNode != a && Undo();
    }

    public bool Redo() {
        if (currentNode.Next != null) {
            currentNode = currentNode.Next;
            currentNode.Value.Redo();
            return true;
        } else {
            return false;
        }
    }

    public bool RedoBefore(LinkedListNode<UndoableAction> a) {
        return currentNode != a && Redo();
    }

    public void Commit() {
        CutAfter(currentNode);
        currentNode = history.AddLast(transaction);
        transaction = new MultiUndoableAction();
        Interactor.instance.currentState.undoEnd = currentNode;
    }

    public void Discard() {
        transaction.Undo();
        transaction.Forget();
        transaction.Clear();
    }

    public void AddLazy(UndoableAction action) {
        Debug.Assert(action != null);
        transaction.Add(action);
    }

    public void CutAll() {
        CutAfter(headNode);
    }

    public void Clear() {
        while (history.Count > 1) history.RemoveLast();
    }

    public void CommitAllAndClear() {
        var node = history.First.Next;
        while (node != null) {
            node.Value.Commit();
            if (node == currentNode) break;
            node = node.Next;
        }
        Clear();
    }

    public void UndoAllAndClear() {
        while (history.Count > 1) Undo();
        Clear();
    }

    public string Format() {
        var s = new StringBuilder();
        foreach (MultiUndoableAction action in history) {
            if (action != null) s.AppendLine(action.ToString());
        }
        return s.ToString();
    }

    private void CutAfter(LinkedListNode<UndoableAction> node) {
        while (history.Count > 1 && history.Last != node) {
            history.Last.Value.Forget();
            history.RemoveLast();
        }
    }

}
