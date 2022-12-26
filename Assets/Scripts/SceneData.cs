using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class SceneData : MonoBehaviour
{
    public CinemachineVirtualCamera followCamera;
    public CinemachineVirtualCamera waitCamera;

    [HideInInspector] public Transform[] SpawnPositions;

    public GameObject winnerWindow;
    public Text winnerText;
}
