# 🙇🏻‍♀️ 프로젝트 소개

| **Engine**     | Unity 3D |
| --- | --- |
| **Platform**  | PC |
| **Period**  | 25.03 (약 1달간) |
| Team | 1인 개발 |


3D FPS 게임

좀비들을 피해 보급품을 얻어 다음 스테이지까지 무사히 이동하는 게임

# 💁🏻‍♀️ 프로젝트 구현


## 🔵 TCP 패킷 통신을 활용하여 Local DB에 회원 정보 저장

### 🔹 사용 기술 및 라이브러리

- **MySQL**
- **TCP 소켓 통신**

- **MySQL Connector (C#)**
- **Newtonsoft.Json (Json.NET)**

### 🔹 시스템 구조

| 구성 요소 | 설명 |
| --- | --- |
| **DB** | MySQL 사용, 사용자 정보(local DB)에 저장 |
| **SERVER** | C# 서버 (Socket 통신) + MySQL Connector 이용, Json 포맷으로 데이터 처리 |
| **UNITY** | 로그인 시 Json 패킷 전송, 응답으로 받은 사용자 정보를 ScriptableObject로 저장 및 사용 |

### 🔹 MySQL로 유저 데이터 관리

- 게임 외부에서 유저 데이터 관리 가능
<img width="554" alt="image (8)" src="https://github.com/user-attachments/assets/ffd0aa55-ab59-448e-8b86-18e4d7d141a8" />


### 🔹 TCP 소켓 통신 서버 구현

- MySQL Connector 패키지로 DB와 연결
- 유니티 클라이언트로부터 받은 로그인 정보와 
mySQL 의 회원 정보를 쿼리문을 활용하여 비교 후 로그인 성공, 실패 여부 전송
- JSON 형식으로 데이터들을 파싱하여 처리

### 🔹 유니티 클라이언트 - 로그인, 회원가입

- 로그인 시도 등의 패킷을 Json 형태로 서버에 전송
- 서버로부터 받아온 정보들은 ScriptableObject 로 저장
    
    → 유저 정보를 게임 전역에서 사용 가능하도록 설계
    

## 🔵 Chat GPT API 활용
<img width="87" alt="image (9)" src="https://github.com/user-attachments/assets/8255aff1-9e9c-4e49-8e99-668ead8cb357" /> [OpenAI Platform](https://platform.openai.com/settings/organization/general)

### 🔹게임 관련 내용을 미리 학습 시켜 사용

### 🔹메인메뉴 씬에서 지피티를 활용한 자유로운 대화 가능
<img width="425" alt="image (10)" src="https://github.com/user-attachments/assets/586a5291-cc5e-4d17-a5f0-7c50575e2289" />

### 🔹로그인 한 유저의 정보를 활용하여 상황에 맞는 멘트를 메번 다르게 출력
<img width="292" alt="image (11)" src="https://github.com/user-attachments/assets/106314ad-1381-4c61-8788-9b28a7d34a09" />
<img width="295" alt="image (12)" src="https://github.com/user-attachments/assets/35df19a4-f6f1-4008-9e74-c1570c9c58cc" />

## 🔵 Occlusion Culling 옵션 사용

### 🔹 다른 오브젝트에 가려 카메라에 보이지 않는 오브젝트들이 많음

→ Occlusion Culling 기능을 사용하여 오브젝트의 렌더링 비활성화  ⇒ 드로우 콜 횟수를 줄여 성능 향상 

### 실제 게임화면

<img width="642" alt="image (13)" src="https://github.com/user-attachments/assets/d5227f62-5878-4e20-927c-0276129cb34f" />

### 적용된 게임 화면
<img width="547" alt="image (14)" src="https://github.com/user-attachments/assets/807b8925-2998-43d8-a53a-4c40164790dd" />


## 🔵 플레이어 시점 변경 및 애니메이션 적용

### 🔹 카메라의 위치 조정을 통해 시점 변경 구현

- 3인칭 시점
    
    <img width="265" alt="image (15)" src="https://github.com/user-attachments/assets/dc09f239-f12b-4dfb-9fee-af422274547d" />


- 1인칭 시점
  
    <img width="330" alt="image (16)" src="https://github.com/user-attachments/assets/814780bb-705a-4540-ad5c-0a317063154a" />

    
- 3인칭 시점에서는 F키를 통해 **카메라 모드 변경 가능**
    1. 플레이어 주위를 도는 모드
    2. 플레이어가 바라보는 방향을 카메라도 향하는 모드

### 🔹 플레이어 애니메이션 | Animation Layer

서로 다른 신체 부위의 애니메이션 재생 관리를 쉽게 하기 위해 애니메이션 레이어를 구분하여 제작 
<img width="701" alt="image (17)" src="https://github.com/user-attachments/assets/d3b17d62-3887-4875-a4f4-95cdfb18b841" />


### 🔹 여러 애니메이션을 부드럽게 연결하기 위해 Blend Tree 사용
<img width="353" alt="image (18)" src="https://github.com/user-attachments/assets/14dd569b-988c-4720-bd9a-18ec3093a360" />
<img width="236" alt="image (19)" src="https://github.com/user-attachments/assets/e8a452b0-ea9c-426f-804b-1d1c6f37d163" />


## 🔵 플레이어와의 거리에 따라서 행동 패턴이 변경되는 좀비 AI 구현

<br/>

# 🙆🏻‍♀️ 문제 발생 및 해결


### ⚠️ 유니티에서 서버로부터 데이터를 받아오는 함수 내에서 씬 전환을 시도하면 오류 발생
<img width="464" alt="image (20)" src="https://github.com/user-attachments/assets/adf95cca-7408-4c9a-a904-c5c44641d10a" />


### ❔ 발생 원인

네트워크 통신을 위한 소켓 관련 작업은 일반적으로 **백그라운드 쓰레드에서 실행**

⇒ 유니티의 함수들은 **메인 쓰레드에서 실행**되어야 함

### ✅ Queue를 사용하여 해결

Queue에 로그인 성공(씬 전환 요청) 저장한 후 ➡️ 유니티의 메인 스레드( Update() )에서 실행

# 🙋🏻‍♀️시연 영상
//추가 예정

