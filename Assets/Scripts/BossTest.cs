using System.Collections;
using UnityEngine;

public class BossTest : MonoBehaviour
{
    BossMoves moves;

    void Start()
    {
        moves = GetComponent<BossMoves>();
        StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(moves.BatoSlam());
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(moves.BatoCharge());
        }
    }
}