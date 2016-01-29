using System.Threading;
using System.Threading.Tasks;
using DynamoDbAutoScaler.Configuration;

namespace DynamoDbAutoScaler.Interfaces
{
    public interface IAutoscaler
    {
        Task EnsureProvisionAsync(
            GlobalAutoscalingConfigurationSet configurationSet,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
