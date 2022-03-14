using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

[DisallowMultipleComponent]
public class InputController : MonoBehaviour {

    public static InputController instance;

    public SteamVR_Action_Boolean handGrabAction;
    public SteamVR_Action_Boolean laserGrabAction;
    public SteamVR_Action_Boolean undoAction;
    public SteamVR_Action_Boolean redoAction;
    public SteamVR_Action_Boolean acceptAction;
    public SteamVR_Action_Boolean cancelAction;

    public LayerMask priorityLayer, normalLayer;
    public UnityEvent<GrabSource, Transform> OnLaserCollisionBegin;
    public UnityEvent<GrabSource, Transform> OnLaserCollisionEnd;

    public Transform currentLaserHovered {
        get {
            bool right = MainController.instance.rightHanded;
            GrabSource source = right ? GrabSource.RightLaser : GrabSource.LeftLaser;
            if (collisionLaser[source] != null) {
                return Interactive.GetLastUnexposed(collisionLaser[source])?.transform;
            } else {
                return null;
            }
        }
    }

    DelegateLinkedMultiset<Transform> collisions;
    Dictionary<GrabSource, DelegateLinkedSet<Transform>> collisionsHand;
    Dictionary<GrabSource, Transform> collisionLaser;

    InputController() {
        if (instance != null) Object.Destroy(instance);
        instance = this;

        collisions = new DelegateLinkedMultiset<Transform>();
        collisionsHand = new Dictionary<GrabSource, DelegateLinkedSet<Transform>>();
        collisionLaser = new Dictionary<GrabSource, Transform>();

        collisions.OnInsert.AddListener(Interactive.BeginHover);
        collisions.OnRemove.AddListener(Interactive.EndHover);

        collisionsHand.Add(GrabSource.LeftHand, new DelegateLinkedSet<Transform>());
        collisionsHand.Add(GrabSource.RightHand, new DelegateLinkedSet<Transform>());
        collisionLaser.Add(GrabSource.LeftLaser, null);
        collisionLaser.Add(GrabSource.RightLaser, null);
    }

    void Awake() {
        var leftHand = SteamVR_Input_Sources.LeftHand;
        var rightHand = SteamVR_Input_Sources.RightHand;

        handGrabAction?.AddOnStateDownListener(LeftHandGrabDown, leftHand);
        handGrabAction?.AddOnStateUpListener(LeftHandGrabUp, leftHand);
        handGrabAction?.AddOnStateDownListener(RightHandGrabDown, rightHand);
        handGrabAction?.AddOnStateUpListener(RightHandGrabUp, rightHand);
        laserGrabAction?.AddOnStateDownListener(LeftLaserGrabDown, leftHand);
        laserGrabAction?.AddOnStateUpListener(LeftLaserGrabUp, leftHand);
        laserGrabAction?.AddOnStateDownListener(RightLaserGrabDown, rightHand);
        laserGrabAction?.AddOnStateUpListener(RightLaserGrabUp, rightHand);

        undoAction?.AddOnStateDownListener(UndoButtonDown, leftHand);
        undoAction?.AddOnStateDownListener(UndoButtonDown, rightHand);
        redoAction?.AddOnStateDownListener(RedoButtonDown, leftHand);
        redoAction?.AddOnStateDownListener(RedoButtonDown, rightHand);
        acceptAction?.AddOnStateDownListener(AcceptButtonDown, leftHand);
        acceptAction?.AddOnStateDownListener(AcceptButtonDown, rightHand);
        cancelAction?.AddOnStateDownListener(CancelButtonDown, leftHand);
        cancelAction?.AddOnStateDownListener(CancelButtonDown, rightHand);

        ProceduralFactory.root = GameObject.Find("ProceduralRoot").transform;
    }

    // ***** Handle collision updates from various sources (hand, laser, Interactive)

    internal void AddHandCollision(Transform target, GrabSource hand) {
        Transform actual = Interactive.GetFirst(target)?.transform;
        collisionsHand[hand].AddLast(actual);
        collisions.AddLast(actual);
    }

    internal void RemoveHandCollision(Transform target, GrabSource hand) {
        Transform actual = Interactive.GetFirst(target)?.transform;
        collisionsHand[hand].Remove(actual);
        collisions.Remove(actual);
    }

    internal void UpdateLaserCollision(Transform collided, GrabSource laser) {
        Transform actual = Interactive.GetFirst(collided)?.transform;
        if (collisionLaser[laser] != null) {
            OnLaserCollisionEnd.Invoke(laser, collisionLaser[laser]);
        }
        collisions.Remove(collisionLaser[laser]);
        collisionLaser[laser] = actual;
        if (collisionLaser[laser] != null) {
            OnLaserCollisionBegin.Invoke(laser, collisionLaser[laser]);
        }
        collisions.AddLast(collisionLaser[laser]);
    }

    internal void ClearCollision(Transform who) {
        if (collisionLaser[GrabSource.LeftLaser] == who) {
            OnLaserCollisionEnd.Invoke(GrabSource.LeftLaser, who);
            collisionLaser[GrabSource.LeftLaser] = null;
        }
        if (collisionLaser[GrabSource.RightLaser] == who) {
            OnLaserCollisionEnd.Invoke(GrabSource.RightLaser, who);
            collisionLaser[GrabSource.RightLaser] = null;
        }
        collisionsHand[GrabSource.LeftHand].Remove(who);
        collisionsHand[GrabSource.RightHand].Remove(who);
        collisions.RemoveAll(who);
    }

    // ***** SteamVR Input

    void LeftHandGrabDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        DispatchGrabDown(GrabSource.LeftHand);
    }
    void LeftHandGrabUp(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        DispatchGrabUp(GrabSource.LeftHand);
    }

    void RightHandGrabDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        DispatchGrabDown(GrabSource.RightHand);
    }
    void RightHandGrabUp(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        DispatchGrabUp(GrabSource.RightHand);
    }

    void LeftLaserGrabDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        DispatchGrabDown(GrabSource.LeftLaser);
    }
    void LeftLaserGrabUp(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        DispatchGrabUp(GrabSource.LeftLaser);
    }

    void RightLaserGrabDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        DispatchGrabDown(GrabSource.RightLaser);
    }
    void RightLaserGrabUp(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        DispatchGrabUp(GrabSource.RightLaser);
    }

    void AcceptUndoButtonDown(bool right) {
        if (MainController.instance.rightHanded == right)
            Interactor.instance.Accept();
        else
            Interactor.instance.Undo();
    }
    void CancelRedoButtonDown(bool right) {
        if (MainController.instance.rightHanded == right)
            Interactor.instance.Cancel();
        else
            Interactor.instance.Redo();
    }

    void UndoButtonDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        AcceptUndoButtonDown(source == SteamVR_Input_Sources.RightHand);
    }

    void RedoButtonDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        CancelRedoButtonDown(source == SteamVR_Input_Sources.RightHand);
    }

    void AcceptButtonDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        AcceptUndoButtonDown(source == SteamVR_Input_Sources.RightHand);
    }

    void CancelButtonDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
        CancelRedoButtonDown(source == SteamVR_Input_Sources.RightHand);
    }

    // ***** Dispatch to InteractionController

    void DispatchGrabDown(GrabSource source) {
        var dominant = MainController.instance.rightHanded ? GrabSource.Right : GrabSource.Left;
        if (!source.HasFlag(dominant)) return;
        if (source == GrabSource.RightHand || source == GrabSource.LeftHand) {
            if (collisionsHand[source].Count > 0) {
                var oldestOnHand = collisionsHand[source].First.Value; // oldest hovered
                var target = Interactive.GetLastUnexposed(oldestOnHand)?.transform;
                if (transform != null) {
                    Interactor.instance.GrabDown(target, source);
                } else {
                    Debug.LogWarningFormat("Invalid hand grab {0}", oldestOnHand?.name);
                }
            }
        } else {
            if (collisionLaser[source] != null) {
                var onLaser = collisionLaser[source];
                var target = Interactive.GetLastUnexposed(onLaser)?.transform;
                Interactor.instance.GrabDown(target, source);
            }
        }
    }

    void DispatchGrabUp(GrabSource source) {
        Interactor.instance.GrabUp(source);
    }

}
