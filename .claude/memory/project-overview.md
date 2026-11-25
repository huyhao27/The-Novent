# The Novent — tổng quan project

> Nguồn: `Docs/Game Design Document - The Novent.xlsx` (tab chính = **v0.5.0, 20/11/2026**),
> `Docs/The Novent Design Document.docx` (bản tiếng Anh: Controls + Game Elements), teardown Dave the Diver.
> Bản GDD đủ (khung game, 10 GDD hệ thống, sim, validator) ở repo **`huycv18/DATN-SP27`**, thư mục
> `projects/game-moi/gdd` (nhánh `claude-cty`, `claude-g15`). **Chưa đọc**, nhất là `gdd/he-thong/ky-thuat.md`
> (schema dữ liệu, yêu cầu đồng bộ co-op).
> ⚠️ Các tab có tiền tố `v0.1 …` là concept cũ "Space Salvage" (jetpack, quán ăn, cảnh sát), **đã bỏ** từ v0.3.
> Không dùng số liệu ở đó. Đọc lần đầu 2026-11-23.

## 1. Game là gì
| Mục | Nội dung |
|---|---|
| Tên | The Novent (DATN SP27) |
| Thể loại | Hành động phiêu lưu + mô phỏng quản lý trạm, khung Dave the Diver (ban ngày ra ngoài, cuối ngày quản lý) |
| Góc nhìn | **2.5D: cảnh 3D trong Unity, mọi asset là 2D, camera top-down 45°** ở mọi nơi (Lead chốt 2026-11-23) |
| Nền tảng | PC, **chơi đơn hoặc co-op online 1–4 người** (MVP: 2 người). Không PvP, không chơi chung một máy |
| Bối cảnh | Tàu mẹ Cradle nổ; người chơi tỉnh dậy trên Station Ember trôi giữa bãi vỡ The Wake |
| Tiền | **Tally** là tiền duy nhất. Không XP, không tiền thứ hai |
| Chỉ số sống | **Oxy = thời gian + máu**, không có thanh máu riêng |
| Nhóm | 1 Game Designer, 4 Dev, 1 Artist 2D. Tab Ref: Hào, Vinh, Hoàng, Huy (Huy = GD/Lead) |
| Mốc thời gian | **Chưa có, chờ Lead** |

## 2. Vòng chơi
- **Một ngày có 5 pha**: Chuẩn bị → Chuyến 1 → Chuyến 2 → Giờ giao thương → Cuối ngày (nâng cấp, nghiên cứu, nói chuyện, ngủ = sang ngày + lưu). Tối đa 2 chuyến/ngày, Skiff tính 1 chuyến. Ngày không có đồng hồ.
- **Ra ngoài**: bay 8 hướng, quán tính nhẹ, có Hãm. Nhặt/khai thác điểm tài nguyên, combat + **choáng → bắt sống bằng lưới** Driftlife (cỡ S/M bắt sống có giá hơn, cỡ L chỉ lấy mẫu). Map nhánh dựng tay chứa người để vớt, manh mối, boss. Về bằng cửa khí hoặc Recall Beacon (giữ phím, bị đánh thì hủy).
- **Hết Oxy** → Ngạt 12 giây (đồng đội chia Oxy để cứu) → bị kéo về, **chỉ giữ 1 món ghim**, không mất Tally hay nâng cấp.
- **Map tổng** 3 dải (Vành đai ngoài, Vành đai trong, Vùng trung tâm), **mỗi chuyến ghép lại từ mảnh dựng tay**. Tài nguyên và Driftlife sinh lại.
- **Về trạm**: đi bộ 8 hướng trên mặt bằng Ember. Sửa khoang bằng vật liệu (mỗi khoang đổi hình và mở một chức năng), xưởng chế (hàng đợi qua đêm), kho chung, nuôi Driftlife (sau MVP).
- **Giờ giao thương** (kiểu bán sushi của Dave): lập danh mục ≤4 loại hàng → tàu The Veyl cập 3 cửa neo, mỗi tàu gọi 1 thùng → mang thùng từ quầy tới đúng tàu trước khi hết kiên nhẫn, bơm khí (giữ E 1 giây) → nhận giá + tip. Hạng cảng tăng theo số tàu phục vụ xong.
- **Tiến trình**: Tally → nâng cấp bộ đồ (5 trục: Bình Oxy, Túi, Bộ đẩy, Vỏ, Đai dụng cụ), dụng cụ, khoang. Shop đặt hàng, hàng tới sáng hôm sau. Đích cuối: sửa xong động cơ chính (5 giai đoạn) → rời neo.
- **Truyện**: 5 chương, một đường, không lựa chọn. Mốc truyện = danh sách điều kiện; chỉ khoá nội dung mới, không khoá vòng ngày. Chi tiết ở `story.md`.

## 3. Phạm vi MVP = trọn chương 1 (1,5–2 giờ chơi)
Vành đai ngoài · 1 xác Seed Station (Station Cinder) · 2 người vớt (Ingrid = Kỹ sư → xưởng; Linh = Thông dịch viên → khoang liên lạc, cảng, shop) · giờ giao thương 1 bến 3 cửa neo, 1 thương nhân (Tikko, tàu Loose Bolt) · 3–4 loài Driftlife + boss nhỏ Scrapmaw · lưới bắt loài nhỏ · động cơ giai đoạn 1 · co-op 2 người · 9 mốc truyện chương 1.
Tiêu chí nghiệm thu nằm ở tab **MVP Criteria Checklist** (33+ dòng, tất cả đang "chưa có build").

## 4. Tech core (ưu tiên trước). Đây là đề xuất, cần team chốt ở knowledge.md
| # | Hạng mục | Vì sao quan trọng | Đề xuất |
|---|---|---|---|
| 1 | **Netcode co-op 1–4** | Rủi ro kỹ thuật lớn nhất. Mọi hệ thống phải viết sẵn cho mạng từ đầu; để sau là phải viết lại | Host-authoritative (thế giới thuộc host). Netcode for GameObjects + Unity Relay/Lobby (vào bằng mã), hoặc Steam (Facepunch) nếu cần "mời bạn bè". Giữ chỗ 15 giây khi rớt mạng; host thoát = kết thúc phiên |
| 2 | Camera + render 2.5D | Sprite 2D trong cảnh 3D, góc 45° | URP 3D, sprite billboard/nghiêng theo camera, Cinemachine. Đơn vị `u` = 1 unit = chiều cao nhân vật |
| 3 | Bay 8 hướng + Hãm | Tiêu chí MVP: dừng lệch ≤0,5u ở >90% lần thử | Rigidbody có quán tính; chuyển động do client sở hữu (owner-authoritative) hoặc có prediction |
| 4 | Dữ liệu từ Sheet | GDD là "nguồn chuẩn duy nhất", số cân bằng do sim in ra | Import TSV/CSV → ScriptableObject (vật phẩm, công thức, Driftlife, nâng cấp, tàu, mốc truyện). Không hardcode |
| 5 | Game flow + vòng ngày | 5 pha, khoá cửa khí, xác nhận ra chuyến 30s, ngủ 30s | State machine chạy ở host, client chỉ nhận trạng thái |
| 6 | Save/Load thế giới | Tự lưu sau chuyến, sau giao thương, lúc ngủ; nhiều thế giới | JSON có `version`, host giữ. Tách phần chung (trạm, kho, Tally, truyện) và phần riêng (Oxy, túi) |
| 7 | Sinh map từ mảnh | Map tổng đổi mỗi chuyến, map nhánh cố định | Chunk prefab + điểm spawn có trọng số, seed do host phát |
| 8 | Mốc truyện + Sổ tay + thoại | Điều kiện → mốc → mở nội dung; cảnh bỏ qua được | Hệ condition/flag data-driven; thoại tự viết đơn giản (không nhánh, ≤80 ký tự) hoặc Yarn Spinner |
| 9 | Input System + đổi phím | Wireframe có màn Đổi phím, hỗ trợ tay cầm | Input System package, 1 InputActions asset, rebinding UI |
| 10 | UI 16:9, đọc được ở 720p | 30 màn trong tab WIREFRAME | uGUI + TextMeshPro, font có dấu tiếng Việt |
| — | Tracking/analytics (tab Event Tracking) | — | **Bỏ qua**: user chốt chưa làm SDK/tracking |

## 5. Hệ thống gameplay MVP
Player (Oxy, Ngạt, chia Oxy, kéo về + ghim món) · Túi (đơn vị Khối) + Đai dụng cụ · Điểm khai thác · Combat + Choáng + Bắt sống · AI Driftlife (3–4 loài) + boss Scrapmaw · Hazard · Cửa khí / Recall Beacon / Bản đồ chuyến · Trạm: đi bộ, sửa khoang, Động cơ chính · Kho chung · Xưởng + hàng đợi · Giờ giao thương (thùng, cửa neo, kiên nhẫn, bơm khí, tip) · Shop · Nâng cấp · Hạng cảng · Mốc truyện + Sổ tay + Hộp thoại + radio · Menu chính / Tạo thế giới / Tham gia thế giới · Cài đặt + Đổi phím.
Sau MVP: nghiên cứu, khoang sinh quyển (nuôi), Skiff, sự cố trong giờ giao thương (từ chương 3), chương 2–5.

## 6. Cần chuẩn bị
- **Team chốt**: phiên bản Unity (đề xuất Unity 6 LTS, cả team dùng cùng bản), giải pháp netcode, mốc thời gian, phân công hệ thống cho từng dev.
- **Repo**: tạo Unity project + `.gitattributes` Git LFS (art/audio), Force Text serialization, quy ước nhánh cho 4 dev (feature branch + PR), UnityYAMLMerge.
- **Tài khoản**: Unity Cloud (Relay/Lobby), hoặc Steamworks App ID test 480 nếu dùng Steam.
- **Tài liệu**: đọc `ky-thuat.md` và 10 GDD hệ thống ở `huycv18/DATN-SP27`. Lấy link Figma Wireframe MVP (GDD có nhắc nhưng chưa có link).
- **Art**: Artist làm theo tab Assets. Chốt pipeline sprite → 3D (pivot, pixel-per-unit, 72 px/u ở 720p).
- **Công cụ AI**: MCP for Unity (`com.coplaydev.unity-mcp`) để Claude compile và đọc Console.
