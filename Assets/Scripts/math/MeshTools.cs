using System.Collections.Generic;
using UnityEngine;

public static class MeshTools {

    const float torusIsomorphicThreshold = 0.10f;
    static List<(float, Mesh)> torusPool;

    static MeshTools() {
        torusPool = new List<(float, Mesh)>();
    }

    public static void UpdateTorus(Transform torus, float a) {
        float best_threshold = 1.0f;
        Mesh best = null;
        foreach (var (ma, mesh) in torusPool) {
            var f = Mathf.Abs(a - ma) / a;
            if (f <= torusIsomorphicThreshold && f < best_threshold) {
                best_threshold = f;
                best = mesh;
            }
        }
        if (best == null) {
            best = BuildTorusMesh(torus, a);
            torusPool.Add((a, best));
            Debug.LogFormat("New torus for a={0}", a);
        }
        torus.GetComponent<MeshFilter>().sharedMesh = best;
    }

    private static Mesh BuildTorusMesh(Transform torus, float a) {
        var mesh = Object.Instantiate(torus.GetComponent<MeshFilter>().sharedMesh);
        var surface = UVSurface.TorusSurface(1, a);
        surface.BuildMesh(mesh, 40, 10);
        torus.GetComponent<MeshFilter>().sharedMesh = mesh;
        return mesh;
    }

    public static void UpdateArc(Transform arc, float A, int slices = 100) {
        float R = 1;
        int V = 2 * slices + 2;
        A = A * Mathf.Deg2Rad;

        var mesh = arc.GetComponent<MeshFilter>().mesh;
        var vertices = new Vector3[V];
        var normals = new Vector3[V];
        var triangles = new int[6 * slices - 6];

        vertices[0] = vertices[1] = Vector3.zero;

        for (int i = 1; i <= slices; i++) {
            float a = (2 * A * (i - 1)) / (slices - 1) - A;
            float x = Mathf.Cos(a), z = Mathf.Sin(a);
            Vector3 vertex = R * new Vector3(x, 0, z);
            vertices[2 * i] = vertex;
            vertices[2 * i + 1] = vertex;
        }
        for (int i = 0; i <= slices; i++) {
            normals[2 * i] = Vector3.up;
            normals[2 * i + 1] = Vector3.down;
        }

        for (int i = 1, j = 0; i < slices; i++) {
            triangles[j++] = 0;
            triangles[j++] = 2 * i;
            triangles[j++] = 2 * i + 2;
            triangles[j++] = 1;
            triangles[j++] = 2 * i + 3;
            triangles[j++] = 2 * i + 1;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

}
