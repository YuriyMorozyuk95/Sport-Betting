﻿using System.Windows;
using System.Windows.Controls;
using SportBetting.WPF.Prism.Shared.Models;

namespace SportBetting.WPF.Prism.Shared
{
    public class TournamentTemplateSelector : DataTemplateSelector
    {
        private DataTemplate usualTemplate;
        private DataTemplate allTournamentsTemplate;
        private DataTemplate outrightsTemplate;

        public TournamentTemplateSelector()
        {
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var currentEntity = (TournamentVw)item;
            if (currentEntity == null)
            {
                return this.UsualTemplate;
            }
            else
            {
                long id = currentEntity.SvrId;
                if (id == -999)
                    return this.AllTournamentsTemplate;
                if (currentEntity.Name == "Outrights")
                    return this.OutrightsTemplate;
            }

            return this.UsualTemplate;
        }

        public DataTemplate UsualTemplate
        {
            get
            {
                return usualTemplate;
            }
            set
            {
                this.usualTemplate = value;
            }
        }

        public DataTemplate AllTournamentsTemplate
        {
            get
            {
                return allTournamentsTemplate;
            }
            set
            {
                this.allTournamentsTemplate = value;
            }
        }

        public DataTemplate OutrightsTemplate
        {
            get
            {
                return outrightsTemplate;
            }
            set
            {
                this.outrightsTemplate = value;
            }
        }
        

        
    }
}
