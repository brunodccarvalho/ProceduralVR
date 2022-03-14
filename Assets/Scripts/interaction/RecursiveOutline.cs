using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * Nesting-friendly script version of QuickOutline.
 * Instead of dynamically changing the outline material's properties at runtime,
 * we instead create single global instance of the material of each type (hover,
 * selected, etc)
 */
[DisallowMultipleComponent]
public class RecursiveOutline : MonoBehaviour {

    enum OutlineMode {
        OutlineAll,
        OutlineVisible,
        OutlineHidden,
        OutlineAndSilhouette,
        SilhouetteOnly
    }

    static Material outlineMaskMaterial, outlineFillMaterial;
    static HashSet<Material> outlineMaterialSet;
    static Dictionary<string, Material[]> variantMaterials;
    static Dictionary<string, Dictionary<string, string>> styleMap;
    static Shader standardShader;

    const string defaultStyle = "Outline";
    public string currentStyle = null;
    protected string currentVariant = "Default";

    public static int[] PlainAxis = new int[] { 7, 8, 9 };

    static void LoadMaterialSet() {
        outlineMaskMaterial = Resources.Load<Material>(@"Materials/OutlineMask");
        outlineFillMaterial = Resources.Load<Material>(@"Materials/OutlineFill");
        outlineMaskMaterial.name = "OutlineMask";
        outlineFillMaterial.name = "OutlineFill";

        standardShader = Shader.Find("Standard");
        Debug.Assert(standardShader != null);

        outlineMaterialSet = new HashSet<Material>();
        variantMaterials = new Dictionary<string, Material[]>();
        styleMap = new Dictionary<string, Dictionary<string, string>>();

        /**
         * Colors:
         *   0: none
         *   1: light red
         *   2: light green
         *   3: light blue
         *   4: light orange
         *   5: light purple
         */

        AddOutlineMaterial("Outline-Red", Palette.lightred, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Outline-Green", Palette.lightgreen, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Outline-Blue", Palette.lightblue, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Outline-Black", Palette.black, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Outline-Gray", Palette.gray, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Outline-White", Palette.white, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Outline-Yellow", Palette.yellow, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Outline-Cyan", Palette.cyan, 3.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Outline-Orange", Palette.orange, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Outline-Orchid", Palette.orchid, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Outline-Seafoam", Palette.seafoam, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Outline-Purple", Palette.purple, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Outline-Yellow-S", Palette.yellow, 5.0f, OutlineMode.OutlineAll);

        AddOutlineMaterial("Augment-Red", Palette.lightred, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Augment-Green", Palette.lightgreen, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Augment-Blue", Palette.lightblue, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Augment-Black", Palette.black, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Augment-Gray", Palette.gray, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Augment-White", Palette.white, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Augment-Yellow", Palette.yellow, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Augment-Cyan", Palette.cyan, 3.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Augment-Orange", Palette.orange, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Augment-Orchid", Palette.orchid, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Augment-Seafoam", Palette.seafoam, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Augment-Purple", Palette.purple, 5.0f, OutlineMode.OutlineAll);
        AddOutlineMaterial("Augment-Yellow-S", Palette.yellow, 5.0f, OutlineMode.OutlineAll);

        AddPlainMaterial("Plain-Red", Palette.lightred);
        AddPlainMaterial("Plain-Green", Palette.lightgreen);
        AddPlainMaterial("Plain-Blue", Palette.lightblue);
        AddPlainMaterial("Plain-Black", Palette.black);
        AddPlainMaterial("Plain-Gray", Palette.gray);
        AddPlainMaterial("Plain-White", Palette.white);
        AddPlainMaterial("Plain-Yellow", Palette.yellow);
        AddPlainMaterial("Plain-Cyan", Palette.cyan);
        AddPlainMaterial("Plain-Orange", Palette.orange);
        AddPlainMaterial("Plain-Orchid", Palette.orchid);
        AddPlainMaterial("Plain-Seafoam", Palette.seafoam);
        AddPlainMaterial("Plain-Purple", Palette.purple);

        styleMap.Add("Outline", new Dictionary<string, string> {
            {"Hovered", "Outline-Yellow-S"},
            {"Grabbed", "Outline-Red"},
            {"SlaveGrabbed", "Outline-Orchid"},
            {"Selected", "Outline-Blue"},
            {"Exposed", "Outline-Cyan"},
            {"HoveredSelected", "Outline-Purple"},
            {"Color-2", "Outline-Seafoam"},   // Edits
            {"Color-5", "Outline-Orchid"},    // Create...
            {"Color-6", "Outline-White"},     // Unlink,Disband
            {"Color-7", "Outline-Green"},     // Randomize
            {"Color-8", "Outline-Black"},     // Delete
            {"Color-10", "Outline-White"},    // Information
            {"Color-11", "Outline-Orange"},   // Warning
            {"Color-12", "Outline-Red"},      // Error
            {"Color-13", "Outline-Red"},      // InternalError
        });

        styleMap.Add("Augment", new Dictionary<string, string> {
            {"Hovered", "Augment-Yellow-S"},
            {"Grabbed", "Augment-Red"},
            {"SlaveGrabbed", "Augment-Orchid"},
            {"Selected", "Augment-Blue"},
            {"Exposed", "Augment-Cyan"},
            {"HoveredSelected", "Augment-Purple"},
            {"Color-10", "Augment-White"},    // Information
            {"Color-11", "Augment-Orange"},   // Warning
            {"Color-12", "Augment-Red"},      // Error
            {"Color-13", "Augment-Red"},      // InternalError
        });

        styleMap.Add("ModeToggleButton", new Dictionary<string, string> {
            {"Hovered", "Plain-Yellow"},
            {"Selected", "Plain-Blue"},
            {"HoveredSelected", "Plain-Purple"},
        });

        styleMap.Add("ProceduralActionButton", new Dictionary<string, string> {
            {"Hovered", "Plain-Yellow"},
            {"Selected", "Plain-Blue"},
            {"HoveredSelected", "Plain-Purple"},
        });

        styleMap.Add("UserActionButton", new Dictionary<string, string> {
            {"Hovered", "Plain-Yellow"},
            {"Selected", "Plain-Blue"},
            {"HoveredSelected", "Plain-Purple"},
            {"Color-1", "Plain-Seafoam"},
            {"Color-2", "Plain-Orchid"},
            {"Color-7", "Plain-Red"},   // X
            {"Color-8", "Plain-Green"}, // Y
            {"Color-9", "Plain-Blue"},  // Z
        });
    }

    static void AddOutlineMaterial(string name, Color color, float width, OutlineMode mode) {
        var mask = Instantiate(outlineMaskMaterial);
        var fill = Instantiate(outlineFillMaterial);
        variantMaterials.Add(name, new Material[2] { mask, fill });
        outlineMaterialSet.Add(mask);
        outlineMaterialSet.Add(fill);

        fill.SetColor("_OutlineColor", color);

        var Always = (float)UnityEngine.Rendering.CompareFunction.Always;
        var LessEqual = (float)UnityEngine.Rendering.CompareFunction.LessEqual;
        var Greater = (float)UnityEngine.Rendering.CompareFunction.Greater;

        switch (mode) {
            case OutlineMode.OutlineAll:
                mask.SetFloat("_ZTest", Always);
                fill.SetFloat("_ZTest", Always);
                fill.SetFloat("_OutlineWidth", width);
                break;

            case OutlineMode.OutlineVisible:
                mask.SetFloat("_ZTest", Always);
                fill.SetFloat("_ZTest", LessEqual);
                fill.SetFloat("_OutlineWidth", width);
                break;

            case OutlineMode.OutlineHidden:
                mask.SetFloat("_ZTest", Always);
                fill.SetFloat("_ZTest", Greater);
                fill.SetFloat("_OutlineWidth", width);
                break;

            case OutlineMode.OutlineAndSilhouette:
                mask.SetFloat("_ZTest", LessEqual);
                fill.SetFloat("_ZTest", Always);
                fill.SetFloat("_OutlineWidth", width);
                break;

            case OutlineMode.SilhouetteOnly:
                mask.SetFloat("_ZTest", LessEqual);
                fill.SetFloat("_ZTest", Greater);
                fill.SetFloat("_OutlineWidth", 0);
                break;
        }
    }

    static void AddPlainMaterial(string name, Color color) {
        var mat = new Material(standardShader);
        mat.SetColor("_Color", color);
        variantMaterials.Add(name, new Material[1] { mat });
        outlineMaterialSet.Add(mat);
    }

    public static void EnsureMaterialSetLoaded() {
        if (outlineMaterialSet == null) {
            LoadMaterialSet();
        }
    }

    void Start() {
        EnsureMaterialSetLoaded();
        ClearSharedMaterials();
        if (currentStyle == null || currentStyle == "") {
            SetStyle(defaultStyle);
        }
        LoadSmoothNormals();
    }

    void ClearSharedMaterials() {
        foreach (Renderer renderer in GetComponents<Renderer>()) {
            var materials = new List<Material>();
            foreach (Material material in renderer.sharedMaterials) {
                if (!outlineMaterialSet.Contains(material)) {
                    materials.Add(material);
                }
            }
            renderer.sharedMaterials = materials.ToArray();
        }
    }

    public void SetStyle(string style) {
        currentStyle = style;
        if (currentVariant != "Default") {
            SetVariant(styleMap[currentStyle][currentVariant]);
        }
    }

    public void SetVariant(string variant) {
        if (currentStyle == null || currentStyle == "") {
            currentStyle = defaultStyle;
        }
        if (currentVariant == variant) {
            return;
        }
        if (!styleMap.ContainsKey(currentStyle)) {
            Debug.LogErrorFormat("Did not find style {0} in {1}", currentStyle, transform.name);
            return;
        }
        var map = styleMap[currentStyle];
        foreach (Renderer renderer in GetComponents<Renderer>()) {
            var materials = new List<Material>(renderer.sharedMaterials);
            var filtered = new List<Material>();
            foreach (Material material in materials) {
                if (!outlineMaterialSet.Contains(material)) {
                    filtered.Add(material);
                }
            }
            if (map.ContainsKey(variant)) {
                foreach (Material mat in variantMaterials[map[variant]]) {
                    filtered.Add(mat);
                }
            }
            renderer.sharedMaterials = filtered.ToArray();
        }
        currentVariant = variant;
    }

    public void SetVariantRecursive(string variant, bool includeInactive = false) {
        SetVariant(variant);
        foreach (var outline in GetComponentsInChildren<RecursiveOutline>(includeInactive)) {
            outline.SetVariant(variant);
        }
    }

    // ***** INTERFACE

    // ***** Outline script internals (without baking or skinned mesh renderers)

    void LoadSmoothNormals() {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter && registeredMeshes.Add(meshFilter.sharedMesh)) {
            // Retrieve or generate smooth normals
            var smoothNormals = SmoothNormals(meshFilter.sharedMesh);
            // Store smooth normals in UV3
            meshFilter.sharedMesh.SetUVs(3, smoothNormals);
        }
    }

    static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

    static List<Vector3> SmoothNormals(Mesh mesh) {
        // Group vertices by location
        var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);

        // Copy normals to a new list
        var smoothNormals = new List<Vector3>(mesh.normals);

        // Average normals for grouped vertices
        foreach (var group in groups) {
            // Skip single vertices
            if (group.Count() == 1) {
                continue;
            }

            // Calculate the average normal
            var smoothNormal = Vector3.zero;

            foreach (var pair in group) {
                smoothNormal += mesh.normals[pair.Value];
            }

            smoothNormal.Normalize();

            // Assign smooth normal to each vertex
            foreach (var pair in group) {
                smoothNormals[pair.Value] = smoothNormal;
            }
        }

        return smoothNormals;
    }

}
