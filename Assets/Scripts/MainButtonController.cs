using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainButtonController : MonoBehaviour
{
    // Cached references to UI components
    public RectTransform RectTransform { get; private set; }
    public Button Button { get; private set; }

    private void Awake()
    {
        // Get required UI components
        RectTransform = GetComponent<RectTransform>();
        Button = GetComponent<Button>();
    }

    // Animate the button's scale over a duration
    public void AnimateScale(float targetScale, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleCoroutine(targetScale, duration));
    }

    // Coroutine to smoothly scale the button
    private IEnumerator ScaleCoroutine(float targetScale, float duration)
    {
        if (RectTransform == null) RectTransform = GetComponent<RectTransform>();
        Vector3 startScale = RectTransform.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            RectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        RectTransform.localScale = endScale;
    }
}