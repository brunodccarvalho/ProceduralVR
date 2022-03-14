#if false
using UnityEngine;

public class CreatePositionInteraction : InteractionStrategy {

    public void Start() {
        // * Do not change active buttons
        ButtonsController.instance.SelectButton("randposition-mode", true);
        GrabLaser.SetLaserColor(Color.blue);
        StateTree.instance.SetState(Color.blue, "Creating position randomizers");
    }

    public void Close() {
        ButtonsController.instance.SelectButton("randposition-mode", false);
    }

    public void GrabDown(Transform target, GrabSource source) {
        Interactive config = target.GetComponent<Interactive>();
        Procedural proc = config as Procedural;
        if (config.clickable) {
            config.Click();
            return;
        }
        if (config.procedural) {
            InteractionFactory.TryCreateRandomPosition(target);
            return;
        }
    }

    public void GrabUp(GrabSource source) { }

    public void ClickButton(string button) {
        if (Locked()) {
            return;
        }
        if (button == "randposition-mode") {
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
