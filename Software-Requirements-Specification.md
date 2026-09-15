# SOFTWARE REQUIREMENTS SPECIFICATION (SRS)

## BOOK SHOP — NỀN TẢNG BÁN SÁCH TRỰC TUYẾN

**Phiên bản:** 1.0  
**Trạng thái:** Draft for Review  
**Loại tài liệu:** Software Requirements Specification  
**Tài liệu nguồn:** Business Requirements Document (BRD) v1.0  
**Chủ sở hữu:** Founder & CEO — Book Shop  

---

# MỤC LỤC

1. [Giới thiệu](#1-giới-thiệu)
2. [Tổng quan hệ thống](#2-tổng-quan-hệ-thống)
3. [Đối tượng sử dụng và phân quyền](#3-đối-tượng-sử-dụng-và-phân-quyền)
4. [Phạm vi hệ thống](#4-phạm-vi-hệ-thống)
5. [Yêu cầu chức năng](#5-yêu-cầu-chức-năng)
   - 5.1. Quản lý tài khoản
   - 5.2. Quản lý sách và danh mục
   - 5.3. Tìm kiếm và lọc sách
   - 5.4. Chi tiết sách
   - 5.5. Giỏ hàng
   - 5.6. Đặt hàng
   - 5.7. Thanh toán
   - 5.8. Quản lý đơn hàng
   - 5.9. Trả hàng và hoàn tiền
   - 5.10. Quản lý tồn kho
   - 5.11. Quản lý cửa hàng
   - 5.12. Khuyến mãi
   - 5.13. Đánh giá sản phẩm
   - 5.14. Thông báo
   - 5.15. Chatbot
   - 5.16. Báo cáo kinh doanh
6. [Yêu cầu nghiệp vụ và Business Rules](#6-yêu-cầu-nghiệp-vụ-và-business-rules)
7. [Yêu cầu phi chức năng](#7-yêu-cầu-phi-chức-năng)
8. [Yêu cầu dữ liệu](#8-yêu-cầu-dữ-liệu)
9. [Quy trình nghiệp vụ và trạng thái](#9-quy-trình-nghiệp-vụ-và-trạng-thái)
10. [Phân quyền và kiểm soát truy cập](#10-phân-quyền-và-kiểm-soát-truy-cập)
11. [Yêu cầu giao diện và trải nghiệm người dùng](#11-yêu-cầu-giao-diện-và-trải-nghiệm-người-dùng)
12. [Yêu cầu tích hợp](#12-yêu-cầu-tích-hợp)
13. [Yêu cầu báo cáo](#13-yêu-cầu-báo-cáo)
14. [Tiêu chí chấp nhận](#14-tiêu-chí-chấp-nhận)
15. [Traceability Matrix](#15-traceability-matrix)
16. [Các vấn đề cần làm rõ](#16-các-vấn-đề-cần-làm-rõ)
17. [Ngoài phạm vi](#17-ngoài-phạm-vi)
18. [Phụ lục](#18-phụ-lục)

---

# 1. GIỚI THIỆU

## 1.1. Mục đích tài liệu

Tài liệu Software Requirements Specification (SRS) mô tả các yêu cầu phần mềm của hệ thống **Book Shop — Nền tảng bán sách trực tuyến**.

SRS chuyển đổi các nhu cầu kinh doanh được xác định trong BRD thành các yêu cầu mà hệ thống phần mềm cần đáp ứng.

Tài liệu này được sử dụng làm cơ sở cho:

- Phân tích và thiết kế hệ thống.
- Thiết kế kiến trúc phần mềm.
- Thiết kế cơ sở dữ liệu.
- Phát triển chức năng.
- Thiết kế API.
- Thiết kế giao diện.
- Viết test case và kiểm thử hệ thống.
- Nghiệm thu sản phẩm.

## 1.2. Tài liệu tham chiếu

Tài liệu nguồn chính:

**Business Requirements Document — Book Shop, Version 1.0**

BRD xác định nhu cầu kinh doanh, đối tượng sử dụng và phạm vi tổng thể của hệ thống.

## 1.3. Phạm vi tài liệu

SRS bao gồm các yêu cầu liên quan đến:

- Tài khoản người dùng.
- Sách và danh mục.
- Tìm kiếm.
- Giỏ hàng.
- Đặt hàng.
- Thanh toán.
- Đơn hàng.
- Trả hàng và hoàn tiền.
- Tồn kho.
- Cửa hàng.
- Khuyến mãi.
- Đánh giá.
- Thông báo.
- Chatbot.
- Báo cáo.
- Phân quyền.
- Các yêu cầu phi chức năng ở mức yêu cầu hệ thống.

---

# 2. TỔNG QUAN HỆ THỐNG

## 2.1. Mục tiêu hệ thống

Book Shop là nền tảng bán sách trực tuyến được xây dựng để mở rộng hoạt động kinh doanh từ cửa hàng vật lý sang môi trường trực tuyến.

Hệ thống cung cấp một kênh bán hàng trực tuyến đồng thời hỗ trợ quản lý:

- Khách hàng.
- Sách.
- Đơn hàng.
- Thanh toán.
- Giao hàng.
- Tồn kho.
- Cửa hàng.
- Khuyến mãi.
- Hoạt động kinh doanh.

Mục tiêu tổng thể là kết nối **khách hàng — website — cửa hàng — kho hàng — hoạt động kinh doanh** trong cùng một nền tảng.

## 2.2. Mục tiêu kinh doanh

Hệ thống phải hỗ trợ Book Shop:

1. Tăng doanh thu từ kênh trực tuyến.
2. Mở rộng phạm vi tiếp cận khách hàng.
3. Cải thiện trải nghiệm mua sách.
4. Quản lý sản phẩm và tồn kho chính xác.
5. Giảm thời gian xử lý đơn hàng.
6. Cung cấp báo cáo hỗ trợ quản lý.
7. Tạo nền tảng có khả năng mở rộng trong tương lai.

## 2.3. Tổng quan tác nhân

Hệ thống có các nhóm người dùng:

| Actor | Mô tả |
|---|---|
| Guest | Người chưa đăng nhập, có thể xem và tìm kiếm sách |
| Customer | Khách hàng đã có tài khoản |
| Employee | Nhân viên xử lý đơn hàng và xác nhận trạng thái giao hàng |
| Store Manager | Quản lý hàng hóa, đơn hàng và thông tin kinh doanh của cửa hàng được phân công |
| Administrator | Quản lý toàn bộ hoạt động được Book Shop cho phép |

Các đối tượng trên được xác định trong BRD.

---

# 3. ĐỐI TƯỢNG SỬ DỤNG VÀ PHÂN QUYỀN

## 3.1. Guest

Guest có thể:

- Xem danh sách sách.
- Tìm kiếm sách.
- Lọc sách.
- Sắp xếp sách.
- Xem thông tin chi tiết sách.
- Khám phá sản phẩm.

Guest không được sử dụng các chức năng yêu cầu tài khoản.

## 3.2. Customer

Customer có thể:

- Đăng ký tài khoản.
- Đăng nhập.
- Xác minh tài khoản.
- Lấy lại mật khẩu.
- Cập nhật thông tin cá nhân.
- Quản lý địa chỉ.
- Quản lý giỏ hàng.
- Đặt hàng.
- Theo dõi đơn hàng.
- Hủy đơn nếu đáp ứng điều kiện.
- Yêu cầu trả hàng/hoàn tiền.
- Đánh giá sản phẩm.
- Nhận thông báo.
- Sử dụng chatbot.

## 3.3. Employee

Employee có thể:

- Tìm kiếm đơn hàng.
- Xác nhận đơn.
- Xử lý đơn.
- Chuyển đơn cho bên thứ ba vận chuyển.
- Nhận thông tin trạng thái giao hàng từ bên thứ ba.
- Xác nhận trạng thái giao hàng thành công cho khách hàng.
- Hủy đơn theo chính sách.
- Xử lý yêu cầu trả hàng và hoàn tiền theo phân quyền.

## 3.4. Store Manager

Store Manager có thể:

- Xem tồn kho cửa hàng được phân công.
- Kiểm kê.
- Nhập hàng.
- Xuất hàng.
- Điều chuyển hàng.
- Nhận hàng từ cửa hàng khác.
- Theo dõi lịch sử thay đổi.
- Quản lý đơn hàng trong phạm vi được phân quyền.
- Xem thông tin kinh doanh trong phạm vi cửa hàng.

## 3.5. Administrator

Administrator có quyền quản lý toàn bộ hoạt động được Book Shop cho phép, bao gồm:

- Người dùng.
- Sản phẩm.
- Danh mục.
- Đơn hàng.
- Tồn kho.
- Cửa hàng.
- Khuyến mãi.
- Đánh giá.
- Báo cáo.
- Các cấu hình quản trị khác.

---

# 4. PHẠM VI HỆ THỐNG

## 4.1. In Scope

Hệ thống bao gồm:

- Account Management.
- Product & Category Management.
- Search & Filtering.
- Shopping Cart.
- Order Management.
- Payment.
- Inventory Management.
- Store Management.
- Promotion Management.
- Product Review.
- Notification.
- Customer Chatbot.
- Business Reporting.

Đây là các chức năng nằm trong phạm vi của BRD.

## 4.2. Out of Scope

Các chức năng sau không thuộc phiên bản đầu:

- Marketplace cho bên bán thứ ba.
- Livestream bán hàng.
- Cho thuê sách.
- Hệ thống kế toán/ERP chuyên sâu.
- Mobile App riêng.
- Quản lý việc vận chuyển đơn hàng.
- Thanh toán trực tuyến qua ngân hàng hoặc các phương thức online khác.

Việc vận chuyển được giao cho bên thứ ba và phiên bản 1.0 chỉ hỗ trợ thanh toán khi nhận hàng.

---

# 5. YÊU CẦU CHỨC NĂNG

# 5.1. Quản lý tài khoản

## FR-ACC-001 — Đăng ký tài khoản

**Actor:** Guest

Hệ thống phải cho phép Guest đăng ký tài khoản.

### Preconditions

- Người dùng chưa có tài khoản hoặc đang sử dụng chức năng đăng ký.

### Main Flow

1. Guest mở chức năng đăng ký.
2. Guest nhập thông tin yêu cầu.
3. Hệ thống kiểm tra dữ liệu.
4. Hệ thống tạo tài khoản.
5. Hệ thống yêu cầu xác minh tài khoản nếu cần.
6. Hệ thống thông báo kết quả.

Ngoài việc đang ký bằng cách nhập thông tin thì Guest còn có thể đang ký tài khoảng với google (Chức năng sign in google).

### Business Rules

- Thông tin tài khoản phải hợp lệ.
- Tài khoản phải được xác minh theo quy trình của Book Shop.

---

## FR-ACC-002 — Đăng nhập

**Actor:** Customer

Hệ thống phải cho phép Customer đăng nhập vào tài khoản.

### Main Flow

1. Customer nhập thông tin đăng nhập.
2. Hệ thống xác thực thông tin.
3. Nếu hợp lệ, hệ thống cho phép truy cập tài khoản.
4. Nếu không hợp lệ, hệ thống thông báo lỗi.

- Ngoài ra khách hàng còn có thể đang nhập bằng google mà không cần phải nhập thông tin.
---

## FR-ACC-003 — Xác minh tài khoản

 - Hệ thống phải hỗ trợ xác minh tài khoản sau khi đăng ký. Ở đây hệ thông có thể xác minh tài khoản người dùng bằng mã OTP.
---

## FR-ACC-004 — Lấy lại mật khẩu

Customer phải có khả năng thực hiện quy trình lấy lại mật khẩu bằng cách xác thực email bằng mã otp và cung cấp mật khẩu mới.

---

## FR-ACC-005 — Cập nhật thông tin cá nhân

Customer phải có khả năng:

- Xem thông tin cá nhân.
- Cập nhật thông tin được phép.
- Lưu thông tin mới.

---

## FR-ACC-006 — Quản lý địa chỉ giao hàng

Customer phải có khả năng:

- Thêm địa chỉ.
- Xem địa chỉ.
- Chỉnh sửa địa chỉ.
- Chọn địa chỉ sử dụng khi đặt hàng.

---

## FR-ACC-007 — Xem lịch sử mua hàng

Customer phải có khả năng xem lịch sử các đơn hàng đã thực hiện.

---

# 5.2. Quản lý sách và danh mục

## FR-PROD-001 — Thêm sách

**Actor:** Administrator.

Administrator có thể thêm sản phẩm mới.

Thông tin tối thiểu:

- Tên sách.
- Tác giả.
- Nhà xuất bản.
- Thể loại.
- Mô tả.
- Hình ảnh.
- Giá bán.

Các thông tin này được xác định trong BRD.

---

## FR-PROD-002 — Xem sách

Hệ thống phải cho phép người dùng xem thông tin sách phù hợp với quyền truy cập.

---

## FR-PROD-003 — Chỉnh sửa sách

Administrator có thể chỉnh sửa thông tin sản phẩm.

---

## FR-PROD-004 — Ngừng bán sách

Administrator có thể ngừng bán sản phẩm.

Sản phẩm ngừng bán không được xuất hiện như sản phẩm có thể mua.

---

## FR-PROD-005 — Kích hoạt lại sách

Administrator có thể kích hoạt lại sản phẩm đã ngừng bán.

---

# 5.3. Tìm kiếm và lọc sách

## FR-SEARCH-001 — Tìm kiếm

Hệ thống phải cho phép tìm kiếm theo:

- Tên sách.
- Tác giả.
- Nhà xuất bản.
- Thể loại.
- Từ khóa liên quan.

## FR-SEARCH-002 — Lọc

Hệ thống phải hỗ trợ lọc theo:

- Giá.
- Thể loại.
- Nhà xuất bản.
- Đánh giá.
- Tình trạng còn hàng.

## FR-SEARCH-003 — Sắp xếp

Hệ thống phải hỗ trợ sắp xếp theo:

- Giá.
- Bán chạy.
- Đánh giá.
- Sách mới.

Các tiêu chí tìm kiếm, lọc và sắp xếp trên được lấy từ BRD.

---

# 5.4. Chi tiết sách

## FR-PROD-006 — Hiển thị chi tiết sách

Hệ thống phải hiển thị:

- Tên sách.
- Tác giả.
- Hình ảnh.
- Giá.
- Giá khuyến mãi nếu có.
- Mô tả.
- Nhà xuất bản.
- Đánh giá.
- Tình trạng còn hàng.

## FR-PROD-007 — Thêm vào giỏ hàng

Customer có thể thêm sách vào giỏ hàng từ trang chi tiết.

## FR-PROD-008 — Mua ngay

Customer có thể mua sản phẩm trực tiếp từ trang chi tiết.



---

# 5.5. Giỏ hàng

## FR-CART-001 — Thêm sản phẩm

Customer có thể thêm sách vào giỏ hàng.

## FR-CART-002 — Thay đổi số lượng

Customer có thể tăng hoặc giảm số lượng sách.

## FR-CART-003 — Xóa sản phẩm

Customer có thể xóa sách khỏi giỏ hàng.

## FR-CART-004 — Xem tổng tiền

Hệ thống phải hiển thị tổng giá trị giỏ hàng.

## FR-CART-005 — Kiểm tra tồn kho

Hệ thống không được cho phép Customer đặt số lượng vượt quá số lượng hàng có thể bán.

Đây là yêu cầu nghiệp vụ bắt buộc của BRD.

---

# 5.6. Đặt hàng

## FR-ORDER-001 — Tạo đơn hàng

Customer có thể tạo đơn hàng theo quy trình:

```text
Tìm sách
   ↓
Chọn sách
   ↓
Thêm vào giỏ hàng (optional)
   ↓
Nhập địa chỉ
   ↓
Chọn phương thức thanh toán
   ↓
Áp dụng khuyến mãi (optional)
   ↓
Xác nhận đơn
   ↓
Chờ xác nhận
   ↓
Đang vận chuyển
   ↓
Giao hàng
   ↓
Hoàn tất
```

Quy trình trên được xác định trong BRD.

## FR-ORDER-002 — Xác định tổng tiền

Trước khi Customer xác nhận đơn, hệ thống phải hiển thị rõ tổng số tiền phải thanh toán.

Tổng tiền phải phản ánh:

- Giá sản phẩm.
- Số lượng.
- Giá khuyến mãi nếu có.
- Các khoản liên quan khác được Book Shop hỗ trợ.

## FR-ORDER-003 — Kiểm tra tồn kho trước khi đặt

Hệ thống phải kiểm tra số lượng hàng có thể bán trước khi tạo đơn.

## FR-ORDER-004 — Áp dụng khuyến mãi

Customer có thể áp dụng chương trình hoặc mã khuyến mãi hợp lệ.

## FR-ORDER-005 — Xác nhận đơn

Sau khi Customer xác nhận, đơn hàng được tạo với trạng thái:

**Chờ xác nhận**

---

# 5.7. Thanh toán

## FR-PAY-001 — Thanh toán khi nhận hàng

Phiên bản 1.0 phải hỗ trợ phương thức:

**Cash on Delivery (COD)**.

Customer thanh toán cho bên thứ ba vận chuyển khi nhận hàng.

## FR-PAY-002 — Xác nhận hoàn tất thanh toán

Sau khi bên thứ ba giao hàng và Book Shop nhận được tiền, hệ thống phải cho phép xác nhận đơn hàng đã hoàn tất.

## FR-PAY-003 — Không hỗ trợ thanh toán online

Phiên bản 1.0 không bao gồm:

- Thanh toán ngân hàng trực tuyến.
- Các phương thức thanh toán online khác.

---

# 5.8. Quản lý đơn hàng

## FR-ORDER-006 — Customer xem đơn hàng

Customer có thể:

- Xem danh sách đơn hàng.
- Xem chi tiết đơn hàng.
- Xem sản phẩm.
- Xem số tiền.
- Theo dõi trạng thái.

## FR-ORDER-007 — Hủy đơn hàng

Customer có thể hủy đơn nếu trạng thái và chính sách cho phép.

**Không được phép hủy đơn khi trạng thái là "Đang vận chuyển".**



## FR-ORDER-008 — Nhân viên tìm kiếm đơn

Employee có thể tìm kiếm đơn hàng.

## FR-ORDER-009 — Nhân viên xác nhận đơn

Employee có thể xác nhận đơn hàng. 
Nhân viên xác nhận đơn hàng đã chuyển sang trạng thái đang vẫn chuyển. 
Nhân viên xác nhận đơn hàng đã hoàn tắt khi nhận được tiền từ bên vận chuyển thứ 3.

## FR-ORDER-010 — Xử lý đơn

Employee có thể xử lý đơn hàng theo quy trình nghiệp vụ.

## FR-ORDER-011 — Hủy đơn bởi nhân viên

Employee có thể hủy đơn theo chính sách.

---

# 5.9. Trả hàng và hoàn tiền

## FR-RETURN-001 — Tạo yêu cầu trả hàng

Customer có thể yêu cầu trả hàng sau khi đã nhận hàng và đơn hàng ở trạng thái **Hoàn tất**. Thời gian hoàn trả không quá 3 ngày sau khi nhận hàng.

Customer phải cung cấp:

- Thông tin minh chứng.
- Thông tin tài khoản ngân hàng.

## FR-RETURN-002 — Kiểm tra yêu cầu

Book Shop có thể kiểm tra yêu cầu trả hàng.

## FR-RETURN-003 — Chấp nhận hoặc từ chối

Book Shop phải có khả năng:

- Chấp nhận yêu cầu.
- Từ chối yêu cầu.

## FR-RETURN-004 — Nhận và kiểm tra hàng

Book Shop thực hiện nhận và kiểm tra sản phẩm trả lại.

## FR-RETURN-005 — Hoàn tiền

Nếu yêu cầu đủ điều kiện, hệ thống phải hỗ trợ ghi nhận việc hoàn tiền.

## FR-RETURN-006 — Cập nhật đơn hàng và hàng hóa

Sau khi xử lý trả hàng, hệ thống phải cập nhật:

- Trạng thái đơn hàng.
- Tình trạng hàng hóa.
- Thông tin hoàn tiền.

Quy trình được xác định trong BRD.

---

# 5.10. Quản lý tồn kho

## FR-INV-001 — Theo dõi tồn kho

Hệ thống phải theo dõi số lượng sách tại từng cửa hàng hoặc kho.

Mỗi sản phẩm cần có thông tin:

- Số lượng hiện có.
- Số lượng đã giữ cho đơn hàng.
- Số lượng có thể bán.
- Số lượng hư hỏng.
- Số lượng đang chờ nhập.

## FR-INV-002 — Cập nhật tồn kho

Tồn kho phải được cập nhật khi xảy ra:

- Nhập hàng.
- Bán hàng.
- Đặt hàng.
- Hủy đơn.
- Hoàn hàng.
- Điều chuyển.
- Ghi nhận hàng hư hỏng.

## FR-INV-003 — Không bán vượt tồn kho

Hệ thống tuyệt đối không được cho phép bán vượt số lượng hàng có thể bán.



---

# 5.11. Quản lý cửa hàng

## FR-STORE-001 — Xem tồn kho cửa hàng

Người dùng có quyền có thể xem tồn kho theo từng cửa hàng.

## FR-STORE-002 — Kiểm kê

Hệ thống phải hỗ trợ ghi nhận hoạt động kiểm kê.

## FR-STORE-003 — Nhập hàng

Hệ thống phải hỗ trợ ghi nhận hàng nhập.

## FR-STORE-004 — Xuất hàng

Hệ thống phải hỗ trợ ghi nhận hàng xuất.

## FR-STORE-005 — Điều chuyển

Hệ thống phải hỗ trợ điều chuyển hàng giữa các cửa hàng/kho.

## FR-STORE-006 — Nhận hàng

Cửa hàng nhận có thể ghi nhận việc nhận hàng.

## FR-STORE-007 — Lịch sử tồn kho

Hệ thống phải lưu và cho phép theo dõi lịch sử thay đổi tồn kho.

---

# 5.12. Khuyến mãi

## FR-PROMO-001 — Khuyến mãi theo phần trăm

Hệ thống hỗ trợ giảm giá theo phần trăm.

## FR-PROMO-002 — Khuyến mãi theo số tiền

Hệ thống hỗ trợ giảm một số tiền cố định.

## FR-PROMO-003 — Mã giảm giá

Hệ thống hỗ trợ mã giảm giá.

## FR-PROMO-004 — Miễn phí giao hàng

Hệ thống hỗ trợ chương trình miễn phí giao hàng.

## FR-PROMO-005 — Flash Sale

Hệ thống hỗ trợ chương trình Flash Sale.

## FR-PROMO-006 — Khuyến mãi theo thể loại

Hệ thống hỗ trợ khuyến mãi theo thể loại sách.

## FR-PROMO-007 — Kiểm tra điều kiện

Khuyến mãi phải được kiểm tra theo:

- Sản phẩm.
- Giá trị đơn hàng.
- Thời gian.
- Số lần sử dụng.

Khuyến mãi đã hết hạn không được áp dụng.

---

# 5.13. Đánh giá sản phẩm

## FR-REVIEW-001 — Đánh giá sao

Customer có thể đánh giá sản phẩm từ:

**1 đến 5 sao.**

## FR-REVIEW-002 — Nhận xét

Customer có thể viết nhận xét về sản phẩm.

## FR-REVIEW-003 — Kiểm tra quyền đánh giá

Book Shop có thể yêu cầu chỉ Customer đã mua sản phẩm mới được đánh giá.

## FR-REVIEW-004 — Kiểm duyệt

Administrator, Store Manager, Employee có thể:

- Xem đánh giá.
- Kiểm duyệt.
- Ẩn đánh giá vi phạm.



---

# 5.14. Thông báo

## FR-NOTI-001 — Thông báo sự kiện

Hệ thống phải hỗ trợ thông báo cho Customer về các sự kiện quan trọng:

- Đăng ký tài khoản.
- Đặt hàng.
- Thanh toán.
- Xác nhận đơn.
- Giao hàng.
- Hủy đơn.
- Hoàn tiền.
- Khuyến mãi.

**Kênh thông báo cụ thể:** Trong hệ thống và Email.

---

# 5.15. Chatbot

## FR-CHAT-001 — Tìm sách

Chatbot phải hỗ trợ Customer tìm kiếm sách.

## FR-CHAT-002 — Gợi ý sách

Chatbot phải có khả năng gợi ý sách.

## FR-CHAT-003 — Giải đáp thông tin sản phẩm

Chatbot phải hỗ trợ trả lời câu hỏi liên quan đến sản phẩm.

## FR-CHAT-004 — Mở rộng trong tương lai

Hệ thống có thể được mở rộng thành hệ thống đề xuất sách cá nhân hóa.

Chức năng đề xuất cá nhân hóa chưa phải yêu cầu bắt buộc của phiên bản đầu.

---

# 5.16. Báo cáo kinh doanh

## FR-REPORT-001 — Báo cáo doanh thu

Hệ thống phải cung cấp thông tin doanh thu theo:

- Ngày.
- Tuần.
- Tháng.
- Quý.
- Năm.
- Cửa hàng.
- Sản phẩm.
- Thể loại.
- Có thể cuất bao cáo đơn hàng ra file csv và báo có qua mail hằng ngày vào 08:00 AM. Chỉ báo cáo cho người Administrator.

## FR-REPORT-002 — Báo cáo đơn hàng

Hệ thống phải cung cấp:

- Đơn mới.
- Đơn đang xử lý.
- Đơn hoàn tất.
- Đơn hủy.

## FR-REPORT-003 — Báo cáo hàng hóa

Hệ thống phải cung cấp:

- Sách sắp hết.
- Sách hết hàng.
- Sách bán chạy.
- Sách bán chậm.

## FR-REPORT-004 — Báo cáo khách hàng

Hệ thống phải cung cấp:

- Khách hàng mới.
- Khách hàng quay lại.
- Tần suất mua hàng.



---

# 6. YÊU CẦU NGHIỆP VỤ VÀ BUSINESS RULES

| ID | Business Rule | Yêu cầu hệ thống |
|---|---|---|
| BR-001 | Giá bán không được nhỏ hơn 0 | Hệ thống phải từ chối giá bán < 0 |
| BR-002 | Không đặt vượt hàng có thể bán | Hệ thống phải kiểm tra available quantity |
| BR-003 | Khuyến mãi hết hạn không được áp dụng | Hệ thống phải kiểm tra thời gian hiệu lực |
| BR-004 | Không sử dụng điểm vượt quá số điểm hiện có | Nếu hệ thống hỗ trợ điểm, phải kiểm tra số dư |
| BR-005 | Đánh giá tuân thủ chính sách | Hệ thống phải áp dụng policy đánh giá |
| BR-006 | Đơn hàng chỉ được hủy khi đáp ứng điều kiện | Hệ thống phải kiểm tra trạng thái/policy |
| BR-007 | Hoàn tiền tuân thủ chính sách trả hàng | Hệ thống phải kiểm tra điều kiện refund |
| BR-008 | Người dùng chỉ thực hiện công việc phù hợp trách nhiệm | Hệ thống phải kiểm soát authorization |
| BR-009 | Tổng tiền phải xác định rõ trước khi đặt hàng | Checkout phải hiển thị tổng tiền |
| BR-010 | Không hủy đơn khi đang vận chuyển | Hệ thống phải từ chối thao tác hủy |

Các business rules trên được chuyển từ BRD.

> **Lưu ý:** BR-004 về điểm thưởng xuất hiện trong BRD nhưng phần phạm vi chức năng chưa mô tả hệ thống Loyalty/Point. Do đó cần làm rõ trước khi triển khai.

---

# 7. YÊU CẦU PHI CHỨC NĂNG

BRD không đưa ra các chỉ số kỹ thuật cụ thể về hiệu năng, khả năng chịu tải, SLA hoặc bảo mật kỹ thuật. Vì vậy các yêu cầu dưới đây được xác định ở mức **system requirement cần được làm rõ**, không tự đặt giá trị kỹ thuật.

## NFR-001 — Performance

Hệ thống phải đáp ứng thời gian phản hồi phù hợp với trải nghiệm mua sắm trực tuyến.

- Thời gian phản hồi của một API không quá **500 ms** đối với các request thông thường.
- Hệ thống phải có khả năng xử lý tối thiểu **100 requests/giây (RPS)** trong điều kiện tải bình thường.
- Hệ thống phải hỗ trợ tối thiểu **500 người dùng đồng thời** mà không làm hệ thống ngừng hoạt động hoặc suy giảm nghiêm trọng hiệu năng.

## NFR-002 — Availability

Hệ thống phải đảm bảo khả năng truy cập phù hợp với hoạt động kinh doanh.

## NFR-003 — Scalability

Hệ thống phải có khả năng mở rộng khi số lượng:

- Khách hàng.
- Sản phẩm.
- Đơn hàng.
- Cửa hàng.

tăng lên.

## NFR-004 — Security

Hệ thống phải bảo vệ:

- Thông tin tài khoản.
- Thông tin cá nhân.
- Địa chỉ giao hàng.
- Thông tin liên quan đến đơn hàng.
- Thông tin tài khoản ngân hàng phục vụ hoàn tiền.

Các cơ chế bảo mật kỹ thuật cụ thể: **TBD**.

## NFR-005 — Authorization

Người dùng chỉ được truy cập chức năng và dữ liệu phù hợp với trách nhiệm được phân quyền.

## NFR-006 — Data Integrity

Hệ thống phải đảm bảo tính nhất quán của:

- Đơn hàng.
- Tồn kho.
- Thanh toán.
- Khuyến mãi.
- Trả hàng.
- Hoàn tiền.

## NFR-007 — Auditability

Các thay đổi quan trọng đối với tồn kho và hoạt động quản trị cần có khả năng truy vết.

Chi tiết audit log.

## NFR-008 — Usability

Giao diện phải hỗ trợ người dùng thực hiện quy trình mua sách rõ ràng và thuận tiện.

## NFR-009 — Maintainability

Hệ thống phải có khả năng bảo trì và mở rộng trong các giai đoạn tiếp theo.

## NFR-010 — Compatibility

Nền tảng web phải hoạt động trên các trình duyệt được Book Shop hỗ trợ.

---

# 8. YÊU CẦU DỮ LIỆU

## 8.1. Product

| Field | Mô tả | Required |
|---|---|---|
| Book ID | Định danh sách | Yes |
| Book Name | Tên sách | Yes |
| Author | Tác giả | No |
| Publisher | Nhà xuất bản | Yes |
| Category | Thể loại | Yes |
| Description | Mô tả | No |
| Image | Hình ảnh | No |
| Selling Price | Giá bán | Yes |
| Status | Trạng thái bán | Yes |

## 8.2. Customer

Thông tin Customer cần hỗ trợ:

- Thông tin tài khoản.
- Thông tin cá nhân.
- Địa chỉ giao hàng.
- Lịch sử mua hàng.

## 8.3. Order

Đơn hàng cần quản lý tối thiểu:

- Mã đơn.
- Khách hàng.
- Sản phẩm.
- Số lượng.
- Giá.
- Khuyến mãi.
- Tổng tiền.
- Địa chỉ giao hàng.
- Phương thức thanh toán.
- Trạng thái đơn hàng.
- Thông tin liên quan đến giao hàng.
- Thông tin hoàn tiền nếu có.

## 8.4. Inventory

Inventory phải quản lý:

- Số lượng hiện có.
- Số lượng đã giữ.
- Số lượng có thể bán.
- Số lượng hư hỏng.
- Số lượng chờ nhập.
- Cửa hàng/kho.
- Lịch sử thay đổi.

## 8.5. Review

Review phải chứa tối thiểu:

- Người đánh giá.
- Sản phẩm.
- Số sao.
- Nội dung nhận xét.
- Trạng thái kiểm duyệt.

## 8.6. Promotion

Promotion cần quản lý:

- Loại khuyến mãi.
- Giá trị giảm.
- Sản phẩm/thể loại áp dụng.
- Điều kiện đơn hàng.
- Thời gian hiệu lực.
- Số lần sử dụng.

---

# 9. QUY TRÌNH NGHIỆP VỤ VÀ TRẠNG THÁI

# 9.1. Order Lifecycle

Trạng thái tối thiểu:

```text
Chờ xác nhận
      ↓
Đang vận chuyển
      ↓
Hoàn tất
```

Ngoài ra, đơn hàng có thể có trạng thái:

```text
Hủy
```

theo điều kiện nghiệp vụ.

## 9.2. Order Cancellation Rule

Customer được phép hủy khi:

```text
Order Status != Đang vận chuyển
AND
Cancellation Policy = Allowed
```

Customer không được hủy:

```text
Order Status = Đang vận chuyển
```

## 9.3. Return & Refund Lifecycle

```text
Customer tạo yêu cầu
        ↓
Book Shop kiểm tra
        ↓
   ┌────┴────┐
   ↓         ↓
Chấp nhận   Từ chối
   ↓
Nhận hàng
   ↓
Kiểm tra hàng
   ↓
Hoàn tiền
   ↓
Cập nhật đơn hàng + hàng hóa
```

---

# 10. PHÂN QUYỀN VÀ KIỂM SOÁT TRUY CẬP

Ma trận quyền ở mức SRS:

| Chức năng | Guest | Customer | Employee | Store Manager | Admin |
|---|---:|---:|---:|---:|---:|
| Xem sách | ✓ | ✓ | ✓ | ✓ | ✓ |
| Tìm kiếm sách | ✓ | ✓ | ✓ | ✓ | ✓ |
| Quản lý giỏ hàng | - | ✓ | - | - | - |
| Đặt hàng | - | ✓ | - | - | - |
| Xem đơn của mình | - | ✓ | - | - | - |
| Xử lý đơn | - | - | ✓ | ✓* | ✓ |
| Quản lý sản phẩm | - | - | - | - | ✓ |
| Quản lý tồn kho | - | - | - | ✓ | ✓ |
| Quản lý cửa hàng | - | - | - | ✓* | ✓ |
| Quản lý khuyến mãi | - | - | - | - | ✓ |
| Kiểm duyệt review | - | - | - | - | ✓ |
| Báo cáo | - | - | TBD | ✓* | ✓ |
| Chatbot | - | ✓ | - | - | ✓ |

`*` Phạm vi quyền phải giới hạn theo cửa hàng/trách nhiệm được phân công.

Nguyên tắc chung:

> Người dùng chỉ được thực hiện công việc phù hợp với trách nhiệm được cấp.

Điều này tương ứng với BR-008 trong BRD.

---

# 11. YÊU CẦU GIAO DIỆN VÀ TRẢI NGHIỆM NGƯỜI DÙNG

## 11.1. Customer-facing pages

Hệ thống nên cung cấp các nhóm màn hình:

### Public

- Home.
- Book Listing.
- Search Result.
- Book Detail.
- Login.
- Register.

### Customer

- Profile.
- Address Management.
- Cart.
- Checkout.
- Order History.
- Order Detail.
- Return/Refund Request.
- Review.
- Notification.
- Chatbot.

### Back Office

- Dashboard.
- Product Management.
- Category Management.
- Order Management.
- Inventory Management.
- Store Management.
- Promotion Management.
- Review Moderation.
- Business Reports.

## 11.2. Checkout

Checkout phải thể hiện rõ:

- Sản phẩm.
- Số lượng.
- Địa chỉ.
- Phương thức thanh toán.
- Khuyến mãi.
- Tổng tiền.
- Nút xác nhận đơn.

Tổng tiền phải được xác định rõ trước khi Customer đặt hàng.

---

# 12. YÊU CẦU TÍCH HỢP

## 12.1. Third-party Shipping Provider

Hệ thống có liên quan đến bên thứ ba vận chuyển.

Phiên bản đầu:

- Book Shop chuyển đơn cho bên thứ ba.
- Bên thứ ba thực hiện vận chuyển.
- Khách hàng thanh toán khi nhận hàng.
- Book Shop nhận thông tin trạng thái giao hàng.
- Nhân viên xác nhận trạng thái giao hàng thành công trên hệ thống.

Book Shop **không quản lý hoạt động vận chuyển** trong phạm vi phiên bản đầu.

## 12.2. Payment Integration

Không yêu cầu payment gateway online trong phiên bản 1.0.

Payment method:

**COD**

## 12.3. Notification Integration

Hổ trợ gửi thông báo qua email

## 12.4. Chatbot

Chatbot là một thành phần hỗ trợ Customer:

- Tìm sách.
- Gợi ý sách.
- Trả lời câu hỏi sản phẩm.
---

# 13. YÊU CẦU BÁO CÁO

## 13.1. Revenue Report

Bộ lọc:

- Date.
- Store.
- Product.
- Category.

Khoảng thời gian:

- Day.
- Week.
- Month.
- Quarter.
- Year.

## 13.2. Order Report

Các chỉ số:

- New Orders.
- Processing Orders.
- Completed Orders.
- Cancelled Orders.

## 13.3. Inventory Report

Các chỉ số:

- Low Stock Books.
- Out-of-stock Books.
- Best-selling Books.
- Slow-moving Books.

## 13.4. Customer Report

Các chỉ số:

- New Customers.
- Returning Customers.
- Purchase Frequency.

---

# 14. TIÊU CHÍ CHẤP NHẬN

Hệ thống được xem là đáp ứng SRS khi các yêu cầu cốt lõi sau được kiểm thử thành công:

### AC-001 — Search

Customer/Guest có thể tìm kiếm sách.

### AC-002 — Product Detail

Người dùng có thể xem đầy đủ thông tin sách.

### AC-003 — Cart

Customer có thể:

- Thêm sách.
- Sửa số lượng.
- Xóa sách.
- Xem tổng tiền.

### AC-004 — Order

Customer có thể tạo đơn hàng thành công.

### AC-005 — Inventory Validation

Hệ thống không cho phép đặt vượt số lượng hàng có thể bán.

### AC-006 — Payment

Customer có thể chọn COD.

### AC-007 — Order Tracking

Customer có thể theo dõi trạng thái đơn hàng.

### AC-008 — Order Cancellation

Customer không thể hủy đơn khi đơn đang vận chuyển.

### AC-009 — Product Management

Administrator có thể quản lý sản phẩm.

### AC-010 — Order Management

Nhân viên có quyền có thể xử lý đơn hàng.

### AC-011 — Inventory Management

Book Shop có thể theo dõi tồn kho.

### AC-012 — Authorization

Người dùng không được truy cập chức năng ngoài quyền.

### AC-013 — Reporting

Ban quản lý có thể xem các báo cáo cần thiết.

### AC-014 — Review

Customer có thể đánh giá theo chính sách.

### AC-015 — Promotion

Hệ thống không áp dụng khuyến mãi đã hết hạn.

### AC-016 — Return/Refund

Hệ thống hỗ trợ quy trình yêu cầu trả hàng và hoàn tiền theo chính sách.

Các tiêu chí này được xây dựng từ tiêu chí nghiệm thu của BRD.

---

# 15. TRACEABILITY MATRIX

| BRD Requirement | SRS Requirement | Module |
|---|---|---|
| Quản lý tài khoản | FR-ACC-001 → FR-ACC-007 | Account |
| Quản lý sách | FR-PROD-001 → FR-PROD-005 | Product |
| Tìm kiếm | FR-SEARCH-001 → FR-SEARCH-003 | Search |
| Chi tiết sách | FR-PROD-006 → FR-PROD-008 | Product |
| Giỏ hàng | FR-CART-001 → FR-CART-005 | Cart |
| Đặt hàng | FR-ORDER-001 → FR-ORDER-005 | Order |
| Thanh toán | FR-PAY-001 → FR-PAY-003 | Payment |
| Quản lý đơn | FR-ORDER-006 → FR-ORDER-011 | Order |
| Trả hàng | FR-RETURN-001 → FR-RETURN-006 | Return/Refund |
| Tồn kho | FR-INV-001 → FR-INV-003 | Inventory |
| Cửa hàng | FR-STORE-001 → FR-STORE-007 | Store |
| Khuyến mãi | FR-PROMO-001 → FR-PROMO-007 | Promotion |
| Đánh giá | FR-REVIEW-001 → FR-REVIEW-004 | Review |
| Thông báo | FR-NOTI-001 | Notification |
| Chatbot | FR-CHAT-001 → FR-CHAT-004 | Chatbot |
| Báo cáo | FR-REPORT-001 → FR-REPORT-004 | Reporting |

---

# 16. CÁC VẤN ĐỀ CẦN LÀM RÕ

BRD hiện đã xác định khá đầy đủ phạm vi nghiệp vụ, nhưng trước khi chuyển sang thiết kế kỹ thuật cần làm rõ các vấn đề sau.

## 16.1. Chatbot

Cần xác định:

- Chatbot chỉ trả lời thông tin sách hay có thể truy cập đơn hàng?
- Chatbot có thể thực hiện thao tác đặt hàng không?
- Có lưu lịch sử hội thoại không?
- Có chuyển tiếp sang nhân viên CSKH không?

---

# 17. NGOÀI PHẠM VI

Các chức năng sau không thuộc phiên bản 1.0:

1. Marketplace bên thứ ba.
2. Livestream.
3. Cho thuê sách.
4. ERP/Kế toán chuyên sâu.
5. Mobile application riêng.
6. Quản lý vận chuyển nội bộ.
7. Thanh toán online qua ngân hàng.
8. Các tính năng khác chưa được xác định trong BRD.

Các chức năng này có thể được xem xét ở giai đoạn tiếp theo.

---

# 18. PHỤ LỤC

## 18.1. Glossary

| Term | Definition |
|---|---|
| BRD | Business Requirements Document |
| SRS | Software Requirements Specification |
| Guest | Người dùng chưa đăng nhập |
| Customer | Khách hàng có tài khoản |
| Employee | Nhân viên Book Shop |
| Store Manager | Quản lý cửa hàng |
| Administrator | Quản trị viên |
| Product/Book | Sản phẩm sách |
| Inventory | Tồn kho |
| Available Quantity | Số lượng hàng có thể bán |
| Reserved Quantity | Số lượng đã giữ cho đơn |
| Promotion | Chương trình khuyến mãi |
| COD | Cash on Delivery |
| Review | Đánh giá sản phẩm |
| Order | Đơn hàng |
| Return | Trả hàng |
| Refund | Hoàn tiền |
| Third-party Shipping Provider | Đơn vị vận chuyển bên thứ ba |

## 18.2. Requirement ID Convention

Các Requirement ID được quy ước:

```text
FR-ACC      Account
FR-PROD     Product
FR-SEARCH   Search
FR-CART     Cart
FR-ORDER    Order
FR-PAY      Payment
FR-RETURN   Return/Refund
FR-INV      Inventory
FR-STORE    Store
FR-PROMO    Promotion
FR-REVIEW   Review
FR-NOTI     Notification
FR-CHAT     Chatbot
FR-REPORT   Reporting
NFR         Non-functional Requirement
BR          Business Rule
AC          Acceptance Criteria
```

## 18.3. Requirement Priority

Có thể sử dụng quy ước:

| Priority | Meaning |
|---|---|
| Must Have | Bắt buộc để hệ thống hoạt động |
| Should Have | Quan trọng nhưng có thể trì hoãn |
| Could Have | Có thể triển khai nếu còn nguồn lực |
| Won't Have | Không thuộc phiên bản hiện tại |

Đối với phiên bản 1.0, các chức năng cốt lõi như:

- Product.
- Search.
- Cart.
- Order.
- COD.
- Inventory.
- Order Tracking.
- Authorization.
- Reporting.

được xem là nhóm chức năng trọng tâm dựa trên tiêu chí nghiệm thu của BRD.

---

# DOCUMENT CONTROL

| Item | Value |
|---|---|
| Document | Software Requirements Specification |
| Project | Book Shop |
| Version | 1.0 |
| Status | Draft for Review |
| Based On | Business Requirements Document v1.0 |
| Owner | Founder & CEO — Book Shop |
| Prepared By | Business Analysis |
| Reviewers | TBD |
| Approval Date | TBD |

---

# KẾT LUẬN

SRS này chuyển đổi yêu cầu nghiệp vụ của Book Shop thành các yêu cầu chức năng, yêu cầu phi chức năng, business rules, quyền truy cập, quy trình nghiệp vụ, yêu cầu dữ liệu, acceptance criteria và traceability.

SRS **không xác định thay cho tài liệu kiến trúc hoặc technical design** các nội dung như:

- Công nghệ.
- Database engine.
- Kiến trúc hệ thống.
- API endpoint.
- Framework.
- Infrastructure.
- Deployment.
- Chi tiết bảo mật kỹ thuật.

Các nội dung này sẽ được xác định trong các tài liệu kỹ thuật tương ứng ở giai đoạn tiếp theo.

**End of Document**
