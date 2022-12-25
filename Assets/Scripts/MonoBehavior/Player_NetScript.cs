using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player_NetScript : NetworkBehaviour
{
    public StaticData StaticData;
    public Renderer SkinRenderer;
    public Transform followTarget;

    Transform _locTransform;

    public override void OnStartClient()
    {
        _locTransform = transform;

        if (isLocalPlayer)
        {
            World.Singleton.SceneData.followCamera.Follow = followTarget;
            World.Singleton.SceneData.waitCamera.Priority = 0;
            _locTransform.LookAt(new Vector3(), Vector3.up);
        }
    }

    public override void OnStopClient()
    {
        if (isLocalPlayer)
        {
            World.Singleton.SceneData.waitCamera.Priority = 100;
        }
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            var inputFront = Input.GetAxis("Vertical");
            var inputSide  = Input.GetAxis("Horizontal");
            var speed = StaticData.playerSpeed * Time.deltaTime;

            var moveDir = new Vector2(inputFront, inputSide).normalized;
            var aimDir = (_locTransform.rotation * Vector3.forward).GetXZ();

            var front = Vector2.Dot(aimDir, moveDir);
            var side = Vector2.Dot(aimDir, Vector2.Perpendicular(moveDir));

            transform.position += new Vector3(front, 0, side) * speed;
        }
    }

    void SetDefMaterial()
    {
        SkinRenderer.material = StaticData.defMaterial;
    }

    void SetInvincibleMaterial()
    {
        SkinRenderer.material = StaticData.invincibleMaterial;
    }
}
