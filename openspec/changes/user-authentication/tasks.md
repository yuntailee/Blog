# 使用者認證系統任務清單

## 1. 資料庫設定

- [ ] 1.1 建立 User 資料模型（C# Entity Framework Core，包含 totp_secret 欄位）
- [ ] 1.2 建立資料庫 Migration 腳本
- [ ] 1.3 執行 Migration 建立 Users 資料表
- [ ] 1.4 建立初始管理員帳號（seed 資料或手動建立）
- [ ] 1.5 為初始使用者生成 TOTP Secret

## 2. 後端核心功能

- [ ] 2.1 安裝必要的 NuGet 套件（JWT、Otp.NET、Entity Framework Core）
- [ ] 2.2 設定 JWT Authentication 在 Startup.cs/Program.cs
- [ ] 2.3 建立 User 資料模型和 DbContext
- [ ] 2.4 建立 TOTP Service（生成 Secret、生成 QR Code、驗證驗證碼）
- [ ] 2.5 建立 Authentication Service（處理登入邏輯、TOTP 驗證）
- [ ] 2.6 建立 Authentication Controller（登入、登出、TOTP 設定、取得使用者資訊端點）
- [ ] 2.7 實作 TOTP Secret 加密儲存功能（使用 AES 對稱加密）

## 3. 後端 API 端點

- [ ] 3.1 實作 POST /api/auth/login 端點（使用者名稱 + TOTP 驗證碼）
- [ ] 3.2 實作 GET /api/auth/setup 端點（取得 TOTP 設定資訊和 QR Code）
- [ ] 3.3 實作 POST /api/auth/verify-setup 端點（驗證 TOTP 設定）
- [ ] 3.4 實作 POST /api/auth/logout 端點（可選）
- [ ] 3.5 實作 GET /api/auth/me 端點（驗證 token 並回傳使用者資訊）
- [ ] 3.6 新增 API 錯誤處理（401 Unauthorized 回應）

## 4. 後端權限控制

- [ ] 4.1 在需要權限的 Controller/Action 加上 [Authorize] attribute
- [ ] 4.2 測試權限控制（確保未認證請求回傳 401）
- [ ] 4.3 設定 CORS 政策（允許前端域名）

## 5. 前端 Pinia Store

- [ ] 5.1 建立 auth store（Pinia）
- [ ] 5.2 實作 login action（發送登入請求、儲存 token）
- [ ] 5.3 實作 logout action（清除 token 和狀態）
- [ ] 5.4 實作 checkAuth action（檢查 localStorage 中的 token）
- [ ] 5.5 定義 state：isAuthenticated、user、token

## 6. 前端 TOTP 設定頁面

- [ ] 6.1 安裝 QR Code 生成庫（qrcode.js）
- [ ] 6.2 建立 TOTP 設定頁面元件（SetupTotp.vue）
- [ ] 6.3 實作 QR Code 顯示功能
- [ ] 6.4 實作 Secret 手動輸入顯示（備用方案）
- [ ] 6.5 實作驗證碼輸入和驗證功能
- [ ] 6.6 連接 API 取得 TOTP 設定資訊
- [ ] 6.7 處理設定錯誤訊息顯示

## 7. 前端登入頁面

- [ ] 7.1 建立登入頁面元件（Login.vue）
- [ ] 7.2 實作登入表單（帳號、TOTP 驗證碼輸入欄位）
- [ ] 7.3 實作表單驗證（必填欄位檢查、驗證碼格式檢查）
- [ ] 7.4 連接 auth store 的 login action
- [ ] 7.5 處理登入錯誤訊息顯示
- [ ] 7.6 處理 TOTP 未設定情況（引導使用者先設定）
- [ ] 7.7 登入成功後導向到管理頁面或首頁

## 8. 前端路由保護

- [ ] 8.1 安裝 Vue Router（如果尚未安裝）
- [ ] 8.2 建立路由守衛（beforeEach navigation guard）
- [ ] 8.3 在路由守衛中檢查認證狀態
- [ ] 8.4 未登入使用者導向登入頁面
- [ ] 8.5 登入後導向原本想存取的頁面（redirect）

## 9. 前端 API 整合

- [ ] 9.1 安裝 HTTP 客戶端庫（axios）
- [ ] 9.2 建立 API 服務檔案（api.js 或 api.ts）
- [ ] 9.3 設定 axios 攔截器（request interceptor：自動附加 Authorization header）
- [ ] 9.4 設定 axios 攔截器（response interceptor：處理 401 錯誤）
- [ ] 9.5 實作登入 API 呼叫函數
- [ ] 9.6 實作 TOTP 設定 API 呼叫函數
- [ ] 9.7 實作 TOTP 驗證設定 API 呼叫函數
- [ ] 9.8 實作登出 API 呼叫函數（可選）
- [ ] 9.9 實作取得使用者資訊 API 呼叫函數

## 10. 前端認證狀態管理

- [ ] 10.1 實作 token 儲存到 localStorage
- [ ] 10.2 實作應用程式啟動時檢查 token（App.vue 或 main.js）
- [ ] 10.3 實作 token 過期處理（清除 token 和狀態）
- [ ] 10.4 更新 UI 顯示登入狀態（導航列顯示登入/登出按鈕）

## 11. 測試和驗證

- [ ] 11.1 測試 TOTP 設定功能（QR Code 生成、Secret 儲存、驗證碼驗證）
- [ ] 11.2 測試登入功能（正確驗證碼、錯誤驗證碼、過期驗證碼、缺少欄位）
- [ ] 11.3 測試登出功能
- [ ] 11.4 測試 token 驗證（有效 token、無效 token、過期 token）
- [ ] 11.5 測試權限控制（已登入使用者可以 CUD、未登入只能 Read）
- [ ] 11.6 測試路由保護（未登入無法存取受保護路由）
- [ ] 11.7 測試頁面重新整理後保持登入狀態
- [ ] 11.8 測試 API 請求自動附加 token
- [ ] 11.9 測試 TOTP Secret 加密儲存和解密

## 12. 文件和完善

- [ ] 12.1 更新 API 文件（端點說明、請求/回應格式）
- [ ] 12.2 新增環境變數設定（JWT secret、過期時間、TOTP Secret 加密金鑰等）
- [ ] 12.3 新增錯誤處理和日誌記錄
- [ ] 12.4 編寫 TOTP 設定和使用說明文件
- [ ] 12.5 程式碼審查和重構（確保符合專案約定）
