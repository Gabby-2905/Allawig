using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages player inventory UI and collected loot.
/// Accessible via Tab key (or custom keybind).
/// Shows what items have been collected and their quantities across 8 inventory slots.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public int count;
        public Sprite icon;

        public InventoryItem(string name, int amount, Sprite iconSprite = null)
        {
            itemName = name;
            count = amount;
            icon = iconSprite;
        }
    }

    [Header("Panels")]
    public GameObject inventoryRoot; // The parent holding the slots
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("Slots")]
    public List<TextMeshProUGUI> itemLabels = new List<TextMeshProUGUI>();
    public List<Image> slotBorders = new List<Image>();

    // Current stored loot items
    [SerializeField]
    private List<InventoryItem> items = new List<InventoryItem>();
    private bool isOpen = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (inventoryRoot == null)
        {
            Transform t = transform.Find("InventoryRoot");
            if (t != null) inventoryRoot = t.gameObject;
        }

        FindSlots();
        RefreshUI();

        if (inventoryRoot != null)
            inventoryRoot.SetActive(false);
    }

    void Start()
    {
        FindSlots();
        RefreshUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
        else if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInventory();
        }
    }

    /// <summary>
    /// Searches for all Slot0..Slot7 children and caches their ItemLabel and Border components.
    /// </summary>
    public void FindSlots()
    {
        if (inventoryRoot == null) return;

        if (itemLabels.Count == 0)
        {
            for (int i = 0; i < 8; i++)
            {
                Transform slotTr = inventoryRoot.transform.Find("Slot" + i);
                if (slotTr != null)
                {
                    Transform labelTr = slotTr.Find("ItemLabel");
                    if (labelTr != null)
                    {
                        var tmp = labelTr.GetComponent<TextMeshProUGUI>();
                        if (tmp != null) itemLabels.Add(tmp);
                    }

                    Transform borderTr = slotTr.Find("Border");
                    if (borderTr != null)
                    {
                        var borderImg = borderTr.GetComponent<Image>();
                        if (borderImg != null) slotBorders.Add(borderImg);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adds an item to the inventory, stacking with existing items of the same name.
    /// </summary>
    public void AddItem(string itemName, int count = 1, Sprite icon = null)
    {
        if (string.IsNullOrEmpty(itemName))
            itemName = "Loot Item";

        var existing = items.Find(it => it.itemName == itemName);
        if (existing != null)
        {
            existing.count += count;
        }
        else
        {
            if (items.Count < 8)
            {
                items.Add(new InventoryItem(itemName, count, icon));
            }
            else
            {
                Debug.LogWarning("[Inventory] Inventory slots full! Cannot add " + itemName);
            }
        }

        Debug.Log($"[Inventory] Added {count}x {itemName}. Total distinct items: {items.Count}");
        RefreshUI();
    }

    /// <summary>
    /// Returns the quantity of an item currently stored.
    /// </summary>
    public int GetItemCount(string itemName)
    {
        var it = items.Find(x => x.itemName == itemName);
        return it != null ? it.count : 0;
    }

    /// <summary>
    /// Refreshes all 8 slot labels with current items.
    /// </summary>
    public void RefreshUI()
    {
        if (itemLabels == null || itemLabels.Count == 0)
            FindSlots();

        for (int i = 0; i < itemLabels.Count && i < 8; i++)
        {
            var label = itemLabels[i];
            if (label == null) continue;

            if (i < items.Count)
            {
                var item = items[i];
                label.text = $"<b>{item.itemName}</b>\n<color=#FFD700>x{item.count}</color>";
                label.color = Color.white;

                if (i < slotBorders.Count && slotBorders[i] != null)
                {
                    slotBorders[i].color = new Color(0.2f, 0.8f, 1f, 0.8f); // Active slot color
                }
            }
            else
            {
                label.text = "<color=#555555>[Empty]</color>";

                if (i < slotBorders.Count && slotBorders[i] != null)
                {
                    slotBorders[i].color = new Color(0.3f, 0.3f, 0.35f, 0.4f); // Inactive slot color
                }
            }
        }
    }

    public void ToggleInventory()
    {
        if (isOpen)
            CloseInventory();
        else
            OpenInventory();
    }

    public void OpenInventory()
    {
        isOpen = true;
        if (inventoryRoot != null)
            inventoryRoot.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshUI();
    }

    public void CloseInventory()
    {
        isOpen = false;
        if (inventoryRoot != null)
            inventoryRoot.SetActive(false);

        // Only lock the cursor if the game is not currently paused
        PauseMenuManager pause = FindFirstObjectByType<PauseMenuManager>();
        if (pause == null || !pause.IsPaused())
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}
