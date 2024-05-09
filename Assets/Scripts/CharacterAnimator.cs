using System.Collections;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        if ( _anim == null )
            Debug.LogError("[CharacterAnimator] Awake : Animator cannot be null");
    }

    public IEnumerator Move(Vector3 toPos)
    {
        Debug.Log("[CharacterAnimator] Move / currPos : " + gameObject.transform.position + " / toPos : " + toPos);

        _anim.SetBool("move", true);
        Vector3 fromPos = this.transform.position;

        float speed = 22.0f;
        this.transform.LookAt(toPos);
        while (Vector3.Distance(this.transform.position, toPos) > 0.01f)
        {
            yield return null;
            var dir = (toPos - this.transform.position).normalized;
            this.transform.position += dir * speed * Time.deltaTime;
        }
        _anim.SetBool("move", false);
    }

    public IEnumerator Attack()
    {
        Debug.Log("[CharacterAnimator] Attack");

        _anim.SetBool("attack", true);
        yield return new WaitForSeconds(1.334f);
        _anim.SetBool("attack", false);
    }

    public IEnumerator Guard() // 0.8 speed
    {
        _anim.SetBool("guard", true);
        yield return new WaitForSeconds(0.417f);
        _anim.SetBool("guard", false);
    }

    public IEnumerator Restore()
    {
        _anim.SetBool("restore", true);
        yield return new WaitForSeconds(1.667f);
        _anim.SetBool("restore", false);

    }

    public IEnumerator GetHit() // 0.8 speed
    {
        _anim.SetBool("gethit", true);
        yield return new WaitForSeconds(0.584f);
        _anim.SetBool("gethit", false);
    }

    public IEnumerator Die()
    {
        _anim.SetTrigger("die");
        yield return new WaitForSeconds(1.0f);
    }

}
