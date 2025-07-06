using UnityEngine;

public class UIAnimatorController : MonoBehaviour
{
    // Reference to the Animator component
    public Animator Animator { get; private set; }
    
    // Add a serialized list of tab panels
    [Header("Tab Panels (assign in inspector)")]
    [SerializeField] private GameObject[] tabPanels;

    private void Awake()
    {
        // Get the Animator component
        Animator = GetComponent<Animator>();
    }

    // Set the 'IsOpen' parameter in the Animator
    public void SetIsOpen(bool isOpen)
    {
        if (Animator != null)
            Animator.SetBool("IsOpen", isOpen);
    }


    // Open tab by index (scale to 1)
    public void OpenTab(int tabIndex)
    {
        if (tabPanels == null || tabIndex < 0 || tabIndex >= tabPanels.Length) return;
        tabPanels[tabIndex].transform.localScale = Vector3.one;
    }

    // Close tab by index (scale to 0)
    public void CloseTab(int tabIndex)
    {
        if (tabPanels == null || tabIndex < 0 || tabIndex >= tabPanels.Length) return;
        tabPanels[tabIndex].transform.localScale = Vector3.zero;
    }

    // Optionally, close all tabs
    public void CloseAllTabs()
    {
        if (tabPanels == null) return;
        foreach (var tab in tabPanels)
        {
            if (tab != null)
                tab.transform.localScale = Vector3.zero;
        }
    }
}