# 資料庫設定指南

## 問題：密碼驗證失敗

如果遇到 `password authentication failed for user "postgres"` 錯誤，請按照以下步驟設定：

## 步驟 1：確認 PostgreSQL 正在運行

```powershell
# 檢查 PostgreSQL 服務狀態
Get-Service -Name postgresql*
```

如果服務未運行，請啟動它。

## 步驟 2：更新資料庫連接字串

### 方式 1：更新 appsettings.Development.json（推薦）

編輯 `backend/appsettings.Development.json`，將 `YOUR_ACTUAL_PASSWORD_HERE` 替換為您的 PostgreSQL postgres 用戶密碼：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=blogdb;Username=postgres;Password=您的實際密碼"
  }
}
```

### 方式 2：使用環境變數（更安全）

在 PowerShell 中設定：

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Database=blogdb;Username=postgres;Password=您的實際密碼"
```

## 步驟 3：建立資料庫（如果不存在）

連接到 PostgreSQL 並建立資料庫：

```sql
-- 使用 psql 或 pgAdmin
CREATE DATABASE blogdb;
```

或者使用命令列：

```powershell
# 如果 PostgreSQL 在 PATH 中
psql -U postgres -c "CREATE DATABASE blogdb;"
```

## 步驟 4：執行 Migration

```powershell
cd backend
dotnet ef database update
```

## 常見問題

### 忘記 PostgreSQL 密碼？

1. 編輯 PostgreSQL 的 `pg_hba.conf` 檔案
2. 將認證方式改為 `trust`（僅用於重置密碼）
3. 重啟 PostgreSQL 服務
4. 使用 `psql` 連接並重置密碼：
   ```sql
   ALTER USER postgres WITH PASSWORD '新密碼';
   ```
5. 將 `pg_hba.conf` 改回原來的設定
6. 重啟 PostgreSQL 服務

### 使用不同的用戶名？

如果您的 PostgreSQL 用戶名不是 `postgres`，請更新連接字串：

```json
"DefaultConnection": "Host=localhost;Database=blogdb;Username=您的用戶名;Password=您的密碼"
```

### 使用不同的資料庫名稱？

如果資料庫名稱不是 `blogdb`，請更新連接字串中的 `Database` 參數。
