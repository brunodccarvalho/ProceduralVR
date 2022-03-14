/**
 * We decided not to use Unity's Undo system, and roll our own due to the complicated
 * reparenting, translation and procedural logic.
 *
 * An undoable (procedural) action modifies the scene model in some reversible way and is
 * always in one of two states:
 *   - Online: the action has been applied, with Redo() or implicitly
 *   - Offline: the action has not been applied or has been undone with Undo()
 * When online, the action can be undone or committed
 * When offline, the action can be redone or aborted
 * If the action is online and there is no further possibility of calling Undo() then Commit() is called.
 * If the action is offline and there is no further possbility of calling Redo() then Forget() is called.
 *
 * All actions are structs which take a constructor(s) that grants enough data to fully
 * encapsulate their operation in Undo() and Redo() in the future.
 *
 * Two actions created at two different times do not commute: if A is created (and applied)
 * before B, then when B is constructed or applied with Redo(), A must be online.
 * Reciprocally, when A is undone, B must be offline.
 *
 * Actions do not represent *things that could be performed* by later calling Redo() and
 * undone with Undo(). The issue with this approach would be that commutativity would be
 * non-trivial, and the state of the scene is non-deterministic upon calling Undo() or Redo().
 * Instead, the philosophy here is: constructing an action immediately performs that
 * action (in some cases it has been performed already), making it online. In the future
 * the action can be undone and redone, as long as the history is made offline/online in
 * the same order the actions were created.
 *
 * Proper ordering is enforced in MultiUndoable and UndoHistory.
 * The undo stack is kept in UndoHistory.
 *
 * For security, all actions are implemented in an idempotent way, meaning that if we call
 * Undo(); Undo() or Redo(); Redo() on them, the operation is still applied as if the
 * function were only called once. This requirement should not be necessary, but will
 * reduce the likelihood of bugs.
 */
public interface UndoableAction {

    // Undo the action, which is online
    void Undo();

    // Redo the action, which is offline
    void Redo();

    // Commit the action as it is online
    void Commit();

    // Forget the action as it is offline
    void Forget();

}
