# Unity-Package-Base

Unity 프로젝트를 위한 네트워크 유틸리티 패키지입니다.

## 요구사항

- Unity 2021.3 이상
- .NET Standard 2.1

## 개요

이 패키지는 Unity 프로젝트에서 자주 사용되는 네트워크 관련 기능들을 제공합니다. 인터넷 연결 확인, IP 주소 관리, 네트워크 진단 등의 기능을 포함합니다.

## 주요 기능

### 네트워크 상태 확인

- 인터넷 연결 상태 확인 (동기/비동기)
- 로컬 네트워크 연결 상태 확인
- 네트워크 상태 변경 이벤트 처리

### IP 주소 관리

- 로컬 IP 주소 조회
- 공인 IP 주소 조회 (동기/비동기)
- 네트워크 어댑터별 IP 주소 조회
- 전체 네트워크 IP 목록 조회

### 네트워크 진단

- Ping 테스트 (동기/비동기)
- 네트워크 타임아웃 설정
- 상세한 오류 로깅

### HTTP 클라이언트

- 다양한 HTTP 메서드 지원 (GET, POST, PUT, DELETE, PATCH, HEAD)
- 자동 재시도 메커니즘
- 동시 요청 제한
- 요청 메트릭스 추적
- 타임아웃 및 취소 기능
- 동기/비동기 작업 지원

## 설치 방법

### UPM을 통한 설치 (Git URL 사용)

#### 선행 조건

- Git 클라이언트(최소 버전 2.14.0)가 설치되어 있어야 합니다.
- Windows 사용자의 경우 `PATH` 시스템 환경 변수에 Git 실행 파일 경로가 추가되어 있어야 합니다.

#### 설치 방법 1: Package Manager UI 사용

1. Unity 에디터에서 Window > Package Manager를 엽니다.
2. 좌측 상단의 + 버튼을 클릭하고 "Add package from git URL"을 선택합니다.

   ![Package Manager Add Git URL](Document/upm-ui-giturl.png)
3. 다음 URL을 입력합니다:

  ```text
  https://github.com/creator-hian/Unity-Network-Utilities.git
  ```

4. 'Add' 버튼을 클릭합니다.

   ![Package Manager Add Button](Document/upm-ui-giturl-add.png)

#### 설치 방법 2: manifest.json 직접 수정

1. Unity 프로젝트의 `Packages/manifest.json` 파일을 열어 다음과 같이 dependencies 블록에 패키지를 추가하세요:

```json
{
  "dependencies": {
    "com.creator-hian.unity.network-utilities": "https://github.com/creator-hian/Unity-Network-Utilities.git",
    ...
  }
}
```

#### 특정 버전 설치

특정 버전을 설치하려면 URL 끝에 #{version} 을 추가하세요:

```json
{
  "dependencies": {
    "com.creator-hian.unity.network-utilities": "https://github.com/creator-hian/Unity-Network-Utilities.git#0.0.1",
    ...
  }
}
```

#### 참조 문서

- [Unity 공식 매뉴얼 - Git URL을 통한 패키지 설치](https://docs.unity3d.com/kr/2023.2/Manual/upm-ui-giturl.html)

## 문서

각 기능에 대한 자세한 설명은 해당 기능의 README를 참조하세요:

## 원작성자

- [Hian](https://github.com/creator-hian)

## 기여자

## 라이센스

[라이센스 정보 추가 필요]
