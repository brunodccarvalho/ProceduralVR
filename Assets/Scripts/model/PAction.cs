[System.Flags]
public enum PAction {
    Global = 1 << 26,
    Local = 1 << 27,

    Group = Local | 1 << 10,
    Random = Local | 1 << 11,
    Position = Local | 1 << 12,
    Rotation = Local | 1 << 13,
    Tiling = Local | 1 << 14,

    // ---

    Randomize = 1 << 16,
    Delete = 1 << 17,
    Unlink = 1 << 18,
    Disband = 1 << 19,

    GroupDeleteChild = Group | Delete,
    GroupRandomize = Group | Randomize,
    RandomDeleteChild = Random | Delete,
    RandomCycle = Random | (1 << 21),
    PositionRefresh = Position | (1 << 20),
    RotationRefresh = Rotation | (1 << 20),
    TilingAdd = Tiling | (1 << 22),
    TilingRemove = Tiling | (1 << 23),
}
