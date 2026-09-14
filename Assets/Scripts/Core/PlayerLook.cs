using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float mouseSensitivity = 4f;
    public Transform playerBody;

    private float xRotation = 0f;
    private bool canLook = true;

    private void Start()
    {
        if (GameSettings.Instance != null)
        {
            mouseSensitivity = GameSettings.Instance.GetMouseSensitivity();
        }

        ShowCursor();
    }

    private void Update()
    {
        if (!canLook)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    public void SetCanLook(bool value)
    {
        canLook = value;
        
        ShowCursor();
    }

    public void SetMouseSensitivity(float value)
    {
        mouseSensitivity = value;
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}