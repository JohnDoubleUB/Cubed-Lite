using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class LevelData
{
    public DateTime DateOfAttemptUTC = DateTime.UtcNow;
    public int Attempts = 0;
    public int HintsUsed = 0;

    public LevelData() { }

    //Date is initialized to the date of the instantiation
    public LevelData(int Attempts, int HintsUsed) 
    {
        DateOfAttemptUTC = DateTime.UtcNow;
        this.Attempts = Attempts;
        this.HintsUsed = HintsUsed;
    }
}