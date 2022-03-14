#if false
using System.Collections.Generic;
using UnityEngine;

public class EditInteraction : InteractionStrategy {

    enum EditMode {
        Edit = 0,
        GroupGrab = 1,
        GroupAdd = 2,
        GroupDelete = 3,
        RandomGrab = 4,
        RandomAdd = 5,
        RandomDelete = 6,
        PositionGrab = 7,
        RotationGrab = 8,
        TilingGrab = 9,
    }

    DelegateLinkedSet<Transform> editStack;
    Dictionary<Transform, int> editCount;
    EditMode mode;

    // Useful getters
    GrabSource grabSource => Grabber.instance.grabSource;
    bool isGrabbing => Grabber.instance.IsGrabbing;
    bool isExposed => editStack.Count > 0;
    Transform head => editStack.First?.Value;
    Transform tail => editStack.Last?.Value;
    bool tailGroup => isExposed && IsGroup(tail);
    bool tailRandom => isExposed && IsRandom(tail);
    bool tailPosition => isExposed && IsPosition(tail);
    bool tailRotation => isExposed && IsRotation(tail);
    bool tailTiling => isExposed && IsTiling(tail);
    ProceduralGroup procGroup => tail.GetComponent<ProceduralGroup>();
    ProceduralRandom procRandom => tail.GetComponent<ProceduralRandom>();
    ProceduralPosition procPosition => tail.GetComponent<ProceduralPosition>();
    ProceduralRotation procRotation => tail.GetComponent<ProceduralRotation>();
    ProceduralTiling procTiling => tail.GetComponent<ProceduralTiling>();

    public EditInteraction() {
        editStack = new DelegateLinkedSet<Transform>();
        editCount = new Dictionary<Transform, int>();
        editStack.OnInsert.AddListener(Interactive.BeginExpose);
        editStack.OnRemove.AddListener(Interactive.EndExpose);
        editStack.OnRemove.AddListener(AugmentController.EndAxis);
        mode = EditMode.Edit;
    }

    public void Start() {
        mode = EditMode.Edit;
        editStack.Clear();
        editCount.Clear();
        SetNewUI();
    }

    public void Close() {
        Debug.Assert(!isGrabbing);
        editStack.Clear();
        editCount.Clear();
        ButtonsController.instance.SelectButton("edit-mode", false);
    }

    public void GrabDown(Transform target, GrabSource source) {
        Interactive config = target.GetComponent<Interactive>();
        Procedural proc = config as Procedural;
        if (isGrabbing) {
            return;
        }
        if (config.clickable) {
            config.Click();
            return;
        }
        if (config.procedural) {
            if (mode == EditMode.Edit) {
                if (!isExposed && proc.Composite()) {
                    StartEdit(target);
                    return;
                }
                if (isExposed && target.parent == tail && proc.Composite()) {
                    RecurseEdit(target);
                    return;
                }
            }
            if (mode == EditMode.GroupGrab && target.parent == tail) {
                InteractionFactory.StartGroupMoveGrab(tail, target, source);
                return;
            }
            if (mode == EditMode.GroupAdd && proc.cloneable) {
                InteractionFactory.TryStartGroupCloneGrab(tail, target, source);
                return;
            }
            if (mode == EditMode.GroupDelete && target.parent == tail) {
                bool successful = InteractionFactory.TryDeleteGroupChild(tail, target);
                editCount[tail] += successful ? 1 : 0;
                return;
            }
            if (mode == EditMode.RandomGrab && target.parent == tail) {
                InteractionFactory.StartRandomMoveGrab(tail, target, source);
                return;
            }
            if (mode == EditMode.RandomAdd && proc.cloneable) {
                InteractionFactory.TryStartRandomCloneGrab(tail, target, source);
                return;
            }
            if (mode == EditMode.RandomDelete && target.parent == tail) {
                bool successful = InteractionFactory.TryDeleteRandomChild(tail, target);
                editCount[tail] += successful ? 1 : 0;
                return;
            }
            if (mode == EditMode.PositionGrab && target.parent == tail) {
                InteractionFactory.TryStartPositionGrab(tail, target, source);
                return;
            }
            if (mode == EditMode.RotationGrab && target.parent == tail) {
                InteractionFactory.TryStartRotationGrab(tail, target, source);
                return;
            }
            if (mode == EditMode.TilingGrab && target.parent == tail) {
                InteractionFactory.TryStartTilingGrab(tail, target, source);
                return;
            }
        }
        if (config.augmentation) {
            if (mode == EditMode.PositionGrab) {
                InteractionFactory.TryStartPositionGrab(tail, target, source);
                return;
            }
            if (mode == EditMode.RotationGrab) {
                InteractionFactory.TryStartRotationGrab(tail, target, source);
                return;
            }
            if (mode == EditMode.TilingGrab) {
                InteractionFactory.TryStartTilingGrab(tail, target, source);
                return;
            }
        }
    }

    public void GrabUp(GrabSource source) {
        if (isGrabbing && grabSource == source) {
            ReleaseGrab();
        }
    }

    public void ClickButton(string button) {
        if (Locked()) {
            return;
        } else if (!isExposed) {
            if (InteractionController.instance.IsStrategyButton(button)) {
                InteractionController.instance.Transition(button);
            }
        } else if (tailGroup) {
            if (button == "edit-mode") {
                SwitchMode(EditMode.Edit);
            } else if (button == "grab-mode") {
                SwitchMode(EditMode.GroupGrab);
            } else if (button == "clone-mode") {
                SwitchMode(EditMode.GroupAdd);
            } else if (button == "delete-mode") {
                SwitchMode(EditMode.GroupDelete);
            } else if (button == "randomize") {
                bool successful = InteractionFactory.TryRandomizeTarget(tail);
                editCount[tail] += successful ? 1 : 0;
            }
        } else if (tailRandom) {
            if (button == "edit-mode") {
                SwitchMode(EditMode.Edit);
            } else if (button == "grab-mode") {
                SwitchMode(EditMode.RandomGrab);
            } else if (button == "clone-mode") {
                SwitchMode(EditMode.RandomAdd);
            } else if (button == "delete-mode") {
                SwitchMode(EditMode.RandomDelete);
            } else if (button == "cycle") {
                InteractionFactory.CycleRandomChild(tail);
                editCount[tail]++;
            }
        } else if (tailPosition) {
            if (button == "edit-mode") {
                SwitchMode(EditMode.Edit);
            } else if (button == "grab-mode") {
                SwitchMode(EditMode.PositionGrab);
            } else if (button == "refresh") {
                InteractionFactory.RefreshProceduralPosition(tail);
                editCount[tail]++;
            }
        } else if (tailRotation) {
            if (button == "edit-mode") {
                SwitchMode(EditMode.Edit);
            } else if (button == "grab-mode") {
                SwitchMode(EditMode.RotationGrab);
            } else if (button == "refresh") {
                InteractionFactory.RefreshProceduralRotation(tail);
                editCount[tail]++;
            }
        } else if (tailTiling) {
            if (button == "edit-mode") {
                SwitchMode(EditMode.Edit);
            } else if (button == "grab-mode") {
                SwitchMode(EditMode.TilingGrab);
            } else if (button == "add") {
                InteractionFactory.AddAllLinksTilingChild(tail);
                editCount[tail]++;
            } else if (button == "remove") {
                editCount[tail]++;
                InteractionFactory.RemoveAllLinksTilingChild(tail);
            }
        }
    }

    public void Accept() { }

    public void Cancel() {
        if (isGrabbing) {
            CancelGrab();
        } else if (!isExposed) {
            InteractionController.instance.Transition("grab-mode");
        } else if (mode == EditMode.Edit) {
            EndEdit();
        } else {
            SwitchMode(EditMode.Edit);
        }
    }

    public void Undo() {
        if (!isGrabbing) {
            if (!isExposed) {
                UndoHistory.Undo();
            } else if (editCount[tail] > 0) {
                bool undid = UndoHistory.Undo();
                editCount[tail] -= undid ? 1 : 0;
            }
        }
    }

    public void Redo() {
        if (!isGrabbing) {
            if (!isExposed) {
                UndoHistory.Redo();
            } else {
                bool redid = UndoHistory.Redo();
                editCount[tail] += redid ? 1 : 0;
            }
        }
    }

    public bool Locked() { return isGrabbing; }

    // *****

    void SetGroupUI() {
        int i = mode != EditMode.Edit ? mode - EditMode.GroupGrab + 1 : 0;

        var buttons = new string[] {
            "edit-mode",
            "grab-mode",
            "clone-mode",
            "delete-mode",
        };
        var label = new string[] {
            "[GROUP] Select to edit recursively",
            "[GROUP] Move parts",
            "[GROUP] Add new parts",
            "[GROUP] Delete parts",
        };
        var color = new Color[] { Color.cyan, Color.red, Color.green, Color.black };

        ButtonsController.instance.SwitchUI("GroupEditUI");
        ButtonsController.instance.SelectButton(buttons[i], true);
        GrabLaser.SetLaserColor(color[i]);
        StateTree.instance.SetState(color[i], label[i]);
    }

    void SetRandomUI() {
        int i = mode != EditMode.Edit ? mode - EditMode.RandomGrab + 1 : 0;

        var buttons = new string[] {
            "edit-mode",
            "grab-mode",
            "clone-mode",
            "delete-mode",
        };
        var label = new string[] {
            "[RANDOM] Select to edit recursively",
            "[RANDOM] Move variants",
            "[RANDOM] Add new variants",
            "[RANDOM] Delete variants",
        };
        var color = new Color[] { Color.cyan, Color.red, Color.green, Color.black };

        ButtonsController.instance.SwitchUI("RandomEditUI");
        ButtonsController.instance.SelectButton(buttons[i], true);
        GrabLaser.SetLaserColor(color[i]);
        StateTree.instance.SetState(color[i], label[i]);
    }

    void SetPositionUI() {
        int i = mode != EditMode.Edit ? mode - EditMode.PositionGrab + 1 : 0;

        var buttons = new string[] {
            "edit-mode",
            "grab-mode",
        };
        var label = new string[] {
            "[SHIFTER] Select to edit recursively",
            "[SHIFTER] Adjust allowable box",
        };
        var color = new Color[] { Color.cyan, Color.red };

        ButtonsController.instance.SwitchUI("RandPositionEditUI");
        ButtonsController.instance.SelectButton(buttons[i], true);
        GrabLaser.SetLaserColor(color[i]);
        StateTree.instance.SetState(color[i], label[i]);
    }

    void SetRotationUI() {
        int i = mode != EditMode.Edit ? mode - EditMode.RotationGrab + 1 : 0;

        var buttons = new string[] {
            "edit-mode",
            "grab-mode",
         };
        var label = new string[] {
            "[ROTATOR] Select to edit recursively",
            "[ROTATOR] Adjust angles",
        };
        var color = new Color[] { Color.cyan, Color.red };

        ButtonsController.instance.SwitchUI("RandRotationEditUI");
        ButtonsController.instance.SelectButton(buttons[i], true);
        GrabLaser.SetLaserColor(color[i]);
        StateTree.instance.SetState(color[i], label[i]);
    }

    void SetTilingUI() {
        Debug.Assert(tailTiling);
        Debug.Assert(mode == EditMode.Edit || mode == EditMode.TilingGrab);
        int i = mode != EditMode.Edit ? mode - EditMode.TilingGrab + 1 : 0;

        var buttons = new string[] {
            "edit-mode",
            "grab-mode",
        };
        var label = new string[] {
            "[TILING] Select to edit recursively",
            "[TILING] Adjust tiling range / rotation",
        };
        var color = new Color[] { Color.cyan, Color.red };

        ButtonsController.instance.SwitchUI("TilingEditUI");
        ButtonsController.instance.SelectButton(buttons[i], true);
        GrabLaser.SetLaserColor(color[i]);
        StateTree.instance.SetState(color[i], label[i]);
    }

    void SetClearUI() {
        Debug.Assert(!isExposed);
        ButtonsController.instance.SwitchUI("MainUI");
        ButtonsController.instance.SelectButton("edit-mode", true);
        GrabLaser.SetLaserColor(Color.cyan);
        StateTree.instance.SetState(Color.cyan, "Select entity to edit");
    }

    bool ClearOldUI() => mode switch {
        EditMode.Edit => ButtonsController.instance.SelectButton("edit-mode", false),
        EditMode.GroupGrab => ButtonsController.instance.SelectButton("grab-mode", false),
        EditMode.GroupAdd => ButtonsController.instance.SelectButton("clone-mode", false),
        EditMode.GroupDelete => ButtonsController.instance.SelectButton("delete-mode", false),
        EditMode.RandomGrab => ButtonsController.instance.SelectButton("grab-mode", false),
        EditMode.RandomAdd => ButtonsController.instance.SelectButton("clone-mode", false),
        EditMode.RandomDelete => ButtonsController.instance.SelectButton("delete-mode", false),
        EditMode.PositionGrab => ButtonsController.instance.SelectButton("grab-mode", false),
        EditMode.RotationGrab => ButtonsController.instance.SelectButton("grab-mode", false),
        EditMode.TilingGrab => ButtonsController.instance.SelectButton("grab-mode", false),
        _ => false,
    };

    void SetNewUI() {
        if (!isExposed) {
            SetClearUI();
        } else if (tailGroup) {
            SetGroupUI();
        } else if (tailRandom) {
            SetRandomUI();
        } else if (tailPosition) {
            SetPositionUI();
        } else if (tailRotation) {
            SetRotationUI();
        } else if (tailTiling) {
            SetTilingUI();
        }
    }

    void SwitchMode(EditMode newMode) {
        Debug.Assert(!isGrabbing);
        ClearOldUI();
        mode = newMode;
        SetNewUI();
    }

    // ***** Recursion

    void StartEdit(Transform target) {
        editCount.Add(target, 0);
        editStack.AddLast(target);
        tail.GetComponent<Procedural>().ShowAugmentation();
        SetNewUI();
    }

    void RecurseEdit(Transform target) {
        tail.GetComponent<Procedural>()?.HideAugmentation();
        editCount.Add(target, 0);
        editStack.AddLast(target);
        tail.GetComponent<Procedural>().ShowAugmentation();
        SetNewUI();
    }

    void EndEdit() {
        tail.GetComponent<Procedural>().HideAugmentation();
        editCount.Remove(tail);
        editStack.RemoveLast();
        tail?.GetComponent<Procedural>().ShowAugmentation();
        SetNewUI();
    }

    // ***** Grab Operations

    void ReleaseGrab() {
        if (mode == EditMode.GroupGrab) {
            InteractionFactory.ReleaseGroupMoveGrab(tail);
        } else if (mode == EditMode.GroupAdd) {
            InteractionFactory.ReleaseGroupCloneGrab(tail);
        } else if (mode == EditMode.RandomGrab) {
            InteractionFactory.ReleaseRandomMoveGrab(tail);
        } else if (mode == EditMode.RandomAdd) {
            InteractionFactory.ReleaseRandomCloneGrab(tail);
        } else if (mode == EditMode.PositionGrab) {
            InteractionFactory.ReleasePositionGrab(tail);
        } else if (mode == EditMode.RotationGrab) {
            InteractionFactory.ReleaseRotationGrab(tail);
        } else if (mode == EditMode.TilingGrab) {
            InteractionFactory.ReleaseTilingGrab(tail);
        }
        editCount[tail]++;
    }

    void CancelGrab() {
        if (mode == EditMode.GroupGrab) {
            InteractionFactory.CancelGroupMoveGrab();
        } else if (mode == EditMode.GroupAdd) {
            InteractionFactory.CancelGroupCloneGrab();
        } else if (mode == EditMode.RandomGrab) {
            InteractionFactory.CancelRandomMoveGrab();
        } else if (mode == EditMode.RandomAdd) {
            InteractionFactory.CancelRandomCloneGrab();
        } else if (mode == EditMode.PositionGrab) {
            InteractionFactory.CancelPositionGrab();
        } else if (mode == EditMode.RotationGrab) {
            InteractionFactory.CancelRotationGrab();
        } else if (mode == EditMode.TilingGrab) {
            InteractionFactory.CancelTilingGrab();
        }
    }

    // *****

    bool IsGroup(Transform transform) {
        return transform.GetComponent<Procedural>() is ProceduralGroup;
    }
    bool IsRandom(Transform transform) {
        return transform.GetComponent<Procedural>() is ProceduralRandom;
    }
    bool IsPosition(Transform transform) {
        return transform.GetComponent<Procedural>() is ProceduralPosition;
    }
    bool IsRotation(Transform transform) {
        return transform.GetComponent<Procedural>() is ProceduralRotation;
    }
    bool IsTiling(Transform transform) {
        return transform.GetComponent<Procedural>() is ProceduralTiling;
    }

}
#endif
