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
    public Image[] inventorySlots = new Image[8];
    public TextMeshProUGUI[] inventoryLabels = new TextMeshProUGUI[8];

    // Simple inventory data
    private string[] itemNames = new string[8];
    private Color[] slotColors = new Color[8];

    void Start()
    {
        // Load persisted inventory (or initialize empty)
        for (int i = 0; i < 8; i++)
        {
            var (name, color) = GameSaveManager.LoadInventorySlot(i);
            itemNames[i]  = name;
            slotColors[i] = color;
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
        if (slot < 0 || slot >= 8) return;
        itemNames[slot] = itemName;
        slotColors[slot] = color;
        GameSaveManager.SaveInventorySlot(slot, itemName, color);
        RefreshInventoryUI();
    }

    public void RemoveItem(int slot)
    {
        if (slot < 0 || slot >= 8) return;
        itemNames[slot] = "";
        slotColors[slot] = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        GameSaveManager.SaveInventorySlot(slot, "", slotColors[slot]);
        RefreshInventoryUI();
    }

    void RefreshInventoryUI()
    {
        for (int i = 0; i < 8; i++)
        {
            if (inventorySlots[i] != null)
                inventorySlots[i].color = slotColors[i];
            if (inventoryLabels[i] != null)
                inventoryLabels[i].text = itemNames[i];
        }
    }
}
