using AzureOps;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureExample
{
    class Program
    {
        // VM variables
        static public class vmDetails
        {
            static public string resourceGroupName = "RG-FluentResourceGroup";
            static public Region location = Region.USCentral;
            static public string vmName = "VMFluent";
            static public string vNetName = "VNET-Fluent";
            static public string vNetAddress = "172.16.0.0/16";
            static public string subnetName = "Subnet-Fluent";
            static public string subnetAddress = "172.16.0.0/24";
            static public string nicName = "NIC-Fluent";
            static public string adminUser = "azureadminuser";
            static public string adminPassword = "Pas$m0rd$123";
            static public string publicIPName = "publicIP-Fluent";
            static public string nsgName = "NSG-Fluent";
        }

        static void CreateVM(AzureAccount azure)
        {
            Console.WriteLine($"Creating resource group {vmDetails.resourceGroupName} ...");
            azure.CreateResourceGroup(vmDetails.resourceGroupName, vmDetails.location);

            //Every virtual machine needs to be connected to a virtual network.
            Console.WriteLine($"Creating virtual network {vmDetails.vNetName} ...");
            INetwork network = azure.CreateVirtualNetwork(vmDetails.vNetName, vmDetails.resourceGroupName, vmDetails.location, vmDetails.vNetAddress, vmDetails.subnetAddress, vmDetails.subnetName);

            //You need a public IP to be able to connect to the VM from the Internet
            Console.WriteLine($"Creating public IP {vmDetails.publicIPName} ...");
            IPublicIPAddress publicIP = azure.CreatePublicIp(vmDetails.publicIPName, vmDetails.resourceGroupName, vmDetails.location);

            //You need a network security group for controlling the access to the VM
            Console.WriteLine($"Creating Network Security Group {vmDetails.nsgName} ...");
            INetworkSecurityGroup nsg = azure.CreatNetworkSecurityGroup(vmDetails.resourceGroupName, vmDetails.nsgName, vmDetails.location);

            //You need a security rule for allowing the remote TCP connection over the Internet
            Console.WriteLine($"Creating a Security Rule for allowing the remote");
            azure.AllowInternetConnection(nsg, 3389, SecurityRuleProtocol.Tcp);

            Console.WriteLine($"Creating network interface {vmDetails.nicName} ...");
            INetworkInterface nic = azure.CreateNetworkInterface(vmDetails.nicName, vmDetails.resourceGroupName, vmDetails.location, network, vmDetails.subnetName, publicIP, nsg);

            Console.WriteLine($"Creating virtual machine {vmDetails.vmName} ...");
            azure.CreateVM(vmDetails.vmName, vmDetails.resourceGroupName, vmDetails.location, nic, vmDetails.adminUser, vmDetails.adminPassword);
        }

        static void Main(string[] args)
        {
            AzureAccount azure = new AzureAccount("../../azure-configuration.json");

            // create VM in azure AzureAccount, according to vmDetails values
            CreateVM(azure);

            AzureVM vm = new AzureVM(azure._azure, vmDetails.vmName);

            // check the creation of 
            PowerState state = vm.GetState();
            if (state == PowerState.Running)
                Console.WriteLine("Successfully created a new VM: {0}!", vmDetails.vmName);
            
            vm.Stop();

            state = vm.GetState();
            if (state == PowerState.Stopping || state == PowerState.Stopped)
                Console.WriteLine("Successfully stopped the VM: {0}!", vmDetails.vmName);

            azure.DeleteVM(vmDetails.resourceGroupName, vmDetails.vmName);
            Console.WriteLine("VM {0} deleted.", vmDetails.vmName);

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
