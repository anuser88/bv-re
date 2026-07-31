# 🚀 SCRATCH BOOSTING VIEW
![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)
![License](https://img.shields.io/badge/license-AGPL_v3-red)

Đây là một công cụ giúp tự động tăng lượt xem cho các project trên Scratch bằng cách sử dụng Proxy. Vui lòng không lạm dụng.

## 📋 Mục lục
- [Tính năng](#-tính-năng)
- [Yêu cầu](#-yêu-cầu)
- [Cài đặt](#-cài-đặt)
- [Sử dụng](#-sử-dụng)
- [Giấy phép](#-giấy-phép)
- [Vá lỗi](#-vá-lỗi)

## ✨ Tính năng
- Tự động cào (crawl) proxy từ 30+ nguồn khác nhau.
- Tự động kiểm tra (check) proxy sống/chết.
- Tự động tìm kiếm username thông qua Project ID.

## 🛠️ Yêu cầu
Hệ điều hành: Windows 10/11
RAM: 4GB trở lên

## 🚀 Sử dụng
Tải xuống và chạy công cụ được build sẵn trong phần Release.

Sau khi chạy, màn hình console sẽ hiện ra dòng chữ `Nhập ID dự án (VD: 12345678): `. Bạn chỉ cần nhập ID dự án Scratch của bạn và nhấn Enter.

**Ví dụ:**
```text
Nhập ID dự án (VD: 12345678): 123456789
Lấy được ... proxy từ nguồn ...
Tìm thấy proxy hoạt động: ...
Tìm thấy tổng cộng ... proxy hoạt động
Đã nhận phản hồi từ proxy: 200 ...
```

## 🤝 Đóng góp
Mọi đóng góp đều được chào đón! Vui lòng tạo một Pull Request hoặc mở một Issue nếu bạn thấy lỗi.

## 📄 Giấy phép
Dự án này được phân phối dưới giấy phép GNU AGPL v3 - xem file [LICENSE](LICENSE) để biết thêm chi tiết.

## 📄 Vá lỗi

1. Belief
int.TryParse là hàm kiểm tra xem chữ bạn nhập có phải là số nguyên không. Nếu bạn nhập "abc" hoặc ấn Enter (bỏ trống), hàm này trả về false. Thay vì dùng một vòng lặp while để "mắng" người dùng và ép họ nhập lại cho đến khi đúng là số thì thôi, tác giả lại tặc lưỡi: "Thôi nhập sai thì gán tạm ID là -1 rồi chạy tiếp". Tác giả đã dùng số -1 như một cờ báo hiệu (flag) ban đầu, nhưng lại tái sử dụng nó làm giá trị mặc định khi lỗi xảy ra. Đây là một thiết kế lười biếng.

2. Cascade Failure
Vì chương trình không chịu dừng lại để sửa lỗi ngay ở khâu nhập liệu, một "dữ liệu độc hại" (số -1) bắt đầu lọt sâu vào hệ thống và gây ra sự sụp đổ dây chuyền:
Chương trình tự động ghép chuỗi để tạo link API: https://api.scratch.mit.edu/projects/-1.
Chương trình gửi Request (yêu cầu) đến máy chủ của Scratch.
Máy chủ Scratch nhận được yêu cầu tìm project có ID là -1. Vì trên đời làm gì có project nào ID âm, máy chủ Scratch sẽ trả về mã lỗi 404 Not Found (Không tìm thấy).

3. API Contract và lưới an toàn
Hàm GetStringAsync trong C# được thiết kế rất nghiêm ngặt: Nếu máy chủ trả về mã thành công (200 OK), nó sẽ tải dữ liệu về. Nhưng nếu máy chủ trả về lỗi (như 404 ở trên), nó sẽ ném ra một Ngoại lệ (Exception) có tên là HttpRequestException.
Theo nguyên tắc lập trình, khi chương trình kết nối Internet, lập trình viên BẮT BUỘC phải giăng một cái "lưới an toàn" gọi là Try/Catch để hứng những lỗi bất ngờ (như mất mạng, server sập, link chết...).
Nhưng tác giả đã không bọc try/catch ở đoạn này. Ngoại lệ bị ném ra, không có ai hứng lấy nó, và hệ điều hành quyết định "giết chết" tiến trình. Lỗi văng (Crash) chương trình xuất hiện!

4. Sửa lại các thông báo của chương trình
Các thông báo được dịch lại sang tiếng Việt.

**✅ Các Lỗi Đã Được Vá 100%**
