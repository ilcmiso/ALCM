using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace ALCM
{
    public partial class NewPage3 : ContentPage
    {
        // 返済表データを保持・UIにバインドするコレクション
        public ObservableCollection<AmortizationItem> AmortizationItems { get; } = [];

        public NewPage3()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            // 新規登録画面を呼ぶなどの処理
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            // 更新処理（再読み込み・再計算など）
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var input = SharedLoanInputData.Current;
            if (input != null)
            {
                var result = LoanCalculator.Generate(input);
                AmortizationItems.Clear();
                foreach (var item in result)
                    AmortizationItems.Add(new AmortizationItem
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
    }

    // バインド用データモデル
    public class AmortizationItem
    {
        public int 回数 { get; set; }
        public DateTime 振替日 { get; set; }
        public decimal 返済金額 { get; set; }
        public decimal 元金額 { get; set; }
        public decimal 利息額 { get; set; }
        public decimal 残高 { get; set; }
    }


}
