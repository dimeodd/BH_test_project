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
    SceneData _scene = null;

    [SyncVar]
    public bool isInvincible = false;


    public bool blockInput = false;

    void Awake()
    {
        _provider = GetComponent<PlayerProvider>();
    }

    public override void OnStartLocalPlayer()
    {
        _scene = World.Singleton.SceneData;

        _provider.cameraSwaper = new CameraSwaper(_provider, World.Singleton.SceneData);
        _provider.cameraSwaper.ToThirdViev();
        DisableCursor();
        World.Singleton.myIndex = netId;

        Restart();

        CmdPlayerRegister();
    }

    public override void OnStopServer()
    {
        World.Singleton.PlayerRemove(netId);
    }
    public override void OnStopLocalPlayer()
    {
        _provider.cameraSwaper.ToWaitViev();
        EnableCursor();
    }

    void Update()
    {
        if (blockInput)
        {
            _provider.moveScript.SetMoveDirection(new Vector2());
            return;
        }

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


    void OnGUI()
    {
        GUILayout.Label("Удерживаете \"Левый Alt\", чтобы использовать курсор");
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
    [Command]
    public void CmdPlayerRegister()
    {
        World.Singleton.PlayerRegistr(netId, this);
    }
    [Command]
    public void CmdPlayerRemove(uint a_netId)
    {
        Debug.Log("net PlayerRemove.  isServ:" + isServer);
        World.Singleton.PlayerRemove(netId);
    }


    [ClientRpc]
    public void RpcSetInvincible()
    {
        isInvincible = true;
        _provider.skinRenderer.material = StaticData.invincibleMaterial;
    }

    [ClientRpc]
    public void RpcRemoveInvincible()
    {
        isInvincible = false;
        _provider.skinRenderer.material = StaticData.defMaterial;
    }

    [ClientRpc]
    public void RpcShowWinWindow(string text)
    {
        if (isLocalPlayer)
        {
            EnableCursor();
            blockInput = true;
            _scene.winnerText.text = text;
            _scene.winnerWindow.SetActive(true);
        }
    }

    [ClientRpc]
    public void RpcRestart()
    {
        if (isLocalPlayer)
        {
            Restart();
        }
    }

    void Restart()
    {
        DisableCursor();
        blockInput = false;
        _scene.winnerWindow.SetActive(false);
        var pos = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
        transform.position = pos;
    }
}
