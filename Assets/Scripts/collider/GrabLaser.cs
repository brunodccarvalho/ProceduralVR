using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GrabLaser : MonoBehaviour {

    static Dictionary<GrabSource, GrabLaser> instances;
    public GrabSource source;
    public float lineLength = 5000;

    bool rightHanded => MainController.instance.rightHanded;
    GrabSource domLaser => rightHanded ? GrabSource.RightLaser : GrabSource.LeftLaser;

    static GrabLaser() {
        instances = new Dictionary<GrabSource, GrabLaser>();
    }

    LineRenderer line;
    Transform collision;

    void Awake() {
        Debug.Assert(source == GrabSource.LeftLaser || source == GrabSource.RightLaser);
        instances.Add(source, this);

        Debug.Assert(lineLength > 0);
        line = this.GetComponent<LineRenderer>();

        if (source != domLaser) {
            this.gameObject.SetActive(false);
        }
    }

    void Refresh() {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out hit, lineLength, InputController.instance.priorityLayer)) {
            var diff = hit.point - transform.position;
            var distance = Vector3.Magnitude(transform.TransformDirection(diff));
            line.SetPosition(1, Vector3.forward * distance);
            if (collision != hit.collider.transform) {
                collision = hit.collider.transform;
                InputController.instance.UpdateLaserCollision(collision, source);
            }
        } else if (Physics.Raycast(ray, out hit, lineLength, InputController.instance.normalLayer)) {
            var diff = hit.point - transform.position;
            var distance = Vector3.Magnitude(transform.TransformDirection(diff));
            line.SetPosition(1, Vector3.forward * distance);
            if (collision != hit.collider.transform) {
                collision = hit.collider.transform;
                InputController.instance.UpdateLaserCollision(collision, source);
            }
        } else {
            line.SetPosition(1, Vector3.forward * lineLength);
            if (collision != null) {
                collision = null;
                InputController.instance.UpdateLaserCollision(collision, source);
            }
        }

        var color = Palette.ModeColor();
        line.startColor = color;
        line.endColor = color;
    }

    void FixedUpdate() {
        // Check if we should switch laser hand
        if (!Grabber.instance.IsGrabbing && source != domLaser) {
            var other = instances[domLaser];
            other.gameObject.SetActive(true);
            other.Refresh();
            this.gameObject.SetActive(false);
            return;
        }

        Refresh();
    }

    public Ray GetRay() {
        return new Ray(transform.position, transform.forward);
    }

    public bool RayPlane(Plane plane, out Vector3 hit) {
        float distance;
        Ray ray = GetRay();

        if (plane.Raycast(ray, out distance)) {
            hit = transform.position + transform.forward * distance;
            return true;
        } else {
            hit = transform.position + transform.forward * distance;
            return false;
        }
    }

    public bool RayLine(Ray ray, out Vector3 hit) {
        Vector3 dummy;
        return Math3d.ClosestPointOnTwoLines(ray, GetRay(), out hit, out dummy);
    }

    public static GrabLaser GetLaser(GrabSource source) {
        if (instances.ContainsKey(source)) {
            return instances[source];
        } else {
            return null;
        }
    }

}
