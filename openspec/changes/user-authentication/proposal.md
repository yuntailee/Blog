# 使用者認證系統提案

## Why

部落格系統需要使用者認證功能來保護內容的建立、編輯和刪除操作。目前系統沒有任何權限控制機制，任何人都可以修改內容。建立使用者認證系統後，只有已登入的使用者才能執行建立、更新、刪除（CUD）操作，而未登入的使用者只能讀取（Read）內容。這是整個部落格系統的基礎功能，必須優先實作，因為後續的文章管理、留言系統等功能都依賴於此認證機制。

## What Changes

- **新增使用者認證功能**：
  - 使用者註冊（可選，或僅支援單一管理員帳號）
  - 使用者登入（使用 Google Authenticator TOTP 驗證碼，無密碼）
  - TOTP Secret 設定和 QR Code 生成
  - 使用者登出
  - Session/Token 管理

- **新增權限控制機制**：
  - 後端 API 端點需要驗證使用者身份
  - 前端路由保護，未登入使用者無法存取管理頁面
  - 區分已登入和未登入使用者的功能權限

- **新增使用者資料模型**：
  - 使用者資料表（User table）
  - TOTP Secret 安全儲存（加密）
  - Session/Token 儲存機制

## Capabilities

### New Capabilities
- `user-authentication`: 使用者認證和授權功能，包括登入、登出、權限驗證
  - 登入功能：驗證使用者帳號和 Google Authenticator TOTP 驗證碼（無密碼）
  - TOTP 設定功能：生成 TOTP Secret 和 QR Code，供使用者掃描設定 Google Authenticator
  - 登出功能：清除使用者 session/token
  - 權限驗證：檢查使用者是否已登入，以及是否有權限執行特定操作
  - Session 管理：維護使用者的登入狀態

### Modified Capabilities
（目前沒有需要修改的現有能力，因為這是全新的專案）

## Impact

### 後端影響
- **C# API 專案**：
  - 新增 Authentication Controller 處理登入/登出請求
  - 新增 JWT Token 或 Session 認證中介軟體
  - 所有需要權限的 API 端點都需要加上認證檢查
  - 資料庫新增 User 資料表
  - 使用 Entity Framework Core 建立 User 模型

### 前端影響
- **Vue 應用程式**：
  - 新增登入頁面元件
  - 新增認證狀態管理（使用 Pinia store）
  - 新增路由守衛（Route Guards）保護需要登入的頁面
  - API 請求攔截器，自動附加認證 token
  - 登入狀態的持久化儲存（localStorage/sessionStorage）

### 資料庫影響
- 新增 `Users` 資料表，包含：
  - id (主鍵)
  - username (使用者名稱，唯一)
  - totp_secret (加密後的 TOTP Secret，用於 Google Authenticator)
  - email (電子郵件，可選)
  - created_at (建立時間)
  - updated_at (更新時間)

### 依賴影響
- 後端需要新增 JWT 或 Session 管理套件
- 後端需要新增 TOTP 驗證庫（如 Otp.NET 或 GoogleAuthenticator）
- 前端需要新增 HTTP 客戶端庫（如 axios）處理認證請求
- 前端需要新增 QR Code 生成庫（如 qrcode.js）顯示 TOTP 設定 QR Code

### 系統架構影響
- 建立認證服務層（Authentication Service）
- 建立權限檢查中介軟體
- 前端和後端都需要處理認證錯誤（401 Unauthorized）
