#include "Server.h"

static std::vector<CUserData> userData;	//쓰레드에서도 사용해야하는데, 유저 데이터가 겁나많으면 어떻게 하려고!!, 전역으로 선언해서 걍 쓰자!ㅎㅎㅎㅎㅎㅎㅎㅎ
static bool isSaveOn{ false };	// 저장 여부 판단 (클라에 의해, 정보 변경 요청 받을 시 true로 변경 / 굳이 동기화 필요없을듯)

DWORD WINAPI SaveUserDate(LPVOID arg);

int main(int argc, char * argv[])
{
	char* retIPChar;
	retIPChar = new char[20]; // IPv4가 20 char보다 클일 죽어도 없음.
	GetExternalIP(retIPChar);

	printf("■■■■■■■■■■■■■■■■■■■■■■■■■\n");
	printf("■ IOCP Server  - Bridge Unity Project          \n");
	printf("■                                ver 0.1 180815\n");
	printf("■\n");
	printf("■    IP Address : %s \n", retIPChar);
	printf("■    Server Port : %d \n", SERVER_PORT);
	printf("■■■■■■■■■■■■■■■■■■■■■■■■■\n\n");

	delete []retIPChar;

#pragma region [Load UserData]

	std::ifstream inFile("UserData.txt", std::ios::in);
	
	std::string ID;
	int PW, winCount, loseCount, Money;
	int userDataCount{};

	inFile >> userDataCount;
	userData.reserve(userDataCount);

	for (int i = 0; i < userDataCount; i++) {
		inFile >> ID >> PW >> winCount >> loseCount >> Money;

		userData.emplace_back(ID, PW, winCount, loseCount, Money);
	}
	
	inFile.close();

	std::cout << "  - UserData Load Complete! " << std::endl << std::endl;


	//for (auto i : userData) 
	//{
	//	std::cout << i.GetID() << " " << i.GetPW() << " " << i.GetWinCount() << " " << i.GetLoseCount() << "  " << i.GetMoney() << std::endl;
	//}

#pragma endregion

#pragma region [ 윈속 초기화 및 입출력 완료 포트 생성 ]
	//윈속 초기화
	WSADATA wsa;
	if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) return 1;

	// 입출력 완료 포트 생성
	HANDLE hcp = CreateIoCompletionPort(INVALID_HANDLE_VALUE, NULL, 0, 0);
	/*
		CreateIoCompletionPort는 두가지 역할을 함!
			1. 입출력 완료 포트 생성
			2. 소켓과 입출력 완료 포트 연결 (IO장치와 IOCP연결)

		1번째 인자값,  IOCP와 연결할 핸들, 생성시는 INVALID_HANDLE_VALUE를 인자로 넘김
		2번째 인자값,  IOCP 핸들, 첫 생성시는 NULL
		3번째 인자값, IO완료시 넘어갈 값, 사용자가 넘기고 싶은 값 넘김
		4번째 인자값, 한번에 동작할 최대 스레드 개수, 0 넘기면 프로세스 숫자로 자동 지정됨
	*/

	if (hcp == NULL) 
	{ 
		printf("입출력 완료 포트 생성");
		return 1; 
	}

	// CPU 개수 확인
	SYSTEM_INFO si;
	GetSystemInfo(&si);

	// CPU 개수 == 12개 * 2개의 작업자 스레드 생성
	// IO작업이 완료된 후, 완료된 IO에 대한 처리를 수행할 스레드 풀을 구성한다.
	// 일반적으로 스레드 풀의 크기는 프로세서 개수의 2배 정도를 할당한다.
	HANDLE hThread;  
	for (int i = 0; i < (int)si.dwNumberOfProcessors * 2; ++i) 
	{
		hThread = CreateThread(NULL, 0, WorkerThread, hcp, 0, NULL);
		if (hThread == NULL) return 1;
		CloseHandle(hThread);
	}

#pragma endregion

#pragma region [ 소켓 생성 및, 바인드, 리슨 ]
	
	//Socket()
	SOCKET listenSocket = socket(AF_INET, SOCK_STREAM, 0);
	if (listenSocket == INVALID_SOCKET) err_quit((char *)"socket()");

	//bind()
	SOCKADDR_IN serverAddr;
	ZeroMemory(&serverAddr, sizeof(serverAddr));
	serverAddr.sin_family = AF_INET;
	serverAddr.sin_addr.s_addr = htonl(INADDR_ANY);
	serverAddr.sin_port = htons(SERVER_PORT);
	int retVal = bind(listenSocket, (SOCKADDR *)&serverAddr, sizeof(serverAddr));
	if (retVal == SOCKET_ERROR) err_quit((char *)"bind()");

	// Listen()!
	retVal = listen(listenSocket, SOMAXCONN);
	if(retVal == SOCKET_ERROR) err_quit((char *)"listen()");

#pragma endregion

	printf("  - Dedicated server activated!\n\n");

#pragma region [ Thread Run! Accept and Save UserData ]
	SOCKET clientSocket;
	SOCKADDR_IN clientAddr;
	int addrLength;
	DWORD recvBytes, flags;

	HANDLE hSaveUserDataThread;
	hSaveUserDataThread = CreateThread(NULL, 0, SaveUserDate, NULL, 0, NULL);
	CloseHandle(hSaveUserDataThread);

	while (7) {
		//accept()
		addrLength = sizeof(clientAddr);
		clientSocket = accept(listenSocket, (SOCKADDR *)&clientAddr, &addrLength);
		if (clientSocket == INVALID_SOCKET)
		{
			err_display((char *)"accept()");
			break;
		}

		// 클라이언트 서버에 접속(Accept) 함을 알림
		printf("[TCP 서버] 클라이언트 접속 : IP 주소 =%s, Port 번호 = %d \n", inet_ntoa(clientAddr.sin_addr), ntohs(clientAddr.sin_port));
		
		// 소켓과 입출력 완료 포트 연결
		CreateIoCompletionPort((HANDLE)clientSocket, hcp, clientSocket, 0);
		
		// 소켓 정보 구조체 할당
		SOCKETINFO *ptr = new SOCKETINFO;
		if (ptr == NULL) break;
		
		ZeroMemory(&ptr->overlapped, sizeof(ptr->overlapped));
		ptr->sock = clientSocket;
		ptr->recvBytes = ptr->sendBytes = 0;
		ptr->wsabuf.buf = ptr->buf;
		ptr->wsabuf.len = BUF_SIZE;

		// 비동기 입출력의 시작
		flags = 0;
		retVal = WSARecv(
			clientSocket, // 클라이언트 소켓
			&ptr->wsabuf, // 읽을 데이터 버퍼의 포인터
			1,			 // 데이터 입력 버퍼의 개수
			&recvBytes,  // recv 결과 읽은 바이트 수, IOCP에서는 비동기 방식으로 사용하지 않으므로 nullPtr를 넘겨도 무방
			&flags,		 // recv에 사용될 플래그
			&ptr->overlapped, // overlapped구조체의 포인터
			NULL			// IOCP에서는 사용하지 않으므로 NULL, nullptr넘겨도 무방
		);

		if (retVal == SOCKET_ERROR)
		{
			if (WSAGetLastError() != ERROR_IO_PENDING)
			{
				err_display((char *)"WSARecv()");
			}

			continue;
		}
	}
#pragma endregion

#pragma region [ Destroy() && plzDoNotQuit! ]
	char plzDoNotQuit{};
	std::cin >> plzDoNotQuit;
	
	closesocket(listenSocket);
	WSACleanup();

	return 0;
#pragma endregion
}

DWORD WINAPI SaveUserDate(LPVOID arg) {
	while (7) {
		Sleep(10000);

		if (isSaveOn) {
			isSaveOn = false;

			Sleep(2000);
			std::ofstream outFile("UserData.txt", std::ios::out);

			outFile << userData.size() << std::endl;

			for (auto i : userData) {
				outFile << " " << i.GetID()
					<< " " << i.GetPW()
					<< " " << i.GetWinCount()
					<< " " << i.GetLoseCount()
					<< " " << i.GetMoney()
					<< std::endl;
			}
			outFile.close();

			std::cout << "[ System(Server Core) - UserDataSave ]" << std::endl;
			isSaveOn = false;
			Sleep(2000);
		}
	}
	return 0;
}