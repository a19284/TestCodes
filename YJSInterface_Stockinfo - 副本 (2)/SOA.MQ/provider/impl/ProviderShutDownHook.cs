using SOA.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SOA.provider.impl
{
    public class ProviderShutDownHook
    {

        protected static HashSet<IServiceProvider> providerSet = new HashSet<IServiceProvider>();
        public static void register(IServiceProvider provider)
        {
            StringBuilder sb = new StringBuilder().Append("Register provider ").Append(provider).Append(" to ProviderShutDownHook");
            LogUtil.Debug("Debug", sb.ToString());
            sb = null;
            providerSet.Add(provider);
        }
        public void run()
        {
            LogUtil.Info(this, "Providers are shutting down ...");
            Object[] providers = providerSet.ToArray();
            for (int i = 0; i < providers.Length; i++)
            {
                IServiceProvider provider = (IServiceProvider)providers[i];
                provider.closeConnection();
                provider.stop();
            }
        }
    }
}
