using System;
using System.Windows;

namespace ConwayLife;

public partial class InputBox
{
    public InputBox(string boxTitle, string promptText, string defaultResponse)
    {
        InitializeComponent();
        this.Title = boxTitle;
        textblockPrompt.Text = promptText;
        textboxResponse.SelectedText = defaultResponse;
    }

    public string ResponseText => textboxResponse.Text;

    private void buttonOkay_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void buttonCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        Icon = this.Owner.Icon;
        textboxResponse.Focus();
    }
}