using System.Collections.Generic;
using UnityEngine;

// Undo not working adequately because of currentOffset

[DisallowMultipleComponent]
public class ProceduralTiling : LinkedProcedural {

    public const float threshold = 0.10f; // world-space units

    public Transform template, handle, bell, terminal;
    public Transform[] augments => new Transform[] { handle, bell };
    public LineRenderer line;
    private Vector3 currentOffset = Vector3.zero;

    ProceduralTiling() { proctype = ProceduralType.Tiling; }

    public override void GetReferences() {
        this.handle = transform.Find("Tiling-Handle");
        this.bell = transform.Find("Tiling-HiddenBell");
        this.terminal = transform.Find("Tiling-Terminal");
        this.line = transform.GetComponent<LineRenderer>();
        this.currentOffset = terminal.localPosition - handle.localPosition;
        Debug.Assert(handle != null && handle != null && bell != null);
    }

    protected override void Start() {
        base.Start();
        template = FirstProceduralChild(true);
        GetReferences();
        bell.localRotation = template.localRotation;
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (handle == null) GetReferences();
        if (handle.hasChanged || bell.hasChanged) {
            RefreshChildren();
            handle.hasChanged = false;
            bell.hasChanged = false;
        }
        if (bell.gameObject.activeSelf && !Grabber.instance.IsGrabbing) {
            RefreshAugments();
        }
        if (bell.gameObject.activeSelf) {
            RefreshLineRenderer();
        }
    }

    public override void Randomize() {
        base.Randomize();
        foreach (Transform child in ProceduralChildren(true)) {
            child.GetComponent<Procedural>().Randomize();
        }
    }

    public override void RandomizeSaved() {
        base.RandomizeSaved();
        foreach (Transform child in ProceduralChildren(true)) {
            child.GetComponent<Procedural>().RandomizeSaved();
        }
    }

    public override void CopyTraits(Procedural other) {
        base.CopyTraits(other);
    }

    public override string Description(bool small) {
        if (modifiable) {
            var children = CountProceduralChildren(transform, true);
            if (small) {
                return string.Format("{0} (#{1})", this.name, children);
            } else {
                var links = LinksDescription();
                return string.Format("{0} (#{1}) {2}", this.name, children, links);
            }
        } else {
            return this.name;
        }
    }

    public string GetDescription(Transform target) {
        if (target == handle) return "Handle";
        if (target == bell) return "Rotator";
        return null;
    }

    // ***** Augmentation

    void SetHandleOffset(Vector3 offset) {
        currentOffset = offset;
        handle.localPosition = terminal.localPosition + currentOffset;
    }

    void RefreshAugments() {
        var transformSave = new TransformRecord(this.transform);
        this.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        // Find the bounding box as-is in world space
        Vector3 size = Vector3.zero;
        foreach (Transform child in ProceduralChildren(this.transform, true)) {
            var childsize = Math3d.GetWorldBounds(child, true, true).size;
            size = Vector3.Max(childsize, size);
        }

        float M = Vector3.Magnitude(transform.InverseTransformVector(Vector3.up));

        float S = M * (1.2f * AugmentController.bellHeight + size.y);
        bell.localPosition = S * Vector3.up;

        float L = M * (1.2f * AugmentController.handleRadius + size.x);
        var snapScale = Grabber.instance.userLocks.gridSnapScale;
        var offset = Math3d.CeilSnap(L * Vector3.right, snapScale);

        transformSave.Apply();
        SetHandleOffset(offset);
    }

    void RefreshLineRenderer() {
        line?.SetPosition(0, terminal.localPosition);
        line?.SetPosition(1, -terminal.localPosition);
    }

    void RefreshTerminal() {
        terminal.localPosition = handle.localPosition - currentOffset;
    }

    public override void ShowAugmentation() {
        RefreshAugments(); // Before handles
        AugmentController.instance.AddAxis(this.transform);
        handle.gameObject.SetActive(true);
        bell.gameObject.SetActive(true);
        line.enabled = true;
        Interactive.BeginSelect(handle);
        Interactive.BeginSelect(bell);
    }

    public override void HideAugmentation() {
        AugmentController.instance.RemoveAxis(this.transform);
        handle.gameObject.SetActive(false);
        bell.gameObject.SetActive(false);
        line.enabled = false;
        Interactive.EndSelect(handle);
        Interactive.EndSelect(bell);
    }

    public Transform InitializeGrab(Transform target) {
        if (target == handle) {
            Grabber.instance.EnableUserPositionLocks();
            Grabber.instance.AllowBelowFloor();
            Grabber.instance.IgnoreRotation();
            Grabber.instance.LockOnLine(transform.position, transform.TransformDirection(Vector3.right));
            Grabber.instance.DontSaveTarget();

            var cousins = LinkedProcedural.GetLinks(this.transform, true);
            foreach (Transform cousin in cousins) {
                var proc = cousin.GetComponent<ProceduralTiling>();
                proc.SetHandleOffset(currentOffset);
                Grabber.instance.AddCopycat(proc.handle);
                Grabber.instance.AddSaveTarget(proc.terminal);
                Grabber.instance.AddAfterRefreshCallback(proc.RefreshTerminal);
            }

            return target;
        } else if (target == bell) {
            Grabber.instance.EnableUserRotationLocks();
            Grabber.instance.AllowBelowFloor();
            Grabber.instance.LockOnPoint(bell.position);

            var cousins = LinkedProcedural.GetLinks(this.transform, false);
            foreach (Transform cousin in cousins) {
                var proc = cousin.GetComponent<ProceduralTiling>();
                Grabber.instance.AddSlave(proc.bell);
            }

            return target;
        }
        return null;
    }

    // ***** Refreshing (live)

    public List<Transform> MakeActiveCount(int count) {
        var children = ProceduralChildren(false);
        int C = children.Count;
        for (int i = C; i < count; i++) {
            var child = ProceduralFactory.ClonePlain(template);
            child.parent = this.transform;
            children.Add(child);
        }
        for (int i = 0; i < count; i++) {
            children[i].gameObject.SetActive(true);
        }
        for (int i = count; i < C; i++) {
            Object.Destroy(children[i].gameObject);
        }
        return children.GetRange(0, count);
    }

    void RefreshChildren() {
        var children = ProceduralChildren(transform, true);
        Vector3 a = -terminal.localPosition;
        Vector3 b = terminal.localPosition;
        int count = children.Count;
        for (int i = 0; i < count; i++) {
            var t = (2.0f * i + 1) / (2.0f * count);
            var pos = Vector3.Lerp(a, b, t);
            children[i].localPosition = pos;
            children[i].localRotation = bell.localRotation;
        }
    }

    // ? Add a child

    void AddChild(int count) {
        MakeActiveCount(count);
        UndoHistory.current.AddLazy(new TilingRecountAction(this, count - 1, count));
        RefreshChildren();
    }

    public void AddChildAllLinks() {
        int count = CountProceduralChildren(transform, true) + 1;
        foreach (Transform link in GetLinks(this.transform, true)) {
            var tiling = link.GetComponent<ProceduralTiling>();
            tiling.AddChild(count);
        }
        RefreshAugments();
    }

    // ? Remove a child

    void RemoveChild(int count) {
        Debug.Assert(count >= 1);
        MakeActiveCount(count);
        UndoHistory.current.AddLazy(new TilingRecountAction(this, count + 1, count));
        RefreshChildren();
    }

    public void RemoveChildAllLinks() {
        int count = CountProceduralChildren(transform, true) - 1;
        foreach (Transform link in GetLinks(this.transform, true)) {
            var tiling = link.GetComponent<ProceduralTiling>();
            tiling.RemoveChild(count);
        }
        RefreshAugments();
    }

}
