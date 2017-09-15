namespace Tfs.BuildNotifications.Common.Helpers.Interfaces
{
    public interface IRegistryHelper
    {
        T GetValue<T>(string subKey, string name);

        void SetValue(string subKey, string name, string value);

        bool KeyExistsWithValue(string subKey, string name);
    }
}
