using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DisbandRotationAction : UndoableAction {
    public ProceduralRotation procedural;
    public Transform parent;
    public List<ParentRecord> before, after;

    public DisbandRotationAction(ProceduralRotation procedural) {
        var list = procedural.ProceduralChildren(true);
        this.procedural = procedural;
        this.parent = procedural.transform.parent;
        this.before = list.ConvertAll(child => new ParentRecord(child));
        this.before.Add(new ParentRecord(procedural.transform));
        list.ForEach(child => child.transform.parent = procedural.transform.parent);
        procedural.transform.parent = Scenario.current?.root;
        this.after = list.ConvertAll(child => new ParentRecord(child));
        this.after.Add(new ParentRecord(procedural.transform));
    }

    public DisbandRotationAction(Transform transform)
        : this(transform.GetComponent<ProceduralRotation>()) { }

    public void Undo() {
        before.ForEach(rec => rec.Apply());
        procedural.gameObject.SetActive(true);
    }

    public void Redo() {
        after.ForEach(rec => rec.Apply());
        procedural.gameObject.SetActive(false);
    }

    public void Commit() { Object.Destroy(procedural.gameObject); }

    public void Forget() { }

    public override string ToString() {
        var names = after.ConvertAll(child => child.transform.name);
        string nameList = string.Join(", ", names);
        return string.Format("DisbandPosition [{0}] [{1}]", procedural.name, nameList);
    }
}
