using System.Threading;
using System.Threading.Tasks;
using DynamoDbAutoscaler.Configuration;

namespace DynamoDbAutoscaler.Interfaces
{
    public interface IAutoscaler
    {
        Task EnsureProvisionAsync(
            GlobalAutoscalingConfigurationSet configurationSet,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
