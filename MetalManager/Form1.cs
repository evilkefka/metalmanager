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
using System.Diagnostics;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Collections;


namespace MetalManager
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        /// <summary>
        /// If the date hits March 29, 2023, the day the DLC is supposed to be release, warn the user that errors may occur
        /// </summary>
        private void AlertDLCUntested()
        {
            //this doesn't work, so I'm just not using it instead of figuring it out
            string warningGiven = ConfigurationManager.AppSettings["dlcwrng"];
            if (!string.IsNullOrEmpty(warningGiven)) return; //if dlcwrng in Config says anything, we will never show this again

            DateTime releaseOfDLC = new DateTime(2023, 3, 13, 0, 0, 1);
            DateTime today = DateTime.Today;

            if (today >= releaseOfDLC)
            {
                string dlcWarningTitle = "Warning: New Flames Found!";
                string dlcWarningMsg = "Note: DLC for Metal Hellsinger was scheduled to be released March 29, 2023. " +
                                       "Please be aware that this version of Metal Manager remains untested with new " +
                                       "content.\n\nAlternatively, until a new version of Metal Manager is released, you " +
                                       "can consult the Metal Hellsinger Discord server for any help regarding modding.\n\n" +
                                       "     (Metal Manager won't bother you about this again! <3)";

                MessageBox.Show(dlcWarningMsg, dlcWarningTitle);

                //AddOrUpdateAppSettings("dlcwrng", "1");
            }
        }


        private string dirsToList(string[] songs)
        {

            string resultStr = "";

            for (int i = 0; i < songs.Length; i++)
            {
                resultStr += songs[i];
            }

            return resultStr;
        }


        string[] customSong;
        string[] csSupportedLevels = new string[0]; //this isn't used, I don't think
        List<string> csSupLvls; //this is used, for the love of God, don't delete it

        /// <summary>
        /// Creates a list of strings respective to the custom songs that say which level each song supports
        /// </summary>
        private void storeModListInfo()
        {
            //this is used by Set List page. We're storing hidden information regarding each Mod's supported level
            //we do this so we don't have to search through the Mod's .json everytime we want to know the levels it supports

            //this function doesn't need the actual info, we just need to detect if we can find levels, and each one has a level. 
            // if it can find the level, have the level store the event ID. if it cant, have it store nothing i guess
            //for now it just store yes or no

            //DirectoryInfo[] songList = di.GetDirectories();
            //string songList = ((ListItem)setListCatalog.Items[1]).Path;


            //We always call this after loading the song list, so we're going to revolve this around the info(mods) that just got filled into that

            //string[] songList = 

            if (csSupLvls == null) csSupLvls = new List<string>();

            csSupLvls.Clear(); //first, clear it

            for (int i = 0; i < setListCatalog.Items.Count; i++)
            {
                string modName = setListCatalog.Items[i].ToString();//gives us the name of the song in the setListCatalog
                if (modName.ToString().Substring(0, 1) == "_") continue; //we shouldn't need this if setListCatalog was told to ignore it already
                if (modName == "(game)") continue;
                string csSupportString = getModSupportedLevels(modName);
                csSupLvls.Add(csSupportString);
                //////testFindJson.Text += songList[i] + "'s supported levels: " + csSupportString;
            }
        }

        private void moveCSSupportSpot(int origIndex, int direction, bool extreme = false)
        {

            //MessageBox.Show("Moving index " + origIndex + " to " + direction);

            int newIndex = -1;
            if (extreme)
            {
                if (direction == -1)
                {
                    newIndex = 0;
                }
                else if (direction == 1)
                {
                    newIndex = csSupLvls.Count - 1;
                }

                goto GotIndex;

            }


            // Calculate new index using direction
            newIndex = origIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= csSupLvls.Count)
                return; // Index out of range - nothing to do

            GotIndex:

            string originalSupportLine = csSupLvls[origIndex];
            csSupLvls.RemoveAt(origIndex);
            csSupLvls.Insert(newIndex, originalSupportLine);

        }

        private void swapCSSupportSpots(int firstIndex, int secondIndex)
        {
            string tmp = csSupLvls[firstIndex];
            csSupLvls[firstIndex] = csSupLvls[secondIndex];
            csSupLvls[secondIndex] = tmp;
        }

        private void cancelBfGWorker()
        {
            if (!BfGWorkerMain.IsBusy) return;

            BfGWorkerMain.CancelAsync();
        }

        private void reloadModsSelectionOrder()
        {
            mmLoading = true;
            setList_topLabel.Visible = true;
            string[] modList = loadModListFromConfig(null, false); //this loads the contents into the setListCatalog ListBox
            if (modList == null) { setList_topLabel.Visible = false; return; }
            fillSongSelection(modList);
            storeModListInfo(); //this stores the info for our song; specifically which levels it supports; the info is hidden and is used to quickly know what levels each mod has info for
            setOldSongsArray(); //this stores an array of info of our current customsongs.json file in the game folder

            mmLoading = false;
        }

        /// <summary>
        /// This resets our selections for each ComboBox in Set List to match the order in the Catalog
        /// </summary>
        private void reloadModsListViaCatalog()
        {
            mmLoading = true;
            List<string> modList = new List<string>();
            foreach (ListItem item in setListCatalog.Items)
            {
                modList.Add(item.Name);
            }
            fillSongSelection(modList.ToArray());
            storeModListInfo(); //this stores the info for our song; specifically which levels it supports; the info is hidden and is used to quickly know what levels each mod has info for
            setOldSongsArray(); //this stores an array of info of our current customsongs.json file in the game folder

            mmLoading = false;
        }


        public DirectoryInfo gameDir;
        //DirectoryInfo gameDir;
        //di is the directory where we store our mods
        public DirectoryInfo di;

        //DirectoryInfo di = new DirectoryInfo(@"R:\SteamLibrary\steamapps\common\Metal Hellsinger\MODS");
        //gameDir = new DirectoryInfo(@"R:\SteamLibrary\steamapps\common\Metal Hellsinger\Metal_Data\StreamingAssets");

        string gameDirStr = "";
        string modDirStr = "";
        private void GetDirectoriesFromConfig()
        {
            gameDirStr = ConfigurationManager.AppSettings["gameDirectory"];
            modDirStr = ConfigurationManager.AppSettings["modDirectory"];
            if (modDirStr == "" || modDirStr == null)
            {
                OpenSetDirsDialogue();
            }
            else
            {
                if (string.IsNullOrEmpty(gameDirStr))
                {
                    gameDir = null;
                } else
                {
                    gameDir = new DirectoryInfo(@gameDirStr);
                }
                di = new DirectoryInfo(@modDirStr);
            }
        }

        string catalogsort = null;
        bool slt = false;
        bool ort = false;


        /// <summary>
        /// Looks for certain settings that can be saved. Reads config, and sets them
        /// </summary>
        private void SetMMSettingsFromConfig()
        {
            string catalogsortStr = ConfigurationManager.AppSettings["catalogSort"];
            string sltStr = ConfigurationManager.AppSettings["showTutSL"];
            string ortStr = ConfigurationManager.AppSettings["showTutOr"];
            string allowASStr = ConfigurationManager.AppSettings["allowAS"];
            string org_slctIndxChngStr = ConfigurationManager.AppSettings["o_chnglvlindx"];


            //Show SetList tutorial is initally true; if it's false, take it away
            if (sltStr == "false")
            {
                tsm_showTutSetList.Checked = false;
                setListShowTutorial(false);
            }

            if (ortStr == "false")
            {
                tsm_showTutOrganizer.Checked = false;
                OrganizerShowTutorial(false);
            }

            if (string.IsNullOrEmpty(catalogsortStr))
            {
                catalogsort = "custom";
            } else
            {
                catalogsort = catalogsortStr;
            }
            if (string.IsNullOrEmpty(sltStr))
            {
                slt = false;
            } else
            {
                if (sltStr == "true")
                {
                    slt = true;
                } else
                {
                    slt = false;
                }
            }
            if (string.IsNullOrEmpty(sltStr))
            {

                ort = false;
            } else
            {
                if (ortStr == "true")
                {
                    ort = true;
                }
                else
                {
                    ort = false;
                }
            }
            if (string.IsNullOrEmpty(org_slctIndxChngStr))
            {

                org_selectIndexLevelChoice = "first";
            }
            else
            {
                //it's initially set to "first"
                if (org_slctIndxChngStr == "supported")
                {
                    org_selectIndexLevelChoice = "supported";
                    tsm_orgDntChngLvlSlct.Checked = false;
                    tsm_orgSlctFirstSupprtdLvl.Checked = true;
                    tsm_orgSlctFirstLevel.Checked = false;
                } else if (org_slctIndxChngStr == "none")
                {
                    org_selectIndexLevelChoice = "none";
                    tsm_orgDntChngLvlSlct.Checked = true;
                    tsm_orgSlctFirstSupprtdLvl.Checked = false;
                    tsm_orgSlctFirstLevel.Checked = false;
                }
            }


            if (sltStr == "false")
            {
                tsm_showTutSetList.Checked = false;
                setListShowTutorial(false);
            }

            //allow auto select's default is true
            if (string.IsNullOrEmpty(allowASStr))
            {
                tsm_AllowAutoSelect.Checked = true;
            } else
            {
                if (allowASStr == "false")
                {
                    tsm_AllowAutoSelect.Checked = false;
                }
            }



            //it's initially set to custom sort; if it still says custom, keep it that way
            if (catalogsort == "a-z")
            {
                tsm_sortAtoZ.Checked = true;
                tsm_sortZtoA.Checked = false;
                tsm_customSort.Checked = false;
                enableCustomSortButtons(false);
            } else if (catalogsort == "z-a")
            {
                tsm_sortZtoA.Checked = true;
                tsm_sortAtoZ.Checked = false;
                tsm_customSort.Checked = false;
                enableCustomSortButtons(false);
            }



        }

        private void setFileMenuSelections()
        {
            //sets certain selections in File menu, based on if we have a game directory, and a mod directory
            if (gameDir == null)
            {
                tsm_openStrmgAssts.Visible = false;
                tsm_linkGameDir.Visible = true;
                tsm_revertSetList.Visible = false;
            } else
            {
                tsm_linkGameDir.Visible = false;
                tsm_openStrmgAssts.Visible = true;
                tsm_revertSetList.Visible = true;
            }

            if (di == null)
            {
                tsm_changeModFolder.Visible = false;
                tsm_reloadMods.Visible = false;
                tsm_openModFolder.Visible = false;
                tsm_FileSep1.Visible = false;
                tsm_FileSep2.Visible = false;

                tsm_setModFolder.Visible = true;
            } else
            {
                tsm_setModFolder.Visible = false;

                tsm_changeModFolder.Visible = true;
                tsm_reloadMods.Visible = true;
                tsm_openModFolder.Visible = true;
                tsm_FileSep1.Visible = true;
                tsm_FileSep2.Visible = true;
            }

        }

        /// <summary>
        /// Known filesize of Metal Hellsinger's default Music.bank
        /// </summary>
        public long gameMBFileSize;
        /// <summary>
        /// Known filesize of Low Health Library's Music.bank
        /// </summary>
        public long LHLibraryFileSize;
        private void GetMusicBankFilesizes()
        {
            string origMusicBankFileSz = ConfigurationManager.AppSettings["gmbfs"];
            string LHLibraryFileSz = ConfigurationManager.AppSettings["lhlfs"];

            if (String.IsNullOrEmpty(origMusicBankFileSz))
            {
                gameMBFileSize = 17373248; //default
            }
            else
            {
                if (long.TryParse(origMusicBankFileSz, out long goodbye))
                {
                    gameMBFileSize = long.Parse(origMusicBankFileSz);
                }
            }


            if (String.IsNullOrEmpty(LHLibraryFileSz))
            {
                LHLibraryFileSize = 796256;
            }
            else
            {
                if (long.TryParse(LHLibraryFileSz, out long solong))
                {
                    LHLibraryFileSize = long.Parse(LHLibraryFileSz);
                }
            }
        }

        bool ModFolderHoldsOrgnlMusicBank = false;
        List<ListItem> SongsWithCustomMusicBanks = new List<ListItem>();
        private void GetAllCustomMusicBanks()
        {
            //since the user can make their own custom music.bank and it can be named whatever, we'll just grab all of them
            GetMusicBankFilesizes();
            string[] allMusicBankPaths = Directory.GetFiles(di.ToString(), "Music.bank", SearchOption.AllDirectories);
            bool foundGameMusicBank = false;
            bool foundLHLibrary = false;

            SongsWithCustomMusicBanks = new List<ListItem>();

            foreach (string musicBankpath in allMusicBankPaths)
            {

                FileInfo musicBankFile = new System.IO.FileInfo(musicBankpath);
                long thisMusicBanksFileSize = musicBankFile.Length;

                string[] foldersInPath = musicBankpath.Split('\\'); //splits our string into an array of string of directory names; the \\'s that were once there are now gone
                string directoryName = foldersInPath[foldersInPath.Length - 2];

                if (thisMusicBanksFileSize == gameMBFileSize)
                {
                    if (foundGameMusicBank) continue;

                    if (foundLHLibrary)
                    {
                        SongsWithCustomMusicBanks.Insert(1, new ListItem { Name = "Game's Default", Path = musicBankpath });
                    } else
                    {
                        SongsWithCustomMusicBanks.Insert(0, new ListItem { Name = "Game's Default", Path = musicBankpath });
                    }
                    foundGameMusicBank = true;
                }
                else if (thisMusicBanksFileSize == LHLibraryFileSize)
                {
                    if (foundLHLibrary) continue;
                    SongsWithCustomMusicBanks.Insert(0, new ListItem { Name = "The Library", Path = musicBankpath });
                    foundLHLibrary = true;
                }
                else
                {
                    //music.bank doesn't match our known filesizes
                    string directoryWereIn = musicBankpath.Replace("\\Music.bank", "");

                    //we only want to add it if we have this tied to a customsongs.json,
                    //or we have some sort of info establishing what it is
                    if (File.Exists(directoryWereIn + "\\customsongs.json") ||
                        File.Exists(directoryWereIn + "\\GUIDs.txt") ||
                        File.Exists(directoryWereIn + "\\Index.txt"))
                    {
                        SongsWithCustomMusicBanks.Add(new ListItem { Name = directoryName, Path = musicBankpath });
                    }




                }
            }




            ModFolderHoldsOrgnlMusicBank = foundGameMusicBank;
            //if we've ever seen the game's original Music.bank in Mods folder, ModFolderHoldsOriginalMusicBank gets set to true until mods reload
            //we'll store this boolean for later if we're replacing the Music.bank

            //if we never found and added the gameMusic bank from the Mods folder, we'll see if it's sitting in the actual game folder
            if (!foundGameMusicBank)
            {
                if (gameDir == null) return;
                if (!Directory.Exists(gameDir.ToString())) return;
                if (!File.Exists(gameDir + "\\Music.bank")) return;

                string gameMusicBankPath = gameDir.ToString() + "\\Music.bank";
                FileInfo gamesMusicBankFile = new System.IO.FileInfo(gameMusicBankPath);
                long gamesCrntMusicBanksFileSize = gamesMusicBankFile.Length;
                if (gamesCrntMusicBanksFileSize != gameMBFileSize) return;

                //if we got this far, we just found the game's original Music.bank in its StreamingAssets folder, allow it to be a selection

                if (foundLHLibrary)
                {
                    SongsWithCustomMusicBanks.Insert(1, new ListItem { Name = "Game's Default", Path = gameMusicBankPath });
                }
                else
                {
                    SongsWithCustomMusicBanks.Insert(0, new ListItem { Name = "Game's Default", Path = gameMusicBankPath });
                }
            }

        }




        private void OpenSetDirsDialogue()
        {
            using (SetDirsForm setD = new SetDirsForm())
            {
                setD.MyParentForm = this;
                if (setD.ShowDialog() == DialogResult.OK)
                {
                    gameDir = setD.GameDirVal;
                    di = setD.ModDirVal;
                    //gameDir will be null if checkmark was set to ignore
                }
                else
                {
                    MessageBox.Show("Directories were not set. Metal Manager will now close.", "No directories set");
                    System.Windows.Forms.Application.Exit();
                }
            }
        }


        List<string[]> ConfirmSuspendedSongs = new List<string[]>();

        /// <summary>
        /// Runs StartUpScan Form. Adds/removes/edits song settings, scans songs for errors, and verifies no duplicates exist
        /// </summary>
        private void OpenErrorGatekeeperDialogue(string summoner = null)
        {
            using (StartupScanForm startupScanner = new StartupScanForm(summoner))
            {
                startupScanner.MyParentForm = this;
                if (startupScanner.ShowDialog() == DialogResult.OK)
                {
                    ConfigurationManager.RefreshSection("appSettings");
                    ConfigurationManager.RefreshSection("CustomSongsConfig/Customsongs");
                    ConfirmSuspendedSongs = new List<string[]>();
                    string[][] songsWithProblems = startupScanner.SuspenededSongList;
                    int songsWithProbsNum = songsWithProblems.Count();


                    if (songsWithProbsNum > 0) {
                        ConfirmSuspendedSongs.AddRange(startupScanner.SuspenededSongList);

                    }
                }
                else
                {
                    //the the dialog box was closed before being given the OK, which is given once it's complete
                    CloseManagerFromError("startUpInteruption-01");
                }
            }
        }

        /// <summary>
        /// Shows a MessageBox with an identifiable error code before closing the program
        /// </summary>
        /// <param name="errorCode">Code to display to user to pinpoint where Manager crashed</param>
        public void CloseManagerFromError(string errorCode)
        {
            MessageBox.Show("Metal Manager encountered a catastrophic error and had to close.\nError Code: " + errorCode, "Crash and Burn");
            System.Windows.Forms.Application.Exit();
        }

        private string getModSupportedLevels(string mod)
        {
            //this function just performs getLevelSupport 8 times, returning a string that we can recognize/decipher later
            string fullModJson = SetList_GetModJson(mod);

            string result = "";
            int numberOfLevels = allLevelNames.Length;
            //if we want to change tutorial, this needs to be 9
            //we're doingthe tutorial...

            //making the condition i < levelsNames.length kept making it only run 5 times
            for (int i = 0; i < numberOfLevels; i++)
            {
                result += i; //allows us to differentiate levels in the string easier
                result += getLevelSupport(fullModJson, allLevelNames[i]);//this returns a string, either m, b, mb, or ""

            }


            return result;
        }




        private string getLevelSupport(string fullJson, string Level)
        {
            //retrieves the info for one level's custom music, for setlist
            string result = "";

            string capitalizeLevelName = "\"" + Level.Substring(0, 1).ToUpper() + Level.Substring(1) + "\"";
            int indexOfLevelInfo = fullJson.IndexOf(capitalizeLevelName);


            if (indexOfLevelInfo == -1)
            {
                //this level is not here
                //clearSongInfoBoxes();
                return result;
            }

            //if we got this far, we have info for the level

            int indexOfLevelInfoEnd = fullJson.IndexOf("} }", indexOfLevelInfo);
            if (indexOfLevelInfoEnd == -1)
            {
                //we have a problem, try to fix it
                fullJson = fullJson.Replace("\t", "");
                indexOfLevelInfoEnd = fullJson.IndexOf("}}", indexOfLevelInfo);
            }
            if (indexOfLevelInfoEnd == -1)
            {
                indexOfLevelInfoEnd = fullJson.IndexOf("}}", indexOfLevelInfo);
            }

            if (indexOfLevelInfoEnd == -1) return "error;";

            string fullLevelInfo = fullJson.Substring(indexOfLevelInfo, indexOfLevelInfoEnd - indexOfLevelInfo);


            int indexOfMainLevelMusic = fullLevelInfo.IndexOf("\"MainMusic\"");
            int indexOfBossFightMusic = fullLevelInfo.IndexOf("\"BossMusic\"");

            //check if we have MainMusic info in this level
            if (indexOfMainLevelMusic != -1)
            {
                //we have SOME kind of info for "MainMusic"
                result += "m";
            }


            if (indexOfBossFightMusic != -1)
            {
                //we have SOME kind of info for "BossMusic"       
                result += "b";

            } else if (Level == "\"Sheol\"")
            {
                //we don't have anything----read note below--.
                result += "u";//for "unsupported"            ↓
                //this has never come up because the Level doesn't have a capitalized level name
            }
            return result;
        }


        //DirectoryInfo gameDir = new DirectoryInfo(@"M:\steamLibraryExmple\steamapps\common\Metal Hellsinger\Metal_Data\StreamingAssets");
        /// <summary>
        /// Returns the game's current Json with normalized whitespace, while purging line returns, tabs, etc. Returns -1 if gameDir isn't set, -2 if no customsongs.json yet
        /// </summary>
        /// <returns></returns>
        private string getCurrentCustomsongsJson(bool removeWhiteSpace = true)
        {
            //with this function, we're returning a string of the information from our actual customsongs.json that the game reads (in the StreamingAssets folder)
            if (gameDir == null) return "-1";
            if (!Directory.Exists(gameDir.ToString())) return "-1";
            if (!File.Exists(gameDir + "\\customsongs.json")) return "-2";

            string currentJSONString = gameDir + "\\customsongs.json";
            using (StreamReader sr = File.OpenText(@currentJSONString))
            {
                string s = "";

                string fullText = sr.ReadToEnd();
                string trimmedLine = NormalizeWhiteSpace(fullText);
                if (removeWhiteSpace)
                    s = trimmedLine;
                else
                    s = fullText;

                return s;
            }
        }



        List<ListItem> modsWithCustMusicBank = new List<ListItem>(); //we're going to store TWO things in here instead now—the mod name, and its path


        string[] defaultMainSongNames = { "This Is the End", "Stygia", "Burial At Night", "This Devastation", "Poetry of Cinder", "Dissolution", "Acheron", "Silent No More", "Through You" };
        string[] defaultBossSongNames = { "Blood and Law", "Infernal Invocation I", "Infernal Invocation II", "Infernal Invocation III", "Infernal Invocation II", "Infernal Invocation I", "Infernal Invocation III", "No Tomorrow" };


        /// <summary>
        /// Uses a string[] to fill all valid songs into each Level's music selection ComboBox
        /// </summary>
        /// <param name="modList"></param>
        private void fillSongSelection(string[] modList)
        {
            //this fills the Items for each ComboBox in our SetList page; the Valid Mod list is filled as each Combo's selection
            //string[] modListArray = modList.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries); //our modList is a string that looks like:  Bodies::Prey For Me::Unstoppable;

            ComboBox[] songSelectBox = { mainCombo1, bossCombo1, mainCombo2, bossCombo2, mainCombo3, bossCombo3, mainCombo4, bossCombo4, mainCombo5, bossCombo5, mainCombo6, bossCombo6, mainCombo7, bossCombo7, mainCombo8, mainCombo9 };
            for (int i = 0; i < songSelectBox.Length; i++)
            {
                string songBoxText = songSelectBox[i].Text;
                songSelectBox[i].DataSource = modList.ToList();
                songSelectBox[i].Text = songBoxText;
                //songSelectBox[i].DisplayMember = "Name";
                // songSelectBox[i].ValueMember = "Path";


                //songSelectBox[i].Items.Clear(); //first, clear each combo box
                //songSelectBox[i].Items.AddRange(modList.ToArray());


                /*for (int z = 0; z < modListArray.Length; z++)
                {
                    songSelectBox[i].Items.Add(modListArray[z]);
                }*/

            }


        }

        /// <summary>
        /// Updates AppSettings with "key" name, to have "value"; adds the setting if it doesn't exist yet
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
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

        /// <summary>
        /// Opens a Directory Dialog box to set gameDir
        /// </summary>
        private void GetGameDir()
        {
            //This SETS our Game directory!
            //di is modpath, gameDir is game directory
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
                        gameDir = new DirectoryInfo(@streamingAssetsDir);
                        AddOrUpdateAppSettings("gameDirectory", streamingAssetsDir);

                        //we're going to reset these too, since it wasn't available before
                        setOldSongsArray(); //this stores an array of info of our current customsongs.json file in the game folder
                        loadOldInfoIntoSetList(false); //this loads the array from the previous line into the fields
                        setFileMenuSelections();
                        if (getCurrentCustomsongsJson().Length > 2)
                        {
                            //getCurrentCustomsongsJson would have been "-1" or "-2" if there's nothing to grab
                            //we didn't have a "Current customsongs.json" in Organizer's Listbox yet, so we're adding one now
                            listBox1.Items.Insert(0, new ListItem { Name = "Current customsongs.json", Path = gameDir + "\\customsongs.json" });
                        }

                        saveCurrSLButton.Enabled = true; //this was disabled when we didn't have a game linked yet
                        reApplyAllBanksBtn.Enabled = true;
                        cleanUpSABtn.Enabled = true;

                        //gameDirInfo.Text = "Game Directory Found!";
                    } else
                    {
                        MessageBox.Show("Please select Metal Hellsinger's game directory or its StreamingAssets folder.");
                        //gameDirInfo.Text = "No game directory found!";
                    }

                }
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
            } else if (dirs.Last() == "Metal_Data")
            {
                for (int i = 0; i < dirs.Length - 1; i++)
                {
                    exeVerifyPath += dirs[i] + "\\";
                }
            } else if (dirs.Last() == "Metal Hellsinger")
            {
                exeVerifyPath = String.Join("\\", dirs);
            }

            if (Directory.Exists(@exeVerifyPath))
            {
                string look4Game = exeVerifyPath + "\\Metal.exe";
                //string look4Ignore = exeVerifyPath + "\\ignore.txt";
                if (File.Exists(@look4Game))
                {
                    returnString = exeVerifyPath + "\\Metal_Data\\StreamingAssets";
                    return returnString;
                }
                /* Used before we had the checkbox
                if (File.Exists(@look4Ignore))
                {
                    returnString = exeVerifyPath;
                    return returnString;
                }
                else
                {
                    return null;
                }*/
            }

            //if we got this far, something goofed
            return null;





        }

        private void OpenDir(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = dirPath,
                    FileName = "explorer.exe"
                };
                Process.Start(startInfo);

            } else
            {
                MessageBox.Show("Error: Directory could not be found.");
            }
        }

        //This is the function that runs when we want to set our Mods folder to something else
        //I should have named it something besides get
        /// <summary>
        /// Opens up directory dialog. If a folder was selected, sets Mod folder(di), and updates its value in config
        /// </summary>
        private void GetModsFolder()
        {
            //di is ModPath, gameDir is gamedirectory
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
                    di = new DirectoryInfo(@modPath);
                    AddOrUpdateAppSettings("modDirectory", di.ToString());
                    /*
                    string modListDir = di.ToString();
                    modListDir = modListDir.Replace("\\\\", "\\");
                    modListDir = pathShortener(modListDir, 40);*/
                    //ModDirLabel.Text = modListDir;
                }
            }
        }

        /// <summary>
        /// Hopefully this does exactly what it says it does, or I'm going to run towards the nearest living thing and kill it.
        /// </summary>
        public static string[][] ReadTheGODDAMNConfigFile()
        {
            List<string[]> CSListFromConfig = new List<string[]>();

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            var songNodes = xmlDoc.SelectNodes("//CustomSongsConfig/Customsongs/add");
            foreach (XmlNode songNode in songNodes)
            {
                XmlAttributeCollection nodeAtt = songNode.Attributes;
                string[] songInfo = { nodeAtt["name"].Value.ToString(),
                nodeAtt["path"].Value.ToString(),
                nodeAtt["lwt"].Value.ToString(),
                nodeAtt["lvt"].Value.ToString()};
                CSListFromConfig.Add(songInfo);

            }

            return CSListFromConfig.ToArray();
        }


        BindingSource ValidSongBindSrc = new BindingSource();
        List<ListItem> validSongsLI = new List<ListItem>();

        /// <summary>
        /// Recorded from loadModListFromConfig
        /// </summary>
        int numberOfModsWithErrors = 0;

        /// <summary>
        /// Reads the config file to load Catalog with song selections
        /// </summary>
        /// <param name="lBox">The only function that has this to null, isn't used in my program. Don't set this to null.</param>
        /// <param name="storeCustomMusicBank"></param>
        /// <returns></returns>
        private string[] loadModListFromConfig(ListBox lBox, bool storeCustomMusicBank = false)
        {

            validSongsLI = new List<ListItem>();
            List<string> validSongs = new List<string>();
            numberOfModsWithErrors = 0;

            if (lBox != null) lBox.Items.Clear();
            string modListString = "";
            if (di == null) { /*ModDirLabel.Text = "No Mod directory set!";*/ return null; }
            if (!di.Exists) { /*ModDirLabel.Text = "No Mod directory set!";*/ return null; }

            string modListDir = di.ToString();
            modListDir = modListDir.Replace("\\\\", "\\");
            modListDir = pathShortener(modListDir, 40);
            //ModDirLabel.Text = modListDir;

            string[][] songListScrapedFromConfig = ReadTheGODDAMNConfigFile();

            /*
            foreach (var endpoint in ConfigDataDaddy.Customsongs.CustomsongsList)
            {
                //string securityGroups = string.Join(",", endpoint.SongInfo.SecurityGroupsAllowedToSaveChanges);
                MessageBox.Show("Adding: " + endpoint.Name + "\n Path: \n" + endpoint.SongInfo.Path);
                lBox.Items.Add(new ListItem { Name = endpoint.Name, Path = di + endpoint.SongInfo.Path + "\\customsongs.json" });
                if(endpoint.SongInfo.LastVerifiedTime != "suspended")
                {
                    modListString += endpoint.Name + "::";//we return the modListString to know what to fill our ComboBoxes with in SetList
                }  
            }
            */

            foreach (string[] sngInfo in songListScrapedFromConfig)
            {
                //if lbox was null, ignore this
                if (lBox != null)
                {
                    //we don't want setListCatalog to have "Current customsongs.json" in it
                    if (lBox.Name == "setListCatalog")
                    {
                        if (sngInfo[0] != "(game)")
                        {
                            lBox.Items.Add(new ListItem { Name = sngInfo[0], Path = di + sngInfo[1] + "\\customsongs.json" });
                        }
                    } else if (lBox.Name == "listBox1")
                    {
                        if (sngInfo[0] == "(game)")
                        {
                            //lBox.Items.Add(new ListItem { Name = sngInfo[0], Path = di + sngInfo[1] + "\\customsongs.json" });
                            listBox1.Items.Insert(0, new ListItem { Name = "Current customsongs.json", Path = gameDir + "\\customsongs.json" });
                            //at the moment, our (game) spot can be anywhere in the Config's list, but we always put it to the top
                            //we could do something where we just push it to the top
                        } else
                        {
                            lBox.Items.Add(new ListItem { Name = sngInfo[0], Path = di + sngInfo[1] + "\\customsongs.json" });
                        }
                    }
                }
                //lBox.Items[lBox.Items.Count].Cont
                if (sngInfo[0] != "(game)" && (sngInfo[3] == "1" || sngInfo[3] == "2"))
                {
                    //we have a song from config that isn't the game, and it's valid
                    modListString += sngInfo[0] + "::";//we return the modListString to know what to fill our ComboBoxes with in SetList; we don't use this anymore
                    validSongs.Add(sngInfo[0]);
                    validSongsLI.Add(new ListItem { Name = sngInfo[0], Path = di + sngInfo[1] + "\\customsongs.json" });
                } else if (sngInfo[0] != "(game)" && ((sngInfo[3] != "1" && sngInfo[3] != "2")))
                {
                    //we found a mod with an error, we're going to just say so in case it's the only one(so we don't block it with "No custom songs found" panel)
                    numberOfModsWithErrors++;
                }
            }

            /*
            DirectoryInfo[] directoriesInModFolder = di.GetDirectories();
            //string[] songs = { "Hey Jude", "Yesterday", "Obladi Oblada" };

            //we store an array of strings that say EVERY folder that has a "customsongs.json" in it
            //we will have to look to make sure we're not in a "_Original" folder

            string[] allJSONPaths = Directory.GetFiles(di.ToString(), "customsongs.json", SearchOption.AllDirectories);

            foreach (string path in allJSONPaths)
            {
                string[] foldersInPath = path.Split('\\'); //splits our string into an array of string of directory names; the \\'s that were once there are now gone
                if (foldersInPath[foldersInPath.Length - 2] == "_Original")
                {
                    //we found a JSON in a folder with an Original file. We could store it later if we want a "restore" button to only appear if there's a JSON in an _Original folder
                    continue;
                }
                //if we got this far, we see a customsongs.json, and it's not in an "Original" folder

                string NameOfMod = foldersInPath[foldersInPath.Length - 2]; //-1 would give us the filename of the Bank, -2 gives us the foldername of whatever harbors it

                //listBox1.Items.Add(NameOfMod);
                //listBox1.Items[listBox1.Items.Count-1].
                
            }*/


            /*
            if (storeCustomMusicBank)
            {
                string[] allMusicDotBanks = Directory.GetFiles(di.ToString(), "Music.bank", SearchOption.AllDirectories); //gets a list of all directories/subdirectories with a "Music.Bank" file

                foreach (string path in allMusicDotBanks)
                {
                    //for each path that we found a Music.bank file in it....

                    string[] foldersInPath = path.Split('\\'); //splits our string into an array of string of directory names; the \\'s that were once there are now gone

                    //We should never be in an "_Original" folder

                    string NameOfMod = foldersInPath[foldersInPath.Length - 2]; //-1 would give us the filename of the Bank, -2 gives us the foldername of whatever harbors it

                    string[] custMusicInfo = { NameOfMod, path };

                    FileInfo fi = new FileInfo(path);
                    if (fi.Length == 796256)
                    {
                        //we found a Music.bank with a filesize of 796256—we found TheLibrary
                    }

                    modsWithCustMusicBank.Add(new ListItem { Name = NameOfMod, Path = path });


                }
            }*/
            ValidSongBindSrc.DataSource = validSongsLI;
            return validSongs.ToArray();

        }

        /// <summary>
        /// Reads the Config file, and fills both Catalog Listboxes based on order of custom song entries
        /// </summary>
        private void SetToConfigSort()
        {
            if (setListCatalog.Items.Count == 0) return;

            validSongsLI = new List<ListItem>();
            List<string> validSongs = new List<string>();

            //lBox.Items.Clear();
            bool hadCurrCSjson = false;
            if (listBox1.Items.Count == 0) goto SkipLookingForCurrentJson;
            if (((ListItem)listBox1.Items[0]).Name == "Current customsongs.json")
            {
                listBox1.Items.RemoveAt(0);
                hadCurrCSjson = true;
            }

        SkipLookingForCurrentJson:

            listBox1.Items.Clear();
            setListCatalog.Items.Clear();
            string modListString = "";
            if (di == null) { return; }
            if (!di.Exists) { return; }

            if (hadCurrCSjson)
            {
                listBox1.Items.Insert(0, new ListItem { Name = "Current customsongs.json", Path = gameDir + "\\customsongs.json" });
            }

            string modListDir = di.ToString();
            modListDir = modListDir.Replace("\\\\", "\\");
            modListDir = pathShortener(modListDir, 40);
            //ModDirLabel.Text = modListDir;

            string[][] songListScrapedFromConfig = ReadTheGODDAMNConfigFile();

            foreach (string[] sngInfo in songListScrapedFromConfig)
            {
                if (sngInfo[0] == "(game)") continue;
                listBox1.Items.Add(new ListItem { Name = sngInfo[0], Path = di + sngInfo[1] + "\\customsongs.json" });
                setListCatalog.Items.Add(new ListItem { Name = sngInfo[0], Path = di + sngInfo[1] + "\\customsongs.json" });
            }



        }


        /// <summary>
        /// This changes the order of the songs permanently in our actual Config file
        /// </summary>
        /// <param name="sName">Name of the song to find in config</param>
        /// <param name="direction">-1 when moving Up, 1 when moving Down</param>
        /// <param name="topOrBottom">If true, will move to index 0 for -1, or last index for 1</param>
        public static void ChangeCatalogOrder(string sName, int direction, bool topOrBottom = false)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            XmlNode songBeingMoved = xmlDoc.SelectSingleNode("//CustomSongsConfig/Customsongs/add[@name='" + sName + "']");
            XmlNode parent = songBeingMoved.ParentNode;
            if (direction == -1)
            {

                if (topOrBottom == false) {

                    //move up
                    XmlNode previousNode = songBeingMoved.PreviousSibling;
                    if (previousNode == null) return; //we're at the top
                    if (previousNode.Attributes["name"].Value.ToString() == "(game)")
                    {
                        previousNode = previousNode.PreviousSibling;
                    }
                    if (previousNode == null) return; //we're at the top


                    parent.InsertBefore(songBeingMoved, previousNode);
                } else
                {
                    //move to top     
                    parent.InsertBefore(songBeingMoved, parent.FirstChild);
                }

            } else if (direction == 1)
            {
                //move down
                if (topOrBottom == false)
                {
                    XmlNode nextNode = songBeingMoved.NextSibling;
                    if (nextNode == null) return;
                    if (nextNode.Attributes["name"].Value.ToString() == "(game)")
                    {
                        nextNode = nextNode.NextSibling;
                    }
                    if (nextNode == null) return;

                    parent.InsertAfter(songBeingMoved, nextNode);
                } else
                {
                    //move to bottom
                    parent.InsertAfter(songBeingMoved, parent.LastChild);
                }
            }



            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("CustomSongsConfig/Customsongs");
        }


        /// <summary>
        /// Moves an item in a listBox up or down
        /// </summary>
        /// <param name="listBox">The ListBox to alter</param>
        /// <param name="direction">-1 for up, 1 for down</param>
        /// <param name="extreme">If true, sets item to index 0 for -1, sets item to last index for 1</param>
        static void ListBoxMoveItem(ListBox listBox, int direction, bool extreme = false)
        {
            // Checking selected item
            if (listBox.SelectedItem == null || listBox.SelectedIndex < 0)
                return; // No selected item

            int newIndex = -1;
            if (extreme)
            {
                if (direction == -1)
                {
                    if (((ListItem)listBox.Items[0]).Name == "Current customsongs.json")
                    {
                        newIndex = 1;
                    }
                    else
                    {
                        newIndex = 0;
                    }

                }
                else if (direction == 1)
                {
                    newIndex = listBox.Items.Count - 1;
                }
                goto GotIndex;
            }


            // Calculate new index using move direction
            newIndex = listBox.SelectedIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= listBox.Items.Count)
                return; // Index out of range - nothing to do

            if (newIndex == 0 && ((ListItem)listBox.Items[0]).Name == "Current customsongs.json")
            {
                return;
            }

        GotIndex:

            object selected = listBox.SelectedItem;

            // Save checked state if it is applicable


            // Removing removable element
            listBox.Items.Remove(selected);
            // Insert it in new position
            listBox.Items.Insert(newIndex, selected);
            // Restore selection
            listBox.SetSelected(newIndex, true);
        }

        private void Catalog_MouseDown(object sender, MouseEventArgs e)
        {
            ListBox lBox = sender as ListBox;

            if (e.Button == MouseButtons.Left)
            {
                int indexOfItemUnderCursor = lBox.IndexFromPoint(e.Location);
                if (lBox.Name == "listBox1" && indexOfItemUnderCursor != -1)
                {
                    lBox.SelectedIndex = indexOfItemUnderCursor;
                }

                //whatever we were selecting, yet it'll have the same information on the page (the selection is changing, but not the selected index)
            } else if (e.Button == MouseButtons.Right)
            {
                //We've right-clicked a Catalog ListBox
                rightClickedListBox = null;

                //select the item under the mouse pointer
                int indexOfItemUnderCursor = lBox.IndexFromPoint(e.Location);
                if (indexOfItemUnderCursor == -1)
                {
                    if (lBox.Name == "setListCatalog")
                    {
                        lBox.SelectedIndex = lBox.IndexFromPoint(e.Location);
                    }
                    //we won't reset our selection if we right-clicked and it wasn't on an item for Organizer
                } else
                {
                    lBox.SelectedIndex = lBox.IndexFromPoint(e.Location);
                }

                if (indexOfItemUnderCursor != -1)
                {
                    //we rightclicked over an item
                    rightClickedListBox = lBox;
                    if (causedSaveChngsWarning)
                    {
                        //we triggered the saveChanges warning, so we're going to block this
                        causedSaveChngsWarning = false;
                        return;
                    }


                    listSlctnRightClickMenu.Show(Cursor.Position);


                    //show these, whether enabled or not
                    moveToTopToolStripMenuItem.Visible = true;
                    moveToBottomToolStripMenuItem.Visible = true;
                    moveUpToolStripMenuItem.Visible = true;
                    moveDownToolStripMenuItem.Visible = true;
                    toolStripSeparator4.Visible = true;

                    //We keep "Current customsongs.json" at the top
                    if (((ListItem)lBox.Items[indexOfItemUnderCursor]).Name == "Current customsongs.json")
                    {
                        enableCustomSortButtons(false, "Cannot move item");
                        return;
                    }

                    //if we're editing a song in Organizer, we don't want to allow sorting
                    if (tabControl1.SelectedIndex == 1 &&
                        (mSaveLevelInfo.Enabled || bSaveLevelInfo.Enabled))
                    {
                        enableCustomSortButtons(false, "Cannot move while editing song");
                        return;
                    }

                    //lastly, check if we're on custom sort. if it's alphabetical, disable them. if not, we can finally enable them
                    if (catalogsort == "a-z" || catalogsort == "z-a")
                    {
                        enableCustomSortButtons(false);
                    }
                    else
                    {
                        enableCustomSortButtons(true, "");
                    }

                } else
                {
                    //we rightclicked, but it wasn't over an item
                    //we'll take away the "Move Up", "Move Down" selections
                    listSlctnRightClickMenu.Show(Cursor.Position);
                    rightClickedListBox = lBox;


                    moveToTopToolStripMenuItem.Visible = false;
                    moveToBottomToolStripMenuItem.Visible = false;
                    moveUpToolStripMenuItem.Visible = false;
                    moveDownToolStripMenuItem.Visible = false;
                    toolStripSeparator4.Visible = false;
                }
            }
        }

        private void refreshListSlctns(object sender, EventArgs e)
        {
            //makes the selection go away if we focus on something else
            ListBox lb = sender as ListBox;
            lb.SelectedItem = null;


        }

        Color standardSongLBColor = Color.FromArgb(255, 255, 255, 255);
        Color standardSongLBColorSlctd = SystemColors.Highlight;
        Color suspendedSongLBColor = Color.FromArgb(255, 255, 128, 128);
        Color suspendedSongLBColorSlctd = Color.FromArgb(255, 64, 0, 0);
        private void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb.Items.Count == 0) return;

            string songNameBeingDrawn = ((ListItem)lb.Items[e.Index]).Name;
            bool suspendedSng = false;
            foreach (string[] suspendedSong in ConfirmSuspendedSongs)
            {
                if (songNameBeingDrawn == suspendedSong[0])
                {
                    //the name being drawn matches a song that's been suspeneded
                    suspendedSng = true;
                    break;
                }
                if (suspendedSong[0] == "(game)" && songNameBeingDrawn == "Current customsongs.json")
                {
                    suspendedSng = true;
                    break;
                }
            }

            // See if the item is selected.
            if ((e.State & DrawItemState.Selected) ==
                DrawItemState.Selected)
            {
                // Selected.
                if (suspendedSng)
                {
                    //suspended song
                    e.DrawBackground();
                    Graphics g = e.Graphics;
                    g.FillRectangle(new SolidBrush(suspendedSongLBColorSlctd), e.Bounds);
                    e.DrawFocusRectangle();
                }
                else
                {
                    //standard song
                    e.DrawBackground();
                    Graphics g = e.Graphics;
                    g.FillRectangle(new SolidBrush(standardSongLBColorSlctd), e.Bounds);
                    e.DrawFocusRectangle();
                }
                e.Graphics.DrawString(lb.Items[e.Index].ToString(),
         e.Font, Brushes.White, e.Bounds, StringFormat.GenericDefault);

            }
            else
            {
                // Not selected
                if (suspendedSng)
                {
                    //suspended song
                    e.DrawBackground();
                    Graphics g = e.Graphics;
                    g.FillRectangle(new SolidBrush(suspendedSongLBColor), e.Bounds);
                    e.DrawFocusRectangle();
                }
                else
                {
                    //standard song
                    e.DrawBackground();
                    Graphics g = e.Graphics;
                    g.FillRectangle(new SolidBrush(standardSongLBColor), e.Bounds);
                    e.DrawFocusRectangle();
                }

                using (SolidBrush br = new SolidBrush(e.ForeColor))
                {
                    e.Graphics.DrawString(lb.Items[e.Index].ToString(),
         e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
                }
            }

        }


        private string loadModListWithSubs(ListBox lBox, bool storeCustomMusicBank = false)
        {
            //this returns a string that says the names of all VALID Mod selections, searching even through subdirectories for a customsongs.json
            //example of unvalid Mod is one in an _Original folder

            lBox.Items.Clear();
            string modListString = "";
            if (di == null) { /*ModDirLabel.Text = "No Mod directory set!";*/ return ""; }
            if (di.Exists)
            {
                //since we know it, change the label:
                string modListDir = di.ToString();
                modListDir = modListDir.Replace("\\\\", "\\");
                modListDir = pathShortener(modListDir, 40);
                /*ModDirLabel.Text = modListDir;*/

                DirectoryInfo[] directoriesInModFolder = di.GetDirectories();
                //string[] songs = { "Hey Jude", "Yesterday", "Obladi Oblada" };

                //we store an array of strings that say EVERY folder that has a "customsongs.json" in it
                //we will have to look to make sure we're not in a "_Original" folder

                string[] allJSONPaths = Directory.GetFiles(di.ToString(), "customsongs.json", SearchOption.AllDirectories);

                foreach (string path in allJSONPaths)
                {
                    string[] foldersInPath = path.Split('\\'); //splits our string into an array of string of directory names; the \\'s that were once there are now gone
                    if (foldersInPath[foldersInPath.Length - 2] == "_Original")
                    {
                        //we found a JSON in a folder with an Original file. We could store it later if we want a "restore" button to only appear if there's a JSON in an _Original folder
                        continue;
                    }
                    //if we got this far, we see a customsongs.json, and it's not in an "Original" folder

                    string NameOfMod = foldersInPath[foldersInPath.Length - 2]; //-1 would give us the filename of the Bank, -2 gives us the foldername of whatever harbors it

                    //listBox1.Items.Add(NameOfMod);
                    //listBox1.Items[listBox1.Items.Count-1].
                    lBox.Items.Add(new ListItem { Name = NameOfMod, Path = path });
                    modListString += NameOfMod + "::";//we return the modListString to know what to fill our ComboBoxes with in SetList
                }


                if (storeCustomMusicBank)
                {
                    string[] allMusicDotBanks = Directory.GetFiles(di.ToString(), "Music.bank", SearchOption.AllDirectories); //gets a list of all directories/subdirectories with a "Music.Bank" file

                    foreach (string path in allMusicDotBanks)
                    {
                        //for each path that we found a Music.bank file in it....

                        string[] foldersInPath = path.Split('\\'); //splits our string into an array of string of directory names; the \\'s that were once there are now gone

                        //We should never be in an "_Original" folder

                        string NameOfMod = foldersInPath[foldersInPath.Length - 2]; //-1 would give us the filename of the Bank, -2 gives us the foldername of whatever harbors it

                        string[] custMusicInfo = { NameOfMod, path };

                        FileInfo fi = new FileInfo(path);
                        if (fi.Length == 796256)
                        {
                            //we found a Music.bank with a filesize of 796256—we found TheLibrary
                        }

                        modsWithCustMusicBank.Add(new ListItem { Name = NameOfMod, Path = path });



                    }
                }

                return modListString;
            }
            else
            {
                //Alert! No Mods folder!
                /*ModDirLabel.Text = "No Mod directory set!";*/
                return "";
            }
        }


        //this is the same as enable, but enable thinks more...
        private void disableGrabLvlButton(Button whichButton, string replacementText = "")
        {
            whichButton.Text = replacementText;
            whichButton.Enabled = false;
        }

        private bool comboBoxSelectionMatchesOld(ComboBox cBox)
        {
            string boxCalledNStr = cBox.Name.Substring(cBox.Name.Length - 1, 1);
            int whichOldIndex = Int32.Parse(boxCalledNStr);

            int mIndex = cBox.SelectedIndex;
            if (cBox.Name.Substring(0, 4) == "main")
            {
                if (mIndex == currentSetListIndexes_main[whichOldIndex])
                {
                    return true;
                }
            } else if (cBox.Name.Substring(0, 4) == "boss")
            {
                if (mIndex == currentSetListIndexes_boss[whichOldIndex])
                {
                    return true;
                }
            }

            return false;
        }



        private void OnKeyDown_GrabLvlBox(object sender, KeyEventArgs e)
        {
            //we're going to look and see if our user hit ENTER

            ComboBox combo = sender as ComboBox;
            if (e.KeyCode == Keys.Return)
            {

                if (!wasComboBoxChanged(combo)) return;
                setGrabLvlButton(combo);
                ////testFindJson.Text += " .hitEnter. ";
                e.SuppressKeyPress = true;
            }

            //I don't think this is working
            if (e.KeyCode == Keys.Tab)
            {
                ////testFindJson.Text = "HI";

                setGrabLvlButton(combo);
                e.SuppressKeyPress = true;
            }


            string lvlNumStr = combo.Name.Substring(combo.Name.Length - 1, 1);
            int lvlNum = Int32.Parse(lvlNumStr); //gives 1-based index
            lvlNum -= 1;

            alertLevelIfModIntegrityComprimised(lvlNum, combo); //this just checks for 



        }

        private void explainAutoSelect()
        {
            SetList_DebugLabel1.Text = "When selecting a mod that has no info for the Level we're changing, this will";
            SetList_DebugLabel2.Text = "automatically select the most similar level we can find.";
            SetList_DebugLabel1.Visible = true;
            SetList_DebugLabel2.Visible = true;
        }
        private void stopExplainingAutoSelect()
        {
            if (SetList_DebugLabel1.Text == "When selecting a mod that has no info for the Level we're changing, this will")
            {
                SetList_DebugLabel1.Text = ""; //why am I doing this part?
                SetList_DebugLabel2.Text = "";
                SetList_DebugLabel1.Visible = false;
                SetList_DebugLabel2.Visible = false;
            }
        }

        private void AutoSelectHover(object sender, EventArgs e)
        {
            explainAutoSelect();
        }

        private void AutoSelectLeave(object sender, EventArgs e)
        {
            stopExplainingAutoSelect();
        }


        /// <summary>
        /// Checks if two mods share the same Event ID, without matching the same filename for Bank file
        /// </summary>
        /// <param name="levelNum">The zero-based index of our song in levelGroupBoxes array</param>
        /// <param name="combo">The ComboBox that just got called</param>
        private void alertLevelIfModIntegrityComprimised(int levelNum, ComboBox combo)
        {
            //before we do all this, turn the alertInfo off to reset it
            alertLevel(levelNum, false);
            ////testFindJson.Text += "Alert test running";

            if (checkIfTwoModsSelected(levelNum))
            {
                //two mods are selected

                //find out which mods, and what level we want to grab info from the mod
                //the combo box has our mod, the grabLvlButton has the level info
                Button mGrabLvlButton = getGrabLvlBtnFromCombo(combo, "m");
                if (mGrabLvlButton == null) { ////testFindJson.Text += "...nulll..."; 
                    return; }
                Button bGrabLvlButton = getGrabLvlBtnFromCombo(combo, "b");
                if (bGrabLvlButton == null) { return; }
                int mainLvlNum = getLevelNumFromModGrabLvlButton(mGrabLvlButton);
                int bossLvlNum = getLevelNumFromModGrabLvlButton(bGrabLvlButton);

                ComboBox mainMusicCombo = getComboFromCombo(combo, "m"); //doesn't matter if we're messing with the main or the boss music when we set this off, we're going to grab both, based on
                ComboBox bossMusicCombo = getComboFromCombo(combo, "b"); //our names, since mainCombo2 is next to bossCombo2, mainCombo4 is next to bossCombo4, etc.

                if (mainMusicCombo == null) return;
                if (bossMusicCombo == null) return;




                string mMusicSelection = mainMusicCombo.Text;

                string bMusicSelection = bossMusicCombo.Text;

                //////testFindJson.Text += " Checking (" + mainLvlNum + ", " + bossLvlNum + ", " + mMusicSelection + ", " + bMusicSelection + ")";


                //checkLevelsModsIntegrity will return false if the program catches that we have mods with different file names yet the same bank ID
                if (checkLevelsModsIntegrity(mainLvlNum, bossLvlNum, mMusicSelection, bMusicSelection) == false)
                {
                    alertLevel(levelNum);

                }

            }
        }

        private ComboBox getComboFromCombo(ComboBox calledBox, string return_M_or_B)
        {
            //we use this code to get either the main or boss Combo Box, based on which combo we're affecting.
            //we use this code when we want to do something to the Main AND the Boss combo boxes for one level. We choose these boxes by this one's ID number
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };

            int boxCurrentlySelected = -1;

            if (calledBox.Name.Substring(0, 1) == "m")
            {

                //boxCurrentlySelected = Array.FindIndex(mainCBox, element => element.Name == calledBox.Name); //this is giving us the 0-based index, though our combo box name is 1-based
                //^^ this won't work for some reason
                for (int i = 0; i < mainCBox.Length; i++)
                {
                    if (calledBox == mainCBox[i])
                    {
                        boxCurrentlySelected = i;
                    }
                }

            } else if (calledBox.Name.Substring(0, 1) == "b")
            {

                //boxCurrentlySelected = Array.FindIndex(mainCBox, element => element.Name == calledBox.Name); //this is giving us the 0-based index, though our combo box name is 1-based
                //^^ this won't work for some reason
                for (int i = 0; i < bossCBox.Length; i++)
                {
                    if (calledBox == bossCBox[i])
                    {
                        boxCurrentlySelected = i;
                    }
                }
            }




            if (return_M_or_B == "m")
            {
                return mainCBox[boxCurrentlySelected];
            }

            return bossCBox[boxCurrentlySelected];

        }

        private int getLevelNumFromModGrabLvlButton(Button whichButton)
        {
            //name is confusing
            //this checks a certain mod's grabLvlButton, and returns the 0-based index of the level we want to grab from it
            //this is only used to alert the user if mod integrity compromised
            string selectedLevelAbbreviation = whichButton.Text; //this gets whatever our mod's grabLvlButton is selected on
                                                                 //int levelCurrentlySelected = Array.FindIndex(LvlAbbreviations, element => element == selectedLevelAbbreviation); //this converts the letter in our GrabLvl box to a number
                                                                 //I hate findIndex so much


            //levelCurrentlySelected = Array.FindIndex(LvlAbbreviations, t => t == whichButton.Text);


            int levelCurrentlySelected = -1;
            for (int i = 0; i < LvlAbbreviations.Length; i++)
            {
                if (whichButton.Text == LvlAbbreviations[i])
                {

                    levelCurrentlySelected = i;
                    break;
                }
            }

            return levelCurrentlySelected;
        }

        private Button getGrabLvlBtnFromCombo(ComboBox combo, string mainOrBossButton = "")
        {
            //this gives us the proper grabLvlButton based on what Combo Box we're asking about
            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton, ML9ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8, mainCombo9 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };

            string buttonID = "";


            if (mainOrBossButton == "")
            {
                buttonID = combo.Name.Substring(0, 1);
            } else
            {
                buttonID = mainOrBossButton;
            }

            if (buttonID == "m")
            {
                //int indexOfComboBox = Array.FindIndex(mainCBox, element => element == combo);
                //^^ this won't work for some reason
                int indexOfComboBox = -1;
                for (int i = 0; i < mainCBox.Length; i++)
                {
                    if (combo == mainCBox[i])
                    {
                        indexOfComboBox = i;
                        break;
                    }
                }

                if (indexOfComboBox == -1)
                {
                    //we still haven't found it, it's possible we were looking for a bossComboBox
                    for (int i = 0; i < bossCBox.Length; i++)
                    {
                        if (combo == bossCBox[i])
                        {
                            indexOfComboBox = i;
                            break;
                        }
                    }
                }

                if (indexOfComboBox == -1) { ////testFindJson.Text += " no1 "; 
                    return null; }

                return mainLvlGrabButton[indexOfComboBox];

            } else if (buttonID == "b")
            {
                //int indexOfComboBox = Array.FindIndex(bossCBox, element => element == combo);
                //^^ this won't work for some reason
                int indexOfComboBox = -1;
                for (int i = 0; i < bossCBox.Length; i++)
                {
                    if (combo == bossCBox[i])
                    {
                        indexOfComboBox = i;
                        break;
                    }
                }
                if (indexOfComboBox == -1)
                {
                    //we still haven't found it, it's possible we were looking for a mainComboBox
                    for (int i = 0; i < mainCBox.Length; i++)
                    {
                        if (combo == mainCBox[i])
                        {
                            indexOfComboBox = i;
                            break;
                        }
                    }
                }
                if (indexOfComboBox == -1)
                { ////testFindJson.Text += " no2 "; 
                    return null;
                }
                return bossLvlGrabButton[indexOfComboBox];
            }

            //we shouldn't get this far.
            ////testFindJson.Text += " no3 ";
            return null;
        }
        private ComboBox getComboFromGrabLvlBtn(Button grabLvlBtn)
        {
            //this gives us the proper combo box based on what grabLvlButton we're asking about
            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton, ML9ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8, mainCombo9 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };

            if (grabLvlBtn.Name.Substring(0, 1).ToLower() == "m")
            {
                int indexOfGrabLvlButton = Array.FindIndex(mainLvlGrabButton, element => element == grabLvlBtn);
                return mainCBox[indexOfGrabLvlButton];
            }
            else if (grabLvlBtn.Name.Substring(0, 1).ToLower() == "b")
            {
                int indexOfGrabLvlButton = Array.FindIndex(bossLvlGrabButton, element => element == grabLvlBtn);
                return bossCBox[indexOfGrabLvlButton];
            }

            //we shouldn't get this far.
            return mainCBox[0];
        }

        private void setGrabLvlButton(ComboBox cBox)
        {
            //In this function, we're going to decide what we do to our LvlGrab button
            //it's meant to run whenever we've selected something as a song, so we need the grab button to be enabled or disabled, and have it say "?" or a level abbreviation, etc.

            /*
            if (!textChanged)
            {
                //we didn't change anything, just return
                ////testFindJson.Text += ".sGLB denied.";
                return;
            }*/


            ComboBox boxWasSelecting = cBox; //boxWasSelecting stores whatever main/boss music ComboBox we just changed


            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton, ML9ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };

            string songString = "";


            if (boxWasSelecting == null) //I actually have no idea why this would ever be null, unless there's just a flat error
                return;

            //this function is shared by all mainCombo and bossCombo boxes, so we need to find out which button called it
            //this will allow us to find out where to get default song info from

            string boxCalled = boxWasSelecting.Name;
            string m_or_b = boxCalled.Substring(0, 1); //this will be "m" or "b", from mainCombo1, or bossCombo2, etc.

            string boxCalledNumStr = boxCalled.Substring(boxCalled.Length - 1, 1);
            int whichLvl = Int32.Parse(boxCalledNumStr);
            whichLvl -= 1; //whichLevel is 0 index (0-7)

            if (boxWasSelecting.SelectedIndex > -1)
            {
                //if our selected index is 0 or above, then we're selecting a Mod in our list
                setModGrabLvlSelection(boxWasSelecting);
                //setSongSelectionArray(boxWasSelecting);
                setCheckFromSelection(boxWasSelecting);
                ////testFindJson.Text += ".*o*.";
                //justCheckWithoutThinking(boxWasSelecting.)

                return;
            }


            if (boxWasSelecting.Text == "")
            {
                boxWasSelecting.Text = getDefaultSong(whichLvl, m_or_b);
                setCheckFromSelection(boxWasSelecting);
                //getDefaultSong also disables the button. that's probably a bad idea

                return;
            }

            //if the our combo box was empty, and we just ran getDefaultSong, none of the below will run

            //if whatever box we just changed now has a selected Index of -1, and we're at this point of the code, it means we have something that the program doesn't understand
            //an example of this is the user being silly and putting their mod into the root game folder, instead of the MODS folder, while the JSON is somehow getting info from it. 
            //This is meant to be a catch-all, marking something in the example box (such as ! or ?), 



            //if (boxWasSelecting.SelectedIndex == -1)
            int songSelectIndex = boxWasSelecting.FindStringExact(boxWasSelecting.Text); //gives the index of the custom song, or -1 if none of them are selected
            if (songSelectIndex == -1)
            {

                if (m_or_b == "m")
                {
                    //a main music box was called
                    //first verify that it isn't the default song

                    if (boxWasSelecting.Text == getDefaultSong(whichLvl, m_or_b))
                    {
                        //it IS the default song! Just disable the button, don't put ?
                        disableGrabLvlButton(mainLvlGrabButton[whichLvl]);
                        setCheckFromSelection(boxWasSelecting);
                        return;
                    }
                    //if we got this far, it's not the default song name
                    disableGrabLvlButton(mainLvlGrabButton[whichLvl], "?");
                    setCheckFromSelection(boxWasSelecting);


                }
                else if (m_or_b == "b")
                {
                    //a boss music box was called
                    //first verify that it isn't the boss fight's default song

                    if (boxWasSelecting.Text == getDefaultSong(whichLvl, m_or_b))
                    {
                        //it IS the default song! Just disable the button, don't put ?
                        disableGrabLvlButton(bossLvlGrabButton[whichLvl]);
                        setCheckFromSelection(boxWasSelecting);
                        return;
                    }

                    disableGrabLvlButton(bossLvlGrabButton[whichLvl], "?");
                    setCheckFromSelection(boxWasSelecting);
                }

            } else
            {
                //MessageBox.Show("D: " + cBox.Name + " (" + cBox.Text + ") ");
                //whatever's in our box is a selection in our list
                //We already have something that runs if the index isn't -1, that's why this isn't running -_-
                /*
                string grabLStr = mainLvlGrabButton[whichLvl].Text;
                int songCurrentlySelected = Array.FindIndex(LvlAbbreviations, element => element == grabLStr); //this converts the letter in our GrabLvl box to a number
                ////testFindJson.Text += " run ";
                if (!modSupportsLevel(songSelectIndex, songCurrentlySelected, m_or_b))
                {
                    setModGrabLvlSelection(boxWasSelecting);
                }*/
                setModGrabLvlSelection(boxWasSelecting);
                setCheckFromSelection(boxWasSelecting);
            }
        }



        /*
        private void songSelectClick(object sender, EventArgs e)
        {
            ////testFindJson.Text += " O_O WTF!!! ";
            verifyAllGrabLvlButtons();
        }*/

        private void enableGrabLvlButton(ComboBox cBox, string whatitShouldSay)
        {
            string m_or_b = cBox.Name.Substring(0, 1);
            string boxCalledNumStr = cBox.Name.Substring(cBox.Name.Length - 1, 1);
            int whichLvl = Int32.Parse(boxCalledNumStr);
            whichLvl -= 1;
            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton, ML9ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };

            Button whichButton = mainLvlGrabButton[0];
            if (m_or_b == "m")
            {
                whichButton = mainLvlGrabButton[whichLvl];
            } else if (m_or_b == "b")
            {
                whichButton = bossLvlGrabButton[whichLvl];
            }

            whichButton.Text = whatitShouldSay;
            if (whatitShouldSay == "!")
            {
                whichButton.Enabled = true;
                whichButton.Image = null;
            }
            else if (whatitShouldSay == " ")
            {
                whichButton.Enabled = true;
                whichButton.Image = null;

            }
            else if (whatitShouldSay == "")
            {
                whichButton.Enabled = false;
                whichButton.Image = null;
            }
            else
            {
                whichButton.Enabled = true;
                whichButton.Image = null;
            }

        }

        //we want to run a function that verifies each level being selected is actually on a selectable level
        /* Delete this code
        private void verifyAllGrabLvlButtons()
        {
            return;
            //check each combo box, see what its selection is
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };

            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };

            for (int m = 0; m < mainCBox.Length; m++)
            {
                string boxText = mainCBox[m].Text;
                int customSongIndex = mainCBox[m].FindString(boxText);

                //if customsongindex = -1, that means we can't find a song
                if (customSongIndex == -1)
                {
                    setGrabLvlButton(mainCBox[m]);
                    continue;
                }

                //we need to be checking not what level we're on, but what level that box is selecting, unless our level says ! or blank
                string currentGrabLvlSelection = mainLvlGrabButton[m].Text;
                int currentGrabLvlSelectionInt = Array.FindIndex(LvlAbbreviations, element => element == currentGrabLvlSelection); //this converts the letter in our GrabLvl box to a number
                if (modSupportsLevel(customSongIndex, currentGrabLvlSelectionInt, "m"))
                {
                    //whatever we're already selecting is a possible level for this, so just choose it

                    continue;
                }


                if (!modSupportsLevel(customSongIndex, m, "m"))
                {
                    //we ran into a combo box that shows something that's not a custom song, and not the default song
                    enableGrabLvlButton(mainCBox[m], "!");
                    ////testFindJson.Text += mainCBox[m].Text + "doesn't support this level";

                } else
                {
                    enableGrabLvlButton(mainCBox[m], LvlAbbreviations[m]);
                    ////testFindJson.Text += mainCBox[m].Text + " supports this level";
                }
                //if the mod DOES support the level, everything's fine, just move on

            }

            for (int b = 0; b < bossCBox.Length; b++)
            {
                string boxText = bossCBox[b].Text;
                int customSongIndex = bossCBox[b].FindString(boxText);

                //if customsongindex = -1, that means we can't find a song
                if (customSongIndex == -1)
                {
                    setGrabLvlButton(bossCBox[b]);
                    continue;
                }

                if (!modSupportsLevel(customSongIndex, b, "b"))
                {
                    setGrabLvlButton(bossCBox[b]); //setGrabLvlButton is also ran when we first select a mod
                }

            }

        }
        */


        private void musicSelectGainedFocus(object sender, EventArgs e)
        {
            //all we want to do is store our song info
            if (mmLoading) return;
            ComboBox cBox = sender as ComboBox;
            setSongSelectionArray(cBox);//whatever we just selected just had its "current song selection" (currentComboBoxText) set to whatever it is now
        }

        private void musicSelectChangedIndex(object sender, EventArgs e)
        {
            //tabPage1.Focus();
            musicSelectLostFocus(sender, e);
        }


        int someday = 0;
        private void musicSelectCombo_KeyDown(object sender, KeyEventArgs e)
        {
            ////testFindJson.Text += someday++;
            ////testFindJson.Text += "KeyDown ";

            ComboBox boxCalled = sender as ComboBox;

            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) return;
            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey || e.KeyCode == Keys.Shift || e.KeyCode == Keys.ShiftKey) return;

            if (boxCalled.DroppedDown)
            {
                boxCalled.DropDownClosed -= this.musicSelectCombo_drDwnClz;
                boxCalled.DroppedDown = false;
                boxCalled.DropDownClosed += this.musicSelectCombo_drDwnClz;
            }
            if (e.KeyCode == Keys.Return)
            {

                if (!wasComboBoxChanged(boxCalled)) return;
                setGrabLvlButton(boxCalled);
                ////testFindJson.Text += " .hitEnter. ";
                e.SuppressKeyPress = true;
            }
        }
        private void musicSelectCombo_drDwnClz(object sender, EventArgs e)
        {
            ComboBox cBox = sender as ComboBox;
            ////testFindJson.Text += "DDC->";
            if (!wasComboBoxChanged(cBox)) return;

            ////testFindJson.Text += someday++;
            ////testFindJson.Text += "DrDwnClose ";
        }
        private void musicSelectCombo_txtUpdate(object sender, EventArgs e)
        {
            MSC_TextChanged(sender, e);
            setSongSelectionArray(sender as ComboBox, " ");
            ////testFindJson.Text += someday++;
            ////testFindJson.Text += "txtUpdate ";
        }
        private void musicSelectCombo_slctChngCmtd(object sender, EventArgs e)
        {
            /* THIS FUNCTION IS UNNECESSARY
             if (mmLoading) return;
            return;
            ComboBox cBox = sender as ComboBox;
            ////testFindJson.Text += "SCC->";
            if (!wasComboBoxChanged(cBox)) return;
            setGrabLvlButton(cBox);
            ////testFindJson.Text += someday++;
            ////testFindJson.Text += "slctChngCommitted ";*/
        }

        private void musicSelectCombo_musicSlctChIndx(object sender, EventArgs e)
        {
            //this is most likely what got called, doesn't mean it always will
            //will get called if we typed in selection and hit enter, with a song selected
            //will get called if we clicked on something in dropdown box
            //won't get called if we didn't have a song from Mod's list selected when we hit enter, or lost focus
            if (mmLoading) return;
            ComboBox cBox = sender as ComboBox;
            ////testFindJson.Text += "MSIC->";
            if (!wasComboBoxChanged(cBox)) return;
            setGrabLvlButton(cBox);

            string lvlNumStr = cBox.Name.Substring(cBox.Name.Length - 1, 1);
            int lvlNum = Int32.Parse(lvlNumStr); //gives 1-based index
            lvlNum -= 1;
            alertLevelIfModIntegrityComprimised(lvlNum, cBox);

            ////testFindJson.Text += someday++;
            ////testFindJson.Text += "SlctChngIndex ";
        }

        private void musicSelectCombo_gainFocus(object sender, EventArgs e)
        {

            setSongSelectionArray(sender as ComboBox);

            ////testFindJson.Text += someday++;
            ////testFindJson.Text += "gainedFocus ";
        }

        private void musicSelectCombo_lostFocus(object sender, EventArgs e)
        {
            //if we didn't hit enter, or we didn't click a button, such as hitting Tab, or focusing on something else, this is the catch-all to
            //verify that we can understand what's in the box

            ComboBox cBox = sender as ComboBox;
            ////testFindJson.Text += "LF->";
            if (!wasComboBoxChanged(cBox)) return;
            ////testFindJson.Text += "(LF: " + cBox.Text +") ";
            setGrabLvlButton(cBox);

            string lvlNumStr = cBox.Name.Substring(cBox.Name.Length - 1, 1);
            int lvlNum = Int32.Parse(lvlNumStr); //gives 1-based index
            lvlNum -= 1;
            alertLevelIfModIntegrityComprimised(lvlNum, cBox);

            ////testFindJson.Text += someday++;
            ////testFindJson.Text += "lostFocus ";
        }


        /// <summary>
        /// Disables the GrabLevel button/blanks it out. Resets this combo's "Song Selection" to look for changes. Closes dropdown box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MSC_TextChanged(object sender, EventArgs e)
        {
            if (mmLoading) return;
            ComboBox boxCalled = sender as ComboBox;


            disableGrabLvlBox(boxCalled);//this disables our button and makes it blank
            setSongSelectionArray(boxCalled, " "); //it can't be ""; we can set it to "blah" if we want, it just CANNOT match any more

        }




        /// <summary>
        /// DEPRECATED: Replaced with musicComboSelect function
        /// </summary>
        private void musicSelectLostFocus(object sender, EventArgs e)
        {

            if (mmLoading) return;
            ComboBox cBox = sender as ComboBox;
            ////testFindJson.Text += " .LostFocus. ";
            if (!wasComboBoxChanged(cBox)) return;
            setGrabLvlButton(cBox);

            string lvlNumStr = cBox.Name.Substring(cBox.Name.Length - 1, 1);
            int lvlNum = Int32.Parse(lvlNumStr); //gives 1-based index
            lvlNum -= 1;
            alertLevelIfModIntegrityComprimised(lvlNum, cBox);

            return;
        }



        //fillDefaultSong is meant to be ran when we want to fill the Combo Box for Main Level or Boss Level
        /// <summary>
        /// Returns a string in this format: This Is the End (Default)
        /// </summary>
        /// <param name="lvlNum"></param>
        /// <param name="m_or_b"></param>
        /// <returns></returns>
        private string getDefaultSong(int lvlNum, string m_or_b)
        {
            //this is just meant to change the text of the comboBox in question to the default song
            //this needs to be ran if we have a blank field in the comboBox

            //need to update the above comments, after i see if this work


            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton, ML9ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };

            string songString = "";
            if (m_or_b == "m")
            {
                songString = defaultMainSongNames[lvlNum];

            } else if (m_or_b == "b")
            {
                songString = defaultBossSongNames[lvlNum];
            }

            songString += " (Default)";

            return songString;

        }


        private void getmodsdir_Click(object sender, EventArgs e)
        {
            loadModListWithSubs(listBox1);
        }

        private void TC1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            int curTab = (sender as TabControl).SelectedIndex;
            if (curTab == 0)
            {
                if (!Organizer_checkAndAlertUnsavedChanges(true, "TCSelect"))
                {
                    e.Cancel = true;
                }
            }
        }



        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                //string modList = loadModListFromConfig(setListCatalog); //this loads the contents into the setListCatalog ListBox
                //fillSongSelection(modList);
                //storeModListInfo(); //this stores the info for our song; specifically which levels it supports



                if (setList_topLabel.Text == "Saving any changes in Organizer will clear all unsaved changes in Set List.")
                {
                    setList_topLabel.Visible = false;
                }
            } else if (tabControl1.SelectedIndex == 1)
            {
                //Changed tab to Organizer

                if (setListCheckForUnsavedChanges())
                {
                    setList_topLabel.Text = "Saving any changes in Organizer will clear all unsaved changes in Set List.";
                    setList_topLabel.Visible = true;
                }

                organizer_enableLevelButtons(false); //a song is no longer selected, so disable buttons
                clearSongInfoBoxes(); //song is no longer selected, clear the song info
                //loadModListWithSubs(listBox1);

                if (listBox1.Items.Count == 0)
                {
                    //this should never come up since we load it at the beginning
                    loadModListFromConfig(listBox1);
                    placeCurrCSjsonInOrgnzr();
                }

                org_modHasErrorsLbl.Visible = false; Org_OpenSongInDebug.Visible = false;
                org_openSongDir.Enabled = false; organizer_songDirLbl.Text = "...";
                organizer_restoreJson.Visible = false;
                restoredLabel.Visible = false;

                mCopyLevelInfo.Enabled = false; mPasteLevelInfo.Enabled = false; mSaveLevelInfo.Enabled = false; mDeleteLevelInfo.Enabled = false;
                bCopyLevelButton.Enabled = false; bPasteLevelInfo.Enabled = false; bSaveLevelInfo.Enabled = false; bDeleteLevelInfo.Enabled = false;

                listBox1.SelectedIndex = -1;//remove the selection from index if we had it
                currentListSelection = -1; //this is used for confirmation for switching Mods when we have unsaved changes
            }
        }

        /// <summary>
        /// Puts "Current customsongs.json" in Organizer ListBox IF it exists
        /// </summary>
        private void placeCurrCSjsonInOrgnzr()
        {
            if (currentJsonExists())
                listBox1.Items.Insert(0, new ListItem { Name = "Current customsongs.json", Path = gameDir + "\\customsongs.json" });
        }

        /// <summary>
        /// Checks if game folder is set, if it exists, and if the game's customsongs.json exists
        /// </summary>
        /// <returns></returns>
        private bool currentJsonExists()
        {
            if (gameDir == null) return false;
            if (!Directory.Exists(gameDir.ToString())) { return false; }
            if (!File.Exists(gameDir + "\\customsongs.json")) return false;

            return true;
        }


        private string NormalizeWhiteSpace(string input, bool scorchedEarth = false)
        {
            if (input == "" || input == null) return ""; //added this later, hope I'm not screwing something up

            string buffer = " "; //it'll normally leave a space between everything that isn't a space
            if (scorchedEarth)
            {
                //if we want, we can eradicate ALL white space
                buffer = "";
            }

            string a = String.Join(buffer, input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            a = a.Replace("\r", "").Replace("\n", "");
            a = a.Replace("\t", "");  //I don't know if we need this, but just in case

            return a;
        }

        /// <summary>
        /// Takes an item from Organizer's listbox and returns its Json
        /// </summary>
        /// <param name="orgListBoxIndex"></param>
        /// <returns></returns>
        public string GetJsonFromPath(string path, bool whiteSpaceNormalized = false)
        {
            //this gives us the JSON in its full, unaltered form

            /*
            if (orgListBoxIndex != -1)
                selectedSong = ((ListItem)listBox1.Items[orgListBoxIndex]).Path;
            else
                selectedSong = ((ListItem)listBox1.SelectedItem).Path; //we now store our path in the listbox with the modname, so we'll just grab that

            if (orgListBoxIndex == -1 && listBox1.SelectedItem.ToString() == "Current customsongs.json")
            {
                //we want the current custom song
                //selectedSong = gameDir + "\\customsongs.json"; We get this anyways now
                currentGameJsonIndicator = "<>"; //if we see this at the beginning of our string, we'll know we're accessing the game's current json
            }
            else if (orgListBoxIndex != -1 && listBox1.Items[orgListBoxIndex].ToString() == "Current customsongs.json")
            {
                currentGameJsonIndicator = "<>"; //if we see this at the beginning of our string, we'll know we're accessing the game's current json
            }*/

            using (StreamReader sr = File.OpenText(@path))
            {
                string s = "";
                string fullText = sr.ReadToEnd();

                if (whiteSpaceNormalized)
                {
                    fullText = NormalizeWhiteSpace(fullText);
                }
                //fullText = currentGameJsonIndicator + fullText;


                return fullText;
            }

        }


        /// <summary>
        /// Takes an item from Organizer's listbox and returns its Json with linebreaks
        /// </summary>
        /// <param name="orgListBoxIndex"></param>
        /// <returns></returns>
        public string Injector_GetModJson(int orgListBoxIndex = -1)
        {
            //this gives us the JSON in its full, unaltered form
            string selectedSong = "";

            //Open Json file and retrieve info
            //check what we're opening
            string currentGameJsonIndicator = ""; //we will change this if we see we're accessing the game's current customsongs.json
            if (orgListBoxIndex == -1 && listBox1.SelectedItem.ToString() == "Current customsongs.json")
            {
                //we want the current custom song
                //selectedSong = gameDir + "\\customsongs.json"; We get this anyways now
                currentGameJsonIndicator = "<>"; //if we see this at the beginning of our string, we'll know we're accessing the game's current json
            } else if (orgListBoxIndex != -1 && listBox1.Items[orgListBoxIndex].ToString() == "Current customsongs.json")
            {
                currentGameJsonIndicator = "<>"; //if we see this at the beginning of our string, we'll know we're accessing the game's current json
            }



            //selectedSong = "\\" + listBox1.SelectedItem.ToString();
            //selectedSong = di + selectedSong + "\\customsongs.json";


            if (orgListBoxIndex != -1)
                selectedSong = ((ListItem)listBox1.Items[orgListBoxIndex]).Path;
            else
                selectedSong = ((ListItem)listBox1.SelectedItem).Path; //we now store our path in the listbox with the modname, so we'll just grab that



            using (StreamReader sr = File.OpenText(@selectedSong))
            {
                string s = "";
                string fullText = sr.ReadToEnd();

                //string trimmedLine = NormalizeWhiteSpace(fullText);
                s = currentGameJsonIndicator + fullText;

                return s;
            }

        }




        string[] allLevelNames = { "voke", "stygia", "yhelm", "incaustis", "gehenna", "nihil", "acheron", "sheol", "tutorial" };

        string[] levelNames = { "voke", "stygia", "yhelm", "incaustis", "gehenna", "nihil", "acheron", "sheol" };

        /// <summary>
        /// Retreives the Json of listBox1's selected item with all whitespace removed
        /// </summary>
        /// <returns></returns>
        public string Organizer_GetModJson()
        {
            //this retrieves the Mod info from Organizer's listbox with all whitespace removed
            string selectedSong = "";

            //Open Json file and retrieve info
            //check what we're opening


            //I don't think we need this anymore. We know we needed it when we weren't allowing bankPath changes unless we were on the game's main customsongs.json, but we allow it now
            if (listBox1.SelectedItem.ToString() == "Current customsongs.json")
            {
                if (((ListItem)listBox1.SelectedItem).Path == "none")
                {
                    return "-2";
                }
                //we want the current custom song
                //selectedSong = gameDir + "\\customsongs.json"; We get this anyways now

            }


            //selectedSong = "\\" + listBox1.SelectedItem.ToString();
            //selectedSong = di + selectedSong + "\\customsongs.json";
            //string selectedSong2 = ((ListItem)listBox1.SelectedItem).Name;
            selectedSong = ((ListItem)listBox1.SelectedItem).Path; //we now store our path in the listbox with the modname, so we'll just grab that


            if (!File.Exists(selectedSong)) return "-1";

            using (StreamReader sr = File.OpenText(@selectedSong))
            {
                string s = "";
                string fullText = sr.ReadToEnd();
                ////testFindJson.Text = fullText;
                fullText.Replace("\t", " "); //get rid of all indentations
                string trimmedLine = NormalizeWhiteSpace(fullText);
                s = trimmedLine;

                return s;
            }

        }


        public string GetRealBankPath_GetModJson(string modName)
        {
            //until I figure out what to do with sub directories, this is an exact copy and paste of SetList_GetModJsonNoSubs
            //I found out what to do with subdirectories!

            //string selectedSong = "\\" + modName;
            //selectedSong = di + selectedSong + "\\customsongs.json";
            string selectedSong = ((ListItem)setListCatalog.SelectedItem).Path; //we now store our path in the listbox with the modname, so we'll just grab that

            using (StreamReader sr = File.OpenText(@selectedSong))
            {
                string s = "";

                string fullText = sr.ReadToEnd();
                string trimmedLine = NormalizeWhiteSpace(fullText);
                s = trimmedLine;

                return s;
            }

        }


        /// <summary>
        /// Looks for a certain item in the Set List catalog and gets its JSON
        /// </summary>
        /// <param name="modDirectory"></param>
        /// <returns></returns>
        public string SetList_GetModJson(string modDirectory)
        {
            //Open Json file and retrieve info
            //it wants "modDirectory", meaning just the folder name, aka the mod name

            int modIndex = setListCatalog.FindStringExact(modDirectory); //this has proven safer than selectedIndex

            string selectedSong = ((ListItem)setListCatalog.Items[modIndex]).Path; //we now store our path in the listbox with the modname, so we'll just grab that
            using (StreamReader sr = File.OpenText(@selectedSong))
            {
                string s = "";

                string fullText = sr.ReadToEnd();
                string trimmedLine = NormalizeWhiteSpace(fullText);
                s = trimmedLine;

                return s;
            }

        }


        string[] currentComboBoxText = new string[17];

        /// <summary>
        /// Saves the current selections of the SetList, to know if it's been edited after losing focus
        /// </summary>
        /// <param name="justThisOne"></param>
        /// <param name="toWhat"></param>
        private void setSongSelectionArray(ComboBox justThisOne = null, string toWhat = "")
        {
            //this doesn't work :(
            //return;

            ComboBox[] allComboBoxes = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8,
                bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7, customMusicBankCombo, mainCombo9
            };

            if (justThisOne != null)
            {
                int whichTextBox = Array.FindIndex(allComboBoxes, element => element == justThisOne);//I'm going to try this ONE more time
                if (whichTextBox == -1)
                {
                    return;
                }
                if (toWhat == "")
                {
                    currentComboBoxText[whichTextBox] = allComboBoxes[whichTextBox].Text;
                } else
                {
                    currentComboBoxText[whichTextBox] = toWhat;
                }
            }

            /*for (int i=0; i<allComboBoxes.Length; i++)
            {
                currentComboBoxText[i] = allComboBoxes[i].Text;
            }*/
            ////testFindJson.Text += ".SSA Set.";
        }

        /// <summary>
        /// Checks if current selections of the SetList were altered, to know if we should look for the song info again/which level to grab
        /// </summary>
        /// <param name="whichComboBox"></param>
        /// <returns></returns>
        private bool wasComboBoxChanged(ComboBox whichComboBox)
        {


            //checks the currentComboBoxText, aka the info that setSongSelectionArray writes
            ComboBox[] allComboBoxes = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8,
                bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7, customMusicBankCombo, mainCombo9
            };

            int whichTextBox = Array.FindIndex(allComboBoxes, element => element == whichComboBox);//I'm going to try this ONE more time
            if (whichTextBox == -1) {
                //wait this isn't popping up?
                return false;
            }
            if (allComboBoxes[whichTextBox].Text == currentComboBoxText[whichTextBox])
            {
                //Combo box was the same
                //setSongSelectionArray(allComboBoxes[whichTextBox]);
                ////testFindJson.Text += ".wCBC..box was same..";
                return false;
            }
            //if we got this far, it didn't match what we had last

            //testFindJson.Text += ".wCBC..box was new..";
            setSongSelectionArray(allComboBoxes[whichTextBox]);
            return true;
        }


        bool causedSaveChngsWarning = false;
        private bool Organizer_checkAndAlertUnsavedChanges(bool sayModName = false, string whatCalledUs = null)
        {
            //this will return true or false, to tell the L1Settings, L2Settings, etc., to switch to that level or not. True says change it, false says stay on the page.
            //That's all those button care about
            //however, this function will also be what says to Save the information if we're about to close the info by switching to another level 

            //if (whatCalledUs != null) MessageBox.Show(whatCalledUs + " called us. Current list slection:" + currentListSelection +"\n"+"index is: " + listBox1.SelectedIndex);

            //if (listBox1.SelectedIndex == -1) { return true; } //nothing's selected <-we want to stop us from resetting unsaved info if we're unselecting!
            int levelInt = getSelectedLevel_OrganizerInjector(); //despite this being for the injector, it just tells us the 0-based level number we're on
            if (levelInt == -1)
            {
                return true; //no level is selected
            }
            if (!mSaveLevelInfo.Enabled && !bSaveLevelInfo.Enabled) return true; //we don't have anything changed

            //If we're this far, we have unsaved changes.

            causedSaveChngsWarning = true; //used for our right-click menu; we want it blocked if we made this appear

            Button[] LevelButtons = { L1Settings, L2Settings, L3Settings, L4Settings, L5Settings, L6Settings, L7Settings, L8Settings, L0Settings };
            //we use these ^^ just to make the focus get put back on the button of the level we're on, in the case that we want to cancel switching levels

            //string selectedMod = listBox1.SelectedItem.ToString();

            string Level = allLevelNames[levelInt].Substring(0, 1).ToUpper() + allLevelNames[levelInt].Substring(1).ToLower(); //voke->Voke


            string message = "You have unsaved changes for ";
            if (sayModName)
            {
                message += listBox1.Items[currentListSelection].ToString() + " on ";
            }
            message += Level + "!" + Environment.NewLine;
            message += "Would you like to save them?" + Environment.NewLine + Environment.NewLine + "Yes saves the changes, No discards changes," + Environment.NewLine + "Cancel takes you back to ";
            if (sayModName)
            {
                message += listBox1.Items[currentListSelection].ToString();
            }
            else
            {
                message += Level;
            }
            message += ".";
            string title = "Save changes?";
            MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.Yes)
            {
                //we want to save changes as we switch
                if (forceSaveOrganizer() == false)
                {
                    LevelButtons[levelInt].Focus();
                    string inTheEndYouWillThankMe = "An error occured when trying to save changes. Taking you back to " + Level;
                    if (sayModName) inTheEndYouWillThankMe += " for " + listBox1.Items[currentListSelection].ToString();
                    message += ".";
                    MessageBox.Show(inTheEndYouWillThankMe);
                    return false;
                }

                //if we got this far, we successfully saved

                storeModListInfo(); //recheck our supported levels
                setOldSongsArray(); //this stores an array of info of our current customsongs.json file in the game folder
                //loadOldInfoIntoSetList(); //this loads the array from the previous line into the fields
                RevertOldInfoIntoSetList();
                setList_topLabel.Visible = false;
                setList_topLabel.Text = "Starting Metal Manager...";

                return true;
            }
            if (result == DialogResult.No)
            {
                //just switch the page
                return true;
            }
            else
            {
                LevelButtons[levelInt].Focus();
                return false;

                //don't carry out the page switch
            }

        }

        /// <summary>
        /// Should be used if we ever alter a customsongs.json. Rechecks level support, and 
        /// </summary>
        private void refreshAfterSaving()
        {
            setList_topLabel.Visible = false;
            setList_topLabel.Text = "Starting Metal Manager...";
            storeModListInfo(); //recheck our supported levels
            setOldSongsArray(); //this stores an array of info of our current customsongs.json file in the game folder
            //loadOldInfoIntoSetList(); //this loads the array from the previous line into the fields
            RevertOldInfoIntoSetList();
        }


        private bool forceSaveOrganizer()
        {
            //try to save either if their button is shown
            bool mSaveSuccess = true;
            bool bSaveSuccess = true;
            if (mSaveLevelInfo.Enabled && bSaveLevelInfo.Enabled)
            {
                mSaveSuccess = AttemptToSaveLevel_Organizer("m", true);
                if (mSaveSuccess)
                {
                    //don't do this unless the first succeeds
                    bSaveSuccess = AttemptToSaveLevel_Organizer("b", true);
                }
            } else if (mSaveLevelInfo.Enabled)
            {
                mSaveSuccess = AttemptToSaveLevel_Organizer("m", true);
            } else if (bSaveLevelInfo.Enabled)
            {
                bSaveSuccess = AttemptToSaveLevel_Organizer("b", true);
            }
            if (!mSaveSuccess || !bSaveSuccess) { return false; }

            return true;
        }


        /// <summary>
        /// Returns true if we're on the Organizer tab and have unsaved changes. Return false otherwise
        /// </summary>
        /// <returns></returns>
        private bool checkUnsavedChangesOrganizer()
        {
            if (tabControl1.SelectedIndex != 1) return false;
            if (mSaveLevelInfo.Enabled || bSaveLevelInfo.Enabled) return true;

            return false;
        }


        string[] songInfoLabels = { "Bank", "Event", "LowHealthBeatEvent", "BeatInputOffset", "BPM" }; // need to add , "bankPath" 

        private string[] SplitAndGetLevelInfo(string fullJson, int indexOfLInfo)
        {
            string[] songInfoSegments = fullJson.Split(':');
            return songInfoSegments;
        }


        private void clearSongInfoBoxes()
        {
            TextBox[] mainLevelTextBoxes = { MLNameBox, MLEventBox, MLLHBEBox, MLOffsetBox, MLBPMBox };
            TextBox[] bossFightTextBoxes = { BFNameBox, BFEventBox, BFLHBEBox, BFOffsetBox, BFBPMBox };
            for (int i = 0; i < mainLevelTextBoxes.Length; i++)
            {
                mainLevelTextBoxes[i].Text = "";
                storedOriginalInfo_m[i] = ""; //we're going to reset these too
            }

            for (int i = 0; i < bossFightTextBoxes.Length; i++)
            {
                bossFightTextBoxes[i].Text = "";
                storedOriginalInfo_b[i] = ""; //we're going to reset these too
            }
            mBankPathLabel.Text = "";
            mBankPathLabel.BackColor = Color.Transparent;
            mTrueBankPath.Text = "";

            bBankPathLabel.Text = "";
            bBankPathLabel.BackColor = Color.Transparent;
            bTrueBankPath.Text = "";

            resetDebugLabel();
        }

        //this uses getOldJsonLevelNoSubs to store an array of info of our current customsongs.json file in the game folder; we use it later for loadOldInfoIntoSetList
        /// <summary>
        /// Goes through the game's customsongs.json and matches its info to custom songs, then records the results to currentSetListName_(m/b) 
        /// </summary>
        private void setOldSongsArray()
        {
            bool gameJsonGoofed = gameJsonHasErrors();

            string fullModString = getCurrentCustomsongsJson();

            if (fullModString == "-2" || fullModString == "-1" || gameJsonGoofed)
            {
                //there's no game directory
                if (fullModString == "-2")
                {
                    //fullModString == "-2"

                    SetList_DebugLabel1.Visible = true;
                    SetList_DebugLabel1.Text = "No current customsongs.json found—new slate loaded!";
                } else if (gameJsonGoofed)
                {
                    SetList_DebugLabel1.Visible = true;
                    SetList_DebugLabel1.Text = "Current customsongs.json contains errors—a new slate was loaded.";
                    SetList_DebugLabel2.Visible = true;
                    SetList_DebugLabel2.Text = "Visit the Debug panel to find/remove errors!";
                }

                for (int m = 0; m < currentSetListName_m.Length; m++)
                {
                    string lvlInfoName = getDefaultSong(m, "m");
                    currentSetListName_m[m] = lvlInfoName;
                    currentSetListIndexes_main[m] = -1;
                }

                for (int b = 0; b < currentSetListName_b.Length; b++)
                {
                    string lvlInfoName = getDefaultSong(b, "b");

                    currentSetListName_b[b] = lvlInfoName;
                    currentSetListIndexes_boss[b] = -1;
                }

                return;
            }
            //if we got this far, there's a game directory



            for (int m = 0; m < currentSetListName_m.Length; m++)
            {
                string[] lvlInfoInCurrentJson = getOldJsonLevel(fullModString, allLevelNames[m], "m");

                string lvlInfoName = lvlInfoInCurrentJson[0];
                if (lvlInfoName == "<default>")
                {
                    lvlInfoName = getDefaultSong(m, "m");
                }
                currentSetListName_m[m] = lvlInfoName;
                string modIndexStr = lvlInfoInCurrentJson[1];
                int modIndex = Int32.Parse(modIndexStr);
                currentSetListIndexes_main[m] = modIndex;


                //testFindJson.Text += "OldSong" + m + ": " + modIndex;
            }

            for (int b = 0; b < currentSetListName_b.Length; b++)
            {
                string[] lvlInfoInCurrentJson = getOldJsonLevel(fullModString, allLevelNames[b], "b");

                string lvlInfoName = lvlInfoInCurrentJson[0];
                if (lvlInfoName == "<default>")
                {
                    lvlInfoName = getDefaultSong(b, "b");
                }

                currentSetListName_b[b] = lvlInfoName;
                string modIndexStr = lvlInfoInCurrentJson[1];
                int modIndex = Int32.Parse(modIndexStr);
                currentSetListIndexes_boss[b] = modIndex;

                //testFindJson.Text += "OldSong" + b + ": " + modIndex;
            }


        }


        private void alertLevel(int levelNum, bool alertOn = true)
        {
            //this is used by setList to check 
            //this function just changes the background color of the level selection

            GroupBox[] levelGroupBoxes = { groupBox1, groupBox2, groupBox3, groupBox4, groupBox5, groupBox6, groupBox7, groupBox8, tutorialGroupBox }; //we should never have to mess with groupBox8, but o well
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8, mainCombo9 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };
            if (alertOn)
            {
                levelGroupBoxes[levelNum].BackColor = Color.RosyBrown;

                string debug_lvlName = allLevelNames[levelNum].Substring(0, 1).ToUpper() + allLevelNames[levelNum].Substring(1); //turns "voke" into "Voke"
                string debug_mSongName = mainCBox[levelNum].Text;
                if (debug_mSongName.Length > 25) debug_mSongName = "Main Level";
                string debug_bSongName = bossCBox[levelNum].Text;
                //if (debug_bSongName.Length > 25) debug_bSongName = "Main Level"; meh, this is fine
                SetList_DebugLabel1.Text = debug_lvlName + "'s custom songs have different names, but their files have the same Event ID. This will result in ";
                SetList_DebugLabel2.Text = debug_mSongName + "'s song being played during the boss as well, instead of " + debug_bSongName + ". Only the";
                SetList_DebugLabel3.Text = "creator of the mod can set a new Event ID. We recommend choosing different selections for " + debug_lvlName + ".";
                SetList_DebugLabel1.Visible = true;
                SetList_DebugLabel2.Visible = true;
                SetList_DebugLabel3.Visible = true;
            } else
            {
                levelGroupBoxes[levelNum].BackColor = Color.Transparent;
                if (SetList_DebugLabel3.Text != "" || !SetList_DebugLabel3.Visible) return;
                if (SetList_DebugLabel3.Text.Substring(0, "creator of the mod can set a new Event ID.".Length) == "creator of the mod can set a new Event ID.")
                {
                    SetList_DebugLabel1.Visible = false;
                    SetList_DebugLabel2.Visible = false;
                    SetList_DebugLabel3.Visible = false;
                }
                //testFindJson.Text += "Resetting";
            }

        }

        private bool checkIfTwoModsSelected(int zeroBasedLvlNum)
        {
            if (zeroBasedLvlNum >= 7) return false; //if we're on Sheol, we can't have 2 anyways

            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8, mainCombo9 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };

            //we're already given the level number, so check from there what combo boxes to compare
            int mainSongIndex = setListCatalog.FindStringExact(mainCBox[zeroBasedLvlNum].Text); //this has proven safer than selectedIndex
            //testFindJson.Text += " song1: " + mainSongIndex;
            int bossSongIndex = setListCatalog.FindStringExact(bossCBox[zeroBasedLvlNum].Text); //these are just checking that we have custom songs on the selections for one level
            //testFindJson.Text += " song2: " + bossSongIndex;

            if (mainSongIndex > -1 && bossSongIndex > -1) return true;

            return false;
        }

        private bool checkLevelsModsIntegrity(int mainGrabLvlNum, int bossGrabLvlNum, string mainSong, string bossSong)
        {
            //this code is meant to run when we see that we have chosen two custom songs for one level. (we chose one for the main and the boss)
            //we want to grab two things from each song: the file name, and the event ID; if they have different file names and the same event ID, integrity cannot be verified

            if (mainGrabLvlNum == -1)
            { //testFindJson.Text += " A0 "; 
                return true;
            } //right now, we're only running this checker with verfied Mods. Not when our grabLvlBox has a "?"
            if (bossGrabLvlNum == -1)
            {
                //testFindJson.Text += " B0 "; 
                return true;
            } //I guess we COULD get it to run if it had a "?" ...



            string levelStringM = allLevelNames[mainGrabLvlNum].Substring(0, 1).ToUpper() + allLevelNames[mainGrabLvlNum].Substring(1); //turns "voke" into "Voke"
            string levelStringB = allLevelNames[bossGrabLvlNum].Substring(0, 1).ToUpper() + allLevelNames[bossGrabLvlNum].Substring(1); //turns "voke" into "Voke"

            string mainFullJson = SetList_GetModJson(mainSong);
            string[] mainSongInfo = getModNameAndID(mainFullJson, levelStringM, "m");

            string bossFullJson = SetList_GetModJson(bossSong);
            string[] bossSongInfo = getModNameAndID(bossFullJson, levelStringB, "b");

            if (mainSongInfo[0] == "" || mainSongInfo[1] == "") return true;
            if (bossSongInfo[0] == "" || bossSongInfo[1] == "") return true; //we can't verify anything if we somehow got no info, we don't have any info to alert the user



            if (mainSongInfo[1] != bossSongInfo[1]) return true; //our event IDs don't match, the integrity is fine



            if (mainSongInfo[0] == bossSongInfo[0]) return true; //we're putting in the same song for Main and Boss, for whatever reason. But, that'll work, so that's not an issue

            //if we got this far, then our event IDs match, yet our filenames do NOT. This means that the song for Main music will unintentionally roll over to Boss music—ALERT THE USER!
            return false;

        }


        private int convertLevelNameToInt(string songName)
        {
            for (int i = 0; i < allLevelNames.Length; i++)
            {
                if (songName.ToLower() == allLevelNames[i])
                {
                    return i;
                }
            }
            return -1; //the level name isn't in the list
        }
        private string convertIntToLevelName(int zeroBasedLevelNum)
        {
            return allLevelNames[zeroBasedLevelNum]; //well this function was pointless
        }

        private string[] getModNameAndID(string fullJson, string Level, string m_or_b)
        {
            //retrieves the info for one Main or Boss music's custom song; this is used for checkLevelsModsIntegrity
            //it is a very similar copy of setSpecificLevelInfo_Org, but decides to only grab the Name and ID instead; it also only grabs main or boss info

            string[] returnStrings = { "", "" };
            int numberOfItemsWeWant = 2;//this is just to make the for loops easier to read; we're able to get away with this because we're grabbing the first two items in the original for loop

            int indexOfLevelInfo = fullJson.IndexOf(Level); //appears as, for example, "LevelName" : "Voke"

            if (indexOfLevelInfo == -1)
            {
                //this level is not in the JSON file
                return returnStrings;
            }



            int indexOfLevelInfoEnd = fullJson.IndexOf("} }", indexOfLevelInfo);
            if (indexOfLevelInfoEnd == -1)
            {
                //we have a problem, try to fix it
                fullJson.Replace("\t", "");
                indexOfLevelInfoEnd = fullJson.IndexOf("}}", indexOfLevelInfo);
            }
            if (indexOfLevelInfoEnd == -1)
            {
                indexOfLevelInfoEnd = fullJson.IndexOf("}}", indexOfLevelInfo);
            }

            if (indexOfLevelInfoEnd == -1) return returnStrings;

            string fullLevelInfo = fullJson.Substring(indexOfLevelInfo, indexOfLevelInfoEnd - indexOfLevelInfo);
            ////testFindJson.Text = fullLevelInfo;


            TextBox[] mainLevelTextBoxes = { MLNameBox, MLEventBox, MLLHBEBox, MLOffsetBox, MLBPMBox };
            TextBox[] bossFightTextBoxes = { BFNameBox, BFEventBox, BFLHBEBox, BFOffsetBox, BFBPMBox };



            int indexOfMainLevelMusic = fullLevelInfo.IndexOf("\"MainMusic\"");
            int indexOfBossFightMusic = fullLevelInfo.IndexOf("\"BossMusic\"");

            //we're also immediately going to find out if we have a bank path
            //Label bankPathM = mBankPathLabel; nevermind
            //Label bankPathB = bBankPathLabel;

            string[] songLabels = songInfoLabels;

            if (m_or_b == "b")
            {
                goto bossMusicCheck;
            }

            //if we got this far, we're looking for main music info, not boss

            //check if we have MainMusic info in this level
            if (indexOfMainLevelMusic == -1)
            {
                //we don't have any info for "MainMusic"
                //something went wrong
                return returnStrings;
            }

            //if we got this far, we DO have a MainMusic entry for this level

            //check to see where MainMusic ends, by looking for "BossMusic", or "} }"
            int indexofMainMusicEnd = indexOfBossFightMusic;
            if (indexofMainMusicEnd == -1)
            {
                indexofMainMusicEnd = fullLevelInfo.Length;
            }
            string fullMainMusicInfo = fullLevelInfo.Substring(0, indexofMainMusicEnd);







            for (int i = 0; i < numberOfItemsWeWant; i++)
            {
                //we only want the first two items, Bank and Event


                int indexOfSongInfo = fullMainMusicInfo.IndexOf(songLabels[i]); //For example, this finds "Bank"
                int indexOfSongInfoSeperator = fullMainMusicInfo.IndexOf(':', indexOfSongInfo); //This finds the index of the next : after the label. What's after this colon is our value we want
                indexOfSongInfoSeperator += 2; //now we're directly after the space ( ) after the colon
                int indexOfSongInfoEnd = 0;

                if (i != songLabels.Length - 1)
                {
                    //we're checking the last item, which is BPM
                    //first see if we're checking the game's current Json

                    indexOfSongInfoEnd = fullMainMusicInfo.IndexOf(',', indexOfSongInfoSeperator);

                }
                else
                {
                    indexOfSongInfoEnd = fullMainMusicInfo.IndexOf(" }", indexOfSongInfoSeperator);
                }
                if (indexOfSongInfoEnd == -1)
                {
                    indexOfSongInfoEnd = fullMainMusicInfo.Length; //we need this if we don't have a boss music
                }

                if (fullMainMusicInfo.Substring(indexOfSongInfoSeperator, 1) == "\"")
                {
                    //if the first character of our info is a quote
                    indexOfSongInfoSeperator += 1;
                    indexOfSongInfoEnd -= 1;
                }


                int lengthOfSongInfo = indexOfSongInfoSeperator - indexOfSongInfo;
                string songInfo = fullMainMusicInfo.Substring(indexOfSongInfoSeperator, indexOfSongInfoEnd - indexOfSongInfoSeperator); //this should grab .... "Bank":"EVERYTHINGINHERE"

                returnStrings[i] = songInfo; //if i is 0, this gives us our bank. if it's 1, this gives us our eventID.

                /*
                if (indexOfLevelInfo != -1)
                {
                    //there's no point to this, I don't think. if it was -1, our function would have fallen apart by now
                    
                }
                else
                {
                    //Json does not have song in it
                    //we already checked for this, I don't think this will ever run
                }*/
            }

            return returnStrings;

        //we skip to here if we wanted the Boss Music info from this level

        bossMusicCheck:


            //check if we have boss music
            if (indexOfBossFightMusic == -1)
            {
                //we don't have any info for "BossMusic"

                return returnStrings;


            }



            string fullBossMusicInfo = fullLevelInfo.Substring(indexOfBossFightMusic);
            for (int i = 0; i < numberOfItemsWeWant; i++)
            {

                int indexOfSongInfo = fullBossMusicInfo.IndexOf(songLabels[i]); //For example, this finds "Bank"
                int indexOfSongInfoSeperator = fullBossMusicInfo.IndexOf(':', indexOfSongInfo); //This finds the index of the next : after the label. What's after this colon is our value we want
                indexOfSongInfoSeperator += 2; //now we're directly after the space ( ) after the colon
                int indexOfSongInfoEnd = 0;

                if (i != songLabels.Length - 1)
                {
                    indexOfSongInfoEnd = fullBossMusicInfo.IndexOf(',', indexOfSongInfoSeperator);

                }
                else
                {
                    indexOfSongInfoEnd = fullBossMusicInfo.Length;
                }

                if (fullBossMusicInfo.Substring(indexOfSongInfoSeperator, 1) == "\"")
                {
                    //if the first character of our info is a quote
                    indexOfSongInfoSeperator += 1;
                    indexOfSongInfoEnd -= 1;
                }


                int lengthOfSongInfo = indexOfSongInfoSeperator - indexOfSongInfo;
                string songInfo = fullBossMusicInfo.Substring(indexOfSongInfoSeperator, indexOfSongInfoEnd - indexOfSongInfoSeperator); //this should grab .... "Bank":"EVERYTHINGINHERE"

                returnStrings[i] = songInfo; //if i is 0, this gives us our bank. if it's 1, this gives us our eventID.

                /* I realized this is pointless
                if (indexOfLevelInfo != -1)
                {
                    

                }
                else
                {
                    //Json does not have song in it
                }*/
            }
            return returnStrings;

        }


        //this returns an array of strings; it just holds two strings, with the second string saying if the song is a custom song or not (or -2 for unknown?)
        //we COULD also make it return one string, being the mod name, then something to seperate that that can't be in a file name (like < or >), then the custom song index/indicator

        //since this is called only after we've created the list for setListCatalog, we can just use that ListBox's information

        private string[] getOldJsonLevel(string fullJson, string Level, string m_or_b)
        {
            //retrieves the info for one level's custom music; it's used to read the actual JSON in the game folder
            //it's either going to give us a directory name(the mod name), or a song title if no directory


            // we're going to take the ListBox item's value, which is its JSON path. Now we're going to have to look through THAT and see if it matches the information! ***
            //this is because we can have a JSON with no .Bank file anywhere to be seen, pointing to another .Bank file in an another folder
            //if the bankPath in our Json ...^--(this one) matches with what we have in the game's current customsongs.json, we're golden!
            //    and by 'golden', i mean it returns the mod name (its last FOLDER name), and the modIndex (where it matches in our list)
            //if it doesn't, we want to give back the name of whatever's in our "Bank": field, and indicate that we have no idea what this is

            //. *** IF we cannot find a bankPath in the listbox item's JSON file, it means the file is to be expected in the same folder as listbox item's Json file
            //      '-> therefore, we can verify that our game's current customsongs.json is pointing to what the JSONs agree are where the files SHOULD be.
            //  At the time of writing, the program is only meant to look for matching file names in JSON files. We could technically verify a file actually exists, similar to Organizer, and alert user if not

            string capitalizeLevelName = Level.Substring(0, 1).ToUpper() + Level.Substring(1);
            int indexOfLevelInfo = fullJson.IndexOf(capitalizeLevelName); //appears as, for example, "LevelName" : "Voke"

            if (indexOfLevelInfo == -1)
            {
                //this level is not in the JSON file
                //that means our current customsongs.json has the default/vanilla song from the game
                string[] returnString = { "<default>", "-1" };
                return returnString;
            }


            int indexOfLevelInfoEnd = fullJson.IndexOf("} }", indexOfLevelInfo);
            if (indexOfLevelInfoEnd == -1)
            {
                //we have a problem, try to fix it
                fullJson.Replace("\t", "");
                indexOfLevelInfoEnd = fullJson.IndexOf("}}", indexOfLevelInfo);
            }
            if (indexOfLevelInfoEnd == -1)
            {
                indexOfLevelInfoEnd = fullJson.IndexOf("}}", indexOfLevelInfo);
            }

            if (indexOfLevelInfoEnd == -1)
            {
                string[] returnString = { "ERROR", "-1" };
                return returnString;
            }
            string fullLevelInfo = fullJson.Substring(indexOfLevelInfo, indexOfLevelInfoEnd - indexOfLevelInfo);


            int indexOfMainLevelMusic = fullLevelInfo.IndexOf("\"MainMusic\"");
            int indexOfBossFightMusic = fullLevelInfo.IndexOf("\"BossMusic\"");

            //check if we have MainMusic info in this level

            if (m_or_b == "b") goto BossChecker;

            if (indexOfMainLevelMusic == -1 && m_or_b == "m")
            {
                //we don't have any info for "MainMusic"

                string[] returnString = { "<default>", "-1" };
                return returnString;


            }

            //if we got this far, we DO have a MainMusic entry for this level

            //check to see where MainMusic ends, by looking for "BossMusic"; our current information stops right before the "} }", so we won't find that
            int indexofMainMusicEnd = indexOfBossFightMusic;
            if (indexofMainMusicEnd == -1)
            {
                indexofMainMusicEnd = fullLevelInfo.Length;
            }
            string fullMainMusicInfo = fullLevelInfo.Substring(0, indexofMainMusicEnd);
            //we now have the chunk of info we need for this one run of this code


            /*
            #region QuickDetour
            //quick detour, we're going to look for our BPM in here and see if it matches with a default
            string fullMMInfoNoSpaces = NormalizeWhiteSpace(fullMainMusicInfo, true);
            CheckToAddCustomBPM(fullMMInfoNoSpaces);
            #endregion QuickDetour*/


            //Next, we look for the information after our "Bank":  ... it's going to be the bank's filename
            //if we end up unable to find a bankPath (we're expecting to, this is our game's current JSON), we will default to writing in this filename

            int indexOfName = fullMainMusicInfo.IndexOf("Bank"); //it will look like this: "Bank" : "Unstoppable_All",
            indexOfName = fullMainMusicInfo.IndexOf(":", indexOfName); //now we're at here-------^
            indexOfName = fullMainMusicInfo.IndexOf("\"", indexOfName); // now we're right before the fileName's first quote
            indexOfName += 1; //now we're 100% at the beginning of the bank's info (whatever's after "Bank" : ")


            int indexOf2ndQuote = fullMainMusicInfo.IndexOf("\"", indexOfName + 1);
            int lengthOfName = indexOf2ndQuote - indexOfName;

            string fileName = fullMainMusicInfo.Substring(indexOfName, lengthOfName); //this is whatever we have after "Bank":
            fileName = shaveSurroundingQuotesAndSpaces(fileName);

            //Now that we have the filename, let's use it to look for a bankPath, and the directory its giving

            //Start looking for where the bankPath info begins and ends
            int indexOfBankPath = fullMainMusicInfo.IndexOf("bankPath");
            if (indexOfBankPath == -1)
            {
                //we don't have a bankPath at all, return the fileName and bail
                //for all we know, they could have the Bank in the game's folder and they're testing it or they don't know how this program works
                string[] returnString = { fileName, "-2" };
                return returnString;

            }
            //indexOfBankPath is currently here: "*bankPath": " (at *)
            indexOfBankPath = fullMainMusicInfo.IndexOf(":", indexOfBankPath + 1);//now it's currently here: "bankPath"*: "
            indexOfBankPath = fullMainMusicInfo.IndexOf("\"", indexOfBankPath + 1);//now it's currently here: "bankPath": *"
            indexOfBankPath += 1; //we are now 100% AFTER the quote


            //need to fix this, in case user has colon or spaces in weird spots

            //whatever's in the "Bank": field is going to match the filename in bankPath
            string fileNameWithExt = fileName + ".bank\""; //just to be sure that a folder name shouldn't mess with this
            int indexOfFileNameInBankPath = fullMainMusicInfo.IndexOf(fileNameWithExt, indexOfBankPath); //since this will be the ending of substring, we will have ending //'s, but we're not going to have "customsongs.json"

            if (indexOfFileNameInBankPath == -1)
            {

                string[] returnString = { fileName, "-2" };
                return returnString;//we have a bankpath, but we can't find a filename for some reason in the bankpath, abort

            }

            //we need to find the foldername of the Mod. THAT is the name of our mod, and the selection that appears on the list
            //...........okay, i just realized we can't pull the info from another JSON without having a bank next to it
            //we can, but we'll have to do something besides this....
            //maybe if we just want to make a new song from a mod that already exists, and we don't want to have a bunch of copies of the .Bank,
            //we have another JSON or TXT file that has the info we want, renamed to something else (customsongnobank.json?)


            int bankPath_pathLength = indexOfFileNameInBankPath - indexOfBankPath;
            string bankPathInfo = fullMainMusicInfo.Substring(indexOfBankPath, bankPath_pathLength); //this will have extra \\ at the end; this is the folder of our bankpath, no filename

            string[] bankPathDirectories = bankPathInfo.Split(new string[] { "\\\\" }, StringSplitOptions.RemoveEmptyEntries); //since we remove empties, our last entry should be our mod name
            string allegedModName = bankPathDirectories[bankPathDirectories.Length - 1];

            //now we use the mod name to compare the info to our matching song in our mod list


            int modIndex = setListCatalog.FindStringExact(allegedModName); //this gives us the 0-based index of our mod number
            if (modIndex == -1)
            {

                //MessageBox.Show("We aren't finding our names as strings, or the folder doesn't match");
                string[] returnString = { fileName, "-2" };
                return returnString;
            }
            //SetOldSongArray kept thinking our index was +1 what it should be because of the (game) index being hidden behind it
            int indexOfGameJson = setListCatalog.FindStringExact("(game)");
            if (indexOfGameJson != -1 && indexOfGameJson > modIndex) { modIndex--; }

            //the way this works right now, there has to be a .bank file next to the JSON
            //otherwise, we don't know how to differentiate the bank and json file
            //maybe i'm just tired

            string modPath = ((ListItem)setListCatalog.Items[modIndex]).Path; //this gives us something that has the CUSTOMSONGS.JSON in it
            modPath = modPath.Substring(0, 1).ToUpper() + modPath.Substring(1);
            //string bankPathInfoWFilename = bankPathInfo + fileName + ".bank";
            string bankPathInfoWCustomSongsJson = bankPathInfo + "customsongs.json";
            //bankPathInfo lacked the filename, so we're putting it back in

            bankPathInfoWCustomSongsJson = bankPathInfoWCustomSongsJson.Replace("\\\\", "\\");


            // THE WAY THIS IS HANDLED, we can only have ONE song with the name!
            //We need to come up with something that stops the user and makes them rename one of the folders on first run
            if (modPath == bankPathInfoWCustomSongsJson)
            {
                string[] returnString = { allegedModName, modIndex.ToString() };

                return returnString;
            } else
            {

                //we have a bank path that doesn't match up with our mod. Just return the name
                string[] returnString = { fileName, "-2" };
                return returnString;
            }




        //we skip to here if we were looking for bossMusic info (this function runs once per music selection)
        //if we were looking for mainMusic, our function would have returned by now.

        BossChecker:

            //Sheol should never come up


            //check if we have boss music
            if (indexOfBossFightMusic == -1 && m_or_b == "b")
            {
                //we don't have any info for "BossMusic"

                string[] returnString = { "<default>", "-1" };
                return returnString;


            }

            //if we got this far, we DO have a BossMusic entry for this level


            //int indexofBossMusicEnd = fullLevelInfo.Length; this isn't necessary
            string fullBossMusicInfo = fullLevelInfo.Substring(indexOfBossFightMusic);

            /*
            #region QuickDetour
            //quick detour, we're going to look for our BPM in here and see if it matches with a default
            string fullBMInfoNoSpaces = NormalizeWhiteSpace(fullBossMusicInfo, true);
            CheckToAddCustomBPM(fullBMInfoNoSpaces);
            #endregion QuickDetour*/


            int indexOfNameB = fullBossMusicInfo.IndexOf("Bank"); //it will look like this: "Bank" : "Unstoppable_All",
            int tmpGoAfter1stValQuote = fullBossMusicInfo.IndexOf(":", indexOfNameB);
            tmpGoAfter1stValQuote = fullBossMusicInfo.IndexOf("\"", tmpGoAfter1stValQuote);//we're now before the value's first "
            tmpGoAfter1stValQuote += 1;
            indexOfNameB = tmpGoAfter1stValQuote;
            //indexOfNameB += 9; //now we're after the first quote of the filename
            int indexOf2ndQuoteB = fullBossMusicInfo.IndexOf("\"", indexOfNameB + 1);
            int lengthOfNameB = indexOf2ndQuoteB - indexOfNameB;


            string fileNameB = fullBossMusicInfo.Substring(indexOfNameB, lengthOfNameB);

            //Now that we have the filename, let's use it to look for a bankPath, and the directory its giving

            //Start looking for where the bankPath info begins and ends
            indexOfBankPath = fullBossMusicInfo.IndexOf("bankPath");
            if (indexOfBankPath == -1)
            {
                //we don't have a bankPath at all, return the fileName and bail
                //for all we know, they could have the Bank in the game's folder and they're testing it or they don't know how this program works
                string[] returnString = { fileNameB, "-2" };
                return returnString;

            }

            int tmpGoAfterFirstValQuote = fullBossMusicInfo.IndexOf(":", indexOfBankPath); //we are now before the colon
            tmpGoAfterFirstValQuote = fullBossMusicInfo.IndexOf("\"", tmpGoAfterFirstValQuote + 1);//we are now before the Value's first "
            tmpGoAfterFirstValQuote += 1; //now we're after the Value's first quote
            if (tmpGoAfterFirstValQuote > fullBossMusicInfo.Length)
            {
                string[] returnString = { fileNameB, "-2" };
                return returnString;
            }
            indexOfBankPath = tmpGoAfterFirstValQuote;
            //indexOfBankPath += 12; // bankPath": " <- adds up to 12 characters before we get to the file path
            //need to fix this, in case user has colon or spaces in weird spots

            //whatever's in the "Bank": field is going to match the filename
            fileNameWithExt = fileNameB + ".bank\""; //just to be sure that a folder name shouldn't mess with this
            indexOfFileNameInBankPath = fullBossMusicInfo.IndexOf(fileNameWithExt, indexOfBankPath); //since this will be the ending of substring, we will have ending //'s, but we're not going to have "customsongs.json"

            if (indexOfFileNameInBankPath == -1)
            {
                string[] returnString = { fileNameB, "-2" };
                return returnString;//we have a bankpath, but we can't find a filename for some reason in the bankpath, abort

            }


            bankPath_pathLength = indexOfFileNameInBankPath - indexOfBankPath;
            bankPathInfo = fullBossMusicInfo.Substring(indexOfBankPath, bankPath_pathLength); //this will have extra \\ at the end; this is the folder of our bankpath, no filename

            bankPathDirectories = bankPathInfo.Split(new string[] { "\\\\" }, StringSplitOptions.RemoveEmptyEntries); //since we remove empties, our last entry should be our mod name
            allegedModName = bankPathDirectories[bankPathDirectories.Length - 1];

            //now we use the mod name to compare the info to our matching song in our mod list



            modIndex = setListCatalog.FindStringExact(allegedModName); //this gives us the 0-based index of our mod number
            if (modIndex == -1)
            {
                //MessageBox.Show("We aren't finding " + allegedModName + ", or the folder doesn't match; listbox length is: " + setListCatalog.Items.Count);
                string[] returnString = { fileNameB, "-2" };
                return returnString;
            }

            indexOfGameJson = setListCatalog.FindStringExact("(game)");
            if (indexOfGameJson != -1 && indexOfGameJson > modIndex) { modIndex--; }


            modPath = ((ListItem)setListCatalog.Items[modIndex]).Path; //this gives us something that HAS a .bank filename in it
            bankPathInfoWCustomSongsJson = bankPathInfo + "customsongs.json";
            //bankPathInfo lacked the filename, so we're putting it back in

            bankPathInfoWCustomSongsJson = bankPathInfoWCustomSongsJson.Replace("\\\\", "\\");


            // THE WAY THIS IS HANDLED, we can only have ONE song with the name!
            //We need to come up with something that stops the user and makes them rename one of the folders on first run
            if (modPath == bankPathInfoWCustomSongsJson)
            {
                //Success! Our bankPath matches with a mod!
                string[] returnString = { allegedModName, modIndex.ToString() };
                return returnString;
            } else
            {
                //we have a bank path that doesn't match up with our mod. Just return the name
                string[] returnString = { fileNameB, "-2" };
                return returnString;
            }



        }

        private string capFirst(string str)
        {
            string s = str.Substring(0, 1).ToUpper() + str.Substring(1);
            return s;
        }


        /// <summary>
        /// Stores the song's values, to later recognize if a user has made changes in Organizer
        /// </summary>
        /// <param name="m_or_b">"m" for Main level, "b" for Boss fight, "" for both</param>
        private void resetSongOriginalInfo(string m_or_b)
        {
            TextBox[] mainLevelTextBoxes = { MLNameBox, MLEventBox, MLLHBEBox, MLOffsetBox, MLBPMBox };
            TextBox[] bossFightTextBoxes = { BFNameBox, BFEventBox, BFLHBEBox, BFOffsetBox, BFBPMBox };
            if (m_or_b == "m" || m_or_b == "")
            {
                for (int i = 0; i < storedOriginalInfo_m.Length; i++)
                {
                    if (i == 5)
                    {
                        storedOriginalInfo_m[5] = mTrueBankPath.Text;
                        continue;
                    }

                    storedOriginalInfo_m[i] = mainLevelTextBoxes[i].Text;

                }

                mSaveLevelInfo.Enabled = false;

            }
            if (m_or_b == "b" || m_or_b == "")
            {
                for (int i = 0; i < storedOriginalInfo_b.Length; i++)
                {
                    if (i == 5)
                    {
                        storedOriginalInfo_b[5] = bTrueBankPath.Text;
                        continue;
                    }
                    storedOriginalInfo_b[i] = bossFightTextBoxes[i].Text;
                }

                bSaveLevelInfo.Enabled = false;
            }


        }


        /* Unused..?
        private int getBPMfromJson(string fullJson, string Level, string m_or_b)
        {
            //fullJson wants the normalizedwhitespace version, not the unaltered version

            string capitalizeLevelName = Level.Substring(0, 1).ToUpper() + Level.Substring(1);
            int indexOfLevelInfo = fullJson.IndexOf(capitalizeLevelName); //appears as, for example, "LevelName" : "Voke"

            if (indexOfLevelInfo == -1)
            {
                //this level is not in the JSON file
                return -1;
            }

            int indexOfLevelInfoEnd = fullJson.IndexOf("} }", indexOfLevelInfo);
            if (indexOfLevelInfoEnd == -1)
            {
                //we have a problem, try to fix it
                fullJson.Replace("\t", "");
                indexOfLevelInfoEnd = fullJson.IndexOf("}}", indexOfLevelInfo);
            }
            if (indexOfLevelInfoEnd == -1)
            {
                indexOfLevelInfoEnd = fullJson.IndexOf("}}", indexOfLevelInfo);
            }

            if (indexOfLevelInfoEnd == -1)
            {

                return -1;
            }
            string fullLevelInfo = fullJson.Substring(indexOfLevelInfo, indexOfLevelInfoEnd - indexOfLevelInfo);


            int indexOfMainLevelMusic = fullLevelInfo.IndexOf("\"MainMusic\"");
            int indexOfBossFightMusic = fullLevelInfo.IndexOf("\"BossMusic\"");

            //check if we have MainMusic info in this level

            if (m_or_b == "b") goto BossChecker;

            if (indexOfMainLevelMusic == -1 && m_or_b == "m")
            {
                //we don't have any info for "MainMusic"
                return -1;
            }

            //if we got this far, we DO have a MainMusic entry for this level

            //check to see where MainMusic ends, by looking for "BossMusic"; our current information stops right before the "} }", so we won't find that
            int indexofMainMusicEnd = indexOfBossFightMusic;
            if (indexofMainMusicEnd == -1)
            {
                indexofMainMusicEnd = fullLevelInfo.Length;
            }
            string fullMainMusicInfo = fullLevelInfo.Substring(0, indexofMainMusicEnd);
            //we now have the chunk of info we need for this one run of this code


            int possibleCstmBPM = GetCustomBPMFrmChunk(fullMainMusicInfo);
            return possibleCstmBPM;

            //if we were checking for Main Music, we're done

            BossChecker:
            if (indexOfBossFightMusic == -1) return -1;//we don't have any info for "BossMusic"
            
            string fullBossMusicInfo = fullLevelInfo.Substring(indexOfBossFightMusic);

            
            int possibleCstmBPM_b = GetCustomBPMFrmChunk(fullBossMusicInfo);
            return possibleCstmBPM_b;

        }
        */

        /*
        int[] mainCustomBPMs = new int[9]; //there's 9 main levels with the tutorial
        int[] bossCustomBPMs = new int[7]; //Sheol and Tutorial don't have a [changeable] boss
        private void CheckAllSelectionsForBPMs()
        {
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8, mainCombo9 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };

            foreach(ComboBox combo in mainCBox)
            {
                int combosSlctdIndx = setListCatalog.FindStringExact(combo.Text);
                if (combosSlctdIndx == -1) continue;

                Button combosGrabLvlBtn = getGrabLvlBtnFromCombo(combo, "");

                int lvlNum = getLevelNumFromModGrabLvlButton(combosGrabLvlBtn);

                if(lvlNum == -1)
                {
                    //we're selected on a level but it doesn't have any song info in it
                    //that only happens if we're
                }
                


                //getBPMfromJson();

            }

            
        }*/


        /*
    List<int> customBPMs = new List<int>();
    private void CheckToAddCustomBPM(string chunkToCheck)
    {
        int indexAfterBPM = chunkToCheck.IndexOf("\"BPM\":") + "\"BPM\":".Length;
        int indexOfNxtCma = chunkToCheck.IndexOf(",", indexAfterBPM);
        int endOfBPMValue = -1;
        if (indexOfNxtCma != -1)
        {
            endOfBPMValue = indexOfNxtCma;
        }

        if (endOfBPMValue == -1) { endOfBPMValue = chunkToCheck.Length; }
        if (endOfBPMValue == -1) return;
        int lengthOfBPMValue = endOfBPMValue - indexAfterBPM;
        string allegedBPMValue = chunkToCheck.Substring(indexAfterBPM, lengthOfBPMValue);

        if (!Int32.TryParse(allegedBPMValue, out int theyWantedTheHighway)) { return; } //they're happier there, today

        int foundBPMValue = Int32.Parse(allegedBPMValue);

        if (gameMusicBankBPMs.Contains(foundBPMValue)) { return; };

        if (!customBPMs.Contains(foundBPMValue))
        {
            customBPMs.Add(foundBPMValue);
        }
    }*/



        /// <summary>
        /// Returns an int of a custom BPM # found in a chunk of code. Returns -1 if it can't be found, or 0 it's not a custom BPM
        /// </summary>
        /// <param name="chunkToCheck">A chunk of a JSON with only the Main or Boss level info isolated</param>
        private int GetCustomBPMFrmChunk(string chunkToCheck)
        {
            int indexAfterBPM = chunkToCheck.IndexOf("\"BPM\":") + "\"BPM\":".Length;
            int indexOfNxtCma = chunkToCheck.IndexOf(",", indexAfterBPM);
            int endOfBPMValue = -1;
            if (indexOfNxtCma != -1)
            {
                endOfBPMValue = indexOfNxtCma;
            }

            if (endOfBPMValue == -1) { endOfBPMValue = chunkToCheck.Length; }
            if (endOfBPMValue == -1) return -1;
            int lengthOfBPMValue = endOfBPMValue - indexAfterBPM;
            string allegedBPMValue = chunkToCheck.Substring(indexAfterBPM, lengthOfBPMValue);

            if (!Int32.TryParse(allegedBPMValue, out int theyWantedTheHighway)) { return -1; } //they're happier there, today

            int foundBPMValue = Int32.Parse(allegedBPMValue);

            if (gameMusicBankBPMs.Contains(foundBPMValue)) { return -1; };

            return foundBPMValue;

            /*if (!customBPMs.Contains(foundBPMValue))
            {
                customBPMs.Add(foundBPMValue);
                
            }*/
        }


        /// <summary>
        /// Attempts to parse an string to int, to check if it's a custom BPM #. Returns -1 if it can't be parsed, or 0 it's not a custom BPM
        /// </summary>
        /// <param name="bpmString">A chunk of a JSON with only the Main or Boss level info isolated</param>
        private double GetCustomBPM(string bpmString)
        {
            if (!double.TryParse(bpmString, out double theyWantedTheHighway)) { return -1; } //they're happier there, today

            double foundBPMValue = double.Parse(bpmString);

            if (gameMusicBankBPMs.Contains(foundBPMValue)) { return 0; };

            return foundBPMValue;
        }


        private void songInfoModified(object sender, EventArgs e)
        {

            //this function runs automatically when any text box gets changed in organizer
            TextBox calledTextbox = sender as TextBox;
            string m_or_b = calledTextbox.Name.Substring(0, 1).ToLower();

            if (m_or_b == "m" && MLNameBox.Enabled == false)
            {
                return;
            }
            if (m_or_b == "b" && BFNameBox.Enabled == false)
            {
                return;
            }

            if (areAnyTextboxesModified(m_or_b))
            {
                if (m_or_b == "m")
                {
                    mSaveLevelInfo.Enabled = true;
                } else if (m_or_b == "b")
                {
                    bSaveLevelInfo.Enabled = true;
                }
            } else
            {
                if (m_or_b == "m")
                {
                    mSaveLevelInfo.Enabled = false;
                }
                else if (m_or_b == "b")
                {
                    bSaveLevelInfo.Enabled = false;
                }
            }
        }
        private void bankInfoModified(object sender, EventArgs e)
        {
            Label calledLabel = sender as Label;
            string m_or_b = calledLabel.Name.Substring(0, 1).ToLower();
            if (areAnyTextboxesModified(m_or_b))
            {
                if (m_or_b == "m")
                {
                    mSaveLevelInfo.Enabled = true;
                }
                else if (m_or_b == "b")
                {
                    bSaveLevelInfo.Enabled = true;
                }
            }
            else
            {
                if (m_or_b == "m")
                {
                    mSaveLevelInfo.Enabled = false;
                }
                else if (m_or_b == "b")
                {
                    bSaveLevelInfo.Enabled = false;
                }
            }
        }

        //I didn't finish this because I still need to figure out why there's invisible characters in my strings
        private string[] isolateLabelAndValue(string originalLine)
        {
            string[] returnString = { "error", "error" };
            if (!originalLine.Contains(":"))
            {
                //we can't split it

                return returnString;
            }

            string[] originalLSplit = originalLine.Split(':');

            //trim left side
            //original


            return returnString;
        }

        /*
        private bool songInfoTextboxModified(TextBox tBox)
        {
            bool modified = false;
            if (tBox.Name.Substring(0, 1).ToLower() == "m")
            {
                
                int whichTextBox = Array.FindIndex(songInfoLabels, element => element == tBox.Name);
                
                if (tBox.Text == storedOriginalInfo_b[whichTextBox])


                //we want to check all of the boxes. 
                for (int i = 0; i < songInfoLabels.Length; i++)
                {
                    
                    modified = true;
                }



            }
            else if (tBox.Name.Substring(0, 1).ToLower() == "b")
            {
                songInfoLabels
            }

 
        }*/

        private bool areAnyTextboxesModified(string m_or_b)
        {
            bool modified = false;
            TextBox[] mainLevelTextBoxes = { MLNameBox, MLEventBox, MLLHBEBox, MLOffsetBox, MLBPMBox }; //does mTrueBankPath too
            TextBox[] bossFightTextBoxes = { BFNameBox, BFEventBox, BFLHBEBox, BFOffsetBox, BFBPMBox };//does bTrueBankPath too

            if (m_or_b == "m")
            {
                for (int i = 0; i < mainLevelTextBoxes.Length; i++)
                {

                    if (mainLevelTextBoxes[i].Text != storedOriginalInfo_m[i]
                        && mainLevelTextBoxes[i].Enabled)
                    {
                        modified = true;
                        return modified;
                    }

                }
                if (mTrueBankPath.Text != storedOriginalInfo_m[5]
                    && mainLevelTextBoxes[0].Enabled)
                {
                    return true;
                }
            }
            else if (m_or_b == "b")
            {

                for (int i = 0; i < bossFightTextBoxes.Length; i++)
                {

                    if (bossFightTextBoxes[i].Text != storedOriginalInfo_b[i]
                        && bossFightTextBoxes[i].Enabled)
                    {
                        modified = true;
                        return modified;
                    }

                }
                if (bTrueBankPath.Text != storedOriginalInfo_b[5]
                    && bossFightTextBoxes[0].Enabled)
                    return true;

            }

            return modified;
        }
        private bool bankPathModified(string m_or_b)
        {

            if (m_or_b == "m")
            {
                if (mTrueBankPath.Text == storedOriginalInfo_m[5])
                    return true;
                else
                    return false;

            } else if (m_or_b == "b")
            {
                if (bTrueBankPath.Text == storedOriginalInfo_b[5])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }


        /// <summary>
        /// Retrieves the info for one level's custom music, isolated to main or boss music; returns null if info can't be found or encountered error
        /// </summary>
        /// <param name="fullJson">The full customsongs.json, with normalized whitespace</param>
        /// <param name="Level">Capitalized level name we want music for</param>
        /// <param name="m_or_b">Decision to grab main or boss music</param>
        /// <returns></returns>
        private string[] getCustomInfo_MakeSetList(string fullJson, string Level, string m_or_b)
        {
            //retrieves the info for one level's custom music, isolated to main or boss music; this is used for making the Set List


            if (fullJson.Substring(0, 2) == "<>")
            {
                fullJson = fullJson.Substring(2);
            }

            if (fullJson.Contains("\t")) { fullJson = fullJson.Replace("\t", ""); } //I'm just going to do this now

            int indexOfLevelInfo = fullJson.IndexOf(Level); //appears as, for example, "LevelName" : "Voke"

            if (indexOfLevelInfo == -1)
            {
                //this level is not in the JSON file
                return null;
            }

            List<string> SngInfo = new List<string>();


            int indexOfLevelInfoEnd = fullJson.IndexOf("} }", indexOfLevelInfo);
            if (indexOfLevelInfoEnd == -1)
            {
                //we have a problem, try to fix it
                fullJson = fullJson.Replace("\t", "");
                indexOfLevelInfoEnd = fullJson.IndexOf("} }", indexOfLevelInfo);
            }
            if (indexOfLevelInfoEnd == -1)
            {
                indexOfLevelInfoEnd = fullJson.IndexOf("}}", indexOfLevelInfo);
            }

            if (indexOfLevelInfoEnd == -1) return null;
            string fullLevelInfo = fullJson.Substring(indexOfLevelInfo, indexOfLevelInfoEnd - indexOfLevelInfo);




            int indexOfMainLevelMusic = fullLevelInfo.IndexOf("\"MainMusic\"");
            int indexOfBossFightMusic = fullLevelInfo.IndexOf("\"BossMusic\"");

            if (m_or_b == "b") goto BossChecker;




            //check if we have MainMusic info in this level
            if (indexOfMainLevelMusic == -1) return null;


            //if we got this far, we DO have a MainMusic entry for this level

            //check to see where MainMusic ends, by looking for "BossMusic", or "} }"
            int indexofMainMusicEnd = indexOfBossFightMusic;
            if (indexofMainMusicEnd == -1)
            {
                indexofMainMusicEnd = fullLevelInfo.Length;
            }
            string fullMainMusicInfo = fullLevelInfo.Substring(0, indexofMainMusicEnd);


            //int currentWantedSongInfo = 0; //0 if we want Bank, 1 if we want Event, etc. ... might need to make a 6 to LOOK for possible "bankPath"

            string[] songInfo = getCustomInfoFromChunk(fullMainMusicInfo);
            return songInfo;


        //we skip to here if we didn't have a MainMusic in this level

        BossChecker:


            //check if we have boss music
            if (indexOfBossFightMusic == -1) return null;

            string fullBossMusicInfo = fullLevelInfo.Substring(indexOfBossFightMusic);
            string[] songInfoB = getCustomInfoFromChunk(fullBossMusicInfo);
            return songInfoB;



        }

        /// <summary>
        /// Gets the Bank, EventID, LowHealth EventID, Offset, BPM, and bankPath info from the chunk of code we were given
        /// </summary>
        /// <param name="chunk">A chunk from a Json, isolated as either only Main Music or Boss Music</param>
        /// <returns></returns>
        private string[] getCustomInfoFromChunk(string chunk)
        {
            string[] lbls = { "Bank", "Event", "LowHealthBeatEvent", "BeatInputOffset", "BPM", "bankPath" };

            List<string> customInfo = new List<string>();
            for (int i = 0; i < lbls.Length; i++)
            {

                int indexOfSongInfo = chunk.IndexOf(lbls[i]); //For example, this finds "Bank"

                if (indexOfSongInfo == -1) { customInfo.Add(""); continue; }//if we can't find the info for something, return an empty string(getNewLevelInfoLines will be fine for bankPath)


                int indexOfSongInfoSeperator = chunk.IndexOf(':', indexOfSongInfo); //This finds the index of the next : after the label. What's after this colon is our value we want
                //indexOfSongInfoSeperator += 2; //now we're directly after the space after colon
                indexOfSongInfoSeperator += 1; //now we're directly after colon

                //at this point, we're going to verify what we're looking at is the correct info
                if (verifyLabelNotInfo(chunk, indexOfSongInfo) == false)
                {
                    indexOfSongInfo = chunk.IndexOf(lbls[i], indexOfSongInfo + 1);
                    indexOfSongInfoSeperator = chunk.IndexOf(':', indexOfSongInfo);
                    indexOfSongInfoSeperator += 1; //now we're directly after colon
                }




                if (chunk.Substring(indexOfSongInfoSeperator + 1, 1) == " ")
                {
                    indexOfSongInfoSeperator += 1; //if there was a space after the colon, we're after it now
                }

                int indexOfSongInfoEnd = 0;

                if (i == lbls.Length - 1 && chunk.Contains("bankPath"))
                {
                    //we shouldn't need contains bankPath, since if it didn't, we would have continued out already
                    indexOfSongInfoEnd = chunk.IndexOf(" }", indexOfSongInfoSeperator);

                }
                else if (i == lbls.Length - 2 && !chunk.Contains("bankPath"))
                {
                    //if we're at BPM, and we just saw there's no BankPath
                    indexOfSongInfoEnd = chunk.IndexOf(" }", indexOfSongInfoSeperator);

                }
                else
                {
                    //we aren't at the last, so just look for the next comma
                    indexOfSongInfoEnd = chunk.IndexOf(',', indexOfSongInfoSeperator);

                }

                if (indexOfSongInfoEnd == -1)
                {
                    indexOfSongInfoEnd = chunk.Length; //we need this if we don't have a boss music
                }

                if (chunk.Substring(indexOfSongInfoSeperator, 1) == "\"")
                {
                    //if the first character of our info is a quote
                    indexOfSongInfoSeperator += 1;
                    indexOfSongInfoEnd -= 1;
                }

                string infoValue = chunk.Substring(indexOfSongInfoSeperator, indexOfSongInfoEnd - indexOfSongInfoSeperator); //this should grab .... "Bank":"EVERYTHINGINHERE"
                customInfo.Add(infoValue);
            }


            return customInfo.ToArray();
        }


        string[] storedOriginalInfo_m = new string[6]; //we're not going to worry about bankpaths->we do now
        string[] storedOriginalInfo_b = new string[6];


        /// <summary>
        /// Fills the Textboxes in Organizer with specified level's values
        /// </summary>
        /// <param name="fullJson"></param>
        /// <param name="Level"></param>
        /// <returns></returns>
        private string setSpecificLevelInfo_Org(string fullJson, string Level)
        {
            //retrieves the info for one level's custom music; this is used for ORGANIZER
            //we're returning bunk strings; the actions are handled in this code

            bool isCurrentJson = false;
            //if we see <> at the beginning, it's just an indicator to know we're looking at a Json in the game folder, meaning the one the game's currently using
            if (fullJson.Substring(0, 2) == "<>")
            {
                isCurrentJson = true;
                fullJson = fullJson.Substring(2);
            }
            int indexOfLevelInfo = fullJson.IndexOf(Level); //appears as, for example, "LevelName" : "Voke"

            clearSongInfoBoxes(); //we want to clear these anyways

            TextBox[] mainLevelTextBoxes = { MLNameBox, MLEventBox, MLLHBEBox, MLOffsetBox, MLBPMBox };
            TextBox[] bossFightTextBoxes = { BFNameBox, BFEventBox, BFLHBEBox, BFOffsetBox, BFBPMBox };

            //first check if we're on Sheol. Disable or enable Boss info input accordingly; we want this even if this level isn't supported and the function is
            //about to be canceled by "indexOfLevelInfo == -1"
            if ((Level == "Sheol" || Level == "Tutorial")
                && BFNameBox.Enabled)
            {

                for (int i = 0; i < songInfoLabels.Length; i++)
                {
                    // if (songLabels.Length > songInfoLabels.Length && i == songLabels.Length - 1) continue; //if songLabels has more than songInfoLabels, then we're looking for bankpath.
                    bossFightTextBoxes[i].Enabled = false;


                }
                bPasteLevelInfo.Enabled = false;
                bDeleteLevelInfo.Enabled = false;
            }
            else if ((Level != "Sheol" && Level != "Tutorial") && !BFNameBox.Enabled)
            {
                //if (songLabels.Length > songInfoLabels.Length && i == songLabels.Length - 1) continue; //if songLabels has more than songInfoLabels, then we're looking for bankpath.
                for (int i = 0; i < songInfoLabels.Length; i++)
                {
                    bossFightTextBoxes[i].Enabled = true;
                }
                bPasteLevelInfo.Enabled = true;
            }




            if (indexOfLevelInfo == -1)
            {
                //this level is not in the JSON file
                return "";
            }




            int indexOfLevelInfoEnd = fullJson.IndexOf("} }", indexOfLevelInfo);
            if (indexOfLevelInfoEnd == -1)
            {
                //we have a problem, try to fix it
                fullJson = fullJson.Replace("\t", "");
                indexOfLevelInfoEnd = fullJson.IndexOf("} }", indexOfLevelInfo);
            }
            if (indexOfLevelInfoEnd == -1)
            {
                indexOfLevelInfoEnd = fullJson.IndexOf("}}", indexOfLevelInfo);
            }

            if (indexOfLevelInfoEnd == -1) return "";
            string fullLevelInfo = fullJson.Substring(indexOfLevelInfo, indexOfLevelInfoEnd - indexOfLevelInfo);
            ////testFindJson.Text = fullLevelInfo;



            int indexOfMainLevelMusic = fullLevelInfo.IndexOf("\"MainMusic\"");
            int indexOfBossFightMusic = fullLevelInfo.IndexOf("\"BossMusic\"");

            /*
            if((indexOfMainLevelMusic != -1 && indexOfBossFightMusic != -1) &&
                indexOfMainLevelMusic < indexOfBossFightMusic)
            {
                indexOfMainLevelMusic = fullLevelInfo.IndexOf("\"BossMusic\"");
                indexOfBossFightMusic = fullLevelInfo.IndexOf("\"MainMusic\"");
            }*/



            string[] songLabels = songInfoLabels;


            //check if we have MainMusic info in this level
            if (indexOfMainLevelMusic == -1)
            {
                //we don't have any info for "MainMusic"

                //clear the fields
                for (int i = 0; i < songInfoLabels.Length; i++)
                {
                    mainLevelTextBoxes[i].Text = "";
                }
                mBankPathLabel.Text = "";
                mTrueBankPath.Text = "";
                //mBankPathLabel.Visible = false;
                bBankPathLabel.Text = "";
                bTrueBankPath.Text = "";
                //bBankPathLabel.Visible = false;

                mDeleteLevelInfo.Enabled = false;

                //skip down to bossMusic checker
                goto bossMusicCheck;
            }

            //if we got this far, we DO have a MainMusic entry for this level
            mDeleteLevelInfo.Enabled = true;

            //check to see where MainMusic ends, by looking for "BossMusic", or "} }"
            int indexofMainMusicEnd = indexOfBossFightMusic;


            if ((indexOfMainLevelMusic != -1 && indexOfBossFightMusic != -1) &&
                indexOfMainLevelMusic > indexOfBossFightMusic)
            {
                //so, the user put Boss Music before Main Music...
                indexofMainMusicEnd = fullLevelInfo.Length;
            }


            if (indexofMainMusicEnd == -1)
            {
                indexofMainMusicEnd = fullLevelInfo.Length;
            }
            int mainMusicLength = indexofMainMusicEnd - indexOfMainLevelMusic;

            string fullMainMusicInfo = fullLevelInfo.Substring(indexOfMainLevelMusic, mainMusicLength);


            //int currentWantedSongInfo = 0; //0 if we want Bank, 1 if we want Event, etc. ... might need to make a 6 to LOOK for possible "bankPath"


            if (1 == 1)
            {
                songLabels = new string[6];
                for (int i = 0; i < songInfoLabels.Length; i++)
                {
                    songLabels[i] = songInfoLabels[i];
                }
                songLabels[5] = "bankPath";
            }

            int indexOfMainBankPath = fullMainMusicInfo.IndexOf("bankPath");
            for (int i = 0; i < songLabels.Length; i++)
            {

                int indexOfSongInfo = fullMainMusicInfo.IndexOf(songLabels[i]); //For example, this finds "Bank"

                if (indexOfSongInfo == -1) continue; //if we can't find the info for something, forget it. This should stop not having bankPath from becoming an issue


                int indexOfSongInfoSeperator = fullMainMusicInfo.IndexOf(':', indexOfSongInfo); //This finds the index of the next : after the label. What's after this colon is our value we want
                //indexOfSongInfoSeperator += 2; //now we're directly after the space after colon
                indexOfSongInfoSeperator += 1; //now we're directly after colon

                //at this point, we're going to verify what we're looking at is the correct info

                if (verifyLabelNotInfo(fullMainMusicInfo, indexOfSongInfo) == false)
                {
                    indexOfSongInfo = fullMainMusicInfo.IndexOf(songLabels[i], indexOfSongInfo + 1);
                    indexOfSongInfoSeperator = fullMainMusicInfo.IndexOf(':', indexOfSongInfo);
                    indexOfSongInfoSeperator += 1; //now we're directly after colon
                }




                if (fullMainMusicInfo.Substring(indexOfSongInfoSeperator + 1, 1) == " ")
                {
                    indexOfSongInfoSeperator += 1; //if there was a space after the colon, we're after it now
                }

                int indexOfSongInfoEnd = 0;

                if (i == songLabels.Length - 1 && indexOfMainBankPath > -1)
                {
                    //we shouldn't need indexOfMainBankPath > -1, since if it was, we would have continued out already
                    indexOfSongInfoEnd = fullMainMusicInfo.IndexOf(" }", indexOfSongInfoSeperator);

                }
                else if (i == songLabels.Length - 2 && indexOfMainBankPath == -1)
                {
                    //if we're at BPM, and we just saw there's no BankPath for main
                    indexOfSongInfoEnd = fullMainMusicInfo.IndexOf(" }", indexOfSongInfoSeperator);

                }
                else
                {
                    //we aren't at the last, so just look for the next comma
                    indexOfSongInfoEnd = fullMainMusicInfo.IndexOf(',', indexOfSongInfoSeperator);

                }

                /*
                if (i != songLabels.Length - 2 && indexOfMainBankPath == -1)
                {
                    //we're checking the last item, which is BPM

                    indexOfSongInfoEnd = fullMainMusicInfo.IndexOf(',', indexOfSongInfoSeperator);

                }else if (i != songLabels.Length - 2 && indexOfMainBankPath == -1)
                {
                    //we're checking the last item, which is BPM

                    indexOfSongInfoEnd = fullMainMusicInfo.IndexOf(',', indexOfSongInfoSeperator);

                }
                else
                {
                    indexOfSongInfoEnd = fullMainMusicInfo.IndexOf(" }", indexOfSongInfoSeperator);
                }*/

                if (indexOfSongInfoEnd == -1)
                {
                    indexOfSongInfoEnd = fullMainMusicInfo.Length; //we need this if we don't have a boss music
                }

                if (fullMainMusicInfo.Substring(indexOfSongInfoSeperator, 1) == "\"")
                {
                    //if the first character of our info is a quote
                    indexOfSongInfoSeperator += 1;
                    indexOfSongInfoEnd -= 1;
                }


                int lengthOfSongInfo = indexOfSongInfoSeperator - indexOfSongInfo;
                string songInfo = fullMainMusicInfo.Substring(indexOfSongInfoSeperator, indexOfSongInfoEnd - indexOfSongInfoSeperator); //this should grab .... "Bank":"EVERYTHINGINHERE"


                if (indexOfLevelInfo != -1)
                {

                    if (i <= 4)
                    {
                        songInfo = shaveSurroundingQuotesAndSpaces(songInfo); //just in case
                        mainLevelTextBoxes[i].Text = songInfo; //fill the text box based on the info
                        storedOriginalInfo_m[i] = songInfo; //we want to store this to check later
                    }
                    else
                    {
                        if (songInfo.Substring(0, songInfo.Length - 1) == "\"")
                        {
                            songInfo = songInfo.Substring(0, songInfo.Length - 1); //we have a " at the end that we're gonna get rid of
                        }
                        mTrueBankPath.Text = shaveSurroundingQuotesAndSpaces(songInfo);
                        songInfo = songInfo.Replace("\\\\", "\\");
                        songInfo = pathShortener(songInfo, 40);
                        songInfo = shaveSurroundingQuotesAndSpaces(songInfo); //this needs to be before we add "bankPath":
                        songInfo = songInfo.Substring(0, 1).ToUpper() + songInfo.Substring(1); //goes from c:/ to C:/
                        songInfo = "bankPath: " + songInfo;
                        mBankPathLabel.Text = songInfo;

                        //mBankPathLabel.Visible = true;
                    }
                } else
                {
                    //Json does not have song in it
                }
            }


        //we skip to here if we didn't have a MainMusic in this level

        bossMusicCheck:

            /* This is originaly where we used to check for Sheol */


            //check if we have boss music
            if (indexOfBossFightMusic == -1)
            {
                //we don't have any info for "BossMusic"

                //clear the fields
                for (int i = 0; i < songInfoLabels.Length; i++)
                {
                    bossFightTextBoxes[i].Text = "";
                }
                bBankPathLabel.Text = "";
                bTrueBankPath.Text = "";
                //bBankPathLabel.Visible = false;

                bDeleteLevelInfo.Enabled = false;

                //skip down to end checker
                goto skipBossChecker;
            }

            //we do have boss music if we've gotten this far
            bDeleteLevelInfo.Enabled = true;

            //i wish i knew why i did 1 == 1
            if (1 == 1)
            {
                songLabels = new string[6];
                for (int i = 0; i < songInfoLabels.Length; i++)
                {
                    songLabels[i] = songInfoLabels[i];
                }
                songLabels[5] = "bankPath";
            }
            /*
            if (isCurrentJson)
            {
                songLabels = new string[6];
                for (int i = 0; i < songInfoLabels.Length; i++)
                {
                    songLabels[i] = songInfoLabels[i];
                }
                songLabels[5] = "bankPath";
            }*/

            int indexofBossMusicEnd = -1;
            if ((indexOfMainLevelMusic != -1 && indexOfBossFightMusic != -1) &&
                indexOfMainLevelMusic > indexOfBossFightMusic)
            {
                //so, the user put Boss Music before Main Music...
                indexofBossMusicEnd = indexOfMainLevelMusic;
            }

            if (indexofBossMusicEnd == -1)
            {
                indexofBossMusicEnd = fullLevelInfo.Length;
            }
            int bossMusicLength = indexofBossMusicEnd - indexOfBossFightMusic;

            string fullBossMusicInfo = fullLevelInfo.Substring(indexOfBossFightMusic, bossMusicLength);


            int indexOfBossBankPath = fullBossMusicInfo.IndexOf("bankPath");
            for (int i = 0; i < songLabels.Length; i++)
            {


                int indexOfSongInfo = fullBossMusicInfo.IndexOf(songLabels[i]); //For example, this finds "Bank"
                if (indexOfSongInfo == -1) continue; //if we can't find the info for something, forget it. This should stop not having bankPath from becoming an issue
                int indexOfSongInfoSeperator = fullBossMusicInfo.IndexOf(':', indexOfSongInfo); //This finds the index of the next : after the label. What's after this colon is our value we want
                indexOfSongInfoSeperator += 1; //now we're directly after the colon

                //at this point, we're going to verify what we're looking at is the correct info

                if (verifyLabelNotInfo(fullBossMusicInfo, indexOfSongInfo) == false)
                {
                    indexOfSongInfo = fullBossMusicInfo.IndexOf(songLabels[i], indexOfSongInfo + 1);
                    indexOfSongInfoSeperator = fullBossMusicInfo.IndexOf(':', indexOfSongInfo);
                    indexOfSongInfoSeperator += 1; //now we're directly after colon
                }


                if (fullBossMusicInfo.Substring(indexOfSongInfoSeperator + 1, 1) == " ")
                {
                    indexOfSongInfoSeperator += 1; //if there was a space after the colon, we're after it
                }

                int indexOfSongInfoEnd = 0;
                /*
                if (i == songLabels.Length - 1 && indexOfBossBankPath > -1)
                {
                    //we shouldn't need indexOfBossBankPath > -1, since if it was, we would have continued out already
                    indexOfSongInfoEnd = fullBossMusicInfo.Length;

                } else if (i == songLabels.Length - 2 && indexOfBossBankPath == -1)
                {
                    //if we're at BPM, and we just saw there's no BankPath for boss
                    indexOfSongInfoEnd = fullBossMusicInfo.Length;

                }
                else
                {
                    //we aren't at the last, so just look for the next comma
                    indexOfSongInfoEnd = fullBossMusicInfo.IndexOf(',', indexOfSongInfoSeperator);

                }*/

                if (i == songLabels.Length - 1 && indexOfBossBankPath > -1)
                {
                    //we shouldn't need indexOfMainBankPath > -1, since if it was, we would have continued out already
                    indexOfSongInfoEnd = fullBossMusicInfo.IndexOf(" }", indexOfSongInfoSeperator);

                }
                else if (i == songLabels.Length - 2 && indexOfBossBankPath == -1)
                {
                    //if we're at BPM, and we just saw there's no BankPath for main
                    indexOfSongInfoEnd = fullBossMusicInfo.IndexOf(" }", indexOfSongInfoSeperator);

                }
                else
                {
                    //we aren't at the last, so just look for the next comma
                    indexOfSongInfoEnd = fullBossMusicInfo.IndexOf(',', indexOfSongInfoSeperator);
                }
                if (indexOfSongInfoEnd == -1) indexOfSongInfoEnd = fullBossMusicInfo.Length;



                if (fullBossMusicInfo.Substring(indexOfSongInfoSeperator, 1) == "\"")
                {
                    //if the first character of our info is a quote
                    indexOfSongInfoSeperator += 1;
                    indexOfSongInfoEnd -= 1;
                }


                int lengthOfSongInfo = indexOfSongInfoSeperator - indexOfSongInfo;
                string songInfo = fullBossMusicInfo.Substring(indexOfSongInfoSeperator, indexOfSongInfoEnd - indexOfSongInfoSeperator); //this should grab .... "Bank":"EVERYTHINGINHERE"


                if (indexOfLevelInfo != -1)
                {
                    if (i <= 4)
                    {
                        songInfo = shaveSurroundingQuotesAndSpaces(songInfo);
                        bossFightTextBoxes[i].Text = songInfo;
                        storedOriginalInfo_b[i] = songInfo;
                    } else
                    {
                        if (songInfo.Substring(0, songInfo.Length - 1) == "\"")
                        {
                            songInfo = songInfo.Substring(0, songInfo.Length - 1); //we have a " at the end that we're gonna get rid of
                            //this doesn't seem to be working, so i'm adding in shaveSurroundingQuotesAndSpaces
                        }

                        bTrueBankPath.Text = shaveSurroundingQuotesAndSpaces(songInfo);
                        bBankPathTextbox.Text = songInfo;
                        songInfo = songInfo.Replace("\\\\", "\\");
                        songInfo = pathShortener(songInfo, 40);
                        songInfo = shaveSurroundingQuotesAndSpaces(songInfo); //this needs to be before we add "bankPath":
                        songInfo = "bankPath: " + songInfo;
                        bBankPathLabel.Text = songInfo;


                        //bBankPathLabel.Visible = true;
                    }

                }
                else
                {
                    //Json does not have song in it
                }
            }

        skipBossChecker:

            return "hello world";
        }

        /// <summary>
        /// Saves the new info in Organizer to the customsongs.json. Returns true if successful, false if not
        /// </summary>
        /// <param name="zeroIndexLevel"></param>
        /// <param name="m_or_b"></param>
        /// <param name="newInfo"></param>
        /// <returns></returns>
        private bool SaveLevelInfo_Organizer(int zeroIndexLevel, string m_or_b, string newInfo)
        {
            //if (listBox1.SelectedIndex == -1) { return false; } //nothing's selected, we don't know what to change. this shouldn't ever happen
            if (currentListSelection == -1) return false;

            string jsonSelection = listBox1.Items[currentListSelection].ToString();
            //string jsonSelection = listBox1.SelectedItem.ToString(); <-used to be this




            bool skipBackupCreator = false; //gets set to true if we're editing the game's customsongs.json

            //if we got this far, we're selecting something
            //we want to check if there's a folder called "Original JSON"
            string thisModsFolder = di + "\\" + jsonSelection;
            string possibleOriginalFolder = thisModsFolder + "\\" + "_Original";
            DirectoryInfo possibleOgFolder = new DirectoryInfo(@possibleOriginalFolder);
            string possibleOriginalJson = thisModsFolder + "\\" + "_Original\\customsongs.json";

            string fullSongJsonInfo = Injector_GetModJson(currentListSelection);//gets the entire Json for the mod we're selecting
            //MessageBox.Show("Grr:\n" + fullSongJsonInfo);
            if (fullSongJsonInfo.Substring(0, 2) == "<>")
            {
                //if we're editing our game's current customsongs.json, we're not making an "original" folder
                fullSongJsonInfo = fullSongJsonInfo.Substring(2);
                thisModsFolder = gameDir.ToString();
                skipBackupCreator = true;
            }

            string LvlNameCapd = allLevelNames[zeroIndexLevel].Substring(0, 1).ToUpper() + allLevelNames[zeroIndexLevel].Substring(1).ToLower(); //voke->Voke
            string newJson = getJsonWithInjection(fullSongJsonInfo, LvlNameCapd, m_or_b, newInfo);


            newJson = newJson.Replace("\n\n", "\n"); //having too many returns is easy. having not enough is stupid

            newJson = fixAllCommas(newJson);


            bool newJsonHasErrors = verifyNoErrors(newJson.ToString());
            if (newJsonHasErrors)
            {
                //MessageBox.Show("The updated info could not be saved: errors were found in Json's formatting when attempting to save. Please check the formatting of your new information.");
                //Clipboard.SetText(newJson);

                askToSendAttemptedSaveToDebug(newJson);

                //MessageBox.Show("New Json:\n" + newJson);
                return false;
            }

            if (skipBackupCreator) goto EditJson;

            if (!possibleOgFolder.Exists) Directory.CreateDirectory(possibleOriginalFolder); //if the directory already exists, this shouldn't do anything<-BULLSHIT!!

            if (!File.Exists(possibleOriginalJson))
            {
                //we don't have an "Original" customsongs.json
                string[] filePaths = Directory.GetFiles(thisModsFolder, "*.json"); //just get a list of JSON files; i donno why there'd be more than one
                foreach (string filename in filePaths)
                {
                    //filename is going to be the whole filepath
                    if (filename.Substring(filename.Length - 16) == "customsongs.json")
                    {
                        //we have the Mod's customsongs.json file
                        string sourceFile = filename;
                        string destinationFile = possibleOriginalFolder + "\\customsongs.json";
                        //string destinationFile = possibleOriginalFolder;
                        try
                        {
                            File.Copy(filename, destinationFile); //copy the original customsongs.json to the "Original" folder
                        }
                        catch
                        {
                            MessageBox.Show("The updated info could not be saved: error occured when attemping to back up original customsongs.json");
                            return false;
                        }

                    }
                }
                //if we got this far, we didn't have a backup before this, and now we do. Enable the restore buttons
                organizer_restoreJson.Visible = true;
                restoredLabel.Visible = true;

            }

        //if Original file already exists, and we skipped the last if statement, all we need to do is edit our already-made Json
        EditJson:



            try
            {
                File.WriteAllText(thisModsFolder + "\\customsongs.json", newJson);
            }
            catch
            {
                if (thisModsFolder == gameDir.ToString())
                {
                    MessageBox.Show("An error occured when saving game's customsongs.json. :(");
                } else
                {
                    // MessageBox.Show("This mods folder:\n" + thisModsFolder);
                    MessageBox.Show("An error occured when saving Json file. A backup of it should be found in song's directory.");
                }
                return false;
            }

            return true;
        }

        private void SaveJsonFromDebug(string newJson, string pathFromModFolder)
        {
            string thisModsFolder = pathFromModFolder.Replace("\\customsongs.json", "");
            if (thisModsFolder == gameDir.ToString()) goto SaveJson;

            if (!thisModsFolder.Contains(di.ToString())) thisModsFolder = di + pathFromModFolder;

            if (pathFromModFolder == "(gamedir)")
            {
                //we don't make a backup for the game's customsongs.json
                goto SaveJson;
            }



            string possibleOriginalFolder = thisModsFolder + "\\" + "_Original";
            DirectoryInfo possibleOgFolder = new DirectoryInfo(@possibleOriginalFolder);
            string possibleOriginalJson = thisModsFolder + "\\" + "_Original\\customsongs.json";

            //string fSongJsonInfo = GetJsonFromPath(pathFromModFolder); 
            //string fullSongJsonInfo = Injector_GetModJson(); This was causing errors and I made a new function and we didn't even NEED THIS!!!alsd;kfjsd;lfk

            if (!Directory.Exists(possibleOriginalFolder))
            {
                Directory.CreateDirectory(possibleOriginalFolder);
            }

            if (!File.Exists(possibleOriginalJson))
            {
                //we don't have an "Original" folder, and/or we don't have a JSON file inside of it
                string[] filePaths = Directory.GetFiles(thisModsFolder, "*.json"); //just get a list of JSON files; i donno why there'd be more than one
                foreach (string filename in filePaths)
                {
                    //filename is going to be the whole filepath
                    if (filename.Substring(filename.Length - 16) == "customsongs.json")
                    {
                        //we have the Mod's customsongs.json file
                        string sourceFile = filename;
                        string destinationFile = possibleOriginalFolder + "\\customsongs.json";
                        //string destinationFile = possibleOriginalFolder;
                        try
                        {
                            File.Copy(filename, destinationFile); //copy the original customsongs.json to the "Original" folder
                        }
                        catch
                        {
                            MessageBox.Show("Json file could not be saved because there was an error making a back up of the original. We apologize for any inconvenience.");
                            return;
                        }

                    }
                }

            }

        SaveJson:

            try
            {
                File.WriteAllText(thisModsFolder + "\\customsongs.json", newJson);
            }
            catch
            {
                //MessageBox.Show("This mods folder:\n" + thisModsFolder);
                MessageBox.Show("An error occured when saving Json file. A backup of it should be found in song's directory.");
            }

        }


        private bool verifyLabelNotInfo(string fullInfo, int anyIndexInsideLabel)
        {
            int indexOfCheck = fullInfo.LastIndexOf("\"", anyIndexInsideLabel); //anyIndexInsideLabel must be AFTER the first quote; now we have the quote

            int indexOf2ndLabelQuote = fullInfo.IndexOf("\"", indexOfCheck) + 1;


            int indexOfLineEnd = fullInfo.IndexOf(",", indexOfCheck);
            if (indexOfLineEnd == -1)
            {
                indexOfLineEnd = fullInfo.IndexOf("}", indexOfCheck);
            }
            if (indexOfLineEnd == -1)
            {
                indexOfLineEnd = fullInfo.Length;
            }

            int infoLength = indexOfLineEnd - indexOfCheck;
            string info = fullInfo.Substring(indexOfCheck, infoLength);

            if (!info.Contains(":"))
            {
                //no colon can be found
                return false;
            }

            if ((info.IndexOf(":") + indexOfCheck) > indexOfCheck && (info.IndexOf(":") + indexOfCheck) < indexOfLineEnd)
            {

                return true;
            }

            return false;
        }

        private int findNextLineWith(string[] hayLines, string needleToFind, int startingLine = 0)
        {
            int currentLine = startingLine;

            while (currentLine < hayLines.Length)
            {
                if (hayLines[currentLine].Contains(needleToFind))
                {
                    return currentLine;
                }
                currentLine++;
            }


            return -1;
        }


        //this function is meant to look at a string(hay) like: "AbbbbbbbbHello"
        // and our needle, like a b
        // and return the index between the first b after A
        //needle in haystack doesn't work for a metaphor here, unless we have a bunch of needles, and we're looking for where we find a thumbtack or something else instead
        private int findFirstInstance_fromLastIndex(string originalHay, string needle, int startingIndex)
        {
            int currentIndexLoc = startingIndex;
            string ogHay = originalHay;
            if (startingIndex < 1) return -1;


            startingIndex -= 1; //without this, we would just immediately look at the thing in front of us. we can keep this in, technically, but not if our starting index had another 'needle' after it
                                //we could have just put the currentIndexLoc before the analyzedChar declaration



            //starting with the character before startingIndex, we look at the character and see if it matches our needle
            //we're trying to return the index of the first 
            while (currentIndexLoc >= 0)
            {
                string analyzedChar = ogHay.Substring(currentIndexLoc, 1);
                //if we hit something that doesn't match our needle, we found our fist instance
                if (analyzedChar != needle)
                {
                    return currentIndexLoc + 1; //+1 because we want the first instance of whatever our needle is
                }

                currentIndexLoc--;
            }

            //we got to the beginning of the entire string without finding a result
            return -1;

        }

        /// <summary>
        /// Gets the full block of the level info from the json given
        /// </summary>
        /// <param name="fullJson"></param>
        /// <param name="LevelCapped"></param>
        /// <returns></returns>
        private string getTrueFullLevelBlock(string fullJson, string LevelCapped)
        {
            string[] lines = fullJson.Split('\n');

            int curlyCloseCounter = 0;
            bool foundLevel = false;
            string fullLevelBlock = "";
            if (!LevelCapped.Contains("\""))
            {
                LevelCapped = "\"" + LevelCapped + "\"";
            }


            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(LevelCapped) && !foundLevel)
                {
                    foundLevel = true;
                    fullLevelBlock += lines[i - 1] + "\n";
                    fullLevelBlock += lines[i] + "\n";
                    continue;
                }

                if (foundLevel)
                {
                    fullLevelBlock += lines[i] + "\n";
                    if (lines[i].Contains("}") && !lines[i].Contains("{"))
                    {
                        curlyCloseCounter++;
                    }
                    else
                    {
                        curlyCloseCounter = 0;
                    }
                }

                if (curlyCloseCounter == 2)
                    break;
            }


            return fullLevelBlock;
        }

        /// <summary>
        /// Deprecated, do not use, does not work
        /// </summary>
        /// <param name="fullJson"></param>
        /// <param name="LevelCapped"></param>
        /// <returns></returns>
        private string getTRUEFullInfoForLevel(string fullJson, string LevelCapped)
        {
            //this code is meant to give us the full level string

            //we're not going to split the level info like we do in the other method


            string returnString = "1A";

            int indexOfLevelName = fullJson.IndexOf("\"" + LevelCapped + "\",");

            if (indexOfLevelName == -1) return "2B-a";

            int indexOfLevelCurlyOpen = fullJson.LastIndexOf("{", indexOfLevelName);
            if (indexOfLevelCurlyOpen == -1) return "2B-b";

            int indexOfLevelCurlyOpenLINE = findFirstInstance_fromLastIndex(fullJson, " ", indexOfLevelCurlyOpen - 1);//this gives us the index of the beginning of the line with the level's opening {
            //originally this kept returning the startingIndex+1.. with indexOfLevelCurlyOpen-1, it doesn't. I don't get it. hopefully there's actual spaces before the Json's first level's opening {

            if (indexOfLevelCurlyOpenLINE == -1) return "2B-c";


            //first look for the first }. From there, we'll search if we have MainMusic or just BossMusic
            int indexOfFirstCurlyClose = fullJson.IndexOf("}", indexOfLevelName); //event's curly close
            if (indexOfFirstCurlyClose == -1) return "0F-a";
            int indexOf2ndCurlyClose = fullJson.IndexOf("}", indexOfFirstCurlyClose + 1); //lowHealthBeatEvent's curly close
            if (indexOf2ndCurlyClose == -1) return "0F-b";
            int indexOfMusicCurlyClose = fullJson.IndexOf("}", indexOf2ndCurlyClose + 1); //MainMusic or Boss Music's curly close
            if (indexOfMusicCurlyClose == -1) return "0F-c";
            //this was stupid, I could have just put IndexOf(" }", with a space...

            //if (indexOfFirstCurlyClose == -1) return "3C";

            //firstMusicInfo gives us the level's information up until the MainMusic or BossMusic's(whatever's there first) closing }
            int firstMusicInfoLength = indexOfMusicCurlyClose - indexOfLevelCurlyOpenLINE;
            if (firstMusicInfoLength < 0) return "4D";

            int indexOfLevelCurlyClose = -1;






            if (fullJson.Substring(indexOfLevelCurlyOpenLINE, firstMusicInfoLength).Contains("BossMusic"))
            {
                //we already have the end of the Level, so get the level's closing }
                indexOfLevelCurlyClose = fullJson.IndexOf("}", indexOfMusicCurlyClose + 1);

                //if we have a comma after the level bracket, it should get fixed later)
                /*
                if(fullJson.Substring(indexOfLevelCurlyClose+1, 1) == ",")
                {

                }*/
            } else if (fullJson.Substring(indexOfLevelCurlyOpenLINE, firstMusicInfoLength).Contains("MainMusic"))
            {
                //We have MainMusic in our first section
                //it's possible we have a Boss section too, though


                //the next string in quotes will have "BossMusic" or "LevelName", or there won't be any quotes
                //since they both have the same Length (9), we can just look for that

                int indexOf1stQuoteAfterMain = fullJson.IndexOf("\"", indexOfMusicCurlyClose); //index of the first " after the MainMusic block
                if (indexOf1stQuoteAfterMain == -1)
                {
                    //we had Main Music, but we don't have boss music, AND we don't have any other music info
                    indexOfLevelCurlyClose = fullJson.IndexOf("}", indexOfMusicCurlyClose + 1); //same as if we only had boss music
                }

                //we'll do 11 and make sure we're grabbing the quotes. Back slashes aren't counted in length apparently
                if (fullJson.Substring(indexOf1stQuoteAfterMain, 11) == "\"BossMusic\"")
                {
                    int indexOfFirstCurlyCloseB = fullJson.IndexOf("}", indexOfMusicCurlyClose + 1); //boss's event's curly close
                    if (indexOfFirstCurlyCloseB == -1) return "0F-z";
                    int indexOf2ndCurlyCloseB = fullJson.IndexOf("}", indexOfFirstCurlyCloseB + 1); //boss's lowHealthBeatEvent's curly close
                    if (indexOf2ndCurlyCloseB == -1) return "0F-y";
                    int indexOfMusicCurlyCloseB = fullJson.IndexOf("}", indexOf2ndCurlyCloseB + 1); //Boss Music's curly close
                    if (indexOfMusicCurlyCloseB == -1) return "0F-x";
                    indexOfLevelCurlyClose = fullJson.IndexOf("}", indexOfMusicCurlyCloseB + 1);//same as if we only had boss music
                } else
                {
                    //we only had MainMusic
                    indexOfLevelCurlyClose = fullJson.IndexOf("}", indexOfMusicCurlyClose + 1);//same as if we only had boss music
                }

            }

            if (indexOfLevelCurlyClose == -1) return "5E-a";

            int indexOfFirstSpaceAfterLevelInfo = fullJson.IndexOf(" ", indexOfLevelCurlyClose); //this will hopefully give us the line-return
            if (indexOfFirstSpaceAfterLevelInfo == -1)
            {
                indexOfFirstSpaceAfterLevelInfo = fullJson.IndexOf("\t", indexOfLevelCurlyClose + 1); //try this again.... we'll look for an indentation
            };
            if (indexOfFirstSpaceAfterLevelInfo == -1)
            {
                indexOfFirstSpaceAfterLevelInfo = fullJson.IndexOf("}", indexOfLevelCurlyClose + 1); //try this again.... we'll just look for the next goddamn }
            };


            if (indexOfFirstSpaceAfterLevelInfo == -1) return "5E-b";



            //int fullLevelInfoLength = indexOfLevelCurlyClose + 1 - indexOfLevelCurlyOpenLINE ; //+1 to actually get the level's curly close
            int fullLevelInfoLength = indexOfFirstSpaceAfterLevelInfo - indexOfLevelCurlyOpenLINE;

            /* we did this when we weren't returning the line-break
            string charAfterLevelCurlyClose = fullJson.Substring(indexOfLevelCurlyOpenLINE + fullLevelInfoLength, 1);//the string after the level's closing bracket
            //if the character after the level's curly close is a comma, include it
            if(charAfterLevelCurlyClose == ",")
            {
                fullLevelInfoLength++;
            }*/

            returnString = fullJson.Substring(indexOfLevelCurlyOpenLINE, fullLevelInfoLength);
            //MessageBox.Show(returnString);

            return returnString;
        }




        //need to create a function to inject code into a JSON that already exists
        //the function will take a string (the full JSON)
        //it then checks to see if the level exists in the JSON file
        //if it doesn't, we're adding new code
        //if it DOES exist, we need to check and see if that level has information for the main or boss (whichever we're trying to change)
        //if it does NOT have information for the main/boss that we're choosing to edit, yet had the level, that means we had information for the other choice
        //if it DOES have information for the main/boss that we're choosing to edit, then we're just replacing THAT information

        private string getJsonWithInjection(string fullJson, string Level, string m_or_b, string newLevelInfoNugget)
        {
            //this takes a given json, and returns the same full Json with new code written inside of it

            //check if level exists in JSON
            //MessageBox.Show("New Nugget: \n" + newLevelInfoNugget);

            int selectedLevelInt = getSelectedLevel_OrganizerInjector(); //zero-based index, gives us what level we're selecting in Organizer
            string selectedLevelNameCapped = allLevelNames[selectedLevelInt].Substring(0, 1).ToUpper() + allLevelNames[selectedLevelInt].Substring(1).ToLower(); //voke->Voke

            bool[] supportedLevels = jsonHasLevelAlready(fullJson); //get an array of booleans, one for each level saying if the level is in the Json already or not

            //if the level we're editing is in the Json already
            if (supportedLevels[selectedLevelInt])
            {
                //the level we're editing had info in the JSON already
                //replace the info
                string replacementBlock = replaceInfoForExistingLevel(fullJson, Level, m_or_b, newLevelInfoNugget); //make a chunk that has new level info

                replacementBlock = replacementBlock.Replace("/n/n", "/n"); //we're going to get rid of all double-returns...

                string originalBlock = getTrueFullLevelBlock(fullJson, Level); //used to be getTRUEFullInfoForLevel(fullJson, Level);

                //originalBlock = originalBlock.Replace("\r", "");
                //string originalBlock = getFullInfoForLevel(fullJson, Level); //find the original chunk of the level info
                //return originalBlock;
                //originalBlock = originalBlock.Replace("\n", "");
                if (originalBlock.Length == 2 || originalBlock.Length == 4)
                {

                    MessageBox.Show("Could not replace level info," + Environment.NewLine +
                        "Error Code: " + originalBlock);
                }

                string returnString = fullJson;
                //returnString = returnString.Replace("\r", "");
                //string testString = fullJson;


                returnString = returnString.Replace(originalBlock, replacementBlock); //replace the original chunk with the new level info chunk



                //if (!returnString.Contains(originalBlock)) { MessageBox.Show("We couldn't find the original block in the JSON file"); }
                //if(returnString == fullJson) { MessageBox.Show("Original Block: " + originalBlock+"\n Replacement Block: "+replacementBlock); }

                return returnString;

            } else
            {
                //the level we're editing wasn't in the JSON before we hit Save

                //we're going to check if we have the other levels before this. if we don't, we'll add it to the top
                /*
                for(int i= selectedLevelInt; i>=0; i--)
                {

                }*/
                //MessageBox.Show("Adding new!");

                string newLevelInfoBlock = getNewLevelInjection(Level, m_or_b, newLevelInfoNugget);
                //MessageBox.Show("newLevelInfoBlock:\n" + newLevelInfoBlock);
                string returnString = injectNewLevelIntoJson(fullJson, newLevelInfoNugget, selectedLevelInt, m_or_b);
                //MessageBox.Show("Return string before fixing commas:\n" + returnString);

                returnString = fixAllCommas(returnString);
                //MessageBox.Show("Return string after fixing commas:\n" + returnString);
                return returnString;
            }



            //return "";

        }


        private void deleteMusicInfo(object sender, EventArgs e)
        {
            Button dltBtn = sender as Button;
            string m_or_b = dltBtn.Name.Substring(0, 1).ToLower();

            if (AttemptToDeleteMusic(m_or_b) == true)
            {
                //delete successful! reload Organizer page
                int dontChangeLevel = getSelectedLevel_OrganizerInjector(); //gives us whatever level Organizer is selected on for its song
                string dontChangeLevelName = allLevelNames[dontChangeLevel].Substring(0, 1).ToUpper() + allLevelNames[dontChangeLevel].Substring(1);

                clearSongInfoBoxes();
                organizer_enableLevelButtons(); //needs to be after we get the level, since this "resets" our buttons



                string songJsonInfo = Organizer_GetModJson();

                setSupportedLevelColors(songJsonInfo);
                SetSelectedLevelColors(dontChangeLevel);
                setSpecificLevelInfo_Org(songJsonInfo, dontChangeLevelName);
                resetSongOriginalInfo("");

                refreshAfterSaving();
            }
        }

        private bool AttemptToDeleteMusic(string m_or_b)
        {


            string modName = listBox1.Items[currentListSelection].ToString();
            if (modName == "Current customsongs.json")
            {
                modName = "game's current info";
            }

            int lvlInt = getSelectedLevel_OrganizerInjector();

            string MusicString = "";
            if (m_or_b == "m") MusicString = "Main";
            else MusicString = "Boss";

            string message = "Are you sure you want to delete the " + MusicString + " Music on ";
            message += capFirst(allLevelNames[lvlInt]);
            message += " for " + modName + "?";


            if (!organizer_restoreJson.Visible && modName != "game's current info")
            {
                message += "\n\nA backup of the original will be saved if you ever wish to restore it.";
            }
            else if (modName == "game's current info")
            {
                message += "\n\nThis action cannot be undone.";
            }

            MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
            string title = "Confirm";
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result != DialogResult.OK)
            {
                return false;
            }
            string modPath = "";
            string modFolder = "";
            if (modName == "game's current info")
            {
                modPath = gameDir.ToString() + "\\customsongs.json";
                modFolder = gameDir.ToString();
            }
            else
            {
                modPath = ((ListItem)listBox1.Items[currentListSelection]).Path;
                modFolder = ((ListItem)listBox1.Items[currentListSelection]).Path.Replace("\\customsongs.json", "");
            }

            string fullJson = Injector_GetModJson(currentListSelection);
            if (fullJson.Substring(0, 2) == "<>") fullJson = fullJson.Substring(2);

            string newJson = getJsonWithDeletedMusic(fullJson, lvlInt, m_or_b);
            newJson = newJson.Replace("\n\n", "\n");
            newJson = fixAllCommas(newJson);

            if (verifyNoErrors(newJson.ToString()))
            {
                //operation cancelled;
                askToSendAttemptedSaveToDebug(newJson);
                //Clipboard.SetText(newJson);
                return false;
            }


            //check if we should make a backup of json
            if (organizer_restoreJson.Visible || modName == "game's current info") goto EditJson;

            string possibleOriginalFolder = modFolder + "\\" + "_Original";
            DirectoryInfo possibleOgFolder = new DirectoryInfo(@possibleOriginalFolder);
            string possibleOriginalJson = modFolder + "\\" + "_Original\\customsongs.json";

            if (!possibleOgFolder.Exists) Directory.CreateDirectory(possibleOriginalFolder); //if the directory already exists, this shouldn't do anything<-BULLSHIT!!

            if (!File.Exists(possibleOriginalJson))
            {
                //we don't have an "Original" customsongs.json
                string[] filePaths = Directory.GetFiles(modFolder, "*.json"); //just get a list of JSON files; i donno why there'd be more than one
                foreach (string filename in filePaths)
                {
                    //filename is going to be the whole filepath
                    if (filename.Substring(filename.Length - 16) == "customsongs.json")
                    {
                        //we have the Mod's customsongs.json file
                        string sourceFile = filename;
                        string destinationFile = possibleOriginalFolder + "\\customsongs.json";
                        //string destinationFile = possibleOriginalFolder;
                        try
                        {
                            File.Copy(filename, destinationFile); //copy the original customsongs.json to the "Original" folder
                        }
                        catch
                        {
                            MessageBox.Show("The updated info could not be saved: error occured when attemping to back up original customsongs.json");
                            return false;
                        }

                    }
                }
                //if we got this far, we didn't have a backup before this, and now we do. Enable the restore buttons
                organizer_restoreJson.Visible = true;
                restoredLabel.Visible = true;

            }

        EditJson:

            try
            {
                File.WriteAllText(modPath, newJson);
            }
            catch
            {
                if (modFolder == gameDir.ToString())
                {
                    MessageBox.Show("An error occured when saving game's customsongs.json. :(");
                }
                else
                {
                    //MessageBox.Show("Mods folder:\n" + modFolder);
                    MessageBox.Show("An error occured when saving Json file. A backup of it should be found in song's directory.");
                }
                return false;

            }

            return true;

        }



        private string getJsonWithDeletedMusic(string fullJson, int levelInt, string m_or_b)
        {
            string returnString = "";
            string levelName = capFirst(allLevelNames[levelInt]);
            string originalLevelBlock = getTrueFullLevelBlock(fullJson, levelName);
            string newLevelBlock = "";
            if (m_or_b == "m")
            {
                //we're deleting Main Music

                if (originalLevelBlock.Contains("\"BossMusic\""))
                {
                    //there's also BossMusic in level chunk, don't get rid of it
                    string[] lvlChunkLines = originalLevelBlock.Split('\n');

                    bool foundMusicWeDontWant = false;
                    int foundCurlyClose = 0;//when we find one of these right after another, we're done with the level
                    foreach (string line in lvlChunkLines)
                    {
                        if (line.Contains("}") && !line.Contains("{"))
                            foundCurlyClose++;
                        else
                            foundCurlyClose = 0;


                        if (foundCurlyClose == 2)
                        {
                            //we're on the level closer, take it and end this
                            newLevelBlock += line + "\n";
                            break;
                        }

                        if (line.Contains("\"MainMusic\""))
                        {
                            foundMusicWeDontWant = true;
                            continue;
                        }

                        if (foundMusicWeDontWant)
                        {
                            if (line.Contains("\"BossMusic\""))
                            {
                                foundMusicWeDontWant = false;
                                newLevelBlock += line + "\n";
                            }
                        } else
                        {
                            newLevelBlock += line + "\n";
                        }

                    }
                    returnString = fullJson.Replace(originalLevelBlock, newLevelBlock);

                } else
                {
                    //there's no BossMusic here, just delete the whole thing
                    returnString = fullJson.Replace(originalLevelBlock, "");
                }
            }
            else
            {
                //we're deleting Boss Music

                if (originalLevelBlock.Contains("\"MainMusic\""))
                {
                    //there's also MainMusic in level chunk, don't get rid of it
                    string[] lvlChunkLines = originalLevelBlock.Split('\n');

                    bool foundMusicWeDontWant = false;
                    int foundCurlyClose = 0;
                    foreach (string line in lvlChunkLines)
                    {
                        if (line.Contains("}") && !line.Contains("{"))
                            foundCurlyClose++;
                        else
                            foundCurlyClose = 0;


                        if (foundCurlyClose == 2)
                        {
                            //we're on the level closer, take it and end this
                            newLevelBlock += line + "\n";
                            break;
                        }

                        if (line.Contains("\"BossMusic\""))
                        {
                            foundMusicWeDontWant = true;
                            continue;
                        }

                        if (foundMusicWeDontWant)
                        {
                            if (line.Contains("\"MainMusic\""))
                            {
                                foundMusicWeDontWant = false;
                                newLevelBlock += line + "\n";
                            }
                        }
                        else
                        {
                            newLevelBlock += line + "\n";
                        }

                    }

                    returnString = fullJson.Replace(originalLevelBlock, newLevelBlock);
                }
                else
                {
                    //there's no BossMusic here, just delete the whole thing
                    returnString = fullJson.Replace(originalLevelBlock, "");
                }
            }

            return returnString;
        }


        private string injectNewLevelIntoJson(string fullJson, string injection, int levelNum, string m_or_b)
        {

            //we want to put the Level information in order, so we're just going to rewrite the whole damn code
            //rearrange the blocks, and put them together.
            //then run through the new code, and add and remove commas as necessary
            string returnString = "{\n";
            returnString += "    \"customLevelMusic\" : [\n";

            for (int i = 0; i < allLevelNames.Length; i++)
            {
                string injLvlNameCapd = capFirst(allLevelNames[i]); //voke->Voke

                if (i == levelNum)
                {

                    returnString += "        {\n";
                    returnString += "            \"LevelName\" : \"" + injLvlNameCapd + "\",\n";
                    if (m_or_b == "m")
                    {
                        returnString += "            \"MainMusic\" : {\n";
                    } else
                    {
                        returnString += "            \"BossMusic\" : {\n";
                    }
                    returnString += injection + "\n"; //the injection doesn't give us the levelName line
                    returnString += "            }\n";
                    returnString += "        },\n";
                    continue;
                }

                string queriedLvlNameCapd = capFirst(allLevelNames[i]); //voke->Voke
                if (queriedLvlNameCapd.Contains(queriedLvlNameCapd))
                {
                    returnString += getTrueFullLevelBlock(fullJson, queriedLvlNameCapd);
                    //returnString += getFullInfoForLevel(fullJson, queriedLvlNameCapd); //this gives us the chunk of the level with all the whitespace and linebreaks
                }

            }
            returnString += "    ]\n";
            returnString += "}";

            //returnString.Replace("        }\n        {\n", "        },\n        {\n");//this will make sure there are commas between levels
            //returnString = fixAllCommas(returnString); //we probably don't need removeLastCommaIfFound, but I'M SICK OF COMMAS
            //returnString.Replace("        },\n    ]\n}", "        }\n    ]\n}"); //this will make sure to remove a comma if we had one on the last level
            //returnString = removeLastCommaIfFound(returnString); //this do a better job at making sure to remove a comma if we had one on the last level

            return returnString;
        }

        private string fixAllCommas(string fullText)
        {
            string[] lines = fullText.Split('\n');
            string fixedText = "";

            bool squareBracketFound = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("]"))
                {
                    squareBracketFound = true;
                    fixedText += lines[i] + "\n";
                    continue;
                }




                //we're also going to fix any bpms that don't get their bankPaths properly put in
                string lineNoWhtSpc = NormalizeWhiteSpace(lines[i], true);
                if (lineNoWhtSpc.Contains("\"BPM\":"))
                {
                    string nxtLnNS = NormalizeWhiteSpace(lines[i + 1], true);
                    if (nxtLnNS.Contains("\"bankPath\":"))
                    {

                        if (!lines[i].Contains(","))
                        {
                            fixedText += lines[i] + ",\n";
                        }
                        else
                        {
                            fixedText += lines[i] + "\n";
                        }
                        continue;
                    }
                    else if (!nxtLnNS.Contains("\"bankPath\":"))
                    {
                        //this line has BPM, and next line does not have bankPath. Make sure we have no comma
                        if (lines[i].Contains(","))
                        {
                            fixedText += lines[i].Replace(",", "") + "\n";
                        } else
                        {
                            fixedText += lines[i] + "\n";
                        }

                    }
                    continue; //dont do more work if this was BPM
                }

                if ((lines[i].Contains("}") && !lines[i].Contains("{"))
                    && !squareBracketFound)
                {
                    //the line we're on is either a Music closing line, or level closing line
                    string previousLineNoSpaces = NormalizeWhiteSpace(lines[i - 1], true);
                    string nextLineNoSpaces = NormalizeWhiteSpace(lines[i + 1], true);

                    if (previousLineNoSpaces.Contains("\"BPM\":") || lines[i - 1].Contains("\"bankPath\":"))
                    {
                        //previous line says has BPM or bankPath, we're on the Music Closing line
                        //we're on the music closing line
                        if (nextLineNoSpaces.Contains("\"BossMusic\":") || lines[i + 1].Contains("\"MainMusic\":"))
                        {
                            //we're about to open up for another level, make sure there's a comma
                            if (!lines[i].Contains(","))
                            {
                                fixedText += lines[i] + ",\n";
                            }
                            else
                            {
                                fixedText += lines[i] + "\n";
                            }
                        }
                        else
                        {
                            //we're not opening for another music next, remove this comma
                            fixedText += lines[i].Replace(",", "") + "\n";
                            //fixedText += lines[i] + "\n";
                        }


                    } else if (lines[i - 1].Contains("}") && !lines[i - 1].Contains("{"))
                    {
                        //previous line also contains } closers,
                        //we're on the level closing line
                        if (lines[i + 1].Contains("{"))
                        {
                            //we're opening up for another level, make sure there's a comma
                            if (!lines[i].Contains(","))
                            {
                                fixedText += lines[i] + ",\n";
                            } else
                            {
                                fixedText += lines[i] + "\n";
                            }
                        } else
                        {
                            //next line does not open another level, make sure there's no comma. If user put a comma in the middle, this might even throw it away
                            fixedText += lines[i].Replace(",", "") + "\n";
                        }
                    }

                } else
                {
                    fixedText += lines[i] + "\n";
                }
            }

            return fixedText;
        }

        private string removeLastCommaIfFound(string fullText)
        {
            string fixedText = "";
            int indexOfSquareBracket = fullText.IndexOf("]");
            if (indexOfSquareBracket == -1)
            {
                //MessageBox.Show("No square bracket found");
                return fullText;
            }
            int indxLastLvlsClsngCurly = fullText.LastIndexOf("}", indexOfSquareBracket);
            if (fullText.Substring(indxLastLvlsClsngCurly + 1, 1) == ",")
            {
                //we have an unneeded comma, remove it
                fixedText = fullText.Substring(0, indxLastLvlsClsngCurly + 1) + fullText.Substring(indxLastLvlsClsngCurly + 2);
                return fixedText;
            }

            //there wasn't a comma there
            return fullText;
        }

        private int getSelectedLevel_OrganizerInjector()
        {
            //this returns the zero-based index of what level is currently selected in Organizer
            //it's used by the injector to know what level we're injecting our new info into

            Button[] LevelButtons = { L1Settings, L2Settings, L3Settings, L4Settings, L5Settings, L6Settings, L7Settings, L8Settings, L0Settings };

            for (int i = 0; i < LevelButtons.Length; i++)
            {
                if (LevelButtons[i].ForeColor == Color.White)
                {
                    return i;
                }
            }
            return -1; //no level selected?
        }


        /* We don't use this anymore
        private string CheckAndFixBankPath(string bankPath)
        {
            //first we check if we have a bank path
            //if we do, we want to retrieve the REAL bank path

            if (bankPath == "") return bankPath; //no bank path
            if (bankPath == "bankPath: ") return ""; //no bank path, but we had the default text there

            //already ran checks, now we just fix it if we're still going
            string returnString = getRealBankPath(bankPath);
            return returnString;
        }*/

        /* We don't use this anymore. We just store our info in a hidden field, because this was nonsense
        private string getRealBankPath(string garbledBankPath)
        {
            //this looks at our bankPath string, and gives back the actual file we want from the original JSON
            //our bankPath string can be shortened, but if it isn't, it still doesn't have the correct // placement between directories; it also needs quotes around the filepath

            //we're either grabbing a bankPath from a mod, (which I don't know why it would have one, unless WE put one there)
            //or we're grabbing a bankPath from the main file

            //this part assumes we're grabbing a bankpath from the mainfile

            string thisJson = "";
            string[] directoryNames = garbledBankPath.Split('\\');
            if (directoryNames[directoryNames.Length - 2] == "StreamingAssets")
            {
                //our bank path is pointing a file in our game folder
                thisJson = getCurrentCustomsongsJson();
            } else
            {
                string modName = directoryNames[directoryNames.Length - 2];
                thisJson = GetRealBankPath_GetModJsonNoSubs(modName);
            }


            string returnString = "";
            string garbled = garbledBankPath;
            if(garbled.Substring(0,10) == "bankPath: ")
            {
                garbled = garbled.Substring(10);
            }

            if (garbled.Length < 4)
            {
                //our file path was extremely short and weird; either way it wasn't shortened
                goto JustGetDoubleSlashes;
            }

            int indexOfFirstSlash = garbled.IndexOf("\\");
            if (indexOfFirstSlash == -1) return "error";

            
            int indexOf2ndSlash = garbled.IndexOf("\\");
            if (indexOf2ndSlash == -1) return "error";


            if (garbled.Substring(indexOfFirstSlash+1, indexOf2ndSlash) == "..."){
                //our file path was shortened

                //look for a string in the JSON that matches the last few directories; it's HIGHLY unlikely that there's another folder with the exact same subdirectories—we're screwed if that's the case

                string lastFewDirectories = garbled.Substring(indexOf2ndSlash + 1); //gives us everything after the 2nd slash in C:/.../somewhere/MODS/modDirectory
                string lastFewDirs_jsonFormat = lastFewDirectories.Replace("\\", "\\\\");
                int indexOfBankFilepath = thisJson.IndexOf(lastFewDirs_jsonFormat); //this will tell us the index of where we first find a bankpath matching the file path we put in
                if(indexOfBankFilepath == -1) { return "error"; }

                //string everythingBeforeThisPoint = fromThisJson.Substring(0, indexOfBankFilepath);
                int indexOfBankFilepathsFirstQuote = thisJson.LastIndexOf("\"", indexOfBankFilepath); //this will give us the index right before the first quote
                if (indexOfBankFilepathsFirstQuote == -1) return "error";


                int indexOfBankFilepaths2ndQuote = thisJson.IndexOf("\"", indexOfBankFilepathsFirstQuote);
                if (indexOfBankFilepaths2ndQuote == -1) return "error";


                returnString = thisJson.Substring(indexOfBankFilepaths2ndQuote + 1, indexOfBankFilepaths2ndQuote); //this gives us the string inside the quotes; it already has the slashes added
                return returnString;

            }
            
            
            // if we got this far, our file path was not shortened

            JustGetDoubleSlashes:

            returnString = garbled.Replace("\\", "\\\\");


            return returnString;
        }
        */

        //replaceexisting replace existing
        private string replaceInfoForExistingLevel(string fullJson, string Level, string m_or_b, string newLevelInfoNugget, bool lastLineReturns = true)
        {
            //this code takes a JSON and changes only the individual lines for a level that already exists
            //MessageBox.Show("New Level info: " + newLevelInfoNugget);
            string returnString = "";


            string fullSongJsonInfo = fullJson;
            string fullInfo = getTrueFullLevelBlock(fullSongJsonInfo, Level);//level must be capped; this gives us the chunk of the level with all the whitespace and linebreaks
            //string fullInfo = getFullInfoForLevel(fullSongJsonInfo, Level); //why did i have like 5 of these that don't work!?!

            string[] levelChunkLines = fullInfo.Split('\n'); //breaks up the lines into an array of strings
            if (levelChunkLines.Length == 1)
            {
                levelChunkLines = fullInfo.Split('\r'); //all of a sudden the program keeps crashing when this isn't here... hmmmmmm......
            }


            //first line is { ... index 0
            //second line is "LevelName" : "Stygia", index 1
            //next line is "MainMusic" : { (or BossMusic), index 2
            int lineWereOn = 2; //we're on the line with "MainMusic" or "Boss Music"

            if (m_or_b.Substring(0, 1).ToLower() == "m")
            {
                //We're wanting to save the info for the MainMusic

                if (levelChunkLines[lineWereOn].Contains("\"MainMusic\""))
                {
                    //we found the info we want to change!

                    //bool addingNewLine = false;
                    int lineToAddBankpath = -1;
                    lineWereOn += 1; //now we're on the Bank line
                    string[] newLevelInfoLines = newLevelInfoNugget.Split('\n'); //the first line is going to be "Bank":
                    //newLevelInfoLines.Length can be 5 or 6, if we have a bankPath or not
                    for (int i = 0; i < newLevelInfoLines.Length; i++)
                    {
                        if (i == 5)
                        {
                            //if i is 5, it means we had a bankPath; make sure there's one here too, or make a new line if not
                            if (levelChunkLines[lineWereOn].Contains("bankPath"))
                            {
                                //the line has a "bankPath" in it already, so just replace it as normal
                                //this won't get called if we didn't have a bankPath entered when saving in Organizer
                            } else
                            {
                                //we don't have a bank path in here, we need to stop this code before it hits the code to replace the line
                                lineToAddBankpath = lineWereOn;
                                break;
                            }

                        }

                        levelChunkLines[lineWereOn] = newLevelInfoLines[i];

                        lineWereOn++;
                    }

                    if (newLevelInfoLines.Length == 5)
                    {
                        //we did not have a bankPath being added. if we had one, make it blank
                        if (levelChunkLines[lineWereOn].Contains("\"bankPath\""))
                        {

                            levelChunkLines[lineWereOn] = "";
                            lineWereOn++;
                        }
                    }


                    if (lineToAddBankpath != -1)
                    {
                        levelChunkLines = addToStringArray(levelChunkLines, newLevelInfoLines[5], lineWereOn); //runs a for loop that gives us a new string[] with newLevelInfoLines[5] added
                    }

                } else
                {
                    //we're adding MainMusic, but we already had BossMusic without MainMusic info
                    bool addingNewLine = false; //for bankPath
                    string[] newLevelInfoLines = newLevelInfoNugget.Split('\n'); //the first line is going to be "Bank":

                    //we are currently at the "BossMusic" line
                    returnString += levelChunkLines[0] + '\n' + levelChunkLines[1] + '\n'; //pull the first { and the "LevelName": lines

                    returnString += "            \"MainMusic\" : {" + '\n';

                    //add each new line into the code
                    for (int i = 0; i < newLevelInfoLines.Length; i++)
                    {
                        returnString += newLevelInfoLines[i] + '\n';
                    }

                    returnString += "            }," + '\n'; //...this close out the MainMusic
                    //EXTRA \n CLOSING N

                    //now we add the rest of our old chunk; lineWereOn should still have us at "BossMusic":
                    for (int i = lineWereOn; i < levelChunkLines.Length; i++)
                    {
                        returnString += levelChunkLines[i] + "\n";
                        lineWereOn = i;
                    }

                    return returnString; //we want to back out of this now, so we don't hit the foreach loop at the end
                    //there was probably a better way to do this, but I'm tired damnit

                }
            } else
            {
                //We're injecting code for BOSS music
                //I should probably do else if, but oh well

                if (levelChunkLines[lineWereOn].Contains("\"BossMusic\""))
                {
                    //we immediately found the info we want to change underneath the Level line!
                    //the user probably doesn't have info for MainMusic

                    int lineToAddBankpath = -1;

                    lineWereOn += 1; //now we're on the Bank line
                    string[] newLevelInfoLines = newLevelInfoNugget.Split('\n'); //the first line is going to be "Bank":
                    //if (!newLevelInfoLines[0].Contains("Bank")) MessageBox.Show("ProblemA");
                    //if (newLevelInfoLines.Length == 1) MessageBox.Show("ProblemB");


                    //newLevelInfoLines.Length can be 5 or 6, if we have a bankPath or not
                    for (int i = 0; i < newLevelInfoLines.Length; i++)
                    {
                        //i will be 5 if we were trying to change or add a bankpath
                        if (i == 5)
                        {
                            //if i is 5, it means we had a bankPath; make sure there's one here too, or make a new line if not
                            if (levelChunkLines[lineWereOn].Contains("bankPath"))
                            {
                                //the line has a "bankPath" in it already, so just replace it as normal

                            }
                            else
                            {
                                //we don't have a bank path in here, we need to stop this code before it hits the code to replace the line

                                lineToAddBankpath = lineWereOn;
                                break;
                            }

                        }

                        levelChunkLines[lineWereOn] = newLevelInfoLines[i];
                        lineWereOn++;
                    }

                    if (newLevelInfoLines.Length == 5)
                    {
                        //we did not have a bankPath being added. if we had one, make it blank
                        if (levelChunkLines[lineWereOn].Contains("\"bankPath\""))
                        {

                            levelChunkLines[lineWereOn] = "";
                            lineWereOn++;
                        }
                    }

                    if (lineToAddBankpath != -1)
                    {
                        levelChunkLines = addToStringArray(levelChunkLines, newLevelInfoLines[5], lineToAddBankpath); //runs a for loop that gives us a new string[] with newLevelInfoLines[5] added
                        //levelChunkLines = addToStringArray(levelChunkLines, newLevelInfoLines[5] + "            }\n", lineWereOn); //runs a for loop that gives us a new string[] with newLevelInfoLines[5] added
                        //newLevelInfoLines[5] +"            }\n" gives us the bankLine, with the following music-closing bracket
                    }

                }
                else
                {
                    int lineToAddBankpath = -1;

                    //we're editing or adding BossMusic, but we have all this MainMusic info in the way already
                    //bool addingNewLine = false; //for bankPath, uneeded since we use actual lineToAddBankPath
                    string[] newLevelInfoLines = newLevelInfoNugget.Split('\n'); //the first line is going to be "Bank":

                    lineWereOn += 6; //+1 puts us on Bank, +5 puts us on BPM, +6 is either "bankPath", or MainMusic's closing }
                    if (levelChunkLines[lineWereOn].Contains("bankPath"))
                    {
                        lineWereOn += 1;
                    }
                    //now we're 100% on BossMusic's closing }
                    lineWereOn += 1; //now we're on the Level's closing } or existing BossMusic info

                    if (levelChunkLines[lineWereOn].Contains("\"BossMusic\""))
                    {
                        //we already had BossMusic, so just edit it instead of adding it

                        lineWereOn += 1; //now we're the line after "BossMusic", which is "Bank":

                        for (int i = 0; i < newLevelInfoLines.Length; i++)
                        {
                            if (i == 5)
                            {
                                //if i is 5, it means we had a bankPath; make sure there's one here too, or make a new line if not
                                if (levelChunkLines[lineWereOn].Contains("bankPath"))
                                {
                                    //the line has a "bankPath" in it already, so just replace it as normal

                                }
                                else
                                {
                                    //we don't have a bank path in here, we need to stop this code before it hits the code to replace the line

                                    lineToAddBankpath = lineWereOn;

                                    continue;
                                }

                            }

                            levelChunkLines[lineWereOn] = newLevelInfoLines[i];

                            lineWereOn++;
                        }

                        if (newLevelInfoLines.Length == 5)
                        {
                            //we did not have a bankPath being added. if we had one, make it blank
                            if (levelChunkLines[lineWereOn].Contains("\"bankPath\""))
                            {

                                levelChunkLines[lineWereOn] = "";
                                lineWereOn++;
                            }
                        }


                        if (lineToAddBankpath != -1)
                        {
                            levelChunkLines = addToStringArray(levelChunkLines, newLevelInfoLines[5], lineWereOn); //runs a for loop that gives us a new string[] with newLevelInfoLines[5] added
                        }

                    } else
                    {
                        //we didn't have BossMusic yet, add it

                        string bossMusicChunk = "";
                        bossMusicChunk += "            \"BossMusic\" : {" + '\n';

                        for (int i = 0; i < newLevelInfoLines.Length; i++)
                        {
                            bossMusicChunk += newLevelInfoLines[i] + '\n';
                        }
                        bossMusicChunk += "            }";

                        levelChunkLines = addToStringArray(levelChunkLines, bossMusicChunk, lineWereOn);


                    }

                }
            }

            //if we got this far, we've replaced info inside of levelChunkLines and need to return it

            foreach (string line in levelChunkLines)
            {
                //if line was null, it was about to give us an extra return
                //if (line != null && line != "") <-used to be
                if (!string.IsNullOrWhiteSpace(line))
                {

                    returnString += line + '\n'; //I don't get how Join works
                }
            }


            //this gives me an extra return after BPM for replacing Boss Music with a Bankpath

            return returnString;

        }

        //add one entry to a string[]
        private string[] addToStringArray(string[] bookshelf, string book, int zeroBasedShelfSpace)
        {
            string[] returnStringArray = new string[bookshelf.Length + 1];
            if (zeroBasedShelfSpace == -1)
            {
                zeroBasedShelfSpace = returnStringArray.Length;//just add it to the end
            }

            for (int i = 0; i < returnStringArray.Length; i++)
            {
                if (i < zeroBasedShelfSpace)
                {
                    //we're not at our new entry yet, all's the same as before
                    returnStringArray[i] = bookshelf[i];
                } else if (i == zeroBasedShelfSpace)
                {
                    //we're at our new entry, replace it
                    returnStringArray[i] = book;
                } else if (i > zeroBasedShelfSpace)
                {
                    //we've gone past our new entry
                    returnStringArray[i] = bookshelf[i - 1];
                }
            }

            return returnStringArray;

        }

        //add string[] to a string[]
        private string[] addSeveralToStringArray(string[] bookshelf, string[] books, int zeroBasedShelfSpace)
        {
            string[] returnStringArray = new string[bookshelf.Length + books.Length];
            if (zeroBasedShelfSpace == -1)
            {
                zeroBasedShelfSpace = returnStringArray.Length;//just add it to the end
            }

            for (int i = 0; i < returnStringArray.Length; i++)
            {
                if (i < zeroBasedShelfSpace)
                {
                    //we're not at our new entry yet, all's the same as before
                    returnStringArray[i] = bookshelf[i];
                }
                else if (i == zeroBasedShelfSpace)
                {
                    for (int j = 0; j < books.Length; j++)
                    {
                        bookshelf[i] = books[j];
                    }
                    //we're at our new entry space, add them in

                }
                else if (i > zeroBasedShelfSpace)
                {
                    //we've gone past our new entry
                    returnStringArray[i] = bookshelf[i + books.Length];
                }
            }

            return returnStringArray;

        }

        private string getNewLevelInjection(string levelName, string mainOrBoss, string newLevelInfoLines)
        {
            //this function will give us our code injection if we need to add a whole new level
            //this code DOES give us a closing } for the { before the LevelName, AND it gives a comma and line break—this is corrected in getJsonWithInjection

            string returnString = "";
            string returnLine = "\n";

            returnString += "        {" + returnLine; //need to add the closing one as well
            returnString += "            \"LevelName\" : \"" + levelName + "\"," + returnLine;

            if (mainOrBoss.Substring(0, 1).ToLower() == "m")
            {
                returnString += "            \"MainMusic\" : {" + returnLine; //need to add closing one as well
            } else if (mainOrBoss.Substring(0, 1).ToLower() == "b")
            {
                returnString += "            \"BossMusic\" : {" + returnLine; //need to add closing one as well
            }

            returnString += newLevelInfoLines;
            if (returnString.Last() != '\n')
                returnString += "\n";

            returnString += "            }" + returnLine; //closes Main/BossMusic
            returnString += "        }," + returnLine; //closes the level, adding a comma. getJsonWithInjection will remove it if it's not correctly placed

            return returnString;
        }

        private bool[] jsonHasLevelAlready(string fullJson)
        {
            //returns 8 booleans, each saying if the json has information for its respective zero-based level number
            bool[] levelSupported = new bool[9];


            for (int i = 0; i < allLevelNames.Length; i++)
            {
                string cappedLvlName = allLevelNames[i].Substring(0, 1).ToUpper() + allLevelNames[i].Substring(1).ToLower();
                //contains is faster than indexof... i might need to change some things
                if (fullJson.Contains(cappedLvlName))
                {
                    levelSupported[i] = true;
                } else
                {
                    levelSupported[i] = false;
                }
            }

            return levelSupported;
        }


        private string getNewLevelInfoLines(string levelName, string mainOrBoss, string bank, string eventID, string lowHealthID, string offset, string bpm, string bankPath = "")
        {
            //this returns a string of the lines that go after "MainMusic" : {, and before the first }
            //                                                    ^- or "BossMusic"

            //if (bankPath.Contains("\n")) MessageBox.Show("FOUND IT"); //delete me
            //if (bpm.Contains("\n")) MessageBox.Show("FOUND IT"); //delete me

            string returnString = "";
            string returnLine = "\n";

            string bankInfo = shaveSurroundingQuotesAndSpaces(bank);
            string eventIDInfo = eventID.Replace("\"", "").Replace(" ", ""); //did this when I was putting "bank" in returnstring instead of "bankInfo". But screw it...
            string lowHealthIDInfo = lowHealthID.Replace("\"", "").Replace(" ", "");
            //add { if it's not there for events
            if (eventIDInfo.First() != '{') eventIDInfo = "{" + eventIDInfo;
            if (lowHealthIDInfo.First() != '{') lowHealthIDInfo = "{" + lowHealthIDInfo;

            if (eventIDInfo.Last() != '}') eventIDInfo = eventIDInfo + "}";
            if (lowHealthIDInfo.Last() != '}') lowHealthIDInfo = lowHealthIDInfo + "}";


            string offsetInfo = shaveSurroundingQuotesAndSpaces(offset); //there shouldn't be quotes, but... just in case
            string bpmInfo = shaveSurroundingQuotesAndSpaces(bpm); //there shouldn't be quotes here either

            string bankPathInfo = "";
            if (bankPath != "") { bankPathInfo = shaveSurroundingQuotesAndSpaces(bankPath); }
            //bankPathInfo = CheckAndFixBankPath(bankPathInfo);
            if (bankPathInfo == "error") return "ERROR";


            returnString += "                \"Bank\" : \"" + bankInfo + "\"," + returnLine;
            returnString += "                \"Event\": \"" + eventIDInfo + "\"," + returnLine;
            returnString += "                \"LowHealthBeatEvent\": \"" + lowHealthIDInfo + "\"," + returnLine;
            returnString += "                \"BeatInputOffset\": " + offsetInfo + "," + returnLine;
            returnString += "                \"BPM\": " + bpmInfo.Trim();

            if (string.IsNullOrEmpty(bankPathInfo))
            {
                //we don't have a bank path 
                //returnString += returnLine; we don't need this
            } else
            {
                //we DO have something in the bankpath
                returnString += "," + returnLine;
                bankPath = capFirst(bankPath);
                returnString += "                \"bankPath\": \"" + bankPathInfo + "\""; //used to have  + returnLine
            }
            //a line return is being added somewhere before bankPath!! Why!?!

            return returnString;
        }

        private string correctBankPathString(string originalInput)
        {
            //this takes a string (which will be whatever's in the bankPath field)
            //wait, people can't mess with the bank path. i don't need to do this.
            return "";
        }

        private string shaveSurroundingQuotesAndSpaces(string ogString, bool doQuotes = true, bool doSpaces = true)
        {
            //this function will take a string such as->{         "hello my good man "    }
            //and turn it into this.....................{hello my good man }

            string returnString = ogString;
            //if we have a " at the beginning, get rid of it

            if (ogString == "") return ogString;

            if (doSpaces)
            {
                returnString = ogString.Trim();
            }
            if (returnString == "") return returnString;
            if (doQuotes)
            {
                if (returnString.Substring(returnString.Length - 1, 1) == "\"")
                {
                    //if there's a " at the end of the string
                    returnString = returnString.Substring(0, returnString.Length - 1);
                }
                if (returnString.Substring(0, 1) == "\"")
                {
                    //if there's a " at the beginning of the string
                    returnString = returnString.Substring(1); //get rid of it
                }
                /* why did I do this?
                if (returnString.Substring(returnString.Length - 1, 1) == "\"")
                {
                    returnString = returnString.Substring(1);
                }*/
            }

            return returnString;
        }

        private string pathShortener(string originalPath, int maxCharacters)
        {
            string returnString = originalPath;

            if (originalPath.Length > maxCharacters)
            {
                //find the first /, which is likely going to look like R:/
                int indexOfFirstSlash = originalPath.IndexOf("\\");
                returnString = "";
                returnString += originalPath.Substring(0, indexOfFirstSlash + 1); //+1 gives us the slash too
                returnString += "...\\";
                string[] dirs = originalPath.Split('\\');
                //give last few directories
                int numberOfDirectoriesToShow = 4;
                ////testFindJson.Text += "Dirs Length:" + dirs.Length;

                //make sure none of the directories have ridiculous file names

                if (dirs.Length > numberOfDirectoriesToShow) {
                    for (int p = 0; p < numberOfDirectoriesToShow; p++)
                    {
                        if (dirs[dirs.Length - numberOfDirectoriesToShow + p].ToString().Length > 30)
                        {
                            dirs[dirs.Length - numberOfDirectoriesToShow + p] = dirs[dirs.Length - numberOfDirectoriesToShow + p].Substring(0, 27) + "...";
                        }

                        returnString += dirs[dirs.Length - numberOfDirectoriesToShow + p].ToString();
                        if (p < numberOfDirectoriesToShow - 1)
                        {
                            returnString += "\\";
                        }
                    }
                }

            }

            return returnString;
        }


        private void copyLevelInfo_Click(object sender, EventArgs e)
        {
            Button buttonCalled = sender as Button;
            CopyLevelInfoToClipboard(buttonCalled);
        }

        private void CopyLevelInfoToClipboard(Button b)
        {
            TextBox[] mainLevelTextBoxes = { MLNameBox, MLEventBox, MLLHBEBox, MLOffsetBox, MLBPMBox };
            TextBox[] bossFightTextBoxes = { BFNameBox, BFEventBox, BFLHBEBox, BFOffsetBox, BFBPMBox };


            string copyString = "";
            string separator = "\n"; //y'gotta keep 'em separated

            if (b.Name.Substring(0, 1).ToLower() == "m")
            {

                for (int i = 0; i < mainLevelTextBoxes.Length; i++)
                {
                    copyString += mainLevelTextBoxes[i].Text;
                    copyString += separator;
                }

                //if the bankPath length is greater than "bankPath: " (no quotes)
                if (mBankPathLabel.Text.Length > 10)
                {
                    //copyString += mBankPath.Text;
                    copyString += mTrueBankPath.Text;
                    copyString += separator;
                }
            } else if (b.Name.Substring(0, 1).ToLower() == "b")
            {
                for (int i = 0; i < bossFightTextBoxes.Length; i++)
                {
                    copyString += bossFightTextBoxes[i].Text;
                    copyString += separator;
                }
                if (bBankPathLabel.Text.Length > 10)
                {
                    //copyString += bBankPath.Text;
                    copyString += bTrueBankPath.Text;
                    copyString += separator;
                }
            }

            copyString += "Press the 'Paste to...' button on the right(→) to paste the info here"; //we want this in case user doesn't understand the point of the buttons
                                                                                                   //since our text box continues going right as we type, we need this message to be at the end to see it




            Clipboard.SetText(copyString);
        }


        private string[] cleanUpPastedInfo(string[] originalLevelInfoSplit)
        {
            /* If the user pasted something that looks like this:
             *  "Bank" : "Unstoppable_All",
                "Event": "{576553c8-4c42-49c8-8fc4-cbc9d0d4fe44}",
                "LowHealthBeatEvent": "{cd12c5c1-6d28-49c9-bcf5-567c9c3ae5bf}",
                "BeatInputOffset": 0.00,
                "BPM": 121
             *  Then we want to get rid of "Bank":, "Event":, etc.
             * */
            string[] labels = { "Bank", "Event", "LowHealthBeatEvent", "BeatInputOffset", "BPM", "bankPath" };

            string[] levelInfoSplit = originalLevelInfoSplit;
            for (int i = 0; i < levelInfoSplit.Length && i < labels.Length; i++)
            {
                levelInfoSplit[i] = levelInfoSplit[i].Replace("\"" + labels[i] + "\"", ""); //gets rid of the label and its surrounding quotes

                if (labels[i] == "bankPath")
                {
                    //we're supposed to have a colon for bankPath (ie: bankPath: M:/steam/common/etc./), but we don't want the one from "bankPath":
                    int numberOfColons = levelInfoSplit[i].Split(':').Length - 1;
                    if (numberOfColons > 1)
                    {
                        int indexOfFirstColon = levelInfoSplit[i].IndexOf(":");
                        levelInfoSplit[i] = levelInfoSplit[i].Substring(indexOfFirstColon + 1); //we erase everything before the first colon
                    }

                } else
                {
                    //just get rid of all colons
                    levelInfoSplit[i] = levelInfoSplit[i].Replace(":", ""); //gets rid of the colon that was after our label
                }



                if (i == 3 || i == 4)
                {
                    //if i is either 3 or 4, meaning we're at the line for Offset or BPM
                    levelInfoSplit[i] = levelInfoSplit[i].Replace(",", ""); //gets rid of a , if we had it at the end
                } else
                {
                    //if i is pointing at Bank, Event, LHEvent, or bankPath
                    levelInfoSplit[i] = levelInfoSplit[i].Replace("\",", ""); //gets rid of a , if we had it at the end. it's possible to have , in our filename, so we look for both->",
                }


                levelInfoSplit[i] = shaveSurroundingQuotesAndSpaces(levelInfoSplit[i]); //removes any spaces on the outside, then removes any quotes.
            }
            return levelInfoSplit;
        }

        private void pasteLevelInfo_Click(object sender, EventArgs e)
        {
            Button buttonCalled = sender as Button;
            pasteLevelInfoFromClipboard(buttonCalled);
        }

        private void pasteLevelInfoFromClipboard(Button b)
        {
            TextBox[] mainLevelTextBoxes = { MLNameBox, MLEventBox, MLLHBEBox, MLOffsetBox, MLBPMBox };
            TextBox[] bossFightTextBoxes = { BFNameBox, BFEventBox, BFLHBEBox, BFOffsetBox, BFBPMBox };
            string levelInfoFullText = Clipboard.GetText().ToString();
            string[] levelInfoSplit = levelInfoFullText.Split('|');

            if (levelInfoSplit.Length < 5)
            {
                //we didn't split anything. the user might have copied a bunch of lines from a JSON, though, so we'll check for that
                if (levelInfoFullText.Contains('\n'))
                {
                    levelInfoSplit = levelInfoFullText.Split('\n');


                    levelInfoSplit = cleanUpPastedInfo(levelInfoSplit); //if the user pasted something, this will get rid of "Bank":, "Event":, etc.


                }
            }

            if (levelInfoSplit.Length < 5)
            {
                //we still didn't split anything. the user might have copied everything on the same line for some reason, and we can still check for that
                if (levelInfoFullText.Contains(','))
                {
                    levelInfoSplit = levelInfoFullText.Split(',');

                    levelInfoSplit = cleanUpPastedInfo(levelInfoSplit); //if the user pasted something, this will get rid of "Bank":, "Event":, etc.


                }
            }

            if (levelInfoSplit.Length < 5) { MessageBox.Show("No level has its info copied to the clipboard.\n" +
                "Click 'Copy Main Level Info' or 'Copy Boss Fight Info' to properly copy its info, or" +
                "if copying from an external source, make sure you're copying all 5 lines of info."
                , "No Level Info Copied"); return; }

            if (b.Name.Substring(0, 1).ToLower() == "m")
            {
                for (int i = 0; i < mainLevelTextBoxes.Length; i++)
                {
                    mainLevelTextBoxes[i].Text = levelInfoSplit[i]; //levelInfoSplit should have 1 more than the mainLevel/bossFightTextBoxes, but that shouldn't matter

                }

                if (levelInfoSplit.Length < 7) { return; }//it will be 6 if we didn't have a bank path; it'll be 5 if the user copied and pasted something [without a bankpath]
                if (!levelInfoSplit[5].Contains(":\\")) { return; }//if they user somehow copied something that doesn't have, for example, C:/, R:/, D:/, then don't let it be pasted
                //if we got this far, we have something copied for our bankPath field
                mTrueBankPath.Text = shaveSurroundingQuotesAndSpaces(levelInfoSplit[5]);
                string shortPath = levelInfoSplit[5];
                shortPath = shortPath.Replace("\\\\", "\\");
                shortPath = shaveSurroundingQuotesAndSpaces(shortPath);
                shortPath = pathShortener(shortPath, 40);

                mBankPathLabel.Text = "bankPath: " + shortPath;

                string last5characters = mTrueBankPath.Text.Substring(mTrueBankPath.Text.Length - 5);//we can and probably should do mTrueBankPath...
                if (last5characters != ".bank") {
                    bankPathRedAlert(mBankPathLabel, 1);
                    ////testFindJson.Text += " XX " + last5characters + " X ";
                    return;
                }//if they user copied something that didn't have the .Bank file in it, alert them

                if (!verifyFileExists(mTrueBankPath.Text)) {
                    //could not verify the file exists
                    bankPathRedAlert(mBankPathLabel);
                } else
                {
                    warnUserIfBadBankPath(mBankPathLabel);
                }

                /*//seriously, this works...????
                if (levelInfoSplit[5].Length < "bankPath: ".Length) { return; }
                if(levelInfoSplit[5].Substring(0, 10) == "bankPath: ")
                {
                    mTrueBankPath.Text = levelInfoSplit[5];
                    string shortPath = pathShortener(levelInfoSplit[5], 40);
                    mBankPath.Text = shortPath;
                }*/
            } else if (b.Name.Substring(0, 1).ToLower() == "b")
            {
                for (int i = 0; i < bossFightTextBoxes.Length; i++)
                {
                    bossFightTextBoxes[i].Text = levelInfoSplit[i]; //levelInfoSplit should have 1 more than the mainLevel/bossFightTextBoxes, but that shouldn't matter

                }
                if (levelInfoSplit.Length < 7) { return; }//it will be 6 if we didn't have a bank path; it'll be 5 if the user copied and pasted something [without a bankpath]
                if (!levelInfoSplit[5].Contains(":\\")) { return; }//if they user somehow copied something that doesn't have, for example, C:/, R:/, D:/, then don't let it be pasted

                //if we got this far, we have something copied for our bankPath field
                bTrueBankPath.Text = shaveSurroundingQuotesAndSpaces(levelInfoSplit[5]);
                string shortPath = levelInfoSplit[5];
                shortPath = shortPath.Replace("\\\\", "\\");
                shortPath = shaveSurroundingQuotesAndSpaces(shortPath);
                shortPath = pathShortener(shortPath, 40);

                bBankPathLabel.Text = "bankPath: " + shortPath;

                string last5characters = bTrueBankPath.Text.Substring(bTrueBankPath.Text.Length - 5);//we can and probably should do mTrueBankPath...
                if (last5characters != ".bank")
                {
                    bankPathRedAlert(bBankPathLabel, 1);
                    return;
                }//if they user copied something that didn't have the .Bank file in it, alert them

                if (!verifyFileExists(bTrueBankPath.Text))
                {
                    //could not verify the file exists
                    bankPathRedAlert(bBankPathLabel);
                }
                else
                {
                    warnUserIfBadBankPath(bBankPathLabel);
                }

                /*
                if (levelInfoSplit[5].Length < "bankPath: ".Length) { return; }
                if (levelInfoSplit[5].Substring(0, 10) == "bankPath: ")
                {
                    bTrueBankPath.Text = levelInfoSplit[5];
                    string shortPath = pathShortener(levelInfoSplit[5], 40);
                    bBankPath.Text = shortPath;
                }*/
            }
        }


        /// <summary>
        /// Resets the Level buttons in Organizer to have standard background, enabling the button, or disabling it and clearing fields if parameter is false
        /// </summary>
        /// <param name="enableButtons"></param>
        private void organizer_enableLevelButtons(bool enableButtons = true)
        {
            //this resets the colors of the level buttons in Organizer; resetting their colors and making them Disabled (before a level is selected)
            Button[] LevelButtons = { L1Settings, L2Settings, L3Settings, L4Settings, L5Settings, L6Settings, L7Settings, L8Settings, L0Settings };

            if (!enableButtons) goto resetLButtons;

            for (int i = 0; i < LevelButtons.Length; i++)
            {
                LevelButtons[i].BackColor = Color.Transparent;
                LevelButtons[i].ForeColor = Color.Black;
                LevelButtons[i].Enabled = true;
            }

            return;
        //if we didn't want to reset the buttons, we're done

        resetLButtons:
            TextBox[] allOrganizerTextboxes = { MLNameBox, MLEventBox, MLLHBEBox, MLOffsetBox, MLBPMBox, BFNameBox, BFEventBox, BFLHBEBox, BFOffsetBox, BFBPMBox };

            for (int i = 0; i < LevelButtons.Length; i++)
            {
                LevelButtons[i].BackColor = Color.Transparent;
                LevelButtons[i].ForeColor = Color.Black;
                LevelButtons[i].Enabled = false;
            }
            foreach (TextBox tbox in allOrganizerTextboxes)
            {
                tbox.Enabled = false;
            }

        }

        //nothing uses this function
        private string[] getSongName(string fullModJson)
        {
            int indexOfLevelInfo = fullModJson.IndexOf("\"LevelName\""); //appears as, for example, "LevelName":"Voke"
            //we will take all info directly after this


            int indexOfSongName = fullModJson.IndexOf("\"Bank\":", indexOfLevelInfo); //this is where "Bank": starts, so we still need to add the 7 characters (8 with the quotes)
            indexOfSongName += 8; //now the index is right after the song name's (file name's) first quote (")
            int indexOfSongNameEnd = fullModJson.IndexOf("\"", indexOfSongName); //this takes us to the space right before song name's (file name's) last quote (")
            int lengthOfSongName = indexOfSongNameEnd - indexOfSongName;
            string songName = fullModJson.Substring(indexOfSongName, indexOfSongNameEnd - indexOfSongName); //this should grab .... "Bank":"EVERYTHINGINHERE"
            ////testFindJson.Text = "IndexOfSongName: " + indexOfSongName + "; IndexOfSongNameEnd: " + indexOfSongNameEnd + "; Song name: " + songName;

            if (indexOfLevelInfo != -1)
            {
                MLNameBox.Text = songName;
            }

            string[] bunkSongInfo = { "hello", "world" };
            return bunkSongInfo;
        }


        string[] currentSetListName_m = new string[9];
        string[] currentSetListName_b = new string[7];

        int[] currentSetListIndexes_main = { -1, -1, -1, -1, -1, -1, -1, -1, -1 }; //-1 if default song; otherwise, this holds the index # of the Mod in the set list
        int[] currentSetListIndexes_boss = { -1, -1, -1, -1, -1, -1, -1 }; //same as ^^

        private bool inputtedLevelMatchesOld(int changedCBox, string m_or_b, int changedSelectionIndex)
        {
            //this is ran after we've changed the selection of a Level's main or boss Combo box
            //we're trying to see if the field we just changed matches what we already have (already in the JSON)
            //this is to help the user create more variety in what they've already been using



            if (m_or_b == "m")
            {
                ////testFindJson.Text += "Old index: " + currentSetListIndexes_main[changedCBox];
                if (changedSelectionIndex == currentSetListIndexes_main[changedCBox])
                {
                    return true;
                }
            } else if (m_or_b == "b")
            {
                ////testFindJson.Text += "Old index: " + currentSetListIndexes_boss[changedCBox];
                if (changedSelectionIndex == currentSetListIndexes_boss[changedCBox])
                {
                    return true;
                }
            }

            return false;

        }

        private bool autoSelectOn()
        {
            return tsm_AllowAutoSelect.Checked;
            /*
            if (autoSelectGrabLvl.Checked)
            {
                return true;
            } else
            {
                return false;
            }*/
        }

        //gives us 2 ints: the first tells us which level we want now; the 2nd tells us if we had to change off of main or boss (0 means we didn't have to, 1 means we changed)
        private int[] getNextBestChoice(int modIndex, int whichLevel, string m_or_b)
        {
            //this function will give us the 0-based Level number of the next best option for the level we want, as apparently it's unavailable
            string supportedLvlString = csSupLvls[modIndex]; //gives us the supported levels string of the mod that's selected
            //int levelInfoIndex = supportedLvlString.IndexOf(whichLevel.ToString());//this is going to give us the spot right before the level number. After the number is m, b, mb, or (nothing, next number)
            //we don't use this

            ////testFindJson.Text += "Support for Mod " + modIndex + ": " + supportedLvlString + "...";

            int[] returnBunk = { -10, -10 };
            int[] returnBunk1 = { -7, -7 };
            int[] returnBunk2 = { -2, -2 };
            int[] returnBunk3 = { -3, -3 };
            int[] returnBunk4 = { -4, -4 };
            int[] returnBunk5 = { -5, -5 };
            int[] bunkAngry = { -11, -11 };

            if (m_or_b == "m")
            {


                int numberOfSupportedLevels_m = 0;
                numberOfSupportedLevels_m = supportedLvlString.Count(f => f == 'm'); //gives us the number of times we see an "m"
                if (numberOfSupportedLevels_m == 0)
                {
                    //this mod doesn't support any main levels, go to boss checker
                    goto BossFightSupportCheck;
                }
                if (numberOfSupportedLevels_m == 1)
                {
                    //we already have our answer, the only level the mod supports
                    int indexOfOnlyM = supportedLvlString.IndexOf("m"); //this takes us right after the level number, so...
                    indexOfOnlyM -= 1;
                    string onlySupportedLevel = supportedLvlString.Substring(indexOfOnlyM, 1); //this gives us a string of the level num
                    int onlySupportedLvl = Int32.Parse(onlySupportedLevel);
                    int[] returnStrings = { onlySupportedLvl, 0 };
                    return returnStrings;
                }

                //if we got this far, we have more than one option for "m"
                List<int> supportedLevels = new List<int>();

                //because there's 8 main levels
                for (int i = 0; i < 9; i++)
                {
                    int[] ourNextBestOptions = mainlevelDefaultSlcts[whichLevel];
                    int zeroBasedLevel = ourNextBestOptions[i];
                    zeroBasedLevel -= 1;
                    int indexOfConsideredLevel = supportedLvlString.IndexOf(zeroBasedLevel.ToString());
                    if (indexOfConsideredLevel == supportedLvlString.Length - 1) continue; //we're at the last, which doesn't support anything
                    if (supportedLvlString.Substring(indexOfConsideredLevel + 1, 1) == "m")
                    {
                        int[] returnStrings = { zeroBasedLevel, 0 };
                        return returnStrings;

                    }
                }

                //we shouldn't ever get this far

                return returnBunk;


            BossFightSupportCheck:
                //if we didn't have any main levels, we skipped to here
                //we're still checking to replace a MAIN song

                int numberOfSupportedLevels_b = 0;
                numberOfSupportedLevels_b = supportedLvlString.Count(f => f == 'b'); //gives us the number of times we see a "b"
                if (numberOfSupportedLevels_b == 0)
                {
                    //this mod doesn't support any main levels or boss fights, set everything on fire
                    int[] returnStrings = { -1, -1 };
                    return returnStrings;
                }
                if (numberOfSupportedLevels_b == 1)
                {
                    //we already have our answer, the only level the mod supports
                    int indexOfOnlyB = supportedLvlString.IndexOf("b"); //this takes us right after the level number, so...
                    indexOfOnlyB -= 1;
                    string onlySupportedLevel = supportedLvlString.Substring(indexOfOnlyB, 1); //this gives us a string of the level num
                    if (onlySupportedLevel == "m")
                    {
                        indexOfOnlyB -= 1;
                        onlySupportedLevel = supportedLvlString.Substring(indexOfOnlyB, 1); //this gives us a string of the level num
                    }
                    int onlySupportedLvl = Int32.Parse(onlySupportedLevel);
                    int[] returnStrings = { onlySupportedLvl, 1 };
                    return returnStrings;

                }

                //if we got this far, we have more than one option for "b"

                //we're still checking to replace a MAIN song

                //because there's 7 boss levels

                for (int i = 0; i < 7; i++)
                {

                    int[] ourNextBestOptions = mainlevelBOSSDefaultSlcts[whichLevel];
                    int zeroBasedLevel = ourNextBestOptions[i];
                    zeroBasedLevel -= 1;
                    int indexOfConsideredLevel = supportedLvlString.IndexOf(zeroBasedLevel.ToString());
                    if (indexOfConsideredLevel == supportedLvlString.Length - 1) continue; //we're at the last and it doesn't support anything
                    if (supportedLvlString.Substring(indexOfConsideredLevel + 1, 1) == "b")
                    {
                        int[] returnStrings = { zeroBasedLevel, 1 };
                        return returnStrings;
                    }
                    //we didn't get a b from the last call, but it could still be right after that (it doesn't look like 4b, but it could look like 4mb)
                    if (indexOfConsideredLevel >= supportedLvlString.Length - 2)
                    { ////testFindJson.Text += "no support on lvl" + zeroBasedLevel; 
                        continue;
                    }//supportedLvlString.Length - 2 would be here -> 12345b6mb7*mb 12345b6m*b7 (it can't be 12345b6mb*7b, or the last 'if' would have seen it)
                    //^this ensures we're not hitting looking for the last level while it has no support
                    //4m5m6
                    string checkForNumStr = supportedLvlString.Substring(indexOfConsideredLevel + 1, 1);
                    if (Int32.TryParse(checkForNumStr, out int j))
                    {
                        //this should activate if our character after Level number was another number; meaning the Mod doesn't have info for either main music or boss on this level, ie. the 1 in 0mb12m3mb
                        continue;
                    }
                    else if (supportedLvlString.Substring(indexOfConsideredLevel + 2, 1) == "b")
                    {
                        int[] returnStrings = { zeroBasedLevel, 0 };
                        return returnStrings;
                    }
                }

                //we shouldn't ever get this far

                return returnBunk1;

            } else if (m_or_b == "b")
            {
                int numberOfSupportedLevels_b = 0;
                numberOfSupportedLevels_b = supportedLvlString.Count(f => f == 'b'); //gives us the number of times we see a "b"
                if (numberOfSupportedLevels_b == 0)
                {
                    //this mod doesn't support any main levels, go to main checker
                    goto MainLevelSupportCheck;
                }
                if (numberOfSupportedLevels_b == 1)
                {
                    //we already have our answer, the only level the mod supports
                    int indexOfOnlyB = supportedLvlString.IndexOf("b"); //this takes us right after the level number, so...
                    indexOfOnlyB -= 1;
                    string onlySupportedLevel = supportedLvlString.Substring(indexOfOnlyB, 1); //this gives us a string of the level num
                    if (onlySupportedLevel == "m")
                    {
                        indexOfOnlyB -= 1;
                        onlySupportedLevel = supportedLvlString.Substring(indexOfOnlyB, 1); //NOW we should have the level num
                    }
                    int onlySupportedLvl = Int32.Parse(onlySupportedLevel);
                    ////testFindJson.Text += "onlyFoundOn(" + onlySupportedLvl + ") ";
                    int[] returnStrings = { onlySupportedLvl, 0 };
                    return returnStrings;
                }

                //if we got this far, we have more than one option for "b"


                //because there's 7 boss levels
                for (int i = 0; i < 7; i++)
                {
                    int[] ourNextBestOptions = bossfightDefaultSlcts[whichLevel];
                    ////testFindJson.Text += "..NBO: " + ourNextBestOptions[0] + ourNextBestOptions[1] + ourNextBestOptions[2] + ourNextBestOptions[3] + ourNextBestOptions[4] + ourNextBestOptions[5] + ourNextBestOptions[6];
                    int zeroBasedLevel = ourNextBestOptions[i];
                    zeroBasedLevel -= 1;
                    ////testFindJson.Text += "testingLvlA(" + zeroBasedLevel + ")->";
                    int indexOfConsideredLevel = supportedLvlString.IndexOf(zeroBasedLevel.ToString());
                    if (indexOfConsideredLevel == supportedLvlString.Length - 1) continue; //we're at the last, which doesn't support anything
                    if (supportedLvlString.Substring(indexOfConsideredLevel + 1, 1) == "b")
                    {
                        int[] returnStrings = { zeroBasedLevel, 0 };
                        ////testFindJson.Text += "found on levelA(" + allLevelNames[zeroBasedLevel] + ", supportString: " + supportedLvlString + ")";
                        return returnStrings;
                    }
                    //we didn't get a b from the last call, but it could still be right after that (it doesn't look like 4b, but it could look like 4mb)
                    if (indexOfConsideredLevel >= supportedLvlString.Length - 2) continue; //supportedLvlString.Length - 2 would be here -> 12345b6mb7*mb 12345b6m*b7 (it can't be 12345b6mb*7b, or the last 'if' would have seen it)
                    //^this ensures we're not hitting looking for the last level while it has no support
                    string checkForNumStr = supportedLvlString.Substring(indexOfConsideredLevel + 1, 1);
                    if (Int32.TryParse(checkForNumStr, out int j))
                    {
                        //this should activate if our character after Level number was another number; meaning the Mod doesn't have info for either main music or boss on this level, ie. the 1 in 0mb12m3mb
                        continue;
                    } else if (supportedLvlString.Substring(indexOfConsideredLevel + 2, 1) == "b")
                    {
                        int[] returnStrings = { zeroBasedLevel, 0 };
                        ////testFindJson.Text += "found on levelB(" + allLevelNames[zeroBasedLevel] + ")";
                        return returnStrings;
                    }
                }

                //we shouldn't ever get this far
                return returnBunk2;

            MainLevelSupportCheck:
                //mostly copy and paste, but looking at bossfightMAINDefaultSlcts instead

                int numberOfSupportedLevels_m = 0;
                numberOfSupportedLevels_m = supportedLvlString.Count(f => f == 'm'); //gives us the number of times we see an "m"
                if (numberOfSupportedLevels_m == 0)
                {
                    //this mod doesn't support any main levels, or boss levels
                    //HOW!?!
                    return returnBunk3;
                }
                if (numberOfSupportedLevels_m == 1)
                {
                    //we already have our answer, the only level the mod supports
                    int indexOfOnlyM = supportedLvlString.IndexOf("m"); //this takes us right after the level number, so...
                    indexOfOnlyM -= 1;
                    string onlySupportedLevel = supportedLvlString.Substring(indexOfOnlyM, 1); //this gives us a string of the level num
                    int onlySupportedLvl = Int32.Parse(onlySupportedLevel);
                    int[] returnStrings = { onlySupportedLvl, 1 };
                    return returnStrings;

                }

                //if we got this far, we have more than one option for "m"
                List<int> supportedLevels = new List<int>();

                //because there's 8 main levels
                for (int i = 0; i < 8; i++)
                {
                    int[] ourNextBestOptions = bossfightMAINDefaultSlcts[whichLevel];
                    int zeroBasedLevel = ourNextBestOptions[i];
                    zeroBasedLevel -= 1;
                    int indexOfConsideredLevel = supportedLvlString.IndexOf(zeroBasedLevel.ToString());
                    if (indexOfConsideredLevel == supportedLvlString.Length - 1) continue; //we're at the last and it doesn't support anything
                    if (supportedLvlString.Substring(indexOfConsideredLevel + 1, 1) == "m")
                    {
                        int[] returnStrings = { zeroBasedLevel, 1 };
                        ////testFindJson.Text = "found on levelC(" + zeroBasedLevel + ")";
                        return returnStrings;

                    }
                }

                //we shouldn't ever get this far
                return returnBunk4;

            }
            return returnBunk5;


        }


        //these are our default selections when our mod is looking for where we're taking song info from (via its JSON file)
        int[][] mainlevelDefaultSlcts =
        {
            new int[] {1, 3, 6, 7, 2, 4, 5, 8, 9},
            new int[] {2, 4, 5, 8, 1, 3, 6, 7, 9},
            new int[] {3, 1, 6, 7, 2, 4, 5, 8, 9},
            new int[] {4, 2, 5, 8, 1, 3, 6, 7, 9},
            new int[] {5, 2, 4, 8, 1, 3, 6, 7, 9},
            new int[] {6, 7, 3, 1, 2, 4, 5, 8, 9},
            new int[] {7, 6, 3, 1, 2, 4, 5, 8, 9},
            new int[] {8, 2, 4, 5, 1, 3, 6, 7, 9},
            new int[] {9, 2, 4, 5, 8, 1, 3, 6, 7}
        };
        //this next set of arrays are the BOSS levels to grab FOR OUR MAIN if we cannot find anything from our mains
        int[][] mainlevelBOSSDefaultSlcts =
        {
            new int[] {1, 2, 3, 4, 5, 6, 7, 9},
            new int[] {2, 3, 4, 5, 6, 7, 1, 9},
            new int[] {1, 3, 2, 4, 5, 6, 7, 9},
            new int[] {4, 2, 3, 5, 6, 7, 1, 9},
            new int[] {5, 2, 3, 4, 6, 7, 1, 9},
            new int[] {1, 5, 2, 3, 4, 6, 7, 9},
            new int[] {1, 6, 2, 3, 4, 5, 7, 9},
            new int[] {7, 2, 3, 4, 5, 6, 1, 9},
            new int[] {2, 3, 4, 5, 6, 7, 8, 1}
    };
        int[][] bossfightDefaultSlcts =
        {
            new int[] {1, 2, 3, 4, 5, 6, 7, 9},
            new int[] {2, 3, 4, 5, 6, 7, 1, 9},
            new int[] {3, 2, 4, 5, 6, 7, 1, 9},
            new int[] {4, 2, 3, 5, 6, 7, 1, 9},
            new int[] {5, 2, 3, 4, 6, 7, 1, 9},
            new int[] {6, 2, 3, 4, 5, 7, 1, 9},
            new int[] {7, 2, 3, 4, 5, 6, 1, 9},
        };
        //this next set of arrays are the MAIN levels to grab for our BOSS fights if we could not find anything from our bosses
        int[][] bossfightMAINDefaultSlcts =
        {
            new int[] {3, 1, 6, 7, 2, 4, 5, 8, 9},
            new int[] {2, 4, 5, 8, 1, 3, 6, 7, 9},
            new int[] {2, 4, 5, 8, 1, 3, 6, 7, 9},
            new int[] {4, 2, 5, 8, 1, 3, 6, 7, 9},
            new int[] {5, 2, 4, 8, 1, 3, 6, 7, 9},
            new int[] {2, 4, 5, 8, 1, 3, 6, 7, 9},
            new int[] {2, 4, 5, 8, 1, 3, 6, 7, 9},
        };

        int[] tutorialMainDefaultSelects = { 9, 2, 4, 5, 8, 1, 3, 6, 7 };
        int[] tutorialBossDefaultSelects = { 9, 2, 3, 4, 5, 6, 7, 8, 1 };


        string[] LvlAbbreviations = { "V", "St", "Y", "I", "G", "N", "A", "Sh", "T" };


        private void setModGrabLvlSelection(ComboBox changedBox, string howItWasCalled = "")
        {
            //this is called by other functions when we select a custom song, or change the text in the Level's main or boss Combo Box
            //this function looks at what Level we just changed the song for. IE: Putting Du Hast on Stygia
            // we should be looking for Stygia first. if it's not available, we look for other similar levels.
            //whatever level we're changing the music for, needs to be the first choice of the default selection
            //if that level isn't available, we need to go through a list of other similar levels, and pick that

            //this also enables the button (or disables)

            //the perameter will be the combo box we just changed, either mainCombo1 (thru 8) or bossCombo1 (thru 8)


            //first we want to see if the song we just touched was actually changed from a second ago
            /*
            if (!textChanged)
            {
           
                return;

            }*/

            Image bImg = radioButton3.Image;
            Image mImg = radioButton5.Image;


            string musicBeingChanged = changedBox.Name;

            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton, ML9ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };

            string boxCalledNumStr = musicBeingChanged.Substring(musicBeingChanged.Length - 1, 1);
            int whichLvl = Int32.Parse(boxCalledNumStr);
            whichLvl -= 1; //since named our buttons 1-8, instead of 0-7; but our array is indexed at 0-7

            if (musicBeingChanged.Substring(0, 4) == "main")
            {
                //need to match all of this for the boss

                mainLvlGrabButton[whichLvl].Font = radioButton3.Font; //we're taking away the bold

                ////testFindJson.Text += "WhichLvl: " + whichLvl.ToString() + "; m; " + "SelectedIndex: " + changedBox.SelectedIndex.ToString() + "; ";

                //first we want to see if the song we selected is already what's in the JSON
                /* WTF is this..?
                 * there's somewhere else that we do this
                if (inputtedLevelMatchesOld(whichLvl, "m", changedBox.SelectedIndex))
                {
                    //Our song is what we already have in the JSON
                    //we just want to change the checkbox to be unchecked, and the box to be blanked out, but not disabled
                    return;
                }*/

                int changedBoxIndex = setListCatalog.FindStringExact(changedBox.Text); //because changedBox.SelectedIndex doesn't give me an updated value for typed info
                //NOW THIS ISN'T WORKING. I SUCK AT PROGRAMMING
                //int changedBoxIndex = changedBox.SelectedIndex;
                ////testFindJson.Text += "Finding index for " + changedBox.Text;



                if (howItWasCalled == "select")
                {
                    changedBoxIndex = setListCatalog.SelectedIndex;
                }

                if (changedBoxIndex == -1)
                {
                    //whatever we changed the song to is not a custom song in our list
                    //this will never run via setGrabLvlButton, but this still runs during changeDefaultGrabLvlText, which runs by default when we've changed our selection for a Combo Box
                    //setGrabLvlButton runs when we hit enter. I'm pretty sure I need to be combining these functions

                    //the selected index can be -1 if it's blank (where we want default song)....
                    //wait....
                    //this only runs when we actually selected something, it shouldn't go if we typed something that doesn't exist on our list
                    //I don't think this code is ever going to run....?
                    // '-> Correction! This code DOES run now, whenever we hit the check box and the default song is in here

                    mainLvlGrabButton[whichLvl].Text = "";
                    mainLvlGrabButton[whichLvl].Enabled = false;
                    mainLvlGrabButton[whichLvl].Image = null;
                    return;


                }

                //next we want to see if the level we're changing is supported in the Mod; if it is, enabled the button and change the text
                if (modSupportsLevel(changedBoxIndex, whichLvl, "m"))
                {
                    /*
                    if (!wasComboBoxChanged(getComboFromGrabLvlBtn(mainLvlGrabButton[whichLvl]))) {
                        return;
                    }*/
                    ////testFindJson.Text += "SUPPORTED! ";
                    string hi = mainLvlGrabButton[whichLvl].Text;
                    ////testFindJson.Text += "\n!sMGLS: Before, box said: " + hi;
                    mainLvlGrabButton[whichLvl].Text = LvlAbbreviations[whichLvl];
                    ////testFindJson.Text += ", but now it says: " + mainLvlGrabButton[whichLvl].Text;
                    mainLvlGrabButton[whichLvl].Enabled = true;
                    mainLvlGrabButton[whichLvl].Image = null;
                } else
                {
                    mainLvlGrabButton[whichLvl].Image = null;
                    mainLvlGrabButton[whichLvl].Font = radioButton3.Font;
                    if (autoSelectOn())
                    {
                        int[] autoSelectInfo = getNextBestChoice(changedBoxIndex, whichLvl, "m");

                        mainLvlGrabButton[whichLvl].Text = LvlAbbreviations[autoSelectInfo[0]];
                        if (autoSelectInfo[1] == 1)
                        {
                            //1 means we had to choose the alternate. we were inquring about M, so alternative is B
                            mainLvlGrabButton[whichLvl].Image = bImg;
                        }
                    }
                    else
                    {
                        mainLvlGrabButton[whichLvl].Text = "!";
                        mainLvlGrabButton[whichLvl].Font = boldRadio.Font;
                    }
                    mainLvlGrabButton[whichLvl].Enabled = true;

                }

            }
            else if (musicBeingChanged.Substring(0, 4) == "boss")
            {
                bossLvlGrabButton[whichLvl].Font = radioButton3.Font; //we're taking away the bold. I'm a bad programmer

                int changedBoxIndex = setListCatalog.FindStringExact(changedBox.Text); //because changedBox.SelectedIndex doesn't give me an updated value

                if (changedBoxIndex == -1)
                {
                    // This code runs now whenever we hit the check box and the default song is in here

                    bossLvlGrabButton[whichLvl].Text = "";
                    bossLvlGrabButton[whichLvl].Enabled = false;
                    bossLvlGrabButton[whichLvl].Image = null;
                    return;

                }

                //next we want to see if the level we're changing is supported in the Mod; if it is, enabled the button and change the text
                if (modSupportsLevel(changedBoxIndex, whichLvl, "b"))
                {
                    ////testFindJson.Text += "SUPPORTED! ";
                    string hi = mainLvlGrabButton[whichLvl].Text;
                    ////testFindJson.Text += "\n!sMGLS: Before, box said: " + hi;
                    bossLvlGrabButton[whichLvl].Text = LvlAbbreviations[whichLvl];
                    ////testFindJson.Text += ", but now it says: " + mainLvlGrabButton[whichLvl].Text;
                    bossLvlGrabButton[whichLvl].Enabled = true;
                    bossLvlGrabButton[whichLvl].Image = null;
                }
                else
                {
                    bossLvlGrabButton[whichLvl].Image = null;
                    bossLvlGrabButton[whichLvl].Font = radioButton3.Font;
                    if (autoSelectOn())
                    {
                        int[] autoSelectInfo = getNextBestChoice(changedBoxIndex, whichLvl, "b");
                        bossLvlGrabButton[whichLvl].Text = LvlAbbreviations[autoSelectInfo[0]];
                        if (autoSelectInfo[1] == 1)
                        {
                            //1 means we had to choose the alternate. we were inquring about B, so alternative is M
                            bossLvlGrabButton[whichLvl].Image = mImg;
                        }
                    }
                    else
                    {
                        bossLvlGrabButton[whichLvl].Text = "!";
                        bossLvlGrabButton[whichLvl].Font = boldRadio.Font;
                    }
                    bossLvlGrabButton[whichLvl].Enabled = true;

                }
            }
        }

        private void changeMusicCheckBox_check(object sender, EventArgs e)
        {
            ////testFindJson.Text += "Sender: " + sender;
            CheckBox chkB = sender as CheckBox;
            setSelectionFromCheck(chkB);
        }

        private void setSelectionFromCheck(CheckBox chkBox)
        {
            //this changes our selection and GrabLvlButton based on what we just did to our checkbox
            //this assumes we just clicked or changed a checkbox

            if (chkBox == null) return;

            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8, mainCombo9 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };

            string lvlNumStr = chkBox.Name.Substring(chkBox.Name.Length - 1, 1); //checkboxes look like checkm1, checkb1
            int lvlNum = Int32.Parse(lvlNumStr);
            lvlNum -= 1; //this drove me insane for a long time
            string id = chkBox.Name.Substring(chkBox.Name.Length - 2, 1);

            if (chkBox.Checked == true)
            {
                //we checked the box, we want to change the level
                if (id == "m")
                {
                    setModGrabLvlSelection(mainCBox[lvlNum]);

                    int modIndex = mainCBox[lvlNum].FindStringExact(mainCBox[lvlNum].Text);
                    if (modIndex == -1 && !mainCBox[lvlNum].Text.Contains("(Default)"))
                    {
                        //if we just checked a checkbox and its ComboBox has something we don't recognize, we don't want it in the box anymore
                        mainCBox[lvlNum].Text = "";
                    }

                    //setSongSelectionArray(mainCBox[lvlNum]);
                    ////testFindJson.Text += ".ssFC.";
                } else if (id == "b")
                {
                    setModGrabLvlSelection(bossCBox[lvlNum]);

                    int modIndex = bossCBox[lvlNum].FindStringExact(bossCBox[lvlNum].Text);
                    if (modIndex == -1 && !bossCBox[lvlNum].Text.Contains("(Default)"))
                    {
                        //if we just checked a box and it had something we don't recognize, we don't want it in the box anymore
                        bossCBox[lvlNum].Text = "";
                    }

                    //setSongSelectionArray(bossCBox[lvlNum]);
                    ////testFindJson.Text += ".ssFC.";
                }

            } else
            {

                //we unchecked the box, we want to keep our level info the same as whatever's in the current game's JSON
                if (id == "m")
                {
                    mainCBox[lvlNum].Text = currentSetListName_m[lvlNum];
                    if (currentSetListIndexes_main[lvlNum] > -1)
                    {
                        //whatever our song was previously, it was a custom song
                        enableGrabLvlButton(mainCBox[lvlNum], " ");
                    } else
                    {
                        //whatever our song was previously was either the default, or something we don't understand
                        enableGrabLvlButton(mainCBox[lvlNum], ""); //this will disable the button
                    }


                    ////testFindJson.Text += "Boxtext is now: " + mainCBox[lvlNum].Text + " and our old set list was " + currentSetListName_m[lvlNum];
                } else if (id == "b")
                {
                    bossCBox[lvlNum].Text = currentSetListName_b[lvlNum];

                    if (currentSetListIndexes_boss[lvlNum] > -1)
                    {
                        //our song was previously a custom song
                        enableGrabLvlButton(bossCBox[lvlNum], " ");

                    }
                    else
                    {
                        //whatever our song was previously was either the default, or something we don't understand
                        enableGrabLvlButton(bossCBox[lvlNum], ""); //this will disable the button
                    }

                    ////testFindJson.Text += " b_ " + currentSetListName_b[lvlNum];
                }

            }
        }


        //this will still run after we reset our selection by clicking the checkbox
        /// <summary>
        /// Looks at whatever Combo box was changed, and checks its box if it's a song that wasn't in the JSON, or unchecks it if it was
        /// </summary>
        /// <param name="cBox">The Combo box that got changed</param>
        private void setCheckFromSelection(ComboBox cBox)
        {
            //This is just meant to look at our combo box, and verify if we're changing a song
            //if we change it to song that was already in our game's current Json file, we don't want a check, UNTIL the player sets a Level
            //otherwise, if we're changing to a custom song that WASN'T in our game's current Json, we want a checkmark
            //if we change it to a default song, we need to verify our game's current Json file DIDN'T have anything there. if it did, put a check because we're changing it
            //cBox should be a song selection box that just got changed, so this function is meant to react to that decision

            //we need to see what our comboBox just set its selection to
            string comboSlctn = cBox.Text;
            int selectedSong = setListCatalog.FindStringExact(comboSlctn); // = cBox.SelectedIndex; << this was when we wouldn't display songs with errors in Catalog; now we use modIndex

            //find out where this was sent from, and compare the old values
            string boxType = cBox.Name.Substring(0, 1);
            string boxIDNumStr = cBox.Name.Substring(cBox.Name.Length - 1, 1);
            int boxIDNum = Int32.Parse(boxIDNumStr);
            boxIDNum -= 1; //we want 0-based index from our 1-based comboBox#'s

            CheckBox[] mainCheckBoxes = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8, checkm9 };
            CheckBox[] bossCheckBoxes = { checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };

            /*
            if(boxType == "m")
            {
                string comboSlctn = cBox.Text;
                selectedSong = setListCatalog.FindStringExact(comboSlctn);
            } else if (boxType == "b")
            {
                string comboSlctn = cBox.Text;
                selectedSong = setListCatalog.FindStringExact(comboSlctn);
            }*/

            //tabPage1.Focus(); //<-------------------- remove this. With it, it's at 
            //least recognizing to set the button level and checkbox, but it's not setting the message

            if (selectedSong > -1)
            {
                //MessageBox.Show("CurrntIndx: " + currentSetListIndexes_main[boxIDNum] + "\nSelectedSong: " + selectedSong);
                //we have a custom song selected; need to make sure it wasn't there already
                if (boxType == "m")
                {
                    //compare to mains
                    if (currentSetListIndexes_main[boxIDNum] == selectedSong)
                    {

                        //our selected song matches our current JSON's song; don't do a check, and make the GrabLvl button text say " "
                        mainCheckBoxes[boxIDNum].Checked = false;
                        enableGrabLvlButton(cBox, " ");
                        string levelNameCapd = allLevelNames[boxIDNum].Substring(0, 1).ToUpper() + allLevelNames[boxIDNum].Substring(1);
                        if (cBox.Focused)
                        {
                            //this command was called by the combo box, not the check box
                            SetList_DebugLabel1.Text = "Your selection for " + levelNameCapd + "'s Main Music matches the old Set List's song; we assume you want to keep old info.";
                            SetList_DebugLabel2.Text = "Check the box on the left or click the box on the right and choose a level to pull new info from the mod.";
                            SetList_DebugLabel3.Text = "";
                            ////testFindJson.Text += "BoxID: " + boxIDNum + "; " + currentSetListIndexes_main[boxIDNum] + "==" + selectedSong;
                            SetList_DebugLabel1.Visible = true;
                            SetList_DebugLabel2.Visible = true;
                        }
                    } else
                    {
                        //this is a new song we're selecting
                        //do checkmark

                        mainCheckBoxes[boxIDNum].Checked = true;

                        //also reset these
                        SetList_DebugLabel1.Text = "";
                        SetList_DebugLabel2.Text = "";
                        SetList_DebugLabel3.Text = "";

                    }
                } else if (boxType == "b")
                {
                    //compare to bosses
                    if (currentSetListIndexes_boss[boxIDNum] == selectedSong)
                    {
                        //our selected song matches our current JSON's song; don't do a check, and make the GrabLvl button text say " "
                        bossCheckBoxes[boxIDNum].Checked = false;
                        enableGrabLvlButton(cBox, " ");
                        string levelNameCapd = allLevelNames[boxIDNum].Substring(0, 1).ToUpper() + allLevelNames[boxIDNum].Substring(1);
                        if (cBox.Focused)
                        {
                            //this command was called by the combo box, not the check box

                            SetList_DebugLabel1.Text = "Your selection for " + levelNameCapd + "'s Boss Music matches the old Set List's song; we assume you want to keep old info.";
                            SetList_DebugLabel2.Text = "Check the box on the left or click the box on the right and choose a level to pull new info from the mod.";
                            ////testFindJson.Text += "BoxID: " + boxIDNum + "; " + currentSetListIndexes_boss[boxIDNum] + "==" + selectedSong;
                            SetList_DebugLabel1.Visible = true;
                            SetList_DebugLabel2.Visible = true;
                        }
                    }
                    else
                    {
                        //this is a new song we're selecting
                        //do checkmark
                        bossCheckBoxes[boxIDNum].Checked = true;

                        SetList_DebugLabel1.Text = "";
                        SetList_DebugLabel2.Text = "";
                        SetList_DebugLabel3.Text = "";
                    }
                }


            } else
            {
                //we do not have a custom song selected; it's either default, or something goofy
                if (cBox.Text == getDefaultSong(boxIDNum, boxType))
                {
                    //we chose the level's default song
                    if (boxType == "m")
                    {
                        if (currentSetListIndexes_main[boxIDNum] == selectedSong)
                        {
                            //our current JSON also had the default level song; don't do a check
                            mainCheckBoxes[boxIDNum].Checked = false;
                        }
                        else
                        {
                            //we're setting our song to default, and our old JSON actually had something
                            //do checkmark
                            mainCheckBoxes[boxIDNum].Checked = true;
                        }
                    } else if (boxType == "b")
                    {
                        if (currentSetListIndexes_boss[boxIDNum] == selectedSong)
                        {
                            //our current JSON also had the default level song; don't do a check
                            bossCheckBoxes[boxIDNum].Checked = false;
                        }
                        else
                        {
                            //we're setting our song to default, and our old JSON actually had something
                            //that's a check
                            bossCheckBoxes[boxIDNum].Checked = true;
                        }
                    }
                } else
                {
                    //whatever's in the combo box is something we don't recognize. see if it matches with the name of the original entry in our current JSON
                    if (boxType == "m")
                    {
                        if (currentSetListIndexes_main[boxIDNum] == -2 && currentSetListName_m[boxIDNum] == cBox.Text)
                        {
                            //our current JSON matches with whatever the hell was typed in, don't do a check; we can still have the "?" in the box if we want
                            mainCheckBoxes[boxIDNum].Checked = false;
                        }
                        else
                        {
                            //we're setting our song to something nuts, and it doesn't even match our old JSON
                            //no check, we don't know what they're doing
                            mainCheckBoxes[boxIDNum].Checked = false;
                        }
                    }
                    else if (boxType == "b")
                    {
                        if (currentSetListIndexes_boss[boxIDNum] == -2 && currentSetListName_b[boxIDNum] == cBox.Text)
                        {
                            //our current JSON matches with whatever the hell was typed in, don't do a check; we can still have the "?" in the box if we want
                            bossCheckBoxes[boxIDNum].Checked = false;
                        }
                        else
                        {
                            //the user is having a nervous breakdown and is typing in random shit
                            //that's a check
                            //bossCheckBoxes[boxIDNum].Checked = true;
                            //no wait, not a check, we have no idea what they're doing
                            bossCheckBoxes[boxIDNum].Checked = false;
                        }
                    }

                }

            }


        }

        /*
        private void setChecksAndGrabLvlBoxesFromSelection(ComboBox cBox)
        {
            //we want this to run at some point when we're selecting a custom song, or changing it back to default, etc.
            //we want to change the checkmark next to our Combo Box based on if we've changed the song to something that WASN'T in the JSON before
            //if the cBox contains a string the program doesn't recognize, ie: half the song name, check should be turned off, to indicate we have no idea what the user is doing so we're not gonna mess with the file

            //Gotta change this so it only changes the checkmark, don't change enable grab lvl button

            CheckBox[] mainCheckBoxes = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8 };
            CheckBox[] bossCheckBoxes = { checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };

            string m_or_b = cBox.Name.Substring(0, 1); //ie mainCombo1, bossCombo2
            string whichCBoxStr = cBox.Name.Substring(cBox.Name.Length - 1, 1);
            int cBoxInt = Int32.Parse(whichCBoxStr);
            cBoxInt -= 1; //because we want 0-based index, not 1-based

            if (cBox.SelectedIndex > -1)
            {
                //we chose a custom song
                //I don't think we need to check this, we should already know, now we're just executing the code
                if(inputtedLevelMatchesOld(cBoxInt, m_or_b, cBox.SelectedIndex))
                {
                    enableGrabLvlButton(cBox, " "); //this will enable the box if it sees we're putting " " in its text; it means we're not taking from the level info of another mod's JSON
                    if (m_or_b == "m")
                    {
                        mainCheckBoxes[cBoxInt].Checked = false;
                    } else if(m_or_b == "b")
                    {
                        bossCheckBoxes[cBoxInt].Checked = false;
                    }
                } else
                {
                    if (m_or_b == "m")
                    {
                        mainCheckBoxes[cBoxInt].Checked = true;
                    }
                    else if (m_or_b == "b")
                    {
                        bossCheckBoxes[cBoxInt].Checked = true;
                    }
                }
            } else
            {
                //we don't have a custom song in the field, its either blank, default, or something we don't recognize

                string cBoxDefaultSong = getDefaultSong(cBoxInt, m_or_b);

                if (cBox.Text == "" || cBox.Text == cBoxDefaultSong)
                {
                    //no need for the question mark
                    if (cBox.Text == "") cBox.Text = cBoxDefaultSong;

                    enableGrabLvlButton(cBox, ""); //this will disable the box if it sees we're putting nothing in its text
                    //we need to set it up so -2 is a song we don't recognize and isn't a default song
                    //or maybe we set it to not store ints, but instead stores the string of the name of the song
                    if (inputtedLevelMatchesOld(cBoxInt, m_or_b, cBox.SelectedIndex))
                    {
                        if (m_or_b == "m")
                        {
                            mainCheckBoxes[cBoxInt].Checked = false;
                        }
                        else if (m_or_b == "b")
                        {
                            bossCheckBoxes[cBoxInt].Checked = false;
                        }
                    }
                    else
                    {
                        //they're default songs, but this doesn't match the old inputs, so mark that they're going to be changed
                        if (m_or_b == "m")
                        {
                            mainCheckBoxes[cBoxInt].Checked = true;
                        }
                        else if (m_or_b == "b")
                        {
                            bossCheckBoxes[cBoxInt].Checked = true;
                        }
                    }
                } else
                {
                    //we inputted something the program doesn't recognize

                    enableGrabLvlButton(cBox, "?"); //this will disable the box if it sees we're putting "?" in its text
                    if (m_or_b == "m")
                    {
                        mainCheckBoxes[cBoxInt].Checked = false;
                    }
                    else if (m_or_b == "b")
                    {
                        bossCheckBoxes[cBoxInt].Checked = false;
                    }
                }

            }

        }*/

        private void fixJsonPreliminary(string fullJson)
        {
            //this function is meant to take a Json file and rewrite it line-by-line if it has to, if it sees there's anomolies or errors
            //if it's unfixable, we can at least say where there's a problematic area
            //it's not that intricate...

            string fixedJson = fullJson;

            string[] fixedJsonLines = fixedJson.Split('\n'); //split our Json into an array of strings, line by line

            if (fixedJsonLines.Length == 1)
            {
                //we couldn't find any line breaks using \n, we'll try to split it with \r
                fixedJsonLines = fixedJson.Split('\r');
            }
            if (fixedJsonLines.Length == 1)
            {
                //there's either an error, or the mod creator is a psychopath and didn't put line returns
                return;
            }


            List<string> expectedLevelEntry = new List<string>();
            //this is an array of strings regarding what the JSON is expected to have, per level
            expectedLevelEntry.Add("\"LevelName\"");




        }

        private void interpretJsonFnPErrors(string[] errorStringList)
        {
            //this function will fill our listview box with the interpreted info

            string[] criticalErrors = { "noLines", "{", "}", ",", "-,", "[", "]", "mb",
            "Bank", "Event", "LowHealthBeatEvent", "BPM", "bp", "noBP" };




            foreach (string error in errorStringList)
            {
                string[] errorInfo = error.Split(':');
                bool criticalError = true;

                if (errorInfo[1].Substring(0, 1) == "_")
                {
                    //whatever our error is, it's a minor error
                    criticalError = false;
                }

                string errorCode = errorInfo[1];
                string errorDetails = "";

                switch (errorCode)
                {
                    case "noLines":
                        errorDetails = "There was an error reading the lines of the JSON—possibly no line breaks in file.";
                        break;
                    case "{":
                        errorDetails = "Expected { is missing";
                        break;
                    case "}":
                        errorDetails = "Expected } is missing";
                        break;
                    case ",":
                        errorDetails = "Comma expected at the end of the line";
                        break;
                    case "-,":
                        errorDetails = "A comma was found that shouldn't be here";
                        break;
                    case "clb":
                        errorDetails = "Expected \"customLevelMusic\" : [ is missing";
                        break;
                    case "[":
                        errorDetails = "[ expected at the end of \"customLevelMusic\" line";
                        break;
                    case "]":
                        errorDetails = "Expected ] is missing";
                        break;
                    case "mb":
                        errorDetails = "Expected Music label is missing (\"MainMusic\" or \"BossMusic\"); label might be mispelled or missing quotes";
                        break;
                    case "Bank":
                        errorDetails = "Expected \"Bank\" label is missing; label might be mispelled or missing quotes";
                        break;
                    case "Event":
                        errorDetails = "Expected \"Event\" label is missing; label might be mispelled or missing quotes";
                        break;
                    case "LowHealthBeatEvent":
                        errorDetails = "Expected \"LowHealthBeatEvent\" label is missing; label might be mispelled or missing quotes";
                        break;
                    case "BeatInputOffset":
                        errorDetails = "Expected \"BeatInputOffset\" label is missing; label might be mispelled or missing quotes";
                        break;
                    case "BPM":
                        errorDetails = "Expected \"BPM\" label is missing; label might be mispelled or missing quotes";
                        break;
                    case "bP":
                        errorDetails = "{ Expected";
                        break;
                    case "noBP":
                        errorDetails = "Expected \"bankPath\" label is missing; label might be mispelled or missing quotes";
                        break;
                }



            }

        }


        //i'm rewriting this. i hate my life
        /*
        private string[] debuggy(string fullJson)
        {
            string fixedJson = fullJson;
            List<string> linesWithErrors = new List<string>();

            string[] fixedJsonLines = fixedJson.Split('\n'); //we're going to keep our empty lines, to be sure what line we're on

            int currPlaceInExpctdEntry = 0; //current place in expected entry
            int i = 0;
            int threshold = 0;
            while (i < fixedJsonLines.Length)
            {
                #region DebugLines
                string line_unaltered = fixedJsonLines[i];
                string line_nospaces = NormalizeWhiteSpace(fixedJsonLines[i], true); //gives us us the individual line in the JSON, with no spaces whatsoever
                if (line_nospaces == null || line_nospaces == "")
                {

                    linesWithErrors.Add(i + ":empty");
                    i++;
                    continue;
                }

                string finalChar = line_nospaces.Substring(line_nospaces.Length - 1, 1); //we only care about the last item on the line

                if (i >= 2)
                {
                    //if we're above our length, we might be done, check for closing ]
                    if (currPlaceInExpctdEntry >= expectedFields.Length)
                    {
                        if (line_unaltered.Contains("]"))
                        {
                            linesWithErrors.Add("Found]");
                            break;
                        } else
                        {
                            currPlaceInExpctdEntry = 0;
                        }
                    }

                    bool hasMatchingLabel = line_nospaces.Contains(expectedFields[currPlaceInExpctdEntry]); //labelMatches is true if our expectedField matches with whatever's on the line

                    if (currPlaceInExpctdEntry == 0)
                    {
                        //Level Opening {
                        if (!hasMatchingLabel)
                        {

                        }

                    } else if (currPlaceInExpctdEntry == 1)
                    {
                        //"LevelName" : "Voke",

                    }
                    else if (currPlaceInExpctdEntry == 2)
                    {
                        //"MainMusic" : {
                        bool hasMainMusic = line_nospaces.Contains("\"MainMusic\""); //labelMatches is true if our expectedField matches with whatever's on the line
                        bool hasBossMusic = line_nospaces.Contains("\"BossMusic\""); //labelMatches is true if our expectedField matches with whatever's on the line
                        if (hasMainMusic || hasBossMusic)
                        {
                            hasMatchingLabel = true;
                        }


                    }
                    else if (currPlaceInExpctdEntry == 3)
                    {
                        //"Bank" : "Unstoppable_All",

                    }
                    else if (currPlaceInExpctdEntry == 4)
                    {
                        //"Event": "{5fe68f72-ee2e-4ef8-907e-b30a324b6f9b}",

                    }
                    else if (currPlaceInExpctdEntry == 5)
                    {
                        //"LowHealthBeatEvent": "{cd12c5c1-6d28-49c9-bcf5-567c9c3ae5bf}",
                    }
                    else if (currPlaceInExpctdEntry == 6)
                    {
                        //"BeatInputOffset": 0.00,
                    }
                    else if (currPlaceInExpctdEntry == 7)
                    {
                        //"BPM": 121 (, if bankPath on next line)
                    }
                    else if (currPlaceInExpctdEntry == 8)
                    {
                        //"bankPath": "R:\\SteamLibrary\\steamapps\\common\\Metal Hellsinger\\MODS\\Unstoppable\\Unstoppable_All.bank"
                        if (!hasMatchingLabel)
                        {
                            if (fixedJsonLines[i].Contains("}"))
                            {
                                //linesWithErrors.Add("(" + i + ":nobp)");
                                currPlaceInExpctdEntry++; //move our current place forward
                                //i--; //move our current line back to recheck this line this isn't a for loop, we don't auto add
                                continue; //cancel all other calculations, we're rechecking this line

                            }
                        }
                    }
                    else if (currPlaceInExpctdEntry == 9)
                    {
                        //Music Cosing } (, if going to BossMusic)
                        if (!hasMatchingLabel)
                        {

                        }
                    }
                    else if (currPlaceInExpctdEntry == 10)
                    {
                        //Level Closing }, or BossMusic
                        if (fixedJsonLines[i].Contains("\"BossMusic\""))
                        {
                            currPlaceInExpctdEntry = 2;
                            continue;
                        } else if (!hasMatchingLabel)
                        {

                        }
                    }

                    string lineErrors = getLineErrors(fixedJsonLines[i], fixedJsonLines[i + 1], currPlaceInExpctdEntry);
                    if (lineErrors.Length > 0)
                    {
                        linesWithErrors.Add(i + 1 + ":" + lineErrors + "\n\r"); //i+1 because we don't start on line 0
                    }

                    currPlaceInExpctdEntry++;
                }


                i++;

                threshold++;
                #endregion DebugLines
                if (threshold > 300)
                {
                    //string[] tooLong = { "2long" };
                    linesWithErrors.Add("(2long)");
                    return linesWithErrors.ToArray();
                }
            }

            if (linesWithErrors.Count > 0)
            {
                string[] errorList = linesWithErrors.ToArray();
                return errorList;
            } else
            {
                string[] noErrors = { "none" };
                return noErrors;
            }


        }*/

        private string getLineErrors(string line, string lineAfter, int indexOfLabelWeWant)
        {

            //, bool quotesOnValue, string format = ""


            List<string> errorsOnLine = new List<string>();
            string lineNS = NormalizeWhiteSpace(line);
            string endingWeWant = expectedEndings[indexOfLabelWeWant];
            string labelWeWantNoQuotes = expectedFields[indexOfLabelWeWant].Replace("\"", "");
            string finalCharOnLine = lineNS.Substring(lineNS.Length - 1, 1); //we only care about the last item on the line

            string nextLineNS = NormalizeWhiteSpace(lineAfter);





            if (line.Contains(expectedFields[indexOfLabelWeWant]))
            {
                //line contains label without issues
            }
            else if (line.Contains(labelWeWantNoQuotes))
            {
                //label is missing quotes
                errorsOnLine.Add("labelquotesmissing");
            }
            else
            {
                //we cannot find the label we mentioned

                //see if we have two options
                if (expectedFields[indexOfLabelWeWant].Contains("|"))
                {
                    //this will only happen for Main/BossMusic
                    string[] possibleLabels = expectedFields[indexOfLabelWeWant].Split('|');
                    bool foundPossLbl1 = line.Contains(possibleLabels[0]);
                    bool foundPossLbl2 = line.Contains(possibleLabels[1]);
                    //bool foundPossLblNQ = line.Contains(possibleLabels[0].Replace("\"", "")) || line.Contains(possibleLabels[1].Replace("\"", ""));

                    if (foundPossLbl1 || foundPossLbl2)
                    {
                        goto EndLabelCheck;
                    }


                    string labelMissing = "labelmissing_Musiclabel";
                    return labelMissing;
                } else
                {
                    //we don't have two options, and we can't find the label
                    errorsOnLine.Add("labelmissing");
                }
            }


        EndLabelCheck:

            if (endingWeWant.Length == 1)
            {
                if (finalCharOnLine != endingWeWant)
                {
                    errorsOnLine.Add("unexpectedEnd-Wanted_" + endingWeWant);
                    //this might not be neccessary
                }
            } else
            {
                /*string[] possibleEndings = endingWeWant.Split(new string[] { "or" }, StringSplitOptions.None);
                bool foundAltEnding1 = finalCharOnLine == possibleEndings[0];
                bool foundAltEnding2 = finalCharOnLine == possibleEndings[1];
                if (foundAltEnding1 || foundAltEnding2)
                {

                } screw this*/
                if (indexOfLabelWeWant == 7)
                {
                    //we're looking at BPM
                    if (nextLineNS.Contains("bankPath")) {
                        //we're looking at BPM, and next line has Bankpath
                        if (finalCharOnLine != ",")
                        {
                            errorsOnLine.Add("missingcomma");
                        }
                    } else
                    {
                        //the next line doesn't have a bankPath, we want a number on this line's final char

                        if (Int32.TryParse(finalCharOnLine, out int yo))
                        {
                            //the last character on the line is a number

                        } else
                        {
                            //the last character on the line is not a number

                            errorsOnLine.Add("unexpectedEnd-Wanted_Num");
                        }
                    }
                } else if (indexOfLabelWeWant == 8)
                {
                    //we're looking at BankPath

                    //ending should always be quotes

                } else if (indexOfLabelWeWant == 9)
                {
                    //we're looking at MainMusic close or BossMusic close
                    if (nextLineNS.Contains("BossMusic") || nextLineNS.Contains("MainMusic"))
                    {
                        //we're on the MainMusic close, opening for BossMusic next
                        //or the user copied and pasted and forgot to change MainMusic to BossMusic, which we'll handle first
                        /*
                        if (nextLineNS.Contains("MainMusic"))
                        {
                            errorsOnLine.Add("NL_bossnotmain"); //next line, we want boss not main music
                        }*/

                        if (finalCharOnLine != ",")
                        {
                            errorsOnLine.Add("missingcomma");
                        }
                    }
                    else
                    {
                        //we don't have BossMusic on the next line, we're just closing out Music
                        string unexpectedCharacters = lineNS.Replace("}", "");
                        if (lineNS != "}")
                        {
                            errorsOnLine.Add("unexpectedCharsC_" + unexpectedCharacters);
                        }

                    }
                }
                else if (indexOfLabelWeWant == 10)
                {
                    if (nextLineNS.Contains("{"))
                    {
                        //we're doing another level on the next line, so we want a comma
                        if (finalCharOnLine != ",")
                        {
                            errorsOnLine.Add("missingcomma");
                        }
                    } else
                    {
                        //we don't have another level on the next line
                        //just make sure we didn't have anything else in this line
                        string unexpectedCharacters = lineNS.Replace("}", "");
                        if (lineNS != "}")
                        {
                            errorsOnLine.Add("unexpectedCharsD_" + unexpectedCharacters);
                        }
                    }


                }
            }


            string[] LineFormatErrors = getFormatErrors(line, indexOfLabelWeWant); //get the format errors
            errorsOnLine.AddRange(LineFormatErrors); //add them to our line errors

            string errorReportString = "";
            foreach (string error in errorsOnLine)
            {
                errorReportString += error + "|";
            }

            return errorReportString;



        }

        private string[] getFormatErrors(string line, int indexOfLabelWeWant)
        {
            //i'm initally expecting this to only check values, but we'll see

            //need to remember to go back and omit ending checks if we only have } or { on the line

            string lineNS = NormalizeWhiteSpace(line); //line with no spaces; we shouldn't purge all spaces
            List<string> formatErrors = new List<string>();
            string errorReport = "";

            string[] lineColonSplit = lineNS.Split(':');

            if (indexOfLabelWeWant == 0)
            {
                //Level Opening {
                string unexpectedCharacters = lineNS.Replace("{", "");
                if (unexpectedCharacters.Length > 0)
                {
                    formatErrors.Add("unexpectedCharsA_" + unexpectedCharacters);
                }
            } else if (indexOfLabelWeWant == 9 || indexOfLabelWeWant == 10)
            {
                //either a level-closing or music-closing }
                string unexpectedCharacters = lineNS;
                if (unexpectedCharacters.Substring(unexpectedCharacters.Length - 1, 1) == ",")
                {
                    unexpectedCharacters = unexpectedCharacters.Substring(0, unexpectedCharacters.Length - 1);
                }
                unexpectedCharacters = unexpectedCharacters.Replace("}", "").Replace("\r", "").Replace("\n", "");//why is this not working!?!!!
                                                                                                                 //removing control characters. I'm about to throw something through my wall

                // Get the integral value of the character.

                if (!string.IsNullOrEmpty(unexpectedCharacters))
                {

                    formatErrors.Add("unexpectedCharsB_" + unexpectedCharacters);
                }

            } else
            {
                if (lineColonSplit.Length == 1) {
                    formatErrors.Add("nocolon");
                } else
                {
                    string[] returnString = checkFormatLineWithLabel(lineColonSplit, indexOfLabelWeWant);
                    return returnString;
                }
            }

            return formatErrors.ToArray();
        }

        private string[] checkFormatLineWithLabel(string[] splitInfo, int indexOfLabel)
        {
            string labelStr = splitInfo[0];
            string valueStr = splitInfo[1];
            List<string> lineFormatErrors = new List<string>();
            valueStr.TrimEnd();//get rid of all whitespace on the right of value/value's comma
            if (valueStr.Substring(valueStr.Length - 1, 1) == ",")
            {
                valueStr = valueStr.Substring(0, valueStr.Length - 1); //if we had a comma, it's gone now—we already checked for endings
            }
            string labelNS = NormalizeWhiteSpace(labelStr, true);
            string valueNS = NormalizeWhiteSpace(valueStr);

            int numberOfQuotesInValue = valueNS.Split('\"').Length - 1;
            int numberOfQuotesInLabel = labelNS.Split('\"').Length - 1;


            //Label check; we already checked for if it has the exact label name with quotes around it

            if (indexOfLabel == 2)
            {
                string checkMainMusic = labelNS.Replace("\"MainMusic\"", "");
                string checkBossMusic = labelNS.Replace("\"BossMusic\"", ""); //doing both of these in case our user somehow put MainMusic BossMusic: {
                string unexpectedCharsInLabel = labelNS.Replace("\"MainMusic\"", "").Replace("\"BossMusic\"", ""); //but this will look weird if they did
                if (checkMainMusic.Length > 0 && checkBossMusic.Length > 0)
                {
                    lineFormatErrors.Add("unexpectedCharsLOfC_" + unexpectedCharsInLabel);
                }
            } else
            {
                string unexpectedCharsInLabel = labelNS.Replace(expectedFields[indexOfLabel], "");
                if (unexpectedCharsInLabel.Length > 0)
                {
                    lineFormatErrors.Add("unexpectedCharsLOfC_" + unexpectedCharsInLabel);
                }
            }


            if (indexOfLabel == 6 || indexOfLabel == 7 || indexOfLabel == 2) goto ValueNoQuoteCheck;

            ValueWithQuoteCheck:

            if (numberOfQuotesInValue > 2)
            {
                lineFormatErrors.Add("2mqVal");
            } else if (numberOfQuotesInValue < 2)
            {
                lineFormatErrors.Add("neqVal");
            } else
            {
                //we have only two quotes in the value, which is what we want
                if (indexOfLabel == 4 || indexOfLabel == 5)
                {
                    //we're checking an event
                    string checkEvent = shaveSurroundingQuotesAndSpaces(valueNS); //getting rid of the spaces, then quotes
                    if (checkEvent.Length != valueNS.Length - 2)
                    {
                        //the two quotes we found weren't surrounding
                        lineFormatErrors.Add("evF1");
                        return lineFormatErrors.ToArray();

                    }
                    checkEvent = checkEvent.TrimStart('{').TrimEnd('}');

                    if (checkEvent.Length != valueNS.Length - 4 || checkEvent.Length != 36)
                    {
                        //Event string does NOT have { and }, OR it does not have the full 36-digit ID
                        lineFormatErrors.Add("evF2");
                        return lineFormatErrors.ToArray();
                    }
                }

            }

            return lineFormatErrors.ToArray();

        // We've stopped if we were expecting quotes

        ValueNoQuoteCheck:
            if (numberOfQuotesInValue > 0)
            {
                lineFormatErrors.Add("unwantedquotes");
            }
            else
            {
                //we don't have any quotes, which is what we want
                if (indexOfLabel == 2)
                {
                    if (valueNS.Trim() != "{")
                    {
                        lineFormatErrors.Add("unexpectedCharsROfC_" + labelNS.Replace("{", ""));
                    }
                }
                else
                {
                    //if we're here, then we're looking at BeatInputOffset or BPM, both are number-only variables
                    
                    if (IsValueANumber(valueNS.Trim()) == false)
                    {
                        lineFormatErrors.Add("numFormat");
                    }
                    /*
                    if (Decimal.TryParse(valueNS.Trim(), out decimal hi))
                    {
                        //the value is a number
                    }
                    else
                    {
                        //the value is NOT a number
                        lineFormatErrors.Add("numFormat");
                    }*/
                }
            }
            return lineFormatErrors.ToArray();
        }


        string[] expectedEndings = { "{", ",", "{", ",", ",", ",", ",", ",or ", "bnkp", "}or,", "}or," };
        //                            L   LN   MM   Ba   Ev   LH   Ofs   BPM*  BnkPth   MMcBMc  Lc
        //                                      ^-----<----------<------------<--------' if(,)

        //                          0          1                       2                    3            4                   5                        6              7            8          9    10
        string[] expectedFields = { "{", "\"LevelName\"", "\"MainMusic\"|\"BossMusic\"", "\"Bank\"", "\"Event\"", "\"LowHealthBeatEvent\"", "\"BeatInputOffset\"", "\"BPM\"", "\"bankPath\"", "}", "}" };


        private int nextValidLineContaining(string needle, string[] allLines, int startingLine = 0, int maxAttempts = 5)
        {
            //this starts by checking the startingLine's line (that is, we don't omit the startingLine)
            //maxAttempts is to stop us from going too far
            int curLine = startingLine;
            int attempts = 0; //if attempts is 1, this  ↓ should be '< or equal to'
            while (curLine < allLines.Length && attempts < maxAttempts)
            {
                string nakedLine = NormalizeWhiteSpace(allLines[curLine], true);
                if (nakedLine == null) continue;
                if (nakedLine.Contains(needle))
                {
                    //this is a line that has SOMETHING on it
                    return curLine;
                }

                curLine++; //go to the next line to check
            }
            return -1;
        }

        private string getQuotesError(int howManyWeHave, int howManyWeWant)
        {
            //we either want 0, or 2 quotes. We shouldn't ever want ONE quote

            if (howManyWeWant == 2)
            {
                //we want 2
                if (howManyWeHave > 2)
                {
                    //we have more than 2 though
                    return "2mq"; //needs L or R still
                } else if (howManyWeHave == 1)
                {
                    return "msq"; //missing 1 quote
                } else if (howManyWeHave == 0)
                {
                    return "addq@";
                }
            }

            if (howManyWeWant == 0)
            {
                //we want 0
                if (howManyWeHave == 2)
                {
                    //we confused the formatting
                    return "remq";
                } else if (howManyWeHave < 2)
                {
                    //we have more than 2 though
                    return "2mq";
                }
                else if (howManyWeHave == 1)
                {
                    return "lnq"; //lone quote, remove it
                }
            }

            return null; //there were no issues

        }

        private string fixAnyLinesWithTabs(string fullJson)
        {
            //this actually takes a full JSON, and looks for any tabs, removes them, then rewrites the blemished line
            //we return the full JSON string, fixed

            string fixedJson = fullJson;

            if (!fixedJson.Contains('\t'))
            {
                //there's no horizontal tabs in here, just return what we had
                return fixedJson;
            }

            //if we got this far, we have horizontal tabs we need to fix

            string[] fixedJsonLines = fixedJson.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries); //split our Json into an array of strings, line by line

            if (fixedJsonLines.Length == 1)
            {
                //we couldn't find any line breaks using \n, we'll try to split it with \r
                fixedJsonLines = fixedJson.Split(new string[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
            }
            if (fixedJsonLines.Length == 1)
            {
                //there's either an error, or the mod creator is a psychopath and didn't put line returns
                return "Error, no line returns";
            }

            string[] openingKeywords = { "{", "\"customLevelMusic\"" };
            string[] keywords = { "{", "\"LevelName\"", "\"MainMusic\"", "\"Bank\"", "\"Event\"", "\"LowHealthBeatEvent\"", "\"BeatInputOffset\"", "\"BPM\"", "\"bankPath\"", "}",
            "\"BossMusic\"", "\"Bank\"", "\"Event\"", "\"LowHealthBeatEvent\"", "\"BeatInputOffset\"", "\"BPM\"", "\"bankPath\"", "}", "}" };
            string[] closingKeywords = { "]", "}" };

            //for keywords string:
            //BossMusic is index 10 of keywords
            //main music closer is 9
            //boss music closer is 17
            //LEVEL music closer is 18

            string[] uniqueKeywords = { "\"LevelName\"", "\"MainMusic\"", "\"BossMusic\"", "\"Bank\"", "\"Event\"", "\"LowHealthBeatEvent\"", "\"BeatInputOffset\"", "\"BPM\"", "\"bankPath\"" };
            //uniqueKeywords are used to determine which line we might be on in the JSON

            //for uniqueKeywords string:
            //Bank - bankPath are Index 3-8
            //main music and boss music are 1-2
            //LevelName is 0





            fixedJson = ""; //reset this, we're going to use it to return the info
            for (int i = 0; i < fixedJsonLines.Length; i++)
            {
                if (fixedJsonLines[i].Contains("\t"))
                {
                    //the line contains a tab.

                    string fixedLine = fixedJsonLines[i].Replace("\t", ""); //first, burn the tabs

                    // now find out where we are

                    bool uniqueFound = false;

                    if (hasStringWithColonAfterIt(fixedLine, "customLevelMusic"))
                    {
                        fixedJson += "    \"customLevelMusic\" : [\n";
                        continue;
                    }
                    if (fixedLine.Contains("]") && i == fixedJsonLines.Length - 2)
                    {
                        fixedJson += "    ]\n";
                        continue;
                    } else if (fixedLine.Contains("]") && i != fixedJsonLines.Length - 2)
                    {
                        return "Error - there is a weird placement of ] in the JSON";
                    }


                    for (int z = 0; z < uniqueKeywords.Length; z++)
                    {
                        if (hasStringWithColonAfterIt(fixedLine, uniqueKeywords[z]))
                        {
                            string rewrittenLine = "";

                            //we found a unique keyword, we can immediately know what the line should look like
                            int lineLabelStart = fixedLine.IndexOf(uniqueKeywords[z]);
                            int lineLabelAfterColon = fixedLine.IndexOf(":", lineLabelStart + uniqueKeywords[z].Length) + 1;
                            string lineLabel = fixedLine.Substring(lineLabelStart, lineLabelAfterColon);
                            rewrittenLine += lineLabel + " ";

                            string lineInfo = fixedLine.Substring(lineLabelAfterColon); //gets everything after the colon. could have a space, could have quotes (we're about to get rid of potential comma)

                            int indexOfComma = fixedLine.IndexOf(",");
                            if (indexOfComma != -1)
                            {
                                //there's a comma, make our line info only have what's before it
                                int realInfoLength = indexOfComma - lineLabelAfterColon;
                                lineInfo = fixedLine.Substring(realInfoLength);
                            }



                            lineInfo = shaveSurroundingQuotesAndSpaces(lineInfo); //now we only have the information


                            //fixedJson += fixedJsonLines[i];

                            if (z == 0)
                            {
                                //levelName

                                rewrittenLine = "            \"" + lineLabel + " " + lineInfo + "\",\n";//+\n? //will return (spaces) "LevelName" : "Stygia",
                            } else if (z >= 2 && z <= 3)
                            {
                                //BossMusic or MainMusic
                                rewrittenLine = "            \"" + lineLabel + " {\n";
                            } else
                            {
                                //one of the labels
                                if (uniqueKeywords[z] == "\"BeatInputOffset\"" || uniqueKeywords[z] == "\"BPM\"")
                                {

                                    //we don't want to add quotes to info
                                    rewrittenLine = "                " + lineLabel + " " + lineInfo;
                                } else
                                {
                                    //we want to add quotes to info, since we took them away
                                    rewrittenLine = "                " + lineLabel + " \"" + lineInfo + "\"";
                                }

                                if (uniqueKeywords[z] == "\"BPM\"")
                                {
                                    //we're doing a BPM line, we need to check to add a comma or not

                                    if (hasStringWithColonAfterIt(fixedJsonLines[i + 1], "\"bankPath\""))
                                    {
                                        //there IS a bank path
                                        rewrittenLine += ",\n";
                                    } else
                                    {
                                        //no bankPath detected after this line
                                        rewrittenLine += "\n";
                                    }
                                }

                            }

                            fixedJson += rewrittenLine;
                            break; //since we found a unique keyword, get out of this
                        }
                    }

                    if (!uniqueFound)
                    {
                        //we couldn't find a unique keyword, we still don't know where we're at

                        //we're either on a line with only {, or }, possibly next to a comma
                        if (fixedLine.Contains("{"))
                        {
                            if (i == 0)
                            {
                                //we're on the first line
                                fixedJson += "}";
                                continue;
                            }
                            else
                            {
                                //after first line, the only time we can have { without any of the uniqueKeywords is when opening a level. we know where we're at
                                fixedJson += "        {\n";
                                continue;
                            }
                        }
                        else if (fixedLine.Contains("}"))
                        {
                            string rewrittenLine = "";

                            //need to find out if we just closed the Main/BossMusic, or the Level

                            //we might be on the last line of the code
                            if (i == fixedJsonLines.Length - 1)
                            {
                                //we're on the last line
                                fixedJson += "}\n";
                                continue;
                            }
                            else if (hasStringWithColonAfterIt(fixedJsonLines[i - 1], "BPM") || hasStringWithColonAfterIt(fixedJsonLines[i - 1], "bankPath"))
                            {
                                //check to see if the line before us has a label for "BPM" or "bankPath"


                                //the line before this has the BPM or the bankPath, we're either closing the Main or Boss Music
                                if (fixedJsonLines[i + 1].Contains("\"BossMusic\""))
                                {
                                    //the line underneath us has the bossMusic info, so put a comma
                                    fixedJson += "            },\n";

                                    continue;
                                } else if (fixedJsonLines[i + 1].Contains("}"))
                                {
                                    //the line underneath us is closing out the level, don't put a comma
                                    fixedJson += "            }\n";

                                    continue;
                                }
                            } else if (fixedJsonLines[i - 1].Contains("}"))
                            {
                                //the line before this is closing out the music, we need to close out the level
                                rewrittenLine = "        }";
                                if (fixedJsonLines[i + 1].Contains("{"))
                                {
                                    //the next line opens up to another level, so put a comma
                                    rewrittenLine += ",\n";
                                } else
                                {
                                    rewrittenLine += "\n";
                                }
                                fixedJson += rewrittenLine;
                                continue;


                            } else
                            {
                                return "Error - there was an error trying to fix the horizontal tabs";
                            }
                        }

                    }



                } else
                {
                    //we didn't have a tab here, and that's this function's jurisdiction
                    fixedJson += fixedJsonLines[i] + '\n';

                }


            }
            return fixedJson;


        }

        //we use this function to see what we're looking at in a line like this "Bank": "BPM"
        private bool hasStringWithColonAfterIt(string hay, string needle)
        {
            //MessageBox.Show("Checking " + hay + "for " + needle);

            if (!hay.Contains(needle)) return false; //we don't have it at all

            int indexOfColon = hay.IndexOf(":");
            if (indexOfColon == -1) return false;//we don't have a colon at all in this label, which is weird



            if (indexOfColon < hay.IndexOf(needle)) return false; //our colon is before our needle, return false

            int indexAfterNeedle = hay.IndexOf(needle) + needle.Length;
            int lengthOfArea = indexOfColon - indexAfterNeedle;
            string textBetweenNeedleAndColon = hay.Substring(indexAfterNeedle, lengthOfArea);
            textBetweenNeedleAndColon = textBetweenNeedleAndColon.Replace("\t", "");
            textBetweenNeedleAndColon = textBetweenNeedleAndColon.Replace(" ", "");
            textBetweenNeedleAndColon = textBetweenNeedleAndColon.Replace("\"", "");
            if (textBetweenNeedleAndColon.Length > 0) return false; //there's something weird in between the needle and the colon

            return true; //our colon is directly after our needle, return true!
        }


        private bool modSupportsLevel(int modIndex, int Level, string m_or_b)
        {
            ////testFindJson.Text += "MSLCalled: (" + modIndex + ", " + Level + ", " + m_or_b + ")";
            bool result = false;

            //example of supported Lvl string is 0b1mb23mb4m567m
            if (modIndex == -1)
            {
                //we're looking at the default level, just return true
                return true;
            }

            ////testFindJson.Text += "ModIndex: " + modIndex + "; ";

            string supportedLvlString = csSupLvls[modIndex]; //gives us the supported levels string of the mod that's selected
            int levelInfoIndex = supportedLvlString.IndexOf(Level.ToString());//this is going to give us the spot right before the level number. After the number is m, b, mb, or (nothing, next number)

            if (levelInfoIndex == -1)
            { ////testFindJson.Text += "ERROR OCCURED, no levelInfo found in string"; 
                return result;
            }//this shouldn't ever happen, but just in case
            if (levelInfoIndex == supportedLvlString.Length - 1) return result; //if it has no support on last number, then we're at the last character

            ////testFindJson.Text += "MSL: " + supportedLvlString + "; ";

            if (m_or_b == "m")
            {
                //m will always be right after the level number
                if (supportedLvlString.Substring(levelInfoIndex + 1, 1) == "m")
                {
                    result = true;
                    return result;
                } else
                {
                    ////testFindJson.Text += " F6 " + supportedLvlString + "; ";
                    return result;
                }
            } else if (m_or_b == "b")
            {
                //we can have 4mb5, 4b5, 45b
                if (supportedLvlString.Substring(levelInfoIndex + 1, 1) == "b")
                {
                    //we found a "b" after our number, return true
                    ////testFindJson.Text += " A1: " + supportedLvlString + "; ";
                    result = true;
                    return result;
                } else
                {
                    ////testFindJson.Text += " A2: " + supportedLvlString.Substring(levelInfoIndex + 1, 1) + "..";
                }


                //if we got this far, there was no "b" RIGHT after number; next possibilities are 1mb2, or 12b
                //or it could be ..6mb7 (meaning NOTHING is there)
                if (levelInfoIndex >= supportedLvlString.Length - 2)
                { ////testFindJson.Text += " D4 ";
                    return result;
                }
                string checkForNumStr = supportedLvlString.Substring(levelInfoIndex + 1, 1);


                /*
                bool twoSpacesOverIsNumber = false;

                for (int i = 0; i < 8; i++)
                {
                    
                    if(checkForNumStr == i.ToString())
                    {
                        ////testFindJson.Text += "\n" + checkForNumStr + "==" + i.ToString();
                        twoSpacesOverIsNumber = true;
                        ////testFindJson.Text += "> >2spacesover: "+ twoSpacesOverIsNumber + "; ";
                    } else
                    {
                        ////testFindJson.Text += "\n" + checkForNumStr + "!=" + i.ToString() + "; ";
                    }
                }
                
                if (twoSpacesOverIsNumber)
                I WAS GOING FUCKING INSANE. checkForNumStr was levelInfoIndex+2, and kept pulling the wrong information
                */

                if (Int32.TryParse(checkForNumStr, out int j))
                {
                    //this should activate if our character after Level number was another number; meaning the Mod doesn't have info for either main music or boss on this level, ie. the 1 in 0mb12m3mb
                    ////testFindJson.Text += " B2 " + supportedLvlString + "; ";
                    return false;
                }

                else if (supportedLvlString.Substring(levelInfoIndex + 2, 1) == "b")
                {
                    ////testFindJson.Text += " C3 " + supportedLvlString + "; ";
                    result = true;
                    return result;
                }
            }

            ////testFindJson.Text += " E5 " + supportedLvlString + "; ";

            return false;
        }

        /*private bool[] levelsSupportedInMod(string supportedLevelsString, string m_or_b )
        {
            bool[] result = new bool[0];

            int queriedLevel = -1;


            int L = 0;
            while (L < supportedLevelsString.Length)
            {
                string nextChar = supportedLevelsString.Substring(L, 1);

                //why can't switch statements use variables??


                if (!Int32.TryParse(nextChar, out int j))
                {
                    //this should activate if our nextChar wasn't a number
                    goto SupportLetter;
                }

                switch (nextChar)
                {
                    case "0":
                        queriedLevel = 0;
                        break;
                    case "1":
                        queriedLevel = 1;
                        break;
                    case "2":
                        queriedLevel = 2;
                        break;
                    case "3":
                        queriedLevel = 3;
                        break;
                    case "4":
                        queriedLevel = 4;
                        break;
                    case "5":
                        queriedLevel = 5;
                        break;
                    case "6":
                        queriedLevel = 6;
                        break;
                    case "7":
                        queriedLevel = 7;
                        break;


                }

            SupportLetter:


                if (m_or_b == "m" && nextChar == "m")
                {
                    modLvlButtons[queriedLevel].Enabled = true;
                }
                else if (m_or_b == "b" && nextChar == "b")
                {
                    modLvlButtons[queriedLevel].Enabled = true;
                }

                L++;
            }

            return result;
        }*/

        private void changeDefaultGrabLvlText(object sender, EventArgs e)
        {
            //this is ran whenever we change the selection of a Level's main music or boss music Combo box
            //this does not run when we TYPE it in though, (until we hit Enter)
            //setModGrabLvlSelection also enables the button
            ComboBox changedComboBox = sender as ComboBox;
            if (mmLoading) return;

            if (changedComboBox == null)
                return;

            ////testFindJson.Text += "Hi! Trying to change: " + changedComboBox.Name + "; ... ";

            if (!wasComboBoxChanged(changedComboBox)) { return; }
            setModGrabLvlSelection(changedComboBox);
            //setSongSelectionArray(changedComboBox);
            ////testFindJson.Text += ".cDGLT.";
        }
        private void changeDefaultGrabLvlTextJIC(object sender, EventArgs e)
        {
            //this is ran whenever we change the selection of a Level's main music or boss music Combo box
            //this does not run when we TYPE it in though, (until we hit Enter)
            //setModGrabLvlSelection also enables the button
            ComboBox changedComboBox = sender as ComboBox;
            if (mmLoading) return;

            if (changedComboBox == null)
                return;

            ////testFindJson.Text += "Hi! Trying to change: " + changedComboBox.Name + "; ... ";

            if (!wasComboBoxChanged(changedComboBox)) { return; }
            setModGrabLvlSelection(changedComboBox);
            //setSongSelectionArray(changedComboBox);
            ////testFindJson.Text += ".cDGLT.";
        }


        int numberOfLevels = 9; //8 levels + tutorial
        /// <summary>
        /// Looks through JSON, and determines which levels it supports, setting the colors for each level button in Organizer. Returns 0-base index of first supported level.
        /// </summary>
        /// <param name="fullJson"></param>
        private int setSupportedLevelColors(string fullJson)
        {
            Button[] LevelButtons = { L1Settings, L2Settings, L3Settings, L4Settings, L5Settings, L6Settings, L7Settings, L8Settings, L0Settings };

            //find the full level info
            //look for main music
            //look for boss music
            if (fullJson.Contains("\n"))
            {
                fullJson = NormalizeWhiteSpace(fullJson);
            }
            int firstLevelSupported = -1;

            //right now, level L0 is considered Level 9 or index 8 in certain spots.
            //that's going to be a problem in two weeks
            int veryFirstLevel = 8; //the zero-based index number for tutorial
            if (tsm_showTutOrganizer.Checked == false)
            {
                //we're told not to show tutorial in Organizer
                veryFirstLevel = 0;//our first level is Voke
            }

            for (int i = 0; i < numberOfLevels; i++)
            {

                string capitalizeLevelName = "\"" + allLevelNames[i].Substring(0, 1).ToUpper() + allLevelNames[i].Substring(1) + "\"";
                int indexOfLevelInfo = fullJson.IndexOf(capitalizeLevelName);


                if (indexOfLevelInfo == -1)
                {
                    LevelButtons[i].BackColor = Color.RosyBrown;
                    continue; //we can't find the level name, it's not in the JSON
                }

                //at this point we found the level name in the Json
                if (firstLevelSupported == -1) firstLevelSupported = i;
                if (veryFirstLevel == i)
                {
                    firstLevelSupported = veryFirstLevel; //it's done this way because our tutorial is at the end
                }


                int indexOfLevelInfoEnd = fullJson.IndexOf("} }", indexOfLevelInfo);
                if (indexOfLevelInfoEnd == -1)
                {
                    //we have a problem, try to fix it
                    fullJson = fullJson.Replace("\t", "");
                    indexOfLevelInfoEnd = fullJson.IndexOf("}}", indexOfLevelInfo);
                }
                if (indexOfLevelInfoEnd == -1)
                {
                    indexOfLevelInfoEnd = fullJson.IndexOf("}}", indexOfLevelInfo);
                }


                if (indexOfLevelInfoEnd == -1) { MessageBox.Show("BOOBOO"); return -1; }//we found start of a level, but couldn't find } }, there must be formatting errors

                //at this point, we have information for whatever level this is

                string fullLevelInfo = fullJson.Substring(indexOfLevelInfo, indexOfLevelInfoEnd - indexOfLevelInfo);

                int indexOfMainLevelMusic = fullLevelInfo.IndexOf("\"MainMusic\"");
                int indexOfBossFightMusic = fullLevelInfo.IndexOf("\"BossMusic\"");

                if (indexOfMainLevelMusic != -1 && indexOfBossFightMusic != -1)
                {
                    //this level has a MainMusic and BossMusic entry
                    LevelButtons[i].BackColor = Color.DarkSeaGreen;
                } else if (indexOfMainLevelMusic != -1 && indexOfBossFightMusic == -1)
                {
                    //this level has custom MainMusic but has no custom BossMusic
                    if (capitalizeLevelName == "\"Sheol\"" || capitalizeLevelName == "\"Tutorial\"")
                    {
                        //Sheol's final boss can't be changed, and there's no boss on Tutorial
                        LevelButtons[i].BackColor = Color.DarkSeaGreen;
                    }
                    else
                    {
                        LevelButtons[i].BackColor = Color.Transparent;
                    }
                    //LevelButtons[i].Image = ;
                }
                else if (indexOfMainLevelMusic == -1 && indexOfBossFightMusic != -1)
                {
                    //this level has custom BossMusic but has no custom MainMusic
                    LevelButtons[i].BackColor = Color.Transparent;
                } else
                {
                    //wtf? We have an entry for the level but don't have any info for main level or boss fight?
                    LevelButtons[i].BackColor = Color.RosyBrown;
                }

            }


            return firstLevelSupported;
        }


        private void enableOrganizerFields()
        {

            //the bankPaths will be told to not activate if MLNameBox isn't enabled
            if (MLNameBox.Enabled) return; //if we have our boxes already enabled, cancel this operation
            TextBox[] organizerTextBoxes = { MLNameBox, MLEventBox, MLLHBEBox, MLOffsetBox, MLBPMBox, BFNameBox, BFEventBox, BFLHBEBox, BFOffsetBox, BFBPMBox, };

            int selectedLevel = getSelectedLevel_OrganizerInjector(); //gives us our currently selected level

            for (int i = 0; i < organizerTextBoxes.Length; i++)
            {
                //if (selectedLevel == 7 && organizerTextBoxes[i].Name == "BFNameBox") return;
                organizerTextBoxes[i].Enabled = true;
            }
        }


        int currentListSelection = -1;
        bool blockListBoxSelIndexChng = false;
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (organizerBlockSongInfoReset) { organizerBlockSongInfoReset = false; return; }//we're resetting our ListBox order, don't change anything
            if (currentListSelection == listBox1.SelectedIndex) { return; } //we didn't change anything, block this


            if (blockListBoxSelIndexChng)
            {
                //blockListBoxSelIndexChng is only set to true if we had unsaved changes and we're wanting to go back to them
                blockListBoxSelIndexChng = false;
                ////testFindJson.Text += "BLOCKED!";
                return;
            }



            if (!Organizer_checkAndAlertUnsavedChanges(true, "selctedINdxChange"))
            {
                //when the user changed the selection in ListBox1, we saw there were unsaved changes, wherever we were
                //with checkAndAlertUnsavedChanges, we will decide to continue to go to the new selection, and checkAndAlert... will save the information, or not save it
                //if we do Cancel instead, this function returns "False" and we switch back to the already-selected index
                //blockListBoxSelIndexChng will stop this from running again when we hit Cancel, or X out the box

                //blockListBoxSelIndexChng = true; //I realized we can just see if we're changing the currentListSelection
                listBox1.SelectedIndex = currentListSelection;

                return;
            } else
            {
                currentListSelection = listBox1.SelectedIndex;
            }

            //if this far, we have confirmation to switch songs

            if (listBox1.SelectedIndex == -1)
            {
                currentListSelection = -1;
                organizer_enableLevelButtons(false);
                clearSongInfoBoxes();
                debugLabel.Text = "No song selected!";
                debugLabel.Visible = true;
                debugLabel.Left = 604;
                restoredLabel.Visible = false;
                organizer_restoreJson.Visible = false;
                resetSongOriginalInfo("");
                return;
            }

            resetDebugLabel(""); //resets the debug text if it said this
            org_openSongDir.Enabled = true;

            //whether the song is suspended or not, we want to allow the Restore Original .json button to appear if one exists in _Original folder
            organizer_restoreJson.Visible = false;//reset this either way
            restoredLabel.Visible = false;
            string slctdItmName = ((ListItem)listBox1.SelectedItem).Name;

            string slctdItmPath = ((ListItem)listBox1.SelectedItem).Path;
            string possibleOriginal = slctdItmPath.Replace("customsongs.json", "_Original\\customsongs.json"); //looks for, ie: DuHast/_Original/customsongs.json
            if (File.Exists(possibleOriginal))
            {
                organizer_restoreJson.Visible = true;
                restoredLabel.Visible = true;
            }
            string songsJsonDir = pathShortener(slctdItmPath, 40);
            songsJsonDir = songsJsonDir.Substring(0, 1).ToUpper() + songsJsonDir.Substring(1);
            organizer_songDirLbl.Text = songsJsonDir;

            mCopyLevelInfo.Enabled = true; //first set all these true
            mPasteLevelInfo.Enabled = true;
            bCopyLevelButton.Enabled = true;
            bPasteLevelInfo.Enabled = true;

            mDeleteLevelInfo.Enabled = false; //setting these to false because I DON'T KNOW WHY THEY KEEP BEING ENABLED
            bDeleteLevelInfo.Enabled = false;
            if (SongIsSuspended(listBox1.SelectedItem.ToString()))
            {
                org_modHasErrorsLbl.Visible = true;
                Org_OpenSongInDebug.Visible = true;

                mCopyLevelInfo.Enabled = false;
                mPasteLevelInfo.Enabled = false;
                bCopyLevelButton.Enabled = false;
                bPasteLevelInfo.Enabled = false;
                SetSelectedLevelColors(0);
                clearSongInfoBoxes();
                resetSongOriginalInfo("");

                organizer_enableLevelButtons(false);

                return;
            }

            //if we got this far, song isn't suspended. if the debug warning and button were visible, hide them
            if (org_modHasErrorsLbl.Visible || Org_OpenSongInDebug.Visible) { org_modHasErrorsLbl.Hide(); Org_OpenSongInDebug.Hide(); }


            enableOrganizerFields(); //enables the text boxes; does nothing if they're already enabled

            string songJsonInfo = Organizer_GetModJson();

            if (songJsonInfo == "-1") { MessageBox.Show("No customsongs.json found in directory"); return; }
            if (songJsonInfo == "-2")
            {
                setNoJsonYetInfo_organizer();
                return;
            }
            ////testFindJson.Text = songJsonInfo;
            string displayText = "";

            if (L1Settings.Enabled == false)
            {
                organizer_enableLevelButtons();
            }

            int levelToGoto = -1;
            int firstLevelSupportedIndex = setSupportedLevelColors(songJsonInfo);//sets level button colors, and gets first supported level

            if (org_selectIndexLevelChoice == "first")
            {
                levelToGoto = 0;
                if (tsm_showTutOrganizer.Checked)
                {
                    levelToGoto = 8;
                }
            } else if (org_selectIndexLevelChoice == "supported")
            {
                levelToGoto = firstLevelSupportedIndex;
            } else if (org_selectIndexLevelChoice == "none")
            {
                levelToGoto = getSelectedLevel_OrganizerInjector();
            }


            if (levelToGoto == -1) levelToGoto = 0; //set it to Voke if it screwed up somehow
            //firstLevelSupportedIndex = 0;
            //if(we have a setting that tells us to select the first level, not first supported level) firstLevelSupportedIndex = 0;
            SetSelectedLevelColors(levelToGoto);
            string firstShownLvlNm = allLevelNames[levelToGoto]; //gets name of level we're going to
            firstShownLvlNm = firstShownLvlNm.Substring(0, 1).ToUpper() + firstShownLvlNm.Substring(1); //capitalize said level
            setSpecificLevelInfo_Org(songJsonInfo, firstShownLvlNm); //resets values to have given level's info
            resetSongOriginalInfo("");
            currentListSelection = listBox1.SelectedIndex;









            /*DirectoryInfo[] songs = di.GetDirectories();
            for(int i=0; i < songs.Length; i++)
            {
                if()
            }*/

        }



        private void setNoJsonYetInfo_organizer()
        {
            debugLabel.Text = "A customsongs.json list has not been created yet. Make one using Set List page!";
            organizer_enableLevelButtons(false); //disable level selection buttons
            clearSongInfoBoxes(); //clear the textboxes of any info
            org_modHasErrorsLbl.Visible = false; Org_OpenSongInDebug.Visible = false; //if these were visible, reset them
            org_openSongDir.Enabled = false; organizer_songDirLbl.Text = "...\\Metal Hellsinger\\Metal_Data\\StreamingAssets\\customsongs.json not found.";

            mCopyLevelInfo.Enabled = false; mPasteLevelInfo.Enabled = false; mSaveLevelInfo.Enabled = false; mDeleteLevelInfo.Enabled = false;
            bCopyLevelButton.Enabled = false; bPasteLevelInfo.Enabled = false; bSaveLevelInfo.Enabled = false; bDeleteLevelInfo.Enabled = false;
        }

        private bool SongIsSuspended(string susSongName)
        {
            if (ConfirmSuspendedSongs.Count == 0) return false;

            foreach (string[] suspendedSong in ConfirmSuspendedSongs)
            {
                if (susSongName == suspendedSong[0])
                {
                    //the name being drawn matches a song that's been suspeneded
                    return true;
                }
                if (susSongName == "Current customsongs.json" && suspendedSong[0] == "(game)")
                {
                    //the name being drawn matches a song that's been suspeneded
                    return true;
                }
            }
            return false;
        }


        int[] debugOriginalLoc = { 604, 18 };
        public int[] randomJitter(int originalX, int originalY)
        {
            //i hate RNG...

            int XDir = 1;
            int YDir = 1;
            if (debugLabel.Left > debugOriginalLoc[0])
            {
                XDir = -1;
            }
            if (debugLabel.Top > debugOriginalLoc[1])
            {
                YDir = -1;
            }


            Random rnd = new Random();
            int randomNumX = rnd.Next(3);
            int randomDirectionX = XDir;
            int randomNumY = rnd.Next(2);
            int randomDirectionY = YDir;

            int[] resultingNums = new int[2];
            resultingNums[0] = randomNumX * randomDirectionX;
            resultingNums[1] = randomNumY * randomDirectionY;

            return resultingNums;

        }

        public void JitterBugMove()
        {
            int[] newLoc = randomJitter(debugOriginalLoc[0], debugOriginalLoc[1]);
            debugLabel.Left = debugOriginalLoc[0] + newLoc[0];
            debugLabel.Top = debugOriginalLoc[1] + newLoc[1];
        }
        int maxJitterbugs = 7;
        int currentJitterbugs = 0;
        public void JitterBug(object sender, EventArgs e)
        {
            JitterBugMove();
            currentJitterbugs++;
            if (currentJitterbugs >= maxJitterbugs)
            {
                Timer jitterTimer = sender as Timer;
                jitterTimer.Stop();
                jitterTimer.Dispose();
                currentJitterbugs = 0;
            }
        }

        private Timer timer1;
        public void AngryText()
        {
            timer1 = new Timer();
            timer1.Tick += new EventHandler(JitterBug);
            timer1.Interval = 50; // in miliseconds
            timer1.Start();
        }

        int slideInTime = 1; //finishes sliding in at one second
        int pauseTill = 3; //when it pauses till
        int slideOutTime = 1;
        int phase = 0;
        int slideInSpeed = 5;
        int slideOutSpeed = 8;
        int fadeOutSpeed = 20;
        int Phase2Ticking = 0;
        int curColor = 0;
        private void SlideInSlideOut(object sender, EventArgs e)
        {
            if (phase == 0)
            {
                successLabel.Top = 316 + 20;
                successLabel.Visible = true;
                Phase2Ticking = 0;
                curColor = 0;
                successLabel.ForeColor = Color.Black;

                if (successLabel.Text == "Success — Saved!")
                {
                    //saveCurrSLButton.Text = "Saved to Game";
                }
                saveCurrSLButton.Enabled = false;
                tabControl1.Focus();
                phase++;
                return;
            } else if (phase == 1)
            {
                if (successLabel.Top > 316)
                {
                    successLabel.Top -= slideInSpeed;
                }
                if (successLabel.Top <= 316)
                {
                    successLabel.Top = 316;
                    phase++;
                }
                return;
            } else if (phase == 2)
            {

                Phase2Ticking++;
                if (Phase2Ticking >= 70)
                {
                    saveCurrSLButton.Text = "Save Current Set List";
                    saveCurrSLButton.Enabled = true;
                    phase++;
                }
                return;
            }
            else if (phase == 3)
            {

                successLabel.Top -= slideOutSpeed;
                curColor += fadeOutSpeed;
                if (curColor >= 255)
                {
                    successLabel.Visible = false;

                    Timer slideTimer = sender as Timer;
                    slideTimer.Stop();
                    slideTimer.Dispose();
                    successLabel.Top = 316 + 40;
                    curColor = 0;
                    phase = 0;
                } else
                {
                    Color c = Color.FromArgb(255, curColor, curColor, curColor);
                    successLabel.ForeColor = c;
                    curColor += fadeOutSpeed;
                }

            }
        }
        private Timer bTimer;
        /// <summary>
        /// Sends out a label to capture the user's attention and their heart
        /// </summary>
        /// <param name="labelTxt"></param>
        private void Text_NotifyAnim(string labelTxt = "Success — Saved!")
        {
            successLabel.Text = labelTxt;
            bTimer = new Timer();
            bTimer.Tick += new EventHandler(SlideInSlideOut);
            bTimer.Interval = 20; // in miliseconds
            bTimer.Start();
        }

        private void SlideInSlideOutOrg(object sender, EventArgs e)
        {
            if (phase == 0)
            {
                restoredLabel.Top = 42;
                restoredLabel.Visible = true;
                //organizer_restoreJson.Enabled = false;
                organizer_restoreJson.Visible = false;
                Phase2Ticking = 0;
                curColor = 0;
                restoredLabel.ForeColor = Color.Black;
                phase++;
                return;
            }
            else if (phase == 1)
            {


                if (restoredLabel.Top > 22)
                {
                    restoredLabel.Top -= slideInSpeed;
                }
                if (restoredLabel.Top <= 42)
                {
                    restoredLabel.Top = 42;
                    phase++;
                }
                return;
            }
            else if (phase == 2)
            {

                Phase2Ticking++;
                if (Phase2Ticking >= 70)
                {
                    phase++;
                }
                return;
            }
            else if (phase == 3)
            {

                restoredLabel.Top -= slideOutSpeed;
                curColor += fadeOutSpeed;
                if (curColor >= 255)
                {
                    //restoredLabel.Visible = false;

                    Timer slideTimer = sender as Timer;
                    slideTimer.Stop();
                    slideTimer.Dispose();
                    restoredLabel.Top = 42;
                    curColor = 0;
                    phase = 0;
                    successLabel.Text = "Success — Saved!";
                }
                else
                {
                    Color c = Color.FromArgb(255, curColor, curColor, curColor);
                    restoredLabel.ForeColor = c;
                    curColor += fadeOutSpeed;
                }

            }

        }
        private Timer cTimer;
        private void Text_NotifyAnimOrg()
        {
            cTimer = new Timer();
            cTimer.Tick += new EventHandler(SlideInSlideOutOrg);
            cTimer.Interval = 20; // in miliseconds
            cTimer.Start();
        }

        /*
         * 
         * this is a code to shake the program's window
         * I wanna find a way to use it *o*
         * 
         
        private void shakeButton_Click(object sender, EventArgs e)
        {
            Shake(this);
        }

        private static void Shake(Form form)
        {
            var original = form.Location;
            var rnd = new Random(1337);
            const int shake_amplitude = 10;
            for (int i = 0; i < 10; i++)
            {
                form.Location = new Point(original.X + rnd.Next(-shake_amplitude, shake_amplitude), original.Y + rnd.Next(-shake_amplitude, shake_amplitude));
                System.Threading.Thread.Sleep(20);
            }
            form.Location = original;
        }
        */

        //The rest of this is not the code above. This code below is garbage that I need to delete
        /*
        private static System.Timers.Timer sTimer;
        private static void ActivateAngryTextLoop()
        {
            // Create a timer with a two second interval.
            sTimer = new System.Timers.Timer(20);
            // Hook up the Elapsed event for the timer. 
         //   sTimer.Elapsed += new System.ElapsedEventHandler(OnTimedEvent); well this was dumb
            sTimer.AutoReset = true;
            sTimer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            shakesNum = 4;
            ShakeLabelNew(debugLabel, debugLabel.Location);
        }

        Stopwatch stopWatch = new Stopwatch();
        Point shakerStartLoc = new Point();
        private void shakeLabelActivate()
        {
            shakerStartLoc = debugLabel.Location;
            stopWatch.Start();

            ActivateAngryTextLoop();
        }

        private void stopShaking()
        {
            stopWatch.Stop();
            stopWatch.Reset(); //pretty sure this stops it too
            sTimer.Enabled = false;
        }

        int totalShakeSeconds = 2;
        int shakePixelDistance = 5;
        int shakesNum = 0;
        private void ShakeLabelNew(Label lab, Point originalLoc)
        {
            //use stopwatch to stop after certain amount of time
            TimeSpan shakeTime = stopWatch.Elapsed;
            if (shakeTime.Seconds >= totalShakeSeconds)
            {
                stopShaking();
                return;
            }

            int xDirection = -1;
            int yDirection = -1;

            int elapsed = shakeTime.Milliseconds;
            if (elapsed == 0) elapsed = 1;
            float intensityPercentage = (totalShakeSeconds * 1000) / elapsed;
            double shakeXDubs = Math.Round((shakePixelDistance/2) * intensityPercentage * xDirection);
            int shakeX = Convert.ToInt32(shakeXDubs);
            if(Math.Abs(shakeX) < 1)
            {
                shakeX = xDirection;
            }
            
            //testFindJson.Text += "shake" + shakesNum;

            lab.Left += shakeX;

            if (shakesNum % 2 == 0) xDirection *= -1;

            
            shakesNum++;
        }

        private void ShakeLabel(Label lab)
        {
            //this was a dumb idea
            Point loc = lab.Location;
            int labYOg = lab.Top;
            int numberOfShakes = 10;
            int dramaticScaleX = 1;
            int dramaticScaleY = 1;
            int xDirection = -1;
            int yDirection = -1;
            Point ogLoc = loc;

            int shakesNum = 0;
            Timer timer = new Timer();
            while (shakesNum < numberOfShakes)
            {
                int xShake = numberOfShakes - shakesNum; //so if we have 10, and we already did one, this is now 9
                //testFindJson.Text += "shake" + shakesNum;

                lab.Left += xShake * xDirection;

                if (shakesNum % 2 == 0) xDirection *= -1;

                System.Threading.Thread.Sleep(10);
                shakesNum++;
            }

            lab.Location = loc; //put it back where ever it once was
        }*/

        Color LightGreenSelected = Color.FromArgb(255, 200, 255, 200); //used to be Color.DarkSeaGreen;
        Color LightRedSelected = Color.FromArgb(255, 255, 200, 200); //used to be Color.RosyBrown.... nevermind, these didn't work
        Color DarkGreenSelected = Color.FromArgb(255, 0, 40, 0);
        Color DarkRedSelected = Color.FromArgb(255, 40, 0, 0);
        Color NoColorDarkSelected = Color.MediumBlue;

        private void SetSelectedLevelColors(int selectedLevel)
        {
            //this is used by ORGANIZER
            Button[] LevelButtons = { L1Settings, L2Settings, L3Settings, L4Settings, L5Settings, L6Settings, L7Settings, L8Settings, L0Settings };

            //find the full level info
            //look for main music
            //look for boss music
            for (int i = 0; i < numberOfLevels; i++)
            {
                LevelButtons[i].ForeColor = Color.Black;
                if (LevelButtons[i].BackColor == DarkRedSelected)
                {

                    LevelButtons[i].BackColor = Color.RosyBrown;
                }
                if (LevelButtons[i].BackColor == DarkGreenSelected)
                {
                    LevelButtons[i].BackColor = Color.DarkSeaGreen;
                }
                if (LevelButtons[i].BackColor == NoColorDarkSelected)
                {
                    LevelButtons[i].BackColor = Color.Transparent;
                }
            }


            LevelButtons[selectedLevel].ForeColor = Color.White;
            if (LevelButtons[selectedLevel].BackColor == Color.RosyBrown)
            {
                LevelButtons[selectedLevel].BackColor = DarkRedSelected;
            }
            if (LevelButtons[selectedLevel].BackColor == Color.DarkSeaGreen)
            {
                LevelButtons[selectedLevel].BackColor = DarkGreenSelected;
            }
            if (LevelButtons[selectedLevel].BackColor == Color.Transparent)
            {
                LevelButtons[selectedLevel].BackColor = NoColorDarkSelected;
            }
        }



        //these were like the first buttons i programmed—shut up
        private void L1Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return; //this returns false if we don't want to cancelChanges

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(0);
            setSpecificLevelInfo_Org(songJsonInfo, "Voke");
            resetSongOriginalInfo("");
            //string fullSongJsonInfo = Organizer_GetModJson();
            //string fullInfo = getFullInfoForLevel(fullSongJsonInfo, "Voke");
            ////testFindJson.Text = "FullInfo: " + fullSongJsonInfo;
        }
        private void L2Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return;

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(1);
            setSpecificLevelInfo_Org(songJsonInfo, "Stygia");
            resetSongOriginalInfo("");
            //string fullSongJsonInfo = Organizer_GetModJson();
            //string fullInfo = getFullInfoForLevel(fullSongJsonInfo, "Stygia");
            ////testFindJson.Text = "FullInfo: " + fullSongJsonInfo;
        }
        private void L3Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return; //this returns false if we don't want to cancelChanges

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(2);
            setSpecificLevelInfo_Org(songJsonInfo, "Yhelm");
            resetSongOriginalInfo("");
        }
        private void L4Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return; //this returns false if we don't want to cancelChanges

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(3);
            setSpecificLevelInfo_Org(songJsonInfo, "Incaustis");
            resetSongOriginalInfo("");
        }
        private void L5Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return; //this returns false if we don't want to cancelChanges

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(4);
            setSpecificLevelInfo_Org(songJsonInfo, "Gehenna");
            resetSongOriginalInfo("");
        }
        private void L6Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return; //this returns false if we don't want to cancelChanges

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(5);
            setSpecificLevelInfo_Org(songJsonInfo, "Nihil");
            resetSongOriginalInfo("");
        }
        private void L7Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return; //this returns false if we don't want to cancelChanges

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(6);
            setSpecificLevelInfo_Org(songJsonInfo, "Acheron");
            resetSongOriginalInfo("");
        }
        private void L8Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return; //this returns false if we don't want to cancelChanges

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(7);
            setSpecificLevelInfo_Org(songJsonInfo, "Sheol");
            resetSongOriginalInfo("");
            //string fullSongJsonInfo = Injector_GetModJson();
            //string fullInfo = getFullInfoForLevel(fullSongJsonInfo, "Sheol");
            ////testFindJson.Text = fullInfo;
        }

        /* Started making dark mode, then gave up
        private void tabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabby = sender as TabControl;
            TabPage page = tabby.TabPages[0];
            e.Graphics.FillRectangle(new SolidBrush(page.BackColor), e.Bounds);

            Rectangle paddedBounds = e.Bounds;
            int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
            paddedBounds.Offset(1, yOffset);
            TextRenderer.DrawText(e.Graphics, page.Text, e.Font, paddedBounds, page.ForeColor);
        }

        private void ComboB_DrawItem(object sender, DrawItemEventArgs e)
        {
            ComboBox comboDrawn = sender as ComboBox;

            int index = e.Index >= 0 ? e.Index : 0;
            var brush = Brushes.Black;
            e.DrawBackground();
            e.Graphics.DrawString(comboDrawn.Items[index].ToString(), e.Font, brush, e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }*/

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void label23_Click(object sender, EventArgs e)
        {

        }




        //this is storing our location positions for each ModLvlButton
        int[] modMRadioPosX = { 286, 286, 286, 286, 565, 565, 565, 565, 286 };
        int[] modMRadioPosY = { 58, 132, 206, 280, 58, 132, 206, 301, 58 };
        int[] modBRadioPosX = { 286, 286, 286, 286, 565, 565, 565 };
        int[] modBRadioPosY = { 105, 178, 251, 324, 105, 175, 251 };
        int[] modMwTRadioPosY = { 107, 179, 252, 325, 58, 132, 206, 301, 58 }; //M with Tutorial
        int[] modBwTRadioPosY = { 154, 226, 299, 372, 105, 175, 251 }; //B with Tutorial
        int tutRadioButtonY = 19;

        private void setModLvlButtonColors(string supportedLevelsString, string m_or_b, bool resetImg = false)
        {
            //this is the supported-level indicator; this runs through the Radio Panel and enables/disables the Level radio buttons based on the Mod's supported levels
            //this doesn't actually set "colors", it just disables/enables the buttons

            RadioButton[] modLvlButtons = { VokeRadioButtonM1, StygiaRadioButtonM1, YhelmRadioButtonM1, IncaustisRadioButtonM1, GehennaRadioButtonM1, NihilRadioButtonM1, AcheronRadioButtonM1, SheolRadioButtonM1 };

            //first disable all the radio buttons
            for (int mB = 0; mB < modLvlButtons.Length; mB++)
            {
                modLvlButtons[mB].Enabled = false;

                //since we're running through all the radio buttons, if we want to reset their image, now's a good time to do so
                if (resetImg)
                {
                    modLvlButtons[mB].Image = null;
                }
            }

            //now we're going to see which buttons we want to enable

            int queriedLevel = -1;

            int L = 0;
            while (L < supportedLevelsString.Length)
            {
                string nextChar = supportedLevelsString.Substring(L, 1);

                //why can't switch statements use variables??


                if (!Int32.TryParse(nextChar, out int j))
                {
                    //this should activate if our nextChar wasn't a number
                    goto SupportLetter;
                }

                switch (nextChar)
                {
                    case "0":
                        queriedLevel = 0;
                        break;
                    case "1":
                        queriedLevel = 1;
                        break;
                    case "2":
                        queriedLevel = 2;
                        break;
                    case "3":
                        queriedLevel = 3;
                        break;
                    case "4":
                        queriedLevel = 4;
                        break;
                    case "5":
                        queriedLevel = 5;
                        break;
                    case "6":
                        queriedLevel = 6;
                        break;
                    case "7":
                        queriedLevel = 7;
                        break;


                }

            SupportLetter:


                if (m_or_b == "m" && nextChar == "m")
                {
                    modLvlButtons[queriedLevel].Enabled = true;
                } else if (m_or_b == "b" && nextChar == "b")
                {
                    modLvlButtons[queriedLevel].Enabled = true;
                }

                L++;
            }




        }


        private int getSelectedMod(string senderButtonName)
        {
            //this is used by SetList
            //returns the index with the directory name matching the text in the Level's MainMusic field; returns -1 if whatever we typed in can't be found
            //we need this because we need to know what the RadioButtonsPanel support colors need to reflect

            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8, mainCombo9 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };

            int modIndex = -1;

            //based on what button we pressed to trigger this (we press the level button, with the V next to combo boxes), we check that adjacent field
            string comboNumStr = senderButtonName.Substring(2, 1);
            int whichComboBox = Int32.Parse(comboNumStr);
            whichComboBox -= 1;
            if (senderButtonName.Substring(0, 2) == "ML")
            {

                modIndex = mainCBox[whichComboBox].FindStringExact(mainCBox[whichComboBox].Text);
                ////testFindJson.Text += "mainCBox[whichComboBox].Text: " + mainCBox[whichComboBox].Text;
            } else if (senderButtonName.Substring(0, 2) == "BF")
            {
                modIndex = bossCBox[whichComboBox].FindStringExact(bossCBox[whichComboBox].Text);
            }






            return modIndex;
        }



        private void OnModGrabLvlChangeText(object sender, EventArgs e)
        {
            if (mmLoading) return;
            ComboBox boxCalled = sender as ComboBox;

            disableGrabLvlBox(boxCalled);//this disables our button and makes it blank
            setSongSelectionArray(boxCalled, " "); //it can't be ""; we can set it to "blah" if we want, it just CANNOT match any more
        }

        private void disableGrabLvlBox(ComboBox cBox)
        {
            //this is meant to run if we ever start typing something in
            //we should only verify a selection once the user has clicked, or hit enter. Until then, we explicitly tell them that we haven't gotten a valid input yet
            //since whatever we're typing into will be a combo box, that's what we need to see is calling this

            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton, ML9ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };

            string cBoxName = cBox.Name;
            //ie mainCombo1
            string modLvlNumStr = cBoxName.Substring(cBoxName.Length - 1, 1);
            int whichLvl = Int32.Parse(modLvlNumStr);
            whichLvl -= 1;

            if (cBoxName.Substring(0, 1) == "m")
            {
                mainLvlGrabButton[whichLvl].Enabled = false;
                mainLvlGrabButton[whichLvl].Text = "";
                mainLvlGrabButton[whichLvl].Image = null;
                justCheckWithoutThinking(cBox.Name, false);
            } else if (cBoxName.Substring(0, 1) == "b")
            {
                bossLvlGrabButton[whichLvl].Enabled = false;
                bossLvlGrabButton[whichLvl].Text = "";
                bossLvlGrabButton[whichLvl].Image = null;
                justCheckWithoutThinking(cBox.Name, false);
            }

        }

        private void ML1ModLvlButton_Click(object sender, EventArgs e)
        {
            //this runs whenever any mod's GrabLvl button was clicked.

            //this function just sets the location of our RadioPanel, by finding out which button called it

            RadioButton[] rButtons = { VokeRadioButtonM1, StygiaRadioButtonM1, YhelmRadioButtonM1, IncaustisRadioButtonM1, GehennaRadioButtonM1, NihilRadioButtonM1, AcheronRadioButtonM1, SheolRadioButtonM1 };
            //reset all radio button selections to not be selected
            for (int r = 0; r < rButtons.Length; r++)
            {
                rButtons[r].Checked = false;
            }


            Button clickedLvlButton = sender as Button;
            ////testFindJson.Text += clickedLvlButton.Name + " was clicked; ";

            int xLocation = 0;
            int yLocation = 0;

            int selectedMod = getSelectedMod(clickedLvlButton.Name);
            if (selectedMod == -1) { ////testFindJson.Text += ">.<"; 
                return; } //getSelectedMod returned -1, meaning no mod is properly selected

            if (clickedLvlButton == null) //checking that we see something
                return;

            //see if we called a Main or Boss music button
            if (clickedLvlButton.Name.Substring(0, 2) == "ML")
            {
                string modLvlNumStr = clickedLvlButton.Name.Substring(2, 1);
                int whichLvl = Int32.Parse(modLvlNumStr);
                whichLvl -= 1;

                setModLvlButtonColors(csSupLvls[selectedMod], "m", true); //since we're bringing this up, we want to reset the Img for the button, hence "true"
                if (tsm_showTutSetList.Checked)
                {
                    //tutorial spot is visible
                    ML1RadioPanel.Location = new Point(modMRadioPosX[whichLvl], modMwTRadioPosY[whichLvl]);
                } else
                {
                    ML1RadioPanel.Location = new Point(modMRadioPosX[whichLvl], modMRadioPosY[whichLvl]);
                }
            } else if (clickedLvlButton.Name.Substring(0, 2) == "BF")
            {
                string modLvlNumStr = clickedLvlButton.Name.Substring(2, 1);
                int whichLvl = Int32.Parse(modLvlNumStr);
                whichLvl -= 1; //because our form starts at ML1

                setModLvlButtonColors(csSupLvls[selectedMod], "b", true);  //since we're bringing this up, we want to reset the Img for the button, hence "true"

                if (tsm_showTutSetList.Checked)
                {
                    //tutorial spot is visible
                    ML1RadioPanel.Location = new Point(modBRadioPosX[whichLvl], modBwTRadioPosY[whichLvl]);
                }
                else
                {
                    ML1RadioPanel.Location = new Point(modBRadioPosX[whichLvl], modBRadioPosY[whichLvl]);
                }

            }



            grabLvlSelectSwitched = false; //since we're bringing this up for the first time, we want to reset this boolean. 
                                           //Otherwise, if we just held Shift and selected a Level, we will have to hit shift twice to get it to work again

            if (ML1RadioPanel.Visible)
            {
                ML1RadioPanel.Visible = false;

                SetList_DebugLabel1.Visible = false;
                SetList_DebugLabel2.Visible = false;
                SetList_DebugLabel3.Visible = false;

                return;
            }

            if (ML1RadioPanel.Visible == false)
            {
                ML1RadioPanel.Visible = true;


                SetList_DebugLabel1.Text = "Click to select the level from which you'd like to take the song info in Mod's .json file.";
                SetList_DebugLabel2.Text = "Hold SHIFT to select the level's ";

                if (clickedLvlButton.Name.Substring(0, 2) == "ML")
                {
                    SetList_DebugLabel2.Text += "Boss";
                }
                else if (clickedLvlButton.Name.Substring(0, 2) == "BF")
                {
                    SetList_DebugLabel2.Text += "Main";
                }
                SetList_DebugLabel2.Text += " Music info instead.";


                SetList_DebugLabel1.Visible = true;
                SetList_DebugLabel2.Visible = true;
                if (csSupLvls[selectedMod].Contains("8m"))
                {
                    SetList_DebugLabel3.Text = "Press the T key to select the Tutorial level info";
                    SetList_DebugLabel3.Visible = true;
                }
            }
        }

        private void resetSetListDebugLabel(string onlyIfItSaysThis = "", int whichLabel = 0)
        {
            Label[] setListDebugLabels = { SetList_DebugLabel1, SetList_DebugLabel2, SetList_DebugLabel3 };
            if (onlyIfItSaysThis == "")
            {
                setListDebugLabels[whichLabel].Text = "";
            }
            if (onlyIfItSaysThis == "")
            {
                setListDebugLabels[whichLabel].Text = "";
            }

        }

        private void resetDebugLabel(string onlyIfItSaysThis = "")
        {
            if (onlyIfItSaysThis == "")
            {
                debugLabel.Visible = false;
                debugLabel.Left = 604;
            } else if (debugLabel.Text == onlyIfItSaysThis)
            {
                debugLabel.Visible = false;
                debugLabel.Text = "";
                debugLabel.Left = 604;
            }
        }

        private void justCheckWithoutThinking(string whatCalledUsName, bool isChecked = true)
        {
            CheckBox[] mainCheckBoxes = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8, checkm9 };
            CheckBox[] bossCheckBoxes = { checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };

            string whichLevelStr = whatCalledUsName.Substring(2, 1); //ML1, BF2
            if (whichLevelStr == "i" || whichLevelStr == "s")
            {
                whichLevelStr = whatCalledUsName.Substring(whatCalledUsName.Length - 1, 1);
            }
            int whichLevel = Int32.Parse(whichLevelStr);
            whichLevel -= 1;
            if (whatCalledUsName.Substring(0, 1).ToLower() == "m")
            {

                mainCheckBoxes[whichLevel].Checked = isChecked;
            } else if (whatCalledUsName.Substring(0, 1).ToLower() == "b")
            {

                bossCheckBoxes[whichLevel].Checked = isChecked;
            }
        }

        public void selectModTutorial()
        {
            if (SetList_DebugLabel3.Text != "Press the T key to select the Tutorial level info" || !SetList_DebugLabel3.Visible)
            {
                return;
            }

            //this gets called if we hit T and we had a tutorial available
            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton, ML9ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };

            ML1RadioPanel.Visible = false; //first, make the panel invisible
            SetList_DebugLabel1.Visible = false; //make the debug invisible too
            SetList_DebugLabel2.Visible = false;
            SetList_DebugLabel3.Visible = false;

            //we need to find out what this panel's location is. that tells us what level we're changing
            int xLocation = ML1RadioPanel.Location.X;
            int yLocation = ML1RadioPanel.Location.Y;

            for (int m = 0; m < modMRadioPosY.Length; m++)
            {
                if (tsm_showTutSetList.Checked)
                {
                    if (yLocation == tutRadioButtonY)
                    {
                        mainLvlGrabButton[m].Text = "T";
                        mainLvlGrabButton[m].Font = radioButton3.Font;
                        mainLvlGrabButton[m].Image = null;
                        justCheckWithoutThinking(mainLvlGrabButton[m].Name);
                        return;
                    }
                    else if (yLocation == modMwTRadioPosY[m])
                    {
                        if (xLocation == modMRadioPosX[m])
                        {
                            //we found the location!
                            mainLvlGrabButton[m].Text = "T";
                            mainLvlGrabButton[m].Font = radioButton3.Font;
                            mainLvlGrabButton[m].Image = null;
                            justCheckWithoutThinking(mainLvlGrabButton[m].Name);

                            ComboBox adjacentCombo = getComboFromGrabLvlBtn(mainLvlGrabButton[m]);
                            alertLevelIfModIntegrityComprimised(m, adjacentCombo);
                            return;
                        }

                    }
                }
                else if (yLocation == modMRadioPosY[m])
                {
                    //show Tutorial on set list isn't checked

                    if (xLocation == modMRadioPosX[m])
                    {
                        //we found the location!
                        mainLvlGrabButton[m].Text = "T";
                        mainLvlGrabButton[m].Font = radioButton3.Font;
                        mainLvlGrabButton[m].Image = null;
                        justCheckWithoutThinking(mainLvlGrabButton[m].Name);

                        ComboBox adjacentCombo = getComboFromGrabLvlBtn(mainLvlGrabButton[m]);
                        alertLevelIfModIntegrityComprimised(m, adjacentCombo);
                        return;
                    }

                }
            }

            for (int b = 0; b < modBRadioPosY.Length; b++)
            {
                if (tsm_showTutSetList.Checked)
                {

                    if (yLocation == modBwTRadioPosY[b])
                    {
                        if (xLocation == modBRadioPosX[b])
                        {
                            //we found the location!
                            bossLvlGrabButton[b].Text = "T";
                            bossLvlGrabButton[b].Font = radioButton3.Font;
                            bossLvlGrabButton[b].Image = null;
                            justCheckWithoutThinking(bossLvlGrabButton[b].Name);

                            ComboBox adjacentCombo = getComboFromGrabLvlBtn(bossLvlGrabButton[b]);
                            alertLevelIfModIntegrityComprimised(b, adjacentCombo);
                            return;
                        }

                    }
                }
                else if (yLocation == modBRadioPosY[b])
                {
                    //not showing tutorial
                    if (xLocation == modBRadioPosX[b])
                    {
                        //we found the location!
                        bossLvlGrabButton[b].Text = "T";
                        bossLvlGrabButton[b].Font = radioButton3.Font;
                        bossLvlGrabButton[b].Image = null;
                        justCheckWithoutThinking(bossLvlGrabButton[b].Name);

                        ComboBox adjacentCombo = getComboFromGrabLvlBtn(bossLvlGrabButton[b]);
                        alertLevelIfModIntegrityComprimised(b, adjacentCombo);
                        return;
                    }

                }
            }

        }

        public void selectModLevelRadio(object sender, EventArgs e)
        {
            //runs when we select which level we want to pull the mod's song info from

            //find out radio list is being used (what level we're changing the song for), and set it accordingly
            RadioButton clickedRadio = sender as RadioButton;
            ////testFindJson.Text += clickedRadio.Name + " was clicked; ";



            if (clickedRadio == null) // just to be on the safe side
                return;

            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton, ML9ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };

            ML1RadioPanel.Visible = false; //first, make the panel invisible
            SetList_DebugLabel1.Visible = false; //make the debug invisible too
            SetList_DebugLabel2.Visible = false;
            SetList_DebugLabel3.Visible = false;

            //we need to find out what this panel's location is. that tells us what level we're changing
            int xLocation = ML1RadioPanel.Location.X;
            int yLocation = ML1RadioPanel.Location.Y;

            for (int m = 0; m < modMRadioPosY.Length; m++)
            {
                if (tsm_showTutSetList.Checked)
                {
                    if (yLocation == tutRadioButtonY)
                    {
                        mainLvlGrabButton[m].Text = clickedRadio.Text;
                        mainLvlGrabButton[m].Font = clickedRadio.Font;
                        mainLvlGrabButton[m].Image = clickedRadio.Image;
                        justCheckWithoutThinking(mainLvlGrabButton[m].Name);
                        return;
                    } else if (yLocation == modMwTRadioPosY[m])
                    {
                        if (xLocation == modMRadioPosX[m])
                        {
                            //we found the location!
                            mainLvlGrabButton[m].Text = clickedRadio.Text;
                            mainLvlGrabButton[m].Image = clickedRadio.Image;
                            mainLvlGrabButton[m].Font = clickedRadio.Font;
                            justCheckWithoutThinking(mainLvlGrabButton[m].Name);

                            ComboBox adjacentCombo = getComboFromGrabLvlBtn(mainLvlGrabButton[m]);
                            alertLevelIfModIntegrityComprimised(m, adjacentCombo);
                            return;
                        }

                    }
                } else if (yLocation == modMRadioPosY[m])
                {
                    //show Tutorial on set list isn't checked

                    if (xLocation == modMRadioPosX[m])
                    {
                        //we found the location!
                        mainLvlGrabButton[m].Text = clickedRadio.Text;
                        mainLvlGrabButton[m].Image = clickedRadio.Image;
                        mainLvlGrabButton[m].Font = clickedRadio.Font;
                        justCheckWithoutThinking(mainLvlGrabButton[m].Name);

                        ComboBox adjacentCombo = getComboFromGrabLvlBtn(mainLvlGrabButton[m]);
                        alertLevelIfModIntegrityComprimised(m, adjacentCombo);
                        return;
                    }

                }
            }

            for (int b = 0; b < modBRadioPosY.Length; b++)
            {
                if (tsm_showTutSetList.Checked)
                {

                    if (yLocation == modBwTRadioPosY[b])
                    {
                        if (xLocation == modBRadioPosX[b])
                        {
                            //we found the location!
                            bossLvlGrabButton[b].Text = clickedRadio.Text;
                            bossLvlGrabButton[b].Image = clickedRadio.Image;
                            bossLvlGrabButton[b].Font = clickedRadio.Font;
                            justCheckWithoutThinking(bossLvlGrabButton[b].Name);

                            ComboBox adjacentCombo = getComboFromGrabLvlBtn(bossLvlGrabButton[b]);
                            alertLevelIfModIntegrityComprimised(b, adjacentCombo);
                            return;
                        }

                    }
                } else if (yLocation == modBRadioPosY[b])
                {
                    //not showing tutorial
                    if (xLocation == modBRadioPosX[b])
                    {
                        //we found the location!
                        bossLvlGrabButton[b].Text = clickedRadio.Text;
                        bossLvlGrabButton[b].Image = clickedRadio.Image;
                        bossLvlGrabButton[b].Font = clickedRadio.Font;
                        justCheckWithoutThinking(bossLvlGrabButton[b].Name);

                        ComboBox adjacentCombo = getComboFromGrabLvlBtn(bossLvlGrabButton[b]);
                        alertLevelIfModIntegrityComprimised(b, adjacentCombo);
                        return;
                    }

                }
            }


        }

        private void LevelTextBoxesGroup_MouseOver(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                //if (debugLabel.Text == "No song selected!" && debugLabel.Visible) ShakeLabel(debugLabel);
                debugLabel.Left = 604;
                debugLabel.Text = "No song selected!";
                debugLabel.Visible = true;
                AngryText();
            }
        }

        private void supportedLevelsGroup_MouseOver(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                //if (debugLabel.Text == "No song selected!" && debugLabel.Visible) ShakeLabel(debugLabel);
                debugLabel.Left = 604;
                debugLabel.Text = "No song selected!";
                debugLabel.Visible = true;
                AngryText();
            }
        }

        //this does nothing, but it's confusing me that it exists. All I did was double click something. why did i double click it. why don't i just delete this
        private void mainCombo1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void modGrabLvl_KeyDown(object sender, KeyEventArgs e)
        {
            if (grabLvlSelectSwitched) return;

            Button grabLvlButton = sender as Button;


            if ((Control.ModifierKeys & Keys.Shift) != 0)
            {
                //shift was pressed
                ////testFindJson.Text += "Shift";
                switchToMainOrBossSelection(grabLvlButton);
                grabLvlSelectSwitched = true;
            } else if (e.KeyCode == Keys.T)
            {
                selectModTutorial();

            }

        }
        private void modGrabLvl_KeyPress(object sender, KeyPressEventArgs e)
        {
            Button grabLvlButton = sender as Button;

            if ((Control.ModifierKeys & Keys.Shift) != 0)
            {
                //shift was pressed


                switchToMainOrBossSelection(grabLvlButton);
            }

        }

        private void modGrabLvl_KeyUp(object sender, KeyEventArgs e)
        {
            Button grabLvlButton = sender as Button;

            if ((Control.ModifierKeys & Keys.Shift) == 0)
            {
                //shift was released


                switchToMainOrBossSelection(grabLvlButton, true);
                grabLvlSelectSwitched = false;
            }
        }

        bool grabLvlSelectSwitched = false;
        private void switchToMainOrBossSelection(Button grabLvlButton, bool backToNorm = false)
        {
            //this assumes that Shift is being held
            //this only changes the background to have a B or an M. It doesn't have any idea nor cares what levels are supported by the mod

            if (grabLvlButton == null) return; //something immediately went wrong, bail

            RadioButton[] modLvlButtons = { VokeRadioButtonM1, StygiaRadioButtonM1, YhelmRadioButtonM1, IncaustisRadioButtonM1, GehennaRadioButtonM1, NihilRadioButtonM1, AcheronRadioButtonM1, SheolRadioButtonM1 };

            Image bImg = radioButton3.Image;
            Image mImg = radioButton5.Image;

            string glbID = grabLvlButton.Name.Substring(0, 1); //the name will be, for example, ML1ModLvlButton, ML2ModLvlButton, BF3ModLvlButton
            glbID = glbID.ToLower(); //we want this for setModLvlButtonColors

            //first, a for loop to change the backgrounds, to show user we're selecting the alternative choice
            for (int l = 0; l < modLvlButtons.Length; l++)
            {
                if (backToNorm) { modLvlButtons[l].Image = null; continue; } //if we're just changing the buttons back to normal, we don't care what button called us

                //see if we were being called by a Main music button or Boss music button
                if (glbID == "m")
                {
                    modLvlButtons[l].Image = bImg; //we're being called by a Main Music button, so the alternative should be the Boss Music
                }
                else if (glbID == "b")
                {
                    modLvlButtons[l].Image = mImg; //we're being called by a Boss Music button, so the alternative should be the Main Music
                }
            }


            //this is copied from the ML1...click code. I should just isolate it as its own function.
            //this code looks for the the mod we have selected next to the button we just pressed (grabLvlButton)
            //from there, it looks through its "supported levels" string, and enables/disables the buttons accordingly
            int selectedMod = getSelectedMod(grabLvlButton.Name);
            if (selectedMod == -1) { ////testFindJson.Text += ">.<"; 
                return; } //getSelectedMod returned -1, meaning no mod is properly selected; this shouldn't ever happen, but csSupLvls can't have a negative

            //check if we're changing the music for a Main or a Boss music selection; set the alternative level support accordingly
            if (glbID == "m")
            {
                //we clicked a Main music button to bring up this radio panel
                if (backToNorm)
                {
                    setModLvlButtonColors(csSupLvls[selectedMod], "m"); //we want it back to normal, so set this back to main music info
                } else
                {
                    setModLvlButtonColors(csSupLvls[selectedMod], "b"); //we don't want it back to normal, so get the alternative
                }

            } else if (glbID == "b")
            {
                //we clicked a Boss music button to bring up this radio panel
                if (backToNorm)
                {
                    setModLvlButtonColors(csSupLvls[selectedMod], "b"); //we want it back to normal, so set this back to boss music info
                }
                else
                {
                    setModLvlButtonColors(csSupLvls[selectedMod], "m"); //we don't want it back to normal, so get the alternative
                }
            }

        }

        private void loadOldInfoIntoSetList(bool clearExisting = true)
        {
            //This function uses setOldSongsArray's information to load the selections of our SetList, based on what was there before (based on what's in game's Json)
            //clearExisting will be set to false if we loaded our game folder AFTER the program loaded
            //the reason we want this: the user doesn't have to link the game folder to use the program. They can just let the program make a new JSON using
            //the Mods, then copy and paste everything if they want
            //If for some reason they decide to do link the game after they started making their set list, we don't want to wipe their decisions
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8, mainCombo9 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };
            CheckBox[] mainCheckBoxes = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8, checkm9 };
            CheckBox[] bossCheckBoxes = { checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };

            for (int m = 0; m < mainCBox.Length; m++)
            {
                if (!clearExisting)
                {
                    if (!mainCheckBoxes[m].Checked)
                    {
                        //if clearExisting is false, and the checkbox was checked, we don't fill it
                        //if clearExisting is true, we don't give a fuck if it's checked
                        mainCBox[m].Text = currentSetListName_m[m];
                        if (currentSetListIndexes_main[m] > -1) { getGrabLvlBtnFromCombo(mainCBox[m]).Enabled = true; } //when loading, if it's an actual mod, enable GrabLvlButton
                    }
                } else
                {
                    mainCBox[m].Text = currentSetListName_m[m];
                    if (currentSetListIndexes_main[m] > -1) { getGrabLvlBtnFromCombo(mainCBox[m]).Enabled = true; } //when loading, if it's an actual mod, enable GrabLvlButton
                }
            }
            for (int b = 0; b < bossCBox.Length; b++)
            {
                if (!clearExisting)
                {
                    if (!bossCheckBoxes[b].Checked)
                    {
                        bossCBox[b].Text = currentSetListName_b[b];
                        if (currentSetListIndexes_boss[b] > -1) { getGrabLvlBtnFromCombo(bossCBox[b]).Enabled = true; } //when loading, if it's an actual mod, enable GrabLvlButton
                    }
                } else
                {
                    bossCBox[b].Text = currentSetListName_b[b];
                    if (currentSetListIndexes_boss[b] > -1) { getGrabLvlBtnFromCombo(bossCBox[b]).Enabled = true; } //when loading, if it's an actual mod, enable GrabLvlButton
                }
            }

        }

        private void RevertOldInfoIntoSetList()
        {

            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8, mainCombo9 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };
            CheckBox[] mainCheckBoxes = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8, checkm9 };
            CheckBox[] bossCheckBoxes = { checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };

            for (int m = 0; m < mainCBox.Length; m++)
            {
                mainCBox[m].Text = currentSetListName_m[m];
                Button grbBtn = getGrabLvlBtnFromCombo(mainCBox[m]);
                grbBtn.Text = "";
                grbBtn.Image = null;

                if (currentSetListIndexes_main[m] > -1)
                {
                    grbBtn.Enabled = true;
                }
                else
                {
                    grbBtn.Enabled = false;
                }

                mainCheckBoxes[m].Checked = false;
            }

            for (int b = 0; b < bossCBox.Length; b++)
            {
                bossCBox[b].Text = currentSetListName_b[b];
                Button grbBtn = getGrabLvlBtnFromCombo(bossCBox[b]);
                grbBtn.Text = "";
                grbBtn.Image = null;

                if (currentSetListIndexes_boss[b] > -1)
                {
                    grbBtn.Enabled = true;
                }
                else
                {
                    grbBtn.Enabled = false;
                }

                bossCheckBoxes[b].Checked = false;
            }

        }

        private void ClearSetListToDefault()
        {

            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8, mainCombo9 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };
            CheckBox[] mainCheckBoxes = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8, checkm9 };
            CheckBox[] bossCheckBoxes = { checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };

            for (int m = 0; m < mainCBox.Length; m++)
            {
                mainCBox[m].Text = getDefaultSong(m, "m");
                Button grbBtn = getGrabLvlBtnFromCombo(mainCBox[m]);
                grbBtn.Text = "";
                grbBtn.Image = null;
                grbBtn.Enabled = false;
                if (currentSetListIndexes_main[m] == -1)
                {
                    //our old setlist was already default
                    mainCheckBoxes[m].Checked = false;
                }
                else
                {
                    //our old setlist wasn't default before
                    mainCheckBoxes[m].Checked = true;
                }


            }

            for (int b = 0; b < bossCBox.Length; b++)
            {

                bossCBox[b].Text = getDefaultSong(b, "b");
                Button grbBtn = getGrabLvlBtnFromCombo(bossCBox[b]);
                grbBtn.Text = "";
                grbBtn.Image = null;
                grbBtn.Enabled = false;
                if (currentSetListIndexes_boss[b] == -1)
                {
                    //our old setlist was already default
                    bossCheckBoxes[b].Checked = false;
                }
                else
                {
                    //our old setlist wasn't default before
                    bossCheckBoxes[b].Checked = true;
                }
            }

        }

        bool SetListHasUnsavedChanges = false;

        /// <summary>
        /// Runs through every song's checkmark and returns true if any are checked
        /// </summary>
        /// <returns></returns>
        private bool setListCheckForUnsavedChanges()
        {
            CheckBox[] allChx = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8, checkm9, checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };
            foreach (CheckBox chk in allChx)
            {
                if (chk.Checked)
                {
                    return true;
                }
            }
            return false;
        }


        //this function currently assumes you downloaded the LowHealth Library Bank, and have the original game's bank
        private void loadMusicBankList()
        {
            customMusicBankCombo.Items.Clear();//clear it if it had anything in it
            for (int i = 0; i < SongsWithCustomMusicBanks.Count; i++)
            {
                string nameToAdd = SongsWithCustomMusicBanks[i].Name;
                if (nameToAdd.Substring(nameToAdd.Length - 1, 1) == "s")
                {
                    nameToAdd += "'";
                } else
                {
                    nameToAdd += "'s";//this is technically correct, so this was pointless
                }
                if (SongsWithCustomMusicBanks[i].Name == "Game's Default") nameToAdd = SongsWithCustomMusicBanks[i].Name;
                nameToAdd += " .Bank";
                customMusicBankCombo.Items.Add(new ListItem { Name = nameToAdd, Path = SongsWithCustomMusicBanks[i].Path });
            }

            //we automatically select it if we see The Library exists
            if (customMusicBankCombo.Items.Count != 0 && customMusicBankCombo.Items[0].ToString() == "The Library's .Bank")
            {
                customMusicBankCombo.SelectedIndex = 0;
                customMusicBankCombo.Text = "The Library's .Bank";
                showGetLHLibrary(false);
            } else
            {
                showGetLHLibrary(true);
            }


            if (gameDir == null)
            {
                AddDefaultMusicBank();
                return;
            }
            if (!Directory.Exists(gameDir.ToString())) { return; }


            string musicBankpath = gameDir + "\\Music.bank";
            if (!File.Exists(musicBankpath)) { return; }

            /* We do this somewhere else now
            FileInfo musicBankFile = new System.IO.FileInfo(musicBankpath);
            long gamesCurrMusicBanksFiSz = musicBankFile.Length;

            if (customMusicBankCombo.Items.Count > 0 &&
                gamesCurrMusicBanksFiSz == gameMBFileSize &&
                customMusicBankCombo.Items[0].ToString() != "Game's Default .Bank" && (customMusicBankCombo.Items.Count > 1 && customMusicBankCombo.Items[1].ToString() != "Game's Default .Bank"))
            {
                //up until this moment, we didn't know we had the Game's Default bank. The user probably doesn't have a back up in Mod's folder.
                //if we overwrite this, we need to copy it to the ModsFolder
                if (customMusicBankCombo.Items[0].ToString() != "The Library's .Bank")
                {
                    customMusicBankCombo.Items.Insert(0, new ListItem { Name = "Game's Default .Bank", Path = musicBankpath });
                    customMusicBankCombo.SelectedIndex = 0;
                    customMusicBankCombo.Text = "Game's Default .Bank";
                } else
                {
                    customMusicBankCombo.Items.Insert(1, new ListItem { Name = "Game's Default .Bank", Path = musicBankpath });
                }
            }*/

            //after going this far, if we have NO items in musicBank, we can't leave the user stranded. So just add the game's current
            AddCurrentMusicBankIfComboEmpty();

        }

        /// <summary>
        /// Hides or reveals "Get Low Health Library" selection in Help menu
        /// </summary>
        /// <param name="show"></param>
        private void showGetLHLibrary(bool show = true)
        {
            tsm_getLHLibrary.Visible = show;
            getLHLibrarySeparator.Visible = show;
        }

        /// <summary>
        /// Adds "Game's Current .Bank" and disables Music.bank combo box if the combo box was empty
        /// </summary>
        private void AddCurrentMusicBankIfComboEmpty()
        {
            if (customMusicBankCombo.Items.Count == 0)
            {
                customMusicBankCombo.Items.Insert(0, new ListItem { Name = "Game's Current .Bank", Path = "(default)" });
                customMusicBankCombo.SelectedIndex = 0;
                customMusicBankCombo.Text = "Game's Current .Bank";
                customMusicBankCombo.Enabled = false;
            } else
            {
                customMusicBankCombo.Enabled = true; //we have something in here, enable it
                if (customMusicBankCombo.Items[0].ToString() != "Game's Default .Bank"
                    && customMusicBankCombo.Items[0].ToString() != "The Library's .Bank")
                {
                    //if we don't have 
                    //if we have The Library there, we're not going to add this because we want to persuade the user to use the Library one
                    customMusicBankCombo.Items.Insert(0, new ListItem { Name = "Game's Current .Bank", Path = "(default)" });
                    //we're not going to select it, though
                }

            }
        }

        /// <summary>
        /// Adds "Game's Default .Bank" and disables Music.bank combo box if the combo box was empty
        /// </summary>
        private void AddDefaultMusicBank()
        {
            customMusicBankCombo.Items.Insert(0, new ListItem { Name = "Game's Default .Bank", Path = "(default)" });
            customMusicBankCombo.SelectedIndex = 0;
            customMusicBankCombo.Text = "Game's Default .Bank";
            if (customMusicBankCombo.Items.Count == 1)
                customMusicBankCombo.Enabled = false;
            else
                customMusicBankCombo.Enabled = true;
        }

        private void musicSelectChangeTextDB(object sender, EventArgs e)
        {
            ComboBox cBox = sender as ComboBox;
            ////testFindJson.Text += " .changeDB. ";
            SetList_DebugLabel1.Visible = true;
            SetList_DebugLabel1.Text += "CAT";
            if (!wasComboBoxChanged(cBox)) return;
            setGrabLvlButton(cBox);

            string lvlNumStr = cBox.Name.Substring(cBox.Name.Length - 1, 1);
            int lvlNum = Int32.Parse(lvlNumStr); //gives 1-based index
            lvlNum -= 1;
            alertLevelIfModIntegrityComprimised(lvlNum, cBox);
        }

        private void SendSongsWithErrorsToBuggyD()
        {
            /*
            if(MandatoryDebugSongs.Count > 0)
            {
                mandatoryDebugDialog();
            } else
            {
                MessageBox.Show("There we no songs in MandatoryDebugSongs");
            }*/

        }

        private void FormLoad(object sender, EventArgs e)
        {
            //this is ran when the program first loads
            DisableAllInputs();

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "MetalManager.exe.config"))
            {
                ConfigCreator.CreateConfig();
            }
            GetDirectoriesFromConfig();
            //AlertDLCUntested();
        }


        //this doesn't work either
        bool textChanged = false;
        private void ComboTextChangeReset(object sender, EventArgs e)
        {
            if (mmLoading) return;
            textChanged = false;
        }

        private void ComboTextChanged(object sender, EventArgs e)
        {
            if (mmLoading) return;
            textChanged = true;
        }


        const int WM_PARENTNOTIFY = 0x210;
        const int WM_LBUTTONDOWN = 0x201;
        //I have no idea how this code works, but it makes our Radio Panel go away if the user clicks outside of it, and it does it well
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN || (m.Msg == WM_PARENTNOTIFY &&
                (int)m.WParam == WM_LBUTTONDOWN))
                if (!ML1RadioPanel.ClientRectangle.Contains(
                                 ML1RadioPanel.PointToClient(Cursor.Position)))
                {
                    ML1RadioPanel.Visible = false;
                    SetList_DebugLabel1.Visible = false; SetList_DebugLabel2.Visible = false; SetList_DebugLabel3.Visible = false;
                }
            base.WndProc(ref m);
        }


        /// <summary>
        /// Save Main/Boss Music button was clicked in Organizer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveLevelInfo_Click(object sender, EventArgs e)
        {
            Button saveButtonHit = sender as Button;
            string m_or_b = saveButtonHit.Name.Substring(0, 1);

            if (AttemptToSaveLevel_Organizer(m_or_b) == true)
            {
                //successful save! reset organizer page

                int dontChangeLevel = getSelectedLevel_OrganizerInjector(); //gives us whatever level Organizer is selected on for its song
                string dontChangeLevelName = allLevelNames[dontChangeLevel].Substring(0, 1).ToUpper() + allLevelNames[dontChangeLevel].Substring(1);

                clearSongInfoBoxes();
                organizer_enableLevelButtons(); //needs to be after we get the level, since this "resets" our buttons



                string songJsonInfo = Organizer_GetModJson();

                setSupportedLevelColors(songJsonInfo);
                SetSelectedLevelColors(dontChangeLevel);
                setSpecificLevelInfo_Org(songJsonInfo, dontChangeLevelName);
                resetSongOriginalInfo("");

                refreshAfterSaving();

            }

        }

        private bool AttemptToSaveLevel_Organizer(string m_or_b, bool alreadyConfirmed = false, int levelNum = -1)
        {
            if (Org_HasEmptyBoxes(m_or_b))
            {
                MessageBox.Show("Bank, Event, LowHealthBeatEvent, Offset, and BPM cannot be blank when saving info to customsongs.json.");
                return false;
            }

            TextBox[] mainLevelTextBoxes = { MLNameBox, MLEventBox, MLLHBEBox, MLOffsetBox, MLBPMBox };
            TextBox[] bossFightTextBoxes = { BFNameBox, BFEventBox, BFLHBEBox, BFOffsetBox, BFBPMBox };
            Label mBankPath = mTrueBankPath;
            Label bBankPath = bTrueBankPath;

            string bankText = "";
            string eventText = "";
            string lowHealthText = "";
            string offsetText = "";
            string bpmText = "";
            string bankPathText = "";

            if (m_or_b == "m")
            {
                bankText = MLNameBox.Text;
                eventText = MLEventBox.Text;
                lowHealthText = MLLHBEBox.Text;
                offsetText = MLOffsetBox.Text;
                bpmText = MLBPMBox.Text;
                bankPathText = mTrueBankPath.Text;
                //bankPathText = mBankPathLabel.Text;
            }
            else if (m_or_b == "b")
            {
                bankText = BFNameBox.Text;
                eventText = BFEventBox.Text;
                lowHealthText = BFLHBEBox.Text;
                offsetText = BFOffsetBox.Text;
                bpmText = BFBPMBox.Text;
                bankPathText = bTrueBankPath.Text;
                //bankPathText = bBankPathLabel.Text;
            }

            string modName = listBox1.Items[currentListSelection].ToString();

            if (modName == "Current customsongs.json")
            {
                modName = "game's current";
            }
            else
            {
                modName += "'s";
            }

            if (alreadyConfirmed) goto StartInjection;

            MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
            string title = "Confirm";
            string message = "Are you sure you want to make changes to " + modName + " customsongs.json?";
            if (!organizer_restoreJson.Visible && modName != "game's current")
            {
                message += "\n\nA backup of the original will be saved if you ever wish to restore it.";
            }
            else if (modName == "game's current")
            {
                message += "\n\nThis action cannot be undone.";
            }

            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result != DialogResult.OK)
            {
                return false;
            }


        //we got the go to Save info
        StartInjection:

            int levelWereSelecting = levelNum;
            if (levelWereSelecting == -1)
            {
                levelWereSelecting = getSelectedLevel_OrganizerInjector();
            }

            string LvlNameCapd = allLevelNames[levelWereSelecting].Substring(0, 1).ToUpper() + allLevelNames[levelWereSelecting].Substring(1).ToLower(); //voke->Voke



            string injection = getNewLevelInfoLines(LvlNameCapd, m_or_b, bankText, eventText, lowHealthText, offsetText, bpmText, bankPathText);


            if (SaveLevelInfo_Organizer(levelWereSelecting, m_or_b, injection) == true)
            {
                if (m_or_b == "m")
                {
                    mSaveLevelInfo.Enabled = false;
                } else
                {
                    bSaveLevelInfo.Enabled = false;
                }
                return true;
            } else
            {
                return false;
            }

        }

        private void BankPathTextboxUnfocus(object sender, EventArgs e)
        {
            TextBox textBoxCalled = sender as TextBox;
            if (blockBPUnfocusCall) { blockBPUnfocusCall = false; return; }
            closeBankPathAndApply(textBoxCalled);
        }

        bool blockBPUnfocusCall = false;

        private void BankPathTextboxOnKeydown(object sender, KeyEventArgs e)
        {
            TextBox textBoxCalled = sender as TextBox;

            if (e.KeyCode == Keys.Escape)
            {
                blockBPUnfocusCall = true;
                closeBankPathfuckgoback(textBoxCalled);
                //e.SuppressKeyPress = true;
                tabPage2.Focus();
                return;
            }
            if (e.KeyCode == Keys.Tab)
            {
                closeBankPathAndApply(textBoxCalled);
                tabPage2.Focus();
                //e.SuppressKeyPress = true;
                return;
            }
            if (e.KeyCode == Keys.Return)
            {
                closeBankPathAndApply(textBoxCalled);
                e.SuppressKeyPress = true;
                tabPage2.Focus();
                return;
            }
        }



        private void closeBankPathfuckgoback(TextBox whichBankPathBox)
        {
            //this just runs when we hit Escape, so we didn't want to change anything
            if (debugLabel.Text.Contains("It is a bad idea"))
                debugLabel.Text = "";//we'll reset this

            if (whichBankPathBox.Name.Substring(0, 1) == "m")
            {
                mBankPathTextbox.Visible = false;
                if (mTrueBankPath.Text == "")
                {
                    //the box had nothing beforehand
                    mBankPathLabel.Text = "";
                    mTrueBankPath.Text = "";
                    return;
                }

                string DisplayPath = mTrueBankPath.Text.ToString();
                if (DisplayPath.Contains("\\\\"))
                {
                    //I don't think I need this if statement
                    DisplayPath = DisplayPath.Replace("\\\\", "\\");
                }
                DisplayPath = pathShortener(DisplayPath, 40);
                DisplayPath = shaveSurroundingQuotesAndSpaces(DisplayPath); //this needs to be before we add "bankPath":
                DisplayPath = "bankPath: " + DisplayPath;

                mBankPathLabel.Text = DisplayPath;
                if (!verifyFileExists(mTrueBankPath.Text))
                {
                    bankPathRedAlert(mBankPathLabel);
                } else
                {

                    warnUserIfBadBankPath(mBankPathLabel);
                }
            } else if (whichBankPathBox.Name.Substring(0, 1) == "b")
            {
                bBankPathTextbox.Visible = false;
                if (bTrueBankPath.Text == "")
                {
                    //box had nothing to begin with, pretty sure I don't need this
                    bBankPathLabel.Text = "";
                    bTrueBankPath.Text = "";
                    return;
                }

                string DisplayPath = bTrueBankPath.Text.ToString();
                if (DisplayPath.Contains("\\\\"))
                {
                    DisplayPath = DisplayPath.Replace("\\\\", "\\");
                }
                DisplayPath = pathShortener(DisplayPath, 40);
                DisplayPath = shaveSurroundingQuotesAndSpaces(DisplayPath); //this needs to be before we add "bankPath":
                DisplayPath = "bankPath: " + DisplayPath;

                bBankPathLabel.Text = DisplayPath;
                if (!verifyFileExists(bTrueBankPath.Text))
                {
                    bankPathRedAlert(bBankPathLabel);
                } else
                {
                    warnUserIfBadBankPath(bBankPathLabel);
                }
            }
            tabPage2.Focus();
        }

        /// <summary>
        /// Warns the user that we don't a bankPath for anything except Current customsongs.json
        /// </summary>
        private void warnUserIfBadBankPath(Label whichLabel)
        {
            if (listBox1.SelectedItem.ToString() != "Current customsongs.json")
            {
                bankPathRedAlert(whichLabel, 2);
            }
        }


        private void bankPathRedAlert(Label bankPathLabel, int message = 0)
        {
            bankPathLabel.BackColor = Color.RosyBrown;

            string whichMusic = "";

            if (bankPathLabel.Name.Substring(0, 1) == "m")
                whichMusic = "under Main Music ";
            else
                whichMusic = "under Boss Music ";



            switch (message)
            {
                case 0:
                    debugLabel.Text = "The file in bankPath " + whichMusic + "doesn't exist.";
                    debugLabel.Left = 604;
                    break;
                case 1:
                    debugLabel.Text = "bankPath needs to point to Bank, not just directory.";
                    debugLabel.Left = 604;
                    break;
                case 2:
                    debugLabel.Text = "It is a bad idea to put a bankPath in any customsongs.json besides the game's.";
                    debugLabel.Left = 540;
                    break;
            }

            debugLabel.Visible = true;
        }

        private bool verifyFileExists(string filePath)
        {
            string correctedPath = filePath.ToString();
            if (correctedPath.Contains("\\\\"))
            {
                correctedPath = correctedPath.Replace("\\\\", "\\");
            }
            correctedPath = shaveSurroundingQuotesAndSpaces(correctedPath);
            //all that was just in case, but pretty sure we didn't need it
            ////testFindJson.Text += "<<"+correctedPath +">>";

            if (File.Exists(correctedPath))
            {
                return true;
            }

            return false;
        }




        private void closeBankPathAndApply(TextBox whichBankPathBox)
        {
            //this runs when we unfocus, which also happens when we hit enter
            //we can verify the file exists, or we can let the user doom himself
            //......i'm tired....
            //I did it anyways!

            debugLabel.Text = "";//we'll reset this, regardless if we're going to add something to it

            string DisplayPath = whichBankPathBox.Text.ToString();
            if (DisplayPath.Contains("\\\\"))
            {
                DisplayPath = DisplayPath.Replace("\\\\", "\\");
            }
            DisplayPath = pathShortener(DisplayPath, 40);
            DisplayPath = shaveSurroundingQuotesAndSpaces(DisplayPath); //this needs to be before we add "bankPath":
            DisplayPath = "bankPath: " + DisplayPath;


            if (whichBankPathBox.Name.Substring(0, 1) == "m")
            {

                mBankPathTextbox.Visible = false;

                if (mBankPathTextbox.Text == "")
                {
                    //we put nothing in the box, as if we don't want this field; take away the info, and the label, so user doesn't think we need it
                    mBankPathLabel.Text = "";
                    mTrueBankPath.Text = "";
                    return;
                }
                mTrueBankPath.Text = shaveSurroundingQuotesAndSpaces(mBankPathTextbox.Text);
                mBankPathLabel.Text = DisplayPath;
                if (!verifyFileExists(mTrueBankPath.Text))
                {
                    bankPathRedAlert(mBankPathLabel);
                } else
                {
                    warnUserIfBadBankPath(mBankPathLabel);
                }
            } else if (whichBankPathBox.Name.Substring(0, 1) == "b")
            {
                bBankPathTextbox.Visible = false;
                if (bBankPathTextbox.Text == "")
                {
                    //we put nothing in the box, as if we don't want this field; take away the info, and the label
                    bBankPathLabel.Text = "";
                    bTrueBankPath.Text = "";
                    return;
                }
                bTrueBankPath.Text = shaveSurroundingQuotesAndSpaces(bBankPathTextbox.Text);
                bBankPathLabel.Text = DisplayPath;
                if (!verifyFileExists(bTrueBankPath.Text))
                {
                    bankPathRedAlert(bBankPathLabel);
                }
                else
                {
                    warnUserIfBadBankPath(bBankPathLabel);
                }
            }
        }

        private void openBankPathTextbox(Label labelDoubleclicked)
        {
            if (!MLNameBox.Enabled) return; //if MLNameBox isn't enabled, none of the textboxes are; meaning we don't have anything selected yet, don't allow this

            string m_or_b = labelDoubleclicked.Name.Substring(0, 1);
            if (m_or_b == "m")
            {
                mBankPathTextbox.Visible = true;
                mBankPathTextbox.Text = shaveSurroundingQuotesAndSpaces(mTrueBankPath.Text);
                mBankPathTextbox.Focus();
                mBankPathLabel.BackColor = Color.Transparent;
            } else if (m_or_b == "b")
            {
                if (!BFNameBox.Enabled) return; //we only checked MainLevel's info being enabled, cancel if we wanted to change Boss info and it's not enabled

                bBankPathTextbox.Visible = true;
                bBankPathTextbox.Text = shaveSurroundingQuotesAndSpaces(bTrueBankPath.Text);
                bBankPathTextbox.Focus();
                bBankPathLabel.BackColor = Color.Transparent;
            }
            debugLabel.Visible = true;
            debugLabel.Left = 604;
            debugLabel.Text = "Enter/Return to apply, ESC to cancel";
            labelDoubleclicked.Text = "bankPath: ";
        }

        string clipboardBeforeClickingBankpath = "";
        private void BankPathFirstClick(object sender, EventArgs e)
        {
            Label labelCalled = sender as Label;
            clipboardBeforeClickingBankpath = Clipboard.GetText();
        }

        private void BankPathDblClick(object sender, EventArgs e)
        {
            Label labelCalled = sender as Label;
            openBankPathTextbox(labelCalled);

            try
            {
                Clipboard.SetText(clipboardBeforeClickingBankpath);
            }
            catch
            {
                //I have no way of stopping the labels from being copied, unless we had copied text 
                //(it'll delete any images the user copied from clipboard on Label double click)
                return;
            }
        }




        private void fixMyShit(object sender, EventArgs e)
        {
            /* this is something that has nothing to do with this program. When using FMOD to export GUID, I get this:
             * {c4cfdf28-5839-4144-8a1b-663f2d4d1b3b} event:/LowHealth83BPM
             * as a line. I want this:
             * 83BPM "{c4cfdf28-5839-4144-8a1b-663f2d4d1b3b}"
             * and I don't want to rewrite everything and end up fucking something up
             * */


            string clipboardText = Clipboard.GetText();
            string returnString = "";
            string[] lines = clipboardText.Split('\n');
            string[] fixedlines = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                string theLine = lines[i];
                int indexOfLowHealth = theLine.IndexOf("LowHealth");
                int indexOfBPM = theLine.IndexOf("BPM");
                int indexOfSpaceEvent = theLine.IndexOf(" event");
                if (indexOfLowHealth == -1 || indexOfSpaceEvent == -1 || indexOfBPM == -1) { fixedlines[i] = ""; continue; }
                int length = indexOfBPM + 3 - indexOfLowHealth;
                string returnLine = "";
                if (theLine.Contains("LowHealth1") || theLine.Contains("LowHealth2"))
                {
                    //we have triple-digit low Health (LowHealth100+, or LowHealth200+)
                    returnLine = theLine.Substring(indexOfLowHealth, length) + "  \"" + theLine.Substring(0, indexOfSpaceEvent) + "\"";
                }
                else
                {
                    //we have double-digit LowHealth
                    returnLine = theLine.Substring(indexOfLowHealth, length) + "   \"" + theLine.Substring(0, indexOfSpaceEvent) + "\"";
                }

                fixedlines[i] = returnLine;
            }
            foreach (string line in fixedlines)
            {
                returnString += line + "\n";
            }
            Clipboard.SetText(returnString);

            //this took far longer than it would have to copy and paste...
        }

        private void showCsSupports()
        {
            string msg = "";
            foreach (string supportString in csSupLvls)
            {
                msg += supportString + '\n';
            }
            MessageBox.Show(msg);
        }

        private void testBuddyClick(object sender, EventArgs e)
        {
            showCsSupports();
        }

        private void tabRemoval(object sender, EventArgs e)
        {
            string fullMod = Injector_GetModJson();
            string fixedMod = fixAnyLinesWithTabs(fullMod);
            Clipboard.SetText(fixedMod);

        }

        /*
        private void debugTest1()
        {
            string initialInfo = testFindJson.Text;
            string[] errors = debuggy(initialInfo);
            ////testFindJson.Text = "";
            foreach (string errorCode in errors)
            {
                ////testFindJson.Text += errorCode + "\n";
            }

        }*/

        List<string[]> MandatoryDebugSongs;
        public string[][] SongsRequireMandatoryDebug
        {
            get { return MandatoryDebugSongs.ToArray(); }
        }
        public string[][] PullSongsFromConfig
        {
            get { return MandatoryDebugSongs.ToArray(); }
        }

        public string TesterBoxValue
        {

            get { return testFindJson.Text; }
        }


        /// <summary>
        /// Called when the user opens Debugger on his own from Tools>>Debug
        /// </summary>
        private void OpenDebug()
        {

            using (DebugForm debugger = new DebugForm("user"))
            {
                debugger.Icon = this.Icon;
                debugger.MyParentForm = this;
                debugger.StartPosition = FormStartPosition.CenterParent;
                if (debugger.ShowDialog() == DialogResult.OK)
                {
                    if (forceRecheckClicked)
                    {
                        ForceRecheckAllMods();
                        return;
                    }


                    string cleanJson = debugger.CleanedJson;
                    string jsonSongPath = debugger.DebuggedSongPath;
                    if (cleanJson != null)
                    {
                        SaveJsonFromDebug(cleanJson, jsonSongPath);
                        RepeatStartup();
                    }
                }
            }
        }

        /// <summary>
        /// Called when we hit the "Open in Debugger" button in Organizer
        /// </summary>
        private void SendSongToDebug()
        {
            string sName = ((ListItem)listBox1.SelectedItem).Name;
            string sPath = ((ListItem)listBox1.SelectedItem).Path;

            using (DebugForm debugger = new DebugForm("song:" + sName + "|" + sPath))
            {
                debugger.Icon = this.Icon;
                debugger.MyParentForm = this;
                debugger.StartPosition = FormStartPosition.CenterParent;
                if (debugger.ShowDialog() == DialogResult.OK)
                {
                    string cleanJson = debugger.CleanedJson;
                    if (cleanJson != null)
                    {
                        SaveJsonFromDebug(cleanJson, sPath);
                        RepeatStartup();
                        refreshAfterSaving();

                        clearSongInfoBoxes();
                        organizer_enableLevelButtons();
                        enableOrganizerFields();


                        string songJsonInfo = cleanJson;


                        int levelToGoto = -1;
                        int firstLevelSupportedIndex = setSupportedLevelColors(songJsonInfo);//sets level button colors, and gets first supported level

                        if (org_selectIndexLevelChoice == "first")
                        {
                            levelToGoto = 0;
                            if (tsm_showTutOrganizer.Checked)
                            {
                                levelToGoto = 8;
                            }
                        }
                        else if (org_selectIndexLevelChoice == "supported")
                        {
                            levelToGoto = firstLevelSupportedIndex;
                        }
                        else if (org_selectIndexLevelChoice == "none")
                        {
                            levelToGoto = 0; //not selected to anything, select first level
                        }

                        if (levelToGoto == -1) levelToGoto = 0; //set it to Voke if it screwed up somehow


                        SetSelectedLevelColors(levelToGoto);

                        string firstShownLvlNm = allLevelNames[levelToGoto]; //gets name of level we're going to
                        firstShownLvlNm = capFirst(firstShownLvlNm); //capitalize said level
                        setSpecificLevelInfo_Org(songJsonInfo, firstShownLvlNm); //resets values to have given level's info

                        resetSongOriginalInfo("");
                        org_modHasErrorsLbl.Visible = false;
                        Org_OpenSongInDebug.Visible = false;

                        int slctIndxAgain = listBox1.FindStringExact(sName);
                        listBox1.SelectedIndex = slctIndxAgain;
                        currentListSelection = listBox1.SelectedIndex;

                    }
                }
            }
        }


        private void askToSendAttemptedSaveToDebug(string attmptdSave)
        {
            string message = "Operation was cancelled because the JSON contained errors.\nGo to File > Reload Mods List to force a scan of possible errors in custom songs." +
                "\n\nWould you like to send the attempted save to the Debug panel?";
            string title = "That's a whoopsie!";
            MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result != DialogResult.Yes)
            {
                return;
            }

            SendAttemptedSaveToDebug(attmptdSave);
        }


        /// <summary>
        /// Called when we hit an error during Save or Delete in Organizer
        /// </summary>
        private void SendAttemptedSaveToDebug(string attemptedJson)
        {


            using (DebugForm debugger = new DebugForm("(.saveAttmpt.)|" + attemptedJson))
            {
                debugger.MyParentForm = this;
                debugger.StartPosition = FormStartPosition.CenterParent;
                debugger.ShowDialog();
                /* We don't need this, right..?
                if (ebugger.ShowDialog(); == DialogResult.OK)
                {
                    
                }*/
            }
        }



        private void mandatoryDebugDialog()
        {
            using (DebugForm debugger = new DebugForm("mandatory"))
            {
                debugger.MyParentForm = this;
                debugger.StartPosition = FormStartPosition.CenterParent;
                if (debugger.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Got an OK from Debugger");
                } else
                {
                    MessageBox.Show("Got an NO from Debugger");
                }
            }
        }

        private void debugButtonDialogueBox()
        {
            //the hell is this??
            using (DebugForm debugger = new DebugForm("nah"))
            {
                debugger.Icon = this.Icon;
                debugger.MyParentForm = this;
                debugger.StartPosition = FormStartPosition.CenterParent;
                if (debugger.ShowDialog() == DialogResult.OK)
                {
                    //////testFindJson.Text = debugger.TheValue; I don't think we need this
                }
            }
        }

        public ListItem[] getSongsList()
        {
            List<ListItem> songsList = new List<ListItem>();

            foreach (ListItem song in listBox1.Items)
            {
                songsList.Add(song);
            }

            return songsList.ToArray();

        }

        private void debugButtonClick(object sender, EventArgs e)
        {
            debugButtonDialogueBox();
            //debugTest1();
        }

        private void SetGameDir(object sender, EventArgs e)
        {
            GetGameDir();
        }
        private void SetModDir(object sender, EventArgs e)
        {
            GetModsFolder();
        }
        private void ChangeModDir(object sender, EventArgs e)
        {
            //Same as set mod dir, but we warn the user that we're about to reset the config file
            string message = "Changing the Mod directory will clear several settings,\n" +
                "such as any Sorting changes you made to Catalog.\n" +
                "This action cannot be undone.\n\n" +
                "Do you wish to continue?";
            string title = "Warning!";
            MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result != DialogResult.Yes)
            {
                return;
            }

            GetModsFolder();
            RepeatStartup();
        }

        private void OpenGameDir(object sender, EventArgs e)
        {
            OpenDir(gameDir.ToString());
        }
        private void OpenModDir(object sender, EventArgs e)
        {
            OpenDir(di.ToString());
        }

        /// <summary>
        /// Disables the form. Used as we're initially loading Metal Manager
        /// </summary>
        private void DisableAllInputs(bool enabled = false)
        {
            /*
            CheckBox[] mainCheckBoxes = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8, checkm9 };
            CheckBox[] bossCheckBoxes = { checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8, mainCombo9 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };
            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton, ML9ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };

            //i = input
            foreach(CheckBox i in mainCheckBoxes)
            {
                i.Enabled = enabled;
            }
            foreach (CheckBox i in bossCheckBoxes)
            {
                i.Enabled = enabled;
            }

            foreach (ComboBox i in mainCBox)
            {
                i.Enabled = enabled;
            }
            foreach (ComboBox i in bossCBox)
            {
                i.Enabled = enabled;
            }

            foreach (Button i in mainLvlGrabButton)
            {
                i.Enabled = enabled;
            }
            foreach (Button i in bossLvlGrabButton)
            {
                i.Enabled = enabled;
            }*/

            tabControl1.Enabled = enabled;
        }


        /// <summary>
        /// Parsing Numbers does not work for all users, so instead, this strips down the value of a number to make sure nothing 
        /// exists after verifying one dot, one dash, and removing all numbers
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool IsValueANumber(string val)
        {
            string newVal = val.Trim();
            int dotCount = newVal.Split('.').Length - 1;
            if (dotCount > 1) return false;

            //check for negative numbers, and only one dash
            if (newVal.Substring(0, 1) == "-") newVal = newVal.Substring(1);
            if (newVal.Contains("-")) return false;

            newVal = newVal.Replace(".", "");
            newVal = newVal.Replace("0", "").Replace("1", "").Replace("2", "").Replace("3", "").Replace("4", "").Replace("5", "").Replace("6", "").Replace("7", "").Replace("8", "").Replace("9", "");
            if (string.IsNullOrWhiteSpace(newVal))
            {
                return true;
            } else
            {
                return false;
            }
        }


        /// <summary>
        /// Enables Tabs, and thus all checkboxes/combo boxes
        /// </summary>
        private void TurnOnTheLights(bool lightsOn = true)
        {
            tabControl1.Enabled = lightsOn;

            if (gameDir == null)
            {
                saveCurrSLButton.Enabled = false;
                reApplyAllBanksBtn.Enabled = false;
                cleanUpSABtn.Enabled = false;
            }

            /* I didn't have to do all this....
            CheckBox[] mainCheckBoxes = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8, checkm9 };
            CheckBox[] bossCheckBoxes = { checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8, mainCombo9 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };

            //i = input
            foreach (CheckBox i in mainCheckBoxes)
            {
                i.Enabled = lightsOn;
            }
            foreach (CheckBox i in bossCheckBoxes)
            {
                i.Enabled = lightsOn;
            }

            foreach (ComboBox i in mainCBox)
            {
                i.Enabled = lightsOn;
            }
            foreach (ComboBox i in bossCBox)
            {
                i.Enabled = lightsOn;
            }
            MessageBox.Show("Lights on");*/
        }

        bool mmLoading = true; //metal manager is loading
        private void FormShown(object sender, EventArgs e)
        {
            //BfGWorkerMain.RunWorkerAsync(); 

            
            OpenErrorGatekeeperDialogue();
            //SendSongsWithErrorsToBuggyD(); no longer require errors fixed

            SetMMSettingsFromConfig();
            setFileMenuSelections();//sets our file menu selections, based on if we have game and mod directory set

            setList_topLabel.Visible = true;
            //setList_topLabel.Text = "Reading Mod Folder...";
            string[] modList = loadModListFromConfig(setListCatalog, false); //this loads the contents into the setListCatalog ListBox

            AddOrRemoveNoSongsFound(modList);

            //resets the list alphabetically if setting has been set
            if (catalogsort == "a-z" || catalogsort == "z-a")
            {
                var list = setListCatalog.Items.Cast<ListItem>().OrderBy(item => item.Name).ToList();
                if(catalogsort == "z-a") { list.Reverse(); }

                setListCatalog.Items.Clear();
                foreach (ListItem listItem in list)
                {
                    setListCatalog.Items.Add(listItem);
                }
            }


            if (modList == null)
            {
                //modList will be null a mod folder isn't set
                setList_topLabel.Visible = false;
                SetList_DebugLabel1.Text = "No Mods folder found";
                return;
            } //if modList is null, it will never enable the Combo or Checkboxes
            placeCurrCSjsonInOrgnzr();
            resetOtherListbox("setListCatalog");

            //setList_topLabel.Text = "Reading song selections...";//really we're setting song selections
            fillSongSelection(modList);
            //setList_topLabel.Text = "Reading Mod Folder...";
            storeModListInfo(); //this stores the info for our song; specifically which levels it supports; the info is hidden and is used to quickly know what levels each mod has info for
            //setList_topLabel.Text = "Reading current selections...";
            
            setOldSongsArray(); //this stores an array of info of our current customsongs.json file in the game folder
            //setList_topLabel.Text = "Setting selections to current...";
            loadOldInfoIntoSetList(); //this loads the array from the previous line into the fields

            GetAllCustomMusicBanks();
            loadMusicBankList();
            setList_topLabel.Visible = false;
            setSongSelectionArray();
            TurnOnTheLights();//enable the form's selectables

            mmLoading = false;
        }

        private void AddOrRemoveNoSongsFound(string[] modList)
        {
            if (modList.Length == 0 && numberOfModsWithErrors == 0)
            {
                //we had no songs to fill for Catalogs
                //modList won't have songs with errors
                sL_noCSFoundPanel.Visible = true;

                //still might have the game's customsongs.json selection in organizer
                if (!currentJsonExists())
                    org_noCSFoundPanel.Visible = true;
            }
            else
            {
                //we had SOMETHING to fill into Catalogs
                sL_noCSFoundPanel.Visible = false;
                org_noCSFoundPanel.Visible = false;
            }
        }

        private bool gameJsonHasErrors()
        {
            return SongIsSuspended("(game)");
        }

        string org_selectIndexLevelChoice = "first";
        private void Org_ChLvlIndx_byFirstLvl(object sender, EventArgs e)
        {
            org_selectIndexLevelChoice = "first";
            tsm_orgDntChngLvlSlct.Checked = false;
            tsm_orgSlctFirstSupprtdLvl.Checked = false;
            tsm_orgSlctFirstLevel.Checked = true;
            AddOrUpdateAppSettings("o_chnglvlindx", "first");
        }
        private void Org_ChLvlIndx_by1stSupported(object sender, EventArgs e)
        {
            org_selectIndexLevelChoice = "supported";
            tsm_orgDntChngLvlSlct.Checked = false;
            tsm_orgSlctFirstSupprtdLvl.Checked = true;
            tsm_orgSlctFirstLevel.Checked = false;
            AddOrUpdateAppSettings("o_chnglvlindx", "supported");
        }
        private void Org_ChLvlIndx_noChng(object sender, EventArgs e)
        {
            org_selectIndexLevelChoice = "none";
            setListShowTutorial(tsm_showTutSetList.Checked);
            tsm_orgDntChngLvlSlct.Checked = true;
            tsm_orgSlctFirstSupprtdLvl.Checked = false;
            tsm_orgSlctFirstLevel.Checked = false;
            AddOrUpdateAppSettings("o_chnglvlindx", "none");
        }


        private void ShowTutInSetListChecked(object sender, EventArgs e)
        {
            setListShowTutorial(tsm_showTutSetList.Checked);
            AddOrUpdateAppSettings("showTutSL", tsm_showTutSetList.Checked.ToString().ToLower());
        }
        private void setListShowTutorial(bool show)
        {
            //shows tutorial and moves all other songs down
            if (show)
            {
                tutorialGroupBox.Visible = true;
                tutorialGroupBox.Top = 6;
                groupBox1.Top = 55;
                groupBox2.Top = 127;
                groupBox3.Top = 200;
                groupBox4.Top = 273;
            } else
            {
                tutorialGroupBox.Visible = false;
                groupBox1.Top = 6;
                groupBox2.Top = 79;
                groupBox3.Top = 152;
                groupBox4.Top = 225;
            }
        }
        private void ShowTutInOrganizerChecked(object sender, EventArgs e)
        {
            OrganizerShowTutorial(tsm_showTutOrganizer.Checked);
            AddOrUpdateAppSettings("showTutOr", tsm_showTutOrganizer.Checked.ToString().ToLower());
        }
        private void OrganizerShowTutorial(bool show)
        {
            //shows tutorial button and moves all Level buttons down
            if (show)
            {
                L0Settings.Visible = true;
                L1Settings.Left = 43;
                L2Settings.Left = 105;
                L3Settings.Left = 167;
                L4Settings.Left = 229;
                L5Settings.Left = 291;
                L6Settings.Left = 353;
                L7Settings.Left = 415;
                L8Settings.Left = 477;

                L1Settings.Width = 63;
                L2Settings.Width = 63;
                L3Settings.Width = 63;
                L4Settings.Width = 63;
                L5Settings.Width = 63;
                L6Settings.Width = 63;
                L7Settings.Width = 63;
                L8Settings.Width = 63;
            }
            else
            {
                L0Settings.Visible = false;
                L1Settings.Left = 1;
                L2Settings.Left = 69; // B)
                L3Settings.Left = 136;
                L4Settings.Left = 203;
                L5Settings.Left = 270;
                L6Settings.Left = 337;
                L7Settings.Left = 404;
                L8Settings.Left = 471;

                L1Settings.Width = 69;
                L2Settings.Width = 68;
                L3Settings.Width = 68;
                L4Settings.Width = 68;
                L5Settings.Width = 68;
                L6Settings.Width = 68;
                L7Settings.Width = 68;
                L8Settings.Width = 69;

                selectVokeIfSelectingTutorial();//if we were selecting tutorial, we automatically select voke
            }
        }
        private void selectVokeIfSelectingTutorial()
        {
            if(L0Settings.ForeColor == Color.White)
            {
                SetSelectedLevelColors(0);
                string songJsonInfo = Organizer_GetModJson();
                setSpecificLevelInfo_Org(songJsonInfo, "Voke");
                resetSongOriginalInfo("");
            }

        }

        private void L0Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return; //this returns false if we don't want to cancelChanges

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(8);
            setSpecificLevelInfo_Org(songJsonInfo, "Tutorial");
            resetSongOriginalInfo("");
        }

        private void OpenSongDir_click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            string pathOfSelectedSong = ((ListItem)listBox1.SelectedItem).Path.Replace("\\customsongs.json", "");
            OpenDir(pathOfSelectedSong);
        }

        private void organizer_debugSelectedSong(object sender, EventArgs e)
        {
            SendSongToDebug();
        }

        private void OpenDebugPanel(object sender, EventArgs e)
        {
            OpenDebug();
        }

        private void QuitMM(object sender, EventArgs e)
        {
            //need to warn user if they had unsaved changes
            System.Windows.Forms.Application.Exit();
        }

        ListBox rightClickedListBox = null;

        /// <summary>
        /// Set the other listbox's info to match the given listbox
        /// </summary>
        /// <param name="listboxName">The listbox we just changed, whose info we want the other to match</param>
        private void resetOtherListbox(string listboxName)
        {
            if (listboxName == "listBox1")
            {
                setListCatalog.Items.Clear();
                foreach(ListItem item in listBox1.Items)
                {
                    if(item.Name != "Current customsongs.json")
                    {
                        setListCatalog.Items.Add(item);
                    }
                }
                //setListCatalog.Items.AddRange(setListCatalog.Items);
                
            } else if (listboxName == "setListCatalog")
            {
                bool hadCurrCSjson = false;
                if (listBox1.Items.Count == 0) goto skipCurrJsonCheck;
                if (((ListItem)listBox1.Items[0]).Name == "Current customsongs.json")
                {
                    hadCurrCSjson = true;
                }
                skipCurrJsonCheck:
                listBox1.Items.Clear();
                listBox1.Items.AddRange(setListCatalog.Items);
                /*foreach (ListItem item in setListCatalog.Items)
                {
                    listBox1.Items.Add(item);
                }*/
                if (hadCurrCSjson)
                {
                    listBox1.Items.Insert(0, new ListItem { Name = "Current customsongs.json", Path = gameDir + "\\customsongs.json" });
                }
            }
        }

        bool organizerBlockSongInfoReset = false;
        private void LBMoveSongUp(object sender, EventArgs e)
        {
            setList_topLabel.Text = "Reloading Selection Order...";
            setList_topLabel.Visible = true;
            setListCatalog.Enabled = false;
            listBox1.Enabled = false;
            
            string selectedSong = ((ListItem)rightClickedListBox.SelectedItem).Name;
            int selectedIndex = rightClickedListBox.SelectedIndex;
            if(rightClickedListBox.Name == "listBox1")
            {
                if (((ListItem)rightClickedListBox.Items[0]).Name == "Current customsongs.json")
                {
                    selectedIndex -= 1;
                }
                organizerBlockSongInfoReset = true; //block the Organizer from resetting any info
            }

            ChangeCatalogOrder(selectedSong, -1);
            ListBoxMoveItem(rightClickedListBox, -1);
            //moveCSSupportSpot(selectedIndex, -1);
            resetOtherListbox(rightClickedListBox.Name);
            reloadModsListViaCatalog();
            
            setList_topLabel.Visible = false;
            organizerBlockSongInfoReset = false;
            setListCatalog.Enabled = true;
            listBox1.Enabled = true;
        }
        private void LBMoveSongDown(object sender, EventArgs e)
        {
            setList_topLabel.Text = "Reloading Selection Order...";
            setList_topLabel.Visible = true;
            setListCatalog.Enabled = false;
            listBox1.Enabled = false;
            string selectedSong = ((ListItem)rightClickedListBox.SelectedItem).Name;
            int selectedIndex = rightClickedListBox.SelectedIndex;
            if (rightClickedListBox.Name == "listBox1")
            {
                if (((ListItem)rightClickedListBox.Items[0]).Name == "Current customsongs.json")
                {
                    selectedIndex -= 1;
                }
                organizerBlockSongInfoReset = true; //block the Organizer from resetting any info
            }

            ChangeCatalogOrder(selectedSong, 1);
            //ListBoxMoveItem(rightClickedListBox, 1);
            ListBoxMoveItem(rightClickedListBox, 1);
            resetOtherListbox(rightClickedListBox.Name);
            //moveCSSupportSpot(selectedIndex, 1);
            reloadModsListViaCatalog();

            organizerBlockSongInfoReset = false;
            setListCatalog.Enabled = true;
            listBox1.Enabled = true;
            setList_topLabel.Visible = false;
        }
        private void LBMoveSong2Top(object sender, EventArgs e)
        {
            setList_topLabel.Text = "Reloading Selection Order...";
            setList_topLabel.Visible = true;
            setListCatalog.Enabled = false;
            listBox1.Enabled = false;
            string selectedSong = ((ListItem)rightClickedListBox.SelectedItem).Name;
            int selectedIndex = rightClickedListBox.SelectedIndex;
            if (rightClickedListBox.Name == "listBox1")
            {
                if (((ListItem)rightClickedListBox.Items[0]).Name == "Current customsongs.json")
                {
                    selectedIndex -= 1;
                }
                organizerBlockSongInfoReset = true; //block the Organizer from resetting any info
            }

            ChangeCatalogOrder(selectedSong, -1, true);
            //ListBoxMoveItem(rightClickedListBox, -1, true);
            ListBoxMoveItem(rightClickedListBox, -1, true);
            resetOtherListbox(rightClickedListBox.Name);
            //moveCSSupportSpot(selectedIndex, -1, true);

            reloadModsListViaCatalog();
            //resetOtherListbox(rightClickedListBox.Name);

            organizerBlockSongInfoReset = false;
            setListCatalog.Enabled = true;
            listBox1.Enabled = true;
            setList_topLabel.Visible = false;
        }
        private void LBMoveSong2Bot(object sender, EventArgs e)
        {
            setList_topLabel.Text = "Reloading Selection Order...";
            setList_topLabel.Visible = true;
            setListCatalog.Enabled = false;
            listBox1.Enabled = false;
            string selectedSong = ((ListItem)rightClickedListBox.SelectedItem).Name;
            int selectedIndex = rightClickedListBox.SelectedIndex;
            if (rightClickedListBox.Name == "listBox1")
            {
                if (((ListItem)rightClickedListBox.Items[0]).Name == "Current customsongs.json")
                {
                    selectedIndex -= 1;
                }
                organizerBlockSongInfoReset = true; //block the Organizer from resetting any info
            }

            ChangeCatalogOrder(selectedSong, 1, true);
            //ListBoxMoveItem(rightClickedListBox, 1, true);
            ListBoxMoveItem(rightClickedListBox, 1, true);
            resetOtherListbox(rightClickedListBox.Name);
            //moveCSSupportSpot(selectedIndex, 1, true);
            reloadModsListViaCatalog();

            organizerBlockSongInfoReset = false;
            setListCatalog.Enabled = true;
            listBox1.Enabled = true;
            setList_topLabel.Visible = false;
        }

        private void enableCustomSortButtons(bool enabled, string ttText = null)
        {
            if(ttText == null) ttText = "Custom Sort must be selected.";
            if (enabled) ttText = "";

            moveToTopToolStripMenuItem.Enabled = enabled;
            moveToBottomToolStripMenuItem.Enabled = enabled;
            moveUpToolStripMenuItem.Enabled = enabled;
            moveDownToolStripMenuItem.Enabled = enabled;
            moveToTopToolStripMenuItem.ToolTipText = ttText;
            moveToBottomToolStripMenuItem.ToolTipText = ttText;
            moveUpToolStripMenuItem.ToolTipText = ttText;
            moveDownToolStripMenuItem.ToolTipText = ttText;
        }

        private void tsm_sortAtoZ_click(object sender, EventArgs e)
        {
            tsm_sortAtoZ.Checked = true;
            tsm_sortZtoA.Checked = false;
            tsm_customSort.Checked = false;
            enableCustomSortButtons(false);
            setList_topLabel.Text = "Reloading Selection Order...";
            setList_topLabel.Visible = true;

            rightClickedListBox.Enabled = false;

            string selectedSong = null;
            if (rightClickedListBox.SelectedIndex != -1)
            {
                selectedSong = ((ListItem)rightClickedListBox.SelectedItem).Name;
                if(rightClickedListBox.Name == "listBox1")
                {
                    organizerBlockSongInfoReset = true;
                }
            }


            bool hadCurrCSjson = false;
            if (listBox1.Items.Count > 0)
            {
                if (listBox1.Items.Cast<ListItem>().First().Name == "Current customsongs.json")
                {
                    listBox1.Items.RemoveAt(0);
                    hadCurrCSjson = true;
                }
            }


            var list = rightClickedListBox.Items.Cast<ListItem>().OrderBy(item => item.Name).ToList();
            listBox1.Items.Clear();
            setListCatalog.Items.Clear();
            foreach (ListItem listItem in list)
            {
                listBox1.Items.Add(listItem);
                setListCatalog.Items.Add(listItem);
            }
            reloadModsListViaCatalog();

            if (hadCurrCSjson) listBox1.Items.Insert(0, new ListItem { Name = "Current customsongs.json", Path = gameDir + "\\customsongs.json" });

            setOldSongSlctnAfterSort(rightClickedListBox, selectedSong);

            rightClickedListBox.Enabled = true;
            setList_topLabel.Visible = false;
            AddOrUpdateAppSettings("catalogSort", "a-z");
            catalogsort = "a-z";
        }
        private void tsm_sortZtoA_click(object sender, EventArgs e)
        {
            tsm_sortZtoA.Checked = true;
            tsm_sortAtoZ.Checked = false;
            tsm_customSort.Checked = false;
            enableCustomSortButtons(false);
            setList_topLabel.Text = "Reloading Selection Order...";
            setList_topLabel.Visible = true;

            rightClickedListBox.Enabled = false;

            string selectedSong = null;
            if (rightClickedListBox.SelectedIndex != -1)
            {
                // we were selecting something
                selectedSong = ((ListItem)rightClickedListBox.SelectedItem).Name;
                if (rightClickedListBox.Name == "listBox1")
                {
                    organizerBlockSongInfoReset = true;
                }
            }

            bool hadCurrCSjson = false;
            if (listBox1.Items.Count > 0)
            {
                if (listBox1.Items.Cast<ListItem>().First().Name == "Current customsongs.json")
                {
                    listBox1.Items.RemoveAt(0);
                    hadCurrCSjson = true;
                }
            }

            var list = rightClickedListBox.Items.Cast<ListItem>().OrderBy(item => item.Name).ToList();
            list.Reverse();
            listBox1.Items.Clear();
            setListCatalog.Items.Clear();
            foreach (ListItem listItem in list)
            {
                listBox1.Items.Add(listItem);
                setListCatalog.Items.Add(listItem);
            }
            reloadModsListViaCatalog();

            

            if (hadCurrCSjson) listBox1.Items.Insert(0, new ListItem { Name = "Current customsongs.json", Path = gameDir + "\\customsongs.json" });

            setOldSongSlctnAfterSort(rightClickedListBox, selectedSong);

            rightClickedListBox.Enabled = true;
            setList_topLabel.Visible = false;
            AddOrUpdateAppSettings("catalogSort", "z-a");
            catalogsort = "z-a";
        }
        private void tsm_customSort_click(object sender, EventArgs e)
        {
            rightClickedListBox.Enabled = false;
            tsm_sortZtoA.Checked = false;
            tsm_sortAtoZ.Checked = false;
            tsm_customSort.Checked = true;
            enableCustomSortButtons(true);

            string selectedSong = null;
            if (rightClickedListBox.SelectedIndex != -1)
            {
                //we were selecting something
                selectedSong = ((ListItem)rightClickedListBox.SelectedItem).Name;
                if (rightClickedListBox.Name == "listBox1")
                {
                    organizerBlockSongInfoReset = true;
                }
            }

            SetToConfigSort(); //does both list boxes
            reloadModsListViaCatalog();

            setOldSongSlctnAfterSort(rightClickedListBox, selectedSong);

            rightClickedListBox.Enabled = true;
            AddOrUpdateAppSettings("catalogSort", "custom");
            catalogsort = "custom";
        }

        /// <summary>
        /// Whatever we were selecting before will be selected again
        /// </summary>
        /// <param name="lb">ListBox we were selecting</param>
        /// <param name="songName">Song name/Selection we were on</param>
        private void setOldSongSlctnAfterSort(ListBox lb, string songName)
        {
            if (songName == null) return;
            
            for(int i=0; i<lb.Items.Count; i++)
            {
                if (((ListItem)lb.Items[i]).Name == songName)
                {
                    lb.SelectedIndex = i;
                    if (lb.Name == "listBox1") currentListSelection = i;
                    return;
                }
            }
        }

        private void reloadModsList_click(object sender, EventArgs e)
        {
            DisableAllInputs();
            RepeatStartup();
        }

        private void FocusTabPage(object sender, EventArgs e)
        {
            tabControl1.Focus();
        }

        private void TopMenuFocus(object sender, EventArgs e)
        {
            menuStrip1.Focus();
        }

        private void AllowAutoSelect_click(object sender, EventArgs e)
        {
            AddOrUpdateAppSettings("allowAS", tsm_AllowAutoSelect.Checked.ToString().ToLower());
        }


        double[] gameMusicBankBPMs = { 103, 110, 113, 120, 125, 127, 130, 140 };
        string[] gameMusicBanks = 
            {
                "b3c4fa80-6d1f-48bb-a99e-c1a8d907202a",
                "f148051f-18b3-41e8-a7e4-c5b1e8d9fad0",
                "2d9aeac6-2861-46c1-8171-fe86467ad3aa",
                "8317f105-4ce6-429d-ab15-581bb764bf51",
                "167ff263-40fc-4ce1-9dd1-ef0c6f953719",
                "33591d7d-60b2-4d78-991b-31faad363ec5",
                "c4e32493-b007-45b9-b517-a85bdc1da220",
                "31cc1d08-29bc-4d40-bde2-d664e427af1b"
            };
        //103 is "b3c4fa80-6d1f-48bb-a99e-c1a8d907202a"
        //140 is "31cc1d08-29bc-4d40-bde2-d664e427af1b"

        
        private string getLowHealthEvent(string musicBankPath, string bpmString)
        {
            double bpm = -1;
            if (double.TryParse(bpmString, out double vampires))
            {
                bpm = double.Parse(bpmString);
            }
            else { return null; }

            if (gameMusicBankBPMs.Contains(bpm))
            {
                //the BPM we want is one from the default BPM list
                int indexOfDefaultBPM = Array.FindIndex(gameMusicBankBPMs, b => b == bpm);
                return gameMusicBanks[indexOfDefaultBPM];
            }

            if (musicBankPath == "(default)") return null;//this only happens if we already see we already saw we don't have any found Music.banks

            string possibleIndex = musicBankPath.Replace("Music.bank", "Index.txt");
            if (File.Exists(possibleIndex))
            {
                //we have an index file
                string eventFromTxt = getBPMInTxt(possibleIndex, bpm);
                if (eventFromTxt != null)
                {
                    //we found a valid event ID
                    return eventFromTxt;
                }
            }
            //we don't have an Index, or couldn't find BPM

            string possibleGUID = musicBankPath.Replace("Music.bank", "GUIDs.txt");
            if (File.Exists(possibleGUID))
            {
                //we have an GUID.txt file
                string eventFromTxt = getBPMInTxt(possibleGUID, bpm);
                return eventFromTxt;

            }

            //we don't have an Index, or a GUID.txt, or they were no help to us in finding the event ID

            string possibleCSjson = musicBankPath.Replace("Music.bank", "customsongs.json");
            if (File.Exists(possibleCSjson))
            {
                //we have a JSON file
                string eventFromJson = getBPMInJson(possibleCSjson, bpm);
                if (eventFromJson != null)
                {
                    //we found a valid event ID
                    return eventFromJson;
                }

            }


            //at this point, we've gone through every possibility of knowing our BPM without guessing it
            //if we're this far, we don't have anything to tell us where this BPM belongs
            //we'll set this to BPM to "null" so we don't try searching again for it

            //when we go through this list later, we have no choice but to just 
            //grab the info that's in the JSON we're pulling the song info from.
            //when we do so, we can alert the user that we couldn't verify all LowHealth beats

            return null;


        }

        private ListItem[] getLowHealthEvents(string musicBankPath, double[] listOfBPMs)
        {
            //where ever our Music.bank path is, we want to look for a customsongs.json, a GUID.txt, or an Index.txt

            //we're going to look at what BPMs we want, then verify that we can find information for each one.
            //if there's a GUID or an Index, it'll be easier to know right away if we can find it
            //if the user is using a customsongs.json, we're limited to only knowing what BPMs that song's json contains

            List<double> foundBPMs = new List<double>(); //will hold the number of our BPMs
            List<string> bpmEvents = new List<string>(); //will hold the correlating event ID of our BPM from previous List
            List<ListItem> bpms = new List<ListItem>();

            foreach(double bpm in listOfBPMs)
            {
                //check and see if we've already gotten this one
                if (foundBPMs.Contains(bpm))
                {
                    int indexOfRepeatedBPM = Array.FindIndex(gameMusicBankBPMs, b => b == bpm);
                }


                if (gameMusicBankBPMs.Contains(bpm))
                {
                    //the BPM we want is one from the default BPM list
                    int indexOfDefaultBPM = Array.FindIndex(gameMusicBankBPMs, b => b == bpm);
                    foundBPMs.Add(gameMusicBankBPMs[indexOfDefaultBPM]);
                    bpmEvents.Add(gameMusicBanks[indexOfDefaultBPM]);
                    bpms.Add(new ListItem { Name = gameMusicBankBPMs[indexOfDefaultBPM].ToString(), Path = gameMusicBanks[indexOfDefaultBPM] });
                    continue;
                }



                //if we've gone this far, we're looking for a custom BPM

                string possibleIndex = musicBankPath.Replace("Music.bank", "Index.txt");
                if (File.Exists(possibleIndex))
                {
                    //we have an index file
                    string eventFromTxt = getBPMInTxt(possibleIndex, bpm);
                    if(eventFromTxt != null)
                    {
                        //we found a valid event ID
                        foundBPMs.Add(bpm);
                        bpmEvents.Add(eventFromTxt);
                        bpms.Add(new ListItem { Name = bpm.ToString(), Path = eventFromTxt });
                        continue;
                    } else
                    {
                        //we have an index file, yet our BPM wasn't in it
                        //keep going. maybe we'll find it somewhere else
                    }

                    
                }
                //we don't have an Index, or couldn't find BPM

                string possibleGUID = musicBankPath.Replace("Music.bank", "GUIDs.txt");
                if (File.Exists(possibleGUID))
                {
                    //we have an GUID.txt file
                    
                    string eventFromTxt = getBPMInTxt(possibleGUID, bpm);
                    if (eventFromTxt != null)
                    {
                        //we found a valid event ID
                        foundBPMs.Add(bpm);
                        bpmEvents.Add(eventFromTxt);
                        bpms.Add(new ListItem { Name = bpm.ToString(), Path = eventFromTxt });
                        continue;
                    }
                    else
                    {
                        //we have an GUID.txt file, yet our BPM wasn't in it
                        //keep going. maybe we'll find it in a customsongs.json
                    }

                }

                //we don't have an Index, or a GUID.txt, or they were no help to us in finding the event ID

                //the best we can hope for is that the user is pointing us to the valid info
                //we can warn them that if crashes occur, to download the GODDAMN LIBRARY FILE

                string possibleCSjson = musicBankPath.Replace("Music.bank", "customsongs.json");
                if (File.Exists(possibleCSjson))
                {
                    //we have a JSON file

                    string eventFromJson = getBPMInJson(possibleCSjson, bpm);
                    if (eventFromJson != null)
                    {
                        //we found a valid event ID
                        foundBPMs.Add(bpm);
                        bpmEvents.Add(eventFromJson);
                        bpms.Add(new ListItem { Name = bpm.ToString(), Path = eventFromJson });
                        continue;
                    }
                    else
                    {
                        //we have an customsongs.json file, yet our BPM wasn't in it
                        //keep going. maybe--wait.. that was the last possibility
                        //..............
                        //hmmmm.........
                    }

                }


                //at this point, we've gone through every possibility of knowing our BPM without guessing it
                //if we're this far, we don't have anything to tell us where this BPM belongs
                //we'll set this to BPM to "null" so we don't try searching again for it

                //when we go through this list later, we have no choice but to just 
                //grab the info that's in the JSON we're pulling the song info from.
                //when we do so, we can alert the user that we couldn't verify all LowHealth beats


                foundBPMs.Add(bpm); //add the BPM ...
                bpmEvents.Add(null); //... and set it to null
                bpms.Add(new ListItem { Name = bpm.ToString(), Path = null });


                //end of for each
            }

            //done with for each
            return bpms.ToArray();

            

            //Error;

        }

        private string getBPMInJson(string pathToIndex, double bpm)
        {
            //first we need to look for the BPM. Once we find it, we need to look 2 lines back for the "LowHealthBeatEvent" info.

            string jsonText = "";
            using (StreamReader sr = File.OpenText(@pathToIndex))
            {
                string fullText = sr.ReadToEnd();
                fullText = NormalizeWhiteSpace(fullText, true); //this (might?) make it easier to find stuff
                jsonText = fullText.ToLower(); //i just realized the event IDs will never be capital letters; lower in case user does Lowhealth100bpm, etc.
            }

            string treasureChest = "\"bpm\":" + bpm.ToString(); //we're looking for ie "BPM":145
            int indexOfBPM = jsonText.IndexOf(treasureChest); //let's hunt for treasure, phillip

            if (indexOfBPM == -1) return null; //there's no treasure here

            int indexOfPreviousLHBE = jsonText.LastIndexOf("\"lowhealthbeatevent\"", indexOfBPM);
            if (indexOfPreviousLHBE == -1) return null;

            int lengthOfSectionToChop = indexOfBPM - indexOfPreviousLHBE;//I assume it'd be faster to chop down the info we're looking at
            string choppedInfo = jsonText.Substring(indexOfPreviousLHBE, lengthOfSectionToChop);

            int indexOfCurlyStart = choppedInfo.IndexOf("{");
            int indexOfCurlyEnd = choppedInfo.IndexOf("}");
            if (indexOfCurlyStart == -1 || indexOfCurlyEnd == -1) return null;

            int lengthOfFoundEvent = indexOfCurlyEnd - (indexOfCurlyStart + 1);//+1 because want to be after the {
            if (lengthOfFoundEvent != 36) return null; //events are always supposed to be 36 characters

            string foundBPMTreasure = choppedInfo.Substring(indexOfCurlyStart + 1, 36);
            return foundBPMTreasure;




        }


        private string getBPMInTxt(string pathToIndex, double bpm)
        {
            //this can work for either GUID.txt or an Index.txt file
            //GUIDs put their event first, and their label after for some reason
            //human beings usually do Label -> Value. This can do either way


            string indexText = "";
            using (StreamReader sr = File.OpenText(@pathToIndex))
            {
                string fullText = sr.ReadToEnd();
                indexText = fullText.ToLower(); //i just realized the event IDs will never be capital letters; lower in case user does Lowhealth100bpm, etc.
            }
            

            string bpmLabel = "lowhealth" + bpm.ToString(); //we could make it do + "bpm" if we want to be a stickler

            int indexOfBPMLabel = indexText.IndexOf(bpmLabel);
            //we could do contains, but we immediately want to use its location if we found it
            if (indexOfBPMLabel == -1) { return null; }//our BPM is not in this Index.txt file

            //our BPM is in the list

            //the format we recognize is one BPM per line, and we need the damn curly brackets. So isolate this line.

            //find the index of the previous and next \n
            int indexOfLineStart = indexText.LastIndexOf("\n", indexOfBPMLabel)+1; //+1 because \n counts as 1 char; instead of end of prev line, we're at beginning of curr line
            if (indexOfLineStart == -1) indexOfLineStart = 0; //apparently we're on the top line?
            int indexOfNextLine = indexText.IndexOf("\n", indexOfBPMLabel);
            if (indexOfNextLine == -1) indexOfNextLine = indexText.Length; //apparently we're at the last line
            int lengthOfLine = indexOfNextLine - indexOfLineStart;

            string isolatedLine = indexText.Substring(indexOfLineStart, lengthOfLine);

            //find damn curly brackets
            int indexOfCurlyStart = isolatedLine.IndexOf("{");
            int indexOfCurlyEnd = isolatedLine.IndexOf("}");
            if (indexOfCurlyStart == -1 || indexOfCurlyEnd == -1) { return null; }

            int lengthOfFoundEvent = indexOfCurlyEnd - (indexOfCurlyStart + 1);//+1 because want to be after the {
            if (lengthOfFoundEvent != 36) { return null; }; //events are always supposed to be 36 characters

            string foundBPMTreasure = isolatedLine.Substring(indexOfCurlyStart + 1, 36);
             
            return foundBPMTreasure;
        }

        private void getLowHealthsTestClick(object sender, EventArgs e)
        {
            double[] testListOfBPMs = { 69, 110, 125, 135 };
            string musicPath = ((ListItem)customMusicBankCombo.SelectedItem).Path;
            ListItem[] allLowHealthEvents = getLowHealthEvents(musicPath, testListOfBPMs);
            MessageBox.Show(allLowHealthEvents[0].Path + "\n" + allLowHealthEvents[1].Path + "\n" + allLowHealthEvents[2].Path + "\n" + allLowHealthEvents[3].Path + "\nCount:" + allLowHealthEvents.Length);
        }





        /// <summary>
        /// Reads the Set List, and fetches the info of each selection's .json, returning a full customsongs.json. :D
        /// </summary>
        /// <returns></returns>
        private string MakeSetList(bool moveFiles = false)
        {
            

            //for each of these, we need the same number of Checkboxes, as Combo boxes, as grabLvlButtons
            CheckBox[] mainCheckBoxes = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8, checkm9 };
            CheckBox[] bossCheckBoxes = { checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8, mainCombo9 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };
            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton, ML9ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };
            int numberOfDefaultSong = 0;

            List<double> customBPMs = new List<double>();
            List<FileInfo> bankPaths = new List<FileInfo>();

            string newSetList = "{\n";
            newSetList += "    \"customLevelMusic\" : [\n";

            string currentJson = getCurrentCustomsongsJson(); //we'll store this in case we need it, so we don't keep grabbing it

            string musicBankPath = null;
            if (customMusicBankCombo.SelectedIndex > -1)
            {
                musicBankPath = ((ListItem)customMusicBankCombo.SelectedItem).Path;
            }
            bool cantVerifyBankEvents = false;

            for (int i=0; i<mainCheckBoxes.Length; i++)
            {
                string capdLevelName = allLevelNames[i].Substring(0, 1).ToUpper() + allLevelNames[i].Substring(1);

                if (i >= bossCheckBoxes.Length)
                {
                    //we're outside the range of the bossCheckBoxes; our Main doesn't have a coinciding Boss selection

                    if (mainCheckBoxes[i].Checked)
                    {
                        //check box is checked, meaning we're changing its info to something new
                        

                        if (mainLvlGrabButton[i].Text == "")
                        {
                            if (mainCBox[i].Text.Contains("(Default)"))
                            {

                                //we're changing it back to default, we don't want anything written here, and we don't have a boss music here
                                numberOfDefaultSong++;
                                continue;
                            }
                        }

                        string songsJson = GetJsonFromComboBoxText(mainCBox[i].Text);
                        if (songsJson == null) continue; //this will return null if the song wasn't in the list, or was default

                        int lvlToGetFromModInt = getLevelNumFromModGrabLvlButton(mainLvlGrabButton[i]);
                        string capdLvlToGrab = allLevelNames[lvlToGetFromModInt].Substring(0, 1).ToUpper() + allLevelNames[lvlToGetFromModInt].Substring(1);

                        string mainOrBoss = "m";
                        if (mainLvlGrabButton[i].Image != null) mainOrBoss = "b";

                        string[] sInfo = getCustomInfo_MakeSetList(songsJson, capdLvlToGrab, mainOrBoss);
                        if (sInfo == null) continue;

                        

                        string lhEvent = "";
                        if (musicBankPath != null)
                        {
                            lhEvent = getLowHealthEvent(musicBankPath, shaveSurroundingQuotesAndSpaces(sInfo[4]));
                        }
                        else { cantVerifyBankEvents = true; }

                        if (lhEvent != null) sInfo[2] = lhEvent;
                        else cantVerifyBankEvents = true;

                        string bankPathValue = getPathFromComboBoxSlctn(mainCBox[i].Text, sInfo[0]);

                        string modsBankPath = bankPathValue.Replace("\\\\", "\\");
                        bankPaths.Add(new FileInfo(modsBankPath));


                        newSetList += "        {\n"; //need to add the closing one as well
                        newSetList += "            \"LevelName\" : \"" + capdLevelName + "\",\n";
                        newSetList += "            \"MainMusic\" : {\n"; //need to add closing one as well

                        newSetList += getNewLevelInfoLines("", "m",
                            shaveSurroundingQuotesAndSpaces(sInfo[0]), shaveSurroundingQuotesAndSpaces(sInfo[1]),
                            shaveSurroundingQuotesAndSpaces(sInfo[2]), shaveSurroundingQuotesAndSpaces(sInfo[3]),
                            shaveSurroundingQuotesAndSpaces(sInfo[4]), bankPathValue) + "\n";

                        newSetList += "            }\n"; //closes Main/BossMusic
                        newSetList += "        },\n"; //closes the level, adding a comma.(we'll need to remove it later if necessary)



                    }
                    else
                    {
                        //the check box isn't checked, we want whatever's in our current JSON   
                        string[] sInfo = getCustomInfo_MakeSetList(currentJson, capdLevelName, "m");
                        if (sInfo == null) continue;

                        string lhEvent = "";
                        if (musicBankPath != null)
                        {
                            lhEvent = getLowHealthEvent(musicBankPath, shaveSurroundingQuotesAndSpaces(sInfo[4]));
                        }
                        else { cantVerifyBankEvents = true; }
                    
                        if (lhEvent != null) sInfo[2] = lhEvent;
                        else cantVerifyBankEvents = true;

                        newSetList += "        {\n"; //need to add the closing one as well
                        newSetList += "            \"LevelName\" : \"" + capdLevelName + "\",\n";
                        newSetList += "            \"MainMusic\" : {\n"; //need to add closing one as well

                        newSetList += getNewLevelInfoLines("", "m",
                            shaveSurroundingQuotesAndSpaces(sInfo[0]), shaveSurroundingQuotesAndSpaces(sInfo[1]),
                            shaveSurroundingQuotesAndSpaces(sInfo[2]), shaveSurroundingQuotesAndSpaces(sInfo[3]),
                            shaveSurroundingQuotesAndSpaces(sInfo[4]), shaveSurroundingQuotesAndSpaces(sInfo[5])) + "\n";

                        newSetList += "            }\n"; //closes Main/BossMusic
                        newSetList += "        },\n"; //closes the level, adding a comma.(we'll need to remove it later if necessary)

                    }




                } else
                {
                    //we're checking a song that has a Main and a Boss selection



                    string[] mSongInfo = null;
                    string[] bSongInfo = null;
                    //Main Start
                    if (mainCheckBoxes[i].Checked)
                    {
                        //check box is checked, meaning we're changing its info to something new

                        //we have something to stop this function from starting if our grab button text was "?"
                        if (mainLvlGrabButton[i].Text == "")
                        {
                            if (mainCBox[i].Text.Contains("(Default)"))
                            {
                                numberOfDefaultSong++;
                                goto CheckBoss;
                            }
                        }



                        string songsJson = GetJsonFromComboBoxText(mainCBox[i].Text);


                        int lvlToGetFromModInt = getLevelNumFromModGrabLvlButton(mainLvlGrabButton[i]);
                        string capdLvlToGrab = allLevelNames[lvlToGetFromModInt].Substring(0, 1).ToUpper() + allLevelNames[lvlToGetFromModInt].Substring(1);


                        if (songsJson != null)
                        {
                            string mainOrBoss = "m";
                            if (mainLvlGrabButton[i].Image != null) mainOrBoss = "b";

                            mSongInfo = getCustomInfo_MakeSetList(songsJson, capdLvlToGrab, mainOrBoss);
                            if (mSongInfo != null)
                            {
                                string bankPathValue = getPathFromComboBoxSlctn(mainCBox[i].Text, mSongInfo[0]);
                                mSongInfo[5] = bankPathValue;
                            }
                        }
                    }
                    else
                    {
                        //the check box isn't checked, we want whatever's in our current JSON   
                        string[] sInfo = getCustomInfo_MakeSetList(currentJson, capdLevelName, "m");
                        if (sInfo != null)
                        {
                            mSongInfo = sInfo;
                        }
                    }
                    //Main End
                    CheckBoss:
                    //Boss Start
                    if (bossCheckBoxes[i].Checked)
                    {
                        //check box is checked, meaning we're changing its info to something new

                        //check if we're wanting to change it back to default
                        if (bossLvlGrabButton[i].Text == "")
                        {
                            if (bossCBox[i].Text.Contains("(Default)"))
                            {
                                numberOfDefaultSong++;
                                goto WriteLevel;
                            }
                        }

                        string songsJson = GetJsonFromComboBoxText(bossCBox[i].Text);
                        int lvlToGetFromModInt = getLevelNumFromModGrabLvlButton(bossLvlGrabButton[i]);
                        string capdLvlToGrab = allLevelNames[lvlToGetFromModInt].Substring(0, 1).ToUpper() + allLevelNames[lvlToGetFromModInt].Substring(1);

                        if (songsJson != null)
                        {
                            string mainOrBoss = "b";
                            if (bossLvlGrabButton[i].Image != null) mainOrBoss = "m";
                            bSongInfo = getCustomInfo_MakeSetList(songsJson, capdLvlToGrab, mainOrBoss);
                            if (bSongInfo != null)
                            {
                                string bankPathValue = getPathFromComboBoxSlctn(bossCBox[i].Text, bSongInfo[0]);
                                bSongInfo[5] = bankPathValue;
                            }
                        }
                        
                    }
                    else
                    {
                        //the check box isn't checked, we want whatever's in our current JSON   
                        string[] sInfo = getCustomInfo_MakeSetList(currentJson, capdLevelName, "b");
                        if (sInfo != null)
                        {
                            bSongInfo = sInfo;
                        }
                    }
                    //Boss End

                    WriteLevel:

                    if (mSongInfo == null && bSongInfo == null)
                    {
                        //we didn't have any info here, skip it
                        continue;
                    };

                    newSetList += "        {\n";
                    newSetList += "            \"LevelName\" : \"" + capdLevelName + "\",\n";

                    if (mSongInfo != null)
                    {
                        string lhEvent = "";
                        if (musicBankPath != null)
                        {
                            lhEvent = getLowHealthEvent(musicBankPath, shaveSurroundingQuotesAndSpaces(mSongInfo[4]));
                        }
                        else { cantVerifyBankEvents = true; }
                    
                        if (lhEvent != null) mSongInfo[2] = lhEvent;
                        else cantVerifyBankEvents = true;

                        
                        string modsBankPath = shaveSurroundingQuotesAndSpaces(mSongInfo[5]).Replace("\\\\", "\\");
                        bankPaths.Add(new FileInfo(modsBankPath));

                        newSetList += "            \"MainMusic\" : {\n"; //need to add closing one as well
                        newSetList += getNewLevelInfoLines("", "m",
                            shaveSurroundingQuotesAndSpaces(mSongInfo[0]), shaveSurroundingQuotesAndSpaces(mSongInfo[1]),
                            shaveSurroundingQuotesAndSpaces(mSongInfo[2]), shaveSurroundingQuotesAndSpaces(mSongInfo[3]),
                            shaveSurroundingQuotesAndSpaces(mSongInfo[4]), shaveSurroundingQuotesAndSpaces(mSongInfo[5])) + "\n";

                        
                    }

                    if(mSongInfo != null && bSongInfo != null)
                    {
                        //we have both a Main and Boss song, we need to add the seperator
                        newSetList += "            },\n"; //closes Main/BossMusic
                    }

                    if(bSongInfo != null)
                    {
                        string lhEvent = "";
                        if (musicBankPath != null)
                        {
                            lhEvent = getLowHealthEvent(musicBankPath, shaveSurroundingQuotesAndSpaces(bSongInfo[4]));
                        }
                        else { cantVerifyBankEvents = true; }

                        if (lhEvent != null) bSongInfo[2] = lhEvent;
                        else cantVerifyBankEvents = true;

                        string modsBankPath = shaveSurroundingQuotesAndSpaces(bSongInfo[5]).Replace("\\\\", "\\");
                        bankPaths.Add(new FileInfo(modsBankPath));

                        newSetList += "            \"BossMusic\" : {\n"; //need to add closing one as well
                        newSetList += getNewLevelInfoLines("", "b",
                            shaveSurroundingQuotesAndSpaces(bSongInfo[0]), shaveSurroundingQuotesAndSpaces(bSongInfo[1]),
                            shaveSurroundingQuotesAndSpaces(bSongInfo[2]), shaveSurroundingQuotesAndSpaces(bSongInfo[3]),
                            shaveSurroundingQuotesAndSpaces(bSongInfo[4]), shaveSurroundingQuotesAndSpaces(bSongInfo[5])) + "\n";

                    }

                    newSetList += "            }\n"; //closes Main/BossMusic
                    newSetList += "        },\n"; //closes the level, adding a comma.(we'll need to remove it later if necessary)
                }

            }

            //remove the last comma
            newSetList = newSetList.Substring(0, newSetList.Length - 2);

            newSetList += "\n    ]\n";
            newSetList += "}";


            if (cantVerifyBankEvents)
            {
                SetList_DebugLabel2.Text = "Could not verify LowHealthBeatEvents";
            }



            int numberOfLinesMade = newSetList.Split(':').Length - 1;
            if (numberOfLinesMade < 6)
            {
                //all of our songs are default... give them what they want (and skip error verification)
                return "{\n    \"customLevelMusic\" : [\n\n    ]\n}";
            }


            //we should be done making the setlist. Before we do anything with the Music.bank, we'll verify there's no errors
            bool setListHasErrors = verifyNoErrors(newSetList.ToString());
            if (setListHasErrors)
            {
                //MessageBox.Show("Operation was cancelled because the JSON contained errors.\nGo to File > Reload Mods List to force a scan of possible errors in custom songs.");
                //now we'll ask to send it to debug instead of just burning it
                askToSendAttemptedSaveToDebug(newSetList);
                return null;
            }


            int verificationFromMusicBankCreation = 0;

            //moveFiles is true if we're writing this json with the intention of saving it
            if (moveFiles)
            {
                //musicBankPath is null if Combo box wasn't selected to anything
                if (musicBankPath == null)
                {
                    //Combo Box wasn't selected to anything. If this happens, we attempt to grab all our info straight from the Json

                } else
                {
                    //Music.bank combo box was selected to something when making Set List.

                    string slctdMusicBankName = ((ListItem)customMusicBankCombo.SelectedItem).Name; //this has already been confirmed to be selecting something
                    //if we're selecting anything except the Default Music.bank, we need to verify one exists in Mods folder
                    //if it doesn't, we're going to see if the game is holding its original. 
                    //If it is, we're going to automatically store it in Mods folder before rewriting it
                    if (slctdMusicBankName != "Game's Default .Bank")
                    {
                        //CheckToMakeDefaultMusicBankBackup will return false if we needed to make a backup and it failed doing so
                        if(CheckToMakeDefaultMusicBankBackup() == false)
                        {
                            string bankBackupUnsuccessful = "Set List creation was cancelled because we hit an error when\n" +
                                                         "attemping to backup the game's default Music.bank";
                            MessageBox.Show(bankBackupUnsuccessful);
                            return null;
                        }
                    }

                    verificationFromMusicBankCreation = ReplaceCurrentMusicDotBank(musicBankPath);
                    //if we got a 1 from ReplaceCurrentMusicDotBank, we're good.
                    //if it was a negative number, we can stop the process, or alert the user, etc.
                }

                
                string m = "";
                foreach(FileInfo b in bankPaths)
                {
                    m += b.FullName + "\n";
                }

                //string[] allBanksWanted = GetBanksMissingFromSA(bankPaths.ToArray());
                string[] allBanksWanted = GetBanksMissingFromSA(bankPaths.ToArray());
                string[] banksMissingInThePit = allBanksWanted.Distinct().ToArray(); //I don't think this is doing anything
                //MessageBox.Show("New bankPaths:\n" + m);
                MoveAllNewBankFiles(banksMissingInThePit);

            }

            if (verificationFromMusicBankCreation != 0)
            {
                //unsuccessful copy attempt
                
                string slctdMusicBnkNm = ((ListItem)customMusicBankCombo.SelectedItem).Name;
                slctdMusicBnkNm = slctdMusicBnkNm.Replace(".Bank", "Music.bank");
                string bankUnsuccessfulMsg = "Set List creation was cancelled because " + slctdMusicBnkNm + " file could not be successfully copied to game's directory.\n\n";
                bankUnsuccessfulMsg += faildMusicRplcmntReasons[verificationFromMusicBankCreation];
                MessageBox.Show(bankUnsuccessfulMsg);
                return null;
            }

            return newSetList;
        }

        public string[] getAllBanksUsedByCSJson(string fromLabel = "Bank")
        {
            if (!File.Exists(gameDir.ToString() + "\\customsongs.json")) { MessageBox.Show("Operation halted: no customsongs.json exists in StreamingAssets"); return null; }
            if (gameJsonHasErrors()) { MessageBox.Show("Operation halted: cannot read customsongs.json in StreamingAssets while it contains errors. Visit Organizer or DebugPanel to remove them."); return null; }

            List<string> banksInSA = new List<string>();

            string fullJson = "";
            using (StreamReader sr = File.OpenText(gameDir.ToString() + "\\customsongs.json"))
            {
                fullJson = sr.ReadToEnd();
            }

            string[] fullJsonLines = fullJson.Split('\n');
            foreach (string line in fullJsonLines)
            {
                int indexOfBankInQuotes = line.IndexOf("\"" + fromLabel + "\"");
                if (indexOfBankInQuotes == -1) continue;
                int indexOfColon = line.IndexOf(":");
                if (indexOfColon < indexOfBankInQuotes) continue;

                int indexOfValueStart = line.IndexOf("\"", indexOfColon) + 1;
                int indexOfValueEnd = line.IndexOf("\"", indexOfValueStart + 1);
                int valueLength = indexOfValueEnd - indexOfValueStart;
                string bankPulledFromCustomInfo = line.Substring(indexOfValueStart, valueLength);
                if(!bankPulledFromCustomInfo.Contains(".bank"))
                {
                    bankPulledFromCustomInfo += ".bank";
                }
                if (bankPulledFromCustomInfo.Contains("\\\\")) bankPulledFromCustomInfo = bankPulledFromCustomInfo.Replace("\\\\", "\\");

                banksInSA.Add(bankPulledFromCustomInfo);
            }
            banksInSA = banksInSA.Distinct().ToList();
            return banksInSA.ToArray();
        }


        string[] faildMusicRplcmntReasons =
        {
            "Selected Music.bank couldn't be found.",
            "Failure to alter game's Music.bank file.",
            "Failure mid-copy: please visit your StreamingAssets folder and rename \"Music_zpdh8udnljxk.bank\" back to \"Music.bank\" before trying again.",
            "Failure cleaning up: temporary file \"Music_zpdh8udnljxk.bank\" could not be deleted."
        };

        /// <summary>
        /// Deletes the Music.bank in game's StreamingAsset folder, and copies the given Music.bank file in its place. 
        /// Returns 0 if successful; otherwise, a number based on where it failed
        /// </summary>
        private int ReplaceCurrentMusicDotBank(string withThisBank)
        {
            string newMusicBank = withThisBank;
            string temporaryMusicBankSpot = gameDir + "\\Music_zpdh8udnljxk.bank";
            string gameMusicBank = gameDir + "\\Music.bank";

            if (!File.Exists(newMusicBank)) return 1; //immediately we know we can't do anything, don't try anything

            if (!File.Exists(gameMusicBank)) goto TryToCopy; //the user deleted their Music.bank? Or maybe they moved it so it wouldn't get deleted?

            //first move the game directory
            //this is incase something goes wrong during the copy, Music.bank isn't completely gone
            try
            {
                //if this was here, delete it
                if (File.Exists(temporaryMusicBankSpot)) { File.Delete(temporaryMusicBankSpot); }

                File.Move(gameMusicBank, temporaryMusicBankSpot);
            }
            catch
            {
                return 2;
            }

            //we successfully moved it, we're free to copy it

            TryToCopy:

            try
            {
                File.Copy(newMusicBank, gameMusicBank);
            }
            catch
            {
                return 3;
            }

            //we successfully copied the Music.bank to the game directory

            //now we delete our temporary one
            try
            {
                File.Delete(temporaryMusicBankSpot);
            }
            catch
            {
                //return -4;
                return 0;//technically this is an error, but we didn't HAVE to delete it. And if we run this again, we'll delete it anyways
            }

            return 0;
        }


        /// <summary>
        /// If about to rewrite Music.bank, checks if we should and can create a backup of game's default Music.bank, before doing so
        /// </summary>
        private bool CheckToMakeDefaultMusicBankBackup()
        {
            if (customMusicBankCombo.SelectedIndex <= -1) return true; //Music.bank combo box wasn't selected to anything, we're not overwriting it

            string slctdMBankName = ((ListItem)customMusicBankCombo.SelectedItem).Name;

            //Game's Default .Bank selection shows up if it sees we have the game's default bank in Mods folder OR game's StreamingAssets folder<-No it doesn't!
            if (slctdMBankName != "Game's Default .Bank")
            {
                if (!ModFolderHoldsOrgnlMusicBank)
                {
                    //our mod folder does not have the original bank in it
                    //check if the game is currently using the default Music.bank
                    FileInfo musicBankFile = new System.IO.FileInfo(gameDir + "\\Music.bank");
                    long thisMusicBanksFileSize = musicBankFile.Length;
                    bool gameUsingDefaultMusicBank = thisMusicBanksFileSize == gameMBFileSize;
                    if (gameUsingDefaultMusicBank)
                    {
                        //We have the game in our StreamingAssets folder and we're about to rewrite the only known copy
                        //Make a backup of game's default Music.bank
                        if (BackupDefaultMusicBankFile())
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        // The game isn't using its default Music.bank, and we don't have it in mods folder
                        // Hopefully it wasn't our fault and also: we're kinda SOL if we ever need it
                        return true;
                    }
                }
                else
                {
                    //mod folder already has backup of default Music.bank
                    return true;
                }
            }
            return true; //we're seleting the game's default Music.bank, we're not going to replace it
        }


        /// <summary>
        /// Copies the game's current Music.bank file and puts it in a folder called _DefaultMusicBank
        /// </summary>
        private bool BackupDefaultMusicBankFile()
        {
            string possibleDfltMscBnkFldr = di + "\\_DefaultMusicBank";

            if (!File.Exists(gameDir + "\\Music.bank")) return true; //there's nothing to copy

            if (!Directory.Exists(@possibleDfltMscBnkFldr)) goto CopyBank;

            //if we got this far, the directory exists; check if it has no files
            DirectoryInfo DirPath = new DirectoryInfo(@possibleDfltMscBnkFldr);
            int fileCount = DirPath.GetFiles("*", SearchOption.AllDirectories).Length;
            if (fileCount == 0)
            {
                //no files, delete it and continue
                try
                {
                    Directory.Delete(DirPath.ToString(), true);
                }
                catch
                {
                    return false;
                }
            } else
            {
                //there's something in here. We've already verified game's default doesn't exist in Mods folder, though
                //user might have seen this and replaced it (like if DLC was released and default Music.bank filesize changed)
                //we're just going to make a folder for something else and hope that works
                possibleDfltMscBnkFldr = di + "\\_OtherDefaultMusicBank";
            }

        CopyBank:
            Directory.CreateDirectory(possibleDfltMscBnkFldr);
            string ogPath = gameDir + "\\Music.bank";
            string newPath = possibleDfltMscBnkFldr + "\\Music.bank";
            try
            {
                File.Move(ogPath, newPath);
            }
            catch
            {
                return false;
            }
            //File.Copy(ogPath, newPath);
            //if we got this far, it was successful
            ModFolderHoldsOrgnlMusicBank = true;
            return true;
        }



        private string getPathFromComboBoxSlctn(string comboText, string bankName)
        {
            int indexOfSongInCatalog = setListCatalog.FindStringExact(comboText); //our setListCatalog holds the path info
            string pathToSongsBank = "";
            if (indexOfSongInCatalog == -1)
            {
                //song is either default, or doesn't exist
                return null;
            }
            else
            {
                //it's a mod
                pathToSongsBank = ((ListItem)setListCatalog.Items[indexOfSongInCatalog]).Path; //path contains our path to json
                //the path we currently got was to the customsongs.json, and it only has one \ between directories—json wants 2 between each
                pathToSongsBank = pathToSongsBank.Replace("customsongs.json", shaveSurroundingQuotesAndSpaces(bankName) + ".bank");
                pathToSongsBank = pathToSongsBank.Replace("\\", "\\\\");

                return pathToSongsBank;
            }

        }

        /// <summary>
        /// Gets the full customsongs.json (with NormalizedWhiteSpace) for whatever song matches comboText
        /// </summary>
        /// <returns>The full customsongs.json with normalized whitespace</returns>
        private string GetJsonFromComboBoxText(string comboText)
        {
            int indexOfSongInCatalog = setListCatalog.FindStringExact(comboText); //our setListCatalog holds the path info
            string pathToSongsJson = "";
            if (indexOfSongInCatalog == -1)
            {
                //it's not a mod; check if it's a Default song
                /*
                if (comboText.Contains("Default"))
                {
                    return "default";
                } else
                {
                    return null;
                }*/
                return null;
            } else
            {
                //it's a mod
                pathToSongsJson = ((ListItem)setListCatalog.Items[indexOfSongInCatalog]).Path; //path contains our literal path, I think
            }

            

            using (StreamReader sr = File.OpenText(@pathToSongsJson))
            {
                string s = "";

                string fullText = sr.ReadToEnd();
                string trimmedLine = NormalizeWhiteSpace(fullText);
                s = trimmedLine;
                return s;
            }
        }



        private void MakeSetListAndCopyIt()
        {
            if(customMusicBankCombo.SelectedIndex == -1)
            {
                MessageBox.Show("A Music.bank must be made selected when creating a Set List.");
                customMusicBankCombo.Focus();
                return;
            }

            int invalidEntryCheck = setList_checkForInvalidEntries();
            if (invalidEntryCheck < 0)
            {
                return;
            }

            if (!setListCheckForUnsavedChanges())
            {
                //there were no changes in our set list
                //we'll just copy the game's current json to clipboard, if the game is linked
                if (gameDir == null)
                {
                    //there's no game directory for us to copy
                    MessageBox.Show("Nothing to copy to clipboard;\nno changes have been made to Set List.");
                    return;
                } else
                {
                    string fullCurrJson = getCurrentCustomsongsJson(false);
                    Clipboard.SetText(fullCurrJson);
                    Text_NotifyAnim("Copied to clipboard!");
                    return;
                }
            }

            string newJson = MakeSetList();
            if(newJson == null)
            {
                return;
            }

            if(SetList_DebugLabel2.Text == "Could not verify LowHealthBeatEvents")
            {
                SetList_DebugLabel2.Text = "";

                string downloadingAnd = "";
                if (customMusicBankCombo.Items[0].ToString() != "The Library's .Bank")
                {
                    downloadingAnd = "downloading and ";
                }

                MessageBox.Show("Customsongs.json was successfully copied to clipboard.\nHowever, not all LowHealthBeatEvents could be verified with its matching tempo. " +
                    "If you experience crashes, please consider "+ downloadingAnd + "using the \"Low Health Library\" as your Music.bank selection.");
            }
            

            Clipboard.SetText(newJson);
            Text_NotifyAnim("Copied to clipboard!");
        }

        private void copySLButton_click(object sender, EventArgs e)
        {
            MakeSetListAndCopyIt();
        }

        private void revertSetList_click(object sender, EventArgs e)
        {
            MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
            string title = "Revert?";
            string message = "Revert Set List back to its current selections?";
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result != DialogResult.OK)
            {
                return;
            }

            RevertOldInfoIntoSetList();
        }

        private void clearSetList_click(object sender, EventArgs e)
        {
            ClearSetListToDefault();
        }

        private void SLCheckForUnsaved(object sender, EventArgs e)
        {
            SetListHasUnsavedChanges = setListCheckForUnsavedChanges();
        }

        private void MMCurtainClosing(object sender, FormClosingEventArgs e)
        {
            if (mmLoading) return;

            if (SetListHasUnsavedChanges || checkUnsavedChangesOrganizer())
            {
                string message = "Any unsaved changes ";

                if (SetListHasUnsavedChanges && checkUnsavedChangesOrganizer())
                {
                    message = "Any unsaved changes ";
                } else if (SetListHasUnsavedChanges)
                {
                    //only setList has unsaved changes
                    message = "All unsaved changes in Set List ";
                } else
                {
                    //only organizer has unsaved changes
                    message = "All unsaved changes in Organizer ";
                }

                
                message += "will be lost. Are you sure you want to close Metal Manager?";
                

                //MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                //DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Exclamation);
                switch (MessageBox.Show(message,
                             Text,
                             MessageBoxButtons.YesNo,
                             MessageBoxIcon.Exclamation))
                {
                    case DialogResult.Yes:
                        break;
                    case DialogResult.No:
                        e.Cancel = true;
                        break;
                }


                /*
                switch (MessageBox.Show("Message",
                             Text,
                             MessageBoxButtons.YesNoCancel,
                             MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        DoSave();
                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        
                        break;
                }*/
            }
        }

        //We don't need this technically, because we don't allow the user to input the Game or Mod directory after the SetDirsForm
        //If we want to have this though, put this at the top:
        /*
        using System.Runtime.InteropServices;// <- This goes at the top

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
        */

        /// <summary>
        /// Is supposed to be a replica of FormShown. I don't know why I don't rename it and just put it in FormShown
        /// </summary>
        private void RepeatStartup(string summoner = null)
        {
            mmLoading = true;

            OpenErrorGatekeeperDialogue(summoner);
            SetMMSettingsFromConfig();
            setFileMenuSelections();//sets our file menu selections, based on if we have game and mod directory set

            setList_topLabel.Visible = true;
            setList_topLabel.Text = "Reading Mod Folder...";
            string[] modList = loadModListFromConfig(setListCatalog, false); //this loads the contents into the setListCatalog ListBox
            AddOrRemoveNoSongsFound(modList);

            //resets the list alphabetically if setting has been set
            if (catalogsort == "a-z" || catalogsort == "z-a")
            {
                var list = setListCatalog.Items.Cast<ListItem>().OrderBy(item => item.Name).ToList();
                if (catalogsort == "z-a") { list.Reverse(); }

                setListCatalog.Items.Clear();
                foreach (ListItem listItem in list)
                {
                    if (listItem.Name == "(game)") continue;
                    setListCatalog.Items.Add(listItem);
                }
            }

            if (modList == null) { setList_topLabel.Visible = false; return; }
            placeCurrCSjsonInOrgnzr();
            resetOtherListbox("setListCatalog");

           //setList_topLabel.Text = "Reading song selections...";//really we're setting song selections
            fillSongSelection(modList);
            //setList_topLabel.Text = "Reading Mod Folder...";
            storeModListInfo(); //this stores the info for our song; specifically which levels it supports; the info is hidden and is used to quickly know what levels each mod has info for
            //setList_topLabel.Text = "Reading current selections...";
            setOldSongsArray(); //this stores an array of info of our current customsongs.json file in the game folder
            //setList_topLabel.Text = "Setting selections to current...";
            loadOldInfoIntoSetList(); //this loads the array from the previous line into the fields

            GetAllCustomMusicBanks();
            loadMusicBankList();
            setList_topLabel.Visible = false;
            
            setSongSelectionArray();
            TurnOnTheLights();//enable the form's selectables

            mmLoading = false;
        }


        

        /*
        /// <summary>
        /// Deprecated. Swing and a miss.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BfGWorkerMain_DoWork(object sender, DoWorkEventArgs e)
        {
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.WatchForCancel); 
            //Main_KeyDwn just looks for Escape if we're running the worker

            mmLoading = true;
            setList_topLabel.Visible = true;
            setList_topLabel.Text = "Reading Mod Folder...";

            for(int i=0; i< sequenceReport.Length-1; i++)
            {
                if (i == 0)
                {
                    OpenErrorGatekeeperDialogue();
                    SetMMSettingsFromConfig();
                    setFileMenuSelections();//sets our file menu selections, based on if we have game and mod directory set

                    

                } else if (i == 1)
                {

                    string[] modList = loadModListFromConfig(setListCatalog, false); //this loads the contents into the setListCatalog ListBox

                    //resets the list alphabetically if setting has been set
                    if (catalogsort == "a-z" || catalogsort == "z-a")
                    {
                        var list = setListCatalog.Items.Cast<ListItem>().OrderBy(item => item.Name).ToList();
                        if (catalogsort == "z-a") { list.Reverse(); }

                        setListCatalog.Items.Clear();
                        foreach (ListItem listItem in list)
                        {
                            setListCatalog.Items.Add(listItem);
                        }
                    }
                    if (modList == null) { setList_topLabel.Visible = false; return; }
                    placeCurrCSjsonInOrgnzr();
                    resetOtherListbox("setListCatalog");

                    setList_topLabel.Text = "Reading song selections...";//really we're setting song selections
                    fillSongSelection(modList);



                }
                if (i == 2)
                {
                    storeModListInfo(); //this stores the info for our song; specifically which levels it supports; the info is hidden and is used to quickly know what levels each mod has info for
                } else if (i == 3)
                {
                    setOldSongsArray(); //this stores an array of info of our current customsongs.json file in the game folder
                } else if (i == 4)
                {
                    loadOldInfoIntoSetList(); //this loads the array from the previous line into the fields
                } else if (i == 5)
                {
                    GetAllCustomMusicBanks();
                    loadMusicBankList();
                }

                BfGWorkerMain.ReportProgress(i);

                if (BfGWorkerMain.CancellationPending)
                {
                    //we cancelled the process (close or hitting Esc)
                    e.Cancel = true;
                    BfGWorkerMain.ReportProgress(0);
                    return;
                }

            }

            setList_topLabel.Visible = false;
            mmLoading = false;
        }
        */

        private void MoveAllNewBankFiles(string[] enteringThePit)
        {
            if(enteringThePit != null && enteringThePit.Length > 0)
            {
                //setList_topLabel.Visible = true;
                //setList_topLabel.Text = "Copying new .banks to StreamingAssets";

                /*Random rnd = new Random();
                int msgToShow = rnd.Next(1, metalMessages.Length);
                copyingBanksLabel.Text = metalMessages[msgToShow] + ",\nPlease wait...";*/

                copyingBanksLabel.Text = "Assigning .bank files to StreamingAssets,\n0/"+ enteringThePit.Length;

                copyingBanksLabel.Visible = true;
                //copyProgressBar.Visible = true;

                BfGWorkerMain.RunWorkerAsync(argument: enteringThePit);
            }
        }



        /// <summary>
        /// Using the list of .banks collected when making a set list, checks for the songs being modified into MH, returning a 
        /// list of full paths to .banks needing to be copied
        /// </summary>
        /// <param name="joiningThePit"></param>
        /// <returns></returns>
        private string[] GetBanksMissingFromSA(FileInfo[] joiningThePit)
        {
            //joiningThePit will be the .bank files we're setting in the customsongs.json
            string[] alreadyInThePit = GetAnomaliesInSA();//gives us an instance of all custom .bank files in StreamingAssets

            List<string> newToThePit = new List<string>();
            foreach(FileInfo mosher in joiningThePit)
            {
                //string bankFileName = mosher.Split('\\').Last(); //this is for strings, below is for FileInfo
                string bankFileName = mosher.Name;

                if (alreadyInThePit.Contains(bankFileName))
                {
                    //the file we're trying to add is already in the StreamingAssets folder

                    //check to see if they're the same (er same file size at least)
                    string bankInGameDir = gameDir.ToString() + "\\" + bankFileName;
                    bool fileSizesMatch = verifyMatchingFileSizes(mosher.Length.ToString(), bankInGameDir);

                    if (!fileSizesMatch)
                    {
                        //they're different files, we need the current one to move out of the way
                        if (moveOldFile(bankInGameDir) == false)
                            return null;

                        newToThePit.Add(mosher.FullName);
                        //moving a file should be lightning fast, but everything in newToThePit is going to be copied, which takes longer
                    }

                    
                } else
                {
                    //the file we're trying to add isn't in StreamingAssets yet
                    newToThePit.Add(mosher.FullName);
                }

            }
            return newToThePit.ToArray();
            
        }

        /// <summary>
        /// Renames a file from "Filename" to "Filename_Old", or "..._Old2", increasing till finding a free spot. Returns true if successful
        /// </summary>
        /// <param name="originalPath"></param>
        /// <returns></returns>
        private bool moveOldFile(string originalPath)
        {
            int currentCheck = 1;
            int maximumChecks = 99;
            string newPath = "";

            while (currentCheck <= maximumChecks)
            {
                //we're looking for a file that DOESN'T exist
                
                string pathToCheck = originalPath + "_Old";
                if (currentCheck > 1) pathToCheck += currentCheck.ToString(); //if this becomes greater than one, we'll start putting/looking for DuHast_Old2

                if (!File.Exists(@pathToCheck))
                {
                    try
                    {
                        File.Move(originalPath, pathToCheck);
                        return true;
                    }
                    catch(IOException)
                    {
                        //IOException means Windows hit an error when trying to be able to read/write the file
                        string fileInUseMsg = "Metal Manager is trying to rename an old .bank file (" + originalPath.Split('\\').Last() + ") in the StreamingAssets folder to replace it with a different variation," +
                            "but the file seems to be locked. It may be in use by Metal Hellsinger. If you're currently playing a level using this .bank, please return to Hell Select screen for Metal Hellsinger before continuing" +
                            "\n\nHit Retry to Continue, Cancel to stop all operations.";
                        MessageBoxButtons fileInUseButtons = MessageBoxButtons.RetryCancel;
                        DialogResult retryRename = MessageBox.Show(fileInUseMsg, "Error moving old bank", fileInUseButtons);
                        if (retryRename == DialogResult.Retry)
                        {
                            try
                            {
                                if (!File.Exists(originalPath))
                                    throw new FileNotFoundException();

                                File.Move(originalPath, pathToCheck);
                                return true;
                            }
                            catch (FileNotFoundException)
                            {
                                //the file isn't here anymore...? we just want the green light to move the .bank we want in StreamingAssets, so...
                                return true;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error moving " + originalPath.Split('\\').Last() + " to " + pathToCheck.Split('\\').Last() + ", operation was halted." +
                                    "\n" + ex.Message);
                                return false;
                            }

                        }


                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error moving " + originalPath.Split('\\').Last() + " to " + pathToCheck.Split('\\').Last() + ", operation was halted." +
                            "\n" + ex.Message);
                        return false;
                    }


                } else
                {
                    //file we just checked already exists, we're about to try another name
                    currentCheck++;
                }
            }

            string fileNm = originalPath.Split('\\').Last();
            MessageBox.Show("Metal Manager encountered an error:\n100 variations of " + fileNm + "_Old.bank already exist.");
            return false;
        }


        /// <summary>
        /// Checks if two files have the same filesize, using either their paths, or one's path and one's size
        /// </summary>
        /// <param name="file1PathOrSize"></param>
        /// <param name="file2Path"></param>
        /// <returns></returns>
        private bool verifyMatchingFileSizes(string file1PathOrSize, string file2Path)
        {
            long file1Size = new long();
            if (file1PathOrSize.Contains('\\'))
            {
                file1Size = new FileInfo(file2Path).Length;
            } else
            {
                file1Size = long.Parse(file1PathOrSize);
            }

            long file2Size = new FileInfo(file2Path).Length;
            if (file1Size == file2Size)
                return true;
            else
                return false;

        }


        /// <summary>
        /// Returns an array of strings with the full path to all .bank files in StreamingAssets that aren't there by default
        /// </summary>
        /// <returns></returns>
        private string[] GetAnomaliesInSA(bool report = false, bool deleet = false)
        {
            //this does not look for a custom Music.bank. Music.bank can be the game's, the LowHealth Library, or an unrecognized custom one
            //the following .banks are default .banks, at least for non-DLC 

            var banks = Directory.EnumerateFiles(gameDir.ToString(), "*.bank", SearchOption.TopDirectoryOnly)
            .Where(s => !s.Contains("AcheronChallengeSongBank.bank") &&
            !s.Contains("AcheronSongBank.bank") &&
            !s.Contains("Ambience.bank") &&
            !s.Contains("BeatTrackBank.bank") &&
            !s.Contains("BossSongBank.bank") &&
            !s.Contains("BossVariation01SongBank.bank") &&
            !s.Contains("BossVariation02SongBank.bank") &&
            !s.Contains("BossVariation03SongBank.bank") &&
            !s.Contains("Cutscenes.bank") && 
            !s.Contains("DLC01SongBank.bank") &&
            !s.Contains("DLC02SongBank.bank") &&
            !s.Contains("ExtrasSongBank.bank") &&
            !s.Contains("FinalBossSongBank.bank") &&
            !s.Contains("GehennaChallengeSongBank.bank") &&
            !s.Contains("GehennaSongBank.bank") &&
            !s.Contains("IncaustisChallengeSongBank.bank") &&
            !s.Contains("IncaustisSongBank.bank") &&
            !s.Contains("IngameSFX.bank") &&
            !s.Contains("Master.bank") &&
            !s.Contains("Master.strings.bank") &&
            !s.Contains("Music.bank") &&
            !s.Contains("NihilChallengeSongBank.bank") &&
            !s.Contains("NihilSongBank.bank") &&
            !s.Contains("PlayerWeaponsBank.bank") &&
            !s.Contains("PreviewSongBank.bank") &&
            !s.Contains("PrototypeSongBank.bank") &&
            !s.Contains("SheolBossSongBank.bank") &&
            !s.Contains("SheolSongBank.bank") &&
            !s.Contains("StygiaChallengeSongBank.bank") &&
            !s.Contains("StygiaSongBank.bank") &&
            !s.Contains("TempBossSongBank.bank") &&
            !s.Contains("TestSong.bank") &&
            !s.Contains("TitleMusicBank.bank") &&
            !s.Contains("TutorialSongBank.bank") &&
            !s.Contains("UI.bank") &&
            !s.Contains("VO.bank") &&
            !s.Contains("VokeChallengeSongBank.bank") &&
            !s.Contains("VokeSongBank.bank") &&
            !s.Contains("YhelmChallengeSongBank.bank") &&
            !s.Contains("YhelmSongBank"));

            
            if (deleet)
            {
                bool foundGameDefaultBank = false;
                string undeletableMsg = "";
                //List<string> cantBeDeleted = new List<string>();
                //List<string> readOnlyBanks = new List<string>();

                foreach (string bank in banks)
                {
                    if (ModFolderHoldsOrgnlMusicBank || foundGameDefaultBank)
                    {
                        try
                        {
                            File.Delete(bank);
                        }
                        catch
                        {
                            //doing this because some people set their mods to be READ ONLY.. WHY!?!?!
                            FileInfo fi = new FileInfo(@bank);
                            if (fi.IsReadOnly)
                            {
                                File.SetAttributes(bank, ~FileAttributes.ReadOnly);
                                try
                                {
                                    File.Delete(bank);
                                }
                                catch
                                {
                                    undeletableMsg += "(Read Only) " + bank.Replace(gameDir + "\\", "") + "\n";
                                }
                                //readOnlyBanks.Add(bank);
                                //cantBeDeleted.Add("(Read Only) " + bank.Replace(gameDir + "\\", ""));
                                
                            } else
                            {
                                //cantBeDeleted.Add(bank.Replace(gameDir + "\\", ""));
                                undeletableMsg += bank.Replace(gameDir + "\\", "") + "\n";
                            }
                        }
                    } else
                    {
                        //we want to ensure we're not deleting our original
                        long banksFileSz = new FileInfo(bank).Length;
                        if (banksFileSz == gameMBFileSize)
                        {
                            //we just found the game's default Music.bank, most likely renamed as a backup by the user. Don't delete it.
                            foundGameDefaultBank = true; //We could just keep all of them, or comment/uncomment this to make it go once...
                            continue;
                        } else
                        {
                            try
                            {
                                File.Delete(bank);
                            }
                            catch
                            {
                                FileInfo fi = new FileInfo(@bank);
                                if (fi.IsReadOnly)
                                {
                                    File.SetAttributes(bank, ~FileAttributes.ReadOnly);
                                    try
                                    {
                                        File.Delete(bank);
                                    }
                                    catch
                                    {
                                        undeletableMsg += "(Read Only) " + bank.Replace(gameDir + "\\", "") + "\n";
                                    }
                                    //readOnlyBanks.Add(bank);
                                    //cantBeDeleted.Add("(Read Only) " + bank.Replace(gameDir + "\\", ""));
                                }
                                else
                                {
                                    //cantBeDeleted.Add(bank.Replace(gameDir + "\\", ""));
                                    undeletableMsg += bank.Replace(gameDir + "\\", "") + "\n";
                                }
                            }
                        }
                    }
                    
                    
                }

                if (!string.IsNullOrWhiteSpace(undeletableMsg))
                {
                    MessageBox.Show("Couldn't be deleted:\n" + undeletableMsg);
                }
                
                return null;
            }

            return banks.ToArray();
        }

        


        string[] sequenceReport =
            {
                "Reading Mods Folder...",
                "Loading custom songs to Metal Manager...",
                "Reading song selections...",
                "Reading Mods supported levels...",
                "Reading current selections...",
                "Setting selections to current...",
                "Starting Metal Manager..."
            };

        string[] metalMessages = new string[] { "Turning it up to 11", "Intensifying head banging", "Taking care of business",
        "Settling this in the parking lot", "Posing for a bad ass album cover", "Smoking the last of what we got",
        "Acquiring more cow bell", "Releasing the bulls on parade", "Raising the horns", "Sharpening axes and tightening skins",
        "Using music to save a mortal soul", "Finishing this last bottle", "Tearing normality", "Consuming Fire", "Wondering where this tug of war will end",
        "Taking a bullet for JB", "Remembering, before we forget", "Dropping plates"};


        List<string> unsuccessfulMoveAttempts = new List<string>();

        private void BfGWorkerMain_DoWork(object sender, DoWorkEventArgs e)
        {
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.WatchForCancel);
            //WatchForCancel just looks for Escape if we're running the worker

            string[] newToThePit = e.Argument as string[]; //this won't get called if this was empty

            unsuccessfulMoveAttempts = new List<string>();

            for (int i = 0; i < newToThePit.Length; i++)
            {
                //double currentPercentage = (i + 1 / newToThePit.Length)*100;
                //int percInt = Convert.ToInt32(currentPercentage);
                //if (percInt > copyProgressBar.Maximum) percInt = copyProgressBar.Maximum;

                BfGWorkerMain.ReportProgress(i+1);

                //before we try to copy anything or check its FileInfo, make sure the file actually exists
                if (!File.Exists(newToThePit[i]))
                {
                    //the file we were about to try to copy somewhere does not exist...
                    unsuccessfulMoveAttempts.Add(newToThePit[i].Split('\\').Last());
                    continue;
                }

                string newBankDestination = gameDir.ToString() + "\\" + newToThePit[i].Split('\\').Last();
                if (File.Exists(newBankDestination))
                {

                    if (!verifyMatchingFileSizes(newToThePit[i], newBankDestination))
                    {
                        try
                        {
                            File.Copy(newToThePit[i], newBankDestination);
                        }
                        catch
                        {
                            unsuccessfulMoveAttempts.Add(newToThePit[i].Split('\\').Last());
                        }
                    }

                } else
                {
                    try
                    {
                        File.Copy(newToThePit[i], newBankDestination);
                    }
                    catch
                    {
                        unsuccessfulMoveAttempts.Add(newToThePit[i].Split('\\').Last());
                    }
                }


                if (BfGWorkerMain.CancellationPending)
                {
                    //we cancelled the process (close or hitting Esc)
                    e.Cancel = true;
                    BfGWorkerMain.ReportProgress(0);
                    return;
                }

            }

            

        }

        private void BfGWorkerMain_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //copyProgressBar.Value = e.ProgressPercentage;
            //setList_topLabel.Text = e.ProgressPercentage.ToString() + "%";
            //copyingBanksLabel.Text = "Assigning .bank files to StreamingAssets,\n0/" + enteringThePit.Length;
        }

        private void BfGWorkerMain_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.KeyDown -= this.WatchForCancel;//we don't need this anymore

            if (e.Cancelled)
            {
                MessageBox.Show("Operation was cancelled.");
                Text_NotifyAnim("Cancelled :(");
            }
            else if (e.Error != null)
            {
                string errMsg = e.Error.Message;
                if (errMsg.Length > 1000)
                {
                    errMsg = errMsg.Substring(0, 1000) + "...";
                }
                MessageBox.Show("Metal Manager encountered an error when trying to copy new .bank files to the StreamingAssets folder.\n" + e.Error.Message);
                Text_NotifyAnim("Error :(");
            }
            else
            {
                //we successfully got through all work!

                //copyProgressBar.Visible = false;
                copyingBanksLabel.Visible = false;
                setList_topLabel.Visible = false;
                //Text_NotifyAnim();
                mmLoading = false;
            }
        }

        private void WatchForCancel(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                cancelBfGWorker();
            }
        }

        private void getOldSetTest(object sender, EventArgs e)
        {
            string s = "";
            foreach (int m in currentSetListIndexes_main)
            {
                s += m + " ";
            }
            s += '\n';
            foreach (int b in currentSetListIndexes_boss)
            {
                s += b + " ";
            }
            MessageBox.Show(s);
        }

        private void openAboutPage(object sender, EventArgs e)
        {
            using (AboutForm aboutFrm = new AboutForm(null))
            {
                aboutFrm.Icon = this.Icon;
                aboutFrm.StartPosition = FormStartPosition.CenterScreen;
                aboutFrm.ShowDialog();
            }
        }
        private void openSendFeedback(object sender, EventArgs e)
        {
            using (AboutForm aboutFrm = new AboutForm("sendfeedback"))
            {
                aboutFrm.Icon = this.Icon;
                aboutFrm.StartPosition = FormStartPosition.CenterScreen;
                aboutFrm.ShowDialog();
            }
        }
        private void openGetLHLibraryPage(object sender, EventArgs e)
        {
            using (AboutForm aboutFrm = new AboutForm("getlhlibrary"))
            {
                aboutFrm.Icon = this.Icon;
                aboutFrm.StartPosition = FormStartPosition.CenterScreen;
                aboutFrm.ShowDialog();
            }
        }
        private void openHelpPage(object sender, EventArgs e)
        {
            using (AboutForm aboutFrm = new AboutForm("help"))
            {
                aboutFrm.Icon = this.Icon;
                aboutFrm.StartPosition = FormStartPosition.CenterScreen;
                aboutFrm.ShowDialog();
            }
        }

        private void testAnim(object sender, EventArgs e)
        {
            Text_NotifyAnimOrg();
        }

        /// <summary>
        /// Used in Organizer; Deletes song's current customsongs.json and restores its Original. Returns true if successful
        /// </summary>
        /// <returns></returns>
        private bool restoreOriginalJson()
        {   
            if (listBox1.SelectedIndex == -1) return false; //nothing selected 
            if (((ListItem)listBox1.SelectedItem).Name == "Current customsongs.json") return false; //somehow we're selected to the game

            string aboutToBeReplaced = ((ListItem)listBox1.SelectedItem).Path; //the path we get is the full path to song's customsongs.json
            string originalToRestore = aboutToBeReplaced.Replace("customsongs.json", "_Original\\customsongs.json");

            //ensure the actual file exists
            //string checkDirectoryToo = di + ((ListItem)listBox1.SelectedItem).Path + "\\_Original";
            // || !Directory.Exists(checkDirectoryToo) do i need to do this..?
            if (!File.Exists(originalToRestore))
            {
                string path2Show = pathShortener(originalToRestore, 40);
                path2Show = path2Show.Substring(0, 1).ToUpper() + path2Show.Substring(1);
                MessageBox.Show("An error occured: the original JSON cannot be found in \n" + path2Show);
                organizer_restoreJson.Visible = false;
                restoredLabel.Visible = false;
                return false;
            }

            if (!File.Exists(aboutToBeReplaced)) goto restoreOgJson; //no reason to try to delete it; maybe the user deleted it; we don't want them SOL if they did

            try
            {
                File.Delete(aboutToBeReplaced);
            }
            catch
            {
                MessageBox.Show("An error occured when trying to restore original Json.");
                return false;
            }

            restoreOgJson:

            try
            {
                File.Move(originalToRestore, aboutToBeReplaced);
            }
            catch
            {
                MessageBox.Show("An error occured when trying to restore original Json.");
                return false;
            }

            //Text_NotifyAnimOrg(); This is too much. Just make the button invisible and reveal "success"
            organizer_restoreJson.Visible = false;
            return true;

        }

        private void RestoreOriginalJson(object sender, EventArgs e)
        {
            string message = "Are you sure you want to restore the original .json for " + ((ListItem)listBox1.SelectedItem).Name + "?\n\nThis action cannot be undone.";
            string title = "Restore original .json?";
            MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result != DialogResult.Yes)
            {
                return;
            }

            //Text_NotifyAnimOrg(); this is too much
            //organizer_restoreJson.Visible = false; //it'll already do this
            if (restoreOriginalJson() == false) return; //if this failed, don't do the rest of the code

            int dontChangeLevel = getSelectedLevel_OrganizerInjector(); //gives us whatever level Organizer is selected on for its song
            string dontChangeLevelName = allLevelNames[dontChangeLevel].Substring(0, 1).ToUpper() + allLevelNames[dontChangeLevel].Substring(1);
            
            clearSongInfoBoxes();
            organizer_enableLevelButtons(); //needs to be after we get the level, since this "resets" our buttons

            

            string songJsonInfo = Organizer_GetModJson();

            setSupportedLevelColors(songJsonInfo);
            SetSelectedLevelColors(dontChangeLevel);
            setSpecificLevelInfo_Org(songJsonInfo, dontChangeLevelName);
            resetSongOriginalInfo("");
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private bool verifyNoErrors(string json)
        {
            StartupScanForm starter = new StartupScanForm();
            bool returnBool = starter.BuggyD_StopAtFirst(json) == false;
            return returnBool;
        }

        //I'll do this when I update it....
        private void filterSetList(object sender, EventArgs e)
        {
            TextBox filterTbox = sender as TextBox;
            string filterText = filterTbox.Text;

            /*
            foreach(ListViewItem item in setListCatalog.Items)
            {
                if (!item.ToString().Contains(filterText))
                {
                    setListCatalog.
                }
                item.Visible = false;
            }*/
            
            /*
            for (int i = 0; i < setListCatalog.Items.Count; i++)
            {
                if (!setListCatalog.Items[i].ToString().Contains(filterText))
                {
                    setListCatalog.Items[i]
                }
                
            }*/

        }

        /// <summary>
        /// Returns a negative number if we any empty song selections, have all default songs on SetList
        /// </summary>
        /// <returns></returns>
        private int setList_checkForInvalidEntries(bool verifyAllDefault = true, bool checkEmpties = true, bool checkInvalidEntries = true)
        {
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8, mainCombo9 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };
            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton, ML9ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };
            CheckBox[] mainCheckBoxes = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8, checkm9 };
            CheckBox[] bossCheckBoxes = { checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };
            bool allDefault = true;

            for(int i = 0; i<mainCBox.Length; i++)
            {
                if (mainCBox[i].Text != getDefaultSong(i, "m"))
                {
                    allDefault = false;
                }

                if (!mainCheckBoxes[i].Checked) continue; //this selection isn't checked, we don't care

                if (mainCBox[i].Text.Length == 0)
                {
                    string lvl = capFirst(allLevelNames[i]);
                    MessageBox.Show("Operation halted:\n" + lvl + "'s Main Music selection was empty.");
                    
                    mainCBox[i].Focus();
                    return -2;
                }

                if(mainLvlGrabButton[i].Text == "?")
                {
                    string lvl = capFirst(allLevelNames[i]);
                    MessageBox.Show("Operation halted:\nInvalid selection found on " + lvl + "'s Main Music selection.");
                    mainCBox[i].Focus();
                    return -3;
                }
                if (mainLvlGrabButton[i].Text == "!")
                {
                    string lvl = capFirst(allLevelNames[i]);
                    MessageBox.Show("Operation halted:\nNo level info selected for " + lvl + "'s Main Music selection.");
                    mainCBox[i].Focus();
                    return -4;
                }
            }

            for (int i = 0; i < bossCBox.Length; i++)
            {
                if (bossCBox[i].Text != getDefaultSong(i, "b"))
                {
                    allDefault = false;
                }
                if (!bossCheckBoxes[i].Checked) continue; //this selection isn't checked, we don't care

                if (bossCBox[i].Text.Length == 0)
                {
                    string lvl = capFirst(allLevelNames[i]);
                    MessageBox.Show("Operation halted:\n" + lvl + "'s Boss Music selection was empty.");

                    bossCBox[i].Focus();
                    return -2;
                }

                if (bossLvlGrabButton[i].Text == "?")
                {
                    string lvl = capFirst(allLevelNames[i]);
                    MessageBox.Show("Operation halted:\nInvalid selection found on " + lvl + "'s Boss Music selection.");
                    bossCBox[i].Focus();
                    return -3;
                }
                if(bossLvlGrabButton[i].Text == "!")
                {
                    string lvl = capFirst(allLevelNames[i]);
                    MessageBox.Show("Operation halted:\nNo level info selected for " + lvl + "'s Boss Music selection.");
                    bossCBox[i].Focus();
                    return -4;
                }
            }



            if(allDefault == true && verifyAllDefault)
            {
                MessageBox.Show("Operation halted:\nAll entries are default.");
                
                return -1;
            } else
            {
                return 1;
            }
        }

        /// <summary>
        /// Returns true if any boxes are empty when trying to save via Organizer; false if neccessary fields are accounted for
        /// </summary>
        /// <param name="m_or_b"></param>
        /// <returns></returns>
        private bool Org_HasEmptyBoxes(string m_or_b)
        {
            
            TextBox[] mainLevelTextBoxes = { MLNameBox, MLEventBox, MLLHBEBox, MLOffsetBox, MLBPMBox }; //we only care about these 5
            TextBox[] bossFightTextBoxes = { BFNameBox, BFEventBox, BFLHBEBox, BFOffsetBox, BFBPMBox }; //<---------^
            if(m_or_b == "m")
            {
                foreach(TextBox tb in mainLevelTextBoxes)
                {
                    if (string.IsNullOrWhiteSpace(tb.Text) || string.IsNullOrWhiteSpace(tb.Text)) return true;
                }
                return false;
            }
            else
            {
                foreach (TextBox tb in bossFightTextBoxes)
                {
                    if (string.IsNullOrWhiteSpace(tb.Text) || string.IsNullOrWhiteSpace(tb.Text)) return true;
                }
                return false;
            }


        }

        private void setList_uncheckAll()
        {
            CheckBox[] mainCheckBoxes = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8, checkm9 };
            CheckBox[] bossCheckBoxes = { checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };

            foreach (CheckBox chkBox in mainCheckBoxes)
            {
                chkBox.Checked = false;
            }
            foreach (CheckBox chkBox in bossCheckBoxes)
            {
                chkBox.Checked = false;
            }   
        }
        private void clearAllGrabBoxes()
        {
            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton, ML9ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };

            foreach(Button mBtn in mainLvlGrabButton)
            {
                if (mBtn.Text == "?") continue;
                mBtn.Text = "";
                mBtn.Image = null;
            }
            foreach (Button bBtn in bossLvlGrabButton)
            {
                if (bBtn.Text == "?") continue;
                bBtn.Text = "";
                bBtn.Image = null;
            }
        }


        private void saveCurrentSetList(object sender, EventArgs e)
        {
            if(setListCheckForUnsavedChanges() == false)
            {
                MessageBox.Show("No changes have been made to the Set List!");
                return;
            }

            int invalidEntryCheck = setList_checkForInvalidEntries(false); //we're only going to check for empties and invalid entries that are checked
            if (invalidEntryCheck < 0)
            {
                return;
            }

            if (customMusicBankCombo.SelectedIndex == -1)
            {
                MessageBox.Show("A Music.bank must be made selected when creating a Set List.");
                customMusicBankCombo.Focus();
                return;
            }

            //we're set to start trying to make a new Set List
            saveCurrSLButton.Enabled = false;

            string newJson = MakeSetList(true); //if MakeSetList has true, we're going to copy the Music.bank
            if (newJson == null)
            {
                SetList_DebugLabel1.Text = "Something prevented customsongs.json from being written. :(";
                Text_NotifyAnim("Error :(");
                mmLoading = false;
                return;
            }

            if (unsuccessfulMoveAttempts.Count > 0)
            {
                string unsuccessfulMoveMessage = "Set List was successfully made, but the following .banks could not be found:";
                foreach (string unsuccessfulBank in unsuccessfulMoveAttempts)
                {

                }
            }

            if (SetList_DebugLabel2.Text == "Could not verify LowHealthBeatEvents")
            {
                SetList_DebugLabel2.Text = "";

                string downloadingAnd = "";
                if (customMusicBankCombo.Items[0].ToString() != "The Library's .Bank")
                    downloadingAnd = "downloading and ";        

                MessageBox.Show("Customsongs.json was successfully written.\nHowever, not all LowHealthBeatEvents could be verified with its matching tempo. " +
                    "If you experience crashes, please consider " + downloadingAnd + "using the \"Low Health Library\" as your Music.bank selection.");
            }


            try
            {
                File.WriteAllText(gameDir + "\\customsongs.json", newJson);
            }
            catch(Exception ex)
            {
                MessageBox.Show("An error occured when saving game's Set List. :(\n"+ex.Message);
                mmLoading = false;
                Text_NotifyAnim("Error :(");
                return;
            }

            mmLoading = true;
            setOldSongsArray();
            setList_uncheckAll();
            clearAllGrabBoxes();

            //if a customsongs.json didn't exist in StreamingAssets before, we'll add it to the Organizer catalog now
            if (listBox1.Items[0].ToString() != "Current customsongs.json")
            {
                listBox1.Items.Insert(0, new ListItem { Name = "Current customsongs.json", Path = gameDir + "\\customsongs.json" });
            }
            
            mmLoading = false;

            Text_NotifyAnim();

        }

        private void whocares(object sender, EventArgs e)
        {
            GetAnomaliesInSA(true);
        }

        private void dltCstmBanks(object sender, EventArgs e)
        {
            GetAnomaliesInSA(false, true);
        }

        int currntCntdwnCount = 0;
        private Timer odTimer;
        private TimeSpan odTimespan = TimeSpan.FromSeconds(10); //we're wanting to have it so if the user types in 6:66, it'll go to 6:65, 6:64, etc. because trollolol

        public void odTimerGo()
        {
            if (odUseMain.Checked)
            {
                //we want to use custom main music and game's Song Selector boss music
                odExplainLbl.TextAlign = ContentAlignment.BottomCenter;
                odExplainLbl.Text = "Custom music mods enabled until timer ends.";
                odExplainLbl.Visible = true;
            }
            else if (odUseBoss.Checked)
            {
                //we want to use game's Song Selector main music and custom boss music
                odExplainLbl.TextAlign = ContentAlignment.BottomCenter;
                odExplainLbl.Text = "Custom music mods disabled until timer ends.";
                odExplainLbl.Visible = true;

            }
            else
            {
                return;
            }

            odTimeLabel.Text = odTimeTextbox.Text;
            odTimeLabel.Visible = true;
            odTimeTextbox.Visible = false;

            odRadioPanel.Enabled = false;

            odStart.Enabled = false;
            odPause.Enabled = true;
            odStop.Enabled = true;
            odReset.Enabled = true;
            odMMSS.Visible = false;

            

            string[] minSec = odTimeTextbox.Text.Split(':');
            int minutesLeft = Int32.Parse(minSec[0]);
            int secondsLeft = Int32.Parse(minSec[1]);
            odTimespan = TimeSpan.FromSeconds((minutesLeft * 60) + secondsLeft);

            currntCntdwnCount = 0;
            odTimer = new Timer();
            odTimer.Tick += new EventHandler(odCntdwnUpdate);
            odTimer.Interval = 1000; // in miliseconds
            odTimer.Start();
        }
        public void odCntdwnUpdate(object sender, EventArgs e)
        {
            int minutesLeft = 0;
            int secondsLeft = 0;
            if (odTimeLabel.Text.Contains(":"))
            {
                string[] minSec = odTimeLabel.Text.Split(':');
                minutesLeft = Int32.Parse(minSec[0]);
                secondsLeft = Int32.Parse(minSec[1]);
            } else
            {
                secondsLeft = Int32.Parse(odTimeLabel.Text.Replace("in ", ""));
            }

            if (minutesLeft == 0 && secondsLeft <= 10)
            {
                if(secondsLeft == 10)
                {
                    Random rnd = new Random();
                    int msgToShow = rnd.Next(1, metalMessages.Length);
                    odExplainLbl.Text = metalMessages[msgToShow];
                    odExplainLbl.TextAlign = ContentAlignment.BottomLeft;
                }
                goto FinalTen;
            }

            //we won't allow our count down to start at less than 10 seconds
            if (currntCntdwnCount < 10)
            {

                string lblText = "";
                if(secondsLeft > 0)
                {
                    secondsLeft--;
                    
                } else
                {
                    minutesLeft--;
                    secondsLeft = 59;
                }

                
                if (secondsLeft >= 10)
                    lblText = minutesLeft.ToString() + ":" + secondsLeft.ToString();    
                 else
                    lblText = minutesLeft.ToString() + ":0" + secondsLeft.ToString();
                
                
                odTimeLabel.Text = lblText;

                currntCntdwnCount++;
                return;
            } else
            {
                if (secondsLeft > 0)
                {
                    secondsLeft--;

                }
                else
                {
                    minutesLeft--;
                    secondsLeft = 59;
                }
                string lblText = "";
                if (secondsLeft >= 10)
                    lblText = minutesLeft.ToString() + ":" + secondsLeft.ToString();
                else
                    lblText = minutesLeft.ToString() + ":0" + secondsLeft.ToString();
                odTimeLabel.Text = lblText;
                return;
            }


        FinalTen:
            string lblTxt = "in " + secondsLeft;
            odTimeTextbox.Text = lblTxt;


            if (secondsLeft > 0)
            {
                secondsLeft--;
                return;
            } else
            {
                Timer odTmr = sender as Timer;
                odTmr.Stop();
                odTmr.Dispose();

                //odTimeLabel.Text = odTimeTextbox.Text;
                odTimeLabel.Visible = false;
                odTimeTextbox.Visible = true;

                odRadioPanel.Enabled = true;

                odStart.Enabled = true;
                odPause.Enabled = false;
                odStop.Enabled = false;
                odReset.Enabled = false;
                odMMSS.Visible = true;
            }

        }


        private void odStartClick(object sender, EventArgs e)
        {
            
            if(odStop.Enabled)
            {
                odTimer.Start();
                odStart.Enabled = false;
                odPause.Enabled = true;
            } else
            {
                odTimerGo();
            }
            
            


        }

        
        private void odPauseClick(object sender, EventArgs e)
        {
            odTimer.Stop();
            odPause.Enabled = false;
            odStart.Enabled = true;
        }
        private void odStopClick(object sender, EventArgs e)
        {
            odTimer.Stop();
            odTimer.Dispose();

            odExplainLbl.Visible = false;
            odTimeLabel.Visible = false;
            odTimeTextbox.Visible = true;

            odRadioPanel.Enabled = true;

            odStart.Enabled = true;
            odPause.Enabled = false;
            odStop.Enabled = false;
            odReset.Enabled = false;
            odMMSS.Visible = true;
        }
        private void odResetClick(object sender, EventArgs e)
        {
            odTimer.Stop();
            odTimer.Dispose();

            odPause.Enabled = true;
            odStart.Enabled = false;

            odTimeLabel.Text = odTimeTextbox.Text;
            currntCntdwnCount = 0;
            odTimer = new Timer();
            odTimer.Tick += new EventHandler(odCntdwnUpdate);
            odTimer.Interval = 1000; // in miliseconds
            odTimer.Start();
        }

        private void cleanUpSAClick(object sender, EventArgs e)
        {
            using (CleanUpSAForm cleanSA = new CleanUpSAForm(null))
            {
                cleanSA.Icon = this.Icon;
                cleanSA.MyParentForm = this;
                cleanSA.StartPosition = FormStartPosition.CenterParent;
                cleanSA.ShowDialog();
            }
        }

        private void reApplyAllBanksClick(object sender, EventArgs e)
        {
            string[] currentlyUsingBanks = getAllBanksUsedByCSJson("bankPath");
            if (currentlyUsingBanks == null || currentlyUsingBanks.Length == 0) return;

            MoveAllNewBankFiles(currentlyUsingBanks);
            if (!successLabel.Text.Contains("Error"))
            {
                MessageBox.Show("All .banks used by game's music customization have been sucessfully applied to StreamingAssets folder.");
            }
        }

        private void reApplyAllBanks_mouseOver(object sender, MouseEventArgs e)
        {
            CustomBanksExplainLbl.Text = "Copies all .bank files in use by game's customsongs.json that can't be found in the StreamingAssets folder.";
            CustomBanksExplainLbl.Visible = true;
        }

        private void cleanUpSA_mouseOver(object sender, MouseEventArgs e)
        {
            CustomBanksExplainLbl.Text = "Deletes unused .bank files from StreamingAssets. You will be asked for confirmation before deleting.";
            CustomBanksExplainLbl.Visible = true;
        }

        private void reApplyAllBanks_mouseOut(object sender, EventArgs e)
        {
            if(CustomBanksExplainLbl.Text == "Copies all .bank files in use by game's customsongs.json that can't be found in the StreamingAssets folder.")
            {
                CustomBanksExplainLbl.Visible = false;
            }
        }
        private void cleanUpSA_mouseOut(object sender, EventArgs e)
        {
            if (CustomBanksExplainLbl.Text == "Deletes unused .bank files from StreamingAssets. You will be asked for confirmation before deleting.")
            {
                CustomBanksExplainLbl.Visible = false;
            }
        }


        public bool forceRecheckClicked = false;
        private void ForceRecheckAllMods()
        {
            int prevNumberOfSongsWithErrors = ConfirmSuspendedSongs.Count; //numberOfModsWithErrors is used to know to put "No Custom Songs Found" on catalog. i don't wanna fuck with that

            DisableAllInputs();
            RepeatStartup("forceRecheck");
            forceRecheckClicked = false;

            int newNumOfSongsWithErrs = ConfirmSuspendedSongs.Count;

            string purgedPlural = ""; string newErrSongsPlural = "";
            string recheckMsg = "";
            if (prevNumberOfSongsWithErrors == 0 && newNumOfSongsWithErrs == 0)
            {
                // no errors, and no errors before
                recheckMsg += "no errors found!";
                
            } else if (prevNumberOfSongsWithErrors > 0 && newNumOfSongsWithErrs == 0)
            {
                // no errors, but had errors before
                recheckMsg += "all suspended songs purged!";

            }
            else if(prevNumberOfSongsWithErrors == newNumOfSongsWithErrs)
            {
                //has errors, and it matches the same number as before
                recheckMsg += "the same number of songs with errors remains.";
            }
            else if (prevNumberOfSongsWithErrors > newNumOfSongsWithErrs)
            {
                //has errors, but we have less than we did before
                int purgedSongsCnt = prevNumberOfSongsWithErrors - newNumOfSongsWithErrs; //if we had 7 songs with errors, now have 4, we purged 3
                string verbGrammar = ""; //we'll add an s to remain if there's only 1 song left
                if (purgedSongsCnt > 1) purgedPlural = "s"; //we'll add an s if purged songs is more than 1
                if (newNumOfSongsWithErrs > 1)
                {
                    newErrSongsPlural = "s"; //adding an s if new # of songs with errors is more than 1
                } else
                {
                    verbGrammar = "s";
                }
                recheckMsg += "successfully removed " + purgedSongsCnt + " song" + purgedPlural + " from suspension. However, " + newNumOfSongsWithErrs + " song"+newErrSongsPlural+" with errors remain"+verbGrammar+" suspended.";
            }
            else if (prevNumberOfSongsWithErrors < newNumOfSongsWithErrs)
            {
                //has errors, and we have more than we did before (whether we had 0 or more before)
                if (newNumOfSongsWithErrs > 1)
                    newErrSongsPlural = "s"; //adding an s if new # of songs with errors is more than 1
                
                recheckMsg += "new errors have been detected in " + newNumOfSongsWithErrs + " song"+ newErrSongsPlural + ".";
            }

            MessageBox.Show("Force re-check complete: " + recheckMsg);
        }
    
    }
    //making this ListItem class to harbor a Name and a hidden Value for listbox selections
    public class ListItem
    {
        public string Name { get; set; }
        public string Path { get; set; }

        //since listbox uses its items .toString method to create Texts, we must override the ToString method
        public override string ToString()
        {
            return Name;
        }
    }

}
