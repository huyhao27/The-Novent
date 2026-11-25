# Bộ AI của The Novent — cài đặt và cách dùng

Thư mục `.claude/` này là "chuẩn chung" cho cả nhóm khi code với Claude Code: luật code, skill,
agent, hook và bộ nhớ của project. Clone repo là có — chỉ cần cài vài thứ trên máy một lần.

## Có gì trong này

| Đường dẫn | Là gì | Nạp khi nào |
|---|---|---|
| `CLAUDE.md` | Ngữ cảnh project: stack, code để đâu, quy trình nhóm | Mọi session |
| `rules/*.md` | Luật theo vùng file: `csharp`, `architecture`, `coop`, `tests`, `unity-assets`, `memory`, `kit` | Chỉ khi Claude đọc/sửa file khớp `paths:` |
| `skills/novent-*` | Quy trình: context, clarify, plan, architecture, implement, test, verify, debug, code-style, challenge, memory, dream, desktop + `unity-mcp` | Khi việc cần (hook gợi ý) |
| `agents/` | `novent-explorer` (tìm kiếm, chạy Haiku cho rẻ), `novent-reviewer` (review diff trước PR) | Khi Claude giao việc |
| `hooks/` | `session_start.py` (chèn tóm tắt memory), `skill_router.py` (gợi ý skill theo câu bạn gõ), `guard.py` (chặn sửa YAML scene/prefab, package, API Unity cấm) | Tự động |
| `memory/` | Quyết định của team (`knowledge.md`), tổng quan, cốt truyện, plan, log từng người (`log/<tên git>.md`) | Theo INDEX |
| `settings.json` | Hook, chặn đọc thư mục rác (`Library/`, `Temp/`…), tắt dòng ghi công Claude trong commit | Mọi session |
| `tools/check_kit.py` | Kiểm tra bộ AI sau khi sửa | Bạn chạy |
| `../.mcp.json` | Khai báo server `UnityMCP` cho Claude Code | Claude Code hỏi duyệt lần đầu |

## Cài đặt (mỗi máy, một lần)

1. **Git for Windows** (có Git Bash — hook chạy trong Git Bash) + **Git LFS**: `git lfs install`.
2. **Python 3.10+** từ python.org, tick *Add python.exe to PATH*. Kiểm tra trong terminal mới:

   ```powershell
   python --version
   ```

   Nếu lệnh mở Microsoft Store: *Settings → Apps → Advanced app settings → App execution aliases* → tắt
   `python.exe` / `python3.exe`. Không có Python thì hook không chạy (không báo lỗi) — Claude vẫn dùng được
   nhưng mất gợi ý skill và lớp chặn sửa YAML.
3. **Unity 6000.3.15f1** qua Unity Hub (đúng bản này cho cả nhóm).
4. **Claude Code**: dùng app Claude (Desktop, tab Code) hoặc CLI:

   ```powershell
   irm https://claude.ai/install.ps1 | iex
   ```

5. Clone repo theo `README.md` ở gốc (có `--recurse-submodules`), mở project bằng Unity Hub.
6. **MCP for Unity** (package đã có sẵn trong project): trong Unity mở `Window > MCP for Unity` → **Start Server**.
   Làm lại mỗi lần mở Editor.
7. Mở Claude Code tại thư mục repo. Lần đầu:
   - Chọn **Trust** thư mục (hook và permission của project chỉ chạy sau khi trust).
   - Duyệt server `UnityMCP` từ `.mcp.json` khi được hỏi.
8. Kiểm tra nhanh — trong PowerShell ở thư mục repo:

   ```powershell
   python .claude/tools/check_kit.py
   ```

   In ra `OK` là hook chạy được. Trong Claude Code gõ `/mcp` thấy `UnityMCP` connected (Editor đang mở và đã Start Server).

Không bắt buộc:
- **Plugin chính thức của Unity cho Claude Code** — có skill UI/URP/Multiplayer Services; cài chọn lọc, đừng bật hết (mỗi skill tốn chỗ trong context).
- **agent-desktop** (`npm i -g agent-desktop`) nếu muốn Claude bấm giúp trong Unity Hub/Editor (skill `novent-desktop`).

## Dùng hằng ngày

1. `git pull`, tạo nhánh: `git checkout -b feat/<việc>`.
2. Mở Claude Code ở repo. Session mới tự có tóm tắt memory + session trước của bạn.
3. Gõ việc bằng tiếng Việt như bình thường. Ví dụ:
   - "đọc GDD phần Oxy rồi lên plan hệ thống Oxy" → `novent-plan` (+ `novent-architecture`)
   - "implement bước 1–3 của plan" → `novent-implement` + `novent-verify`, compile qua MCP sau mỗi bước
   - "viết test cho OxygenModel" → `novent-test`
   - "NullReferenceException ở PlayerMotor" + dán Console → `novent-debug`
   - "review nhánh này trước khi PR" → agent `novent-reviewer`
   Hook tự gợi ý skill; mỗi skill chỉ gợi ý một lần mỗi session.
4. Trước khi tắt: "ghi log session" → Claude ghi vào `.claude/memory/log/<tên git>.md`. Commit cùng nhánh.
5. Cuối ngày (tuỳ chọn): gõ `dream` — Claude đọc transcript hôm nay **trên máy bạn**, đề xuất bài học có trích
   dẫn; chỉ ghi khi bạn trả lời `ok 1 3`.
6. Đổi việc không liên quan → `/clear` (rẻ hơn nhiều so với kéo dài session).

## Memory hoạt động thế nào

- **Của team** (commit): `knowledge.md` (quyết định + lý do + bẫy), `project-overview.md`, `story.md`, `plans/`.
  Sửa `knowledge.md` = quyết định cho cả nhóm → đi qua PR.
- **Của bạn** (commit, chỉ bạn sửa): `log/<tên git>.md` — không ai conflict với ai.
- **Riêng máy bạn** (không commit): auto memory của Claude Code (`~/.claude/projects/...`) cho đường dẫn,
  tool cài riêng, thói quen cá nhân. Ghi chú riêng thêm vào `CLAUDE.local.md` ở gốc repo (đã gitignore).

## Tiết kiệm token

- `/clear` giữa các việc; `/context` và `/usage` để xem cái gì đang tốn.
- Để Claude giao tìm kiếm rộng cho `novent-explorer` (Haiku), không đọc nguyên file lớn.
- Skill global không liên quan project (của công ty, của việc khác): tắt riêng trong repo này bằng
  `.claude/settings.local.json` (gitignore) → `"skillOverrides": {"<tên-skill>": "off"}`.
- Tắt MCP server không dùng (`/mcp`).

## Lỗi thường gặp

| Hiện tượng | Cách xử lý |
|---|---|
| Không thấy `[novent-memory brief]` / không có gợi ý skill | Chưa trust thư mục, hoặc `python` không chạy — xem bước 2 |
| `UnityMCP` failed | Editor chưa mở / chưa Start Server / đang import |
| Claude bị chặn `[novent-guard] …` | Đúng luật: sửa scene/prefab qua MCP hoặc làm tay; API cấm thì dùng API được gợi ý |
| Hai người cùng sửa một scene | Tách phần mình thành prefab, chỉ đặt một instance vào scene |

## Sửa bộ AI

Đọc `rules/kit.md` (Claude tự nạp khi bạn sửa trong `.claude/`), chạy `python .claude/tools/check_kit.py`,
commit qua PR như code. Cụm từ bạn hay gõ mà skill không tự chạy → thêm vào `ROUTES` trong `hooks/skill_router.py`
(dream sẽ đề xuất).
