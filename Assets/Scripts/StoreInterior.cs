using UnityEngine;

/// <summary>
/// Manages entering and exiting the convenience store interior.
/// Attach to the GasStation parent object.
/// </summary>
public class StoreInterior : MonoBehaviour
{
    [Header("References")]
    public GameObject exteriorRoot;
    public GameObject interiorRoot;
    public Transform enterSpawnPoint;
    public Transform exitSpawnPoint;
    public Transform playerTransform;

    [Header("UI Hint")]
    public GameObject enterHint;
    public GameObject exitHint;

    private bool playerNearEntrance = false;
    private bool playerNearExit = false;
    private bool isInside = false;

    void Start()
    {
        if (interiorRoot != null) interiorRoot.SetActive(false);
        if (enterHint != null) enterHint.SetActive(false);
        if (exitHint != null) exitHint.SetActive(false);

        if (playerTransform == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go == null) go = GameObject.Find("Player");
            if (go != null) playerTransform = go.transform;
        }
    }

    void Update()
    {
        if (playerNearEntrance && !isInside && Input.GetKeyDown(KeyCode.E))
        {
            EnterStore();
        }
        else if (playerNearExit && isInside && Input.GetKeyDown(KeyCode.E))
        {
            ExitStore();
        }
    }

    void EnterStore()
    {
        isInside = true;
        if (exteriorRoot != null) exteriorRoot.SetActive(false);
        if (interiorRoot != null) interiorRoot.SetActive(true);
        if (enterHint != null) enterHint.SetActive(false);
        if (exitHint != null) exitHint.SetActive(true);

        if (playerTransform != null && enterSpawnPoint != null)
        {
            var cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerTransform.position = enterSpawnPoint.position;
            playerTransform.rotation = enterSpawnPoint.rotation;
            if (cc != null) cc.enabled = true;
        }
    }

    void ExitStore()
    {
        isInside = false;
        if (exteriorRoot != null) exteriorRoot.SetActive(true);
        if (interiorRoot != null) interiorRoot.SetActive(false);
        if (exitHint != null) exitHint.SetActive(false);

        if (playerTransform != null && exitSpawnPoint != null)
        {
            var cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerTransform.position = exitSpawnPoint.position;
            playerTransform.rotation = exitSpawnPoint.rotation;
            if (cc != null) cc.enabled = true;
        }
    }

    // Called by trigger colliders
    public void OnEnterEntranceTrigger() { playerNearEntrance = true; if (!isInside && enterHint != null) enterHint.SetActive(true); }
    public void OnExitEntranceTrigger() { playerNearEntrance = false; if (enterHint != null) enterHint.SetActive(false); }
    public void OnEnterExitTrigger() { playerNearExit = true; if (isInside && exitHint != null) exitHint.SetActive(true); }
    public void OnExitExitTrigger() { playerNearExit = false; if (exitHint != null) exitHint.SetActive(false); }
}
