#if false

using System.Collections.Generic;
using UnityEngine;

public class ProceduralGrabber : MonoBehaviour {

    public static ProceduralGrabber instance;
    public Transform leftGrabber, rightGrabber;

    DelegateLinkedSet<Transform> grabSlaves;
    Dictionary<Transform, TransformRecord> slaveRecords;
    ParentRecord startRecord;
    Transform grabTarget;
    GrabSource grabSource;
    public bool isGrabbing => grabSource != GrabSource.None;

    void Awake() {
        Debug.Assert(instance == null);
        instance = this;

        grabSlaves = new DelegateLinkedSet<Transform>();
        slaveRecords = new Dictionary<Transform, TransformRecord>();
        grabTarget = null;
        grabSource = GrabSource.None;

        grabSlaves.OnInsert.AddListener(Interactive.BeginSlaveGrab);
        grabSlaves.OnRemove.AddListener(Interactive.EndSlaveGrab);
    }

    void Update() {
        if (isGrabbing) {
            var grabber = grabTarget.parent;
            startRecord.Apply();
            var current = new LocalDeltaRecord(grabTarget);
            grabTarget.parent = grabber;

            foreach (Transform slave in grabSlaves) {
                if (slave != grabTarget) {
                    current.Apply(slave);
                }
            }
        }
    }

    public void StartGrab(Transform target, GrabSource source) {
        Debug.Assert(!isGrabbing, "Attempt to overload grab while already grabbing");

        var grabber = GetGrabberFor(source);
        var links = LinkedProcedural.GetCousinLinks(target);
        Debug.LogFormat("Grab {0}, cousins: {1}", target, string.Join(",", links.ConvertAll(l => l.name)));

        grabTarget = target;
        grabTarget.GetComponent<Interactive>().Grab(true);
        startRecord = new ParentRecord(target);

        foreach (Transform slave in links) { // includes the primary target
            slaveRecords.Add(slave, new TransformRecord(slave));
            grabSlaves.AddLast(slave);
        }

        target.parent = grabber;
        grabSource = source;
    }

    public void ReleaseGrab() {
        Debug.Assert(isGrabbing, "Attempt to end grab while not grabbing");

        startRecord.Apply();
        var current = new LocalDeltaRecord(grabTarget);

        foreach (Transform slave in grabSlaves) {
            current.Apply(slave);
            var startSlave = slaveRecords[slave];
            var endSlave = new TransformRecord(slave);
            var action = new GrabAction(startSlave, endSlave);
            UndoHistory.AddLazy(action);
        }

        grabSlaves.Clear();
        slaveRecords.Clear();
        grabTarget.GetComponent<Interactive>().Grab(false);
        grabTarget = null;
        grabSource = GrabSource.None;
    }

    public void CancelGrab() {
        Debug.Assert(isGrabbing, "Attempt to cancel grab while not grabbing");

        startRecord.Apply();

        foreach (Transform slave in grabSlaves) {
            slaveRecords[slave].Apply();
        }

        grabSlaves.Clear();
        slaveRecords.Clear();
        grabTarget.GetComponent<Interactive>().Grab(false);
        grabTarget = null;
        grabSource = GrabSource.None;
    }

    public GrabSource GetGrabSource() {
        return grabSource;
    }

    public Transform GetTarget() {
        return grabTarget;
    }

    Transform GetGrabberFor(GrabSource source) {
        if (source == GrabSource.LeftHand || source == GrabSource.LeftLaser) {
            return leftGrabber;
        } else if (source == GrabSource.RightHand || source == GrabSource.RightLaser) {
            return rightGrabber;
        } else {
            Debug.LogAssertion("NULL source in GetGrabberFor");
            return null;
        }
    }

}

#endif
