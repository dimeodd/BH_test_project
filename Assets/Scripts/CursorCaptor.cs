using UnityEngine;

public class CursorCaptor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Lock Cursor");
        Cursor.lockState = CursorLockMode.Confined;
    }
}
