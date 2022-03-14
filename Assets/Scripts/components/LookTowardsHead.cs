using UnityEngine;
using Valve.VR.InteractionSystem;

/**
 * Every frame, redirect this component so that it looks towards the HMD head
 */
[DisallowMultipleComponent]
public class LookTowardsHead : MonoBehaviour {

    public Vector3 offset = Vector3.zero;

    void FixedUpdate() {
        var head = Player.instance?.headCollider?.transform;
        if (head != null) {
            var glob = Vector3.Scale(transform.lossyScale, offset);
            var where = transform.position + glob - head.position;
            var facing = Quaternion.LookRotation(where);
            transform.rotation = facing;
        }
    }

}
