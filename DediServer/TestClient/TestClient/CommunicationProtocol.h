#pragma once

#include "TestClient.h"

enum Protocol {
	DEMAND_LOGIN = 100
	, FAIL_LOGIN = 101
	, PERMIT_LOGIN = 102
};

// type 100일때, 서버에 바로 다음 날려주는 구조체
struct DemandLoginStruct {
	int type{};	// 1일때는 로그인, 2일때는 회원가입
	int PW{};
	std::string ID;
};

// type 101 Server -> Client 로그인 실패, 회원가입 실패
struct FailLoginStruct {
};

// type 102 Server -> Client 로그인 성공, Lobby정보, 계정정보 전달
struct PermitLoginStruct {
	int winCount{};
	int loseCount{};
	int money{};
};