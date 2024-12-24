# Changelog

All notable changes to this project will be documented in this file.

## 버전 관리 정책

이 프로젝트는 Semantic Versioning을 따릅니다:

- **Major.Minor.Patch** 형식
  - **Major**: 호환성이 깨지는 변경
  - **Minor**: 하위 호환성 있는 기능 추가
  - **Patch**: 하위 호환성 있는 버그 수정
- **최신 버전이 상단에, 이전 버전이 하단에 기록됩니다.**

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

### Changed

### Fixed
