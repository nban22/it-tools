# **Hướng dẫn chạy dự án Blazor Web App**

Dự án này là một ứng dụng Blazor Web App sử dụng PostgreSQL làm cơ sở dữ liệu. Dưới đây là các bước để cấu hình và chạy dự án trên Visual Studio Code (VS Code) hoặc Visual Studio 2022.

---

## **1. Cấu hình cơ sở dữ liệu PostgreSQL**
Trước khi chạy dự án, bạn cần cài đặt và cấu hình PostgreSQL làm cơ sở dữ liệu.

### **Bước 1: Tải và cài đặt PostgreSQL**
- Truy cập trang tải PostgreSQL tại: [https://www.postgresql.org/download/](https://www.postgresql.org/download/).
- Chọn phiên bản phù hợp với hệ điều hành của bạn (Windows, macOS, Linux) và làm theo hướng dẫn cài đặt.
- Trong quá trình cài đặt:
  - **Lưu ý**: Ghi lại thông tin **username** (mặc định thường là `postgres`), **password**, và tên **database** (bạn có thể tự đặt, ví dụ: `it_tools_db`) vì sẽ cần để cấu hình sau.

### **Bước 2: Cấu hình chuỗi kết nối trong `appsettings.json`**
- Mở file **`appsettings.json`** trong thư mục gốc của dự án.
- Tìm phần **`ConnectionStrings`** và cập nhật chuỗi kết nối `DefaultConnection` với thông tin PostgreSQL của bạn. Ví dụ:
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=it_tools_db;Username=postgres;Password=your_password"
  }
  ```
  - Thay `it_tools_db` bằng tên database bạn đã tạo.
  - Thay `postgres` bằng username của bạn.
  - Thay `your_password` bằng password bạn đã đặt.

### **Bước 3: Cài đặt công cụ `dotnet-ef` (nếu chưa cài)**
- Nếu bạn chưa cài công cụ Entity Framework Core CLI (`dotnet-ef`), mở terminal và chạy lệnh sau để cài đặt toàn cục:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
- Kiểm tra cài đặt thành công bằng lệnh:
  ```bash
  dotnet ef --version
  ```

### **Bước 4: Cập nhật cơ sở dữ liệu (Migrations)**
- Nếu cơ sở dữ liệu chưa được tạo hoặc chưa áp dụng migration:
  1. Mở terminal trong thư mục dự án (nơi chứa file `it-tools.csproj`).
  2. Chạy lệnh sau để áp dụng migration và tạo các bảng trong PostgreSQL:
     ```bash
     dotnet ef database update
     ```
  - Lệnh này sẽ tạo các bảng cần thiết (như `AspNetUsers`, `AspNetRoles`, v.v.) dựa trên migration đã có trong thư mục `Migrations`.

---

## **2. Chạy dự án**

Tùy theo công cụ bạn sử dụng (VS Code hoặc Visual Studio 2022), làm theo hướng dẫn bên dưới:

### **Cách 1: Sử dụng Visual Studio Code (VS Code)**
1. **Mở dự án**:
   - Mở VS Code.
   - Chọn **File > Open Folder** và chọn thư mục chứa dự án (`it-tools`).

2. **Mở terminal**:
   - Trong VS Code, nhấn `Ctrl + `` (dấu huyền) hoặc vào menu **Terminal > New Terminal**.

3. **Chạy ứng dụng**:
   - Dùng một trong hai lệnh sau trong terminal:
     - **`dotnet run`**:
       ```bash
       dotnet run
       ```
       - Build và chạy ứng dụng một lần. Sau khi chạy, truy cập địa chỉ như `http://localhost:5000` trên trình duyệt.
     - **`dotnet watch`**:
       ```bash
       dotnet watch
       ```
       - Chạy ứng dụng với chế độ theo dõi thay đổi (hot reload). Khi bạn chỉnh sửa code, ứng dụng sẽ tự động khởi động lại.

### **Cách 2: Sử dụng Visual Studio 2022**
1. **Mở dự án**:
   - Mở Visual Studio 2022.
   - Chọn **File > Open > Project/Solution**.
   - Duyệt đến thư mục dự án và chọn file **`it-tools.sln`**, sau đó nhấn **Open**.

2. **Chạy ứng dụng**:
   - Nhấn phím **F5** hoặc chọn **Debug > Start Debugging** từ menu.
   - Visual Studio sẽ tự động build và chạy dự án. Trình duyệt sẽ mở với địa chỉ như `http://localhost:5000`.

---

## **Lưu ý**
- Đảm bảo PostgreSQL server đang chạy trên máy của bạn trước khi chạy dự án (dùng pgAdmin hoặc lệnh `psql` để kiểm tra).
- Nếu gặp lỗi liên quan đến kết nối cơ sở dữ liệu, kiểm tra lại chuỗi kết nối trong `appsettings.json`.
- Nếu cần tạo lại migration (ví dụ: sau khi thay đổi model), dùng lệnh:
  ```bash
  dotnet ef migrations add <Tên_Migration>
  dotnet ef database update
  ```