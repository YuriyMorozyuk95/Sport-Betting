using IocContainer;
using Ninject;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;

namespace SportBetting.WPF.Prism.Shared.OldCode
{
    /// <summary>
    /// diese Klasse erg�nzt die in DataBinding definierte ObservableCollection von TipItems.
    /// ZB Informationen die in der TipListe angezeigt werden, wie das Produkt der Quoten der TipItems der Tipliste.
    /// </summary>
    public class TipListInfo
    {
        public static int MinSystemY = 3;
        public static int MinSystemX = MinSystemY - 1;

        #region Properties Getters and Setters


        private static decimal _stake = 0;
        private static decimal Stake
        {
            get { return ChangeTracker.MultipleSingles == null ? ChangeTracker.NewTicket.Stake : _stake; }
        }
        private static IChangeTracker ChangeTracker
        {
            get { return IoCContainer.Kernel.Get<IChangeTracker>(); }
        }

        public decimal Bet
        {
            get { return Stake; }
            set
            {
                _stake = ChangeTracker.MultipleSingles == null ? 0 : value;
                CurrentTicketPossibleWin = (MinBet == 0 && MinWin == 0) ? 0 : (MinWin / MinBet * Stake); //Just in case that the minBet is not 1 Euro
            }
        }

        public decimal CurrentTicketPossibleWin { get; set; }

        public int? MinCombination { get; set; }

        public int SystemX
        {
            get { return _systemX; }
            set
            {
                if (value >= MinSystemX && value < SystemY && value != _systemX)
                {
                    _systemX = value;
                    //SystemXY = SystemX.ToString() + " / " + numSystemY.ToString();
                }
            }
        }

        public int SystemY
        {
            get { return NumOfTipps - PathCount; }
        }


        public int PathCount { get; set; }


        public decimal MultiWayOddFactor
        { //usefull??
            get;
            set;
        }

        public decimal MinBet { get; set; }

        public decimal MaxBet { get; set; }

        public decimal MinWin { get; set; }

        public decimal MaxWin { get; set; }


        public decimal BonusFactor { get; set; }

        public decimal OddOfTipps { get; set; }

        public decimal FullOddFactor { get; set; }

        public int NumOfTipps { get; set; }

        public bool IsMaxOddBet { get; set; }

        #endregion

        private int _systemX;

        public bool WarningsAlreadyConfirmed { get; set; }

        public int RowCount { get; set; }

        public decimal IllegalOddFactor { get; set; }

        public decimal ManipulationFeePercentage { get; set; }

        public decimal ManipulationFeeReduction
        {
            get { return (100 - ManipulationFeePercentage) / 100; }
        }

        public decimal ManipulationFeeValue
        {
            get { return (ManipulationFeePercentage * Stake / 100); }
        }

        public decimal ReducedStakeValue
        {
            get { return (1 - ManipulationFeePercentage / 100) * Stake; }
        }


        public decimal BonusValue
        {
            get { return (BonusFactor - 1) * (Stake - ManipulationFeeValue) * FullOddFactor; }
        }

        public decimal BonusFactorPerc
        {
            get { return (BonusFactor - 1) * 100; }
        }


        public void ResetNumXY()
        {
            if (SystemY >= MinSystemY)
                SystemX = SystemY - 1;
        }
    }
}