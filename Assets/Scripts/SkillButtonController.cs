using UnityEngine;
using UnityEngine.UI;

public class SkillButtonController : MonoBehaviour
{
    // Cached references to UI components
    public RectTransform RectTransform { get; private set; }
    public Image Image { get; private set; }
    public Button Button { get; private set; }
    // Event for button click
    public event System.Action<SkillButtonController> OnButtonClicked;

    private void Awake()
    {
        // Get required UI components
        RectTransform = GetComponent<RectTransform>();
        Image = GetComponent<Image>();
        Button = GetComponent<Button>();
        // Register click event
        if (Button != null)
            Button.onClick.AddListener(() => OnButtonClicked?.Invoke(this));
    }

    // Set the button's scale
    public void SetScale(float scale)
    {
        RectTransform.localScale = Vector3.one * scale;
    }

    // Enable or disable raycast for the button's image
    public void SetRaycast(bool enabled)
    {
        if (Image != null)
            Image.raycastTarget = enabled;
    }

    // Set the button's anchored position
    public void SetPosition(Vector2 anchoredPosition)
    {
        RectTransform.anchoredPosition = anchoredPosition;
    }
}