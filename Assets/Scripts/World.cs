using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using MyEcs;
using EcsStructs;
using EcsSystems;

public class World : NetworkBehaviour
{
    static World _singleton;
    public static World Singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindObjectOfType<World>();
            }
            return _singleton;
        }
    }

    public StaticData StaticData;
    public SceneData SceneData;

    EcsWorld _world;
    EcsSystem _allSys, _upd, _fixUpd;

    void OnDestroy()
    {
        _singleton = null;
    }


    [Server]
    public override void OnStartServer()
    {
        if (!isServer) return;

        InitSpawnPoints();

        if (_world != null)
            throw new System.Exception("EcsWorld не очищен");
        _world = new EcsWorld();

        // _upd = new EcsSystem(_world)
        //     .Add(new InpytSystem())
        //     ;

        // _fixUpd = new EcsSystem(_world)
        //     .Add(new MoveSystem())
        //     ;

        // _allSys = new EcsSystem(_world)
        //     .Add(_upd)
        //     .Add(_fixUpd)
        //     .Inject(StaticData)
        //     .Inject(SceneData)
        //     ;
        // _allSys.Init();
    }

    [Server]
    public override void OnStopServer()
    {
        _world.Dispose();
        _world = null;
    }

    [Server]
    void Update()
    {
        // _upd.Upd();
    }

    [Server]
    void FixedUpdate()
    {
        // _fixUpd.Upd();
    }


    void InitSpawnPoints()
    {
        var goArray = GameObject.FindGameObjectsWithTag("Respawn");
        SceneData.SpawnPositions = new Transform[goArray.Length];

        for (int i = 0, iMax = goArray.Length; i < iMax; i++)
        {
            SceneData.SpawnPositions[i] = goArray[i].transform;
        }
    }

    [Server]
    internal void CreatePlayer(uint a_netId, PlayerInput_NetScript inputScript)
    {
        var ecsWorld = World.Singleton._world;
        var stData = World.Singleton.StaticData;
        var sceneData = World.Singleton.SceneData;

        var rndIndex = Random.Range(0, sceneData.SpawnPositions.Length - 1);
        var rndPos = sceneData.SpawnPositions[rndIndex];

        var playerGo = inputScript.gameObject;
        playerGo.transform.position = rndPos.position;
        var playerProvider = playerGo.GetComponent<PlayerProvider>();

        var ent = ecsWorld.NewEntity();
        MyEcs.GoPool.Helper.LinkObjects(ent, playerGo);

        ref var playerData = ref ent.Get<PlayerData>();
        playerData.followTarget = playerProvider.followTarget;

        ref var input = ref ent.Get<InputData>();
        input.HorizontalRotation = 0;
        input.VerticalRotation = 0;

        ref var netData = ref ent.Get<NetData>();
        netData.netId = a_netId;

        ref var rbData = ref ent.Get<RigidbodyData>();
        rbData.rigidbody = playerGo.GetComponent<Rigidbody>();

        ref var tfData = ref ent.Get<TransformData>();
        tfData.transform = playerGo.transform;

        inputScript.playerEnt = ent;
        inputScript.playerGo = playerGo;
    }

    [Server]
    internal void DestroyPlayer(PlayerInput_NetScript inputScript)
    {
        inputScript.playerEnt.Destroy();
    }
}
