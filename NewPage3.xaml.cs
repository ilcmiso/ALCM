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

        /// <summary>
        /// ���ҕ\�� Excel �ɏo�͂���{�^�������C�x���g�B
        /// </summary>
        private async void BtnExcelOut_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is AmortizationViewModel vm)
            {
                try
                {
                    // �ۑ���p�X���\�z
                    string fileName = "LoanRepay.xlsx";
                    string folder = FileSystem.AppDataDirectory;
                    string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
#if ANDROID
                    string? downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
                    if (!string.IsNullOrEmpty(downloadsPath))
                        filePath = Path.Combine(downloadsPath, fileName);
#elif WINDOWS
                    string? downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
                    if (!string.IsNullOrEmpty(downloadsPath))
                        filePath = Path.Combine(Path.Combine(downloadsPath, "Downloads"), fileName);
#endif
                    // Excel �o�͎��s
                    await OutputExcel.SaveAmortizationAsync(vm.AmortizationItems, filePath);

                    // �o�͊�����Ƀt�@�C�����J���m�F�_�C�A���O��\��
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

        /// <summary>
        /// ���ҕ\�� PDF �ɏo�͂���{�^�������C�x���g�B
        /// </summary>
        private async void BtnPDFOut_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is AmortizationViewModel vm)
            {
                try
                {
                    // �ۑ���p�X���\�z
                    string fileName = "LoanRepay.pdf";
                    string folder = FileSystem.AppDataDirectory;
                    string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
#if ANDROID
                    string? downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
                    if (!string.IsNullOrEmpty(downloadsPath))
                        filePath = Path.Combine(downloadsPath, fileName);
#elif WINDOWS
                    string? downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
                    if (!string.IsNullOrEmpty(downloadsPath))
                        filePath = Path.Combine(Path.Combine(downloadsPath, "Downloads"), fileName);
#endif
                    // PDF �o�͎��s
                    await OutputPdf.SaveAmortizationAsync(vm.AmortizationItems, filePath);

                    // �o�͊�����Ƀt�@�C�����J���m�F�_�C�A���O��\��
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
