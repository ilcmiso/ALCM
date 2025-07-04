// 編集が必要なファイル名2: NewPage3.xaml.cs
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace ALCM
{
    public partial class NewPage3 : ContentPage
    {
        public NewPage3()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // NewPage2で入力されたデータを読み込む
            var input = SharedLoanInputData.Current;

            if (input != null)
            {
                var result = LoanCalculator.Generate(input);
                AmortizationListView.ItemsSource = result;
            }
            else
            {
                // データが未入力だったときのフォールバック（エラー表示や初期状態）
                AmortizationListView.ItemsSource = null;
            }
        }
    }
}
