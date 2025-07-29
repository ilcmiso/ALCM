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
    }

    public static class LoanCalculator
    {
        public static List<AmortizationRow> Generate(LoanInputData input)
        {
            var result = new List<AmortizationRow>();
            var baseDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            double balance = input.LoanAmount;      // 残高
            int periodCounter = 0;

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
                    monthlyPayment = balance * monthlyRate * Math.Pow(1 + monthlyRate, totalMonths) / (Math.Pow(1 + monthlyRate, totalMonths) - 1);
                }
                else // 元金均等
                {
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

                    if (principal > balance) principal = balance;

                    double payment = principal + interest;
                    balance -= principal;

                    result.Add(new AmortizationRow
                    {
                        回数 = periodCounter,
                        振替日 = baseDate.AddMonths(periodCounter - 1),
                        返済金額 = (int)Math.Round(payment),
                        元金額 = (int)Math.Round(principal),
                        利息額 = (int)Math.Round(interest),
                        残高 = (int)Math.Round(balance)
                    });
                }
            }
            return result;
        }
    }
}
