using UnityEngine;

public class WindowDither : MonoBehaviour
{
    [SerializeField] private RenderTexture sourceTexture;
    [SerializeField] private RenderTexture outputTexture;
    [SerializeField] private Material ditherMaterial;

    private void LateUpdate()
    {
        if (sourceTexture == null || outputTexture == null)
        {
            return;
        }

        if (ditherMaterial == null)
        {
            Graphics.Blit(sourceTexture, outputTexture);
            return;
        }

        Graphics.Blit(sourceTexture, outputTexture, ditherMaterial);
    }
}