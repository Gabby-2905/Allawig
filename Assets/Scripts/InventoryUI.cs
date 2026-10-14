using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryRoot; // The parent holding both panels

    private bool isOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isOpen = !isOpen;
            if (inventoryRoot != null)
                inventoryRoot.SetActive(isOpen);

            // Pause/unpause time and cursor
            if (isOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = false;
            }
        }
    }

    public void CloseInventory()
    {
        isOpen = false;
        if (inventoryRoot != null)
            inventoryRoot.SetActive(false);
        Cursor.visible = false;
    }
}
