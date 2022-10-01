using System;

namespace ConwayLife;

public class LifeCell
{
    public LifeCell()
    {
        Neighbours = 0;
        EverBeen = false;
        Touched = false;
        AffectedNow = false;
        ColourValue = 0;    
    }

    public int Neighbours { get; set; }

    public bool Alive => ColourValue>0;

    public void Kill()
    {
        ColourValue = 0;
    }

    public void Awaken(int immigrationTint)
    {
        ColourValue = immigrationTint;
    }
    
    public void Awaken(Core.CellDisplayOptions option)
    {
        var generator = new Random();
        if (option == Core.CellDisplayOptions.Immigration)
        {
            ColourValue =1+ generator.Next(2);    
        }
        else if (option == Core.CellDisplayOptions.QuadLife)
        {
            ColourValue =1+ generator.Next(4);
        }
        else
        {
            ColourValue = 1;
        }
    }
    
    public bool EverBeen { get; private set; }
    public bool AffectedNow { get; set; }
    public bool Touched { get; set; }
    public int  ColourValue { get; set; }
    
}