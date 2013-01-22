namespace NServiceBus
{
    using Config;
    using Logging;
    using Unicast.Queuing.Installers;
    using Unicast.Queuing.Msmq;
    using Unicast.Transport;

    /// <summary>
    /// Configuration class for MSMQ transport.
    /// </summary>
    public static class ConfigureMsmqMessageQueue
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (ConfigureMsmqMessageQueue));

        private const string Message =
            @"
MsmqMessageQueueConfig section has been deprecated in favor of using a connectionString instead.
Here is an example of what is required:
  <connectionStrings>
    <add name=""NServiceBus/Transport"" connectionString=""cacheSendConnection=true;journal=false;deadLetter=true"" />
  </connectionStrings>";

        internal static bool Selected { get; set; }

        /// <summary>
        /// Use MSMQ for your queuing infrastructure.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static Configure MsmqTransport(this Configure config)
        {
            Selected = true;

            config.Configurer.ConfigureComponent<MsmqMessageSender>(DependencyLifecycle.InstancePerCall);
            config.Configurer.ConfigureComponent<MsmqDequeueStrategy>(DependencyLifecycle.InstancePerCall)
                .ConfigureProperty(p => p.PurgeOnStartup, ConfigurePurging.PurgeRequested);
            config.Configurer.ConfigureComponent<MsmqQueueCreator>(DependencyLifecycle.SingleInstance);

            var settings = new MsmqSettings();

            var cfg = Configure.GetConfigSection<MsmqMessageQueueConfig>();

            if (cfg != null)
            {
                settings.UseJournalQueue = cfg.UseJournalQueue;
                settings.UseDeadLetterQueue = cfg.UseDeadLetterQueue;

                Logger.Warn(Message);
            }
            else
            {
                var connectionString = TransportConnectionString.GetConnectionStringOrNull();

                if (connectionString != null)
                {
                    settings = new MsmqConnectionStringBuilder(connectionString).RetrieveSettings();
                }
            }

            config.Configurer.ConfigureProperty<MsmqMessageSender>(t => t.UseDeadLetterQueue, settings.UseDeadLetterQueue);
            config.Configurer.ConfigureProperty<MsmqMessageSender>(t => t.UseJournalQueue, settings.UseJournalQueue);
            config.Configurer.ConfigureProperty<MsmqMessageSender>(t => t.UseConnectionCache, settings.UseConnectionCache);

            EndpointInputQueueCreator.Enabled = true;

            return config;
        }
    }
}