using System;
using System.Activities;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;

namespace CopyFileToNetworkFolder
{
    public class CopyFileToNetworkFolder : CodeActivity
    {
        /// <summary>
        /// Folder path for source file.
        /// </summary>
        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> LocalFolderPath { get; set; }

        /// <summary>
        /// Folder path for destination file.
        /// </summary>
        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> DestinationFolderPath { get; set; }

        /// <summary>
        /// File name to copy.
        /// </summary>
        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> FileName { get; set; }

        /// <summary>
        /// Credential Username
        /// </summary>
        [Category("Input")]
        [RequiredArgument]
        public InArgument<string> UserName { get; set; }

        /// <summary>
        /// Credential Password
        /// </summary>
        [Category("Input")]
        [RequiredArgument]
        public InArgument<SecureString> Password { get; set; }

        /// <summary>
        /// Delete local file after successful copy?
        /// </summary>
        [Category("Input")]
        public InArgument<bool> DeleteLocalFile { get; set; }

        /// <summary>
        /// Ignore error?
        /// </summary>
        [Category("Input")]
        public InArgument<bool> ContinueOnError { get; set; }

        /// <summary>
        /// Overwrite File?
        /// </summary>
        [Category("Input")]
        [RequiredArgument]
        public InArgument<bool> OverwriteFile { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                var userName = UserName.Get(context).Trim();
                var password = new NetworkCredential(string.Empty, Password.Get(context)).Password;
                var local = LocalFolderPath.Get(context).Trim();
                var remote = DestinationFolderPath.Get(context).Trim();
                remote = remote.EndsWith("\\") ? remote.Remove(remote.Length - 1) : remote;
                var fileName = FileName.Get(context).Trim();
                bool overWrite = OverwriteFile.Get(context);

                local = Path.Combine(local, fileName);
                if (!File.Exists(local)) throw new Exception("File absent at path: " + local);

                NetworkCredential writeCredentials = new NetworkCredential(userName, password);
                using (new NetworkConnection(remote, writeCredentials))
                {
                    File.Copy(local, Path.Combine(remote, fileName), overWrite);
                }

                var deleteLocal = DeleteLocalFile.Get(context);
                if (deleteLocal) if (File.Exists(local)) File.Delete(local);
            }
            catch
            {
                bool ignoreError = ContinueOnError.Get(context);
                if (!ignoreError) throw;
            }
        }
    }

    public class NetworkConnection : IDisposable
    {
        private string _networkName;

        public NetworkConnection(string networkName,
            NetworkCredential credentials)
        {
            _networkName = networkName;

            var netResource = new NetResource()
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplaytype.Share,
                RemoteName = networkName
            };

            var userName = string.IsNullOrEmpty(credentials.Domain)
                ? credentials.UserName
                : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

            var result = WNetAddConnection2(
                netResource,
                credentials.Password,
                userName,
                0);

            if (result != 0)
            {
                throw new Win32Exception(result);
            }
        }

        ~NetworkConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            WNetCancelConnection2(_networkName, 0, true);
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource,
            string password, string username, int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags,
            bool force);
    }

    [StructLayout(LayoutKind.Sequential)]
    public class NetResource
    {
        public ResourceScope Scope;
        public ResourceType ResourceType;
        public ResourceDisplaytype DisplayType;
        public int Usage;
        public string LocalName;
        public string RemoteName;
        public string Comment;
        public string Provider;
    }

    public enum ResourceScope : int
    {
        Connected = 1,
        GlobalNetwork,
        Remembered,
        Recent,
        Context
    };

    public enum ResourceType : int
    {
        Any = 0,
        Disk = 1,
        Print = 2,
        Reserved = 8,
    }

    public enum ResourceDisplaytype : int
    {
        Generic = 0x0,
        Domain = 0x01,
        Server = 0x02,
        Share = 0x03,
        File = 0x04,
        Group = 0x05,
        Network = 0x06,
        Root = 0x07,
        Shareadmin = 0x08,
        Directory = 0x09,
        Tree = 0x0a,
        Ndscontainer = 0x0b
    }
}