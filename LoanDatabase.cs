using SQLite;

namespace ALCM.Data
{
    /// <summary>
    /// ローン情報と段階金利を管理するSQLiteラッパークラス。
    /// 償還表は保存せず、取り出すときに計算を行う。
    /// </summary>
    public class LoanDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public LoanDatabase(string dbPath)
        {
            // フラグ指定によりDBが無ければ作成し、マルチスレッドアクセスを許可する
            var flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;
            _database = new SQLiteAsyncConnection(dbPath, flags);
        }

        /// <summary>
        /// テーブルを初期化する。
        /// </summary>
        public async Task InitializeAsync()
        {
            await _database.CreateTableAsync<LoanInfo>();
            await _database.CreateTableAsync<LoanTier>();
        }

        /// <summary>
        /// ローンを追加し、IDを返す。段階金利もまとめて登録する。
        /// </summary>
        public async Task<int> InsertLoanAsync(LoanInfo loan, IEnumerable<LoanTier> tiers)
        {
            await _database.InsertAsync(loan);
            foreach (var tier in tiers.Select((t, idx) =>
            { t.LoanId = loan.LoanId; t.TierIndex = idx; return t; }))
            {
                await _database.InsertAsync(tier);
            }
            return loan.LoanId;
        }

        /// <summary>
        /// 全てのローンとその段階金利を取得する。
        /// </summary>
        public async Task<List<LoanWithTiers>> GetAllLoansAsync()
        {
            var loans = await _database.Table<LoanInfo>().ToListAsync();
            var tiers = await _database.Table<LoanTier>().ToListAsync();
            var result = from loan in loans
                         select new LoanWithTiers
                         {
                             Loan = loan,
                             Tiers = tiers.Where(t => t.LoanId == loan.LoanId)
                                          .OrderBy(t => t.TierIndex)
                                          .ToList()
                         };
            return result.ToList();
        }

        /// <summary>
        /// 指定ローンの償還表を計算して返す。DBには保存しない。
        /// </summary>
        public async Task<List<AmortizationRow>> CalculateScheduleAsync(int loanId)
        {
            var loan = await _database.FindAsync<LoanInfo>(loanId);
            if (loan == null) return new List<AmortizationRow>();
            var tiers = await _database.Table<LoanTier>()
                                       .Where(t => t.LoanId == loanId)
                                       .OrderBy(t => t.TierIndex)
                                       .ToListAsync();

            // LoanCalculator.Generateを呼んで償還表を作成
            var input = new LoanInputData
            {
                LoanAmount = loan.LoanAmount,
                Years1 = tiers.ElementAtOrDefault(0)?.Years ?? 0,
                Years2 = tiers.ElementAtOrDefault(1)?.Years ?? 0,
                Years3 = tiers.ElementAtOrDefault(2)?.Years ?? 0,
                Years4 = tiers.ElementAtOrDefault(3)?.Years ?? 0,
                InterestRate1 = tiers.ElementAtOrDefault(0)?.InterestRate ?? 0,
                InterestRate2 = tiers.ElementAtOrDefault(1)?.InterestRate ?? 0,
                InterestRate3 = tiers.ElementAtOrDefault(2)?.InterestRate ?? 0,
                InterestRate4 = tiers.ElementAtOrDefault(3)?.InterestRate ?? 0,
                RepaymentType = loan.RepaymentType
            };
            return LoanCalculator.Generate(input);
        }
    }

    /// <summary>
    /// ローン基本情報のエンティティ。
    /// </summary>
    public class LoanInfo
    {
        [PrimaryKey, AutoIncrement]
        public int LoanId { get; set; }

        public string LoanName { get; set; } = "";
        public int LoanAmount { get; set; }
        public string AgreementDate { get; set; } = "";
        public string StartDate { get; set; } = "";
        public string RepaymentType { get; set; } = "";
    }

    /// <summary>
    /// 段階金利のエンティティ。
    /// </summary>
    public class LoanTier
    {
        [PrimaryKey, AutoIncrement]
        public int TierId { get; set; }

        public int LoanId { get; set; }
        public int TierIndex { get; set; }
        public int Years { get; set; }
        public double InterestRate { get; set; }
    }

    /// <summary>
    /// ローンと段階金利をまとめたDTO。
    /// </summary>
    public class LoanWithTiers
    {
        public LoanInfo Loan { get; set; } = new();
        public List<LoanTier> Tiers { get; set; } = new();
    }
}
