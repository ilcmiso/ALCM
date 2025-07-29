using System;

namespace ALCM.Models
{
    /// <summary>
    /// 償還表の 1 行分のデータを表します。
    /// </summary>
    public class AmortizationItem
    {
        public int 回数 { get; set; }
        public DateTime 振替日 { get; set; }
        public int 返済金額 { get; set; }
        public int 元金額 { get; set; }
        public int 利息額 { get; set; }
        public int 残高 { get; set; }
    }
}