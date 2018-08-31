using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameSceneCameraController : MonoBehaviour {

    GameObject focusTarget;
    public string TargetName = "TestCharacter";

    float nowDistance = 0.0f;

    const float STANDARD_DISTANCE = 0.25f;    //  카메라와 캐릭터의 기준 거리를 STANDATD_DISTANCE로 둔다.
    const float SOFT_LIMIT_DISTANCE = 0.5f;    //  카메라와 캐릭터의 거리가 
    const float HARD_LIMIT_DISTANCE = 1.0f;

    const float SOFT_MOVE_SPEED = 90.0f;
    const float HARD_MOVE_SPEED = 20.0f;

    Vector3 positionBuffer;
    int intBuffer;

    // Use this for initialization
    void Start () {
        //Debug.Log("CameraStart!!");

        focusTarget = GameObject.Find(TargetName);
        positionBuffer.Set((focusTarget.transform.position.x - STANDARD_DISTANCE) , (focusTarget.transform.position.y + 0.5f), -10);
        transform.position = positionBuffer;
        // 이거 쪼금 그런게, 객체 자체를 저장하기보다는, 객체의 포지션을 하나 갖고 있는게 적합할듯?? -> 고러면 갱신이 알아서 되나? 포인터를 공유해서?, 안될껄?, 몰러이씨
    }
	
    public void SetNewTarget (string InString)
    {
        TargetName = InString;
        focusTarget = GameObject.Find(TargetName); // InString 바로 넣어도 되는데, 굳이..?
    }

    // Update is called once per frame
    void FixedUpdate() {

        //transform.position = focusTarget.transform.position;

        CameraStateTest();
    }

    void CameraStateTest()
    {
        intBuffer = focusTarget.GetComponent<CharacterController>().dirLeftOrRightBuffer; // 이거 나중에는 메세징으로 변경해서, 캐릭터에 해당 키가 눌릴 때 마다 변경하는게 낳긴 한데 구찮어


        if (intBuffer == 1)
        {
            positionBuffer.Set(focusTarget.transform.position.x - STANDARD_DISTANCE, focusTarget.transform.position.y + 0.5f, -10);
        }
        else if (intBuffer == -1)
        {
            positionBuffer.Set(focusTarget.transform.position.x + STANDARD_DISTANCE, focusTarget.transform.position.y + 0.5f, -10);
        }

        XDistanceTest();
        YDistanceTest();
    }

    void XDistanceTest()
    {
        if (positionBuffer.x >= transform.position.x)
        {
            nowDistance = positionBuffer.x - transform.position.x;

            if (nowDistance > HARD_LIMIT_DISTANCE)
            {
                //transform.position.Set(transform.position.x + nowDistance / 20.0f, transform.position.y, 0);
                transform.position = new Vector3(transform.position.x + nowDistance / HARD_MOVE_SPEED, transform.position.y, -10);
            }
            else if (nowDistance > SOFT_LIMIT_DISTANCE)
            {
                //transform.position.Set(transform.position.x + nowDistance / 60.0f, transform.position.y, 0);
                transform.position = new Vector3(transform.position.x + nowDistance / SOFT_MOVE_SPEED, transform.position.y, -10);
            }
            else if (nowDistance < STANDARD_DISTANCE)
            {
                //transform.position = positionBuffer;
                //transform.position = new Vector3(transform.position.x + 1.0f / 180.0f , transform.position.y, -10);
            }
        }
        else if (positionBuffer.x < transform.position.x)
        {
            nowDistance = transform.position.x - positionBuffer.x;

            if (nowDistance > HARD_LIMIT_DISTANCE)
            {
                //transform.position.Set(transform.position.x - nowDistance / 20.0f, transform.position.y, 0);
                transform.position = new Vector3(transform.position.x - nowDistance / HARD_MOVE_SPEED, transform.position.y, -10);
            }
            else if (nowDistance > SOFT_LIMIT_DISTANCE)
            {
                //transform.position.Set(transform.position.x - nowDistance / 60.0f, transform.position.y, 0);
                transform.position = new Vector3(transform.position.x - nowDistance / SOFT_MOVE_SPEED, transform.position.y, -10);
            }
            else if (nowDistance < STANDARD_DISTANCE)
            {
                //transform.position = positionBuffer;
                //transform.position = new Vector3(transform.position.x - 1.0f / 180.0f, transform.position.y, -10);
            }
        }
    }

    void YDistanceTest()
    {
        if (positionBuffer.y >= transform.position.y)
        {
            nowDistance = (positionBuffer.y - transform.position.y) * 2;

            if (nowDistance > HARD_LIMIT_DISTANCE )
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + nowDistance / HARD_MOVE_SPEED, -10);
            }
            else if (nowDistance > SOFT_LIMIT_DISTANCE )
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + nowDistance / SOFT_MOVE_SPEED, -10);
            }
            else if (nowDistance < STANDARD_DISTANCE )
            {
                //transform.position = positionBuffer; // 이렇게 바꾸면 x, y 좌표 둘다 바뀐다 바보야
            }
        }
        else if (positionBuffer.y < transform.position.y)
        {
            nowDistance = (transform.position.y - positionBuffer.y) * 2;

            if (nowDistance > HARD_LIMIT_DISTANCE )
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - nowDistance / HARD_MOVE_SPEED, -10);
            }
            else if (nowDistance > SOFT_LIMIT_DISTANCE )
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - nowDistance / SOFT_MOVE_SPEED, -10);
            }
            else if (nowDistance < STANDARD_DISTANCE )
            {
                //transform.position = positionBuffer;
            }
        }
    }

    //void FixedUpdate()
    //{
    // 음 굳이... 카메라 움직임을 고정 업데이트에서 처리해줄 필요가 있는가... 이건 테스트를 해봐야 알 거 같어!
    //}
}
