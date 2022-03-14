using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.Events;

public class SceneEvents : MonoBehaviour {

    public static SceneEvents instance;

    public float eventInterval = 3000;
    public UnityEvent<SceneEvent> OnEvent;
    public UnityEvent<SceneEvent> OnEventEnd;

    Dictionary<SceneEvent, Timer> events;
    private ConcurrentQueue<SceneEvent> removalQueue;

    SceneEvents() {
        if (instance != null) Object.Destroy(instance);
        instance = this;

        if (OnEvent == null) OnEvent = new UnityEvent<SceneEvent>();
        if (OnEventEnd == null) OnEventEnd = new UnityEvent<SceneEvent>();

        this.events = new Dictionary<SceneEvent, Timer>();
        this.removalQueue = new ConcurrentQueue<SceneEvent>();
    }

    void Update() {
        SceneEvent evt;
        while (removalQueue.TryDequeue(out evt)) {
            var timer = events[evt];
            OnEventEnd.Invoke(evt);
            events.Remove(evt);
            try { timer.Dispose(); } catch { }
            foreach (Transform t in evt.affected) {
                if (t == null) continue;
                Interactive.EndEvent(t, evt);
            }
        }
    }

    public void AddEvent(SceneEvent evt) {
        Timer timer = new Timer(eventInterval);
        events.Add(evt, timer);
        OnEvent.Invoke(evt);
        timer.Elapsed += (e, args) => removalQueue.Enqueue(evt);
        timer.Enabled = true;
        foreach (Transform t in evt.affected) {
            if (t == null) continue;
            Interactive.StartEvent(t, evt);
        }
    }

    public static (int, Color) EventColor(SceneEventType type) {
        switch (type) {
            case SceneEventType.Info: return (10, Palette.white);
            case SceneEventType.Warning: return (11, Palette.orange);
            case SceneEventType.Error: return (12, Palette.lightred);
            case SceneEventType.InternalError: return (13, Palette.lightred);
            case SceneEventType.UndoRedo: return (1, Palette.gray);
            case SceneEventType.Delete: return (8, Palette.black);
            case SceneEventType.Unlink:
            case SceneEventType.Disband: return (6, Palette.white);
            case SceneEventType.Randomize: return (7, Palette.green);
            case SceneEventType.CreateGroup:
            case SceneEventType.CreateRandom:
            case SceneEventType.CreatePosition:
            case SceneEventType.CreateRotation:
            case SceneEventType.CreateTiling: return (5, Palette.blue);
            case SceneEventType.DeleteChild:
            case SceneEventType.Cycle:
            case SceneEventType.Refresh:
            case SceneEventType.AddChild:
            case SceneEventType.RemoveChild: return (2, Palette.cyan);
            case SceneEventType.AddSelection:
            case SceneEventType.RemoveSelection:
            default: return (0, Palette.white);
        }
    }

    // ***** Event constructors

    private static int CountEnum(IEnumerable<Transform> ts) {
        int c = 0;
        foreach (Transform t in ts) c++;
        return c;
    }

    public static void Logging(string message, Transform affected = null) {
        var e = new SceneEvent(SceneEventType.Logging, message, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void Logging(string message, IEnumerable<Transform> affected) {
        var e = new SceneEvent(SceneEventType.Logging, message, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void Info(string message, IEnumerable<Transform> affected = null) {
        var e = new SceneEvent(SceneEventType.Info, message, affected);
        SceneEvents.instance.AddEvent(e);
    }
    public static void Info(string message, Transform affected) {
        var e = new SceneEvent(SceneEventType.Info, message, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void Warn(string message, IEnumerable<Transform> affected = null) {
        var e = new SceneEvent(SceneEventType.Warning, message, affected);
        SceneEvents.instance.AddEvent(e);
    }
    public static void Warn(string message, Transform affected) {
        var e = new SceneEvent(SceneEventType.Warning, message, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void Error(string message, IEnumerable<Transform> affected = null) {
        var e = new SceneEvent(SceneEventType.Error, message, affected);
        SceneEvents.instance.AddEvent(e);
    }
    public static void Error(string message, Transform affected) {
        var e = new SceneEvent(SceneEventType.Warning, message, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void InternalError(string message, IEnumerable<Transform> affected = null) {
        var e = new SceneEvent(SceneEventType.InternalError, message, affected);
        SceneEvents.instance.AddEvent(e);
    }
    public static void InternalError(string message, Transform affected) {
        var e = new SceneEvent(SceneEventType.Warning, message, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void UndoRedo(List<Transform> affected = null) {
        var e = new SceneEvent(SceneEventType.UndoRedo, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void Delete(Transform deleted) {
        var description = deleted.GetComponent<Procedural>().Description(true);
        var m = "Deleted " + description;
        var e = new SceneEvent(SceneEventType.Delete, m, deleted);
        SceneEvents.instance.AddEvent(e);
    }

    public static void Randomize(Transform affected) {
        var e = new SceneEvent(SceneEventType.Randomize, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void Unlink(Transform affected) {
        var description = affected.GetComponent<Procedural>().Description(true);
        var m = "Unlinked " + description;
        var e = new SceneEvent(SceneEventType.Unlink, m, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void Disband(Transform parent, IEnumerable<Transform> affected) {
        var description = parent.GetComponent<Procedural>().Description(true);
        var m = "Disbanded " + description;
        var e = new SceneEvent(SceneEventType.Disband, m, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void AddSelection(Transform affected) {
        var description = affected.GetComponent<Procedural>().Description(true);
        var m = "Added " + description;
        var e = new SceneEvent(SceneEventType.AddSelection, m, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void RemoveSelection(Transform affected) {
        var description = affected.GetComponent<Procedural>().Description(true);
        var m = "Removed " + description;
        var e = new SceneEvent(SceneEventType.RemoveSelection, m, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void CancelSelection(IEnumerable<Transform> affected) {
        var m = string.Format("Cleared selection ({0})", CountEnum(affected));
        var e = new SceneEvent(SceneEventType.CancelSelection, m, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void CreateGroup(Transform affected) {
        var description = affected.GetComponent<Procedural>().Description();
        var m = "Created " + description;
        var e = new SceneEvent(SceneEventType.CreateGroup, m, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void CreateRandom(Transform affected) {
        var description = affected.GetComponent<Procedural>().Description();
        var m = "Created " + description;
        var e = new SceneEvent(SceneEventType.CreateRandom, m, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void CreatePosition(Transform affected) {
        var description = affected.GetComponent<Procedural>().Description();
        var m = "Created " + description;
        var e = new SceneEvent(SceneEventType.CreatePosition, m);
        SceneEvents.instance.AddEvent(e);
    }

    public static void CreateRotation(Transform affected) {
        var description = affected.GetComponent<Procedural>().Description();
        var m = "Created " + description;
        var e = new SceneEvent(SceneEventType.CreateRotation, m, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void CreateTiling(Transform affected) {
        var description = affected.GetComponent<Procedural>().Description();
        var m = "Created " + description;
        var e = new SceneEvent(SceneEventType.CreateTiling, m, affected);
        SceneEvents.instance.AddEvent(e);
    }

    public static void DeleteChild(Transform parent) {
        var e = new SceneEvent(SceneEventType.DeleteChild, parent);
        SceneEvents.instance.AddEvent(e);
    }

    public static void Cycle(Transform random) {
        var e = new SceneEvent(SceneEventType.Cycle, random);
        SceneEvents.instance.AddEvent(e);
    }

    public static void Refresh(Transform procedural) {
        var e = new SceneEvent(SceneEventType.Refresh, procedural);
        SceneEvents.instance.AddEvent(e);
    }

    public static void AddChild(Transform tiling) {
        var e = new SceneEvent(SceneEventType.AddChild, tiling);
        SceneEvents.instance.AddEvent(e);
    }

    public static void RemoveChild(Transform tiling) {
        var e = new SceneEvent(SceneEventType.RemoveChild, tiling);
        SceneEvents.instance.AddEvent(e);
    }

}
