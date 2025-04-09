# **Hướng dẫn chạy dự án Blazor Web App**

Dự án này là một ứng dụng Blazor Web App sử dụng PostgreSQL làm cơ sở dữ liệu và Tailwind CSS để tạo kiểu. Dưới đây là các bước để cấu hình và chạy dự án trên Visual Studio Code (VS Code) hoặc Visual Studio 2022.

---

## 1. Cấu hình cơ sở dữ liệu PostgreSQL
Trước khi chạy dự án, bạn cần cài đặt và cấu hình PostgreSQL làm cơ sở dữ liệu.

### Bước 1: Tải và cài đặt PostgreSQL
- Truy cập trang tải PostgreSQL tại: [https://www.postgresql.org/download/](https://www.postgresql.org/download/).
- Chọn phiên bản phù hợp với hệ điều hành của bạn (Windows, macOS, Linux) và làm theo hướng dẫn cài đặt.
- **Lưu ý**: Trong quá trình cài đặt, ghi lại thông tin:
  - **Username**: Mặc định thường là `postgres`.
  - **Password**: Do bạn đặt trong quá trình cài đặt.
  - **Tên database**: Bạn có thể tự đặt, ví dụ: `it_tools_db`.

### Bước 2: Cấu hình chuỗi kết nối trong `appsettings.json`
- Mở file `appsettings.json` trong thư mục gốc của dự án.
- Tìm phần `ConnectionStrings` và cập nhật chuỗi kết nối `DefaultConnection` với thông tin PostgreSQL của bạn. Ví dụ:
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=it_tools_db;Username=postgres;Password=your_password"
  }
  ```
  - Thay `it_tools_db` bằng tên database bạn đã tạo.
  - Thay `postgres` bằng username của bạn.
  - Thay `your_password` bằng password bạn đã đặt.

### Bước 3: Cài đặt công cụ `dotnet-ef` (nếu chưa cài)
- Nếu bạn chưa cài công cụ Entity Framework Core CLI (`dotnet-ef`), mở terminal và chạy lệnh sau để cài đặt toàn cục:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
- Kiểm tra cài đặt thành công bằng lệnh:
  ```bash
  dotnet ef --version
  ```

### Bước 4: Cập nhật cơ sở dữ liệu (Migrations)
- Nếu cơ sở dữ liệu chưa được tạo hoặc chưa áp dụng migration:
  - Mở terminal trong thư mục dự án (nơi chứa file `it-tools.csproj`).
  - Chạy lệnh sau để áp dụng migration và tạo các bảng trong PostgreSQL:
    ```bash
    dotnet ef database update
    ```

---

## 2. Cấu hình và biên dịch Tailwind CSS
Dự án sử dụng Tailwind CSS để tạo kiểu. Bạn cần cài đặt các package cần thiết và chạy lệnh biên dịch để theo dõi các thay đổi.

### Bước 1: Cài đặt Node.js và các package (nếu chưa cài)
- Dự án sử dụng Node.js và npm để quản lý Tailwind CSS. Nếu chưa cài Node.js, tải và cài đặt từ [https://nodejs.org/](https://nodejs.org/).
- Sau khi cài Node.js, mở terminal trong thư mục gốc của dự án (nơi chứa file `package.json`) và chạy:
  ```bash
  npm install
  ```
  Lệnh này sẽ cài đặt Tailwind CSS và các dependencies khác được liệt kê trong `package.json`.

### Bước 2: Chạy lệnh biên dịch Tailwind CSS với chế độ theo dõi
- Mở một terminal riêng (ngoài terminal chạy ứng dụng Blazor).
- Trong terminal mới, chạy lệnh sau tại thư mục gốc của dự án:
  ```bash
  npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.min.css --watch
  ```
  - Lệnh này sẽ:
    - Biên dịch file `./Styles/app.css` (chứa các directive của Tailwind CSS).
    - Xuất file CSS đã biên dịch ra `./wwwroot/css/app.min.css`.
    - Theo dõi các thay đổi trong file `./Styles/app.css` và tự động biên dịch lại khi có thay đổi.
- **Lưu ý**: Giữ terminal này chạy trong suốt quá trình phát triển để cập nhật CSS dynamically.

---

## 3. Chạy dự án
Tùy theo công cụ bạn sử dụng (VS Code hoặc Visual Studio 2022), làm theo hướng dẫn bên dưới:

### Cách 1: Sử dụng Visual Studio Code (VS Code)
- **Mở dự án**:
  - Mở VS Code.
  - Chọn `File > Open Folder` và chọn thư mục chứa dự án (`it-tools`).
- **Mở hai terminal**:
  - Nhấn `Ctrl + `` (dấu huyền)` hoặc vào `Terminal > New Terminal` để mở terminal đầu tiên.
  - Nhấn biểu tượng "+" trong terminal để mở terminal thứ hai.
- **Chạy ứng dụng và Tailwind CSS**:
  - **Terminal 1**: Chạy ứng dụng Blazor bằng một trong hai lệnh:
    - `dotnet run`: Build và chạy ứng dụng một lần.
    - `dotnet watch`: Chạy ứng dụng với chế độ theo dõi thay đổi (hot reload).
  - **Terminal 2**: Chạy lệnh biên dịch Tailwind CSS:
    ```bash
    npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.min.css --watch
    ```
- **Truy cập ứng dụng**:
  - Mở trình duyệt và truy cập địa chỉ như `http://localhost:5000` (hoặc địa chỉ được hiển thị trong terminal).

### Cách 2: Sử dụng Visual Studio 2022
- **Mở dự án**:
  - Mở Visual Studio 2022.
  - Chọn `File > Open > Project/Solution`.
  - Duyệt đến thư mục dự án, chọn file `it-tools.sln`, sau đó nhấn `Open`.
- **Chạy Tailwind CSS**:
  - Mở một terminal (Command Prompt, PowerShell, hoặc terminal trong VS Code) tại thư mục gốc của dự án.
  - Chạy lệnh:
    ```bash
    npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.min.css --watch
    ```
- **Chạy ứng dụng**:
  - Nhấn phím `F5` hoặc chọn `Debug > Start Debugging` từ menu.
  - Trình duyệt sẽ mở với địa chỉ như `http://localhost:5000`.

---

## 4. Tạo và tích hợp công cụ mới vào hệ thống
Dưới đây là các bước để tạo một công cụ mới dưới dạng Razor Class Library (RCL) và tích hợp vào dự án chính.

### Bước 1: Tạo dự án Razor Class Library (RCL) mới
- Mở terminal trong thư mục gốc của dự án (nơi chứa file `it-tools.sln`).
- Chạy lệnh sau để tạo một dự án RCL mới với tên `ToolName` và framework .NET 8.0:
  ```bash
  dotnet new razorclasslib -o ToolName --framework net8.0
  ```
  - Lệnh này sẽ tạo một thư mục mới tên `ToolName` chứa các file cần thiết cho Razor Class Library.

### Bước 2: Thêm tham chiếu đến dự án chính
- Để công cụ mới truy cập được các thành phần và dịch vụ từ dự án chính (`it-tools`), bạn cần thêm tham chiếu:
  - Điều hướng đến thư mục `ToolName` và mở file `ToolName.csproj` bằng trình soạn thảo văn bản (như VS Code).
  - Trong file `ToolName.csproj`, tìm phần `<ItemGroup>` và thêm đoạn mã sau:
    ```xml
    <ItemGroup>
      <Reference Include="it-tools">
        <HintPath>../it-tools/bin/Debug/net8.0/it-tools.dll</HintPath>
      </Reference>
    </ItemGroup>
    ```
  - **Lưu ý**:
    - Đường dẫn `../it-tools/bin/Debug/net8.0/it-tools.dll` giả định thư mục `ToolName` và `it-tools` nằm cùng cấp. Điều chỉnh đường dẫn nếu cấu trúc thư mục khác.
    - Thay `Debug` thành `Release` nếu bạn biên dịch ở chế độ Release.

### Bước 3: Phát triển giao diện và logic cho công cụ
- Trong thư mục `ToolName`, tạo một file Razor component mới, ví dụ: `ToolComponent.razor`.
- Viết mã HTML và C# trong `ToolComponent.razor` để định nghĩa giao diện và logic. Ví dụ:
  ```razor
  @page "/ToolName"

  <h3>Teen Tool</h3>

  <p>Đây là công cụ Teen Tool.</p>

  @code {
      // Logic của công cụ
  }
  ```
  - **Lưu ý**: Định nghĩa route (như `@page "/ToolName"`) để truy cập công cụ nếu cần.

### Bước 4: Biên dịch dự án RCL
- Mở terminal trong thư mục `ToolName`.
- Chạy lệnh biên dịch ở chế độ Release:
  ```bash
  dotnet build -c Release
  ```
  - Lệnh này tạo file `ToolName.dll` trong thư mục `bin/Release/net8.0`.

### Bước 5: Tích hợp công cụ vào dự án chính
- **Chạy ứng dụng chính**:
  - Trong thư mục `it-tools`, chạy:
    ```bash
    dotnet run
    ```
    hoặc
    ```bash
    dotnet watch
    ```
- **Truy cập giao diện admin**:
  - Mở trình duyệt và truy cập route `admin/tools/add` (ví dụ: `http://localhost:5000/admin/tools/add`). Đảm bảo bạn đã đăng nhập với quyền admin.
- **Tải file DLL lên**:
  - Trong giao diện admin, tìm phần tải lên file DLL.
  - Chọn file `ToolName.dll` từ thư mục `ToolName/bin/Release/net8.0` và tải lên.
- **Hoàn tất tích hợp**:
  - Sau khi tải lên, ứng dụng sẽ xử lý file DLL và tích hợp công cụ mới. Làm theo hướng dẫn trên giao diện nếu cần.

---

## Lưu ý quan trọng
- Đảm bảo PostgreSQL server đang chạy trước khi khởi động ứng dụng (kiểm tra bằng pgAdmin hoặc lệnh `psql`).
- Nếu gặp lỗi kết nối cơ sở dữ liệu, kiểm tra lại chuỗi kết nối trong `appsettings.json`.
- Đảm bảo dự án chính (`it-tools`) đã được biên dịch ít nhất một lần để tạo file `it-tools.dll`.
- Khi phát triển, bạn có thể dùng chế độ Debug (thay `Release` bằng `Debug` trong các lệnh và đường dẫn).

---

## Tóm tắt các lệnh quan trọng
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
- **Tạo dự án RCL mới**:
  ```bash
  dotnet new razorclasslib -o ToolName --framework net8.0
  ```
- **Biên dịch dự án RCL**:
  ```bash
  dotnet build -c Release
  ```
