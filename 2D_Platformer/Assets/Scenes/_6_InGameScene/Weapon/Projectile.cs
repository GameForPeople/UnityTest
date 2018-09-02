using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    new Rigidbody2D rigidbody;

    int moveDir;
    float newPosX;
    Vector3 newMove;
    public int moveCount = 0;

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

        newPosX = moveDir * 3.0f;
        newMove.Set(newPosX, 0, 0);
        rigidbody.MovePosition(transform.position + newMove);

        moveDir = InDir;

        moveCount = 90;
        StartCoroutine("ProjectileCoroutine");
    }

    void MoveLeftOrRight()
    {
        newPosX = moveDir * 15.0f * Time.deltaTime;
        newMove.Set(newPosX, 0, 0);
        //transform.position = new Vector3(newPosX, transform.position.y, transform.position.z);
        rigidbody.MovePosition(transform.position + newMove);
    }

    IEnumerator ProjectileCoroutine()
    {
        while (moveCount > 0)
        {
            --moveCount;
            MoveLeftOrRight();

            yield return new WaitForSeconds(1.0f / 60.0f);
        }
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
        moveCount = 0;

        rigidbody.MovePosition(new Vector2(-100, -100));
    }
}
