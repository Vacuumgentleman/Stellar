using UnityEngine;
using UnityEngine.UI;

public class IconSelector : MonoBehaviour
{
    [Header("Icon UI")]
    [SerializeField] private RawImage iconDisplay;
    [SerializeField] private Texture[] icons; // assign 7 textures in inspector (index 0..6)

    private int currentIndex = 0;

    private void Start()
    {
        ValidateIcons();
        UpdateDisplay();
    }

    private void ValidateIcons()
    {
        if (icons == null || icons.Length == 0)
            Debug.LogWarning("[IconSelector] No icons assigned.");
    }

    public void NextIcon()
    {
        if (icons == null || icons.Length == 0) return;
        currentIndex = (currentIndex + 1) % icons.Length;
        UpdateDisplay();
    }

    public void PreviousIcon()
    {
        if (icons == null || icons.Length == 0) return;
        currentIndex = (currentIndex - 1 + icons.Length) % icons.Length;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (iconDisplay == null || icons == null || icons.Length == 0) return;
        iconDisplay.texture = icons[currentIndex];
    }

    /// <summary>
    /// Returns the selected icon index for Unity arrays (0..6).
    /// </summary>
    public int GetSelectedIndexZeroBased()
    {
        return currentIndex;
    }

    /// <summary>
    /// Returns the selected icon number in 1..N (1..7).
    /// This value is what will be saved to Firestore (IconNumber).
    /// </summary>
    public int GetSelectedNumberOneBased()
    {
        return currentIndex + 1;
    }

    /// <summary>
    /// Gets the assigned icons array (for TopScoresDisplay to use same textures).
    /// </summary>
    public Texture[] GetIconsArray()
    {
        return icons;
    }
}
