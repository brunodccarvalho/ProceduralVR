using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CreateRandomAction : UndoableAction {
    public ProceduralRandom procedural;
    public List<ParentTransformRecord> before, after;

    public CreateRandomAction(List<Transform> variants) {
        this.procedural = ProceduralFactory.EmptyRandom();
        this.before = variants.ConvertAll(child => new ParentTransformRecord(child));
        var first = variants[0];
        procedural.transform.position = first.position;
        foreach (Transform child in variants) {
            child.parent = procedural.transform;
            if (child == first) {
                child.transform.localPosition = Vector3.zero;
                child.gameObject.SetActive(true);
            } else {
                child.transform.localPosition = first.transform.localPosition;
                // child.transform.localRotation = first.transform.localRotation;
                child.gameObject.SetActive(false);
            }
        }
        procedural.activeChild = variants[0];
        this.after = variants.ConvertAll(child => new ParentTransformRecord(child));
    }

    public void Undo() {
        before.ForEach(child => child.Apply());
        before.ForEach(child => child.transform.gameObject.SetActive(true));
        procedural.gameObject.SetActive(false);
    }

    public void Redo() {
        after.ForEach(child => child.Apply());
        after.ForEach(child => child.transform.gameObject.SetActive(false));
        procedural.gameObject.SetActive(true);
        procedural.activeChild = after[0].transform;
        after[0].transform.gameObject.SetActive(true);
    }

    public void Commit() { }

    public void Forget() { Object.Destroy(procedural.gameObject); }

    public override string ToString() {
        var names = before.ConvertAll(child => child.transform.name);
        string nameList = string.Join(", ", names);
        return string.Format("CreateRandom [{0}] [{1}]", procedural.name, nameList);
    }
}
