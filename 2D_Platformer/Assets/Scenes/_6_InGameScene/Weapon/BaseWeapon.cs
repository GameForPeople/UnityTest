using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseWeapon : MonoBehaviour {

    public GameObject ownerCharacter;
    public string ownerCharacterName = "TestCharacter";

    public int damage = 0;
    public float coolTime = 0;
    public bool isEnableFire = true;
    public bool isFireAnimationLoop = true;

	// Use this for initialization
	public void Start ()
    {
        ownerCharacter = GameObject.Find(ownerCharacterName);
        InitWeaponSpec();
    }

    public virtual void InitWeaponSpec()
    {
    }

    // Update is called once per frame
    //public virtual void Update () {
	//	
	//}

    public void Fire()
    {
        if (isEnableFire)
        {
            StartCoroutine("ReloadCoolTime");   
            StartCoroutine("AnimationCoroutine");

        }
    }

    IEnumerable ReloadCoolTime()
    {
        isEnableFire = false;

        yield return new WaitForSeconds(coolTime);
        isEnableFire = true;
    }

    IEnumerable AnimationCoroutine()
    {
        isFireAnimationLoop = true;

        while (isFireAnimationLoop)
        {
            FireAnimation();
            yield return new WaitForSeconds( 1.0f / 60.0f ); // 이거 시간말고, 업데이트 끝나면 바꾸는거 그걸로..
        }
    }

    public virtual void FireAnimation()
    {
    }
}
