using UnityEngine;

public class CameraSwaper_Script : MonoBehaviour
{
    SceneData _scene;
    PlayerProvider provider = null;

    void Awake()
    {
        provider = GetComponent<PlayerProvider>();
    }

    public void OnStartClient()
    {
        _scene = World.Singleton.SceneData;
        ToThirdViev();
    }
    public void OnStopClient()
    {
        ToWaitViev();
    }

    public void ToThirdViev()
    {
        _scene.followCamera.Follow = provider.followTarget;
        _scene.waitCamera.Priority = 0;
    }

    public void ToWaitViev()
    {
        _scene.waitCamera.Priority = 100;
    }
}
