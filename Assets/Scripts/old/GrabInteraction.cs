#if false
using UnityEngine;

public class GrabInteraction : InteractionStrategy {

    GrabSource grabSource => Grabber.instance.grabSource;
    bool isGrabbing => Grabber.instance.IsGrabbing;

    public void Start() {
        // * Set active buttons
        ButtonsController.instance.SwitchUI("MainUI");
        ButtonsController.instance.SelectButton("grab-mode", true);
        GrabLaser.SetLaserColor(Color.red);
        StateTree.instance.SetState(Color.red, "Grabbing");
    }

    public void Close() {
        Debug.Assert(!isGrabbing);
        ButtonsController.instance.SelectButton("grab-mode", false);
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
        if (config.procedural && proc.grabbable) {
            InteractionFactory.TryStartPlainGrab(target, source);
            return;
        }
    }

    public void GrabUp(GrabSource source) {
        if (isGrabbing && grabSource == source) {
            InteractionFactory.ReleasePlainGrab();
        }
    }

    public void ClickButton(string button) {
        if (Locked()) {
            return;
        }
        if (InteractionController.instance.IsStrategyButton(button)) {
            InteractionController.instance.Transition(button);
        }
    }

    public void Accept() { }

    public void Cancel() {
        if (isGrabbing) {
            InteractionFactory.CancelPlainGrab();
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
