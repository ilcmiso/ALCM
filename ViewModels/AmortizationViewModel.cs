using System.Collections.ObjectModel;

namespace ALCM
{
    /// <summary>
    /// 償還表画面用のビューモデル。
    /// </summary>
    public class AmortizationViewModel
    {
        /// <summary>
        /// 償還表のデータコレクション。
        /// </summary>
        public ObservableCollection<AmortizationItem> AmortizationItems { get; } = new();
    }
}