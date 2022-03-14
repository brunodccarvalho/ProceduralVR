using UnityEngine;

[DisallowMultipleComponent]
public class Interactor : MonoBehaviour {

    public static Interactor instance;

    public DelegateLinkedSet<Transform> selectedProcedurals;
    public DelegateLinkedSet<InteractionState> modeStack;

    public InteractionState currentState => modeStack.Last.Value;
    public PMode currentMode => currentState.mode;
    public Transform currentTail => currentState.tail?.transform;
    public bool IsEditing => currentTail != null;
    public int editDepth => modeStack.Count - 1;

    public Interactor() {
        instance = this;

        selectedProcedurals = new DelegateLinkedSet<Transform>();
        modeStack = new DelegateLinkedSet<InteractionState>();
        modeStack.AddLast(new InteractionState(PMode.GlobalGrab));

        selectedProcedurals.OnInsert.AddListener(Interactive.BeginSelect);
        selectedProcedurals.OnRemove.AddListener(Interactive.EndSelect);
        modeStack.OnInsert.AddListener(state => state.tail?.Expose(true));
        modeStack.OnRemove.AddListener(state => state.tail?.Expose(false));
    }

    public void Reset() {
        while (modeStack.Count > 1) FinishEdit();
        TransitionMode(PMode.GlobalGrab);
    }

    public void TransitionMode(PMode mode) {
        if (Grabber.instance.IsGrabbing) {
            SceneEvents.Warn("Drop first", Grabber.instance.grabTarget);
            return;
        }
        // Verify compatibility for pedanticness
        Debug.Assert(mode.HasFlag(PMode.Global) == currentMode.HasFlag(PMode.Global));
        Debug.Assert(mode.HasFlag(PMode.EditGroup) == currentMode.HasFlag(PMode.EditGroup));
        Debug.Assert(mode.HasFlag(PMode.EditRandom) == currentMode.HasFlag(PMode.EditRandom));
        Debug.Assert(mode.HasFlag(PMode.EditPosition) == currentMode.HasFlag(PMode.EditPosition));
        Debug.Assert(mode.HasFlag(PMode.EditRotation) == currentMode.HasFlag(PMode.EditRotation));
        Debug.Assert(mode.HasFlag(PMode.EditTiling) == currentMode.HasFlag(PMode.EditTiling));

        selectedProcedurals.Clear();
        Metrics.current?.ModeTransition(currentMode);
        currentState.mode = mode;
    }

    public void PerformAction(PAction action) {
        if (Grabber.instance.IsGrabbing) {
            SceneEvents.Warn("Drop first", Grabber.instance.grabTarget);
            return;
        }
        switch (action) {
            case PAction.GroupRandomize:
                InteractionFactory.TryRandomizeTarget(currentTail, true);
                break;
            case PAction.RandomCycle:
                InteractionFactory.CycleRandomChild(currentTail);
                break;
            case PAction.PositionRefresh:
                InteractionFactory.RefreshProceduralPosition(currentTail);
                break;
            case PAction.RotationRefresh:
                InteractionFactory.RefreshProceduralRotation(currentTail);
                break;
            case PAction.TilingAdd:
                InteractionFactory.AddAllLinksTilingChild(currentTail);
                break;
            case PAction.TilingRemove:
                InteractionFactory.RemoveAllLinksTilingChild(currentTail);
                break;
            default:
                // ! INTERNAL ERROR
                Debug.LogErrorFormat("PerformAction: Skipped action {0}", action);
                break;
        }
    }

    public void PerformUserAction(PUserAction action) {
        if (Grabber.instance.IsGrabbing) {
            SceneEvents.Warn("Drop first", Grabber.instance.grabTarget);
            return;
        }
        switch (action) {
            case PUserAction.GrabTogglePlaneLock:
                Grabber.instance.userLocks.TogglePlaneLock();
                break;
            case PUserAction.GrabToggleVerticalLock:
                Grabber.instance.userLocks.ToggleVerticalLock();
                break;
            case PUserAction.GrabToggleRotaxis:
                Grabber.instance.userLocks.ToggleRotaxis();
                break;
            case PUserAction.GrabToggleRotationSnap:
                Grabber.instance.userLocks.ToggleRotationSnap();
                break;
            case PUserAction.GrabToggleGridSnap:
                Grabber.instance.userLocks.ToggleGridSnap();
                break;
            case PUserAction.ToggleVisibleEmpty:
                ProceduralEmpty.ToggleStateAll();
                break;
            case PUserAction.UpscaleScene:
                SceneView.instance.UpscaleSceneBy(1);
                break;
            case PUserAction.DownscaleScene:
                SceneView.instance.DownscaleSceneBy(1);
                break;
            default:
                // ! INTERNAL ERROR
                Debug.LogErrorFormat("PerformUserAction: Skipped action {0}", action);
                break;
        }
    }

    public void Undo() {
        if (Grabber.instance.IsGrabbing) {
            SceneEvents.Warn("Drop first", Grabber.instance.grabTarget);
        } else if (currentMode.HasFlag(PMode.Global)) {
            selectedProcedurals.Clear();
            UndoHistory.current.Undo();
            Metrics.current?.AddUndo();
        } else if (UndoHistory.current.UndoAfter(currentState.undoBegin)) {
            Metrics.current?.AddUndo();
        }
    }

    public void Redo() {
        if (Grabber.instance.IsGrabbing) {
            SceneEvents.Warn("Drop first", Grabber.instance.grabTarget);
        } else if (currentMode.HasFlag(PMode.Global)) {
            selectedProcedurals.Clear();
            UndoHistory.current.Redo();
            Metrics.current?.AddRedo();
        } else if (UndoHistory.current.RedoBefore(currentState.undoEnd)) {
            Metrics.current?.AddRedo();
        }
    }

    public void Accept() {
        if (currentMode == PMode.CreateGroup) {
            InteractionFactory.CreateGroupSelection(selectedProcedurals);
        } else if (currentMode == PMode.CreateRandom) {
            InteractionFactory.CreateRandomSelection(selectedProcedurals);
        }
    }

    public void Cancel() {
        if (Grabber.instance.IsGrabbing) {
            Grabber.instance.CancelGrab();
        } else if (selectedProcedurals.Count > 0) {
            selectedProcedurals.Clear();
        } else if (currentMode.HasFlag(PMode.Global)) {
            TransitionMode(PMode.GlobalGrab);
        } else if (currentMode.HasFlag(PMode.EditWait)) {
            FinishEdit();
        } else if (currentMode.HasFlag(PMode.Local)) {
            var mode = currentMode & ~PMode.EditOp | PMode.EditWait;
            TransitionMode(mode);
        }
    }

    public void GrabDown(Transform target, GrabSource source) {
        Interactive interactive = target.GetComponent<Interactive>();
        if (!target.gameObject.activeInHierarchy) {
            // ! INTERNAL ERROR
            Debug.LogErrorFormat("GrabDown: Tried to grab inactive {0}", target?.name);
            return;
        }
        if (Grabber.instance.IsGrabbing) {
            SceneEvents.Warn("Drop first", Grabber.instance.grabTarget);
            return;
        }
        if (interactive.clickable) {
            interactive.Click();
            return;
        }
        if (interactive.procedural) {
            switch (currentMode) {
                case PMode.GlobalGrab:
                    InteractionFactory.TryStartPlainGrab(target, source);
                    break;
                case PMode.GlobalClone:
                    InteractionFactory.TryStartCloneGrab(target, source);
                    break;
                case PMode.GlobalRandomize:
                    InteractionFactory.TryRandomizeTarget(target);
                    break;
                case PMode.GlobalDelete:
                    InteractionFactory.TryDeleteTarget(target);
                    break;
                case PMode.GlobalUnlink:
                    InteractionFactory.TryUnlinkTarget(target);
                    break;
                case PMode.GlobalDisband:
                    InteractionFactory.TryDisbandTarget(target);
                    break;
                case PMode.CreateGroup:
                    InteractionFactory.ToggleGroupSelection(selectedProcedurals, target);
                    break;
                case PMode.CreateRandom:
                    InteractionFactory.ToggleRandomSelection(selectedProcedurals, target);
                    break;
                case PMode.CreateRandomPosition:
                    if (InteractionFactory.TryCreateRandomPosition(target)) {
                        TryStartEdit(target.parent);
                    }
                    break;
                case PMode.CreateRandomRotation:
                    if (InteractionFactory.TryCreateRandomRotation(target)) {
                        TryStartEdit(target.parent);
                    }
                    break;
                case PMode.CreateTiling:
                    if (InteractionFactory.TryCreateTiling(target)) {
                        TryStartEdit(target.parent);
                    }
                    break;
                case PMode.GlobalEdit:
                case PMode.EditGroupWait:
                case PMode.EditRandomWait:
                case PMode.EditPositionWait:
                case PMode.EditRotationWait:
                case PMode.EditTilingWait:
                    TryStartEdit(target);
                    break;
                case PMode.EditGroupGrab:
                    InteractionFactory.TryStartGroupMoveGrab(currentTail, target, source);
                    break;
                case PMode.EditGroupClone:
                    InteractionFactory.TryStartGroupCloneGrab(currentTail, target, source);
                    break;
                case PMode.EditGroupDelete:
                    InteractionFactory.TryDeleteGroupChild(currentTail, target);
                    break;
                case PMode.EditRandomGrab:
                    InteractionFactory.TryStartRandomMoveGrab(currentTail, target, source);
                    break;
                case PMode.EditRandomClone:
                    InteractionFactory.TryStartRandomCloneGrab(currentTail, target, source);
                    break;
                case PMode.EditRandomDelete:
                    InteractionFactory.TryDeleteRandomChild(currentTail, target);
                    break;
                case PMode.EditPositionGrab:
                    InteractionFactory.TryStartPositionGrab(currentTail, target, source);
                    break;
                case PMode.EditRotationGrab:
                    InteractionFactory.TryStartRotationGrab(currentTail, target, source);
                    break;
                case PMode.EditTilingGrab:
                    InteractionFactory.TryStartTilingGrab(currentTail, target, source);
                    break;
                default:
                    // ! INTERNAL ERROR
                    Debug.LogErrorFormat("GrabDown: Skipped mode {0}", currentMode);
                    break;
            }
            return;
        }
        if (interactive.augmentation) {
            switch (currentMode) {
                case PMode.GlobalGrab:
                case PMode.GlobalClone:
                case PMode.GlobalRandomize:
                case PMode.GlobalDelete:
                case PMode.GlobalUnlink:
                case PMode.GlobalDisband:
                case PMode.CreateGroup:
                case PMode.CreateRandom:
                case PMode.CreateRandomPosition:
                case PMode.CreateRandomRotation:
                case PMode.CreateTiling:
                case PMode.GlobalEdit:
                case PMode.EditGroupWait:
                case PMode.EditGroupGrab:
                case PMode.EditGroupClone:
                case PMode.EditGroupDelete:
                case PMode.EditRandomWait:
                case PMode.EditRandomGrab:
                case PMode.EditRandomClone:
                case PMode.EditRandomDelete:
                    // ! INTERNAL ERROR
                    Debug.LogErrorFormat("GrabDown: Grabbed augment in mode {0}", currentMode);
                    break;
                case PMode.EditPositionWait:
                case PMode.EditRotationWait:
                case PMode.EditTilingWait:
                    SceneEvents.Info("Switch to Grab mode first");
                    break;
                case PMode.EditPositionGrab:
                    InteractionFactory.TryStartPositionGrab(currentTail, target, source);
                    break;
                case PMode.EditRotationGrab:
                    InteractionFactory.TryStartRotationGrab(currentTail, target, source);
                    break;
                case PMode.EditTilingGrab:
                    InteractionFactory.TryStartTilingGrab(currentTail, target, source);
                    break;
                default:
                    // ! INTERNAL ERROR
                    Debug.LogErrorFormat("GrabDown: Skipped mode {0}", currentMode);
                    break;
            }
            return;
        }
        // ! INTERNAL ERROR
        Debug.LogErrorFormat("GrabDown: Not procedural or augmentation {0}", target.name);
    }

    public void GrabUp(GrabSource source) {
        if (Grabber.instance.IsGrabbing && Grabber.instance.grabSource == source) {
            Grabber.instance.ReleaseGrab();
        }
    }

    private void TryStartEdit(Transform target) {
        var procedural = target.GetComponent<Procedural>();
        if (currentMode.HasFlag(PMode.EditWait) && target.parent != currentTail) {
            SceneEvents.Error("To edit an unrelated object go back first", target);
            return;
        }
        if (procedural.proctype.HasFlag(ProceduralType.Singular)) {
            SceneEvents.Info("Not a rule", target);
            return;
        }
        if (!procedural.modifiable || !procedural.groupable) {
            SceneEvents.Info("Can't edit this object", target);
            return;
        }
        InteractionState state = null;
        if (procedural is ProceduralGroup) {
            state = new InteractionState(PMode.EditGroupGrab, procedural);
        } else if (procedural is ProceduralRandom) {
            state = new InteractionState(PMode.EditRandomGrab, procedural);
        } else if (procedural is ProceduralPosition) {
            state = new InteractionState(PMode.EditPositionGrab, procedural);
        } else if (procedural is ProceduralRotation) {
            state = new InteractionState(PMode.EditRotationGrab, procedural);
        } else if (procedural is ProceduralTiling) {
            state = new InteractionState(PMode.EditTilingGrab, procedural);
        } else {
            SceneEvents.Info("Can't edit this object", target);
            return;
        }
        if (state != null) {
            currentTail?.GetComponent<Procedural>().HideAugmentation();
            Metrics.current?.ModeTransition(currentMode);
            modeStack.AddLast(state);
            currentTail?.GetComponent<Procedural>().ShowAugmentation();
        }
    }

    private void FinishEdit() {
        currentTail?.GetComponent<Procedural>().HideAugmentation();
        Metrics.current?.ModeTransition(currentMode);
        modeStack.RemoveLast();
        currentTail?.GetComponent<Procedural>().ShowAugmentation();
    }

}
