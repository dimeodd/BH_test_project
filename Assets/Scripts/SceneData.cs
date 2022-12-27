using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class SceneData : MonoBehaviour
{
    public CinemachineVirtualCamera followCamera;
    public CinemachineVirtualCamera waitCamera;
    public Transform cameraTransform;


    [HideInInspector] public Transform[] SpawnPositions;

    [Header("Win Window")]
    public GameObject winnerWindow;
    public Text winnerText;

    [Header("Nickname")]
    public GameObject nameWindow;
    public InputField nameText;
}
