using System;
using IocContainer;
using NLogger;
using Ninject;
using Shared;
using Shared.Interfaces;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using WsdlRepository.WsdlServiceReference;

namespace SportBetting.WPF.Prism.OldCode {
    /// <summary>
    /// checks limits on Odds and Tournaments, locks offer if a limit is hit and StationSr.LockOfferOnLimit is true and sends warning to server
    /// </summary>
    public class LimitHandling
    {
        //private static log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(LimitHandling));

        // Repositories

        private static IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }
        public static IChangeTracker ChangeTracker
        {
            get { return IoCContainer.Kernel.Get<IChangeTracker>(); }
        }

		private static decimal MaxWinSystemBet
		{
            get { return StationRepository.GetMaxWinSystemBet(ChangeTracker.NewTicket); }
		}

		private static decimal MinStakeSystemBet
		{
            get { return StationRepository.GetMinStakeSystemBet(ChangeTracker.NewTicket); }
		}




        /// <summary>
        /// common limit for all bets (single, combi, system)
        /// </summary>
        /// <param name="maxOdd">maximum odd of the bet</param>
        /// <returns></returns>
        public static bool BetMaxOddAllowed(decimal maxOdd)
        {
            return maxOdd <= StationRepository.GetMaxOdd((ChangeTracker.NewTicket));
        }

        /// <summary>
        /// checks if the possible win of a singleBet is not highter than allowed
        /// </summary>
        /// <param name="possWin">the possible win of the single bet</param>
        /// <returns>true if ok</returns>
        public static bool SingleBetWinAllowed(decimal possWin, Ticket ticketToCalculate)
        {
            return (possWin <= StationRepository.GetMaxWinSingleBet(ticketToCalculate));
        }

        /// <summary>
        /// calculates the maximum stake for a specific singleBet is according to its Odd
        /// </summary>
        /// <param name="maxOdd">the odd of the singleBet</param>
        /// <returns>the value of the maximum stake allowed</returns>
        public static decimal SingleMaxStake(decimal maxOdd, Ticket ticketToCalculate)
        {
            return Math.Min(StationRepository.GetMaxWinSingleBet((ticketToCalculate)) / maxOdd, StationRepository.GetMaxStakeSingleBet(ticketToCalculate));

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
            return (StationRepository.GetMinStakeSystemBet(ChangeTracker.NewTicket) <= actStake);
        }

        /// <summary>
        /// Checks if curStake is not higher than maxStake
        /// </summary>
        /// <param name="actStake">the stake to check</param>
        /// <returns>true if ok</returns>
        public static bool SystemBetMaxStakeAllowed(decimal actStake)
        {
            return (actStake <= StationRepository.GetMaxStakeSystemBet(ChangeTracker.NewTicket));
        }

        /// <summary>
        /// Checks if curStake is between minStake and maxStake
        /// </summary>
        /// <param name="actStake">the stake to check</param>
        /// <returns>true if ok</returns>
        public static bool SystemBetStakeAllowed(decimal actStake)
        {
            return (StationRepository.GetMinStakeSystemBet(ChangeTracker.NewTicket) <= actStake && actStake <= StationRepository.GetMaxStakeSystemBet(ChangeTracker.NewTicket));
        }

        /// <summary>
        /// checks if Y of system bet x/Y is allowed because of StationSr.MaxSystemBet
        /// </summary>
        /// <param name="numSystemY">the Y value of System X/Y</param>
        /// <returns>true if ok</returns>
        public static bool SystemBetYAllowed(int numSystemY)
        {
            return (numSystemY <= StationRepository.GetMaxSystemBet(ChangeTracker.NewTicket));
        }

        /// <summary>
        /// checks if possible win of bet is not larger than StationSr.MaxWinSystemBet
        /// </summary>
        /// <param name="possWin">the possible win to check</param>
        /// <returns>returns true if ok</returns>
        public static bool SystemBetWinAllowed(decimal possWin)
        {
            return (possWin <= StationRepository.GetMaxWinSystemBet(ChangeTracker.NewTicket));
        }

        /// <summary>
        /// calculates max Stake for a systemBet with maxOdd (highest path multiplicated by MultiWayOddFacotor)
        /// </summary>
        /// <param name="maxOdd">maxOdd to check</param>
        /// <returns>the maximum allowed stake</returns>
        public static decimal SystemMaxStake(decimal maxOdd)
        {
            return Math.Round(Math.Min(StationRepository.GetMaxWinSystemBet(ChangeTracker.NewTicket) / maxOdd, StationRepository.GetMaxStakeSystemBet(ChangeTracker.NewTicket)), 1);
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
                foreach (BetWS bet in rTicketWs.bets)
                {
                    if (!(BetMaxOddAllowed(bet.maxOdd) && SystemBetStakeAllowed(bet.stake) && SystemBetYAllowed(bet.systemY) &&
                        /*SystemBetWinAllowed((float)bet.Stake*bet.MaxOdd) &&*/  SystemBetWinAllowed(bet.maxWin)))
                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error("Error checking Combi Ticket!", e);
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
            return (StationRepository.GetMinStakeCombiBet(ChangeTracker.NewTicket) <= actStake && actStake <= StationRepository.GetMaxStakeCombi(ChangeTracker.NewTicket));
        }

        /// <summary>
        /// Checks if MinStakeCombiBet is reached
        /// </summary>
        /// <param name="actStake">the stake to check</param>
        /// <returns>true if ok</returns>
        public static bool CombiBetMinStakeAllowed(decimal actStake)
        {
            return (StationRepository.GetMinStakeCombiBet(ChangeTracker.NewTicket) <= actStake);
        }

        /// <summary>
        /// Checks if stake is not higher than maxStakeCombi
        /// </summary>
        /// <param name="actStake">the stake to check</param>
        /// <returns>true if ok</returns>
        public static bool CombiBetMaxStakeAllowed(decimal actStake)
        {
            return (actStake <= StationRepository.GetMaxStakeCombi(ChangeTracker.NewTicket));
        }

        /// <summary>
        /// checks if a CombiBet has enough tips (more than StationSr.MinCombination)
        /// </summary>
        /// <param name="numTips">number of tipps in selected bet</param>
        /// <returns>true if ok</returns>
        public static bool CombiBetMinSizeAllowed(int numTips)
        {
            return (StationRepository.GetMinCombination(ChangeTracker.NewTicket) <= numTips);
        }

        /// <summary>
        /// checks if number of Combi-Tipps is not higher than allowed (stationSr.MaxCombination)
        /// </summary>
        /// <param name="numTips">the number of tips</param>
        /// <returns>true if ok</returns>
        public static bool CombiBetMaxSizeAllowed(int numTips)
        {
            return (numTips <= StationRepository.GetMaxCombination(ChangeTracker.NewTicket));
        }

        /// <summary>
        /// checks if combiSize is between Station.MinCombination and stationSr.MaxCombination
        /// </summary>
        /// <param name="numTips">number of tips in the selected bet</param>
        /// <returns>true if ok</returns>
        public static bool CombiBetSizeAllowed(int numTips)
        {
            return (StationRepository.GetMinCombination(ChangeTracker.NewTicket) <= numTips && numTips <= StationRepository.GetMaxCombination(ChangeTracker.NewTicket));
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
