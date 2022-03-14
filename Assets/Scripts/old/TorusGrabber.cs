#if false

using System.Collections.Generic;
using UnityEngine;

public class TorusGrabber : MonoBehaviour {

    public static TorusGrabber instance;
    public Laser leftLaser, rightLaser;

    TransformRecord startRecord;
    Transform grabTarget;
    Transform grabTorus;
    GrabSource grabSource;
    public Vector3 initialHit;
    public Vector3 latestHit;
    public Quaternion latestRotation;
    Vector3 normal;
    Plane plane;
    public bool isGrabbing => grabSource != GrabSource.None;

    void Awake() {
        Debug.Assert(instance == null);
        instance = this;

        grabSource = GrabSource.None;
    }

    public Vector3 GetDelta() {
        var dim = AugmentController.instance.GetTorusAxis(grabTorus);
        var euler = latestRotation.eulerAngles;
        Vector3 delta = Vector3.zero;
        delta[dim] = euler[dim];
        return delta;
    }

    void Update() {
        if (isGrabbing) {
            Vector3 hit;
            if (GetLaserFor(grabSource).RayPlane(plane, out hit)) {
                var initial = initialHit - grabTarget.position; // constant
                var current = hit - grabTarget.position;
                var angle = Vector3.SignedAngle(initial, current, normal);
                var rotation = Quaternion.AngleAxis(angle, normal);
                startRecord.Apply();
                grabTarget.rotation *= rotation;
                grabTorus.rotation = rotation;

                // isto nao interessa até eu largar:
                latestHit = hit;
                latestRotation = rotation;
                // latestAngle = angle;
            }
        }
    }

    void OnDrawGizmos() {
        Gizmos.DrawSphere(latestHit, 0.3f);
    }

    public void StartGrab(Transform target, Transform torus, GrabSource source) {
        Debug.Assert(!isGrabbing, "Attempt to overload grab while already grabbing");

        grabTarget = target;
        grabTorus = torus;
        startRecord = new TransformRecord(target);
        grabTarget.GetComponent<Interactive>().Grab(true);

        int dim = AugmentController.instance.GetTorusAxis(grabTorus);
        if (dim == 0) {
            normal = grabTarget.right;
        } else if (dim == 1) {
            normal = grabTarget.up;
        } else if (dim == 2) {
            normal = grabTarget.forward;
        }
        plane = new Plane(normal, grabTarget.position);

        var ok = GetLaserFor(source).RayPlane(plane, out initialHit);
        latestHit = initialHit;
        Debug.Assert(ok, "Initial raycast did not hit the plane");

        grabSource = source;
    }

    public Quaternion ReleaseGrab() {
        Debug.Assert(isGrabbing, "Attempt to end grab while not grabbing");

        startRecord.Apply();

        grabTarget.GetComponent<Interactive>().Grab(false);
        grabSource = GrabSource.None;

        return Quaternion.FromToRotation(initialHit, latestHit);
    }

    public void CancelGrab() {
        Debug.Assert(isGrabbing, "Attempt to cancel grab while not grabbing");

        startRecord.Apply();

        grabTarget.GetComponent<Interactive>().Grab(false);
        grabSource = GrabSource.None;
    }

    public GrabSource GetGrabSource() {
        return grabSource;
    }

    public Transform GetTarget() {
        return grabTarget;
    }

    public Transform GetTorus() {
        return grabTorus;
    }

    Laser GetLaserFor(GrabSource source) {
        if (source == GrabSource.LeftLaser) {
            return leftLaser;
        } else if (source == GrabSource.RightLaser) {
            return rightLaser;
        } else {
            Debug.LogAssertion("NULL/Hand source in GetLaserFor");
            return null;
        }
    }

}

#endif
