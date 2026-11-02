using UnityEngine;
using System.Collections;

public class PlayerStun : MonoBehaviour
{
    [Header("Character Controller")]
    public CharacterController characterController;

    [Header("Movement Scripts")]
    public MonoBehaviour[] movementScripts;

    public bool IsStunned => stunCoroutine != null;

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
        // Stop movement animation if animator is present
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.SetFloat("Speed", 0f);
            anim.SetFloat("VelocityX", 0f);
            anim.SetFloat("VelocityZ", 0f);
        }

        // Disable movement scripts so input is ignored
        foreach (MonoBehaviour script in movementScripts)
        {
            if (script != null)
                script.enabled = false;
        }

        // NOTE: We deliberately do NOT disable characterController.
        // Disabling CharacterController removes the player's collider, causing
        // OnTriggerExit to fire on trigger zones (such as ExitPoint) and breaking trigger interactions.

        Debug.Log("PLAYER IS STUNNED!");

        yield return new WaitForSeconds(duration);

        // Re-enable movement scripts
        foreach (MonoBehaviour script in movementScripts)
        {
            if (script != null)
                script.enabled = true;
        }

        Debug.Log("PLAYER CAN MOVE AGAIN!");

        stunCoroutine = null;
    }
}
