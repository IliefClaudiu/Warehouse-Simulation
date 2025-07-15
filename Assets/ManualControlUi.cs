using UnityEngine;

public class ManualControlUI : MonoBehaviour
{
    public ForkliftController forklift;
    public GameManager GameManager;

    public void MoveForward() => forklift.SetHeldDirection("F");
    public void MoveBackward() => forklift.SetHeldDirection("B");
    public void TurnLeft() => forklift.SetHeldDirection("L");
    public void TurnRight() => forklift.SetHeldDirection("R");
    public void Stop() => forklift.SetHeldDirection(null);

    public void SwitchToAutoMode()
    {
        GameManager.SetManualMode(false);
    }
}