# The Novent

DATN SP27 — game Unity 2.5D (cảnh 3D, asset 2D, top-down 45°), co-op online 1–4 người.
Thiết kế: `Docs/` (GDD v0.5; các tab `v0.1` là concept cũ đã bỏ).

## Yêu cầu

| Thứ | Phiên bản |
|---|---|
| Unity | **6000.3.15f1** (cả team dùng đúng bản này) |
| Render | URP 17.3 (template Universal 3D) |
| Input | Input System 1.19 (`activeInputHandler` = Input System only) |
| Git | Git + **Git LFS** |

## Clone

```bash
git clone --recurse-submodules https://github.com/huyhao27/The-Novent.git
cd The-Novent
git lfs install
git submodule update --init --recursive
```

Đã clone rồi mà thiếu asset: `git submodule update --init --recursive`.

## Cấu trúc

| Đường dẫn | Là gì |
|---|---|
| `Assets/_Project/Scripts/` | Code game theo module: `Core` (service dùng chung), `Gameplay/<Hệ thống>`, `UI`, `Net`, `Boot` — mỗi module một asmdef, namespace `Novent.<Module>` |
| `Assets/_Project/Tests/` | EditMode/PlayMode test (Test Runner) |
| `Assets/Scenes/SampleScene.unity` | Scene mẫu của template, thay khi có scene của game |
| `Assets/NoventAssets/` | **Submodule** `huyhao27/the-novent-assets` — art, anim, effect, sound |
| `Assets/Settings/` | URP pipeline/renderer assets |
| `Docs/` | GDD và tài liệu |
| `.claude/` | Bộ AI chung cho Claude Code: luật code, skill, hook, memory của project — xem `.claude/README.md` |

## Code cùng Claude Code

Cả nhóm dùng chung bộ trong `.claude/`. Cài một lần theo [`.claude/README.md`](.claude/README.md)
(Python 3, MCP for Unity → Start Server, trust thư mục), sau đó mở Claude Code ở thư mục repo là dùng được.

### Làm việc với submodule asset

Asset nằm ở repo riêng. Thêm/sửa asset:

```bash
cd Assets/NoventAssets
git checkout main && git pull
# thêm file trong Unity (để Unity sinh .meta), rồi:
git add -A && git commit -m "..." && git push
cd ../..
git add Assets/NoventAssets && git commit -m "Bump assets" && git push
```

- Luôn commit cả file `.meta` trong repo asset, nếu không mọi tham chiếu tới asset sẽ mất.
- Repo game chỉ ghi lại *commit* của submodule; sau khi push asset phải commit "bump" ở repo game.

## Merge scene/prefab

Bật UnityYAMLMerge một lần trên máy (đổi đường dẫn Unity cho đúng máy bạn):

```bash
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'D:/Study/Unity/6000.3.15f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
git config merge.unityyamlmerge.recursive binary
```

Tránh hai người cùng sửa một scene: làm thành prefab rồi đặt vào scene.

## Unity CLI (không cần mở Editor)

```bash
unity projects add .            # đăng ký project vào Hub
unity projects verify . --json  # kiểm tra .meta thiếu/thừa, GUID trùng, conflict marker
unity run . --log-file build.log --timeout 600   # mở batchmode: import + compile
```
