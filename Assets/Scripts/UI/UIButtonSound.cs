using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to any UI Button to automatically play click sounds
/// Simple component that hooks into button events
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Sound Options")]
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private bool playHoverSound = false; // Usually too much for every button
    
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
    }
    
    /// <summary>
    /// Called when button is clicked
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (playClickSound && button.interactable && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
    
    /// <summary>
    /// Called when mouse/finger hovers over button
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHoverSound && button.interactable && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHover();
        }
    }
}
