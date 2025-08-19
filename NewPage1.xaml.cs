#nullable enable
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace ALCM
{
    public partial class NewPage1 : ContentPage
    {
        public NewPage1()
        {
            // �� XAML��StaticResource�������ł���悤�ɁA��Ƀ��\�[�X�֓o�^
            var appRes = Application.Current?.Resources;
            if (appRes is not null)
            {
                if (!appRes.ContainsKey("NullToBoolConverter"))
                    appRes.Add("NullToBoolConverter", new NullToBoolConverter());

                if (!appRes.ContainsKey("SelectedItemComparer"))
                    appRes.Add("SelectedItemComparer", new SelectedItemComparerConverter());
            }

            InitializeComponent();

            // �� ��ʂ�BindingContext��ݒ�
            BindingContext = new NewPage1ViewModel();
        }
    }

    /// <summary>
    /// ��ʂ�VM�B�ڋq�ꗗ�E�I�𒆌ڋq�E�ҏW/�폜�R�}���h��ێ�����B
    /// �u�ҏW�v�����ŃT���v��1����ǉ�����B
    /// </summary>
    public sealed class NewPage1ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<CustomerRow> CustomerList { get; } = new();

        private CustomerRow? _selectedCustomer;
        public CustomerRow? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (_selectedCustomer == value) return;
                _selectedCustomer = value;
                OnPropertyChanged();
                // CanExecute�̍X�V
                (DeleteCommand as Command)?.ChangeCanExecute();
                (EditCommand as Command)?.ChangeCanExecute();
            }
        }

        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public NewPage1ViewModel()
        {
            // �ҏW�{�^���F�T���v���f�[�^��1���ǉ�
            EditCommand = new Command(AddSampleRow);

            //EditCommand = new Command(
            //    execute: AddSampleRow,
            //    canExecute: () => SelectedCustomer != null   // XAML�����I�𒆂̂ݗL���̑z��Ȃ炱���OK
            //);

            // �폜�{�^���F�I���s���폜
            DeleteCommand = new Command(
                execute: DeleteSelected,
                canExecute: () => SelectedCustomer != null
            );
        }

        /// <summary>
        /// �T���v��1����ǉ�����B
        /// ����/PRJ: �Z�Z�}���V����, ����: 101, �ڋq��: �R�c���Y, �i��: ��
        /// </summary>
        private void AddSampleRow()
        {
            CustomerList.Add(new CustomerRow
            {
                PropertyName = "�}���V����",
                RoomNumber = "101",
                CustomerName = "�R�c���Y",
                Progress = "��"
            });
        }

        /// <summary>
        /// �I�𒆂̌ڋq���폜����B
        /// </summary>
        private void DeleteSelected()
        {
            if (SelectedCustomer is null) return;
            CustomerList.Remove(SelectedCustomer);
            SelectedCustomer = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// �ڋq�ꗗ��1�s��\�����f���B
    /// </summary>
    public sealed class CustomerRow
    {
        // ����/PRJ
        public string PropertyName { get; set; } = "";
        // ����
        public string RoomNumber { get; set; } = "";
        // �ڋq��
        public string CustomerName { get; set; } = "";
        // �i��
        public string Progress { get; set; } = "";
    }

    /// <summary>
    /// �l�� null �łȂ��ꍇ�� true ��Ԃ��R���o�[�^�B
    /// �i�{�^���� IsEnabled �o�C���h�Ɏg�p�j
    /// </summary>
    public sealed class NullToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is not null;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// CollectionView �� SelectedItem �ƍs�A�C�e��������Ȃ� true�B
    /// �i�I���s�̃n�C���C�g�p DataTrigger �ȂǂŎg�p�j
    /// ConverterParameter �� CollectionView ��n���z��B
    /// </summary>
    public sealed class SelectedItemComparerConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not CollectionView cv) return false;
            return ReferenceEquals(cv.SelectedItem, value);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
