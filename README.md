# ECHO LAB: Thoát khỏi phòng thí nghiệm sắp sụp đổ

Game giải đố tương tác vật lý 3D (Physics-based Escape Room) góc nhìn thứ nhất, làm bằng **Unity 6** (URP). Đồ án môn lập trình game.

Bạn vào vai một "Echo", bản sao thời gian của tiến sĩ Hạo, bị kẹt trong phòng thí nghiệm đang sụp đổ. Mỗi phòng có một đồng hồ đếm ngược và những câu đố dựa trên vật lý.

> Thêm ảnh chụp màn hình vào thư mục `docs/` rồi chèn vào đây, ví dụ: `![Phòng 1](docs/room1.png)`

## Tính năng chính

- Cầm, xoay, ném vật thể bằng Rigidbody.
- Tấm áp suất (cộng dồn khối lượng), cần gạt (bật/tắt hoặc giữ), cửa tự động nhiều điều kiện.
- 3 kỹ năng mở khóa bằng găng tay:
  - **Mass Tuner**: lăn chuột để đổi khối lượng vật; kích thước đổi theo căn bậc ba để giữ mật độ không đổi.
  - **Time Echo**: ghi lại 10 giây di chuyển, bóng ma phát lại và đứng đè tấm áp suất.
  - **Gravity Flip**: đảo trọng lực của vật thể (người chơi không bị ảnh hưởng).
- Laser phản xạ qua gương (Raycast lặp + `Vector3.Reflect`), laser chết người.
- Câu đố thứ tự cần gạt, cổng dịch chuyển giữ nguyên vận tốc vật thể.
- 8 phòng, menu chính, giao diện tiếng Việt, âm thanh và nhạc nền tự tạo bằng code.

## Các phòng

| Phòng | Chủ đề | Cơ chế |
|---|---|---|
| 1 | Khởi động | Hộp lên tấm + gạt cần |
| 2 | Cân nặng | Mass Tuner: thu nhỏ để mang, phóng to để đè tấm |
| 3 | Bóng ma | Time Echo: bóng ma giữ tấm, giữ E ở cần gạt |
| 4 | Đảo trọng lực | Gravity Flip: hộp bay lên tấm gắn trần |
| 5 | Gương laser | Xoay gương đưa tia laser vào ô thu |
| 6 | Thứ tự cần gạt | Gạt đúng thứ tự, sai thì reset |
| 7 | Cổng dịch chuyển | Ném hộp qua cổng sang nửa phòng bên kia |
| 8 | Lõi Echo | Kết hợp tất cả, có laser chết người |

## Điều khiển

| Phím | Chức năng |
|---|---|
| W A S D | Di chuyển |
| Chuột | Nhìn quanh |
| Space | Nhảy |
| E | Nhặt / thả / tương tác (giữ E ở cần gạt giữ) |
| Giữ R + chuột | Xoay vật đang cầm |
| Chuột trái | Ném vật đang cầm |
| Lăn chuột (nhìn vào vật) | Đổi khối lượng (Mass Tuner) |
| Q | Ghi bóng ma (Time Echo) |
| F | Đảo trọng lực (Gravity Flip) |
| M | Bật/tắt nhạc nền |

## Cách chạy

1. Cài **Unity Hub** và Unity **6000.6.6f1** (hoặc bản 6 gần nhất).
2. Clone repo: `git clone <đường-dẫn-repo>`.
3. Trong Unity Hub chọn **Add project from disk**, trỏ tới thư mục vừa clone.
4. Mở scene `Assets/Scenes/MainMenu.unity` rồi bấm **Play**.

Bản build chạy sẵn (file `.exe`) nằm ở mục **Releases** của repo (nếu có).

## Cấu trúc thư mục

```
Assets/
  scripts/      Mã nguồn gameplay (GrabSystem, PressurePlate, Door, Lever, EchoRecorder, ...)
  Editor/       Công cụ dựng cảnh tự động (menu EchoLab trong Unity)
  Scenes/       MainMenu, Room1 đến Room8
  Resources/    Âm thanh (Audio2)
  Textures/     Texture sàn, tường, trần
```

## Kỹ thuật chính

- **Hướng sự kiện**: `UnityEvent`, interface `IInteractable`, nối sự kiện giữa tấm, cần gạt, cửa và các câu đố.
- **Vật lý**: Rigidbody, trigger, Raycast, tính tổng khối lượng trên tấm áp suất.
- **Ghi/phát lại bóng ma**: lưu vị trí và góc xoay mỗi khung vật lý, phát lại theo thứ tự.
- **Laser**: Raycast lặp, phản xạ theo pháp tuyến mặt gương, ô thu nhận tia.
- **Tự động hóa**: Editor script tạo cửa, tấm, giao diện, 8 phòng và Build Settings.
- **Âm thanh**: tổng hợp bằng code (sóng sin, chuông, chuỗi hợp âm), quản lý bởi lớp `Sfx`.

## Hạn chế và hướng phát triển

- Đồ họa dùng khối cơ bản, cần thêm mô hình và hiệu ứng.
- Chưa có lưu tiến trình giữa các lần chơi.
- Có thể thêm kẻ địch, phòng bí mật, bảng xếp hạng thời gian.

## Tác giả

- Họ tên: Lê Minh Sơn
- Lớp / MSV: TH29.14 / 2924115065
