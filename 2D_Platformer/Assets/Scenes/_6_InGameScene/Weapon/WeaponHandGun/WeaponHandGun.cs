using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandGun : BaseWeapon {

    const int HANDGUN_DAMAGE = 35;
    const float HANDGUN_COOLTIME = 0.5f;

    // Use this for initialization
    public override void InitWeaponSpec() {
        damage = HANDGUN_DAMAGE;
        coolTime = HANDGUN_COOLTIME;
    }
	
	// Update is called once per frame
	//void Update () {
	//	
	//}

    public override void FireAnimation()
    {
        //base.FireAnimation(); // 이거 아무것도 없는데 굳이 해야해..?


        // isFireAnimationLoop = false;
    }
}
