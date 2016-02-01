using DynamoDbAutoscaler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoDbAutoscaler
{
    internal class NullCircuitBreaker : ICircuitBreaker
    {
        public Task<bool> IsTrippedAsync()
        {
            return Task.FromResult(false);
        }
    }
}
