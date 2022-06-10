using AAA.Services.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace AAA.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class EventServerMachineService : IEventServerMachineService
    {
        public void SignalMachineStarted(string machineName)
        {
            bool isSlave = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("IsSlave"));
            if (!isSlave) { return; }

            Proxy proxy = new Proxy("BasicHttpBinding_IMasterService");

            try
            {
                proxy.SignalMachineStarted(machineName);
            }
            catch (EndpointNotFoundException ex) { }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                proxy.Close();
            }
        }
    }
}
