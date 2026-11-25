# Project knowledge — The Novent

Mỗi entry phải qua câu hỏi: session mới thiếu dòng này có ra quyết định tệ hơn không?
Thông tin riêng từng máy (đường dẫn Unity, tool cài riêng) không ghi ở đây — để trong auto memory của mỗi người.

## Bối cảnh làm việc — Phạm vi: cả team
- **Luật**: Đồ án tốt nghiệp của **nhóm** (1 GD, 4 dev, 1 artist). Không SDK, tracking, ads, IAP, remote config.
- **Vì sao**: User chốt 2026-11-23. Tab Event Tracking trong GDD bỏ qua.
- **Nguồn**: GDD v0.5 tab OVERVIEW § Project Scope.

## Commit — Phạm vi: cả team
- **Luật**: Không thêm `Co-Authored-By: Claude` hay bất kỳ dòng ghi công Claude nào vào commit/PR.
- **Vì sao**: User yêu cầu 2026-11-23. `.claude/settings.json` đặt `attribution.commit/pr = ""` để Claude Code tự tắt.

## Tài liệu thiết kế
- **Luật**: Tab chính của GDD là v0.5 (game 2.5D co-op, Driftlife, giờ giao thương). Các tab `v0.1 …` là concept cũ đã bỏ, không dùng.
- **Vì sao**: Hai concept có tên và số liệu khác hẳn nhau (jetpack/₵/quán ăn so với Oxy/Tally/cảng). Dùng nhầm là code sai từ gốc.
- **Nguồn**: GDD tab OVERVIEW § Tracking GDD (v0.3.0 "bỏ concept Space Salvage").
- Bản GDD đủ + `ky-thuat.md` nằm ở repo `huycv18/DATN-SP27`, `projects/game-moi/gdd` — chưa đọc.

## Bộ AI của project (2026-11-25) — Phạm vi: cả team
- **Luật**: `.claude/` được commit trong repo game: `CLAUDE.md`, `rules/` (nạp theo đường dẫn), `skills/novent-*` + `unity-mcp`,
  `agents/` (explorer, reviewer), `hooks/` (session_start, skill_router, guard), `memory/`. Clone repo là dùng được; cài đặt ở `.claude/README.md`.
- **Vì sao**: trước đó `.claude` là junction sang repo kit riêng nên teammate không có skill/memory. User chọn đưa vào repo game 2026-11-25.
- Session log theo người: `memory/log/<git user.name>.md` — tránh conflict khi 4 người cùng ghi.
- Bản sao của bộ này nằm ở `huyhao27/AI-Agent-Memory-Promax` nhánh `game/the-novent` (đồng bộ bằng `tools/sync-game.ps1`); **nguồn chính là repo game**.

## Kiến trúc code (2026-11-25) — Phạm vi: cả team
- **Luật**: Module + asmdef: `Novent.Core` ← `Novent.Gameplay` ← `Novent.UI` / `Novent.Net`; `Novent.Boot` (composition root)
  tham chiếu tất cả. Service tạo duy nhất ở `GameBootstrap` (`RuntimeInitializeOnLoadMethod BeforeSceneLoad`), lấy qua `Services.Get<T>()`.
  Chi tiết: `.claude/rules/architecture.md`, skill `novent-architecture`.
- **Đã có** (`Assets/_Project/Scripts/`): `Services`, `EventBus<T>`, `PoolService`, `SaveService` (JSON, có `version`), `SeededRandom`,
  `INetSession`/`OfflineSession`/`PlayerId`, `Panel`/`PanelService`/`PanelRegistry`/`UIRoot`. 17 EditMode test pass (2026-11-25).
- **Vì sao**: kiến trúc "lite" — một composition root, phụ thuộc một chiều, logic là C# thuần để test và để đổi netcode rẻ. User chọn 2026-11-25.
- **Đã cân nhắc và bỏ**: DI container (Zenject ngừng phát triển; VContainer/Reflex thêm độ khó cho team), service = ScriptableObject + asset
  danh sách (quên đăng ký → null, lỗi phổ biến nhất), locator dùng reflection (khó debug, rủi ro IL2CPP stripping), một file `GameEvents.cs`
  chung (conflict khi 4 người cùng sửa).
- Quy ước đã chốt: private field `_camelCase`, static `s_camelCase`; comment tiếng Anh; async dùng `Awaitable` (không trộn coroutine trong
  một class, không thêm UniTask); UI dùng uGUI + TextMeshPro; namespace = asmdef + thư mục con.

## Unity project (tạo 2026-11-25, commit `c092aaa`) — Phạm vi: cả team
- **Luật**: Unity **6000.3.15f1** cho cả team; URP 17.3 (template Universal 3D, `Assets/Settings/PC_*` + `Mobile_*`);
  Input System only (`activeInputHandler: 1`); Force Text. Project nằm ở **gốc repo**.
- **Vì sao**: user chốt version 2026-11-25; `.gitignore` Unity đặt ở gốc repo.
- Template đã xoá TutorialInfo/Readme; còn `Assets/Scenes/SampleScene.unity` + `InputSystem_Actions.inputactions` mặc định.
- MCP for Unity **v10.3.0** (`com.coplaydev.unity-mcp`, ghim tag). Mỗi lần mở Editor phải bấm **Start Server** trong
  `Window > MCP for Unity` (skill `novent-desktop`); Claude Code kết nối `UnityMCP` qua HTTP `http://127.0.0.1:8080/mcp`.

## Submodule asset — Phạm vi: cả team
- **Luật**: `huyhao27/the-novent-assets` gắn ở `Assets/NoventAssets` (track `main`). Asset sửa trong submodule → commit +
  push ở repo asset (kèm `.meta`) → commit "bump" gitlink ở repo game.
- **Vì sao**: user yêu cầu 2026-11-25. Repo asset có nhánh 2D/3D/anim/effect/sound (chưa có nội dung).
- **Nếu vi phạm**: thiếu `.meta` trong repo asset → mọi máy khác mất tham chiếu; quên bump → team không thấy asset mới.
- Cả hai repo có `.gitattributes` LFS (png/psd/wav/fbx/ttf…) + UnityYAMLMerge; `Docs/**` không qua LFS.

## Bẫy môi trường Windows
- **Bẫy**: PowerShell 5.1 `Get-Content`/`Set-Content` đọc file UTF-8 không BOM thành ANSI → vỡ dấu tiếng Việt
  (đã làm hỏng INDEX.md 2026-11-25). Sửa file text bằng Edit tool hoặc bash `sed`, không dùng PS.
- **Bẫy**: `Move-Item` thư mục `Library` giữa hai ổ hỏng giữa chừng vì đường dẫn dài — đừng chuyển Library, để Unity tự tạo lại.
- `unity projects verify <project> --json`: exit ≠ 0 kèm `success: true` thường chỉ là thông báo có bản CLI mới — đọc `data`.
- Hook của `.claude/settings.json` gọi `python` → `python3` → `py -3`; máy không có Python 3 thì hook không chạy (không báo lỗi).

## Chờ chốt (Claude đề xuất — team duyệt rồi chuyển thành entry chính thức)
| Quyết định | Đề xuất | Lý do ngắn |
|---|---|---|
| **Netcode** | Netcode for GameObjects **2.x** (Unity 6.3 chỉ hỗ trợ 2.x; 3.0 cần 6000.7+) + **Multiplayer Services Sessions API** (`com.unity.services.multiplayer`, thay gói Lobby/Relay riêng đã deprecated), listen-host client-server; test bằng **Multiplayer Play Mode 2.0** (tối đa 3 virtual player) | Co-op 1–4 online là MVP; phải chọn trước khi viết hệ thống có state chung. Seam `INetSession` đã sẵn |
| Camera | Cinemachine + sprite billboard (URP đã chốt) | Chưa cài Cinemachine |
| Dữ liệu | Import TSV/CSV từ Google Sheet → ScriptableObject (tool ở `Novent.Editor`) | GDD quy định Sheet là nguồn số duy nhất |
| Input | Viết lại `InputSystem_Actions` thành actions asset của game + rebind UI | Hỗ trợ gamepad + màn Đổi phím |
| Hội thoại / cốt truyện | Thoại đơn giản bằng ScriptableObject tự viết; Yarn Spinner nếu cần nhánh | Đừng viết dialogue engine từ đầu |
| Scene | `MainMenu`, `Station`, `Expedition` trong `Assets/_Project/Scenes/` | Mỗi scene chơi thẳng được nhờ `GameBootstrap` |
