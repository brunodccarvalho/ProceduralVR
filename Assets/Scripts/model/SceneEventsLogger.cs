using System.Collections.Generic;
using System.Text;

public class SceneEventsLogger {

    public static SceneEventsLogger instance = null;
    public List<(double, SceneEvent)> events = new List<(double, SceneEvent)>();

    void OnStartEvent(SceneEvent evt) { events.Add((Metrics.current.Time / 1e3, evt)); }

    public void Activate() {
        instance?.Deactivate();
        SceneEvents.instance.OnEvent.AddListener(OnStartEvent);
        instance = this;
    }

    public void Deactivate() {
        SceneEvents.instance.OnEvent.RemoveListener(OnStartEvent);
        if (instance == this) instance = null;
    }

    public void Clear() {
        events.Clear();
    }

    public string Format() {
        var builder = new StringBuilder();
        foreach (var (time, evt) in events) {
            builder.AppendLine(string.Format("{0:0.000}s {1}", time, evt.ToString()));
        }
        return builder.ToString();
    }

}
