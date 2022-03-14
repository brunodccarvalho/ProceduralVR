using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LinkedDelete : UndoableAction {
    public MultiUndoableAction deletes;
    public Transform first => ((DeleteAction)deletes.actions[0]).transform;

    public LinkedDelete(List<Transform> transforms) {
        this.deletes = new MultiUndoableAction();
        for (int i = 0; i < transforms.Count; i++) {
            this.deletes.Add(new DeleteAction(transforms[i]));
        }
    }

    public void Undo() { deletes.Undo(); }

    public void Redo() { deletes.Redo(); }

    public void Commit() { deletes.Commit(); }

    public void Forget() { deletes.Forget(); }

    public override string ToString() {
        var name = first.name;
        return string.Format("LinkedDelete [{0}] x{1}", name, deletes.Count);
    }
}
