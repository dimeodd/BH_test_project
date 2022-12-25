using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    static World _singleton;
    public static World Singleton => _singleton;

    public StaticData StaticData;
    public SceneData SceneData;

    void Start()
    {
        if (_singleton != null)
        {
            throw new Exception("синглтон не очищен");
        }
        _singleton = this;
    }
}
