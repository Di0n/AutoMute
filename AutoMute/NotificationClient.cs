using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System.Diagnostics;

namespace AutoMute
{
    class NotificationClient : NAudio.CoreAudioApi.Interfaces.IMMNotificationClient
    {
        public delegate void AudioDeviceStateChangedHandler(string pwstrDeviceId, PropertyKey key);
        public event AudioDeviceStateChangedHandler AudioDeviceStateChanged;
        void IMMNotificationClient.OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            Debug.WriteLine("Device changed.");
        }

        void IMMNotificationClient.OnDeviceAdded(string pwstrDeviceId)
        {
            Debug.WriteLine("Device added.");
        }

        void IMMNotificationClient.OnDeviceRemoved(string deviceId)
        {
            Debug.WriteLine("Device Removed.");
        }

        void IMMNotificationClient.OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            Debug.WriteLine("Device state changed.");
        }

        void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            AudioDeviceStateChanged?.Invoke(pwstrDeviceId, key);
        }
    }
}
