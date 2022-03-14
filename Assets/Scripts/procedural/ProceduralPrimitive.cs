using UnityEngine;

[DisallowMultipleComponent]
public class ProceduralPrimitive : Procedural {

    public ProceduralPrimitive() { proctype = ProceduralType.Primitive; }

    public override string Description(bool small = false) {
        return this.name;
    }

}
