# 내 프롬프트 로그

> **제출물입니다.** 
> 수업과 과제에서 실제로 사용한 프롬프트를 **전문 그대로** 기록하세요.
> **실패한 시도를 지우지 마세요.** 실패 기록이 회고 점수의 근거가 됩니다.

- 이름: 이예경
- 학번: 2026103319

---

## 기록 형식

각 작업마다 아래 형식으로 남깁니다.

````markdown
## <장> — <무엇을 만들었나>

### 시도 1
```
(프롬프트 전문을 여기에 붙여넣기)
```
**결과**:
**문제점**:
**원인 분석**:

### 시도 2
```
(수정한 프롬프트 전문)
```
**무엇을 바꿨나**:
**결과**:
**배운 점**:
````

---

## 1장

### 1-1. CLAUDE.md 작성

수업 중 강사가 제공한 초안을 바탕으로 함께 작성함. 별도 AI 프롬프트 없음.

**결과**: 네이밍 규약(`_camelCase`, `PascalCase`), `Update()` 성능 규칙, 디렉터리 구조, MCP 사용 범위, "한 번에 파일 1~2개" 규칙이 CLAUDE.md에 정의됨. 이후 모든 AI 작업의 기준이 됨.

---

### 1-2. 플레이어 이동

### 시도 1
```
1인칭 플레이어 컨트롤러를 만들어줘.

요구사항:
- Input System(신규) 사용, CharacterController 컴포넌트 기반
- WASD 이동, 마우스 시점 회전(상하 각도 -80~80도 제한), Space 점프
- 이동 속도 / 마우스 감도 / 점프 높이는 인스펙터에서 조절 가능
- 카메라는 자식 오브젝트로 두고 상하 회전만 담당
- 중력 직접 적용

파일: Assets/Scripts/Player/PlayerController.cs 하나만 생성.
씬은 건드리지 말고, 내가 직접 붙일 테니 필요한 세팅 절차만 알려줘.
```
**결과**: `PlayerController.cs` 155줄 생성. WASD 이동, 마우스 조준, 스페이스 점프 모두 동작. `_cameraPivot` 분리 구조로 상하/좌우 회전 분리됨.
**문제점**: 없음 (1회 성공)

---

### 1-3. 사격 시스템

### 시도 1
```
Raycast 기반 사격 시스템을 추가해줘.

요구사항:
- 좌클릭 발사, 발사 간격 0.15초, 사거리 50m
- 명중 지점에 디버그용으로 작은 구체를 0.5초간 표시
- 탄약 12발, R키 재장전(1.5초, 재장전 중 발사 불가)
- 카메라 정중앙에서 Ray를 쏠 것
- PlayerController는 수정하지 말고 별도 컴포넌트로

파일: Assets/Scripts/Player/WeaponSystem.cs
```
**결과**: `WeaponSystem.cs` 193줄 생성. Raycast 사격, 탄약 카운트, R키 재장전, 히트마커 구 표시 모두 동작.
**문제점**: 없음 (1회 성공)

---

### 1-4. 과제 — 대시 또는 조준

### 시도 1
```
Shift 키로 전방 대시 기능을 추가해줘.

요구사항:
- PlayerController는 수정하지 말고 별도 컴포넌트로
- 대시 거리 6m, 지속 시간 0.2초, 쿨다운 0.5초
- 수직 이동은 건드리지 마 (점프 중이면 착지할 때까지 그대로)
- 대시 중에는 추가 대시 불가
- 거리·시간·쿨다운은 인스펙터에서 조절 가능

파일: Assets/Scripts/Player/DashAbility.cs
```
**결과**: `DashAbility.cs` 123줄 생성. Sprint 액션 바인딩, 코루틴으로 프레임 단위 이동, 쿨다운 정상 동작.
**문제점**: 없음 (1회 성공)
**배운 점**: "PlayerController는 수정하지 마"를 명시하지 않으면 AI가 기존 파일에 대시 로직을 끼워 넣는다.

---

## 2장

### 2-1. 체력/데미지 설계 논의

### 시도 1
```
체력/데미지 시스템을 만들 건데, 먼저 설계부터 논의하자. 코드는 아직 쓰지 마.

플레이어와 적이 같은 방식으로 데미지를 받게 하고 싶어.
IDamageable 인터페이스와 Health 컴포넌트로 나누는 게 좋을까?
사망 처리, 무적 시간, 체력 변화 이벤트를 어디에 두는 게 좋을지
2~3가지 안을 장단점과 함께 제시해줘.
```
**고른 안**: IDamageable 인터페이스 + Health MonoBehaviour 분리 안.
**고른 이유**: `IDamageable`을 쓰면 WeaponSystem이 적인지 플레이어인지 모르고도 데미지를 줄 수 있다. 나중에 폭발물이나 함정을 추가해도 TakeDamage 한 줄로 연결된다. Health에 이벤트를 두면 UI·AI·사운드가 각자 구독하면 되고 Health가 직접 UI를 모른다.

---

### 2-2. 체력과 데미지 구현

### 시도 1
```
앞서 논의한 설계대로 구현해줘.

- IDamageable: TakeDamage(float) 메서드 하나
- Health: 최대 체력 SerializeField, HealthChanged(float current, float max) 이벤트,
  Died 이벤트, 무적 시간 SerializeField
- WeaponSystem에서 Raycast 명중 시 IDamageable.TakeDamage 호출할 것
  (지금은 적에게 Health가 없으니 플레이어에게만 붙이고 테스트)

파일 2개만: Core/Health.cs, Core/IDamageable.cs
WeaponSystem은 TakeDamage 호출 한 줄만 추가. 나머지는 건드리지 마.
```
**결과**: `Health.cs`, `IDamageable.cs`, `DestroyOnDeath.cs` 생성. WeaponSystem에 `GetComponentInParent<IDamageable>()` + `TakeDamage` 연결됨.
**문제점**: 요청하지 않은 `DestroyOnDeath.cs`가 함께 생성됨. 사망 시 오브젝트를 삭제하는 헬퍼 클래스인데, 나중에 풀링으로 교체할 때 충돌함.
**원인 분석**: "파일 2개만"이라고 명시했지만 AI가 "유용할 것 같은" 파일을 추가로 만들었다. 범위 제한을 더 강하게 써야 한다 — "다른 파일은 절대 생성하지 마".

---

### 2-3. 적 AI

### 시도 1
```
NavMeshAgent 기반 근접 적을 만들어줘.

- 플레이어를 추적. 추적 60m / 공격 2m
- 공격은 2초 간격, 데미지 10. 앞서 만든 IDamageable로 줄 것
- 공격할 때마다 콘솔에 로그를 남길 것.
  지금은 플레이어에게 Health가 없어서 공격이 일어나는지 로그로만 확인할 수 있다
- 체력은 앞서 만든 Health 컴포넌트를 쓴다. 체력을 따로 갖지 마
- Health의 사망 이벤트를 구독하고, OnDisable에서 반드시 구독 해제할 것
- 상태: Idle / Chase / Attack / Dead — enum + switch로 단순하게
- 플레이어는 [SerializeField] Transform 참조로 받되,
  프리팹은 씬 오브젝트를 참조할 수 없으니
  스폰한 쪽이 넣어줄 공개 메서드를 하나 둘 것
- 값은 전부 SerializeField

파일: Assets/Scripts/Enemy/EnemyAI.cs
NavMesh 베이크는 내가 할 테니 절차만 알려줘.
```
**결과**: `EnemyAI.cs` 170줄 생성. NavMesh 추적, 4상태 머신, Health 구독, 공격 로그 모두 정상 동작.
**문제점**: 없음 (1회 성공)
**배운 점**: "스폰한 쪽이 넣어줄 공개 메서드"를 명시하지 않으면 프리팹이 씬 Player를 참조할 수 없어서 적이 전부 제자리에 서 있다.

---

### 2-4. 웨이브 시스템

### 시도 1
```
ScriptableObject 기반 웨이브 시스템을 만들어줘.

- WaveData(ScriptableObject): 적 프리팹, 스폰 수, 스폰 간격.
  CreateAssetMenu를 붙여 에디터에서 만들 수 있게
- WaveSpawner: WaveData 리스트를 순서대로 진행,
  현재 웨이브의 적이 전멸하면 3초 후 다음 웨이브
- 스폰 위치는 SpawnPoints 오브젝트 하나를 [SerializeField]로 받아
  그 자식들을 Awake에서 모아 쓰고 그중 랜덤으로.
  Transform 배열을 인스펙터에 노출하지 마
- 플레이어 Transform을 받아서, 스폰한 적에게 넘겨줄 것
- 살아 있는 적 수는 스폰할 때 세고 각 적의 사망 이벤트로 줄일 것.
  매 프레임 FindObjectsOfType으로 세지 마
- 웨이브 시작 / 클리어 / 전체 클리어를 C# 이벤트로 노출

파일 2개만: Core/WaveData.cs, Core/WaveSpawner.cs
오브젝트 풀링은 아직 넣지 마. 다음 단계에서 할 거야.
```
**결과**: `WaveData.cs` + `WaveSpawner.cs` 생성, `Wave_1~3.asset` 에셋 생성. 웨이브 순서 진행, 클리어 감지, 이벤트 발화 모두 동작.
**문제점**: 없음 (1회 성공)
**배운 점**: "Transform 배열을 노출하지 마"를 명시하지 않으면 MCP가 씬 오브젝트 참조 배열을 채우지 못해서 스폰 위치가 None으로 남는다.

---

### 2-5. 오브젝트 풀링 리팩터링

### 시도 1
```
방금 만든 WaveSpawner에 오브젝트 풀링을 적용해줘.
Instantiate/Destroy를 풀 재사용으로 바꾸되,
WaveSpawner의 공개 API와 이벤트는 그대로 유지할 것.
변경 전후 차이를 요약해서 설명해줘.
```
**결과**: `WaveSpawner`에 `GetFromPool`/`ReturnToPool` 추가. `Health.Reinitialize()`, `EnemyAI.Reinitialize()` 추가. 적이 풀에서 꺼내져 위치·상태가 초기화된 뒤 활성화됨.
**밖에서 보면 아무것도 안 바뀌었는가**: **맞다.** `WaveStarted`, `WaveCleared`, `AllWavesCleared`, `EnemyKilled` 이벤트 시그니처 그대로, 웨이브 흐름 그대로. 플레이어 입장에서 차이 없음.
**배운 점**: 리팩터링의 기준은 "공개 계약이 그대로인가"다. AI에게 "공개 API와 이벤트는 그대로"를 명시하면 내부 구현만 바꾸게 강제할 수 있다.

### 2-6. 과제 — 원거리 적 또는 돌진 적

### 시도 1
```
기존 Enemy AI를 수정하지 않고 별도 클래스로 변형 된 Enemy AI 를 만들어야 해
Health / IDamageable 는 그대로 재사용 해야 함
WaveData 에 넣어서 실제로 웨이브에 기존 Enemy AI 와 섞여 나오게 해야 함

별도 클래스로 변형 된 Enemy AI 설명
평소엔 느리다가 일정 거리 안에 들어오면 2초 동안 빠르게 돌진하고 이후 5초간의 쿨다운이 있음
```
**결과**: ChargeEnemyAI : EnemyAI 상속 구조로 접근함
**문제점**: EnemyAI의 `_state`, `_agent`, `SetState()`, `Update()` 가 전부 `private` / 비-virtual → 하위 클래스에서 접근·override 불가. 돌진 상태를 끼워 넣으려면 EnemyAI 자체를 고쳐야 함
**원인 분석**: 상속은 부모의 내부를 열어 줄 때만 유효하다. EnemyAI는 처음부터 단일 책임으로 닫혀 있어서 상속 확장점이 없음. "기존 클래스를 수정하지 않는다"는 조건과 충돌

### 시도 2
```
기존 Enemy AI를 수정하지 않고 별도 클래스로 변형 된 Enemy AI 를 만들어야 해
Health / IDamageable 는 그대로 재사용 해야 함
WaveData 에 넣어서 실제로 웨이브에 기존 Enemy AI 와 섞여 나오게 해야 함

최소 2번 이상 시도해야하며 실패한 프롬프트도 my-prompts.md 에 남겨줘

별도 클래스로 변형 된 Enemy AI 설명
평소엔 느리다가 일정 거리 안에 들어오면 2초 동안 빠르게 돌진하고 이후 5초간의 쿨다운이 있음
```
**무엇을 바꿨나**: 상속을 버리고 `ChargeEnemyAI : MonoBehaviour` 완전 독립 클래스로 재설계. WaveData에 `List<GameObject> _enemyPrefabs` 추가해 웨이브마다 혼합 스폰 가능하게 함. WaveSpawner에 `TryReinitializeController()` 추가로 EnemyAI / ChargeEnemyAI 둘 다 처리
**결과**: 컴파일 에러 없이 작동. EnemyAI 코드 한 줄도 수정하지 않음. 기존 Wave_1~3 에셋의 `_enemyPrefab` 참조 그대로 유지되면서, Inspector에서 `_enemyPrefabs` 리스트에 ChargeEnemy 프리팹을 추가하면 혼합 스폰됨
**배운 점**: 닫힌 클래스를 확장할 때는 상속보다 독립 컴포넌트가 맞다. WaveSpawner가 구체 타입을 두 개 아는 건 이상적이지 않지만, 지금 규모에서는 인터페이스 도입보다 단순한 쪽이 낫다는 트레이드오프를 선택함

---

## 3장

### 3-1. 게임 상태 머신

### 시도 1
```
게임 상태 머신을 만들어줘.

GameManager: Ready → Playing → GameOver → Cleared — enum + switch로 단순하게
- 플레이어 Health의 사망 이벤트, WaveSpawner의 전체 클리어 이벤트를 구독
- 정지 시 커서 표시 + Time.timeScale = 0
- 정지 시 PlayerController와 WeaponSystem을 각각 [SerializeField]로 받아 enabled를 끌 것.
  timeScale = 0은 Update를 멈추지 않아서, 안 끄면 죽은 뒤에도 총이 나간다.
  배열이나 리스트 하나로 묶지 마
- R키로 씬 리로드 (timeScale을 1로 되돌리는 것을 잊지 말 것)
- Esc로 종료 (Application.Quit)
- 대기 시간은 코루틴 말고 Time.unscaledTime으로 잴 것
- 상태가 바뀌면 C# 이벤트로 알릴 것

파일: Core/GameManager.cs
```
**결과**: `GameManager.cs` 129줄 생성. 상태 전환, 커서 잠금/해제, R 재시작, Esc 종료 모두 동작.
**문제점**: "대기 시간은 Time.unscaledTime으로"라고 명시했는데 `WaitForSeconds` 코루틴을 씀. `WaitForSeconds`는 `Time.timeScale`의 영향을 받아서 `timeScale=0`인 상황에서 멈춘다.
**원인 분석**: Ready 대기는 게임 시작 직후라 `timeScale=0`이 되는 타이밍이 없어서 실제로 버그는 나지 않음. 하지만 AI가 제약을 무시한 경우다. "절대로 WaitForSeconds를 쓰지 마"처럼 더 강하게 써야 한다.

---

### 3-2. HUD

### 시도 1
```
HUD가 구독할 수 있도록 두 곳에 이벤트를 추가해줘.
WeaponSystem: 탄약이 변할 때 (현재, 최대)를 알리는 이벤트. 기존 Debug.Log는 지울 것.
WaveSpawner: 적이 죽을 때마다 누적 킬 수를 알리는 이벤트.
이미 각 적의 사망 이벤트를 구독하고 있으니 거기서 세면 된다.
기존 기능과 공개 이벤트 시그니처는 그대로 유지할 것.
파일 2개만 수정하고 다른 파일은 건드리지 마.
```
**결과**: `WeaponSystem`에 `AmmoChanged(int current, int max)` 이벤트, `WaveSpawner`에 `EnemyKilled(int killCount)` 이벤트 추가됨. 기존 Debug.Log 제거.
**문제점**: 없음

### 시도 2
```
HUD를 만들어줘. UI Toolkit 말고 uGUI로.
텍스트는 TextMeshPro 말고 레거시 UnityEngine.UI.Text를 쓸 것.
(TMP를 처음 쓰면 임포트 창이 떠서 그동안 MCP가 멈춘다)

- 좌하단 체력바, 우하단 탄약(현재/최대), 상단 중앙 웨이브 번호, 킬 수
- 화면 중앙에 게임오버/클리어 안내. 평소에는 숨길 것
- 체력바는 Image.fillAmount 말고 RectTransform 앵커 폭으로 줄일 것
  (fillAmount는 Source Image가 있어야 동작한다)
- 전부 이벤트 구독. Update에서 폴링하지 말 것
- 구독은 OnEnable, 구독 해제는 OnDisable
- 참조는 전부 [SerializeField]. Find 계열 금지

파일: UI/HUDController.cs
```
**결과**: `HUDController.cs` 생성. 체력바 앵커 방식, 탄약/웨이브/킬 텍스트, 엔드 패널 모두 이벤트 기반으로 동작.
**문제점**: 없음

### 시도 3
```
방금 알려준 UI 오브젝트들을 씬에 만들고 HUDController에 연결해줘.
MCP 도구로 직접 해줘.
끝나면 HUDController의 각 참조 칸에 실제로 값이 들어갔는지 다시 읽어서 보여줘.
```
**결과**: Canvas, 체력바 BG/Fill, 각 Text, EndPanel 씬에 생성됨. HUDController의 10개 SerializeField 참조 모두 연결됨.
**배운 점**: HUD를 만들 때 스크립트와 씬 작업을 한 번에 요청하면 연결이 빠진다. 순서대로 — 이벤트 추가 → 스크립트 → 씬 오브젝트 — 로 나눠야 한다.

---

### 3-3. 게임필

<!-- 3개를 모두 AI가 만들었다. 단, "1번부터 시작해, 2번은 내가 말하기 전엔 하지 마"로 순서를 직접 통제했다 -->

### 시도 1 (적 피격 플래시)
```
타격감을 넣어줘. 단, 한 번에 하나씩 하고 매번 내가 테스트할게.
1) 적 피격 시 머티리얼을 0.05초 흰색으로 플래시
2) 피격 시 카메라 셰이크 (0.1초, 강도는 인스펙터에서 조절)
3) 명중 지점에 파티클
1번부터 시작해. 2번은 내가 말하기 전에는 하지 마.
```
**결과**: `HitFlash.cs` 생성. MaterialPropertyBlock으로 흰색 → 원래 색 복구. Enemy, ChargeEnemy 프리팹에 추가됨.
**문제점**: 없음

### 시도 2 (카메라 셰이크)
```
push 하고 2번 카메라 셰이크로 넘어가줘.
```
**결과**: `CameraShake.cs` 생성. Health 이벤트 구독, 피격 시 X/Y 축 랜덤 오프셋, Z축 제외(전방 흔들림 방지).
**문제점**: 없음

### 시도 3 (명중 파티클)
```
git add -A git commit -m 'feat: 카메라 셰이크 구현' git push 후 3번 명중 지점 파티클 고고
```
**결과**: `HitParticle.prefab` 생성. 반구 모양 12개 파티클, 주황→흰색 페이드, WeaponSystem에 연결됨.
**문제점**: 없음
**배운 점**: "1번부터 시작해, 2번은 말하기 전에 하지 마" 한 줄이 핵심이다. 이 줄이 없으면 3개를 한꺼번에 만들어서 어느 단계에서 문제가 생겼는지 모른다.

### 3-4. AI 코드 리뷰

<!-- 아래 표를 채울 것. 틀린 지적이 최소 1개는 있어야 한다 -->

| # | AI의 지적 | 내 판정 | 근거 |
| - | --------- | ------- | ---- |
| 1 | HUDController: Start()에 null 검사 없어 인스펙터 미연결 시 NRE 발생 | 맞다 | HUDController.cs:30 — Start() 자체가 없었음, OnEnable에서 직접 구독 |
| 2 | EnemyAI/ChargeEnemyAI: Update()에서 Vector3.Distance 사용, sqrMagnitude로 교체해야 함 | 애매하다 | 사실이지만 아레나 1개·최대 20마리 규모에서 sqrt 20번은 프로파일러에 잡히지 않는 수준. Vector3.Distance가 의도를 더 명확히 표현함 |
| 3 | WeaponSystem: 발사·재장전·레이캐스트·파티클·히트마커를 한 클래스가 처리해 SRP 위반 | 애매하다 | 무기가 1종이므로 분리하면 클래스 간 참조 보일러플레이트만 늘어남. ~215줄로 가독성 문제 없음 |
| 4 | GameManager: Start에서 구독하고 OnDisable에서 해제하는 비대칭 패턴 | **틀렸다** | 원래 OnDisable에 null 가드(`if (_playerHealth != null)`)가 이미 있었음. GameManager는 씬 수명 내내 비활성화되지 않으므로 실질적 버그 경로가 존재하지 않음 |
| 5 | WaveData: EnemyPrefab 프로퍼티가 아무 데서도 쓰이지 않음 | 맞다 | WaveData.cs:15 — Grep 결과 호출 없음, 순수 데드 코드 |
| 6 | EnemyAI: PerformAttack()에 Debug.Log가 남아 있어 릴리즈 빌드에도 로그 발생 | 맞다 | EnemyAI.cs:161 — 공격마다 로그, 초당 최대 10개 |
| 7 | PlayerController: TryBindActions()에서 _cameraPivot null 체크 누락 | 맞다 | PlayerController.cs:125 — UpdateLook()에서 null 없이 직접 접근 |
| 8 | WaveData: GetRandomPrefab()이 null을 LogError 없이 반환 | 맞다 | WaveData.cs:19 — null 반환 → Instantiate(null) 크래시, 에러 위치 추적 어려움 |
| 9 | WaveSpawner: SpawnWave()에 SpawnCount == 0 가드 없음 | 맞다 | WaveSpawner.cs:112 — SpawnCount 0이면 _aliveCount가 0 그대로라 웨이브 클리어가 즉시 발동됨 |
| 10 | WeaponSystem: HitMarker를 발사마다 생성/파괴, 풀링 필요 | 애매하다 | fireInterval 0.15s → 초당 최대 7개. HitMarker는 디버그 보조 오브젝트로 추후 제거 가능성 높음, 지금 풀링은 과하다 |

### 3-5. 빌드

### 시도 1
```
Windows Standalone 빌드 설정을 점검해줘.
(Unity 6.3은 File > Build Settings가 아니라 File > Build Profiles다)

포함 씬, 회사명/제품명, 커서 잠금 동작을 확인하고
빌드 후 흔히 나는 문제(에디터에서만 되는 코드)가 있는지 스크립트를 훑어봐.

화면 설정은 이미 창 모드(1280x720)로 맞춰져 있다. 바꾸지 말고 그대로인지만 확인해줘.
```
**결과**: Player Settings 확인 — 창 모드 1280×720 ✅, Arena.unity 씬 포함 ✅, `UnityEditor` 계열 코드 없음 ✅. Company Name이 `DefaultCompany`로 미변경 상태 발견.

```
File > Build Profiles → Windows 프로필 선택 → Build
```
**결과**: `Builds/Windows/ArenaBreak.exe` 98.2 MB, 에러 0, 경고 7개(URP Volume Profile 미저장 경고 + MCP 타임아웃). 빌드 성공.
**문제점**: Company Name `DefaultCompany`, Product Name `arena-break-starter` 미변경 — 제출 전 수동으로 수정 필요.
**배운 점**: "바꾸지 말고 그대로인지만 확인해줘"처럼 조작 범위를 명시하면 AI가 설정을 임의로 변경하지 않는다.

---

## 자체 점검

제출 전에 확인하세요.

```
□ 프롬프트를 요약하지 않고 전문 그대로 붙여넣었는가
□ 실패한 시도가 최소 3개 이상 기록되어 있는가
□ 각 실패마다 "왜 실패했는지" 분석이 있는가
□ 프롬프트를 어떻게 고쳤는지 비교할 수 있는가
```

> 실습 동안 한 번도 실패하지 않는 것은 불가능합니다.
> 실패 기록도 중요합니다.
