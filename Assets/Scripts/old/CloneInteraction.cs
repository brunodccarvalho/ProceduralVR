#if false
using UnityEngine;

public class CloneInteraction : InteractionStrategy {

    GrabSource grabSource => Grabber.instance.grabSource;
    bool isGrabbing => Grabber.instance.IsGrabbing;

    public void Start() {
        // * Do not change active buttons
        ButtonsController.instance.SelectButton("clone-mode", true);
        GrabLaser.SetLaserColor(Color.green);
        StateTree.instance.SetState(Color.green, "Cloning");
    }

    public void Close() {
        Debug.Assert(!isGrabbing);
        ButtonsController.instance.SelectButton("clone-mode", false);
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
            InteractionFactory.TryStartCloneGrab(target, source);
            return;
        }
    }

    public void GrabUp(GrabSource source) {
        if (isGrabbing && grabSource == source) {
            InteractionFactory.ReleaseCloneGrab();
        }
    }

    public void ClickButton(string button) {
        if (Locked()) {
            return;
        }
        if (button == "clone-mode") {
            InteractionController.instance.Transition("grab-mode");
        } else if (InteractionController.instance.IsStrategyButton(button)) {
            InteractionController.instance.Transition(button);
        }
    }

    public void Accept() { }

    public void Cancel() {
        if (isGrabbing) {
            InteractionFactory.CancelCloneGrab();
        } else {
            InteractionController.instance.Transition("grab-mode");
        }
    }

    public void Undo() {
        if (!isGrabbing) {
            UndoHistory.Undo();
        }
    }

    public void Redo() {
        if (!isGrabbing) {
            UndoHistory.Redo();
        }
    }

    public bool Locked() { return isGrabbing; }

}
#endif
