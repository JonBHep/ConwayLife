using System.Collections.Generic;

namespace ConwayLife;

public class StillLife
{
    private int _serial;
    private Pattern _basepattern;
    private string _name;
    private List<Pattern> _variant = new List<Pattern>();

    public StillLife(int serial, string name, int width, int height, string pattern)
    {
        _serial = serial;
        _name = name;
        _basepattern = new Pattern(width, height, pattern);
        BuildVariantList();
    }

    public List<Pattern> Variant
    {
        get { return _variant; }
    }

    public int VariantCount
    {
        get { return _variant.Count; }
    }

    public int SerialNumber
    {
        get { return _serial; }
    }

    public string Title
    {
        get { return _name; }
    }

    private void BuildVariantList()
    {
        _variant.Add(_basepattern);

        Pattern p2 = _basepattern.RotatedRight;
        bool already = false;
        foreach (Pattern v in _variant)
        {
            if (v.IsSameAs(p2))
            {
                already = true;
                break;
            }
        }

        if (!already)
        {
            _variant.Add(p2);
        }

        Pattern p3 = p2.RotatedRight;
        already = false;
        foreach (Pattern v in _variant)
        {
            if (v.IsSameAs(p3))
            {
                already = true;
                break;
            }
        }

        if (!already)
        {
            _variant.Add(p3);
        }

        Pattern p4 = p3.RotatedRight;
        already = false;
        foreach (Pattern v in _variant)
        {
            if (v.IsSameAs(p4))
            {
                already = true;
                break;
            }
        }

        if (!already)
        {
            _variant.Add(p4);
        }

        Pattern p5 = _basepattern.FlippedHorizontal;
        already = false;
        foreach (Pattern v in _variant)
        {
            if (v.IsSameAs(p5))
            {
                already = true;
                break;
            }
        }

        if (!already)
        {
            _variant.Add(p5);
        }

        Pattern p6 = p5.RotatedRight;
        already = false;
        foreach (Pattern v in _variant)
        {
            if (v.IsSameAs(p6))
            {
                already = true;
                break;
            }
        }

        if (!already)
        {
            _variant.Add(p6);
        }

        Pattern p7 = p6.RotatedRight;
        already = false;
        foreach (Pattern v in _variant)
        {
            if (v.IsSameAs(p7))
            {
                already = true;
                break;
            }
        }

        if (!already)
        {
            _variant.Add(p7);
        }

        Pattern p8 = p7.RotatedRight;
        already = false;
        foreach (Pattern v in _variant)
        {
            if (v.IsSameAs(p8))
            {
                already = true;
                break;
            }
        }

        if (!already)
        {
            _variant.Add(p8);
        }

    }
}