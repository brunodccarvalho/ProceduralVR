using System.Collections.Generic;
using System.Text;

/**
 * Action aggregator, encapsulates an ordered group of actions as a single action
 * for UndoHistory.
 */
[System.Serializable]
public class MultiUndoableAction : UndoableAction {
    public List<UndoableAction> actions;
    public int Count => actions.Count;

    public MultiUndoableAction() {
        actions = new List<UndoableAction>();
    }

    public MultiUndoableAction(List<UndoableAction> actions) {
        this.actions = actions;
    }

    public void Add(UndoableAction action) {
        actions.Add(action);
    }

    public void Clear() { actions.Clear(); }

    public void Undo() {
        for (int L = actions.Count, i = L - 1; i >= 0; i--) {
            actions[i].Undo();
        }
    }

    public void Redo() {
        for (int L = actions.Count, i = 0; i < L; i++) {
            actions[i].Redo();
        }
    }

    public void Commit() {
        for (int L = actions.Count, i = 0; i < L; i++) {
            actions[i].Commit();
        }
    }

    public void Forget() {
        for (int L = actions.Count, i = L - 1; i >= 0; i--) {
            actions[i].Forget();
        }
    }

    public override string ToString() {
        StringBuilder builder = new StringBuilder("MultiUndoable {\n");
        var children = actions.ConvertAll(each => each.ToString());
        for (int i = 0, S = children.Count; i < S; i++) {
            var lines = children[i].Split('\n');
            for (int j = 0, L = lines.Length; j < L; j++) {
                lines[j] = "   " + lines[j];
            }
            children[i] = string.Join("\n", lines);
        }
        builder.AppendLine(string.Join("\n", children));
        builder.Append("}");
        return builder.ToString();
    }
}
