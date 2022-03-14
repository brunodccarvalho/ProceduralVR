using UnityEngine;

[DisallowMultipleComponent]
public class ProceduralRotation : LinkedProcedural {

    public const float snapThreshold = 10f; // degrees

    public Transform child;
    public Transform bell;
    public Transform[] arc;
    public Transform[] pivot;
    public Transform[] torus;
    public Plane[] plane;

    ProceduralRotation() { proctype = ProceduralType.Rotation; }

    public override void GetReferences() {
        this.bell = transform.Find("Rotation-HiddenBell");
        if (bell == null) return;

        this.arc = new Transform[3];
        this.pivot = new Transform[3];
        this.torus = new Transform[3];

        this.arc[0] = transform.Find("Rotation-Arc-X");
        this.arc[1] = transform.Find("Rotation-Arc-Y");
        this.arc[2] = transform.Find("Rotation-Arc-Z");

        this.pivot[0] = transform.Find("Rotation-Handle-X");
        this.pivot[1] = transform.Find("Rotation-Handle-Y");
        this.pivot[2] = transform.Find("Rotation-Handle-Z");

        this.torus[0] = transform.Find("Rotation-Torus-X");
        this.torus[1] = transform.Find("Rotation-Torus-Y");
        this.torus[2] = transform.Find("Rotation-Torus-Z");
    }

    protected override void Start() {
        base.Start();
        this.child = FirstProceduralChild(true);
        Debug.Assert(child != null);
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (bell == null) GetReferences();
        RefreshAugments();
    }

    public override void Randomize() {
        base.Randomize();
        Refresh();
        child.GetComponent<Procedural>().Randomize();
    }

    public override void RandomizeSaved() {
        base.RandomizeSaved();
        RefreshSaved();
        child.GetComponent<Procedural>().RandomizeSaved();
    }

    public override string Description(bool small) {
        if (modifiable) {
            var angle = "--";
            for (int i = 0; i < 3; i++) {
                float A = Vector3.Angle(pivot[i].localPosition, PivotForward(i));
                if (Mathf.Abs(A) >= snapThreshold) {
                    int D = Mathf.RoundToInt(A);
                    angle = string.Format("±{0}º {1}", D, (char)('X' + i));
                    break;
                }
            }
            if (small) {
                return string.Format("{0} ({1})", this.name, angle);
            } else {
                var links = CountLinks(transform);
                return string.Format("{0} ({1}) {2}", this.name, angle, links);
            }
        } else {
            return this.name;
        }
    }

    public string GetDescription(Transform target) {
        if (target == arc[0] || target == torus[0]) return "X Handle";
        if (target == arc[1] || target == torus[1]) return "Y Handle";
        if (target == arc[2] || target == torus[2]) return "Z Handle";
        return null;
    }

    // ***** Augmentation

    float RecomputeBoundingSphereRadius() {
        var size = Math3d.GetWorldBounds(child, true).size;
        Debug.LogFormat("Bounding: {0}", size);
        var S = 1.2f * Vector3.Magnitude(size);
        return Mathf.Max(S, 0.40f);
    }

    static Vector3 PivotForward(int i) {
        Vector3 forward = Vector3.zero;
        forward[(i + 1) % 3] = 1;
        return forward;
    }

    static Vector3 PlaneNormal(int i) {
        Vector3 normal = Vector3.zero;
        normal[i] = 1;
        return normal;
    }

    public override void ShowAugmentation() {
        float S = RecomputeBoundingSphereRadius();

        for (int i = 0; i < 3; i++) {
            float A = Vector3.Angle(pivot[i].localPosition, PivotForward(i));

            var direction = pivot[i].position - transform.position;
            pivot[i].position = transform.position + direction.normalized * S;
            float L = Vector3.Magnitude(pivot[i].localPosition);

            MeshTools.UpdateArc(arc[i], A);
            MeshTools.UpdateTorus(torus[i], 0.010f / S);

            arc[i].localScale = L * Vector3.one;
            torus[i].localScale = L * Vector3.one;

            arc[i].gameObject.SetActive(true);
            pivot[i].gameObject.SetActive(true);
            torus[i].gameObject.SetActive(true);
            Interactive.BeginSelect(pivot[i]);
        }
    }

    public override void HideAugmentation() {
        for (int i = 0; i < 3; i++) {
            arc[i].gameObject.SetActive(false);
            pivot[i].gameObject.SetActive(false);
            torus[i].gameObject.SetActive(false);
            Interactive.EndSelect(pivot[i]);
        }
    }

    public Transform InitializeGrab(Transform target) {
        for (int i = 0; i < 3; i++) {
            if (target == pivot[i] || target == torus[i]) {
                float S = Vector3.Magnitude(pivot[i].position - transform.position);
                float L = Vector3.Magnitude(pivot[i].localPosition);

                var normal = transform.TransformDirection(PlaneNormal(i)).normalized;
                var plane = new Plane(normal, transform.position);

                var cousins = LinkedProcedural.GetLinks(this.transform, true);

                Grabber.instance.EnableUserPositionLocks();
                Grabber.instance.AllowBelowFloor();
                Grabber.instance.IgnoreRotation();
                Grabber.instance.LockOnPlane(plane);
                Grabber.instance.LockOnSphere(transform.position, S);
                Grabber.instance.AddAfterRefreshCallback(this.SnapPivots);

                foreach (Transform link in cousins) {
                    var proc = link.GetComponent<ProceduralRotation>();

                    if (link != this.transform) {
                        Grabber.instance.AddSlave(proc.pivot[i]);
                    }
                    for (int j = 0; j < 3; j++) {
                        if (i != j) {
                            var prev = new TransformRecord(proc.pivot[j]);
                            proc.pivot[j].localPosition = PivotForward(j) * L;
                            var next = new TransformRecord(proc.pivot[j]);
                            UndoHistory.current.AddLazy(new GrabAction(prev, next));
                        }
                    }
                }

                return pivot[i];
            }
        }
        return null;
    }

    void SnapPivots(Transform target) {
        for (int i = 0; i < 3; i++) {
            if (target == pivot[i]) {
                float L = Vector3.Magnitude(pivot[i].localPosition);
                float A = Vector3.Angle(pivot[i].localPosition, PivotForward(i));

                foreach (float offset in new float[] { 0, 90, 180, 270, 360 }) {
                    if (offset - snapThreshold <= A && A <= offset + snapThreshold) {
                        var rotator = Quaternion.AngleAxis(offset, PlaneNormal(i));
                        var a = rotator * (L * PivotForward(i));
                        var b = -a;
                        var c = pivot[i].localPosition;
                        if (Vector3.Distance(a, c) < Vector3.Distance(b, c)) {
                            pivot[i].localPosition = a;
                        } else {
                            pivot[i].localPosition = b;
                        }
                        break;
                    }
                }
            }
        }
    }

    // ***** ProceduralRotation

    void RefreshAugments() {
        for (int i = 0; i < 3; i++) {
            if (torus[i].gameObject.activeSelf) {
                float S = Vector3.Magnitude(pivot[i].position - transform.position);
                float L = Vector3.Magnitude(pivot[i].localPosition);
                float A = Vector3.Angle(pivot[i].localPosition, PivotForward(i));

                MeshTools.UpdateArc(arc[i], A);
                MeshTools.UpdateTorus(torus[i], 0.025f / S);

                arc[i].localScale = L * Vector3.one;
                torus[i].localScale = L * Vector3.one;
            }
        }
    }

    public Vector3 GetEulerInterval() {
        return new Vector3(
             Vector3.Angle(pivot[0].localPosition, PivotForward(0)),
             Vector3.Angle(pivot[1].localPosition, PivotForward(1)),
             Vector3.Angle(pivot[2].localPosition, PivotForward(2))
        );
    }

    void Refresh() {
        var euler = Math3d.ChooseInBox(GetEulerInterval());
        child.transform.localRotation = bell.localRotation * Quaternion.Euler(euler);
    }

    public void RefreshSaved() {
        var start = new TransformRecord(child);
        var euler = Math3d.ChooseInBox(GetEulerInterval());
        child.transform.localRotation = bell.localRotation * Quaternion.Euler(euler);
        var end = new TransformRecord(child);
        UndoHistory.current.AddLazy(new GrabAction(start, end));
    }

}
