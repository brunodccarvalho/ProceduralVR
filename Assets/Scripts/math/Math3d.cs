using UnityEngine;

public static class Math3d {

    // Determine the two closest points between two lines 1 and 2. Returns false if the
    // two lines are parallel, and otherwise returns the two closest points.
    // This works with bidirectional lines, not one-sided rays.
    public static bool ClosestPointOnTwoLines(Ray ray1, Ray ray2, out Vector3 closest1, out Vector3 closest2) {
        closest1 = closest2 = Vector3.zero;

        float a = Vector3.Dot(ray1.direction, ray1.direction);
        float b = Vector3.Dot(ray1.direction, ray2.direction);
        float e = Vector3.Dot(ray2.direction, ray2.direction);
        float d = a * e - b * b;

        if (d != 0.0f) {
            Vector3 r = ray1.origin - ray2.origin;
            float c = Vector3.Dot(ray1.direction, r);
            float f = Vector3.Dot(ray2.direction, r);
            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;
            closest1 = ray1.origin + ray1.direction * s;
            closest2 = ray2.origin + ray2.direction * t;
            return true;
        } else {
            return false;
        }
    }

    // Compute the intersection of a line with a plane.
    // Returns false if the ray is parallel to the plane, even if coplanar
    // This works with bidirectional lines, not one-sided rays.
    public static bool LinePlaneIntersection(Ray ray, Plane plane, out Vector3 hit) {
        hit = Vector3.zero;
        var planePoint = plane.ClosestPointOnPlane(Vector3.zero);
        float n = Vector3.Dot((planePoint - ray.origin), plane.normal);
        float d = Vector3.Dot(ray.direction.normalized, plane.normal);

        if (d != 0.0f) {
            hit = ray.origin + (n / d) * ray.direction.normalized;
            return true;
        } else {
            return false;
        }
    }

    // ***** Transform bounds

    public static Bounds GetWorldBounds(Transform o, bool keepGoing, bool includeCenter, System.Func<Transform, bool> ignore) {
        Bounds bounds = GetRenderBounds(o);
        if (includeCenter) {
            bounds = MergeBounds(bounds, new Bounds(o.position, Vector3.zero));
        }
        var extents = bounds.extents;
        if (extents.x == 0 || extents.y == 0 || extents.z == 0 || keepGoing) {
            foreach (Transform child in o) {
                if (child.gameObject.activeSelf && !ignore(child)) {
                    var childRender = child.GetComponent<Renderer>();
                    if (childRender) {
                        bounds = MergeBounds(bounds, childRender.bounds);
                    }
                    if (childRender == null || keepGoing) {
                        bounds = MergeBounds(bounds, GetWorldBounds(child));
                    }
                }
            }
        }
        return bounds;
    }

    public static Bounds GetWorldBounds(Transform o, bool keepGoing = true, bool includeCenter = false) {
        return GetWorldBounds(o, keepGoing, includeCenter, Interactive.IsAugmentation);
    }

    public static Bounds GetLocalBounds(Transform o, bool keepGoing, bool includeCenter, System.Func<Transform, bool> ignore) {
        Bounds bounds = GetMeshBounds(o);
        if (includeCenter) {
            bounds = MergeBounds(bounds, new Bounds(o.localPosition, Vector3.zero));
        }
        var extents = bounds.extents;
        if (extents.x == 0 || extents.y == 0 || extents.z == 0 || keepGoing) {
            foreach (Transform child in o) {
                if (child.gameObject.activeSelf && !ignore(child)) {
                    var filter = child.GetComponent<MeshFilter>();
                    Mesh childMesh = null;
                    if (filter != null) {
                        childMesh = filter.sharedMesh;
                        if (childMesh) {
                            bounds = MergeBounds(bounds, childMesh.bounds);
                        }
                    }
                    if (filter == null || childMesh == null || keepGoing) {
                        bounds = MergeBounds(bounds, GetLocalBounds(child));
                    }
                }
            }
        }
        return bounds;
    }

    public static Bounds GetLocalBounds(Transform o, bool keepGoing = true, bool includeCenter = false) {
        return GetLocalBounds(o, keepGoing, includeCenter, Interactive.IsAugmentation);
    }

    public static Bounds MergeBounds(Bounds a, Bounds b) {
        if (b.center == Vector3.zero && b.size == Vector3.zero) {
            return a;
        } else if (a.center == Vector3.zero && a.size == Vector3.zero) {
            return b;
        } else {
            a.Encapsulate(b);
            return a;
        }
    }

    public static Bounds GetRenderBounds(Transform o) {
        Renderer render = o.GetComponent<Renderer>();
        return render != null ? render.bounds : new Bounds();
    }

    public static Bounds GetMeshBounds(Transform o) {
        MeshFilter filter = o.GetComponent<MeshFilter>();
        if (filter != null) {
            Mesh mesh = filter.sharedMesh;
            return mesh != null ? mesh.bounds : new Bounds();
        } else {
            return new Bounds();
        }
    }

    // ***** Vector3 Snapping

    public static Vector3 Abs(Vector3 v) => Vector3.Max(v, -v);

    public static Vector3 FloorSnap(Vector3 v, Vector3 grid) {
        grid = Abs(grid);
        for (int i = 0; i < 3; i++) {
            if (grid[i] != 0.0f) {
                var size = grid[i];
                var block = v[i] / size;
                v[i] = Mathf.Floor(v[i] / size) * size;
            } else {
                v[i] = 0.0f;
            }
        }
        return v;
    }

    public static Vector3 CeilSnap(Vector3 v, Vector3 grid) {
        grid = Abs(grid);
        for (int i = 0; i < 3; i++) {
            if (grid[i] != 0.0f && v[i] != 0.0f) {
                var size = grid[i];
                var block = v[i] / size;
                v[i] = Mathf.Ceil(v[i] / size) * size;
            } else {
                v[i] = 0.0f;
            }
        }
        return v;
    }

    public static Vector3 GridSnap(Vector3 v, Vector3 grid, float threshold) {
        Debug.Assert(0 < threshold && threshold <= 0.5);
        grid = Abs(grid);
        Vector3 snap = v;
        for (int i = 0; i < 3; i++) {
            if (grid[i] != 0.0f) {
                var size = grid[i];
                var block = v[i] / size;
                snap[i] = Mathf.Round(v[i] / size) * size;
            }
        }
        if (Vector3.Distance(snap, v) < Vector3.Magnitude(grid) * threshold) {
            return snap;
        } else {
            return v;
        }
    }

    public static Vector3 GridlineSnap(Vector3 v, Vector3 grid, float threshold) {
        Debug.Assert(0 < threshold && threshold <= 0.5);
        for (int i = 0; i < 3; i++) {
            if (grid[i] != 0.0f) {
                var size = grid[i];
                var block = v[i] / size;
                int cell = Mathf.FloorToInt(block);
                var offset = block - cell;
                if (offset <= threshold) {
                    v[i] = size * cell;
                } else if (offset >= 1 - threshold) {
                    v[i] = size * (cell + 1);
                }
            }
        }
        return v;
    }

    public static Vector3 Snap(Vector3 v, Vector3 threshold, Vector3 origin) {
        for (int i = 0; i < 3; i++) {
            if (Mathf.Abs(v[i] - origin[i]) <= threshold[i]) {
                v[i] = origin[i];
            }
        }
        return v;
    }

    public static Vector3 Snap(Vector3 v, Vector3 threshold) {
        return Snap(v, threshold, Vector3.zero);
    }

    public static Vector3 ChooseInBox(Vector3 box, bool alsoNegative = true) {
        box = Abs(box);
        if (alsoNegative) {
            return new Vector3(
                box.x != 0 ? Random.Range(-box.x, box.x) : 0,
                box.y != 0 ? Random.Range(-box.y, box.y) : 0,
                box.z != 0 ? Random.Range(-box.z, box.z) : 0
            );
        } else {
            return new Vector3(
                box.x > 0 ? Random.Range(0, box.x) : 0,
                box.y > 0 ? Random.Range(0, box.y) : 0,
                box.z > 0 ? Random.Range(0, box.z) : 0
            );
        }
    }

    // ***** Formatting...

    public static string FormatTreeTransform(Transform transform) {
        return string.Format("{3}[{0};{1};{2}]", FormatVector(transform.localPosition),
                                                FormatQuaternion(transform.localRotation),
                                                FormatVector(transform.localScale),
                                                transform.gameObject.activeSelf ? "+" : "-");
    }

    public static string FormatVector(Vector3 v) {
        return string.Format("({0},{1},{2})",
            v.x.ToString(), v.y.ToString(), v.z.ToString());
    }

    public static string FormatQuaternion(Quaternion q) {
        return string.Format("({0},{1},{2},{3})",
            q.x.ToString(), q.y.ToString(), q.z.ToString(), q.w.ToString());
    }

    public static Vector3 ParseVector3(string s) {
        s = s.TrimStart('('); s = s.TrimEnd(')');
        var items = s.Split(',');
        Debug.AssertFormat(items.Length == 3, "Invalid number of items");
        var x = float.Parse(items[0].Trim());
        var y = float.Parse(items[1].Trim());
        var z = float.Parse(items[2].Trim());
        return new Vector3(x, y, z);
    }

    public static Quaternion ParseQuaternion(string s) {
        s = s.TrimStart('('); s = s.TrimEnd(')');
        var items = s.Split(',');
        Debug.AssertFormat(items.Length == 4, "Invalid number of items");
        var x = float.Parse(items[0].Trim());
        var y = float.Parse(items[1].Trim());
        var z = float.Parse(items[2].Trim());
        var w = float.Parse(items[3].Trim());
        return new Quaternion(x, y, z, w);
    }


}
