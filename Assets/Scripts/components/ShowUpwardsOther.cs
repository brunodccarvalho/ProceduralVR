using UnityEngine;
using UnityEngine.Events;

/**
 * Every frame, set the 'subject' component to active/inactive if the angle that
 * this transform's rotation * direction makes with the z axis is less than 45 degrees.
 * Notice you cannot put this script in the subject itself.
 */
[DisallowMultipleComponent]
public class ShowUpwardsOther : MonoBehaviour {

    public GameObject subject;
    public Vector3 direction = Vector3.up;
    public float maxAngleDegrees = 45;
    public bool onlyIfParent = true; // dirty hack to support switching dominant hand

    void Start() {
        Debug.Assert(subject != null);
        Debug.Assert(0 < maxAngleDegrees && maxAngleDegrees <= 90);
    }

    void FixedUpdate() {
        float angle = Vector3.Angle(transform.rotation * direction, Vector3.up);
        Debug.Assert(angle >= -360 && angle <= 360);
        if (angle < 0) angle += 360;
        bool upwards = angle >= 360 - maxAngleDegrees || angle <= maxAngleDegrees;
        if (!onlyIfParent || subject.transform.parent == this.transform) {
            subject.SetActive(upwards);
        }
    }

}
