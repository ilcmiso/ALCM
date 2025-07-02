using Microsoft.Maui.Platform;
using System.Diagnostics;

namespace ALCM;

public partial class NewPage2 : ContentPage
{
	public NewPage2()
	{
		InitializeComponent();
	}

    private void Entry_LoanAmount_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
            ClampEntryValueInt(entry, 1, 100000, 3000); // 1万～10億 初期3000万
    }

    private void Entry_LoanYears_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
            ClampEntryValueInt(entry, 1, 50, 35); // デフォルト35
    }

    private void Entry_InterestRate_Unfocused(object sender, FocusEventArgs e)
    {
        var entry = sender as Entry;
        if (entry == null) return;

        string raw = entry.Text?.Trim();
        if (string.IsNullOrEmpty(raw))
        {
            entry.Text = "1.000";
            return;
        }

        if (raw.Contains("."))
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


    private void ClampEntryValueInt(Entry entry, int min, int max, int defaultValue)
    {
        string raw = entry.Text?.Trim();

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

}