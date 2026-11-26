# Biên bản họp nhóm — The Novent

Cách dùng: mỗi buổi họp là một mục `## YYYY-MM-DD — <chủ đề>`, **thêm vào cuối file** (cũ trên, mới dưới).
Khung mỗi mục: Tóm tắt · Quyết định · Phân công · Câu hỏi mở · Thông tin chính · Lệch với memory.
`(tạm)` = chưa chốt. Buổi sau đổi quyết định của buổi trước thì gạch dòng cũ: `~~cũ~~ → mới (họp <ngày>)`.
Quyết định ở đây là biên bản; thứ code phải theo thì chuyển thành entry trong `knowledge.md` (qua PR).

---

## 2026-11-23 — Kickoff: giảng viên, MVP, quy trình, hướng art

Gộp từ 2 bản ghi tự động (tiếng Anh + tiếng Việt) của cùng buổi họp.

### Tóm tắt
Chọn giảng viên hướng dẫn, chốt phạm vi MVP, quy trình làm việc/git và hướng art. MVP là vòng lặp
**thám hiểm/thu thập + trade hall**, làm trước dạng **gray box chơi đơn trên máy**; đủ tính năng ~20/12,
MVP cuối tháng 12. Game góc isometric 2.5D: team art vẽ asset 2D tĩnh, Vũ dựng nhân vật chính và một
số quái bằng 3D; phong cách cụ thể chờ concept art. Bắt đầu làm từ 24/11, check-in mỗi cuối tuần.

Agenda: chọn GVHD ✔ · duyệt GDD ✔ · chốt scope/MVP ✔ · quy trình làm việc ✔ · quy trình thực hiện dự án ✔ ·
nội dung game cần làm · duyệt concept art.

### Quyết định

**Giảng viên hướng dẫn**
- Nhờ lần lượt từng người (mỗi lần chỉ nhờ được một): thầy Đông → cô Chi → thầy Thọ; không ai nhận thì để trường phân công.
- (tạm) Nếu nhờ cô Chi: đề xuất 2 người lên trường ngày thường, 3 người cuối tuần; ngày thường làm online hoặc offline trong thành phố.

**Phạm vi MVP & tiến độ**
- MVP = hoàn thiện 2 luồng: thám hiểm/thu thập và trade hall. Cốt truyện tuyến tính làm sau như bản mở rộng.
- Nuôi sinh vật vẫn giữ trong GDD nhưng **không** thuộc MVP.
- Demo chơi đơn trên máy trước; **multiplayer làm sau khi refactor**.
- Làm theo độ ưu tiên cao → thấp, tới đâu hay tới đó. Ai thấy phần nào quá khó thì báo ngay để cắt scope.
- Mốc: xong tính năng ~**20/12** → ~10 ngày tích hợp + sửa lỗi → **MVP cuối tháng 12**. Hiệu ứng làm sau.
- (tạm) Giai đoạn 1: toàn bộ game dạng gray box, đủ tính năng, chưa có art và mạng; viết tài liệu kiến trúc
  song song, sau đó cho AI refactor code theo tài liệu. Asset/animation placeholder đưa vào sau gray box.
- (tạm) Quy trình: waterfall kết hợp Scrum.

**Phân vai & duyệt**
- Mỗi mảng có 1 người phụ trách làm phần lớn việc và là người duyệt chính; người khác vẫn góp ý.
- Các cụm việc: (1) code; (2) art, GDD, hiệu ứng/animation; (3) test, tài liệu, build, business, yêu cầu môn học.
- Mọi người đều code (không đều nhau), chia nhiều commit nhỏ. Một **code owner** quyết kiến trúc và hướng kỹ thuật,
  người khác làm theo; code chia nhiều phần, bắt đầu từ base game.
- Trưởng nhóm lo kế hoạch + theo dõi tiến độ (có thể là Hào, chưa xác nhận). Bảng theo dõi ban đầu làm sơ sài,
  sau làm gọn để trình GVHD.

**Test**
- Ai code xong tính năng thì tự test logic bằng AI agent; test tay phần hình, âm thanh, giật lag, lỗi animation.
- Một file test case chung ghi loại test và bug; ai cũng báo bug, người phụ trách test chịu trách nhiệm.
- (tạm) Hoàng phụ trách test và tìm hiểu phương pháp test.

**Git & repo**
- Mọi commit/push/rebase làm qua AI, không commit tay.
- Mỗi tính năng một nhánh → gộp vào `dev` → ổn mới lên `main` (giả định). Ưu tiên rebase thay merge; xoá nhánh
  tính năng sau khi gộp. Dùng worktree để test.
- Game designer có nhánh `config` tách từ `dev` để cân bằng game, liên tục rebase lên `dev`.
- Art có repo riêng; thành viên art chỉ vào repo asset, không vào repo chính. Asset trả phí chỉ để ở repo asset.
- `CLAUDE.md` chung giữ quy ước team cho tool AI, chỉ Hào sửa; chỉnh riêng thì dùng file local trên máy mình.
- Repo dự kiến: thư mục Unity và backend tách riêng trong The-Novent, cạnh docs/mockup (xem "Lệch với memory").

**Art**
- Toàn bộ asset là **2D tĩnh** do team art vẽ (map, nhân vật, vũ khí, item, UI, splash, loading); map là ảnh tĩnh,
  nhân vật vẽ 4 hướng (trước, sau, trái, phải). Team art không làm animation, không đụng Unity.
- Ngoại lệ: nhân vật chính và một số quái làm **3D** do Vũ dựng theo bản vẽ 2D (không ảnh hưởng đồ án team art).
- Vũ làm animation (rigging) + VFX, Huy đưa reference và duyệt. Huy thiết kế luồng UI/UX và map, lắp map và tích hợp UI
  vào Unity. Dev quyết các việc kỹ thuật.
- Camera isometric 2.5D; mọi vật thể gameplay vẽ isometric (trừ UI/menu) — chốt sớm để khỏi làm lại. Không pixel art;
  (tạm) có thể không có nhảy.
- (tạm) Phong cách: tối giản, không loè loẹt. Ref: Ori, Death's Door, Hades (map top-down), Octopath Traveler (chỉ lấy
  bố cục/ánh sáng 2D trên khối 3D). Team art quyết chi tiết và màu.
- Chốt vài phương án art + dự phòng rồi thử. Dự phòng nếu 3D không ổn: sprite 2D lật frame kiểu Octopath trên môi trường 3D.
- Bộ concept art để khoá style: nhân vật, background/môi trường, animation, enemy, item, tàu. Phác bằng gen AI rồi vẽ lại tay.
- (tạm) Nhân vật chính đổi ngoại hình sang nét châu Á (bản tiếng Anh ghi "Đông Á hoặc da trắng").

**Nhịp làm việc**
- Bắt đầu 24/11 khi có checklist chia việc; check-in mỗi cuối tuần, buổi đầu là cuối tuần này.

### Phân công
| Ai | Việc | Hạn / trạng thái |
|---|---|---|
| Trưởng nhóm (có thể Hào) | Gửi checklist chia việc | tối 24/11 |
| Hào | Bảng theo dõi tiến độ bản nháp → hoàn thiện thành template | |
| Hào | Viết + commit `CLAUDE.md` chung (gồm quy tắc git) | ✔ 25/11 (nhánh `chore/ai-standard`, chưa merge) |
| Hào | Thêm thành viên làm collaborator repo | |
| Huy (có thể) | Thêm cột độ ưu tiên cho danh sách item trong GDD | |
| Huy (có thể) | Xác định asset bắt buộc cho MVP, cắt phần art không kịp | |
| Huy | Gửi ghi chú của mình cho Hào | |
| Vũ | Chia sẻ công cụ rigging thay Spine | |
| Vũ | Khi art xong: chọn nhân vật/quái làm 3D (có thể nhân vật chính + boss) | |
| Vũ | Thử model 3D + shader để chốt phương án art và dự phòng | |
| Team art | Tìm hiểu và làm concept art | |
| Ahy18, Hoàng, Vinh, Vũ | Chấp nhận lời mời vào repo | |
| (chưa giao) | Tìm liên hệ + đánh giá thầy Đông, nhờ thầy hướng dẫn | 24/11 |
| (chưa giao) | Gửi bản demo HTML cho nhóm | |

### Câu hỏi mở
- Liên hệ thầy Đông thế nào, thầy đang hướng dẫn nhóm nào? Rủi ro phương án sau: cô Chi yêu cầu lên lab; thầy Thọ có thể đòi SRS/SDS.
- **Ai là code owner?** Kiến trúc nào; code sẵn sàng multiplayer từ đầu hay viết lại sau?
- Ai phụ trách tài liệu + build (có thể Vinh)? Có nên để một người dựng framework test cho cả nhóm?
- Cốt truyện và cutscene làm lúc nào? Quan trọng với trường nhưng đang ngoài MVP.
- Scope MVP có quá nặng? Cần art placeholder hay asset có sẵn/mua?
- Công cụ rigging nào thay Spine?
- Phần nào làm 3D: vật thể như cây làm 3D (Vũ) hay giữ 2D để team art có việc cho đồ án (đang nghiêng 2D)? Ai hỗ trợ Vũ?
- Đồ án của team art có được vẽ isometric không?
- Team art kịp vẽ hết asset MVP trước deadline của họ không? Thứ tự nhờ: 2 map (trong nhà, ngoài trời) → nhân vật → item.
  Team art xem cần cắt gì; có thể mua asset bù.
- Sinh vật concept tạo bằng AI có quá phức tạp để làm, kể cả bản 2D? Concept art mất bao lâu?
- Mọi người đã rõ giai đoạn 1 gồm những gì chưa? Có việc cần chốt vào thứ Tư (chưa rõ việc gì).

### Thông tin chính
- The Novent: action RPG phiêu lưu + mô phỏng quản lý; 2.5D, môi trường 3D Unity, asset 2D góc 45°.
- GDD v0.5.0 (20/11/2026) trên Google Sheet: ~61 asset, ~150 item, có checklist MVP; MVP ước 30–40 item. Tab Ref có liên hệ
  và link Figma, Git, Drive. Figma "Wireframe - The Novent" là prototype gần đủ màn hình (link trong chat Meet).
- Gameplay: tỉnh dậy ở trạm Ember hư hỏng sau khi tàu nổ; mục tiêu đầu là sửa docking bay. Mỗi ngày 2 chuyến thám hiểm
  (khai thác, bắt "deep life") rồi một ca trade hall có giờ để làm đơn hàng tàu. Kiếm Tally, tiêu vào oxy, nâng túi, trang bị.
  Oxy vừa là máu vừa giới hạn độ sâu; chết mất hết đồ; bản đồ sâu dần trái → phải. Co-op tối đa 4/trạm; bản đồ hải trình
  nối Ember, Noor, Beacon; cốt truyện 5 chương.
- Còn ~25 ngày làm việc tới hạn tiền đồ án; trường yêu cầu có số commit, nhưng khi bảo vệ GV chỉ hỏi mỗi người có code không —
  commit không cần đều.
- Đơn nhờ GVHD đã gửi, chưa duyệt. Thầy Đông làm ngành game. Trường phân công thì trưởng bộ môn SE chọn trong nhóm chuyên ngành game.
- Chi phí: Spine ~300+ USD/tháng (không kham nổi); Meshy AI ~200–300k VND/tháng (nhóm chấp nhận được); dự án nhóm khác nghe nói tốn 50–60 triệu.
- Dựng nhân vật 3D khả thi — thành viên đã từng làm model tương tự.
- Vinh dùng Codex (đọc `AGENTS.md`, không đọc `CLAUDE.md`).
- Repo GitHub "The-Novent" (tài khoản Hào) lúc họp mới có `Docs/` (GDD, teardown game tham khảo).

### Lệch với memory (cần xử lý)
- **Multiplayer**: họp chốt MVP chơi đơn gray box, multiplayer sau refactor ↔ `knowledge.md` § Chờ chốt và nháp
  `plans/mvp-kickoff.md` đều đặt co-op 2 người vào MVP, chọn netcode trước. Cần chốt lại với team.
- **Mốc**: họp ghi MVP cuối tháng 12 (tính năng ~20/12) ↔ nháp `plans/mvp-kickoff.md` đề xuất 25/12.
- **Vị trí Unity project**: họp dự kiến thư mục con (cạnh backend) ↔ thực tế 25/11 đã tạo ở **gốc repo**. Cần báo team.
- **Git flow**: họp dự kiến nhánh tính năng → `dev` → `main` ↔ `CLAUDE.md` hiện ghi nhánh từ `main`, PR thẳng vào `main`.
- **Codex**: repo chưa có `AGENTS.md` → Vinh không nhận quy ước chung. Nên thêm `AGENTS.md` trỏ về `CLAUDE.md`.
