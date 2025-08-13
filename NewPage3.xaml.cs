using System.Collections.ObjectModel;
using ALCM.Models;
using ALCM.ViewModels;
#if ANDROID
using Android.OS;
using Android.Content;
using Java.IO;
#endif

namespace ALCM
{
    public partial class NewPage3 : ContentPage
    {
        // �ԍϕ\�f�[�^��ێ��EUI�Ƀo�C���h����R���N�V����
        public ObservableCollection<AmortizationItem> AmortizationItems { get; } = [];

        public NewPage3()
        {
            InitializeComponent();
            BindingContext = new AmortizationViewModel();
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            // �V�K�o�^��ʂ��ĂԂȂǂ̏���
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            // �X�V�����i�ēǂݍ��݁E�Čv�Z�Ȃǁj
        }

        // ��ʂ����[�h���ꂽ�ۂ̃C�x���g
        protected override void OnAppearing()
        {
            base.OnAppearing();

            var input = SharedLoanInputData.Current;
            if (input != null && BindingContext is AmortizationViewModel vm)
            {
                // �����v�Z�̈ꗗ���쐬
                var result = LoanCalculator.Generate(input);
                vm.AmortizationItems.Clear();
                foreach (var item in result)
                    vm.AmortizationItems.Add(new AmortizationItem
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

        // Excel,PDF�o�̓{�^�������C�x���g
        private async void BtnExport_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is AmortizationViewModel vm && sender is Button btn)
            {
                // �{�^������n���ꂽCommandParameter���Q��
                string? format = btn.CommandParameter as string;
                if (string.IsNullOrEmpty(format))
                    return; // �p�����[�^��������Ή������Ȃ�

                try
                {
                    // 1. �t�@�C�����Ɗg���q������
                    string extension = format.Equals("pdf", StringComparison.CurrentCultureIgnoreCase) ? "pdf" : "xlsx";
                    string fileName = $"LoanRepay.{extension}";

                    // 2. �ۑ���p�X�𐶐��i���ʏ����j
                    string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
#if ANDROID
            string? downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
                                        Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
            if (!string.IsNullOrEmpty(downloadsPath))
            {
                filePath = Path.Combine(downloadsPath, fileName);
            }
#elif WINDOWS
            string? userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(userProfile))
            {
                filePath = Path.Combine(Path.Combine(userProfile, "Downloads"), fileName);
            }
#endif

                    // 3. �o�͏����̎��s�i�`���ɉ����Đ؂�ւ��j
                    if (format.Equals("EXCEL", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await OutputExcel.SaveAmortizationAsync(vm.AmortizationItems, filePath);
                    }
                    else if (format.Equals("PDF", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await OutputPdf.SaveAmortizationAsync(vm.AmortizationItems, filePath);
                    }
                    else
                    {
                        throw new NotSupportedException($"���Ή��̏o�͌`���ł�: {format}");
                    }

                    // 4. �o�͊�����A�t�@�C�����J�����m�F����_�C�A���O��\���i���ʏ����j
                    await Common.ShowOpenFileDialogAsync(filePath);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("�G���[", $"�o�͒��ɃG���[���������܂���: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("�f�[�^�Ȃ�", "���ҕ\�f�[�^��������܂���ł����B", "OK");
            }
        }
    }
}
