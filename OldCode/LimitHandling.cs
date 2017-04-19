using System;
using System.Diagnostics;
using System.Text;
using Nbt.Common.BusinessObjects;
using Nbt.Common.Database;
using Nbt.Common.src.Nbt.Common.BusinessObjects;
using Nbt.EntityObject.BetOffer;
using Nbt.EntityObject.SportBase;
using Nbt.EntityObject.Stations;
using Nbt.EntityObject.BetDomains;
using Nbt.EntityObject.Ticket;
using Nbt.EntityObject.Common;
using SportBetting.WPF.Prism.Services;


namespace Nbt.Station.Common {
    /// <summary>
    /// checks limits on Odds and Tournaments, locks offer if a limit is hit and StationSr.LockOfferOnLimit is true and sends warning to server
    /// </summary>
    public class LimitHandling
    {
        private static bool alreadyWarnedCombiLimits = false;

		private static StationSr Station
		{
			get { return Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().Station; }
			set { Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().Station = value; }
		}
		private static CombiLimit[] CombiSize
		{
			get { return Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().CombiSize; }
			set { Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().CombiSize = value; }
		}
		private static decimal MaxWinSystemBet
		{
			get { return Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().MaxWinSystemBet; }
			set { Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().MaxWinSystemBet = value; }
		}
		private static decimal MaxOdd
		{
			get { return Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().MaxOdd; }
			set { Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().MaxOdd = value; }
		}
		private static decimal MinStakeCombiBet
		{
			get { return Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().MinStakeCombiBet; }
			set { Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().MinStakeCombiBet = value; }
		}
		private static decimal MinStakeSystemBet
		{
			get { return Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().MinStakeSystemBet; }
			set { Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().MinStakeSystemBet = value; }
		}
		private static decimal MinStakeSingleBet
		{
			get { return Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().MinStakeSingleBet; }
			set { Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().MinStakeSingleBet = value; }
		}
        /// <summary>
        /// common limit for all bets (single, combi, system)
        /// </summary>
        /// <param name="maxOdd">maximum odd of the bet</param>
        /// <returns></returns>
        public static bool BetMaxOddAllowed(decimal maxOdd)
        {
            if (Station == null)
                return false;

            return maxOdd <= Station.MaxOdd;
        }

        /// <summary>
        /// Checks, if the mincombination is reached for a specific mincombination and the number of tips selected
        /// </summary>
        /// <param name="numSystemXplusBank_or_numCombiTips">the number of tips selected</param>
        /// <param name="minCombination">largest the mincombination needed by one of the tips</param>
        /// <returns></returns>
        public static bool MinCombinationReached(int numSystemXplusBank_or_numCombiTips, int minCombination)
        {
            return (numSystemXplusBank_or_numCombiTips >= minCombination);
        }

        /// <summary>
        /// checks the mincombination on Match and betDomain and checks, which mincombination is larger
        /// is called by LimitHandling.MinCombinationReachedForAllBetDomains
        /// </summary>
        /// <param name="pBetDomain">the betDomain to check</param>
        /// <returns>the largest mincombination</returns>
        public static int MinCombination(BetDomainSr pBetDomain)
        {
            int result = pBetDomain.MinCombination;
            MatchSr tempMatch = pBetDomain.Match;
            if (result < tempMatch.MinCombination)
            {
                result = tempMatch.MinCombination;
            }
            ///Removed check for MinCombi in Tournament which is not used any more

            ///g GMA: 20.02.2008 no FranchisorToTournamentFactory on client
            //FranchisorToTournament tempF2T = FranchisorToTournamentFactory.LoadFranchisorToTournamentByQuery(" TournamentID = " + tempTournament.ORMID + " and FranchisorID = " + stationSr.Franchisor2Station.ORMID, null);
            //if (tempF2T != null)
            //{
            //    if (result < tempF2T.MinCombination)
            //    {
            //        result = tempF2T.MinCombination;
            //    }
            //}
            return result;
        }

        /// <summary>
        /// Checks if mincombination for all selected BetDomains is reached
        /// </summary>
        /// <param name="pBetDomains">the BetDomains to be checked</param>
        /// <returns>true if mincombination is smaller than pBetDomains.length</returns>
        public static bool MinCombinationReachedForAllBetDomains(BetDomainSr[] pBetDomains)
        {
            int minComb = 0;
            for (int i = 0; i < pBetDomains.Length; i++)
            {
                int tempMinComb = MinCombination(pBetDomains[i]);
                minComb = Math.Max(minComb, tempMinComb);
            }
            return MinCombinationReached(pBetDomains.Length, minComb);
        }

        /********************
         * SingleBet Checks *
         *******************/
        /// <summary>
        /// Checks if stake larger than StationSr.MinStakeSingleBet
        /// </summary>
        /// <param name="actStake">the stake of the single bet</param>
        /// <returns>true if ok</returns>
        public static bool SingleBetMinStakeAllowed(decimal actStake)
        {
            if (Station == null)
                return false;

            return (Station.MinStakeSingleBet <= actStake);
        }

        /// <summary>
        /// Checks if stake smaller than StationSr.MaxStakeSingleBet
        /// </summary>
        /// <param name="actStake">the stake of the single bet</param>
        /// <returns>true if ok</returns>
        public static bool SingleBetMaxStakeAllowed(decimal actStake)
        {
            if (Station == null)
                return false;

            return (Station.MaxStakeSingleBet >= actStake);
        }

        /// <summary>
        /// checks if stake is between minStake and maxStake
        /// </summary>
        /// <param name="actStake">the current stake of the single bet</param>
        /// <returns>true if ok</returns>
        public static bool SingleBetStakeAllowed(decimal actStake)
        {
            if (Station == null)
                return false;

            return (Station.MinStakeSingleBet <= actStake && actStake <= Station.MaxStakeSingleBet);
        }

        /// <summary>
        /// checks if the possible win of a singleBet is not highter than allowed
        /// </summary>
        /// <param name="possWin">the possible win of the single bet</param>
        /// <returns>true if ok</returns>
        public static bool SingleBetWinAllowed(decimal possWin)
        {
            if (Station == null)
                return false;

            return (possWin <= Station.MaxWinSingleBet);
        }

        /// <summary>
        /// calculates the maximum stake for a specific singleBet is according to its Odd
        /// </summary>
        /// <param name="maxOdd">the odd of the singleBet</param>
        /// <returns>the value of the maximum stake allowed</returns>
        public static decimal SingleMaxStake(decimal maxOdd)
        {
            if (Station == null)
                return 0;

            return Math.Min(Station.MaxWinSingleBet / maxOdd, Station.MaxStakeSingleBet);

        }




        /*********************
         * System-bet-Checks *
         ********************/
        /// <summary>
        /// Checks if min stake for a system bet is reached
        /// </summary>
        /// <param name="actStake">the stake to check</param>
        /// <returns>true if ok</returns>
        public static bool SystemBetMinStakeAllowed(decimal actStake)
        {
            if (Station == null)
                return false;

            return (Station.MinStakeSystemBet <= actStake);
        }

        /// <summary>
        /// Checks if curStake is not higher than maxStake
        /// </summary>
        /// <param name="actStake">the stake to check</param>
        /// <returns>true if ok</returns>
        public static bool SystemBetMaxStakeAllowed(decimal actStake)
        {
            if (Station == null)
                return false;

            return (actStake <= Station.MaxStakeSystemBet);
        }

        /// <summary>
        /// Checks if curStake is between minStake and maxStake
        /// </summary>
        /// <param name="actStake">the stake to check</param>
        /// <returns>true if ok</returns>
        public static bool SystemBetStakeAllowed(decimal actStake)
        {
            if (Station == null)
                return false;

            return (Station.MinStakeSystemBet <= actStake && actStake <= Station.MaxStakeSystemBet);
        }

        /// <summary>
        /// checks if Y of system bet x/Y is allowed because of StationSr.MaxSystemBet
        /// </summary>
        /// <param name="numSystemY">the Y value of System X/Y</param>
        /// <returns>true if ok</returns>
        public static bool SystemBetYAllowed(int numSystemY)
        {
            if (Station == null)
                return false;

            return (numSystemY <= Station.MaxSystemBet);
        }

        /// <summary>
        /// checks if possible win of bet is not larger than StationSr.MaxWinSystemBet
        /// </summary>
        /// <param name="possWin">the possible win to check</param>
        /// <returns>returns true if ok</returns>
        public static bool SystemBetWinAllowed(decimal possWin)
        {
            if (Station == null)
                return false;

            return (possWin <= Station.MaxWinSystemBet);
        }

        /// <summary>
        /// calculates max Stake for a systemBet with maxOdd (highest path multiplicated by MultiWayOddFacotor)
        /// </summary>
        /// <param name="maxOdd">maxOdd to check</param>
        /// <returns>the maximum allowed stake</returns>
        public static decimal SystemMaxStake(decimal maxOdd)
        {
            if (Station == null)
                return 0;
            return  Math.Round(Math.Min(Station.MaxWinSystemBet / maxOdd, Station.MaxStakeSystemBet),1);
        }
        /// <summary>
        /// Checks, if a systemTicket is allowed because of BetMaxOddAllowed, SystemBetStakeAllowed and SystemYAllowed and SystemBetWinWinAllowed
        /// </summary>
        /// <param name="rTicketWs">the ticket to check</param>
        /// <returns>true if ok</returns>
        public static bool SystemBetAllowed(TicketWS rTicketWs)
        {
            try
            {
                foreach (BetWS bet in rTicketWs.Bets)
                {
                    if (!(BetMaxOddAllowed(bet.MaxOdd) && SystemBetStakeAllowed(bet.Stake) && SystemBetYAllowed(bet.SystemY) &&
                        /*SystemBetWinAllowed((float)bet.Stake*bet.MaxOdd) &&*/  SystemBetWinAllowed(bet.MaxWin)))
                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                //_logger.Error("Error checking Combi Ticket!", e);
                return false;
            }
        }

        /**********************
         *   Combi-Bet-Checks *
         *********************/
        /// <summary>
        /// Checks if stake of combi bet between MinStakeCombiBet and MaxStakeCombiBet
        /// </summary>
        /// <param name="actStake">the stake to check</param>
        /// <returns>true if ok</returns>
        public static bool CombiBetStakeAllowed(decimal actStake)
        {
            if (Station == null)
                return false;

            return (Station.MinStakeCombiBet <= actStake && actStake <= Station.MaxStakeCombi);
        }

        /// <summary>
        /// Checks if MinStakeCombiBet is reached
        /// </summary>
        /// <param name="actStake">the stake to check</param>
        /// <returns>true if ok</returns>
        public static bool CombiBetMinStakeAllowed(decimal actStake)
        {
            if (Station == null)
                return false;

            return (Station.MinStakeCombiBet <= actStake);
        }

        /// <summary>
        /// Checks if stake is not higher than maxStakeCombi
        /// </summary>
        /// <param name="actStake">the stake to check</param>
        /// <returns>true if ok</returns>
        public static bool CombiBetMaxStakeAllowed(decimal actStake)
        {
            if (Station == null)
                return false;

            return (actStake <= Station.MaxStakeCombi);
        }

        /// <summary>
        /// checks if a CombiBet has enough tips (more than StationSr.MinCombination)
        /// </summary>
        /// <param name="numTips">number of tipps in selected bet</param>
        /// <returns>true if ok</returns>
        public static bool CombiBetMinSizeAllowed(int numTips)
        {
            if (Station == null)
                return false;

            return (Station.MinCombination <= numTips);
        }

        /// <summary>
        /// checks if number of Combi-Tipps is not higher than allowed (stationSr.MaxCombination)
        /// </summary>
        /// <param name="numTips">the number of tips</param>
        /// <returns>true if ok</returns>
        public static bool CombiBetMaxSizeAllowed(int numTips)
        {
            if (Station == null)
                return false;

            return (numTips <= Station.MaxCombination);
        }

        /// <summary>
        /// checks if combiSize is between Station.MinCombination and stationSr.MaxCombination
        /// </summary>
        /// <param name="numTips">number of tips in the selected bet</param>
        /// <returns>true if ok</returns>
        public static bool CombiBetSizeAllowed(int numTips)
        {
            if (Station == null)
                return false;

            return (Station.MinCombination <= numTips && numTips <= Station.MaxCombination);
        }

        /// <summary>
        /// checks if maxWin of a CombiBet is allowed
        /// </summary>
        /// <param name="odd">the total odd value of the bet</param>
        /// <param name="betStake">the stake of the bet</param>
        /// <param name="numTips">number of tips in combination</param>
        /// <returns>true if ok</returns>
        public static bool CombiBetWinAllowed(decimal odd, decimal betStake, int numTips)
        {
            Station = StationSr.LoadStationByStationId(Station.StationID);

            try
            {

                decimal netProfit = (odd - 1) * betStake;
                return netProfit <= CombiMaxStake(odd, numTips)*odd;
            }
            catch (Exception e)
            {
				//_logger.Error("Combi Limits not set in Table CombiLimit!", e);
				//if (!alreadyWarnedCombiLimits)
				//{
				//    NbtLogSr.WriteNbtLogEntry(LogMessages.LimitHandling1, NBTLogBO.PRIORITY_LOW, StationSettings.GetSettings.StationNumber, NBTLogBO.MSG_LIMITS);
				//    alreadyWarnedCombiLimits = true;
				//}
                return false;
            }
        }


        /// <summary>
        /// Checks if a whole combiBet-Ticket is allowed according to BetMaxOddAllowed, CombiBetStakeAllowed, CombiBetSizeAllowed and CombiBetWinAllowed
        /// </summary>
        /// <param name="rTicketWs">the ticket to check</param>
        /// <returns>returns true if ok</returns>
        public static bool CombiBetAllowed(TicketWS rTicketWs)
        {
            try
            {
                foreach (BetWS bet in rTicketWs.Bets)
                {

                    if (!BetMaxOddAllowed(bet.MaxOdd))
                        return false;

                    if (!CombiBetStakeAllowed(bet.Stake))
                        return false;

                    if (!CombiBetSizeAllowed(bet.BankTips.Count))
                        return false;

                    if (!CombiBetWinAllowed(bet.MaxOdd, bet.Stake, bet.Tips2BetMulti.Count + bet.BankTips.Count))
                        return false;
                }
                return true;
            }
            catch (Exception e)
            {
                //_logger.Error("Error checking Combi Ticket!", e);
                return false;
            }
        }

        /// <summary>
        /// calculates the maximum allowed stake for a combiBet
        /// </summary>
        /// <param name="maxOdd">the maxOdd if the combination</param>
        /// <param name="numTips">the number of tips in the combination</param>
        /// <returns>the maximum allowed stake</returns>
        public static decimal CombiMaxStake(decimal maxOdd, int numTips)
        {
            try
            {

                if (numTips < Station.MinCombination)
                    return Station.MaxStakeCombi;
				CombiLimit[] clTable = CombiSize;
                //CombiLimit[] clTable = (CombiLimit[])Station.combiLimit.ToArray();
				CombiLimit clMax = (CombiLimit)clTable[0];//Kleinsten Wert zuweisen
                decimal maxStake;
                // Combisize: ab. größter Combisize wert kleiner gleich numTips
                for (int i = 1; i < clTable.Length; i++)
                {
                    if (clTable[i].CombiSize <= numTips && (clTable[i].CombiSize > clMax.CombiSize || clMax.CombiSize > numTips))
                        clMax = clTable[i];
                    if (clTable[i].CombiSize == numTips)
                        break;
                }
                maxStake = clMax.Limit / (maxOdd - 1);

                return Math.Min(maxStake, Station.MaxStakeCombi);

            } catch (Exception e) {
				//_logger.Error("Info: Combi Limits not set in Table CombiLimit, using MaxStake from Station!", e);
				//NbtLogSr.WriteNbtLogEntry(LogMessages.LimitHandling2, NBTLogBO.PRIORITY_LOW, StationSettings.GetSettings.StationNumber, NBTLogBO.MSG_LIMITS);
				return Station.MaxStakeCombi;
			}
        }


 		/// <summary>
		/// calculates BonusFactor based upon the number of tips
		/// </summary>
		/// <param name="numBonusTips">tips with odds > 1.3 which are allowed to get a super bonus</param>
		/// <returns>BonusValue (1,03 for 3%)</returns>
		public static decimal CombiBetSuperBonus(int numBonusTips)
        {
			BonusRangeSr br = CombiBetBonusRange(numBonusTips);

 		    return br == null ? 1 : 1 + br.Bonus/100;
		}

        /// <summary>
        /// Loads BonusRangeBO for a specific combination of Tips
        /// </summary>
        /// <param name="pNumTips">the number of tips in a combination</param>
        /// <returns>the appropriate BonusRangeBO</returns>
        public static BonusRangeSr CombiBetBonusRange(int pNumTips)
        {
            try
            {
                Station = StationSr.LoadStationByStationId(Station.StationID);
				//if (pOddVal <= stationSr.BonusFromOdd)
				//    return null;


                BonusRangeSr[] brs = Station.BonusRangeList.ToArray();

                BonusRangeSr brMax = new BonusRangeSr();
                brMax.Bonus = 0;
                brMax.TipSize = Station.MinCombination;

                //Bonus ab (numTips>TipSize)
                for (int i = 0; i < brs.Length; i++)
                {
                    if (brs[i].TipSize <= pNumTips && brs[i].TipSize > brMax.TipSize)
                    {
                        brMax = brs[i];
                        if (brs[i].TipSize == pNumTips)
                            break;
                    }
                }
                return brMax;

            }
            catch (Exception e)
            {
                //_logger.Error("Super Bonus not set in Table BonusRange!", e);
                return null;
            }
        }

        /// <summary>
        /// Calculates the appropriate ManipulationFee which is charged for playing singleBets or combi-bets with two tips
        /// The odd can become smaller than 1 if there you play an odd 1.05 with a 10% manipulation fee. therefore the bet will be not allowed
        /// </summary>
        /// <param name="pNumTips">the number of tips selected</param>
        /// <returns>manipulationFee-value e.g. 0.95</returns>
        public static decimal ManipulationFeePercentage(int pNumTips)
        {
            try
            {
                Station = StationSr.LoadStationByStationId(Station.StationID);

                CombiLimitSr[] cls = Station.CombiLimitList.ToArray();

                CombiLimitSr clMax = new CombiLimitSr();
                clMax.CombiSize = 0;

                //Bonus ab (numTips>TipSize)
                for (int i = 0; i < cls.Length; i++)
                {
                    if (cls[i].CombiSize <= pNumTips && cls[i].CombiSize > clMax.CombiSize)
                    {
                        clMax = cls[i];
                        if (cls[i].CombiSize == pNumTips)
                            break;
                    }
                }

                return clMax.ManipulationFee;
            }
            catch (Exception e)
            {
                //_logger.Error("Super Bonus not set in Table BonusRange!", e);
                return 0;
            }
        }

        public static decimal ManipulationFeeLivebetPercentage(int pNumTips)
        {
            try
            {
                Station = StationSr.LoadStationByStationId(Station.StationID);
 
                CombiLimitSr[] cls = Station.CombiLimitList.ToArray();

                CombiLimitSr clMax = new CombiLimitSr();
                clMax.CombiSize = 0;

                //Bonus ab (numTips>TipSize)
                for (int i = 0; i < cls.Length; i++)
                {
                    if (cls[i].CombiSize <= pNumTips && cls[i].CombiSize > clMax.CombiSize)
                    {
                        clMax = cls[i];
                        if (cls[i].CombiSize == pNumTips)
                            break;
                    }
                }

                return clMax.ManipulationFee_LiveBet;
            }
            catch (Exception e)
            {
                //_logger.Error("Super Bonus not set in Table BonusRange!", e);
                return 0;
            }
        }


        /// <summary>
        /// Checks if the totalOdd of a systemBet is smaller than BetMaxOddAllowed and maxWinSystemBet is not larger than allowed
        /// </summary>
        /// <param name="maxOdd">the maxOdd of the system</param>
        /// <param name="multiWayOddFactor">the multiplication-factor of the multiWayTips</param>
        /// <param name="rowCount">Number of rows of the system</param>
        /// <returns></returns>
        public static bool SystemBetOddAllowed(decimal maxOdd, decimal multiWayOddFactor, decimal rowCount)
        {
            bool retVal = ((MinStakeSystemBet * maxOdd * multiWayOddFactor / rowCount) < MaxWinSystemBet);
            return retVal && BetMaxOddAllowed(maxOdd);
        }
    }
}
