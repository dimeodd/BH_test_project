using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using System;
using System.Text;

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

    Dictionary<uint, PlayerInput_NetScript> _netScriptDict = new Dictionary<uint, PlayerInput_NetScript>();
    SyncList<uint> _netIndexes = new SyncList<uint>();
    SyncList<int> _SyncScoreCount = new SyncList<int>();

    [Server]
    public void PlayerRegistr(uint targetNetId, PlayerInput_NetScript netScript)
    {
        Debug.Log("Add netID " + targetNetId);
        if (!_netScriptDict.TryAdd(targetNetId, netScript))
        {
            if (_netScriptDict[targetNetId] == null)
            {
                PlayerRemove(targetNetId);
            }
            _netScriptDict.Remove(targetNetId);
            _netScriptDict.Add(targetNetId, netScript);
        }
        _netIndexes.Add(targetNetId);
        _SyncScoreCount.Add(0);
    }
    [Server]
    public void PlayerRemove(uint targetNetId)
    {
        Debug.Log("Remove netID " + targetNetId);
        var index = _netIndexes.FindIndex(x => x == targetNetId);
        if (index >= 0) _SyncScoreCount.RemoveAt(index);
        _netIndexes.Remove(targetNetId);

        _netScriptDict.Remove(targetNetId);
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
    public void DashHit(uint owner, uint targetNetId)
    {
        AddPoint(owner);

        var otherNetScript = _netScriptDict[targetNetId];
        otherNetScript.RpcSetInvincible();
        StartCoroutine(RemoveInvincible(otherNetScript));
    }

    [Server]
    public IEnumerator RemoveInvincible(PlayerInput_NetScript netScript)
    {
        yield return new WaitForSeconds(StaticData.invincibleCooldown_sec);
        netScript.RpcRemoveInvincible();
    }
    [Server]
    public IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(5f);

        for (int i = 0; i < _SyncScoreCount.Count; i++)
        {
            _SyncScoreCount[i] = 0;
        }

        foreach (var a_netId in _netIndexes)
        {
            _netScriptDict[a_netId].RpcRestart();
        }
    }

    #region Client

    internal uint myIndex = 0;
    GUIStyle selectedStyle = null;

    void Awake()
    {
        selectedStyle = new GUIStyle();
        selectedStyle.normal.textColor = Color.red;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 80, 200, 500));

        GUILayout.Label("LOL " + _SyncScoreCount.Count);
        StringBuilder sb = new StringBuilder();
        var i = 0;
        foreach (var SCORE in _SyncScoreCount)
        {
            sb.Append("ID:");
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
