using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Configuration;
using ConsoleHostedService.Client;

namespace ConsoleHostedService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class EventServerMachineService : IEventServerMachineService
    {
        protected static bool IsSlave = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("IsSlave"));
        protected static string MachineName = ConfigurationManager.AppSettings.Get("MachineName");

        public static ServiceHost EventServerMachineHost = new ServiceHost(typeof(EventServerMachineService));

        public EventServerMachineService()
        {
            EventServerMachineHost = new ServiceHost(typeof(EventServerMachineService));
        }

        public void SignalMachineStarted(string machineName)
        {
            Console.WriteLine($"The machine with name={machineName} Has Started!");

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
