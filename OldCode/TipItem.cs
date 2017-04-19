using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Nbt.Common.BusinessObjects;
using Nbt.Common.Database;
using Nbt.EntityObject.BetOffer;
using Nbt.Common.Utils.Date;
using Nbt.EntityObject.DatabaseUpdate;
using Nbt.EntityObject.Ticket;
using Nbt.Station.Common;
using Nbt.EntityObject.BetDomains;
using System.Collections.Generic;
using OldCode;
using SportBetting.WPF.Prism.Services;


namespace Nbt.Station.Design
{
	/// <summary>
	/// repräsentiert die Elemente des zusammenzusellenden Tickets (TicketView)
	/// </summary>
	public class TipItem : INotifyPropertyChanged
	{
		//this site height
		private int thisRowHeight;
		//fix row height
		//private bool isMultiWay = false;

		//property changed event must be implemented because is an interface member
		public event PropertyChangedEventHandler PropertyChanged;

		#region Properties Getters and Setters

		/// <summary>
		/// getter and setter properties
		/// </summary>
        public int ID { get; set; }
        public string Opponents { get; set; }
        public string OpponentsLongSingleLine { get; set; }
        public string FormattedTipText { get; set; }


		private static int BetConfirmedCount
		{
			get { return Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().BetConfirmedCount; }
			set { Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().BetConfirmedCount = value; }
		}
		private static string LastTicketNbr
		{
			get { return Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().LastTicketNbr; }
			set { Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().LastTicketNbr = value; }
		}
		private static string StationTyp
		{
			get { return Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().StationTyp; }
			set { Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().StationTyp = value; }
		}
		private static int NewTicketNumber
		{
			get { return Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().NewTicketNumber; }
			set { Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().NewTicketNumber = value; }
		}


        public string FormattedDate { get; set; }
        public string FormattedOdd { get; set; }
        public string FormattedBetDomain { get; set; }
        public string FormattedMincombination { get; set; }
        public MatchSr Match { get; set; }
        public OddSr Odd { get; set; }


        public bool IsBank { get; set; }
        public bool EnableBankBtn { get; set; }
        public string HeadlineComb { get; set; }
        public string HeadlineOdd { get; set; }
        public string HeadlineBank { get; set; }
        public string HeadlineDel { get; set; }
        public string Headline { get; set; }
        public int HeadlineSize { get; set; }
        public int Spacer { get; set; }
        public bool IsMultiWay { get; set; }

		#endregion

		public override string ToString()
		{
			return !string.IsNullOrEmpty(this.Opponents) && this.Odd != null ? string.Format("ID: {0};  SvrID: {1};  ORMID: {2};  Fctr: {3};  '{4}';  {5})", this.ID, this.Odd.SvrOddID, this.Odd.ORMID, this.Odd.Value, this.Opponents, this.Match) : base.ToString();
		}

		public TipItem(int id, string opponents, string tiptext, OddSr odd, MatchSr match, string headline, int headlineSize)
		{
			//set class variables
			ID = id;
			Opponents = opponents;  //==match.DisplayShortName
			FormattedTipText = tiptext;  //==odd.DisplayName ==matchbo.DisplayOddName ==betdomain+odd
			this.Odd = odd;
			this.Match = match;

			if (odd != null && odd.BetDomain != null && odd.BetDomain.MinCombination > 0)
			{
				FormattedMincombination = odd.BetDomain.MinCombination.ToString();
			}
			else
			{
				FormattedMincombination = String.Empty;
			}
			//für Ticket:
			if (match != null)
			{
				//this.opponentsLongSingleLine = match.DisplayName.Replace("\n   ", String.Empty);
				OpponentsLongSingleLine = opponents;
				FormattedDate = DateTimeUtils.DisplayDateTime(match.StartDate, new CultureInfo("En"));
			}
			if (OpponentsLongSingleLine == null)
				OpponentsLongSingleLine = String.Empty;
			if (FormattedDate == null)
				FormattedDate = String.Empty;
			if (odd != null && odd.BetDomain != null)
			{
				FormattedBetDomain = odd.BetDomain.DisplayName;
				FormattedOdd = odd.TextLangStr;
			}
			if (FormattedBetDomain == null)
				FormattedBetDomain = String.Empty;
			if (FormattedOdd == null)
				FormattedOdd = String.Empty;

			Headline = headline;
			HeadlineSize = headlineSize;
			Spacer = 0;

		}



		public static TicketWS CreateNewTicketWS(ObservableCollection<TipItem> tipItems, TicketStates ticketState, TipListInfo tipListInfo)
		{
			try
			{
				Nbt.Common.Utils.PasswordGenerator PWGen = new Nbt.Common.Utils.PasswordGenerator();
				TicketWS rTicketWs = new TicketWS();
				if (BetConfirmedCount > 0)
				{
					rTicketWs.TicketNbr = LastTicketNbr;
				}
				else
				{
					rTicketWs.TicketNbr = Catel.IoC.ServiceLocator.Instance.ResolveType<IStationProperties>().FullTicketNumber(NewTicketNumber);
				}
				//rTicketWs.TicketNbr = StationSettings.GetSettings.StationNumber+DateTime.Now.ToString("yy")+StationSettings.GetSettings.NewTicketNumber.ToString("0000000");
				//rTicketWs.TicketNbr = PWGen.Generate(14, 14, true);
				rTicketWs.CheckSum = PWGen.Generate(4, 4, true);


                rTicketWs.PaidBy = StationTyp;
                rTicketWs.PaidTime = DateTime.Now;
                rTicketWs.Paid = false;
                rTicketWs.AcceptedBy = StationTyp;
                rTicketWs.AcceptedTime = DateTime.Now;
                rTicketWs.Stake = tipListInfo.Bet;
                rTicketWs.Bets = new ObservableCollection<BetWS>();
                rTicketWs.CancelledTime = DateTimeUtils.DATETIMENULL;
                rTicketWs.EnablePayTime = DateTime.MaxValue;
                rTicketWs.WonExpireTime = DateTimeUtils.DATETIMENULL;
                rTicketWs.ManipulationFee = tipListInfo.ManipulationFeePercentage;
                BetWS bets = new BetWS();
				bets.SystemX = bets.SystemY = tipListInfo.NumOfTipps;

				switch (ticketState)
				{
					case TicketStates.Combi:
						bets.BetType = BetBO.BET_TYPE_COMBI;
						break;
					case TicketStates.System:
						bets.BetType = BetBO.BET_TYPE_SYSTEM;
						bets.SystemX = tipListInfo.NumSystemX;
						bets.SystemY = tipListInfo.NumSystemY;
						break;
					default:
						bets.BetType = BetBO.BET_TYPE_SINGLE;
						break;
				}
				bets.MaxOdd = tipListInfo.FullOddFactor;
				//bets.MaxWin = rTicketWs.Stake * bets.MaxOdd;
				bets.MaxWin = tipListInfo.PossWin;
				bets.Stake = rTicketWs.Stake;
				bets.IsMaxOddBet = tipListInfo.IsMaxOddBet;

				bets.BankTips = new ObservableCollection<TipWS>();
				bets.Tips2BetMulti = new ObservableCollection<TipWS>();
				rTicketWs.Bets.Add(bets);
				rTicketWs.SuperBonus = tipListInfo.BonusFactor;

				Dictionary<int, int> matchIDCountDictionary = new Dictionary<int, int>();

				foreach (TipItem t in tipItems)
				{
					int iMatchCode = t.Match.Code;

					if (matchIDCountDictionary.ContainsKey(iMatchCode))
					{
						matchIDCountDictionary[iMatchCode]++;
					}
					else
					{
						matchIDCountDictionary.Add(iMatchCode, 1);
					}
				}

				int liveBetTipCount = 0;
				int sportBetTipCount = 0;
				foreach (TipItem t in tipItems)
				{
					if (t.Odd != null)
					{
						if (t.Odd.IsLiveBet)
						{
							liveBetTipCount++;
						}
						else
						{
							sportBetTipCount++;
						}
					}
					TipWS tip = new TipWS();
					tip.Bank = t.IsBank;

					if (t.Odd != null)
					{
						tip.Odd = t.Odd.Value;
						tip.SvrOddID = (int)t.Odd.SvrOddID; //TODO: Wurde auf ServerOddID umgewandelt *g* 05.03.2008 by GMU
						tip.BetDomainNumber = t.Odd.BetDomain.BetDomainNumber;
						tip.MatchCode = t.Odd.BetDomain.Match.Code;
					}

					if (tip.Bank || matchIDCountDictionary[tip.MatchCode] != 1)
					{
						tip.BankGroupID = (int)t.Match.SvrMatchID;
						tip.Bank = matchIDCountDictionary[tip.MatchCode] == 1;//Mehrwege haben kein Bank-Flag
						if (ticketState == TicketStates.Combi)
						{
							bets.BetType = BetBO.BET_TYPE_COMBIPATH;
						}
						else if (ticketState == TicketStates.System)
						{
							bets.BetType = BetBO.BET_TYPE_SYSTEMPATH;
						}
						bets.Tips2BetMulti.Add(tip);	//banken von system mit banken
					}
					else
					{
						bets.BankTips.Add(tip);			//system, kombi, einzeln
					}
				}

				if (liveBetTipCount > 0 && sportBetTipCount == 0)
				{
					rTicketWs.TicketTyp = TicketBO.TICKET_TYP_LIVEBET;
				}
				else if (liveBetTipCount == 0 && sportBetTipCount > 0)
				{
					rTicketWs.TicketTyp = TicketBO.TICKET_TYP_SPORTBET;
				}
				else
				{
					rTicketWs.TicketTyp = TicketBO.TICKET_TYP_BOTH;
				}
				int rowCount = 1;
				int[,] temp;
				if (ticketState == TicketStates.System)
				{
					Nbt.Common.Odd.OddUtilities.SetPermutations(out temp, bets.SystemY, bets.SystemX);
					rowCount = temp.GetLength(0);
				}
				Dictionary<int, int> tempMatchCodeCountDict = new Dictionary<int, int>();
				foreach (TipWS t in bets.Tips2BetMulti)
				{
					if (tempMatchCodeCountDict.ContainsKey(t.MatchCode))
					{
						tempMatchCodeCountDict[t.MatchCode]++;
					}
					else
					{
						tempMatchCodeCountDict.Add(t.MatchCode, 1);
					}
				}
				foreach (int curCount in tempMatchCodeCountDict.Values)
				{
					rowCount *= curCount;
				}
				bets.Rows = rowCount;
				return rTicketWs;
			}
			catch (Exception ex)
			{
				//WCFService.LogRemoteError(ex.Message, 1, ex.GetType().ToString(), EntityObject.Common.NBTLogBO.MSG_TERMINAL);
				return null;
			}

		}





		#region INotifyPropertyChanged Member
		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}
		#endregion

	}
}

