
using UnityEngine;
using System.Collections;

public class PlayerStun : MonoBehaviour
{
    [Header("Character Controller")]
    public CharacterController characterController;

    [Header("Movement Scripts")]
    public MonoBehaviour[] movementScripts;

    private Coroutine stunCoroutine;

    void Awake()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
    }

    public void Stun(float duration)
    {
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }

        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        // Disable movement scripts
        foreach (MonoBehaviour script in movementScripts)
        {
            if (script != null)
                script.enabled = false;
        }

        // Disable character controller
        if (characterController != null)
            characterController.enabled = false;

        Debug.Log("PLAYER IS STUNNED!");

        yield return new WaitForSeconds(duration);

        // Enable character controller
        if (characterController != null)
            characterController.enabled = true;

        // Enable movement scripts
        foreach (MonoBehaviour script in movementScripts)
        {
            if (script != null)
                script.enabled = true;
        }

        Debug.Log("PLAYER CAN MOVE AGAIN!");

        stunCoroutine = null;
    }
}
