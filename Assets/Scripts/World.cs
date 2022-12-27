using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using System.Text;


public struct PlayerData
{
    public uint netIndex;
    public int Score;
    public string Name;
}

public class World : NetworkBehaviour
{
    #region SyncData

    SyncList<uint> _netIndexes = new SyncList<uint>();
    SyncList<int> _SyncScoreCount = new SyncList<int>();

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
    public void PlayerRegistr(uint targetNetId, PlayerInput_NetScript netScript)
    {
        Debug.Log("Add netID " + targetNetId);

        _netScriptDict.Add(targetNetId, netScript);
        _netIndexes.Add(targetNetId);
        _SyncScoreCount.Add(0);
    }
    [Server]
    public void PlayerRemove(uint targetNetId)
    {
        Debug.Log("Remove netID " + targetNetId);

        var index = _netIndexes.FindIndex(x => x == targetNetId);
        if (index != -1) _SyncScoreCount.RemoveAt(index);

        _netIndexes.Remove(targetNetId);
        _netScriptDict.Remove(targetNetId);
    }


    [Server]
    void EndOfGame(int winnerIndex)
    {
        var winnerName = "Player " + winnerIndex.ToString();
        foreach (var a_netId in _netIndexes)
        {
            _netScriptDict[a_netId].RpcShowWinWindow(winnerName);
        }
        StartCoroutine(RestartGame());
    }
    [Server]
    public IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(5f);

        //Сброс счёта
        for (int i = 0; i < _SyncScoreCount.Count; i++)
        {
            _SyncScoreCount[i] = 0;
        }

        var spawnPosList = ArrayExtention<NetworkStartPosition>.ConvertToList(ref _spawnPosArray);


        //Перезапуск клиентов
        foreach (var a_netId in _netIndexes)
        {
            var count = spawnPosList.Count;

            if (count > 0)
            {
                var rndIndex = UnityEngine.Random.Range(0, count - 1);
                var pos = spawnPosList[rndIndex].transform.position;
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
        var index = _netIndexes.FindIndex(x => x == targetNetId);
        if (index >= 0)
        {
            _SyncScoreCount[index]++;

            if (_SyncScoreCount[index] >= 3)
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

        GUILayout.Label("Players Count: " + _SyncScoreCount.Count);

        StringBuilder sb = new StringBuilder();
        var i = 0;
        foreach (var SCORE in _SyncScoreCount)
        {
            sb.Append("Player ");
            sb.Append(i);
            sb.Append("\tScore:");
            sb.Append(SCORE);
            if (myIndex == _netIndexes[i])
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
