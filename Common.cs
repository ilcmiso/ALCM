namespace ALCM
{
    public static class Common
    {
        /// <summary>
        /// ファイル保存後、「開きますか？」と確認して Yes なら開く
        /// </summary>
        public static async Task ShowOpenFileDialogAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            // Application.CurrentがNullじゃなくて、Windowsが1個以上ならPage取得。なければNullとする
            var page = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0].Page : null;
            if (page == null) return;

            bool open = await page.DisplayAlert(
                "保存完了",
                $"償還表を {filePath} に保存しました。\nファイルを開きますか？",
                "Yes",
                "No");

            if (open)
            {
                try
                {
                    await Launcher.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(filePath)
                    });
                }
                catch (Exception ex)
                {
                    await page.DisplayAlert("エラー", $"ファイルを開けませんでした。\n{ex.Message}", "OK");
                }
            }
        }

    }
}
