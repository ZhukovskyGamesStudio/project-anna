using UnityEngine;

public class EmberFlicker : MonoBehaviour
{
    [SerializeField] private Light emberLight;
    [SerializeField] private float baseIntensity = 1.2f;
    [SerializeField] private float flickerAmount = 0.4f;
    [SerializeField] private float flickerSpeed = 12f;

    private void Update()
    {
        if (emberLight == null)
        {
            return;
        }

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        emberLight.intensity = baseIntensity + (noise - 0.5f) * 2f * flickerAmount;
    }
}