using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DelayedNotification = System.Timers.Timer;

namespace AutoMute
{
    public class MainApplicationContext : ApplicationContext
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int WM_APPCOMMAND = 0x319;

        private readonly IntPtr handle;

        private MMDeviceEnumerator enumerator;
        private NotificationClient nc;

        private DelayedNotification delayedNotification;

        private NotifyIcon notifyIcon;
        private ContextMenu notifyContextMenu;

        public MainApplicationContext()
        {
            handle = Process.GetCurrentProcess().Handle;

            notifyContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Website", new EventHandler((s, e)=>{ Process.Start("http://anveon.nl"); })),
                new MenuItem("Exit", new EventHandler((s, e) => 
                {
                    DialogResult result = MessageBox.Show("Are you sure you want to quit AutoMute?", "Exit Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                        Application.Exit();
                }))
            });
            notifyIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.Volume_Mute_256,
                ContextMenu = notifyContextMenu,
                Text = "AutoMute",
                Visible = true
            };

            enumerator = new MMDeviceEnumerator();

            delayedNotification = new DelayedNotification();
            delayedNotification.Interval = 1000;
            delayedNotification.AutoReset = false;
            delayedNotification.Elapsed += (s, e) => notifyIcon.ShowBalloonTip(3000, "Volume muted", "Headphones added/removed.", ToolTipIcon.Info);

            if (!enumerator.HasDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia))
            {
                MessageBox.Show("Couldn't find default Audio Device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            nc = new NotificationClient();
            nc.AudioDeviceStateChanged += AudioDeviceStateChanged;
            enumerator.RegisterEndpointNotificationCallback(nc);

            notifyIcon.ShowBalloonTip(2000, "", "AutoMute started...", ToolTipIcon.Info);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (notifyIcon != null)
                    notifyIcon.Dispose();
                if (notifyContextMenu != null)
                    notifyContextMenu.Dispose();
                if (enumerator != null)
                {
                    if (nc != null)
                        enumerator.UnregisterEndpointNotificationCallback(nc);
                    enumerator.Dispose();
                }
                if (delayedNotification != null)
                    delayedNotification.Dispose();
            }
            base.Dispose(disposing);
        }

        private void AudioDeviceStateChanged(string pwstrDeviceId, PropertyKey key)
        {
            MMDevice device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (!device.AudioEndpointVolume.Mute)
            {
                delayedNotification.Start(); // Vertraagd omdat SendMessageW() vertraagd is en de notificatie het systeem anders unmute. (Win 10)
                SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
            }
        }
    }
}
