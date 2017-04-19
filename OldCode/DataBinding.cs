using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nbt.Common.Database;
using Nbt.Station.Common;
using OldCode;
using SportBetting.WPF.Prism.Services;

namespace Nbt.Station.Design
{

	public enum TicketError { NoError = 0, MaxOddLimitError = 2, SystemItemsRangeError = 5, OtherLimitError = 3, MinCombinationLimit = 4, MinMaxLimitError = 6, CombiItemsRangeError = 8 };
	public enum TicketStates { Combi = 1, System = 2, Multiway = 3, Single = 4 };
	public static class DataBinding
	{
		public static TicketError TicketError = TicketError.NoError;

		private static ObservableCollection<TipItem> _tipItems = new ObservableCollection<TipItem>();

		private static TipListInfo _tipListInfo = new TipListInfo();

		private static Dictionary<int, int> _lockedTournaments = new Dictionary<int, int>();

		private static decimal BonusFromOdd
		{
			get { return Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().BonusFromOdd; }
			set { Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().BonusFromOdd = value; }
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
		/// tip items (=selected odds)
		/// </summary>
		public static ObservableCollection<TipItem> TipItems
		{
			get { return _tipItems; }
			set { _tipItems = value; }
		}

		/// <summary>
		/// Infos aubout TipItems
		/// </summary>
		public static TipListInfo TipListInfo
		{
			get { return _tipListInfo; }
			set { _tipListInfo = value; }
		}

		

		public static Dictionary<int, int> LockedTournaments
		{
			get { return DataBinding._lockedTournaments; }
			set { DataBinding._lockedTournaments = value; }
		}

		

        public static IdentityList GetMatchIdentityListFromTipItems(ObservableCollection<TipItem> tipItems)
        {
            IdentityList ilMatches = new IdentityList();

            foreach (TipItem t in tipItems)
            {
                ilMatches.AddUnique(t.Match.MatchID);
            }

            return ilMatches;
        }

        public static SportRadarMatchCollection GetTipItemSportRadarMatchCollection(ObservableCollection<TipItem> tipItems)
        {
            try
            {
                SportRadarMatchCollection srmc = new SportRadarMatchCollection(eBetFilter.All, "en");
                srmc.SelectMatches(GetMatchIdentityListFromTipItems(tipItems));

                return srmc;
            }
            catch (Exception excp)
            {
                //_logger.ErrorFormat("GetTipItemSportRadarMatchCollection(Count = {0}) ERROR:\r\n{1}\r\n{2}", tipItems != null ? tipItems.Count.ToString() : "NULL", excp.Message, excp.StackTrace);
            }

            return null;
        }

		public static TipItem AddTipData(string opponents, string tiptext, OddSr odd, MatchSr match)
		{
			return AddTipData(opponents, tiptext, odd, match, false);
		}

		public static TipItem AddTipData(string opponents, string tiptext, OddSr odd, MatchSr match, bool isBank)
		{
			// To be ensure that duplicated Ticket Tip will not be added
			if (DataBinding.FindTipDataBySvrOddId((int)odd.SvrOddID) != null)
			{
				return null;
			}

			//new instance of tip item
			TipItem tip = new TipItem(_tipItems.Count, opponents, tiptext, odd, match, String.Empty, 0);
			tip.IsBank = addBank(odd);
			int index = 0;

			_tipItems.Insert(index, tip);
			//AfterTipItemsChanged();//Removed by GMU 11.07.2008 because of possibly being redundant
			//handle the animation for showing/hiding of menu and tiplist
			return tip;

		}

		public static bool addBank(OddSr curOdd)
		{
			try
			{
				int count = 0;
				foreach (TipItem curItem in TipItems)
				{
					if (curItem.Odd.BetDomain.ORMID == curOdd.BetDomain.ORMID)
					{
						count++;
						curItem.IsBank = true;
						curItem.IsMultiWay = true;
					}
				}
				return count > 0;
			}
			catch (Exception ex)
			{
				//_logger.Debug("Error adding Bank: " + ex);
				return false;
			}
		}
		public static bool removeBank(OddSr curOdd)
		{
			try
			{
				int count = 0;
				TipItem modifyItem = null;
				foreach (TipItem curItem in TipItems)
				{
					if (curItem.Odd.BetDomain.ORMID == curOdd.BetDomain.ORMID)
					{
						count++;
						modifyItem = curItem;
					}
				}
				if (count < 2 && modifyItem != null)
				{
					modifyItem.IsBank = false;
					modifyItem.IsMultiWay = false;
				}
				return count < 2;
			}
			catch (Exception ex)
			{
				//_logger.Error("in removeBank: " + ex);
				return false;
			}
		}

		/// <summary>
		/// removes a tip out of the _ticket list
		/// </summary>
		/// <param name="id">id of item</param>
		public static void RemoveTipData(TipItem t)
		{
			//remove tip
			if (_tipItems.Remove(t))
			{
				removeBank(t.Odd);
			}
		}

		/// <summary>
		/// find a tip item
		/// </summary>
		/// <param name="id">id of the item to find</param>
		public static TipItem FindTipData(int id)
		{
			foreach (TipItem t in _tipItems)
			{
				if (t.ID == id)
				{
					return t;
				}
			}
			return null;
		}

		/// <summary>
		/// find a tip item with odd server id
		/// </summary>
		/// <param name="iSvrOddId">id of the odd </param>
		public static TipItem FindTipDataBySvrOddId(int iSvrOddId)
		{
			foreach (TipItem t in _tipItems)
			{
				if (t.Odd != null && t.Odd.SvrOddID == iSvrOddId)
				{
					return t;
				}
			}

			return null;
		}
		public static TipItem FindTipDataByORMId(int iORMId)
		{
			foreach (TipItem t in _tipItems)
			{
				if (t.Odd != null && t.Odd.ORMID == iORMId)
				{
					return t;
				}
			}

			return null;
		}

		/// <summary>
		/// find a tip item with Match ORMID 
		/// </summary>
		/// <param name="iMatchOrmId">id of the odd </param>
		public static TipItem FindTipDataByMatchId(int iMatchOrmId)
		{
			foreach (TipItem t in _tipItems)
			{
				if (t.Match != null && t.Match.ORMID == iMatchOrmId)
				{
					return t;
				}
			}

			return null;
		}

		/// <summary>
		/// Eintrag aus Ticket löschen
		/// </summary>
		/// <param name="p"></param>



		public static void UpdateSystemOrCombiticket(bool reset, TicketStates ticketState, ref TipListInfo tipListInfo, ObservableCollection<TipItem> tipItems)
		{

			decimal oddVal = 1;
			decimal multiWayOddVal = 1;
			int minCombMax = 0;
			int bonusTipsCount = 0;

			Dictionary<int, List<TipItem>> tipItemDict = new Dictionary<int, List<TipItem>>();
			foreach (TipItem t in tipItems)
			{
				int iSvrMatchId = (int)t.Match.SvrMatchID;

				if (tipItemDict.ContainsKey(iSvrMatchId))
				{
					tipItemDict[iSvrMatchId].Add(t);
				}
				else
				{
					List<TipItem> list = new List<TipItem>();
					list.Add(t);
					tipItemDict.Add(iSvrMatchId, list);
				}
			}
			// de: Anzahl der Wege berechnen                // en: Number of ways to calculate
			// de: höchste Quote der Mehrwege finden        // en: highest rate of multipath are
			//
			int pathCount = 0;
			int rowCount = 1;
			foreach (List<TipItem> list in tipItemDict.Values)
			{
				if (list.Count >= 1)
				{
					decimal maxOdd = 1;
					foreach (TipItem tip in list)
					{
						if (maxOdd < tip.Odd.Value)
						{
							maxOdd = tip.Odd.Value;
						}
					}
					TipItem t = list[0];
					if (t.IsBank || list.Count > 1)
					{
						pathCount++;
						rowCount *= list.Count;
						multiWayOddVal *= maxOdd;
					}
					else
					{
						oddVal *= maxOdd;
					}
					int curCombMax = minCombinationOfAll(t);
					if (curCombMax > minCombMax)
					{
						minCombMax = curCombMax;
					}
					if (list.Count == 1 && ticketState == TicketStates.Combi)
					{
						if (t.Odd.Value >= BonusFromOdd)
							//Bonus gibt es nicht bei Mehrwegen
							bonusTipsCount++;
					}
				}
			}

			tipListInfo.MinCombination = minCombMax;
			tipListInfo.NumOfTipps = tipItemDict.Count;
			tipListInfo.PathCount = pathCount;
			tipListInfo.RowCount = rowCount;
			tipListInfo.MultiWayOddFactor = multiWayOddVal;

			if (tipItems.Count > 0 && tipItems[0] != null && tipItems[0].Match != null)
			{
				if (!tipItems[0].Match.IsLiveBet)
				{
					//Use manipulationFee only on Sport Bet, not on Live Bet according to Harri 2010-02-22 GMU
					tipListInfo.ManipulationFeePercentage =
						LimitHandling.ManipulationFeePercentage(tipListInfo.NumOfTipps);
					tipListInfo.BonusFactor = LimitHandling.CombiBetSuperBonus(bonusTipsCount);
				}
				else
				{

					// ale 14.07.2011: use manipulationfee also on livebet
					tipListInfo.ManipulationFeePercentage =
						LimitHandling.ManipulationFeeLivebetPercentage(tipListInfo.NumOfTipps);
					tipListInfo.BonusFactor = 1;

				}
			}
			else
			{
				tipListInfo.ManipulationFeePercentage = 0;
				tipListInfo.BonusFactor = 1;
			}
			switch (ticketState)
			{
				case TicketStates.Single:
					UpdateSingleticketItems(ref tipListInfo, tipItems);
					break;
				case TicketStates.Combi:
					tipListInfo.FullOddFactor = oddVal * multiWayOddVal * tipListInfo.BonusFactor;
					if (tipListInfo.FullOddFactor >= MaxOdd)
					{
						tipListInfo.IsMaxOddBet = true;
						if (tipListInfo.FullOddFactor > MaxOdd)
						{
							tipListInfo.IllegalOddFactor = tipListInfo.FullOddFactor;
						}
						tipListInfo.FullOddFactor = MaxOdd;
						tipListInfo.OddOfTipps = MaxOdd;
					}
					else
					{
						tipListInfo.OddOfTipps = oddVal * multiWayOddVal;
						tipListInfo.IllegalOddFactor = 0;
						tipListInfo.IsMaxOddBet = false;
					}
					tipListInfo.MinBet = MinStakeCombiBet;
					tipListInfo.MaxBet = Math.Round(LimitHandling.CombiMaxStake(tipListInfo.FullOddFactor * (100 - tipListInfo.ManipulationFeePercentage) / 100, tipItems.Count) * rowCount, 1);

					tipListInfo.MinWin = tipListInfo.FullOddFactor * tipListInfo.MinBet / rowCount * (100 - tipListInfo.ManipulationFeePercentage) / 100;
					tipListInfo.MaxWin = tipListInfo.FullOddFactor * tipListInfo.MaxBet / rowCount * (100 - tipListInfo.ManipulationFeePercentage) / 100;

					tipListInfo.PossWin = (tipListInfo.FullOddFactor * tipListInfo.Bet / rowCount * (100 - tipListInfo.ManipulationFeePercentage) / 100);

					tipListInfo.NumOfBets = 1;
					if ((tipListInfo.BonusFactor < 0.999m || tipListInfo.BonusFactor > 1.001m)
						)
					{
						tipListInfo.BonusDesc = "!TERMINAL_BONUS!";
					}

					break;
				case TicketStates.System:
					tipListInfo.BonusFactor = 1;
					tipListInfo.FullOddFactor = 0; //TODO: Calc system
					tipListInfo.IllegalOddFactor = 0;

					int ind = 0;
					TicketError = TicketError.NoError;
					if (!LimitHandling.SystemBetYAllowed(tipListInfo.NumSystemY))
					{  //avoid useless time and memory consuming calculations
						tipListInfo.ResetNumXY();
						tipListInfo.NumOfBets = 0;
						tipListInfo.MinBet = 0;
						tipListInfo.OddOfTipps = 0;
						tipListInfo.MaxBet = 0;
						tipListInfo.MinWin = 0;
						tipListInfo.MaxWin = 0;
						tipListInfo.EnableSystemAddBtn = false;
						tipListInfo.EnableSystemSubBtn = false;
						return;
					}


					if (reset)
						tipListInfo.ResetNumXY();

					tipListInfo.EnableSystemAddBtn = tipListInfo.NumSystemX < tipListInfo.NumSystemY - 1;
					tipListInfo.EnableSystemSubBtn = minCombMax < tipListInfo.NumSystemX + pathCount && tipListInfo.NumSystemX > TipListInfo.MinSystemX;


					decimal[] oddVals = new decimal[tipListInfo.NumSystemY];
					bool disableBankBtn = tipListInfo.PathCount + TipListInfo.MinSystemY >= tipListInfo.NumOfTipps;

					foreach (TipItem t in tipItems)
					{
						if (!t.IsBank && t.Odd != null)
						{
							if (ind < oddVals.Length)
								oddVals[ind++] = t.Odd.Value;
							else
								//_logger.Error("System Y value smaller as expected");


							t.EnableBankBtn = !disableBankBtn;

						}

					}
					tipListInfo.NumOfBets = (decimal)MathNet.Numerics.Fn.BinomialCoefficient(tipListInfo.NumSystemY, tipListInfo.NumSystemX) * tipListInfo.RowCount;
					decimal maxOdd = Nbt.Common.Odd.OddUtilities.AllCombinationsSum(oddVals, tipListInfo.NumSystemX);
					if (oddVals.Length < tipListInfo.NumSystemX)
					{//GMU 2011-02-01 fixed wrong || statement
						string msg = ("in UpdateSystemOfCombiTicket reset=" + reset + " OddVals.Length: " + oddVals.Length + " smaller than " + tipListInfo.NumSystemX);
						//_logger.Debug(msg);
                        //NbtLogSr.WriteNbtLogEntry(msg, NBTLogBO.PRIORITY_MEDIUM, StationSettings.GetSettings.StationNumber, NBTLogBO.MSG_TERMINAL);
						return;
					}
					decimal minOdd = Nbt.Common.Odd.OddUtilities.MinCombinationsSum(oddVals, tipListInfo.NumSystemX);
					if (!LimitHandling.SystemBetOddAllowed(maxOdd, tipListInfo.MultiWayOddFactor, tipListInfo.RowCount))
					{
						throw new Exception(TicketError.SystemItemsRangeError.ToString());
					}
					else
					{
						//ViewControl.SetTicketErrorMessage(TicketError.NoError);
						tipListInfo.MinBet = MinStakeSystemBet;
						//tipListInfo.MaxBet = StationSettings.Station.MaxStakeSystemBet;
						tipListInfo.OddOfTipps = maxOdd;
						tipListInfo.MaxBet = LimitHandling.SystemMaxStake(maxOdd * tipListInfo.MultiWayOddFactor) * tipListInfo.RowCount;

						tipListInfo.MinWin = tipListInfo.MinBet * maxOdd * tipListInfo.MultiWayOddFactor / tipListInfo.RowCount;
						tipListInfo.MaxWin = tipListInfo.MaxBet * maxOdd * tipListInfo.MultiWayOddFactor / tipListInfo.RowCount;
						tipListInfo.PossWin = tipListInfo.Bet * maxOdd * tipListInfo.MultiWayOddFactor / tipListInfo.RowCount;
					}
					tipListInfo.FullOddFactor = maxOdd;
					tipListInfo.OddOfTipps = maxOdd;

					break;
			}

			if (tipListInfo.ManipulationFeePercentage > 0)
			{
				tipListInfo.ManipulationFeeDesc = BetDomainSr.ManipulationFeeString + " " + String.Format("{0:F2}%", tipListInfo.ManipulationFeePercentage);
			}
			//ViewControl.UpdateBetNowButton();TODO: Removed by GMU 2008-09-04 because of beeing redundant
		}

		public static void UpdateSingleticketItems()
		{
			UpdateSingleticketItems(ref _tipListInfo, DataBinding._tipItems);
		}

		public static void UpdateSingleticketItems(ref TipListInfo tipListInfo, ObservableCollection<TipItem> tipItems)
		{
			decimal oddVal = 1;
			///ASSERTION  :  Tipitem has exactly one item
			if (tipItems.Count == 1)
			{
				oddVal = tipItems[0].Odd.Value;
				tipListInfo.MinCombination = minCombinationOfAll(tipItems[0]);
			}
			tipListInfo.NumOfTipps = 1;
			tipListInfo.NumOfBets = 1;
			tipListInfo.OddOfTipps = oddVal;
			tipListInfo.FullOddFactor = oddVal;
			tipListInfo.BonusFactor = 1;

			tipListInfo.MinBet = MinStakeSingleBet;
			tipListInfo.MaxBet = Math.Round(LimitHandling.SingleMaxStake(oddVal * tipListInfo.ManipulationFeeReduction), 1);

			tipListInfo.MinWin = tipListInfo.OddOfTipps * tipListInfo.MinBet * ((tipListInfo.BonusFactor > 0) ? tipListInfo.BonusFactor : 1) * tipListInfo.ManipulationFeeReduction;
			tipListInfo.MaxWin = tipListInfo.OddOfTipps * tipListInfo.MaxBet * ((tipListInfo.BonusFactor > 0) ? tipListInfo.BonusFactor : 1) * tipListInfo.ManipulationFeeReduction;

			tipListInfo.PossWin = tipListInfo.OddOfTipps * tipListInfo.Bet * ((tipListInfo.BonusFactor > 0) ? tipListInfo.BonusFactor : 1) * tipListInfo.ManipulationFeeReduction;
		}

		private static int minCombinationOfAll(TipItem item)
		{
			int minComb = 0;
			if (item != null && item.Odd != null && item.Odd.BetDomain != null)
			{
				minComb = item.Odd.BetDomain.MinCombination;
				MatchSr curMatch = item.Odd.BetDomain.Match;
				if (curMatch != null && curMatch.MinCombination > minComb)
				{
					minComb = curMatch.MinCombination;
				}
			}
			return minComb;
		}



		/// <summary>
		/// TicketItem finden
		/// </summary>
		/// <param name="p"></param>
		public static bool TicketItemSetBank(int ticketId, bool bank)
		{
			if (bank && _tipListInfo.PathCount + TipListInfo.MinSystemY >= _tipListInfo.NumOfTipps)
				return false;
			foreach (TipItem t in _tipItems)
				if (t.ID == ticketId)
				{
					if (t.IsMultiWay)
					{
						return false;
					}
					if (!t.IsBank && bank)
					{
						_tipListInfo.PathCount++;

					}
					else if (t.IsBank && !bank)
					{
						_tipListInfo.PathCount--;

					}
					t.IsBank = bank;
					_tipListInfo.ResetNumXY();
					break;
				}
			//ViewControl.UpdateTipTicketSystemGridInfo();
			return true;
		}

		public static void UpdateSystemOrCombiticket(bool reset,TicketStates ticketState)
		{
			UpdateSystemOrCombiticket(reset, ticketState, ref _tipListInfo, _tipItems);
		}
		/// <summary>
		/// Change SystemX Value
		/// </summary>
		public static void ChangeSystemX(int val,TicketStates state)
		{
			if (val > 0 || _tipListInfo.MinCombination < _tipListInfo.NumSystemX + _tipListInfo.PathCount)
			{
				_tipListInfo.NumSystemX += val;
				UpdateSystemOrCombiticket(false,state);
			}


		}

	}
}