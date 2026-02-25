# Unity MultiThreaded A* Pathfinding (Burst & Job System)

유니티의 **Burst Compiler**와 **C# Job System**을 활용하여 성능을 극대화한 멀티스레드 기반 A* 길찾기 프로젝트입니다. 육각형(Hexagon) 3D 타일맵 시스템과 대규모 유닛 이동 및 렌더링 최적화를 구현했습니다.

---

## 주요 기능 (Key Features)

### 1. High-Performance Pathfinding
* **Burst-Compiled A***: 수천 개의 노드 연산을 네이티브 수준의 속도로 처리하여 다수의 유닛이 동시에 경로를 계산해도 프레임 드랍을 최소화합니다.
* **Dynamic Obstacle Avoidance**: 유닛 간의 충돌을 실시간으로 감지합니다. 
    * 길이 막히면 즉시 우회로를 탐색합니다.
    * 경로가 완전히 차단될 경우 그 자리에서 대기하거나 다음 경로를 탐색하는 지능적 로직을 포함합니다.
* **Train Movement**: 유닛들이 일렬로 줄을 지어 자연스럽게 이동하는 '기차놀이' 형태의 이동 로직을 구현했습니다.

### 2. Hexagonal 3D Tilemap System
* **Custom Hex-Grid**: 육각형 타일 기반의 좌표계 및 인접 타일 탐색 알고리즘을 구축했습니다.
* **Interactive Gameplay**:
    * **Spawn**: 빈 타일을 클릭하여 실시간으로 캐릭터 생성.
    * **Command**: 캐릭터 선택 후 목표 타일을 지정하여 이동 명령 수행.

---

## 최적화 상세 (Optimization)

### 3. Memory & Performance Optimization (Zero GC Alloc)
대규모 연산 시 발생하는 성능 병목을 방지하기 위해 런타임 중 메모리 할당을 극도로 제한했습니다.

* **Zero GC Allocation**: 모든 연산 과정에서 `GC Alloc`을 **0B**로 유지하여 가비지 컬렉션(GC Spike)으로 인한 프레임 드랍을 완전히 제거했습니다.
* **Native Containers**: `NativeArray`, `NativeList` 등 유니티의 네이티브 컨테이너를 활용하여 워커 스레드 간 데이터 전달 시 발생하는 가비지 생성을 차단했습니다.
* **Struct-Based Design**: 참조 타입(Class) 대신 값 타입(Struct) 중심의 설계를 통해 메모리 레이아웃을 최적화하고 CPU 캐시 히트율을 높였습니다.

### 4. Rendering Optimization (Batching)
수천 개의 타일이 존재하는 환경에서도 안정적인 프레임을 유지하기 위해 렌더링 최적화를 진행했습니다.

* **Texture Atlas & Material Unification**: 프로젝트에 사용된 모든 타일 텍스처를 하나로 통합하여, 머티리얼 교체로 인해 발생하는 불필요한 드로우 콜을 원천 차단했습니다.
* **Chunk-based Mesh Baking**: 타일을 일정 단위의 청크(Chunk)로 묶어 하나의 메시로 결합(Baking)함으로써 CPU가 GPU에 보내는 명령 횟수를 줄였습니다.
* **Optimization Result**: 
    * **Batches (Draw Calls)**: 700+ ➔ **70 (약 90% 절감)**
    * **GC Alloc per Frame**: **0B (Garbage Free)**
