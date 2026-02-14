# 前端專案說明

## 技術棧

- **Vue 3** - 前端框架
- **Vue Router** - 路由管理
- **Pinia** - 狀態管理
- **Axios** - HTTP 請求
- **Tailwind CSS** - 樣式框架
- **Vite** - 建置工具

## 安裝與啟動

### 1. 安裝依賴

```bash
cd frontend
npm install
```

### 2. 啟動開發伺服器

```bash
npm run dev
```

前端將在 `http://localhost:5173` 啟動。

## 環境變數

可以在 `frontend` 目錄下建立 `.env` 檔案來設定環境變數：

```env
VITE_API_URL=http://localhost:5000/api
```

如果不設定，預設使用 `http://localhost:5000/api`。

## 功能說明

### 登入頁面 (`/login`)

- 使用使用者名稱和 TOTP 驗證碼登入
- 自動驗證碼格式化（只允許 6 位數字）
- 錯誤提示和狀態管理
- 如果未設定 TOTP，會提示前往設定頁面

### TOTP 設定頁面 (`/setup-totp`)

- 需要先登入才能訪問
- 顯示 QR Code 供 Google Authenticator 掃描
- 驗證 TOTP 設定

### 管理後台 (`/admin`)

- 需要登入才能訪問
- 顯示當前使用者資訊

## API 端點

前端會自動調用以下後端 API：

- `POST /api/Auth/login` - 登入
- `GET /api/Auth/me` - 獲取當前使用者資訊
- `POST /api/Auth/logout` - 登出
- `GET /api/Auth/setup` - 獲取 TOTP 設定資訊
- `POST /api/Auth/verify-setup` - 驗證 TOTP 設定

## 認證流程

1. 使用者輸入使用者名稱和 TOTP 驗證碼
2. 前端調用 `/api/Auth/login` API
3. 後端驗證 TOTP 並返回 JWT token
4. 前端將 token 儲存到 localStorage
5. 後續 API 請求自動在 Header 中附加 token
6. 如果 token 過期或無效，自動跳轉到登入頁面

## 路由守衛

- 需要認證的路由會自動檢查登入狀態
- 未登入的使用者會被重定向到登入頁面
- 登入後會自動跳轉回原本要訪問的頁面

## 注意事項

1. 確保後端 API 正在運行（`http://localhost:5000`）
2. 確保 CORS 設定正確（後端已配置允許 `http://localhost:5173`）
3. TOTP 驗證碼每 30 秒更新一次，請使用最新的驗證碼
