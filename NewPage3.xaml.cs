using System.Collections.ObjectModel;
using ALCM.Models;
using ALCM.ViewModels;
#if ANDROID
using Android.OS;
using Android.Content;
using Java.IO;
#endif

namespace ALCM
{
    public partial class NewPage3 : ContentPage
    {
        // 返済表データを保持・UIにバインドするコレクション
        public ObservableCollection<AmortizationItem> AmortizationItems { get; } = [];

        public NewPage3()
        {
            InitializeComponent();
            BindingContext = new AmortizationViewModel();
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            // 新規登録画面を呼ぶなどの処理
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            // 更新処理（再読み込み・再計算など）
        }

        // 画面がロードされた際のイベント
        protected override void OnAppearing()
        {
            base.OnAppearing();

            var input = SharedLoanInputData.Current;
            if (input != null && BindingContext is AmortizationViewModel vm)
            {
                // 資金計算の一覧を作成
                var result = LoanCalculator.Generate(input);
                vm.AmortizationItems.Clear();
                foreach (var item in result)
                    vm.AmortizationItems.Add(new AmortizationItem
                    {
                        回数 = item.回数,
                        振替日 = item.振替日,
                        返済金額 = item.返済金額,
                        元金額 = item.元金額,
                        利息額 = item.利息額,
                        残高 = item.残高
                    });
            }
            else
            {
                AmortizationItems.Clear();
            }
        }

        // Excel,PDF出力ボタン押下イベント
        private async void BtnExport_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is AmortizationViewModel vm && sender is Button btn)
            {
                // ボタンから渡されたCommandParameterを参照
                string? format = btn.CommandParameter as string;
                if (string.IsNullOrEmpty(format))
                    return; // パラメータが無ければ何もしない

                try
                {
                    // 1. ファイル名と拡張子を決定
                    string extension = format.Equals("pdf", StringComparison.CurrentCultureIgnoreCase) ? "pdf" : "xlsx";
                    string fileName = $"LoanRepay.{extension}";

                    // 2. 保存先パスを生成（共通処理）
                    string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
#if ANDROID
            string? downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
                                        Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
            if (!string.IsNullOrEmpty(downloadsPath))
            {
                filePath = Path.Combine(downloadsPath, fileName);
            }
#elif WINDOWS
            string? userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(userProfile))
            {
                filePath = Path.Combine(Path.Combine(userProfile, "Downloads"), fileName);
            }
#endif

                    // 3. 出力処理の実行（形式に応じて切り替え）
                    if (format.Equals("EXCEL", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await OutputExcel.SaveAmortizationAsync(vm.AmortizationItems, filePath);
                    }
                    else if (format.Equals("PDF", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await OutputPdf.SaveAmortizationAsync(vm.AmortizationItems, filePath);
                    }
                    else
                    {
                        throw new NotSupportedException($"未対応の出力形式です: {format}");
                    }

                    // 4. 出力完了後、ファイルを開くか確認するダイアログを表示（共通処理）
                    await Common.ShowOpenFileDialogAsync(filePath);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("エラー", $"出力中にエラーが発生しました: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("データなし", "償還表データが見つかりませんでした。", "OK");
            }
        }
    }
}
