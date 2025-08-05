// See https://aka.ms/new-console-template for more information

using System;
using System.Threading.Tasks;
using Azure;
using Azure.Core; // Required for ResourceIdentifier
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;

class Program
{
    static async Task Main()
    {
        // Azure parameters
        string subscriptionId = "c95a3f8d-b151-4208-9c8f-b5448b640503";
        string resourceGroupName = "my-sdk-rg";
        string location = "centralus";
        string storageAccountName = "mystorageacc1254" + Guid.NewGuid().ToString("n").Substring(0, 8);

        // Authenticate and create ArmClient 
        var credential = new DefaultAzureCredential();
        ArmClient armClient = new ArmClient(credential, subscriptionId);

        // Get subscription resource
        SubscriptionResource subscription = armClient.GetSubscriptionResource(
            new ResourceIdentifier($"/subscriptions/{subscriptionId}")
        );

        // Create the resource group
        ResourceGroupData rgData = new ResourceGroupData(location);
        var rgOperation = await subscription.GetResourceGroups()
            .CreateOrUpdateAsync(WaitUntil.Completed, resourceGroupName, rgData);
        ResourceGroupResource resourceGroup = rgOperation.Value;

        // Create the storage account
        StorageSku sku = new StorageSku(StorageSkuName.StandardLrs);
        StorageAccountCreateOrUpdateContent storageData = new StorageAccountCreateOrUpdateContent(
            sku, StorageKind.StorageV2, location)
        {
            AccessTier = StorageAccountAccessTier.Hot,
            AllowBlobPublicAccess = false
        };

        var storageOperation = await resourceGroup.GetStorageAccounts()
            .CreateOrUpdateAsync(WaitUntil.Completed, storageAccountName, storageData);
        StorageAccountResource storageAccount = storageOperation.Value;

        Console.WriteLine($"Storage account '{storageAccount.Data.Name}' created in resource group '{resourceGroup.Data.Name}' in '{location}'.");
    }
}
