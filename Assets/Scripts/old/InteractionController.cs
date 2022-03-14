#if false
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InteractionController : MonoBehaviour {

    public static InteractionController instance;

    Dictionary<string, InteractionStrategy> graph;
    string currentStrategy;
    InteractionStrategy node => currentStrategy != null ? graph[currentStrategy] : null;

    InteractionController() {
        Debug.Assert(instance == null);
        instance = this;
    }

    void Awake() {
        graph = new Dictionary<string, InteractionStrategy>();
        graph.Add("grab-mode", new GrabInteraction());
        graph.Add("group-mode", new CreateGroupInteraction());
        graph.Add("randposition-mode", new CreatePositionInteraction());
        graph.Add("randrotation-mode", new CreateRotationInteraction());
        graph.Add("random-mode", new CreateRandomInteraction());
        graph.Add("tiling-mode", new CreateTilingInteraction());
        graph.Add("clone-mode", new CloneInteraction());
        graph.Add("delete-mode", new DeleteInteraction());
        graph.Add("randomize-mode", new RandomizeInteraction());
        graph.Add("unlink-mode", new UnlinkInteraction());
        graph.Add("disband-mode", new DisbandInteraction());
        graph.Add("edit-mode", new EditInteraction());
    }

    void Start() {
        Transition("grab-mode");
        HintsController.instance.ShowUndoRedoHints();
    }

    public bool IsStrategyButton(string button) {
        return graph.ContainsKey(button);
    }

    public void Toggle(string button) {
        if (!node.Locked()) {
            if (Grabber.instance.IsGrabLockButton(button)) {
                Grabber.instance.ToggleLock(button);
            } else if (button == "emptytoggle") {
                ProceduralEmpty.ToggleStateAll();
            }
        }
    }

    public void Transition(string nextStrategy) {
        Debug.LogFormat("Transition {0} -> {1}", currentStrategy, nextStrategy);
        if (currentStrategy != null && currentStrategy == nextStrategy) {
            Debug.LogErrorFormat("Attempt to self-transition {0}", nextStrategy);
        } else if (currentStrategy != null && node.Locked()) {
            Debug.LogErrorFormat("Attempt to transition out of locked state");
        } else {
            node?.Close();
            currentStrategy = nextStrategy;
            node.Start();
        }
    }

    internal void ClickButton(string button) {
        Debug.LogFormat("ClickButton {0}", button);
        node.ClickButton(button);
    }

    internal void GrabDown(Transform transform, GrabSource source) {
        Debug.LogFormat("GrabDown {0} {1}", source, transform.name);
        node.GrabDown(transform, source);
    }

    internal void GrabUp(GrabSource source) {
        Debug.LogFormat("GrabUp {0}", source);
        node.GrabUp(source);
    }

    internal void Undo() {
        Debug.LogFormat("Undo");
        node.Undo();
    }

    internal void Redo() {
        Debug.LogFormat("Redo");
        node.Redo();
    }

    internal void Accept() {
        Debug.LogFormat("Accept");
        node.Accept();
    }

    internal void Cancel() {
        Debug.LogFormat("Cancel");
        node.Cancel();
    }

}
#endif
