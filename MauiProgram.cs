using ALCM.Data;
using CommunityToolkit.Maui;
using Microsoft.Maui.Storage;   // FileSystem.AppDataDirectory を使う場合に必要

namespace ALCM
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("NotoSansJP-Regular.ttf", "FontRegular");
                });

            // LoanDatabase DI登録 非同期用
            builder.Services.AddSingleton<LoanDatabase>(sp =>
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "loans.db3");
                return new LoanDatabase(dbPath);
            });

            var app = builder.Build();

            // アプリ生成後に非同期でLoanDatabase初期化を実行
            var db = app.Services.GetRequiredService<LoanDatabase>();
            _ = db.InitializeAsync(); // fire-and-forget

            return app;
        }
    }
}
