using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ConwayLife;

public partial class ObjectEditor
{
    public ObjectEditor()
    {
        InitializeComponent();
        _gridDimension = 32;
        _squares = new Rectangle[_gridDimension, _gridDimension];
    }

    // public ObjectEditor(int dimension)
    // {
    //     InitializeComponent();
    //     gridDimension = dimension;
    // }

    public ObjectEditor(string shapeKey)
    {
        InitializeComponent();
        _squares = new Rectangle[8, 8];
        _sourceShapeKey = shapeKey;
        _sourceShape = Core.Instance.ObjectForKey(shapeKey);
        if (_sourceShape is { })
        {
            _gridDimension = _sourceShape.LargestDimension;
            _squares = new Rectangle[_gridDimension, _gridDimension];    
        }
    }

    public ObjectEditor(Shape shape)
    {
        InitializeComponent();
        _sourceShape = shape;
        _gridDimension = _sourceShape.LargestDimension;
        _squares = new Rectangle[_gridDimension, _gridDimension];
    }

    private readonly string _sourceShapeKey = string.Empty;
    private readonly Shape? _sourceShape;
    private int _gridDimension = 32;
    private int _liveSquares;
    private int _xMargin;
    private int _yMargin;
    private int _xSpan;
    private int _ySpan;
    private Rectangle[,] _squares;

    private void GetGridRange()
    {
        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;
        for (int x = 0; x < _gridDimension; x++)
        {
            for (int y = 0; y < _gridDimension; y++)
            {
                if (_squares[x, y].Tag is string tagger)
                {
                    if (tagger.Equals("Y"))
                    {
                        if (x > maxX)
                        {
                            maxX = x;
                        }

                        if (y > maxY)
                        {
                            maxY = y;
                        }

                        if (x < minX)
                        {
                            minX = x;
                        }

                        if (y < minY)
                        {
                            minY = y;
                        }
                        
                    }    
                }
            }
        }

        _xMargin = minX;
        _yMargin = minY;
        _xSpan = 1 + maxX - minX;
        _ySpan = 1 + maxY - minY;
        // show range on grid
        for (int x = 0; x < _gridDimension; x++)
        {
            for (int y = 0; y < _gridDimension; y++)
            {
                if (((x >= _xMargin) && (x < (_xMargin + _xSpan))) && ((y >= _yMargin) && (y < (_yMargin + _ySpan))))
                {
                    if (_squares[x, y].Tag is string tagger)
                    {
                        if (tagger.Equals("N"))
                        {
                            _squares[x, y].Fill = Brushes.Ivory;
                        }    
                        else
                        {
                            _squares[x, y].Fill = Brushes.WhiteSmoke;
                        }        
                    }
                }
                else
                {
                    _squares[x, y].Fill = Brushes.WhiteSmoke;
                }
            }
        }

        // report values
        textblockGrid.Text = $"GRID: {_xSpan} x {_ySpan}";
        textblockCells.Text = $"CELLS: {_liveSquares}";
    }

    private void SquareClicked(object sender, MouseButtonEventArgs e)
    {
        if (sender is Rectangle r)
        {
            if (r.Tag is string tagger)
            {
                if (tagger.Equals("Y"))
                {
                    r.Tag = "N";
                    r.Fill = Brushes.Ivory;
                    _liveSquares--;
                    if (_liveSquares == 0)
                    {
                        buttonSaveObject.IsEnabled = false;
                    }
                }
                else
                {
                    r.Tag = "Y";
                    r.Fill = Brushes.SaddleBrown;
                    _liveSquares++;
                    if (_liveSquares > 0)
                    {
                        buttonSaveObject.IsEnabled = true;
                    }
                }
            }

            GetGridRange();
        }
    }

    private Shape? PatternToSave()
    {
        if (string.IsNullOrWhiteSpace(textboxDescription.Text))
        {
            MessageBox.Show("Please enter the object's name", "Object editor", MessageBoxButton.OK
                , MessageBoxImage.Warning);
            return null;
        }

        if (string.IsNullOrWhiteSpace(comboboxGroups.Text))
        {
            MessageBox.Show("Please enter the kind of object", "Object editor", MessageBoxButton.OK
                , MessageBoxImage.Warning);
            return null;
        }

        GetGridRange();

        Shape thing = new Shape(_xSpan, _ySpan);
        for (int x = _xMargin; x < (_xMargin + _xSpan); x++)
        {
            for (int y = _yMargin; y < (_yMargin + _ySpan); y++)
            {
                if (_squares[x, y].Tag is string tagger)
                {
                    if (tagger.Equals("Y"))
                    {
                        thing.SetCellValue(x - _xMargin, y - _yMargin, true);    
                    }
                }
            }
        }

        thing.Name = textboxDescription.Text.Trim();
        thing.Kind = comboboxGroups.Text;
        thing.Comment = textboxComment.Text.Trim();
        thing.Discoverer = textboxDiscoverer.Text.Trim();
        thing.DiscoveryYear = textboxYear.Text.Trim();
        return thing;
    }

    private void buttonSaveObject_Click(object sender, RoutedEventArgs e)
    {
        Shape? pattern = PatternToSave();

        if (pattern == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_sourceShapeKey))
        {
            Core.Instance.AddNewObject(pattern);
        }
        else
        {
            Core.Instance.DeleteObject(_sourceShapeKey);
            Core.Instance.AddNewObject(pattern);
        }

        DialogResult = true;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        double scrX = SystemParameters.PrimaryScreenWidth;
        double scrY = SystemParameters.PrimaryScreenHeight;
        double winX = scrX * .98;
        double winY = scrY * .94;
        double xm = (scrX - winX) / 2;
        double ym = (scrY - winY) / 4;
        Width = winX;
        Height = winY;
        Left = xm;
        Top = ym;

        InitialiseGrid();
        // fill combo box of existing group names
        comboboxGroups.Items.Clear();
        List<string> grps = Core.Instance.ListOfGroups();
        foreach (string g in grps)
        {
            ComboBoxItem itm = new ComboBoxItem();
            itm.Content = g;
            comboboxGroups.Items.Add(itm);
        }
    }

    private void InitialiseGrid()
    {
        canvasDesign.Children.Clear();
        // calculate grid square size
        double qw = canvasDesign.ActualWidth / _gridDimension;
        double qh = canvasDesign.ActualHeight / _gridDimension;
        if (qh < qw)
        {
            qw = qh;
        }

        int side = (int) qw;
        _squares = new Rectangle[_gridDimension, _gridDimension];
        for (int x = 0; x < _gridDimension; x++)
        {
            for (int y = 0; y < _gridDimension; y++)
            {
                _squares[x, y] = new Rectangle();
                _squares[x, y].Width = side;
                _squares[x, y].Height = side;
                _squares[x, y].Stroke = Brushes.Gray;
                _squares[x, y].StrokeThickness = 0.5;
                _squares[x, y].Fill = Brushes.White;
                _squares[x, y].Tag = "N";
                _squares[x, y].MouseDown += SquareClicked;
                Canvas.SetTop(_squares[x, y], side * y);
                Canvas.SetLeft(_squares[x, y], side * x);
                canvasDesign.Children.Add(_squares[x, y]);
            }
        }
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        textboxDescription.Focus();
        if (_sourceShape == null)
        {
            return;
        }

        textboxDescription.Text = _sourceShape.Name;
        comboboxGroups.Text = _sourceShape.Kind;
        textboxComment.Text = _sourceShape.Comment;
        textboxDiscoverer.Text = _sourceShape.Discoverer;
        textboxYear.Text = _sourceShape.DiscoveryYear;

        for (int x = 0; x < _sourceShape.Width; x++)
        {
            for (int y = 0; y < _sourceShape.Height; y++)
            {
                if (_sourceShape.CellValue(x, y))
                {
                    _squares[x, y].Tag = "Y";
                    _squares[x, y].Fill = Brushes.SaddleBrown;
                    _liveSquares++;
                }
            }

        }

        if (_liveSquares > 0)
        {
            buttonSaveObject.IsEnabled = true;
        }

        GetGridRange();
    }

    private void mnuShiftDown_Click(object sender, RoutedEventArgs e)
    {
        for (int y = _gridDimension - 1; y > 0; y--)
        {
            for (int x = 0; x < _gridDimension; x++)
            {
                if (_squares[x, y - 1].Tag is string tagger)
                {
                    if (tagger.Equals("Y"))
                    {
                        _squares[x, y].Tag = "Y";
                        _squares[x, y].Fill = Brushes.SaddleBrown;    
                    }
                    else
                    {
                        _squares[x, y].Tag = "N";
                        _squares[x, y].Fill = Brushes.Ivory;
                    }
                }
                
            }
        }

        for (int x = 0; x < _gridDimension; x++)
        {
            {
                _squares[x, 0].Tag = "N";
                _squares[x, 0].Fill = Brushes.Ivory;
            }
        }

        GetGridRange();
    }

    private void mnuShiftRight_Click(object sender, RoutedEventArgs e)
    {
        for (int x = _gridDimension - 1; x > 0; x--)
        {
            for (int y = 0; y < _gridDimension; y++)
            {
                if (_squares[x - 1, y].Tag is string tagger)
                {
                    if (tagger.Equals("Y"))
                    {
                        _squares[x, y].Tag = "Y";
                        _squares[x, y].Fill = Brushes.SaddleBrown;
                    }
                    else
                    {
                        _squares[x, y].Tag = "N";
                        _squares[x, y].Fill = Brushes.Ivory;
                    }    
                }
            }
        }

        for (int y = 0; y < _gridDimension; y++)
        {
            {
                _squares[0, y].Tag = "N";
                _squares[0, y].Fill = Brushes.Ivory;
            }
        }

        GetGridRange();
    }

    private void mnuGridExpand_Click(object sender, RoutedEventArgs e)
    {
        List<int> points = new List<int>();
        for (int x = 0; x < _gridDimension; x++)
        {
            for (int y = 0; y < _gridDimension; y++)
            {
                if (_squares[x, y].Tag is string tagger)
                {
                    if (tagger.Equals("Y"))
                    {
                        int q = x * 10000 + y;
                        points.Add(q);
                    }    
                }
                
            }
        }

        _gridDimension++;
        InitialiseGrid();
        foreach (int q in points)
        {
            int y = q % 10000;
            int x = (q - y) / 10000;
            {
                _squares[x, y].Tag = "Y";
                _squares[x, y].Fill = Brushes.SaddleBrown;
            }
        }

        GetGridRange();
    }

    private void mnuFileSaveRle_Click(object sender, RoutedEventArgs e)
    {
        Shape? pattern = PatternToSave();

        if (pattern == null)
        {
            return;
        }

        // Define dialog box
        Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
        sfd.AddExtension = true;
        sfd.CheckPathExists = true;
        sfd.DefaultExt = "rle";
        sfd.Filter = "Run length encoding (RLE) files (*.rle)|*.rle";
        sfd.InitialDirectory = Jbh.AppManager.DataPath;
        sfd.OverwritePrompt = true;
        sfd.Title = "Save RLE file";
        sfd.ValidateNames = true;
        bool? tosave = sfd.ShowDialog();
        if (tosave.HasValue && tosave.Value)
        {
            string filepath = sfd.FileName;
            Core.Instance.SavePatternAsRleFile(filepath, pattern);
        }
    }
}