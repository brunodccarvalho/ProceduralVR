#if false
using UnityEngine;

public class CreateGroupInteraction : InteractionStrategy {

    DelegateLinkedSet<Transform> selection;

    public CreateGroupInteraction() {
        selection = new DelegateLinkedSet<Transform>();
        selection.OnInsert.AddListener(Interactive.BeginSelect);
        selection.OnRemove.AddListener(Interactive.EndSelect);
    }

    public void Start() {
        // * Do not change active buttons
        ButtonsController.instance.SelectButton("group-mode", true);
        GrabLaser.SetLaserColor(Color.blue);
        StateTree.instance.SetState(Color.blue, "Creating groups");
    }

    public void Close() {
        selection.Clear();
        ButtonsController.instance.SelectButton("group-mode", false);
    }

    public void GrabDown(Transform target, GrabSource source) {
        Interactive config = target.GetComponent<Interactive>();
        Procedural proc = config as Procedural;
        if (config.clickable) {
            config.Click();
            return;
        }
        if (config.procedural) {
            InteractionFactory.ToggleGroupSelection(selection, target);
            return;
        }
    }

    public void GrabUp(GrabSource source) { }

    public void ClickButton(string button) {
        if (Locked()) {
            return;
        }
        if (button == "group-mode") {
            InteractionController.instance.Transition("grab-mode");
        } else if (InteractionController.instance.IsStrategyButton(button)) {
            InteractionController.instance.Transition(button);
        }
    }

    public void Accept() {
        InteractionFactory.CreateGroupSelection(selection);
    }

    public void Cancel() {
        if (selection.Count > 0) {
            InteractionFactory.CancelGroupSelection(selection);
        } else {
            InteractionController.instance.Transition("grab-mode");
        }
    }

    public void Undo() {
        InteractionFactory.CancelGroupSelection(selection);
        UndoHistory.Undo();
    }

    public void Redo() {
        InteractionFactory.CancelGroupSelection(selection);
        UndoHistory.Redo();
    }

    public bool Locked() { return false; }

}
#endif
