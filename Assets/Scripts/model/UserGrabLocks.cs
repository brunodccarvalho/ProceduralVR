using UnityEngine;

public enum RotaxisLock {
    Free = -2, Lock = -1, XAxis = 0, YAxis = 1, ZAxis = 2
}

public enum GridSnapType {
    Free = 0, Gridpoint = 1, Gridline = 2
}

[System.Serializable]
public class UserGrabLocks {

    [Tooltip("Lock the subject on the xOz plane in local space")]
    public bool planeLock = false;

    [Tooltip("Lock the subject on the Oy vertical line in local space")]
    public bool verticalLock = false;

    [Tooltip("Subject rotates freely (-2), not at all (-1), or around a given axis")]
    public RotaxisLock rotaxis = RotaxisLock.Free;
    public bool rotationLock => rotaxis == RotaxisLock.Lock;
    public bool rotationFree => rotaxis == RotaxisLock.Free;

    [Tooltip("Subject rotation snaps to multiples of 90º if locked around an axis")]
    public bool rotationSnap = false;
    public float rotationSnapThreshold = 20.0f;

    [Tooltip("Subject's position snaps to grid points (1) or grid lines (2)")]
    public GridSnapType gridType = GridSnapType.Free;
    public Vector3 gridSnapScale = Vector3.one;
    public float gridSnapThreshold = 0.25f;
    public bool gridFree => gridType == GridSnapType.Free;
    public bool gridPoint => gridType == GridSnapType.Gridpoint;
    public bool gridLine => gridType == GridSnapType.Gridline;

    public Vector3 tilingSnapDistance = Vector3.zero;
    public Vector3 positionSnapDistance = Vector3.zero;

    public bool positionAxisSnap = false;
    public bool tilingAxisSnap = true;

    public void Clear() {
        planeLock = false;
        verticalLock = false;
        rotaxis = RotaxisLock.Free;
        rotationSnap = false;
        gridType = GridSnapType.Free;
    }

    public void TogglePlaneLock() => planeLock = !planeLock;
    public void ToggleVerticalLock() => verticalLock = !verticalLock;
    public void ToggleRotaxis() {
        rotaxis = rotaxis != RotaxisLock.ZAxis ? rotaxis + 1 : rotaxis = RotaxisLock.Free;
    }
    public void ToggleRotationSnap() => rotationSnap = !rotationSnap;
    public void ToggleGridSnap() {
        gridType = gridType != GridSnapType.Gridline ? gridType + 1 : GridSnapType.Free;
    }

    public Vector3 TilingSnap(Vector3 v) {
        var snap = tilingSnapDistance == Vector3.zero ? gridSnapThreshold * gridSnapScale : tilingSnapDistance;
        return tilingAxisSnap ? Math3d.Snap(v, snap) : v;
    }

    public Vector3 PositionSnap(Vector3 v) {
        var snap = positionSnapDistance == Vector3.zero ? gridSnapThreshold * gridSnapScale : positionSnapDistance;
        return positionAxisSnap ? Math3d.Snap(v, snap) : v;
    }

}
