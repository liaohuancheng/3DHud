using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCamera;

    void Start()
    {
        mainCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + mainCamera.forward);
    }
}