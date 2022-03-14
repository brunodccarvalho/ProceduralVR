using UnityEngine;

[DisallowMultipleComponent]
public class HandCollisionDispatch : MonoBehaviour {

    public GrabSource source;

    void Awake() {
        Debug.Assert(source == GrabSource.LeftHand || source == GrabSource.RightHand);
    }

    void OnTriggerEnter(Collider collision) {
        InputController.instance.AddHandCollision(collision.transform, source);
    }

    void OnTriggerExit(Collider collision) {
        InputController.instance.RemoveHandCollision(collision.transform, source);
    }

}
