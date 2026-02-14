# 初始使用者設定指南

## 已完成的設定

✅ **初始使用者已建立**
- 使用者名稱：`admin`
- 電子郵件：`admin@blog.local`
- TOTP 狀態：尚未設定

✅ **臨時 API 端點已加入**
- `POST /api/auth/initial-setup` - 取得 TOTP 設定資訊（不需要認證）
- `POST /api/auth/initial-verify` - 驗證並完成 TOTP 設定（不需要認證）

## 設定 TOTP 的步驟

### 步驟 1：啟動後端 API

```powershell
cd "C:\Users\hp\OneDrive\桌面\format engineer\Blog\backend"
dotnet run
```

後端會在以下位置啟動：
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger` 或 `http://localhost:5000/swagger`

### 步驟 2：取得 TOTP 設定資訊

使用以下方式之一：

#### 方式 A：使用 Swagger UI（推薦）

1. 開啟瀏覽器，前往 `http://localhost:5000/swagger`
2. 找到 `POST /api/auth/initial-setup` 端點
3. 點擊 "Try it out"
4. 輸入請求內容：
   ```json
   {
     "username": "admin"
   }
   ```
5. 點擊 "Execute"
6. 複製回應中的 `secret` 和 `qrCodeUri`

#### 方式 B：使用 curl

```bash
curl -X POST http://localhost:5000/api/auth/initial-setup \
  -H "Content-Type: application/json" \
  -d '{"username":"admin"}'
```

#### 方式 C：使用 Postman

- Method: `POST`
- URL: `http://localhost:5000/api/auth/initial-setup`
- Headers: `Content-Type: application/json`
- Body (raw JSON):
  ```json
  {
    "username": "admin"
  }
  ```

**回應範例：**
```json
{
  "secret": "JBSWY3DPEHPK3PXP",
  "qrCodeUri": "otpauth://totp/Blog:admin?secret=JBSWY3DPEHPK3PXP&issuer=Blog",
  "username": "admin"
}
```

### 步驟 3：使用 Google Authenticator 掃描 QR Code

1. 在手機上開啟 **Google Authenticator** 應用程式
2. 點擊右下角的 **「+」** 按鈕
3. 選擇 **「掃描 QR Code」**
4. 掃描步驟 2 中取得的 `qrCodeUri`（您可以使用線上 QR Code 生成器將 URI 轉換為 QR Code）

   **或手動輸入：**
   - 選擇「手動輸入」
   - 帳號名稱：`Blog:admin`
   - 金鑰：輸入 `secret` 的值（例如：`JBSWY3DPEHPK3PXP`）
   - 類型：選擇「基於時間」

5. 完成後，Google Authenticator 會顯示 6 位數的驗證碼（每 30 秒更新一次）

### 步驟 4：驗證並完成設定

取得 Google Authenticator 中的 6 位數驗證碼後：

#### 方式 A：使用 Swagger UI

1. 在 Swagger UI 中找到 `POST /api/auth/initial-verify` 端點
2. 點擊 "Try it out"
3. 輸入請求內容（使用步驟 2 取得的 `secret` 和步驟 3 取得的驗證碼）：
   ```json
   {
     "username": "admin",
     "secret": "JBSWY3DPEHPK3PXP",
     "code": "123456"
   }
   ```
4. 點擊 "Execute"

#### 方式 B：使用 curl

```bash
curl -X POST http://localhost:5000/api/auth/initial-verify \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "secret": "JBSWY3DPEHPK3PXP",
    "code": "123456"
  }'
```

**回應範例：**
```json
{
  "message": "TOTP setup completed successfully",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@blog.local"
  }
}
```

**重要：** 請保存這個 `token`，它可以用於後續的 API 請求。

### 步驟 5：之後的登入

設定完成後，使用正常的登入端點：

#### 使用 Swagger UI

1. 找到 `POST /api/auth/login` 端點
2. 輸入：
   ```json
   {
     "username": "admin",
     "code": "123456"
   }
   ```
   （`code` 是 Google Authenticator 中當前的 6 位數驗證碼）

#### 使用 curl

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "code": "123456"
  }'
```

## 測試其他端點

設定完成後，您可以使用取得的 `token` 測試其他需要認證的端點：

### 取得當前使用者資訊

```bash
curl -X GET http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### 登出

```bash
curl -X POST http://localhost:5000/api/auth/logout \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## 注意事項

1. **臨時端點僅用於首次設定**：`initial-setup` 和 `initial-verify` 只能在 TOTP 未設定時使用
2. **安全性**：完成設定後，請確保這些臨時端點在生產環境中被移除或加上額外的安全檢查
3. **Token 過期時間**：預設為 24 小時，可在 `appsettings.json` 中調整
4. **QR Code 生成**：如果無法掃描 QR Code，可以使用 `secret` 手動輸入到 Google Authenticator

## 疑難排解

### 問題：驗證碼總是錯誤

- 確保手機時間與伺服器時間同步
- 確保使用最新的驗證碼（每 30 秒更新）
- 檢查是否正確掃描了 QR Code 或手動輸入的 Secret 是否正確

### 問題：無法連接 API

- 確認後端正在運行
- 檢查防火牆設定
- 確認使用的是正確的 URL（http://localhost:5000 或 https://localhost:5001）

### 問題：TOTP 已設定但無法登入

- 確認 Google Authenticator 中的帳號名稱正確
- 確認使用的是正確的使用者名稱
- 檢查後端日誌是否有錯誤訊息
