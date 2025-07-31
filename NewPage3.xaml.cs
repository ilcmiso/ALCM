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

        /// <summary>
        /// 償還表を Excel に出力するボタン押下イベント。
        /// </summary>
        private async void BtnExcelOut_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is AmortizationViewModel vm)
            {
                try
                {
                    // 保存先パスを構築
                    string fileName = "LoanRepay.xlsx";
                    string folder = FileSystem.AppDataDirectory;
                    string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
#if ANDROID
                    string? downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
                    if (!string.IsNullOrEmpty(downloadsPath))
                        filePath = Path.Combine(downloadsPath, fileName);
#elif WINDOWS
                    string? downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
                    if (!string.IsNullOrEmpty(downloadsPath))
                        filePath = Path.Combine(Path.Combine(downloadsPath, "Downloads"), fileName);
#endif
                    // Excel 出力実行
                    await OutputExcel.SaveAmortizationAsync(vm.AmortizationItems, filePath);

                    // 出力完了後にファイルを開く確認ダイアログを表示
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

        /// <summary>
        /// 償還表を PDF に出力するボタン押下イベント。
        /// </summary>
        private async void BtnPDFOut_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is AmortizationViewModel vm)
            {
                try
                {
                    // 保存先パスを構築
                    string fileName = "LoanRepay.pdf";
                    string folder = FileSystem.AppDataDirectory;
                    string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
#if ANDROID
                    string? downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
                    if (!string.IsNullOrEmpty(downloadsPath))
                        filePath = Path.Combine(downloadsPath, fileName);
#elif WINDOWS
                    string? downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
                    if (!string.IsNullOrEmpty(downloadsPath))
                        filePath = Path.Combine(Path.Combine(downloadsPath, "Downloads"), fileName);
#endif
                    // PDF 出力実行
                    await OutputPdf.SaveAmortizationAsync(vm.AmortizationItems, filePath);

                    // 出力完了後にファイルを開く確認ダイアログを表示
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
