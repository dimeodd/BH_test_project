using UnityEngine;
using Mirror;

public class PlayerInput_NetScript : NetworkBehaviour
{
    [SyncVar]
    public bool isInvincible = false;

    public bool blockInput = false;
    public StaticData StaticData;

    SceneData _scene = null;
    PlayerProvider _provider = null;
    CameraSwaper _cameraSwaper = null;

    void Awake()
    {
        _provider = GetComponent<PlayerProvider>();
    }

    public override void OnStartLocalPlayer()
    {
        _scene = World.Singleton.SceneData;
        _cameraSwaper = new CameraSwaper(_provider, World.Singleton.SceneData);

        World.Singleton.myIndex = netId;

        RestartUI();
        RandomSpawn();
    }
    public override void OnStartServer()
    {
        World.Singleton.PlayerRegistr(netId, this);
    }

    public override void OnStopLocalPlayer()
    {
        _cameraSwaper.ToWaitViev();
        EnableCursor();
    }
    public override void OnStopServer()
    {
        World.Singleton.PlayerRemove(netId);
    }

    void Update()
    {
        if (!isLocalPlayer) return;


        if (blockInput)
        {
            _provider.moveScript.SetMoveDirection(new Vector2());
            if (!Cursor.visible) EnableCursor();
            return;
        }

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

    #region GUI & Cursor
    void OnGUI()
    {
        GUILayout.Label("Удерживаете \"Левый Alt\", чтобы использовать курсор");
    }
    void CheckAltButton()
    {
        var altPressedNow = Input.GetKey(KeyCode.LeftAlt);

        if (altPressedNow & !Cursor.visible)
        {
            EnableCursor();
        }
        if (!altPressedNow & Cursor.visible)
        {
            DisableCursor();
        }
    }
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            DisableCursor();
        else
            EnableCursor();
    }
    void EnableCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    void DisableCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #endregion

    #region Dash

    [Command]
    public void CmdDashHit(uint owner, uint target)
    {
        World.Singleton.DashHit(owner, target);
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

    #endregion


    [ClientRpc]
    public void RpcShowWinWindow(string text)
    {
        if (isLocalPlayer)
        {
            EnableCursor();
            _cameraSwaper.ToWaitViev();

            blockInput = true;
            _scene.winnerText.text = text;
            _scene.winnerWindow.SetActive(true);
        }
    }
    [ClientRpc]
    public void RpcRestart(Vector2 spawnPos)
    {
        if (isLocalPlayer)
        {
            RestartUI();
            transform.position = spawnPos;
        }
    }
    void RestartUI()
    {
        DisableCursor();
        _cameraSwaper.ToThirdViev();

        blockInput = false;
        _scene.winnerWindow.SetActive(false);
    }

    void RandomSpawn()
    {
        var spawnPosArray = GameObject.FindObjectsOfType<NetworkStartPosition>();

        //Random spawn
        var rndIndex = Random.Range(0, spawnPosArray.Length);
        var pos = spawnPosArray[rndIndex].transform.position;
        if (!Physics.CheckSphere(pos, 0.6f, StaticData.playerMask))
        {
            transform.position = pos;
            return;
        }

        //Classic spawn if place is taken
        for (int i = 0, iMax = spawnPosArray.Length; i < iMax; i++)
        {
            pos = spawnPosArray[i].transform.position;
            if (!Physics.CheckSphere(pos, 0.6f, StaticData.playerMask))
            {
                transform.position = pos;
                return;
            }
        }

        //Force spawn
        transform.position = new Vector2();
    }

}
