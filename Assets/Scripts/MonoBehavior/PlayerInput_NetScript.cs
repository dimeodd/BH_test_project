using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MyEcs;
using EcsStructs;

public class PlayerInput_NetScript : NetworkBehaviour
{
    public StaticData StaticData;

    [HideInInspector] public Entity playerEnt;
    [HideInInspector] public GameObject playerGo;

    PlayerProvider _provider = null;
    void Awake()
    {
        _provider = GetComponent<PlayerProvider>();
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            _provider.cameraSwaper = new CameraSwaper(_provider, World.Singleton.SceneData);
            _provider.cameraSwaper.ToThirdViev();
        }
    }
    public override void OnStopClient()
    {
        if (isLocalPlayer)
        {
            _provider.cameraSwaper.ToWaitViev();
        }
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            var moveDir = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
            _provider.moveScript.SetMoveDirection(moveDir);

            var mouseOffset = new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")) * StaticData.mouseSensetivity * 0.01f;
            _provider.moveScript.SetLookOffset(mouseOffset * 0.05f);
            if (Input.GetMouseButtonDown(0))
            {
                var dash = new DashSystem(_provider, StaticData);
                dash.UseSkill();
            }
        }
    }


    // [Command]
    // void CmdCreatePlayer(uint a_netId)
    // {
    //     World.Singleton.CreatePlayer(a_netId, this);
    // }

    // void SyncInput(Vector2 oldValue, Vector2 newValue)
    // {
    //     if (!isServer || playerEnt.Is;Destroyed()) return;

    //     ref var input = ref playerEnt.Get<InputData>();
    //     input.move = newValue;
    // }

    // void SyncLook(Vector2 oldValue, Vector2 newValue)
    // {
    //     if (!isServer || playerEnt.IsDestroyed()) return;

    //     ref var input = ref playerEnt.Get<InputData>();
    //     input.HorizontalRotation = newValue.x;
    //     input.VerticalRotation = newValue.y;
    // }

}
