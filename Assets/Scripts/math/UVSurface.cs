using System.Collections.Generic;
using UnityEngine;

using F2 = System.Func<float, float, UnityEngine.Vector2>;
using F3 = System.Func<float, float, UnityEngine.Vector3>;

/**
 * Representation of a UV surface with code to write a mesh.
 */
public class UVSurface {
    F3 pointfn;
    F3 normfn;
    F2 texfn;
    float[] boundary;

    public UVSurface(F3 pointfn) {
        this.pointfn = pointfn;
    }

    public UVSurface(F3 pointfn, F3 normfn) {
        this.pointfn = pointfn; this.normfn = normfn;
    }

    public UVSurface(F3 pointfn, F3 normfn, F2 texfn) {
        this.pointfn = pointfn; this.normfn = normfn; this.texfn = texfn;
    }

    public void SetBoundary(float[] boundary) {
        Debug.Assert(boundary.Length == 4);
        this.boundary = boundary;
    }

    public Vector3 Point(float u, float v) {
        return pointfn(u, v);
    }

    public Vector3 Normal(float u, float v, float uDelta, float vDelta) {
        if (normfn != null) {
            return normfn(u, v);
        } else {
            var point = Point(u, v);
            var uoff = Point(u + uDelta / 256f, v);
            var voff = Point(u, v + vDelta / 256f);
            var utangent = uoff - point;
            var vtangent = voff - point;
            return Vector3.Cross(utangent, vtangent).normalized;
        }
    }

    public Vector2 Tex(float u, float v, float uOff, float vOff) {
        if (texfn != null) {
            return texfn(u, v);
        } else {
            return new Vector2(uOff, vOff);
        }
    }

    // Setup a UV surface mesh, see https://github.com/brunodccarvalho/CGRA/blob/master/proj/build/uvSurface.js
    public void BuildMesh(Mesh mesh, int uslices, int vslices = -1) {
        if (vslices == -1) vslices = uslices;

        float minU = boundary[0];
        float maxU = boundary[1];
        float minV = boundary[2];
        float maxV = boundary[3];

        float uDelta = (maxU - minU) / uslices;
        float vDelta = (maxV - minV) / vslices;

        var vertices = new List<Vector3>();
        var indices = new List<int>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();

        for (int j = 0; j <= vslices; j++) {
            for (int i = 0; i <= uslices; i++) {
                float u = minU + uDelta * i;
                float v = minV + vDelta * j;
                float uOff = 1.0f * i / uslices;
                float vOff = 1.0f * j / vslices;
                Vector3 point = Point(u, v);
                Vector3 normal = Normal(u, v, uDelta, vDelta);
                Vector2 tex = Tex(u, v, uOff, vOff);

                // Up vertex
                vertices.Add(point);
                normals.Add(normal);
                uvs.Add(tex);

                // Down vertex
                vertices.Add(point);
                normals.Add(-normal);
                uvs.Add(tex);
            }
        }

        for (int j = 0; j < vslices; j++) {
            for (int i = 0; i < uslices; i++) {
                int above = 2 * uslices + 2;
                int next = 2, right = 2;
                int line = j * above;
                int current = next * i + line;

                // ... v4U v4D      v3U v3D ... --- line x + 1
                //
                // ... v1U v1D      v2U v2D ... --- line x
                int v1U = current;
                int v2U = current + right;
                int v3U = current + right + above;
                int v4U = current + above;
                int v1D = 1 + v1U;
                int v2D = 1 + v2U;
                int v3D = 1 + v3U;
                int v4D = 1 + v4U;

                // Unity uses clockwise order
                indices.AddRange(new int[3] { v1U, v3U, v2U });
                indices.AddRange(new int[3] { v1U, v4U, v3U });

                indices.AddRange(new int[3] { v1D, v2D, v3D });
                indices.AddRange(new int[3] { v1D, v3D, v4D });
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    public static UVSurface TorusSurface(float c, float a) {
        var surface = new UVSurface((u, v) => new Vector3(
            (c + a * Mathf.Cos(v)) * Mathf.Cos(u),
            a * Mathf.Sin(v),
            (c + a * Mathf.Cos(v)) * Mathf.Sin(u)
        ));
        var tau = 2 * Mathf.PI;
        surface.SetBoundary(new float[4] { 0, tau, 0, tau });
        return surface;
    }
}
