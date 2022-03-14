using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour {

    public static MainController instance;

    public string token = null;
    public bool debugMode = false;
    public bool rightHanded = true;
    public bool simpleMenus = true;
    public string loadToken = null;
    public string loadName = null;
    public int initialScenario = 7;

    [Tooltip("List of test scenarios, in order")]
    public List<Scenario> scenarios;

    KeyCode keyInfoDump = KeyCode.H;
    KeyCode keyTestDump = KeyCode.D;
    KeyCode keyToggleHandedness = KeyCode.A;
    KeyCode keyToggleSimpleMenus = KeyCode.M;
    KeyCode keyToggleVisibleEmpty = KeyCode.E;

    KeyCode keyTogglePlaneLock = KeyCode.P;
    KeyCode keyToggleVerticalLock = KeyCode.V;
    KeyCode keyToggleRotationLock = KeyCode.X;
    KeyCode keyToggleRotationSnap = KeyCode.R;
    KeyCode keyToggleGridSnap = KeyCode.G;

    KeyCode keyReset = KeyCode.R;
    KeyCode keyStart = KeyCode.S;
    KeyCode keyStop = KeyCode.S;
    KeyCode keySave = KeyCode.F;

    KeyCode keyTask = KeyCode.T;
    KeyCode keyAdvance = KeyCode.RightArrow;
    KeyCode keyRetreat = KeyCode.LeftArrow;

    KeyCode keyShiftUp = KeyCode.UpArrow;
    KeyCode keyShiftDown = KeyCode.DownArrow;

    MainController() {
        if (instance != null) Object.Destroy(instance);
        instance = this;
    }

    void Start() {
        if (token == null || token.Length == 0) {
            token = Random.Range(1000, 9999).ToString();
        }
        if (loadToken != null && loadName != null && loadToken.Length > 0 && loadName.Length > 0) {
            scenarios.Add(new Scenario(loadToken, loadName));
        }
        foreach (var scenario in scenarios) {
            scenario.root.gameObject.SetActive(false);
        }
        LoadScenario(initialScenario - 1);
    }

    void Update() {
        Process();
    }

    string D(KeyCode code, string desc) {
        return string.Format("{0:10} -- {1}\n", code.ToString(), desc);
    }
    string D(string codes, string desc) {
        return string.Format("{0:10} -- {1}\n", codes, desc);
    }

    void DumpInfo() {
        string s = string.Format("=== COMMANDS (TOKEN = {0})\n", token);
        s += D(keyInfoDump, "Dump this help info");
        s += D(keyTestDump, "Dump test tracking data");
        s += D(keyToggleHandedness, "Switch user handedness");
        s += D(keyToggleSimpleMenus, "Toggle simple menus");
        s += D(keyToggleVisibleEmpty, "Toggle ProceduralEmpty visibility");
        s += D(keyShiftUp, "Shift board up");
        s += D(keyShiftDown, "Shift board down");

        s += D(keyTogglePlaneLock, "Toggle PlaneLock");
        s += D(keyToggleVerticalLock, "Toggle VerticalLock");
        s += D(keyToggleRotationLock, "Toggle RotationLock");
        s += D(keyToggleRotationSnap, "Toggle RotationSnap");
        s += D(keyToggleGridSnap, "Toggle GridSnap");

        s += D("Shift + " + keyReset.ToString(), "Reset test data");
        s += D("Shift + " + keyStart.ToString(), "Start tracking");
        s += D("Shift + " + keyStop.ToString(), "Stop tracking");
        s += D("Shift + " + keySave.ToString(), "Save tracking");
        s += D("Shift + 1..9", "Load scenario...");
        s += "=== SCENARIOS\n";
        for (int i = 1, S = scenarios.Count; i <= S; i++) {
            s += D(i.ToString(), scenarios[i - 1].ToString());
        }
        Debug.Log(s);
    }

    void Process() {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            if (Grabber.instance.IsGrabbing) {
                return;
            }
            if (Input.GetKeyDown(keyReset)) {
                Scenario.current?.ResetTest(); return;
            }
            if (Input.GetKeyDown(keyStart) && !Metrics.current.IsTracking) {
                Scenario.current?.StartTracking(); return;
            }
            if (Input.GetKeyDown(keyStop) && Metrics.current.IsTracking) {
                Scenario.current?.StopTracking(); return;
            }
            if (Input.GetKeyDown(keySave)) {
                Scenario.current?.FinishTest(); return;
            }
            if (Input.GetKeyDown(keyAdvance)) {
                Scenario.current?.AdvanceGoal(); return;
            }
            if (Input.GetKeyDown(keyRetreat)) {
                Scenario.current?.RetreatGoal(); return;
            }
            if (Input.GetKeyDown(keyTask)) {
                Scenario.current?.ShowGoal(); return;
            }
            if (Input.GetKeyDown(keyShiftUp)) {
                FeedbackBoard.instance.Shift(+1); return;
            }
            if (Input.GetKeyDown(keyShiftDown)) {
                FeedbackBoard.instance.Shift(-1); return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                LoadScenario(0); return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                LoadScenario(1); return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                LoadScenario(2); return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4)) {
                LoadScenario(3); return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha5)) {
                LoadScenario(4); return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha6)) {
                LoadScenario(5); return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha7)) {
                LoadScenario(6); return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha8)) {
                LoadScenario(7); return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha9)) {
                LoadScenario(8); return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha0)) {
                LoadScenario(9); return;
            }
        } else {
            if (Input.GetKeyDown(keyInfoDump)) {
                DumpInfo(); return;
            }
            if (Input.GetKeyDown(keyTestDump)) {
                Scenario.current?.Dump(); return;
            }
            if (Grabber.instance.IsGrabbing) {
                return;
            }
            if (Input.GetKeyDown(keyToggleHandedness)) {
                rightHanded = !rightHanded; return;
            }
            if (Input.GetKeyDown(keyToggleSimpleMenus)) {
                simpleMenus = !simpleMenus; return;
            }
            if (Input.GetKeyDown(keyTogglePlaneLock)) {
                Grabber.instance.userLocks.TogglePlaneLock(); return;
            }
            if (Input.GetKeyDown(keyToggleVerticalLock)) {
                Grabber.instance.userLocks.ToggleVerticalLock(); return;
            }
            if (Input.GetKeyDown(keyToggleRotationLock)) {
                Grabber.instance.userLocks.ToggleRotaxis(); return;
            }
            if (Input.GetKeyDown(keyToggleRotationSnap)) {
                Grabber.instance.userLocks.ToggleRotationSnap(); return;
            }
            if (Input.GetKeyDown(keyToggleGridSnap)) {
                Grabber.instance.userLocks.ToggleGridSnap(); return;
            }
        }
    }

    void LoadScenario(int i) {
        if (i >= scenarios.Count) {
            Debug.LogErrorFormat("No scenario #{0}, you have {1}", i, scenarios.Count);
            return;
        }
        Scenario.current?.Unload();
        scenarios[i].Load();
    }

}
