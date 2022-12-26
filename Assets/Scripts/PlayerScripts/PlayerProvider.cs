using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProvider : MonoBehaviour
{
    public Rigidbody rigidbody;
    public Transform horizontalTransform;
    public Transform followTarget;
    public Animator animator;
    public Renderer skinRenderer;

    public PlayerMove moveScript;
}
