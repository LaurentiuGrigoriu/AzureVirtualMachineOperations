using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Azure.Management.Fluent.Azure;

namespace AzureOps
{
    public class AzureAccount
    {
        public IAzure _azure { get;  }

        public AzureAccount(IAzure azure)
        {
            _azure = azure;
        }

        public AzureAccount(AzureCredentials credentials)
        {
            _azure = Azure.Authenticate(credentials).WithDefaultSubscription();
            //_azure = Azure.Authenticate(credentials).WithSubscription(/*string */subscriptionId);
        }

        public AzureAccount(string authFile)
        {
            AzureCredentials credentials = SdkContext.AzureCredentialsFactory.FromFile(authFile);
            _azure = Azure.Authenticate(credentials).WithDefaultSubscription();            
        }

        // create a resource group
        public IResource CreateResourceGroup(string resourceGroupName, Region location)
        {
            IResource resourceGroup = _azure.ResourceGroups.Define(resourceGroupName)
                .WithRegion(location)
                .Create();

            return resourceGroup;
        }

        // Delete a resource group by name
        public void DeleteResourceGroup(string resourceGroupName)
        {
            _azure.ResourceGroups.DeleteByName(resourceGroupName);
        }

        // Create a virtual network.
        public INetwork CreateVirtualNetwork(string vNetName, string resourceGroupName, Region location, string vNetAddress, string subnetAddress, string subnetName)
        {
            //Every virtual machine needs to be connected to a virtual network.
            INetwork network = _azure.Networks.Define(vNetName)
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroupName)
                .WithAddressSpace(vNetAddress)
                .WithSubnet(subnetName, subnetAddress)
                .Create();

            // network.Id = the network id (if you want to delete it by ID)

            return network;
        }

        // Delete a virtual network by ID
        public void DeleteVirtualNetwork(string networkId)
        {
            _azure.Networks.DeleteById(networkId);
        }

        // Delete a virtual network by ResourceGroup
        public void DeleteVirtualNetwork(string resourceGroupName, string networkName)
        {
            _azure.Networks.DeleteByResourceGroup(resourceGroupName, networkName);
        }

        // Create a public IP
        public IPublicIPAddress CreatePublicIp(string publicIPName, string resourceGroupName, Region location)
        {
            IPublicIPAddress publicIP = _azure.PublicIPAddresses.Define(publicIPName)
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroupName)
                .Create();

            // publicIP.Id = the ID of the public IP (if you want to delete it by ID)

            return publicIP;
        }

        // Delete a public IP by ID
        public void DeletePublicIp(string publicIpId)
        {
            _azure.PublicIPAddresses.DeleteById(publicIpId);
        }

        // Delete a public IP by ResourceGroup
        public void DeletePublicIp(string resourceGroupName, string publicIPName)
        {
            _azure.PublicIPAddresses.DeleteByResourceGroup(resourceGroupName, publicIPName);
        }

        // Create a Network Security Group
        public INetworkSecurityGroup CreatNetworkSecurityGroup(string resourceGroupName, string nsgName, Region location)
        {
            INetworkSecurityGroup nsg = _azure.NetworkSecurityGroups.Define(nsgName)
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroupName)
                .Create();

            // nsg.Id = NetworkSecurityGroup ID (if you want to delete it by ID)

            return nsg;
        }

        // Delete a Network Security Group by ID
        public void DeleteNetworkSecurityGroup(string publicIpId)
        {
            _azure.NetworkSecurityGroups.DeleteById(publicIpId);
        }

        // Delete a Network Security Group by ResourceGroup
        public void DeleteNetworkSecurityGroup(string resourceGroupName, string publicIPName)
        {
            _azure.NetworkSecurityGroups.DeleteByResourceGroup(resourceGroupName, publicIPName);
        }

        // Create a Network Interface
        public INetworkInterface CreateNetworkInterface(string nicName, string resourceGroupName, Region location, INetwork network, string subnetName, IPublicIPAddress publicIPAddr, INetworkSecurityGroup nsg)
        {
            INetworkInterface nic = _azure.NetworkInterfaces.Define(nicName)
                     .WithRegion(location)
                     .WithExistingResourceGroup(resourceGroupName)
                     .WithExistingPrimaryNetwork(network)
                     .WithSubnet(subnetName)
                     .WithPrimaryPrivateIPAddressDynamic()
                     .WithExistingPrimaryPublicIPAddress(publicIPAddr)
                     .WithExistingNetworkSecurityGroup(nsg)
                     .Create();

            return nic;
        }

        public void AllowInternetConnection(INetworkSecurityGroup nsg, int port, SecurityRuleProtocol protocol)
        {
            nsg.Update()
                .DefineRule("Allow-RDP")
                .AllowInbound()
                .FromAnyAddress()
                .FromAnyPort()
                .ToAnyAddress()
                .ToPort(port)
                .WithProtocol(protocol)
                .WithPriority(100)
                .Attach()
                .Apply();
        }

        // Create an WindowsServer Virtual Machine
        public IVirtualMachine CreateVM (string vmName, string resourceGroupName, Region location, INetworkInterface nic, string adminUser, string adminPassword)
        {
            IVirtualMachine vm = _azure.VirtualMachines.Define(vmName)
                    .WithRegion(location)
                    .WithExistingResourceGroup(resourceGroupName)
                    .WithExistingPrimaryNetworkInterface(nic)
                    .WithLatestWindowsImage("MicrosoftWindowsServer", "WindowsServer", "2012-R2-Datacenter")
                    .WithAdminUsername(adminUser)
                    .WithAdminPassword(adminPassword)
                    .WithComputerName(vmName)
                    .WithSize(VirtualMachineSizeTypes.StandardDS2V2)
                    .Create();
           
            return vm;
        }

        // Delete a Virtual Machine by ID
        public void DeleteVM(string id, bool force = true)
        {
            _azure.VirtualMachines.DeleteById(id, force);
        }

        // Delete a Virtual Machine by ResourceGroup
        public void DeleteVM(string resourceGroupName, string vmName, bool force = true)
        {
            _azure.VirtualMachines.DeleteByResourceGroup(resourceGroupName, vmName, force);
        }
    }


    public class AzureVM
    {
        private readonly IAzure _azure;
        private readonly string _vmName;

        private readonly IVirtualMachine _VM;

        public AzureVM(IAzure azure, string vmName)
        {
            this._azure = azure;
            _vmName = vmName;

            if (azure is null) 
                return;

            foreach (IVirtualMachine virtualMachine in this._azure.VirtualMachines.List())
            {
                if (virtualMachine.Name == _vmName)
                {
                    _VM = virtualMachine;
                }
            }
        }

        public PowerState GetState()
        {
            return _VM.PowerState;
        }

        public void Start()
        {
            _VM.Start();
        }

        public void Stop()
        {
            _VM.PowerOff();
        }
    }
}
