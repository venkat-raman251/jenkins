﻿using Dashboard.Azure;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;

namespace Dashboard.Tests
{
    internal static class Util
    {
        internal static CloudStorageAccount GetStorageAccount()
        {
            // This is using the storage emulator account.  Make sure to run the following before starting
            // "C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe" start
            var account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            AzureUtil.EnsureAzureResources(account);
            return account;
        }
    }
}
