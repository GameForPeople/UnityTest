using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;


public class InGameScene : MonoBehaviour {

    public GameObject players;

	// Use this for initialization
	void Start () {
        players = GameObject.Find("OtherPlayer");
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void ProcessRecvData(byte[] Inbyte)
    {
        int DataCount = BitConverter.ToInt32(Inbyte, 0) - 400;

        for( int i = 0; i < DataCount ; i++)
        {
            int index = BitConverter.ToInt32(Inbyte, 4 + 8 * i); // 해당 인덱스는 플레이어 인덱스를 뜻함..!

            players.SendMessage("ProcessRecvData", (BitConverter.ToInt32(Inbyte, 8 + 8 * i) )); // 해당 인덱스한테 날려줌.
        }
    }

}
