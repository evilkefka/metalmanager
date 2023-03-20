using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace MetalManager
{
    public partial class AboutForm : Form
    {
        string pInfo = "";
        public AboutForm(string pageInfo)
        {
            InitializeComponent();
            pInfo = pageInfo;
        }

        private void About_FormLoad(object sender, EventArgs e)
        {
            if (pInfo != null)
            {
                if (pInfo == "sendfeedback")
                {
                    this.Text = "Send Feedback";
                    aboutLabel1.Text = "Please send all feedback to:";
                    aboutLabel2.Text = "metalmanagerfeedback@gmail.com";
                    aboutLabel2.Click += new System.EventHandler(this.CopyEmail);
                    aboutLabel3.Text = "And thank you for taking an\n       interest in my program!  :)";
                    aboutLabel3.Top += 8;
                    aboutLabel4.Text = "                — Evil_Kefka";
                    aboutLabel4.Top += 11;
                    aboutLabel3.Left += 30;
                    aboutLabel4.Left += 40;
                    sendFdbkCopyText.Visible = true;
                } else if (pInfo == "getlhlibrary")
                {
                    this.Text = "Get Low Health Library";
                    aboutLabel1.Text = "To get the Low Health Library, visit:";
                    aboutLabel1.Left = 12;
                    aboutLabel1.Top = 11;

                    aboutLabel2.Text = "https://gamebanana.com/mods/433022";
                    aboutLabel2.Click += new System.EventHandler(this.CopyLHLibraryLink);
                    aboutLabel2.Location = new Point(16, 29);
                    aboutLabel2.ForeColor = Color.FromArgb(255, 0, 0, 77);

                    sendFdbkCopyText.Location = new Point(25, 42);
                    sendFdbkCopyText.Visible = true;

                    aboutLabel3.Text = "Scroll down to Files, and download low_health_library.zip. Open the Zip, and";
                    aboutLabel3.Location = new Point(16, 59);
                    aboutLabel3.AutoSize = false;
                    aboutLabel3.Width = 200;
                    aboutLabel3.Height = 28;
                    


                    aboutLabel4.Text = "extract or drag-and-drop the _LHLibrary folder to your Mods folder. Then go to File>Reload Mods Folder, or restart Metal Manager.";
                    aboutLabel4.Location = new Point(16, 86);
                    aboutLabel4.AutoSize = false;
                    aboutLabel4.Width = 264;
                    aboutLabel4.Height = 42;

                    aboutLabelBot.Visible = false;

                }
                else if (pInfo == "help")
                {
                    this.Text = "Help";
                    aboutLabel1.Visible = false;
                    aboutLabel2.Visible = false;
                    aboutLabel3.Visible = false;
                    aboutLabel4.Visible = false;
                    sendFdbkCopyText.Visible = false;
                    aboutLabelBot.Visible = false;


                    discordExplainLbl1.Visible = true;
                    discordExplainLbl2.Visible = true;
                    discordExplainLbl3.Visible = true;
                    discordExplainLbl4.Visible = true;
                    DiscordBorderedPanel.Visible = true;
                    signedEKlabel.Visible = true;

                    discordExplainLbl1.Location = new Point(6, 6);
                    discordExplainLbl2.Location = new Point(11, 72);
                    discordExplainLbl3.Location = new Point(11, 86);
                    discordExplainLbl4.Location = new Point(11, 100);
                    DiscordBorderedPanel.Location = new Point(128, 85);
                    signedEKlabel.Location = new Point(124, 113);

                }
            }
        }

        private void CopyEmail(object sender, EventArgs e)
        {
            Clipboard.SetText("metalmanagerfeedback@gmail.com");
            sendFdbkCopyText.Text = "Copied!";
        }

        private void CopyLHLibraryLink(object sender, EventArgs e)
        {
            Clipboard.SetText("https://gamebanana.com/mods/433022");
            sendFdbkCopyText.Text = "Copied!";
        }

        private void MHDiscordBtnClick(object sender, EventArgs e)
        {
            GoToMHDiscord();
        }

        private void GoToMHDiscord()
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("discord:///invite-proxy/jRrapbDA9x");
            Process.Start(sInfo);
        }

        bool overDiscordText = false;
        bool overDiscordButton = false;
        private void discordPanel_mouseOver(object sender, EventArgs e)
        {
            overDiscordButton = true;
            lightUpTheSign();
        }

        private void discordPanel_mouseOut(object sender, EventArgs e)
        {
            overDiscordButton = false;
            lightUpTheSign();
        }

        private void discordText_mouseOver(object sender, EventArgs e)
        {
            overDiscordText = true;
            lightUpTheSign();
        }
        private void discordText_mouseOut(object sender, EventArgs e)
        {
            overDiscordText = false;
            lightUpTheSign();
        }

        private void lightUpTheSign()
        {
            if(overDiscordText || overDiscordButton)
            {
                discordButtonPanel1.BackColor = SystemColors.ControlLightLight;
            } else
            {
                discordButtonPanel1.BackColor = SystemColors.ControlLight;
            }
            
        }
    }
}
