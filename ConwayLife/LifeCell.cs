using System;

namespace ConwayLife;

public class LifeCell
{
    public LifeCell()
    {
        JustTouched = false;
        EverAlive = false;
        AffectedNow = false;
        ColourValue = 0;    
    }

    public bool IsAlive => ColourValue>0;

    public void Kill()
    {
        ColourToBeAssigned = 0;
    }

    public void Awaken(int tint)
    {
        EverAlive = true;
        ColourToBeAssigned=tint;
    }
    
    public void DefaultToNoChange()
    {
        ColourToBeAssigned=ColourValue;
    }

    public void Awaken(Core.CellDisplayOptions option)
    {
        EverAlive = true;
        
        if (option == Core.CellDisplayOptions.Immigration)
        {
            var generator = new Random();
            ColourValue = 1 + generator.Next(2);
            return;
        }
        if (option == Core.CellDisplayOptions.QuadLife)
        {
            var generator = new Random();
            ColourValue = 1 + generator.Next(4);
            return;
        }
        
        ColourValue = 1;
    }

    public bool EverAlive { get; private set; }
    public bool AffectedNow { get; set; }
    public bool JustTouched { get; set; }
    public int  ColourValue { get; private set; }
    private int  ColourToBeAssigned { get; set; }

    public bool Evolve()
    {
        bool changed = false;
        if (ColourValue != ColourToBeAssigned)
        {
            changed = true;
            ColourValue = ColourToBeAssigned;
        }

        JustTouched = AffectedNow;
        AffectedNow = false;
        return changed;
    }
    
}