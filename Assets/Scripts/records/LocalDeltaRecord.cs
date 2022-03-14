using UnityEngine;

/**
 * Record of local-space delta
 */
public struct LocalDeltaRecord {
    public Vector3 position;
    public Quaternion rotation;

    public LocalDeltaRecord(Transform transform) {
        position = transform.localPosition;
        rotation = transform.localRotation;
    }

    public LocalDeltaRecord(Vector3 position, Quaternion rotation) {
        this.position = position;
        this.rotation = rotation;
    }

    public void Apply(Transform transform) {
        transform.localPosition = position;
        transform.localRotation = rotation;
    }

    public override string ToString() {
        return string.Format("LocalDeltaRecord +{0} O{1}", position, rotation.eulerAngles);
    }
}
