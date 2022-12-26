using UnityEngine;

public class DashSystem
{
    PlayerProvider _provider;
    StaticData _stData;
    PlayerInput_NetScript _netScript;

    public DashSystem(PlayerProvider provider, StaticData stData, PlayerInput_NetScript netScript)
    {
        _provider = provider;
        _stData = stData;
        _netScript = netScript;
    }

    public void UseSkill()
    {
        var moveDir = _provider.rigidbody.velocity;

        if (moveDir.magnitude < 0.1f)
        {
            moveDir = _provider.horizontalTransform.forward;
        }

        var startPos = _provider.rigidbody.position;
        var pos2 = startPos + Vector3.up * 2;
        var capsuleRadius = 0.5f;
        var dir = moveDir.normalized;
        var distance = _stData.dashLeght_m;

        var targetPos = startPos + dir * distance;

        var hitList = Physics.CapsuleCastAll(startPos, pos2, capsuleRadius, dir, distance);

        for (int i = 0, iMax = hitList.Length; i < iMax; i++)
        {
            //skip owner
            if (hitList[i].transform == _provider.transform) continue;

            var tag = hitList[i].transform.tag;
            switch (tag)
            {
                case "Wall":
                    {
                        targetPos = hitList[i].point + hitList[i].normal.normalized * capsuleRadius;
                        if (hitList[i].point == new Vector3()) //это случается если мы касаемся этого объекта
                        {
                            targetPos = startPos;
                        }
                        break;
                    }
                case "Player":
                    {
                        targetPos = hitList[i].point + hitList[i].normal.normalized * capsuleRadius;
                        if (hitList[i].point == new Vector3()) //это случается если мы касаемся этого объекта
                        {
                            targetPos = startPos;
                        }

                        var otherNetScript = hitList[i].transform.gameObject.GetComponent<PlayerInput_NetScript>();

                        if (otherNetScript.isInvincible) break;

                        if (_netScript.isServer)
                            World.Singleton.DashHit(_netScript.netId, otherNetScript.netId);
                        else
                            _netScript.CmdDashHit(_netScript.netId, otherNetScript.netId);

                        break;
                    }
            }
        }

        targetPos.y = 0;
        _provider.rigidbody.MovePosition(targetPos);
    }

}