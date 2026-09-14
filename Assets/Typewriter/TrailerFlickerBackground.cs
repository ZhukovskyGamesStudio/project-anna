using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TrailerFlickerBackground : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite frameA;
    [SerializeField] private Sprite frameB;
    [SerializeField] private Sprite frameC;
    [SerializeField] private float switchInterval = 0.07f;

    private void Start()
    {
        StartCoroutine(Flicker());
    }

    private IEnumerator Flicker()
    {
        Sprite[] frames = { frameA, frameB, frameC };
        int index = 0;

        while (true)
        {
            if (backgroundImage != null && frames[index] != null)
            {
                backgroundImage.sprite = frames[index];
            }

            index++;

            if (index >= frames.Length)
            {
                index = 0;
            }

            yield return new WaitForSeconds(switchInterval);
        }
    }
}