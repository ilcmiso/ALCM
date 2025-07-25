using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace ALCM
{
    public partial class NewPage3 : ContentPage
    {
        // �ԍϕ\�f�[�^��ێ��EUI�Ƀo�C���h����R���N�V����
        public ObservableCollection<AmortizationItem> AmortizationItems { get; } = [];

        public NewPage3()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            // �V�K�o�^��ʂ��ĂԂȂǂ̏���
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            // �X�V�����i�ēǂݍ��݁E�Čv�Z�Ȃǁj
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
                        �� = item.��,
                        �U�֓� = item.�U�֓�,
                        �ԍϋ��z = item.�ԍϋ��z,
                        �����z = item.�����z,
                        �����z = item.�����z,
                        �c�� = item.�c��
                    });
            }
            else
            {
                AmortizationItems.Clear();
            }
        }
    }

    // �o�C���h�p�f�[�^���f��
    public class AmortizationItem
    {
        public int �� { get; set; }
        public DateTime �U�֓� { get; set; }
        public decimal �ԍϋ��z { get; set; }
        public decimal �����z { get; set; }
        public decimal �����z { get; set; }
        public decimal �c�� { get; set; }
    }


}
