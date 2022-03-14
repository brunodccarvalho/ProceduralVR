using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CreateGroupAction : UndoableAction {
    public ProceduralGroup procedural;
    public List<ParentTransformRecord> before;
    public List<ParentTransformRecord> after;

    // * OK: Idempotent, applies immediately
    public CreateGroupAction(List<Transform> elements) {
        this.procedural = ProceduralFactory.EmptyGroup();
        procedural.transform.position = Vector3.zero;
        this.before = elements.ConvertAll(child => new ParentTransformRecord(child));
        foreach (Transform child in elements) {
            child.parent = procedural.transform;
        }
        var offset = elements[0].position;
        foreach (Transform child in elements) {
            child.position -= offset;
        }
        procedural.transform.position = offset;
        this.after = elements.ConvertAll(child => new ParentTransformRecord(child));
    }

    public void Undo() {
        before.ForEach(child => child.Apply());
        procedural.gameObject.SetActive(false);
    }

    public void Redo() {
        after.ForEach(child => child.Apply());
        procedural.gameObject.SetActive(true);
    }

    public void Commit() { }

    public void Forget() { Object.Destroy(procedural.gameObject); }

    public override string ToString() {
        var namesList = after.ConvertAll(child => child.transform.name);
        string names = string.Join(", ", namesList);
        return string.Format("CreateGroup [{0}] [{1}]", procedural.name, names);
    }
}
