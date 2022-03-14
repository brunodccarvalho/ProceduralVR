using UnityEngine;

public struct ParentRecord {
    public Transform parent;
    public int index;
    public Transform transform;

    public ParentRecord(Transform transform) {
        this.parent = transform.parent;
        this.index = transform.parent != null ? transform.GetSiblingIndex() : -1;
        this.transform = transform;
    }

    public void Apply() {
        this.transform.parent = this.parent;
        if (this.parent != null) {
            this.transform.SetSiblingIndex(index);
        }
    }

    public override string ToString() {
        if (parent != null) {
            return string.Format("{0}->{1}[{2}]", transform.name, parent?.name, index);
        } else {
            return string.Format("{0}->NULL", transform.name);
        }
    }
}
