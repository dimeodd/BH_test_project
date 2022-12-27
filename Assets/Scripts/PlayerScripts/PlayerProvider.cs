using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerProvider : MonoBehaviour
{
    public Rigidbody rigidbody;
    public Transform horizontalTransform;
    public Transform followTarget;
    public Animator animator;
    public Renderer skinRenderer;

    public PlayerMove moveScript;
    public TextMeshPro nameText;
}
