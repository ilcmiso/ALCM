namespace ALCM;

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

    /// <summary>
    /// 全段階の毎月の返済金額を計算し、表示を更新する。
    /// 段階ごとの年数が 0 以下の場合はその段階のラベルを非表示とする。
    /// </summary>
    private void UpdateRepayments()
    {
        // 借入金額は万円単位で入力されるため、実際の元本は ×10000
        if (!double.TryParse(Entry_LoanAmount.Text, out var loanAmountTenThousand))
        {
            // 数値に変換できなければ何もしない
            return;
        }
        // 金額を円単位に変換
        int principal = (int)Math.Round(loanAmountTenThousand * 10000.0);

        bool anyVisible = false;

        // 返済方法を日本語文字列に変換
        string repaymentType = (RB_Ganri?.IsChecked ?? true) ? "元利均等" : "元金均等";

        // 段階1
        UpdateTier(principal, Entry_LoanYears.Text, Entry_InterestRate.Text, repaymentType, Layout_Payment1, Label_Payment1, ref anyVisible);
        // 段階2
        UpdateTier(principal, Entry_LoanYears2.Text, Entry_InterestRate2.Text, repaymentType, Layout_Payment2, Label_Payment2, ref anyVisible);
        // 段階3
        UpdateTier(principal, Entry_LoanYears3.Text, Entry_InterestRate3.Text, repaymentType, Layout_Payment3, Label_Payment3, ref anyVisible);
        // 段階4
        UpdateTier(principal, Entry_LoanYears4.Text, Entry_InterestRate4.Text, repaymentType, Layout_Payment4, Label_Payment4, ref anyVisible);

        // 見出し自体の表示
        Label_MonthlyRepaymentHeader.IsVisible = anyVisible;
    }

    /// <summary>
    /// 指定した年数・金利の組み合わせで毎月の返済金額を計算し、UI に反映する。
    /// 返済方法はラジオボタンの状態から取得する。
    /// </summary>
    /// <param name="principal">借入元本（円）</param>
    /// <param name="yearsText">年数の文字列</param>
    /// <param name="rateText">金利の文字列（年率%）</param>
    /// <param name="layout">表示レイアウト（水平）</param>
    /// <param name="paymentLabel">返済金額を表示するラベル</param>
    /// <param name="anyVisible">少なくとも1件表示したかどうかを呼び出し元で保持するフラグ</param>
    private void UpdateTier(int principal, string yearsText, string rateText, string repaymentType, HorizontalStackLayout layout, Label paymentLabel, ref bool anyVisible)
    {
        // 年数と金利を数値に変換し、年数が 0 以下または金利が負なら非表示
        if (int.TryParse(yearsText, out var years) && double.TryParse(rateText, out var rate) && years > 0 && rate > 0)
        {
            // LoanCalculator を利用して月々の返済額を取得
            int monthlyPayment = LoanCalculator.CalculateMonthlyPayment(principal, years, rate, repaymentType);
            // カンマ区切りフォーマットを適用
            paymentLabel.Text = string.Format("{0:N0}円", monthlyPayment);
            layout.IsVisible = true;
            anyVisible = true;
        }
        else
        {
            // 該当段階を非表示
            layout.IsVisible = false;
        }
    }
}

// 償還表へのデータ通知用 共通データ
public static class SharedLoanInputData
{
    public static LoanInputData? Current { get; set; }
}
