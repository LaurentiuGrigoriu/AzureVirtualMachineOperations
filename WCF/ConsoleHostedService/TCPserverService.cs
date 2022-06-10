using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using ConsoleHostedService.Client;

namespace ConsoleHostedService
{
    class TCPserverService : ServiceBase
    {
        protected static bool IsSlave = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("IsSlave"));
        protected static string MachineName = ConfigurationManager.AppSettings.Get("MachineName");

        public static ServiceHost EventServerMachineHost = new ServiceHost(typeof(EventServerMachineService));

        public TCPserverService()
        {
            // Update to set the working directory in windows service
            // This is required to run as windows service!!
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

            EventServerMachineHost = new ServiceHost(typeof(EventServerMachineService));

        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ServiceBase.Run(new TCPserverService());
            }

            else if (args[0].Equals("C")) //For Commandline execution
            {
                EventServerMachineHost = new ServiceHost(typeof(EventServerMachineService));

                OpenServerHost();

                string SignalMachineStartedAPIAddress = string.Join(",", EventServerMachineHost.BaseAddresses.Select(x => x.AbsoluteUri).ToList());
                Console.WriteLine(Environment.NewLine + "SignalMachineStartedAPI is {0} at {1}.", EventServerMachineHost.State, SignalMachineStartedAPIAddress);
                Console.WriteLine();

                if (IsSlave)
                {
                    Console.Title = "Slave";
                    SignalMachineStarted(MachineName);
                }
                else
                {
                    Console.Title = "Master";
                }

                Console.WriteLine("Press CTRL + C to exit...");
                Console.ReadLine();

                Cleanup();
            }
        }

        private static void SignalMachineStarted(string machineName)
        {
            Console.WriteLine($"Inform Master that currentMachineName={machineName} has started.");

            bool isSlave = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("IsSlave"));
            if (!isSlave) { return; }

            Proxy proxy = new Proxy("BasicHttpBinding_IMasterService");

            try
            {
                proxy.SignalMachineStarted(machineName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                proxy.Close();
            }
        }

        private static void OpenServerHost()
        {
            try
            {
                EventServerMachineHost.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Initialization failed.");
                Console.WriteLine(ex);
            }
        }

        private static void Cleanup()
        {
            if (EventServerMachineHost != null) { EventServerMachineHost.Close(); }
        }

        protected override void OnStart(string[] args)
        {
            OpenServerHost();
        }

        protected override void OnStop()
        {
            Cleanup();
        }
    }
}
