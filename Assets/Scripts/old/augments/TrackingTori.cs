#if false
using UnityEngine;

[DisallowMultipleComponent]
public class TrackingTori : MonoBehaviour {

    public Transform root; // this.transform
    public Transform center;

    public Transform[] arcs;
    public Transform[] pivot;
    public Transform[] torus;
    public Plane[] planes;

    public float sphereRadius;
    public Vector3 currentDelta;

    public ProceduralRotation procedural => center.GetComponent<ProceduralRotation>();
    public Vector3 procDelta => procedural.delta;

    public static Transform Add(Transform center, GameObject prefab) {
        var root = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        var tracker = root.AddComponent<TrackingTori>();
        root.transform.SetPositionAndRotation(center.position, center.rotation);

        tracker.root = root.transform;
        tracker.center = center;
        tracker.Setup();
        return root.transform;
    }

    float ComputeSphereRadius() {
        var bounds = Math3d.GetBounds(center);
        float S = Mathf.Max(bounds.size[0], Mathf.Max(bounds.size[1], bounds.size[2]));
        return S;
    }

    Vector3 PivotForward(int i) {
        Vector3 forward = Vector3.zero;
        forward[(i + 1) % 3] = -1;
        return forward;
    }

    Vector3 PlaneNormal(int i) {
        Vector3 normal = Vector3.zero;
        normal[i] = 1;
        return normal;
    }

    public void Setup() {
        sphereRadius = ComputeSphereRadius();

        arcs = new Transform[3];
        arcs[0] = root.transform.Find("Arc-X");
        arcs[1] = root.transform.Find("Arc-Y");
        arcs[2] = root.transform.Find("Arc-Z");

        pivot = new Transform[3];
        pivot[0] = root.transform.Find("Pivot-X");
        pivot[1] = root.transform.Find("Pivot-Y");
        pivot[2] = root.transform.Find("Pivot-Z");

        torus = new Transform[3];
        torus[0] = root.transform.Find("Torus-X");
        torus[1] = root.transform.Find("Torus-Y");
        torus[2] = root.transform.Find("Torus-Z");

        planes = new Plane[3];

        for (int i = 0; i < 3; i++) {
            var normal = center.TransformDirection(PlaneNormal(i)).normalized;
            planes[i] = new Plane(normal, center.position);
            UpdateTorus(torus[i], sphereRadius, 0.025f);
        }

        SetDelta(procDelta);
    }

    public void SetDelta(Vector3 delta) {
        currentDelta = delta;
        for (int i = 0; i < 3; i++) {
            RepositionPivot(i);
            UpdateArc(arcs[i], sphereRadius, currentDelta[i]);
        }
    }

    public void SetDeltaOnly(int d) {
        SetDelta(Vector3.Scale(currentDelta, PlaneNormal(d)));
    }

    public void RepositionPivot(int i) {
        var euler = Vector3.Scale(currentDelta, PlaneNormal(i)) * Mathf.Rad2Deg;
        var forward = PivotForward(i) * sphereRadius;
        pivot[i].localPosition = Quaternion.Euler(euler) * forward;
        Debug.LogFormat("pivot[{0}]: {1} {2} {3}", i, euler, forward, pivot[i].localPosition);
    }

    public int GetDimension(Transform target) {
        for (int i = 0; i < 3; i++) {
            if (target == pivot[i] || target == torus[i]) {
                return i;
            }
        }
        return -1;
    }

    void Update() {
        Refresh();
    }

    void Refresh() {
        var prevDelta = currentDelta;
        for (int i = 0; i < 3; i++) {
            var line = pivot[i].position - center.position;
            var forward = center.TransformDirection(PivotForward(i)).normalized;
            var angle = Vector3.Angle(line, forward);
            currentDelta[i] = angle * Mathf.Deg2Rad;
        }
        currentDelta = ProceduralRotation.NormalizeDelta(currentDelta);
        for (int i = 0; i < 3; i++) {
            if (currentDelta[i] != prevDelta[i]) {
                UpdateArc(arcs[i], sphereRadius, currentDelta[i]);
            }
        }
    }

    public Transform ApplyGrabLocks(int i) {
        Grabber.instance.DisableUserLocks();
        Grabber.instance.AllowBelowFloor();
        Grabber.instance.IgnoreRotation();
        Grabber.instance.LockOnPlane(planes[i]);
        Grabber.instance.LockOnSphere(center.position, sphereRadius);
        return pivot[i];
    }

    public void Release() {
        procedural.SetDeltaAllLinks(currentDelta);
    }

    // ***** Dynamic circle arcs

    private static int slices = 100;

    private static void UpdateArc(Transform t, float R, float A) {
        var mesh = t.GetComponent<MeshFilter>().mesh;
        var vertices = new Vector3[slices + 2];
        var triangles = new int[6 * slices];
        vertices[0] = Vector3.zero;

        for (int i = 1; i <= slices; i++) {
            float angle = (2 * A * (i - 1)) / (slices - 1) - A;
            float x = Mathf.Cos(angle), z = Mathf.Sin(angle);
            vertices[i] = R * new Vector3(x, 0, z);
        }

        for (int i = 1, j = 0; i <= slices; i++) {
            triangles[j++] = 0;
            triangles[j++] = i;
            triangles[j++] = i + 1;
            triangles[j++] = 0;
            triangles[j++] = i + 1;
            triangles[j++] = i;
        }

        mesh.triangles = triangles;
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }

    // c -> radius, from center of torus to center of torus tube
    // a -> radius of torus tube
    private static void UpdateTorus(Transform t, float R, float a) {
        var mesh = t.GetComponent<MeshFilter>().mesh;
        var torusSurface = UVSurface.TorusSurface(R, a);
        torusSurface.MakeMesh(mesh, 32);
        t.gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
    }

}
#endif
