using System.ServiceModel;

namespace ConsoleHostedService
{
    [ServiceContract]
    public interface IEventServerMachineService
    {
        [OperationContract(IsOneWay = true)]
        void SignalMachineStarted(string machineName);
    }
}
