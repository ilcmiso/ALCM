using ALCM.Models;

namespace ALCM
{
    public class LoanInputData
    {
        public int LoanAmount { get; set; }                 // 借入金額
        public int Years1 { get; set; }                     // 借入年数 当初
        public int Years2 { get; set; }
        public int Years3 { get; set; }
        public int Years4 { get; set; }
        public double InterestRate1 { get; set; }           // 利率     当初
        public double InterestRate2 { get; set; }
        public double InterestRate3 { get; set; }
        public double InterestRate4 { get; set; }
        public required string RepaymentType { get; set; }  // 元利均等 or 元金均等
    }

    public class AmortizationRow
    {
        public int 回数 { get; set; }
        public DateTime 振替日 { get; set; }
        public int 返済金額 { get; set; }
        public int 元金額 { get; set; }
        public int 利息額 { get; set; }
        public int 残高 { get; set; }

        public int 返済合計額 { get; set; }
        public int 利息合計額 { get; set; }
        public double 実質年率 { get; set; }
    }

    public static class LoanCalculator
    {
        public static List<AmortizationRow> Generate(LoanInputData input)
        {
            var result = new List<AmortizationRow>();
            var baseDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            double balance = input.LoanAmount;      // 残高
            int periodCounter = 0;

            // 累計の返済金額および利息額
            double totalRepayment = 0;
            double totalInterest = 0;

            var tiers = new (int years, double rate)[]
            {
                (input.Years1, input.InterestRate1),
                (input.Years2, input.InterestRate2),
                (input.Years3, input.InterestRate3),
                (input.Years4, input.InterestRate4),
            };

            // 段階金利毎に計算・表作成
            for (int tier = 0; tier < tiers.Length; tier++)
            {
                int years = tiers[tier].years;
                int months = years * 12;
                double rate = tiers[tier].rate;
                double monthlyRate = Math.Pow(1 + rate / 100.0, 1.0 / 12) - 1;
                double monthlyPayment;

                int totalMonths = 0;
                for (int cnt = tier; cnt < tiers.Length; cnt++)
                {
                    totalMonths += tiers[cnt].years * 12;
                }

                // 入力値の検証
                if (years <= 0 || rate <= 0 || balance <= 0)
                {
                    continue;
                }

                if (input.RepaymentType == "元利均等")
                {
                    // 元利均等：ローン期間全体で毎月一定額を支払う
                    monthlyPayment = balance * monthlyRate * Math.Pow(1 + monthlyRate, totalMonths) / (Math.Pow(1 + monthlyRate, totalMonths) - 1);
                }
                else
                {
                    // 元金均等：元本均等返済＋その月の利息（おおよその平均）
                    monthlyPayment = balance / totalMonths + (balance * monthlyRate);
                }

                // 月毎の償還表作成
                for (int j = 0; j < months; j++)
                {
                    // 残高が0以下になったら終了
                    if (balance <= 0) break;
                    periodCounter++;

                    double interest = balance * monthlyRate;
                    double principal = (input.RepaymentType == "元利均等")
                        ? monthlyPayment - interest
                        : monthlyPayment;

                    // 最終月に元金が残りより大きい場合は調整
                    if (principal > balance) principal = balance;

                    double payment = principal + interest;
                    balance -= principal;

                    // 累積値を更新
                    totalRepayment += payment;
                    totalInterest += interest;

                    // 実質年率の計算
                    double effectiveAnnualRate = 0;
                    if (periodCounter > 0)
                    {
                        // (利息合計額 / 元本) * 12 / 経過月数 * 100 で % 表示
                        effectiveAnnualRate = (totalInterest / input.LoanAmount) * (12.0 / periodCounter) * 100.0;
                    }

                    result.Add(new AmortizationRow
                    {
                        回数 = periodCounter,
                        振替日 = baseDate.AddMonths(periodCounter - 1),
                        返済金額 = (int)Math.Round(payment),
                        元金額 = (int)Math.Round(principal),
                        利息額 = (int)Math.Round(interest),
                        残高 = (int)Math.Round(balance),
                        返済合計額 = (int)Math.Round(totalRepayment),
                        利息合計額 = (int)Math.Round(totalInterest),
                        実質年率 = effectiveAnnualRate
                    });
                }
            }
            // 実質年率（IRRベース、年率換算）を最終行にセット
            if (result.Count > 0)
            {
                double earPercent = ComputeEffectiveAnnualRatePercent(input.LoanAmount, result);
                // 最終行にだけ入れる（必要なら全行に同値を入れても良い）
                result[^1].実質年率 = earPercent;
            }
            return result;
        }
        /// <summary>
        /// 月次キャッシュフローのIRRから年率換算の実質年率（%）を返す。
        /// 先頭に +元本、以降に -月次返済額 の系列を仮定する。
        /// </summary>
        private static double ComputeEffectiveAnnualRatePercent(int principal, List<AmortizationRow> rows)
        {
            if (principal <= 0 || rows == null || rows.Count == 0) return 0.0;

            var cashflows = new List<double>(rows.Count + 1)
            {
                principal // t=0 流入（借入）
            };
            foreach (var r in rows)
                cashflows.Add(-r.返済金額); // 毎月の返済は流出

            double? rMonthly = ComputeMonthlyIrr(cashflows);
            if (rMonthly == null) return 0.0;

            // 年率換算（実効年率）
            double ear = Math.Pow(1.0 + rMonthly.Value, 12.0) - 1.0;
            return ear * 100.0; // %
        }

        /// <summary>
        /// 月次IRRを2分法で求める（安定性重視）。失敗時は null。
        /// </summary>
        private static double? ComputeMonthlyIrr(IReadOnlyList<double> cashflows)
        {
            // NPV(r) = Σ cf_t / (1+r)^t = 0 を満たす r を探す
            double Npv(double r)
            {
                double sum = 0.0;
                double denom = 1.0; // (1+r)^t を逐次更新
                for (int t = 0; t < cashflows.Count; t++)
                {
                    if (t > 0) denom *= (1.0 + r);
                    sum += cashflows[t] / denom;
                }
                return sum;
            }

            // 端のNPVが符号反転する区間を探す
            double low = -0.9999;        // r > -1 必須
            double high = 1.0;           // まずは月率100%を上限
            double fLow = Npv(low);
            double fHigh = Npv(high);

            // 反転が見つからなければ上限を拡げる（最大 ~102.4）
            int expand = 0;
            while (fLow * fHigh > 0 && high < 102.4 && expand < 12)
            {
                high *= 2.0;
                fHigh = Npv(high);
                expand++;
            }
            if (fLow * fHigh > 0) return null; // 解が見つからない

            // 2分法
            for (int i = 0; i < 100; i++)
            {
                double mid = (low + high) * 0.5;
                double fMid = Npv(mid);

                if (Math.Abs(fMid) < 1e-10)
                    return mid;

                if (fLow * fMid <= 0)
                {
                    high = mid;
                    fHigh = fMid;
                }
                else
                {
                    low = mid;
                    fLow = fMid;
                }
            }
            return (low + high) * 0.5;
        }

        //
        // （参考）単一の金利期間に対して月々の返済額を算出。
        // 元本、年数、利率および返済方法から一月あたりの返済額を求め、整数で返却する。
        //
        public static int CalculateMonthlyPayment(int loanAmount, int years, double rate, string repaymentType)
        {
            if (loanAmount <= 0 || years <= 0 || rate <= 0) return 0;

            double monthlyRate = Math.Pow(1 + rate / 100.0, 1.0 / 12.0) - 1.0;
            int months = years * 12;
            double payment;

            if (repaymentType == "元利均等")
            {
                double pow = Math.Pow(1 + monthlyRate, months);
                payment = loanAmount * monthlyRate * pow / (pow - 1);
            }
            else
            {
                payment = loanAmount / (double)months + (loanAmount * monthlyRate);
            }
            return (int)Math.Round(payment);
        }
    }
}
