using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    new Rigidbody2D rigidbody;

    int moveDir;
    float newPosX;
    Vector3 newMove;

    public bool isOnMove = false;

    // Use this for initialization
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    //void Update () {
    //	
    //}

    //void FixedUpdate()
    //{
    //}

    public void StartAttack(int InDir, Vector3 InPos)
    {
        transform.position = new Vector3(InPos.x, InPos.y + 0.1f, 1);

        moveDir = InDir;
        newPosX = moveDir * 3.0f;
        newMove.Set(newPosX, 0, 0);
        rigidbody.MovePosition(transform.position + newMove);

        isOnMove = true;
        StartCoroutine("ProjectileCoroutine");
    }

    void FixedUpdate()
    {
        if (isOnMove)
        {
            MoveLeftOrRight();
        }
    }

    void MoveLeftOrRight()
    {
        newPosX = moveDir * 14.0f * Time.deltaTime;
        newMove.Set(newPosX, 0, 0);

        rigidbody.MovePosition(transform.position + newMove);
    }

    public IEnumerator ProjectileCoroutine()
    {
        bool slowTime;

        yield return new WaitForSeconds(0.85f);
        
        TurnOff();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Attach : " + other.gameObject.layer);

        if (other.gameObject.layer == 9 || other.gameObject.layer == 10)
        {
            //other.GetComponent<Projectile>();
            // 이거는 캐릭터꺼야.
        }
        else
        {
            // 여기서 if를 하나 추가해야해, 적일때 추가
            TurnOff();
        }
    }

    void TurnOff()
    {
        isOnMove = false;
        rigidbody.MovePosition(new Vector2(-100, -100));
    }
}
