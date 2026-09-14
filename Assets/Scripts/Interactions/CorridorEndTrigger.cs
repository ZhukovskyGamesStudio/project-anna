using UnityEngine;

public class CorridorEndTrigger : MonoBehaviour
{
    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
        {
            return;
        }

        if (other.GetComponentInParent<PlayerMovement>() == null)
        {
            return;
        }

        if (FinalSequenceManager.Instance == null)
        {
            return;
        }

        triggered = true;
        FinalSequenceManager.Instance.FinishDoorEnding();
    }
}
