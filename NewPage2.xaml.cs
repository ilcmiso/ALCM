namespace ALCM;

public partial class NewPage2 : ContentPage
{
	public NewPage2()
	{
		InitializeComponent();
        UpdateRepayments();
    }

    // ��ʂ��A�����[�h���ꂽ�Ƃ�
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        UpdateLoanInputData();
    }

    // ���͗��Ńt�H�[�J�X�A�E�g ���z
    private void Entry_LoanAmount_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is not Entry entry) return;
        ClampEntryValueInt(entry, 0, 100000, 3000); // 1���`10�� ����3000��
        UpdateRepayments();
    }

    // ���͗��Ńt�H�[�J�X�A�E�g �N��
    private void Entry_LoanYears_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is not Entry entry) return;
        ClampEntryValueInt(entry, 0, 50, 35); // �f�t�H���g35
        UpdateRepayments();
    }

    // ���͗��Ńt�H�[�J�X�A�E�g ����
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

    // ���͒l���w��͈͓��ɒ���
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
            entry.Text = defaultValue.ToString(); // �����Ȃ�f�t�H���g�l�ɖ߂�
        }
    }

    // ���[�����̓f�[�^�����ʃf�[�^�ɐݒ�
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
                RepaymentType = (RB_Ganri.IsChecked ? "�����ϓ�" : "�����ϓ�")
            };

            SharedLoanInputData.Current = data;
        }
        catch
        {
            // �����ȓ��͎��̓X���[�i�K�v�Ȃ烍�O��x���o���j
        }
    }

    // �{�{�^���Œi�K�����̕\��/��\����؂�ւ���
    private bool isTierVisible = false;
    void OnAddTierClicked(object sender, EventArgs e)
    {
        isTierVisible = !isTierVisible;
        Layout_Tier2.IsVisible = isTierVisible;
        Layout_Tier3.IsVisible = isTierVisible;
        Layout_Tier4.IsVisible = isTierVisible;
    }

    /// <summary>
    /// �S�i�K�̖����̕ԍϋ��z���v�Z���A�\�����X�V����B
    /// �i�K���Ƃ̔N���� 0 �ȉ��̏ꍇ�͂��̒i�K�̃��x�����\���Ƃ���B
    /// </summary>
    private void UpdateRepayments()
    {
        // �ؓ����z�͖��~�P�ʂœ��͂���邽�߁A���ۂ̌��{�� �~10000
        if (!double.TryParse(Entry_LoanAmount.Text, out var loanAmountTenThousand))
        {
            // ���l�ɕϊ��ł��Ȃ���Ή������Ȃ�
            return;
        }
        // ���z���~�P�ʂɕϊ�
        int principal = (int)Math.Round(loanAmountTenThousand * 10000.0);

        bool anyVisible = false;

        // �ԍϕ��@����{�ꕶ����ɕϊ�
        string repaymentType = (RB_Ganri?.IsChecked ?? true) ? "�����ϓ�" : "�����ϓ�";

        // �i�K1
        UpdateTier(principal, Entry_LoanYears.Text, Entry_InterestRate.Text, repaymentType, Layout_Payment1, Label_Payment1, ref anyVisible);
        // �i�K2
        UpdateTier(principal, Entry_LoanYears2.Text, Entry_InterestRate2.Text, repaymentType, Layout_Payment2, Label_Payment2, ref anyVisible);
        // �i�K3
        UpdateTier(principal, Entry_LoanYears3.Text, Entry_InterestRate3.Text, repaymentType, Layout_Payment3, Label_Payment3, ref anyVisible);
        // �i�K4
        UpdateTier(principal, Entry_LoanYears4.Text, Entry_InterestRate4.Text, repaymentType, Layout_Payment4, Label_Payment4, ref anyVisible);

        // ���o�����̂̕\��
        Label_MonthlyRepaymentHeader.IsVisible = anyVisible;
    }

    /// <summary>
    /// �w�肵���N���E�����̑g�ݍ��킹�Ŗ����̕ԍϋ��z���v�Z���AUI �ɔ��f����B
    /// �ԍϕ��@�̓��W�I�{�^���̏�Ԃ���擾����B
    /// </summary>
    /// <param name="principal">�ؓ����{�i�~�j</param>
    /// <param name="yearsText">�N���̕�����</param>
    /// <param name="rateText">�����̕�����i�N��%�j</param>
    /// <param name="layout">�\�����C�A�E�g�i�����j</param>
    /// <param name="paymentLabel">�ԍϋ��z��\�����郉�x��</param>
    /// <param name="anyVisible">���Ȃ��Ƃ�1���\���������ǂ������Ăяo�����ŕێ�����t���O</param>
    private void UpdateTier(int principal, string yearsText, string rateText, string repaymentType, HorizontalStackLayout layout, Label paymentLabel, ref bool anyVisible)
    {
        // �N���Ƌ����𐔒l�ɕϊ����A�N���� 0 �ȉ��܂��͋��������Ȃ��\��
        if (int.TryParse(yearsText, out var years) && double.TryParse(rateText, out var rate) && years > 0 && rate > 0)
        {
            // LoanCalculator �𗘗p���Č��X�̕ԍϊz���擾
            int monthlyPayment = LoanCalculator.CalculateMonthlyPayment(principal, years, rate, repaymentType);
            // �J���}��؂�t�H�[�}�b�g��K�p
            paymentLabel.Text = string.Format("{0:N0}�~", monthlyPayment);
            layout.IsVisible = true;
            anyVisible = true;
        }
        else
        {
            // �Y���i�K���\��
            layout.IsVisible = false;
        }
    }
}

// ���ҕ\�ւ̃f�[�^�ʒm�p ���ʃf�[�^
public static class SharedLoanInputData
{
    public static LoanInputData? Current { get; set; }
}
