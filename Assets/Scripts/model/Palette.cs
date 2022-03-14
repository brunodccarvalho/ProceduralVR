using UnityEngine;

public static class Palette {

    public static Color red = Color.red;
    public static Color green = Color.green;
    public static Color blue = Color.blue;

    public static Color lightred = new Color(1, 0.25f, 0.25f);
    public static Color lightgreen = new Color(0.25f, 1, 0.25f);
    public static Color lightblue = new Color(0, 0.70f, 1);

    public static Color black = Color.black;
    public static Color gray = Color.gray;
    public static Color white = Color.white;
    public static Color yellow = Color.yellow;
    public static Color cyan = Color.cyan;
    public static Color magenta = Color.magenta;

    public static Color orange = new Color(1.0f, 0.6f, 0);
    public static Color orchid = new Color(0.85f, 0.44f, 0.84f);
    public static Color seafoam = new Color(0, 1, 0.5f);
    public static Color purple = new Color(0.5f, 0, 0.5f);

    public static Color ModeColor() {
        var currentMode = Interactor.instance.currentMode;

        switch (currentMode) {
            case PMode.GlobalGrab:
                return Palette.red;

            case PMode.GlobalClone:
            case PMode.GlobalRandomize:
                return Palette.green;

            case PMode.GlobalDelete:
                return Palette.black;

            case PMode.GlobalUnlink:
            case PMode.GlobalDisband:
                return Palette.white;

            case PMode.CreateGroup:
            case PMode.CreateRandom:
            case PMode.CreateRandomPosition:
            case PMode.CreateRandomRotation:
            case PMode.CreateTiling:
                return Palette.blue;

            case PMode.GlobalEdit:
            case PMode.EditGroupWait:
            case PMode.EditRandomWait:
            case PMode.EditPositionWait:
            case PMode.EditRotationWait:
            case PMode.EditTilingWait:
                return Palette.cyan;

            case PMode.EditGroupGrab:
            case PMode.EditRandomGrab:
            case PMode.EditPositionGrab:
            case PMode.EditRotationGrab:
            case PMode.EditTilingGrab:
                return Palette.magenta;

            case PMode.EditGroupClone:
            case PMode.EditRandomClone:
                return Palette.green;

            case PMode.EditGroupDelete:
            case PMode.EditRandomDelete:
                return Palette.black;

            default:
                // ! INTERNAL ERROR
                Debug.LogErrorFormat("ModeColor: Skipped mode {0}", currentMode);
                return Palette.gray;
        }
    }

}
