using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEnemy : MonoBehaviour {
    GameObject inGameSceneManager;

    // Use this for initialization
    void Start () {
        inGameSceneManager = GameObject.Find("InGameSceneManager");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 9)
        {
            if (other.GetComponent<CharacterController>().isOnControl == true)
            {
                // 여기 Other에서 받았는데, 여기서 죽이는 게 낮지 않나...? 성능을 위해서는 여기,, 코드의 아름다움을 위해서는! 씐 컨트롤러에서...
                inGameSceneManager.GetComponent<InGameSceneManager>().DeathLocalPlayer();
            }
            // else일 경우는 로컬 플레이어가 아닌 멀티에서 조작되는 플레이어 --> 이거는 너가 감히 충돌체크하지마
        }
        else if (other.gameObject.layer == 10)
        {
            //Debug.Log("프로젝타일과 무빙적이 충돌했습니다.");
            GetComponent<MovingPlat>().ObjectRunSwitch(false);

            GetComponent<Rigidbody2D>().MovePosition(new Vector3(-200, -100, 0));
        }
    }
}
