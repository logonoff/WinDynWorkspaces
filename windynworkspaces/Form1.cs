using System.Windows.Forms;
using WindowsDesktop;
using System.Windows.Automation;
using System.Collections.Generic;
using System;

namespace windynworkspaces
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// The cooldown between dynamic updates in milliseconds
        /// </summary>
        private int cooldown = 300;

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
                }

                if (arg.Contains("-cooldown"))
                {
                    cooldown = int.Parse(arg.Split('=')[1]);
                    Log($"Cooldown set to {cooldown}ms");
                }
            }

            // https://github.com/startnine/windows-sharp/blob/master/WindowsSharp/Processes/ProcessWindow.cs
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

            VirtualDesktop.CurrentChanged += (sender, e) =>
            {
                Log("Desktop changed");
                TriggerDynamicUpdate(sender, null);
            };

            VirtualDesktop.Created += (sender, e) =>
            {
                Log("Desktop created");
                TriggerDynamicUpdate(sender, null);
            };

            VirtualDesktop.Switched += (sender, e) =>
            {
                Log("Desktop switched");
                TriggerDynamicUpdate(sender, null);
            };

            VirtualDesktop.Destroyed += (sender, e) =>
            {
                Log("Desktop destroyed");
                TriggerDynamicUpdate(sender, null);
            };
        }

        private readonly string[] args = System.Environment.GetCommandLineArgs();

        /// <summary>
        /// Determines if a virtual desktop contains any windows or not
        /// </summary>
        /// <param name="desktop">the virtual desktop to check</param>
        /// <returns>true if the virtual desktop contains any windows, false otherwise</returns>
        private static bool VirtualDesktopContainsAnyWindow(VirtualDesktop desktop)
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
        private static VirtualDesktop[] GetEmptyDesktops()
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
        /// Trigger a dynamic update of the virtual desktops
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="automationEventArgs"></param>
        private void TriggerDynamicUpdate(object sender, AutomationEventArgs automationEventArgs)
        {
            // only update every so often to prevent as much lag
            if (DateTime.Now - lastUpdate < TimeSpan.FromMilliseconds(cooldown))
            {
                return;
            }

            lastUpdate = DateTime.Now;

            Log("Dynamic update triggered", false);

            // get all desktops
            _ = Invoke((MethodInvoker)delegate
            {
                try
                {
                    VirtualDesktop[] desktops = Form1.GetEmptyDesktops();

                    Log($" - found {desktops.Length} empty desktops", true, false);

                    if (desktops.Length == 0)
                    {
                        Log("Creating new desktop", false);
                        desktops = new VirtualDesktop[] { VirtualDesktop.Create() };
                        Log($"- id {desktops[0].Id}", true, false);
                        return;
                    }
                    if (desktops.Length == 1)
                    {
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
                    Log($"Exception during update: {ex}");
                }

                try
                {
                    Log("Moving desktop to the end");
                    GetEmptyDesktops()[0].Move(VirtualDesktop.GetDesktops().Length - 1);
                }
                catch (Exception ex)
                {
                    Log($"Exception while moving: {ex}");
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
            if (logBox.InvokeRequired)
            {
                logBox.Invoke(new Action<string, bool, bool>(Log), new object[] { message, newLine, timestamp });
            }
            else
            {
                logBox.AppendText($"{(timestamp ? $"[{DateTime.Now.ToString("HH:mm:ss")}]" : "")} {message}{(newLine ? Environment.NewLine : "")}");
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
            Log("Closing application");
            Automation.RemoveAllEventHandlers();
            VirtualDesktop.CurrentChanged -= (sender, e) => { };
            VirtualDesktop.Created -= (sender, e) => { };
            VirtualDesktop.Switched -= (sender, e) => { };
            VirtualDesktop.Destroyed -= (sender, e) => { };
        }

        private void end_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
