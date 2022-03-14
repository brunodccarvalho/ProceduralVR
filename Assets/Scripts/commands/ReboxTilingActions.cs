using UnityEngine;

[System.Serializable]
public struct TilingRecountAction : UndoableAction {
    ProceduralTiling procedural;
    int before, after;

    // * OK: Idempotent, applies immediately
    public TilingRecountAction(ProceduralTiling proc, int before, int after) {
        this.procedural = proc;
        this.before = before;
        this.after = after;
        Redo();
    }

    public void Undo() { procedural.MakeActiveCount(before); }

    public void Redo() { procedural.MakeActiveCount(after); }

    public void Commit() { }

    public void Forget() { }

    public override string ToString() {
        return string.Format("TilingRecount delta={0} {1}", after - before, procedural.name);
    }
}
