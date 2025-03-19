# **Hướng dẫn chạy dự án Blazor Web App**

Dự án này là một ứng dụng Blazor Web App sử dụng PostgreSQL làm cơ sở dữ liệu và Tailwind CSS để tạo kiểu. Dưới đây là các bước để cấu hình và chạy dự án trên Visual Studio Code (VS Code) hoặc Visual Studio 2022.

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

---

## **2. Cấu hình và biên dịch Tailwind CSS**

Dự án sử dụng Tailwind CSS để tạo kiểu. Để biên dịch Tailwind CSS dynamically và theo dõi các thay đổi trong quá trình phát triển, bạn cần cài đặt các package cần thiết và chạy lệnh biên dịch.

### **Bước 1: Cài đặt Node.js và các package (nếu chưa cài)**
- Dự án sử dụng Node.js và npm để quản lý các package, bao gồm Tailwind CSS. Nếu bạn chưa cài Node.js, hãy tải và cài đặt từ [https://nodejs.org/](https://nodejs.org/).
- Sau khi cài Node.js, mở terminal trong thư mục gốc của dự án (nơi chứa file `package.json`) và chạy lệnh sau để cài đặt các package đã được định nghĩa:
  ```bash
  npm install
  ```
  - Lệnh này sẽ tải và cài đặt Tailwind CSS cùng các dependencies khác được liệt kê trong `package.json`.

### **Bước 2: Chạy lệnh biên dịch Tailwind CSS với chế độ theo dõi**
- Để biên dịch Tailwind CSS và theo dõi các thay đổi dynamically, bạn cần mở một terminal riêng (ngoài terminal đang chạy ứng dụng Blazor).
- Trong terminal mới, chạy lệnh sau tại thư mục gốc của dự án:
  ```bash
  npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.min.css --watch
  ```
  - Lệnh này sẽ:
    - Biên dịch file `./Styles/app.css` (nơi chứa các directive của Tailwind CSS).
    - Xuất file CSS đã biên dịch ra `./wwwroot/css/app.min.css`.
    - Theo dõi các thay đổi trong file `./Styles/app.css` và tự động biên dịch lại khi có thay đổi.

### **Lưu ý**
- Đảm bảo bạn có ít nhất hai terminal mở:
  - Một terminal để chạy ứng dụng Blazor (dùng `dotnet run` hoặc `dotnet watch`).
  - Một terminal để chạy lệnh biên dịch Tailwind CSS với `--watch`.
- Nếu bạn không chạy lệnh biên dịch Tailwind CSS, các thay đổi trong file `./Styles/app.css` sẽ không được phản ánh trong ứng dụng.

---

## **3. Chạy dự án**

Tùy theo công cụ bạn sử dụng (VS Code hoặc Visual Studio 2022), làm theo hướng dẫn bên dưới:

### **Cách 1: Sử dụng Visual Studio Code (VS Code)**
1. **Mở dự án**:
   - Mở VS Code.
   - Chọn **File > Open Folder** và chọn thư mục chứa dự án (`it-tools`).

2. **Mở hai terminal**:
   - Trong VS Code, nhấn `Ctrl + `` (dấu huyền) hoặc vào menu **Terminal > New Terminal** để mở terminal đầu tiên.
   - Nhấn biểu tượng "+" trong terminal để mở terminal thứ hai.

3. **Chạy ứng dụng và Tailwind CSS**:
   - **Terminal 1**: Chạy ứng dụng Blazor bằng một trong hai lệnh:
     - **`dotnet run`**:
       ```bash
       dotnet run
       ```
       - Build và chạy ứng dụng một lần.
     - **`dotnet watch`**:
       ```bash
       dotnet watch
       ```
       - Chạy ứng dụng với chế độ theo dõi thay đổi (hot reload).
   - **Terminal 2**: Chạy lệnh biên dịch Tailwind CSS:
     ```bash
     npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.min.css --watch
     ```

4. **Truy cập ứng dụng**:
   - Mở trình duyệt và truy cập địa chỉ như `http://localhost:5000` (hoặc địa chỉ được hiển thị trong terminal).

### **Cách 2: Sử dụng Visual Studio 2022**
1. **Mở dự án**:
   - Mở Visual Studio 2022.
   - Chọn **File > Open > Project/Solution**.
   - Duyệt đến thư mục dự án và chọn file **`it-tools.sln`**, sau đó nhấn **Open**.

2. **Chạy Tailwind CSS**:
   - Mở một terminal (Command Prompt, PowerShell, hoặc terminal trong VS Code) tại thư mục gốc của dự án.
   - Chạy lệnh biên dịch Tailwind CSS:
     ```bash
     npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.min.css --watch
     ```

3. **Chạy ứng dụng**:
   - Trong Visual Studio, nhấn phím **F5** hoặc chọn **Debug > Start Debugging** từ menu.
   - Trình duyệt sẽ mở với địa chỉ như `http://localhost:5000`.

---

## **Lưu ý**
- Đảm bảo PostgreSQL server đang chạy trên máy của bạn trước khi chạy dự án (dùng pgAdmin hoặc lệnh `psql` để kiểm tra).
- Nếu gặp lỗi liên quan đến kết nối cơ sở dữ liệu, kiểm tra lại chuỗi kết nối trong `appsettings.json`.
- Nếu cần tạo lại migration (ví dụ: sau khi thay đổi model), dùng lệnh:
  ```bash
  dotnet ef migrations add <Tên_Migration>
  dotnet ef database update
  ```
- Khi chỉnh sửa các file CSS hoặc component Blazor có sử dụng class Tailwind, lệnh biên dịch Tailwind CSS với `--watch` sẽ tự động cập nhật file `app.min.css`.

---

## **Tóm tắt các lệnh quan trọng**
- **Cài đặt dependencies Tailwind CSS**:
  ```bash
  npm install
  ```
- **Chạy ứng dụng Blazor**:
  ```bash
  dotnet run
  ```
  hoặc
  ```bash
  dotnet watch
  ```
- **Biên dịch Tailwind CSS với chế độ theo dõi**:
  ```bash
  npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.min.css --watch
  ```
- **Cập nhật cơ sở dữ liệu**:
  ```bash
  dotnet ef database update
  ```