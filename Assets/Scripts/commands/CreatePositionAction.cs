using UnityEngine;

[System.Serializable]
public struct CreatePositionAction : UndoableAction {
    public ProceduralPosition procedural;
    ParentTransformRecord before, after;
    public Transform child => after.transform;

    // * OK: Idempotent, applies immediately
    public CreatePositionAction(Transform element) {
        this.procedural = ProceduralFactory.EmptyPosition();
        this.before = new ParentTransformRecord(element);
        this.procedural.child = element;
        this.procedural.transform.position = element.position;
        // this.procedural.transform.rotation = element.rotation;
        element.parent = procedural.transform;
        element.localPosition = Vector3.zero;
        this.after = new ParentTransformRecord(element);
    }

    public void Undo() {
        before.Apply();
        procedural.gameObject.SetActive(false);
    }

    public void Redo() {
        after.Apply();
        procedural.gameObject.SetActive(true);
    }

    public void Commit() { }

    public void Forget() { Object.Destroy(procedural.gameObject); }

    public override string ToString() {
        return string.Format("CreatePosition {0}", child.name);
    }
}
