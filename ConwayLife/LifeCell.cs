namespace ConwayLife;

public class LifeCell
{
    public LifeCell()
    {
        _alive = false;
        Neighbours = 0;
        EverBeen = false;
        Affected = false;
        AffectedNow = false;
        ColourValue = 0;
    }

    public int Neighbours { get; set; }
    private bool _alive;
    public bool Alive
    {
        get
        {
            return _alive;
        }
        set
        {
            _alive = value;
            if (value)
            {
                EverBeen = true;
            }
        }
    }

    public bool EverBeen { get; private set; }
    public bool AffectedNow { get; set; }
    public bool Affected { get; set; }
    public int ColourValue { get; set; }
}