using System.ServiceModel;
using System.ServiceModel.Channels;

namespace AAA.Services.Client
{
    public class Proxy : ClientBase<IEventServerMachineService>, IEventServerMachineService
    {
        public Proxy() { }
        public Proxy(string endpointName) : base(endpointName) { }
        public Proxy(Binding binding, string address) : base(binding, new EndpointAddress(address)) { }

        public void SignalMachineStarted(string machineName)
        {
            Channel.SignalMachineStarted(machineName);
        }
    }
}
