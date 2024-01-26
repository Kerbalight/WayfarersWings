namespace WayfarersWings.Managers.Analyzers;

public struct VesselParachutesState
{
    public int deployedCount = 0;
    public int stowedCount = 0;
    public int semiDeployedCount = 0;
    public int activeCount = 0;
    public int cutCount = 0;

    public VesselParachutesState() { }

    public bool DidUseParachutes => deployedCount > 0 || semiDeployedCount > 0 || activeCount > 0 || cutCount > 0;
}