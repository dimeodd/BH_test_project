using UnityEngine;
using Mirror;

public class CameraSwaper_Script : NetworkBehaviour
{
    public Transform followTarget;
    SceneData _scene;

    public override void OnStartClient()
    {
        Debug.Log("CameraSwaper_Script 1");
        if (isLocalPlayer)
        {
            Debug.Log("CameraSwaper_Script 2");
            _scene = World.Singleton.SceneData;
            ToThirdViev();
        }
    }
    public override void OnStopClient()
    {
        if (isLocalPlayer)
        {
            ToWaitViev();
        }
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
