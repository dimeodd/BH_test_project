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

    void OnGUI()
    {
        GUILayout.Label("Удерживаете \"Левый Alt\", чтобы использовать курсор");
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            _provider.cameraSwaper = new CameraSwaper(_provider, World.Singleton.SceneData);
            _provider.cameraSwaper.ToThirdViev();
            DisableCursor();
        }

        if (isServer)
        {
            World.Singleton.PlayerRegistr(netId, this);
        }
    }
    public override void OnStopClient()
    {
        if (isLocalPlayer)
        {
            _provider.cameraSwaper.ToWaitViev();
            EnableCursor();
        }

        if (isServer)
        {
            World.Singleton.PlayerRemove(netId);
        }
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            CheckAltButton();

            var moveDir = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
            _provider.moveScript.SetMoveDirection(moveDir);

            if (!Cursor.visible)
            {
                var mouseOffset = new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")) * StaticData.mouseSensetivity * 0.01f;
                _provider.moveScript.SetLookOffset(mouseOffset * 0.05f);

                if (Input.GetMouseButtonDown(0))
                {
                    var dash = new DashSystem(_provider, StaticData, this);
                    dash.UseSkill();
                }
            }
        }
    }

    void CheckAltButton()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            EnableCursor();
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            DisableCursor();
        }
    }

    void EnableCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void DisableCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    [Command]
    public void CmdDashHit(uint owner, uint target)
    {
        World.Singleton.DashHit(owner, target);
    }


    [ClientRpc]
    public void RpcDamage()
    {
        _provider.skinRenderer.material = StaticData.invincibleMaterial;
    }
}
