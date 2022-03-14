#if false
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class TrackingPilars : MonoBehaviour {

    public const float threshold = ProceduralTiling.threshold;

    public Transform root;
    public Transform center;

    public Transform bell;
    public Transform[] pivot;

    public Vector3[] currentPilar = new Vector3[2];
    public Vector3[] initialPilar;

    public ProceduralTiling procedural => center.GetComponent<ProceduralTiling>();
    public Quaternion procBell => procedural.template.localRotation;
    public TilingStrategy procStrategy => procedural.strategy;
    public Vector3[] procPilar => procedural.pilar;

    public static Transform Add(Transform center, GameObject prefab) {
        var root = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        var tracker = root.AddComponent<TrackingPilars>();
        root.transform.SetPositionAndRotation(center.position, center.rotation);

        tracker.root = root.transform;
        tracker.center = center;
        tracker.Setup();
        return root.transform;
    }

    public void Setup() {
        bell = root.transform.Find("Bell");

        pivot = new Transform[2];
        pivot[0] = root.transform.Find("Pivot-X");
        pivot[1] = root.transform.Find("Pivot+X");

        AdoptSpecs();
    }

    public void AdoptSpecs() {
        currentPilar = (Vector3[])procPilar.Clone();
        pivot[0].localPosition = procPilar[0];
        pivot[1].localPosition = procPilar[1];
        bell.localRotation = procBell;
    }

    public void SetPivotPositions(Vector3[] ab) {
        currentPilar = (Vector3[])ab.Clone();
        pivot[0].localPosition = ab[0];
        pivot[1].localPosition = ab[1];
    }

    public void SetBellRotation(Quaternion local) {
        bell.rotation = local;
    }

    public Transform GetTargetedPilar(Transform transform) {
        if (transform == pivot[0]) {
            return pivot[0];
        } else if (transform == pivot[1]) {
            return pivot[1];
        } else {
            return null;
        }
    }

    public Transform GetTargetedBell(Transform transform) {
        if (transform == bell) {
            return bell;
        } else {
            return null;
        }
    }

    void Update() {
        Refresh();
        procedural.SetPivotsLive(currentPilar);
    }

    void Refresh() {
        var a = pivot[0].localPosition;
        var b = pivot[1].localPosition;
        a = ProceduralTiling.Normalize(a);
        b = ProceduralTiling.Normalize(b);
        pivot[0].localPosition = a;
        pivot[1].localPosition = b;
        currentPilar = new Vector3[2] { a, b };
    }

    public void ApplyPilarGrabLocks(Transform pilar) {
        initialPilar = currentPilar;
        procedural.GetTilingStateAllLinks();
        Grabber.instance.EnableUserLocks();
        Grabber.instance.AllowBelowFloor();
        Grabber.instance.IgnoreRotation();
    }

    public void ApplyBellGrabLocks(Transform bell) {
        initialPilar = currentPilar;
        procedural.GetTilingStateAllLinks();
        Grabber.instance.DisableUserLocks();
        Grabber.instance.AllowBelowFloor();
        Grabber.instance.LockOnPoint(bell.position);
        Grabber.instance.AddSlaves(procedural.GetChildrenLinks(true));
    }

    public void Release(Transform target) {
        if (target == pivot[0] || target == pivot[1]) {
            procedural.SetPivotsAllLinks(currentPilar, initialPilar);
        } else if (target == bell) {
            Grabber.instance.SaveSlaveGrabActions();
        }
    }

    public void Cancel(Transform target) {
        if (target == pivot[0] || target == pivot[1]) {
            procedural.SetPivotsAllLinks(initialPilar);
        }
    }

}
#endif
