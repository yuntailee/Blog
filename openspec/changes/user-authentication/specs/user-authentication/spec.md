# 使用者認證系統規格

## ADDED Requirements

### Requirement: TOTP 設定功能
系統 SHALL 允許使用者設定 Google Authenticator。系統 SHALL 生成 TOTP Secret 和 QR Code，使用者 SHALL 可以掃描 QR Code 將帳號加入到 Google Authenticator。

#### Scenario: 取得 TOTP 設定資訊
- **WHEN** 使用者請求 TOTP 設定資訊（首次設定或重新設定）
- **THEN** 系統 SHALL 生成唯一的 TOTP Secret
- **THEN** 系統 SHALL 生成 QR Code（包含 Secret 和帳號資訊）
- **THEN** 系統 SHALL 回傳 QR Code 資料和 Secret（用於手動輸入）
- **THEN** 系統 SHALL 加密儲存 TOTP Secret 到資料庫

#### Scenario: 驗證 TOTP 設定
- **WHEN** 使用者掃描 QR Code 並提供第一個驗證碼
- **THEN** 系統 SHALL 驗證提供的驗證碼是否正確
- **THEN** 如果驗證碼正確，系統 SHALL 標記 TOTP 已設定完成
- **THEN** 如果驗證碼錯誤，系統 SHALL 回傳錯誤訊息，允許重新嘗試

### Requirement: 使用者登入功能
系統 SHALL 允許使用者使用帳號和 Google Authenticator TOTP 驗證碼進行登入（無密碼）。登入成功後，系統 SHALL 回傳 JWT token，前端 SHALL 儲存此 token 並在後續 API 請求中使用。

#### Scenario: 成功登入
- **WHEN** 使用者提供正確的帳號和有效的 TOTP 驗證碼
- **THEN** 系統 SHALL 回傳 HTTP 200 狀態碼
- **THEN** 回應內容 SHALL 包含 JWT token
- **THEN** 回應內容 SHALL 包含使用者基本資訊（可選）

#### Scenario: 登入失敗 - 帳號不存在
- **WHEN** 使用者提供不存在的帳號
- **THEN** 系統 SHALL 回傳 HTTP 401 狀態碼
- **THEN** 回應內容 SHALL 包含錯誤訊息

#### Scenario: 登入失敗 - TOTP 驗證碼錯誤
- **WHEN** 使用者提供正確的帳號但錯誤的 TOTP 驗證碼
- **THEN** 系統 SHALL 回傳 HTTP 401 狀態碼
- **THEN** 回應內容 SHALL 包含錯誤訊息（不應明確指出是帳號或驗證碼錯誤，以提升安全性）

#### Scenario: 登入失敗 - TOTP 未設定
- **WHEN** 使用者嘗試登入但尚未設定 TOTP
- **THEN** 系統 SHALL 回傳 HTTP 400 狀態碼
- **THEN** 回應內容 SHALL 包含提示訊息，引導使用者先設定 TOTP

#### Scenario: 登入失敗 - 缺少必要欄位
- **WHEN** 使用者未提供帳號或 TOTP 驗證碼
- **THEN** 系統 SHALL 回傳 HTTP 400 狀態碼
- **THEN** 回應內容 SHALL 包含驗證錯誤訊息

#### Scenario: 登入失敗 - TOTP 驗證碼過期
- **WHEN** 使用者提供已過期的 TOTP 驗證碼（超過時間視窗）
- **THEN** 系統 SHALL 回傳 HTTP 401 狀態碼
- **THEN** 回應內容 SHALL 包含錯誤訊息，提示使用者使用最新的驗證碼

### Requirement: 使用者登出功能
系統 SHALL 允許已登入的使用者登出。登出後，前端 SHALL 清除儲存的 token，使用者 SHALL 無法再存取需要認證的功能。

#### Scenario: 成功登出
- **WHEN** 已登入的使用者執行登出操作
- **THEN** 前端 SHALL 清除 localStorage 中的 token
- **THEN** 前端 SHALL 清除 Pinia store 中的認證狀態
- **THEN** 使用者 SHALL 被導向到登入頁面或首頁

### Requirement: Token 驗證
系統 SHALL 驗證所有包含 JWT token 的 API 請求。如果 token 有效，請求 SHALL 被允許；如果 token 無效或過期，請求 SHALL 被拒絕。

#### Scenario: Token 驗證成功
- **WHEN** API 請求包含有效的 JWT token
- **THEN** 系統 SHALL 允許請求繼續處理
- **THEN** 系統 SHALL 可以從 token 中取得使用者資訊

#### Scenario: Token 驗證失敗 - Token 不存在
- **WHEN** API 請求未包含 token
- **THEN** 系統 SHALL 回傳 HTTP 401 狀態碼
- **THEN** 回應內容 SHALL 包含 "Unauthorized" 錯誤訊息

#### Scenario: Token 驗證失敗 - Token 無效
- **WHEN** API 請求包含無效的 token（簽名錯誤、格式錯誤）
- **THEN** 系統 SHALL 回傳 HTTP 401 狀態碼
- **THEN** 回應內容 SHALL 包含 "Invalid token" 錯誤訊息

#### Scenario: Token 驗證失敗 - Token 過期
- **WHEN** API 請求包含已過期的 token
- **THEN** 系統 SHALL 回傳 HTTP 401 狀態碼
- **THEN** 回應內容 SHALL 包含 "Token expired" 錯誤訊息
- **THEN** 前端 SHALL 清除過期 token 並導向登入頁面

### Requirement: 權限控制 - 已登入使用者
已登入的使用者 SHALL 可以執行建立、更新、刪除（CUD）操作。

#### Scenario: 已登入使用者建立內容
- **WHEN** 已登入的使用者嘗試建立新內容（如文章）
- **THEN** 系統 SHALL 允許操作
- **THEN** 系統 SHALL 記錄建立者資訊

#### Scenario: 已登入使用者更新內容
- **WHEN** 已登入的使用者嘗試更新內容
- **THEN** 系統 SHALL 允許操作
- **THEN** 系統 SHALL 記錄更新時間

#### Scenario: 已登入使用者刪除內容
- **WHEN** 已登入的使用者嘗試刪除內容
- **THEN** 系統 SHALL 允許操作

### Requirement: 權限控制 - 未登入使用者
未登入的使用者 SHALL 只能執行讀取（Read）操作，SHALL 無法執行建立、更新、刪除操作。

#### Scenario: 未登入使用者讀取內容
- **WHEN** 未登入的使用者嘗試讀取公開內容（如文章列表、文章詳情）
- **THEN** 系統 SHALL 允許操作
- **THEN** 系統 SHALL 回傳內容資料

#### Scenario: 未登入使用者嘗試建立內容
- **WHEN** 未登入的使用者嘗試建立新內容
- **THEN** 系統 SHALL 回傳 HTTP 401 狀態碼
- **THEN** 系統 SHALL 拒絕操作
- **THEN** 回應內容 SHALL 包含 "Authentication required" 錯誤訊息

#### Scenario: 未登入使用者嘗試更新內容
- **WHEN** 未登入的使用者嘗試更新內容
- **THEN** 系統 SHALL 回傳 HTTP 401 狀態碼
- **THEN** 系統 SHALL 拒絕操作

#### Scenario: 未登入使用者嘗試刪除內容
- **WHEN** 未登入的使用者嘗試刪除內容
- **THEN** 系統 SHALL 回傳 HTTP 401 狀態碼
- **THEN** 系統 SHALL 拒絕操作

### Requirement: 前端路由保護
前端應用程式 SHALL 保護需要認證的路由，未登入的使用者 SHALL 無法存取這些路由。

#### Scenario: 未登入使用者存取受保護路由
- **WHEN** 未登入的使用者嘗試存取需要認證的路由（如文章管理頁面）
- **THEN** 前端路由守衛 SHALL 攔截請求
- **THEN** 使用者 SHALL 被導向到登入頁面
- **THEN** 登入成功後 SHALL 可以回到原本想存取的頁面

#### Scenario: 已登入使用者存取受保護路由
- **WHEN** 已登入的使用者嘗試存取需要認證的路由
- **THEN** 前端路由守衛 SHALL 允許存取
- **THEN** 頁面 SHALL 正常載入

### Requirement: TOTP Secret 安全儲存
系統 SHALL 使用安全的加密方式儲存 TOTP Secret，SHALL 不儲存明文 Secret。

#### Scenario: TOTP Secret 加密儲存
- **WHEN** 為使用者生成或更新 TOTP Secret
- **THEN** 系統 SHALL 使用對稱加密（如 AES）加密 TOTP Secret
- **THEN** 系統 SHALL 儲存加密後的 Secret
- **THEN** 系統 SHALL 不儲存明文 Secret
- **THEN** 加密金鑰 SHALL 儲存在安全的環境變數或配置中

#### Scenario: TOTP 驗證碼驗證
- **WHEN** 使用者嘗試登入並提供 TOTP 驗證碼
- **THEN** 系統 SHALL 解密儲存的 TOTP Secret
- **THEN** 系統 SHALL 使用 TOTP 演算法（RFC 6238）驗證提供的驗證碼
- **THEN** 系統 SHALL 允許前後時間視窗（±1 個時間步，約 ±30 秒）以處理時鐘不同步

### Requirement: 認證狀態持久化
前端應用程式 SHALL 在瀏覽器重新整理後保持使用者的登入狀態。

#### Scenario: 頁面重新整理後保持登入狀態
- **WHEN** 已登入的使用者重新整理頁面
- **THEN** 前端 SHALL 從 localStorage 讀取 token
- **THEN** 前端 SHALL 驗證 token 是否有效
- **THEN** 如果 token 有效，使用者 SHALL 保持登入狀態
- **THEN** 如果 token 無效或過期，使用者 SHALL 被視為未登入

### Requirement: API 請求自動附加 Token
前端應用程式 SHALL 在所有需要認證的 API 請求中自動附加 JWT token。

#### Scenario: API 請求包含 Token
- **WHEN** 已登入的使用者發送 API 請求
- **THEN** 前端 HTTP 攔截器 SHALL 自動在請求標頭中加入 Authorization header
- **THEN** Authorization header 的格式 SHALL 為 "Bearer <token>"

#### Scenario: API 請求處理認證錯誤
- **WHEN** API 請求回傳 401 狀態碼
- **THEN** 前端 SHALL 清除儲存的 token
- **THEN** 前端 SHALL 清除認證狀態
- **THEN** 前端 SHALL 導向使用者到登入頁面
