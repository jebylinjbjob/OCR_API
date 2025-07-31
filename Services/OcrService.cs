using System;
using System.IO;
using System.Threading.Tasks;
using Tesseract;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace OCR_API.Services
{
    public class OcrService
    {
        private readonly string _tesseractDataPath;
        private readonly ILogger<OcrService> _logger;

        public OcrService(string tesseractDataPath, ILogger<OcrService> logger = null)
        {
            _tesseractDataPath = tesseractDataPath;
            _logger = logger;
        }

        public async Task<string> ExtractTextFromImageAsync(byte[] imageData, string language = "eng")
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger?.LogInformation($"開始OCR處理，語言: {language}，圖片大小: {imageData.Length} bytes");
                    
                    // 驗證語言包是否存在
                    string languagePath = Path.Combine(_tesseractDataPath, $"{language}.traineddata");
                    if (!File.Exists(languagePath))
                    {
                        _logger?.LogWarning($"語言包不存在: {languagePath}，嘗試使用英文");
                        language = "eng";
                    }

                    _logger?.LogInformation($"使用語言包: {language}");

                    using var engine = new TesseractEngine(_tesseractDataPath, language, EngineMode.Default);
                    
                    // 設定OCR引擎參數以提升識別準確度
                    engine.SetVariable("tessedit_pageseg_mode", "1"); // 自動分頁模式
                    engine.SetVariable("tessedit_ocr_engine_mode", "3"); // 預設模式
                    
                    // 移除字符白名單限制，讓Tesseract自動識別所有字符
                    // engine.SetVariable("tessedit_char_whitelist", "..."); // 移除這行
                    
                    using var img = Pix.LoadFromMemory(imageData);
                    
                    // 檢查圖片是否成功載入
                    if (img == null)
                    {
                        throw new Exception("無法載入圖片檔案");
                    }
                    
                    _logger?.LogInformation($"圖片載入成功，尺寸: {img.Width}x{img.Height}");
                    
                    using var page = engine.Process(img);
                    string result = page.GetText();
                    
                    _logger?.LogInformation($"OCR處理完成，識別文字長度: {result?.Length ?? 0}");
                    
                    // 如果沒有識別到文字，嘗試其他語言
                    if (string.IsNullOrWhiteSpace(result) && language != "eng")
                    {
                        _logger?.LogWarning("使用指定語言未識別到文字，嘗試使用英文");
                        return ExtractTextFromImageAsync(imageData, "eng").Result;
                    }
                    
                    // 如果仍然沒有結果，返回提示信息
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        _logger?.LogWarning("OCR未能識別到任何文字");
                        return "未識別到文字 - 請檢查圖片品質或嘗試其他語言";
                    }
                    
                    return result;
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"OCR處理失敗: {ex.Message}");
                    _logger?.LogError($"詳細錯誤: {ex.ToString()}");
                    throw new Exception($"OCR 處理失敗: {ex.Message}", ex);
                }
            });
        }

        public async Task<string> ExtractTextFromFileAsync(string imagePath, string language = "eng")
        {
            try
            {
                if (!File.Exists(imagePath))
                {
                    throw new FileNotFoundException($"圖片檔案不存在: {imagePath}");
                }
                
                byte[] imageData = await File.ReadAllBytesAsync(imagePath);
                return await ExtractTextFromImageAsync(imageData, language);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"檔案處理失敗: {ex.Message}");
                throw;
            }
        }

        // 檢查語言包是否可用
        public bool IsLanguageAvailable(string language)
        {
            string languagePath = Path.Combine(_tesseractDataPath, $"{language}.traineddata");
            return File.Exists(languagePath);
        }

        // 取得可用的語言列表
        public List<string> GetAvailableLanguages()
        {
            var languages = new List<string>();
            if (Directory.Exists(_tesseractDataPath))
            {
                var files = Directory.GetFiles(_tesseractDataPath, "*.traineddata");
                foreach (var file in files)
                {
                    string language = Path.GetFileNameWithoutExtension(file);
                    languages.Add(language);
                }
            }
            return languages;
        }

        // 新增：測試OCR功能
        public async Task<string> TestOcrFunctionality()
        {
            try
            {
                _logger?.LogInformation("開始測試OCR功能");
                
                // 檢查tessdata路徑
                if (!Directory.Exists(_tesseractDataPath))
                {
                    return $"錯誤: tessdata路徑不存在: {_tesseractDataPath}";
                }
                
                // 檢查語言包
                var availableLanguages = GetAvailableLanguages();
                if (availableLanguages.Count == 0)
                {
                    return "錯誤: 沒有找到任何語言包檔案";
                }
                
                return $"OCR功能正常，可用語言: {string.Join(", ", availableLanguages)}";
            }
            catch (Exception ex)
            {
                return $"OCR功能測試失敗: {ex.Message}";
            }
        }
    }
}