using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Valve.VR.InteractionSystem;

[System.Serializable]
public class Scenario {

    public static Scenario current, independent;
    public void Activate() => current = this;

    [Header("Scenario data")]
    public string name;
    public Transform root;
    public float initialZoom = 1.0f;
    public UserGrabLocks locks = new UserGrabLocks();
    public VisibleLock visibleLock = VisibleLock.None;
    public bool useBaseline = false;
    public bool visibleEmpty = false;
    [TextArea] public List<string> goals;

    public bool IsTracking => metrics.IsTracking;
    Metrics metrics = new Metrics();
    UndoHistory history = new UndoHistory();
    SceneEventsLogger logger = new SceneEventsLogger();
    int goalIndex = 0;

    static Scenario() {
        independent = new Scenario();
        independent.name = "Independent";
        independent.Activate();
    }

    public Scenario() { }

    public Scenario(string token, string name) {
        this.name = name;

        var dir = Application.persistentDataPath;
        dir = dir.TrimEnd('/');
        var scene = string.Format("{0}/{1}-{2}.scene.txt", dir, token, name);
        StreamReader reader = new StreamReader(scene);

        // Split header / scene
        var lines = new List<string>(reader.ReadToEnd().Split('\n'));

        // Read the header
        var zoomRegex = new Regex(@"\s*- Zoom: (.+)");
        var goalRegex = new Regex(@"\s*- Goal: (\d+) (.*)");
        var baselineRegex = new Regex(@"\s*- (?:Use)?Baseline: (\d)");
        var visibleEmptyRegex = new Regex(@"\s*- VisibleEmpty: (\d)");
        int p = 0;
        while (p < lines.Count && !lines[p].StartsWith("@")) {
            var line = lines[p++];
            var match = zoomRegex.Match(line);
            if (match.Success) {
                this.initialZoom = float.Parse(match.Groups[1].Value);
                continue;
            }
            match = goalRegex.Match(line);
            if (match.Success) {
                var i = int.Parse(match.Groups[1].Value);
                while (this.goals.Count <= i) this.goals.Add("");
                this.goals[i] = match.Groups[2].Value;
                continue;
            }
            match = baselineRegex.Match(line);
            if (match.Success) {
                this.useBaseline = int.Parse(match.Groups[1].Value) == 1;
                continue;
            }
            match = visibleEmptyRegex.Match(line);
            if (match.Success) {
                this.visibleEmpty = int.Parse(match.Groups[1].Value) == 1;
                ProceduralEmpty.SetStateAll(visibleEmpty);
                continue;
            }
        }

        // Parse the tree structure
        var treeLines = lines.GetRange(p, lines.Count - p).ToArray();
        root = ProceduralFactory.LoadScenarioInplace(treeLines);
        root.name = root.name + " " + "Loaded";
        root.parent = ProceduralFactory.root;
    }

    public void Load() {
        if (current != this) { // idempotency for safety
            Activate();
            Interactor.instance.Reset();
            SceneView.instance.ScaleSceneTo(initialZoom, false);
            history.Activate();
            metrics.Activate();
            logger.Activate();
            Grabber.instance.userLocks = locks;
            ProceduralEmpty.SetStateAll(visibleEmpty);
            Player.instance.transform.position = Vector3.zero;
        }
        root?.gameObject.SetActive(true);
    }

    public void Unload() {
        root?.gameObject.SetActive(false);
        UndoHistory.independent.Activate();
        metrics.Stop();
        logger.Deactivate();
        if (current == this) {
            independent.Activate();
            Metrics.independent.Activate();
            visibleEmpty = ProceduralEmpty.stateVisible;
        }
    }

    public void ResetTest() {
        metrics.Stop();
        metrics = new Metrics();
        metrics.Activate();
        history.UndoAllAndClear();
        ResetGoal();
        Debug.LogFormat("Reset tracking {0}", name);
    }

    public void StartTracking() {
        metrics.Start();
        Debug.LogFormat("Start tracking {0}", name);
    }

    public void StopTracking() {
        metrics.Stop();
        Debug.LogFormat("Stop tracking {0}", name);
    }

    public void FinishTest() {
        metrics.Stop();
        DumpToFile();
        Debug.LogFormat("Finish tracking {0}", name);
    }

    public void Dump() { Debug.Log(this.ToString()); }

    public void DumpToFile() {
        var token = MainController.instance.token;
        var dir = Application.persistentDataPath;
        dir.TrimEnd('/');

        try {
            var metricsText = FormatMetrics();
            var metricsFile = string.Format("{0}/{1}-{2}.metrics.txt", dir, token, name);
            StreamWriter writer = new StreamWriter(metricsFile, false);
            writer.Write(metricsText); writer.Close();
        } catch { }

        try {
            var sceneText = FormatScene();
            var sceneFile = string.Format("{0}/{1}-{2}.scene.txt", dir, token, name);
            StreamWriter writer = new StreamWriter(sceneFile, false);
            writer.Write(sceneText); writer.Close();
        } catch { }

        try {
            var eventsText = FormatEvents();
            var eventsFile = string.Format("{0}/{1}-{2}.events.txt", dir, token, name);
            StreamWriter writer = new StreamWriter(eventsFile, false);
            writer.Write(eventsText); writer.Close();
        } catch { }

        try {
            var historyText = FormatHistory();
            var historyFile = string.Format("{0}/{1}-{2}.history.txt", dir, token, name);
            StreamWriter writer = new StreamWriter(historyFile, false);
            writer.Write(historyText); writer.Close();
        } catch { }
    }

    public void ShowGoal() {
        if (goals != null && goalIndex < goals.Count) {
            Debug.Log(goals[goalIndex]);
            SceneEvents.Info(goals[goalIndex]);
        }
    }

    public void AdvanceGoal() { goalIndex = System.Math.Min(goalIndex + 1, goals.Count); }
    public void RetreatGoal() { goalIndex = System.Math.Max(goalIndex - 1, 0); }
    public void ResetGoal() { goalIndex = 0; }

    public string FormatMetrics() {
        var s = string.Format("=== {0} ({1})\n", name, useBaseline ? "baseline" : "complete");
        return s + metrics.ToString();
    }

    public string FormatScene() {
        var s = string.Format("- Name: {0}\n", name);
        for (int i = 0; i < goals.Count; i++) {
            var t = goals[i].Replace("\n", string.Empty);
            s += string.Format("- Goal: [{0}] {1}\n", i, t);
        }
        s += string.Format("- InitialZoom: {0}\n", initialZoom);
        s += string.Format("- Baseline: {0}\n", useBaseline ? 1 : 0);
        s += string.Format("- VisibleEmpty: {0}\n", visibleEmpty ? 1 : 0);
        s += ProceduralFactory.FormatTree(root);
        return s;
    }

    public string FormatHistory() {
        return history.Format();
    }

    public string FormatEvents() {
        return logger.Format();
    }

}

// Scenario actions:
// - Reset(): roll scenario back to starting state:
//    - Tracked metrics
//    - Goals
//    - History
//    - Don't reset other UI
// - Start() / Stop() tracking
// - Finish()
//    - Dump metrics
//    - Dump scene
//    - Dump history
//    - Dump event log
// - Display current goal
// - Advance/retreat/reset current goal
