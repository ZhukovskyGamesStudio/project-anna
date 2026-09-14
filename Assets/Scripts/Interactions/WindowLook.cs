using UnityEngine;

public class WindowLook : MonoBehaviour
{
    [SerializeField] private float maxYaw = 25f;
    [SerializeField] private float maxPitch = 15f;
    [SerializeField] private float smoothSpeed = 6f;

    private Camera cam;
    private Quaternion centerRotation;
    private bool wasEnabled;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        centerRotation = transform.localRotation;
    }

    private void Update()
    {
        if (cam == null || !cam.enabled)
        {
            wasEnabled = false;
            return;
        }

        if (!wasEnabled)
        {
            transform.localRotation = centerRotation;
            wasEnabled = true;
        }

        float mx = Mathf.Clamp((Input.mousePosition.x / Screen.width - 0.5f) * 2f, -1f, 1f);
        float my = Mathf.Clamp((Input.mousePosition.y / Screen.height - 0.5f) * 2f, -1f, 1f);

        Quaternion target = centerRotation * Quaternion.Euler(-my * maxPitch, mx * maxYaw, 0f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * smoothSpeed);
    }
}