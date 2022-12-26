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

    #region GUI & Cursor

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

    #region Win

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
    public void RpcRestartUI()
    {
        if (isLocalPlayer)
        {
            RestartUI();

            var spawnPosArray = GameObject.FindObjectsOfType<NetworkStartPosition>();

            var rndIndex = Random.Range(0, spawnPosArray.Length);
            var pos = spawnPosArray[rndIndex].transform.position;
            transform.position = pos;
        }
    }
    void RestartUI()
    {
        DisableCursor();
        _cameraSwaper.ToThirdViev();

        blockInput = false;
        _scene.winnerWindow.SetActive(false);
    }

    #endregion 
}
