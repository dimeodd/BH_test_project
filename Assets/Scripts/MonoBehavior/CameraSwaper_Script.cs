using UnityEngine;

public class CameraSwaper_Script : MonoBehaviour
{
    public Transform followTarget;
    SceneData _scene;

    void Start()
    {
        _scene = World.Singleton.SceneData;
        ToThirdViev();
    }
    void OnDestroy()
    {
        if (_scene.followCamera.Follow == followTarget)
            ToWaitViev();
    }

    public void ToThirdViev()
    {
        _scene.followCamera.Follow = followTarget;
        _scene.waitCamera.Priority = 0;
    }

    public void ToWaitViev()
    {
        _scene.waitCamera.Priority = 100;
    }
}
