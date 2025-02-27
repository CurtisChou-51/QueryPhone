# QueryPhone

- QueryPhone.Core 是一個 .NET Core 類別庫，用途為電話號碼查詢，輸入電話號碼並查詢特定網站，快速獲取相關資訊摘要，提供範例 WinForm 與 Web 介面
  - QueryPhone 為 WinForm 介面
  - QueryPhone.Web 為 Web 介面

![](screenshot.png)

## 概述

1. 輸入電話號碼：使用者在 WinForm 或 Web 介面中輸入電話號碼
2. 發送查詢請求：發送 HTTP 請求至特定網站查詢電話號碼相關資訊
3. 解析HTML回應：解析網站回應的 HTML 內容，提取所需的資訊摘要
4. 顯示查詢結果：在 WinForm 或 Web 介面中顯示查詢結果摘要


## 限制

- 程式可能會被反爬蟲機制阻擋，也不要過於頻繁地發送請求以免對伺服器造成負擔
- 查詢結果依賴於目標網站的 HTML 結構，若網站結構變更解析可能失敗
- 網站資料大多為使用者提交，資料可能不完整或不準確
- 僅供學習與測試使用，請勿用於不當用途
