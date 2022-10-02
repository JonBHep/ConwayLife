using System;

namespace ConwayLife;

public class Pattern
{
    private readonly int _dimensionX;
    private readonly int _dimensionY;
    private readonly string _patternCode;

    public Pattern(int x, int y, string pattern)
    {
        _patternCode = pattern;
        _dimensionX = x;
        _dimensionY = y;
        if (pattern.Length != (_dimensionX * _dimensionY))
        {
            throw new ArgumentException(
                $"Pattern specification is wrong length!\n{pattern.Length} should be {_dimensionX * _dimensionY}");
        }
    }

    public int XDimension => _dimensionX;

    public int YDimension => _dimensionY;

    public string Code => _patternCode;

    public bool IsSameAs(Pattern other)
    {
        if (other._dimensionX != this._dimensionX)
        {
            return false;
        }

        if (other._dimensionY != this._dimensionY)
        {
            return false;
        }

        if (!other._patternCode.Equals(this._patternCode))
        {
            return false;
        }

        return true;
    }

    private char CellChar(int x, int y)
    {
        return _patternCode[_dimensionX * y + x];
    }

    public bool CellLive(int x, int y)
    {
        return (CellChar(x, y) == 'O');
    }

    public Pattern RotatedRight
    {
        get
        {
            string code = string.Empty;
            for (int nx = 0; nx < _dimensionX; nx++)
            {
                for (int ny = _dimensionY - 1; ny >= 0; ny--)
                {
                    code += CellChar(nx, ny);
                }
            }

            Pattern retVal = new Pattern(_dimensionY, _dimensionX, code);
            return retVal;
        }
    }

    public Pattern FlippedHorizontal
    {
        get
        {
            string code = string.Empty;
            for (int ny = 0; ny < _dimensionY; ny++)
            {
                for (int nx = _dimensionX - 1; nx >= 0; nx--)
                {
                    code += CellChar(nx, ny);
                }
            }

            Pattern retVal = new Pattern(_dimensionX, _dimensionY, code);
            return retVal;
        }
    }
}