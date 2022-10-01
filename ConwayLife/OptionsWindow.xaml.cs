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
        if (WrapRadioButton.IsChecked.HasValue && WrapRadioButton.IsChecked.Value)
        {
            Core.Instance.Wrapping = true;
        }
        else
        {
            Core.Instance.Wrapping = false;
        }

        if (EverRadioButton.IsChecked ?? false)
        {
            Core.Instance.CellDisplay = Core.CellDisplayOptions.EverLive;
        }
        else if (AffectedRadioButton.IsChecked ?? false)
        {
            Core.Instance.CellDisplay = Core.CellDisplayOptions.Affected;
        }
        else if (ImmigrationRadioButton.IsChecked ?? false)
        {
            Core.Instance.CellDisplay = Core.CellDisplayOptions.Immigration;
        }
        else if (QuadLifeRadioButton.IsChecked ?? false)
        {
            Core.Instance.CellDisplay = Core.CellDisplayOptions.QuadLife;
        }
        else
        {
            Core.Instance.CellDisplay = Core.CellDisplayOptions.Life;
        }

        if (ConwayRadioButton.IsChecked.HasValue && ConwayRadioButton.IsChecked.Value)
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
            WrapRadioButton.IsChecked = true;
        }
        else
        {
            NoWrapRadioButton.IsChecked = true;
        }

        switch (Core.Instance.CellDisplay)
        {
            case Core.CellDisplayOptions.EverLive:
                EverRadioButton.IsChecked = true;
                break;
            case Core.CellDisplayOptions.Affected:
                AffectedRadioButton.IsChecked = true;
                break;
            default:
                NoneRadioButton.IsChecked = true;
                break;
        }

        if (Core.Instance.RuleString.Equals("B3/S23"))
        {
            ConwayRadioButton.IsChecked = true;
        }
        else
        {
            HighLifeRadioButton.IsChecked = true;
        }
    }

}