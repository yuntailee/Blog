# 使用者認證系統設計文件

## Context

### 背景
這是一個全新的部落格專案，目前沒有任何認證機制。需要建立完整的使用者認證系統，作為後續功能（文章管理、留言系統）的基礎。

### 當前狀態
- 專案處於初始階段，尚未實作任何功能
- 前端使用 Vue 3 + Pinia + Vite
- 後端使用 C# (.NET/ASP.NET Core)
- 資料庫使用 PostgreSQL
- 沒有任何現有的認證機制或使用者資料

### 約束條件
- 必須支援前後端分離架構
- 使用無密碼認證（TOTP），需要安全的 TOTP Secret 儲存機制
- 必須支援跨域請求（CORS）
- 需要考慮未來的擴展性（可能支援多使用者或僅單一管理員）

### 利害關係人
- 專案開發者
- 未來的部落格管理員

## Goals / Non-Goals

**Goals:**
- 實作安全的無密碼使用者認證機制（使用 Google Authenticator TOTP）
- 建立權限控制系統，區分已登入和未登入使用者的權限
- 提供前後端一致的認證狀態管理
- 確保 TOTP Secret 安全儲存（加密）
- 提供 TOTP 設定流程（QR Code 生成）
- 建立可擴展的認證架構，方便未來新增功能

**Non-Goals:**
- 不實作使用者註冊功能（初期僅支援單一管理員帳號，可手動建立）
- 不實作密碼登入（完全使用 TOTP 無密碼認證）
- 不實作 TOTP Secret 重設功能（此版本不包含）
- 不實作多因素認證（MFA，因為 TOTP 已經是主要認證方式）
- 不實作社交媒體登入（OAuth）
- 不實作角色權限系統（此版本僅區分登入/未登入）

## Decisions

### 1. 認證機制：JWT Token vs Session

**決策：使用 JWT Token**

**理由：**
- 前後端分離架構，JWT 更適合無狀態 API
- 易於擴展，不需要伺服器端儲存 session
- 支援跨域請求
- 符合現代 Web 應用程式最佳實踐

**替代方案考慮：**
- Session-based：需要伺服器端儲存，增加複雜度，不適合分散式系統
- OAuth 2.0：過於複雜，不符合當前需求

### 2. TOTP 認證：Google Authenticator 整合

**決策：使用 TOTP (Time-based One-Time Password) 作為主要認證方式**

**理由：**
- 無密碼認證，提升安全性（不需要儲存和驗證密碼）
- 使用業界標準的 TOTP 演算法（RFC 6238）
- Google Authenticator 廣泛支援，使用者體驗良好
- 驗證碼每 30 秒更新，即使被截獲也很快失效
- 適合個人部落格的使用場景

**實作方式：**
- 使用 Otp.NET 或 GoogleAuthenticator NuGet 套件
- 為每個使用者生成唯一的 TOTP Secret
- 使用 QR Code 讓使用者掃描設定 Google Authenticator
- 登入時驗證使用者提供的 6 位數驗證碼

**TOTP Secret 儲存：**
- 使用對稱加密（如 AES）加密儲存 TOTP Secret
- 加密金鑰儲存在環境變數或安全配置中
- 不儲存明文 Secret

**替代方案考慮：**
- 密碼登入：安全性較低，需要處理密碼重設等複雜流程
- WebAuthn/Passkeys：更現代但需要瀏覽器支援，實作複雜度較高
- SMS 驗證碼：需要第三方服務，有成本且安全性較低

### 3. Token 儲存位置：localStorage vs sessionStorage vs httpOnly Cookie

**決策：前端使用 localStorage，後端驗證 JWT**

**理由：**
- localStorage 提供持久化儲存，使用者重新整理頁面後仍保持登入
- 適合單頁應用程式（SPA）
- 前端可以輕鬆存取 token 並附加到 API 請求

**安全考量：**
- JWT 包含過期時間，降低長期風險
- 後端驗證 token 簽名，防止偽造
- 未來可考慮實作 refresh token 機制

**替代方案考慮：**
- httpOnly Cookie：更安全但需要處理 CSRF 保護，增加複雜度
- sessionStorage：關閉瀏覽器後失效，使用者體驗較差

### 4. 前端狀態管理：Pinia Store 結構

**決策：建立獨立的 auth store**

**理由：**
- 分離關注點，認證邏輯獨立管理
- 易於在整個應用程式中存取認證狀態
- 支援響應式更新，元件自動反映登入狀態變化

**Store 結構：**
- `isAuthenticated`: 布林值，表示是否已登入
- `user`: 使用者資訊物件（可選）
- `token`: JWT token 字串
- Actions: `login()`, `logout()`, `checkAuth()`

### 5. 後端 API 設計：RESTful 端點

**決策：使用 RESTful API 設計**

**端點設計：**
- `POST /api/auth/login` - 登入（使用者名稱 + TOTP 驗證碼）
- `GET /api/auth/setup` - 取得 TOTP 設定資訊（QR Code、Secret，僅首次設定時使用）
- `POST /api/auth/verify-setup` - 驗證 TOTP 設定（確認使用者已正確設定 Google Authenticator）
- `POST /api/auth/logout` - 登出（可選，主要在前端清除 token）
- `GET /api/auth/me` - 取得當前使用者資訊（驗證 token）

**理由：**
- 符合 REST 標準，易於理解和維護
- 清晰的端點命名
- 易於擴展
- 分離 TOTP 設定和登入流程

### 6. 權限檢查：Middleware vs Attribute

**決策：使用 `[Authorize]` Attribute + JWT Authentication Middleware**

**理由：**
- ASP.NET Core 內建支援，標準做法
- 易於在 Controller 或 Action 層級套用
- 清晰的權限控制語意

**實作方式：**
- 在 `Startup.cs` 或 `Program.cs` 中設定 JWT Authentication
- 在需要權限的 Controller/Action 上加上 `[Authorize]` attribute
- 前端路由守衛檢查認證狀態

## Risks / Trade-offs

### 風險 1：JWT Token 被竊取
**風險描述：** 如果 token 被惡意取得，攻擊者可以冒充使用者
**緩解措施：**
- 使用 HTTPS 傳輸
- Token 設定合理的過期時間（建議 24 小時）
- 未來可實作 refresh token 機制
- 實作 token 黑名單（登出時）

### 風險 2：XSS 攻擊導致 token 洩露
**風險描述：** 如果前端有 XSS 漏洞，攻擊者可能取得 localStorage 中的 token
**緩解措施：**
- 前端輸入驗證和清理
- 使用 Content Security Policy (CSP)
- 考慮未來遷移到 httpOnly Cookie

### 風險 3：TOTP Secret 洩露或遺失
**風險描述：** 如果 TOTP Secret 被洩露，攻擊者可以生成有效的驗證碼；如果使用者遺失手機，無法登入
**緩解措施：**
- TOTP Secret 使用加密儲存，加密金鑰安全保管
- 提供備份碼機制（可選，未來實作）
- 在設定 TOTP 時提醒使用者備份 Secret 或使用多個裝置
- 提供管理員重置機制（需要額外驗證）

### 權衡 1：簡單性 vs 安全性
**權衡：** 選擇了較簡單的 JWT 方案而非更複雜的 Session + httpOnly Cookie
**理由：** 符合當前專案規模和需求，未來可以升級

### 權衡 2：單一管理員 vs 多使用者
**權衡：** 初期僅支援單一管理員，不實作註冊功能
**理由：** 簡化開發，符合個人部落格的使用場景，未來可以擴展

## Migration Plan

### 階段 1：資料庫準備
1. 建立 User 資料表（使用 Entity Framework Core Migration）
2. 手動建立初始管理員帳號（或提供 seed 腳本）
3. 為初始使用者生成 TOTP Secret 和 QR Code

### 階段 2：後端實作
1. 安裝 TOTP 驗證庫（Otp.NET）
2. 設定 JWT Authentication Middleware
3. 實作 TOTP 服務（生成 Secret、驗證驗證碼）
4. 實作 Authentication Controller（登入、TOTP 設定）
5. 實作 User Service 和 Repository
6. 實作 TOTP Secret 加密儲存
7. 在需要權限的端點加上 `[Authorize]` attribute

### 階段 3：前端實作
1. 安裝 QR Code 生成庫（qrcode.js）
2. 建立 Pinia auth store
3. 建立 TOTP 設定頁面元件（顯示 QR Code）
4. 建立登入頁面元件（使用者名稱 + TOTP 驗證碼）
5. 實作路由守衛
6. 實作 API 請求攔截器（附加 token）
7. 更新 UI 顯示登入狀態

### 階段 4：測試和部署
1. 測試 TOTP 設定流程（QR Code 生成、Secret 儲存）
2. 測試登入流程（正確驗證碼、錯誤驗證碼、過期驗證碼）
3. 測試登出流程
4. 測試權限控制（已登入 vs 未登入）
5. 測試 token 過期處理
6. 測試 TOTP Secret 加密儲存
7. 部署到開發/測試環境

### 回滾策略
- 如果認證系統有問題，可以暫時移除 `[Authorize]` attributes
- 保留資料庫 migration 腳本，可以回滾資料表變更
- 前端可以透過環境變數控制是否啟用認證

## Open Questions

1. **Token 過期時間：** 應該設定多長？建議 24 小時，但需要根據實際使用情況調整
2. **Refresh Token：** 是否需要實作 refresh token 機制？初期可以省略，未來再新增
3. **使用者資訊：** 登入後是否需要儲存使用者詳細資訊（如 email、頭像）？目前僅需要知道是否已登入
4. **登出機制：** 是否需要後端登出端點？目前主要在前端清除 token，但未來可能需要 token 黑名單
5. **TOTP 視窗容差：** 驗證碼驗證時是否允許前後時間視窗（±1 個時間步）？建議允許，以處理時鐘不同步問題
6. **備份碼：** 是否需要提供備份碼機制，以防使用者遺失手機？初期可以省略，未來再新增
7. **TOTP 設定流程：** 是否需要在首次登入時強制設定 TOTP？建議是，確保安全性
