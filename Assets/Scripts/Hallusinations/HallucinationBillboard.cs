using UnityEngine;

public class HallucinationBillboard : MonoBehaviour
{
    [SerializeField] private Transform target;

    private void LateUpdate()
    {
        if (target == null)
        {
            if (Camera.main != null)
            {
                target = Camera.main.transform;
            }
            else
            {
                return;
            }
        }

        transform.LookAt(target.position);
        transform.Rotate(0f, 180f, 0f);
    }
}