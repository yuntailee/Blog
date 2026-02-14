# TOTP QR Code 生成器使用說明

## 快速使用

1. **開啟 QR Code 生成器**
   - 在瀏覽器中開啟 `backend/qr-code-generator.html`
   - 或直接雙擊檔案

2. **確認資訊**
   - Secret 已自動填入：`MA3SQM2GIQFTQL2H7ZL62LY73WOWOUZX`
   - 使用者名稱：`admin`
   - 發行者：`Blog`

3. **生成 QR Code**
   - 點擊「生成 QR Code」按鈕
   - QR Code 會自動顯示

4. **掃描 QR Code**
   - 在手機上開啟 Google Authenticator
   - 點擊「+」新增帳號
   - 選擇「掃描 QR Code」
   - 掃描頁面上顯示的 QR Code

## 手動輸入方式（如果無法掃描）

如果無法掃描 QR Code，可以手動輸入：

1. 在 Google Authenticator 中選擇「手動輸入」
2. 填寫以下資訊：
   - **帳號名稱**：`Blog:admin`
   - **您的金鑰**：`MA3SQM2GIQFTQL2H7ZL62LY73WOWOUZX`
   - **類型**：選擇「基於時間」

## 取得驗證碼後

完成設定後，Google Authenticator 會顯示 6 位數驗證碼（每 30 秒更新一次）。

然後在 Swagger UI 中使用 `POST /api/Auth/initial-verify`：

```json
{
  "username": "admin",
  "secret": "MA3SQM2GIQFTQL2H7ZL62LY73WOWOUZX",
  "code": "123456"
}
```

（將 `123456` 替換為 Google Authenticator 中顯示的實際驗證碼）
