using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;

public class TopScoresDisplayFirebase : MonoBehaviour
{
    [Header("UI - Top 3")]
    [SerializeField] private RawImage[] iconSlots;      // length 3
    [SerializeField] private TMP_Text[] nameLabels;     // length 3
    [SerializeField] private TMP_Text[] scoreLabels;    // length 3

    [Header("Shared Icons (assign same textures as IconSelector)")]
    [SerializeField] private Texture[] availableIcons;  // length 7, indexes 0..6

    private async void Start()
    {
        await RefreshTop3Async();
    }

    public async Task RefreshTop3Async()
    {
        if (FirestoreScoreService.Instance == null)
        {
            Debug.LogError("[TopScoresDisplayFirebase] FirestoreScoreService missing.");
            return;
        }

        List<Dictionary<string, object>> top = await FirestoreScoreService.Instance.GetTopScoresAsync(3);

        // Clear UI
        for (int i = 0; i < nameLabels.Length; i++)
        {
            nameLabels[i].text = "-";
            scoreLabels[i].text = "-";
            if (iconSlots[i] != null) iconSlots[i].texture = null;
        }

        for (int i = 0; i < top.Count && i < 3; i++)
        {
            var entry = top[i];

            string name = entry.ContainsKey("PlayerName") ? entry["PlayerName"].ToString() : "Unknown";
            int score = 0;
            int iconNumberOneBased = 1;

            if (entry.ContainsKey("Score"))
            {
                int.TryParse(entry["Score"].ToString(), out score);
            }

            if (entry.ContainsKey("IconNumber"))
            {
                int.TryParse(entry["IconNumber"].ToString(), out iconNumberOneBased);
            }

            nameLabels[i].text = name;
            scoreLabels[i].text = score.ToString();

            int iconIndex = Mathf.Clamp(iconNumberOneBased - 1, 0, (availableIcons != null ? availableIcons.Length - 1 : 0));

            if (availableIcons != null && iconIndex >= 0 && iconIndex < availableIcons.Length)
            {
                if (iconSlots[i] != null)
                    iconSlots[i].texture = availableIcons[iconIndex];
            }
        }
    }
}
