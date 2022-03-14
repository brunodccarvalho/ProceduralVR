using System;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum GrabSource {
    Hand = 1 << 0,
    Laser = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3,

    None = 0,

    LeftHand = Left | Hand,
    RightHand = Right | Hand,
    LeftLaser = Left | Laser,
    RightLaser = Right | Laser,
}

[DisallowMultipleComponent]
public class Grabber : MonoBehaviour {

    public static Grabber instance;

    [Tooltip("True for right-handed people (menu on the left, laser on the right)")]
    public Transform leftGrabber, rightGrabber;

    public delegate void OnGrabEvent();

    // ***** Specifications

    // [PROGRAM] Call before/after Refresh()
    public delegate void OnRefreshCallback(Transform t);
    public delegate void OnRefreshCallbackClean();
    OnRefreshCallback beforeRefresh, afterRefresh;
    OnRefreshCallback onReleaseRun, onCancelRun;

    // [PROGRAM] The subject is locked onto a fixed sphere in world space.
    bool sphereLocked = false;
    Vector3 sphereCenter;
    float sphereRadius;

    // [PROGRAM] The subject is locked onto a fixed plane in world space.
    bool planeLocked = false;
    Plane planeLock;

    // [PROGRAM] The subject is locked onto a fixed line in world space.
    bool lineLocked = false;
    Ray lineLock;

    // [PROGRAM] The subject is locked above the floor plane in world space.
    bool floorLocked = true;

    // [PROGRAM] The subject's rotation is unmodified and ignored.
    bool rotationIgnored = false;

    // [PROGRAM] Lock the subject on a local space vertical line.
    bool verticalLocked = false;

    // [PROGRAM] Should the current grab respect user locks or ignore them?
    bool applyUserLocks = true;
    bool applyUserRotationLocks = true;

    [Header("User grab constraints")]
    public UserGrabLocks userLocks;

    // ***** Grab subjects, slaves and initial positions, vertical and plane

    // Primary info
    Transform target, hand, phantom;
    GrabSource source = GrabSource.None;
    bool isGrabbing = false;

    // Slave info
    DelegateLinkedSet<Transform> copycats, saves;
    Dictionary<Transform, TransformRecord> startCopycats, startSaves;
    bool saveTarget = true;

    public GrabSource grabSource => this.source;
    public bool IsGrabbing => this.isGrabbing;
    public Transform grabTarget => this.target;

    // Grab data
    TransformRecord startLocal;
    DeltaRecord start;
    Ray localVertical; // vertical line at the start of the grab
    Plane localHorizontal;

    // ***** Adjust user and program specifications

    public void EnableUserLocks() {
        applyUserLocks = true; applyUserRotationLocks = true;
    }
    public void DisableUserLocks() {
        applyUserLocks = true; applyUserRotationLocks = false;
    }
    public void EnableUserRotationLocks() {
        applyUserLocks = false; applyUserRotationLocks = true;
    }
    public void EnableUserPositionLocks() {
        applyUserLocks = true; applyUserRotationLocks = false;
    }

    public void ClearSpecs() {
        applyUserLocks = applyUserRotationLocks = true;
        sphereLocked = false;
        planeLocked = false;
        lineLocked = false;
        floorLocked = true;
        rotationIgnored = false;
        verticalLocked = false;
        beforeRefresh = null;
        afterRefresh = null;
        onReleaseRun = null;
        onCancelRun = null;
        saveTarget = true;
        copycats.Clear();
        saves.Clear();
        startCopycats.Clear();
        startSaves.Clear();
    }

    public void AddBeforeRefreshCallback(OnRefreshCallback callback) {
        beforeRefresh += callback;
    }
    public void AddBeforeRefreshCallback(OnRefreshCallbackClean callback) {
        beforeRefresh += target => callback();
    }
    public void AddAfterRefreshCallback(OnRefreshCallback callback) {
        afterRefresh += callback;
    }
    public void AddAfterRefreshCallback(OnRefreshCallbackClean callback) {
        afterRefresh += target => callback();
    }
    public void CallOnRelease(OnRefreshCallback callback) {
        onReleaseRun += callback;
    }
    public void CallOnRelease(OnRefreshCallbackClean callback) {
        onReleaseRun += target => callback();
    }
    public void CallOnCancel(OnRefreshCallback callback) {
        onCancelRun += callback;
    }
    public void CallOnCancel(OnRefreshCallbackClean callback) {
        onCancelRun += dummy => callback();
    }
    public void CallOnFinish(OnRefreshCallback callback) {
        onReleaseRun += callback; onCancelRun += callback;
    }
    public void CallOnFinish(OnRefreshCallbackClean callback) {
        onReleaseRun += dummy => callback(); onCancelRun += dummy => callback();
    }

    public void AllowBelowFloor() {
        this.floorLocked = false;
    }

    public void LockOnSphere(Vector3 center, float radius) {
        this.sphereLocked = true;
        this.sphereCenter = center;
        this.sphereRadius = radius;
    }

    public void LockOnPoint(Vector3 point) {
        this.sphereLocked = true;
        this.sphereCenter = point;
        this.sphereRadius = 0.0f;
    }

    public void LockOnPlane(Plane plane) {
        this.planeLocked = true;
        this.planeLock = plane;
    }

    public void LockOnLine(Vector3 origin, Vector3 direction) {
        Debug.Assert(direction != Vector3.zero);
        this.lineLocked = true;
        this.lineLock = new Ray(origin, direction);
    }

    public void LockOnVerticalLine() {
        this.verticalLocked = true;
    }

    public void IgnoreRotation() {
        this.rotationIgnored = true;
    }

    // ***** Slaves grab actions

    public void DontSaveTarget() { saveTarget = false; }

    public void AddCopycat(Transform copycat) {
        copycats.Add(copycat);
        startCopycats.Remove(copycat);
        startCopycats.Add(copycat, new TransformRecord(copycat));
    }

    public void AddSaveTarget(Transform save) {
        saves.Add(save);
        startSaves.Remove(save);
        startSaves.Add(save, new TransformRecord(save));
    }

    public void AddSlave(Transform slave) {
        AddCopycat(slave); AddSaveTarget(slave);
    }

    public void AddSlaves(IEnumerable<Transform> slavesList) {
        foreach (Transform slave in slavesList) AddSlave(slave);
    }

    public void SaveAllGrabs() {
        if (saveTarget) {
            var current = new TransformRecord(target);
            var action = new GrabAction(startLocal, current);
            UndoHistory.current.AddLazy(action);
        }
        foreach (Transform save in saves) {
            if (save != target) {
                var current = new TransformRecord(save);
                var action = new GrabAction(startSaves[save], current);
                UndoHistory.current.AddLazy(action);
            }
        }
    }

    public void RevertAllGrabs() {
        start.Apply(target);
        foreach (Transform copycat in copycats) {
            if (copycat != target) {
                startCopycats[copycat].Apply();
            }
        }
    }

    // ***** Grab initialization

    // Project the target onto the laser initially. Call after StartGrab().
    public void ProjectOnLaser() {
        var laser = GetLaser(source);
        if (laser != null) {
            var origin = laser.transform.position;
            var forward = laser.transform.forward;
            var adjusted = target.position - origin;
            var projected = Vector3.Project(adjusted, forward);
            phantom.position = projected + origin;
        }
    }

    public void StartGrab(Transform newTarget, GrabSource newSource) {
        Debug.Assert(!isGrabbing);

        target = newTarget;
        source = newSource;
        hand = GetGrabber(source);

        // ProceduralEmpty.RequireVisible(target);

        phantom.SetPositionAndRotation(target.position, target.rotation);
        phantom.parent = hand;

        var up = target.parent.TransformDirection(Vector3.up).normalized;
        localVertical = new Ray(target.position, up);
        localHorizontal = new Plane(up, target.position);

        startLocal = new TransformRecord(target);
        start = new DeltaRecord(target);
        isGrabbing = true;
        target.GetComponent<Interactive>()?.Grab(true);

        Refresh();
    }

    public void ReleaseGrab() {
        Debug.Assert(isGrabbing);

        Refresh();
        isGrabbing = false;
        target.GetComponent<Interactive>()?.Grab(false);

        if (onReleaseRun != null) onReleaseRun(target);
        ClearSpecs();
    }

    public void CancelGrab() {
        Debug.Assert(isGrabbing);

        Refresh();
        isGrabbing = false;
        target.GetComponent<Interactive>()?.Grab(false);
        RevertAllGrabs();

        if (onCancelRun != null) onCancelRun(target);
        ClearSpecs();
    }

    // ***** Grab state changes

    Grabber() {
        Debug.Assert(instance == null);
        instance = this;

        copycats = new DelegateLinkedSet<Transform>();
        saves = new DelegateLinkedSet<Transform>();
        startCopycats = new Dictionary<Transform, TransformRecord>();
        startSaves = new Dictionary<Transform, TransformRecord>();

        copycats.OnInsert.AddListener(Interactive.BeginSlaveGrab);
        copycats.OnRemove.AddListener(Interactive.EndSlaveGrab);
    }

    void Awake() {
        phantom = new GameObject("GrabberPhantom").transform;
        phantom.parent = rightGrabber;
        phantom.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    void FixedUpdate() {
        Refresh();
    }

    private void Refresh() {
        if (!isGrabbing) return;
        // Warning, geometry ahead :)

        if (beforeRefresh != null) beforeRefresh(target);

        // Project the object onto a plane or onto a line or onto a point
        if (lineLocked) {
            ApplyLineOrPointLock(lineLock);
        } else if (verticalLocked || applyUserLocks && userLocks.verticalLock) {
            ApplyLineOrPointLock(localVertical);
        } else if (planeLocked) {
            ApplyPlaneLock(planeLock);
        } else if (applyUserLocks && userLocks.planeLock) {
            ApplyPlaneLock(localHorizontal);
        }

        // Project the object onto a sphere
        if (sphereLocked) {
            if (sphereRadius == 0) {
                phantom.position = sphereCenter;
            } else {
                Vector3 normal = (phantom.position - sphereCenter).normalized;
                phantom.position = normal * sphereRadius + sphereCenter;
            }
        }

        // Apply rotation axis lock and rotation snap
        if (rotationIgnored || applyUserRotationLocks && userLocks.rotationLock) {
            target.position = phantom.position;
        } else if (applyUserRotationLocks && userLocks.rotaxis >= 0) {
            target.position = phantom.position;
            target.rotation = phantom.rotation;
            var quaternion = target.localRotation;
            var scaler = Vector3.zero;
            int d = (int)userLocks.rotaxis;
            scaler[d] = 1.0f;
            var euler = Vector3.Scale(scaler, quaternion.eulerAngles);
            if (userLocks.rotationSnap) {
                var threshold = userLocks.rotationSnapThreshold * Vector3.one;
                foreach (float offset in new float[] { 0, 90, 180, 270, 360 }) {
                    euler = Math3d.Snap(euler, threshold, offset * Vector3.one);
                }
            }
            target.localRotation = Quaternion.Euler(euler);
        } else {
            target.position = phantom.position;
            target.rotation = phantom.rotation;
        }

        // Apply grid snap. Program locks take precedence and forbid grid snapping
        if (applyUserLocks && !userLocks.gridFree && !planeLocked && !verticalLocked) {
            Vector3 grid;
            if (userLocks.planeLock && !userLocks.verticalLock) {
                grid = Vector3.Scale(userLocks.gridSnapScale, new Vector3(1, 0, 1));
            } else if (userLocks.verticalLock && !userLocks.planeLock) {
                grid = Vector3.Scale(userLocks.gridSnapScale, new Vector3(0, 1, 0));
            } else if (!userLocks.verticalLock && !userLocks.planeLock) {
                grid = userLocks.gridSnapScale;
            } else {
                grid = Vector3.zero;
            }
            if (grid != Vector3.zero) {
                var threshold = userLocks.gridSnapThreshold;
                Vector3 snap = target.localPosition;
                if (userLocks.gridPoint) {
                    snap = Math3d.GridSnap(target.localPosition, grid, threshold);
                } else if (userLocks.gridLine) {
                    snap = Math3d.GridlineSnap(target.localPosition, grid, threshold);
                }
                target.localPosition = snap;
            }
        }

        // Apply the above-floor lock
        if (applyUserLocks && floorLocked) {
            if (target.position.y < 0) {
                target.position += Vector3.down * target.position.y;
            }
        }

        if (afterRefresh != null) afterRefresh(target);

        // Adjust slave positions. They will be given same local position and rotation
        if (copycats.Count > 0) {
            var current = new LocalDeltaRecord(target);

            foreach (Transform copycat in copycats) {
                if (copycat != target) {
                    current.Apply(copycat);
                }
            }
        }
    }

    private void ApplyPlaneLock(Plane plane) {
        if (source == GrabSource.LeftHand || source == GrabSource.RightHand) {
            phantom.position = plane.ClosestPointOnPlane(phantom.position);
        } else {
            Vector3 hit;
            if (GetLaser(source).RayPlane(plane, out hit)) {
                phantom.position = hit;
            } else {
                phantom.position = plane.ClosestPointOnPlane(phantom.position);
            }
        }
    }

    private void ApplyLineLock(Ray ray) {
        if (source == GrabSource.LeftHand || source == GrabSource.RightHand) {
            var adjusted = phantom.position - ray.origin;
            var projected = Vector3.Project(adjusted, ray.direction);
            phantom.position = projected + ray.origin;
        } else {
            Vector3 hit;
            if (GetLaser(source).RayLine(ray, out hit)) {
                phantom.position = hit;
            } else {
                var adjusted = phantom.position - ray.origin;
                var projected = Vector3.Project(adjusted, ray.direction);
                phantom.position = projected + ray.origin;
            }
        }
    }

    private void ApplyLineOrPointLock(Ray ray) {
        Vector3 hit;
        if (planeLocked) {
            if (Vector3.Angle(ray.direction, planeLock.normal) > 0.25) {
                if (Math3d.LinePlaneIntersection(ray, planeLock, out hit)) {
                    phantom.position = hit;
                    return;
                }
            }
        } else if (applyUserLocks && userLocks.planeLock) {
            if (Vector3.Angle(ray.direction, localHorizontal.normal) > 0.25) {
                if (Math3d.LinePlaneIntersection(ray, localHorizontal, out hit)) {
                    phantom.position = hit;
                    return;
                }
            }
        }
        ApplyLineLock(ray);
    }

    private Transform GetGrabber(GrabSource source) {
        if (source == GrabSource.LeftHand || source == GrabSource.LeftLaser) {
            return leftGrabber;
        } else if (source == GrabSource.RightHand || source == GrabSource.RightLaser) {
            return rightGrabber;
        } else {
            throw new Exception(string.Format("Invalid grabber source {0}", source));
        }
    }

    private GrabLaser GetLaser(GrabSource source) {
        return GrabLaser.GetLaser(source);
    }

}
