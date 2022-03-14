using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class ProceduralFactory {

    public static Transform root;

    public static Transform Clone(Transform element, bool active = true) {
        var action = new CloneAction(element, active);
        UndoHistory.current.AddLazy(action);
        return action.clone;
    }

    public static Transform LinkedClone(List<Transform> parents, Transform template, bool active = true) {
        var action = new LinkedClone(parents, template, active);
        UndoHistory.current.AddLazy(action);
        return action.first;
    }

    public static void Delete(Transform element) {
        var action = new DeleteAction(element);
        UndoHistory.current.AddLazy(action);
    }

    public static void LinkedDelete(List<Transform> elements) {
        var action = new LinkedDelete(elements);
        UndoHistory.current.AddLazy(action);
    }

    public static void DisbandGroup(Transform element) {
        Debug.Assert(element.GetComponent<ProceduralGroup>() != null);
        var action = new DisbandGroupAction(element);
        UndoHistory.current.AddLazy(action);
    }

    public static void DisbandRandom(Transform element) {
        Debug.Assert(element.GetComponent<ProceduralRandom>() != null);
        var action = new DisbandRandomAction(element);
        UndoHistory.current.AddLazy(action);
    }

    public static void DisbandTiling(Transform element) {
        Debug.Assert(element.GetComponent<ProceduralTiling>() != null);
        var action = new DisbandTilingAction(element);
        UndoHistory.current.AddLazy(action);
    }

    public static void DisbandPosition(Transform element) {
        Debug.Assert(element.GetComponent<ProceduralPosition>() != null);
        var action = new DisbandPositionAction(element);
        UndoHistory.current.AddLazy(action);
    }

    public static void DisbandRotation(Transform element) {
        Debug.Assert(element.GetComponent<ProceduralRotation>() != null);
        var action = new DisbandRotationAction(element);
        UndoHistory.current.AddLazy(action);
    }

    public static List<Transform> Disband(Transform element) {
        var children = Procedural.ProceduralChildren(element, true);
        if (element.GetComponent<ProceduralGroup>() != null) {
            DisbandGroup(element);
        } else if (element.GetComponent<ProceduralRandom>() != null) {
            DisbandRandom(element);
        } else if (element.GetComponent<ProceduralPosition>() != null) {
            DisbandPosition(element);
        } else if (element.GetComponent<ProceduralRotation>() != null) {
            DisbandRotation(element);
        } else if (element.GetComponent<ProceduralTiling>() != null) {
            DisbandTiling(element);
        } else {
            return null;
        }
        return children;
    }

    public static void Unlink(Transform element) {
        var cousin = LinkedProcedural.AnyCousinLink(element);
        if (cousin != null) {
            var action = new UnlinkAction(element, cousin);
            UndoHistory.current.AddLazy(action);
        }
    }

    public static void Randomize(Transform element) {
        element.GetComponent<Procedural>().RandomizeSaved();
    }

    public static ProceduralGroup CreateGroup(List<Transform> elements) {
        var action = new CreateGroupAction(elements);
        UndoHistory.current.AddLazy(action);
        return action.procedural;
    }

    public static ProceduralRandom CreateRandom(List<Transform> elements) {
        var action = new CreateRandomAction(elements);
        UndoHistory.current.AddLazy(action);
        return action.procedural;
    }

    public static ProceduralPosition CreatePosition(Transform element) {
        var action = new CreatePositionAction(element);
        UndoHistory.current.AddLazy(action);
        return action.procedural;
    }

    public static ProceduralRotation CreateRotation(Transform element) {
        var action = new CreateRotationAction(element);
        UndoHistory.current.AddLazy(action);
        return action.procedural;
    }

    public static ProceduralTiling CreateTiling(Transform element) {
        var action = new CreateTilingAction(element);
        UndoHistory.current.AddLazy(action);
        return action.procedural;
    }

    public static ProceduralGroup EmptyGroup() {
        GameObject root = new GameObject();
        root.transform.SetParent(Scenario.current?.root, true);
        root.transform.localScale = Vector3.one;
        var proc = root.AddComponent<ProceduralGroup>();
        return proc;
    }

    public static ProceduralRandom EmptyRandom() {
        GameObject root = new GameObject();
        root.transform.SetParent(Scenario.current?.root, true);
        root.transform.localScale = Vector3.one;
        var proc = root.AddComponent<ProceduralRandom>();
        return proc;
    }

    public static ProceduralPosition EmptyPosition() {
        GameObject root = AugmentController.instance.InstantiatePosition();
        root.transform.SetParent(Scenario.current?.root, true);
        root.transform.localScale = Vector3.one;
        var proc = root.AddComponent<ProceduralPosition>();
        proc.GetReferences();
        return proc;
    }

    public static ProceduralRotation EmptyRotation() {
        GameObject root = AugmentController.instance.InstantiateRotation();
        root.transform.SetParent(Scenario.current?.root, true);
        root.transform.localScale = Vector3.one;
        var proc = root.AddComponent<ProceduralRotation>();
        proc.GetReferences();
        return proc;
    }

    public static ProceduralTiling EmptyTiling() {
        GameObject root = AugmentController.instance.InstantiateTiling();
        root.transform.SetParent(Scenario.current?.root, true);
        root.transform.localScale = Vector3.one;
        var proc = root.AddComponent<ProceduralTiling>();
        proc.GetReferences();
        return proc;
    }

    public static Transform ClonePlain(Transform original) {
        var clone = GameObject.Instantiate(original.gameObject);
        clone.name = original.name;
        clone.transform.SetParent(Scenario.current?.root, true);
        clone.transform.localScale = original.transform.localScale;
        clone.transform.SetPositionAndRotation(original.position, original.rotation);
        LinkRecursively(clone.transform, original);
        clone.GetComponent<Procedural>().Randomize();
        return clone.transform;
    }

    private static void LinkRecursively(Transform clone, Transform original) {
        var newType = clone.GetComponent<Procedural>()?.proctype;
        var oldType = original.GetComponent<Procedural>()?.proctype;
        var news = Procedural.ProceduralChildren(clone);
        var olds = Procedural.ProceduralChildren(original);
        if (newType != oldType) {
            Debug.LogErrorFormat("Different types {0} {1}", clone.name, original.name);
            return;
        }
        if (news.Count != olds.Count) {
            Debug.LogErrorFormat("Different children count {0} {1}", clone.name, original.name);
            return;
        }
        for (int i = 0, N = news.Count; i < N; i++) {
            Transform a = news[i], b = olds[i];
            bool alinked = LinkedProcedural.IsLinkedProcedural(a);
            bool blinked = LinkedProcedural.IsLinkedProcedural(b);
            if (alinked != blinked) {
                Debug.LogErrorFormat("Different IsLinked {0} {1}", a.name, b.name);
                return;
            }
            if (alinked) LinkRecursively(a, b);
        }
        LinkedProcedural.Relink(clone, original);
        clone.GetComponent<Procedural>().CopyTraits(original.GetComponent<Procedural>());
    }

    // ***** Scenario dump

    public static string FormatTree(Transform node) {
        var s = new StringBuilder();
        FormatTreeDfs(node, s, 0);
        return s.ToString();
    }

    static void FormatTreeDfs(Transform transform, StringBuilder s, int depth) {
        s.Append(' ', depth).AppendLine(FormatTreeProcedural(transform));
        foreach (Transform child in transform) {
            FormatTreeDfs(child, s, depth + 1);
        }
    }

    static string FormatTreeProcedural(Transform transform) {
        var procedural = transform.GetComponent<Procedural>();
        var s = " '" + transform.name + "' " + Math3d.FormatTreeTransform(transform);
        if (procedural is ProceduralGroup) return "@Group " + s;
        if (procedural is ProceduralRandom) return "@Random " + s;
        if (procedural is ProceduralPosition) return "@Position " + s;
        if (procedural is ProceduralRotation) return "@Rotation " + s;
        if (procedural is ProceduralTiling) return "@Tiling " + s;
        if (procedural is ProceduralEmpty) return "@Empty " + s;
        if (procedural is ProceduralPrefab) return "@Prefab " + s;
        if (procedural is ProceduralPrimitive) return "@Primitive " + s;
        return "@Unknown " + s;
    }

    // ***** Scenario read

    private static Dictionary<(ProceduralType, string), GameObject> catalog;

    static Dictionary<(ProceduralType, string), GameObject> LoadFullCatalog() {
        var map = new Dictionary<(ProceduralType, string), GameObject>();
        foreach (GameObject obj in Resources.LoadAll<GameObject>("Prefabs/LoadCatalog")) {
            var proc = obj.GetComponent<Procedural>();
            ProceduralType type = proc != null ? proc.proctype : ProceduralType.None;
            if (!map.ContainsKey((type, obj.name))) map.Add((type, obj.name), obj);
        }
        return map;
    }

    public static Transform LoadScenarioInplace(string[] lines) {
        if (catalog == null) catalog = LoadFullCatalog();
        int id = 0;
        var root = LoadDfs(lines, ref id, 0, ProceduralFactory.root);
        return root;
    }

    static Transform LoadDfs(string[] lines, ref int p, int depth, Transform parent) {
        string lead = new string(' ', depth + 1);
        while (p < lines.Length && lines[p].StartsWith("#")) p++;
        var (type, name, active, position, rotation, scale) = ParseLine(lines[p++]);
        var (skip, child) = SpawnLoad(type, name);
        child.parent = parent;
        child.localPosition = position;
        child.localRotation = rotation;
        child.localScale = scale;
        child.name = name;
        child.gameObject.SetActive(active);
        while (p < lines.Length && lines[p].StartsWith(lead)) {
            if (skip || lines[p].StartsWith("#")) {
                p++;
            } else {
                LoadDfs(lines, ref p, depth + 1, child);
            }
        }
        child.GetComponent<Procedural>()?.GetReferences();
        return child;
    }

    static (bool, Transform) SpawnLoad(ProceduralType type, string name) {
        if (catalog.ContainsKey((type, name))) {
            GameObject spawn = GameObject.Instantiate(catalog[(type, name)]);
            spawn.transform.SetParent(ProceduralFactory.root, true);
            spawn.transform.localScale = Vector3.one;
            return (true, spawn.transform);
        }
        GameObject root = new GameObject();
        root.transform.SetParent(ProceduralFactory.root, true);
        root.transform.localScale = Vector3.one;
        switch (type) {
            case ProceduralType.Group:
                root.AddComponent<ProceduralGroup>();
                break;
            case ProceduralType.Random:
                root.AddComponent<ProceduralRandom>();
                break;
            case ProceduralType.Position:
                root.AddComponent<ProceduralPosition>();
                break;
            case ProceduralType.Rotation:
                root.AddComponent<ProceduralRotation>();
                break;
            case ProceduralType.Tiling:
                root.AddComponent<ProceduralTiling>();
                break;
            case ProceduralType.Prefab:
                Debug.LogWarningFormat("Skipped prefab {0}", name);
                break;
            case ProceduralType.Primitive:
                Debug.LogWarningFormat("Skipped primitive {0}", name);
                break;
            case ProceduralType.Empty:
                Debug.LogWarningFormat("Skipped empty {0}", name);
                break;
            case ProceduralType.None:
            default:
                break;
        }
        return (false, root.transform);
    }

    static Regex regex = new Regex(@"\s*@(\w+)\s+(?:'([^']+)'\s+)?([-+])\[([^;]+);([^;]+);([^]]+)\]\s*");

    static (ProceduralType, string, bool, Vector3, Quaternion, Vector3) ParseLine(string line) {
        var match = regex.Match(line);
        Debug.AssertFormat(match.Success, "Failed to match line");
        string typestring = match.Groups[1].Value;
        string name = match.Groups[2].Value;
        bool active = match.Groups[3].Value == "+";
        var position = Math3d.ParseVector3(match.Groups[4].Value);
        var rotation = Math3d.ParseQuaternion(match.Groups[5].Value);
        var scale = Math3d.ParseVector3(match.Groups[6].Value);
        var type = typestring switch {
            "Prefab" => ProceduralType.Prefab,
            "Primitive" => ProceduralType.Primitive,
            "Group" => ProceduralType.Group,
            "Random" => ProceduralType.Random,
            "Picker" => ProceduralType.Random,
            "Position" => ProceduralType.Position,
            "Mover" => ProceduralType.Position,
            "Rotation" => ProceduralType.Rotation,
            "Rotator" => ProceduralType.Rotation,
            "Tiling" => ProceduralType.Tiling,
            "Empty" => ProceduralType.Empty,
            "Unknown" => ProceduralType.None,
            _ => ProceduralType.None
        };
        return (type, name, active, position, rotation, scale);
    }

    private static int emptyindex = 1;
    private static int groupindex = 1;
    private static int randomindex = 1;
    private static int positionindex = 1;
    private static int rotationindex = 1;
    private static int tilingindex = 1;
    private static int unknownindex = 1;

    public static void SetProceduralName(Procedural procedural) {
        Debug.Assert(procedural.proctype != 0);
        if (!procedural.modifiable) return;
        if (procedural.proctype == ProceduralType.Primitive) return;
        if (procedural.proctype == ProceduralType.Prefab) return;
        procedural.name = procedural.proctype switch {
            ProceduralType.Empty => string.Format("Empty-{0}", emptyindex++),
            ProceduralType.Group => string.Format("Group-{0}", groupindex++),
            ProceduralType.Random => string.Format("Picker-{0}", randomindex++),
            ProceduralType.Position => string.Format("Mover-{0}", positionindex++),
            ProceduralType.Rotation => string.Format("Rotator-{0}", rotationindex++),
            ProceduralType.Tiling => string.Format("Tiling-{0}", tilingindex++),
            _ => string.Format("Unknown-{0}", unknownindex++),
        };
    }

}
