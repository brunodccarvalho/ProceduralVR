using UnityEngine;

/**
 * Record of world-space delta
 */
public struct DeltaRecord {
    public Vector3 position;
    public Quaternion rotation;

    public DeltaRecord(Transform transform) {
        position = transform.position;
        rotation = transform.rotation;
    }

    public DeltaRecord(Vector3 position, Quaternion rotation) {
        this.position = position;
        this.rotation = rotation;
    }

    public void Apply(Transform transform) {
        transform.SetPositionAndRotation(position, rotation);
    }

    public override string ToString() {
        return string.Format("+{0} O{1}", position, rotation.eulerAngles);
    }
}
