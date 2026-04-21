<div align="center">

# 인천공항 납품 조선시대 AI 얼굴 합성 프로그램

**조선시대 직업 및 성향 테스트를 기반으로 사용자 얼굴을 조선시대 인물 스타일로 AI 합성하는 체험형 콘텐츠**

<br/>

<img src="https://img.shields.io/badge/Platform-Unity-black?style=for-the-badge" />
<img src="https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge" />
<img src="https://img.shields.io/badge/Infra-Redis-red?style=for-the-badge" />

</div>

---

## 프로젝트 소개

이 프로젝트는 공항 방문객이 질문에 답하고 사진을 촬영하면,  
응답 결과와 얼굴 이미지를 바탕으로 조선시대 인물 콘셉트의 AI 합성 결과물을 제공하는 체험형 프로그램입니다.

사용자의 입력, 촬영, AI 변환, 결과 출력까지의 흐름이 자연스럽게 이어지도록 구성하였으며,  
현장 환경에서 안정적으로 동작할 수 있도록 예외 처리와 재촬영 플로우를 함께 구현하였습니다.

---

## 주요 화면

<table>
  <tr>
    <th>메인 화면</th>
    <th>질문 선택 화면</th>
  </tr>
  <tr>
    <td align="center">
      프로그램 시작 및 사용자 안내를 위한 초기 화면
      <br/><br/>
      <img width="260" alt="메인 화면" src="https://github.com/user-attachments/assets/12905094-147c-49d2-8470-5cd5cb574718" />
    </td>
    <td align="center">
      사용자가 질문에 답하며 결과 생성에 반영될 정보를 선택하는 화면
      <br/><br/>
      <img width="260" alt="질문 선택 화면" src="https://github.com/user-attachments/assets/15be57db-8690-4a1b-8d40-440d97435300" />
    </td>
  </tr>
  <tr>
    <th>사진 촬영 화면</th>
    <th>AI 얼굴 합성 결과 화면</th>
  </tr>
  <tr>
    <td align="center">
      웹캠을 통해 사용자 얼굴을 촬영하며 얼굴 인식 결과에 따라 자동 촬영 또는 재촬영이 진행되는 화면
      <br/><br/>
      <img width="260" alt="사진 촬영 화면" src="https://github.com/user-attachments/assets/0ea06413-7b5c-4303-8b0a-e32c6de34e8a" />
    </td>
    <td align="center">
      선택한 응답과 촬영한 사진을 바탕으로 조선시대 인물 스타일의 AI 합성 결과를 생성하고 표시하는 화면
      <br/><br/>
      <img width="260" alt="AI 얼굴 합성 결과 화면" src="https://github.com/user-attachments/assets/25a45af9-0e2a-4bff-bd3b-8d2de47e751d" />
    </td>
  </tr>
  <tr>
    <th colspan="2">실제 운영 사진</th>
  </tr>
  <tr>
    <td colspan="2" align="center">
      <img width="260" alt="실제 운영 사진 1" src="https://github.com/user-attachments/assets/37cc1ad0-4027-44b3-be4f-8cbd43adc4f4" />
      <img width="260" alt="실제 운영 사진 2" src="https://github.com/user-attachments/assets/f19c4405-90fb-422c-99fe-001909b80df3" />
      <img width="260" alt="실제 운영 사진 3" src="https://github.com/user-attachments/assets/313b3624-149a-416d-bc30-8963c271e802" />
    </td>
  </tr>
</table>

---

## 담당 역할

### 유니티 엔진 기반 응용프로그램 개발
- 프로그램 초기화 및 무거운 작업 처리 구간에 비동기 로직 적용
- UI 더블클릭 등 사용자 입력 오류 방지를 위한 딜레이 활성화 토글 적용
- 구글 클라우드 스토리지와 연동하여 촬영 사진 업로드 기능 구현

### Redis 기반 Python 및 ComfyUI 연동
- 얼굴 인식 시 자동 촬영이 이루어지는 시스템 구축
- 얼굴 미인식 및 촬영 실패 상황에 대한 재촬영 플로우 구성
- AI 변환 완료 후 결과물이 자동으로 표시되도록 처리
- 예외 상황 발생 시 조건에 따른 오류 화면 출력

### WebRTC 프로토콜 기반 웹캠 화면 실시간 수신
- Python OpenCV에서 전달하는 웹캠 화면을 실시간으로 수신 및 출력

---

## 기술 스택

- Unity
- C#
- Redis
- WebRTC
- OpenCV
- Google Cloud Storage

---

## 주요 기능

- 질문 기반 성향 및 캐릭터 정보 선택
- 얼굴 인식 기반 자동 촬영
- 얼굴 미인식 및 촬영 실패 상황에 대한 재촬영 처리
- Redis를 통한 Python 및 ComfyUI 파이프라인 연동
- AI 변환 완료 후 결과 화면 자동 전환
- 실시간 웹캠 화면 수신 및 출력
- 전시 및 체험형 키오스크 환경 대응

---

## 구현 포인트

### 비동기 로직 기반 흐름 최적화
- 초기화 및 무거운 작업 구간에 비동기 로직을 적용하여 사용자 경험 저하를 최소화하였습니다.
- 사용자의 입력과 화면 전환 흐름이 끊기지 않도록 구성하여 체험형 콘텐츠에 적합한 사용성을 확보하였습니다.

### 얼굴 인식 기반 촬영 흐름 설계
- 얼굴 인식, 자동 촬영, 재촬영 플로우를 연결하여 현장 체험형 환경에 적합한 촬영 흐름을 구성하였습니다.
- 얼굴 미인식 및 촬영 실패 상황에서도 사용자가 자연스럽게 다음 단계로 이어질 수 있도록 예외 흐름을 설계하였습니다.

### Redis 기반 AI 처리 파이프라인 연동
- Unity, Python, ComfyUI 간 결과 생성 흐름을 Redis 기반으로 연결하여 AI 변환 과정을 자동화하였습니다.
- 촬영 이후 결과 생성과 출력까지의 파이프라인이 하나의 체험 흐름으로 이어지도록 구성하였습니다.

### 안정적인 현장 운영을 위한 예외 처리
- 예외 상황에 따라 오류 화면을 분기 처리하여 현장 환경에서도 안정적으로 운영할 수 있도록 구성하였습니다.
- 사용자 입력 오류 방지를 위한 딜레이 활성화 토글을 적용하여 의도치 않은 중복 입력이나 잘못된 조작을 줄였습니다.
