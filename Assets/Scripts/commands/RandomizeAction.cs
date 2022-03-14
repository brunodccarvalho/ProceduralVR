using UnityEngine;

[System.Serializable]
public struct RandomizeAction : UndoableAction {
    public ProceduralRandom random;
    public Transform previous, next;

    // * OK: Idempotent, applies immediately
    public RandomizeAction(ProceduralRandom random, Transform previous, Transform next) {
        this.random = random;
        this.previous = previous;
        this.next = next;
        Redo();
    }

    public void Undo() {
        random.activeChild?.gameObject.SetActive(false);
        previous?.gameObject.SetActive(true);
        next?.gameObject.SetActive(false);
        random.activeChild = previous;
    }

    public void Redo() {
        random.activeChild?.gameObject.SetActive(false);
        previous?.gameObject.SetActive(false);
        next?.gameObject.SetActive(true);
        random.activeChild = next;
    }

    public void Commit() { }

    public void Forget() { }

    public override string ToString() {
        string before = previous ? previous.name : "none";
        string after = next ? next.name : "none";
        return string.Format("Randomize [{0}] {1}->{2}", random.name, before, after);
    }

}
