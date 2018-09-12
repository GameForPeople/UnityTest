using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour {

    new Rigidbody2D rigidbody;

    int moveDir;
    float newPosX;
    public Vector3 newMove;

    public bool isOnMove = false;
    bool isOnSuper = false;
    // Use this for initialization
    void Start () {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    public void StartSuperAttack(int InDir, Vector3 InPos, int InIndex)
    {
        isOnSuper = true;

        transform.position = new Vector3(InPos.x, 2.0f + (float)InIndex, 1);

        moveDir = InDir;

        transform.localScale = new Vector3(moveDir, 1, 1);

        newPosX = moveDir * 3.0f;
        newMove.Set(newPosX, 0, 0);

        rigidbody.MovePosition(transform.position + newMove);

        isOnMove = true;
        StartCoroutine("ProjectileCoroutine");
    }

    public void StartNormalAttack(Vector3 InBossPos, Vector3 InCharPos)
    {
        isOnSuper = false;

        transform.position = new Vector3(InBossPos.x, InBossPos.y - 0.5f, 1);

        if(InBossPos.x > InCharPos.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        newMove = (InCharPos - InBossPos).normalized;

        isOnMove = true;
        StartCoroutine("ProjectileCoroutine");
    }

    void FixedUpdate()
    {
        if (isOnMove)
        {
            if (isOnSuper)
                MoveLeftOrRight();
            else
                MoveToCharacter();
        }
    }

    void MoveLeftOrRight()
    {
        newPosX = moveDir * 8.0f * Time.deltaTime;
        newMove.Set(newPosX, 0, 0);

        rigidbody.MovePosition(transform.position + newMove);
    }

    void MoveToCharacter()
    {
        rigidbody.MovePosition(transform.position + newMove * 10.0f * Time.deltaTime);
    }

    IEnumerator ProjectileCoroutine()
    {
        yield return new WaitForSeconds(1.6f);

        TurnOff();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Attach : " + other.gameObject.layer);

        // 같은 총알들과, 보스 몸뚱아리 등 무시
        if (other.gameObject.layer == 12 
            || other.gameObject.layer == 11
            || other.gameObject.layer == 10)
        {
        }
        else if (other.gameObject.layer == 9)
        {
            if (other.GetComponent<CharacterController>().isOnControl == true)
            {
                // 여기 Other에서 받았는데, 여기서 죽이는 게 낮지 않나...? 성능을 위해서는 여기,, 코드의 아름다움을 위해서는! 씐 컨트롤러에서...
                GameObject.Find("InGameSceneManager").GetComponent<InGameSceneManager>().DeathLocalPlayer();
            }
            // else일 경우는 로컬 플레이어가 아닌 멀티에서 조작되는 플레이어 --> 이거는 너가 감히 충돌체크하지마
        }
        else
        {
            TurnOff();
        }
    }

    void TurnOff()
    {
        isOnMove = false;
        rigidbody.MovePosition(new Vector2(500, 500));
    }
}
