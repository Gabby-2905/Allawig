using UnityEngine;

/// <summary>
/// Attach to trigger collider GameObjects near the store entrance/exit.
/// </summary>
public class StoreTrigger : MonoBehaviour
{
    public enum TriggerType { Entrance, Exit }
    public TriggerType triggerType = TriggerType.Entrance;
    public StoreInterior storeInterior;

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        if (triggerType == TriggerType.Entrance)
            storeInterior.OnEnterEntranceTrigger();
        else
            storeInterior.OnEnterExitTrigger();
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        if (triggerType == TriggerType.Entrance)
            storeInterior.OnExitEntranceTrigger();
        else
            storeInterior.OnExitExitTrigger();
    }

    bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player") || other.name == "Player" || other.GetComponent<GamePlayerMovement>() != null;
    }
}
