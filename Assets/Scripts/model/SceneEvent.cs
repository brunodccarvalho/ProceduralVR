using System.Collections.Generic;
using UnityEngine;

public class SceneEvent {
    public SceneEventType type;
    public string message;
    public List<Transform> affected; // list of affected interactives

    public SceneEvent(SceneEventType type, string message, IEnumerable<Transform> affected = null) {
        this.type = type;
        this.message = message;
        if (affected == null) {
            this.affected = new List<Transform>();
        } else {
            this.affected = new List<Transform>(affected);
        }
    }

    public SceneEvent(SceneEventType type, string message, Transform affected)
        : this(type, message, affected == null ? null : new List<Transform> { affected }) { }

    public SceneEvent(SceneEventType type, IEnumerable<Transform> affected = null)
        : this(type, null, affected) { }

    public SceneEvent(SceneEventType type, Transform affected)
        : this(type, null, affected == null ? null : new List<Transform> { affected }) { }

    public override string ToString() {
        var list = affected != null ? string.Join("\n", affected.ConvertAll(t => t.name)) : "";
        var s = message != null ? message : "";
        return string.Format("{0}: {1} [{2}]", type, s, list);
    }

}
