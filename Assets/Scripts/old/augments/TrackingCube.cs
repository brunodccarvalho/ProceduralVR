#if false
using UnityEngine;

[DisallowMultipleComponent]
public class TrackingCube : MonoBehaviour {

    public Transform root; // this.transform
    public Transform center;

    public Transform cube;
    public Transform pivot;

    public ProceduralPosition procedural => center.GetComponent<ProceduralPosition>();
    public Vector3 currentDelta;
    public Vector3 procDelta => procedural.delta;

    public static Transform Add(Transform center, GameObject prefab) {
        var root = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        var tracker = root.AddComponent<TrackingCube>();
        root.transform.SetPositionAndRotation(center.position, center.rotation);

        tracker.root = root.transform;
        tracker.center = center;
        tracker.Setup();
        return root.transform;
    }

    public void Setup() {
        pivot = root.transform.Find("Pivot");
        cube = root.transform.Find("Cube");

        var delta = currentDelta = procDelta;
        pivot.localPosition = delta;
        cube.localScale = 2 * delta;
    }

    public void SetDelta(Vector3 delta) {
        currentDelta = delta;
        cube.localScale = 2 * currentDelta;
        pivot.localPosition = currentDelta;
    }

    void Update() {
        Refresh();
    }

    void Refresh() {
        var delta = ProceduralPosition.Normalize(pivot.localPosition);
        SetDelta(delta);
    }

    public void Release() {
        procedural.SetDeltaAllLinks(currentDelta);
    }

}
#endif
