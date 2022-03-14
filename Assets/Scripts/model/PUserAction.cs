[System.Flags]
public enum PUserAction {
    Grabber = 1 << 29,
    Procedural = 1 << 30,
    Scene = 1 << 31,

    // ---

    GrabTogglePlaneLock = Grabber | 1 << 0,
    GrabToggleVerticalLock = Grabber | 1 << 1,
    GrabToggleRotaxis = Grabber | 1 << 2,
    GrabToggleRotationSnap = Grabber | 1 << 3,
    GrabToggleGridSnap = Grabber | 1 << 4,

    ToggleVisibleEmpty = Procedural | 1 << 0,

    UpscaleScene = Scene | 1 << 0,
    DownscaleScene = Scene | 1 << 1,
}
