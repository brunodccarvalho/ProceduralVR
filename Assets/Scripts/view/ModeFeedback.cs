using UnityEngine;

public class ModeFeedback : MonoBehaviour {

    TextMesh mesh;
    Transform edit;
    SceneEvent lastEvent;

    void Awake() {
        if (this.mesh == null) {
            this.mesh = this.transform.GetComponentInChildren<TextMesh>();
        }
    }

    void Update() {
        Refresh();
    }

    private void Refresh() {
        var (label, explanation) = ExplainText();
        var html = string.Format("<b>{0}</b>\n{1}", label, explanation);
        mesh.richText = true;
        mesh.text = html;
        mesh.color = Palette.ModeColor();
    }

    private (string, string) ExplainText() {
        var currentMode = Interactor.instance.currentMode;

        switch (currentMode) {
            case PMode.GlobalGrab:
                return ("Grab", "Hold trigger over an object to grab and drag it");

            case PMode.GlobalClone:
                return ("Clone", "Hold trigger over an object to clone and drag it");

            case PMode.GlobalRandomize:
                return ("Reroll", "Click on object to reroll it");

            case PMode.GlobalDelete:
                return ("Delete", "Click on object to delete it");

            case PMode.GlobalUnlink:
                return ("Unlink", "Click on object to unlink it from related objects");

            case PMode.GlobalDisband:
                return ("Disband", "Click on object to disband it");

            case PMode.CreateGroup:
                return ("Create Group", "Select elements to form the group, then accept");

            case PMode.CreateRandom:
                return ("Create Picker", "Select variants to form the picker, then accept");

            case PMode.CreateRandomPosition:
                return ("Create Mover", "Click on object to create a mover inplace");

            case PMode.CreateRandomRotation:
                return ("Create Rotator", "Click on object to create a rotator inplace");

            case PMode.CreateTiling:
                return ("Create Tiling", "Click on object to create a tiling inplace");

            case PMode.GlobalEdit:
                return ("Edit", "Click any composite entity to edit it");

            case PMode.EditGroupWait:
            case PMode.EditRandomWait:
            case PMode.EditPositionWait:
            case PMode.EditRotationWait:
            case PMode.EditTilingWait:
                return ("Edit", "Click any composite entity to edit it");

            case PMode.EditGroupGrab:
                return ("Grab", "Drag elements to change their relative positions");

            case PMode.EditRandomGrab:
                return ("Grab", "Drag element to change its relative position");

            case PMode.EditPositionGrab:
                return ("Grab", "Drag the handles to adjust the mover's box");

            case PMode.EditRotationGrab:
                return ("Grab", "Drag one handle to adjust the rotator's angle");

            case PMode.EditTilingGrab:
                return ("Grab", "Drag the handles to change spacing");

            case PMode.EditGroupClone:
                return ("Clone", "Grab any object to clone it and add as an element");

            case PMode.EditRandomClone:
                return ("Clone", "Grab any object to clone it and add as a variant");

            case PMode.EditGroupDelete:
                return ("Delete", "Click an element to delete it");

            case PMode.EditRandomDelete:
                return ("Delete", "Click on the variant to delete it");

            default:
                // ! INTERNAL ERROR
                Debug.LogErrorFormat("ModeColor: Skipped mode {0}", currentMode);
                return (null, null);
        }
    }

}
