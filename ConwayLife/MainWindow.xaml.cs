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
            _cellule = new LifeCell[DimensionX, DimensionY];
            _square = new Rectangle[DimensionX, DimensionY];
            for (var xx = 0; xx < DimensionX; xx++)
            {
                for (var yy = 0; yy < DimensionY; yy++)
                {
                    _cellule[xx, yy] = new LifeCell();
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
           Apart from the classes of object demonstrated in this implementation, later discoveries included other "guns",
           which are stationary, and which shoot out gliders or other spaceships; "puffers", which move along leaving 
           behind a trail of debris; and "rakes", which move and emit spaceships.[23] Gosper also constructed the first 
           pattern with an asymptotically optimal quadratic growth rate, called a "breeder", or "lobster", which worked 
           by leaving behind a trail of guns.

           It is possible for gliders to interact with other objects in interesting ways. For example, if two gliders 
           are shot at a block in just the right way, the block will move closer to the source of the gliders. If three
           gliders are shot in just the right way, the block will move farther away. This "sliding block memory" can be 
           used to simulate a counter. It is possible to construct logic gates such as AND, OR and NOT using gliders. 
           It is possible to build a pattern that acts like a finite state machine connected to two counters. This has 
           the same computational power as a universal Turing machine, so the Game of Life is theoretically as powerful 
           as any computer with unlimited memory and no time constraints: it is Turing complete.

           Furthermore, a pattern can contain a collection of guns that fire gliders in such a way as to construct new objects, 
           including copies of the original pattern. A "universal constructor" can be built which contains a Turing complete 
           computer, and which can build many types of complex objects, including more copies of itself.
           
         */
        private readonly LifeCell[,] _cellule;
        private readonly Rectangle[,] _square;
        private const int DimensionX=148;
        private const int DimensionY=148;
        private const int Blocksize = 5;
        private int _generation;
        private int _population;
        private int _genchanges;
        private int _randomseed = 11267;
        private readonly DispatcherTimer _tmr;

        private readonly SolidColorBrush _brushBackground = Brushes.Ivory;
        private readonly SolidColorBrush _brushAffected = Brushes.Moccasin;
        //private readonly SolidColorBrush _brushEverlive = Brushes.Yellow;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var scrX = SystemParameters.PrimaryScreenWidth;
            var scrY = SystemParameters.PrimaryScreenHeight;
            var winX = scrX * .98;
            var winY = scrY * .94;
            var xm = (scrX - winX) / 2;
            var ym = (scrY - winY) / 4;
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
            BuildStillLifeList();
            DisplayOptions();
            InitialiseSquares();
            DisplayPopulation();
            mnuGame.IsEnabled = true;
            mnuRandom.IsEnabled = true;
            mnuObjects.IsEnabled = true;
            mnuWeb.IsEnabled = true;
            buttonClose.IsEnabled = true;
            _tmr.Stop();
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
            else if (Core.Instance.CellDisplay == Core.CellDisplayOptions.Immigration)
            {
                textblockShowEver.Text = "Immigration colouring";
            }
            else if (Core.Instance.CellDisplay == Core.CellDisplayOptions.QuadLife)
            {
                textblockShowEver.Text = "QuadLife colouring";
            }
            else
            {
                textblockShowEver.Text = "No highlighting";
            }

            textblockRules.Text = $"Rules: {Core.Instance.RuleString}";
        }

        private void ClearCells()
        {
            for (var x = 0; x < DimensionX; x++)
            {
                for (var y = 0; y < DimensionY; y++)
                {
                    _cellule[x, y] = new LifeCell();
                    _square[x, y].Fill = Brushes.Ivory;
                }
            }
        }

        private void DrawObject(Shape thing, int oX, int oY)
        {
            textblockPattern.Text =$"{thing.Name} ({thing.Kind})";
            textblockDiscovery.Text = $"Discovered by {thing.Discoverer} in {thing.DiscoveryYear}";
            textblockObjectDetails.Text = thing.Comment;
            ClearCells();
            for (var x=0;x< thing.Width; x++)
            {
                for (var y = 0; y < thing.Height; y++)
                {
                    if (thing.CellValue(x, y))
                    {
                        _cellule[oX + x, oY + y].Awaken(Core.Instance.CellDisplay);
                    }
                }
            }
            InitialiseRun();
        }

        private void DoRandom(int grain)
        {
            ClearCells();
            textblockPattern.Text = "Random pattern";
            var rng = new Random();
            if (grain >=0) { rng = new Random(grain); textblockPattern.Text = $"Random pattern [{grain}]"; }
            for (var x = 0; x < DimensionX; x++)
            {
                for (var y = 0; y < DimensionY; y++)
                {
                    var q = (rng.NextDouble() > 0.8);
                    if (q)
                    {
                        _cellule[x, y].Awaken(Core.Instance.CellDisplay);    
                    }
                    else
                    {
                        _cellule[x, y].Kill();
                    }
                }
            }
            InitialiseRun();
        }
        
        private void AdvanceOneGeneration()
        {
            if (Core.Instance.CellDisplay == Core.CellDisplayOptions.Immigration)
            {
                EvolveImmigration();
            }
            else if (Core.Instance.CellDisplay == Core.CellDisplayOptions.QuadLife)
            {
                EvolveQuadLife();
            }
            else
            {
                Evolve(Core.Instance.RuleString);    
            }
            
            DisplayPopulation();
        }

        private void TimerFire(object? sender, EventArgs e)
        {
            AdvanceOneGeneration();
            if (_population == 0) { StopRun(); }
            if (_genchanges == 0) { StopRun(); }
            if ((_generation % 200) == 0) { StopRun(); }
        }

        private void PaintRed(int ox, int oy, PatternEnvelope envelop)
        {
            for (var h = 0; h < envelop.FrameXDimension; h++)
            {
                var hh = PositionOnBoard(ox + h, DimensionX);
                for (var v = 0; v < envelop.FrameYDimension; v++)
                {
                    var vv = PositionOnBoard(oy + v, DimensionY);
                    if (envelop.CellValue(h, v) == PatternEnvelope.liveCell) { _square[hh, vv].Fill = Brushes.Red; }
                }
            }
        }

        private void DisplayPopulation()
        {
            _population = 0;
            for (var x = 0; x < DimensionX; x++)
            {
                for (var y = 0; y < DimensionY; y++)
                {
                    switch (Core.Instance.CellDisplay)
                    {
                        case Core.CellDisplayOptions.Life:
                        {
                            if (_cellule[x, y].IsAlive)
                            {
                                _square[x, y].Fill = Brushes.DarkGreen;
                                _population++;
                            }
                            else
                            {
                                _square[x, y].Fill = _brushBackground;        
                            }
                            break;
                        }
                        
                        case Core.CellDisplayOptions.Affected:
                        {
                            if (_cellule[x, y].IsAlive)
                            {
                                _square[x, y].Fill = Brushes.DarkGreen;
                                _population++;
                            }
                            else if ( _cellule[x, y].JustTouched)        
                            {
                                _square[x, y].Fill = _brushAffected;        
                            }
                            else
                            {
                                _square[x, y].Fill = _brushBackground;        
                            }
                            break;
                        }
                        
                        case Core.CellDisplayOptions.EverLive:
                        {
                            if (_cellule[x, y].IsAlive)
                            {
                                _square[x, y].Fill = Brushes.DarkGreen;
                                _population++;
                            }
                            else if ( _cellule[x, y].EverAlive)        
                            {
                                _square[x, y].Fill = _brushAffected;        
                            }
                            else
                            {
                                _square[x, y].Fill = _brushBackground;        
                            }
                            break;
                        }
                        
                        case Core.CellDisplayOptions.Immigration:
                        {
                            if (_cellule[x, y].IsAlive)
                            {
                                _square[x, y].Fill =ImmigrationBrush(_cellule[x,y].ColourValue);
                                _population++;
                            }
                            else
                            {
                                _square[x, y].Fill = _brushBackground;        
                            }
                            break;
                        }
                        
                        case Core.CellDisplayOptions.QuadLife:
                        {
                            if (_cellule[x, y].IsAlive)
                            {
                                _square[x, y].Fill =ImmigrationBrush(_cellule[x,y].ColourValue);
                                _population++;
                            }
                            else
                            {
                                _square[x, y].Fill = _brushBackground;        
                            }
                            
                            break;
                        }
                        
                    }
                }
            }
            textblockGeneration.Text = $"Generation {_generation}";
            textblockPopulation.Text = $"Population {_population}";
            textblockGenChanges.Text = $"Changed cells {_genchanges}";
        }

        private SolidColorBrush ImmigrationBrush(int v)
        {
            if (v == 1)
            {
                return new SolidColorBrush(Colors.Crimson);
            }
            if (v == 2)
            {
                return new SolidColorBrush(Colors.Blue);
            }
            if (v == 3)
            {
                return new SolidColorBrush(Colors.Magenta);
            }
            if (v == 4)
            {
                return new SolidColorBrush(Colors.ForestGreen);
            }
            return new SolidColorBrush(Colors.DimGray);
        }
        
        private void InitialiseSquares()
        {
            for (var x = 0; x < DimensionX; x++)
            {
                for (var y = 0; y < DimensionY; y++)
                {
                    var r = new Rectangle
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
       
        private void Evolve(string rules)
        {
            // Interpret rule string
            var p = rules.IndexOf('/');
            if (p < 2) { throw new ArgumentException("Improper rule string"); }
            if (rules.Length < (p + 3)) { throw new ArgumentException("Improper rule string"); }
            if (rules[0] != 'B') { throw new ArgumentException("Improper rule string"); }
            if (rules[p + 1] != 'S') { throw new ArgumentException("Improper rule string"); }
            
            var toSurvive = rules[(p + 2)..];
            var toBeBorn = rules.Substring(1, p - 1);

            for (var x = 0; x < DimensionX; x++)
            {
                for (var y = 0; y < DimensionY; y++)
                {
                    CountNeighbours(x, y, toSurvive, toBeBorn); 
                    // The CountNeighbours method takes account of whether wrapping occurs or not
                }
            }

            // Amend cells according to number of neighbours
            _genchanges = 0;
            for (var x = 0; x < DimensionX; x++)
            {
                for (var y = 0; y < DimensionY; y++)
                {
                    if (_cellule[x, y].Evolve())
                    {
                        _genchanges++;
                        MarkNeighbours(x, y);
                    }
                }
            }
           
            _generation++;
        }
        
        private void EvolveImmigration()
        {
            for (var x = 0; x < DimensionX; x++)
            {
                for (var y = 0; y < DimensionY; y++)
                {

                    CountNeighboursImmigration(x, y); 
                    // The CountNeighbours method takes account of whether wrapping occurs or not
                }
            }

            // Amend cells according to number of neighbours
            _genchanges = 0;
            for (var x = 0; x < DimensionX; x++)
            {
                for (var y = 0; y < DimensionY; y++)
                {
                    if (_cellule[x, y].Evolve())
                    {
                        _genchanges++;
                        MarkNeighbours(x, y);
                    }
                }
            }
            
            _generation++;
        }
        
        private void EvolveQuadLife()
        {
            for (var x = 0; x < DimensionX; x++)
            {
                for (var y = 0; y < DimensionY; y++)
                {
                    CountNeighboursQuadLife(x, y); 
                    // The CountNeighbours method takes account of whether wrapping occurs or not
                }
            }

            // Amend cells according to number of neighbours
            _genchanges = 0;
            for (var x = 0; x < DimensionX; x++)
            {
                for (var y = 0; y < DimensionY; y++)
                {
                    if (_cellule[x, y].Evolve())
                    {
                        _genchanges++;
                        MarkNeighbours(x, y);
                    }
                }
            }
           
            _generation++;
        }
        
        private void CountNeighboursImmigration(int px, int py)
        {
            var voisins = 0;
            var colourCount = new int[3];
            
            // Examine the cells adjacent to (but not including) [x, y] (wrapping if necessary)
            if (Core.Instance.Wrapping)
            {
                for (var dx = px - 1; dx <= px + 1; dx++)
                {
                    var xx = PositionOnBoard(dx, DimensionX);
                    {
                        for (var dy = py - 1; dy <= py + 1; dy++)
                        {
                            var yy = PositionOnBoard(dy, DimensionY);
                            {
                                if (!((dx == px) && (dy == py)))
                                {
                                    if (_cellule[xx, yy].IsAlive)
                                    {
                                        voisins++;
                                        colourCount[_cellule[xx, yy].ColourValue]++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (var dx = px - 1; dx <= px + 1; dx++)
                {
                    if (IsOnBoard(dx, DimensionX))
                    {
                        for (var dy = py - 1; dy <= py + 1; dy++)
                        {
                            if (IsOnBoard(dy, DimensionY))
                            {
                                if (dx == px && dy == py) continue;
                                if (!_cellule[dx, dy].IsAlive) continue;
                                voisins++;
                                colourCount[_cellule[dx, dy].ColourValue]++;
                            }
                        }
                    }
                }
            }

            _cellule[px, py].DefaultToNoChange();
            int majorityTint;
            if (colourCount[1] > colourCount[2])
            {
                majorityTint = 1;
            }
            else
            {
                majorityTint = 2;
            }
            
            if (_cellule[px, py].IsAlive)
            {
                if (voisins is not(2 or 3))
                {
                    _cellule[px, py].Kill();
                }
            }
            else
            {
                if (voisins==3)
                {
                    _cellule[px, py].Awaken(majorityTint); // bring to life with appropriate colour
                }
            }
        }

        private void CountNeighboursQuadLife(int px, int py)
        {
            var voisins = 0;
            var colourCount = new int[5];
            int chosenTint;
            // Examine the cells adjacent to (but not including) [x, y] (wrapping if necessary)
            if (Core.Instance.Wrapping)
            {
                for (var dx = px - 1; dx <= px + 1; dx++)
                {
                    var xx = PositionOnBoard(dx, DimensionX);
                    {
                        for (var dy = py - 1; dy <= py + 1; dy++)
                        {
                            var yy = PositionOnBoard(dy, DimensionY);
                            {
                                if (!((dx == px) && (dy == py)))
                                {
                                    if (_cellule[xx, yy].IsAlive)
                                    {
                                        voisins++;
                                        colourCount[_cellule[xx, yy].ColourValue]++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (var dx = px - 1; dx <= px + 1; dx++)
                {
                    if (!IsOnBoard(dx, DimensionX)) continue;
                    for (var dy = py - 1; dy <= py + 1; dy++)
                    {
                        if (IsOnBoard(dy, DimensionY))
                        {
                            if (dx == px && dy == py) continue;
                            if (!_cellule[dx, dy].IsAlive) continue;
                            voisins++;
                            colourCount[_cellule[dx, dy].ColourValue]++;
                        }
                    }
                }
            }

            _cellule[px, py].DefaultToNoChange();
            var singles = 0;
            var other = 0;
            var majority = 1;
            for (var a = 1; a < 5; a++)
            {
                if (colourCount[a] == 1)
                {
                    singles++;
                }
                else
                {
                    other = a;
                }

                if (colourCount[a] > colourCount[majority])
                {
                    majority = a;
                }
            }

            // three colours the same - take the fourth (otherwise take the majority colour)
            
            if (_cellule[px, py].IsAlive)
            {
                if (voisins is not (2 or 3))
                {
                    _cellule[px, py].Kill();
                }
            }
            else
            {
                if (voisins == 3)
                {
                    if (singles == 3)
                    {
                        chosenTint = other;
                    }
                    else
                    {
                        chosenTint = majority;
                    }

                    _cellule[px, py].Awaken(chosenTint); // bring to life with appropriate colour
                }
            }
        }
    
        private void CountNeighbours(int px, int py, string survivors, string midwives)
        {
            var voisins = 0;
            // Examine the cells adjacent to (but not including) [x, y] (wrapping if necessary)
            if (Core.Instance.Wrapping)
            {
                for (var dx = px - 1; dx <= px + 1; dx++)
                {
                    var xx = PositionOnBoard(dx, DimensionX);
                    {
                        for (var dy = py - 1; dy <= py + 1; dy++)
                        {
                            var yy = PositionOnBoard(dy, DimensionY);
                            {
                                if (!((dx == px) && (dy == py)))
                                {
                                    if (_cellule[xx, yy].IsAlive)
                                    {
                                        voisins++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (var dx = px - 1; dx <= px + 1; dx++)
                {
                    if (IsOnBoard(dx, DimensionX))
                    {
                        for (var dy = py - 1; dy <= py + 1; dy++)
                        {
                            if (IsOnBoard(dy, DimensionY))
                            {
                                if (!((dx == px) && (dy == py)))
                                {
                                    if (_cellule[dx, dy].IsAlive)
                                    {
                                        voisins++;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            _cellule[px, py].DefaultToNoChange();
            if (_cellule[px, py].IsAlive)
            {
                if (!survivors.Contains(voisins.ToString()))
                {
                    _cellule[px, py].Kill();
                }
            }
            else
            {
                if (midwives.Contains(voisins.ToString()))
                {
                    _cellule[px, py].Awaken(1); // bring to life
                }
            }
        }

        private void MarkNeighbours(int px, int py)
        {
            // Mark the cells adjacent to and including [x, y] (wrapping if necessary) as affected now
            if (Core.Instance.Wrapping)
            {
                for (var dx = px - 1; dx <= px + 1; dx++)
                {
                    var xx = PositionOnBoard(dx, DimensionX);
                    {
                        for (var dy = py - 1; dy <= py + 1; dy++)
                        {
                            var yy = PositionOnBoard(dy, DimensionY);
                            {
                                _cellule[xx, yy].AffectedNow = true;
                            }
                        }
                    }
                }
            }
            else
            {
                for (var dx = px - 1; dx <= px + 1; dx++)
                {
                    if (IsOnBoard(dx, DimensionX))
                    {
                        for (var dy = py - 1; dy <= py + 1; dy++)
                        {
                            if (IsOnBoard(dy, DimensionY))
                            {
                                _cellule[dx, dy].AffectedNow = true;
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
            for (var x = 0; x < DimensionX; x++)
            {
                for (var y = 0; y < DimensionY; y++)
                {
                    _cellule[x, y].JustTouched = true;
                    _cellule[x, y].AffectedNow = false;
                }
            }
        }
      
        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void mnuGameOptions_Click(object sender, RoutedEventArgs e)
        {
            var w = new OptionsWindow
            {
                Owner = this
            };
            w.ShowDialog();
            DisplayOptions();
        }

        private void mnuObjectRandom_Click(object sender, RoutedEventArgs e)
        {
            var rng = new Random();
            var seed = rng.Next();
            DoRandom(seed);
        }
        
        private void buttonStep_Click(object sender, RoutedEventArgs e)
        {
            AdvanceOneGeneration();
        }

        private void mnuObjectCreate_Click(object sender, RoutedEventArgs e)
        {
            var w = new ObjectEditor
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
            var w = new ObjectSelectWindow
            {
                Owner = this
            };
            if (w.ShowDialog() == false) { return; }
            var objectKey = w.SelectedKey;
            var thing = Core.Instance.ObjectForKey(objectKey);
            if (thing is { })
            {
                var xp = (DimensionX - thing.Width) / 2;
                var yp = (DimensionY - thing.Height) / 2;
                DrawObject(thing, xp, yp);
            }
        }

        private void mnuObjectEdit_Click(object sender, RoutedEventArgs e)
        {
            var wos = new ObjectSelectWindow
            {
                Owner = this
            };
            if (wos.ShowDialog() == false) { return; }
            var objectKey = wos.SelectedKey;
            // Shape thing = Core.Instance.ObjectForKey(objectKey);

            var woe = new ObjectEditor(objectKey)
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
            var ofd = new Microsoft.Win32.OpenFileDialog
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
            var rleFile = ofd.FileName;
            var thing = Core.Instance.LoadPatternFromRleFile(rleFile);
            if (thing == null)
            {
                MessageBox.Show("Error importing object from RLE file", "Game of Life", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var woe = new ObjectEditor(thing)
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
            foreach (var slf in Core.Instance.StillLifes)
            {
                //for(int o = 0; o < slf.VariantCount; o++)
                //{
                    listboxStillLives.Items.Add(StillLifeCanvas(slf, 0));
                //}
            }
        }

        private Canvas StillLifeCanvas(StillLife sl, int ver)
        {
            var p = sl.Variant[ver];
            var patternmargin = 100;
            var cvs = new Canvas
            {
                Width = 160,
                Height = Math.Max(32, 12 + 6 * p.YDimension)
                ,
                Background = Brushes.AliceBlue
            };

            var tb = new TextBlock
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

            var sq = new Rectangle[p.XDimension, p.YDimension];
            
            for (var h = 0; h < p.XDimension; h++)
            {
                for (var v = 0; v < p.YDimension; v++)
                {
                    sq[h, v] = new Rectangle
                    {
                        Width = 6,
                        Height = 6
                        ,
                        Stroke = Brushes.CornflowerBlue
                    };
                    var q = p.Code[p.XDimension * v + h];
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
            var slcount = new int[1 + Core.Instance.StillLifes.Count];

            foreach (var slf in Core.Instance.StillLifes)
            {
                slcount[slf.SerialNumber] = 0;
            }

            for (var h = 0; h < DimensionX; h++)
            {
                for (var v = 0; v < DimensionY; v++)
                {
                    var ticked = false;
                    foreach (var slf in Core.Instance.StillLifes)
                    {
                        for (var vnt = 0; vnt < slf.VariantCount; vnt++)
                        {
                            var env = new PatternEnvelope(slf.Variant[vnt]);
                            var p = PutativePattern(h, v, env.FrameXDimension, env.FrameYDimension);
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
            var msg = "Common Still Lifes:\n";
            foreach (var slf in Core.Instance.StillLifes)
            {
                var q = slf.SerialNumber;
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
                var code = string.Empty;
                for (var y = 0; y < dy; y++)
                {
                    var yy = PositionOnBoard(oy + y, DimensionY);
                    {
                        for (var x = 0; x < dx; x++)
                        {
                            var xx = PositionOnBoard(ox + x, DimensionX);
                            {
                                if (_cellule[xx, yy].IsAlive) { code += 'O'; } else { code += '.'; }
                            }
                        }
                    }
                }
                var putain = new Pattern(dx, dy, code);
                return putain;
            }
            else
            {
                var code = string.Empty;
                var overboard = false;
                for (var y = 0; y < dy; y++)
                {
                    if (IsOnBoard(oy + y, DimensionY))
                    {
                        for (var x = 0; x < dx; x++)
                        {
                            if (IsOnBoard(ox + x, DimensionX))
                            {
                                if (_cellule[ox + x, oy + y].IsAlive) { code += 'O'; } else { code += '.'; }
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