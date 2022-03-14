using UnityEngine;

[System.Serializable]
public struct CreateTilingAction : UndoableAction {
    public ProceduralTiling procedural;
    public ParentTransformRecord before, after;
    public Transform child => after.transform;

    public CreateTilingAction(Transform element) {
        this.procedural = ProceduralFactory.EmptyTiling();
        this.before = new ParentTransformRecord(element);
        procedural.transform.position = element.position;
        procedural.template = element;
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
        return string.Format("CreateTiling {0}", child.name);
    }
}
