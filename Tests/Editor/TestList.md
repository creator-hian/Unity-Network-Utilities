# 테스트 목록

## HttpClientWrapperAnythingTests.cs

- 다양한 HTTP 메서드(/anything) 테스트 (DELETE, GET, PATCH, POST, PUT)
- 경로 파라미터 테스트

## HttpClientWrapperAuthenticationTests.cs

- Basic Auth, Bearer Token Auth, Digest Auth, Hidden Basic Auth 테스트
- 유효/잘못된 자격 증명 테스트

## HttpClientWrapperCookiesTests.cs

- 쿠키 조회, 설정, 삭제 테스트
- 쿠키 지속성 테스트

## HttpClientWrapperDynamicDataTests.cs

- Base64 디코딩, 랜덤 바이트 생성 테스트
- 지연 응답, 데이터 스트리밍 테스트
- UUID 생성 테스트

## HttpClientWrapperHttpMethodTests.cs

- HTTP 메서드(DELETE, GET, PATCH, POST, PUT) JSON 응답 테스트
- 요청 데이터 전송 및 응답 확인

## HttpClientWrapperImagesTests.cs

- 다양한 이미지 형식(JPEG, PNG, SVG, WebP) 응답 테스트
- Accept 헤더에 따른 이미지 형식 테스트
- 잘못된 Accept 헤더 처리 테스트

## HttpClientWrapperRedirectsTests.cs

- 절대/상대 경로 리다이렉트 테스트
- 지정된 URL로 리다이렉트 테스트
- 최대 리다이렉트 횟수 초과 테스트

## HttpClientWrapperRequestInspectionTests.cs

- 요청 헤더 검증 테스트
- 클라이언트/서버 오류 처리 테스트

## HttpClientWrapperResponseFormatsTests.cs

- Brotli, Deflate, GZip 압축 데이터 테스트
- HTML, JSON, XML 문서 테스트
- UTF-8 인코딩, robots.txt 규칙 테스트
- 접근 거부 테스트

## HttpClientWrapperResponseInspectionTests.cs

- Cache, ETag 테스트
- 응답 헤더 검사 테스트

## HttpClientWrapperStatusCodeTests.cs

- 다양한 상태 코드(100, 200, 300, 400, 500) 응답 테스트
- 성공/실패 상태 코드 처리 테스트
