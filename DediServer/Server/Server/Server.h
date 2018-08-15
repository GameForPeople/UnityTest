#pragma once

#pragma comment(lib, "ws2_32")
#pragma comment(lib, "wininet.lib")

#include <WinSock2.h>
#include <iostream>
#include <cstdlib>

#include <fstream>

#include <vector>
#include <string>

// For ExternalIP
#include "wininet.h"
#include <tchar.h>

using namespace std;

#define SERVER_PORT 9000
#define BUF_SIZE 512

// For ExternalIP
#define EXTERNALIP_FINDER_URL "http://checkip.dyndns.org/"
#define TITLE_PARSER "<body>Current IP Address: "

// For Login
#define MAX_ID_LEN 12

class CUserData {
	//basic Data
	std::string m_id{};
	int m_pw{};
	int m_winCount{};
	int m_loseCount{};
	int m_money{};

	//Game use Data
	bool m_isLogin{ false };
	IN_ADDR m_userAddr{};

public:
	__inline CUserData() {};
	__inline CUserData(std::string InID, const int InPW) : m_id(InID), m_pw(InPW), m_winCount(0), m_loseCount(0), m_money(0)
	{ };
	__inline CUserData(const std::string InID, const int InPW, const int InWinCount, const int InloseCount, const int InMoney)
		: m_id(InID), m_pw(InPW), m_winCount(InWinCount), m_loseCount(InloseCount) , m_money(InMoney)
	{ };

	~CUserData() {};

public:
	__inline void	SetIPAddress(IN_ADDR& inputAddress) { m_userAddr = inputAddress; m_isLogin = true; }

	__inline void	PrintUserData() 
	{ 
		std::cout << m_id << "  " << m_pw << "  " << m_winCount << "  " << m_loseCount << std::endl;
	}
	
	__inline string	GetID() { return m_id; }
	__inline int	GetPW() { return m_pw; }
	__inline int	GetWinCount() { return m_winCount; }
	__inline int	GetLoseCount() { return m_loseCount; }
	__inline int	GetMoney() { return m_money; }

	__inline void	SetWinOrLose(int value) {
		if (value == 1) { m_winCount++; }
		else if (value == 2) { m_loseCount++; }
		return;
	}

	__inline bool	GetIsLogin() { return m_isLogin; }
	__inline void	SetIsLogin(bool bValue) { m_isLogin = bValue; }
};

struct SOCKETINFO
{
	OVERLAPPED overlapped;	// OVERLAPPED 구조체
	SOCKET sock;
	char buf[BUF_SIZE + 1];
	int recvBytes{};
	int sendBytes{};
	WSABUF wsabuf;
};

void err_quit(char *msg) 
{
	LPVOID lpMsgBuf;
	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
		NULL,
		WSAGetLastError(),
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&lpMsgBuf,
		0,
		NULL
	);

	MessageBox(NULL, (LPTSTR)lpMsgBuf, msg, MB_ICONERROR);
	LocalFree(lpMsgBuf);
	exit(1);
};

void err_display(char *msg) 
{
	LPVOID lpMsgBuf;
	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
		NULL,
		WSAGetLastError(),
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&lpMsgBuf,
		0,
		NULL
	);
	printf(" [%s]  %S", msg, (char *)lpMsgBuf);
	LocalFree(lpMsgBuf);
};

/*
IOCompletionQueue
FIFO Queue.입출력이 완료된 작업들이 들어간다. thread들은 이 queue에서 작업을 꺼내서 수행한다.

WaitingThreadQueue
LIFO Queue.(왜 이름이 queue지) 작업 대기중인 thread들이 들어있다. 만약 IO가 완료되었다면 이 queue에서 thread를 하나 꺼내서 사용한다.

ReleaseThreadList
현재 작업을 수행하고 있는 thread들이 들어간다.
만약 실행중인 thread가 정지해야 한다면 해당 thread를 PauseThreadList로 보내고 WaitingThreadQueue에서 새로운 thread를 하나 꺼내와서 사용한다. 이 때문에 프로세서의 수보다 많은 thread를 미리 만들어 놓는 것.

PauseThreadList
어떤 원인(임계 구역 등)으로 인해 일시정지된 thread들이 들어간다.
만약 일시정지가 풀리더라도 ReleaseThreadList가 꽉 차있다면 바로 ReleaseThreadList로 보내지 않고 대기한다.
*/

DWORD WINAPI WorkerThread(LPVOID arg)
{
	int retVal{};
	HANDLE hcp = (HANDLE)arg;

	int recvType{};
	int sendType{};

	while (7)
	{
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

		// 할당받은 소켓 즉! 클라이언트 정보 얻기
		SOCKADDR_IN clientAddr;
		int addrLength = sizeof(clientAddr);
		getpeername(ptr->sock, (SOCKADDR *)&clientAddr, &addrLength);

		//비동기 입출력 결과 확인 // 아무것도 안보낼 때는, 해당 클라이언트 접속에 문제가 생긴것으로 판단, 닫아버리겠다!
		if (retVal == 0 || cbTransferred == 0)
		{
			std::cout << "DEBUG - A" << std::endl;

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

		// 데이터 전송량 갱신
		if (ptr->recvBytes == 0)
		{
			std::cout << "DEBUG - B" << std::endl;

			//cbTransferred는 받은 데이터의 크기를 뜻함!! --> 한 글자(영어, 숫자, 공백)에 1byte, (한글)2byte
			ptr->recvBytes = cbTransferred;
			ptr->sendBytes = 0;


			// 받은 데이터 출력
			ptr->buf[ptr->recvBytes] = '\0';
			//printf(" [TCP %s :%d] %s\n", inet_ntoa(clientAddr.sin_addr), ntohs(clientAddr.sin_port), ptr->buf);
			std::cout << "[Debug] : 전송된 Size : "<< cbTransferred << "  내용 :  " << ptr->buf << std::endl;
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
	}
};
 
int GetExternalIP(char *ip)
{
	HINTERNET hInternet, hFile;
	DWORD rSize;
	char buffer[256] = { 0 };

	hInternet = InternetOpen(NULL, INTERNET_OPEN_TYPE_PRECONFIG, NULL, NULL, 0);

	if (NULL == hInternet)
		return 0;

	hFile = InternetOpenUrl(hInternet, EXTERNALIP_FINDER_URL, NULL, 0, INTERNET_FLAG_RELOAD, 0);

	if (hFile)
	{
		InternetReadFile(hFile, &buffer, sizeof(buffer), &rSize);
		buffer[rSize] = '\0';

		int nShift = _tcslen(TITLE_PARSER);
		std::string strHTML = buffer;
		std::string::size_type nIdx = strHTML.find(TITLE_PARSER);
		strHTML.erase(strHTML.begin(), strHTML.begin() + nIdx + nShift);
		nIdx = strHTML.find("</body>");
		strHTML.erase(strHTML.begin() + nIdx, strHTML.end());

		_tcscpy(ip, strHTML.c_str());
		InternetCloseHandle(hFile);
		InternetCloseHandle(hInternet);

		return _tcslen(ip);
	}

	return 0;
}


