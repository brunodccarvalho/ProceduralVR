using UnityEngine;

public enum ButtonType {
    ModeToggle,
    ProceduralAction,
    UserAction,
    None,
}

[DisallowMultipleComponent]
public class InteractiveButton : Interactive {

    public ButtonType buttonType = ButtonType.None;
    public string description;
    public PMode buttonMode = 0;
    public PAction buttonAction = 0;
    public PUserAction buttonUserAction = 0;

    public int[] indexMaterials;
    public Material[] materials;
    private MeshRenderer icon;

    protected override void Start() {
        base.Start();
        Debug.Assert(buttonType != ButtonType.None);
        Debug.Assert(this.clickable && !this.procedural && !this.augmentation);

        this.icon = this.transform.parent.Find("Icon").GetComponent<MeshRenderer>();

        var outline = this.GetComponent<RecursiveOutline>();
        if (buttonType == ButtonType.ModeToggle) {
            Debug.Assert(buttonMode != 0);
            outline.SetStyle("ModeToggleButton");
        } else if (buttonType == ButtonType.ProceduralAction) {
            Debug.Assert(buttonAction != 0);
            outline.SetStyle("ProceduralActionButton");
        } else if (buttonType == ButtonType.UserAction) {
            Debug.Assert(buttonUserAction != 0);
            outline.SetStyle("UserActionButton");
        }
    }

    public override void Click() {
        if (buttonType == ButtonType.ModeToggle) {
            Interactor.instance.TransitionMode(buttonMode);
        } else if (buttonType == ButtonType.ProceduralAction) {
            Interactor.instance.PerformAction(buttonAction);
        } else {
            Interactor.instance.PerformUserAction(buttonUserAction);
        }
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        UpdateButtonState();
        UpdateDynamicIcon();
    }

    private void UpdateButtonState() {
        if (buttonType == ButtonType.ModeToggle) {
            var mode = Interactor.instance.currentMode;
            this.Select(mode == buttonMode);
            this.Color(0);
        } else if (buttonType == ButtonType.ProceduralAction) {
            this.Select(false);
            this.Color(0);
        } else if (buttonType == ButtonType.UserAction) {
            this.Select(false);
            this.Color(0);
            UpdateUserActionState();
        }
    }

    private void UpdateUserActionState() {
        var locks = Grabber.instance.userLocks;
        switch (buttonUserAction) {
            case PUserAction.GrabTogglePlaneLock:
                this.Select(locks.planeLock);
                break;
            case PUserAction.GrabToggleVerticalLock:
                this.Select(locks.verticalLock);
                break;
            case PUserAction.GrabToggleRotaxis:
                int d = (int)locks.rotaxis;
                if (locks.rotationFree) {
                    this.Color(1); // seafoam
                } else if (locks.rotationLock) {
                    this.Color(2); // orchid
                } else {
                    this.Color(7 + d); // red, green, blue
                }
                break;
            case PUserAction.GrabToggleRotationSnap:
                this.Select(locks.rotationSnap);
                break;
            case PUserAction.GrabToggleGridSnap:
                this.Color((int)locks.gridType);
                break;
            case PUserAction.ToggleVisibleEmpty:
                this.Select(ProceduralEmpty.stateVisible);
                break;
            case PUserAction.UpscaleScene:
                break;
            case PUserAction.DownscaleScene:
                break;
            default:
                // ! INTERNAL ERROR
                Debug.LogErrorFormat("UpdateUserActionState: Skipped action {0}", buttonUserAction);
                break;
        }
    }

    private void UpdateDynamicIcon() {
        if (this.indexMaterials != null && this.materials != null) {
            for (int i = 0; i < indexMaterials.Length; i++) {
                if (this.color == indexMaterials[i]) {
                    icon.material = materials[i];
                    break;
                }
            }
        }
    }

}
