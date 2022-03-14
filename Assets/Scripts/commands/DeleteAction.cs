using UnityEngine;

[System.Serializable]
public struct DeleteAction : UndoableAction {
    public ParentRecord parentRecord;
    public Transform transform => parentRecord.transform;
    public Transform parent => parentRecord.parent;
    private Transform cousin;

    // * OK: Idempotent, excludes object and applies immediately
    public DeleteAction(Transform transform) {
        this.parentRecord = new ParentRecord(transform);
        this.cousin = LinkedProcedural.Unlink(transform);
        this.transform.gameObject.SetActive(false);
        this.transform.parent = Scenario.current?.root;
    }

    public void Undo() {
        transform.gameObject.SetActive(true);
        parentRecord.Apply();
        LinkedProcedural.Relink(transform, cousin);
    }

    public void Redo() {
        transform.gameObject.SetActive(false);
        transform.parent = Scenario.current?.root;
        LinkedProcedural.Unlink(transform);
    }

    public void Commit() { Object.Destroy(transform.gameObject); }

    public void Forget() { }

    public override string ToString() {
        return string.Format("Delete [{0}]", transform.name);
    }
}
