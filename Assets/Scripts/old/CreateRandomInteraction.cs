#if false
using UnityEngine;

public class CreateRandomInteraction : InteractionStrategy {

    DelegateLinkedSet<Transform> selection;

    public CreateRandomInteraction() {
        selection = new DelegateLinkedSet<Transform>();
        selection.OnInsert.AddListener(Interactive.BeginSelect);
        selection.OnRemove.AddListener(Interactive.EndSelect);
    }

    public void Start() {
        // * Do not change active buttons
        ButtonsController.instance.SelectButton("random-mode", true);
        GrabLaser.SetLaserColor(Color.blue);
        StateTree.instance.SetState(Color.blue, "Creating randoms");
    }

    public void Close() {
        selection.Clear();
        ButtonsController.instance.SelectButton("random-mode", false);
    }

    public void GrabDown(Transform target, GrabSource source) {
        Interactive config = target.GetComponent<Interactive>();
        Procedural proc = config as Procedural;
        if (config.clickable) {
            config.Click();
            return;
        }
        if (config.procedural) {
            InteractionFactory.ToggleRandomSelection(selection, target);
            return;
        }
    }

    public void GrabUp(GrabSource source) { }

    public void ClickButton(string button) {
        if (Locked()) {
            return;
        }
        if (button == "random-mode") {
            InteractionController.instance.Transition("grab-mode");
        } else if (InteractionController.instance.IsStrategyButton(button)) {
            InteractionController.instance.Transition(button);
        }
    }

    public void Accept() {
        InteractionFactory.CreateRandomSelection(selection);
    }

    public void Cancel() {
        if (selection.Count > 0) {
            InteractionFactory.CancelRandomSelection(selection);
        } else {
            InteractionController.instance.Transition("grab-mode");
        }
    }

    public void Undo() {
        InteractionFactory.CancelRandomSelection(selection);
        UndoHistory.Undo();
    }

    public void Redo() {
        InteractionFactory.CancelRandomSelection(selection);
        UndoHistory.Redo();
    }

    public bool Locked() { return false; }

}
#endif
