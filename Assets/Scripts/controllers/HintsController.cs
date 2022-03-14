using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

[DisallowMultipleComponent]
public class HintsController : MonoBehaviour {

    public static HintsController instance;
    public bool showControllers = true;

    Dictionary<Transform, bool> looking = new Dictionary<Transform, bool>();
    bool updateShowControllers = true;

    HintsController() {
        if (instance != null) Object.Destroy(instance);
        instance = this;
    }

    void FixedUpdate() {
        UpdateNewLooking();
        UpdateShowControllers();
        UpdateControllerTextHints();
    }

    private void BeginLookAtHand(Transform hand) {
        looking[hand] = true;
    }
    private void EndLookAtHand(Transform hand) {
        looking[hand] = false;
    }

    private void UpdateNewLooking() {
        var eye = Player.instance?.headCollider?.transform;

        if (eye != null) {
            foreach (var hand in Player.instance?.hands) {
                if (!looking.ContainsKey(hand.transform)) {
                    looking.Add(hand.transform, false);
                    var from = SeenFromObserver.Add(hand.transform, eye);
                    from.OnActivate.AddListener(BeginLookAtHand);
                    from.OnDeactivate.AddListener(EndLookAtHand);
                }
            }
        }
    }

    private void UpdateShowControllers() {
        if (updateShowControllers) {
            foreach (var hand in Player.instance?.hands) {
                if (showControllers) {
                    hand.ShowController();
                    hand.SetSkeletonRangeOfMotion(Valve.VR.EVRSkeletalMotionRange.WithController);
                } else {
                    hand.HideController();
                    hand.SetSkeletonRangeOfMotion(Valve.VR.EVRSkeletalMotionRange.WithoutController);
                }
            }
        }
    }

    private void UpdateControllerTextHints() {
        string acceptText = null, cancelText = null, undoText = null, redoText = null;

        var currentMode = Interactor.instance.currentMode;
        var selected = Interactor.instance.selectedProcedurals.Count;


        if (MainController.instance.debugMode) {
            acceptText = "Accept";
            cancelText = "Cancel";
            undoText = "Undo";
            redoText = "Redo";
        } else if (Grabber.instance.IsGrabbing) {
            if (currentMode.HasFlag(PMode.EditClone) || currentMode.HasFlag(PMode.GlobalClone)) {
                cancelText = "Cancel Clone";
            } else {
                cancelText = "Cancel Grab";
            }
        } else {
            undoText = "Undo";
            // redoText = "Redo";
            if (currentMode.HasFlag(PMode.CreateGroup)) {
                acceptText = selected > 1 ? "Create Group" : null;
                // cancelText = selected > 0 ? "Clear selection" : null;
            } else if (currentMode.HasFlag(PMode.CreateRandom)) {
                acceptText = selected > 1 ? "Create Random" : null;
                // cancelText = selected > 0 ? "Clear selection" : null;
            } else if (!currentMode.HasFlag(PMode.GlobalGrab)) {
                cancelText = "Return";
            }
        }

        foreach (var hand in Player.instance.hands) {
            if (acceptText != null && looking[hand.transform]) {
                ControllerButtonHints.ShowTextHint(hand, InputController.instance.acceptAction, acceptText);
            } else {
                ControllerButtonHints.HideTextHint(hand, InputController.instance.acceptAction);
            }
            if (cancelText != null && looking[hand.transform]) {
                ControllerButtonHints.ShowTextHint(hand, InputController.instance.cancelAction, cancelText);
            } else {
                ControllerButtonHints.HideTextHint(hand, InputController.instance.cancelAction);
            }
            if (undoText != null && looking[hand.transform]) {
                ControllerButtonHints.ShowTextHint(hand, InputController.instance.undoAction, undoText);
            } else {
                ControllerButtonHints.HideTextHint(hand, InputController.instance.undoAction);
            }
            if (redoText != null && looking[hand.transform]) {
                ControllerButtonHints.ShowTextHint(hand, InputController.instance.redoAction, redoText);
            } else {
                ControllerButtonHints.HideTextHint(hand, InputController.instance.redoAction);
            }
        }
    }

}
