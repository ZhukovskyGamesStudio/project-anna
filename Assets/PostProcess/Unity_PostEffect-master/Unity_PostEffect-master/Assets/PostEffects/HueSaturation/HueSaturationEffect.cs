using UnityEngine;

public class HueSaturationEffect : MonoBehaviour
{
    public bool on = false;

    [Range(-100, 100)]
    public int saturation = 0;

    [Range(0, 360)]
    public int angle = 0;

    [Range(0f, 10f)]
    public float speed = 5f;

    public Material material;

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (on)
        {
            Graphics.Blit(src, dest, material);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

    private void Update()
    {
        if (on && material != null)
        {
            material.SetInt("_Saturation", saturation);
            material.SetInt("_Angle", angle);
            material.SetFloat("_Speed", speed);
        }
    }
}