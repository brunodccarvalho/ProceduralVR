using UnityEngine;

[DisallowMultipleComponent]
public class ProceduralRandom : LinkedProcedural {

    public Transform activeChild;
    public Procedural activeProc => activeChild.GetComponent<Procedural>();

    ProceduralRandom() { proctype = ProceduralType.Random; }

    // protected override void Awake() { base.Awake(); }
    // protected override void Update() { base.Update(); }
    // protected override void LateUpdate() { base.LateUpdate(); }
    // public override void CopyTraits(Procedural other) { }

    protected override void Start() {
        base.Start();
        this.activeChild = FirstProceduralChild(true);
        Debug.Assert(activeChild != null);
    }

    public override void Randomize() {
        base.Randomize();
        Repick();
        ProceduralChildren().ForEach(c => c.GetComponent<Procedural>().Randomize());
    }

    public override void RandomizeSaved() {
        base.RandomizeSaved();
        RepickSaved();
        ProceduralChildren().ForEach(c => c.GetComponent<Procedural>().RandomizeSaved());
    }

    public override string Description(bool small) {
        if (modifiable) {
            var children = CountProceduralChildren(transform);
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

    // ***** Editing / Augmentation

    public override void ShowAugmentation() {
        AugmentController.instance.AddAxis(this.transform);
    }

    public override void HideAugmentation() {
        AugmentController.instance.RemoveAxis(this.transform);
    }

    public Transform InitializeMoveGrab(Transform target) {
        Debug.Assert(target == activeChild);
        var slaves = LinkedProcedural.GetAssociates(target, false);

        Grabber.instance.EnableUserLocks();
        Grabber.instance.AddSlaves(slaves);
        return target;
    }

    public Transform InitializeCloneGrab(Transform target) {
        var parents = LinkedProcedural.GetLinks(this.transform, true);
        var clone = ProceduralFactory.LinkedClone(parents, target, false);
        var slaves = LinkedProcedural.GetAssociates(clone, false);
        Debug.Assert(clone.parent == this.transform);
        UndoHistory.current.AddLazy(new RandomizeAction(this, activeChild, clone));
        activeChild?.gameObject.SetActive(false);
        clone.gameObject.SetActive(true);

        Grabber.instance.EnableUserLocks();
        Grabber.instance.AddSlaves(slaves);
        return clone;
    }

    // ***** ProceduralRandom

    void Repick() {
        var kids = ProceduralChildren();
        Transform next = kids[Random.Range(0, kids.Count)];
        if (activeChild != next) {
            activeChild?.gameObject.SetActive(false);
            next?.gameObject.SetActive(true);
            activeChild = next;
        }
    }

    public void RepickSaved() {
        var kids = ProceduralChildren();
        Transform next = kids[Random.Range(0, kids.Count)];
        if (activeChild != next) {
            UndoHistory.current.AddLazy(new RandomizeAction(this, activeChild, next));
        }
    }

    public void CycleSaved() {
        var list = ProceduralChildren();
        var i = list.IndexOf(activeChild);
        var next = list[(i + 1) % list.Count];
        if (activeChild != next) {
            UndoHistory.current.AddLazy(new RandomizeAction(this, activeChild, next));
        }
    }

}
