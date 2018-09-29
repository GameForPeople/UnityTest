using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InGameSceneManager : MonoBehaviour
{
    GameObject networkObject;
    GameObject hostCharacter;
    GameObject guestCharacter;
    public GameObject projectileController;
    GameObject cameraController;

    int localPlayerIndex; // 1일 경우 Host, 2일 경우 Guest..! 자주써야하니까 

    public bool isLiveLocalPlayer = true; // 로컬플레이어(내가) 죽었을 경우
    public bool isLiveNetworkPlayer = true; // 같이 하는 플레이어가 죽었을 경우

    public bool isNetworkCoroutine = true;

    //use network Send Buffer
    public bool outInputLeft = true;
    public bool outInputRight = true;
    public bool outIsJump = true;
    public bool outIsFire = true;
    public float outCharX = 0.0f;
    public float outCharY = 0.0f;

    // Use this for initialization
    void Start()
    {
        InitMemberObject();
        //hostCharacter = GameObject.Find("HostCharacter");
        //guestCharacter = GameObject.Find("GuestCharacter");
        //projectileController = GameObject.Find("ProjectileController");
        //cameraController = GameObject.Find("MainCamera");

        InitLocalPlayer();
        //cameraController.GetComponent<InGameSceneCameraController>().SetTargetCharacter(1);
        //hostCharacter.GetComponent<CharacterController>().isOnControl = true;
        //localPlayerIndex = 1;

        //for local Test
        //isNetworkCoroutine = false;

        StartCoroutine(InGameNetworkFunction());
    }

    private void InitMemberObject()
    {
        networkObject = GameObject.Find("GameCores").transform.Find("NetworkManager").gameObject;
        networkObject.GetComponent<NetworkManager>().inGameScenemanager = GameObject.Find("InGameSceneManager");

        hostCharacter = GameObject.Find("HostCharacter");
        guestCharacter = GameObject.Find("GuestCharacter");
        projectileController = GameObject.Find("ProjectileController"); 
        cameraController = GameObject.Find("MainCamera");
    }

    private void InitLocalPlayer()
    {
        // 캐릭터 카메라 및 조작On 세팅
        if (networkObject.GetComponent<NetworkManager>().isHost)
        {
            cameraController.GetComponent<InGameSceneCameraController>().SetTargetCharacter(1);
            hostCharacter.GetComponent<CharacterController>().isOnControl = true;
            localPlayerIndex = 1;
        }
        else if (!networkObject.GetComponent<NetworkManager>().isHost)
        {
            cameraController.GetComponent<InGameSceneCameraController>().SetTargetCharacter(2);
            guestCharacter.GetComponent<CharacterController>().isOnControl = true;
            localPlayerIndex = 2;
        }
    }

    IEnumerator InGameNetworkFunction()
    {
        while (isNetworkCoroutine)
        {
            if (isLiveLocalPlayer)
            {
                if (localPlayerIndex == 1)
                {
                    hostCharacter.GetComponent<CharacterController>().SendDataProcess(ref outCharX, ref outCharY, ref outInputLeft, ref outInputRight, ref outIsJump, ref outIsFire);
                }
                else
                {
                    guestCharacter.GetComponent<CharacterController>().SendDataProcess(ref outCharX, ref outCharY, ref outInputLeft, ref outInputRight, ref outIsJump, ref outIsFire);
                }

                networkObject.GetComponent<NetworkManager>().SendData((int)PROTOCOL.SEND_GAMESTATE);
            }
            else
            {
                networkObject.GetComponent<NetworkManager>().SendData((int)PROTOCOL.SEND_VOIDGAMESTATE);
            }

            yield return new WaitForSeconds(1.0f / 30.0f);
        }
    }

    public void RecvDataProcess(float InPosX, float InPosY, bool InInputLeft, bool InInputRight, bool InIsJump, bool InIsFire )
    {
        if (InIsJump || InIsFire)
        {
            Debug.Log("X : " + InPosX + "   Y : " + InPosY);
        }

        if (localPlayerIndex == 1)
        {
            guestCharacter.GetComponent<CharacterController>().RecvDataProcess(InPosX, InPosY, InInputLeft, InInputRight, InIsJump, InIsFire);
        }
        else
        {
            hostCharacter.GetComponent<CharacterController>().RecvDataProcess(InPosX, InPosY, InInputLeft, InInputRight, InIsJump, InIsFire);
        }
    }




    public void PlayerAttackProcess(int InConstCharacterIndex, int Indir, Vector2 InPosition)
    {
        if (InConstCharacterIndex == 1)
        {
           // projectileController.GetComponent<ProjectileController>().AttackFireBall(Indir, InPosition);
        }
        else if (InConstCharacterIndex == 2)
        {
           // projectileController.GetComponent<ProjectileController>().AttackLightBall(Indir, InPosition);
        }

        return;
    }

    public void BossAttack(int InType, Vector3 InBossPosition, int InDir)
    {
        if(InType == 1)
        {
            projectileController.GetComponent<ProjectileController>().AttackBossSuper(InDir, InBossPosition);
        }
        else if (InType == 2)
        {
            StartCoroutine("BossNormalAttack", InBossPosition);
        }
    }

    IEnumerator BossNormalAttack(Vector3 InBossPosition)
    {
        for (int i = 0; i < 5; ++i)
        {
            if (hostCharacter.GetComponent<CharacterController>().hp > 0)
                projectileController.GetComponent<ProjectileController>().AttackBossNormal(InBossPosition, hostCharacter.transform.position);
            else
                projectileController.GetComponent<ProjectileController>().AttackBossNormal(InBossPosition, guestCharacter.transform.position);

            yield return new WaitForSeconds(0.3f);
        }
    }


    public void DeathLocalPlayer()
    {
        // 게임 오버 조건입니다!
        if (!isLiveNetworkPlayer)
        {
            GameEnd();
            return;
        }
        
        // 오 리턴 안됐으면..네트워크 플레이어님은 살아계신거여요!
        // 메롱 조작 못해, 캐릭터님 이미 퇴근했어!
        // 카메라도 바꿔줄거지롱
        if (localPlayerIndex == 1)
        {
            hostCharacter.GetComponent<CharacterController>().isOnControl = false;
            hostCharacter.GetComponent<CharacterController>().Death();
        }
        else if (localPlayerIndex == 2)
        {
            guestCharacter.GetComponent<CharacterController>().isOnControl = false;
            guestCharacter.GetComponent<CharacterController>().Death();
        }

        StartCoroutine("DeathLocalPlayerChangeCameraCoroutine");
    }

    void GameEnd()
    {

    }

    IEnumerator DeathLocalPlayerChangeCameraCoroutine()
    {
        yield return new WaitForSeconds(2.0f);

        if (localPlayerIndex == 1)
        {
            cameraController.GetComponent<InGameSceneCameraController>().SetTargetCharacter(2);
        }
        else if (localPlayerIndex == 2)
        {
            cameraController.GetComponent<InGameSceneCameraController>().SetTargetCharacter(1);
        }
    }

}
