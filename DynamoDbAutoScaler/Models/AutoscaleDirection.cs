namespace DynamoDbAutoScaler.Models
{
    public enum AutoscaleDirection
    {
        UltraUp = 3,
        ExtraUp = 2,
        Up = 1,
        Stay = 0,
        Down = -1,
    }
}
