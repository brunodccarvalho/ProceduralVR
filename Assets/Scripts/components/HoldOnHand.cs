using UnityEngine;
using Valve.VR.InteractionSystem;

public enum HoldWhere {
    DominantHand,
    HoverDominantHand,
    AttachDominantHand,
    OtherHand,
    HoverOtherHand,
    AttachOtherHand,
    FrontOfHead,
}

/**
 * Maintain this object as a child of the hand/head, having it hover a fixed distance
 * in world space and look towards the player's head.
 */
public class HoldOnHand : MonoBehaviour {

    [Tooltip("Where should the object hover")]
    public HoldWhere where = HoldWhere.DominantHand;

    [Tooltip("Hover offset (set only the Y value for reasonable results)")]
    public Vector3 hoverOffset = Vector3.zero;

    [Tooltip("Positive offsets move the object away from the head")]
    public float focalOffset = 0.0f;

    [Tooltip("Should this object be parented to the tracking object")]
    public bool setParent = false;

    [Tooltip("Disable all children when not facing upwards")]
    public bool disableChildren = false;

    [Tooltip("Render below iff angle with hand's down pointer is <= this offset")]
    public float upwardsAngle = 35;

    private bool upwardsActive = true;

    void FixedUpdate() {
        Transform head = Player.instance?.headCollider?.transform?.parent;
        Transform left = Player.instance?.leftHand?.transform;
        Transform right = Player.instance?.rightHand?.transform;
        bool rightHanded = MainController.instance.rightHanded;
        Transform dominant = rightHanded ? right : left;
        Transform other = rightHanded ? left : right;
        Transform tracker = null;

        if (!head || !left || !right)
            return;

        switch (where) {
            case HoldWhere.DominantHand:
                tracker = dominant;
                break;
            case HoldWhere.HoverDominantHand:
                tracker = dominant.Find("HoverPoint");
                break;
            case HoldWhere.AttachDominantHand:
                tracker = dominant.Find("ObjectAttachmentPoint");
                break;
            case HoldWhere.OtherHand:
                tracker = other;
                break;
            case HoldWhere.HoverOtherHand:
                tracker = other.Find("HoverPoint");
                break;
            case HoldWhere.AttachOtherHand:
                tracker = other.Find("ObjectAttachmentPoint");
                break;
            case HoldWhere.FrontOfHead:
                tracker = head;
                break;
            default:
                Debug.LogWarningFormat("Unknown HoldWhere value: {0}", where);
                break;
        }
        if (setParent && tracker != null && !Grabber.instance.IsGrabbing) {
            this.transform.SetParent(tracker, false);
        }

        // Set the hover offset relative to the parent position
        transform.position = tracker.position + hoverOffset;

        // Look towards the head
        if (transform.position != head.position) {
            var sight = (transform.position - head.position).normalized;
            var facing = Quaternion.LookRotation(sight);
            transform.rotation = facing;

            // Add focal offset
            transform.position += focalOffset * sight;
        }

        if (upwardsAngle != 0 && disableChildren) {
            var pointer = tracker.Find("DownPointer");

            if (pointer != null) {
                float angle = Vector3.Angle(pointer.forward, Vector3.up);
                if (angle < 0) angle += 360;
                angle = Mathf.Min(angle, 360 - angle);
                bool newActive = angle <= upwardsAngle;
                if (upwardsActive != newActive) {
                    upwardsActive = newActive;
                    foreach (Transform child in transform) {
                        child.gameObject.SetActive(upwardsActive);
                    }
                }
            }
        }
    }

}
