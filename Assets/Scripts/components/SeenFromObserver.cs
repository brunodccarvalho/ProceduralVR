using UnityEngine;
using UnityEngine.Events;

/**
 * Every frame, check if the observer object is looking towards this object, up to a
 * certain angle. Call a function every frame and when the state changes.
 */
public class SeenFromObserver : MonoBehaviour {

    public Transform subject;
    public Transform observer => transform;
    public float maxAngleDegrees = 30;
    public Vector3 offset = Vector3.zero;
    public UnityEvent<Transform> OnActivate, OnDeactivate;

    bool currentSeen = false;

    public static SeenFromObserver Add(Transform subject, Transform observer) {
        var seen = observer.gameObject.AddComponent<SeenFromObserver>();
        seen.subject = subject;
        return seen;
    }

    SeenFromObserver() {
        if (OnActivate == null)
            OnActivate = new UnityEvent<Transform>();
        if (OnDeactivate == null)
            OnDeactivate = new UnityEvent<Transform>();
    }

    void Start() {
        Debug.Assert(0 < maxAngleDegrees && maxAngleDegrees <= 90);
    }

    void Update() {
        Vector3 ray = subject.position + offset - observer.position;
        Vector3 glare = observer.forward;
        float angle = Vector3.Angle(ray, glare);
        Debug.Assert(angle >= -360 && angle <= 360);
        if (angle < 0) angle += 360;
        bool seen = angle >= 360 - maxAngleDegrees || angle <= maxAngleDegrees;
        if (seen && !currentSeen) {
            OnActivate.Invoke(subject);
        } else if (!seen && currentSeen) {
            OnDeactivate.Invoke(subject);
        }
        currentSeen = seen;
    }

}
