#if false

using UnityEngine;

[DisallowMultipleComponent]
public class TrackingParallelepiped : MonoBehaviour {

    const float epsilon = 0.001f;
    const float threshold = 0.1f;
    public Transform center;
    public Transform corner;
    public Transform cube;
    public Vector3 delta;
    RecursiveOutline outline;

    public static Transform Add(Transform center, Transform corner, GameObject prefab) {
        var cube = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        var tracker = cube.gameObject.AddComponent<TrackingParallelepiped>();
        tracker.center = center;
        tracker.corner = corner;
        tracker.cube = cube.transform;
        tracker.Refresh();
        return cube.transform;
    }

    void Start() {
        outline = cube.gameObject.AddComponent<RecursiveOutline>();
        outline.SetVariant("Parallelepiped");
    }

    void Update() {
        Refresh();
    }

    static Vector3 NormalizeDelta(Vector3 norm) {
        norm.x = Mathf.Abs(norm.x);
        norm.y = Mathf.Abs(norm.y);
        norm.z = Mathf.Abs(norm.z);
        if (norm.x <= threshold) norm.x = epsilon;
        if (norm.y <= threshold) norm.y = epsilon;
        if (norm.z <= threshold) norm.z = epsilon;
        return norm;
    }

    void Refresh() {
        var parent = new ParentRecord(corner);
        corner.parent = center;
        delta = NormalizeDelta(corner.position - center.position);
        parent.Apply();
        parent = new ParentRecord(cube);
        cube.parent = null;
        cube.localScale = 2 * delta;
        cube.position = center.position;
        cube.rotation = center.rotation;
        parent.Apply();
    }

}

#endif
