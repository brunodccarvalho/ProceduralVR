using UnityEngine;

[DisallowMultipleComponent]
public class SoundEventManager : MonoBehaviour {

    public static SoundEventManager instance;

    // Keep it simple. We're not designing a game; just play a simple, short sound so
    // the user doesn't lose his mind 10 minutes in.
    public AudioClip infoSoundClip;
    public AudioClip warningSoundClip;
    public AudioClip errorSoundClip;
    public AudioClip createSoundClip;
    public AudioClip undoSoundClip;
    public AudioClip redoSoundClip;

    SoundEventManager() {
        if (instance != null) Object.Destroy(instance);
        instance = this;
    }

}
