# OCR 文字辨識系統

## 功能說明
- 支援多種圖片格式 (jpg, png, bmp等)
- 可辨識中文、英文、數字等文字
- 提供簡單易用的使用者介面
- 辨識結果可匯出成文字檔

## 系統需求
- Windows 10 或更新版本
- .NET 6.0 或更新版本
- 至少 4GB RAM

## 安裝說明
1. 下載最新版本的安裝檔
2. 執行安裝程式
3. 依照指示完成安裝

## 使用方式
1. 啟動程式
2. 選擇要辨識的圖片
3. 點擊「開始辨識」按鈕
4. 等待辨識完成
5. 檢視辨識結果

## 注意事項
- 圖片解析度建議在 300dpi 以上
- 文字需清晰可見
- 背景盡量單純


## 啟用專案指令
``` powershell
# 進入專案目錄
cd OCR_API 
# 建置專案
dotnet build
# 執行專案
dotnet run --project OCR_API.csproj
# 啟用前端
cd OCR_API/wwwroot
python -m http.server 5173
```

## UI 測試網址  python 前端框架網址
```
http://localhost:5173/test_web/debug.html
http://localhost:5173/test_web/import.html
http://localhost:5173/test_web/UseCarma.html
```


## swgger 網址

http://localhost:5262/swagger/index.html

