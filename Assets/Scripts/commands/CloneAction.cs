using UnityEngine;

[System.Serializable]
public struct CloneAction : UndoableAction {
    public Transform template;
    public Transform clone;
    public ParentRecord parentRecord;
    public bool active;
    public Transform parent => parentRecord.parent;

    // * OK: Idempotent, creates clone and applies immediately
    public CloneAction(Transform template, Transform parent, bool active = true) {
        this.template = template;
        this.clone = ProceduralFactory.ClonePlain(template);
        this.clone.SetParent(parent, true);
        this.clone.gameObject.SetActive(active);
        this.parentRecord = new ParentRecord(this.clone);
        this.active = active;
    }

    // * OK: Idempotent, creates clone and applies immediately, parent is ProceduralRoot.
    public CloneAction(Transform template, bool active = true)
        : this(template, Scenario.current?.root, active) { }

    public void Undo() {
        clone.gameObject.SetActive(false);
        clone.transform.parent = Scenario.current?.root;
        LinkedProcedural.Unlink(clone);
    }

    public void Redo() {
        clone.gameObject.SetActive(active);
        parentRecord.Apply();
        LinkedProcedural.Relink(clone, template);
    }

    public void Commit() { }

    public void Forget() { Object.Destroy(clone.gameObject); }

    public override string ToString() {
        return string.Format("Clone [{0}]", clone.name);
    }
}
