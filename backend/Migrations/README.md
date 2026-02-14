# 資料庫 Migration 說明

## 建立 Migration

```bash
cd backend
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 建立初始使用者

建立初始使用者有兩種方式：

### 方式 1：使用 Seed 資料（建議）

在 `Program.cs` 或建立一個 Seed 服務來建立初始使用者。

### 方式 2：手動建立

1. 先建立使用者（不設定 TOTP Secret）
2. 登入後端 API（需要先實作臨時認證或直接操作資料庫）
3. 呼叫 `/api/auth/setup` 取得 TOTP Secret 和 QR Code
4. 使用 Google Authenticator 掃描 QR Code
5. 呼叫 `/api/auth/verify-setup` 驗證並儲存 TOTP Secret

## 環境變數設定

請確保在 `appsettings.json` 或環境變數中設定：

- `ConnectionStrings:DefaultConnection` - PostgreSQL 連接字串
- `Jwt:Secret` - JWT 簽名金鑰（至少 32 字元）
- `Jwt:ExpirationHours` - Token 過期時間（小時）
- `Totp:EncryptionKey` - TOTP Secret 加密金鑰（至少 32 字元）
- `Frontend:Url` - 前端應用程式 URL（用於 CORS）
