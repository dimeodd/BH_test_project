using UnityEngine;

[CreateAssetMenu(fileName = "StaticData", menuName = "BH_test_project/StaticData", order = 0)]
public class StaticData : ScriptableObject
{
   public LayerMask playerMask;
    [Header("Materials")]
    public Material defMaterial;
    public Material invincibleMaterial;


    [Header("Camera property")]
    [Range(0, 179)]
    public float upMaxAngle = 80;
    [Range(0, 179)]
    public float downMaxAngle = 60;
    [Range(0, 100)]
    public float mouseSensetivity = 50;


    [Header("Player property")]
    public float playerSpeed = 2;
    public float dashLeght_m = 3;
    public float invincibleCooldown_sec = 3;

}
