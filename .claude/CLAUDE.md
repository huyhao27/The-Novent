# The Novent — ngữ cảnh nạp sẵn mỗi session

Đồ án tốt nghiệp DATN SP27 của **nhóm** (1 GD, 4 dev, 1 artist): game Unity 2.5D (cảnh 3D, asset 2D,
top-down 45°), co-op online 1–4 người, host giữ thế giới. Trọng tâm: **code game chạy được với 2 người
và kịp mốc bảo vệ**. Không có SDK, tracking, ads, IAP — đừng đề xuất.

Trả lời bằng tiếng Việt. Code, tên file, comment trong code: tiếng Anh.

## Bắt đầu session
- Hook `SessionStart` đã chèn INDEX memory + session cuối của bạn. Mở file memory chỉ khi task khớp dòng của nó.
- Quyết định chung của team: `.claude/memory/knowledge.md` (🔴). Game/scope/mốc: `project-overview.md` (🔴).
- GDD ở `Docs/`: dùng tab v0.5; tab `v0.1 …` là concept đã bỏ, không lấy số liệu.

## Stack
Unity **6000.3.15f1** · URP 17.3 · Input System only · Force Text · Test Framework 1.6 · project ở gốc repo.
Netcode: _chưa chốt_ (xem `knowledge.md`). MCP for Unity (`UnityMCP`, HTTP `127.0.0.1:8080/mcp`) — mở Editor
xong phải bấm Start Server trong `Window > MCP for Unity`.

## Code ở đâu
| Đường dẫn | Là gì | Quy tắc |
|---|---|---|
| `Assets/_Project/Scripts/<Module>/` | Code game, namespace `Novent.<Module>`, 1 asmdef/module | Tự do — theo `.claude/rules/architecture.md` |
| `Assets/_Project/Tests/` | EditMode/PlayMode tests | Logic mới trong Core/Gameplay có test |
| `Assets/_Project/{Scenes,Prefabs,Data}/` | Scene, prefab, ScriptableObject | Sửa qua Unity/MCP, không sửa YAML |
| `Assets/NoventAssets/` | Submodule art/sound | Không đặt code; asset mới commit ở repo asset rồi bump |
| `Packages/`, `Assets/Plugins/`, pack Asset Store | Bên thứ ba | Không sửa — bọc/copy sang `_Project` |
| `ProjectSettings/`, `Packages/manifest.json` | Cấu hình cả team | Chỉ khi được yêu cầu, nói rõ đổi gì |
| `Docs/` | Tài liệu của team | Chỉ đọc, sửa khi được yêu cầu |

Rule theo đường dẫn tự nạp từ `.claude/rules/` (C#, kiến trúc, co-op, test, asset, memory).
Hook `guard.py` chặn sửa text `.unity/.prefab/.asset/.meta`, `Packages/`, `Library/`.

## Skill (hook `skill_router.py` tự gợi ý — mỗi skill một lần/session)
| Lúc | Skill |
|---|---|
| Đọc/hiểu project, tìm cái đã có | `novent-context` · agent `novent-explorer` cho tìm kiếm rộng |
| Trước khi code | `novent-clarify` → `novent-plan` (≥3 file / hệ thống mới) · `novent-architecture` (module, service, UI, save) |
| Khi code | `novent-implement` + `novent-verify` · `novent-test` · `novent-code-style` khi review/đặt tên |
| Lỗi | `novent-debug` |
| Yêu cầu có hại | `novent-challenge` |
| Unity Editor | `unity-mcp` · GUI/web: `novent-desktop` |
| Memory / cuối ngày | `novent-memory` · `novent-dream` |
| Trước khi mở PR | agent `novent-reviewer` |

Lượt nào sẽ sửa `.cs`, prefab, scene hay ScriptableObject thì nạp `novent-verify` ngay đầu lượt.

## Làm việc nhóm
- Mỗi việc một nhánh `feat/<việc>` hoặc `fix/<việc>` từ `main`, PR để merge. Không commit thẳng `main`.
- Trước khi sửa: `git pull`; file/scene của người khác đang làm (xem `project-overview.md` § phân công,
  `git log`) thì hỏi trước.
- Commit nhỏ, message nói gì + vì sao. Không thêm dòng ghi công Claude (settings đã tắt).

## Verify
Có MCP: `mcp__UnityMCP__refresh_unity` → `mcp__UnityMCP__read_console` (errors) → `mcp__UnityMCP__run_tests` khi có test. Không có: batchmode qua
`.claude/skills/unity-mcp/scripts/unity-batch.ps1`. Chưa chạy trong lượt này = chưa verify, nói rõ.

## Tiết kiệm token
- `/clear` khi chuyển sang việc không liên quan. Tìm kiếm rộng → agent `novent-explorer` (Haiku).
- Grep rồi đọc ±40 dòng; không đọc nguyên file lớn, không đọc `Library/ Temp/ Logs/ *.csproj`.
- Scene/prefab: hỏi qua MCP (`mcp__UnityMCP__find_gameobjects`, trang nhỏ), không mở YAML.

## Kết thúc session
Ghi 3–6 dòng vào `.claude/memory/log/<git user.name>.md` (đã làm / tiếp theo / vướng), commit cùng nhánh
đang làm. Quyết định cả team phải theo → `knowledge.md` qua PR.

## Compact instructions
Khi compact, giữ: nhánh hiện tại, file đang sửa, bước plan đang làm, lỗi compile/test chưa xử lý,
quyết định user vừa chốt. Bỏ: output tool đã đọc xong, nội dung file đã xem.

## Dòng cuối mỗi câu trả lời
Kết thúc bằng một dòng `Skills: <skill đã nạp bằng tool Skill và làm theo trong lượt này>` hoặc
`Skills: none` — `novent-dream` dùng nó để tìm skill bị bỏ sót.
