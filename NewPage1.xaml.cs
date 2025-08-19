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
            // ▼ XAMLのStaticResourceを解決できるように、先にリソースへ登録
            var appRes = Application.Current?.Resources;
            if (appRes is not null)
            {
                if (!appRes.ContainsKey("NullToBoolConverter"))
                    appRes.Add("NullToBoolConverter", new NullToBoolConverter());

                if (!appRes.ContainsKey("SelectedItemComparer"))
                    appRes.Add("SelectedItemComparer", new SelectedItemComparerConverter());
            }

            InitializeComponent();

            // ▼ 画面のBindingContextを設定
            BindingContext = new NewPage1ViewModel();
        }
    }

    /// <summary>
    /// 画面のVM。顧客一覧・選択中顧客・編集/削除コマンドを保持する。
    /// 「編集」押下でサンプル1件を追加する。
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
                // CanExecuteの更新
                (DeleteCommand as Command)?.ChangeCanExecute();
                (EditCommand as Command)?.ChangeCanExecute();
            }
        }

        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public NewPage1ViewModel()
        {
            // 編集ボタン：サンプルデータを1件追加
            EditCommand = new Command(AddSampleRow);

            //EditCommand = new Command(
            //    execute: AddSampleRow,
            //    canExecute: () => SelectedCustomer != null   // XAML側が選択中のみ有効の想定ならこれでOK
            //);

            // 削除ボタン：選択行を削除
            DeleteCommand = new Command(
                execute: DeleteSelected,
                canExecute: () => SelectedCustomer != null
            );
        }

        /// <summary>
        /// サンプル1件を追加する。
        /// 物件/PRJ: 〇〇マンション, 号室: 101, 顧客名: 山田太郎, 進捗: 未
        /// </summary>
        private void AddSampleRow()
        {
            CustomerList.Add(new CustomerRow
            {
                PropertyName = "マンション",
                RoomNumber = "101",
                CustomerName = "山田太郎",
                Progress = "未"
            });
        }

        /// <summary>
        /// 選択中の顧客を削除する。
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
    /// 顧客一覧の1行を表すモデル。
    /// </summary>
    public sealed class CustomerRow
    {
        // 物件/PRJ
        public string PropertyName { get; set; } = "";
        // 号室
        public string RoomNumber { get; set; } = "";
        // 顧客名
        public string CustomerName { get; set; } = "";
        // 進捗
        public string Progress { get; set; } = "";
    }

    /// <summary>
    /// 値が null でない場合に true を返すコンバータ。
    /// （ボタンの IsEnabled バインドに使用）
    /// </summary>
    public sealed class NullToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is not null;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// CollectionView の SelectedItem と行アイテムが同一なら true。
    /// （選択行のハイライト用 DataTrigger などで使用）
    /// ConverterParameter に CollectionView を渡す想定。
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
