using System;
using System.Windows;
using System.Collections.Generic;
using SportBetting.WPF.Prism.Shared.Annotations;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Linq;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class TicketView: INotifyPropertyChanged
    {

        public TicketView(string number, string checksum, string status, long statusId, DateTime createdAt, string currency)
        {
            Number = number;
            CheckSum = checksum;
            FullNumber = number + " " + checksum;
            Status = status;
            StatusId = statusId;
            CreatedAt = createdAt;
            Currency = currency;
            _instances.Add(this);
        }

        public bool Hidden { get; set; }
        public string FullNumber { get; private set; }
        public string Number { get; set; }
        public string CheckSum { get; set; }
        public string Status { get; set; }
       public long StatusId { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedAt { get; set; }

        public static List<TicketView> _instances = new List<TicketView>();
      

        private bool _isButtonChecked = false;
        public bool IsButtonChecked
        {
            get { return _isButtonChecked; }
            set
            {
                      
                if (value == true)
                {                    
                    foreach (var instance in _instances.Where(i => !ReferenceEquals(i, this)))
                    {
                        instance.IsButtonChecked = false;
                    }
                }
                _isButtonChecked = value;
                OnPropertyChanged();
            }
        }


        public Visibility PendingApprovalVisibility
        {
            get
            {
                return StatusId == 5 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string OpenLostWonColor
        {
            get
            {
                switch (StatusId)
                {
                    //case 1:
                    //    return "#ff22b613"; // state 1 == won
                    //case 2:
                    //    return "#FFFF1313"; // state 2 == lost
                    case 4:
                        return "#FFFFFFFF"; // state 0 == open
                    case 5:
                        return "#FF9933"; // pending for approval
                }
               
                return "#FFFFFFFF"; // state 0 == canceled
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}