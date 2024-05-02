using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    Animator anim;
    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
        Debug.Log("[CharacterAnimator] Awake / anim : " + anim);

    }

    private void Update()
    {
    }

    public IEnumerator Move(Vector3 toPos)
    {
        Debug.Log("[CharacterAnimator] Move / currPos : " + gameObject.transform.position + " / toPos : " + toPos);

        anim.SetBool("move", true);
        Vector3 fromPos = this.transform.position;

        float speed = 22.0f;
        this.transform.LookAt(toPos);
        while (Vector3.Distance(this.transform.position, toPos) > 0.01f)
        {
            yield return null;
            var dir = (toPos - this.transform.position).normalized;
            this.transform.position += dir * speed * Time.deltaTime;
        }
        anim.SetBool("move", false);
    }

    public IEnumerator Attack()
    {
        Debug.Log("[CharacterAnimator] Attack");

        anim.SetBool("attack", true);
        yield return new WaitForSeconds(1.334f);
        anim.SetBool("attack", false);
    }

    public IEnumerator Guard() // 0.8 speed
    {
        anim.SetBool("guard", true);
        yield return new WaitForSeconds(0.417f);
        anim.SetBool("guard", false);
    }

    public IEnumerator Restore()
    {
        anim.SetBool("restore", true);
        yield return new WaitForSeconds(1.667f);
        anim.SetBool("restore", false);

    }

    public IEnumerator GetHit() // 0.8 speed
    {
        anim.SetBool("gethit", true);
        yield return new WaitForSeconds(0.584f);
        anim.SetBool("gethit", false);
    }

    public IEnumerator Die()
    {
        anim.SetTrigger("die");
        yield return new WaitForSeconds(1.0f);
    }

}
