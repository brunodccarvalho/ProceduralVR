[System.Flags]
public enum PMode {
    Global = 1 << 30,
    Local = 1 << 31,

    EditWait = Local | 1 << 0,
    EditGrab = Local | 1 << 1,
    EditClone = Local | 1 << 2,
    EditDelete = Local | 1 << 3,
    EditOp = 1 << 0 | 1 << 1 | 1 << 2 | 1 << 3,

    EditGroup = Local | 1 << 10,
    EditRandom = Local | 1 << 11,
    EditPosition = Local | 1 << 12,
    EditRotation = Local | 1 << 13,
    EditTiling = Local | 1 << 14,

    // ---

    GlobalGrab = Global | 1 << 28,
    GlobalEdit = Global | 1 << 29,

    GlobalClone = Global | 1 << 15,
    GlobalRandomize = Global | 1 << 16,
    GlobalDelete = Global | 1 << 17,
    GlobalUnlink = Global | 1 << 18,
    GlobalDisband = Global | 1 << 19,

    CreateGroup = Global | 1 << 20,
    CreateRandom = Global | 1 << 21,
    CreateRandomPosition = Global | 1 << 22,
    CreateRandomRotation = Global | 1 << 23,
    CreateTiling = Global | 1 << 24,

    EditGroupWait = EditGroup | EditWait,
    EditGroupGrab = EditGroup | EditGrab,
    EditGroupClone = EditGroup | EditClone,
    EditGroupDelete = EditGroup | EditDelete,

    EditRandomWait = EditRandom | EditWait,
    EditRandomGrab = EditRandom | EditGrab,
    EditRandomClone = EditRandom | EditClone,
    EditRandomDelete = EditRandom | EditDelete,

    EditPositionWait = EditPosition | EditWait,
    EditPositionGrab = EditPosition | EditGrab,

    EditRotationWait = EditRotation | EditWait,
    EditRotationGrab = EditRotation | EditGrab,

    EditTilingWait = EditTiling | EditWait,
    EditTilingGrab = EditTiling | EditGrab,
}
