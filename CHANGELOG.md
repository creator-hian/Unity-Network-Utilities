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

- 네트워크 캐시 시스템
  - NetworkCache 클래스 구현
    - LRU 캐시 정책
    - 스레드 안전 보장
    - 동기/비동기 API
    - 자동 만료 처리
  - 캐시 관리 기능
    - 배치 처리 지원
    - 타이머 기반 자동 정리
    - 수동 정리 옵션
  - 모니터링 및 통계
    - 캐시 상태 통계
    - 이벤트 기반 알림
  - 확장 가능한 구조
    - INetworkCache 인터페이스
    - DefaultNetworkCacheProvider 구현

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

- 네트워크 캐시 테스트
  - 기본 CRUD 작업 테스트
    - 데이터 저장 및 조회
    - 항목 제거
  - 캐시 만료 테스트
    - 시간 기반 만료
    - 자동 정리
  - 이벤트 발생 테스트
    - 추가/제거/만료 이벤트
  - LRU 정책 테스트
    - 최대 크기 초과 시 동작
    - 가장 오래된 항목 제거
  - 동시성 테스트
    - 멀티스레드 안전성
    - 경합 상황 처리

### Changed

### Fixed
