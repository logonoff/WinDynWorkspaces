using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsDesktop;
using System.Windows.Automation;
using System.Windows.Threading;
using System.Collections.Generic;

namespace windynworkspaces
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Icon = trayIcon.Icon = Resources.bruv;

            // silent command argument
            foreach (string arg in args)
            {
                if (arg.Contains("-silent"))
                {
                    Log(arg);
                    WindowState = FormWindowState.Minimized;
                    ShowInTaskbar = false;
                    trayIcon.Visible = true;
                    // TriggerDynamicUpdate();
                }
            }

            // thank u splitwirez
            Automation.AddAutomationEventHandler(
                eventId: WindowPattern.WindowOpenedEvent,
                element: AutomationElement.RootElement,
                scope: TreeScope.Children,
                eventHandler: TriggerDynamicUpdate
            );

            Automation.AddAutomationEventHandler(
                eventId: WindowPattern.WindowClosedEvent,
                element: AutomationElement.RootElement,
                scope: TreeScope.Subtree,
                eventHandler: TriggerDynamicUpdate
            );
        }

        private readonly string[] args = Environment.GetCommandLineArgs();

        private bool VirtualDesktopContainsAnyWindow(VirtualDesktop desktop)
        {
            foreach (var window in OpenWindowGetter.GetOpenWindows())
            {
                if (VirtualDesktop.FromHwnd(window.Key) == desktop)
                {
                    return true;
                }
            }

            return false;
        }

        private VirtualDesktop[] GetEmptyDesktops()
        {
            VirtualDesktop[] desktops = VirtualDesktop.GetDesktops();
            List<VirtualDesktop> emptyDesktops = new List<VirtualDesktop>();

            foreach (var desktop in desktops)
            {
                if (!VirtualDesktopContainsAnyWindow(desktop))
                {
                    emptyDesktops.Add(desktop);
                }
            }

            return emptyDesktops.ToArray();
        }

        private DateTime lastUpdate = DateTime.Now;

        private void TriggerDynamicUpdate(object sender, AutomationEventArgs automationEventArgs)
        {
            // only update every 1s to prevent as much lag
            if (DateTime.Now - lastUpdate < TimeSpan.FromMilliseconds(1000))
            {
                return;
            }

            lastUpdate = DateTime.Now;

            Log("Dynamic update triggered");

            // get all desktops
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    VirtualDesktop[] desktops = GetEmptyDesktops();

                    if (desktops.Length == 1) {
                        return;
                    }
                    else if (desktops.Length == 0)
                    {
                        Log("Creating new desktop");
                        VirtualDesktop.Create();
                        return;
                    }

                    // delete all virtual desktops but the last one
                    for (int  i = 0; i < desktops.Length - 1; i++)
                    {
                        Log($"Removing desktop {desktops[i].Id}");
                        desktops[i].Remove();
                    }
                }
                catch (Exception ex)
                {
                    Log($"Exception: {ex.Message}");
                }

                Log("Dynamic update complete");
            });
        }

        public void Log(string message)
        {
            if (this.logBox.InvokeRequired)
            {
                this.logBox.Invoke(new Action<string>(Log), new object[] { message });
            }
            else
            {
                this.logBox.AppendText(message + Environment.NewLine);
            }
        }

        // called when a user preference changes
        private async void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            Log($"User preference changing. Category: {e.Category}");
            //if (e.Category == UserPreferenceCategory.Desktop)
            //{
            await Task.Delay(1000);
            //}
        }

        private void forceSetAccent_Click(object sender, EventArgs e)
        {
            TriggerDynamicUpdate(sender, null);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                trayIcon.Visible = true;
                Hide();
            }
            else
            {
                trayIcon.Visible = false;
            }
        }

        private void trayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= UserPreferenceChanged;
        }

        private void end_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
