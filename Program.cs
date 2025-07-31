using OCR_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 設定日誌記錄
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 註冊 OCR 服務
builder.Services.AddSingleton<OcrService>(provider => 
{
    var logger = provider.GetService<ILogger<OcrService>>();
    var tessdataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
    return new OcrService(tessdataPath, logger);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 配置靜態檔案服務
app.UseStaticFiles();

// 設定靜態檔案路徑
app.UseDefaultFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
