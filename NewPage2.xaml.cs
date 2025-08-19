namespace ALCM;
using ALCM.Models;
using System.Collections.ObjectModel;

public partial class NewPage2 : ContentPage
{
	public NewPage2()
	{
		InitializeComponent();
        UpdateRepayments();
    }

    // 画面がアンロードされたとき
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        UpdateLoanInputData();
    }

    // 入力欄でフォーカスアウト 金額
    private void Entry_LoanAmount_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is not Entry entry) return;
        ClampEntryValueInt(entry, 0, 100000, 3000); // 1万～10億 初期3000万
        UpdateRepayments();
    }

    // 入力欄でフォーカスアウト 年数
    private void Entry_LoanYears_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is not Entry entry) return;
        ClampEntryValueInt(entry, 0, 50, 35); // デフォルト35
        UpdateRepayments();
    }

    // 入力欄でフォーカスアウト 金利
    private void Entry_InterestRate_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is not Entry entry) return;
        UpdateRepayments();

        string raw = entry.Text.Trim();
        if (string.IsNullOrEmpty(raw))
        {
            entry.Text = "0";
            return;
        }

        if (raw.Contains('.'))
        {
            if (double.TryParse(raw, out double value))
            {
                value = Math.Clamp(value, 1, 30);
                entry.Text = value.ToString("F3");
            }
            else
            {
                entry.Text = "0";
            }
        }
        else
        {
            if (int.TryParse(raw, out int intVal))
            {
                if (raw.Length < 4)
                {
                    raw = raw.PadRight(4, '0');
                    intVal = int.Parse(raw);
                }

                double value = intVal / 1000.0;
                value = Math.Clamp(value, 1, 30);
                entry.Text = value.ToString("F3");
            }
            else
            {
                entry.Text = "0";
            }
        }
    }

    // 入力値を指定範囲内に調整
    private void ClampEntryValueInt(Entry entry, int min, int max, int defaultValue)
    {
        string raw = entry.Text.Trim();

        if (int.TryParse(raw, out int value))
        {
            value = Math.Clamp(value, min, max);
            entry.Text = value.ToString();
        }
        else
        {
            entry.Text = defaultValue.ToString(); // 無効ならデフォルト値に戻す
        }
    }

    // ローン入力データを共通データに設定
    private void UpdateLoanInputData()
    {
        try
        {
            var data = new LoanInputData
            {
                LoanAmount = int.Parse(Entry_LoanAmount.Text.Replace(",", ""))*10000,
                Years1 = int.Parse(Entry_LoanYears.Text),
                Years2 = int.Parse(Entry_LoanYears2.Text),
                Years3 = int.Parse(Entry_LoanYears3.Text),
                Years4 = int.Parse(Entry_LoanYears4.Text),
                InterestRate1 = double.Parse(Entry_InterestRate.Text),
                InterestRate2 = double.Parse(Entry_InterestRate2.Text),
                InterestRate3 = double.Parse(Entry_InterestRate3.Text),
                InterestRate4 = double.Parse(Entry_InterestRate4.Text),
                RepaymentType = (RB_Ganri.IsChecked ? "元利均等" : "元金均等")
            };

            SharedLoanInputData.Current = data;
        }
        catch
        {
            // 無効な入力時はスルー（必要ならログや警告出す）
        }
    }

    // ＋ボタンで段階金利の表示/非表示を切り替える
    private bool isTierVisible = false;
    void OnAddTierClicked(object sender, EventArgs e)
    {
        isTierVisible = !isTierVisible;
        Layout_Tier2.IsVisible = isTierVisible;
        Layout_Tier3.IsVisible = isTierVisible;
        Layout_Tier4.IsVisible = isTierVisible;
    }

    // 返済表データを保持・UIにバインドするコレクション
    public ObservableCollection<AmortizationItem> AmortizationItems { get; } = [];
    /// <summary>
    /// 全段階の毎月の返済金額を計算し、表示を更新する。
    /// 段階ごとの年数が 0 以下の場合はその段階のラベルを非表示とする。
    /// </summary>
    private void UpdateRepayments()
    {
        UpdateLoanInputData();

        // 返済金額の表示を非表示にしておく
        Layout_Payment1.IsVisible = false;
        Layout_Payment2.IsVisible = false;
        Layout_Payment3.IsVisible = false;
        Layout_Payment4.IsVisible = false;

        // ユーザー入力値を取得
        var input = SharedLoanInputData.Current;
        if (input == null) return;

        // 資金計算の計算をして結果を取得
        var result = LoanCalculator.Generate(input);
        if (result.Count == 0) return;

        // 返済合計額・実質年率の表示
        Label_TotalPaymentsValue1.Text = $"{result[^1].返済合計額:N0}円";
        Label_TotalPaymentsValue2.Text = $"{result[^1].利息合計額:N0}円";
        Label_RealInterestRateValue.Text = $"{result[^1].実質年率:F3}円";

        // 各段階の返済金額を表示
        Layout_Payment1.IsVisible = true;
        Label_Payment1.Text = string.Format("{0:N0}円", result[0].返済金額);
        if (input.Years2 > 0 & input.InterestRate2 > 0)
        {
            Layout_Payment2.IsVisible = true;
            Label_Payment2.Text = string.Format("{0:N0}円", result[(input.Years1) * 12].返済金額);
        }
        if (input.Years3 > 0 & input.InterestRate3 > 0)
        {
            Layout_Payment3.IsVisible = true;
            Label_Payment3.Text = string.Format("{0:N0}円", result[(input.Years1 + input.Years2) * 12].返済金額);
        }
        if (input.Years4 > 0 & input.InterestRate4 >0 )
        {
            Layout_Payment4.IsVisible = true;
            Label_Payment4.Text = string.Format("{0:N0}円", result[(input.Years1 + input.Years2 + input.Years3) * 12].返済金額);
        }
    }
}

// 償還表へのデータ通知用 共通データ
public static class SharedLoanInputData
{
    public static LoanInputData? Current { get; set; }
}
