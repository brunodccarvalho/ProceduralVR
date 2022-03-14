#if false
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ButtonsController : MonoBehaviour {

    public static ButtonsController instance;

    public Transform buttonRoot;
    public Transform leftHoverPoint;
    public Transform rightHoverPoint;

    Dictionary<string, Transform> menus;
    Dictionary<string, List<InteractiveButton>> buttons;

    ButtonsController() {
        Debug.Assert(instance == null);
        instance = this;
    }

    Transform ActiveMenu() {
        foreach (Transform child in buttonRoot) {
            if (child.gameObject.activeSelf) {
                return child;
            }
        }
        return null;
    }

    void Awake() {
        menus = new Dictionary<string, Transform>();
        buttons = new Dictionary<string, List<InteractiveButton>>();
        foreach (Transform menu in buttonRoot) {
            menus[menu.name] = menu;
            var children = buttonRoot.GetComponentsInChildren<InteractiveButton>(true);
            foreach (InteractiveButton button in children) {
                if (!buttons.ContainsKey(button.buttonName)) {
                    buttons.Add(button.buttonName, new List<InteractiveButton>());
                }
                buttons[button.buttonName].Add(button);
            }
        }

        if (InputController.instance.rightHanded) {
            buttonRoot.SetParent(leftHoverPoint, false);
        } else {
            buttonRoot.SetParent(rightHoverPoint, false);
        }
        // Set initially selected buttons
        foreach (string button in new string[] { "emptytoggle" }) {
            SelectButton(button, true);
        }
    }

    public void SwitchUI(string menu) {
        Debug.Assert(menus.ContainsKey(menu));
        Debug.LogFormat("Switch Menu -> {0}", menu);
        ActiveMenu()?.gameObject.SetActive(false);
        menus[menu].gameObject.SetActive(true);
    }

    public bool SelectButton(string name, bool value) {
        Debug.Assert(name != null && buttons.ContainsKey(name));
        foreach (InteractiveButton button in buttons[name]) {
            button.Select(value);
        }
        return true;
    }

    public void ColorButton(string name, int color) {
        Debug.Assert(name != null && buttons.ContainsKey(name));
        foreach (InteractiveButton button in buttons[name]) {
            button.Color(color);
        }
    }

    public void ClearSelectedButtons() {
        foreach (var name in buttons.Keys) {
            foreach (InteractiveButton button in buttons[name]) {
                button.Select(false);
            }
        }
    }

}
#endif
