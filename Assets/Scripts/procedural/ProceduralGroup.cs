using UnityEngine;

[DisallowMultipleComponent]
public class ProceduralGroup : LinkedProcedural {

    public ProceduralGroup() { proctype = ProceduralType.Group; }

    // protected override void Awake() { base.Awake(); }
    // protected override void Start() { base.Start(); }
    // protected override void Update() { base.Update(); }
    // protected override void LateUpdate() { base.LateUpdate(); }
    // public override void CopyTraits(Procedural other) { base.CopyTraits(other); }

    public override void Randomize() {
        ProceduralChildren().ForEach(c => c.GetComponent<Procedural>().Randomize());
    }

    public override void RandomizeSaved() {
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
        var slaves = LinkedProcedural.GetAssociates(target, false);

        Grabber.instance.EnableUserLocks();
        Grabber.instance.AddSlaves(slaves);
        return target;
    }

    public Transform InitializeCloneGrab(Transform target) {
        var proc = target.GetComponent<Procedural>();
        var parents = LinkedProcedural.GetLinks(this.transform, true);
        var clone = ProceduralFactory.LinkedClone(parents, target, true);
        var slaves = LinkedProcedural.GetAssociates(clone, false);

        Grabber.instance.EnableUserLocks();
        Grabber.instance.AddSlaves(slaves);
        return clone;
    }

}
