using UnityEngine;
using System.Collections;

public class PhoneBeepController : MonoBehaviour
{
    [SerializeField] private float minDelay = 15f;
    [SerializeField] private float maxDelay = 35f;

    private Coroutine beepCoroutine;

    private void Start()
    {
        StartBeepLoop();
    }

    public void StartBeepLoop()
    {
        if (beepCoroutine != null)
        {
            StopCoroutine(beepCoroutine);
        }

        beepCoroutine = StartCoroutine(BeepLoop());
    }

    private IEnumerator BeepLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            if (DayManager.Instance == null)
            {
                continue;
            }

            int day = DayManager.Instance.CurrentDay;

            if (day == 2 || day == 3)
            {
                bool finaleActive = FinalSequenceManager.Instance != null && FinalSequenceManager.Instance.HasStarted();

                if (!finaleActive && (UIManager.Instance == null || !UIManager.Instance.IsPanelOpen()))
                {
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayPhoneBeep();
                    }
                }
            }
        }
    }
}