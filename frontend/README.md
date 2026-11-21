# Stockroom — Vue 3 商品庫存系統

Node.js 22 LTS 建議。執行 `npm install`、`npm run dev`，開啟終端機顯示的網址。`npm run build` 建置正式版本。

預設使用 localStorage 與六筆示範商品。新增、編輯、刪除會保存在當前瀏覽器。此為前端練習，非正式多人庫存系統。

## ASP.NET Core API
複製 `.env.example` 為 `.env.local`，填入後端實際 URL，再重啟 Vite。後端需允許前端 origin 的 CORS。

- GET /api/products：回傳商品陣列
- POST /api/products：新增
- PUT /api/products/{id}：更新
- DELETE /api/products/{id}：刪除

欄位：id (number), sku, name, category (string), price, quantity (number)。POST 不含 id。寫入端點可回傳 JSON 或 204。後端須自行實作驗證、SKU 唯一性、權限及資料庫交易。前端目前使用完整清單進行本機分頁與篩選。
