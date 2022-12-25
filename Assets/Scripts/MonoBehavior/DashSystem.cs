using UnityEngine;

public class DashSystem
{
    PlayerProvider _provider;
    StaticData _stData;

    public DashSystem(PlayerProvider provider, StaticData stData)
    {
        _provider = provider;
        _stData = stData;
    }

    public void UseSkill()
    {
        var moveDir = _provider.rigidbody.velocity;

        if (moveDir.magnitude < 0.1f)
        {
            moveDir = _provider.horizontalTransform.forward;
        }

        _provider.rigidbody.MovePosition(_provider.rigidbody.position + moveDir.normalized * _stData.dashLeght_m);
    }

}