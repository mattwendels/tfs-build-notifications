using Microsoft.Win32;
using System;
using Tfs.BuildNotifications.Common.Helpers.Interfaces;

namespace Tfs.BuildNotifications.Common.Helpers
{
    public class RegistryHelper : IRegistryHelper
    {
        public T GetValue<T>(string subKey, string name)
        {
            return (T)GetRegistryKey(subKey).GetValue(name);
        }

        public void SetValue(string subKey, string name, string value)
        {
            GetRegistryKey(subKey).SetValue(name, value);
        }

        public bool KeyExistsWithValue(string subKey, string name)
        {
            return !string.IsNullOrWhiteSpace(GetValue<string>(subKey, name));
        }

        #region Private Methods

        private RegistryKey GetRegistryKey(string subKey)
        {
            // Is the server a 64bit machine? Read registry keys correctly here...
            var baseRegistryArchitecture = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, 
                (IntPtr.Size == 8 ? RegistryView.Registry64 : RegistryView.Registry32));

            baseRegistryArchitecture.CreateSubKey(subKey);

            return baseRegistryArchitecture.OpenSubKey(subKey, true);
        }

        #endregion
    }
}
