#include "Server.h"

static std::vector<CUserData> userData;	//쓰레드에서도 사용해야하는데, 유저 데이터가 겁나많으면 어떻게 하려고!!, 전역으로 선언해서 걍 쓰자!ㅎㅎㅎㅎㅎㅎㅎㅎ
static bool isSaveOn{ false };	// 저장 여부 판단 (클라에 의해, 정보 변경 요청 받을 시 true로 변경 / 굳이 동기화 필요없을듯)

DWORD WINAPI SaveUserDate(LPVOID arg);
DWORD WINAPI WorkerThread(LPVOID arg);

int main(int argc, char * argv[])
{
#pragma region [Server UI]
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

	delete[]retIPChar;

#pragma endregion

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
	if (retVal == SOCKET_ERROR) err_quit((char *)"listen()");

#pragma endregion

#pragma region [ Thread Run! Accept and Save UserData ]

	printf("  - Dedicated server activated!\n\n");

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
		ptr->isRecvTrue = true;
		ptr->bufferProtocol = 0;

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

DWORD WINAPI WorkerThread(LPVOID arg)
{
	HANDLE hcp = (HANDLE)arg;

	int retVal{};
	int recvType{};
	int sendType{};

	while (7)
	{

#pragma region [ Wait For Thread ]
		//비동기 입출력 기다리기
		DWORD cbTransferred;
		SOCKET clientSocket;
		SOCKETINFO *ptr;

		// 입출력 완료 포트에 저장된 결과를 처리하기 위한 함수 // 대기 상태가 됨
		retVal = GetQueuedCompletionStatus(
			hcp, //입출력 완료 포트 핸들
			&cbTransferred, //비동기 입출력 작업으로, 전송된 바이트 수가 여기에 저장된다.
			(LPDWORD)&clientSocket, //함수 호출 시 전달한 세번째 인자(32비트) 가 여기에 저장된다.
			(LPOVERLAPPED *)&ptr, //Overlapped 구조체의 주소값
			INFINITE // 대기 시간 -> 깨울떄 까지 무한대
		);
#pragma endregion

#pragma region [ Get Socket and error Exception ]
		std::cout << "newThread Fire!" << std::endl;

		// 할당받은 소켓 즉! 클라이언트 정보 얻기
		SOCKADDR_IN clientAddr;
		int addrLength = sizeof(clientAddr);
		getpeername(ptr->sock, (SOCKADDR *)&clientAddr, &addrLength);

		//비동기 입출력 결과 확인 // 아무것도 안보낼 때는, 해당 클라이언트 접속에 문제가 생긴것으로 판단, 닫아버리겠다!
		// 근데 이거 에코서버일떄만 그래야되는거 아니야???? 몰봐 임마 뭘봐 모를수도 있지
		if (retVal == 0 || cbTransferred == 0)
		{
			std::cout << "DEBUG - Error or Exit Client A" << std::endl;

			if (retVal == 0)
			{
				DWORD temp1, temp2;
				WSAGetOverlappedResult(ptr->sock, &ptr->overlapped, &temp1, FALSE, &temp2);
				err_display((char *)"WSAGetOverlappedResult()");
			}
			closesocket(ptr->sock);

			printf("[TCP 서버] 클라이언트 종료 : IP 주소 =%s, 포트 번호 =%d\n",
				inet_ntoa(clientAddr.sin_addr), ntohs(clientAddr.sin_port));
			delete ptr;
			continue;
		}

#pragma endregion

		if (ptr->isRecvTrue)
		{
			if (ptr->bufferProtocol == 0) {
				recvType = (int&)(ptr->buf);

				if (recvType == DEMAND_LOGIN) {
					ptr->bufferProtocol = DEMAND_LOGIN;
					ptr->isRecvTrue = true;

					std::cout << "로그인 시도를 받았습니다."<< std::endl;

					//데이터 받기
					ZeroMemory(&ptr->overlapped, sizeof(ptr->overlapped));
					ptr->wsabuf.buf = ptr->buf;
					ptr->wsabuf.len = BUF_SIZE;

					DWORD recvBytes;
					DWORD flags{ 0 };
					retVal = WSARecv(
						ptr->sock, // 클라이언트 소켓
						&ptr->wsabuf, // 읽을 데이터 버퍼의 포인터
						1, // 데이터 입력 버퍼의 개수
						&recvBytes, // recv 결과 읽은 바이트 수, IOCP에서는 비동기 방식으로 사용하지 않으므로 nullPtr를 넘겨도 무방
						&flags,  // recv에 사용될 플래그
						&ptr->overlapped, // overlapped구조체의 포인터
						NULL // IOCP에서는 사용하지 않으므로 NULL, nullptr넘겨도 무방
					);
					if (retVal == SOCKET_ERROR)
					{
						if (WSAGetLastError() != WSA_IO_PENDING)
						{
							err_display((char *)"WSARecv()");
						}
						continue;
					}
					else {
						std::cout << "디맨드 로그인 바로 받아버렸다...짜피 다음 틱에서 처리해줄걸?? " << std::endl;
					}
				}
			}
			else if (ptr->bufferProtocol == DEMAND_LOGIN) {

				DemandLoginStruct demandLogin = (DemandLoginStruct&)(ptr->buf);
				std::cout << "아이디 비밀번호를 입력 받았습니다. ID:  " << demandLogin.ID << "  PW : " << demandLogin.PW << "  type : " << demandLogin.type << std::endl;
				// 1일때 로그인 없는 아이디, 2일때 로그인 잘못된 비밀번호, 3일때 이미 로그인한 아이디, 4일때 회원가입 중복된 아이디! protocol Sync plz!  // 5일때는 정상??
				
				int failReason = 0;

				if (demandLogin.type == 1) {
					int winRate{};
					int loseRate{};
					int money{};

					for (auto &i : userData)
					{
						if ( i.GetID().compare(demandLogin.ID))
						{
							if (!i.GetIsLogin())
							{
								if (i.GetPW() == demandLogin.PW)
								{
									i.SetIsLogin(true);
									winRate = i.GetWinCount();
									loseRate = i.GetLoseCount();
									money = i.GetMoney();
								}
								else
								{
									failReason = 2;
									break;
								}
							}
							else
							{
								failReason = 3;
								break;
							}
						}
					}

					if (!failReason) {

						std::cout << "로그인에 성공했습니다. " << std::endl;

						ptr->dataBuffer = new PermitLoginStruct(winRate, loseRate, money);

						ptr->bufferProtocol = PERMIT_LOGIN;
						ptr->isRecvTrue = false;
						//permitLoginStruct* a = static_cast<PermitLoginStruct *>(ptr->dataBuffer);

						// 데이터 보내기
						ZeroMemory(&ptr->overlapped, sizeof(ptr->overlapped));

						int buffer = PERMIT_LOGIN;
						memcpy(ptr->buf, (char*)&buffer, sizeof(int));
						ptr->dataSize = sizeof(int);

						ptr->wsabuf.buf = ptr->buf; // ptr->buf;
						ptr->wsabuf.len = ptr->dataSize;

						DWORD sendBytes;

						retVal = WSASend(
							ptr->sock, // 클라이언트 소켓
							&ptr->wsabuf, // 읽을 데이터 버퍼의 포인터
							1, // 데이터 입력 버퍼의 개수
							&sendBytes, // Send 바이트 수...?
							0, // ??????????????
							&ptr->overlapped, // overlapped구조체의 포인터
							NULL // IOCP에서는 사용하지 않으므로 NULL, nullptr넘겨도 무방
						);

						if (retVal == SOCKET_ERROR)
						{
							if (WSAGetLastError() != WSA_IO_PENDING)
							{
								err_display((char *)"WSASend()");
							}
							continue;
						}
					}
					else if (failReason){

						std::cout << "로그인에 실패했습니다.  해당 사유는 : " << failReason << std::endl;

						ptr->dataBuffer = new FailLoginStruct(failReason);
						ptr->bufferProtocol = FAIL_LOGIN;
						ptr->isRecvTrue = false;

						// 데이터 보내기
						ZeroMemory(&ptr->overlapped, sizeof(ptr->overlapped));

						int buffer = FAIL_LOGIN;
						memcpy(ptr->buf, (char*)&buffer, sizeof(buffer));
						ptr->dataSize = sizeof(buffer);

						ptr->wsabuf.buf = ptr->buf; // ptr->buf;
						ptr->wsabuf.len = ptr->dataSize;

						DWORD sendBytes;
						retVal = WSASend(
							ptr->sock, // 클라이언트 소켓
							&ptr->wsabuf, // 읽을 데이터 버퍼의 포인터
							1, // 데이터 입력 버퍼의 개수
							&sendBytes, // Send 바이트 수...?
							0, // ??????????????
							&ptr->overlapped, // overlapped구조체의 포인터
							NULL // IOCP에서는 사용하지 않으므로 NULL, nullptr넘겨도 무방
						);

						if (retVal == SOCKET_ERROR)
						{
							if (WSAGetLastError() != WSA_IO_PENDING)
							{
								err_display((char *)"WSASend()");
							}
							continue;
						}
					}
				}
				else if (demandLogin.type == 2) {
					for (auto &i : userData)
					{
						if (i.GetID().compare(demandLogin.ID)) {
							failReason = 4;
							break;
						}
					}

					if (!failReason) {

						std::cout << "회원가입 및 로그인에 성공했습니다. " << std::endl;

						userData.emplace_back(demandLogin.ID, demandLogin.PW);

						ptr->dataBuffer = new PermitLoginStruct(0, 0, 0);
						ptr->bufferProtocol = PERMIT_LOGIN;
						ptr->isRecvTrue = false;

						// 데이터 보내기
						ZeroMemory(&ptr->overlapped, sizeof(ptr->overlapped));

						int buffer = PERMIT_LOGIN;
						memcpy(ptr->buf, (char*)&buffer, sizeof(buffer));
						ptr->dataSize = sizeof(buffer);

						ptr->wsabuf.buf = ptr->buf; // ptr->buf;
						ptr->wsabuf.len = ptr->dataSize;

						DWORD sendBytes;
						retVal = WSASend(
							ptr->sock, // 클라이언트 소켓
							&ptr->wsabuf, // 읽을 데이터 버퍼의 포인터
							1, // 데이터 입력 버퍼의 개수
							&sendBytes, // Send 바이트 수...?
							0, // ??????????????
							&ptr->overlapped, // overlapped구조체의 포인터
							NULL // IOCP에서는 사용하지 않으므로 NULL, nullptr넘겨도 무방
						);

						if (retVal == SOCKET_ERROR)
						{
							if (WSAGetLastError() != WSA_IO_PENDING)
							{
								err_display((char *)"WSASend()");
							}
							continue;
						}
					}
					else if (failReason) {

						std::cout << "회원가입에 실패했습니다.  해당 사유는 : " << failReason << std::endl;

						ptr->dataBuffer = new FailLoginStruct(failReason);
						ptr->bufferProtocol = FAIL_LOGIN;
						ptr->isRecvTrue = false;

						// 데이터 보내기
						ZeroMemory(&ptr->overlapped, sizeof(ptr->overlapped));

						int buffer = FAIL_LOGIN;
						memcpy(ptr->buf, (char*)&buffer, sizeof(buffer));
						ptr->dataSize = sizeof(buffer);

						ptr->wsabuf.buf = ptr->buf; // ptr->buf;
						ptr->wsabuf.len = ptr->dataSize;

						DWORD sendBytes;
						retVal = WSASend(
							ptr->sock, // 클라이언트 소켓
							&ptr->wsabuf, // 읽을 데이터 버퍼의 포인터
							1, // 데이터 입력 버퍼의 개수
							&sendBytes, // Send 바이트 수...?
							0, // ??????????????
							&ptr->overlapped, // overlapped구조체의 포인터
							NULL // IOCP에서는 사용하지 않으므로 NULL, nullptr넘겨도 무방
						);

						if (retVal == SOCKET_ERROR)
						{
							if (WSAGetLastError() != WSA_IO_PENDING)
							{
								err_display((char *)"WSASend()");
							}
							continue;
						}
					}
				}
			}
		}
		else if (!(ptr->isRecvTrue))
		{
			if (ptr->bufferProtocol == PERMIT_LOGIN)
			{
				ptr->bufferProtocol = 0;
				ptr->isRecvTrue = 0;

				ZeroMemory(&ptr->overlapped, sizeof(ptr->overlapped));
				memcpy(ptr->buf, (char*)&(ptr->dataBuffer), sizeof(PermitLoginStruct));
				ptr->dataSize = sizeof(PermitLoginStruct);

				delete (ptr->dataBuffer);

				ptr->wsabuf.buf = ptr->buf; // ptr->buf;
				ptr->wsabuf.len = ptr->dataSize;

				DWORD sendBytes;
				retVal = WSASend(ptr->sock,	&ptr->wsabuf, 1, &sendBytes, 0, &ptr->overlapped, NULL);
				
				if (retVal == SOCKET_ERROR)
				{
					if (WSAGetLastError() != WSA_IO_PENDING)
					{
						err_display((char *)"WSASend()");
					}
					continue;
				}
			}
			else if (ptr->bufferProtocol == FAIL_LOGIN)
			{
				ptr->bufferProtocol = 0;
				ptr->isRecvTrue = 0;

				ZeroMemory(&ptr->overlapped, sizeof(ptr->overlapped));
				memcpy(ptr->buf, (char*)&(ptr->dataBuffer), sizeof(FailLoginStruct));
				ptr->dataSize = sizeof(FailLoginStruct);

				delete (ptr->dataBuffer);

				ptr->wsabuf.buf = ptr->buf; // ptr->buf;
				ptr->wsabuf.len = ptr->dataSize;

				DWORD sendBytes;
				retVal = WSASend(ptr->sock, &ptr->wsabuf, 1, &sendBytes, 0, &ptr->overlapped, NULL);

				if (retVal == SOCKET_ERROR)
				{
					if (WSAGetLastError() != WSA_IO_PENDING)
					{
						err_display((char *)"WSASend()");
					}
					continue;
				}
			}


		}

		// 데이터 전송량 갱신
		/*
		if (ptr->recvBytes == 0)
		{
		std::cout << "DEBUG - B" << std::endl;

		//cbTransferred는 받은 데이터의 크기를 뜻함!! --> 한 글자(영어, 숫자, 공백)에 1byte, (한글)2byte
		ptr->recvBytes = cbTransferred;
		ptr->sendBytes = 0;


		// 받은 데이터 출력
		ptr->buf[ptr->recvBytes] = '\0';

		//printf(" [TCP %s :%d] %s\n", inet_ntoa(clientAddr.sin_addr), ntohs(clientAddr.sin_port), ptr->buf);

		std::cout << "[Debug] : 전송된 Size : " << cbTransferred << "  내용 :  " << (int&)(ptr->buf) << std::endl;

		}
		else
		{
		std::cout << "DEBUG - C" << std::endl;

		ptr->sendBytes += cbTransferred;

		}

		if (ptr->recvBytes > ptr->sendBytes)
		{
		std::cout << "DEBUG - D" << std::endl;

		// 데이터 보내기
		ZeroMemory(&ptr->overlapped, sizeof(ptr->overlapped));
		ptr->wsabuf.buf = ptr->buf + ptr->sendBytes;
		ptr->wsabuf.len = ptr->recvBytes - ptr->sendBytes;

		DWORD sendBytes;
		retVal = WSASend(
		ptr->sock, // 클라이언트 소켓
		&ptr->wsabuf, // 읽을 데이터 버퍼의 포인터
		1, // 데이터 입력 버퍼의 개수
		&sendBytes, // Send 바이트 수...?
		0, // ??????????????
		&ptr->overlapped, // overlapped구조체의 포인터
		NULL // IOCP에서는 사용하지 않으므로 NULL, nullptr넘겨도 무방
		);

		if (retVal == SOCKET_ERROR)
		{
		if (WSAGetLastError() != WSA_IO_PENDING)
		{
		err_display((char *)"WSASend()");
		}
		continue;
		}


		}
		else {
		ptr->recvBytes = 0;

		//데이터 받기
		ZeroMemory(&ptr->overlapped, sizeof(ptr->overlapped));
		ptr->wsabuf.buf = ptr->buf;
		ptr->wsabuf.len = BUF_SIZE;

		DWORD recvBytes;
		DWORD flags{0};
		retVal = WSARecv(
		ptr->sock, // 클라이언트 소켓
		&ptr->wsabuf, // 읽을 데이터 버퍼의 포인터
		1, // 데이터 입력 버퍼의 개수
		&recvBytes, // recv 결과 읽은 바이트 수, IOCP에서는 비동기 방식으로 사용하지 않으므로 nullPtr를 넘겨도 무방
		&flags,  // recv에 사용될 플래그
		&ptr->overlapped, // overlapped구조체의 포인터
		NULL // IOCP에서는 사용하지 않으므로 NULL, nullptr넘겨도 무방
		);

		if (retVal == SOCKET_ERROR)
		{
		if (WSAGetLastError() != WSA_IO_PENDING)
		{
		err_display((char *)"WSARecv()");
		}
		continue;
		}

		std::cout << "DEBUG - E" << std::endl;

		}
		*/
	}
};
