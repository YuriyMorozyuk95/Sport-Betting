using System;
using System.Collections.Generic;
using System.Linq;
using IocContainer;
using Ninject;
using Shared;
using Shared.Interfaces;
using SharedInterfaces;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Services.Interfaces;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.DAL.ViewObjects;
using TranslationByMarkupExtension;

namespace SportBetting.WPF.Prism.Shared.OldCode
{



    public static class DataBinding
    {
        private static TipListInfo _tipListInfo = new TipListInfo();

        private static List<TipItemVw> _tipItems = new List<TipItemVw>();
        public static List<TipItemVw> TipItems
        {
            get
            {
                if (ChangeTracker.MultipleSingles != null)
                    return _tipItems;
                else
                    return ChangeTracker.NewTicket.TipItems.Where(x => x.IsChecked).ToList();
            }

            set
            {
                if (ChangeTracker.MultipleSingles != null)
                    _tipItems = value;
            }
        }

        private static IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }
        private static IChangeTracker ChangeTracker
        {
            get { return IoCContainer.Kernel.Get<IChangeTracker>(); }
        }
        private static ITranslationProvider TranslationProvider
        {
            get { return IoCContainer.Kernel.Get<ITranslationProvider>(); }
        }



        private static decimal BonusFromOdd
        {
            get { return StationRepository.GetBonusFromOdd(ChangeTracker.NewTicket); }
        }

        private static decimal MaxOdd
        {
            get { return StationRepository.GetMaxOdd(ChangeTracker.NewTicket); }
        }

        private static decimal MinStakeCombiBet
        {
            get { return StationRepository.GetMinStakeCombiBet(ChangeTracker.NewTicket); }
        }

        private static decimal MinStakeSystemBet
        {
            get { return StationRepository.GetMinStakeSystemBet(ChangeTracker.NewTicket); }
        }

        private static decimal MinStakeSingleBet
        {
            get { return StationRepository.GetMinStakeSingleBet(ChangeTracker.NewTicket); }
        }
        public static TipListInfo TipListInfo
        {
            get { return _tipListInfo; }
            set { _tipListInfo = value; }
        }

        private static void RefreshBanks()
        {
            foreach (var tipItem in TipItems)
            {
                tipItem.IsBankReadOnly = false;

                if (TipItems.Count(x => x.Odd.BetDomain.BetDomainId == tipItem.Odd.BetDomain.BetDomainId) < 2)
                {
                    continue;
                }
                foreach (TipItemVw curItem in TipItems)
                {
                    if (curItem.Odd.BetDomain.BetDomainId != tipItem.Odd.BetDomain.BetDomainId)
                        continue;
                    curItem.IsBank = true;
                    curItem.IsBankReadOnly = true;
                    curItem.IsWay = true;
                }

            }

        }

        public static bool AddBank(OddVw curOdd)
        {
            int count = 0;
            foreach (TipItemVw curItem in TipItems)
            {
                if (curItem.Odd.BetDomain.BetDomainId == curOdd.LineObject.BetDomain.BetDomainId)
                {
                    count++;
                    curItem.IsBank = true;
                    curItem.IsBankReadOnly = true;
                }
            }
            return count > 0;

        }

        public static bool RemoveBank(OddVw curOdd)
        {
            int count = 0;
            TipItemVw modifyItem = null;
            foreach (TipItemVw curItem in TipItems)
            {
                if (curItem.Odd.BetDomain.BetDomainId == curOdd.LineObject.BetDomain.BetDomainId)
                {
                    count++;
                    modifyItem = curItem;
                }
            }
            if (count < 2 && modifyItem != null)
            {
                modifyItem.IsBank = false;
            }
            return count < 2;
        }




        /// <summary>
        /// Eintrag aus Ticket l?schen
        /// </summary>
        /// <param name="ticketState"> </param>
        /// <param name="TipListInfo"> </param>
        /// <param name="tipItems"> </param>
        /// <param name="reset"> </param>
        /// <param name="p"></param>
        static object locker = new object();
        public static void UpdateSystemOrCombiticket(Ticket ticketToCalculate)
        {
            lock (locker)
            {


                if (ticketToCalculate == null)
                    ticketToCalculate = ChangeTracker.NewTicket;

                List<TipItemVw> localTipItems = new List<TipItemVw>();
                localTipItems = ticketToCalculate.TipItems.Where(x => x.IsChecked).ToList();

                decimal oddVal = 1;
                decimal multiWayOddVal = 1;
                int? minCombMax = 0;
                int bonusTipsCount = 0;

                Dictionary<int, List<TipItemVw>> tipItemDict = new Dictionary<int, List<TipItemVw>>();
                foreach (TipItemVw t in localTipItems)
                {
                    int iSvrMatchId = (int)t.Match.MatchId;

                    if (tipItemDict.ContainsKey(iSvrMatchId))
                    {
                        tipItemDict[iSvrMatchId].Add(t);
                    }
                    else
                    {
                        List<TipItemVw> list = new List<TipItemVw>();
                        list.Add(t);
                        tipItemDict.Add(iSvrMatchId, list);
                    }
                }
                // de: Anzahl der Wege berechnen                // en: Number of ways to calculate
                // de: h?chste Quote der Mehrwege finden        // en: highest rate of multipath are
                //
                int pathCount = 0;
                int rowCount = 1;
                int singlesCount = 0;
                foreach (List<TipItemVw> list in tipItemDict.Values)
                {
                    if (list.Count >= 1)
                    {
                        decimal maxOdd = 1;
                        foreach (TipItemVw tip in list)
                        {
                            if (maxOdd < tip.Odd.Value.Value)
                            {
                                maxOdd = tip.Odd.Value.Value;
                            }
                        }
                        TipItemVw t = list[0];
                        if (t.IsBank || list.Count > 1)
                        {
                            pathCount++;
                            rowCount *= list.Count;
                            multiWayOddVal *= maxOdd;
                        }
                        else
                        {
                            oddVal *= maxOdd;
                            singlesCount++;
                        }
                        int? curCombMax = minCombinationOfAll(t);
                        if (curCombMax > minCombMax)
                        {
                            minCombMax = curCombMax;
                        }
                        if (list.Count == 1 && ticketToCalculate.TicketState == TicketStates.Multy)
                        {
                            if (t.Odd.Value.Value >= BonusFromOdd)
                                //Bonus gibt es nicht bei Mehrwegen
                                bonusTipsCount++;
                        }
                    }
                }

                TipListInfo.MinCombination = minCombMax;
                TipListInfo.NumOfTipps = tipItemDict.Count;
                TipListInfo.PathCount = pathCount;
                TipListInfo.RowCount = rowCount;
                TipListInfo.MultiWayOddFactor = multiWayOddVal;
                TipListInfo.CurrentTicketPossibleWin = 0;
                TipListInfo.FullOddFactor = 0;

                TipListInfo.ManipulationFeePercentage = StationRepository.GetManipulationFeePercentage(ticketToCalculate);
                TipListInfo.BonusFactor = StationRepository.GetBonusValueForBets(ticketToCalculate) / 100 + 1;

                switch (ticketToCalculate.TicketState)
                {
                    default:
                        if (localTipItems.Count > 1 && ticketToCalculate.TicketState != TicketStates.MultySingles)
                        {
                            ticketToCalculate.TicketState = TicketStates.Multy;
                            UpdateSystemOrCombiticket(ticketToCalculate);
                            return;
                        }
                        foreach (var tipItem in localTipItems)
                        {
                            tipItem.IsBank = false;
                        }
                        if (localTipItems.Count == 1 && ticketToCalculate.TicketState != TicketStates.MultySingles)
                        {
                            UpdateSingleticketItems(localTipItems, ticketToCalculate);
                        }
                        if (localTipItems.Count == 0)
                        {
                            TipListInfo.MinBet = 0;
                            TipListInfo.OddOfTipps = 0;
                            TipListInfo.MaxBet = 0;
                            TipListInfo.MinWin = 0;
                            TipListInfo.MaxWin = 0;
                            TipListInfo.ManipulationFeePercentage = 0;
                            TipListInfo.ResetNumXY();
                        }
                        break;
                    case TicketStates.MultySingles:
                        return;
                    case TicketStates.Multy:
                        //rowCount = 1;
                        TipListInfo.ResetNumXY();
                        if (localTipItems.Count < 2)
                        {
                            ticketToCalculate.TicketState = TicketStates.Single;
                            UpdateSystemOrCombiticket(ticketToCalculate);
                            return;
                        }
                        //TipListInfo.FullOddFactor = oddVal * multiWayOddVal * TipListInfo.BonusFactor;
                        TipListInfo.FullOddFactor = oddVal * multiWayOddVal;
                        if (TipListInfo.FullOddFactor >= MaxOdd)
                        {
                            TipListInfo.IsMaxOddBet = true;
                            if (TipListInfo.FullOddFactor > MaxOdd)
                            {
                                TipListInfo.IllegalOddFactor = TipListInfo.FullOddFactor;
                                ticketToCalculate.MaxOddExceeded = true;
                            }
                            TipListInfo.FullOddFactor = MaxOdd;
                            TipListInfo.OddOfTipps = MaxOdd;
                        }
                        else
                        {
                            TipListInfo.OddOfTipps = oddVal * multiWayOddVal;
                            TipListInfo.IllegalOddFactor = 0;
                            TipListInfo.IsMaxOddBet = false;
                            ticketToCalculate.MaxOddExceeded = false;
                        }
                        foreach (var tipItem in localTipItems)
                        {
                            tipItem.IsBank = false;
                        }
                        TipListInfo.MinBet = MinStakeCombiBet;
                        TipListInfo.MaxBet = StationRepository.GetMaxStakeCombi(ticketToCalculate);

                        TipListInfo.MinWin = TipListInfo.FullOddFactor * TipListInfo.MinBet / rowCount * (100 - TipListInfo.ManipulationFeePercentage) / 100;

                        var manipulationFee = (TipListInfo.MaxBet / rowCount * TipListInfo.ManipulationFeePercentage) / 100;
                        //TipListInfo.MaxWin = ((TipListInfo.MaxBet / rowCount - manipulationFee) * TipListInfo.FullOddFactor * TipListInfo.BonusFactor) - TipListInfo.MaxBet / rowCount;
                        //TipListInfo.MaxWin *= rowCount;
                        TipListInfo.MaxWin = (((TipListInfo.MaxBet - manipulationFee) / rowCount) * TipListInfo.FullOddFactor * TipListInfo.BonusFactor) - TipListInfo.MaxBet;

                        var maxWinMultiBetFromAdmin = StationRepository.GetMaxWinMultiBet(ticketToCalculate);
                        if (maxWinMultiBetFromAdmin > 0 && TipListInfo.MaxWin > maxWinMultiBetFromAdmin)
                        {
                            var a0 = 100 * maxWinMultiBetFromAdmin * rowCount;
                            var a1 = (100 - TipListInfo.ManipulationFeePercentage);
                            var a2 = (a1 * TipListInfo.BonusFactor * TipListInfo.FullOddFactor);

                            var varX = (a0) / (a2 - 100 * rowCount);

                            TipListInfo.MaxBet = Round(varX, 2);
                            //manipulationFee = (TipListInfo.MaxBet / rowCount * TipListInfo.ManipulationFeePercentage) / 100;
                        }

                        //var posWin = TipListInfo.FullOddFactor * TipListInfo.Bet / rowCount;
                        //TipListInfo.CurrentTicketPossibleWin = posWin * ((100 - TipListInfo.ManipulationFeePercentage) / 100) * TipListInfo.BonusFactor;

                        var pw1 = (TipListInfo.Bet - (TipListInfo.Bet * TipListInfo.ManipulationFeePercentage / 100)) * TipListInfo.BonusFactor * TipListInfo.FullOddFactor;
                        TipListInfo.CurrentTicketPossibleWin = pw1 / rowCount;

                        //TipListInfo.MaxWin = (TipListInfo.MaxBet - manipulationFee) * TipListInfo.FullOddFactor * TipListInfo.BonusFactor / rowCount;
                        TipListInfo.MaxWin = ((TipListInfo.MaxBet - (TipListInfo.MaxBet * TipListInfo.ManipulationFeePercentage / 100)) * TipListInfo.BonusFactor * TipListInfo.FullOddFactor) / rowCount;
                        TipListInfo.FullOddFactor = TipListInfo.FullOddFactor;
                        break;
                    case TicketStates.System:
                        TipListInfo.SystemX = ticketToCalculate.SystemX;
                        TipListInfo.IllegalOddFactor = 0;

                        int ind = 0;
                        if (!LimitHandling.SystemBetYAllowed(TipListInfo.SystemY))
                        {
                            //avoid useless time and memory consuming calculations
                            TipListInfo.MinBet = 0;
                            TipListInfo.OddOfTipps = 0;
                            TipListInfo.MaxBet = 0;
                            TipListInfo.MinWin = 0;
                            TipListInfo.MaxWin = 0;
                            TipListInfo.ManipulationFeePercentage = 0;
                            TipListInfo.ResetNumXY();
                            //UpdateSystemOrCombiticket(ticketToCalculate);
                            return;
                        }
                        if (TipListInfo.SystemX == 0)
                            TipListInfo.ResetNumXY();


                        decimal[] oddVals = new decimal[TipListInfo.SystemY];
                        bool disableBankBtn = TipListInfo.PathCount + TipListInfo.MinSystemY >= TipListInfo.NumOfTipps;
                        decimal dBankTipValue = 0;
                        foreach (TipItemVw t in localTipItems)
                        {
                            if (!t.IsBank && t.Odd != null)
                            {
                                if (ind < oddVals.Length)
                                    oddVals[ind++] = t.Odd.Value.Value;
                                else
                                {
                                }


                                t.IsBankEnabled = !disableBankBtn;
                            }
                            else if (t.IsBank && t.Odd != null)
                            {
                                if (dBankTipValue == 0)
                                    dBankTipValue = 1; // to make multiplication possible;
                                dBankTipValue *= t.Odd.Value.Value;
                            }
                        }
                        RefreshBanks();

                        decimal maxOdd = OddUtilities.AllCombinationsSum(oddVals, TipListInfo.SystemX); // TipListInfo.SystemX
                        if (maxOdd == 0)
                        {
                            ticketToCalculate.TicketState = TicketStates.Multy;
                            UpdateSystemOrCombiticket(ticketToCalculate);
                            return;
                        }

                        if (maxOdd * TipListInfo.MultiWayOddFactor > MaxOdd)
                        {
                            maxOdd = MaxOdd;
                            TipListInfo.MultiWayOddFactor = 1;
                            ticketToCalculate.MaxOddExceeded = true;
                        }
                        else
                        {
                            ticketToCalculate.MaxOddExceeded = false;
                        }

                        if (oddVals.Length < TipListInfo.SystemX)
                        {
                            UpdateSystemOrCombiticket(ticketToCalculate);
                            return;
                        }

                        //ViewControl.SetTicketErrorMessage(TicketError.NoError);
                        TipListInfo.MinBet = MinStakeSystemBet;
                        //TipListInfo.MaxBet = StationSettings.Station.MaxStakeSystemBet;
                        TipListInfo.OddOfTipps = maxOdd;

                        decimal manFee = 0;
                        manFee = StationRepository.GetManipulationFeePercentage(ChangeTracker.NewTicket);
                        //TipListInfo.MaxBet = (100 * StationRepository.GetMaxWinSystemBet(ChangeTracker.NewTicket)) / ((100 - manFee) * TipListInfo.BonusFactor * (maxOdd * TipListInfo.MultiWayOddFactor / TipListInfo.RowCount));

                        TipListInfo.MaxBet = (100 * StationRepository.GetMaxWinSystemBet(ChangeTracker.NewTicket) * TipListInfo.RowCount) / ((100 - manFee) * TipListInfo.BonusFactor * (maxOdd * TipListInfo.MultiWayOddFactor));

                        TipListInfo.MaxBet = Round(Math.Min(TipListInfo.MaxBet, StationRepository.GetMaxStakeSystemBet(ChangeTracker.NewTicket)), 2);

                        TipListInfo.MinWin = TipListInfo.MinBet * maxOdd * TipListInfo.MultiWayOddFactor / TipListInfo.RowCount;
                        //TipListInfo.MaxWin = TipListInfo.MaxBet * maxOdd * TipListInfo.MultiWayOddFactor / TipListInfo.RowCount * TipListInfo.BonusFactor * (100 - TipListInfo.ManipulationFeePercentage) / 100;

                        TipListInfo.MaxWin = ((TipListInfo.MaxBet * (100 - TipListInfo.ManipulationFeePercentage) / 100) * TipListInfo.BonusFactor * maxOdd * TipListInfo.MultiWayOddFactor) / TipListInfo.RowCount;


                        /////////////////////////////////////////////
                        var maxWinSystemBetFromAdmin = StationRepository.GetMaxWinSystemBet(ticketToCalculate);
                        if (maxWinSystemBetFromAdmin > 0 && TipListInfo.MaxWin > maxWinSystemBetFromAdmin)
                        {
                            TipListInfo.MaxWin = maxWinSystemBetFromAdmin;
                            //manFee = StationRepository.GetManipulationFeePercentage(ChangeTracker.NewTicket);
                            //TipListInfo.MaxBet = (100 * TipListInfo.MaxWin) / ((100 - manFee) * TipListInfo.BonusFactor * (maxOdd * TipListInfo.MultiWayOddFactor / TipListInfo.RowCount));
                            TipListInfo.MaxBet = Round((100 * StationRepository.GetMaxWinSystemBet(ChangeTracker.NewTicket) * TipListInfo.RowCount) / ((100 - manFee) * TipListInfo.BonusFactor * (maxOdd * TipListInfo.MultiWayOddFactor)), 2);
                        }
                        /////////////////////////////////////////////

                        if (dBankTipValue > 0)
                        {
                            if (maxOdd * (multiWayOddVal / rowCount) > MaxOdd)
                            {
                                TipListInfo.FullOddFactor = MaxOdd;
                                ticketToCalculate.MaxOddExceeded = true;
                            }
                            else
                            {
                                TipListInfo.FullOddFactor = maxOdd * multiWayOddVal;
                                ticketToCalculate.MaxOddExceeded = false;
                            }
                        }
                        else
                        {

                            TipListInfo.FullOddFactor = maxOdd;

                        }
                        //TipListInfo.CurrentTicketPossibleWin = TipListInfo.Bet * maxOdd * TipListInfo.MultiWayOddFactor / TipListInfo.RowCount * TipListInfo.BonusFactor * (100 - TipListInfo.ManipulationFeePercentage) / 100;

                        TipListInfo.CurrentTicketPossibleWin = ((TipListInfo.Bet - ((TipListInfo.Bet * TipListInfo.ManipulationFeePercentage) / 100)) * TipListInfo.BonusFactor * maxOdd * TipListInfo.MultiWayOddFactor) / TipListInfo.RowCount;

                        TipListInfo.OddOfTipps = maxOdd;


                        break;
                }


                ticketToCalculate.NumberOfBets = localTipItems.Count;
                ticketToCalculate.TotalOddDisplay = TipListInfo.FullOddFactor;
                ticketToCalculate.ManipulationFeeValue = TipListInfo.ManipulationFeeValue;
                ticketToCalculate.MaxBet = TipListInfo.MaxBet;
                //ticketToCalculate.MaxBet = Math.Min(TipListInfo.MaxBet, StationRepository.GetMaxStake(ticketToCalculate));
                //ticketToCalculate.MaxBet = Math.Min(TipListInfo.MaxBet, TipListInfo.Bet);
                ticketToCalculate.MinBet = TipListInfo.MinBet;
                ticketToCalculate.BonusPercentage = TipListInfo.BonusFactorPerc;
                ticketToCalculate.BonusValue = TipListInfo.BonusValue / TipListInfo.RowCount;
                ticketToCalculate.MaxWin = TipListInfo.MaxWin;
                ticketToCalculate.RowCount = TipListInfo.RowCount;
                ticketToCalculate.StakeByRow = ticketToCalculate.Stake / TipListInfo.RowCount;
                ticketToCalculate.CurrentTicketPossibleWin = TipListInfo.CurrentTicketPossibleWin;
                ticketToCalculate.SystemX = TipListInfo.SystemX;
                ticketToCalculate.SystemY = TipListInfo.SystemY;
                //ticketToCalculate.SystemButtonName = string.Format(TranslationProvider.Translate(MultistringTags.SYSTEM_FORMAT) as string, TipListInfo.SystemX, TipListInfo.SystemY, "+Banker");
            }

        }

        private static decimal Round(decimal value, int digitsAfterComa)
        {
            for (int i = 0; i < digitsAfterComa; i++)
            {
                value *= 10;
            }
            var intValue = (int)value;
            value = intValue;
            for (int i = 0; i < digitsAfterComa; i++)
            {
                value /= 10;
            }
            return value;

        }

        public static void UpdateSingleticketItems()
        {
            UpdateSingleticketItems(TipItems);
        }

        public static void UpdateSingleticketItems(IList<TipItemVw> tipItems, Ticket ticketToCalculate = null)
        {
            decimal oddVal = 0;
            ///ASSERTION  :  Tipitem has exactly one item

            if (ChangeTracker.MultipleSingles != null)
            {
                TipListInfo.Bet = ticketToCalculate.Stake;
                TipListInfo.BonusFactor = ticketToCalculate.BonusValue;
            }

            foreach (var tipItem in tipItems)
            {
                oddVal += tipItem.Odd.Value.Value;
            }

            if (tipItems.Count > 0)
                TipListInfo.MinCombination = minCombinationOfAll(tipItems[0]);

            TipListInfo.NumOfTipps = 1;
            TipListInfo.OddOfTipps = oddVal;
            TipListInfo.FullOddFactor = oddVal;

            if (TipListInfo.FullOddFactor >= MaxOdd)
            {
                TipListInfo.IsMaxOddBet = true;
                ticketToCalculate.MaxOddExceeded = false;
                if (TipListInfo.FullOddFactor > MaxOdd)
                {
                    TipListInfo.IllegalOddFactor = TipListInfo.FullOddFactor;
                    ticketToCalculate.MaxOddExceeded = true;
                }
                TipListInfo.FullOddFactor = MaxOdd;
                TipListInfo.OddOfTipps = MaxOdd;
            }
            else
            {
                TipListInfo.IllegalOddFactor = 0;
                TipListInfo.IsMaxOddBet = false;
                ticketToCalculate.MaxOddExceeded = false;
            }



            TipListInfo.MinBet = MinStakeSingleBet;

            //if (ChangeTracker.MultipleSingles == null)
            //{
            // MaxBet and MaxWin for MultiSingles is assigned in UpdateTicket()
            if (tipItems.Count > 0)
                TipListInfo.MaxBet = Round(LimitHandling.SingleMaxStake(oddVal * TipListInfo.ManipulationFeeReduction, ticketToCalculate), 2);
            else
                TipListInfo.MaxBet = Round(LimitHandling.SingleMaxStake(1 * TipListInfo.ManipulationFeeReduction, ticketToCalculate), 2);

            TipListInfo.MaxWin = TipListInfo.OddOfTipps * TipListInfo.MaxBet *
                             ((TipListInfo.BonusFactor > 0) ? TipListInfo.BonusFactor : 1) *
                             TipListInfo.ManipulationFeeReduction;
            var maxWinSingleBetFromAdmin = StationRepository.GetMaxWinSingleBet(ticketToCalculate);
            if (maxWinSingleBetFromAdmin > 0 && TipListInfo.MaxWin > maxWinSingleBetFromAdmin)
            {
                TipListInfo.MaxWin = maxWinSingleBetFromAdmin;
                var manipulationFee = StationRepository.GetManipulationFeePercentage(ChangeTracker.NewTicket);
                TipListInfo.MaxBet = Round(100 * TipListInfo.MaxWin / ((100 - manipulationFee) * TipListInfo.BonusFactor * TipListInfo.FullOddFactor), 2);
            }
            //}
            //else
            //{
            //    TipListInfo.MaxBet = ticketToCalculate.MaxBet;
            //    TipListInfo.MaxWin = ticketToCalculate.MaxWin;
            //}

            TipListInfo.MinWin = TipListInfo.OddOfTipps * TipListInfo.MinBet *
                                 ((TipListInfo.BonusFactor > 0) ? TipListInfo.BonusFactor : 1) *
                                 TipListInfo.ManipulationFeeReduction;

            TipListInfo.CurrentTicketPossibleWin = TipListInfo.OddOfTipps * TipListInfo.Bet *
                                  ((TipListInfo.BonusFactor > 0) ? TipListInfo.BonusFactor : 1) *
                                  TipListInfo.ManipulationFeeReduction;
        }

        private static int? minCombinationOfAll(TipItemVw item)
        {
            int? minComb = 0;
            //if (item != null && item.Odd != null && item.Odd.BetDomain != null)
            //{
            //    minComb = item.Odd.BetDomain.MinCombination;
            //    var curMatch = item.Odd.BetDomain.Match;
            //    if (curMatch != null && curMatch.MinCombination > minComb)
            //    {
            //        minComb = curMatch.MinCombination;
            //    }
            //}
            return minComb;
        }





        /// <summary>
        /// Change SystemX Value
        /// </summary>
        public static void ChangeSystemX(int val, TicketStates state, Ticket newTicket)
        {
            if (val > 0 || TipListInfo.MinCombination < TipListInfo.SystemX + TipListInfo.PathCount)
            {
                newTicket.SystemX += val;
                UpdateSystemOrCombiticket(newTicket);
            }
        }
        public static void SetSystemX(int val, TicketStates state)
        {
            if (val > 0 || TipListInfo.MinCombination < TipListInfo.SystemX + TipListInfo.PathCount)
            {
                TipListInfo.SystemX = val;
                UpdateSystemOrCombiticket(null);
            }
        }
    }
}