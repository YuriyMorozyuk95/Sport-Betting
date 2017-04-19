using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace SportBetting.WPF.Prism.ModalWindows
{
    public static class TCUtility
    {
        public static readonly DependencyProperty BindableBlocksProperty =
            DependencyProperty.RegisterAttached("BindableBlocks", typeof(FlowDocument), typeof(TCUtility), new UIPropertyMetadata(null, BindableBlocksPropertyChanged));

        public static string GetBindableBlocks(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableBlocksProperty);
        }

        public static void SetBindableBlocks(DependencyObject obj, string value)
        {
            obj.SetValue(BindableBlocksProperty, value);
        }

        public static void BindableBlocksPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            object _lockerTimer = new object();
            lock (_lockerTimer)
            {
                FlowDocument fD = o as FlowDocument;
                if (fD != null)
                {
                    fD.Blocks.Clear();
                    foreach (var b in (e.NewValue as FlowDocument).Blocks.ToArray())
                    {
                        fD.Blocks.Add(b);
                    }
                }
            }
        }

    }
}
