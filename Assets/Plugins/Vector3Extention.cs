using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtention
{
    public static Vector2 GetXZ(this Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }
    public static Vector3 GetXZ(this Vector2 v3)
    {
        return new Vector3(v3.x, 0, v3.y);
    }
}
