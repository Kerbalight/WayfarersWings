using JetBrains.Annotations;
using KSP.Messages;
using KSP.Sim.impl;
using WayfarersWings.Models.Wings;

namespace WayfarersWings.Models.Conditions;

[UsedImplicitly]
public class SphereOfInfluenceCondition : CelestialBodyCondition
{
    public override bool IsValid(Transaction transaction)
    {
        return transaction.Vessel?.mainBody?.Equals(CelestialBody) ?? false;

        if (transaction.Message is not SOIEnteredMessage message)
        {
            return transaction.Vessel?.mainBody?.Equals(CelestialBody) ?? false;
        }

        if (message.bodyEntered.isHomeWorld && message.bodyExited.isHomeWorld)
        {
            //TODO DOuble check bodyExited, is it null?
            return false;
        }

        return message.bodyEntered.Equals(CelestialBody) && !message.bodyExited.Equals(CelestialBody);
    }
}