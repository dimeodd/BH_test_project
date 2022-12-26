using UnityEngine;

public class CameraSwaper
{
    SceneData _scene;
    PlayerProvider _provider = null;

    public CameraSwaper(PlayerProvider provider, SceneData scene)
    {
        _provider = provider;
        _scene = scene;
    }

    public void ToThirdViev()
    {
        _scene.followCamera.Follow = _provider.followTarget;
        _scene.waitCamera.Priority = 0;
    }

    public void ToWaitViev()
    {
        _scene.waitCamera.Priority = 100;
    }
}
