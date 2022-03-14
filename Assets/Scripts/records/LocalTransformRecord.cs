using UnityEngine;

public struct TransformRecord {
    public Transform transform;
    public LocalDeltaRecord delta;
    public Vector3 position => delta.position;
    public Quaternion rotation => delta.rotation;

    public TransformRecord(Transform transform) {
        this.transform = transform;
        this.delta = new LocalDeltaRecord(transform);
    }

    public void Apply() {
        delta.Apply(transform);
    }

    public override string ToString() {
        return string.Format("{0} {1}", transform.name, delta);
    }
}
