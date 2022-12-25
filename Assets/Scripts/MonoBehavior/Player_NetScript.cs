using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player_NetScript : NetworkBehaviour
{
    public StaticData StaticData;
    public Renderer SkinRenderer;

    public Transform followTarget;
    Rigidbody _rigidbody;
    Transform _transform;
    bool _hasFocus = true;
    Vector3 _velocity;

    

    public override void OnStartClient()
    {
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();

        if (!isLocalPlayer) return;

        World.Singleton.SceneData.followCamera.Follow = followTarget;
        World.Singleton.SceneData.waitCamera.Priority = 0;
        _transform.LookAt(new Vector3(), Vector3.up);

        DisableCursor();
    }
    public override void OnStopClient()
    {
        if (!isLocalPlayer) return;

        World.Singleton.SceneData.waitCamera.Priority = 100;

        EnableCursor();
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        CheckAltButton();
        MoveInput();
        if (_hasFocus)
        {
            LookInput();
        }
    }

    void CheckAltButton()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            _hasFocus = false;
            EnableCursor();
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            _hasFocus = true;
            DisableCursor();
        }
    }

    void MoveInput()
    {
        //input
        var inputFront = Input.GetAxis("Vertical");
        var inputSide = Input.GetAxis("Horizontal");
        var speed = StaticData.playerSpeed;// * Time.deltaTime;

        //смещение игрока относительно взгляда
        var moveDir = new Vector2(inputFront, inputSide).normalized;
        var aimDir = (_transform.rotation * Vector3.forward).GetXZ();
        var front = Vector2.Dot(aimDir, moveDir);
        var side = Vector2.Dot(aimDir, Vector2.Perpendicular(moveDir));

        _velocity = new Vector3(front, 0, side) * speed;
    }

    void FixedUpdate()
    {
        _rigidbody.velocity = _velocity;
    }



    void LookInput()
    {
        var mouseOffset = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        mouseOffset *= 0.01f;

        //горизонтальный поворот камеры
        _transform.rotation *= new Quaternion(0, mouseOffset.x, 0, 1);

        //вертикальный поворот камеры
        var newVerticalRotation = followTarget.rotation * new Quaternion(-mouseOffset.y, 0, 0, 1);
        var verticalAngle = newVerticalRotation.eulerAngles.x;
        if (verticalAngle < 180 && verticalAngle > StaticData.upMaxAngle ||
            verticalAngle > 180 && verticalAngle < 360 - StaticData.downMaxAngle)
        {
            return;
        }

        followTarget.rotation = newVerticalRotation;
    }

    void SetDefMaterial()
    {
        SkinRenderer.material = StaticData.defMaterial;
    }
    void SetInvincibleMaterial()
    {
        SkinRenderer.material = StaticData.invincibleMaterial;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        _hasFocus = hasFocus;
        if (hasFocus)
            DisableCursor();
        else
            EnableCursor();
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

    void OnGUI()
    {
        GUILayout.Label("Удерживаете \"Левый Alt\", чтобы использовать курсор");
    }
}
