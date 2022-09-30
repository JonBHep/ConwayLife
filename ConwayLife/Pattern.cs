using System;

namespace ConwayLife;

public class Pattern
{
    private int _dimensionX;
        private int _dimensionY;
        private string patternCode;
        public Pattern(int x, int y, string pattern)
        {
            patternCode = pattern;
            _dimensionX = x;
            _dimensionY = y;
            if (pattern.Length != (_dimensionX * _dimensionY)) { throw new ArgumentException($"Pattern specification is wrong length!\n{pattern.Length} should be {_dimensionX * _dimensionY}"); }
        }
        public int XDimension { get { return _dimensionX; } }
        public int YDimension { get { return _dimensionY; } }
        public string Code { get { return patternCode; } }

        public bool IsSameAs(Pattern other)
        {
            if (other._dimensionX != this._dimensionX) { return false; }
            if (other._dimensionY != this._dimensionY) { return false; }
            if (!other.patternCode.Equals(this.patternCode)) { return false; }
            return true;
        }

        private char CellChar(int x, int y) { return patternCode[_dimensionX * y + x]; }

        public bool CellLive(int x, int y) { return (CellChar(x,y)=='O'); }

        public Pattern RotatedRight
        {
            get
            {
                string code = string.Empty;
                for (int nx = 0; nx < _dimensionX; nx++)
                {
                    for (int ny = _dimensionY-1; ny >=0; ny--)
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