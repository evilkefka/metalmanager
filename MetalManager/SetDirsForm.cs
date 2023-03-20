using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Configuration;
using System.Runtime.InteropServices;

namespace MetalManager
{
    public partial class SetDirsForm : Form
    {
        public SetDirsForm()
        {
            InitializeComponent();
        }

        public Form1 MyParentForm;

        public DirectoryInfo GameDirVal
        {
            get { return gameDi; }
        }
        public DirectoryInfo ModDirVal
        {
            get { return modDi; }
        }

        private void SetDirsForm_Load(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        Color DirBNrmC = SystemColors.Window;
        Color DirBErrC = Color.FromArgb(255, 255, 200, 200);
        Color DirBGudC = Color.FromArgb(255, 200, 255, 200);

        //gameDi is game directory
        //modDi is mod/custom songs directory
        DirectoryInfo gameDi;
        DirectoryInfo modDi;
        private void SetGameDir()
        {
            //This SETS our Game directory!
            //modDi is modpath, gameDi is game directory
            string gamePath = string.Empty;

            using (FolderBrowserDialog openFileDialog = new FolderBrowserDialog())
            {
                //openFileDialog.InitialDirectory = "c:\\";
                //openFileDialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                //openFileDialog.FilterIndex = 2;
                //openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file

                    gamePath = openFileDialog.SelectedPath;
                    string streamingAssetsDir = FindGameAndGetStreamingAssets(gamePath);
                    if (streamingAssetsDir != null)
                    {
                        //gameDi = new DirectoryInfo(@streamingAssetsDir);
                        //AddOrUpdateAppSettings("gameDirectory", streamingAssetsDir);
                        gameDirFlagLabel.Text = "Game/StreamingAssets directory Found!";
                        gameDirTextbox.Text = streamingAssetsDir;
                        gameDirTextbox.BackColor = DirBGudC;
                        unlockGate("game", true);
                    }
                    else
                    {
                        MessageBox.Show("Please select Metal Hellsinger's game directory or its StreamingAssets folder.");
                        gameDirFlagLabel.Text = "Invalid selection";
                        gameDirTextbox.Text = "";
                        gameDirTextbox.BackColor = DirBErrC;
                        unlockGate("game", false);
                    }

                }
            }
        }

        private void SetGameDirTextbox()
        {
            //This is called if we hit enter on the textbox
            string allegedDirectory = gameDirTextbox.Text.Trim();
            //allegedDirectory = removeTrailingSlash(allegedDirectory);
            allegedDirectory = FixPath(allegedDirectory);

            if (!Directory.Exists(allegedDirectory))
            {
                gameDirFlagLabel.Text = "Directory doesn't exist";
                gameDirTextbox.BackColor = DirBErrC;
                unlockGate("game", false);
                return;
            }

            string streamingAssetsDir = FindGameAndGetStreamingAssets(allegedDirectory);
            if (streamingAssetsDir != null)
            {
                //gameDi = new DirectoryInfo(@streamingAssetsDir);
                //AddOrUpdateAppSettings("gameDirectory", streamingAssetsDir);
                gameDirFlagLabel.Text = "Game/StreamingAssets directory Found!";
                gameDirTextbox.BackColor = DirBGudC;
                gameDirTextbox.Text = allegedDirectory; //resets it if we had spaces or a trailing /
                unlockGate("game", true);
            }
            else
            {
                MessageBox.Show("Please select Metal Hellsinger's game directory or its StreamingAssets folder.");
                //AddOrUpdateAppSettings("gameDirectory", "");
                gameDirFlagLabel.Text = "Invalid selection";
                gameDirTextbox.BackColor = DirBErrC;
                unlockGate("game", false);
            }

        }




        private string FindGameAndGetStreamingAssets(string givenPath)
        {
            //this function takes our path and verifies we can find our game EXE. It then returns the StreamingAssets folder

            //whatever path our user gave us, we're going to analyze it and link StreamingAssets, which has our main JSON file
            //we can always go back two directories if we need to access game folder (which would just be to have a "Start Game" button)
            string[] dirs = givenPath.Split('\\');
            if (dirs.Length == 1) { return null; }

            string exeVerifyPath = ""; //we're just verifying the Metal.exe is there
            string returnString = "";

            if (dirs.Last() == "StreamingAssets")
            {
                for (int i = 0; i < dirs.Length - 2; i++)
                {
                    exeVerifyPath += dirs[i] + "\\";
                }
            }
            else if (dirs.Last() == "Metal_Data")
            {
                for (int i = 0; i < dirs.Length - 1; i++)
                {
                    exeVerifyPath += dirs[i] + "\\";
                }
            }
            else if (dirs.Last() == "Metal Hellsinger")
            {
                exeVerifyPath = String.Join("\\", dirs);
            }

            if (Directory.Exists(@exeVerifyPath))
            {
                string look4Game = exeVerifyPath + "\\Metal.exe";
                string look4Ignore = exeVerifyPath + "\\iGnore.txt";
                if (File.Exists(@look4Game))
                {
                    returnString = exeVerifyPath + "\\Metal_Data\\StreamingAssets";
                    return returnString;
                }
                if (File.Exists(@look4Ignore))
                {
                    returnString = exeVerifyPath;
                    returnString.Replace("\\\\", "\\");
                    return returnString;
                }
                else
                {
                    return null;
                }
            }

            //if we got this far, something goofed
            return null;
        }

        private void SetModDirTextbox()
        {
            //This is called if we hit enter on the textbox
            string allegedDirectory = modDirTextbox.Text.Trim(); //gets rid of any white space around it
            //allegedDirectory = removeTrailingSlash(allegedDirectory); //if we had Dir/Ectory/, it would result in error. This prevents it
            allegedDirectory = FixPath(allegedDirectory);

            if (!Directory.Exists(allegedDirectory))
            {
                modDirFlagLabel.Text = "Directory doesn't exist";
                modDirTextbox.BackColor = DirBErrC;
                unlockGate("mod", false);
                return;
            }

            //AddOrUpdateAppSettings("modDirectory", allegedDirectory);
            modDirFlagLabel.Text = "Custom songs folder set!";
            modDirTextbox.Text = allegedDirectory; //resets it to remove the / or spaces if we had it
            modDirTextbox.BackColor = DirBGudC;
            unlockGate("mod", true);
        }

        private void SetModsFolder()
        {
            //this is for the Browse... button next to the textbox
            var fileContent = string.Empty;
            var modPath = string.Empty;

            using (FolderBrowserDialog openFileDialog = new FolderBrowserDialog())
            {
                //openFileDialog.InitialDirectory = "c:\\";
                //openFileDialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                //openFileDialog.FilterIndex = 2;
                //openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    modPath = openFileDialog.SelectedPath;
                    //modDi = new DirectoryInfo(@modPath);
                    //AddOrUpdateAppSettings("modDirectory", modPath);
                    modDirFlagLabel.Text = "Custom songs folder set!";
                    modDirTextbox.Text = modPath;
                    modDirTextbox.BackColor = DirBGudC;
                    unlockGate("mod", true);
                }
            }
        }

        private void ConfirmDirectories()
        {
            string modPath = modDirTextbox.Text.Trim();
            //modPath = removeTrailingSlash(modPath);
            modPath = FixPath(modPath);
            string gamePath = gameDirTextbox.Text.Trim();
            //gamePath = removeTrailingSlash(gamePath);
            gamePath = FixPath(gamePath);
            bool ignoreGame = dontLinkGame.Checked;

            //check if both folders exists
            if (!Directory.Exists(modPath))
            {
                modDirFlagLabel.Text = "Directory doesn't exist";
                modDirTextbox.BackColor = DirBErrC;
                unlockGate("mod", false);
            }
            if (!Directory.Exists(gamePath))
            {
                //game directory doesn't exist and we weren't told to ignore it
                if (!ignoreGame)
                {
                    gameDirFlagLabel.Text = "Directory doesn't exist";
                    gameDirTextbox.BackColor = DirBErrC;
                    unlockGate("game", false);
                    return;
                }
            }
            if (!Directory.Exists(modPath)) return;


            //we know the folders actually exist
            //next check if the game folder is correct
            if (ignoreGame) goto SkipGamePart;

            string streamingAssetsDir = FindGameAndGetStreamingAssets(gamePath); //returns the StreamingAssets folder
            if (streamingAssetsDir == null)
            { 
                MessageBox.Show("Please select Metal Hellsinger's game directory or its StreamingAssets folder.");
                gameDirFlagLabel.Text = "Invalid selection";
                gameDirTextbox.BackColor = DirBErrC;
                unlockGate("game", false);
                return;
            }

            //if we got this far, we're golden!
            streamingAssetsDir = streamingAssetsDir.Substring(0, 1).ToUpper() + streamingAssetsDir.Substring(1); //make the first letter CAPPED!!
            gameDi = new DirectoryInfo(@streamingAssetsDir);
            AddOrUpdateAppSettings("gameDirectory", streamingAssetsDir);

            SkipGamePart:

            modDi = new DirectoryInfo(@modPath);
            AddOrUpdateAppSettings("modDirectory", modPath);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Looks if we're sitting somewhere where we can find Metal Hellsinger
        /// </summary>
        private void checkIfWereInGameFolder()
        {
            string myDir = AppDomain.CurrentDomain.BaseDirectory;
            if (myDir.Last() == '\\') myDir = myDir.Substring(0, myDir.Length - 1);
            string streamingAssetsDir = FindGameAndGetStreamingAssets(myDir); //returns the StreamingAssets folder
            if (streamingAssetsDir == null) return;
            
            //if we got this far, we're in the game folder
            gameDirTextbox.Text = FixPath(streamingAssetsDir);
            gameDirFlagLabel.Text = "Game/StreamingAssets directory Found!";
            gameDirTextbox.BackColor = DirBGudC;
            unlockGate("game", true);
        }


        bool gameKeyInserted = false;
        bool modKeyInserted = false;
        private void unlockGate(string gameOrModKey, bool keyInserted)
        {
            //we need two keys to unlock the gate
            //i'm so tired
            if(gameOrModKey == "game")
            {
                gameKeyInserted = keyInserted;
            } else if(gameOrModKey == "mod")
            {
                modKeyInserted = keyInserted;
            }

            if (modKeyInserted && gameKeyInserted)
            {
                //unlocked!
                confirmDirsBtn.Enabled = true;
                setSunglasses(true);
            } else
            {
                //blocked
                confirmDirsBtn.Enabled = false;
                setSunglasses(false);
            }
        }
        private void setSunglasses(bool on)
        {
            if (on)
            {
                Image sunglasses = MetalManager.Properties.Resources.PazLife;
                pazBuddyIcon.Image = sunglasses;
            } else
            {
                pazBuddyIcon.Image = null;
            }

            
        }

        private string removeTrailingSlash(string origStr)
        {
            if(origStr.Substring(origStr.Length-1, 1) == "\\")
            {
                return origStr.Substring(0, origStr.Length - 1);
            }
            return origStr;
        }

        private void silenceGatekeeper()
        {
            confirmLabel.Text = "";
        }

        private void confirmBtnGatekeeper()
        {
            bool gameBoxIsntGreen = gameDirTextbox.BackColor.R >= 240;
            bool modBoxIsntGreen = modDirTextbox.BackColor.R >= 240;
            bool ignoreGame = dontLinkGame.Checked;

            if (ignoreGame) gameBoxIsntGreen = false;

            if(gameBoxIsntGreen && modBoxIsntGreen)
            {
                confirmLabel.Text = "Must set directories first.";
            } else if (modBoxIsntGreen)
            {
                confirmLabel.Text = "Custom songs directory missing.";
            } else if (gameBoxIsntGreen)
            {
                confirmLabel.Text = "Game directory missing.";
            }
            
        }

        private void dontLinkGameChkChng(object sender, EventArgs e)
        {
            CheckBox chkB = sender as CheckBox;

            gameDirTextbox.Enabled = !chkB.Checked;
            browseGameDirBtn.Enabled = !chkB.Checked;

            bool gameValueNotValid = gameDirTextbox.BackColor.R >= 240; //checks if gamebox is green
            if (!chkB.Checked)
            {
                if (gameValueNotValid)
                {
                    unlockGate("game", false);
                }
            } else
            {
                if (gameValueNotValid)
                {
                    unlockGate("game", true);
                }
            }

            

            
            
        }


        public static void AddOrUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        private void DirTboxEnter(object sender, EventArgs e)
        {
            //we're editing the Textbox
            TextBox tb = sender as TextBox;
            if(tb.Name == "modDirTextbox")
            {
                mod_prsEntrLabel.Visible = true;
            } else
            {
                game_prsEntrLabel.Visible = true;
            }
            if(tb.Text == "Copy directory and Paste it here, or click Browse...")
            {
                tb.Text = "";
            }
        }

        private void dirTboxLostFocus(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            //it'd be nicer if it stayed till they actually hit enter
            //then i'd have to program it going away if we actually tried changing our selection, and confirming it was changed
            //if it's fucked up, it'll get caught anyways
            if (tb.Name == "modDirTextbox")
            {
                mod_prsEntrLabel.Visible = false;
            }
            else
            {
                game_prsEntrLabel.Visible = false;
            }
            if (tb.Text == "")
            {
                tb.Text = "Copy directory and Paste it here, or click Browse...";
            }
        }



        private void DirTboxKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (e.KeyCode != Keys.Return) return; //we don't give a crap if it's not return


            if (tb.Name == "modDirTextbox")
            {
                SetModDirTextbox();
                mod_prsEntrLabel.Visible = false;
            } else if(tb.Name == "gameDirTextbox")
            {
                SetGameDirTextbox();
                game_prsEntrLabel.Visible = false;
            }
            e.SuppressKeyPress = true;
            groupBox1.Focus();
            
        }

        private void SetModFolderDialogue(object sender, EventArgs e)
        {
            SetModsFolder();
        }
        private void SetGameFolderDialogue(object sender, EventArgs e)
        {
            SetGameDir();
        }

        private void ConfirmDirsClick(object sender, EventArgs e)
        {
            ConfirmDirectories();
        }

        private void confirmBtnMouseOver(object sender, EventArgs e)
        {
            confirmBtnGatekeeper();
        }

        private void confirmBtnMouseLeft(object sender, EventArgs e)
        {
            silenceGatekeeper();
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint GetLongPathName(string ShortPath, StringBuilder sb, int buffer);

        [DllImport("kernel32.dll")]
        static extern uint GetShortPathName(string longpath, StringBuilder sb, int buffer);

        protected static string GetWindowsPhysicalPath(string path)
        {
            StringBuilder builder = new StringBuilder(255);

            // names with long extension can cause the short name to be actually larger than
            // the long name.
            GetShortPathName(path, builder, builder.Capacity);

            path = builder.ToString();

            uint result = GetLongPathName(path, builder, builder.Capacity);

            if (result > 0 && result < builder.Capacity)
            {
                //Success retrieved long file name
                builder[0] = char.ToLower(builder[0]);
                return builder.ToString(0, (int)result);
            }

            if (result > 0)
            {
                //Need more capacity in the buffer
                //specified in the result variable
                builder = new StringBuilder((int)result);
                result = GetLongPathName(path, builder, builder.Capacity);
                builder[0] = char.ToLower(builder[0]);
                return builder.ToString(0, (int)result);
            }

            return null;
        }

        private string FixPath(string originalPath)
        {
            
            string returnPath = removeTrailingSlash(originalPath); //remove \ if we had it at the end
            if (returnPath.Contains("\\\\"))
            {
                returnPath = returnPath.Replace("\\\\", "\\"); //remove any double \\ if I didn't write the string correctly, which I'm certainly not
            }
            returnPath = GetWindowsPhysicalPath(returnPath); //gets the correct capitalization of the filePath
            returnPath = returnPath.Substring(0, 1).ToUpper() + returnPath.Substring(1);
            return returnPath;
        }

        private void SetDirsForm_Shown(object sender, EventArgs e)
        {
            checkIfWereInGameFolder();
        }
    }
}
