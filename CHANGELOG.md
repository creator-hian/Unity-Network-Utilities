# Changelog

All notable changes to this project will be documented in this file.

## 버전 관리 정책

이 프로젝트는 Semantic Versioning을 따릅니다:

- **Major.Minor.Patch** 형식
  - **Major**: 호환성이 깨지는 변경
  - **Minor**: 하위 호환성 있는 기능 추가
  - **Patch**: 하위 호환성 있는 버그 수정

## [0.1.0] - 2024-12-05

### Added

- 네트워크 상태 확인 기능
  - IsInternetAvailable (동기/비동기)
  - IsNetworkAvailable
  - 네트워크 상태 변경 이벤트 시스템

- IP 주소 관리 기능
  - GetLocalIPAddress
  - GetPublicIP (동기/비동기)
  - GetAdapterIPAddress
  - GetAllNetworkIPs

- 네트워크 진단 기능
  - PingHost (동기/비동기)
  - 네트워크 타임아웃 설정

- HTTP 클라이언트 기능
  - HttpClientWrapper 구현
  - 다양한 HTTP 메서드 지원 (GET, POST, PUT, DELETE, PATCH, HEAD)
  - 재시도 메커니즘
  - 동시 요청 제한
  - 요청 메트릭스 추적
  - 타임아웃 관리
  - 취소 토큰 지원

### Tests Added

- 네트워크 상태 테스트
  - 인터넷 연결 상태 확인 테스트
  - 네트워크 상태 변경 이벤트 테스트
  - 타임아웃 설정 테스트

- IP 주소 테스트
  - 로컬 IP 주소 유효성 테스트
  - 공인 IP 주소 조회 테스트
  - 네트워크 어댑터 IP 주소 테스트

- Ping 테스트
  - 유효한 호스트 Ping 테스트
  - 잘못된 호스트 Ping 테스트
  - 비동기 Ping 테스트
  - 예외 처리 테스트

- HTTP 클라이언트 테스트
  - HTTP 메서드 테스트 (GET, POST, PUT, DELETE, PATCH, HEAD)
  - HTTP 상태 코드 응답 테스트
  - 요청 검사 테스트 (헤더, IP, User-Agent)
  - 인증 및 쿠키 테스트
  - 리다이렉션 테스트
  - 응답 형식 테스트
  - 동적 데이터 테스트
  - 이미지 응답 테스트

### Changed

### Fixed
