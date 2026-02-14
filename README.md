# 部落格專案

這是一個使用 OpenSpec 規格驅動開發的個人部落格系統。

## 技術棧

### 前端
- Vue 3
- Pinia (狀態管理)
- Vite (建構工具)
- Tailwind CSS
- Axios (HTTP 客戶端)

### 後端
- C# ASP.NET Core
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Otp.NET (TOTP 驗證)

## 專案結構

```
Blog/
├── backend/          # C# ASP.NET Core API
├── frontend/         # Vue 3 應用程式
└── openspec/         # OpenSpec 規格文件
```

## 開發環境需求

### 後端
- .NET 8.0 SDK 或更高版本
- PostgreSQL 資料庫

### 前端
- Node.js 18+ 
- npm 或 pnpm

## 開始使用

### 後端設定
```bash
cd backend
dotnet restore
dotnet run
```

### 前端設定
```bash
cd frontend
npm install
npm run dev
```

## OpenSpec 變更

目前正在實作：**使用者認證系統（TOTP 無密碼認證）**

詳見：`openspec/changes/user-authentication/`
