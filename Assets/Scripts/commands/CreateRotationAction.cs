using UnityEngine;

[System.Serializable]
public struct CreateRotationAction : UndoableAction {
    public ProceduralRotation procedural;
    public ParentTransformRecord before, after;
    public Transform child => after.transform;

    public CreateRotationAction(Transform element) {
        this.procedural = ProceduralFactory.EmptyRotation();
        this.before = new ParentTransformRecord(element);
        procedural.child = element;
        procedural.transform.position = element.position;
        element.parent = procedural.transform;
        element.localPosition = Vector3.zero;
        procedural.bell.localRotation = element.localRotation;
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
        return string.Format("CreateRotation {0}", child.name);
    }
}
