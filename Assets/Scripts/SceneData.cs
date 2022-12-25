using UnityEngine;
using Cinemachine;

public class SceneData : MonoBehaviour
{
    public CinemachineVirtualCamera followCamera;
    public CinemachineVirtualCamera waitCamera;

    [HideInInspector] public Transform[] SpawnPositions;
}
