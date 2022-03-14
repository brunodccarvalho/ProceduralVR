using UnityEngine;

[DisallowMultipleComponent]
public class ProceduralPosition : LinkedProcedural {

    public Transform child;
    public Transform handle, box;

    ProceduralPosition() { proctype = ProceduralType.Position; }

    public override void GetReferences() {
        this.handle = transform.Find("Position-Handle");
        this.box = transform.Find("Position-HiddenCube");
    }

    protected override void Start() {
        base.Start();
        this.child = FirstProceduralChild(true);
        Debug.Assert(child != null);
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (handle == null) GetReferences();
        LiveRefresh();
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
        var angle = Math3d.Abs(handle.transform.localPosition);
        if (small) {
            return string.Format("{0} ({1})", this.name, angle);
        } else {
            var links = LinksDescription();
            return string.Format("{0} ({1}) {2}", this.name, angle, links);
        }
    }

    public string GetDescription(Transform target) {
        if (target == handle) return "Box Handle";
        return null;
    }

    // ***** Augmentation

    void PositionAxisSnap() {
        return;
        var v = handle.localPosition;
        handle.localPosition = Grabber.instance.userLocks.PositionSnap(v);
    }

    public override void ShowAugmentation() {
        AugmentController.instance.AddAxis(this.transform);
        handle.gameObject.SetActive(true);
        box.gameObject.SetActive(true);
        Interactive.BeginSelect(handle);
    }

    public override void HideAugmentation() {
        AugmentController.instance.RemoveAxis(this.transform);
        handle.gameObject.SetActive(false);
        box.gameObject.SetActive(false);
        Interactive.EndSelect(handle);
    }

    public Transform InitializeGrab(Transform target) {
        if (target == handle || target == child) {
            Grabber.instance.EnableUserPositionLocks();
            Grabber.instance.AllowBelowFloor();
            Grabber.instance.AddAfterRefreshCallback(PositionAxisSnap);
            Grabber.instance.IgnoreRotation();

            var cousins = LinkedProcedural.GetLinks(this.transform, false);
            foreach (Transform cousin in cousins) {
                var proc = cousin.GetComponent<ProceduralPosition>();
                Grabber.instance.AddSlave(proc.handle);
            }

            return handle;
        }
        return null;
    }

    void LiveRefresh() {
        var size = Math3d.Abs(handle.localPosition);
        box.localScale = 2 * size;
    }

    // ***** ProceduralPosition

    void Refresh() {
        child.transform.localPosition = Math3d.ChooseInBox(handle.localPosition);
    }

    public void RefreshSaved() {
        var start = new TransformRecord(child);
        child.transform.localPosition = Math3d.ChooseInBox(handle.localPosition);
        var end = new TransformRecord(child);
        UndoHistory.current.AddLazy(new GrabAction(start, end));
    }

}
