using System.Collections.Generic;
using UnityEngine;

public class PinManager : MonoBehaviour
{
    public List<PinController> pins = new List<PinController>();

    public int GetFallenPins()
    {
        int count = 0;
        foreach (var p in pins)
        {
            if (p == null) continue;
            if (p.GetState() != PinController.PinState.Standing)
                count++;
        }
        return count;
    }

    public int GetTotalPins()
    {
        return pins.Count;
    }

    public void ResetPins()
    {
        foreach (var p in pins)
        {
            if (p == null) continue;
            p.ResetPin();
        }
    }
}
