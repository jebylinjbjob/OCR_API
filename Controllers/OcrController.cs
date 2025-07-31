using Microsoft.AspNetCore.Mvc;
using OCR_API.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OCR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OcrController : ControllerBase
    {
        private readonly OcrService _ocrService;
        private readonly ILogger<OcrController> _logger;

        public OcrController(OcrService ocrService, ILogger<OcrController> logger)
        {
            _ocrService = ocrService;
            _logger = logger;
        }

        [HttpPost("extract-text")]
        public async Task<IActionResult> ExtractText(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Error = "請上傳一個有效的圖片檔案", Code = "FILE_MISSING" });

            // 驗證檔案類型
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/tiff" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(new { Error = "不支援的檔案格式，請上傳圖片檔案", Code = "INVALID_FILE_TYPE", SupportedTypes = allowedTypes });

            // 驗證檔案大小 (10MB)
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { Error = "檔案大小不能超過10MB", Code = "FILE_TOO_LARGE", MaxSize = "10MB" });

            try
            {
                _logger.LogInformation($"開始處理檔案: {file.FileName}, 大小: {file.Length} bytes, 類型: {file.ContentType}");

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                byte[] imageData = stream.ToArray();
                
                string result = await _ocrService.ExtractTextFromImageAsync(imageData);
                
                _logger.LogInformation($"檔案處理完成: {file.FileName}, 識別文字長度: {result?.Length ?? 0}");
                
                return Ok(new { 
                    Text = result,
                    FileName = file.FileName,
                    FileSize = file.Length,
                    ProcessedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"處理檔案 {file.FileName} 時發生錯誤: {ex.Message}");
                return StatusCode(500, new { 
                    Error = $"OCR處理失敗: {ex.Message}", 
                    Code = "OCR_PROCESSING_ERROR",
                    FileName = file.FileName
                });
            }
        }

        [HttpPost("extract-text-with-language")]
        public async Task<IActionResult> ExtractTextWithLanguage(IFormFile file, string language = "eng")
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Error = "請上傳一個有效的圖片檔案", Code = "FILE_MISSING" });

            // 驗證檔案類型
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/tiff" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(new { Error = "不支援的檔案格式，請上傳圖片檔案", Code = "INVALID_FILE_TYPE", SupportedTypes = allowedTypes });

            // 驗證檔案大小 (10MB)
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { Error = "檔案大小不能超過10MB", Code = "FILE_TOO_LARGE", MaxSize = "10MB" });

            // 驗證語言是否可用
            if (!_ocrService.IsLanguageAvailable(language))
            {
                var availableLanguages = _ocrService.GetAvailableLanguages();
                return BadRequest(new { 
                    Error = $"指定的語言 '{language}' 不可用", 
                    Code = "LANGUAGE_NOT_AVAILABLE",
                    AvailableLanguages = availableLanguages,
                    RequestedLanguage = language
                });
            }

            try
            {
                _logger.LogInformation($"開始處理檔案: {file.FileName}, 語言: {language}, 大小: {file.Length} bytes");

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                byte[] imageData = stream.ToArray();
                
                string result = await _ocrService.ExtractTextFromImageAsync(imageData, language);
                
                _logger.LogInformation($"檔案處理完成: {file.FileName}, 語言: {language}, 識別文字長度: {result?.Length ?? 0}");
                
                return Ok(new { 
                    Text = result,
                    Language = language,
                    FileName = file.FileName,
                    FileSize = file.Length,
                    ProcessedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"處理檔案 {file.FileName} 時發生錯誤: {ex.Message}");
                return StatusCode(500, new { 
                    Error = $"OCR處理失敗: {ex.Message}", 
                    Code = "OCR_PROCESSING_ERROR",
                    Language = language,
                    FileName = file.FileName
                });
            }
        }

        // 診斷端點
        [HttpGet("diagnostics")]
        public IActionResult GetDiagnostics()
        {
            try
            {
                var availableLanguages = _ocrService.GetAvailableLanguages();
                var tessdataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
                
                return Ok(new {
                    Status = "OK",
                    AvailableLanguages = availableLanguages,
                    TessdataPath = tessdataPath,
                    TessdataExists = Directory.Exists(tessdataPath),
                    LanguageFiles = availableLanguages.Select(lang => new {
                        Language = lang,
                        FilePath = Path.Combine(tessdataPath, $"{lang}.traineddata"),
                        FileExists = _ocrService.IsLanguageAvailable(lang)
                    }),
                    ServerTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"診斷檢查失敗: {ex.Message}");
                return StatusCode(500, new { 
                    Error = $"診斷檢查失敗: {ex.Message}", 
                    Code = "DIAGNOSTICS_ERROR" 
                });
            }
        }

        // 測試端點
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { 
                Message = "OCR API 服務正常運行",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0"
            });
        }

        // 新增：OCR功能測試端點
        [HttpGet("test-ocr")]
        public async Task<IActionResult> TestOcr()
        {
            try
            {
                var result = await _ocrService.TestOcrFunctionality();
                return Ok(new { 
                    Message = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"OCR功能測試失敗: {ex.Message}");
                return StatusCode(500, new { 
                    Error = $"OCR功能測試失敗: {ex.Message}", 
                    Code = "OCR_TEST_ERROR" 
                });
            }
        }
    }
}