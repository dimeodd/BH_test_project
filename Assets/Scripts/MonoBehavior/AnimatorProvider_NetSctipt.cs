using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AnimatorProvider_NetSctipt : NetworkBehaviour
{
    public Animator animator;

    [ClientRpc]
    public void RpcSetAnimatorFloatProperty(string name, float value)
    {
        animator.SetFloat(name, value, 0.2f, Time.deltaTime);
    }
}
