using UnityEngine;

/**
 * Every frame, redirect this component so that it looks towards an eye
 */
[DisallowMultipleComponent]
public class LookTowards : MonoBehaviour {

    public Transform eye;
    public Vector3 offset = Vector3.zero;

    void FixedUpdate() {
        var where = (transform.position + offset) - eye.position;
        var facing = Quaternion.LookRotation(where);
        transform.rotation = facing;
    }

}
