using UnityEngine;

public struct ParentTransformRecord {
    public ParentRecord parent;
    public TransformRecord delta;
    public Transform transform => parent.transform;
    public Vector3 position => delta.position;
    public Quaternion rotation => delta.rotation;

    public ParentTransformRecord(Transform transform) {
        this.parent = new ParentRecord(transform);
        this.delta = new TransformRecord(transform);
    }

    public void Apply() {
        parent.Apply();
        delta.Apply();
    }

    public override string ToString() {
        return string.Format("{0} {1}", parent, delta);
    }
}
