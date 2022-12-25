using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProvider : MonoBehaviour
{
    public Transform horizontalTransform;
    public Transform followTarget;
    public Animator animator;
    public Renderer skinRenderer;

    public PlayerMove moveScript;
    public CameraSwaper_Script cameraSwaper;
}
