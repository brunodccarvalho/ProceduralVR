#if false
using UnityEngine;

public class UnlinkInteraction : InteractionStrategy {

    public void Start() {
        // * Do not change active buttons
        ButtonsController.instance.SelectButton("unlink-mode", true);
        GrabLaser.SetLaserColor(Color.white);
        StateTree.instance.SetState(Color.white, "Unlinking");
    }

    public void Close() {
        ButtonsController.instance.SelectButton("unlink-mode", false);
    }

    public void GrabDown(Transform target, GrabSource source) {
        Interactive config = target.GetComponent<Interactive>();
        Procedural proc = config as Procedural;
        if (config.clickable) {
            config.Click();
            return;
        }
        if (config.procedural) {
            InteractionFactory.TryUnlinkTarget(target);
            return;
        }
    }

    public void GrabUp(GrabSource source) { }

    public void ClickButton(string button) {
        if (Locked()) {
            return;
        }
        if (button == "unlink-mode") {
            InteractionController.instance.Transition("grab-mode");
        } else if (InteractionController.instance.IsStrategyButton(button)) {
            InteractionController.instance.Transition(button);
        }
    }

    public void Accept() { }

    public void Cancel() {
        InteractionController.instance.Transition("grab-mode");
    }

    public void Undo() {
        UndoHistory.Undo();
    }

    public void Redo() {
        UndoHistory.Redo();
    }

    public bool Locked() { return false; }

}
#endif
