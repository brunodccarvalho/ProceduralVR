using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ProceduralEmpty : ProceduralPrimitive {

    static HashSet<ProceduralEmpty> empties = new HashSet<ProceduralEmpty>();
    public static bool stateVisible = true;

    public bool alwaysVisible = false;

    ProceduralEmpty() { proctype = ProceduralType.Empty; }

    protected override void Start() {
        base.Start();
        empties.Add(this);
        SetState(stateVisible);
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        empties.Remove(this);
    }

    public override string Description(bool small = false) {
        return "None Entity";
    }

    // ***** ProceduralEmpty

    void SetState(bool active = true) {
        transform.GetChild(0)?.gameObject.SetActive(active);
    }

    public static void SetStateAll(bool active = true) {
        foreach (ProceduralEmpty empty in empties) {
            empty.SetState(active);
        }
        stateVisible = active;
    }

    public static void ToggleStateAll() { SetStateAll(!stateVisible); }

    public static void RequireVisible(Transform target) {
        var procedurals = target.GetComponentsInChildren<Procedural>(true);
        foreach (Procedural procedural in procedurals) {
            if (procedural is ProceduralEmpty) continue;
            return;
        }
        SetStateAll(true);
    }

}
