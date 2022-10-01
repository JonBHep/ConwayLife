using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ConwayLife;

public partial class ObjectSelectWindow
{
    public ObjectSelectWindow()
    {
        InitializeComponent();
        square = new Rectangle[Core.ObjectDimension, Core.ObjectDimension];
    }

    private string returnedKey = string.Empty;
    private Rectangle[,] square;

    private class ListedItem
    {
        public string GroupName { get; set; } = string.Empty;
        public string ObjectDescription { get; set; }= string.Empty;
        public string ObjectKey { get; set; }= string.Empty;
    }

    public string SelectedKey
    {
        get { return returnedKey; }
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        InitialiseSquares();

        buttonSelect.IsEnabled = false;
        buttonDelete.IsEnabled = false;
        treeviewObjects.Items.Clear();
        var groups = Core.Instance.ListOfGroups();
        foreach (var grp in groups)
        {
            var tvi = new TreeViewItem();
            tvi.Header = grp;
            tvi.Tag = null;
            treeviewObjects.Items.Add(tvi);
        }

        foreach (var key in Core.Instance.ListOfKeys())
        {
            var it = new ListedItem();
            it.ObjectKey = key;
            var thing = Core.Instance.ObjectForKey(key);
            if (thing is null)
            {
                return;
            }
            it.ObjectDescription = thing.Name;
            it.GroupName = thing.Kind;

            var grpItem = new TreeViewItem();
            foreach (TreeViewItem itm in treeviewObjects.Items)
            {
                if (itm.Header is string g)
                {
                    if (g.Equals(it.GroupName))
                    {
                        grpItem = itm;
                    }
                }
            }

            var tvi = new TreeViewItem();
            tvi.Header = it.ObjectDescription;
            tvi.Tag = it;
            grpItem.Items.Add(tvi);
        }
    }

    private void buttonDelete_Click(object sender, RoutedEventArgs e)
    {
        if (treeviewObjects.SelectedItem is TreeViewItem item)
        {
            if (item.Tag is ListedItem listed)
            {
                var mbr = MessageBox.Show("Delete the selected object?", "Game of Life", MessageBoxButton.OKCancel
                    , MessageBoxImage.Question);
                if (mbr == MessageBoxResult.Cancel)
                {
                    return;
                }
                returnedKey = listed.ObjectKey;
                Core.Instance.DeleteObject(returnedKey);
                DialogResult = false;        
            }    
        }
    }

    private void buttonSelect_Click(object sender, RoutedEventArgs e)
    {
        if (treeviewObjects.SelectedItem is TreeViewItem selection)
        {
            if (selection.Tag is ListedItem it)
            {
                returnedKey = it.ObjectKey;
                DialogResult = true;    
            }
        }
    }

    private void treeviewObjects_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (treeviewObjects.SelectedItem is TreeViewItem selection)
        {
            if (selection.Tag is ListedItem item)
            {
                buttonDelete.IsEnabled = true;
                buttonSelect.IsEnabled = true;
                var thing = Core.Instance.ObjectForKey(item.ObjectKey);
                if (thing is { })
                {
                    DrawObject(thing);    
                }
            }
        }
        else
        {
            buttonDelete.IsEnabled = false;
            buttonSelect.IsEnabled = false;
        }
    }

    private void DrawObject(Shape thing)
    {
        ClearCells();
        if (Core.ObjectDimension < thing.Width || Core.ObjectDimension < thing.Height)
        {
            MessageBox.Show(
                $"Grid = {Core.ObjectDimension} x {Core.ObjectDimension}, object = {thing.Width} x {thing.Height}"
                , "Cannot display object", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var xmargin = (Core.ObjectDimension - thing.Width) / 2;
        var ymargin = (Core.ObjectDimension - thing.Height) / 2;
        for (var x = 0; x < thing.Width; x++)
        {
            for (var y = 0; y < thing.Height; y++)
            {
                if (thing.CellValue(x, y))
                {
                    square[x + xmargin, y + ymargin].Fill = Brushes.SaddleBrown;
                }
            }
        }
    }

    private void ClearCells()
    {
        for (var x = 0; x < Core.ObjectDimension; x++)
        {
            for (var y = 0; y < Core.ObjectDimension; y++)
            {
                square[x, y].Fill = Brushes.Ivory;
            }
        }
    }

    private void InitialiseSquares()
    {
        for (var x = 0; x < Core.ObjectDimension; x++)
        {
            for (var y = 0; y < Core.ObjectDimension; y++)
            {
                var r = new Rectangle();
                r.Width = 6;
                r.Height = 6;
                r.Fill = Brushes.Ivory;
                Canvas.SetLeft(r, x * 6 + 8);
                Canvas.SetTop(r, y * 6 + 8);
                canvasShow.Children.Add(r);
                square[x, y] = r;
            }
        }
    }
}