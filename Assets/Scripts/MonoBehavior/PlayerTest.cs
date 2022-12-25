using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerTest : NetworkBehaviour
{
    void Update()
    {
        if (isLocalPlayer)
        {
            var w = Input.GetAxis("Horizontal");
            var h = Input.GetAxis("Vertical");
            var speed = 2f * Time.deltaTime;
            transform.position += new Vector3(w,0,h);
        }

    }
}
