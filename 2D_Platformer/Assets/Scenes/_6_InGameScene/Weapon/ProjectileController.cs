using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour {

    GameObject[] fireballArr = new GameObject[5];
    GameObject[] lightballArr = new GameObject[5];
    GameObject[] enemyProjectileArr = new GameObject[5];

    // Use this for initialization
    void Start () {
        fireballArr[0] = GameObject.Find("FireBall_0");
        fireballArr[1] = GameObject.Find("FireBall_1");
        fireballArr[2] = GameObject.Find("FireBall_2");
        fireballArr[3] = GameObject.Find("FireBall_3");
        fireballArr[4] = GameObject.Find("FireBall_4");

        lightballArr[0] = GameObject.Find("LightBall_0");
        lightballArr[1] = GameObject.Find("LightBall_1");
        lightballArr[2] = GameObject.Find("LightBall_2");
        lightballArr[3] = GameObject.Find("LightBall_3");
        lightballArr[4] = GameObject.Find("LightBall_4");

        enemyProjectileArr[0] = GameObject.Find("BossProjectile_0");
        enemyProjectileArr[1] = GameObject.Find("BossProjectile_1");
        enemyProjectileArr[2] = GameObject.Find("BossProjectile_2");
        enemyProjectileArr[3] = GameObject.Find("BossProjectile_3");
        enemyProjectileArr[4] = GameObject.Find("BossProjectile_4");
    }

    // Update is called once per frame
    //void Update () {
	//	
	//}

    public void AttackFireBall(int InDir, Vector3 InPos)
    {
        for(int i = 0; i < 5; ++i)
        {
            if(fireballArr[i].GetComponent<Projectile>().isOnMove == false)
            {
                fireballArr[i].GetComponent<Projectile>().StartAttack(InDir, InPos);
                return;
            }
        }
    }

    public void AttackLightBall(int InDir, Vector3 InPos)
    {
        for (int i = 0; i < 5; ++i)
        {
            if (lightballArr[i].GetComponent<Projectile>().isOnMove == false)
            {
                lightballArr[i].GetComponent<Projectile>().StartAttack(InDir, InPos);
                return;
            }
        }
    }

    public void AttackBossNormal(Vector3 InBossPos, Vector3 InCharPos)
    {
        for (int i = 0; i < 5; ++i)
        {
            if (enemyProjectileArr[i].GetComponent<EnemyProjectile>().isOnMove == false)
            {
                enemyProjectileArr[i].GetComponent<EnemyProjectile>().StartNormalAttack(InBossPos, InCharPos);
                return;
            }
        }
    }

    public void AttackBossSuper(int InDir, Vector3 InPos)
    {
        int yBuffer = 0;

        for(int i = 0; i < 5; ++i)
        {
            enemyProjectileArr[i].GetComponent<EnemyProjectile>().StartSuperAttack(InDir, InPos, yBuffer);

            yBuffer++;
        }
    }
}
