using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ConwayLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            _cell = new bool[DimensionX, DimensionY];
            _everbeen = new bool[DimensionX, DimensionY];
            _neighbours = new int[DimensionX, DimensionY];
            _affected = new bool[DimensionX, DimensionY];
            _affectedNow = new bool[DimensionX, DimensionY];
            _square = new Rectangle[DimensionX, DimensionY];
            for (int xx = 0; xx < DimensionX; xx++)
            {
                for (int yy = 0; yy < DimensionY; yy++)
                {
                    _square[xx, yy] = new Rectangle();
                }
            }
            _tmr = new DispatcherTimer(interval: TimeSpan.FromSeconds(0.4), priority: DispatcherPriority.Normal, dispatcher: Dispatcher.CurrentDispatcher, callback: TimerFire);
        }
        /*
         
         * Conway's Game of Life
        
         *     Any live cell with fewer than two live neighbours dies, as if caused by under-population.
         *     Any live cell with two or three live neighbours lives on to the next generation.
         *     Any live cell with more than three live neighbours dies, as if by over-population.
         *     Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
         
               The standard Game of Life is symbolised as "B3/S23": A cell is "Born" if it has exactly 3 living neighbours, "Survives" if it has 2 or 3 living neighbours; it dies otherwise.
        */

        /*
           Apart from the classes of object demonstrated in this implementation, later discoveries included other "guns", which are stationary, and which shoot out gliders or other spaceships; "puffers", which move along leaving behind a trail of debris; and "rakes", which move and emit spaceships.[23] Gosper also constructed the first pattern with an asymptotically optimal quadratic growth rate, called a "breeder", or "lobster", which worked by leaving behind a trail of guns.

           It is possible for gliders to interact with other objects in interesting ways. For example, if two gliders are shot at a block in just the right way, the block will move closer to the source of the gliders. If three gliders are shot in just the right way, the block will move farther away. This "sliding block memory" can be used to simulate a counter. It is possible to construct logic gates such as AND, OR and NOT using gliders. It is possible to build a pattern that acts like a finite state machine connected to two counters. This has the same computational power as a universal Turing machine, so the Game of Life is theoretically as powerful as any computer with unlimited memory and no time constraints: it is Turing complete.

           Furthermore, a pattern can contain a collection of guns that fire gliders in such a way as to construct new objects, including copies of the original pattern. A "universal constructor" can be built which contains a Turing complete computer, and which can build many types of complex objects, including more copies of itself.

           
         */
        private readonly bool[,] _cell;
        private readonly bool[,] _everbeen;
        private readonly int[,] _neighbours;
        private readonly bool[,] _affectedNow;
        private readonly bool[,] _affected;
        private readonly Rectangle[,] _square;
        private const int DimensionX=148;
        private const int DimensionY=148;
        private const int Blocksize = 5;
        private int _generation;
        private int _population;
        private int _genchanges;
        private int _randomseed = 11267;
        private readonly DispatcherTimer _tmr;
        //private DispatcherTimer _cloc;
        //private Stopwatch watch;
        //private long calcelapsed;
        //private long drawelapsed;

        private readonly SolidColorBrush _brushBackground = Brushes.Ivory;
        // private SolidColorBrush _brushLive = Brushes.DarkGreen;
        private readonly SolidColorBrush _brushAffected = Brushes.Moccasin;
        private readonly SolidColorBrush _brushEverlive = Brushes.Yellow;

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
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            textblockDiscovery.Text = string.Empty;
            textblockPattern.Text = string.Empty;

            canvasPond.Width = Blocksize * DimensionX;
            canvasPond.Height = Blocksize * DimensionY;
            // _cell = new bool[DimensionX, DimensionY];
            //_everbeen = new bool[DimensionX, DimensionY];
            //_neighbours = new int[DimensionX, DimensionY];
            // _affected = new bool[DimensionX, DimensionY];
            // _affectedNow = new bool[DimensionX, DimensionY];
            // _square = new Rectangle[DimensionX, DimensionY];
            BuildStillLifeList();
            DisplayOptions();
            InitialiseSquares();
            DisplayPopulation();
            mnuGame.IsEnabled = true;
            mnuRandom.IsEnabled = true;
            mnuObjects.IsEnabled = true;
            mnuWeb.IsEnabled = true;
            buttonClose.IsEnabled = true;

            //watch = new Stopwatch();

            // _tmr = new DispatcherTimer(interval: TimeSpan.FromSeconds(0.4), priority: DispatcherPriority.Normal, dispatcher: Dispatcher.CurrentDispatcher, callback: TimerFire);
            _tmr.Stop();
            //_cloc = new DispatcherTimer(interval: TimeSpan.FromSeconds(1), priority: DispatcherPriority.Normal, dispatcher: Dispatcher.CurrentDispatcher, callback: ClockFire);
        }

        private void DisplayOptions()
        {
            textblockWrapping.Text = Core.Instance.Wrapping ? "Wrapping" : "Not wrapping";

            if (Core.Instance.CellDisplay== Core.CellDisplayOptions.Affected)
            {
                textblockShowEver.Text = "Highlighting neighbours of changed cells";
            }
            else if (Core.Instance.CellDisplay == Core.CellDisplayOptions.EverLive)
            {
                textblockShowEver.Text = "Highlighting cells which have ever been live";
            }
            else
            {
                textblockShowEver.Text = "No highlighting";
            }

            textblockRules.Text = $"Rules: {Core.Instance.RuleString}";
        }

        private void ClearCells()
        {
            for (int x = 0; x < DimensionX; x++)
            {
                for (int y = 0; y < DimensionY; y++)
                {
                    _cell[x, y] = false;
                    _everbeen[x, y] = false;
                    _square[x, y].Fill = Brushes.Ivory;
                    _neighbours[x, y] = 0;
                    _affected[x, y] = false;
                    _affectedNow[x, y] = false;
                }
            }
        }

        private void DrawObject(Shape thing, int oX, int oY)
        {
            textblockPattern.Text =$"{thing.Name} ({thing.Kind})";
            textblockDiscovery.Text = $"Discovered by {thing.Discoverer} in {thing.DiscoveryYear}";
            textblockObjectDetails.Text = thing.Comment;
            ClearCells();
            for (int x=0;x< thing.Width; x++)
            {
                for (int y = 0; y < thing.Height; y++)
                {
                    if (thing.CellValue(x, y)) { _cell[oX + x, oY + y] = true; _everbeen[oX + x, oY + y] = true; }
                }
            }
            InitialiseRun();
        }

        private void DoRandom(int grain)
        {
            ClearCells();
            textblockPattern.Text = "Random pattern";
            Random rng = new Random();
            if (grain >=0) { rng = new Random(grain); textblockPattern.Text = $"Random pattern [{grain}]"; }
            for (int x = 0; x < DimensionX; x++)
            {
                for (int y = 0; y < DimensionY; y++)
                {
                    bool q = (rng.NextDouble() > 0.8);
                    _cell[x, y] = q;
                    _everbeen[x, y] = q;
                }
            }
            InitialiseRun();
        }
        
        private void AdvanceOneGeneration()
        {
            Evolve(Core.Instance.RuleString);
            DisplayPopulation();
        }

        private void TimerFire(object? sender, EventArgs e)
        {
            AdvanceOneGeneration();
            if (_population == 0) { StopRun(); }
            if (_genchanges == 0) { StopRun(); }
            if ((_generation % 200) == 0) { StopRun(); }
        }

        // private void ClockFire(object sender, EventArgs e)
        // {
        //     DateTime noo = DateTime.Now;
        //     textblockClock.Text = noo.ToShortTimeString();
        // }

        private void PaintRed(int ox, int oy, PatternEnvelope envelop)
        {
            for (int h = 0; h < envelop.FrameXDimension; h++)
            {
                int hh = PositionOnBoard(ox + h, DimensionX);
                for (int v = 0; v < envelop.FrameYDimension; v++)
                {
                    int vv = PositionOnBoard(oy + v, DimensionY);
                    if (envelop.CellValue(h, v) == PatternEnvelope.liveCell) { _square[hh, vv].Fill = Brushes.Red; }
                    //if (envelop.CellValue(h, v) == classPatternEnvelope.deadCell) { square[hh, vv].Fill = Brushes.MistyRose; }
                }
            }
        }

        private void DisplayPopulation()
        {
            _population = 0;
            for (int x = 0; x < DimensionX; x++)
            {
                for (int y = 0; y < DimensionY; y++)
                {
                    if (_cell[x, y])
                    {
                        _square[x, y].Fill = Brushes.DarkGreen;
                        _population++;
                    }
                    else if ((Core.Instance.CellDisplay == Core.CellDisplayOptions.EverLive) && _everbeen[x, y])
                    {
                        _square[x, y].Fill = _brushEverlive;
                    }
                    else if ((Core.Instance.CellDisplay== Core.CellDisplayOptions.Affected) && _affected[x,y])
                    {
                        _square[x, y].Fill = _brushAffected;
                    }
                    else
                    {
                        _square[x, y].Fill = _brushBackground; 
                    }
                }
            }
            textblockGeneration.Text = $"Generation {_generation}";
            textblockPopulation.Text = $"Population {_population}";
            textblockGenChanges.Text = $"Changed cells {_genchanges}";
        }

        private void InitialiseSquares()
        {
            for (int x = 0; x < DimensionX; x++)
            {
                for (int y = 0; y < DimensionY; y++)
                {
                    Rectangle r = new Rectangle
                    {
                        Width = Blocksize,
                        Height = Blocksize,
                        Fill = Brushes.Ivory
                    };
                    Canvas.SetLeft(r, x * Blocksize);
                    Canvas.SetTop(r, y * Blocksize);
                    canvasPond.Children.Add(r);
                    _square[x, y] = r;
                }
            }
        }

        // private string BlockKey(int x, int y)
        // {
        //     return $"X{x}Y{y}";
        // }

        private void Evolve(string rules)
        {
            // Interpret rule string
            int p = rules.IndexOf('/');
            if (p < 2) { throw new ArgumentException("Improper rule string"); }
            if (rules.Length < (p + 3)) { throw new ArgumentException("Improper rule string"); }
            if (rules[0] != 'B') { throw new ArgumentException("Improper rule string"); }
            if (rules[p + 1] != 'S') { throw new ArgumentException("Improper rule string"); }
            
            string survivors = rules.Substring(p + 2);
            string births = rules.Substring(1, p - 1);

            for (int x = 0; x < DimensionX; x++)
            {
                for (int y = 0; y < DimensionY; y++)
                {
                    if (_affected[x, y])
                    {
                        CountNeighbours(x, y); // The CountNeighbours method takes account of whether wrapping occurs or not
                    }
                }
            }

            // Amend cells according to number of neighbours
            _genchanges = 0;
            for (int x = 0; x < DimensionX; x++)
            {
                for (int y = 0; y < DimensionY; y++)
                {
                    if (_affected[x, y])
                    {
                        if (_cell[x, y])
                        {
                            if (!survivors.Contains(_neighbours[x, y].ToString())) { _cell[x, y] = false; _genchanges++; MarkNeighbours(x, y); }
                        }
                        else
                        {
                            if (births.Contains(_neighbours[x, y].ToString())) { _cell[x, y] = true; _genchanges++; MarkNeighbours(x, y); }
                        }
                    }
                }
            }
            for (int x = 0; x < DimensionX; x++)
            {
                for (int y = 0; y < DimensionY; y++)
                {
                    _affected[x, y] = _affectedNow[x, y];
                    _affectedNow[x, y] = false;
                }
            }
            _generation++;
        }

        private void CountNeighbours(int px, int py)
        {
            int voisins = 0;
            // Examine the cells adjacent to (but not including) [x, y] (wrapping if necessary)
            if (Core.Instance.Wrapping)
            {
                for (int dx = px - 1; dx <= px + 1; dx++)
                {
                    var xx = PositionOnBoard(dx, DimensionX);
                    {
                        for (int dy = py - 1; dy <= py + 1; dy++)
                        {
                            var yy = PositionOnBoard(dy, DimensionY);
                            {
                                if (!((dx == px) && (dy == py)))
                                {
                                    if (_cell[xx, yy]) { voisins++; }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int dx = px - 1; dx <= px + 1; dx++)
                {
                    if (IsOnBoard(dx, DimensionX))
                    {
                        for (int dy = py - 1; dy <= py + 1; dy++)
                        {
                            if (IsOnBoard(dy, DimensionY))
                            {
                                if (!((dx == px) && (dy == py)))
                                {
                                    if (_cell[dx, dy]) { voisins++; }
                                }
                            }
                        }
                    }
                }
            }
            _neighbours[px, py] = voisins;
        }

        private void MarkNeighbours(int px, int py)
        {
            // Mark the cells adjacent to and including [x, y] (wrapping if necessary) as affected now
            if (Core.Instance.Wrapping)
            {
                for (int dx = px - 1; dx <= px + 1; dx++)
                {
                    var xx = PositionOnBoard(dx, DimensionX);
                    {
                        for (int dy = py - 1; dy <= py + 1; dy++)
                        {
                            var yy = PositionOnBoard(dy, DimensionY);
                            {
                                _affectedNow[xx, yy] = true;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int dx = px - 1; dx <= px + 1; dx++)
                {
                    if (IsOnBoard(dx, DimensionX))
                    {
                        for (int dy = py - 1; dy <= py + 1; dy++)
                        {
                            if (IsOnBoard(dy, DimensionY))
                            {
                                _affectedNow[dx, dy] = true;
                            }
                        }
                    }
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _tmr.Stop();
        }

        /// <summary>
        /// Allows calculation of location on board of a value which wraps
        /// </summary>
        /// <param name="posn"></param> // the input value
        /// <param name="stride"></param> // the size of the board
        /// <returns></returns>
        private int PositionOnBoard(int posn, int stride)
        {
            return (stride + posn) % stride;
        }

        /// <summary>
        /// Whether a location is on board which does not wrap
        /// </summary>
        /// <param name="posn"></param> // the input value
        /// <param name="stride"></param> // the size of the board
        /// <returns></returns>
        private bool IsOnBoard(int posn, int stride)
        {
            return ((posn >= 0) && (posn < stride));
        }

        private void StopRun()
        {
            _tmr.Stop();
            buttonRun.IsEnabled = true;
            buttonRunStop.IsEnabled = false;
            buttonStep.IsEnabled = true;
            buttonClose.IsEnabled = true;
        }

        private void InitialiseRun()
        {
            _generation = 1;
            DisplayPopulation();
            InitialiseAffectedCells();
            buttonRun.IsEnabled = true;
            buttonRunStop.IsEnabled = false;
            buttonStep.IsEnabled = true;
        }

        private void InitialiseAffectedCells()
        {
            for (int x = 0; x < DimensionX; x++)
            {
                for (int y = 0; y < DimensionY; y++)
                {
                    _affected[x, y] = true;
                    _affectedNow[x, y] = false;
                }
            }
        }

        // private bool IsOn(RadioButton rb)
        // {
        //     return ((rb.IsChecked.HasValue) && (rb.IsChecked.Value));
        // }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void mnuGameOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow w = new OptionsWindow
            {
                Owner = this
            };
            w.ShowDialog();
            DisplayOptions();
        }

        private void mnuObjectRandom_Click(object sender, RoutedEventArgs e)
        {
            Random rng = new Random();
            int seed = rng.Next();
            DoRandom(seed);
        }
        
        private void buttonStep_Click(object sender, RoutedEventArgs e)
        {
            AdvanceOneGeneration();
        }

        private void mnuObjectCreate_Click(object sender, RoutedEventArgs e)
        {
            ObjectEditor w = new ObjectEditor
            {
                Owner = this
            };
            this.Hide();
            w.ShowDialog();
            this.Show();
        }

        private void mnuGameClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonClose_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            mnuGame.IsEnabled = buttonClose.IsEnabled;
            mnuObjects.IsEnabled = buttonClose.IsEnabled;
            mnuRandom.IsEnabled = buttonClose.IsEnabled;
            mnuWeb.IsEnabled = buttonClose.IsEnabled;
        }

        private void mnuObjectRandomSeed_Click(object sender, RoutedEventArgs e)
        {
            var wib = new InputBox(boxTitle: "Random number generator",promptText: "Specify an integer seed",defaultResponse:_randomseed.ToString())
                {
                    Owner = this
                };
            if (wib.ShowDialog() == false) { return; }

            if (int.TryParse(wib.ResponseText, out var seed))
            {
                _randomseed = seed;
                DoRandom(seed);
            }
            else
            {
                MessageBox.Show("You did not enter a valid integer", "Incorrect input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void mnuObjectSelect_Click(object sender, RoutedEventArgs e)
        {
            ObjectSelectWindow w = new ObjectSelectWindow
            {
                Owner = this
            };
            if (w.ShowDialog() == false) { return; }
            string objectKey = w.SelectedKey;
            Shape? thing = Core.Instance.ObjectForKey(objectKey);
            if (thing is { })
            {
                int xp = (DimensionX - thing.Width) / 2;
                int yp = (DimensionY - thing.Height) / 2;
                DrawObject(thing, xp, yp);
            }
        }

        private void mnuObjectEdit_Click(object sender, RoutedEventArgs e)
        {
            ObjectSelectWindow wos = new ObjectSelectWindow
            {
                Owner = this
            };
            if (wos.ShowDialog() == false) { return; }
            string objectKey = wos.SelectedKey;
            // Shape thing = Core.Instance.ObjectForKey(objectKey);

            ObjectEditor woe = new ObjectEditor(objectKey)
            {
                Owner = this
            };
            this.Hide();
            woe.ShowDialog();
            this.Show();
        }

        private void mnuWebWiki_Click(object sender, RoutedEventArgs e)
        {
            var webaddress = "http://conwaylife.com/wiki/Main_Page";
            System.Diagnostics.ProcessStartInfo pinfo = new(webaddress) {UseShellExecute = true};
            System.Diagnostics.Process.Start(pinfo);
        }

        private void mnuWebLexicon_Click(object sender, RoutedEventArgs e)
        {
            var webaddress = "http://www.bitstorm.org/gameoflife/lexicon/";
            System.Diagnostics.ProcessStartInfo pinfo = new(webaddress) {UseShellExecute = true};
            System.Diagnostics.Process.Start(pinfo);
        }

        private void buttonRun_Click(object sender, RoutedEventArgs e)
        {
            _tmr.Start();
            buttonStep.IsEnabled = false;
            buttonClose.IsEnabled = false;
            buttonRun.IsEnabled = false;
            buttonRunStop.IsEnabled = true;
            textblockObjectDetails.Text = string.Empty;
            textblockObjectDetails.Foreground = Brushes.Black;
        }

        private void buttonRunStop_Click(object sender, RoutedEventArgs e)
        {
            StopRun();
        }

        private void mnuObjectRleImport_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "Run Length Encoding files (*.rle)|*.rle"
                ,
                Multiselect = false
                ,
                RestoreDirectory = false
                ,
                Title = "Import object from RLE"
            };
            if (ofd.ShowDialog() != true) { return; }
            string rleFile = ofd.FileName;
            Shape? thing = Core.Instance.LoadPatternFromRleFile(rleFile);
            if (thing == null)
            {
                MessageBox.Show("Error importing object from RLE file", "Game of Life", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                ObjectEditor woe = new ObjectEditor(thing)
                {
                    Owner = this
                };
                this.Hide();
                woe.ShowDialog();
                this.Show();
            }
        }

        private void BuildStillLifeList()
        {
            listboxStillLives.Items.Clear();
            Core.Instance.DoBuildStillLifeList();
            foreach (StillLife slf in Core.Instance.StillLifes)
            {
                //for(int o = 0; o < slf.VariantCount; o++)
                //{
                    listboxStillLives.Items.Add(StillLifeCanvas(slf, 0));
                //}
            }
        }

        private Canvas StillLifeCanvas(StillLife sl, int ver)
        {
            Pattern p = sl.Variant[ver];
            int patternmargin = 100;
            Canvas cvs = new Canvas
            {
                Width = 160,
                Height = Math.Max(32, 12 + 6 * p.YDimension)
                ,
                Background = Brushes.AliceBlue
            };

            TextBlock tb = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Width = patternmargin-3
                ,
                Text = $"{sl.SerialNumber}: {sl.Title}"
                ,
                Foreground = Brushes.DarkBlue
            };
            Canvas.SetLeft(tb, 4);
            Canvas.SetTop(tb, 6);
            cvs.Children.Add(tb);

            Rectangle[,] sq = new Rectangle[p.XDimension, p.YDimension];
            
            for (int h = 0; h < p.XDimension; h++)
            {
                for (int v = 0; v < p.YDimension; v++)
                {
                    sq[h, v] = new Rectangle
                    {
                        Width = 6,
                        Height = 6
                        ,
                        Stroke = Brushes.CornflowerBlue
                    };
                    char q = p.Code[p.XDimension * v + h];
                    if (q.Equals('O'))
                    {
                        sq[h, v].Fill = Brushes.SteelBlue;
                        sq[h, v].StrokeThickness = 1;
                    }
                    else if (q.Equals('.'))
                    {
                        sq[h, v].Fill = Brushes.AliceBlue;
                        sq[h, v].StrokeThickness = 0.3;
                    }
                    else
                    {
                        throw new ArgumentException("Check pattern spec for illegal character!");
                    }
                    Canvas.SetLeft(sq[h, v], patternmargin + h * 6);
                    Canvas.SetTop(sq[h, v], 6 + v * 6);
                    cvs.Children.Add(sq[h, v]);
                }
            }
            return cvs;
        }

        private void mnuAnalyseStillLifes_Click(object sender, RoutedEventArgs e)
        {
            // TODO Work out why this does not pick up Beehive still lifes
            buttonClose.IsEnabled = false;
            buttonRun.IsEnabled = false;
            buttonStep.IsEnabled = false;
            UiServices.SetBusyState();
            int[] slcount = new int[1 + Core.Instance.StillLifes.Count];

            foreach (StillLife slf in Core.Instance.StillLifes)
            {
                slcount[slf.SerialNumber] = 0;
            }

            for (int h = 0; h < DimensionX; h++)
            {
                for (int v = 0; v < DimensionY; v++)
                {
                    var ticked = false;
                    foreach (StillLife slf in Core.Instance.StillLifes)
                    {
                        for (int vnt = 0; vnt < slf.VariantCount; vnt++)
                        {
                            PatternEnvelope env = new PatternEnvelope(slf.Variant[vnt]);
                            Pattern? p = PutativePattern(h, v, env.FrameXDimension, env.FrameYDimension);
                            if (p is{})
                            {
                                if (env.IsCompatibleWith(p))
                                {
                                    // increment count for this still-life
                                    slcount[slf.SerialNumber]++;
                                    ticked = true;
                                    // make red on grid
                                    PaintRed(h, v, env);
                                }
                            }
                            if (ticked) { break; }  // Do not consider any more variants of this still life at this position
                        }
                        if (ticked) { break; }  // Do not consider any more still lifes at this position
                    }
                }
            }
            string msg = "Common Still Lifes:\n";
            foreach (StillLife slf in Core.Instance.StillLifes)
            {
                int q = slf.SerialNumber;
                if (slcount[q] > 0)
                {
                    msg += "\n";
                    msg += $"#{q} {slf.Title}: {slcount[q]}";
                }
            }
            textblockObjectDetails.Text = msg;
            textblockObjectDetails.Foreground = Brushes.Red;
            buttonClose.IsEnabled = true;
            buttonRun.IsEnabled = true;
            buttonStep.IsEnabled = true;
        }

        private Pattern? PutativePattern(int ox, int oy, int dx, int dy)
        {
            // Copy the pattern with top left at [ox, oy] and dimensions dx * dy
            if (Core.Instance.Wrapping)
            {
                string code = string.Empty;
                for (int y = 0; y < dy; y++)
                {
                    var yy = PositionOnBoard(oy + y, DimensionY);
                    {
                        for (int x = 0; x < dx; x++)
                        {
                            var xx = PositionOnBoard(ox + x, DimensionX);
                            {
                                if (_cell[xx, yy]) { code += 'O'; } else { code += '.'; }
                            }
                        }
                    }
                }
                Pattern putain = new Pattern(dx, dy, code);
                return putain;
            }
            else
            {
                string code = string.Empty;
                bool overboard = false;
                for (int y = 0; y < dy; y++)
                {
                    if (IsOnBoard(oy + y, DimensionY))
                    {
                        for (int x = 0; x < dx; x++)

                        {
                            if (IsOnBoard(ox + x, DimensionX))
                            {
                                if (_cell[ox + x, oy + y]) { code += 'O'; } else { code += '.'; }
                            }
                            else
                            {
                                overboard = true;
                            }
                        }
                    }
                    else
                    {
                        overboard = true;
                    }
                }
                Pattern? putain;
                putain = overboard ? null : new Pattern(dx, dy, code);
                return putain;
            }
        }
    }
}