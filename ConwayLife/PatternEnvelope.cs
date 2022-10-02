namespace ConwayLife;

public class PatternEnvelope
{
    public const int LiveCell = 1;
    public const int DeadCell = 2;
    public const int NullCell = 3;

    private readonly int _patternDimensionX;
    private readonly int _patternDimensionY;
    private int _frameDimensionX;
    private int _frameDimensionY;
    private readonly bool[,] _patternCells;
    private int[,] _frameCells;

    public PatternEnvelope(Pattern corePattern)
    {
        _patternDimensionX = corePattern.XDimension;
        _patternDimensionY = corePattern.YDimension;
        _patternCells = new bool[_patternDimensionX, _patternDimensionY];
        _frameCells = new int[1, 1]; // will be re-dimensioned later
        int n = -1;
        for (int y = 0; y < _patternDimensionY; y++)
        {
            for (int x = 0; x < _patternDimensionX; x++)
            {
                n++;
                if (corePattern.Code[n] == 'O')
                {
                    _patternCells[x, y] = true;
                }
                else
                {
                    _patternCells[x, y] = false;
                }
            }
        }

        BuildFrame();
    }

    public int FrameXDimension
    {
        get { return _frameDimensionX; }
    }

    public int FrameYDimension
    {
        get { return _frameDimensionY; }
    }

    public int CellValue(int x, int y)
    {
        return _frameCells[x, y];
    }

    /// <summary>
    /// Locates the core pattern within a rectangle two units wider and deeper. The neighbours of  the pattern's live cells are required to be dead, while any other cells within the frame can be dead or alive
    /// </summary>
    public void BuildFrame()
    {
        _frameDimensionX = _patternDimensionX + 2;
        _frameDimensionY = _patternDimensionY + 2;
        _frameCells = new int[_frameDimensionX, _frameDimensionY];
        // set all cells of frame to null
        for (int fy = 0; fy < _frameDimensionY; fy++)
        {
            for (int fx = 0; fx < _frameDimensionX; fx++)
            {
                _frameCells[fx, fy] = NullCell;
            }
        }

        // set live cells in pattern to live
        for (int py = 0; py < _patternDimensionY; py++)
        {
            for (int px = 0; px < _patternDimensionX; px++)
            {
                if (_patternCells[px, py])
                {
                    _frameCells[px + 1, py + 1] = LiveCell;
                }
            }
        }

        // Set all neighbours of live cells to dead
        // This creates a snug sheath of dead cells around the pattern while allowing any remaining null cells within the frame to be either live or dead
        for (int py = 0; py < _frameDimensionY; py++)
        {
            for (int px = 0; px < _frameDimensionX; px++)
            {
                if (_frameCells[px, py] == LiveCell)
                {
                    // We have a live cell, so enumerate its neighbours
                    for (int nx = px - 1; nx <= (px + 1); nx++)
                    {
                        for (int ny = py - 1; ny <= (py + 1); ny++)
                        {
                            // neighbour of live cell: if not itself live, it must be dead
                            if (_frameCells[nx, ny] != LiveCell)
                            {
                                _frameCells[nx, ny] = DeadCell;
                            }
                        }
                    }
                }
            }
        }
    }

    public bool IsCompatibleWith(Pattern testPattern)
    {
        bool hypothesis = true;
        for (int py = 0; py < _frameDimensionY; py++)
        {
            for (int px = 0; px < _frameDimensionX; px++)
            {
                if (_frameCells[px, py] == LiveCell)
                {
                    // test pattern must also have live cell
                    if (!testPattern.CellLive(px, py))
                    {
                        hypothesis = false;
                    }
                }
                else if (_frameCells[px, py] == DeadCell)
                {
                    // test pattern must also have dead cell
                    if (testPattern.CellLive(px, py))
                    {
                        hypothesis = false;
                    }
                }
                // else frame cell is null - no implications for test pattern
            }
        }

        return hypothesis;
    }
}