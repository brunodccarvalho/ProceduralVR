using UnityEngine;

[System.Serializable]
public struct GrabAction : UndoableAction {
    TransformRecord start, end;

    // * OK: Idempotent, applies immediately
    public GrabAction(TransformRecord start, TransformRecord end) {
        Debug.Assert(start.transform == end.transform);
        this.start = start;
        this.end = end;
        Redo();
    }

    public void Undo() { start.Apply(); }

    public void Redo() { end.Apply(); }

    public void Commit() { }

    public void Forget() { }

    public override string ToString() {
        return string.Format("Grab [{0}] [{1}]", start, end);
    }
}

public struct ParentGrabAction : UndoableAction {
    ParentTransformRecord start, end;

    // * OK: Idempotent, applies immediately
    public ParentGrabAction(ParentTransformRecord start, ParentTransformRecord end) {
        Debug.Assert(start.transform == end.transform);
        this.start = start;
        this.end = end;
        Redo();
    }

    public void Undo() { start.Apply(); }

    public void Redo() { end.Apply(); }

    public void Commit() { }

    public void Forget() { }

    public override string ToString() {
        return string.Format("Grab [{0}] [{1}]", start, end);
    }
}
