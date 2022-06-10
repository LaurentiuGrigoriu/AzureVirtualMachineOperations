using System.ServiceModel;

namespace AAA.Services.Client
{
    [ServiceContract]
    public interface IEventServerMachineService
    {
        [OperationContract(IsOneWay = true)]
        void SignalMachineStarted(string machineName);
    }
}
