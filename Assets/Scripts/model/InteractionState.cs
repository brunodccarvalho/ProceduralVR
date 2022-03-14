using System.Collections.Generic;

public class InteractionState {
    public PMode mode;
    public LinkedListNode<UndoableAction> undoBegin, undoEnd;
    public Procedural tail;

    public InteractionState(PMode mode) {
        this.mode = mode;
        this.undoBegin = this.undoEnd = null;
        this.tail = null;
    }

    public InteractionState(PMode mode, Procedural tail) {
        this.mode = mode;
        this.undoBegin = this.undoEnd = UndoHistory.current.CurrentPointer();
        this.tail = tail;
    }
}
