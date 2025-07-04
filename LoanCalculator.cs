// 編集が必要なファイル名1: LoanCalculator.cs
using System;
using System.Collections.Generic;

namespace ALCM
{
    public class LoanInputData
    {
        public int LoanAmount { get; set; }
        public int Years { get; set; }
        public double InterestRate { get; set; } // 年利（%）
        public string RepaymentType { get; set; } // "元利均等" or "元金均等"
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
            var list = new List<AmortizationRow>();

            int totalMonths = input.Years * 12;
            double monthlyRate = input.InterestRate / 100.0 / 12.0;
            double balance = input.LoanAmount;
            double monthlyPayment = 0;
            double principalPerMonth = 0;

            if (input.RepaymentType == "元利均等")
            {
                monthlyPayment = input.LoanAmount * monthlyRate * Math.Pow(1 + monthlyRate, totalMonths)
                                / (Math.Pow(1 + monthlyRate, totalMonths) - 1);
            }
            else // 元金均等
            {
                principalPerMonth = (double)input.LoanAmount / totalMonths;
            }

            DateTime baseDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);

            for (int i = 1; i <= totalMonths; i++)
            {
                double interest = balance * monthlyRate;
                double principal = (input.RepaymentType == "元利均等") ? (monthlyPayment - interest) : principalPerMonth;
                double payment = (input.RepaymentType == "元利均等") ? monthlyPayment : principal + interest;

                list.Add(new AmortizationRow
                {
                    回数 = i,
                    振替日 = baseDate.AddMonths(i - 1),
                    返済金額 = (int)Math.Round(payment),
                    元金額 = (int)Math.Round(principal),
                    利息額 = (int)Math.Round(interest),
                    残高 = (int)Math.Round(balance - principal)
                });

                balance -= principal;
                if (balance < 0) balance = 0;
            }

            return list;
        }
    }
}
