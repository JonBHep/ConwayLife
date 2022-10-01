using System;
using System.Collections.Generic;

namespace ConwayLife;

public sealed class Core
{
    // Core is implemented as a Singleton
        // Thus only a single instance can be created and this instance can be accessed globally
        // Also, because the class is sealed, no other class can imherit from it

        // TODO check the cost of checking for lack of change or for return to first position

        // TODO allow edit of description only?
        // TODO importing and exporting other file formats than RLE files
        // TODO allow user selection of display colours
        // TODO allow editor to scroll for large objects, or for dimensions to object requirements

        private static readonly Core Instantiation = new Core();

        private Core()
        {
            // the constructor is private thus preventing instances other than the single private instance from being created
            _optionsfilespec = System.IO.Path.Combine(Jbh.AppManager.DataPath, "Options.txt");
            if (System.IO.File.Exists(_optionsfilespec)) { RetrieveOptions(); }
            _objectsfilespec = System.IO.Path.Combine(Jbh.AppManager.DataPath, "Objects.txt");
            if (System.IO.File.Exists(_objectsfilespec)) { LoadObjects(); }
        }

        public static Core Instance => Instantiation; // this static property allows global access to the single private instance of classVerbaCore

        public enum CellDisplayOptions { None, EverLive, Affected, Immigration, QuadLife};

        private bool _wrapping;
        private CellDisplayOptions _showwhat;
        private string _rulestring = "B3/S23";

        private readonly string _optionsfilespec;
        private readonly string _objectsfilespec;
        public const int ObjectDimension = 128;

        private readonly List<StillLife> _stilllifes = new List<StillLife>();

        public void DoBuildStillLifeList()
        {
            BuildStillLifeList();
        }

        private readonly Dictionary<string, Shape> _dictionaryOfObjects = new Dictionary<string, Shape>();

        ~Core()
        {
            StoreOptions();
            StoreObjects();
        }

        public List<StillLife> StillLifes => _stilllifes;

        private void BuildStillLifeList()
        {
            _stilllifes.Clear();

            _stilllifes.Add(new StillLife(1, "Block", 2, 2, "OOOO"));
            _stilllifes.Add(new StillLife(2, "Beehive", 4, 3, ".OO.O..O.OO."));
            _stilllifes.Add(new StillLife(3, "Loaf", 4, 4, ".OO.O..O.O.O..O."));
            _stilllifes.Add(new StillLife(4, "Boat", 3, 3, "OO.O.O.O."));
            _stilllifes.Add(new StillLife(5, "Tub", 3, 3, ".O.O.O.O."));

            _stilllifes.Add(new StillLife(6, "Pond", 4, 4, ".OO.O..OO..O.OO."));
            _stilllifes.Add(new StillLife(7, "Ship", 3, 3, "OO.O.O.OO"));
            _stilllifes.Add(new StillLife(8, "Long boat", 4, 4, "OO..O.O..O.O..O."));
            _stilllifes.Add(new StillLife(9, "Ship tie", 6, 6, "OO....O.O....OO......OO....O.O....OO"));
            _stilllifes.Add(new StillLife(10, "Barge", 4, 4, ".O..O.O..O.O..O."));

            _stilllifes.Add(new StillLife(11, "Bi-loaf 1", 7, 7, ".O.....O.O....O..O....OO.O.....O.O....O..O....OO."));
            _stilllifes.Add(new StillLife(12, "Mango", 5, 4, ".OO..O..O..O..O..OO."));
            _stilllifes.Add(new StillLife(13, "Eater 1", 4, 4, "OO..O.O...O...OO"));
            _stilllifes.Add(new StillLife(14, "Long barge", 5, 5, ".O...O.O...O.O...O.O...O."));
            _stilllifes.Add(new StillLife(15, "Aircraft carrier", 4, 3, "OO..O..O..OO"));

            _stilllifes.Add(new StillLife(16, "Paperclip", 5, 6, "..OO..O..O.O.OOOO.O.O..O..OO.."));
            _stilllifes.Add(new StillLife(17, "Long ship", 4, 4, "OO..O.O..O.O..OO"));
            _stilllifes.Add(new StillLife(18, "Shillelagh", 5, 3, "OO...O..OO.OO.O"));
            _stilllifes.Add(new StillLife(19, "Integral sign", 5, 5, "...OO..O.O..O..O.O..OO..."));
            _stilllifes.Add(new StillLife(20, "Boat tie", 6, 6, ".O....O.O....OO......OO....O.O....O."));

            _stilllifes.Add(new StillLife(21, "Snake", 4, 2, "OO.OO.OO"));
            _stilllifes.Add(new StillLife(22, "Big S", 7, 6, "....OO....O..O...O.OOOO.O...O..O....OO...."));
            _stilllifes.Add(new StillLife(23, "Bi-pond", 7, 7, ".OO....O..O...O..O....OO.OO....O..O...O..O....OO."));
            _stilllifes.Add(new StillLife(24, "Trans-boat with tail", 5, 5, "OO...O.O...O.O....O....OO"));
            _stilllifes.Add(new StillLife(25, "Hat", 5, 4, "..O...O.O..O.O.OO.OO"));

            _stilllifes.Add(new StillLife(26, "Boat-ship tie", 6, 6, "OO....O.O....OO......OO....O.O....O."));
            _stilllifes.Add(new StillLife(27, "Long long boat", 5, 5, ".O...O.O...O.O...O.O...OO"));
            _stilllifes.Add(new StillLife(28, "Tub with tail", 5, 5, ".O...O.O...O.O....O....OO"));
            _stilllifes.Add(new StillLife(29, "Long long ship", 5, 5, "OO...O.O...O.O...O.O...OO"));
            _stilllifes.Add(new StillLife(30, "Table on table", 4, 5, "O..OOOOO....OOOOO..O"));

            _stilllifes.Add(new StillLife(31, "Canoe", 5, 5, "...OO....O...O.O.O..OO..."));
            _stilllifes.Add(new StillLife(32, "Cis-mirrored R-bee", 7, 4, ".OO.OO.O.O.O.OO.O.O.O.O...O."));
            _stilllifes.Add(new StillLife(33, "Moose antlers", 9, 5, "OO.....OOO.......O.OOO.OOO....O.O.......O...."));
            _stilllifes.Add(new StillLife(34, "Block on table", 4, 5, "..OO..OO....OOOOO..O"));
            _stilllifes.Add(new StillLife(35, "Block and dock", 6, 6, "...OO....OO........OOOO.O....OOO..OO"));

            _stilllifes.Add(new StillLife(36, "Twin hat", 9, 5, "..O...O...O.O.O.O..O.O.O.O.OO.O.O.OO....O...."));
            _stilllifes.Add(new StillLife(37, "Beehive and dock", 6, 7, "...OO...O..O...OO........OOOO.O....OOO..OO"));
            _stilllifes.Add(new StillLife(38, "Beehive with tail", 6, 5, ".OO...O..O...OO.O.....O.....OO"));
            _stilllifes.Add(new StillLife(39, "Scorpion", 7, 6, "...O....OOO...O...OO.O.O.O.O.OO.O.O.....O."));
            _stilllifes.Add(new StillLife(40, "Loop", 5, 4, ".OO..O..O..O.O.OO.OO"));

            _stilllifes.Add(new StillLife(41, "Fourteener", 7, 5, "....OO.OO..O.OO.....O.OOOOO....O..."));
            _stilllifes.Add(new StillLife(42, "Cis-boat with tail", 5, 5, "...OO...O.OO.O.O.O...O..."));
            _stilllifes.Add(new StillLife(43, "Long snake", 5, 3, "OO...O.O.O...OO"));
            _stilllifes.Add(new StillLife(44, "Trans-mirrored R-bee", 7, 5, ".....O..OO.O.OO.O.O.OO.O.OO..O....."));
            _stilllifes.Add(new StillLife(45, "Dead spark coil", 7, 5, "OO...OOO.O.O.O..O.O..O.O.O.OOO...OO"));

            _stilllifes.Add(new StillLife(46, "Bookends", 7, 4, "OO...OOO.O.O.O..O.O...OO.OO."));
            _stilllifes.Add(new StillLife(47, "Elevener", 6, 6, "....OO...O.O...O...OOO..O.....OO...."));
            _stilllifes.Add(new StillLife(48, "Block and cap", 4, 6, ".OO.O..OOOOO....OO..OO.."));
            _stilllifes.Add(new StillLife(49, "Trans-loaf with tail", 6, 6, "OO.....O.....O.OO...O..O...O.O....O."));
            _stilllifes.Add(new StillLife(50, "Cis-rotated hook", 7, 5, ".OO......O..OOO.O.O.OOO..O......OO."));

            _stilllifes.Add(new StillLife(51, "Trans-block and long hook", 5, 6, "...OOO...OOOOO......OO...OO..."));
            _stilllifes.Add(new StillLife(52, "Cis-shillelagh", 6, 5, "....OO.....OOO..O.O..O...OO..."));
            _stilllifes.Add(new StillLife(53, "Mirrored dock", 6, 7, "OO..OOO....O.OOOO........OOOO.O....OOO..OO"));
            _stilllifes.Add(new StillLife(54, "Carrier siamese snake", 7, 3, "OO.OO..O.OO..O.....OO"));
            _stilllifes.Add(new StillLife(55, "Trans-hook and R-bee", 5, 7, "...OO.O..O.OOO.......OOO.O..O..OO.."));

            _stilllifes.Add(new StillLife(56, "Block and two tails", 5, 5, "OO.OOOO.O....O.OOO..O...."));
            _stilllifes.Add(new StillLife(57, "Cis-boat and dock", 6, 7, "..O....O.O...OO..........OOOO.O....OOO..OO"));
            _stilllifes.Add(new StillLife(58, "Eater siamese eater", 4, 7, "OO..O.O...O...OO...OOOO.O..."));
            _stilllifes.Add(new StillLife(59, "Cis-block and long hook", 5, 6, "...OOO...OOOOO........OO...OO."));
            _stilllifes.Add(new StillLife(60, "Long long snake", 6, 4, "OO....O.O......O.O....OO"));

            _stilllifes.Add(new StillLife(61, "Integral with tub", 6, 6, "OO....O.O.....O.....O.O....O.O....O."));
            _stilllifes.Add(new StillLife(62, "Long shillelagh", 6, 3, "OO..OOO..O.O.OO..."));
            _stilllifes.Add(new StillLife(63, "Trans-R-bee and R-loaf", 5, 8, "..OO..O..O.OOO.......OOO.O..O.O.O...O..."));
            _stilllifes.Add(new StillLife(64, "Boat with long tail", 6, 4, "OO....O.O....O..OO..OO.O"));
            _stilllifes.Add(new StillLife(65, "Cis-hook and R-bee", 4, 7, "..OOO..OOOO.....OOO.O..O.OO."));

        }

        private void StoreOptions()
        {
            using System.IO.StreamWriter sw = new System.IO.StreamWriter(_optionsfilespec);
            sw.WriteLine(_wrapping ? "Wrapping=T" : "Wrapping=F");
            if (_showwhat== CellDisplayOptions.Affected)
            {
                sw.WriteLine("HighLight=A");
            }
            else if (_showwhat == CellDisplayOptions.EverLive)
            {
                sw.WriteLine("HighLight=E");
            }
            else
            {
                sw.WriteLine("HighLight=N");
            }
        }

        private void RetrieveOptions()
        {
            using System.IO.StreamReader sr=new System.IO.StreamReader(_optionsfilespec);
            while (!sr.EndOfStream)
            {
                string? red = sr.ReadLine();
                if (red is { } j)
                {
                    if (j.StartsWith("Wrapping="))
                    {
                        _wrapping = j.EndsWith("T");
                    }
                    if (j.StartsWith("HighLight="))
                    {
                        if (j.EndsWith("A"))
                        { _showwhat = CellDisplayOptions.Affected; }
                        else if (j.EndsWith("E"))
                        { _showwhat = CellDisplayOptions.EverLive; }
                        else
                        { _showwhat = CellDisplayOptions.None; }
                    }    
                }
            }
        }
        
        public bool Wrapping { get => _wrapping;
            set => _wrapping = value;
        }

        public CellDisplayOptions CellDisplay { get => _showwhat;
            set => _showwhat = value;
        }
        
        public string RuleString { get => _rulestring;
            set => _rulestring = value;
        }

        private void SortObjects()
        {
            // sort objects by kind and description
            List<Shape> shapes = new List<Shape>();
            foreach (Shape thing in _dictionaryOfObjects.Values)
            {
                shapes.Add(thing);
            }
            shapes.Sort();

            _dictionaryOfObjects.Clear();

            foreach (Shape thing in shapes)
            {
                string newKey = FreshKey();
                _dictionaryOfObjects.Add(newKey, thing);
            }
        }

        private List<string> SplitLines(string source, int maxLineLength)
        {
            List<string> lines = new List<string>();
            while (source.Length > 0)
            {
                if (source.Length <= maxLineLength)
                {
                    lines.Add(source);
                    source = string.Empty;
                }
                else
                {
                    string block = source.Substring(0, maxLineLength);
                    int spacepoint = block.LastIndexOfAny(" -".ToCharArray());
                    if (spacepoint < 1)
                    {
                        // space or hyphen either not found or found in position 0, so just use the whole block
                        lines.Add(block);
                        source = source.Substring(block.Length);
                    }
                    else
                    {
                        // split at position of space or hyphen
                        block = block.Substring(0, spacepoint);
                        lines.Add(block);
                        source = source.Substring(block.Length);
                        source = source.Trim();
                    }
                }
            }
            return lines;
        }

        public void SavePatternAsRleFile(string filepath, Shape thing)
        {
            using System.IO.StreamWriter sw = new System.IO.StreamWriter(filepath);
            sw.WriteLine("#N " + thing.Name);
            sw.WriteLine("#C Kind = " + thing.Kind);
            if (!string.IsNullOrWhiteSpace(thing.Discoverer)) { sw.WriteLine("#C Discoverer = " + thing.Discoverer); }
            if (!string.IsNullOrWhiteSpace(thing.DiscoveryYear)) { sw.WriteLine("#C Discovered = " + thing.DiscoveryYear); }
            sw.WriteLine($"#O Jonathan Hepworth jonathanhepworth@hotmail.co.uk {DateTime.Now:dd MMM yyyy}");
            if (!string.IsNullOrWhiteSpace(thing.Comment))
            {
                List<string> comments = SplitLines(thing.Comment, 67);
                foreach (string s in comments)
                {
                    sw.WriteLine("#C " + s);
                }
            }
            sw.WriteLine($"x = {thing.Width}, y = {thing.Height}, rule = B3/S23");

            List<string> linecode = new List<string>();
            char lineEndMarker = '$';
            for (int y = 0; y < thing.Height; y++)
            {
                if (y == thing.Height - 1) { lineEndMarker = '!'; } // For the final line, the end marker is the end-of-code marker '!'
                string linespec = string.Empty;
                for (int x = 0; x < thing.Width; x++)
                {
                    if (thing.CellValue(x, y)) { linespec += "o"; } else { linespec += "b"; }
                }
                // Trim dead cells from end of line
                while (linespec.EndsWith("b"))
                {
                    linespec = linespec.Substring(0, linespec.Length - 1);
                }
                // Add an end-of-line marker
                linespec += lineEndMarker;
                linecode.Add(linespec);
            }

            // Concatenate code lines and then apply run length encoding
            string allLines = string.Empty;
            foreach (string code in linecode) { allLines += code; }
                
            int currentPosition = 0;
            char currentChar = allLines[currentPosition];
            int charCount = 1;
            string rleCode = string.Empty;
            while (currentPosition < allLines.Length - 1)
            {
                currentPosition++;
                char nextChar = allLines[currentPosition];
                if (nextChar.Equals(currentChar))
                {
                    charCount++;
                }
                else
                {
                    if (charCount == 1) { rleCode += currentChar; } else { rleCode += $"{charCount}{currentChar}"; }
                    currentChar = nextChar;
                    charCount = 1;
                }
            }
            rleCode += currentChar; // This will be '!'
            // Divide final code according to line length limit of 70

            List<string> linesToWrite = new List<string>();
            for (int p = 0; p < rleCode.Length; p += 70)
            {
                string c = rleCode.Substring(p);
                if (c.Length > 70) { c = c.Substring(0, 70); }
                linesToWrite.Add(c);
            }
            foreach (string c in linesToWrite)
            {
                sw.WriteLine(c);
            }
        }

        public Shape? LoadPatternFromRleFile(string filepath)
        {
            // All RLE file lines should be max length 70, although it is not safe for an interpreter to assume this.

            bool fFlag = false; // flag will be turned on by a formatting error
            if (string.IsNullOrWhiteSpace(filepath)) { return null; }
            if (!System.IO.File.Exists(filepath)) { return null; }
            Shape gotObject= new Shape(8,8);
            string jObjectName = string.Empty;
            string jObjectComment = string.Empty;
            string jObjectDiscoverer = string.Empty;
            string jObjectDiscoveryYear = string.Empty;
            string jObjectKind = string.Empty;
            int currentx = 0, currenty = 0;
            List<string> instructions = new List<string>();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(filepath))
            {
                while (!sr.EndOfStream)
                {
                    string? red = sr.ReadLine();
                    if (red is { } jline)
                    {
                        jline = jline.Trim();
                    if (jline.StartsWith("#N"))
                    {
                        jObjectName = jline.Substring(2).Trim();
                    }
                    else if (jline.StartsWith("#C Kind = "))
                    {
                        jObjectKind = jline.Substring(10).Trim();
                    }
                    else if (jline.StartsWith("#C Discoverer = "))
                    {
                        jObjectDiscoverer = jline.Substring(16).Trim();
                    }
                    else if (jline.StartsWith("#C Discovered = "))
                    {
                        jObjectDiscoveryYear = jline.Substring(16).Trim();
                    }
                    else if (jline.StartsWith("#C "))
                    {
                        jObjectComment = jObjectComment.Trim();
                        jObjectComment += " " + jline.Substring(3).Trim();
                    }
                    // Ignore any comment line apart from those above
                    else if (!jline.StartsWith("#"))
                    {
                        // Line which sets x and y dimensions of object
                        if (jline.StartsWith("x"))
                        {
                            int x = 0, y = 0;
                            int pa = jline.IndexOf('=');
                            int pb = jline.IndexOf(',');
                            if (pa < 0) { fFlag = true; }
                            if (pb < 0) { fFlag = true; }
                            if (pa > pb) { fFlag = true; }
                            if (!fFlag)
                            {
                                string nval = jline.Substring(pa + 1, pb - pa - 1).Trim();
                                if (!int.TryParse(nval, out x)) { fFlag = true; }
                                jline = jline.Substring(pb + 1).Trim();
                            }
                            if (!jline.StartsWith("y"))
                            {
                                fFlag = true;
                            }
                            else
                            {
                                pa = jline.IndexOf('=');
                                pb = jline.IndexOf(',');
                                if (pa < 0) { fFlag = true; }
                                // If there is no comma after "y" then use end of line instead
                                if (pb < 0)
                                {
                                    pb = jline.Length - 1;
                                }
                                if (pa > pb) { fFlag = true; }
                                if (fFlag)
                                {
                                    break;
                                }
                                else
                                {
                                    string nval = jline.Substring(pa + 1, pb - pa - 1).Trim();
                                    if (!int.TryParse(nval, out y)) { fFlag = true; }
                                    // Ignore the remainder of the "x = " line, if any (sometimes used to specify the life version the object is designed for {e.g. Life or HighLife) which otherwise may be specified in a comment line, but which I am provisionally ignoring)
                                }
                            }
                            if (fFlag) { break; }
                            gotObject = new Shape(width: x, height: y)
                            {
                                Comment = jObjectComment,
                                Discoverer = jObjectDiscoverer
                                ,
                                DiscoveryYear = jObjectDiscoveryYear
                                ,
                                Kind = jObjectKind
                                ,
                                Name = jObjectName
                            };
                        }
                        else
                        {
                            // This is not a comment line and not the "x = ..." line, so it should be a pattern-specifying line
                            instructions.Add(jline.Trim());
                        }
                    }
                    if (fFlag) { break; }
                    if (jline.Contains("!")) { break; }// Don't read any further lines after a code termination character (!)    
                    }
                }
            }
            if (fFlag) { return null; }
            // Concatenate and then interpret the instruction lines
            string allInstructions = string.Empty;
            foreach (string s in instructions) { allInstructions += s; }

            // KEY    
            // b = dead cell
            // o = live cell
            // $ = line end
            // ! = end

            // First expand instructions by interpreting RLE for b and o and $
            string finalInstructions = string.Empty;
            while (allInstructions.Length > 0)
            {
                int p = allInstructions.IndexOfAny(anyOf: "bo$!".ToCharArray());
                char found = allInstructions[p];

                switch (found)
                {
                    case 'b':
                    case 'o':
                    case '$':
                        {
                            int n = 1;
                            if (p > 0)
                            {
                                string numb = allInstructions.Substring(0, p);
                                if (!int.TryParse(numb, out n))
                                {
                                    fFlag = true;
                                }
                            }
                            for (int j = 0; j < n; j++)
                            {
                                finalInstructions += found;
                            }
                            allInstructions = allInstructions.Substring(p + 1);
                            break;
                        }
                    case '!':
                        {
                            finalInstructions += found;
                            allInstructions = string.Empty;
                            break;
                        }
                    default:
                        {
                            fFlag = true;
                            break;
                        }
                }
            }

            int terminator = finalInstructions.IndexOf('!');
            if (terminator != (finalInstructions.Length - 1)) { fFlag = true; } // flag an error if the code terminator is not at the end of the string (and only there)

            foreach (var found in finalInstructions)
            {
                switch (found)
                {
                    case 'b':
                    {
                        gotObject.SetCellValue(currentx, currenty, false);
                        currentx++;
                        break;
                    }
                    case 'o':
                    {
                        gotObject.SetCellValue(currentx, currenty, true);
                        currentx++;
                        break;
                    }
                    case '$':
                    {
                        currentx = 0;
                        currenty++;
                        break;
                    }
                    case '!':
                    {
                        break;
                    }
                }
            }
            if (fFlag) { return null; }
            return gotObject;
        }

        private void LoadObjects()
        {
            string descrip;
            string objComt;
            _dictionaryOfObjects.Clear();
            using System.IO.StreamReader sr = new System.IO.StreamReader(_objectsfilespec);
            while (!sr.EndOfStream)
            {
                var red = sr.ReadLine();
                descrip = red ?? string.Empty;
                red = sr.ReadLine();
                
                var objKind = red ?? string.Empty;
                red = sr.ReadLine();
                objComt = red ?? string.Empty;
                red = sr.ReadLine();
                var objDisc = red ?? string.Empty;
                red = sr.ReadLine();
                var objYear = red ?? string.Empty;
                red = sr.ReadLine();
                var objSpec = red ?? string.Empty;

                int p = objSpec.IndexOf("+", StringComparison.Ordinal);
                string wdthString = objSpec.Substring(0, p);
                int wdth = int.Parse(wdthString);
                string celldata = objSpec.Substring(p + 1);
                int hght = celldata.Length / wdth;
                Shape thing = new Shape(wdth, hght)
                {
                    CellData = celldata,
                    Name = descrip
                    ,
                    Kind = objKind
                    ,
                    Comment = objComt
                    ,
                    Discoverer = objDisc
                    ,
                    DiscoveryYear = objYear
                };
                string newKey = FreshKey();
                _dictionaryOfObjects.Add(newKey, thing);
            }
        }

        private void StoreObjects()
        {
            using System.IO.StreamWriter sw = new System.IO.StreamWriter(_objectsfilespec);
            foreach (Shape thing in _dictionaryOfObjects.Values)
            {
                sw.WriteLine(thing.Name);
                sw.WriteLine(thing.Kind);
                sw.WriteLine(thing.Comment);
                sw.WriteLine(thing.Discoverer);
                sw.WriteLine(thing.DiscoveryYear);
                sw.WriteLine($"{thing.Width}+{thing.CellData}");
            }
        }

        public void AddNewObject(Shape nova)
        {
            string newKey = FreshKey();
            _dictionaryOfObjects.Add(newKey, nova);
            SortObjects();
        }

        public void DeleteObject(string key)
        {
            _dictionaryOfObjects.Remove(key);
        }

        private string FreshKey()
        {
            string u = RandomKey();
            while (_dictionaryOfObjects.ContainsKey(u))
            {
                u = RandomKey();
            }
            return u;
        }

        private string RandomKey()
        {
            string retVal = string.Empty;
            Random rng = new Random();
            for (int i = 0; i < 8; i++)
            {
                int c = rng.Next(minValue: 65, maxValue: 90);
                char ch = (char)c;
                retVal += ch;
            }
            return retVal;
        }

        public Shape? ObjectForKey(string key)
        {
            if (_dictionaryOfObjects.ContainsKey(key))
            {
                return _dictionaryOfObjects[key];
            }
            else
            {
                return null;
            }
        }

        public List<string> ListOfKeys()
        {
            List<string> lst = new List<string>();
            foreach (string k in _dictionaryOfObjects.Keys) { lst.Add(k); }
            return lst;
        }

        public List<string> ListOfGroups()
        {
            List<string> lst = new List<string>();
            foreach (string k in _dictionaryOfObjects.Keys)
            {
                var s = ObjectForKey(k);
                if (s is null) continue;
                if (!lst.Contains(s.Kind)) { lst.Add(s.Kind); }
            }
            lst.Sort();
            return lst;
        }
}