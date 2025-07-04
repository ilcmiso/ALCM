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
            ClampEntryValueInt(entry, 1, 100000, 3000); // 1万～10億 初期3000万
    }

    // ローン年数の入力欄でフォーカスが外れたときの処理
    private void Entry_LoanYears_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
            ClampEntryValueInt(entry, 1, 50, 35); // デフォルト35
    }

    // 金利の入力欄でフォーカスが外れたときの処理
    private void Entry_InterestRate_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is not Entry entry) return;

        string raw = entry.Text.Trim();
        if (string.IsNullOrEmpty(raw))
        {
            entry.Text = "1.000";
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
                entry.Text = "1.000";
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
                entry.Text = "1.000";
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
                Years = int.Parse(Entry_LoanYears.Text),
                InterestRate = double.Parse(Entry_InterestRate.Text),
                RepaymentType = (RB_Ganri.IsChecked ? "元利均等" : "元金均等")
            };

            SharedLoanInputData.Current = data;
        }
        catch
        {
            // 無効な入力時はスルー（必要ならログや警告出す）
        }
    }

}

// 償還表へのデータ通知用 共通データ
public static class SharedLoanInputData
{
    public static LoanInputData? Current { get; set; }
}
