using UnityEngine;

[System.Serializable]
public struct UnlinkAction : UndoableAction {
    Transform separated, cousin;

    // * OK: Idempotent, applies immediately
    public UnlinkAction(Transform separated, Transform cousin) {
        this.separated = separated;
        this.cousin = cousin;
        Redo();
    }

    public void Undo() {
        LinkedProcedural.Relink(separated, cousin);
    }

    public void Redo() {
        LinkedProcedural.Unlink(separated);
    }

    public void Commit() { }

    public void Forget() { }

    public override string ToString() {
        return string.Format("Unlink [{0}] from [{1}]", separated.name, cousin.name);
    }
}
