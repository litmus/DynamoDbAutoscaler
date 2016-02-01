namespace DynamoDbAutoscaler.Interfaces
{
    public interface IAutoscalingCalculator
    {
        long CalculateProvisionIncrease(long provisionedUnits, double increasePercent);
        long CalculateProvisionDecrease(long provisionedUnits, double decreasePercent);
        long EnsureProvisionInRange(long provisionedUnits, long minProvisioned, long maxProvisioned);
    }
}
