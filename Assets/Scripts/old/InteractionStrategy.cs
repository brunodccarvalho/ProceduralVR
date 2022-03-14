#if false
using UnityEngine;

public interface InteractionStrategy {

    // Begin the strategy by setting up buttons, highlights, laser color, etc.
    void Start();

    // Exit the interaction by removing buttons, highlights, laser color, etc.
    void Close();

    // Action: User grabbed the given target object from the given source
    void GrabDown(Transform target, GrabSource source);

    // Action: User let go from the given source
    void GrabUp(GrabSource source);

    // Action: User clicked a floating button
    void ClickButton(string button);

    // Action: User clicked A to accept
    void Accept();

    // Action: User clicked B to cancel
    void Cancel();

    // Action: User clicked X to undo
    void Undo();

    // Action: User clicked Y to redo.
    void Redo();

    // Can the strategy yield right now?
    bool Locked();

}
#endif
