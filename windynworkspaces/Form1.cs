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

        /// <summary>
        /// Determines if a virtual desktop contains any windows or not
        /// </summary>
        /// <param name="desktop">the virtual desktop to check</param>
        /// <returns>true if the virtual desktop contains any windows, false otherwise</returns>
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

        /// <summary>
        /// Get all empty virtual desktops
        /// </summary>
        /// <returns>an array of all empty virtual desktops</returns>
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="automationEventArgs"></param>
        private void TriggerDynamicUpdate(object sender, AutomationEventArgs automationEventArgs)
        {
            // only update every 1s to prevent as much lag
            if (DateTime.Now - lastUpdate < TimeSpan.FromMilliseconds(1000))
            {
                return;
            }

            lastUpdate = DateTime.Now;

            Log("Dynamic update triggered", false);

            // get all desktops
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    VirtualDesktop[] desktops = GetEmptyDesktops();

                    Log($" - found {desktops.Length} empty desktops", true, false);

                    if (desktops.Length == 1)
                    {
                        return;
                    }
                    else if (desktops.Length == 0)
                    {
                        Log("Creating new desktop", false);
                        desktops = new VirtualDesktop[] { VirtualDesktop.Create() };
                        Log($"- id {desktops[0].Id}", true, false);
                        return;
                    }

                    // delete all virtual desktops but the last one
                    for (int i = 0; i < desktops.Length - 1; i++)
                    {
                        if (desktops[i].Id != VirtualDesktop.Current.Id)
                        {
                            Log($"Removing desktop {desktops[i].Id}");
                            desktops[i].Remove();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"Exception: {ex.Message}");
                }
            });

            // move desktop to the end
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    Log("Moving desktop to the end");
                    GetEmptyDesktops()[0].Move(VirtualDesktop.GetDesktops().Length - 1);
                }
                catch (Exception ex)
                {
                    Log($"Exception: {ex.Message}");
                }
            });

            Log("Dynamic update complete");
        }

        /// <summary>
        /// Log a message to the log box
        /// </summary>
        /// <param name="message">the message to log</param>
        /// <param name="newLine">whether to include a new line after the message</param>
        /// <param name="timestamp">whether to include a timestamp before the message</param>
        public void Log(string message, bool newLine = true, bool timestamp = true)
        {
            if (this.logBox.InvokeRequired)
            {
                this.logBox.Invoke(new Action<string, bool, bool>(Log), new object[] { message, newLine, timestamp });
            }
            else
            {
                this.logBox.AppendText($"{(timestamp ? $"[{DateTime.Now.ToString("HH:mm:ss")}]" : "")} {message}{(newLine ? Environment.NewLine : "")}");
            }
        }

        /// <summary>
        /// forcibly trigger an update of the virtual desktops
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void forceUpdate_Click(object sender, EventArgs e)
        {
            lastUpdate = DateTime.MinValue; // force update
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
            Log("Exiting");
            Automation.RemoveAllEventHandlers();
        }

        private void end_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
