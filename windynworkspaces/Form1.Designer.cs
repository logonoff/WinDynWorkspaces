using System.ComponentModel;
using System.Windows.Forms;

namespace windynworkspaces
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
            trayIcon = new NotifyIcon(components);
            label1 = new Label();
            richTextBox1 = new RichTextBox();
            end = new Button();
            logBox = new TextBox();
            forceUpdate = new Button();
            SuspendLayout();
            //
            // trayIcon
            //
            trayIcon.Text = "windynworkspaces";
            trayIcon.MouseClick += trayIcon_MouseClick;
            //
            // label1
            //
            label1.Location = new System.Drawing.Point(0, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(100, 23);
            label1.TabIndex = 0;
            //
            // richTextBox1
            //
            richTextBox1.Location = new System.Drawing.Point(0, 0);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new System.Drawing.Size(100, 96);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            //
            // end
            //
            end.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            end.Location = new System.Drawing.Point(247, 226);
            end.Name = "end";
            end.Size = new System.Drawing.Size(75, 23);
            end.TabIndex = 0;
            end.Text = "Exit";
            end.UseVisualStyleBackColor = true;
            end.Click += end_Click;
            //
            // logBox
            //
            logBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            logBox.Location = new System.Drawing.Point(12, 12);
            logBox.Multiline = true;
            logBox.Name = "logBox";
            logBox.ReadOnly = true;
            logBox.ScrollBars = ScrollBars.Vertical;
            logBox.Size = new System.Drawing.Size(310, 208);
            logBox.TabIndex = 1;
            //
            // forceUpdate
            //
            forceUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            forceUpdate.Location = new System.Drawing.Point(141, 226);
            forceUpdate.Name = "forceUpdate";
            forceUpdate.Size = new System.Drawing.Size(100, 23);
            forceUpdate.TabIndex = 0;
            forceUpdate.Text = "Update now";
            forceUpdate.UseVisualStyleBackColor = true;
            forceUpdate.Click += forceUpdate_Click;
            //
            // Form1
            //
            ClientSize = new System.Drawing.Size(334, 261);
            Controls.Add(forceUpdate);
            Controls.Add(logBox);
            Controls.Add(end);
            MaximizeBox = false;
            MinimumSize = new System.Drawing.Size(300, 200);
            Name = "Form1";
            Text = "windynworkspaces";
            FormClosing += Form1_FormClosing;
            Resize += Form1_Resize;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NotifyIcon trayIcon;
        private Label label1;
        private RichTextBox richTextBox1;
        private Button end;
        private TextBox logBox;
        private Button forceUpdate;
    }
}
