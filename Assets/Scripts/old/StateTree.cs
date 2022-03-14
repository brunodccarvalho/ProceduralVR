#if false
using UnityEngine;

public class StateTree : MonoBehaviour {

    public static StateTree instance;

    public Transform textRoot;
    TextMesh mesh;

    StateTree() {
        Debug.Assert(instance == null);
        instance = this;
    }

    void Awake() {
        mesh = textRoot.GetComponent<TextMesh>();
    }

    void Update() {
        mesh.color = SceneView.instance.ModeColor();
    }

}
#endif
