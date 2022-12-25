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
    public static World Singleton => _singleton;

    public StaticData StaticData;
    public SceneData SceneData;

    EcsWorld _world;
    EcsSystem _allSys, _upd, _fixUpd;


    [Server]
    public override void OnStartServer()
    {
        if (!isServer) return;

        InitSpawnPoints();

        if (_singleton != null)
            throw new System.Exception("синглтон не очищен");
        _singleton = this;

        if (_world != null)
            throw new System.Exception("EcsWorld не очищен");
        _world = new EcsWorld();

        _upd = new EcsSystem(_world)
            .Add(new InpytSystem())
            .Add(new PlayerAnimationSystem())
            ;

        _fixUpd = new EcsSystem(_world)
            .Add(new MoveSystem())
            ;

        _allSys = new EcsSystem(_world)
            .Add(_upd)
            .Add(_fixUpd)
            .Inject(StaticData)
            .Inject(SceneData)
            ;
        _allSys.Init();
    }


    public override void OnStopServer()
    {
        _singleton = null;
        _world.Dispose();
        _world = null;
    }

    void Update()
    {
        _upd.Upd();
    }

    void FixedUpdate()
    {
        _fixUpd.Upd();
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

        var playerGo = Instantiate(stData.playerPrefab, rndPos.position, Quaternion.identity);
        NetworkServer.Spawn(playerGo);

        var ent = ecsWorld.NewEntity();
        MyEcs.GoPool.Helper.LinkObjects(ent, playerGo);

        ref var playerData = ref ent.Get<PlayerData>();
        playerData.animator = playerGo.GetComponent<Animator>();

        ref var input = ref ent.Get<InputData>();
        input.rotation = playerGo.transform.rotation;

        ref var netData = ref ent.Get<NetData>();
        netData.netId = a_netId;
        netData.animatorProvider = playerGo.GetComponent<AnimatorProvider_NetSctipt>(); ;

        ref var rbData = ref ent.Get<RigidbodyData>();
        rbData.rigidbody = playerGo.GetComponent<Rigidbody>();

        inputScript.playerEnt = ent;
        inputScript.playerGo = playerGo;
        inputScript.rotation = input.rotation;
    }

    [Server]
    internal void DestroyPlayer(PlayerInput_NetScript inputScript)
    {
        NetworkServer.Destroy(inputScript.playerGo);
        Destroy(inputScript.playerGo, 0.001f);
        inputScript.playerEnt.Destroy();
    }
}
