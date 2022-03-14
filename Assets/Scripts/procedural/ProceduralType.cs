[System.Flags]
public enum ProceduralType {
    None = 0,
    Singular = 1 << 30,
    Composite = 1 << 31,
    Many = Composite | 1 << 29,

    Primitive = Singular | 1 << 0,
    Prefab = Singular | 1 << 1,
    Empty = Singular | 1 << 2,
    Group = Many | 1 << 10,
    Random = Many | 1 << 11,
    Position = Composite | 1 << 12,
    Rotation = Composite | 1 << 13,
    Tiling = Many | 1 << 14,
}
