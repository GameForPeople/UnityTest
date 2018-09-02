using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameSceneManager : MonoBehaviour
{
    GameObject networkObject;
    GameObject hostCharacter;
    GameObject guestCharacter;
    GameObject projectileController;
    GameObject cameraController;

    // Use this for initialization
    void Start()
    {
        InitMemberObject();

        //InitLocalPlayer();
        cameraController.GetComponent<InGameSceneCameraController>().SetTargetCharacter(1);
        hostCharacter.GetComponent<CharacterController>().isOnControl = true;
    }

    void InitMemberObject()
    {
        networkObject = GameObject.Find("GameCores").transform.Find("NetworkManager").gameObject;
        hostCharacter = GameObject.Find("HostCharacter");
        guestCharacter = GameObject.Find("GuestCharacter");
        projectileController = GameObject.Find("ProjectileController");
        cameraController = GameObject.Find("MainCamera");
    }

    void InitLocalPlayer()
    {
        // 캐릭터 카메라 및 조작On 세팅
        if (networkObject.GetComponent<NetworkManager>().isHost)
        {
            cameraController.GetComponent<InGameSceneCameraController>().SetTargetCharacter(1);
            hostCharacter.GetComponent<CharacterController>().isOnControl = true;
        }
        else if (!networkObject.GetComponent<NetworkManager>().isHost)
        {
            cameraController.GetComponent<InGameSceneCameraController>().SetTargetCharacter(2);
            guestCharacter.GetComponent<CharacterController>().isOnControl = true;
        }
    }

    public void PlayerAttack(int InConstCharacterIndex, int Indir, Vector3 InPosition)
    {
        if(InConstCharacterIndex == 1)
            projectileController.GetComponent<ProjectileController>().AttackFireBall(Indir, InPosition);
        else if(InConstCharacterIndex == 2)
            projectileController.GetComponent<ProjectileController>().AttackLightBall(Indir, InPosition);
    }

    // Update is called once per frame
    void Update()
    {

    }


}
