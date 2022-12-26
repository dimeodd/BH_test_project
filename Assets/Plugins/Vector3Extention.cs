using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtention
{
    /// <summary>
    /// Конвертирует из v3.xz в v2.xy
    /// </summary>
    public static Vector2 GetXZ_v2(this Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }

    /// <summary>
    /// Конвертирует из v2.xy в v3.xz
    /// </summary>
    public static Vector3 GetXZ_v3(this Vector2 v3)
    {
        return new Vector3(v3.x, 0, v3.y);
    }
}
