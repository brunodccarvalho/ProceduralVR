using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AugmentController : MonoBehaviour {

    public static AugmentController instance;

    // These are constants built into the prefabs themselves, here for convenience only
    public const float handleRadius = 0.1f;
    public const float bellHeight = 0.25f;

    public GameObject axisPrefab;
    public GameObject positionPrefab;
    public GameObject rotationPrefab;
    public GameObject tilingPrefab;
    Dictionary<Transform, Transform> axisSet;

    AugmentController() {
        Debug.Assert(instance == null);
        instance = this;
        axisSet = new Dictionary<Transform, Transform>();
    }

    public Transform GetAxis(Transform target) {
        if (axisSet.ContainsKey(target)) {
            return axisSet[target];
        } else {
            return null;
        }
    }

    public void AddAxis(Transform target) {
        if (target != null && !axisSet.ContainsKey(target)) {
            var axis = GameObject.Instantiate(axisPrefab).transform;
            axisSet.Add(target, axis);
            axis.SetPositionAndRotation(target.position, target.rotation);
            axis.parent = ProceduralFactory.root;
        }
    }

    public void RemoveAxis(Transform target) {
        if (target != null && axisSet.ContainsKey(target)) {
            var axis = axisSet[target];
            GameObject.Destroy(axis.gameObject);
            axisSet.Remove(target);
        }
    }

    public static void EndAxis(Transform target) { // like remove but no warning
        if (target != null && instance.axisSet.ContainsKey(target)) {
            instance.RemoveAxis(target);
        }
    }

    public GameObject InstantiatePosition() {
        return GameObject.Instantiate(positionPrefab);
    }

    public GameObject InstantiateRotation() {
        return GameObject.Instantiate(rotationPrefab);
    }

    public GameObject InstantiateTiling() {
        return GameObject.Instantiate(tilingPrefab);
    }

}
