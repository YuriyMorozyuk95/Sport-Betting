using System;
using System.Collections.Generic;
using System.ComponentModel;
using Nbt.Station.Design;

namespace OldCode {
    /// <summary>
    /// diese Klasse ergänzt die in DataBinding definierte ObservableCollection von TipItems.
	/// ZB Informationen die in der TipListe angezeigt werden, wie das Produkt der Quoten der TipItems der Tipliste.
    /// </summary>
	public class TipListInfo : INotifyPropertyChanged {
       
		private decimal oddOfTipps, fullOddFactor, numOfBets, illegalOddFactor;
		private decimal bonusFactor, multiWayOddFactor;
        private int numOfTipps, numSystemX/*, numSystemY*/, pathCount, minCombination, rowCount;
        private bool warningsAlreadyConfirmed = false;

        public bool WarningsAlreadyConfirmed
        {
            get { return warningsAlreadyConfirmed; }
            set { warningsAlreadyConfirmed = value; }
        }

        public int RowCount
        {
            get { return rowCount; }
            set { rowCount = value; }
        }

		public decimal IllegalOddFactor
		{
			get { return illegalOddFactor; }
			set { illegalOddFactor = value;}
		}
        private decimal manipulationFeePercentage;

        public decimal ManipulationFeePercentage
        {
            get { return manipulationFeePercentage; }
            set { manipulationFeePercentage = value; }
        }

        public decimal ManipulationFeeReduction
        {
            get { return (100 - manipulationFeePercentage)/100;
            }
        }

        public decimal ManipulationFeeValue
        {
            get { return (manipulationFeePercentage * actBet / 100); }
        }

        public decimal ReducedStakeValue
        {
            get { 
                return (1-manipulationFeePercentage/100) * actBet; 
            }
        }


        public decimal BonusValue
        {
            get { return (BonusFactor-1) * actBet * fullOddFactor; }
        }



        
        private string numOfTippsDesc;
		private string oddOfTippsDesc;
		
		private decimal minBet, maxBet, minWin, maxWin;
		private string numOfBetsDesc, bankDesc, minBetDesc, maxBetDesc, minWinDesc, maxWinDesc, systemDesc /*, bonusValStr*/;
        private string systemXY, combiDesc, singleDesc, multiwayDesc, bonusDesc, betDesc, possWinDesc, manipulationFeeDesc, insertionDesc;
		private decimal actBet, possWin;
		private bool enableSystemAddBtn, enableSystemSubBtn;
		private bool isMaxOddBet = false;

		public static int MinSystemY = 3;
		public static int MinSystemX = MinSystemY - 1;
		//public static DependencyProperty BetProperty = DependencyProperty.Register("Bet", typeof(double), typeof(TipListInfo));



		// getter and setter properties
		#region Properties Getters and Setters

		public bool EnableSystemSubBtn {
			get { return enableSystemSubBtn; }
			set { enableSystemSubBtn = value; OnPropertyChanged("EnableSystemSubBtn"); }
		}
		public bool EnableSystemAddBtn {
			get { return enableSystemAddBtn; }
			set { enableSystemAddBtn = value; OnPropertyChanged("EnableSystemAddBtn"); }
		}
		public string BetDesc {
			get { return betDesc; }
			set { betDesc = value; OnPropertyChanged("BetDesc"); }
		}
		public string PossWinDesc {
			get { return possWinDesc; }
			set { possWinDesc = value; OnPropertyChanged("PossWinDesc"); }
		}

        public decimal DecimalBet
        {
            get { return Bet; }
        }
		public decimal Bet {
			get { return actBet; }
			set { actBet = value;
				PossWin = (MinBet==0 && MinWin==0) ? 0 : (MinWin/MinBet * actBet);//Just in case that the minBet is not 1 Euro
                OnPropertyChanged("ManipulationFeeValue");
                OnPropertyChanged("BonusValue");
                OnPropertyChanged("DecimalBet");
                OnPropertyChanged("ReducedStakeValue");
            }
		}
		public decimal PossWin {
            get { return possWin; }
			set { possWin = value; OnPropertyChanged("PossWin"); }
		}

		

        
		public int MinCombination {
			get { return minCombination; }
			set { minCombination = value; }
		}

		public string BonusDesc {
			get { return bonusDesc; }
			set { bonusDesc = value; OnPropertyChanged("BonusDesc"); }
		}

        public string ManipulationFeeDesc
        {
            get { return manipulationFeeDesc; }
            set { manipulationFeeDesc = value; OnPropertyChanged("ManipulationFeeDesc"); }
		}

        public string InsertionDesc
        {
            get { return insertionDesc; }
            set { insertionDesc = value; }
        }
        

		public string CombiDesc {
			get { return combiDesc; }
			set { combiDesc = value; }
		}

		public string SystemDesc {
			get { return systemDesc; }
			set { systemDesc = value; }
		}

		public string SingleDesc {
			get { return singleDesc; }
			set { singleDesc = value; }
		}
		public string MultiwayDesc {
			get { return multiwayDesc; }
			set { multiwayDesc = value; }
		}

		public string SystemXYBank {
			get {
				if (pathCount>0)
					return SystemXY+"  + "+PathCount+" "+BankDesc;
				else
					return SystemXY;
			}		
		}

		public string SystemXY {
			get { return numSystemX.ToString() + " / " + NumSystemY.ToString(); }
			//set { systemXY = value; OnPropertyChanged("SystemXY"); }
		}
		public int NumSystemX {
			get { return numSystemX; }
			set {
				if (value >= MinSystemX  &&  value < NumSystemY  &&  value != numSystemX) {
					numSystemX = value;
					//SystemXY = numSystemX.ToString() + " / " + numSystemY.ToString();
					OnPropertyChanged("SystemXY");
				}
			}
		}
		
		public int NumSystemY {
		    get { return numOfTipps - pathCount; }
		//    set {
		//        numSystemY = value;
		//        SystemXY = numSystemX.ToString() + " / " + numSystemY.ToString();
		//    }
		}
		
		public decimal NumOfBets {
			get { return numOfBets; }
			set { numOfBets = value; OnPropertyChanged("NumOfBets"); }
		}
		public string NumOfBetsDesc {
			get { return numOfBetsDesc; }
			set { numOfBetsDesc = value; OnPropertyChanged("NumOfBetsDesc"); }
		}

		public int PathCount {
			get {
				return pathCount; }
			set {
				//if (value <= numOfTipps - MinSystemY  &&  numOfTipps >= MinSystemY  &&  value >= 0) {//Removed by GMU
					pathCount = value;
					//NumSystemY = numOfTipps - pathCount;
					//NumSystemX = numOfTipps - pathCount - 1;
					OnPropertyChanged("PathCount");
				//}
			}
		}

		//public void UpdatePathCount()
		//{
		//        Dictionary<long, int> CodeCountDict = new Dictionary<long,int>();

		//        for (int i = 0; i < DataBinding.TipItems.Count;i++)
		//        {
		//            if (DataBinding.TipItems[i].IsBank)
		//            {
		//                if (CodeCountDict.ContainsKey(DataBinding.TipItems[i].Match.SvrMatchID))
		//                {
		//                    CodeCountDict[DataBinding.TipItems[i].Match.SvrMatchID]++;
		//                }
		//                else
		//                {
		//                    CodeCountDict[DataBinding.TipItems[i].Match.SvrMatchID] = 1;
		//                }
		//            }
		//        }

		//        int curPathCount = CodeCountDict.Values.Count;

		//        if (Nbt.Station.Design.Controls.ViewControl.TicketState == Nbt.Station.Design.Controls.TicketStates.Combi)
		//        {
		//            foreach (int iSvrMatchId in CodeCountDict.Keys)
		//            {
		//                if (CodeCountDict[iSvrMatchId] == 1)
		//                {
		//                    for (int j = 0; j < DataBinding.TipItems.Count; j++)
		//                    {
		//                        if (DataBinding.TipItems[j].Match.SvrMatchID == iSvrMatchId)
		//                        {
		//                            DataBinding.TipItems[j].IsBank = false;
		//                            curPathCount--;
		//                        }
		//                    }
		//                }
		//            }
		//        }

		//        pathCount = curPathCount;
		//}

		public decimal MultiWayOddFactor {		//usefull??
			get { return multiWayOddFactor; }
			set { multiWayOddFactor = value;				
//					OnPropertyChanged("MultiWayOddFactor");
				}			
		}
		public string BankDesc {
			get { return bankDesc; }
			set { bankDesc = value; OnPropertyChanged("BankDesc"); }
		}

		public string MinBetDesc {
			get { return minBetDesc; }
			set { minBetDesc = value; OnPropertyChanged("MinBetDesc"); }
		}

		public string MaxBetDesc {
			get { return maxBetDesc; }
			set { maxBetDesc = value; OnPropertyChanged("MaxBetDesc"); }
		}

		public string MinWinDesc {
			get { return minWinDesc; }
			set { minWinDesc = value; OnPropertyChanged("MinWinDesc"); }
		}

		public string MaxWinDesc {
			get { return maxWinDesc; }
			set { maxWinDesc = value; OnPropertyChanged("MaxWinDesc"); }
		}

		public decimal MinBet {
			get { return minBet; }
			set { minBet = value; OnPropertyChanged("MinBet"); }
		}

		public decimal MaxBet {
			get { return maxBet; }
			set { maxBet = value; OnPropertyChanged("MaxBet"); }
		}

		public decimal MinWin {
			get { return minWin; }
			set { minWin = value; OnPropertyChanged("MinWin"); }
		}

		public decimal MaxWin {
			get { return maxWin; }
			set { maxWin = value; OnPropertyChanged("MaxWin"); }
		}



		public decimal BonusFactor {
			get { return this.bonusFactor; }
			set {
				this.bonusFactor = value;
				OnPropertyChanged("BonusFactor");
			}
		}
		//public string BonusValStr {
		//    get { return this.bonusValStr; }
		//    set {
		//        this.bonusValStr = value;
		//        OnPropertyChanged("BonusValStr");
		//    }
		//}
        public decimal OddOfTipps {
            get { return this.oddOfTipps; }
            set { 
				this.oddOfTipps = value;
				OnPropertyChanged("OddOfTipps");
			}
        }

		public decimal FullOddFactor {
			get { return this.fullOddFactor; }
			set {
                this.fullOddFactor = value; //(float)Math.Round(value, 2);
				OnPropertyChanged("FullOddFactor");
			}
		}

		public int NumOfTipps {
			get { return this.numOfTipps; }
			set {
				this.numOfTipps = value;
				//ResetNumXY();
				OnPropertyChanged("NumOfTipps");
			}
		}

		public string NumOfTippsDesc {
			get { return this.numOfTippsDesc; }
			set {
				this.numOfTippsDesc = value;
				OnPropertyChanged("NumOfTippsDesc");
			}
		}
		public string OddOfTippsDesc
		{
			get { return this.oddOfTippsDesc; }
			set
			{
				this.oddOfTippsDesc = value;
				OnPropertyChanged("OddOfTippsDesc");
			}
		}
		public bool IsMaxOddBet
		{
			get { return this.isMaxOddBet; }
			set
			{
				this.isMaxOddBet = value;
			}
		}

        #endregion


		public TipListInfo() {
			this.Reset();
		}

		public void ResetNumXY() {
			if (NumSystemY >= MinSystemY ) 
				NumSystemX = NumSystemY-1;
			OnPropertyChanged("SystemXY");
		}

		public void Reset() {
			this.Bet = 0;
			this.oddOfTipps = 1;
			this.bonusFactor = 1;
			this.multiWayOddFactor = 1;
			this.numOfTipps = 0;
			this.pathCount = 0;
			this.numSystemX = 0;
			this.minCombination = 0;			
			this.enableSystemAddBtn = this.enableSystemSubBtn = true;
			//this.numSystemY = 0;
			this.systemXY = String.Empty;
            this.fullOddFactor = 0;
            this.minWin = 0;
            this.maxWin = 0;
            this.MaxBet = 0;
		}
		
		
		//property changed event must be implemented because is an interface member
		public event PropertyChangedEventHandler PropertyChanged;

	#region INotifyPropertyChanged Member

			protected void OnPropertyChanged(string name) {
				if (PropertyChanged != null) {
					PropertyChanged(this, new PropertyChangedEventArgs(name));
				}
			}

	#endregion
	}
}

