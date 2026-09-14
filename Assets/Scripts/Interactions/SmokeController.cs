using UnityEngine;
using System.Collections;

public class SmokeController : MonoBehaviour
{
    [SerializeField] private GameObject cigaretteObject;
    [SerializeField] private GameObject emberObject;
    [SerializeField] private ParticleSystem smokeEffect;
    [SerializeField] private ParticleSystem ashEffect;
    [SerializeField] private AudioSource smokeAudio;
    [SerializeField] private AudioClip smokeClip;
    [SerializeField] private int puffAmount = 40;
    [SerializeField] private float exhaleDelay = 1f;

    private bool isLit;

    public void Smoke()
    {
        if (!isLit)
        {
            isLit = true;

            if (cigaretteObject != null)
            {
                cigaretteObject.SetActive(true);
            }

            if (ashEffect != null)
            {
                ashEffect.gameObject.SetActive(true);
                ashEffect.Play();
            }
        }

        if (smokeAudio != null && smokeClip != null)
        {
            smokeAudio.PlayOneShot(smokeClip);
        }

        if (emberObject != null)
        {
            emberObject.SetActive(true);
        }

        StartCoroutine(ExhaleAfterDelay());
    }

    public void PutOut()
    {
        StopAllCoroutines();

        isLit = false;

        if (cigaretteObject != null)
        {
            cigaretteObject.SetActive(false);
        }

        if (emberObject != null)
        {
            emberObject.SetActive(false);
        }

        if (ashEffect != null)
        {
            ashEffect.Stop();
            ashEffect.gameObject.SetActive(false);
        }
    }

    private IEnumerator ExhaleAfterDelay()
    {
        yield return new WaitForSeconds(exhaleDelay);

        if (smokeEffect != null)
        {
            smokeEffect.Emit(puffAmount);
        }

        if (emberObject != null)
        {
            emberObject.SetActive(false);
        }
    }
}