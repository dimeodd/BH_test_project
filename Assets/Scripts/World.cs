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
    List<uint> _netIndexes = new List<uint>();
    SyncList<int> _SyncScoreCount = new SyncList<int>();

    [Server]
    void ChangeVector3Vars(int newValue)
    {
        _SyncScoreCount.Add(newValue);
    }

    [Server]
    public override void OnStartServer()
    {
        if (!isServer) return;
    }

    [Server]
    public void PlayerRegistr(uint targetNetId, PlayerInput_NetScript netScript)
    {
        _netScriptDict.Add(targetNetId, netScript);
        _netIndexes.Add(targetNetId);
        _SyncScoreCount.Add(0);
    }
    [Server]
    public void PlayerRemove(uint targetNetId)
    {
        _netScriptDict.Remove(targetNetId);

        var index = _netIndexes.FindIndex(x => x == targetNetId);
        _SyncScoreCount.RemoveAt(index);
        _netIndexes.RemoveAt(index);
    }

    [Server]
    public void AddPoint(uint targetNetId)
    {
        var index = _netIndexes.FindIndex(x => x == targetNetId);
        _SyncScoreCount[index]++;
    }


    [Server]
    public void DashHit(uint owner, uint targetNetId)
    {
        AddPoint(owner);

        var otherNetScript = _netScriptDict[targetNetId];
        otherNetScript.RpcDamage();
    }

    #region Client

    public List<int> ScoreCount;
    void SyncScoreCount(SyncList<int>.Operation op, int index, int oldItem, int newItem)
    {
        switch (op)
        {
            case SyncList<int>.Operation.OP_ADD:
                {
                    ScoreCount.Add(newItem);
                    break;
                }
            case SyncList<int>.Operation.OP_CLEAR:
                {
                    ScoreCount.Clear();
                    break;
                }
            case SyncList<int>.Operation.OP_INSERT:
                {

                    break;
                }
            case SyncList<int>.Operation.OP_REMOVEAT:
                {
                    ScoreCount.Remove(index);
                    break;
                }
            case SyncList<int>.Operation.OP_SET:
                {
                    ScoreCount[index] = newItem;
                    break;
                }
        }
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

            GUILayout.Label(sb.ToString());

            sb.Clear();
            i++;
        }

        GUILayout.EndArea();
    }

    #endregion
}
