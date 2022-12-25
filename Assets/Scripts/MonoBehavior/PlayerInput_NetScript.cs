using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MyEcs;
using EcsStructs;

public class PlayerInput_NetScript : NetworkBehaviour
{
    [SyncVar(hook = nameof(SyncInput))]
    public Vector2 _input;
    [SyncVar(hook = nameof(SyncRotation))]
    public Quaternion rotation;

    public Transform followTarget;

    [HideInInspector] public Entity playerEnt;
    [HideInInspector] public GameObject playerGo;

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            CmdCreatePlayer(netId);
        }
    }
    public override void OnStopClient()
    {
        if (isServer)
        {
            World.Singleton.DestroyPlayer(this);
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        var inputFront = Input.GetAxis("Vertical");
        var inputSide = Input.GetAxis("Horizontal");
        var newVec = new Vector2(inputFront, inputSide);
        _input = newVec;
    }


    [Command]
    void CmdCreatePlayer(uint a_netId)
    {
        World.Singleton.CreatePlayer(a_netId, this);
    }

    void SyncInput(Vector2 oldValue, Vector2 newValue)
    {
        if (!isServer || playerEnt.IsDestroyed()) return;

        ref var input = ref playerEnt.Get<InputData>();
        input.move = newValue;
    }

    void SyncRotation(Quaternion oldValue, Quaternion newValue)
    {
        if (!isServer || playerEnt.IsDestroyed()) return;

        ref var input = ref playerEnt.Get<InputData>();
        input.rotation = newValue;
    }
}
