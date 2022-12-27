using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using System.Text;


public struct PlayerData
{
    public uint NetIndex;
    public int Score;
    public string Name;
}

public class World : NetworkBehaviour
{
    #region SyncData

    SyncList<PlayerData> _SyncPlayerData = new SyncList<PlayerData>();

    #endregion

    #region ServerCode

    Dictionary<uint, PlayerInput_NetScript> _netScriptDict = null;

    NetworkStartPosition[] _spawnPosArray = null;

    //Serever Start
    public override void OnStartServer()
    {

        _spawnPosArray = GameObject.FindObjectsOfType<NetworkStartPosition>();
        _netScriptDict = new Dictionary<uint, PlayerInput_NetScript>();
    }
    public override void OnStopServer()
    {
        _netScriptDict.Clear();
        _netScriptDict = null;
    }

    [Server]
    public void PlayerRegistr(uint targetNetId, PlayerInput_NetScript netScript, string PlayerName)
    {
        Debug.Log("Add netID " + targetNetId);

        var playerData = new PlayerData();
        playerData.NetIndex = targetNetId;

        if (PlayerName.Length == 0)
        {
            PlayerName = "NoName";
        }
        playerData.Name = PlayerName;
        var i = 0;
        var tryCount = 20;
        while (_SyncPlayerData.FindIndex(x => x.Name == playerData.Name) != -1)
        {
            playerData.Name = PlayerName + i++;
            if (--tryCount < 0)
                break;
        }

        _SyncPlayerData.Add(playerData);
        _netScriptDict.Add(targetNetId, netScript);

        //Update Names
        foreach (var data in _SyncPlayerData)
        {
            var data_netScript = _netScriptDict[data.NetIndex];
            data_netScript.RpcSetName(data.Name);
        }
    }
    [Server]
    public void PlayerRemove(uint targetNetId)
    {
        Debug.Log("Remove netID " + targetNetId);

        var index = _SyncPlayerData.FindIndex(x => x.NetIndex == targetNetId);
        if (index != -1) _SyncPlayerData.RemoveAt(index);

        _netScriptDict.Remove(targetNetId);
    }


    [Server]
    void EndOfGame(int winnerIndex)
    {
        var winnerName = _SyncPlayerData[winnerIndex].Name;

        foreach (var data in _SyncPlayerData)
        {
            _netScriptDict[data.NetIndex].RpcShowWinWindow(winnerName);
        }
        StartCoroutine(RestartGame());
    }
    [Server]
    public IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(5f);

        //Сброс счёта
        for (int i = 0; i < _SyncPlayerData.Count; i++)
        {
            var data = _SyncPlayerData[i];
            data.Score = 0;
            _SyncPlayerData[i] = data;
        }

        var spawnPosList = ArrayExtention<NetworkStartPosition>.ConvertToList(ref _spawnPosArray);

        //Перезапуск клиентов
        foreach (var data in _SyncPlayerData)
        {
            var a_netId = data.NetIndex;
            var count = spawnPosList.Count;

            if (count > 0)
            {
                var rndIndex = UnityEngine.Random.Range(0, count - 1);
                var pos = spawnPosList[rndIndex].transform.position.GetXZ_v2();
                spawnPosList.RemoveAt(rndIndex);

                _netScriptDict[a_netId].RpcRestart(pos);
            }
            else
            {
                _netScriptDict[a_netId].RpcRestart(new Vector2());
            }
        }
    }


    [Server]
    public void DashHit(uint owner, uint targetNetId)
    {
        AddPoint(owner);

        var otherNetScript = _netScriptDict[targetNetId];
        otherNetScript.RpcSetInvincible();
        StartCoroutine(RemoveInvincible(otherNetScript));
    }
    [Server]
    public void AddPoint(uint targetNetId)
    {
        var index = _SyncPlayerData.FindIndex(x => x.NetIndex == targetNetId);
        if (index >= 0)
        {
            var data = _SyncPlayerData[index];
            data.Score++;
            _SyncPlayerData[index] = data;

            if (data.Score >= 3)
            {
                EndOfGame(index);
            }
        }
    }
    [Server]
    public IEnumerator RemoveInvincible(PlayerInput_NetScript netScript)
    {
        yield return new WaitForSeconds(StaticData.invincibleCooldown_sec);
        netScript.RpcRemoveInvincible();
    }

    #endregion

    #region ClientCode

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

    internal uint myIndex = 0;
    GUIStyle selectedStyle = new GUIStyle();

    void Awake()
    {
        selectedStyle.normal.textColor = Color.red;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 100, 200, 500));

        GUILayout.Label("Players Count: " + _SyncPlayerData.Count);

        StringBuilder sb = new StringBuilder();
        var i = 0;
        foreach (var data in _SyncPlayerData)
        {
            sb.Append(data.Name);
            sb.Append("\tScore:");
            sb.Append(data.Score);
            if (myIndex == data.NetIndex)
            {
                GUILayout.Label(sb.ToString(), selectedStyle);
            }
            else
            {
                GUILayout.Label(sb.ToString());
            }

            sb.Clear();
            i++;
        }

        GUILayout.EndArea();
    }

    #endregion
}
