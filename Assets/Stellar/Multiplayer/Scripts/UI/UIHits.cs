using TMPro;
using UnityEngine;
using Fusion;

public class UIHits : NetworkBehaviour
{
    [SerializeField] private TMP_Text HitsText;
    private Player player;

    public override void Spawned()
    {
        player = Object.GetComponent<Player>();
        UpdateUI(player.Hits);
    }

    private void Update()
    {
        if (player != null)
        {
            UpdateUI(player.Hits);
        }
    }

    private void UpdateUI(int value)
    {
        HitsText.text = $"Hits: {value}";
    }
}
