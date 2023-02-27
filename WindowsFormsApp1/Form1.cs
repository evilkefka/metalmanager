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


namespace WindowsFormsApp1
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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

                string csSupportString = getModSupportedLevels(modName);
                csSupLvls.Add(csSupportString);
                //testFindJson.Text += songList[i] + "'s supported levels: " + csSupportString;
            }
        }
        /* Deprecated, storeModListInfo is used instead
        private void storeModListInfoNoSubs()
        {
            //this is used by Set List page. We're storing hidden information regarding each Mod's supported level
            //we do this so we don't have to search through the Mod's .json everytime we want to know the levels it supports

            //this function doesn't need the actual info, we just need to detect if we can find levels, and each one has a level. 
            // if it can find the level, have the level store the event ID. if it cant, have it store nothing i guess
            //for now it just store yes or no

            DirectoryInfo[] songList = di.GetDirectories();
            //We always call this after loading the song list, so let's just load the list from that

            //string[] songList = 

            if (csSupLvls == null) csSupLvls = new List<string>();

            csSupLvls.Clear(); //first, clear it

            for (int i = 0; i < songList.Length; i++)
            {
                if (songList[i].ToString().Substring(0, 1) == "_") continue;
                string csSupportString = getModSupportedLevels(songList[i].ToString());
                csSupLvls.Add(csSupportString);
                testFindJson.Text += songList[i] + "'s supported levels: " + csSupportString;
            }
        }*/

        private string getModSupportedLevels(string mod)
        {
            //this function just performs getLevelSupport 8 times, returning a string that we can recognize/decipher later
            string fullModJson = SetList_GetModJson(mod);

            string result = "";
            int numberOfLevels = 8;
            //if we want to change tutorial, this needs to be 9

            //making the condition i < levelsNames.length kept making it only run 5 times
            for (int i = 0; i < numberOfLevels; i++)
            {
                result += i; //allows us to differentiate levels in the string easier
                result += getLevelSupport(fullModJson, levelNames[i]);//this returns a string, either m, b, mb, or ""

            }


            return result;
        }




        private string getLevelSupport(string fullJson, string Level)
        {
            //retrieves the info for one level's custom music, for setlist
            string result = "";

            string capitalizeLevelName = Level.Substring(0, 1).ToUpper() + Level.Substring(1);
            int indexOfLevelInfo = fullJson.IndexOf(capitalizeLevelName);


            if (indexOfLevelInfo == -1)
            {
                //this level is not here
                clearSongInfoBoxes();
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

            } else if (Level == "Sheol")
            {
                //we don't have anything
                result += "u";//for "unsupported"
            }
            return result;
        }

        private void getOldInfo_PostLoad()
        {
            //this set our old JSON info into our SetList
            //if the user already started making some selections, let's not throw them away—anything that has a checkmark, don't fill

        }


        DirectoryInfo gameDir = new DirectoryInfo(@"R:\SteamLibrary\steamapps\common\Metal Hellsinger\Metal_Data\StreamingAssets");
        private string getCurrentCustomsongsJson()
        {
            //with this function, we're returning a string of the information from our actual customsongs.json that the game reads (in the StreamingAssets folder)
            if (!Directory.Exists(gameDir.ToString())) return "-1";
            if (!File.Exists(gameDir + "\\customsongs.json")) return "-2";

            string currentJSONString = gameDir + "\\customsongs.json";
            using (StreamReader sr = File.OpenText(@currentJSONString))
            {
                string s = "";

                string fullText = sr.ReadToEnd();
                string trimmedLine = NormalizeWhiteSpace(fullText);
                s = trimmedLine;

                return s;
            }
        }

        private void getCurrentCustomSongNames()
        {

        }

        List<string> modsWithCustomMusicBank = new List<string>();
        List<ListItem> modsWithCustMusicBank = new List<ListItem>(); //we're going to store TWO things in here instead now—the mod name, and its path

        //di is the directory where we store our mods
        DirectoryInfo di = new DirectoryInfo(@"R:\SteamLibrary\steamapps\common\Metal Hellsinger\MODS");


        string[] defaultMainSongNames = { "This Is the End", "Stygia", "Burial At Night", "This Devastation", "Poetry of Cinder", "Dissolution", "Acheron", "Silent No More" };
        string[] defaultBossSongNames = { "Blood and Law", "Infernal Invocation I", "Infernal Invocation II", "Infernal Invocation III", "Infernal Invocation II", "Infernal Invocation I", "Infernal Invocation III" };

        private void fillSongSelection(string modList)
        {
            //this fills the Items for each ComboBox in our SetList page
            string[] modListArray = modList.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries); //our modList is a string that looks like:  Bodies::Prey For Me::Unstoppable



            ComboBox[] songSelectBox = { mainCombo1, bossCombo1, mainCombo2, bossCombo2, mainCombo3, bossCombo3, mainCombo4, bossCombo4, mainCombo5, bossCombo5, mainCombo6, bossCombo6, mainCombo7, bossCombo7, mainCombo8 };
            for (int i = 0; i < songSelectBox.Length; i++)
            {
                songSelectBox[i].Items.Clear(); //first, clear each combo box

                for (int z = 0; z < modListArray.Length; z++)
                {
                    songSelectBox[i].Items.Add(modListArray[z]);
                }

            }


        }

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

                        //we're going to reset these too, since it wasn't available before
                        setOldSongsArray(); //this stores an array of info of our current customsongs.json file in the game folder
                        loadOldInfoIntoSetList(false); //this loads the array from the previous line into the fields

                        gameDirInfo.Text = "Game Directory Found!";
                    } else
                    {
                        MessageBox.Show("Please select Metal Hellsinger's game directory or its StreamingAssets folder.");
                        gameDirInfo.Text = "No game directory found!";
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
                string look4Ignore = exeVerifyPath + "\\ignore.txt";
                if (File.Exists(@look4Game))
                {
                    returnString = exeVerifyPath + "\\Metal_Data\\StreamingAssets";
                    return returnString;
                }
                if (File.Exists(@look4Ignore))
                {
                    returnString = exeVerifyPath;
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

        //I should have named it something besides get
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

                    string modListDir = di.ToString();
                    modListDir = modListDir.Replace("\\\\", "\\");
                    modListDir = pathShortener(modListDir, 40);
                    ModDirLabel.Text = modListDir;
                }
            }
        }



        private string loadModListWithSubs(ListBox lBox, bool storeCustomMusicBank = false)
        {
            //this returns a string that says the names of all VALID Mod selections, searching even through subdirectories for a customsongs.json

            lBox.Items.Clear();
            string modListString = "";

            if (di.Exists)
            {
                //since we know it, change the label:
                string modListDir = di.ToString();
                modListDir = modListDir.Replace("\\\\", "\\");
                modListDir = pathShortener(modListDir, 40);
                ModDirLabel.Text = modListDir;

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

                        modsWithCustMusicBank.Add(new ListItem { Name = NameOfMod, Path = path });



                    }
                }

                return modListString;
            }
            else
            {
                //Alert! No Mods folder!
                ModDirLabel.Text = "No Mod directory set!";
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
                testFindJson.Text += " .hitEnter. ";
                e.SuppressKeyPress = true;
            }

            //I don't think this is working
            if (e.KeyCode == Keys.Tab)
            {
                testFindJson.Text = "HI";

                setGrabLvlButton(combo);
                e.SuppressKeyPress = true;
            }


            string lvlNumStr = combo.Name.Substring(combo.Name.Length - 1, 1);
            int lvlNum = Int32.Parse(lvlNumStr); //gives 1-based index
            lvlNum -= 1;

            alertLevelIfModIntegrityComprimised(lvlNum, combo);



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


        private void alertLevelIfModIntegrityComprimised(int levelNum, ComboBox combo)
        {
            //before we do all this, turn the alertInfo off
            alertLevel(levelNum, false);
            testFindJson.Text += "Alert test running";

            if (checkIfTwoModsSelected(levelNum))
            {
                //two mods are selected

                //find out which mods, and what level we want to grab info from the mod
                //the combo box has our mod, the grabLvlButton has the level info
                Button mGrabLvlButton = getGrabLvlBtnFromCombo(combo, "m");
                if (mGrabLvlButton == null) { testFindJson.Text += "...nulll..."; return; }
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

                //testFindJson.Text += " Checking (" + mainLvlNum + ", " + bossLvlNum + ", " + mMusicSelection + ", " + bMusicSelection + ")";


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
            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8 };
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

                if (indexOfComboBox == -1) { testFindJson.Text += " no1 "; return null; }

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
                if (indexOfComboBox == -1) { testFindJson.Text += " no2 "; return null; }
                return bossLvlGrabButton[indexOfComboBox];
            }

            //we shouldn't get this far.
            testFindJson.Text += " no3 ";
            return null;
        }
        private ComboBox getComboFromGrabLvlBtn(Button grabLvlBtn)
        {
            //this gives us the proper combo box based on what grabLvlButton we're asking about
            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8 };
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
                testFindJson.Text += ".sGLB denied.";
                return;
            }*/


            ComboBox boxWasSelecting = cBox; //boxWasSelecting stores whatever main/boss music ComboBox we just changed


            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton };
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
                testFindJson.Text += ".sGLB.";
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
                //whatever's in our box is a selection in our list
                //We already have something that runs if the index isn't -1, that's why this isn't running -_-
                /*
                string grabLStr = mainLvlGrabButton[whichLvl].Text;
                int songCurrentlySelected = Array.FindIndex(LvlAbbreviations, element => element == grabLStr); //this converts the letter in our GrabLvl box to a number
                testFindJson.Text += " run ";
                if (!modSupportsLevel(songSelectIndex, songCurrentlySelected, m_or_b))
                {
                    setModGrabLvlSelection(boxWasSelecting);
                }*/
            }
        }

        private void clearDebug(object sender, EventArgs e)
        {
            testFindJson.Text = "";
        }

        /*
        private void songSelectClick(object sender, EventArgs e)
        {
            testFindJson.Text += " O_O WTF!!! ";
            verifyAllGrabLvlButtons();
        }*/

        private void enableGrabLvlButton(ComboBox cBox, string whatitShouldSay)
        {
            string m_or_b = cBox.Name.Substring(0, 1);
            string boxCalledNumStr = cBox.Name.Substring(cBox.Name.Length - 1, 1);
            int whichLvl = Int32.Parse(boxCalledNumStr);
            whichLvl -= 1;
            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton };
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
                    testFindJson.Text += mainCBox[m].Text + "doesn't support this level";

                } else
                {
                    enableGrabLvlButton(mainCBox[m], LvlAbbreviations[m]);
                    testFindJson.Text += mainCBox[m].Text + " supports this level";
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
            ComboBox cBox = sender as ComboBox;
            setSongSelectionArray(cBox);//whatever we just selected just had its "current song selection" (currentComboBoxText) set to whatever it is now
        }


        //This is the collection of things we run when we stop focusing on the song.
        private void musicSelectLostFocus(object sender, EventArgs e)
        {

            ComboBox cBox = sender as ComboBox;
            testFindJson.Text += " .LostFocus. ";
            if (!wasComboBoxChanged(cBox)) return;
            setGrabLvlButton(cBox);



            string lvlNumStr = cBox.Name.Substring(cBox.Name.Length - 1, 1);
            int lvlNum = Int32.Parse(lvlNumStr); //gives 1-based index
            lvlNum -= 1;
            alertLevelIfModIntegrityComprimised(lvlNum, cBox);

            return;

            //The choices can either be, in the order of most likely: we chose a custom song, we wrote something blank, we wrote something the mod is not built to recognize (ie a song not in MODS folder)

            //In this function, we're going to decide what we do to our LvlGrab button


            ComboBox boxWasSelecting = sender as ComboBox; //boxWasSelecting stores whatever main/boss music ComboBox we just changed


            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };

            string songString = "";

            if (boxWasSelecting == null) //I actually have no idea why this would ever be null, unless there's just a flat error
                return;


            //if we chose a custom song, I don't think we do anything..?



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
                return;
            }


            if (boxWasSelecting.Text == "")
            {
                boxWasSelecting.Text = getDefaultSong(whichLvl, m_or_b);
                //getDefaultSong might also disable the button
                return;
            }

            //if the our combo box was empty, and we just ran getDefaultSong, none of the below will run

            //if whatever box we just changed now has a selected Index of -1, and we're at this point of the code, it means we have something that the program doesn't understand
            //an example of this is the user being silly and putting their mod into the root game folder, instead of the MODS folder, while the JSON is somehow getting info from it. 
            //This is meant to be a catch-all, marking something in the example box (such as ! or ?), 


            if (boxWasSelecting.SelectedIndex == -1)
            {


                if (m_or_b == "m")
                {
                    //a main music box was called
                    //first verify that it isn't the default song
                    testFindJson.Text += "Default song for Lvl " + whichLvl + " is: " + getDefaultSong(whichLvl, m_or_b);
                    if (boxWasSelecting.Text == getDefaultSong(whichLvl, m_or_b))
                    {
                        //it IS the default song! Just disable the button, don't put ?
                        disableGrabLvlButton(mainLvlGrabButton[whichLvl]);
                        return;
                    }
                    //if we got this far, it's not the default song name
                    disableGrabLvlButton(mainLvlGrabButton[whichLvl], "?");


                }
                else if (m_or_b == "b")
                {
                    //a boss music box was called
                    //first verify that it isn't the boss fight's default song
                    testFindJson.Text += "Default song for Lvl " + whichLvl + " is: " + getDefaultSong(whichLvl, m_or_b);
                    if (boxWasSelecting.Text == getDefaultSong(whichLvl, m_or_b))
                    {
                        //it IS the default song! Just disable the button, don't put ?
                        disableGrabLvlButton(bossLvlGrabButton[whichLvl]);
                        return;
                    }

                    disableGrabLvlButton(bossLvlGrabButton[whichLvl], "?");
                }

            }


            /*



                
            if (boxCalled.Substring(0, 4) == "main")
            {
                //ie mainCombo1
                string boxCalledNumStr = boxCalled.Substring(boxCalled.Length - 1, 1);
                int whichLvl = Int32.Parse(boxCalledNumStr);
                songString = defaultMainSongNames[whichLvl - 1];
                disableGrabLvlButton(mainLvlGrabButton[whichLvl - 1]);
                testFindJson.Text += "Found a main level. Which level: " + boxCalledNumStr + ", parsed as " + whichLvl + "; Song string is: " + songString;
            }
            else if (boxCalled.Substring(0, 4) == "boss")
            {
                //ie mainCombo1
                string boxCalledNumStr = boxCalled.Substring(boxCalled.Length - 1, 1);
                int whichLvl = Int32.Parse(boxCalledNumStr);
                songString = defaultBossSongNames[whichLvl - 1];
                disableGrabLvlButton(mainLvlGrabButton[whichLvl - 1]);
                testFindJson.Text += "Found a boss level";
            }
            */


        }



        //fillDefaultSong is meant to be ran when we want to fill the Combo Box for Main Level or Boss Level
        private string getDefaultSong(int lvlNum, string m_or_b)
        {
            //this is just meant to change the text of the comboBox in question to the default song
            //this needs to be ran if we have a blank field in the comboBox

            //need to update the above comments, after i see if this work


            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton };
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

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                string modList = loadModListWithSubs(setListCatalog); //this loads the contents into the setListCatalog ListBox
                fillSongSelection(modList);
                storeModListInfo(); //this stores the info for our song; specifically which levels it supports
            } else if (tabControl1.SelectedIndex == 1)
            {
                //Changed tab to Organizer

                organizer_enableLevelButtons(false); //a song is no longer selected, so disable buttons
                clearSongInfoBoxes(); //song is no longer selected, clear the song info
                loadModListWithSubs(listBox1);
                listBox1.Items.Insert(0, new ListItem { Name = "Current customsongs.json", Path = gameDir + "\\customsongs.json" });

                currentListSelection = -1; //this is used for confirmation for switching Mods when we have unsaved changes
            }
        }

        private void createjson_Click(object sender, EventArgs e)
        {

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

        public string Injector_GetModJson()
        {
            //this gives us the JSON in its full, unaltered form
            string selectedSong = "";

            //Open Json file and retrieve info
            //check what we're opening
            string currentGameJsonIndicator = ""; //we will change this if we see we're accessing the game's current customsongs.json
            if (listBox1.SelectedItem.ToString() == "Current customsongs.json")
            {
                //we want the current custom song
                //selectedSong = gameDir + "\\customsongs.json"; We get this anyways now
                currentGameJsonIndicator = "<>"; //if we see this at the beginning of our string, we'll know we're accessing the game's current json
            }


            //selectedSong = "\\" + listBox1.SelectedItem.ToString();
            //selectedSong = di + selectedSong + "\\customsongs.json";
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
        public string Organizer_GetModJson()
        {
            //this retrieves the Mod info from Organizer's listbox with all whitespace removed
            string selectedSong = "";

            //Open Json file and retrieve info
            //check what we're opening

            string currentGameJsonIndicator = ""; //we will change this if we see we're accessing the game's current customsongs.json
            //I don't think we need this anymore. We know we needed it when we weren't allowing bankPath changes unless we were on the game's main customsongs.json, but we allow it now
            if (listBox1.SelectedItem.ToString() == "Current customsongs.json")
            {
                //we want the current custom song
                //selectedSong = gameDir + "\\customsongs.json"; We get this anyways now
                currentGameJsonIndicator = "<>"; //if we see this at the beginning of our string, we'll know we're accessing the game's current json
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
                testFindJson.Text = fullText;
                fullText.Replace("\t", " "); //get rid of all indentations
                string trimmedLine = NormalizeWhiteSpace(fullText);
                s = currentGameJsonIndicator + trimmedLine;

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


        string[] currentComboBoxText = new string[16];

        //we're going to make a function that stores all of the text in each field.
        //it gets saved and updated whenever we commit to a change in the box
        //i made the program do this because i don't know how else to get setModGrabLvlSelection to fuck off otherwise when we didn't even change the text box but lost selection or hit enter, etc.
        //I guess I could have just made a boolean turned on or off via a TextChanged event handler
        private void setSongSelectionArray(ComboBox justThisOne = null, string toWhat = "")
        {
            //this doesn't work :(
            //return;

            ComboBox[] allComboBoxes = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8,
                bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7, customMusicBankCombo
            };

            if (justThisOne != null)
            {
                int whichTextBox = Array.FindIndex(allComboBoxes, element => element == justThisOne);//I'm going to try this ONE more time
                if (whichTextBox == -1)
                {
                    testFindJson.Text += "I HATE FIND SO MUCH!!!";
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
            testFindJson.Text += ".SSA Set.";
        }

        private bool wasComboBoxChanged(ComboBox whichComboBox)
        {


            //checks the currentComboBoxText, aka the info that setSongSelectionArray writes
            ComboBox[] allComboBoxes = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8,
                bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7, customMusicBankCombo
            };

            int whichTextBox = Array.FindIndex(allComboBoxes, element => element == whichComboBox);//I'm going to try this ONE more time
            if (whichTextBox == -1) {
                testFindJson.Text += "I HATE FIND SO MUCH!!!";
                //wait this isn't popping up?
                return false;
            }
            if (allComboBoxes[whichTextBox].Text == currentComboBoxText[whichTextBox])
            {
                setSongSelectionArray(allComboBoxes[whichTextBox]);
                testFindJson.Text += ".wCBC..box was same..";
                return false;
            }
            //if we got this far, it didn't match what we had last
            testFindJson.Text += ".wCBC..box was new..";
            //setSongSelectionArray(allComboBoxes[whichTextBox]);
            return true;
        }



        private bool Organizer_checkAndAlertUnsavedChanges(bool sayModName = false)
        {
            //this will return true or false, to tell the L1Settings, L2Settings, etc., to switch to that level or not. True says change it, false says stay on the page.
            //That's all those button care about
            //however, this function will also be what says to Save the information if we're about to close the info by switching to another level 

            if (listBox1.SelectedIndex == -1) { return true; } //nothing's selected
            int levelInt = getSelectedLevel_OrganizerInjector(); //despite this being for the injector, it just tells us the 0-based level number we're on
            if (levelInt == -1) return true; //no level is selected, somehow

            if (!mSaveLevelInfo.Enabled && !bSaveLevelInfo.Enabled) return true; //we don't have anything changed

            Button[] LevelButtons = { L1Settings, L2Settings, L3Settings, L4Settings, L5Settings, L6Settings, L7Settings, L8Settings };
            //we use these ^^ just to make the focus get put back on the button of the level we're on, in the case that we want to cancel switching levels

            //string selectedMod = listBox1.SelectedItem.ToString();

            string Level = levelNames[levelInt].Substring(0, 1).ToUpper() + levelNames[levelInt].Substring(1).ToLower(); //voke->Voke

            string message = "You have unsaved changes for ";
            if (sayModName)
            {
                message += listBox1.Items[currentListSelection].ToString() + " on ";
            }
            message += Level + "!" + Environment.NewLine;
            message += "Would you like to save them?" + Environment.NewLine + "Yes saves the changes, No discards changes," + Environment.NewLine + "Cancel takes you back to ";
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
                //SaveLevelInfo
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
        private void setOldSongsArray()
        {
            string fullModString = getCurrentCustomsongsJson();
            if (fullModString == "-2" || fullModString == "-1")
            {
                gameDirInfo.Text = "No game directory found!";
                return;
            }

            for (int m = 0; m < currentSetListName_m.Length; m++)
            {
                string[] lvlInfoInCurrentJson = getOldJsonLevel(fullModString, levelNames[m], "m");

                string lvlInfoName = lvlInfoInCurrentJson[0];
                if (lvlInfoName == "<default>")
                {
                    lvlInfoName = getDefaultSong(m, "m");
                }

                currentSetListName_m[m] = lvlInfoName;
                string modIndexStr = lvlInfoInCurrentJson[1];
                int modIndex = Int32.Parse(modIndexStr);
                currentSetListIndexes_main[m] = modIndex;


                testFindJson.Text += "OldSong" + m + ": " + modIndex;
            }

            for (int b = 0; b < currentSetListName_b.Length; b++)
            {
                string[] lvlInfoInCurrentJson = getOldJsonLevel(fullModString, levelNames[b], "b");

                string lvlInfoName = lvlInfoInCurrentJson[0];
                if (lvlInfoName == "<default>")
                {
                    lvlInfoName = getDefaultSong(b, "b");
                }

                currentSetListName_b[b] = lvlInfoName;
                string modIndexStr = lvlInfoInCurrentJson[1];
                int modIndex = Int32.Parse(modIndexStr);
                currentSetListIndexes_boss[b] = modIndex;

                testFindJson.Text += "OldSong" + b + ": " + modIndex;
            }


        }


        private void alertLevel(int levelNum, bool alertOn = true)
        {
            //this is used by setList to check 
            //this function just changes the background color of the level selection

            GroupBox[] levelGroupBoxes = { groupBox1, groupBox2, groupBox3, groupBox4, groupBox5, groupBox6, groupBox7, groupBox8 }; //we should never have to mess with groupBox8, but o well
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };
            if (alertOn)
            {
                levelGroupBoxes[levelNum].BackColor = Color.RosyBrown;

                string debug_lvlName = levelNames[levelNum].Substring(0, 1).ToUpper() + levelNames[levelNum].Substring(1); //turns "voke" into "Voke"
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
                testFindJson.Text += "Resetting";
            }

        }

        private bool checkIfTwoModsSelected(int zeroBasedLvlNum)
        {
            if (zeroBasedLvlNum == 7) return false; //if we're on Sheol, we can't have 2 anyways

            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };

            //we're already given the level number, so check from there what combo boxes to compare
            int mainSongIndex = setListCatalog.FindStringExact(mainCBox[zeroBasedLvlNum].Text); //this has proven safer than selectedIndex
            testFindJson.Text += " song1: " + mainSongIndex;
            int bossSongIndex = setListCatalog.FindStringExact(bossCBox[zeroBasedLvlNum].Text); //these are just checking that we have custom songs on the selections for one level
            testFindJson.Text += " song2: " + bossSongIndex;

            if (mainSongIndex > -1 && bossSongIndex > -1) return true;

            return false;
        }

        private bool checkLevelsModsIntegrity(int mainGrabLvlNum, int bossGrabLvlNum, string mainSong, string bossSong)
        {
            //this code is meant to run when we see that we have chosen two custom songs for one level. (we chose one for the main and the boss)
            //we want to grab two things from each song: the file name, and the event ID; if they have different file names and the same event ID, integrity cannot be verified

            if (mainGrabLvlNum == -1) { testFindJson.Text += " A0 "; return true; } //right now, we're only running this checker with verfied Mods. Not when our grabLvlBox has a "?"
            if (bossGrabLvlNum == -1) { testFindJson.Text += " B0 "; return true; } //I guess we COULD get it to run if it had a "?" ...



            string levelStringM = levelNames[mainGrabLvlNum].Substring(0, 1).ToUpper() + levelNames[mainGrabLvlNum].Substring(1); //turns "voke" into "Voke"
            string levelStringB = levelNames[bossGrabLvlNum].Substring(0, 1).ToUpper() + levelNames[bossGrabLvlNum].Substring(1); //turns "voke" into "Voke"

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
            for (int i = 0; i < levelNames.Length; i++)
            {
                if (songName.ToLower() == levelNames[i])
                {
                    return i;
                }
            }
            return -1; //the level name isn't in the list
        }
        private string convertIntToLevelName(int zeroBasedLevelNum)
        {
            return levelNames[zeroBasedLevelNum]; //well this function was pointless
        }

        private string[] getModNameAndID(string fullJson, string Level, string m_or_b)
        {
            //retrieves the info for one Main or Boss music's custom song; this is used for checkLevelsModsIntegrity
            //it is a very similar copy of getSpecificLevelInfo, but decides to only grab the Name and ID instead; it also only grabs main or boss info

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
            //testFindJson.Text = fullLevelInfo;


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

            //our game's main customsongs.json folder is meant to use bankPaths. It is expected that we find one—we return errors if we don't
            // ACTUALLY!!! we're going to take the ListBox item's value, which is its JSON path. Now we're going to have to look through THAT and see if it matches the information! ***
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

            //the way this works right now, there has to be a .bank file next to the JSON
            //otherwise, we don't know how to differentiate the bank and json file
            //maybe i'm just tired

            string modPath = ((ListItem)setListCatalog.Items[modIndex]).Path; //this gives us something that has the CUSTOMSONGS.JSON in it
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

            int indexOfNameB = fullBossMusicInfo.IndexOf("Bank"); //it will look like this: "Bank" : "Unstoppable_All",
            indexOfNameB += 9; //now we're after the first quote of the filename
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

            indexOfBankPath += 12; // bankPath": " <- adds up to 12 characters before we get to the file path
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
                MessageBox.Show("We aren't finding " + allegedModName + ", or the folder doesn't match; listbox length is: " + setListCatalog.Items.Count);
                string[] returnString = { fileNameB, "-2" };
                return returnString;
            }


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




        //this returns an array of strings; it just holds two strings, with the second string saying if the song is a custom song or not (or -2 for unknown?)
        //we COULD also make it return one string, being the mod name, then something to seperate that that can't be in a file name (like < or >), then the custom song index/indicator
        private string[] getOldJsonLevelNoSubs(string fullJson, string Level, string m_or_b)
        {
            //retrieves the info for one level's custom music; it's used to read the actual JSON in the game folder
            //it's either going to give us a directory name(the mod name), or a song title if no directory

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

            //check to see where MainMusic ends, by looking for "BossMusic", or "} }"
            int indexofMainMusicEnd = indexOfBossFightMusic;
            if (indexofMainMusicEnd == -1)
            {
                indexofMainMusicEnd = fullLevelInfo.Length;
            }
            string fullMainMusicInfo = fullLevelInfo.Substring(0, indexofMainMusicEnd);

            int indexOfName = fullMainMusicInfo.IndexOf("Bank"); //it will look like this: "Bank" : "Unstoppable_All",
            indexOfName += 9; //now we're after the first quote of the filename
            int indexOf2ndQuote = fullMainMusicInfo.IndexOf("\"", indexOfName + 1);
            int lengthOfName = indexOf2ndQuote - indexOfName;

            string fileName = fullMainMusicInfo.Substring(indexOfName, lengthOfName);


            //since we now have the file name, let's look for the bankPath
            int indexOfBankPath = fullMainMusicInfo.IndexOf("bankPath");
            if (indexOfBankPath == -1)
            {
                //we don't have a bankPath at all, return the fileName and bail
                string[] returnString = { fileName, "-2" };
                return returnString;

            }

            indexOfBankPath += 12; // bankPath": " <- adds up to 12 characters before we get to the file path

            string fileNameWithExt = fileName + ".bank\""; //just to be sure that a folder name shouldn't mess with this
            int indexOfFileNameInBankPath = fullMainMusicInfo.IndexOf(fileNameWithExt, indexOfBankPath);

            if (indexOfFileNameInBankPath == -1)
            {
                string[] returnString = { fileName, "-2" };
                return returnString;//we can't find a filename for some reason in the bankpath, abort

            }

            int indexOfModsFolderName = fullMainMusicInfo.IndexOf(di.Name, indexOfBankPath);
            if (indexOfModsFolderName == -1)
            {
                string[] returnString = { fileName, "-2" };
                return returnString;//we can't find the program's MODS folder in the bankpath, it's linking to something we don't understand, abort
            }

            //if we got this far, we found a filename in the bank path, and we found our program's MODS (collection of custom songs) folder name in the bankpath

            int indexAfterModsFolderDir = indexOfModsFolderName + di.Name.Length + 2; //+2 because of the / in middle and at the end; this takes us to the end of the MODS folder name; after this is the name of the custom songs's directory
            int pathWithFolderNameLength = indexAfterModsFolderDir - indexOfBankPath;
            int bankPath_pathLength = indexOfFileNameInBankPath - indexOfBankPath;


            string modsPath = di.FullName + "\\"; //this gives us something that looks like R:/SteamLibrary/steamapps/common/Metal Hellsinger/MODS/ (+/ because it's not there without, need it to match thisSongsBankPath)


            string thisSongsBankPath = fullMainMusicInfo.Substring(indexOfBankPath, pathWithFolderNameLength); // this gives us something that looks like R:\\SteamLibrary\\steamapps\\common\\Metal Hellsinger\\MODS\\Unstoppable
            thisSongsBankPath = thisSongsBankPath.Replace("\\\\", "\\"); //and now it doesn't

            if (thisSongsBankPath == modsPath)
            {
                //success! our current JSON file matches with one of our mods
                //instead of writing the file name, write the mod directory
                int lengthOfModDirectory = indexOfFileNameInBankPath - indexAfterModsFolderDir;
                string modName = fullMainMusicInfo.Substring(indexAfterModsFolderDir, lengthOfModDirectory - 2); //this should give us, for example, Unstoppable/, unless we do -1 for length
                int modIndex = setListCatalog.FindStringExact(modName); //this gives us the 0-based index of our mod number. i don't think any of this is necessary

                string[] returnString = { modName, modIndex.ToString() };
                return returnString;//yay


            } else
            {

                //we have a bank path that doesn't match up with the MODS folder. Just return the name
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

            int indexOfNameB = fullBossMusicInfo.IndexOf("Bank"); //it will look like this: "Bank" : "Unstoppable_All",
            indexOfNameB += 9; //now we're after the first quote of the filename
            int indexOf2ndQuoteB = fullBossMusicInfo.IndexOf("\"", indexOfNameB + 1);
            int lengthOfNameB = indexOf2ndQuoteB - indexOfNameB;


            string fileNameB = fullBossMusicInfo.Substring(indexOfNameB, lengthOfNameB);

            //since we now have the file name, let's look for the bankPath
            int indexOfBankPathB = fullBossMusicInfo.IndexOf("bankPath");
            if (indexOfBankPathB == -1)
            {
                //we don't have a bankPath at all, return the fileName and bail
                string[] returnString = { fileNameB, "-2" };
                return returnString;
            }

            indexOfBankPathB += 12; // bankPath": " <- adds up to 12 characters before we get to the file path

            string fileNameWithExtB = fileNameB + ".bank\""; //just to be sure that a folder name shouldn't mess with this
            int indexOfFileNameInBankPathB = fullBossMusicInfo.IndexOf(fileNameWithExtB, indexOfBankPathB);

            if (indexOfFileNameInBankPathB == -1)
            {
                string[] returnString = { fileNameB, "-2" };
                return returnString;//we can't find a filename for some reason in the bankpath, abort
            }

            int indexOfModsFolderNameB = fullBossMusicInfo.IndexOf(di.Name, indexOfBankPathB);
            if (indexOfModsFolderNameB == -1)
            {
                string[] returnString = { fileNameB, "-2" };
                return returnString;//we can't find the program's MODS folder in the bankpath, it's linking to something we don't understand, abort
            }

            //if we got this far, we found a filename in the bank path, and we found our program's MODS (collection of custom songs) folder name in the bankpath

            int indexAfterModsFolderDirB = indexOfModsFolderNameB + di.Name.Length + 2; //+2 because of the / in middle and at the end; this takes us to the end of the MODS folder name; after this is the name of the custom songs's directory
            int pathWithFolderNameLengthB = indexAfterModsFolderDirB - indexOfBankPathB;
            int bankPath_pathLengthB = indexOfFileNameInBankPathB - indexOfBankPathB;


            string modsPathB = di.FullName + "\\"; //this gives us something that looks like R:/SteamLibrary/steamapps/common/Metal Hellsinger/MODS/ (+/ because it's not there without, need it to match thisSongsBankPath)


            string thisSongsBankPathB = fullBossMusicInfo.Substring(indexOfBankPathB, pathWithFolderNameLengthB); // this gives us something that looks like R:\\SteamLibrary\\steamapps\\common\\Metal Hellsinger\\MODS\\Unstoppable
            thisSongsBankPathB = thisSongsBankPathB.Replace("\\\\", "\\"); //and now it doesn't

            if (thisSongsBankPathB == modsPathB)
            {
                //success! our current JSON file matches with one of our mods
                //instead of writing the file name, write the mod directory
                int lengthOfModDirectory = indexOfFileNameInBankPathB - indexAfterModsFolderDirB;
                string modName = fullBossMusicInfo.Substring(indexAfterModsFolderDirB, lengthOfModDirectory - 2); //this should give us, for example, Unstoppable/, unless we do -1 for length
                int modIndex = setListCatalog.FindStringExact(modName); //this gives us the 0-based index of our mod number. i don't think any of this is necessary

                string[] returnString = { modName, modIndex.ToString() };
                return returnString;//yay

            }
            else
            {

                //we have a bank path that doesn't match up with the MODS folder. Just return the name
                string[] returnString = { fileNameB, "-2" };
                return returnString;
            }


        }

        private void resetSongOriginalInfo(string m_or_b)
        {
            TextBox[] mainLevelTextBoxes = { MLNameBox, MLEventBox, MLLHBEBox, MLOffsetBox, MLBPMBox };
            TextBox[] bossFightTextBoxes = { BFNameBox, BFEventBox, BFLHBEBox, BFOffsetBox, BFBPMBox };
            if (m_or_b == "m" || m_or_b == "")
            {
                for (int i = 0; i < storedOriginalInfo_m.Length; i++)
                {
                    storedOriginalInfo_m[i] = mainLevelTextBoxes[i].Text;

                }
                mSaveLevelInfo.Enabled = false;

            }
            if (m_or_b == "b" || m_or_b == "")
            {
                for (int i = 0; i < storedOriginalInfo_b.Length; i++)
                {

                    storedOriginalInfo_b[i] = bossFightTextBoxes[i].Text;
                }
                bSaveLevelInfo.Enabled = false;
            }


        }

        private void songInfoModified(object sender, EventArgs e)
        {
            //this function runs automatically when any text box gets changed in organizer
            TextBox calledTextbox = sender as TextBox;
            string m_or_b = calledTextbox.Name.Substring(0, 1).ToLower();
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
            TextBox[] mainLevelTextBoxes = { MLNameBox, MLEventBox, MLLHBEBox, MLOffsetBox, MLBPMBox };
            TextBox[] bossFightTextBoxes = { BFNameBox, BFEventBox, BFLHBEBox, BFOffsetBox, BFBPMBox };

            if (m_or_b == "m")
            {
                for (int i = 0; i < mainLevelTextBoxes.Length; i++)
                {

                    if (mainLevelTextBoxes[i].Text != storedOriginalInfo_m[i])
                    {
                        modified = true;
                        return modified;
                    }

                }
            }
            else if (m_or_b == "b")
            {

                for (int i = 0; i < bossFightTextBoxes.Length; i++)
                {

                    if (bossFightTextBoxes[i].Text != storedOriginalInfo_b[i])
                    {
                        modified = true;
                        return modified;
                    }

                }
            }

            return modified;
        }


        string[] storedOriginalInfo_m = new string[5]; //we're not going to worry about bankpaths
        string[] storedOriginalInfo_b = new string[5];


        private string getSpecificLevelInfo(string fullJson, string Level)
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
            if (Level == "Sheol" && BFNameBox.Enabled)
            {

                for (int i = 0; i < songInfoLabels.Length; i++)
                {
                    // if (songLabels.Length > songInfoLabels.Length && i == songLabels.Length - 1) continue; //if songLabels has more than songInfoLabels, then we're looking for bankpath.
                    bossFightTextBoxes[i].Enabled = false;


                }
                bPasteLevelInfo.Enabled = false;

            }
            else if (Level != "Sheol" && !BFNameBox.Enabled)
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
            //testFindJson.Text = fullLevelInfo;



            int indexOfMainLevelMusic = fullLevelInfo.IndexOf("\"MainMusic\"");
            int indexOfBossFightMusic = fullLevelInfo.IndexOf("\"BossMusic\"");

            //we're also immediately going to find out if we have a bank path
            //Label bankPathM = mBankPathLabel; nevermind
            //Label bankPathB = bBankPathLabel;

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

                //skip down to bossMusic checker
                goto bossMusicCheck;
            }

            //if we got this far, we DO have a MainMusic entry for this level

            //check to see where MainMusic ends, by looking for "BossMusic", or "} }"
            int indexofMainMusicEnd = indexOfBossFightMusic;
            if (indexofMainMusicEnd == -1)
            {
                indexofMainMusicEnd = fullLevelInfo.Length;
            }
            string fullMainMusicInfo = fullLevelInfo.Substring(0, indexofMainMusicEnd);


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
                        mTrueBankPath.Text = songInfo;
                        songInfo = songInfo.Replace("\\\\", "\\");
                        songInfo = pathShortener(songInfo, 40);
                        songInfo = shaveSurroundingQuotesAndSpaces(songInfo); //this needs to be before we add "bankPath":
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

                //skip down to end checker
                goto skipBossChecker;
            }

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

            string fullBossMusicInfo = fullLevelInfo.Substring(indexOfBossFightMusic);
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

                }



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

                        bTrueBankPath.Text = songInfo;

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

        private void SaveLevelInfo_Organizer(int zeroIndexLevel, string m_or_b, string newInfo)
        {
            if (listBox1.SelectedIndex == -1) { return; } //nothing's selected, we don't know what to change. this shouldn't ever happen

            string jsonSelection = listBox1.SelectedItem.ToString();
            /* We handle this somewhere else
            if(jsonSelection == "Current customsongs.json")
            {
                //we do something else, then return
            }*/



            //if we got this far, we're selecting something
            //we want to check if there's a folder called "Original JSON"
            string thisModsFolder = di + "\\" + jsonSelection;
            string possibleOriginalFolder = thisModsFolder + "\\" + "_Original";
            DirectoryInfo possibleOgFolder = new DirectoryInfo(@possibleOriginalFolder);
            string possibleOriginalJson = thisModsFolder + "\\" + "_Original\\customsongs.json";

            string fullSongJsonInfo = Injector_GetModJson();//gets the entire Json for the mod we're selecting
            if (fullSongJsonInfo.Substring(0, 2) == "<>")
            {
                //if we're editing our game's current customsongs.json, we're not making an "original" folder
                fullSongJsonInfo = fullSongJsonInfo.Substring(2);
                goto EditJson;
            }

            if (!possibleOgFolder.Exists || !File.Exists(possibleOriginalJson))
            {
                //we don't have an "Original" folder, and/or we don't have a JSON file inside of it
                Directory.CreateDirectory(possibleOriginalFolder); //if the directory already exists, this shouldn't do anything
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
                        File.Copy(filename, destinationFile); //copy the original customsongs.json to the "Original" folder

                    }
                }

            }

        //if Original folder already exists, and we skipped the last if statement, all we need to do is edit our already-made Json
        EditJson:


            //testing, get rid of this eventually


            string LvlNameCapd = levelNames[zeroIndexLevel].Substring(0, 1).ToUpper() + levelNames[zeroIndexLevel].Substring(1).ToLower(); //voke->Voke
            string newJson = getJsonWithInjection(fullSongJsonInfo, LvlNameCapd, m_or_b, newInfo);
            Clipboard.SetText(newJson);



            return; //just testing right now, 

            string text = File.ReadAllText("customsongs.json");
            text = text.Replace("some text", "new value");
            File.WriteAllText("customsongs.json", text);
            string currentJSONString = gameDir + "\\customsongs.json";

            string fullJson = "";
            /*
            using (StreamReader sr = File.OpenText(@currentJSONString))
            {
                

                string fullJson = sr.ReadToEnd();
                

                return s;
            }*/


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
            MessageBox.Show(returnString);

            return returnString;
        }

        private string getFullInfoForLevel(string fullJson, string Level)
        {
            //retrieves the entire info block for one level, with all whitespace and linebreaks


            bool isCurrentJson = false;
            //if we see <> at the beginning, it's just an indicator to know we're looking at a Json in the game folder, meaning the one the game's currently using
            if (fullJson.Substring(0, 2) == "<>")
            {
                isCurrentJson = true;//this doesn't do anything at the moment
                fullJson = fullJson.Substring(2);
            }

            string[] jsonLines = fullJson.Split('\n');
            if (jsonLines.Length == 1)
            {
                MessageBox.Show("Hm..");
            }
            int lineWithLevel = findNextLineWith(jsonLines, Level);
            int lastLevelLine = lineWithLevel;//we're about to find out what our last line of our level is

            if (lineWithLevel == -1)
            {
                return "";
            }

            //if we got this far, we have info for this level; either the Main, the Boss, or both
            if (jsonLines[lastLevelLine + 1].Contains("MainMusic"))
            {
                //we have info for the MainMusic on this level
                //that means we have Bank, Event, LowHealth, Offset, and BPM; we also MIGHT have BankPath
                lastLevelLine += 1; //this puts us the "MainMusic" line
                lastLevelLine += 5; //1 would put us on Bank, 5 puts us on the BPM line
                if (jsonLines[lastLevelLine + 1].Contains("bankPath")) lastLevelLine++; //we do have a bankPath, add a line

                lastLevelLine += 1; //this puts us at MainMusic's }

                if (jsonLines[lastLevelLine + 1].Contains("BossMusic"))
                {
                    //we have both MainMusic AND BossMusic in the Json for this Level
                    lastLevelLine += 1; //this puts us at the opening { for "BossMusic"
                    lastLevelLine += 1; //this puts us the "BossMusic" line
                    lastLevelLine += 5; //1 would put us on Bank, 5 puts us on the BPM line
                    if (jsonLines[lastLevelLine + 1].Contains("bankPath")) lastLevelLine++; //we do have a bankPath, add a line
                }


            }
            else
            {
                //the level does NOT have information for the MainMusic
                if (jsonLines[lastLevelLine + 1].Contains("BossMusic"))
                {
                    //we have only BossMusic in the Json for this Level
                    lastLevelLine += 1; //this puts us the "BossMusic" line
                    lastLevelLine += 5; //1 would put us on Bank, 5 puts us on the BPM line
                    if (jsonLines[lastLevelLine + 1].Contains("bankPath")) lastLevelLine++; //we do have a bankPath, add a line

                    lastLevelLine += 1; //this puts us at BossMusic's first closing }
                } else
                {
                    //we don't have boss music, and we don't have main music
                    return "error";
                }
            }

            //wherever we are, we add one more } to close the level
            lastLevelLine++;

            string returnString = "";
            //we want lineWithLevel-1 because we want the { before that
            int numberOfLines = 0;
            for (int i = lineWithLevel - 1; i <= lastLevelLine; i++)
            {
                returnString += jsonLines[i];
                //if (i != lastLevelLine) returnString += "\n"; extra \n extra n
                numberOfLines++;
            }
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
            MessageBox.Show("New Nugget: \n" + newLevelInfoNugget);

            int selectedLevelInt = getSelectedLevel_OrganizerInjector(); //zero-based index, gives us what level we're selecting in Organizer
            string selectedLevelNameCapped = levelNames[selectedLevelInt].Substring(0, 1).ToUpper() + levelNames[selectedLevelInt].Substring(1).ToLower(); //voke->Voke

            bool[] supportedLevels = jsonHasLevelAlready(fullJson); //get an array of booleans, one for each level saying if the level is in the Json already or not

            //if the level we're editing is in the Json already
            if (supportedLevels[selectedLevelInt])
            {
                //the level we're editing had info in the JSON already
                //replace the info
                string replacementBlock = replaceInfoForExistingLevel(fullJson, Level, m_or_b, newLevelInfoNugget); //make a chunk that has new level info
                replacementBlock = replacementBlock.Replace("/n/n", "/n"); //we're going to get rid of all double-returns...

                string originalBlock = getTRUEFullInfoForLevel(fullJson, Level);
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


                string newLevelInfoBlock = getNewLevelInjection(Level, m_or_b, newLevelInfoNugget);

                string returnString = injectNewLevelIntoJson(fullJson, newLevelInfoNugget, selectedLevelInt, m_or_b);
                return returnString;
            }



            //return "";

        }

        private string injectNewLevelIntoJson(string fullJson, string injection, int levelNum, string m_or_b)
        {

            //we want to put the Level information in order, so we're just going to rewrite the whole damn code
            //rearrange the blocks, and put them together.
            //then run through the new code, and add and remove commas as necessary
            string returnString = "{\n";
            returnString += "    \"customLevelMusic\" : [\n";

            for (int i = 0; i < levelNames.Length; i++)
            {
                string injLvlNameCapd = levelNames[i].Substring(0, 1).ToUpper() + levelNames[i].Substring(1).ToLower(); //voke->Voke

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

                string queriedLvlNameCapd = levelNames[i].Substring(0, 1).ToUpper() + levelNames[i].Substring(1).ToLower(); //voke->Voke
                if (queriedLvlNameCapd.Contains(queriedLvlNameCapd))
                {
                    returnString += getFullInfoForLevel(fullJson, queriedLvlNameCapd); //this gives us the chunk of the level with all the whitespace and linebreaks
                }

            }
            returnString += "    ]\n";
            returnString += "}";

            returnString.Replace("        }\n        {\n", "        },\n        {\n");//this will make sure there are commas between levels

            returnString.Replace("        },\n    ]\n}", "        }\n    ]\n}"); //this will make sure to remove a comma if we had one on the last level

            return returnString;
        }

        private int getSelectedLevel_OrganizerInjector()
        {
            //this returns the zero-based index of what level is currently selected in Organizer
            //it's used by the injector to know what level we're injecting our new info into

            Button[] LevelButtons = { L1Settings, L2Settings, L3Settings, L4Settings, L5Settings, L6Settings, L7Settings, L8Settings };

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
            string fullInfo = getFullInfoForLevel(fullSongJsonInfo, Level); //level must be capped; this gives us the chunk of the level with all the whitespace and linebreaks

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

                    bool addingNewLine = false;

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

                            } else
                            {
                                //we don't have a bank path in here, we need to stop this code before it hits the code to replace the line
                                addingNewLine = true;
                                break;
                            }

                        }

                        levelChunkLines[lineWereOn] = newLevelInfoLines[i];

                        lineWereOn++;
                    }

                    if (addingNewLine)
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
                        returnString += levelChunkLines[i];
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
                    if (!newLevelInfoLines[0].Contains("Bank")) MessageBox.Show("ProblemA");
                    if (newLevelInfoLines.Length == 1) MessageBox.Show("ProblemB");


                    //newLevelInfoLines.Length can be 5 or 6, if we have a bankPath or not
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
                    bool addingNewLine = false; //for bankPath
                    string[] newLevelInfoLines = newLevelInfoNugget.Split('\n'); //the first line is going to be "Bank":

                    lineWereOn += 6; //+1 puts us on Bank, +5 puts us on BPM, +6 is either "bankPath", or MainMusic's closing }
                    if (levelChunkLines[lineWereOn].Contains("bankPath"))
                    {
                        lineWereOn += 1;
                    }
                    //now we're 100% on MainMusic's closing }
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


                        if (addingNewLine)
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
                if (line != null && line != "")
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

            returnString += "            }" + returnLine; //closes Main/BossMusic
            returnString += "         }," + returnLine; //closes the level, adding a comma. getJsonWithInjection will remove it if it's not correctly placed

            return returnString;
        }

        private bool[] jsonHasLevelAlready(string fullJson)
        {
            //returns 8 booleans, each saying if the json has information for its respective zero-based level number
            bool[] levelSupported = new bool[8];


            for (int i = 0; i < levelNames.Length; i++)
            {
                string cappedLvlName = levelNames[i].Substring(0, 1).ToUpper() + levelNames[i].Substring(1).ToLower();
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

            if (bankPath.Contains("\n")) MessageBox.Show("FOUND IT"); //delete me
            if (bpm.Contains("\n")) MessageBox.Show("FOUND IT"); //delete me

            string returnString = "";
            string returnLine = "\n";

            string bankInfo = shaveSurroundingQuotesAndSpaces(bank);
            string eventIDInfo = shaveSurroundingQuotesAndSpaces(eventID);
            string lowHealthIDInfo = shaveSurroundingQuotesAndSpaces(lowHealthID);
            string offsetInfo = shaveSurroundingQuotesAndSpaces(offset); //there shouldn't be quotes, but... just in case
            string bpmInfo = shaveSurroundingQuotesAndSpaces(bpm); //there shouldn't be quotes here either

            string bankPathInfo = "";
            if (bankPath != "") { bankPathInfo = shaveSurroundingQuotesAndSpaces(bankPath); }
            //bankPathInfo = CheckAndFixBankPath(bankPathInfo);
            if (bankPathInfo == "error") return "ERROR";


            returnString += "                \"Bank\" : \"" + bank + "\"," + returnLine;
            returnString += "                \"Event\": \"" + eventID + "\"," + returnLine;
            returnString += "                \"LowHealthBeatEvent\": \"" + lowHealthID + "\"," + returnLine;
            returnString += "                \"BeatInputOffset\": " + offset + "," + returnLine;
            returnString += "                \"BPM\": " + bpm.Trim();

            if (bankPath == "")
            {
                //we don't have a bank path 
                //returnString += returnLine; we don't need this
            } else
            {
                //we DO have something in the bankpath
                returnString += "," + returnLine;
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
                testFindJson.Text += "Dirs Length:" + dirs.Length;

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
            Label mBankPath = mBankPathLabel;
            Label bBankPath = bBankPathLabel;

            string copyString = "";
            string separator = "|"; //y'gotta keep 'em separated

            if (b.Name.Substring(0, 1).ToLower() == "m")
            {

                for (int i = 0; i < mainLevelTextBoxes.Length; i++)
                {
                    copyString += mainLevelTextBoxes[i].Text;
                    copyString += separator;
                }

                //if the bankPath length is greater than "bankPath: " (no quotes)
                if (mBankPath.Text.Length > 10)
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
                if (bBankPath.Text.Length > 10)
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
                levelInfoSplit[i] = levelInfoSplit[i].Replace(":", ""); //gets rid of the colon that was after our label

                if (i < 2 && i > 5)
                {
                    //if i is either 3 or 4, meaning we're at the line for Offset or BPM
                    levelInfoSplit[i] = levelInfoSplit[i].Replace(",", ""); //gets rid of a , if we had it at the end
                } else
                {
                    //if i is pointing at Bank, Event, BPM, or bankPath
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
            Label mBankPath = mBankPathLabel;
            Label bBankPath = bBankPathLabel;

            string levelInfoFullText = Clipboard.GetText();
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

                mBankPath.Text = "bankPath: " + shortPath;

                string last5characters = mTrueBankPath.Text.Substring(mTrueBankPath.Text.Length - 5);//we can and probably should do mTrueBankPath...
                if (last5characters != ".bank") {
                    bankPathRedAlert(mBankPath, 1);
                    testFindJson.Text += " XX " + last5characters + " X ";
                    return;
                }//if they user copied something that didn't have the .Bank file in it, alert them

                if (!verifyFileExists(mTrueBankPath.Text)) {
                    //could not verify the file exists
                    bankPathRedAlert(mBankPath);
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

                bBankPath.Text = "bankPath: " + shortPath;

                string last5characters = bTrueBankPath.Text.Substring(bTrueBankPath.Text.Length - 5);//we can and probably should do mTrueBankPath...
                if (last5characters != ".bank")
                {
                    bankPathRedAlert(bBankPath, 1);
                    return;
                }//if they user copied something that didn't have the .Bank file in it, alert them

                if (!verifyFileExists(bTrueBankPath.Text))
                {
                    //could not verify the file exists
                    bankPathRedAlert(bBankPath);
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



        private void organizer_enableLevelButtons(bool enableButtons = true)
        {
            //this resets the colors of the level buttons in Organizer; resetting their colors and making them Disabled (before a level is selected)
            Button[] LevelButtons = { L1Settings, L2Settings, L3Settings, L4Settings, L5Settings, L6Settings, L7Settings, L8Settings };

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
            for (int i = 0; i < LevelButtons.Length; i++)
            {
                LevelButtons[i].BackColor = Color.Transparent;
                LevelButtons[i].ForeColor = Color.Black;
                LevelButtons[i].Enabled = false;
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
            //testFindJson.Text = "IndexOfSongName: " + indexOfSongName + "; IndexOfSongNameEnd: " + indexOfSongNameEnd + "; Song name: " + songName;

            if (indexOfLevelInfo != -1)
            {
                MLNameBox.Text = songName;
            }

            string[] bunkSongInfo = { "hello", "world" };
            return bunkSongInfo;
        }


        string[] currentSetListName_m = new string[8];
        string[] currentSetListName_b = new string[7];

        int[] currentSetListIndexes_main = { -1, -1, -1, -1, -1, -1, -1, -1 }; //-1 if default song; otherwise, this holds the index # of the Mod in the set list
        int[] currentSetListIndexes_boss = { -1, -1, -1, -1, -1, -1, -1 }; //same as ^^

        private bool inputtedLevelMatchesOld(int changedCBox, string m_or_b, int changedSelectionIndex)
        {
            //this is ran after we've changed the selection of a Level's main or boss Combo box
            //we're trying to see if the field we just changed matches what we already have (already in the JSON)
            //this is to help the user create more variety in what they've already been using



            if (m_or_b == "m")
            {
                testFindJson.Text += "Old index: " + currentSetListIndexes_main[changedCBox];
                if (changedSelectionIndex == currentSetListIndexes_main[changedCBox])
                {
                    return true;
                }
            } else if (m_or_b == "b")
            {
                testFindJson.Text += "Old index: " + currentSetListIndexes_boss[changedCBox];
                if (changedSelectionIndex == currentSetListIndexes_boss[changedCBox])
                {
                    return true;
                }
            }

            return false;

        }

        private bool autoSelectOn()
        {
            if (autoSelectGrabLvl.Checked)
            {
                return true;
            } else
            {
                return false;
            }
        }

        //gives us 2 ints: the first tells us which level we want now; the 2nd tells us if we had to change off of main or boss (0 means we didn't have to, 1 means we changed)
        private int[] getNextBestChoice(int modIndex, int whichLevel, string m_or_b)
        {
            //this function will give us the 0-based Level number of the next best option for the level we want, as apparently it's unavailable
            string supportedLvlString = csSupLvls[modIndex]; //gives us the supported levels string of the mod that's selected
            //int levelInfoIndex = supportedLvlString.IndexOf(whichLevel.ToString());//this is going to give us the spot right before the level number. After the number is m, b, mb, or (nothing, next number)
            //we don't use this

            testFindJson.Text += "Support for Mod " + modIndex + ": " + supportedLvlString + "...";

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
                for (int i = 0; i < 8; i++)
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
                    if (indexOfConsideredLevel >= supportedLvlString.Length - 2) { testFindJson.Text += "no support on lvl" + zeroBasedLevel; continue; }//supportedLvlString.Length - 2 would be here -> 12345b6mb7*mb 12345b6m*b7 (it can't be 12345b6mb*7b, or the last 'if' would have seen it)
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
                    testFindJson.Text += "onlyFoundOn(" + onlySupportedLvl + ") ";
                    int[] returnStrings = { onlySupportedLvl, 0 };
                    return returnStrings;
                }

                //if we got this far, we have more than one option for "b"


                //because there's 7 boss levels
                for (int i = 0; i < 7; i++)
                {
                    int[] ourNextBestOptions = bossfightDefaultSlcts[whichLevel];
                    testFindJson.Text += "..NBO: " + ourNextBestOptions[0] + ourNextBestOptions[1] + ourNextBestOptions[2] + ourNextBestOptions[3] + ourNextBestOptions[4] + ourNextBestOptions[5] + ourNextBestOptions[6];
                    int zeroBasedLevel = ourNextBestOptions[i];
                    zeroBasedLevel -= 1;
                    testFindJson.Text += "testingLvlA(" + zeroBasedLevel + ")->";
                    int indexOfConsideredLevel = supportedLvlString.IndexOf(zeroBasedLevel.ToString());
                    if (indexOfConsideredLevel == supportedLvlString.Length - 1) continue; //we're at the last, which doesn't support anything
                    if (supportedLvlString.Substring(indexOfConsideredLevel + 1, 1) == "b")
                    {
                        int[] returnStrings = { zeroBasedLevel, 0 };
                        testFindJson.Text += "found on levelA(" + levelNames[zeroBasedLevel] + ", supportString: " + supportedLvlString + ")";
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
                        testFindJson.Text += "found on levelB(" + levelNames[zeroBasedLevel] + ")";
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
                        testFindJson.Text = "found on levelC(" + zeroBasedLevel + ")";
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
            new int[] {1, 3, 6, 7, 2, 4, 5, 8},
            new int[] {2, 4, 5, 8, 1, 3, 6, 7},
            new int[] {3, 1, 6, 7, 2, 4, 5, 8},
            new int[] {4, 2, 5, 8, 1, 3, 6, 7},
            new int[] {5, 2, 4, 8, 1, 3, 6, 7},
            new int[] {6, 7, 3, 1, 2, 4, 5, 8},
            new int[] {7, 6, 3, 1, 2, 4, 5, 8},
            new int[] {8, 2, 4, 5, 1, 3, 6, 7},
        };
        //this next set of arrays are the BOSS levels to grab FOR OUR MAIN if we cannot find anything from our mains
        int[][] mainlevelBOSSDefaultSlcts =
        {
            new int[] {1, 2, 3, 4, 5, 6, 7},
            new int[] {2, 3, 4, 5, 6, 7, 1},
            new int[] {1, 3, 2, 4, 5, 6, 7},
            new int[] {4, 2, 3, 5, 6, 7, 1},
            new int[] {5, 2, 3, 4, 6, 7, 1},
            new int[] {1, 5, 2, 3, 4, 6, 7},
            new int[] {1, 6, 2, 3, 4, 5, 7},
            new int[] {7, 2, 3, 4, 5, 6, 1},
        };
        int[][] bossfightDefaultSlcts =
        {
            new int[] {1, 2, 3, 4, 5, 6, 7},
            new int[] {2, 3, 4, 5, 6, 7, 1},
            new int[] {3, 2, 4, 5, 6, 7, 1},
            new int[] {4, 2, 3, 5, 6, 7, 1},
            new int[] {5, 2, 3, 4, 6, 7, 1},
            new int[] {6, 2, 3, 4, 5, 7, 1},
            new int[] {7, 2, 3, 4, 5, 6, 1},
        };
        //this next set of arrays are the MAIN levels to grab for our BOSS fights if we could not find anything from our bosses
        int[][] bossfightMAINDefaultSlcts =
        {
            new int[] {3, 1, 6, 7, 2, 4, 5, 8},
            new int[] {2, 4, 5, 8, 1, 3, 6, 7},
            new int[] {2, 4, 5, 8, 1, 3, 6, 7},
            new int[] {4, 2, 5, 8, 1, 3, 6, 7},
            new int[] {5, 2, 4, 8, 1, 3, 6, 7},
            new int[] {2, 4, 5, 8, 1, 3, 6, 7},
            new int[] {2, 4, 5, 8, 1, 3, 6, 7},
        };

        string[] LvlAbbreviations = { "V", "St", "Y", "I", "G", "N", "A", "Sh" };


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

            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };

            string boxCalledNumStr = musicBeingChanged.Substring(musicBeingChanged.Length - 1, 1);
            int whichLvl = Int32.Parse(boxCalledNumStr);
            whichLvl -= 1; //since named our buttons 1-8, instead of 0-7; but our array is indexed at 0-7

            if (musicBeingChanged.Substring(0, 4) == "main")
            {
                //need to match all of this for the boss

                mainLvlGrabButton[whichLvl].Font = radioButton3.Font; //we're taking away the bold

                testFindJson.Text += "WhichLvl: " + whichLvl.ToString() + "; m; " + "SelectedIndex: " + changedBox.SelectedIndex.ToString() + "; ";

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
                testFindJson.Text += "Finding index for " + changedBox.Text;



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
                    if (!wasComboBoxChanged(getComboFromGrabLvlBtn(mainLvlGrabButton[whichLvl]))) {
                        return;
                    }
                    testFindJson.Text += "SUPPORTED! ";
                    string hi = mainLvlGrabButton[whichLvl].Text;
                    testFindJson.Text += "\n!sMGLS: Before, box said: " + hi;
                    mainLvlGrabButton[whichLvl].Text = LvlAbbreviations[whichLvl];
                    testFindJson.Text += ", but now it says: " + mainLvlGrabButton[whichLvl].Text;
                    mainLvlGrabButton[whichLvl].Enabled = true;
                    mainLvlGrabButton[whichLvl].Image = null;
                } else
                {
                    mainLvlGrabButton[whichLvl].Image = null;
                    if (autoSelectOn())
                    {
                        int[] autoSelectInfo = getNextBestChoice(changedBoxIndex, whichLvl, "m");

                        testFindJson.Text += "PLZ(" + autoSelectInfo[0] + ") ";
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
                    testFindJson.Text += "SUPPORTED! ";
                    string hi = mainLvlGrabButton[whichLvl].Text;
                    testFindJson.Text += "\n!sMGLS: Before, box said: " + hi;
                    bossLvlGrabButton[whichLvl].Text = LvlAbbreviations[whichLvl];
                    testFindJson.Text += ", but now it says: " + mainLvlGrabButton[whichLvl].Text;
                    bossLvlGrabButton[whichLvl].Enabled = true;
                    bossLvlGrabButton[whichLvl].Image = null;
                }
                else
                {
                    bossLvlGrabButton[whichLvl].Image = null;
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
            testFindJson.Text += "Sender: " + sender;
            CheckBox chkB = sender as CheckBox;
            setSelectionFromCheck(chkB);
        }

        private void setSelectionFromCheck(CheckBox chkBox)
        {
            //this changes our selection and GrabLvlButton based on what we just did to our checkbox
            //this assumes we just clicked or changed a checkbox

            if (chkBox == null) return;

            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8 };
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
                    //setSongSelectionArray(mainCBox[lvlNum]);
                    testFindJson.Text += ".ssFC.";
                } else if (id == "b")
                {
                    setModGrabLvlSelection(bossCBox[lvlNum]);
                    //setSongSelectionArray(bossCBox[lvlNum]);
                    testFindJson.Text += ".ssFC.";
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


                    testFindJson.Text += "Boxtext is now: " + mainCBox[lvlNum].Text + " and our old set list was " + currentSetListName_m[lvlNum];
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

                    testFindJson.Text += " b_ " + currentSetListName_b[lvlNum];
                }

            }
        }

        //this will still run after we reset our selection by clicking the checkbox
        private void setCheckFromSelection(ComboBox cBox)
        {
            //I don't know what I was thinking with the function below
            //This is just meant to look at our combo box, and verify if we're changing a song
            //if we change it to song that was already in our game's current Json file, we don't want a check, UNTIL the player sets a Level
            //otherwise, if we're changing to a custom song that WASN'T in our game's current Json, we want a checkmark
            //if we change it to a default song, we need to verify our game's current Json file DIDN'T have anything there. if it did, put a check because we're changing it
            //cBox should be a song selection box that just got changed, so this function is meant to react to that decision

            //we need to see what our comboBox just set its selection to
            int selectedSong = cBox.SelectedIndex;
            //find out where this was sent from, and compare the old values
            string boxType = cBox.Name.Substring(0, 1);
            string boxIDNumStr = cBox.Name.Substring(cBox.Name.Length - 1, 1);
            int boxIDNum = Int32.Parse(boxIDNumStr);
            boxIDNum -= 1; //we want 0-based index from our 1-based comboBox#'s

            CheckBox[] mainCheckBoxes = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8 };
            CheckBox[] bossCheckBoxes = { checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };

            if (selectedSong > -1)
            {
                //we have a custom song selected; need to make sure it wasn't there already
                if (boxType == "m")
                {
                    //compare to mains
                    if (currentSetListIndexes_main[boxIDNum] == selectedSong)
                    {
                        //our selected song matches our current JSON's song; don't do a check, and make the GrabLvl button text say " "
                        mainCheckBoxes[boxIDNum].Checked = false;
                        enableGrabLvlButton(cBox, " ");
                        string levelNameCapd = levelNames[boxIDNum].Substring(0, 1).ToUpper() + levelNames[boxIDNum].Substring(1);
                        if (cBox.Focused)
                        {
                            //this command was called by the combo box, not the check box

                            SetList_DebugLabel1.Text = "Your selection for " + levelNameCapd + "'s Main Music matches the old Set List's song; we assume you want to keep old info.";
                            SetList_DebugLabel2.Text = "Check the box on the left or click the box on the right and choose a level to pull new info from the mod.";
                            SetList_DebugLabel3.Text = "";
                            testFindJson.Text += "BoxID: " + boxIDNum + "; " + currentSetListIndexes_main[boxIDNum] + "==" + selectedSong;
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
                        string levelNameCapd = levelNames[boxIDNum].Substring(0, 1).ToUpper() + levelNames[boxIDNum].Substring(1);
                        if (cBox.Focused)
                        {
                            //this command was called by the combo box, not the check box

                            SetList_DebugLabel1.Text = "Your selection for " + levelNameCapd + "'s Boss Music matches the old Set List's song; we assume you want to keep old info.";
                            SetList_DebugLabel2.Text = "Check the box on the left or click the box on the right and choose a level to pull new info from the mod.";
                            testFindJson.Text += "BoxID: " + boxIDNum + "; " + currentSetListIndexes_boss[boxIDNum] + "==" + selectedSong;
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


        }

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
                        if (nextLineNS.Contains("MainMusic"))
                        {
                            errorsOnLine.Add("NL_bossnotmain"); //next line, we want boss not main music
                        }

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
                    if (Decimal.TryParse(valueNS.Trim(), out decimal hi))
                    {
                        //the value is a number
                    }
                    else
                    {
                        //the value is NOT a number
                        lineFormatErrors.Add("numFormat");
                    }
                }
            }
            return lineFormatErrors.ToArray();
        }


        string[] expectedEndings = { "{", ",", "{", ",", ",", ",", ",", ",or ", "bnkp", "}or,", "}or," };
        //                            L   LN   MM   Ba   Ev   LH   Ofs   BPM*  BnkPth   MMcBMc  Lc
        //                                      ^-----<----------<------------<--------' if(,)

        //                          0          1                       2                    3            4                   5                        6              7            8          9    10
        string[] expectedFields = { "{", "\"LevelName\"", "\"MainMusic\"|\"BossMusic\"", "\"Bank\"", "\"Event\"", "\"LowHealthBeatEvent\"", "\"BeatInputOffset\"", "\"BPM\"", "\"bankPath\"", "}", "}" };


        //this doesn't work
        //i'm a bad programmer
        //this will return an array of numbers representing the 0-based line numbers that had a problem with the JSON somehow
        #region DeprecatedDebugger
        private string[] getJsonErrors(string fullJson)
        {
            string fixedJson = fullJson;
            List<string> linesWithErrors = new List<string>();

            if (fixedJson.Contains("/t"))
            {
                linesWithErrors.Add("...:_tabs");
            }

            //string[] fixedJsonLines = fixedJson.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries); //split our Json into an array of strings, line by line
            string[] fixedJsonLines = fixedJson.Split('\n'); //we're going to keep our empty lines, to be sure what line we're on

            if (fixedJsonLines.Length == 1)
            {
                //we couldn't find any line breaks using \n, we'll try to split it with \r
                //fixedJsonLines = fixedJson.Split(new string[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
                fixedJsonLines = fixedJson.Split('\r');
            }
            if (fixedJsonLines.Length == 1)
            {
                //there's either an error, or the mod creator is a psychopath and didn't put line returns
                string[] returnStringsError = { "...:noLines" };
                return returnStringsError;
            }

            //if we got this far, we have line breaks

            //we need the for loop so we can look back at our previous
            //unless there's a way to do that with a foreach loop, then it's because i don't know how to do that
            string[] openersAndClosers = { "{", "[", "]", "}" };


            // ORIGINALLY:(no bankP)        0    1    2    3    4    5    6     7       8       9
            //string[] expectedEndings = { "{", ",", "{", ",", ",", ",", ",", ",or ", "}or,", "}or," };
            //                              L   LN   MM   Ba   Ev   LH   Ofs   BPM*   MMc       Lc
            //                                      ^-----<----------<------------<--' if(,)


            /*
            //                            0    1    2    3    4    5    6     7       8       9     10
            string[] expectedEndings = { "{", ",", "{", ",", ",", ",", ",", ",or ", "bnkp", "}or,", "}or," };
            //                            L   LN   MM   Ba   Ev   LH   Ofs   BPM*  BnkPth   MMcBMc  Lc
            //                                      ^-----<----------<------------<--------' if(,)


            //                          0          1                       2                    3            4                   5                        6              7            8          9    10
            string[] expectedFields = { "", "\"LevelName\"", "\"MainMusic\"|\"BossMusic\"", "\"Bank\"", "\"Event\"", "\"LowHealthBeatEvent\"", "\"BeatInputOffset\"", "\"BPM\"", "\"bankPath\"", "", "" };
            */


            int lastLineNum = fixedJsonLines.Length - 1;
            //right now, we're going to find our last line that isn't blank
            //we'll start at the end, and work backwards until what we find isn't 
            string chekd = "";
            for (int i = fixedJsonLines.Length - 1; i > 0; i--)
            {
                chekd += i;
                string finalLinesChk_ns = NormalizeWhiteSpace(fixedJsonLines[i], true);
                finalLinesChk_ns = finalLinesChk_ns.Replace("\r", "").Replace("\n", "");
                if (finalLinesChk_ns != null || finalLinesChk_ns != "")
                {
                    chekd += "→";
                    if (finalLinesChk_ns == "}")
                    {
                        //we're golden
                        lastLineNum = i;
                        chekd += "}";

                        break;
                    } else if (finalLinesChk_ns == "]")
                    {
                        //JSON doesn't have final }, but the ] is there
                        lastLineNum = i + 1; //we're going to handle everything once we see the ]
                        chekd += "]";

                        break;
                    } else if (finalLinesChk_ns.Length > 0)
                    {
                        //the final character in the JSON is something wacky...
                        chekd += "sad(" + lastLineNum + ")";
                        lastLineNum = -1;//if we see this is a negative, it means we couldn't find the lastLineNum
                        //if we can't know that we're on the last few lines because this failed, our code should keep running and returning "42: empty line, 43: empty line" until the end, where
                        // we should mention "Json is not closed correctly; Expected } on last line, with ] on previous line
                    }


                }

            }
            MessageBox.Show("Chk: " + chekd);

            int[] instancesOfLevelInJson = new int[allLevelNames.Length]; //we'll use this to detect if we have more than one instance of a level in the JSON


            //int currentPlaceInExpEndLoop = 0; //we're going to use this to route where to be looking for punctuation in our expectedEndings
            int currentPlaceInExpFldLoop = 0; //we're going to use this to route where to be looking for our actual label fields
                                              //we don't need two of these anymore, since we have the same length on the arrays

            //this is about to get stupid
            //
            for (int i = 0; i < fixedJsonLines.Length; i++)
            {
                //resetting these if they're over the mark
                if (currentPlaceInExpFldLoop >= expectedFields.Length) currentPlaceInExpFldLoop = 0;
                //if (currentPlaceInExpEndLoop >= expectedEndings.Length) currentPlaceInExpEndLoop = 0;




                //after preliminaries...

                //first, check and see if the line is just blank
                //if we continue, we go to the next line, assuming that we need to stay on where we are in the 
                if (fixedJsonLines[i] == null)
                {
                    linesWithErrors.Add(i + ":_empty");
                    continue;
                }

                string line_nospaces = NormalizeWhiteSpace(fixedJsonLines[i], true); //gives us us the individual line in the JSON, with no spaces whatsoever
                string finalChar = line_nospaces.Substring(line_nospaces.Length - 1, 1); //we only care about the last item on the line


                //before we check anything, we want to see if we're looking for a BankPath, or we're on MainMusic/BankMusic closing }
                #region VerifyPlacement
                if (expectedEndings[currentPlaceInExpFldLoop] == "bnkp")
                {
                    //expecting bankPath
                    //However, we may not have one, so we'll immediately look if we need to fix our placement in our expected fields/endings
                    //if we DON'T have a bankpath, this line will either be }, or }

                    //I seriously need to rewrite this. It was easier to write it this way, but it's not easier to read
                    if (line_nospaces.Contains("\"bankPath\""))
                    {
                        //it has a bankPath in it, there's no issues, let the for loop run
                    } else {
                        //there's no bankPath, we need to know if we need it, or need to skip the check for it
                        //if we know there's no bankPath, we should go back a few lines and check our "Bank": value, then use it to verify the file exists
                        if (line_nospaces.Contains(":\\") || line_nospaces.Contains(":/"))
                        {
                            //the user TRIED to put in a bankPath, but mispelled bankPath or something, continue letting loop run
                        }

                        if (line_nospaces.Contains("}"))
                        {
                            //we've confirmed that we're just closing out the Main/BossMusic
                            //we'll set these so we know what to check for in the remainder of the forLoop
                            currentPlaceInExpFldLoop++;


                        } else
                        {
                            //We couldn't find a "}" on this line, 
                            //we'll look down 3 more times and see if we can find it
                            int nextLineWithCurlyClose = nextValidLineContaining("}", fixedJsonLines, i, 3);
                            if (nextLineWithCurlyClose == -1)
                            {
                                //we still couldn't find another }, the user might have accidentally ommitted } and went straight to boss music
                                //otherwise, unless the user forgot the level closing } too, something doesn't make sense, throw fatal error
                                int lineWithBoss = nextValidLineContaining("\"BossMusic\"", fixedJsonLines, i, 3); //this is 0 if it's the same line
                                if (lineWithBoss == i)
                                {
                                    //we're on the BossMusic line
                                    //we can confirm what's going on: the user forgot to add the } after MainMusic
                                    linesWithErrors.Add(i + ":mcm"); //music close missing
                                    currentPlaceInExpFldLoop = 2; //set these back to know what to look for

                                } else if (lineWithBoss == -1)
                                {
                                    //after 3 attempts, we couldn't find a "BossMusic" line underneath us, we have no idea where we're at
                                    linesWithErrors.Add(i + "ftl"); //fatal error, could not verify line index
                                    return linesWithErrors.ToArray();
                                } else
                                {
                                    //we found the BossMusic line somewhere within our 3 attempts, we're back on track
                                    linesWithErrors.Add(i + ":mcm"); //music close missing
                                    //We also need to have the lines throw "empty" errors in, but they should do that anyways; we can at least confirm that debugger won't be lost after a few lines
                                    currentPlaceInExpFldLoop = 2; //set these back to know what to look for

                                }

                            } else
                            {
                                //we found another }, though it might just be closing out the level instead of the Music
                                if (nextLineWithCurlyClose >= lastLineNum)
                                {
                                    //we're closing out the whole JSON, which means we didn't close out the music (or level??)
                                    linesWithErrors.Add(i + ":mcm"); //music close missing
                                    //shouldn't ever be 'greater than', but just in case...?
                                } else
                                {
                                    //where we landed is somewhere before the lastLineNum, so we can look forward to figure out where we are
                                    if ((fixedJsonLines[nextLineWithCurlyClose + 1].Contains("]") && nextLineWithCurlyClose == lastLineNum - 1) ||
                                        fixedJsonLines[nextLineWithCurlyClose + 1].Contains("{"))
                                    {
                                        //we found level openers
                                        //we can confirm we're closing out the level, but we forgot to close the music
                                        linesWithErrors.Add(i + ":mcm"); //music close missing
                                    } else
                                    {
                                        //we can't confirm what this } is doing, nor what's going on, so throw fatal error
                                        linesWithErrors.Add(i + "ftl"); //fatal error, could not proceed because we don't know where we're at
                                    }
                                }

                            }


                        }

                        //end of "if Line does/doesn't contain "bankPath" label"
                    }
                    //end of if expectedField is "bankPath"
                }
                else if (currentPlaceInExpFldLoop == 9)
                {
                    MessageBox.Show("[Ln" + i + ":" + line_nospaces + "]");

                } else if (currentPlaceInExpFldLoop == 10)
                {
                    //we're here if our expectedField is supposed to be a Level-Closing } — if we had MainMusic and BossMusic, though, this could be opening back up again

                    int nextLineWithCurlyClose = nextValidLineContaining("}", fixedJsonLines, i, 2);


                    if (fixedJsonLines[i].Contains("\"BossMusic\""))
                    {
                        currentPlaceInExpFldLoop = 10;
                    } else if (fixedJsonLines[i].Contains("}"))
                    {
                        //we let it go
                    }



                    /*
                    if (line_nospaces.Contains("BossMusic"))
                    {
                        currentPlaceInExpFldLoop = 2;
                    } else if (nextLineWithCurlyClose == i)
                    {
                        //we're fine, we're just closing out the level as intended, let the function keep running
                    } else if (nextLineWithCurlyClose == -1)
                    {
                        //we don't have a } to close out the level, but we may be on MainMusic, about to be opening up for BossMusic
                        int nextLineWithBoss = nextValidLineContaining("\"BossMusic\"", fixedJsonLines, i, 3);
                        if (nextLineWithBoss == i)
                        {
                            //confirmed, we're fine: we're opening the BossMusic instead, but need to set currentPlaceInExp so we know where we are
                            currentPlaceInExpFldLoop = 2;
                            currentPlaceInExpFldLoop = 2;
                        } else if (nextLineWithBoss == -1)
                        {
                            //okay, we don't have the Level CLosing } on this line, and we can't find the boss music at all
                            //did we forget to add BossMusic line? Did we put MainMusic instead? (Is the level closing } further down? if it is further down, we already do something about it)
                            //check if we forgot to add MusicOpener line
                            if (line_nospaces.Contains("\"Bank\""))
                            {
                                //we can confirm we just forgot to add BossMusic opener
                                linesWithErrors.Add(i + ":mom"); //music opener missing
                                currentPlaceInExpFldLoop = 3;
                            } else if (line_nospaces.Contains("\"MainMusic\""))
                            {
                                //we can confirm that the user wrote MainMusic instead of BossMusic
                                linesWithErrors.Add(i + ":bnm"); //boss not main music
                                currentPlaceInExpFldLoop = 2;
                                currentPlaceInExpFldLoop = 2;
                            }
                            else if (line_nospaces.Contains("{") || line_nospaces.Contains("[") || line_nospaces.Contains("]"))
                            {
                                //it's possible the user put the wrong curly bracket here, which we're checking now
                                linesWithErrors.Add(i + ":lcm"); //level close missing
                                currentPlaceInExpFldLoop = 2;
                                currentPlaceInExpFldLoop = 2;
                            }
                            else
                            {
                                //we can't find a } at all, and we can't find boss music, we're lost 
                                linesWithErrors.Add(i + "ftl"); //fatal error, could not proceed because we don't know where we're at
                            }

                        } else
                        {
                            //our next line that opens the boss music is further ahead
                            //throw line anomaly and let it keep going
                            linesWithErrors.Add(i + ":lAn%"); //line anomaly
                            currentPlaceInExpFldLoop = 2;
                            currentPlaceInExpFldLoop = 2;
                        }
                    } else {
                        //we have a curly close up ahead
                        //then what the hell is here?

                        //we don't reset currentPlaceInExpFldLoop because we see a levelCloser up ahead

                        if (nextLineWithCurlyClose >= lastLineNum)
                        {
                            //up ahead, we're closing out the whole JSON, not just closing the level like we want
                            linesWithErrors.Add(i + ":lcm"); //level close missing
                        }
                        else
                        {
                            linesWithErrors.Add(i + ":lAn`"); //line anomaly
                        }
                        
                    }*/
                }


                #endregion VerifyPlacement
                string expectedField = expectedFields[currentPlaceInExpFldLoop];

                if (i <= 1)
                {
                    //we're at line 0, or 1; should be openers
                    if (i == 1)
                    {
                        if (!line_nospaces.Contains("\"customLevelMusic\""))
                        {
                            linesWithErrors.Add(i + ":clm");
                        }
                        continue;//we don't need to do the punctuation check
                    }
                    if (finalChar != openersAndClosers[i])
                    {
                        //we don't see our expected punctuation
                        //we'll return a string saying what line, and what was expected
                        linesWithErrors.Add(i + ":" + openersAndClosers[i]);
                    }


                } else if (i >= lastLineNum - 1 && lastLineNum != -1)
                {
                    MessageBox.Show("LastLinenum: " + lastLineNum);
                    //we're at the 2nd-to-last line, we should be expecting a ]
                    //this will only ever happen if lastLineNum is not -1
                    if (finalChar != "]")
                    {
                        //we don't see our expected punctuation
                        linesWithErrors.Add(i + ":]");
                    }
                    //we're also going to look for our next one
                    string nextLine_ns = NormalizeWhiteSpace(fixedJsonLines[i + 1], true);
                    if (nextLine_ns != "}")
                    {
                        //we don't see our expected ending
                        linesWithErrors.Add(i + 1 + ":j}");
                    }
                    break; //break out of this, we're done

                }
                else
                {
                    //we're on a line that isn't the first two, nor the last two
                    //most of our code will be handled here


                    if (!line_nospaces.Contains(expectedFields[currentPlaceInExpFldLoop]) &&
                        !expectedFields[currentPlaceInExpFldLoop].Contains("|"))
                    {
                        //we can't find the label we were expecting

                        //see if the label we were expecting was the bankPath, which is optional
                        if (expectedFields[currentPlaceInExpFldLoop] == "\"bankPath\"")
                        {
                            //we ARE looking for the bankPath
                            //now that we fixed currentPlaceInExp, this shouldn't ever come up
                            MessageBox.Show("This somehow came up, please proceed to panic.\n " + line_nospaces + " doesn't contain " + expectedFields[currentPlaceInExpFldLoop] + "\n");

                            //see if this line didn't WANT a bankpath, which would be determined if 
                            /*

                            if (line_nospaces.Contains(":\\"))
                            {
                                //the user TRIED putting in a bankPath, but the bankpath label is goofy
                                linesWithErrors.Add(i + ":noBP");

                                /* NOT SURE IF WE NEED TO CONTINUE * /

                            } else
                            {
                                //we can just let this go, and let the punctuation check for the rest
                            }*/
                        } else
                        {
                            //we can't find the label we were expecting, AND we weren't expecting the bank path
                            string missingLabel = expectedFields[currentPlaceInExpFldLoop];
                            missingLabel = shaveSurroundingQuotesAndSpaces(missingLabel);

                            string nextLn = NormalizeWhiteSpace(fixedJsonLines[i + 1]);

                            string twoLnsDwn = NormalizeWhiteSpace(fixedJsonLines[i + 2]);

                            int nextPlaceInExpFldLoop = currentPlaceInExpFldLoop + 1;
                            if (nextPlaceInExpFldLoop >= expectedFields.Length)
                            {
                                nextPlaceInExpFldLoop = 0;
                            }
                            string nxtMsngLabel = expectedFields[nextPlaceInExpFldLoop];
                            nxtMsngLabel = shaveSurroundingQuotesAndSpaces(nxtMsngLabel);


                            //does the next line have what we just looked for?
                            //if it does, we skip i to that number, before trying to fix our place in the currentPlaceInExpFld
                            if (nextLn.Contains(missingLabel))
                            {
                                //yes it does! at this point, we were unable to find unique text for a certain label, and it WASN'T the optional bankpath
                                //if we got this far, though, we have something in line that we can't recognize
                                linesWithErrors.Add(i + ":lAn@");//line anomaly
                                currentPlaceInExpFldLoop++;
                                continue; //we need to get out of this and restart the check; we don't need i++, unless it's two lines down
                            } else if (twoLnsDwn.Contains(missingLabel))
                            {
                                //no it doesn't, so we're checking the next line
                                linesWithErrors.Add(i + ":lAn#");//line anomaly
                                linesWithErrors.Add((i + 1) + ":lAn#");//line anomaly
                                currentPlaceInExpFldLoop += 2;
                                i++; continue;
                            } else if (nextLn.Contains(nxtMsngLabel))
                            {
                                //next line didn't have it either, this is to check if we just skipped the label we want
                                linesWithErrors.Add(i + ":lAn$");//line anomaly
                                currentPlaceInExpFldLoop++;
                                continue;
                            } else if (twoLnsDwn.Contains(nxtMsngLabel))
                            {
                                //no it doesn't, let's check the next line if we have the NEXT label....
                                linesWithErrors.Add(i + ":lAn%");//line anomaly
                                linesWithErrors.Add((i + 1) + ":lAn%");//line anomaly
                                currentPlaceInExpFldLoop += 2;
                                i++; continue;

                            }
                            else
                            {
                                //alright, we tried. we can't find where we are 
                                linesWithErrors.Add(i + ":ftl"); //fatal error
                            }




                            /* This is critical, we need to find out if the next line is actually going to have the next label we want 
                             
                                We ALSO need to adjust the currentPlaceIn blah blah
                            */




                        }

                    } else if (expectedFields[currentPlaceInExpFldLoop].Contains("|"))
                    {
                        //there's multiple choices
                        string[] possibleFields = expectedFields[currentPlaceInExpFldLoop].Split('|');
                        if (!line_nospaces.Contains(possibleFields[0]) && !line_nospaces.Contains(possibleFields[1]))
                        {
                            //we don't have EITHER choices
                            linesWithErrors.Add(i + ":mb");

                        } else
                        {
                            //we have one of the choices

                            if (line_nospaces.Contains(possibleFields[0])) {
                                //Main Music is in the line, check if the line before it is "LevelName"
                                if (!fixedJsonLines[i - 1].Contains("\"LevelName\"") && fixedJsonLines[i - 1] != null)
                                {
                                    //we wee looking at something that contains 'MainMusic', and the line before us doesn't have "LevelName", (and wasn't blank)
                                    linesWithErrors.Add(i + ":mmp"); //main music placement

                                }

                            } else if (line_nospaces.Contains(possibleFields[1])) {
                                //BossMusic is in the line, check if the line before it is "LevelName"
                                //this should be able to work as just 'else'
                                if (!fixedJsonLines[i - 1].Contains("\"LevelName\"") && fixedJsonLines[i - 1] != null)
                                {
                                    //we wee looking at something that contains 'BossMusic', and the line before us doesn't have "LevelName", (and wasn't blank)
                                    //verify that we actually appear after a MainMusic's info

                                    //immediately check if we're before the MainMusic
                                    if (i < fixedJsonLines.Length - 8)
                                    {
                                        if ((fixedJsonLines[i + 7].Contains("\"MainMusic\"") || fixedJsonLines[i + 8].Contains("\"MainMusic\"")))
                                        {
                                            //we found another entry for "MainMusic" a few lines underneath us, our order is reversed
                                            linesWithErrors.Add(i + ":mbr"); //main boss reversed
                                        }
                                    } else if (i > 8)
                                    {
                                        //we only need i > 8 condition because if we do i-7 while i is 4, our program catches fire
                                        if (!fixedJsonLines[i - 2].Contains("\"BPM\"") &&
                                        !fixedJsonLines[i - 2].Contains("\"bankPath\"") &&
                                        !fixedJsonLines[i - 7].Contains("\"MainMusic\"") &&
                                        !fixedJsonLines[i - 8].Contains("\"MainMusic\""))
                                        {
                                            //we check four different spots for recognizable labels, but we found none of them
                                            linesWithErrors.Add(i + ":bmp"); //boss music placement couldn't be verified
                                        }
                                    }



                                }

                            }


                        }


                    }


                    //next, we handle the Colon, and the line's value (right of the colon)


                    if (expectedFields[currentPlaceInExpFldLoop].Length > 0 && line_nospaces.Contains(":"))
                    {
                        #region FoundColon
                        //if we're looking for a field such as Bank, Event, etc., and it has a colon
                        string[] songLabelAndInfo = line_nospaces.Split(':');
                        string infoAfterColon = songLabelAndInfo[1];
                        infoAfterColon = infoAfterColon.Replace(",", ""); //we don't care about punctuation right now

                        int numberOfQuotesInInfo = infoAfterColon.Split('\"').Length - 1;
                        int numberOfQuotesInLabel = infoAfterColon.Split('\"').Length - 1;


                        string infoShaven = shaveSurroundingQuotesAndSpaces(infoAfterColon);

                        //we want to keep label information close to the other ones, so we'll put this up here
                        if (numberOfQuotesInLabel > 2)
                        {
                            //we got too many quotes on the left of colon
                            linesWithErrors.Add(i + ":2mqL");
                        }

                        if (currentPlaceInExpFldLoop == 4 || currentPlaceInExpFldLoop == 5)
                        {
                            //we're either looking at Event or LowHealthBeatEvent

                            //verify events formatting
                            string checkEvent = infoShaven;
                            checkEvent = checkEvent.Replace("{", "");
                            checkEvent = checkEvent.Replace("}", "");

                            if (checkEvent.Length != infoShaven.Length - 2 || checkEvent.Length != 36)
                            {
                                //Event string does NOT have { and }, OR it does not have the full 36-digit ID
                                linesWithErrors.Add(i + ":evF");
                            } else
                            {
                                //we haven't found a problem with our formatting yet here, make sure there's quotes around our info
                                if (infoShaven.Length != infoAfterColon.Length - 2)
                                {
                                    linesWithErrors.Add(i + ":addq#");
                                }
                            }

                        } else if (currentPlaceInExpFldLoop == 6 || currentPlaceInExpFldLoop == 7)
                        {
                            //we're looking at either our Offset or BPM, we want to verify quotes integrity
                            if (infoShaven.Length != infoAfterColon.Length)
                            {
                                //there's quotes in our info, where there shouldn't be
                                linesWithErrors.Add(i + ":remq");
                            }
                        } else if (currentPlaceInExpFldLoop == 1)
                        {
                            //we're looking at LevelName
                            for (int j = 0; j < allLevelNames.Length; j++)
                            {
                                string capitalizeLevelName = allLevelNames[j].Substring(0, 1).ToUpper() + allLevelNames[j].Substring(1);
                                if (infoShaven == capitalizeLevelName)
                                {
                                    //we found the level, formatted correctly, we're fine
                                    instancesOfLevelInJson[j] = instancesOfLevelInJson[j] + 1;

                                    break; //break out of J's for loop, not I
                                } else if (infoShaven.ToLower() == allLevelNames[j])
                                {
                                    //we found the level, but it's not formatted correctly
                                    instancesOfLevelInJson[j] = instancesOfLevelInJson[j] + 1;
                                    linesWithErrors.Add(i + ":LCap");

                                    break; //break out of J's for loop, not I
                                }
                                if (j == levelNames.Length - 1)
                                {
                                    //we're at the end, and we didn't find anything
                                    linesWithErrors.Add(i + ":LUr(" + infoShaven + ")"); //level unrecognized
                                }
                            }


                        } else if (currentPlaceInExpFldLoop == 2)
                        {
                            //we're looking at MainMusic|BossMusic

                            //we look for punctuation later, we just want to make sure about quotes

                            if (numberOfQuotesInInfo > 0)
                            {
                                //we got too many quotes on the right of colon
                                linesWithErrors.Add(i + ":remq");
                            }

                            //I think we already took care of bankPath issues
                        }
                        else if (currentPlaceInExpFldLoop == 3)
                        {
                            //we're looking at Bank
                            //i don't think i need to do anything
                            //why didn't i just write these in order
                        }


                        if (infoShaven.Length == infoAfterColon.Length - 2)
                        {
                            //we have quotes around the info after our :

                            if (expectedField == "BeatInputOffset" || expectedField == "BPM")
                            {
                                //if we're looking for one of these two, then we DON'T want quotes, but we have them
                                linesWithErrors.Add(i + ":remq");
                            }


                        } else if (infoShaven.Length == infoAfterColon.Length)
                        {
                            //we have no quotes in our info

                            if (currentPlaceInExpFldLoop != 6 && currentPlaceInExpFldLoop != 7 && currentPlaceInExpFldLoop != 2)
                            {
                                //we're looking at something that isn't MainMusic/BossMusic, BeatInputOffset, or BPM, so we need quotes
                                linesWithErrors.Add(i + ":addq" + currentPlaceInExpFldLoop + "$");
                            }
                        }

                        if (numberOfQuotesInInfo > 2)
                        {
                            //we got too many quotes on the right of colon
                            linesWithErrors.Add(i + ":2mqR");
                        }
                        if (numberOfQuotesInInfo == 1)
                        {
                            //we got too many quotes on the right of colon
                            linesWithErrors.Add(i + ":snq");//single quote
                        }

                        #endregion FoundColon
                        //end of if expectedFields[currentPlaceInExpFldLoop].Length>0 && line_nospaces.Contains(":")
                    }
                    else if (expectedFields[currentPlaceInExpFldLoop].Length > 0 && !line_nospaces.Contains(":"))
                    {
                        //we can't find a colon in our line, and we're actually looking at a line that has a label
                        linesWithErrors.Add(i + ":nc");


                        //string stillLookingForInfo = line_nospaces.Replace(expectedFields[currentPlaceInExpFldLoop], ""); //we're going to remove all the info on the left side;
                        //since there's no colon, all that should be left is the information
                        //actually, we'll just tell the user that we can't verify the information


                    }
                    //end of check for labels and info 





                    if (finalChar != expectedEndings[currentPlaceInExpFldLoop])
                    {
                        //final character doesn't match the expectedEndings based on our array of what it should look like in the level


                        if (expectedEndings[currentPlaceInExpFldLoop].Length == 1)
                        {
                            //there's only one thing we're expecting here
                            MessageBox.Show("[Ln" + i + ":Missing " + expectedEndings[currentPlaceInExpFldLoop] + "(" + currentPlaceInExpFldLoop + ") in " + fixedJsonLines[i]);
                            linesWithErrors.Add(i + ":" + expectedEndings[currentPlaceInExpFldLoop]);

                        } else if (expectedEndings[currentPlaceInExpFldLoop].Length == 4)
                        {
                            //The following is what we run if the final character of our current line didn't match what we were expecting, but because it had 
                            //two possible options, and we haven't 

                            //if we're at 7, we're currently looking for "BPM" line
                            if (currentPlaceInExpFldLoop == 7)
                            {
                                //we're expecting , or just line return if bankPath isn't next
                                //if we don't add to our place in currentPlaceInEndings, this will run again if the next line has a bankPath
                                int nextLineWithBankPath = nextValidLineContaining("\"bankPath\"", fixedJsonLines, i + 1, 3);
                                if (nextLineWithBankPath != -1)
                                {
                                    //we have a bankPath on the next line, or there's an anomaly and bankPath is further ahead
                                    if (finalChar != ",")
                                    {
                                        //we have a comma, but we don't have any "bankPath" info on the next line, so it shouldn't be here
                                        linesWithErrors.Add(i + ":,");
                                    }

                                }
                                else
                                {
                                    //we don't have a bankPath
                                    if (finalChar == ",")
                                    {
                                        //we have a comma, but we don't have any "bankPath" info on the next line, so it shouldn't be here
                                        linesWithErrors.Add(i + ":-,");
                                    }
                                    else if (!Int32.TryParse(finalChar, out int k))
                                    {
                                        //the last char is not a number, and it can't be space
                                        linesWithErrors.Add(i + ":ue" + finalChar); //unexpected encounter (+ what we found)
                                    }
                                }

                            } else if (currentPlaceInExpFldLoop == 8)
                            {
                                //we're looking for BankPath info


                                if (!line_nospaces.Contains(".bank"))
                                {
                                    linesWithErrors.Add(i + 1 + ":nobpfile"); //bankpath points to a directory, but needs to include "[Filename].bank"
                                }

                                if (line_nospaces.Split('/').Length - 1 != 0)
                                {
                                    //we have / in our bankpath, when we only want \'s
                                    linesWithErrors.Add(i + 1 + ":bpws"); //bank path wrong slash
                                    //we'll also replace them so we can continue with the code if that's the only issue
                                    line_nospaces.Replace('/', '\\');
                                }

                                string[] directoriesBtwnDblSlashes = line_nospaces.Split(new string[] { "\\\\" }, StringSplitOptions.None);
                                string nextLine_chkIntegrity = line_nospaces.Replace("\\\\", "\\");
                                string[] directoriesBtwnSnglSlashes = nextLine_chkIntegrity.Split(new string[] { "\\" }, StringSplitOptions.None);

                                //we're going to take value for bankPath and analyze the formating, and verify a file exists
                                int indexOfBankPathInfo = nextLine_chkIntegrity.IndexOf(":\\");
                                indexOfBankPathInfo -= 1; //this should be the index of C:\, B:\ X:\ Etc
                                if (indexOfBankPathInfo < 0)
                                {
                                    linesWithErrors.Add(i + 1 + ":cvBP"); //can't verify bankpath value's format
                                    goto CantVerifyBPFormat;
                                }
                                string bankPathInfo = nextLine_chkIntegrity.Substring(indexOfBankPathInfo);
                                bankPathInfo = shaveSurroundingQuotesAndSpaces(bankPathInfo);


                                if (line_nospaces.Contains("\\\\\\") || line_nospaces.Contains("\\\\\\\\"))
                                {
                                    linesWithErrors.Add(i + 1 + ":2mSl"); //too many slashes
                                }
                                else if (directoriesBtwnDblSlashes.Length != directoriesBtwnSnglSlashes.Length)
                                {
                                    //if the # of our directories with double slashes does not match our # of directories after converting to single slashes, it means we don't have enough slashes somewhere er something
                                    linesWithErrors.Add(i + 1 + ":bpF"); //bankpath formatting
                                }
                                else if (!verifyFileExists(bankPathInfo))
                                {
                                    linesWithErrors.Add(i + 1 + ":bpFNF"); //bankpath file not found
                                }

                            CantVerifyBPFormat:


                                if (finalChar == ",")
                                {

                                    linesWithErrors.Add(i + 1 + ":-,");
                                }
                                /* I don't think we need this, because we already check for quotes
                                else if (finalChar != "\"")
                                {

                                    linesWithErrors.Add(i + 1 + ":bP");
                                }*/


                            } else if (currentPlaceInExpFldLoop == 9)
                            {
                                //8 means bankpath, 9 is music closer
                                //currentPlaceInExpFldLoop is currently looking for what's AFTER the bankPath, which is either the MainMusic or BossMusic closing, with a possible comma if there's another level info

                                int nextLineWithBossMusic = nextValidLineContaining("\"BossMusic\"", fixedJsonLines, i + 1, 3);
                                if (nextLineWithBossMusic == i + 1)
                                {
                                    //we have a BossMusic on the next line, we're expecting a ,
                                } else if (nextLineWithBossMusic == -1)
                                {
                                    //we don't have BossMusic on the next line, we should not have a , nor anything else except }
                                    if (finalChar == ",")
                                    {
                                        linesWithErrors.Add(i + ":-," + line_nospaces); // comma shouldn't be here
                                    }

                                    if (line_nospaces.Replace(",", "") != "}")
                                    {
                                        if (line_nospaces.Replace(",", "").Length == 1)
                                        {
                                            linesWithErrors.Add(i + ":ue" + line_nospaces.Replace(",", "")); //unexpected encounter (+ what we found)
                                        } else
                                        {
                                            linesWithErrors.Add(i + ":on}");//unexpected anomalies, we only wanted } or },
                                        }

                                    }
                                }

                                /*
                                if (fixedJsonLines[i].Contains("\"bankPath\""))
                                {
                                    //if we have a bankPath on this line, we don't want to throw any errors, we handled them on the previous if condition
                                } else
                                {
                                    //we're assuming our current line isn't supposed to say bankPath
                                }


                                if (finalChar == "," && fixedJsonLines[i + 1].Contains("\"bankPath\"") == false)
                                {
                                    //we have a comma, but we don't have any "bankPath" info on the next line, so it shouldn't be here
                                    linesWithErrors.Add(i + ":-,");
                                }
                                else if (fixedJsonLines[i + 1].Contains("\"bankPath\""))
                                {
                                    //the next line has a bankPath
                                    //we'll handle both lines now, and tell i to skip one

                                    if (finalChar != ",")
                                    {
                                        //we were expecting a comma, since we have "bankPath" on the next line, but there isn't one here
                                        linesWithErrors.Add(i + ":,");
                                    }
                                    //now we're going to check our bankPath line
                                    string nextLine_ns = NormalizeWhiteSpace(fixedJsonLines[i+1], true);
                                    string nextLineFinalChar = nextLine_ns.Substring(nextLine_ns.Length - 1, 1);
                                    if (nextLineFinalChar != "\"")
                                    {

                                        linesWithErrors.Add(i + ":bP");
                                    }
                                    //i++; //skip the next iteration, we just handled it

                                }*/


                            }
                            else if (currentPlaceInExpFldLoop == 10)
                            {

                            }

                        }


                        //linesWithErrors.Add(i + ":" + expectedEndings[currentPlaceInExpEndLoop]);

                    }

                    //linesWithErrors.Add("(Line: " + i + "; CurrentPlaceInExpLoop: " + currentPlaceInExpFldLoop + "\n");

                    currentPlaceInExpFldLoop++; //add to both of these


                    //resetting them if they're over the mark
                    /* These get reset at the beginning now, because I feel safer doing that 
                    if (currentPlaceInExpFldLoop >= expectedFields.Length)
                    {
                        currentPlaceInExpFldLoop = 0;
                    }
                    if(currentPlaceInExpEndLoop >= expectedEndings.Length)
                    {
                        currentPlaceInExpEndLoop = 0;
                    }*/


                    //end of "if i is within range of levels"
                }


                //end of for loop single iteration
            }

            if (lastLineNum == -1)
            {
                linesWithErrors.Add(fixedJsonLines.Length + ":]}"); //this appears if our last character in our JSON isn't a } or a ]
            }


            string[] errorList = linesWithErrors.ToArray();

            return errorList;


        }
        #endregion DeprecatedDebugger

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
            testFindJson.Text += "MSLCalled: (" + modIndex + ", " + Level + ", " + m_or_b + ")";
            bool result = false;

            //example of supported Lvl string is 0b1mb23mb4m567m
            if (modIndex == -1)
            {
                //we're looking at the default level, just return true
                return true;
            }

            testFindJson.Text += "ModIndex: " + modIndex + "; ";

            string supportedLvlString = csSupLvls[modIndex]; //gives us the supported levels string of the mod that's selected
            int levelInfoIndex = supportedLvlString.IndexOf(Level.ToString());//this is going to give us the spot right before the level number. After the number is m, b, mb, or (nothing, next number)

            if (levelInfoIndex == -1) { testFindJson.Text += "ERROR OCCURED, no levelInfo found in string"; return result; }//this shouldn't ever happen, but just in case
            if (levelInfoIndex == supportedLvlString.Length - 1) return result; //if it has no support on last number, then we're at the last character

            testFindJson.Text += "MSL: " + supportedLvlString + "; ";

            if (m_or_b == "m")
            {
                //m will always be right after the level number
                if (supportedLvlString.Substring(levelInfoIndex + 1, 1) == "m")
                {
                    result = true;
                    return result;
                } else
                {
                    testFindJson.Text += " F6 " + supportedLvlString + "; ";
                    return result;
                }
            } else if (m_or_b == "b")
            {
                //we can have 4mb5, 4b5, 45b
                if (supportedLvlString.Substring(levelInfoIndex + 1, 1) == "b")
                {
                    //we found a "b" after our number, return true
                    testFindJson.Text += " A1: " + supportedLvlString + "; ";
                    result = true;
                    return result;
                } else
                {
                    testFindJson.Text += " A2: " + supportedLvlString.Substring(levelInfoIndex + 1, 1) + "..";
                }


                //if we got this far, there was no "b" RIGHT after number; next possibilities are 1mb2, or 12b
                //or it could be ..6mb7 (meaning NOTHING is there)
                if (levelInfoIndex >= supportedLvlString.Length - 2) { testFindJson.Text += " D4 "; return result; }
                string checkForNumStr = supportedLvlString.Substring(levelInfoIndex + 1, 1);


                /*
                bool twoSpacesOverIsNumber = false;

                for (int i = 0; i < 8; i++)
                {
                    
                    if(checkForNumStr == i.ToString())
                    {
                        testFindJson.Text += "\n" + checkForNumStr + "==" + i.ToString();
                        twoSpacesOverIsNumber = true;
                        testFindJson.Text += "> >2spacesover: "+ twoSpacesOverIsNumber + "; ";
                    } else
                    {
                        testFindJson.Text += "\n" + checkForNumStr + "!=" + i.ToString() + "; ";
                    }
                }
                
                if (twoSpacesOverIsNumber)
                I WAS GOING FUCKING INSANE. checkForNumStr was levelInfoIndex+2, and kept pulling the wrong information
                */

                if (Int32.TryParse(checkForNumStr, out int j))
                {
                    //this should activate if our character after Level number was another number; meaning the Mod doesn't have info for either main music or boss on this level, ie. the 1 in 0mb12m3mb
                    testFindJson.Text += " B2 " + supportedLvlString + "; ";
                    return false;
                }

                else if (supportedLvlString.Substring(levelInfoIndex + 2, 1) == "b")
                {
                    testFindJson.Text += " C3 " + supportedLvlString + "; ";
                    result = true;
                    return result;
                }
            }

            testFindJson.Text += " E5 " + supportedLvlString + "; ";

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


            if (changedComboBox == null) // just to be on the safe side
                return;

            testFindJson.Text += "Hi! Trying to change: " + changedComboBox.Name + "; ... ";

            if (!wasComboBoxChanged(changedComboBox)) { return; }
            setModGrabLvlSelection(changedComboBox);
            //setSongSelectionArray(changedComboBox);
            testFindJson.Text += ".cDGLT.";
        }


        int numberOfLevels = 8; //8 levels
        private void setSupportedLevelColors(string fullJson)
        {
            Button[] LevelButtons = { L1Settings, L2Settings, L3Settings, L4Settings, L5Settings, L6Settings, L7Settings, L8Settings };

            //find the full level info
            //look for main music
            //look for boss music

            for (int i = 0; i < numberOfLevels; i++)
            {
                string capitalizeLevelName = levelNames[i].Substring(0, 1).ToUpper() + levelNames[i].Substring(1);
                int indexOfLevelInfo = fullJson.IndexOf(capitalizeLevelName);

                if (indexOfLevelInfo == -1)
                {
                    LevelButtons[i].BackColor = Color.RosyBrown;
                    continue; //we can't find the level name, it's not in the JSON
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

                if (indexOfLevelInfoEnd == -1) return;


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
                    if (i == numberOfLevels - 1)
                    {
                        //Sheol's final boss can't be changed
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
            if (blockListBoxSelIndexChng)
            {
                //blockListBoxSelIndexChng is only set to true if we had unsaved changes and we're wanting to go back to them
                blockListBoxSelIndexChng = false;
                testFindJson.Text += "BLOCKED!";
                return;
            }
            resetDebugLabel("No song selected!"); //resets the debug text if it said this

            if (!Organizer_checkAndAlertUnsavedChanges(true))
            {
                //when the user changed the selection in ListBox1, we saw there were unsaved changes, wherever we were
                //with checkAndAlertUnsavedChanges, we will decide to continue to go to the new selection, and checkAndAlert... will save the information, or not save it
                //if we do Cancel instead, this function returns "False" and we switch back to the already-selected index
                //blockListBoxSelIndexChng will stop this from running again when we hit Cancel, or X out the box

                blockListBoxSelIndexChng = true;
                listBox1.SelectedIndex = currentListSelection;

                return;
            }

            enableOrganizerFields(); //enables the text boxes; does nothing if they're already enabled

            string songJsonInfo = Organizer_GetModJson();

            if (songJsonInfo == "-1") { MessageBox.Show("No customsongs.json found in directory"); return; }
            //testFindJson.Text = songJsonInfo;
            string displayText = "";

            if (L1Settings.Enabled == false)
            {
                organizer_enableLevelButtons();
            }

            setSupportedLevelColors(songJsonInfo);
            SetSelectedLevelColors(0);
            getSpecificLevelInfo(songJsonInfo, "Voke"); //resets it to be on the Voke tab
            resetSongOriginalInfo("");
            currentListSelection = listBox1.SelectedIndex;









            /*DirectoryInfo[] songs = di.GetDirectories();
            for(int i=0; i < songs.Length; i++)
            {
                if()
            }*/

        }

        int[] debugOriginalLoc = { 550, 18 };
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
            debugLabel.Left = debugOriginalLoc[0]+newLoc[0];
            debugLabel.Top = debugOriginalLoc[1]+newLoc[1];
        }
        int maxJitterbugs = 7;
        int currentJitterbugs = 0; 
        public void JitterBug(object sender, EventArgs e)
        {
            JitterBugMove();
            currentJitterbugs++;
            if(currentJitterbugs >= maxJitterbugs)
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
            
            testFindJson.Text += "shake" + shakesNum;

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
                testFindJson.Text += "shake" + shakesNum;

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
            Button[] LevelButtons = { L1Settings, L2Settings, L3Settings, L4Settings, L5Settings, L6Settings, L7Settings, L8Settings };

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
            if (songJsonInfo == "-1") { MessageBox.Show("Game directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(0);
            getSpecificLevelInfo(songJsonInfo, "Voke");
            resetSongOriginalInfo("");
            string fullSongJsonInfo = Organizer_GetModJson();
            //string fullInfo = getFullInfoForLevel(fullSongJsonInfo, "Voke");
            testFindJson.Text = "FullInfo: " + fullSongJsonInfo;
        }
        private void L2Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return;

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Game directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(1);
            getSpecificLevelInfo(songJsonInfo, "Stygia");
            resetSongOriginalInfo("");
            string fullSongJsonInfo = Organizer_GetModJson();
            //string fullInfo = getFullInfoForLevel(fullSongJsonInfo, "Stygia");
            testFindJson.Text = "FullInfo: " + fullSongJsonInfo;
        }
        private void L3Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return; //this returns false if we don't want to cancelChanges

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Game directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(2);
            getSpecificLevelInfo(songJsonInfo, "Yhelm");
            resetSongOriginalInfo("");
        }
        private void L4Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return; //this returns false if we don't want to cancelChanges

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Game directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(3);
            getSpecificLevelInfo(songJsonInfo, "Incaustis");
            resetSongOriginalInfo("");
        }
        private void L5Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return; //this returns false if we don't want to cancelChanges

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Game directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(4);
            getSpecificLevelInfo(songJsonInfo, "Gehenna");
            resetSongOriginalInfo("");
        }
        private void L6Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return; //this returns false if we don't want to cancelChanges

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Game directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(5);
            getSpecificLevelInfo(songJsonInfo, "Nihil");
            resetSongOriginalInfo("");
        }
        private void L7Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return; //this returns false if we don't want to cancelChanges

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Game directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(6);
            getSpecificLevelInfo(songJsonInfo, "Acheron");
            resetSongOriginalInfo("");
        }
        private void L8Settings_Click(object sender, EventArgs e)
        {
            if (!Organizer_checkAndAlertUnsavedChanges()) return; //this returns false if we don't want to cancelChanges

            string songJsonInfo = Organizer_GetModJson();
            if (songJsonInfo == "-1") { MessageBox.Show("Game directory not found"); return; }
            if (songJsonInfo == "-2") { MessageBox.Show("No customsongs.json found in game directory"); return; }
            SetSelectedLevelColors(7);
            getSpecificLevelInfo(songJsonInfo, "Sheol");
            resetSongOriginalInfo("");
            //string fullSongJsonInfo = Injector_GetModJson();
            //string fullInfo = getFullInfoForLevel(fullSongJsonInfo, "Sheol");
            //testFindJson.Text = fullInfo;
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
        int[] modMRadioPosX = { 286, 286, 286, 286, 565, 565, 565, 565 };
        int[] modMRadioPosY = {  58, 132, 206, 280,  58, 132, 206, 301};
        int[] modBRadioPosX = { 286, 286, 286, 286, 565, 565, 565};
        int[] modBRadioPosY = { 105, 178, 251, 324, 105, 175, 251};


        private void setModLvlButtonColors(string supportedLevelsString, string m_or_b, bool resetImg = false)
        {
            //this is the supported-level indicator; this runs through the Radio Panel and enables/disables the Level radio buttons based on the Mod's supported levels
            //this doesn't actually set "colors", it just disables/enables the buttons

            RadioButton[] modLvlButtons = { VokeRadioButtonM1, StygiaRadioButtonM1, YhelmRadioButtonM1, IncaustisRadioButtonM1, GehennaRadioButtonM1, NihilRadioButtonM1, AcheronRadioButtonM1, SheolRadioButtonM1 };
            
            //first disable all the radio buttons
            for(int mB = 0; mB < modLvlButtons.Length; mB++)
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
            while(L < supportedLevelsString.Length)
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

            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };

            int modIndex = -1;

            //based on what button we pressed to trigger this (we press the level button, with the V next to combo boxes), we check that adjacent field
            string comboNumStr = senderButtonName.Substring(2, 1);
            int whichComboBox = Int32.Parse(comboNumStr);
            whichComboBox -= 1;
            if (senderButtonName.Substring(0, 2) == "ML")
            {
                
                modIndex = mainCBox[whichComboBox].FindStringExact(mainCBox[whichComboBox].Text);
                testFindJson.Text += "mainCBox[whichComboBox].Text: " + mainCBox[whichComboBox].Text;
            } else if (senderButtonName.Substring(0, 2) == "BF")
            {
                modIndex = bossCBox[whichComboBox].FindStringExact(bossCBox[whichComboBox].Text);
            }
            

            
            
            
            
            return modIndex;
        }



        private void OnModGrabLvlChangeText(object sender, EventArgs e)
        {
            ComboBox boxCalled = sender as ComboBox;

            disableGrabLvlBox(boxCalled);//this disables our button and makes it blank
            setSongSelectionArray(boxCalled, " "); //it can't be ""; we can set it to "blah" if we want, it just CANNOT match any more
        }

        private void disableGrabLvlBox(ComboBox cBox)
        {
            //this is meant to run if we ever start typing something in
            //we should only verify a selection once the user has clicked, or hit enter. Until then, we explicitly tell them that we haven't gotten a valid input yet
            //since whatever we're typing into will be a combo box, that's what we need to see is calling this

            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };

            string cBoxName = cBox.Name;
            //ie mainCombo1
            string modLvlNumStr = cBoxName.Substring(cBoxName.Length-1, 1);
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

            RadioButton[] rButtons = { VokeRadioButtonM1, StygiaRadioButtonM1, YhelmRadioButtonM1, IncaustisRadioButtonM1, GehennaRadioButtonM1, NihilRadioButtonM1, AcheronRadioButtonM1, SheolRadioButtonM1};
            //reset all radio button selections to not be selected
            for(int r=0; r<rButtons.Length; r++)
            {
                rButtons[r].Checked = false;
            }


            Button clickedLvlButton = sender as Button;
            testFindJson.Text += clickedLvlButton.Name + " was clicked; ";

            int xLocation = 0;
            int yLocation = 0;

            int selectedMod = getSelectedMod(clickedLvlButton.Name);
            if (selectedMod == -1) { testFindJson.Text += ">.<"; return; } //getSelectedMod returned -1, meaning no mod is properly selected

            if (clickedLvlButton == null) //checking that we see something
                return;

            if (clickedLvlButton.Name.Substring(0, 2) == "ML")
            {
                string modLvlNumStr = clickedLvlButton.Name.Substring(2, 1);
                int whichLvl = Int32.Parse(modLvlNumStr);
                whichLvl -= 1;

                setModLvlButtonColors(csSupLvls[selectedMod], "m", true); //since we're bringing this up, we want to reset the Img for the button, hence "true"

                ML1RadioPanel.Location = new Point(modMRadioPosX[whichLvl], modMRadioPosY[whichLvl]);
            } else if (clickedLvlButton.Name.Substring(0, 2) == "BF")
            {
                string modLvlNumStr = clickedLvlButton.Name.Substring(2, 1);
                int whichLvl = Int32.Parse(modLvlNumStr);
                whichLvl -= 1; //because our form starts at ML1

                setModLvlButtonColors(csSupLvls[selectedMod], "b", true);  //since we're bringing this up, we want to reset the Img for the button, hence "true"

                ML1RadioPanel.Location = new Point(modBRadioPosX[whichLvl], modBRadioPosY[whichLvl]);
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
            }
        }

        private void resetSetListDebugLabel(string onlyIfItSaysThis = "", int whichLabel = 0)
        {
            Label[] setListDebugLabels = { SetList_DebugLabel1, SetList_DebugLabel2, SetList_DebugLabel3 };
            if(onlyIfItSaysThis == "")
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
            } else if(debugLabel.Text == onlyIfItSaysThis)
            {
                debugLabel.Visible = false;
                debugLabel.Text = "";
            }
        }

        private void justCheckWithoutThinking(string whatCalledUsName, bool isChecked = true)
        {
            CheckBox[] mainCheckBoxes = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8 };
            CheckBox[] bossCheckBoxes = { checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };

            string whichLevelStr = whatCalledUsName.Substring(2, 1); //ML1, BF2
            if(whichLevelStr == "i" || whichLevelStr == "s")
            {
                whichLevelStr = whatCalledUsName.Substring(whatCalledUsName.Length - 1, 1);
            }
            int whichLevel = Int32.Parse(whichLevelStr);
            whichLevel -= 1;
            if(whatCalledUsName.Substring(0,1).ToLower() == "m")
            {
                
                mainCheckBoxes[whichLevel].Checked = isChecked;
            } else if (whatCalledUsName.Substring(0, 1).ToLower() == "b")
            {
                
                bossCheckBoxes[whichLevel].Checked = isChecked;
            }
        }


        public void selectModLevelRadio(object sender, EventArgs e)
        {
            //runs when we select which level we want to pull the mod's song info from

            //find out radio list is being used (what level we're changing the song for), and set it accordingly
            RadioButton clickedRadio = sender as RadioButton;
            testFindJson.Text += clickedRadio.Name + " was clicked; ";



            if (clickedRadio == null) // just to be on the safe side
                return;

            Button[] mainLvlGrabButton = { ML1ModLvlButton, ML2ModLvlButton, ML3ModLvlButton, ML4ModLvlButton, ML5ModLvlButton, ML6ModLvlButton, ML7ModLvlButton, ML8ModLvlButton };
            Button[] bossLvlGrabButton = { BF1ModLvlButton, BF2ModLvlButton, BF3ModLvlButton, BF4ModLvlButton, BF5ModLvlButton, BF6ModLvlButton, BF7ModLvlButton };

            ML1RadioPanel.Visible = false; //first, make the panel invisible
            SetList_DebugLabel1.Visible = false; //make the debug invisible too
            SetList_DebugLabel2.Visible = false;
            SetList_DebugLabel3.Visible = false;

            //we need to find out what this panel's location is. that tells us what level we're changing
            int xLocation = ML1RadioPanel.Location.X;
            int yLocation = ML1RadioPanel.Location.Y;

            for(int m=0; m<modMRadioPosY.Length; m++)
            {
                if(yLocation == modMRadioPosY[m])
                {
                    if(xLocation == modMRadioPosX[m])
                    {
                    //we found the location!
                        mainLvlGrabButton[m].Text = clickedRadio.Text;
                        mainLvlGrabButton[m].Image = clickedRadio.Image;
                        justCheckWithoutThinking(mainLvlGrabButton[m].Name);

                        ComboBox adjacentCombo = getComboFromGrabLvlBtn(mainLvlGrabButton[m]);
                        alertLevelIfModIntegrityComprimised(m, adjacentCombo);
                        return;
                    }

                }
            }

            for (int b = 0; b < modMRadioPosY.Length; b++)
            {
                if (yLocation == modBRadioPosY[b])
                {
                    if (xLocation == modBRadioPosX[b])
                    {
                        //we found the location!
                        bossLvlGrabButton[b].Text = clickedRadio.Text;
                        bossLvlGrabButton[b].Image = clickedRadio.Image;
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
                debugLabel.Text = "No song selected!";
                debugLabel.Visible = true;
                AngryText();
            }
        }

        private void supportedLevelsGroup_MouseOver(object sender, EventArgs e)
        {
            if(listBox1.SelectedIndex == -1)
            {
                //if (debugLabel.Text == "No song selected!" && debugLabel.Visible) ShakeLabel(debugLabel);
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
                testFindJson.Text += "Shift";
                switchToMainOrBossSelection(grabLvlButton);
                grabLvlSelectSwitched = true;
            }
            
        }
        private void modGrabLvl_KeyPress(object sender, KeyPressEventArgs e)
        {
            Button grabLvlButton = sender as Button;

            testFindJson.Text += "Hi";

            if ((Control.ModifierKeys & Keys.Shift) != 0)
            {
                //shift was pressed

                
                switchToMainOrBossSelection(grabLvlButton);
            }

        }

        private void modGrabLvl_KeyUp(object sender, KeyEventArgs e)
        {
            Button grabLvlButton = sender as Button;
            testFindJson.Text += "UP";

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

            string glbID = grabLvlButton.Name.Substring(0,1); //the name will be, for example, ML1ModLvlButton, ML2ModLvlButton, BF3ModLvlButton
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
            if (selectedMod == -1) { testFindJson.Text += ">.<"; return; } //getSelectedMod returned -1, meaning no mod is properly selected; this shouldn't ever happen, but csSupLvls can't have a negative

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
            ComboBox[] mainCBox = { mainCombo1, mainCombo2, mainCombo3, mainCombo4, mainCombo5, mainCombo6, mainCombo7, mainCombo8 };
            ComboBox[] bossCBox = { bossCombo1, bossCombo2, bossCombo3, bossCombo4, bossCombo5, bossCombo6, bossCombo7 };
            CheckBox[] mainCheckBoxes = { checkm1, checkm2, checkm3, checkm4, checkm5, checkm6, checkm7, checkm8 };
            CheckBox[] bossCheckBoxes = { checkb1, checkb2, checkb3, checkb4, checkb5, checkb6, checkb7 };

            for (int m=0; m<mainCBox.Length; m++)
            {
                if (!mainCheckBoxes[m].Checked || clearExisting)
                {
                    //if clearExisting is false, and the checkbox was checked, we don't fill it
                    //if clearExisting is true, we don't give a fuck if it's checked
                    mainCBox[m].Text = currentSetListName_m[m];
                }
            }
            for (int b = 0; b < bossCBox.Length; b++)
            {
                if (!bossCheckBoxes[b].Checked || clearExisting)
                {
                    bossCBox[b].Text = currentSetListName_b[b];
                }
            }

        }



        //this function currently assumes you downloaded the LowHealth Library Bank, and have the original game's bank
        private void loadMusicBankList()
        {
            customMusicBankCombo.Items.Add("The Library .Bank");
            customMusicBankCombo.Items.Add("Original .Bank");
            for (int i=0; i< modsWithCustMusicBank.Count; i++)
            {
                string nameToAdd = modsWithCustMusicBank[i].Name;
                if(nameToAdd.Substring(nameToAdd.Length-1, 1) == "s")
                {
                    nameToAdd += "'";
                } else
                {
                    nameToAdd += "'s";//this is technically correct, so this was pointless
                }
                nameToAdd += " .Bank";
                customMusicBankCombo.Items.Add(new ListItem { Name = nameToAdd, Path = modsWithCustMusicBank[i].Path });
            }
        }
        

        private void musicSelectChangeTextDB(object sender, EventArgs e)
        {
            ComboBox cBox = sender as ComboBox;
            testFindJson.Text += " .changeDB. ";
            if (!wasComboBoxChanged(cBox)) return;
            setGrabLvlButton(cBox);

            string lvlNumStr = cBox.Name.Substring(cBox.Name.Length - 1, 1);
            int lvlNum = Int32.Parse(lvlNumStr); //gives 1-based index
            lvlNum -= 1;
            alertLevelIfModIntegrityComprimised(lvlNum, cBox);
        }

        private void FormLoad(object sender, EventArgs e)
        {
            //this is ran when the program first loads
            string modList = loadModListWithSubs(setListCatalog, true); //this loads the contents into the setListCatalog ListBox
            fillSongSelection(modList);
            storeModListInfo(); //this stores the info for our song; specifically which levels it supports; the info is hidden and is used to quickly know what levels each mod has info for
            setOldSongsArray(); //this stores an array of info of our current customsongs.json file in the game folder
            loadOldInfoIntoSetList(); //this loads the array from the previous line into the fields
            loadMusicBankList();
        }


        //this doesn't work either
        bool textChanged = false;
        private void ComboTextChangeReset(object sender, EventArgs e)
        {
            textChanged = false;
        }
        
        private void ComboTextChanged(object sender, EventArgs e)
        {
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
                    ML1RadioPanel.Visible = false;
            base.WndProc(ref m);
        }



        private void SaveLevelInfo_Click(object sender, EventArgs e)
        {
            Button saveButtonHit = sender as Button;
            string m_or_b = saveButtonHit.Name.Substring(0, 1);

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
            } else if(m_or_b == "b")
            {
                bankText = BFNameBox.Text;
                eventText = BFEventBox.Text;
                lowHealthText = BFLHBEBox.Text;
                offsetText = BFOffsetBox.Text;
                bpmText = BFBPMBox.Text;
                bankPathText = bTrueBankPath.Text;
                //bankPathText = bBankPathLabel.Text;
            }

            int levelWereSelecting = getSelectedLevel_OrganizerInjector();
            string LvlNameCapd = levelNames[levelWereSelecting].Substring(0, 1).ToUpper() + levelNames[levelWereSelecting].Substring(1).ToLower(); //voke->Voke



            string injection = getNewLevelInfoLines(LvlNameCapd, m_or_b, bankText, eventText, lowHealthText, offsetText, bpmText, bankPathText);


            SaveLevelInfo_Organizer(levelWereSelecting, m_or_b, injection);
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

            debugLabel.Text = "";//we'll reset this, regardless if we're going to add something to it

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

                string DisplayPath = mTrueBankPath.Text;
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

                string DisplayPath = bTrueBankPath.Text;
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
                }
            }
            tabPage2.Focus();
        }

        private void bankPathRedAlert(Label bankPathLabel, int message = 0)
        {
            bankPathLabel.BackColor = Color.RosyBrown;

            debugLabel.Visible = true;
            if (bankPathLabel.Name.Substring(0,1) == "m")
            {
                switch (message)
                {
                    case 0:
                        debugLabel.Text = "The file in bankPath under Main Music doesn't exist.";
                        break;
                    case 1:
                        debugLabel.Text = "bankPath needs to point to Bank, not just directory.";
                        break;
                }
                
                
            }
            else if (bankPathLabel.Name.Substring(0, 1) == "b")
            {
                switch (message)
                {
                    case 0:
                        debugLabel.Text = "The file in bankPath under Boss Music doesn't exist.";
                        break;
                    case 1:
                        debugLabel.Text = "bankPath needs to point to .Bank file, not just directory.";
                        break;
                }
                debugLabel.Visible = true;
            }
        }

        private bool verifyFileExists(string filePath)
        {
            string correctedPath = filePath;
            if (correctedPath.Contains("\\\\"))
            {
                correctedPath = correctedPath.Replace("\\\\", "\\");
            }
            correctedPath = shaveSurroundingQuotesAndSpaces(correctedPath);
            //all that was just in case, but pretty sure we didn't need it
            testFindJson.Text += "<<"+correctedPath +">>";

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

            debugLabel.Text = "";//we'll reset this, regardless if we're going to add something to it

            string DisplayPath = whichBankPathBox.Text;
            if (DisplayPath.Contains("\\\\"))
            {
                DisplayPath = DisplayPath.Replace("\\\\", "\\");
            }
            DisplayPath = pathShortener(DisplayPath, 40);
            DisplayPath = shaveSurroundingQuotesAndSpaces(DisplayPath); //this needs to be before we add "bankPath":
            DisplayPath = "bankPath: " + DisplayPath;

            if (whichBankPathBox.Name.Substring(0,1) == "m")
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
                if(!verifyFileExists(bTrueBankPath.Text))
                {
                    bankPathRedAlert(bBankPathLabel);
                }
            }
        }

        private void openBankPathTextbox(Label labelDoubleclicked)
        {
            if (!MLNameBox.Enabled) return; //if MLNameBox isn't enabled, none of the textboxes are; meaning we don't have anything selected yet, don't allow this

            string m_or_b = labelDoubleclicked.Name.Substring(0, 1);
            if(m_or_b == "m")
            {
                mBankPathTextbox.Visible = true;
                mBankPathTextbox.Text = shaveSurroundingQuotesAndSpaces(mTrueBankPath.Text);
                mBankPathTextbox.Focus();
                mBankPathLabel.BackColor = Color.Transparent;
            } else if (m_or_b == "b")
            {
                bBankPathTextbox.Visible = true;
                bBankPathTextbox.Text = shaveSurroundingQuotesAndSpaces(bTrueBankPath.Text);
                bBankPathTextbox.Focus();
                bBankPathLabel.BackColor = Color.Transparent;
            }
            debugLabel.Visible = true;
            debugLabel.Text = "Enter/Return to apply, ESC to cancel";
            labelDoubleclicked.Text = "bankPath: ";
        }

        private void BankPathDblClick(object sender, EventArgs e)
        {
            Label labelCalled = sender as Label;
            openBankPathTextbox(labelCalled);
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
            for (int i=0; i<lines.Length; i++)
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
            foreach(string line in fixedlines)
            {
                returnString += line + "\n";
            }
            Clipboard.SetText(returnString);

            //this took far longer than it would have to copy and paste...
        }

        private void showCsSupports()
        {
            string msg = "";
            foreach(string supportString in csSupLvls)
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

        private void debugTest1()
        {
            string initialInfo = testFindJson.Text;
            string[] errors = debuggy(initialInfo);
            testFindJson.Text = "";
            foreach (string errorCode in errors)
            {
                testFindJson.Text += errorCode + "\n";
            }

        }

        public string TesterBoxValue
        {

            get { return testFindJson.Text; }
        }

        private void debugButtonDialogueBox()
        {
            using (DebugForm debugger = new DebugForm())
            {
                debugger.MyParentForm = this;
                if (debugger.ShowDialog() == DialogResult.OK)
                {
                    //testFindJson.Text = debugger.TheValue; I don't think we need this
                }
            }
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

        private void OpenGameDir(object sender, EventArgs e)
        {
            OpenDir(gameDir.ToString());
        }
        private void OpenModDir(object sender, EventArgs e)
        {
            OpenDir(di.ToString());
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
