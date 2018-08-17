#include "TestClient.h"
#include "CommunicationProtocol.h"


struct TestStruct {
	int Type;
	DemandLoginStruct demand;
};

int main(int argc, char *argv[])
{
	int retval;

	// 윈속 초기화
	WSADATA wsa;
	if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
		return 1;

	// socket()
	SOCKET sock = socket(AF_INET, SOCK_STREAM, 0);
	if (sock == INVALID_SOCKET) err_quit((char *)"socket()");

	// connect()
	SOCKADDR_IN serveraddr;
	ZeroMemory(&serveraddr, sizeof(serveraddr));
	serveraddr.sin_family = AF_INET;
	serveraddr.sin_addr.s_addr = inet_addr(SERVERIP);
	serveraddr.sin_port = htons(SERVERPORT);
	retval = connect(sock, (SOCKADDR *)&serveraddr, sizeof(serveraddr));
	if (retval == SOCKET_ERROR) err_quit((char *)"connect()");

	// 데이터 통신에 사용할 변수
	char buf[BUFSIZE + 1];
	int len;

	int recvType{};
	int sendType{};

	// 서버와 데이터 통신
	while (7) {
		// 데이터 입력
		printf("\n[안녕!] 보낼 프로토콜 넘버를 입력하세요  0 = 종료 , 100 = 로그인 및 회원가입 :  ");
		rewind(stdin);
		std::cin >> sendType;

		if (!sendType)
			break;
		// '\n' 문자 제거
		//len = strlen(buf);
		//if (buf[len - 1] == '\n')
		//	buf[len - 1] = '\0';
		//if (strlen(buf) == 0)
		//	break;
		if (sendType == DEMAND_LOGIN) {
			DemandLoginStruct demandLogin;
			std::cout << "ID입력 후, PW 입력하며, 이 후, Type ( 1 로그인 2 회원가입) 입력합니다." << std::endl;
			std::cin >> demandLogin.ID;
			std::cin >> demandLogin.PW;
			std::cin >> demandLogin.type;

			TestStruct testStruct;
			testStruct.Type = sendType;
			testStruct.demand = demandLogin;

			retval = send(sock, (char*)&testStruct, sizeof(testStruct), 0);
			printf("[TCP 클라이언트] %d바이트를 보냈습니다.\n", retval);

			if (retval == SOCKET_ERROR)
			{
				err_display((char *)"send()");
				break;
			}
		}
		/*
						// 데이터 받기
			retval = recvn(sock, (char*)&recvType, sizeof(int), 0);
			if (retval == SOCKET_ERROR)
			{
				err_display((char *)"recv()");
				break;
			}
			else if (retval == 0) {

				break;
			}

			if (recvType == PERMIT_LOGIN)
			{
				PermitLoginStruct permitLogin;

				retval = recvn(sock, (char*)&permitLogin, sizeof(PermitLoginStruct), 0);

				std::cout << "로그인 or 회원가입 성공!  WinCount : " << permitLogin.winCount << "LoseCount : " << permitLogin.loseCount << " Money : " << permitLogin.money << std::endl;
			}
			else if (recvType == FAIL_LOGIN)
			{
				FailLoginStruct failLogin;

				retval = recvn(sock, (char*)&failLogin, sizeof(FailLoginStruct), 0);

				std::cout << "로그인 or 회원가입 실패!" << std::endl;
				std::cout << "사유는 : " << failLogin.type << std::endl;

			}
		}
		*/
	}

	// closesocket()
	closesocket(sock);

	// 윈속 종료
	WSACleanup();
	return 0;
}