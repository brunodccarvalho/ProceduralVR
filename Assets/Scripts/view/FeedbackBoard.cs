using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * We want to show three kinds of information:
 * - Latest event message: While the latest event has not expired, display its output
 *   message in the proper color.
 * - Hovered object: While the user is laser-hovering an object or button, display in
 *   yellow (hover color) the properties of that object
 *     Procedural: object type, # children, # links, and parameters
 *     Button: button name and functionality
 * - Edit subject: While the user is editing an object, display in cyan (edit color) the
 *   properties of that object
 *     edit depth, object type, # children, # links, and parameters
 */
public class FeedbackBoard : MonoBehaviour {

    public static FeedbackBoard instance;

    const float offsetMultiplier = 0.02f;
    TextMesh mesh;
    Transform edit;
    SceneEvent lastEvent;
    public Text uifeed;
    public bool seenEvent = false;

    public void Shift(int delta) {
        this.transform.localPosition += delta * offsetMultiplier * Vector3.up;
    }

    void Awake() {
        instance = this;
        if (this.mesh == null) {
            this.mesh = this.transform.GetComponentInChildren<TextMesh>();
        }
        SceneEvents.instance.OnEvent.AddListener(OnEventBegin);
        SceneEvents.instance.OnEventEnd.AddListener(OnEventEnd);
    }

    void Update() {
        var eventText = EventText();
        var hoverText = HoverText();
        var editText = EditText();
        var selectionText = SelectionText();
        List<string> lines = new List<string>();
        if (eventText != null) lines.Add(eventText);
        if (hoverText != null) lines.Add(hoverText);
        if (editText != null) lines.Add(editText);
        if (selectionText != null) lines.Add(selectionText);
        var text = string.Join("\n", lines);

        if (text.Length <= 1 && !seenEvent) {
            text = "Hello there!";
        } else { seenEvent = true; }

        mesh.richText = true;
        mesh.text = text;

        var feedText = "Token: " + MainController.instance.token + "\n";
        if (Scenario.current != null) {
            feedText += Scenario.current.name + "\n";
            var time = Metrics.current.Time;
            if (Scenario.current.IsTracking) {
                feedText += Wrap(string.Format("Tracking {0:0.###}s", time / 1e3), Palette.green);
            } else {
                feedText += Wrap(string.Format("Not tracking {0:0.###}s", time / 1e3), Palette.red);
            }
        }
        uifeed.supportRichText = true;
        uifeed.text = feedText;
    }

    void OnEventBegin(SceneEvent evt) {
        if (evt.message != null && evt.message != "" && evt.type != SceneEventType.Logging) {
            lastEvent = evt;
        }
    }

    void OnEventEnd(SceneEvent evt) {
        if (lastEvent == evt) {
            lastEvent = null;
        }
    }

    string Wrap(string s, Color color) {
        var hex = ColorUtility.ToHtmlStringRGB(color);
        return s == null ? null : string.Format("<color=#{0}>{1}</color>", hex, s);
    }

    string EventText() {
        if (lastEvent != null && lastEvent.message != "") {
            var (_, color) = SceneEvents.EventColor(lastEvent.type);
            return Wrap(lastEvent.message, color);
        } else {
            return null;
        }
    }

    string HoverText() {
        var hover = InputController.instance.currentLaserHovered;
        var interactive = hover?.GetComponent<Interactive>();
        if (interactive == null) {
            return null;
        } else if (interactive.procedural) {
            return (interactive as Procedural).Description();
        } else if (interactive.clickable) {
            return (interactive as InteractiveButton).description;
        } else if (interactive.augmentation) {
            var above = hover.parent;
            var procedural = above.GetComponent<Procedural>();
            if (procedural is ProceduralPosition) {
                return (procedural as ProceduralPosition).GetDescription(hover);
            } else if (procedural is ProceduralRotation) {
                return (procedural as ProceduralRotation).GetDescription(hover);
            } else if (procedural is ProceduralTiling) {
                return (procedural as ProceduralTiling).GetDescription(hover);
            }
        }
        return null;
    }

    string EditText() {
        var edit = Interactor.instance.currentTail;
        int depth = Interactor.instance.editDepth;
        if (edit == null) {
            return null;
        } else {
            var description = edit.GetComponent<Procedural>().Description();
            if (depth > 1) {
                return Wrap(string.Format("(Depth {0}) {1}", depth, description), Palette.cyan);
            } else {
                return Wrap(description, Palette.cyan);
            }
        }
    }

    string SelectionText() {
        var selection = Interactor.instance.selectedProcedurals;
        if (selection.Count == 0) {
            return null;
        } else {
            return Wrap(string.Format("{0} selected", selection.Count), Palette.blue);
        }
    }

}
