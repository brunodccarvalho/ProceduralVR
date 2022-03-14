using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LinkedClone : UndoableAction {
    public MultiUndoableAction clones;
    public Transform first => ((CloneAction)clones.actions[0]).clone;

    public LinkedClone(List<Transform> parents, Transform template, bool active = true) {
        this.clones = new MultiUndoableAction();
        for (int i = 0; i < parents.Count; i++) {
            this.clones.Add(new CloneAction(template, parents[i], active));
        }
    }

    public void Undo() { clones.Undo(); }

    public void Redo() { clones.Redo(); }

    public void Commit() { clones.Commit(); }

    public void Forget() { clones.Forget(); }

    public override string ToString() {
        var name = first.name;
        return string.Format("LinkedClone [{0}] x{1}", name, clones.Count);
    }
}
