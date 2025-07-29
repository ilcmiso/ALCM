using System.Collections.ObjectModel;
using ALCM.Models;

namespace ALCM.ViewModels
{
    /// <summary>
    /// 償還表画面用のビューモデル。
    /// </summary>
    public class AmortizationViewModel
    {
        /// <summary>
        /// 償還表のデータコレクション。
        /// </summary>
        public ObservableCollection<AmortizationItem> AmortizationItems { get; } = [];
    }
}