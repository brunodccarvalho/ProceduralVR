using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class Procedural : Interactive {

    [Header("Procedural interactivity constraints")]

    [Tooltip("Can this object be grabbed (in global grab or any edit mode)")]
    public bool grabbable = true;

    [Tooltip("Can this object be cloned (and grabbed afterwards)")]
    public bool cloneable = true;

    [Tooltip("Can this object can be grouped (Group,Tiling,...). False => unmodifiable")]
    public bool groupable = true;

    [Tooltip("Can this object be edited, unlinked and disbanded")]
    public bool modifiable = true;

    public ProceduralType proctype = ProceduralType.Prefab;

    protected Procedural() { this.procedural = true; this.proctype = ProceduralType.Prefab; }

    protected virtual void Awake() {
        ProceduralFactory.SetProceduralName(this);
    }

    public virtual void CopyTraits(Procedural other) {
        this.groupable = this.grabbable = this.cloneable = true;
        this.modifiable = other.modifiable;
    }

    public virtual void Randomize() { }

    public virtual void RandomizeSaved() { }

    public virtual void ShowAugmentation() { }

    public virtual void HideAugmentation() { }

    public virtual void GetReferences() { }

    public abstract string Description(bool small = false);

    // ***** Procedural children

    public static Transform FirstProceduralChild(Transform t, bool activeOnly = false) {
        foreach (Transform child in t)
            if (IsProcedural(child, activeOnly)) return child;
        return null;
    }

    public static List<Transform> ProceduralChildren(Transform t, bool activeOnly = false) {
        List<Transform> list = new List<Transform>();
        foreach (Transform child in t)
            if (IsProcedural(child, activeOnly)) list.Add(child);
        return list;
    }

    public static int CountProceduralChildren(Transform t, bool activeOnly = false) {
        return ProceduralChildren(t, activeOnly).Count;
    }

    public static bool HasProceduralChildren(Transform t, bool activeOnly = false) {
        foreach (Transform child in t)
            if (IsProcedural(child, activeOnly)) return true;
        return false;
    }

    public static bool IsTopLevelProcedural(Transform transform, bool activeOnly = false) {
        if (!IsProcedural(transform, activeOnly)) return false;
        var node = transform.parent;
        while (node != null) {
            if (IsProcedural(node, activeOnly)) return false;
            node = node.parent;
        }
        return true;
    }

    public static bool AllProceduralChildrenAreActive(Transform t) {
        foreach (Transform child in t)
            if (IsProcedural(child) && !child.gameObject.activeSelf) return false;
        return true;
    }

    public void CenterOnProceduralChildren(bool activeOnly = false) {
        int procedurals = 0;
        var centroid = Vector3.zero;
        var all = new Dictionary<Transform, int>();
        foreach (Transform child in transform) {
            if (IsProcedural(child, activeOnly)) {
                procedurals++;
                centroid += child.position;
            }
            all.Add(child, child.GetSiblingIndex());
        }
        if (procedurals > 0) {
            centroid /= procedurals;
            transform.DetachChildren();
            transform.position = centroid;
            foreach (Transform child in all.Keys) {
                child.parent = transform;
            }
            foreach (Transform child in all.Keys) {
                child.SetSiblingIndex(all[child]);
            }
        }
    }

    public Transform FirstProceduralChild(bool activeOnly = false) {
        return FirstProceduralChild(this.transform, activeOnly);
    }
    public List<Transform> ProceduralChildren(bool activeOnly = false) {
        return ProceduralChildren(this.transform, activeOnly);
    }

    // ***** Properties

    public static bool Excluded(Transform t) {
        Transform node = t, top = null;
        while (node != null) {
            if (IsProcedural(node)) top = node;
            node = node.parent;
        }
        return !IsProcedural(top, true);
    }

    public static bool IsProcedural(Transform transform, bool activeOnly = false) {
        return transform?.GetComponent<Procedural>() != null
            && (!activeOnly || transform.gameObject.activeSelf);
    }

}
