using DynamoDbAutoscaler.Configuration;
using DynamoDbAutoscaler.Interfaces;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DynamoDbAutoscaler
{
    public class DynamoDbAutoscalerIntervalProvisioner
    {
        private readonly IAutoscaler autoScaler;
        private readonly System.Timers.Timer ensureMetricsAreProvisionedTimer;
        private readonly ILogger logger;
        private readonly IGlobalAutoscalingConfigurationSetFactory configurationSetFactory;

        public TimeSpan DemandCheckInterval { get; set; }

        public DynamoDbAutoscalerIntervalProvisioner(
            ILogger logger,
            IAutoscaler autoScaler, 
            IGlobalAutoscalingConfigurationSetFactory configurationSetFactory)
        {
            this.logger = logger;
            this.autoScaler = autoScaler;
            this.configurationSetFactory = configurationSetFactory;

            DemandCheckInterval = TimeSpan.FromMinutes(5); 
            ensureMetricsAreProvisionedTimer = new System.Timers.Timer();
            ensureMetricsAreProvisionedTimer.Elapsed += EnsureMetricsAreProvisionedTimer_Elapsed;
            ensureMetricsAreProvisionedTimer.Interval = DemandCheckInterval.TotalMilliseconds;
        }

        public DynamoDbAutoscalerIntervalProvisioner(ILogger logger) : 
            this(logger, new Autoscaler(logger), new LocalFileGlobalAutoscalingConfigurationSetFactory())
        {

        }

        private async void EnsureMetricsAreProvisionedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ensureMetricsAreProvisionedTimer.Interval = DemandCheckInterval.TotalMilliseconds;

            try
            {
                using (var cts = new CancellationTokenSource(Convert.ToInt32(DemandCheckInterval.TotalMilliseconds * 0.8)))
                {
                    var configuration = await configurationSetFactory.LoadConfiguration();
                    await autoScaler.EnsureProvisionAsync(configuration, cts.Token);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error ensuring dynamodb provisioning");
            }
        }

        public bool Start()
        {
            ensureMetricsAreProvisionedTimer.Start();
            return true;
        }

        public bool Stop()
        {
            ensureMetricsAreProvisionedTimer.Stop();
            return true;
        }
    }
}
