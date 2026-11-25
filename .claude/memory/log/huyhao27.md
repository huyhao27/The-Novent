# Session log — huyhao27 (mới nhất trên cùng, giữ ≤ 15 mục)

## 2026-11-25 — chuẩn AI chung + kiến trúc lite (nhánh `chore/ai-standard`)
- **Đã làm**: `.claude/` thành thư mục thật trong repo game (bỏ junction); thêm `rules/` theo path, hook `session_start`/`guard`,
  router mới (bỏ qua tin máy, mỗi skill 1 lần/session), agent explorer/reviewer, skill `novent-architecture` + `novent-test`,
  `tools/check_kit.py`, `.mcp.json`, `.claude/README.md`. Core scaffold `Assets/_Project/Scripts/{Core,UI,Boot}` + 17 EditMode test.
- **Verify**: compile 0 lỗi (MCP), `run_tests` EditMode 17/17 pass, `unity projects verify` ok, `check_kit.py` OK.
- **Tiếp theo**: merge `chore/ai-standard` → main và báo team cài theo `.claude/README.md`; team chốt netcode (đề xuất NGO 2.x +
  Multiplayer Services); tạo scene MainMenu/Station/Expedition; prototype bay 8 hướng + Hãm.

## 2026-11-25 — tạo Unity project
- **Đã làm**: project Unity 6000.3.15f1 URP ở gốc repo (batchmode, template Universal 3D), submodule
  `the-novent-assets` → `Assets/NoventAssets` (repo asset commit `a650f06`), LFS + YAML merge, README setup
  (repo game `c092aaa`). Cài `agent-desktop`, update Unity CLI 1.0.1. Thêm skill `novent-desktop`.
- **Verify**: batchmode 0 lỗi compile; `unity projects verify` ok; clone `--recurse-submodules` mới chạy đúng.
- **Sau đó**: cài MCP for Unity v10.3.0, mở Editor bằng `unity open`, bấm OK hộp URP upgrade (agent-desktop) +
  Start Server (Windows-MCP, vì Unity không lộ UIA). `UnityMCP` Connected, `read_console` 0 lỗi. Commit `640c498`.
- **Chưa làm**: Cinemachine, netcode.
- **Tiếp theo**: chốt netcode; tạo `Assets/_Project` + scene Boot; prototype bay 8 hướng + Hãm.

## 2026-11-23 — session local tại D:/DATN
- **Đã làm**: cài kit (`D:\AI-Agent-Memory-Promax`, install.ps1 + init-game). `D:/DATN` thành clone của The-Novent;
  đẩy 3 tài liệu vào `Docs/` (commit `b05b035`). Đọc GDD v0.5 → điền project-overview.md, story.md, knowledge.md.
- **Phát hiện**: game là 2.5D co-op online 1–4, team 6 người (không phải làm một mình); tab v0.1 là concept đã bỏ.
- **Tiếp theo**: (1) đọc `huycv18/DATN-SP27` (`ky-thuat.md`); (2) team chốt Unity version + netcode + mốc;
  (3) tạo Unity project + LFS; (4) prototype bay 8 hướng + Hãm có đồng bộ mạng.

## 2026-11-23 — session khởi tạo (Claude Code cloud)
- **Đã làm**: đọc repo `The-Novent` (mới có README "DATN SP27" + Unity `.gitignore`, chưa có
  Unity project, chưa có tài liệu). Tạo nhánh kit `game/the-novent`: bộ skill `novent-*` (bỏ
  SDK/tracking/ads/backport của bộ skill gốc), hook router riêng, CLAUDE.md, memory khung.
- **Chưa làm được**: đọc tài liệu đồ án — chúng nằm ở `D:/DATN` trên máy bạn, session cloud
  không truy cập được.
- **Tiếp theo**: (1) bạn đẩy tài liệu lên repo (`Docs/`) hoặc mở Claude Code tại `D:/DATN`;
  (2) đọc tài liệu → điền `project-overview.md` + `story.md`; (3) chốt các quyết định ở
  `knowledge.md` § "Chờ chốt"; (4) tạo Unity project + `.gitattributes` LFS.
