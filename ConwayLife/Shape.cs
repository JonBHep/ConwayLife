using System;

namespace ConwayLife;

public class Shape : IComparable<Shape>
{
    private int xdim;
    private int ydim;
    private bool[,] cell;
    private string objectcomment;
    private string objectdiscoverer;
    private string objectdiscoveryyear;
    private string objectkind;
    private string objectname;

    public Shape(int width, int height)
    {
        xdim = width;
        ydim = height;
        cell = new bool[xdim, ydim];
        objectcomment = string.Empty;
        objectdiscoverer = string.Empty;
        objectkind = string.Empty;
        objectname = string.Empty;
    }

    public string Name
    {
        get { return objectname; }
        set { objectname = value; }
    }

    public string Kind
    {
        get { return objectkind; }
        set { objectkind = value; }
    }

    public string Discoverer
    {
        get { return objectdiscoverer; }
        set { objectdiscoverer = value; }
    }

    public string DiscoveryYear
    {
        get { return objectdiscoveryyear; }
        set { objectdiscoveryyear = value; }
    }

    public string Comment
    {
        get { return objectcomment; }
        set { objectcomment = value; }
    }

    private string sortString
    {
        get { return $"{objectkind}:{objectname}"; }
    }

    public string CellData
    {
        get
        {
            string q = string.Empty;
            for (int y = 0; y < ydim; y++)
            {
                for (int x = 0; x < xdim; x++)
                {
                    if (cell[x, y])
                    {
                        q += "1";
                    }
                    else
                    {
                        q += "0";
                    }
                }
            }

            return q;
        }
        set
        {
            int x = -1, y = 0;
            char[] code = value.ToCharArray();
            foreach (char q in code)
            {
                x++;
                if (x == xdim)
                {
                    x = 0;
                    y++;
                }

                cell[x, y] = (q == '1');
            }
        }
    }

    public int Width
    {
        get { return xdim; }
    }

    public int Height
    {
        get { return ydim; }
    }

    public int LargestDimension
    {
        get { return Math.Max(xdim, ydim); }
    }

    public bool CellValue(int x, int y)
    {
        return cell[x, y];
    }

    public void SetCellValue(int x, int y, bool live)
    {
        cell[x, y] = live;
    }

    public int CompareTo(Shape other)
    {
        return string.Compare(sortString, other.sortString, StringComparison.Ordinal);
    }

}