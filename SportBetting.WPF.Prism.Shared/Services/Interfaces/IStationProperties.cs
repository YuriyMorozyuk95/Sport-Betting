using System.Collections.ObjectModel;
using System.ComponentModel;
using Nbt.Common.BusinessObjects;
using SportBetting.WPF.Prism.Models.Interfaces;

namespace SportBetting.WPF.Prism.Services
{
    using Models;

    /// <summary>
    /// Keeps track of last changes.
    /// </summary>
	public interface IStationProperties : INotifyPropertyChanged
    {
		string Currency { get; set; }
		decimal MaxBet { get; }
		decimal MinBet { get; }
    	decimal BonusFromOdd { get; set; }
    	decimal MaxOdd { get; set; }
    	decimal MinStakeCombiBet { get; set; }
    	decimal MinStakeSystemBet { get; set; }
    	decimal MinStakeSingleBet { get; set; }
    	decimal MaxWinSystemBet { get; set; }
    	int BetConfirmedCount { get; set; }
		bool LockOfferOnLimit { get; set; }
    	string LastTicketNbr { get; set; }
    	string StationTyp { get; set; }
    	int NewTicketNumber { get; }
		string StationNumber { get; set; }
    	string FullTicketNumber(int variablePartOfTicketNumber);
    }
}