# BUSINESS REQUIREMENTS DOCUMENT (BRD)

# BOOK SHOP -- NỀN TẢNG BÁN SÁCH TRỰC TUYẾN

**Phiên bản:** 1.0\
**Trạng thái:** Draft for Review\
**Chủ sở hữu:** Founder & CEO -- Book Shop

---

## 1. TỔNG QUAN DỰ ÁN

### 1.1. Bối cảnh

Book Shop là chuỗi cửa hàng bán sách đang mở rộng từ mô hình bán hàng
tại cửa hàng vật lý sang bán hàng trực tuyến.

Website Book Shop được xây dựng nhằm tạo một kênh bán hàng trực tuyến,
đồng thời hỗ trợ Book Shop quản lý thống nhất khách hàng, sách, đơn
hàng, thanh toán, giao hàng, tồn kho và hoạt động kinh doanh.

### 1.2. Vấn đề cần giải quyết

Book Shop hiện cần một nền tảng giúp:

- Tiếp cận khách hàng ngoài khu vực cửa hàng.
- Giúp khách hàng tìm và mua sách thuận tiện.
- Giảm các thao tác xử lý thủ công.
- Theo dõi hàng hóa và đơn hàng chính xác hơn.
- Có thông tin phục vụ quản lý và ra quyết định.

### 1.3. Tầm nhìn

Xây dựng một nền tảng bán sách đa kênh, kết nối khách hàng, website, cửa
hàng, kho hàng và hoạt động kinh doanh của Book Shop.

---

## 2. MỤC TIÊU KINH DOANH

Dự án hướng tới các mục tiêu:

1.  Tăng doanh thu từ kênh trực tuyến.
2.  Mở rộng phạm vi tiếp cận khách hàng.
3.  Cải thiện trải nghiệm mua sách.
4.  Quản lý sản phẩm và tồn kho chính xác.
5.  Giảm thời gian xử lý đơn hàng.
6.  Cung cấp báo cáo hỗ trợ quản lý.
7.  Tạo nền tảng có thể mở rộng trong tương lai.

## 3. PHẠM VI VÀ ĐỐI TƯỢNG SỬ DỤNG

### 3.1. Trong phạm vi

Website và hệ thống quản lý bao gồm:

- Quản lý tài khoản.
- Quản lý sách và danh mục.
- Tìm kiếm và lọc sách.
- Giỏ hàng và đặt hàng.
- Thanh toán.
- Quản lý đơn hàng.
- Quản lý tồn kho.
- Quản lý cửa hàng.
- Khuyến mãi.
- Đánh giá sản phẩm.
- Thông báo.
- Báo cáo kinh doanh.
- Chatbot hỗ trợ khách hàng.

### 3.2. Đối tượng sử dụng

**Khách hàng:** tìm kiếm, mua sách, thanh toán, theo dõi đơn hàng, đánh
giá và quản lý tài khoản.

**Khách vãng lai:** xem, tìm kiếm và khám phá sách nhưng không sử dụng
các chức năng yêu cầu tài khoản.

**Nhân viên:** Xác nhận đơn hàng vận chuyển, xác nhận giao hàng thành công (nhận thông báo từ bên thứ 3 và xác nhận trạng thái giao hàng thành công cho khách hàng).

**Quản lý cửa hàng:** quản lý hàng hóa, đơn hàng và thông tin kinh doanh
trong phạm vi cửa hàng được phân công.

**Quản trị viên:** quản lý toàn bộ hoạt động được Book Shop cho phép.

### 3.3. Ngoài phạm vi giai đoạn đầu

- Marketplace cho bên bán thứ ba.
- Livestream bán hàng.
- Cho thuê sách.
- Hệ thống kế toán/ERP chuyên sâu.
- Ứng dụng mobile riêng.
- Quản lý việc vẫn chuyển đơn hàng (được giao cho bên thứ 3).
- Thanh toán online thông qua ngân hàng hoặc các hình thức online khác.

Các chức năng này có thể được xem xét ở giai đoạn sau.

---

## 4. SẢN PHẨM, TÌM KIẾM VÀ TRẢI NGHIỆM MUA HÀNG

### 4.1. Quản lý sản phẩm

Book Shop cần quản lý các thông tin:

- Tên sách.
- Tác giả.
- Nhà xuất bản.
- Thể loại.
- Mô tả.
- Hình ảnh.
- Giá bán.

Quản trị viên có thể thêm, xem, chỉnh sửa, ngừng bán và kích hoạt lại
sản phẩm.

### 4.2. Tìm kiếm và khám phá

Khách hàng có thể tìm kiếm theo:

- Tên sách.
- Tác giả.
- Nhà xuất bản.
- Thể loại.
- Từ khóa liên quan.

Có thể lọc theo:

- Giá.
- Thể loại.
- Nhà xuất bản.
- Đánh giá.
- Tình trạng còn hàng.

Có thể sắp xếp theo:

- Giá.
- Bán chạy.
- Đánh giá.
- Sách mới.

### 4.3. Trang chi tiết sách

Trang sách cần hiển thị rõ:

- Tên sách.
- Tác giả.
- Hình ảnh.
- Giá.
- Giá khuyến mãi nếu có.
- Mô tả.
- Nhà xuất bản.
- Đánh giá.
- Tình trạng còn hàng.

Khách hàng có thể thêm vào giỏ hàng hoặc mua ngay.

---

## 5. TÀI KHOẢN, GIỎ HÀNG, ĐẶT HÀNG VÀ THANH TOÁN

### 5.1. Tài khoản

Khách hàng có thể:

- Đăng ký.
- Đăng nhập.
- Xác minh tài khoản.
- Lấy lại mật khẩu.
- Cập nhật thông tin cá nhân.
- Quản lý địa chỉ giao hàng.
- Xem lịch sử mua hàng.

### 5.2. Giỏ hàng

Khách hàng có thể:

- Thêm sách.
- Thay đổi số lượng.
- Xóa sản phẩm.
- Xem tổng tiền.

Không được đặt số lượng vượt quá số hàng có thể bán.

### 5.3. Đặt hàng

Quy trình cơ bản:

```text
Tìm sách
   ↓
Chọn sách
   ↓
Thêm vào giỏ (tùy chọn)
   ↓
Nhập địa chỉ
   ↓
Chọn phương thức thanh toán (Phiên bản 1.0 chỉ có thanh toán khi nhận hàng)
   ↓
Áp dụng khuyến mãi (Tùy chọn)
   ↓
Xác nhận đơn (trạng thái đơn hàng là chờ xác nhận)
   ↓
Nhân viên giao hàng cho bên thứ 3 vận chuyển (trạng thái đơn hàng là đang vận chuyển)
   ↓
Giao hàng (Phiên bản 1 là khách hàng thanh toán với bên thứ 3 vẫn chuyển)
   ↓
Hoàn tất (Book shop nhận được tiền từ bên thứ 3 và xác nhận trạng thái đơn hàng là hoàn tất)
```

## 6. ĐƠN HÀNG VÀ CHĂM SÓC KHÁCH HÀNG

### 6.1. Quản lý đơn hàng

Khách hàng có thể:

- Xem đơn hàng.
- Theo dõi trạng thái.
- Xem sản phẩm và số tiền.
- Hủy đơn nếu vẫn được phép.
*Lưu ý:* Đơn hàng không được hủy khi trạng thái đơn hàng là đang vận chuyển.

Nhân viên được phân quyền có thể:

- Tìm kiếm đơn.
- Xác nhận đơn.
- Xử lý đơn.
- Hủy đơn theo chính sách.
- Xử lý yêu cầu trả hàng và hoàn tiền.

### 6.2. Trả hàng và hoàn tiền

- Khi khách hàng đã nhận được hàng (trạng thái đơn hàng là Hoàn tất) và muốn trả hàng thì cần cung cấp các các thông tin minh chứng, tài khoảng ngân hàng.
Quy trình:

```text
Khách hàng yêu cầu
       ↓
Book Shop kiểm tra
       ↓
Chấp nhận / Từ chối
       ↓
Nhận và kiểm tra hàng
       ↓
Hoàn tiền nếu đủ điều kiện
       ↓
Cập nhật đơn hàng và hàng hóa
```

### 6.3. Thông báo

Khách hàng nhận thông báo về các sự kiện quan trọng như:

- Đăng ký tài khoản.
- Đặt hàng.
- Thanh toán.
- Xác nhận đơn.
- Giao hàng.
- Hủy đơn.
- Hoàn tiền.
- Khuyến mãi.

---

## 7. TỒN KHO, CỬA HÀNG VÀ KHUYẾN MÃI

### 7.1. Quản lý tồn kho

Book Shop cần biết số lượng sách tại từng cửa hàng hoặc kho.

Cần theo dõi:

- Số lượng hiện có.
- Số lượng đã giữ cho đơn hàng.
- Số lượng có thể bán.
- Số lượng hư hỏng.
- Số lượng đang chờ nhập.

Tồn kho được cập nhật khi:

- Nhập hàng.
- Bán hàng.
- Đặt hàng.
- Hủy đơn.
- Hoàn hàng.
- Điều chuyển.
- Ghi nhận hàng hư hỏng.

**Nguyên tắc:** không được bán vượt số lượng hàng có thể bán.

### 7.2. Quản lý nhiều cửa hàng

Book Shop có thể:

- Xem tồn kho từng cửa hàng.
- Kiểm kê.
- Nhập và xuất hàng.
- Điều chuyển hàng.
- Nhận hàng từ cửa hàng khác.
- Theo dõi lịch sử thay đổi.

### 7.3. Khuyến mãi

Có thể áp dụng:

- Giảm theo phần trăm.
- Giảm theo số tiền.
- Mã giảm giá.
- Miễn phí giao hàng.
- Flash sale.
- Khuyến mãi theo thể loại.

Khuyến mãi phải tuân theo điều kiện về sản phẩm, giá trị đơn hàng, thời
gian và số lần sử dụng.

---

## 8. ĐÁNH GIÁ VÀ CHATBOT

### 8.1. Đánh giá

Khách hàng có thể đánh giá từ 1 đến 5 sao và viết nhận xét.

Book Shop có thể yêu cầu khách hàng đã mua sản phẩm mới được đánh giá.

Quản trị viên có thể kiểm duyệt và ẩn các đánh giá vi phạm.

### 8.2. Chatbot

Chatbot hỗ trợ:

- Tìm sách.
- Gợi ý sách.
- Giải đáp câu hỏi về sản phẩm.

Trong tương lai có thể mở rộng thành hệ thống đề xuất sách cá nhân hóa.

---

## 9. QUY TẮC KINH DOANH, BÁO CÁO VÀ BẢO MẬT

### 9.1. Quy tắc kinh doanh

---

Mã Quy tắc

---

BR-001 Giá bán không được nhỏ hơn 0.

BR-002 Không được đặt vượt số lượng hàng
có thể bán.

BR-003 Khuyến mãi hết hạn không được áp
dụng.

BR-004 Không được sử dụng điểm vượt quá số
điểm hiện có.

BR-005 Đánh giá phải tuân theo chính sách
Book Shop.

BR-006 Đơn hàng chỉ được hủy khi đáp ứng
điều kiện cho phép.

BR-007 Hoàn tiền phải tuân theo chính sách
trả hàng.

BR-008 Người dùng chỉ được thực hiện công
việc phù hợp với trách nhiệm.

BR-009 Tổng tiền phải được xác định rõ
trước khi đặt hàng.

BR-0010 Việc hủy đơn hàng không được diển ra khi trạng thái đang là đang vận chuyển..

---

### 9.2. Báo cáo

Ban quản lý cần có thông tin về:

**Doanh thu**

- Theo ngày, tuần, tháng, quý, năm.
- Theo cửa hàng.
- Theo sản phẩm.
- Theo thể loại.

**Đơn hàng**

- Đơn mới.
- Đơn đang xử lý.
- Đơn hoàn tất.
- Đơn hủy.

**Hàng hóa**

- Sách sắp hết.
- Sách hết hàng.
- Sách bán chạy.
- Sách bán chậm.

**Khách hàng**

- Khách hàng mới.
- Khách hàng quay lại.
- Tần suất mua hàng.

---

## 10. TIÊU CHÍ THÀNH CÔNG VÀ NGHIỆM THU

### 10.1. Tiêu chí nghiệm thu

Dự án được nghiệm thu khi:

- Khách hàng có thể tìm kiếm và xem sách.
- Khách hàng có thể thêm sách vào giỏ hàng.
- Khách hàng có thể đặt hàng.
- Khách hàng có thể thanh toán bằng các phương thức được Book Shop hỗ
  trợ.
- Không cho phép bán vượt số lượng hàng có thể bán.
- Khách hàng có thể theo dõi đơn hàng.
- Book Shop có thể quản lý sản phẩm.
- Book Shop có thể quản lý đơn hàng.
- Book Shop có thể theo dõi tồn kho.
- Người dùng chỉ được thực hiện các chức năng phù hợp với trách nhiệm
  của mình.
- Ban quản lý có thể xem các báo cáo cần thiết.
- Không còn lỗi nghiêm trọng ảnh hưởng đến hoạt động kinh doanh tại
  thời điểm nghiệm thu.

### 10.2. Kết quả mong đợi

Sau khi hoàn thành, Book Shop có một kênh bán hàng trực tuyến đáp ứng
các nhu cầu kinh doanh chính:

- Khách hàng dễ dàng tìm kiếm và mua sách.
- Book Shop quản lý được sản phẩm, đơn hàng và tồn kho.
- Quy trình mua hàng rõ ràng và thuận tiện.
- Ban quản lý có thông tin cần thiết để theo dõi hoạt động kinh doanh.
- Nền tảng có thể tiếp tục được mở rộng khi nhu cầu kinh doanh tăng
  lên.

### 10.3. Nguyên tắc của BRD

BRD xác định **Book Shop cần gì, ai cần và vì sao cần**.

BRD không đi sâu vào cách đội phát triển xây dựng hệ thống. Các nội dung
như:

- Công nghệ sử dụng.
- Cơ sở dữ liệu.
- Kiến trúc phần mềm.
- Thiết kế API.
- Thiết kế giao diện chi tiết.
- Yêu cầu kỹ thuật.
- Bảo mật ở mức kỹ thuật.
- Cách triển khai và vận hành hệ thống.

sẽ được mô tả trong các tài liệu chuyên môn ở giai đoạn sau, đặc biệt là
**SRS, Software Architecture Document, API Specification và Technical
Design**.

---

**Document Owner:** Founder & CEO -- Book Shop\
**Document Type:** Business Requirements Document\
**Version:** 1.0\
**Status:** Draft for Review
