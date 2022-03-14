using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Math = System.Math;
using Stopwatch = System.Diagnostics.Stopwatch;

public class Metrics {
    public static Metrics current, independent = new Metrics();
    public void Activate() => current = this;

    int undos;
    int redos;
    int cancelGrabs;
    int cancelClones;
    Dictionary<PMode, int> grabs = new Dictionary<PMode, int>();
    Dictionary<PMode, int> modes = new Dictionary<PMode, int>();
    Dictionary<PMode, double> grabDuration = new Dictionary<PMode, double>();
    Dictionary<PMode, double> modeDuration = new Dictionary<PMode, double>();
    Dictionary<ProceduralType, int> entities = new Dictionary<ProceduralType, int>();
    Dictionary<ProceduralType, int> largest = new Dictionary<ProceduralType, int>();
    Dictionary<PAction, int> actions = new Dictionary<PAction, int>();
    Stopwatch stopwatch = new Stopwatch();
    double modeTimestamp = 0;
    double grabTimestamp = 0;

    public bool IsTracking => stopwatch.IsRunning;
    public double Time => stopwatch.Elapsed.TotalMilliseconds;

    double TimeMode() => stopwatch.Elapsed.TotalMilliseconds - modeTimestamp;
    double TimeGrab() => stopwatch.Elapsed.TotalMilliseconds - grabTimestamp;
    public void StartGrab() { grabTimestamp = stopwatch.Elapsed.TotalMilliseconds; }
    public void StartMode() { modeTimestamp = stopwatch.Elapsed.TotalMilliseconds; }

    public void AddUndo() => undos++;
    public void AddRedo() => redos++;
    public void CancelGrab() { cancelGrabs++; StartGrab(); }
    public void CancelClone() { cancelClones++; StartGrab(); }

    public void ReleaseGrab(PMode grab) {
        if (!grabs.ContainsKey(grab)) { grabs.Add(grab, 0); grabDuration.Add(grab, 0); }
        grabs[grab]++; grabDuration[grab] += TimeGrab(); StartGrab();
    }
    public void ModeTransition(PMode mode) {
        if (!modes.ContainsKey(mode)) { modes.Add(mode, 0); modeDuration.Add(mode, 0); }
        modes[mode]++; modeDuration[mode] += TimeMode(); StartMode();
    }
    public void AddAction(PAction action) {
        if (!actions.ContainsKey(action)) { actions.Add(action, 0); }
        actions[action]++;
    }
    public void UpdateLargest(Transform transform) {
        var type = transform.GetComponent<Procedural>().proctype;
        if (!type.HasFlag(ProceduralType.Many)) return;
        if (!largest.ContainsKey(type)) largest.Add(type, 0);
        var count = Procedural.CountProceduralChildren(transform, false);
        largest[type] = largest[type] < count ? count : largest[type];
    }
    public void AddCreate(Transform transform) {
        var type = transform.GetComponent<Procedural>().proctype;
        if (!entities.ContainsKey(type)) entities.Add(type, 0);
        entities[type]++; UpdateLargest(transform);
    }

    public override string ToString() {
        int totalGrabs = 0, totalTransitions = 0, totalActions = 0;
        double totalGrabDuration = 0;
        foreach (int count in grabs.Values) totalGrabs += count;
        foreach (int count in modes.Values) totalTransitions += count;
        foreach (int count in actions.Values) totalActions += count;
        foreach (double duration in grabDuration.Values) totalGrabDuration += duration;

        var (A, I) = SubtreeSizeDfs(Scenario.current.root);

        StringBuilder s = new StringBuilder();
        s.AppendFormat("- number of objects: {0} active / {1} inactive\n", A, I);
        s.AppendFormat("- time: {0}ms\n", stopwatch.ElapsedMilliseconds);
        s.AppendFormat("- undos/redos: {0}/{1}\n", undos, redos);
        s.AppendFormat("--- Grabs\n");
        s.AppendFormat("- cancelled grabs/clones: {0}/{1}\n", cancelGrabs, cancelClones);
        s.AppendFormat("- total grabs: {0}\n", totalGrabs);
        s.AppendFormat("- total duration: {0}ms\n", totalGrabDuration);
        foreach (var grab in grabs.Keys) {
            int count = grabs[grab];
            int duration = (int)Math.Round(grabDuration[grab]);
            s.AppendFormat("  - {0}: x{2} {1}ms\n", grab, duration, count);
        }
        s.AppendFormat("--- Modes\n");
        s.AppendFormat("- total transitions: {0}\n", totalTransitions);
        foreach (var mode in modes.Keys) {
            int count = modes[mode];
            int duration = (int)Math.Round(modeDuration[mode]);
            s.AppendFormat("  - {0}: x{2} {1}ms\n", mode, duration, count);
        }
        s.AppendFormat("--- Actions\n");
        s.AppendFormat("- total actions: {0}\n", totalActions);
        foreach (var action in actions.Keys) {
            int count = actions[action];
            s.AppendFormat("  - {0}: x{1}\n", action, count);
        }
        s.AppendFormat("--- Entities\n");
        foreach (var key in entities.Keys) {
            int count = entities[key];
            s.AppendFormat("  - {0}: {1}\n", key, count);
        }
        s.AppendFormat("--- Miscellaneous\n");
        foreach (var key in largest.Keys) {
            int size = largest[key];
            s.AppendFormat("  - {0}: {1} children\n", key, size);
        }

        return s.ToString();
    }

    public bool Start() {
        if (IsTracking) return false;
        stopwatch.Start(); StartGrab(); StartMode(); return true;
    }
    public bool Stop() {
        if (!IsTracking) return false;
        stopwatch.Stop(); return true;
    }

    (int, int) SubtreeSizeDfs(Transform transform) {
        int active = transform.gameObject.activeSelf ? 1 : 0;
        int inactive = transform.gameObject.activeSelf ? 0 : 1;
        foreach (Transform child in transform) {
            var (A, I) = SubtreeSizeDfs(child);
            active += A; inactive += I;
        }
        return (active, inactive);
    }

}
