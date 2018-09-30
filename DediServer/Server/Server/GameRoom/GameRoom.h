#pragma once

#include "../stdafx.h"
#include "../Protocol/CommunicationProtocol.h"

#define ROOM_MAX 500

enum class ROOM_STATE {
	ROOM_STATE_VOID		,	// 할당만 되어있고, 방 없어..!
	ROOM_STATE_SOLO		,	// 방에 한명이 대기중인 상태
	ROOM_STATE_WAIT		,	// 방이 꽉 차고, 플레이를 대기 중인 상태
	ROOM_STATE_PLAY			// 게임이 플레이 중인 상태
};

class GameRoom {
	InGameDataStruct *clientData; //new InGameDataStruct[2];

public:
	ROOM_STATE roomState;
	
	//std::atomic<bool> isOnRenew[2]{ false };
	int* userIndex;
	//bool isOnReady[2]{false}; //0이 Host, 1이 Guest
	
	bool isDeathBoss{ false };	// 한 명이 나갈 경우! true로 처리해줌. 과연? 이걸 쓸 수 있을까
	bool isLivePlayer[2];
							//
	__inline GameRoom() : roomState(ROOM_STATE::ROOM_STATE_VOID)
	{
	};
	//__inline ~GameRoom() = default;
	__inline ~GameRoom() 
	{
		if (clientData != nullptr)
		{
			delete[]clientData;
			delete[]userIndex;
		}
	}

	__inline void ExitRoom()
	{
		roomState = ROOM_STATE::ROOM_STATE_VOID;
		delete[] clientData;
		delete[] userIndex;
	}

	__inline void CreateRoom(const int InHostIndex)
	{
		roomState = ROOM_STATE::ROOM_STATE_SOLO;
		//isOnRenew[0] = true; // 방을 만드는 입장에서는 항상 0이 비어있어야함!
		//이걸 여기다가 쓰지 말고, 데이터를 보내야 할때만 쓰자.
		
		clientData = new InGameDataStruct[2];
		userIndex = new int[2];

		userIndex[0] = InHostIndex;

		isDeathBoss = false;

		isLivePlayer[0] = true;
		isLivePlayer[1] = true;
	}

	__inline void JoinRoom(const int InGuestIndex)
	{
		// 클라에서 JoinRoom하자마자, 클라자체 바로 로딩 들어가고, 호스트는 모르니까 On시켜줌
		//if (roomState == ROOM_STATE::ROOM_STATE_SOLO) {
		userIndex[1] = InGuestIndex;
		roomState = ROOM_STATE::ROOM_STATE_WAIT;
	}
	
	void SaveClientData(const InGameDataStruct& InStruct, const bool InIsHost)
	{
		if (InIsHost)
		{
			clientData[0].SetValues(InStruct);
		}
		else
		{
			clientData[1].SetValues(InStruct);
		}
	}

	void GetClientData(const bool InIsHost, 
		float& OutPosX, float& OutPosY, bool& OutInputLeft, bool& OutInputRight, 
		bool& OutIsOnJump, bool& OutIsOnFire)
	{
		if (InIsHost)
		{
			clientData[1].GetValues(OutPosX, OutPosY, OutInputLeft, OutInputRight, OutIsOnJump, OutIsOnFire);
		}
		else
		{
			clientData[0].GetValues(OutPosX, OutPosY, OutInputLeft, OutInputRight, OutIsOnJump, OutIsOnFire);
		}
	}

	bool GetGameReady()
	{
		if (roomState == ROOM_STATE::ROOM_STATE_WAIT)
			return true;
		
		return false;
	}

	// 0 아직 안끝남, 1 승리, 2 실패
	int GetEndOfGame()
	{
		if (isDeathBoss)
			return 1;
		else if (isLivePlayer[0] == false && isLivePlayer[1] == false)
		{
			//ExitRoom();
			return 2;
		}
		else return 0;
	}

	InGameDataStruct* GetThis(const bool InIsHost)
	{
		if (InIsHost)
		{
			return clientData[1].GetThis();
		}
		else
		{
			return clientData[0].GetThis();
		}
	}

	void SetDeathPlayer(const bool InIsHost)
	{
		if (InIsHost)
		{
			isLivePlayer[1] = false;
		}
		else
		{
			isLivePlayer[0] = false;
		}
	}
	void SetBossDeath()
	{
		isDeathBoss = true;
	}

	bool GetNetworkPlayerIsLive(const bool InIsHost)
	{
		if (InIsHost)
			return isLivePlayer[1];
		else 
			return isLivePlayer[0];
	}
};

class CGameRoom {
	std::vector<GameRoom> rooms;

public:

	__inline CGameRoom()
	{
		rooms.reserve(ROOM_MAX);

		for (int i = 0; i < ROOM_MAX; ++i)
		{
			rooms.emplace_back();
		}
	}

	__inline ~CGameRoom() = default;

	int CreateRoom(const int& InHostIndex) {
		for (int i = 0; i < ROOM_MAX; ++i) 
		{
			// 나중에 임계영역 걸어주긴 해야함.
			if (rooms[i].roomState == ROOM_STATE::ROOM_STATE_VOID)
			{
				rooms[i].CreateRoom(InHostIndex);

				return i; // roomIndex;
			}
		}
	}

	int JoinRoom(const int& InGuestIndex, int& RetRoomIndex)
	{
		if (RetRoomIndex == -1) 
		{
			for (RetRoomIndex = 0; RetRoomIndex < ROOM_MAX; ++RetRoomIndex)
			{
				if (rooms[RetRoomIndex].roomState == ROOM_STATE::ROOM_STATE_SOLO)
				{
					rooms[RetRoomIndex].JoinRoom(InGuestIndex);

					return 0;
				}
			}
			return 3; //들어갈 수 있는 방이 없음
		}
		else 
		{
			if (rooms[RetRoomIndex].roomState == ROOM_STATE::ROOM_STATE_SOLO)
			{
				rooms[RetRoomIndex].JoinRoom(InGuestIndex);

				return 0;
			}
			else if (rooms[RetRoomIndex].roomState == ROOM_STATE::ROOM_STATE_VOID)
			{
				return 1;
			}
			else if (rooms[RetRoomIndex].roomState == ROOM_STATE::ROOM_STATE_PLAY 
			|| rooms[RetRoomIndex].roomState == ROOM_STATE::ROOM_STATE_WAIT)
			{
				return 2;
			}
		}
	}

	int GetEnemyIndex(const int& InRoomIndex, const bool InIsHost) 
	{
		if (InIsHost)
			return rooms[InRoomIndex].userIndex[1];
		else
			return rooms[InRoomIndex].userIndex[0];
	}

	__inline bool GetGameReady(const int& InRoomIndex)
	{
		return (rooms[InRoomIndex].GetGameReady());
	}

	void SaveClientData(const int& InRoomIndex, const bool InIsHost, const InGameDataStruct & InStruct)
	{
		rooms[InRoomIndex].SaveClientData(InStruct, InIsHost);
	}

	void GetClientData(const int InRoomIndex, const bool InIsHost,
		float& OutPosX, float& OutPosY, bool& OutInputLeft, bool& OutInputRight,
		bool& OutIsOnJump, bool& OutIsOnFire)
	{
		rooms[InRoomIndex].GetClientData(InIsHost,
			OutPosX, OutPosY, OutInputLeft, OutInputRight, OutIsOnJump, OutIsOnFire);
	}

	void PlayerDeath(const int& InRoomIndex, const bool& InIsHost)
	{
		rooms[InRoomIndex].SetDeathPlayer(InIsHost);
	}

	void BossDeath(const int& InRoomIndex)
	{
		rooms[InRoomIndex].SetBossDeath();
	}

	int GetEndOfGame(const int& InRoonIndex)
	{
		return (rooms[InRoonIndex].GetEndOfGame());
	}

	void ExitRoom(const int& InRoomIndex)
	{
		rooms[InRoomIndex].ExitRoom();
	}

	bool GetNetworkPlayerIsLive(const int& InRoomIndex, const bool& InIsHost)
	{
		return rooms[InRoomIndex].GetNetworkPlayerIsLive(InIsHost);
	}
};
