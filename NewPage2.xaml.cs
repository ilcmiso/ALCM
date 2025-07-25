namespace ALCM;

public partial class NewPage2 : ContentPage
{
	public NewPage2()
	{
		InitializeComponent();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        UpdateLoanInputData();
    }

    // ローン金額の入力欄でフォーカスが外れたときの処理
    private void Entry_LoanAmount_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
            ClampEntryValueInt(entry, 0, 100000, 3000); // 1万～10億 初期3000万
    }

    // ローン年数の入力欄でフォーカスが外れたときの処理
    private void Entry_LoanYears_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
            ClampEntryValueInt(entry, 0, 50, 35); // デフォルト35
    }

    // 金利の入力欄でフォーカスが外れたときの処理
    private void Entry_InterestRate_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is not Entry entry) return;

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
}

// 償還表へのデータ通知用 共通データ
public static class SharedLoanInputData
{
    public static LoanInputData? Current { get; set; }
}
