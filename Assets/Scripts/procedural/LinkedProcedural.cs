using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class LinkedProcedural : Procedural {

    static Dictionary<Transform, DelegateLinkedSet<Transform>> linkMap;

    DelegateLinkedSet<Transform> rep;

    static LinkedProcedural() {
        linkMap = new Dictionary<Transform, DelegateLinkedSet<Transform>>();
    }

    protected override void Awake() {
        base.Awake();
        if (rep == null) {
            rep = new DelegateLinkedSet<Transform> { this.transform };
            linkMap.Add(this.transform, rep);
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        rep.Remove(this.transform);
        linkMap.Remove(this.transform);
    }

    protected string LinksDescription() {
        var count = CountLinks(this.transform);
        if (count == 1) {
            return "[0 links]";
        } else if (count == 2) {
            return "[1 link]";
        } else {
            return string.Format("[{0} links]", count - 1);
        }
    }

    // ***** Linking, unlinking and relinking

    public bool Unlinkable() {
        return AnyCousinLink() != null;
    }

    public Transform AnyCousinLink() {
        if (rep == null || rep.Count <= 1) return null;
        foreach (Transform cousin in rep)
            if (cousin != this.transform && !Excluded(cousin)) return cousin;
        return null;
    }

    public Transform Unlink() {
        if (rep.Count > 1) {
            var cousin = AnyCousinLink();
            rep.Remove(this.transform);
            rep = new DelegateLinkedSet<Transform> { this.transform };
            linkMap[this.transform] = rep;
            return cousin;
        }
        return null;
    }

    public void Relink(Transform other) {
        if (other != null) {
            rep = linkMap[other];
            rep.Add(this.transform);
            linkMap[this.transform] = rep;
        }
    }

    public static bool Unlinkable(Transform t) {
        return IsLinkedProcedural(t) && t.GetComponent<LinkedProcedural>().Unlinkable();
    }
    public static Transform AnyCousinLink(Transform t) {
        return t.GetComponent<LinkedProcedural>()?.AnyCousinLink();
    }
    public static Transform Unlink(Transform t) {
        return t.GetComponent<LinkedProcedural>()?.Unlink();
    }
    public static void Relink(Transform t, Transform other) {
        t.GetComponent<LinkedProcedural>()?.Relink(other);
    }

    // ***** Linking for ProceduralFactory

    /**
     * Relink this procedural to original and recursively link all the children that
     * are also LinkedProcedurals. Called in ProceduralFactory after a clone.
     */
    public static void RelinkRecursively(Transform clone, Transform original) {
        var news = Procedural.ProceduralChildren(clone);
        var olds = Procedural.ProceduralChildren(original);
        if (olds.Count != news.Count) {
            // ! OOPS
            Debug.LogErrorFormat("Different children count {0} {1}", clone.name, original.name);
            return;
        }
        for (int i = 0, N = news.Count; i < N; i++) {
            Transform a = news[i], b = olds[i];
            if (IsLinkedProcedural(a) != IsLinkedProcedural(b)) {
                // ! OOPS
                Debug.LogErrorFormat("Different IsLinked {0} {1}", a.name, b.name);
                return;
            }
            if (IsLinkedProcedural(a)) {
                RelinkRecursively(a, b);
            }
        }
        Relink(clone, original);
    }

    // ***** Properties

    // Suppose we're editing a group a and want to clone and add object b
    // We're not allowed to do so if we would cause a loop in the procedural hierarchy
    // A loop would look like root -> ... -> a -> b -> ... -> ancestor of a -> ... -> a
    // This means a cannot exist within the subtree of b
    public static bool Bound(Transform a, Transform b, out Transform blocker) {
        var ap = a.GetComponent<LinkedProcedural>();
        var bp = b.GetComponent<LinkedProcedural>();
        blocker = null;
        return ap && bp && BoundDfs(ap, b, out blocker);
    }

    static bool BoundDfs(LinkedProcedural a, Transform b, out Transform blocker) {
        if (a.rep.Contains(b)) { blocker = b; return true; }
        blocker = null;
        foreach (Transform child in b) if (BoundDfs(a, child, out blocker)) return true;
        return false;
    }

    public static bool IsLinkedProcedural(Transform transform, bool activeOnly = false) {
        return transform?.GetComponent<LinkedProcedural>() != null
            && (!activeOnly || transform.gameObject.activeSelf);
    }

    public static bool AreLinked(Transform a, Transform b) {
        var ap = a.GetComponent<LinkedProcedural>();
        var bp = b.GetComponent<LinkedProcedural>();
        return ap && bp && ap.rep != null && ap.rep.Contains(b);
    }

    // ***** Get links

    /**
     * Return a LinkedProcedural that is linked to all the given transforms and is not
     * any of the transforms themselves.
     * Returns null if no such cousin exists or the objects are not LinkedProcedurals.
     * Either none of the objects are LinkedProcedurals, or they are all linked.
     */
    public static Transform AnyCousinLink(List<Transform> transforms) {
        var first = transforms[0].GetComponent<LinkedProcedural>();
        if (first == null) {
            foreach (Transform link in transforms) {
                Debug.Assert(!IsLinkedProcedural(link));
            }
            return null;
        } else {
            var set = new HashSet<Transform>(transforms);
            var rep = first.rep;
            foreach (Transform link in transforms) {
                Debug.Assert(!Excluded(link) && IsLinkedProcedural(link) && rep.Contains(link));
            }
            foreach (Transform cousin in rep) {
                if (!set.Contains(cousin) && !Excluded(cousin)) return cousin;
            }
            return null;
        }
    }

    /**
     * Return all objects linked to transform.
     * If transform is not a LinkedProcedural it returns transform itself only.
     * The first link is the transform itself if requested.
     */
    public static List<Transform> GetLinks(Transform transform, bool includeSelf = true) {
        var list = new List<Transform>();
        if (includeSelf) list.Add(transform);

        var rep = transform.GetComponent<LinkedProcedural>()?.rep;
        if (rep == null) return list;

        foreach (Transform cousin in rep)
            if (cousin != transform && !Excluded(cousin)) list.Add(cousin);
        return list;
    }

    /**
     * Return all procedurals at the same position in the hierarchy as this object
     * The first associate is the transform itself if requested.
     */
    public static List<Transform> GetAssociates(Transform transform, bool includeSelf = true) {
        var list = new List<Transform>();
        if (includeSelf) list.Add(transform);

        var procParent = transform.parent?.GetComponent<LinkedProcedural>();
        var rep = procParent?.rep;
        if (rep == null) return list;

        int index = procParent.ProceduralChildren().IndexOf(transform);
        if (index == -1) {
            // ! OOPS
            Debug.LogErrorFormat("Did not find self in procedural list", transform.name);
            return list;
        }
        foreach (Transform link in GetLinks(transform.parent, false)) {
            var children = Procedural.ProceduralChildren(link);
            Debug.Assert(index < children.Count && children[index] != transform);
            list.Add(children[index]);
        }
        return list;
    }

    public static int CountLinks(Transform transform) {
        return GetLinks(transform).Count;
    }

    public static int CountAssociates(Transform transform) {
        return GetAssociates(transform).Count;
    }

}
