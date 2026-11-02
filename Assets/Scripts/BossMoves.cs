using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossMoves : MonoBehaviour
{
    public Transform player;
    public GameObject slamWarningPrefab;   // flat red disc
    public LayerMask playerLayer, wallLayer;

    public float slamWindup = 1f, slamRadius = 4f;
    public int slamDamage = 20;
    public float chargeWindup = 0.8f, chargeSpeed = 18f, stunTime = 2.5f;
    public int chargeDamage = 25;

    Rigidbody rb;
    bool charging, hitPlayer;
    Vector3 dir;

    void Awake() => rb = GetComponent<Rigidbody>();

    // Call these from your boss AI: StartCoroutine(BatoSlam());
    public IEnumerator BatoSlam()
    {
        Face();
        GameObject warning = null;
        if (slamWarningPrefab)
        {
            warning = Instantiate(slamWarningPrefab, transform.position + Vector3.up * 0.05f, Quaternion.identity);
            warning.transform.localScale = new Vector3(slamRadius * 2, 0.02f, slamRadius * 2);
        }

        yield return new WaitForSeconds(slamWindup);   // fist raised, player dodges here
        if (warning) Destroy(warning);

        foreach (var h in Physics.OverlapSphere(transform.position, slamRadius, playerLayer))
        {
            // jumping player is safe
            if (!Physics.Raycast(h.transform.position + Vector3.up * 0.1f, Vector3.down, 0.6f)) continue;
            h.GetComponentInParent<PlayerHealth>()?.TakeDamage(slamDamage);
        }
    }

    public IEnumerator BatoCharge()
    {
        Face();
        yield return new WaitForSeconds(chargeWindup); // crouch wind-up

        dir = transform.forward; dir.y = 0; dir.Normalize();   // direction locked
        charging = true; hitPlayer = false;

        float t = 0;
        while (t < 2f)
        {
            t += Time.deltaTime;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = new Vector3(dir.x * chargeSpeed, rb.linearVelocity.y, dir.z * chargeSpeed);
#else
            rb.velocity = new Vector3(dir.x * chargeSpeed, rb.velocity.y, dir.z * chargeSpeed);
#endif

            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, 1.5f, wallLayer))
            {
                charging = false;
                Stop();
                yield return new WaitForSeconds(stunTime);  // vulnerable: deal bonus damage here
                yield break;
            }
            yield return null;
        }
        charging = false;
        Stop();
    }

    void Stop()
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector3.zero;
#else
        rb.velocity = Vector3.zero;
#endif
    }

    void OnCollisionEnter(Collision c)
    {
        if (charging && !hitPlayer && c.collider.CompareTag("Player"))
        {
            hitPlayer = true;
            c.collider.GetComponentInParent<PlayerHealth>()?.TakeDamage(chargeDamage);
        }
    }

    void Face()
    {
        if (!player) return;
        Vector3 d = player.position - transform.position; d.y = 0;
        if (d.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(d);
    }
}