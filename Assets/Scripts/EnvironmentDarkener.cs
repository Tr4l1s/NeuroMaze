using UnityEngine;
using UnityEngine.UI;

public class EnvironmentDarkener : MonoBehaviour
{
    public Image darkOverlay;
    public int stressBpm = 100;
    public float maxAlpha = 0.6f;

    public void OnNewBpm(int bpm)
    {
        if (darkOverlay == null) return;

        float t = Mathf.InverseLerp(stressBpm, stressBpm + 40, bpm);
        float targetAlpha = Mathf.Lerp(0f, maxAlpha, t);

        Color c = darkOverlay.color;
        c.a = targetAlpha;
        darkOverlay.color = c;
    }
}
