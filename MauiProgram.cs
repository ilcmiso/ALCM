namespace ALCM
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("NotoSansJP-Regular.ttf", "FontRegular");
                });
            return builder.Build();
        }
    }
}
