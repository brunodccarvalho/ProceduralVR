using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Interactive : MonoBehaviour {

    [Header("Type of interactive")]

    [Tooltip("Specify whether this object should show highlights on hover")]
    public bool highlightable = true;

    [Tooltip("Specify whether this object is a clickable button")]
    public bool clickable = false;

    [Tooltip("Specify if this interactive is a procedural")]
    public bool procedural = true;

    [Tooltip("Specify if this interactive is an augmentation")]
    public bool augmentation = false;

    RecursiveOutline outline;
    protected Highlight highlight = new Highlight();
    SceneEvent lastEvent = null;
    public int color => highlight.color;

    public bool isHovered => highlight.hovered;

    protected virtual void Start() {
        highlight.hovered = false;
        outline = this.gameObject.GetComponent<RecursiveOutline>();
        if (outline == null) {
            outline = this.gameObject.AddComponent<RecursiveOutline>();
        }
    }

    protected virtual void FixedUpdate() {
        AdjustOutline();
    }

    protected virtual void OnDestroy() {
        if (highlight.hovered) {
            InputController.instance.ClearCollision(this.transform);
            Debug.Assert(!highlight.hovered);
        }
    }

    protected virtual void OnDisable() {
        if (highlight.hovered) {
            InputController.instance.ClearCollision(this.transform);
            Debug.Assert(!highlight.hovered);
        }
    }

    // ***** Hover updates and outline refresh

    private static Dictionary<Transform, Highlight> state = new Dictionary<Transform, Highlight>();

    void AdjustOutline() {
        if (gameObject.activeInHierarchy && IsRootInteractive(this.transform)) {
            ComputeHoverDfs(this.transform);
            AdjustOutlineDfs(this.transform, new Highlight());
        }
    }

    static void ComputeHoverDfs(Transform node) {
        var self = node.GetComponent<Interactive>();
        Highlight highlight = self ? self.highlight : new Highlight();
        foreach (Transform child in node.transform) {
            if (child.gameObject.activeSelf) {
                ComputeHoverDfs(child);
                highlight.hovered |= state[child].hovered;
                highlight.exposed |= state[child].exposed;
            }
        }
        highlight.hovered &= !highlight.exposed;
        state.Remove(node);
        state.Add(node, highlight);
    }

    static void AdjustOutlineDfs(Transform node, Highlight carry) {
        carry += state[node];
        var self = node.GetComponent<Interactive>();
        if (self) {
            carry.hovered &= self.highlightable;
            self.outline?.SetVariant(carry.GetVariant());
        } else {
            var outline = node.GetComponent<RecursiveOutline>();
            outline?.SetVariant(carry.GetVariant());
        }
        bool anyExposed = false;
        foreach (Transform child in node.transform) {
            if (!child.gameObject.activeSelf) continue;
            anyExposed |= state[child].exposed;
        }
        foreach (Transform child in node.transform) {
            if (child.gameObject.activeSelf) {
                Highlight childCarry = carry;
                childCarry.exposed &= !anyExposed || state[child].exposed;
                AdjustOutlineDfs(child, childCarry);
            }
        }
    }

    // ***** Go up the interactive hierarchy

    public static bool IsAugmentation(Transform transform) {
        var inter = transform.GetComponent<Interactive>();
        return inter != null && inter.augmentation;
    }

    public static bool IsButton(Transform transform) {
        var inter = transform.GetComponent<Interactive>();
        return inter != null && inter.clickable;
    }

    public static bool IsRootInteractive(Transform transform) {
        return GetFirst(transform.parent) == null;
    }

    // Get the deepest interactive object above target
    public static Interactive GetFirst(Transform target) {
        while (target != null && target.GetComponent<Interactive>() == null) {
            target = target.parent;
        }
        return target?.GetComponent<Interactive>();
    }

    // Get the shallowest interactive object above target that is not exposed
    public static Interactive GetLastUnexposed(Transform target) {
        Interactive curr = null;
        while (target != null) {
            var conf = target.GetComponent<Interactive>();
            if (conf != null) {
                if (conf.highlight.exposed) {
                    break;
                } else {
                    curr = conf;
                }
            }
            target = target.parent;
        }
        return curr;
    }

    // Get the shallowest interactive object above target, exposed or not
    public static Interactive GetLast(Transform target) {
        target = GetFirst(target)?.transform;
        while (target?.parent?.GetComponent<Interactive>() != null) {
            target = target.parent;
        }
        return target?.GetComponent<Interactive>();
    }

    // ***** On state update handlers for delegate sets

    public static void BeginHover(Transform target) {
        GetFirst(target).highlight.hovered = true;
    }
    public static void EndHover(Transform target) {
        GetFirst(target).highlight.hovered = false;
    }
    public static void BeginExpose(Transform target) {
        GetFirst(target).highlight.hovered = false;
        GetFirst(target).highlight.exposed = true;
    }
    public static void EndExpose(Transform target) {
        GetFirst(target).highlight.exposed = false;
    }
    public static void BeginSelect(Transform target) {
        GetFirst(target).highlight.selected = true;
    }
    public static void EndSelect(Transform target) {
        GetFirst(target).highlight.selected = false;
    }
    public static void BeginGrab(Transform target) {
        GetFirst(target).highlight.grabbed = true;
    }
    public static void EndGrab(Transform target) {
        GetFirst(target).highlight.grabbed = false;
    }
    public static void BeginSlaveGrab(Transform target) {
        GetFirst(target).highlight.slaveGrabbed = true;
    }
    public static void EndSlaveGrab(Transform target) {
        GetFirst(target).highlight.slaveGrabbed = false;
    }
    public static void EndColor(Transform target) {
        GetFirst(target).highlight.color = 0;
    }

    public static void StartEvent(Transform target, SceneEvent evt) {
        var proc = target.GetComponent<Interactive>();
        if (proc != null) {
            proc.lastEvent = evt;
            proc.highlight.color = SceneEvents.EventColor(evt.type).Item1;
        }
    }
    public static void EndEvent(Transform target, SceneEvent evt) {
        var proc = target.GetComponent<Interactive>();
        if (proc != null && proc.lastEvent == evt) {
            proc.lastEvent = null;
            proc.highlight.color = 0;
        }
    }

    // ***** Hand-made updates

    public void ClearHover() {
        InputController.instance.ClearCollision(this.transform);
    }

    public void Expose(bool value) => highlight.exposed = value;
    public void Hover(bool value) => highlight.hovered = value;
    public void Select(bool value) => highlight.selected = value;
    public void Grab(bool value) => highlight.grabbed = value;
    public void SlaveGrab(bool value) => highlight.slaveGrabbed = value;
    public void Color(int color) => highlight.color = color;

    public virtual void Click() { }

}
