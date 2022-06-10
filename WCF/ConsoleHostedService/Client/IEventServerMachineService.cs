using System.ServiceModel;

namespace ConsoleHostedService.Client
{
    [ServiceContract]
    public interface IEventServerMachineService
    {
        [OperationContract(IsOneWay = true)]
        void SignalMachineStarted(string machineName);
    }
}
