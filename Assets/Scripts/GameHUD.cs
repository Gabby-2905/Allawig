using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("References")]
    public GamePlayerMovement player;

    [Header("Health Bar")]
    public Image healthBarFill;
    public TextMeshProUGUI healthText;

    [Header("Inventory Slots")]
    public Image[] inventorySlots = new Image[4];
    public TextMeshProUGUI[] inventoryLabels = new TextMeshProUGUI[4];

    // Simple inventory data
    private string[] itemNames = new string[4];
    private Color[] slotColors = new Color[4];

    void Start()
    {
        // Initialize empty slots
        for (int i = 0; i < 4; i++)
        {
            itemNames[i] = "";
            slotColors[i] = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        }
        RefreshInventoryUI();
    }

    void Update()
    {
        if (player == null) return;

        // Update health bar
        float ratio = player.currentHealth / player.maxHealth;
        if (healthBarFill != null)
            healthBarFill.fillAmount = ratio;
        if (healthText != null)
            healthText.text = $"{Mathf.RoundToInt(player.currentHealth)} / {Mathf.RoundToInt(player.maxHealth)}";
    }

    public void AddItem(int slot, string itemName, Color color)
    {
        if (slot < 0 || slot >= 4) return;
        itemNames[slot] = itemName;
        slotColors[slot] = color;
        RefreshInventoryUI();
    }

    void RefreshInventoryUI()
    {
        for (int i = 0; i < 4; i++)
        {
            if (inventorySlots[i] != null)
                inventorySlots[i].color = slotColors[i];
            if (inventoryLabels[i] != null)
                inventoryLabels[i].text = itemNames[i];
        }
    }
}
