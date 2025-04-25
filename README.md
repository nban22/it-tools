# 🧭 Hướng Dẫn Chạy Dự Án Blazor Web App

**Dự án này là một ứng dụng Blazor Web App sử dụng PostgreSQL làm cơ sở dữ liệu và Tailwind CSS để tạo kiểu giao diện.**  
Tài liệu này hướng dẫn chi tiết cách cấu hình và chạy dự án trên **Visual Studio Code (VS Code)** hoặc **Visual Studio 2022**.

---

## 1. Cấu Hình Cơ Sở Dữ Liệu PostgreSQL

### 1.1. Tải và Cài Đặt PostgreSQL
- Truy cập trang [https://www.postgresql.org/download/](https://www.postgresql.org/download/) và tải phiên bản phù hợp.
- Ghi nhớ:
  - **Username:** Mặc định thường là `postgres`.
  - **Password:** Do bạn đặt trong quá trình cài.
  - **Tên database:** Ví dụ: `it_tools_db`.

---

### 1.2. Cấu Hình Chuỗi Kết Nối
- Mở file `appsettings.json` trong thư mục gốc của dự án.
- Cập nhật phần `ConnectionStrings` như sau:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=it_tools_db;Username=postgres;Password=your_password"
}
```

- **Lưu ý:**
  - Thay `it_tools_db`, `postgres`, `your_password` bằng thông tin thật của bạn.

---

### 1.3. Cài Đặt Công Cụ `dotnet-ef`

```bash
dotnet tool install --global dotnet-ef
```

- Kiểm tra cài đặt thành công:

```bash
dotnet ef --version
```

---

### 1.4. Cập Nhật Cơ Sở Dữ Liệu (Migrations)

```bash
dotnet ef database update
```

Thực hiện trong thư mục chứa file `.csproj` của dự án (ví dụ `it-tools.csproj`).

---

## 2. Cấu Hình và Biên Dịch Tailwind CSS

### 2.1. Cài Đặt Node.js và Các Package

- Cài Node.js từ [https://nodejs.org/](https://nodejs.org/)
- Cài dependencies:

```bash
npm install
```

---

### 2.2. Biên Dịch Tailwind CSS

```bash
npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.min.css --watch
```

- Lệnh này biên dịch và theo dõi file CSS.

---

## 3. Chạy Dự Án

### 3.1. Sử Dụng Visual Studio Code (VS Code)

- Mở dự án: `File > Open Folder > it-tools`

- Mở 2 terminal:
  - Terminal 1: Chạy ứng dụng Blazor:

    ```bash
    dotnet run
    ```

    hoặc:

    ```bash
    dotnet watch
    ```

  - Terminal 2: Biên dịch Tailwind:

    ```bash
    npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.min.css --watch
    ```

- Truy cập: [http://localhost:5000](http://localhost:5000)

---

### 3.2. Sử Dụng Visual Studio 2022

- Mở dự án qua `it-tools.sln`.
- Terminal: Biên dịch Tailwind:

```bash
npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.min.css --watch
```

- Chạy ứng dụng: Nhấn **F5** hoặc `Debug > Start Debugging`.

---

## 4. Tạo và Tích Hợp Công Cụ Mới (RCL)

### 4.1. Tạo Dự Án Razor Class Library

```bash
dotnet new razorclasslib -o ToolName --framework net8.0
```

---

### 4.2. Thêm Tham Chiếu Đến Dự Án Chính

Trong `ToolName.csproj`:

```xml
<ItemGroup>
  <Reference Include="it-tools">
    <HintPath>../it-tools/bin/Debug/net8.0/it-tools.dll</HintPath>
  </Reference>
</ItemGroup>
```

---

### 4.3. Phát Triển Công Cụ

Ví dụ: `ToolComponent.razor`

```razor
@page "/ToolName"

<h3>Teen Tool</h3>
<p>Đây là công cụ Teen Tool.</p>

@code {
    // Logic của công cụ
}
```

---

### 4.4. Biên Dịch Dự Án RCL

```bash
dotnet build -c Release
```

---

### 4.5. Tích Hợp Công Cụ

- Chạy ứng dụng chính:

```bash
dotnet run
```

- Truy cập [http://localhost:5000/admin/tools/add](http://localhost:5000/admin/tools/add)
- Tải file `ToolName.dll` từ `ToolName/bin/Release/net8.0/`.

---

## 5. Lưu Ý Quan Trọng

- PostgreSQL server cần **chạy trước** khi khởi động ứng dụng.
- Kiểm tra chuỗi kết nối nếu gặp lỗi.
- Biên dịch dự án chính để tạo `it-tools.dll`.
- Dùng **Debug mode** trong quá trình phát triển.

---

## 6. Tóm Tắt Các Lệnh Quan Trọng

| Tác vụ | Lệnh |
|-------|------|
| Cài đặt dependencies Tailwind CSS | `npm install` |
| Chạy ứng dụng Blazor | `dotnet run` hoặc `dotnet watch` |
| Biên dịch Tailwind CSS | `npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.min.css --watch` |
| Cập nhật cơ sở dữ liệu | `dotnet ef database update` |
| Tạo dự án RCL | `dotnet new razorclasslib -o ToolName --framework net8.0` |
| Biên dịch dự án RCL | `dotnet build -c Release` |
