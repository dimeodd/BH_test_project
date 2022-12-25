using UnityEngine;

namespace EcsStructs
{
    public struct PlayerData
    {
        public Vector3 velocity;
        public Quaternion horizontalRotation;
        public Transform followTarget;
    }
}