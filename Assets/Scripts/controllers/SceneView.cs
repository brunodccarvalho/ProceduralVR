using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

/**
 * The primary view class. Must be attached to ProceduralRoot.
 */
[DisallowMultipleComponent]
public class SceneView : MonoBehaviour {

    public static SceneView instance;

    public Transform buttonRoot;

    [Tooltip("Scale factor for the scene scale user action")]
    public float scaleFactor = Mathf.Pow(2, 1.0f / 3.0f);

    [Tooltip("Forward offset in player scale")]
    public float scaleOffset = 0.2f;

    [Tooltip("Subjects that should hover which hand")]
    public List<Transform> hoverDominantHand, hoverOtherHand;

    [Tooltip("Transforms to update with a descriptive message of the current mode")]
    public List<Transform> explainTextRecipients;

    [Tooltip("Transforms to update with messages of latest events")]
    public List<Transform> eventTextRecipients;

    [Tooltip("Transforms to update with a descriptive message of the current tail")]
    public List<Transform> tailTextRecipients;

    private List<Transform> menus;
    private List<InteractiveButton> buttonsList;

    public float currentZoom => transform.localScale[0];

    SceneView() {
        if (instance != null) Object.Destroy(instance);
        instance = this;

        Debug.Assert(1.0f < scaleFactor && scaleFactor <= 2.0f);
    }

    void Start() {
        this.menus = new List<Transform>();
        foreach (Transform child in buttonRoot.GetChild(0)) menus.Add(child);
        this.buttonsList = new List<InteractiveButton>();
        buttonRoot.GetComponentsInChildren<InteractiveButton>(true, buttonsList);
    }

    void FixedUpdate() {
        UpdateActiveMenu();
        FixVisibleLockButtons();
    }

    public void UpscaleSceneBy(int times) {
        if (Grabber.instance.IsGrabbing) {
            SceneEvents.Error("Drop the object first");
            return;
        }
        var scale = Mathf.Pow(scaleFactor, times);
        ScaleSceneTo(this.transform.localScale[0] * scale);
    }

    public void DownscaleSceneBy(int times) {
        if (Grabber.instance.IsGrabbing) {
            SceneEvents.Error("Drop the object first");
            return;
        }
        var scale = Mathf.Pow(scaleFactor, times);
        ScaleSceneTo(this.transform.localScale[0] / scale);
    }

    public void ScaleSceneTo(float scale, bool useOffset = true) {
        var ratio = scale / this.transform.localScale[0];
        this.transform.localScale = Vector3.one * scale;
        var player = Player.instance.transform;
        var scaler = new Vector3(ratio, 1, ratio);
        if (player != null && useOffset) {
            var forward = Camera.main.transform.forward;
            forward = Vector3.Scale(new Vector3(1, 0, 1), forward).normalized;
            var center = player.position + forward * scaleOffset;
            center = Vector3.Scale(center, scaler);
            player.position = center - forward * scaleOffset;
        } else if (player != null && !useOffset) {
            player.position = Vector3.Scale(player.position, scaler);
        }
    }

    private string IntendedActiveMenu() {
        var currentMode = Interactor.instance.currentMode;

        if (Scenario.current.useBaseline) {
            return "BaselineUI";
        }

        switch (currentMode) {
            case PMode.GlobalGrab:
            case PMode.GlobalClone:
            case PMode.GlobalRandomize:
            case PMode.GlobalDelete:
            case PMode.GlobalUnlink:
            case PMode.GlobalDisband:
            case PMode.CreateGroup:
            case PMode.CreateRandom:
            case PMode.CreateRandomPosition:
            case PMode.CreateRandomRotation:
            case PMode.CreateTiling:
            case PMode.GlobalEdit:
                return "MainUI";

            case PMode.EditGroupWait:
            case PMode.EditGroupGrab:
            case PMode.EditGroupClone:
            case PMode.EditGroupDelete:
                return "GroupEditUI";

            case PMode.EditRandomWait:
            case PMode.EditRandomGrab:
            case PMode.EditRandomClone:
            case PMode.EditRandomDelete:
                return "RandomEditUI";

            case PMode.EditPositionWait:
            case PMode.EditPositionGrab:
                return "PositionEditUI";

            case PMode.EditRotationWait:
            case PMode.EditRotationGrab:
                return "RotationEditUI";

            case PMode.EditTilingWait:
            case PMode.EditTilingGrab:
                return "TilingEditUI";

            default:
                // ! INTERNAL ERROR
                Debug.LogErrorFormat("ActiveMenu: Skipped mode {0}", currentMode);
                return null;
        }
    }

    private void UpdateActiveMenu() {
        var intended = IntendedActiveMenu();
        if (intended != null && MainController.instance.simpleMenus) intended += "Simple";
        foreach (Transform menu in menus) menu.gameObject.SetActive(menu.name == intended);
    }

    private void FixVisibleLockButtons() {
        bool simpleMenus = MainController.instance.simpleMenus;
        var visible = Scenario.current?.visibleLock;
        foreach (var button in buttonsList) {
            var parent = button.transform.parent.gameObject;
            if (button.buttonType != ButtonType.UserAction) continue;
            if (button.buttonUserAction == PUserAction.ToggleVisibleEmpty) {
                parent.SetActive(Scenario.current.visibleEmpty);
            } else if (button.buttonUserAction.HasFlag(PUserAction.Grabber)) {
                if (!simpleMenus) {
                    parent.SetActive(true);
                } else if (visible == VisibleLock.None) {
                    parent.SetActive(false);
                } else {
                    switch (button.buttonUserAction) {
                        case PUserAction.GrabTogglePlaneLock:
                            parent.SetActive(visible == VisibleLock.PlaneLock);
                            break;
                        case PUserAction.GrabToggleVerticalLock:
                            parent.SetActive(visible == VisibleLock.VerticalLock);
                            break;
                        case PUserAction.GrabToggleRotaxis:
                            parent.SetActive(visible == VisibleLock.Rotaxis);
                            break;
                        case PUserAction.GrabToggleRotationSnap:
                            parent.SetActive(visible == VisibleLock.RotationSnap);
                            break;
                        case PUserAction.GrabToggleGridSnap:
                            parent.SetActive(visible == VisibleLock.GridSnap);
                            break;
                        default:
                            parent.SetActive(false);
                            break;
                    }
                }
            }
        }
    }

}
