using UnityEngine;

[DisallowMultipleComponent]
public class ProceduralPrefab : Procedural {

    public ProceduralPrefab() { proctype = ProceduralType.Prefab; }

    public override string Description(bool small) {
        return this.name;
    }

}
