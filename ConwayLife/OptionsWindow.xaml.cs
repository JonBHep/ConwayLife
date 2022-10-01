using System;
using System.Windows;

namespace ConwayLife;

public partial class OptionsWindow
{
    public OptionsWindow()
    {
        InitializeComponent();
    }
    private void buttonOkay_Click(object sender, RoutedEventArgs e)
        {
            if (radiobuttonWrap.IsChecked.HasValue && radiobuttonWrap.IsChecked.Value)
            {
                Core.Instance.Wrapping = true;
            }
            else
            {
                Core.Instance.Wrapping = false;
            }
            if (radiobuttonEver.IsChecked.HasValue && radiobuttonEver.IsChecked.Value)
            {
                Core.Instance.CellDisplay= Core.CellDisplayOptions.EverLive;
            }
            else if (radiobuttonAffected.IsChecked.HasValue && radiobuttonAffected.IsChecked.Value)
            {
                Core.Instance.CellDisplay = Core.CellDisplayOptions.Affected;
            }
            else
            {
                Core.Instance.CellDisplay= Core.CellDisplayOptions.None;
            }
            if (radiobuttonConway.IsChecked.HasValue && radiobuttonConway.IsChecked.Value)
            {
                Core.Instance.RuleString = "B3/S23";
            }
            else
            {
                Core.Instance.RuleString = "B36/S23";
            }
            DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (Core.Instance.Wrapping)
            {
                radiobuttonWrap.IsChecked = true;
            }
            else
            {
                radiobuttonNoWrap.IsChecked = true;
            }
            if (Core.Instance.CellDisplay== Core.CellDisplayOptions.EverLive)
            {
                radiobuttonEver.IsChecked = true;
            }
            else if (Core.Instance.CellDisplay == Core.CellDisplayOptions.Affected)
            {
                radiobuttonAffected.IsChecked = true;
            }
            else 
            {
                radiobuttonNone.IsChecked = true;
            }
            if (Core.Instance.RuleString.Equals("B3/S23"))
            {
                radiobuttonConway.IsChecked = true;
            }
            else
            {
                radiobuttonHighLife.IsChecked = true;
            }
        }

}