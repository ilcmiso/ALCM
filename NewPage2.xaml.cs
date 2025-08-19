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

    // �ԍϕ\�f�[�^��ێ��EUI�Ƀo�C���h����R���N�V����
    public ObservableCollection<AmortizationItem> AmortizationItems { get; } = [];
    /// <summary>
    /// �S�i�K�̖����̕ԍϋ��z���v�Z���A�\�����X�V����B
    /// �i�K���Ƃ̔N���� 0 �ȉ��̏ꍇ�͂��̒i�K�̃��x�����\���Ƃ���B
    /// </summary>
    private void UpdateRepayments()
    {
        UpdateLoanInputData();

        // �ԍϋ��z�̕\�����\���ɂ��Ă���
        Layout_Payment1.IsVisible = false;
        Layout_Payment2.IsVisible = false;
        Layout_Payment3.IsVisible = false;
        Layout_Payment4.IsVisible = false;

        // ���[�U�[���͒l���擾
        var input = SharedLoanInputData.Current;
        if (input == null) return;

        // �����v�Z�̌v�Z�����Č��ʂ��擾
        var result = LoanCalculator.Generate(input);
        if (result.Count == 0) return;

        // �ԍύ��v�z�E�����N���̕\��
        Label_TotalPaymentsValue1.Text = $"{result[^1].�ԍύ��v�z:N0}�~";
        Label_TotalPaymentsValue2.Text = $"{result[^1].�������v�z:N0}�~";
        Label_RealInterestRateValue.Text = $"{result[^1].�����N��:F3}�~";

        // �e�i�K�̕ԍϋ��z��\��
        Layout_Payment1.IsVisible = true;
        Label_Payment1.Text = string.Format("{0:N0}�~", result[0].�ԍϋ��z);
        if (input.Years2 > 0 & input.InterestRate2 > 0)
        {
            Layout_Payment2.IsVisible = true;
            Label_Payment2.Text = string.Format("{0:N0}�~", result[(input.Years1) * 12].�ԍϋ��z);
        }
        if (input.Years3 > 0 & input.InterestRate3 > 0)
        {
            Layout_Payment3.IsVisible = true;
            Label_Payment3.Text = string.Format("{0:N0}�~", result[(input.Years1 + input.Years2) * 12].�ԍϋ��z);
        }
        if (input.Years4 > 0 & input.InterestRate4 >0 )
        {
            Layout_Payment4.IsVisible = true;
            Label_Payment4.Text = string.Format("{0:N0}�~", result[(input.Years1 + input.Years2 + input.Years3) * 12].�ԍϋ��z);
        }
    }
}

// ���ҕ\�ւ̃f�[�^�ʒm�p ���ʃf�[�^
public static class SharedLoanInputData
{
    public static LoanInputData? Current { get; set; }
}
