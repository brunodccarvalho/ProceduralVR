[System.Flags]
public enum SceneEventType {
    Feedback = 1 << 29,
    History = 1 << 30,
    Procedural = 1 << 31,

    Grab = Procedural | 1 << 25,
    Create = Procedural | 1 << 26,
    Selection = Procedural | 1 << 27,
    OtherAction = Procedural | 1 << 28,

    // ---

    Info = Feedback | 1 << 0,
    Warning = Feedback | 1 << 1,
    Error = Feedback | 1 << 2,
    InternalError = Feedback | 1 << 3,
    Special = Feedback | 1 << 4,
    Logging = Feedback | 1 << 5,

    UndoRedo = History | 1 << 0,

    StartMove = Grab | 1 << 0,
    StartClone = Grab | 1 << 1,
    EndMove = Grab | 1 << 2,
    EndClone = Grab | 1 << 3,
    CancelMove = Grab | 1 << 4,
    CancelClone = Grab | 1 << 5,

    Delete = OtherAction | 1 << 6,
    Randomize = OtherAction | 1 << 7,
    Unlink = OtherAction | 1 << 8,
    Disband = OtherAction | 1 << 9,

    AddSelection = Selection | 1 << 10,
    RemoveSelection = Selection | 1 << 11,
    CancelSelection = Selection | 1 << 13,

    CreateGroup = Create | 1 << 25,
    CreateRandom = Create | 1 << 26,
    CreatePosition = Create | 1 << 27,
    CreateRotation = Create | 1 << 28,
    CreateTiling = Create | 1 << 29,

    DeleteChild = OtherAction | 1 << 14,
    Cycle = OtherAction | 1 << 15,
    Refresh = OtherAction | 1 << 16,
    AddChild = OtherAction | 1 << 17,
    RemoveChild = OtherAction | 1 << 18,
}
