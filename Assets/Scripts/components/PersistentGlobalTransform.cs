using UnityEngine;

/**
 * Fix the transform's global position, scale and/or rotation (individual components)
 * to offset potential changes in parent position, scale or rotation
 * Use the initial position/scale/rotation in Awake() if read is true, otherwise read from
 * the member variables, presumably set in the editor
 */
[DisallowMultipleComponent]
public class PersistentGlobalTransform : MonoBehaviour {

    public Vector3Int persistPosition = Vector3Int.zero;
    public Vector3Int persistScale = Vector3Int.zero;
    public int persistRotation = 0;

    public bool readPosition = true;
    public bool readScale = true;
    public bool readRotation = true;

    public Vector3 fixedPosition = Vector3.zero;
    public Vector3 fixedScale = Vector3.one;
    public Quaternion fixedRotation = Quaternion.identity;

    private bool anyPosition => persistPosition != Vector3Int.zero;
    private bool anyScale => persistScale != Vector3Int.zero;

    void Awake() {
        if (readPosition) fixedPosition = transform.position;
        if (readScale) fixedScale = transform.lossyScale;
        if (readRotation) fixedRotation = transform.rotation;
    }

    void FixedUpdate() {
        var parent = this.transform.parent;
        int index = this.transform.GetSiblingIndex();
        this.transform.parent = null;

        if (anyPosition) {
            Vector3 position = this.transform.position;
            for (int i = 0; i < 3; i++) {
                if (persistPosition[i] != 0) {
                    position[i] = fixedPosition[i];
                }
            }
            this.transform.position = position;
        }
        if (anyScale) {
            Vector3 scale = this.transform.localScale;
            for (int i = 0; i < 3; i++) {
                if (persistScale[i] != 0) {
                    scale[i] = fixedScale[i];
                }
            }
            this.transform.localScale = scale;
        }
        if (persistRotation != 0) {
            this.transform.rotation = fixedRotation;
        }

        this.transform.SetParent(parent);
        this.transform.SetSiblingIndex(index);
    }

}
