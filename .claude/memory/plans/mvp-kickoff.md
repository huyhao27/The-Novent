# MVP kickoff — netcode, kiến trúc lắp ghép, phase, task cả team

> Trạng thái: **NHÁP v3 chờ Hào duyệt** (2026-11-26). v3: VFX do Huy chọn + order owner gắn; Thơ chỉ 2D, mọi 3D/rig/anim
> là Vũ (AI); sound do Vinh + Hoàng; thêm việc còn sót (cheat/debug, kiểm phiên bản, HUD đồng đội, camera mờ tường, danh sách
> thế giới, sprite 4 hướng, ánh sáng, đa ngôn ngữ, preset ngoại hình, license); Must/Should + đánh giá sức người. Chưa ghi Sheet, chưa đưa quyết định netcode vào `knowledge.md`.
> v2: thêm Huy (GD) + Thơ (art), tách **Owner mảng** khỏi **Người làm**, cân tải dev, dời Steam sang tháng 3,
> khớp lại với biên bản họp 23/11 (`meetings.md`).
> Ngày 1 = Thứ Năm 26/11/2026. Lịch theo ngày lịch; ước lượng = ngày công 1 người (~6 ngày làm/tuần).

## Goal
Ngày **20/12** đủ tính năng MVP (vòng thám hiểm/thu thập + giờ giao thương, co-op 2 người, greybox + art đầu),
**31/12** bản MVP ổn định. Hạ tầng dùng tiếp tới Steam không viết lại: transport thay được, hệ thống cắm/rút độc lập,
mỗi mảng có một owner chịu trách nhiệm trọn đời.

## Đối chiếu với họp 23/11 (`meetings.md`) — plan theo cái nào
| Điểm | Họp 23/11 | Yêu cầu 26/11 của Hào | Plan dùng |
|---|---|---|---|
| Multiplayer | Chơi đơn greybox trước, mạng sau refactor | Netcode + session vững ngay Phase 0, mỗi phase build co-op 2 người | **26/11** (mới hơn; viết đơn rồi refactor sang mạng = viết lại). Báo team ở check-in cuối tuần |
| Mốc | Tính năng ~20/12 → ~10 ngày tích hợp → MVP cuối tháng 12 | Feature MVP trong tháng đầu | **Feature freeze 20/12, MVP 31/12** (khớp cả hai) |
| Phạm vi MVP | 2 luồng: thám hiểm/thu thập + trade hall; cốt truyện làm sau; nuôi sinh vật ngoài MVP | Theo GDD v0.5 | **Họp**: tháng 1 = 2 luồng; trọn chương 1 (truyện, Cinder, boss, đủ loài) = tháng 2 |
| Map/UI trong Unity | Huy thiết kế + lắp map, tích hợp UI | — | Huy lắp mảnh map + UI art; dev viết hệ thống |
| 3D/anim/VFX | Vũ dựng nhân vật chính + vài quái 3D, rig, VFX | — | Vũ nhận ít task code hơn để có thời gian 3D |
| Test | Hoàng (tạm) phụ trách test | — | Hoàng là owner QA: test case, kịch bản mỗi build |
| Git | `feat/*` → `dev` → `main`, nhánh `config` cho GD | `CLAUDE.md` đang ghi PR thẳng `main` | **Theo họp** — task P0-05 tạo `dev` + sửa `CLAUDE.md` |
| Codex (Vinh) | Repo chưa có `AGENTS.md` | — | Task P0-05 thêm `AGENTS.md` trỏ về `CLAUDE.md` |

## Giả định đã dùng
- GDD v0.5 vẫn giữ sau buổi chốt ý tưởng ngày 3; task gameplay có cổng **D3** nhưng P0 không phụ thuộc mechanic.
- GDD ghi "MVP tối đa 2 người, chỉ listen-host"; Hào muốn dedicated + 1–4 → code hỗ trợ từ đầu, triển khai thật tháng 2.
- Nguồn số liệu theo `ky-thuat.md`: markdown `gdd/he-thong/` → JSON → importer Unity → SO (`knowledge.md` đang ghi
  "Sheet → SO" — sửa khi duyệt).
- Thơ là người vẽ chính, chỉ làm trong repo asset (không vào Unity). Hạn cứng của Thơ: **15/12**.

---

## 1. Quyết định netcode

| | NGO 2.x + Multiplayer Services | FishNet 4.7 | Mirror 96 | Photon Fusion 2 | PurrNet |
|---|---|---|---|---|---|
| Unity 6000.3 | Có — 6.3 **chỉ** hỗ trợ NGO 2.x (3.x cần 6000.7+) | Có (build trên 6000.3.17) | Chạy được, chưa có xác nhận 6.3 | Chưa xác minh | Có (≥2022) |
| Listen-host / Dedicated / LAN | Có / Có / Có (UTP direct) | Có / Có / Có (Tugboat) | Có / Có / Có | Host mode / Có / **phụ thuộc Photon cloud** | Có / ? / ? |
| Đường lên Steam | Transport cộng đồng — **không bảo đảm** với 2.x → có thể tự viết | FishySteamworks + Multipass — tốt nhất | FizzySteamworks (ít cập nhật) | Không rõ | Không rõ |
| Chi phí | Relay free 50 CCU TB/tháng, 3 GiB/CCU; Lobby free 10 GiB/tháng | Free (Pro $50) | Free | 100 CCU free, thương mại được, chặn cứng | Free (MIT) |
| Docs / AI viết đúng | Docs chính thức dày; API đổi giữa bản | Gitbook, nhiều link chết, 1 tác giả | Docs nhiều, ổn định | Tốt nhưng gắn cloud | Ít |
| Team đã dùng | Có | — | — | Có (Photon) | — |

Thực tế 2026: **Multiplay ngừng từ 01/04/2026**, Hathora dừng 05/05/2026 → dedicated tự chạy trên VPS Linux
(~6–12 USD/tháng, Singapore). Distributed Authority không hợp "host giữ thế giới" → không dùng.

**Chốt: NGO 2.x (bản 2.x ổn định mới nhất cho 6000.3, không lấy pre-release) + Unity Transport
+ `com.unity.services.multiplayer` 2.2.x (Sessions/Relay) + Multiplayer Play Mode 2.0.**

Lý do: stack chính thức của 6000.3; một codebase cho offline/LAN/online/dedicated; team đã dùng; tài liệu và dữ liệu
AI nhiều nhất; Relay miễn phí đủ cho đồ án; MPPM test 2–4 người trên 1 máy. Steam không gấp → rủi ro transport Steam
được kiểm bằng spike ở tháng 2–3, trước khi cần.

| Chế độ | Cách | Cần internet/UGS |
|---|---|---|
| Offline 1 người | `StartHost`, UTP bind 127.0.0.1, không nhận kết nối | Không |
| LAN | UTP direct IP + tự viết UDP broadcast discovery | Không |
| Online | Sessions + Relay, mã tham gia | Có (anonymous auth) |
| Dedicated | Build profile Linux Dedicated Server, `StartServer` | Tuỳ |
| Steam (tháng 3) | Steamworks.NET + `NetworkTransport` trên SteamNetworkingSockets (SDR), lobby + mời bạn | Steam |

Thay transport sang Steam: gameplay chỉ biết `INetSession`/`ISessionLauncher`; transport chọn trong `Novent.Net` qua
`ITransportProfile` (Direct, Relay, Steam). Spike AppID 480 (P6-06) → transport Steam (tháng 3). **Đường lùi**: spike chứng minh
NGO + Steam không khả thi trong 5 ngày công → FishNet; chỉ viết lại `Novent.Net` + adapter `*.Net` vì gameplay không chứa kiểu netcode.
Rủi ro: API NGO/MPS đổi nhanh → ghim version, link docs `@2.x` vào `knowledge.md`.

---

## 2. Kiến trúc module lắp ghép

### 2.1 Đã có (giữ)
`Services` + `GameBootstrap`, `EventBus<T>`, `PoolService`, `SaveService` (JSON + version, ghi tạm rồi swap), `SeededRandom`,
`INetSession`/`OfflineSession`/`PlayerId`, `Panel`/`PanelService`/`PanelRegistry`/`UIRoot`. Core ← Gameplay ← UI/Net ← Boot.

### 2.2 Đề xuất đổi (đổi quyết định "một `Novent.Gameplay`" trong knowledge)
```
Novent.Core                      Services, EventBus, Save, Rng, Session (interfaces), IGameModule, IRequestChannel<T>
Novent.Gameplay.Shared           Hợp đồng giữa các mảng: ContentId, ItemStack, IInteractable, IInventory, IPlayerStats, IOxygen, event dùng chung
Novent.Gameplay.<Feature>        Player, Oxygen, Combat, Driftlife, World, Inventory, Station, Crafting, Trade, Shop, Upgrade, DayCycle, Story…
Novent.Gameplay.<Feature>.Net    Adapter NGO của feature
Novent.Net                       Session NGO, transport profile, launcher, request router, LAN discovery
Novent.UI / Novent.UI.<Feature>  Kit + màn chung / panel của feature
Novent.Boot                      GameBootstrap + ModuleCatalog
Novent.Editor                    Importer JSON → SO, validator, cheat
```
Luật cắm/rút:
1. Feature **không tham chiếu assembly feature khác** (compiler chặn); cần nhau → interface trong `Gameplay.Shared` hoặc `EventBus`.
2. `IGameModule` (`Install`/`Uninstall`, khai báo `Requires`) + `ModuleCatalog` (SO): rút hệ thống = bỏ tick; thiếu `Requires` → lỗi có tên.
3. `SceneScope`: service chỉ sống trong một scene đăng ký khi load, gỡ khi unload.
4. Request → Apply qua `IRequestChannel<TRequest>`: host chạy handler kèm `PlayerId` người gửi; gameplay không chạm RPC.
   State chung đồng bộ bằng adapter `*.Net` (late join thấy).
5. ID nội dung là string qua `ContentDatabase` — mạng và save gửi ID, không gửi tham chiếu SO.
6. Mỗi feature sở hữu: thư mục code, `Configs/<Feature>/`, `Prefabs/<Feature>/`, panel UI, save key, test.

### 2.3 Chỗ thiếu → task
| Thiếu | Task |
|---|---|
| `INetSession` không phân biệt server/host, không có host/join/leave, trạng thái kết nối | P0-02 |
| `IGameModule`, `ModuleCatalog`, `SceneScope`, `IRequestChannel`, `Gameplay.Shared` | P0-04 |
| Game flow + nạp scene (network scene management) | P1-01 |
| `SaveService` 1 slot → world (host) + profile + settings; chưa giữ `.bak` (GDD yêu cầu) | P1-02 |
| Vòng ngày 5 pha ở host | P1-03, P3-01 |
| Input actions + rebind; camera 45° + billboard (Cinemachine chưa cài) | P0-12, P0-13, P1-16 |
| `ContentDatabase` + importer JSON → SO; spec xuất art | P0-16, P0-14 |
| Interaction (nhấn/giữ E) dùng chung | P0-08 |
| CI/build, audio, LAN discovery + kiểm phiên bản, rớt mạng/vào lại, dedicated | P1-04, P2-13, P2-02, P2-03, P5-05 |
| Cheat/debug cho test, HUD đồng đội, camera mờ tường, chọn/xoá thế giới, sprite 4 hướng, đa ngôn ngữ | P1-18, P3-12, P3-13, P3-17, P0-13, P6-08 |

---

## 3. Owner mảng và cách chia việc

**Owner** chịu trách nhiệm cả mảng: quyết thiết kế kỹ thuật trong mảng, **duyệt mọi PR chạm mảng** (bắt buộc), tự sửa/chỉnh
code người khác làm trong mảng, nhận mọi bug của mảng về sau. **Người làm** implement task được giao, theo hợp đồng owner đặt.
Owner tự làm phần lõi; phần quanh rìa chia cho dev khác để cân tải.

| Mảng | Owner | Gồm | Scene/thư mục owner giữ |
|---|---|---|---|
| **PLT** Platform | Hào (code owner kiến trúc) | Net/session/transport/Steam/dedicated, game flow + scene, vòng ngày, save, CI/build, tài liệu kiến trúc | `Boot`, `Scripts/Core`, `Scripts/Net` |
| **EXP** Expedition | Vũ | Player motor, Interaction, Oxy/Ngạt, combat/lưới, Driftlife + boss, map ghép mảnh + map nhánh, hazard, điểm khai thác, dụng cụ; **mọi model 3D (dựng bằng AI) + rig + animation** | `Expedition` |
| **STA** Station & Economy | Vinh | Túi/Khối, kho chung, Tally, sửa khoang, xưởng, động cơ, giao thương, shop, nâng cấp, hạng cảng, người vớt; tìm + gắn sound phần trạm/giao thương | `Station` |
| **PRE** Presentation, Content & QA | Hoàng | Input, camera/billboard, UI kit + màn chung, audio service, importer dữ liệu, truyện/thoại/Sổ tay/tutorial, đa ngôn ngữ, test case + kịch bản build, CI chạy test; tìm + gắn nhạc + sound phần chuyến/UI | `MainMenu` |
| **DES** Design | Huy | GDD, số liệu/JSON, spec hệ thống, mảnh map + layout trạm (lắp trong Unity), UI flow + tích hợp UI art, ánh sáng; **chọn VFX và order owner gắn**; danh sách sound; playtest/cân bằng (nhánh `config`) | `Prefabs/World/Chunks`, `Configs` (dữ liệu) |
| **ART** Art 2D | Thơ | **Chỉ 2D**: concept, map trong/ngoài, nhân vật 4 hướng, Driftlife (bản 2D để Vũ dựng 3D khi cần), item, tàu, UI skin — chỉ repo asset | repo `the-novent-assets` |

VFX: Huy tìm pack + lập bảng "sự kiện → VFX → owner" (D2-05), mỗi owner gắn phần mảng mình (P4-09), chỉnh lẻ dần ở tháng 3–4.
Sound: Huy lập danh sách từ tab AUDIO ASSETS LIST (D2-06); Vinh và Hoàng tìm + gắn (P3-18, P3-19). Mọi asset ngoài (VFX, sound,
font) ghi nguồn + license vào `Docs/licenses.md` — bán trên Steam cần license thương mại.

Việc làm chéo (người làm ≠ owner) trong tháng 1:
| Task | Owner duyệt | Người làm |
|---|---|---|
| Interaction, Điểm khai thác | Vũ | Vinh |
| Map ghép mảnh theo seed | Vũ | Hào |
| Hazard | Vũ | Hoàng |
| Shop | Vinh | Hào |
| CI/build | Hào | Hoàng |
| HUD đồng đội | Hoàng | Vinh |

### Có đủ người làm hết không — trả lời thẳng: **không làm hết 100% trong tháng 1, nhưng phần Must thì vừa khít**
26/11–20/12 = 25 ngày lịch ≈ **21,5 ngày làm** (6 ngày/tuần). Năng suất thực tế khi còn học, review, sửa bug, họp ≈ 75–80%
→ mỗi người **~16–17 ngày công thật**.

| Người | Tổng task P0–P3 | Trong đó Must | Should (trượt sang P4/P5 được) |
|---|---|---|---|
| Hào | 22 | 17,5 | P2-02 LAN discovery (vẫn vào LAN bằng IP), P3-03 Shop, P3-04 tài liệu |
| Vũ | 21 (6,5 là 3D/rig/anim) | 19,5 | P3-09 Driftlife 3D (dùng sprite 2D của Thơ) |
| Vinh | 22 | 18 | P3-19 sound, P3-12 HUD đồng đội, phần bơm khí/tip của P3-10 |
| Hoàng | 21,75 | 18,75 | P3-18 sound, P3-17 danh sách thế giới, P3-13 camera mờ tường |
| Huy | 20,25 | 18 | D3-01 kịch bản ch.1 (cho tháng 2), D3-02 layout Cinder |
| Thơ | 16,5 (+2 nếu kịp) trong ~17 ngày làm đến 15/12 | 16,5 | A3-03 |

Kết luận:
- **Must** của dev vẫn hơn sức thật 1–3 ngày, nặng nhất là **Vũ** (vừa code Expedition vừa 3D). P4 (21–31/12, 11 ngày) là đệm:
  ~4 ngày đầu hứng phần trễ, còn ≥5 ngày sửa bug trước `mvp-1.0`.
- **Should** không ai cam kết trong tháng 1; xong Must sớm thì nhận Should, không thì trượt sang P4 rồi P5.
- Thứ tự cắt khi trễ quá 3 ngày ở check-in: Should → bơm khí/tip → nâng cấp trục thứ 2 → hazard thứ 2 → loài thứ 2
  (MVP vẫn đủ 2 luồng thám hiểm + giao thương).
- Giả định cần xác nhận: mỗi người làm ~6 ngày/tuần, 6–7 giờ/ngày cho đồ án. Ít hơn thì Must không vừa — phải cắt tiếp.

Tránh đụng nhau: chỉ sửa scene mình giữ (Huy lắp map trong prefab mảnh, không mở scene `Expedition` khi Vũ đang sửa);
test riêng ở `Scenes/Dev/<Tên>_*.unity`; `GameBootstrap`/`ModuleCatalog`/`Gameplay.Shared` đổi qua PR có Hào duyệt.
Nhịp: check-in cuối tuần (theo họp) trùng ngày ra build: 28/11, 05/12, 12/12, 20/12.

---

## 4. Phase

| Phase | Ngày | Mục tiêu | Build demo co-op 2 người | Tiêu chí xong |
|---|---|---|---|---|
| **P0 Nền móng** | 26–28/11 | Netcode, hợp đồng module, input, camera, UI kit, túi/kho, chốt style art, chốt ý tưởng | **B0 greybox**: host/join (IP + mã Relay), 2 người bay, nhặt đồ (host phân xử) | 2 máy thấy nhau; 2 người cùng nhặt → đúng 1 người nhận; compile 0 lỗi; test pass |
| **P1 Vòng lõi khung** | 29/11–05/12 | Một ngày tối giản end-to-end | **B1**: menu → tạo/tham gia thế giới → trạm → cửa khí → chuyến (bay, Oxy, khai thác) → về → cất kho → ngủ → lưu → load lại | Load lại khớp kho/Tally/ngày; client thấy đúng pha ngày; CI xanh |
| **P2 Thám hiểm + giao thương v1** | 06–12/12 | Hai luồng MVP bản v1 | **B2**: map ghép mảnh theo seed, combat + choáng, 2 loài, hazard, giao thương v1, xưởng | Cùng seed cùng map 2 máy; rớt mạng 15s đúng luật; 1 vòng giao thương 2 người |
| **P3 MVP đủ tính năng** | 13–20/12 | Đóng vòng kinh tế | **B3 (feature freeze 20/12)**: lưới bắt sống, Ngạt/chia Oxy/kéo về, giao thương v2 (3 cửa neo, bơm khí, tip, hạng cảng), shop, nâng cấp 2 trục, vòng ngày đủ, sound, HUD đồng đội | Chơi nhiều ngày liên tiếp 1 và 2 người không soft lock |
| **P4 Tích hợp art + ổn định** | 21–31/12 | Ráp art (Thơ giao 15/12), bug bash | **MVP 1.0** (tag `mvp-1.0`, 31/12) | 0 bug chặn; chấm MVP checklist phần thuộc phạm vi; build bảo vệ |
| **P5 Trọn chương 1** | 01–13/01 | Truyện + nội dung còn lại | **B5**: 9 mốc truyện + Sổ tay + thoại, Station Cinder + người vớt, đủ 4 loài + Scrapmaw, động cơ gđ1, dụng cụ khai thác, nâng cấp 5 trục, tutorial | Chơi trọn chương 1 (1,5–2 giờ) 1 và 2 người; dedicated chạy trên VPS |
| **P6 Scale + chiều sâu** | 14–25/01 | 3–4 người, chiều sâu, đa ngôn ngữ | **B6**: 4 người, 4 preset, nghiên cứu + khoang sinh quyển, Vành đai trong, tiếng Anh, playtest + tài liệu | 4 người MPPM không lệch; join VPS từ 2 mạng |
| Tháng 3–4 | 26/01–~26/03/2027 | Steam (transport, lobby, mời bạn, trang store + capsule art), chương 2–5, polish visual + VFX chỉnh lẻ, tài liệu đồ án (báo cáo, test report, slide, video demo) | Build mỗi 2 tuần | Theo mốc bảo vệ |

---

## 5. Task

Cổng: **Ngay** · **D2-art** (cần concept/vùng UI ngày 2, không chặn greybox) · **D3** (sau chốt ý tưởng ngày 3) ·
**GD** (cần số liệu/spec của Huy — dùng số tạm trước) · **Art** (cần art của Thơ).
Mỗi task gameplay gồm panel UI của nó (kit của Hoàng), test EditMode cho model, bước kiểm MPPM 2 người, và PR có owner duyệt.
Cột **Ưu tiên** trên Sheet: task P0–P4 là **Must**, trừ các task Should liệt kê ở §3; P5–P6 là **Plan**.

| ID | Phase | Mảng | Owner | Task | Tiêu chí xong | Phụ thuộc | Người làm | Ngày | Bắt đầu | Hạn | Cổng |
|---|---|---|---|---|---|---|---|---|---|---|---|
| P0-01 | P0 | PLT | Hào | Cài NGO 2.x ổn định, Multiplayer Services 2.2.x, MPPM 2.0; ghim version; asmdef `Novent.Net` | Compile 0 lỗi; manifest ghim version | — | Hào | 0.5 | 26/11 | 26/11 | Ngay |
| P0-02 | P0 | PLT | Hào | `NgoNetSession : INetSession` + `ISessionLauncher` (Offline / Host direct / Join IP); thêm `IsServer`, `HasLocalPlayer`, `State` | MPPM host + 1 client, `Players` = 2 trên cả hai; offline không mở cổng | P0-01 | Hào | 1 | 26/11 | 27/11 | Ngay |
| P0-03 | P0 | PLT | Hào | Online qua Sessions + Relay (anonymous auth, mã tham gia) | 2 máy khác mạng join bằng mã | P0-02 | Hào | 1 | 27/11 | 28/11 | Ngay (tạo Unity Cloud project) |
| P0-04 | P0 | PLT | Hào | Hợp đồng lắp ghép: `IGameModule` + `ModuleCatalog`, `SceneScope`, `IRequestChannel<T>`, asmdef `Gameplay.Shared` (ContentId, ItemStack, IInteractable, IInventory) + cập nhật `rules/architecture.md` | Compile; test module install/requires; cả team review chiều 27/11 | — | Hào | 1 | 27/11 | 27/11 | Ngay |
| P0-05 | P0 | PLT | Hào | Git flow theo họp: nhánh `dev` (+ `config` cho GD), sửa `CLAUDE.md`; thêm `AGENTS.md` cho Codex trỏ về `CLAUDE.md` | Nhánh `dev` có trên remote; Vinh mở Codex thấy quy ước | — | Hào | 0.25 | 26/11 | 26/11 | Ngay |
| P0-06 | P0 | EXP | Vũ | `PlayerMotor` bay 8 hướng quán tính + Hãm (model C# + view Rigidbody, `MotorConfig` SO) | Test: Hãm dừng lệch ≤0,5u >90%; chỉnh số trong SO không cần code | P0-12 | Vũ | 1 | 26/11 | 26/11 | Ngay · GD |
| P0-07 | P0 | EXP | Vũ | Player prefab mạng: spawn theo người chơi, di chuyển owner-authoritative, camera theo người local | MPPM 2 người thấy nhau bay, trễ <0,5s | P0-02, P0-13 | Vũ | 1 | 27/11 | 27/11 | Ngay |
| P0-08 | P0 | EXP | Vũ | `Interactor` + `IInteractable` (nhấn/giữ E, prompt); cục đồ greybox, host phân xử nhặt | 2 người cùng nhặt 100 lần → mỗi lần đúng 1 người nhận | P0-04, P0-07 | Vinh | 1 | 28/11 | 28/11 | Ngay |
| P0-09 | P0 | EXP | Vũ | Thử nhân vật 3D + shader dưới camera 45° cạnh sprite 2D → đề xuất phương án art + dự phòng cho buổi D3 | Ảnh so sánh 2–3 phương án trong scene Dev | P0-13 | Vũ | 1 | 28/11 | 28/11 | Ngay |
| P0-10 | P0 | STA | Vinh | Model túi (đơn vị Khối, sức chứa, thêm/bớt/đầy) + Đai dụng cụ, C# thuần | Test EditMode thêm/bớt/đầy/tách stack | — | Vinh | 1 | 26/11 | 26/11 | Ngay |
| P0-11 | P0 | STA | Vinh | Kho chung (host apply qua request), save section `storage` | Test round-trip save; client gửi cất đồ → host apply | P0-04, P0-10 | Vinh | 1 | 27/11 | 27/11 | Ngay |
| P0-12 | P0 | PRE | Hoàng | Actions asset `NoventInput` (map Fly, Walk, UI; phím + tay cầm) + `InputService` đổi context | Đổi Fly↔Walk↔UI không lọt phím; xong buổi sáng để Vũ dùng | — | Hoàng | 0.5 | 26/11 | 26/11 | Ngay |
| P0-13 | P0 | PRE | Hoàng | Cài Cinemachine 3; `CameraRig` top-down 45° + component billboard sprite chọn 1 trong 4 hướng theo hướng nhìn (art của Thơ là 2D tĩnh 4 hướng) | Sprite luôn quay về camera, đổi đúng hướng khi di chuyển; khung 16:9 | — | Hoàng | 0.5 | 26/11 | 26/11 | Ngay (đổi manifest — báo team) |
| P0-14 | P0 | PRE | Hoàng | Spec xuất art cho Thơ: px/u (72 px/u ở 720p), pivot, kích thước, 4 hướng, đặt tên, thư mục repo asset | Thơ đọc hiểu và xác nhận | — | Hoàng | 0.25 | 27/11 | 27/11 | Ngay |
| P0-15 | P0 | PRE | Hoàng | UI kit (theme, font TMP tiếng Việt, Button/Toggle/Slider/Input) + MainMenu, Tạo thế giới, Tham gia (IP/mã) gọi `ISessionLauncher` | Host/join từ menu; chữ có dấu đúng | P0-02 | Hoàng | 1 | 27/11 | 27/11 | D2-art |
| P0-16 | P0 | PRE | Hoàng | `ContentDatabase` (ID → def) + importer JSON → SO (`Novent.Editor`) theo schema `ky-thuat.md` | Import JSON mẫu tạo `ItemDef`; import lại không đổi GUID; thiếu ID báo lỗi | P0-04 | Hoàng | 1 | 28/11 | 28/11 | Ngay |
| P0-17 | P0 | PLT | Hào | Ráp + quay B0 greybox | Video 2 cửa sổ MPPM: host/join, bay, nhặt | P0-03, P0-07, P0-08 | Hào | 0.25 | 28/11 | 28/11 | — |
| D0-01 | P0 | DES | Huy | Danh sách asset MVP bắt buộc + thứ tự (map trong → map ngoài → nhân vật → item) + cột ưu tiên ITEM LIST; giao Thơ | Thơ có list đủ để lên lịch đến 15/12 | — | Huy | 0.5 | 26/11 | 26/11 | Ngay |
| D0-02 | P0 | DES | Huy | Chuẩn bị + chủ trì buổi chốt ý tưởng D3 (phạm vi MVP, co-op, giao thương, map); ghi biên bản `meetings.md` | Biên bản có danh sách đã chốt / còn mở | — | Huy | 1 | 27/11 | 28/11 | Ngay |
| D0-03 | P0 | DES | Huy | Số liệu tạm cho P0–P1 (motor, Oxy, túi Khối, khoang, công thức) dạng JSON theo schema importer | Import được bằng P0-16 | P0-16 | Huy | 1 | 28/11 | 30/11 | Ngay |
| A0-01 | P0 | ART | Thơ | Concept khoá style: nhân vật chính, môi trường trạm + ngoài, 1 Driftlife, item, tàu + vùng UI | Bộ concept đưa họp D2 | — | Thơ | 1.5 | 26/11 | 27/11 | Ngay |
| A0-02 | P0 | ART | Thơ | Chốt style cùng Huy + Vũ (dựa trên test 3D của Vũ), ghi lại bảng màu + quy tắc nét | Style guide 1 trang trong repo asset | A0-01, P0-09 | Thơ | 0.5 | 28/11 | 28/11 | — |
| P1-01 | P1 | PLT | Hào | `GameFlowService` + `SceneLoader` (MainMenu/Station/Expedition, network scene management) | Host đổi scene → client theo; chơi thẳng từng scene vẫn chạy | P0-02 | Hào | 1 | 29/11 | 29/11 | Ngay |
| P1-02 | P1 | PLT | Hào | Slot world (host) + profile + settings; giữ `.bak`; nhiều thế giới; seed lưu cùng thế giới | Test cắt giữa lúc ghi → mở bản trước; 2 thế giới độc lập | — | Hào | 1.5 | 30/11 | 01/12 | Ngay |
| P1-03 | P1 | PLT | Hào | Vòng ngày: state machine 5 pha ở host, ≤2 chuyến/ngày, ngủ → sang ngày + tự lưu; sync pha | Client thấy đúng pha; chuyến 3 bị chặn; event `DayAdvanced` | P1-01, P1-02 | Hào | 2 | 02/12 | 03/12 | D3 |
| P1-04 | P1 | PLT | Hào | GitHub Actions (GameCI): EditMode test + build Windows mỗi PR vào `dev`; dự phòng `tools/build.ps1` | PR có check; artifact tải được | P0-17 | Hoàng | 1 | 29/11 | 29/11 | Ngay |
| P1-05 | P1 | PLT | Hào | Tích hợp + build B1 | Kịch bản B1 của Hoàng pass | Tất cả P1 | Hào | 0.5 | 05/12 | 05/12 | — |
| P1-18 | P1 | PLT | Hào | Menu cheat dev (thêm đồ/Tally, sang ngày, dịch chuyển, bất tử Oxy) + overlay mạng (vai trò, ping, số người) — chỉ có trong build dev | Cả team test nhanh không cần chơi lại từ đầu; build release không có | P1-03 | Hào | 1 | 04/12 | 04/12 | Ngay |
| P1-06 | P1 | STA | Vinh | `Inventory.Net`: đồng bộ túi riêng + kho chung; implement `IInventory` | Client nhặt → túi đổi; kho giống nhau 2 máy; late join thấy kho | P0-08, P0-11 | Vinh | 1 | 29/11 | 29/11 | Ngay |
| P1-07 | P1 | EXP | Vũ | Đi bộ 8 hướng trong trạm (motor chế độ Walk) + chuyển chế độ qua cửa khí | Đổi chế độ không giật; 2 máy thấy đúng | P0-06 | Vũ | 1 | 29/11 | 29/11 | D3 |
| P1-08 | P1 | EXP | Vũ | Oxy model (hao nền 1/s, sát thương trừ Oxy, bình 330) riêng từng người, host kiểm; `IOxygen` | Test EditMode; Oxy 2 người độc lập, đúng trên 2 máy | P0-04 | Vũ | 1.5 | 30/11 | 01/12 | D3 · GD |
| P1-09 | P1 | EXP | Vũ | Điểm khai thác (giữ E, rơi đồ, host quyết) dùng Interaction + `IInventory` | Khai thác → vào túi; 2 người cùng điểm không nhân đôi | P0-08, P1-06 | Vinh | 1.5 | 30/11 | 01/12 | D3 |
| P1-10 | P1 | EXP | Vũ | Cửa khí + Recall Beacon (giữ phím, bị đánh hủy) + kết chuyến | Mỗi người tự kết thúc chuyến; về đúng trạm | P1-03, P1-07 | Vũ | 1.5 | 04/12 | 05/12 | D3 |
| P1-11 | P1 | EXP | Vũ | Dựng 3D nhân vật chính theo turnaround của Thơ | Model trong scene Dev đúng tỉ lệ (1u = chiều cao) | A1-01, A0-02 | Vũ | 2 | 02/12 | 03/12 | Art |
| P1-12 | P1 | STA | Vinh | Scene `Station` greybox theo layout của Huy + điểm cất kho | Đi bộ tới kho, cất từ túi, 2 máy thấy kho đổi | D1-01, P1-06 | Vinh | 1 | 02/12 | 02/12 | GD |
| P1-13 | P1 | STA | Vinh | Ví Tally chung (host), event, save section | Tally giống nhau 2 máy, còn sau load | P0-04 | Vinh | 0.5 | 03/12 | 03/12 | Ngay |
| P1-14 | P1 | STA | Vinh | Sửa khoang: `BayDef` (vật liệu → trạng thái, đổi hình, mở chức năng) + save | Sửa bằng vật liệu trong kho; còn sau load | P1-12, P0-16 | Vinh | 2 | 04/12 | 05/12 | D3 · GD |
| P1-15 | P1 | PRE | Hoàng | HUD bay (Oxy, túi), panel Túi đồ, panel Kho, Hộp xác nhận | Đọc được ở 720p; cập nhật theo event | P0-15 | Hoàng | 2 | 30/11 | 01/12 | D2-art |
| P1-16 | P1 | PRE | Hoàng | Menu tạm dừng (Esc), Cài đặt (4 thanh âm lượng, đồ hoạ), Đổi phím (rebind + lưu profile) | Esc ở mọi màn; phím đổi còn sau khởi động lại | P0-12, P1-02 | Hoàng | 2 | 02/12 | 03/12 | Ngay |
| P1-17 | P1 | PRE | Hoàng | File test case chung (theo họp) + kịch bản test B1 2 người + chạy test B1 | Kịch bản có bước host/client/late join; bug ghi tab Bugs Report | P1-05 | Hoàng | 1 | 04/12 | 05/12 | — |
| D1-01 | P1 | DES | Huy | Layout greybox trạm Ember (khoang, quầy, cửa khí, 3 cửa neo, chỗ ngủ, kho) | Bản vẽ có kích thước theo u, Vinh dựng được | D0-02 | Huy | 1 | 29/11 | 30/11 | D3 |
| D1-02 | P1 | DES | Huy | UI flow + Figma wireframe các màn thuộc phạm vi tháng 1 | Danh sách màn + luồng chuyển màn | D0-02 | Huy | 2 | 29/11 | 01/12 | D3 |
| D1-03 | P1 | DES | Huy | Spec 2 loài Driftlife đầu + số combat (choáng, sát thương, lưới) | Đủ để Vũ code P2-05/P2-06 | D0-02 | Huy | 1 | 02/12 | 02/12 | — |
| D1-04 | P1 | DES | Huy | Spec giờ giao thương v1 + v2 (tàu, kiên nhẫn, giá, danh mục, bơm khí, tip, hạng cảng) | Đủ để Vinh code P2-08/P3-10 | D0-02 | Huy | 1 | 03/12 | 03/12 | — |
| D1-05 | P1 | DES | Huy | Xuất dữ liệu thật JSON (`xuat-data.mjs` hoặc từ Sheet): ITEM, Recipe, Enemy, Customer, Upgrade | Import sạch bằng P0-16 | P0-16 | Huy | 2 | 04/12 | 05/12 | — |
| D1-06 | P1 | DES | Huy | Playtest B1 + ghi bug | Bug ghi vào Bugs Report | P1-05 | Huy | 0.25 | 05/12 | 05/12 | — |
| A1-01 | P1 | ART | Thơ | Turnaround nhân vật chính (trước/sau/trái/phải) cho Vũ dựng 3D | 4 góc cùng tỉ lệ | A0-02 | Thơ | 1 | 29/11 | 30/11 | — |
| A1-02 | P1 | ART | Thơ | Map trong nhà — trạm Ember: sàn, tường, khoang hỏng/đã sửa, quầy, cửa khí, cửa neo, chỗ ngủ, kho | Xuất theo spec P0-14, đẩy repo asset | D1-01, P0-14 | Thơ | 4 | 01/12 | 04/12 | — |
| P2-01 | P2 | EXP | Vũ | Map tổng ghép mảnh theo seed (`hash(seed, ngày, chuyến)`), format prefab mảnh, lưới 40×24u, cổng 8u, spawn trọng số | Cùng seed cùng map 2 máy; 1000 lần sinh hợp lệ; format chốt 07/12 cho Huy | P1-03 | Hào | 3 | 06/12 | 08/12 | GD |
| P2-02 | P2 | PLT | Hào | LAN discovery (UDP broadcast) + danh sách host; duyệt kết nối: lệch phiên bản build → từ chối có thông báo, tối đa 4 người | 2 máy cùng LAN thấy nhau, không cần internet; client khác bản bị từ chối rõ ràng | P0-02 | Hào | 1.5 | 09/12 | 10/12 | Ngay |
| P2-03 | P2 | PLT | Hào | Rớt mạng: giữ chỗ 15s, quá 15s kéo về; vào lại đúng slot; host thoát → về điểm lưu; late join | Kịch bản rút dây mạng pass 2 máy | P1-02, P1-10 | Hào | 2 | 10/12 | 11/12 | Ngay |
| P2-04 | P2 | PLT | Hào | Tích hợp + build B2 | Kịch bản B2 pass | Tất cả P2 | Hào | 0.5 | 12/12 | 12/12 | — |
| P2-05 | P2 | EXP | Vũ | Đòn đánh + nhận đòn (2 đòn 1 khung trừ 1 lần) + Choáng 4s, trần hệ số 3; host quyết | Test; 2 người đánh cùng con không lệch | P1-08, D1-03 | Vũ | 2 | 06/12 | 07/12 | GD |
| P2-06 | P2 | EXP | Vũ | AI Driftlife (state machine ở host, di chuyển 2.5D) + 2 loài đầu, spawn từ map | Hành xử đúng trên 2 máy; báo trước đòn ≥0,7s | P2-05, P2-01 | Vũ | 3 | 08/12 | 10/12 | GD |
| P2-07 | P2 | EXP | Vũ | Rig + animation nhân vật chính (bay, đi, đánh) | Anim chạy trong scene Dev | P1-11 | Vũ | 1.5 | 11/12 | 12/12 | Art |
| P2-08 | P2 | STA | Vinh | Giờ giao thương v1: danh mục ≤4 loại, 1 cửa neo, tàu tới + kiên nhẫn, mang thùng (1 người cầm) giao đúng tàu, nhận Tally | 1 vòng 2 người; 2 người cùng thùng → đúng 1 người cầm | P1-13, P0-08, D1-04 | Vinh | 4 | 06/12 | 09/12 | GD |
| P2-09 | P2 | STA | Vinh | Xưởng: `RecipeDef` + hàng đợi qua đêm | Đặt chế → sang ngày có đồ trong kho | P1-03, D1-05 | Vinh | 2 | 10/12 | 11/12 | GD |
| P2-10 | P2 | PRE | Hoàng | Màn Bản đồ chuyến, Ngạt, Bị kéo về, Kết chuyến | Theo wireframe; nhận event Oxygen/Expedition | D1-02 | Hoàng | 2 | 06/12 | 07/12 | — |
| P2-11 | P2 | PRE | Hoàng | Import dữ liệu thật + validator (thiếu ID, ID trùng, tham chiếu hỏng) | Mọi task dùng ID thật thay số tạm | D1-05 | Hoàng | 1.5 | 08/12 | 09/12 | GD |
| P2-12 | P2 | EXP | Vũ | Hazard (báo trước ≥1s): mảnh trôi −8, cáp chập −12 Oxy | 2 người đều thấy báo trước; trừ Oxy đúng người | P2-01, P1-08 | Hoàng | 1.5 | 10/12 | 11/12 | GD |
| P2-13 | P2 | PRE | Hoàng | `AudioService` (Master/Music/SFX/UI qua AudioMixer) + SFX theo EventBus | 4 thanh âm lượng điều khiển đúng bus | P1-16 | Hoàng | 1 | 12/12 | 12/12 | Ngay |
| D2-01 | P2 | DES | Huy | Thiết kế ≥8 mảnh Vành đai ngoài + lắp prefab mảnh trong Unity theo format P2-01 | Generator ghép được 8 mảnh của Huy | P2-01 | Huy | 3 | 08/12 | 11/12 | — |
| D2-02 | P2 | DES | Huy | Spec Ngạt/chia Oxy/kéo về + shop + nâng cấp 2 trục + động cơ gđ1 | Đủ để code P3 | D0-02 | Huy | 1 | 06/12 | 06/12 | — |
| D2-03 | P2 | DES | Huy | Duyệt model 3D + đưa reference animation cho Vũ | Vũ có ref trước 11/12 | P1-11 | Huy | 0.5 | 07/12 | 07/12 | — |
| D2-05 | P2 | DES | Huy | Danh sách VFX MVP (trúng đòn, choáng, khai thác, Oxy thấp, bơm khí, giao hàng…) + tìm pack VFX phù hợp style, license dùng thương mại; order từng owner gắn | Bảng VFX: sự kiện → asset → owner gắn; pack đã import vào repo asset | A0-02 | Huy | 0.5 | 07/12 | 07/12 | — |
| D2-06 | P2 | DES | Huy | Rà tab AUDIO ASSETS LIST thành danh sách sound MVP (SFX theo sự kiện + nhạc 3 cảnh) giao Vinh + Hoàng | Danh sách có sự kiện, mảng, người tìm | — | Huy | 0.5 | 12/12 | 12/12 | — |
| D2-04 | P2 | DES | Huy | Playtest B2 + cân bằng trên nhánh `config` | Bug + chỉnh số ghi lại | P2-04 | Huy | 0.5 | 12/12 | 12/12 | — |
| A2-01 | P2 | ART | Thơ | Map ngoài trời Vành đai ngoài: nền, mảnh vỡ, điểm khai thác, hazard — cắt theo lưới mảnh | Xuất theo spec, đẩy repo asset | A1-02 | Thơ | 4 | 05/12 | 09/12 | — |
| A2-02 | P2 | ART | Thơ | Driftlife 2 loài đầu bản 2D (4 hướng; Vũ dựng 3D từ bản này nếu loài đó chọn 3D) | Theo spec D1-03 | D1-03 | Thơ | 2 | 10/12 | 11/12 | — |
| A2-03 | P2 | ART | Thơ | Thương nhân Tikko + tàu Loose Bolt / The Veyl | Xuất theo spec | A0-02 | Thơ | 1 | 12/12 | 12/12 | — |
| P3-01 | P3 | PLT | Hào | Vòng ngày đủ: xác nhận ra chuyến/ngủ 30s chung, treo máy tự sang ngày, cửa khí chặn chuyến 3, cuối ngày | Kịch bản treo máy 30s pass 2 người | P1-03 | Hào | 1.5 | 13/12 | 14/12 | GD |
| P3-03 | P3 | STA | Vinh | Shop đặt hàng (trừ Tally, hàng tới sáng hôm sau, mua 1 lần cho nhóm) | Đặt hôm nay → sáng mai có trong kho, 2 máy giống nhau | P1-13, P1-03 | Hào | 2 | 16/12 | 17/12 | GD |
| P3-04 | P3 | PLT | Hào | Tài liệu kiến trúc + netcode bản 1 (theo yêu cầu họp) | `Docs/` có sơ đồ module + luồng request/apply | P0-04 | Hào | 1 | 18/12 | 18/12 | — |
| P3-05 | P3 | PLT | Hào | Feature freeze + build B3 | Kịch bản B3 pass | Tất cả P3 | Hào | 0.5 | 20/12 | 20/12 | — |
| P3-06 | P3 | EXP | Vũ | Súng lưới bắt sống (≤3 con), lấy mẫu loài lớn | Vào túi; người tới host trước thắng | P2-06 | Vũ | 1.5 | 13/12 | 14/12 | GD |
| P3-07 | P3 | EXP | Vũ | Ngạt 12s + chia Oxy (sai số <1) + kéo về giữ 1 món ghim | 2 người cứu nhau được; kéo về giữ đúng 1 món | P1-08, P1-06 | Vũ | 2 | 15/12 | 16/12 | GD |
| P3-08 | P3 | EXP | Vũ | `IPlayerStats` nhận modifier nâng cấp | Nâng cấp đổi stat đúng người | P1-08 | Vũ | 0.5 | 17/12 | 17/12 | — |
| P3-09 | P3 | EXP | Vũ | Dựng 3D (AI) + rig + anim 2 loài Driftlife đầu từ bản 2D của Thơ | Model + anim chạy trong scene Dev | A2-02, P2-07 | Vũ | 1.5 | 18/12 | 19/12 | Art |
| P3-10 | P3 | STA | Vinh | Giao thương v2: 3 cửa neo, bơm khí (giữ E 1s: +25% kiên nhẫn, +10 Tally), tip, Tổng kết, Hạng cảng | Một vòng 230–300s; giao sai cửa → từ chối | P2-08 | Vinh | 3 | 13/12 | 15/12 | GD |
| P3-11 | P3 | STA | Vinh | Nâng cấp 2 trục (Bình Oxy, Túi), mua 1 lần cho nhóm | Bình bậc 0/1 = 330/450 Oxy | P3-08, P1-13 | Vinh | 1.5 | 16/12 | 17/12 | GD |
| P3-12 | P3 | PRE | Hoàng | HUD đồng đội: tên, Oxy đồng đội, mũi tên chỉ hướng khi ngoài màn hình | 2 người thấy Oxy của nhau, trễ <0,5s | P1-08, P1-15 | Vinh | 1 | 18/12 | 18/12 | — |
| P3-13 | P3 | PRE | Hoàng | Camera trong trạm: tường/vật che người chơi thì mờ đi | Không lúc nào mất dấu nhân vật sau tường | P0-13, P1-12 | Hoàng | 1 | 13/12 | 13/12 | — |
| P3-14 | P3 | PRE | Hoàng | Màn Cuối ngày | Theo wireframe; hiện đúng trên 2 máy | P1-03 | Hoàng | 1 | 15/12 | 15/12 | — |
| P3-15 | P3 | PRE | Hoàng | Rà UI: 720p, chuột/phím/tay cầm, Esc mọi màn | Đi hết mọi màn bằng 3 thiết bị | P1-16 | Hoàng | 1.5 | 16/12 | 17/12 | — |
| P3-16 | P3 | PRE | Hoàng | Kịch bản test B3 + hồi quy 2 người (gồm late join, rớt mạng) | Kịch bản chạy được trong 30 phút | P1-17 | Hoàng | 1 | 18/12 | 18/12 | — |
| P3-17 | P3 | PRE | Hoàng | Danh sách thế giới: chọn / xoá (có xác nhận) / đổi tên | Xoá thế giới không đụng thế giới khác | P1-02, P0-15 | Hoàng | 0.5 | 14/12 | 14/12 | — |
| P3-18 | P3 | PRE | Hoàng | Sound phần Hoàng: nhạc nền (menu, trạm, chuyến, giao thương) + SFX chuyến/UI theo danh sách của Huy; ghi nguồn + license vào `Docs/licenses.md` | Mỗi sự kiện trong danh sách có tiếng; license cho phép bán thương mại | D2-06, P2-13 | Hoàng | 1.5 | 19/12 | 20/12 | — |
| P3-19 | P3 | STA | Vinh | Sound phần Vinh: SFX trạm, xưởng, giao thương (tàu tới, giao thùng, bơm khí, tip) theo danh sách của Huy; ghi license | Như P3-18 | D2-06, P2-13 | Vinh | 1.5 | 19/12 | 20/12 | — |
| D3-01 | P3 | DES | Huy | Kịch bản chương 1 (9 mốc, thoại ≤80 ký tự, radio) dạng dữ liệu cho P5 | Import được khi P5 bắt đầu | D0-02 | Huy | 3 | 13/12 | 16/12 | — |
| D3-02 | P3 | DES | Huy | Layout map nhánh Station Cinder (dựng tay) | Đủ để dựng ở P5 | D0-02 | Huy | 1 | 17/12 | 18/12 | — |
| D3-03 | P3 | DES | Huy | Playtest B3 + cân bằng | Bug + chỉnh số ghi lại | P3-05 | Huy | 0.5 | 20/12 | 20/12 | — |
| A3-01 | P3 | ART | Thơ | Icon item MVP (30–40 món theo D0-01) | Đủ icon cho mọi item tháng 1 | D0-01 | Thơ | 1 | 13/12 | 13/12 | — |
| A3-02 | P3 | ART | Thơ | UI skin: HUD Oxy/túi, khung panel, nút, icon Tally/Oxy, logo, màn loading | Hoàng/Huy ráp được | D1-02 | Thơ | 1.5 | 14/12 | 15/12 | — |
| A3-03 | P3 | ART | Thơ | (Nếu kịp) 2 loài còn lại + Scrapmaw, Ingrid, Linh | Giao trước 15/12; không kịp → dùng placeholder tháng 2 | D1-03 | Thơ | 2 | 14/12 | 15/12 | Cắt được |
| P4-01 | P4 | PRE | Hoàng | Import art trạm + ngoài trời theo pipeline (sprite, material URP, billboard) | Không còn greybox ở trạm + Vành đai ngoài | A1-02, A2-01 | Hoàng | 2 | 21/12 | 22/12 | Art |
| P4-02 | P4 | DES | Huy | Lắp lại mảnh map + trạm với art; tích hợp UI skin; ánh sáng + post-process (URP Volume) cho trạm và ngoài | Mảnh và trạm có art; UI theo skin; ánh sáng thống nhất 2 cảnh | P4-01, A3-02 | Huy | 3 | 21/12 | 23/12 | Art |
| P4-03 | P4 | EXP | Vũ | Nhân vật chính + 2 Driftlife 3D + anim vào prefab mạng | Prefab dùng model thật, anim đồng bộ trên 2 máy | P2-07, P3-09 | Vũ | 2 | 21/12 | 22/12 | Art |
| P4-04 | P4 | STA | Vinh | Gắn art vào trạm: khoang hỏng/đã sửa, quầy, cửa neo, tàu | Đổi trạng thái khoang đổi hình | A1-02, A2-03 | Vinh | 2 | 21/12 | 22/12 | Art |
| P4-05 | P4 | PLT | Hào | Rà kiến trúc + refactor theo review (module, Shared, request) | Không có tham chiếu chéo feature; test pass | P3-05 | Hào | 2 | 21/12 | 22/12 | — |
| P4-09 | P4 | — | Owner từng mảng | Gắn VFX theo bảng order của Huy (D2-05), mỗi owner gắn phần mảng mình qua `PoolService` | Mọi dòng trong bảng VFX có hiệu ứng, chạy trên 2 máy | D2-05 | Cả team | 1 | 23/12 | 30/12 | Không chặn MVP |
| P4-06 | P4 | QA | Hoàng | Bug bash 2 người (cả team) + chấm MVP checklist phần thuộc phạm vi | Checklist có trạng thái từng dòng | P4-01..05 | Cả team | 2 | 23/12 | 24/12 | — |
| P4-07 | P4 | QA | Hoàng | Sửa bug chặn (mỗi owner nhận bug mảng mình) | 0 bug chặn | P4-06 | Cả team | 5 | 25/12 | 30/12 | — |
| P4-08 | P4 | PLT | Hào | Tag `mvp-1.0` + build bảo vệ + gửi GVHD | Build chạy trên 2 máy sạch | P4-07 | Hào | 0.5 | 31/12 | 31/12 | — |
| P5-01 | P5 | PRE | Hoàng | Mốc truyện (điều kiện → flag → mở nội dung) + Sổ tay (9 dòng + mục tiêu), tiến trình chung | Đạt điều kiện → mốc mở 2 máy; còn sau load | D3-01 | Hoàng | 3 | 01/01 | 03/01 | — |
| P5-02 | P5 | PRE | Hoàng | Hộp thoại (≤80 ký tự, bỏ qua được) + radio + cảnh tỉnh dậy; tutorial gợi ý không chặn | Thoại đúng số người; mọi máy thấy cảnh | P5-01 | Hoàng | 4 | 04/01 | 08/01 | — |
| P5-03 | P5 | EXP | Vũ | Map nhánh Station Cinder (tiến trình chung, vớt người + manh mối theo host) | Đi hết một mình được; lần 2 mở đúng trạng thái | D3-02, P2-01 | Hào | 3 | 01/01 | 03/01 | — |
| P5-04 | P5 | STA | Vinh | Người vớt (Ingrid → xưởng, Linh → liên lạc/cảng/shop) mở tính năng; khu hồi sức | Vớt xong mở đúng tính năng, 2 máy giống nhau | P5-03, P5-01 | Hào | 2 | 04/01 | 05/01 | — |
| P5-05 | P5 | PLT | Hào | Dedicated: build Linux headless (`StartServer`, không có người local) + đưa lên VPS (~6–12 USD/tháng) + script chạy/khởi động lại | 2 client vào từ 2 mạng khác nhau | P1-01 | Hào | 3 | 06/01 | 08/01 | — |
| P5-09 | P5 | STA | Vinh | Động cơ chính giai đoạn 1 (sửa bằng vật liệu → event mốc truyện) | Sửa xong phát event; còn sau load | P1-14, P5-01 | Vinh | 1 | 06/01 | 06/01 | GD |
| P5-10 | P5 | EXP | Vũ | Dụng cụ khai thác trên đai (khoan, nạy, hạ) gắn vào điểm khai thác | Mỗi dụng cụ khai thác đúng loại điểm | P1-09 | Vinh | 1.5 | 07/01 | 08/01 | GD |
| P5-06 | P5 | EXP | Vũ | 2 loài còn lại + boss Scrapmaw (máu 1+0,6(N−1)), báo trước đòn; Vũ dựng 3D (AI) + anim boss | Scrapmaw một mình 1–2 phút; 2 người không lệch | P2-06 | Vũ | 6 | 01/01 | 07/01 | GD · Art |
| P5-07 | P5 | STA | Vinh | Nâng cấp đủ 5 trục + cửa neo thứ 4 (Cảng cấp 2 + Hạng 4) + số tàu theo số người | Theo checklist 85, 136 | P3-11, P3-10 | Vinh | 4 | 01/01 | 05/01 | GD |
| P5-08 | P5 | PLT | Hào | Build B5 (trọn chương 1) | Chơi trọn ch.1 1 và 2 người | Tất cả P5 | Hào | 0.5 | 13/01 | 13/01 | — |
| P6-01 | P6 | EXP | Vũ | 3–4 người: combat, Driftlife, boss, hệ số k(N) | MPPM 4 người không lệch | P5-06 | Vũ | 3 | 14/01 | 16/01 | — |
| P6-02 | P6 | STA | Vinh | 3–4 người giao thương + 100 lần giao thùng rớt mạng tổng không đổi | Test pass | P5-07 | Vinh | 3 | 14/01 | 16/01 | — |
| P6-03 | P6 | STA | Vinh | Nghiên cứu + Khoang sinh quyển (nuôi Driftlife) | Theo GDD trạm | P5-04 | Vinh | 5 | 17/01 | 22/01 | GD |
| P6-04 | P6 | PRE | Hoàng | Lobby 4 slot + accessibility (mù màu, HUD 200%, chữ 12px 720p) | Theo checklist 103–106 | P1-16 | Hoàng | 3 | 14/01 | 16/01 | — |
| P6-08 | P6 | PRE | Hoàng | Đa ngôn ngữ: mọi chữ UI/thoại qua bảng chuỗi (Unity Localization), thêm tiếng Anh cho Steam | Đổi ngôn ngữ trong Cài đặt, không còn chữ cứng trong prefab | P6-04 | Hoàng | 2 | 17/01 | 18/01 | — |
| P6-09 | P6 | EXP | Vũ | 4 preset ngoại hình người chơi (phân biệt được, theo checklist 36/43) | 4 người chọn 4 preset khác nhau, thấy đúng trên mọi máy | P6-01 | Vũ | 2 | 17/01 | 18/01 | GD (Vũ làm biến thể 3D) |
| P6-05 | P6 | EXP | Vũ | Vành đai trong (dải 2) — mảnh + Driftlife mới | Theo GDD map | P2-01 | Hào | 4 | 14/01 | 18/01 | GD |
| P6-06 | P6 | PLT | Hào | Spike Steam AppID 480: transport cộng đồng với NGO 2.x hoặc ước lượng tự viết | Báo cáo khả thi; quyết giữ NGO hay FishNet | P0-02 | Hào | 2 | 19/01 | 20/01 | Ngay |
| P6-07 | P6 | QA | Hoàng | Playtest (người ngoài) + tài liệu kỹ thuật bản 2 + build B6 | Báo cáo playtest | Tất cả P6 | Cả team | 3 | 23/01 | 25/01 | — |

---

## 6. Ba ngày đầu

| Người | D1 Thứ Năm 26/11 | D2 Thứ Sáu 27/11 | D3 Thứ Bảy 28/11 |
|---|---|---|---|
| Hào | P0-05 git flow + AGENTS.md, P0-01, bắt đầu P0-02 | Xong P0-02, P0-04 hợp đồng (review cả team chiều), bắt đầu P0-03 | Xong P0-03, ráp B0 (P0-17); họp chốt ý tưởng |
| Vũ | P0-06 motor + Hãm (dùng `NoventInput` từ trưa) | P0-07 player mạng | P0-09 thử 3D + shader cho buổi chốt style |
| Vinh | P0-10 túi/Khối + test | P0-11 kho chung + save | P0-08 Interaction + nhặt host phân xử |
| Hoàng | P0-12 input (xong sáng), P0-13 camera | P0-14 spec art cho Thơ, P0-15 UI kit + menu host/join | P0-16 ContentDatabase + importer |
| Huy | D0-01 list asset MVP + ưu tiên cho Thơ | D0-02 chuẩn bị buổi chốt; duyệt concept | Chủ trì chốt ý tưởng; bắt đầu D0-03 |
| Thơ | A0-01 concept | A0-01 giao concept + vùng UI | A0-02 chốt style |

## 7. Rủi ro lớn nhất
0. **Sức người**: Must vừa khít, Vũ quá tải nhẹ (§3). Check-in mỗi cuối tuần so tiến độ với Sheet; trễ >3 ngày thì cắt theo thứ tự ở §3.
1. **Art một người, hạn 15/12, ~16,5 ngày việc trong ~17 ngày làm**: thứ tự ưu tiên theo họp (map trong → map ngoài → nhân vật → item → UI);
   A3-03 cắt được; thiếu thì dùng placeholder/asset mua bù (họp đã chấp nhận).
2. **Code viết như chơi đơn rồi lệch khi có 2 người** → request/apply từ P0; PR nào cũng có bước MPPM 2 người + owner duyệt.
3. **Team chưa thống nhất "mạng ngay từ đầu"** (họp 23/11 chốt ngược lại) → Hào trình lý do ở check-in 28/11 cùng B0.
4. **Owner thành nút thắt review** (Hào giữ Core/Shared) → hợp đồng xong D2; owner duyệt trong 24 giờ, quá hạn thì người làm nhắc.
5. **API NGO/MPS đổi nhanh, AI trộn bản** → ghim version, link docs đúng bản trong knowledge.
6. **Dữ liệu GD chưa có** (`xuat-data.mjs` chưa làm) → số tạm trong SO; P2-11 thay dữ liệu thật.
7. **Transport Steam cho NGO 2.x chưa được xác nhận** → spike P6-06; đường lùi FishNet.

## Verification (mỗi build)
Compile 0 lỗi (`refresh_unity` + `read_console`), EditMode test pass (`run_tests`), kịch bản build của Hoàng với MPPM host + 1 client
+ 1 lần late join; build thật trên 2 máy cho B0, B1, B3, MVP 1.0.
