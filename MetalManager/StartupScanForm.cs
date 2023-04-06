using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using MetalManager.ConfigDataDaddy.Configuration;
using System.Xml;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace MetalManager
{
    public partial class StartupScanForm : Form
    {
        string summoner = null;
        public StartupScanForm(string whatSummonsMe = null)
        {
            InitializeComponent();
            summoner = whatSummonsMe;
        }

        public Form1 MyParentForm;

        DirectoryInfo gDir;
        DirectoryInfo modDi;
        private void GetDirectoriesFromMainForm()
        {
            gDir = ((Form1)MyParentForm).gameDir;
            if (gDir != null)
            {
                if (!Directory.Exists(gDir.ToString()))
                {
                    gDir = null;
                    ((Form1)MyParentForm).gameDir = null;
                    Form1.AddOrUpdateAppSettings("gameDirectory", "");
                }
            }

            modDi = ((Form1)MyParentForm).di;
            if (modDi != null)
            {
                if (!Directory.Exists(modDi.ToString()))
                {
                    modDi = null;
                    ((Form1)MyParentForm).di = null;
                    Form1.AddOrUpdateAppSettings("modDirectory", "");
                }
            }
        }

        public string[][] ProblemSongs
        {
            get { return SongsWithErrors.ToArray(); }
        }

        /// <summary>
        /// I'm mad at myself. I spelled this wrong.
        /// </summary>
        public string[][] SuspenededSongList
        {
            get { return SuspendedSongs.ToArray(); }
        }

        //just an example, I barely have an idea what I'm doing
        private void DisplayConfigCustomsongsToConsole()
        {
            foreach (var endpoint in ConfigDataDaddy.Customsongs.CustomsongsList)
            {
                //string securityGroups = string.Join(",", endpoint.SongInfo.SecurityGroupsAllowedToSaveChanges);
                Console.WriteLine(string.Format("Name = '{0}', Path = '{1}', LastWriteTime = '{2}', LastVerifiedTime = '{3}'.", endpoint.Name, endpoint.SongInfo.Path, endpoint.SongInfo.LastWriteTime, endpoint.SongInfo.LastVerifiedTime));
            }
        }

        List<string[]> SongsToBeScanned;
        List<string[]> SongsToBeIgnored;
        List<string[]> SuspendedSongs;
        List<string[]> SongsToDeleteFromConfig;

        string errTrack = "";
        private void CompareSongsBtwnConfigAndModDir()
        {
            //We're going to compare our lists, and decide what to do with each song. If we can't find a match, we need to add or delete a song from Config
            //Or we edit if we see a matching name (this assumes program has prompted user to rename any matching song names)
            
            
            

            SongsToBeScanned = new List<string[]>();
            SongsToBeIgnored = new List<string[]>();
            SuspendedSongs = new List<string[]>();
            SongsToDeleteFromConfig = new List<string[]>();
            SongsToDeleteFromConfig.AddRange(CurrCustomSongsInConfig);//we'll slowly remove this as we find our entries

            errTrack += "past inits. ";

            foreach (string[] ModDirSongInfo in CurrentSongsInModFolder)
            {
                //first check if there's no songs in Config; if there isn't, we're going to just add everything
                if(CurrCustomSongsInConfig.Count == 0)
                {
                    AddSongToConfigFile(ModDirSongInfo[0], ModDirSongInfo[1], ModDirSongInfo[2], "");
                    SongsToBeScanned.Add(ModDirSongInfo);
                    errTrack += "a. ";
                    continue;
                }


                //go through the songs in our config
                for (int i = 0; i < CurrCustomSongsInConfig.Count; i++)
                { 

                    string[] CnfgSongInfo = CurrCustomSongsInConfig[i];
                    errTrack += "b" + i + " " + CnfgSongInfo[0];

                    //check if a song in Mods folder already exists in config
                    if (CnfgSongInfo[0] == ModDirSongInfo[0])
                    {
                        
                        //The names of a songs match

                        //we'll see if the paths were the same and the Last Write Time match, to see if we need to rewrite them while we're here
                        bool PathsMatch = CnfgSongInfo[1] == ModDirSongInfo[1];
                        bool LWTMatch = CnfgSongInfo[2] == ModDirSongInfo[2];
                        if (!PathsMatch || !LWTMatch)
                        {
                            //our Path or LastWriteTime don't match, we want to rewrite this and tell it to be scanned
                            EditSongInConfigFile(ModDirSongInfo[0], ModDirSongInfo[1], ModDirSongInfo[2], "");
                            SongsToBeScanned.Add(ModDirSongInfo);
                            SongsToDeleteFromConfig[i] = null; //blank out this song from our delete List
                            goto FoundDecision; //break out of the for loop, and check next song in Mods directory
                        }

                        //we're about to start deciding what to do with each song: scan it or ignore it


                        //if this was called by "force recheck all errors", make everything what it was before and set it to a blank error result
                        //if there were also abnormalities in the Path or LWT, we wanted that fixed, which is why this comes after
                        if (summoner != null && summoner == "forceRecheck")
                        {
                            EditSongInConfigFile(ModDirSongInfo[0], ModDirSongInfo[1], ModDirSongInfo[2], "");
                            SongsToBeScanned.Add(ModDirSongInfo);
                            SongsToDeleteFromConfig[i] = null; //blank out this song from our delete List
                            goto FoundDecision; //break out of the for loop, and check next song in Mods directory
                        }

                        //If we're this far, all our song info matches; "suspended" or a Validation would be a real decision
                        string validity = CnfgSongInfo[3];
                        if (validity == "1" || validity == "2")
                        {
                            SongsToBeIgnored.Add(ModDirSongInfo);
                        }
                        if (validity == "suspended")
                        {
                            SuspendedSongs.Add(ModDirSongInfo);
                            //suspendedsongs are ignored from error scan, but are not allowed to be selected in Manager
                        }
                        else
                        {
                            //if (validity == "")
                            SongsToBeScanned.Add(ModDirSongInfo);
                        }
                        SongsToDeleteFromConfig[i] = null; //blank out this song from our delete List
                        goto FoundDecision; //break out of for loop, and stay away from AddSongToConfig
                    }
                }
                errTrack += "c. ";
                //We are done with our for loop. If we don't know what to do with this ModDirSongInfo, it means 
                //it's new/wasn't in our Config, we want to add it (and error scan)
                AddSongToConfigFile(ModDirSongInfo[0], ModDirSongInfo[1], ModDirSongInfo[2], "");
                SongsToBeScanned.Add(ModDirSongInfo);
                continue; //Goes to next ModDir custom song (after adding a new Mod)

            FoundDecision:
                errTrack += "d. ";
                continue; //Check next song in Mod dir

            }
            errTrack += "e. ";
            //we have gone through every entry in our Mod's folder and found matches to our Config songs
            //if there's anything that wasn't just touched in our Config file, we deleted a Mod, we need to delete its entry from the Config file
            foreach (string[] deletedMod in SongsToDeleteFromConfig)
            {
                errTrack += "f. ";
                if (deletedMod == null) { errTrack += "x. "; continue; }
                //if we got this far, it isn't null
                DeleteSongFromConfigFile(deletedMod[0]); //delete the song from the config file
                
            }
            errTrack += "g. ";
        }

        List<string[]> SongsWithErrors;
        private void GetAllSongsWithErrors()
        {
            SongsWithErrors = new List<string[]>();
            if (SongsToBeScanned.Count == 0)
            {
               
                //if we don't do this, we'll attempt to divide by 0
                //explainLabel.Text = "  Starting up Metal Manager...";
                //totalProgressBar.Value = totalProgressBar.Maximum;
                
                return;
            }

            //int remainingValueOfProgressBar = totalProgressBar.Maximum - totalProgressBar.Value;
            //double percentOfProgressValuePerSong = 1/SongsToBeScanned.Count;

            int sNum = 0;
            foreach(string[] songInfo in SongsToBeScanned)
            {
                sNum++;
                
                //explainLabel.Text = "Scanning for errors: " + songInfo[0];

                if(songInfo[0] == "(game)")
                {
                    string gameJson = getModJson(songInfo);
                    if (BuggyD_StopAtFirst(gameJson) == false)
                    {
                        //We found a critical error
                        //SongsWithErrors.Add(songInfo); //<<all this does is go through each of them and say "suspended"
                        SuspendedSongs.Add(songInfo);

                        EditSongInConfigFile(songInfo[0], songInfo[1], songInfo[2], "suspended");
                    }
                    else
                    {
                        //we found NO critical errors! song approved!
                        string intStamp = "1";
                        //if (songHasCustomMusicBank(songInfo))
                        //hang on a mo.. it's ALWAYS gonna have a music.bank!
                        EditSongInConfigFile(songInfo[0], songInfo[1], songInfo[2], intStamp); //we added it to config when we added it to songstobescanned
                    }
                    continue;
                }


                string modJson = getModJson(songInfo); //will take the Mod dir and path dir to find its customsongs.json
                if (BuggyD_StopAtFirst(modJson) == false)
                {
                    //We found a critical error
                    SuspendedSongs.Add(songInfo);
                    EditSongInConfigFile(songInfo[0], songInfo[1], songInfo[2], "suspended");
                    
                } else
                {
                    //we found NO critical errors! song approved!
                    string intStamp = "1";
                    if (songHasCustomMusicBank(songInfo))
                    {
                        intStamp = "2";
                    }
                    EditSongInConfigFile(songInfo[0], songInfo[1], songInfo[2], intStamp);
                }

                /*double totalAddedProgressPrcnt = percentOfProgressValuePerSong * sNum;
                int integerAddedProgressPrcnt = Convert.ToInt32(totalAddedProgressPrcnt);
                if (integerAddedProgressPrcnt + remainingValueOfProgressBar > totalProgressBar.Maximum)
                {
                    totalProgressBar.Value = totalProgressBar.Maximum;
                }*/

            }
            
            //explainLabel.Text = "Starting Metal Manager...";
            //totalProgressBar.Value = totalProgressBar.Maximum;
        }


        private bool songHasCustomMusicBank(string[] songInfo)
        {
            string pathToPossibleMusicBank = modDi + songInfo[1] + "\\Music.bank";
            if (File.Exists(pathToPossibleMusicBank))
            {
                return true;
            } else
            {
                return false;
            }
        }

        private void checkWhichCustomMusicBankWereUsing()
        {
            //we're just checking if we're using the Library or the Game's original



        }

        private string getModJson(string[] songInfo, bool noLineReturns = false)
        {
            //with this function, we're returning a string of the information from our actual customsongs.json that the game reads (in the StreamingAssets folder)
            string pathToModsJson = modDi + songInfo[1] + "\\customsongs.json";
            if(songInfo[0] == "(game)")
            {
                pathToModsJson = gDir + "\\customsongs.json";
            }


            if (!File.Exists(pathToModsJson))
            {
                //MessageBox.Show("PathToModsJson: " + pathToModsJson);
                CloseManagerFromError("GMJ01");
                return "-2";
            }

            using (StreamReader sr = File.OpenText(@pathToModsJson))
            {
                string s = "";

                string fullText = sr.ReadToEnd();
                if (noLineReturns)
                {
                    string trimmedLine = NormalizeWhiteSpace(fullText);
                    s = trimmedLine;
                } else { s = fullText; }

                return s;
            }
        }



        /*
        private void CompareSongsBetweenConfigAndModFolder()
        {
            //We may have songs in our ModFolderSongs List that aren't in our ConfigSongs List
            //This would mean the user added a song to their Mods folder that we're not aware of; we want to immediately add it with the LVT set to ""
            //if we set it to "suspended", it means we're aware of the song, but we have problems/errors with it and refuse to allow it to be used

            //we also might have songs in our ConfigSongs List that aren't in our ModFolderSongs List
            //This would mean the user DELETED a song from their Mods folder. We just need to delete the entry from the config file

            //if we have a matching Song Name in ConfigSongs and ModFolderSongs, we still need to verify they have the same Path and LastWriteTime
            //if they have same name but don't share the Path or LastWriteTime, this would mean our user overwrote a Mod
            //or its .json; we need to edit the ConfigSong info and set LVT to ""(perhaps after this is done, so we dont screw it up??)

            //if they have same name, and matching Path/LWT, we check for a last verified date;
            //>>>if it's "", we want to scan it; if VerifiedTime is before WriteTime, we want to scan it(I donno if that's possible with this sequence)
            //>>>>>if it has "suspended", we don't want to scan it, but we need send info to Form1 to disable the song selection


            foreach(string[] CnfgSongInfo in CurrCustomSongsInConfig)
            {
                for(int i=0; i<CurrentSongsInModFolder.Count; i++)
                {
                    if(CnfgSongInfo[0] == CurrentSongsInModFolder[i][0])
                    {
                        //The Config's song Name matches a song in our Mod folder
                        //We HAVE to make a decision of what to do with it: tell it to be scanned, ignored, or suspended

                        bool songPathsMatch = CnfgSongInfo[1] == CurrentSongsInModFolder[i][1];
                        bool songLWTMatch = CnfgSongInfo[2] == CurrentSongsInModFolder[i][2];
                        bool verifiedAfterLastModify = false;
                        string lvtStr = CnfgSongInfo[3];

                        //Check if LastVerifiedTime has a date, or a
                        if (lvtStr != "" && lvtStr != null && lvtStr != "suspended")
                        {
                            //we have a date in LastVerifiedTime
                            DateTime lvtDT;
                            try
                            {
                                lvtDT = DateTime.Parse(lvtStr);
                                //if we're here, lvtDT has a DateTime
                                string lwtStr = CnfgSongInfo[2];
                                DateTime lwtDT = DateTime.Parse(lwtStr);
                                verifiedAfterLastModify = lwtDT < lvtDT;
                            }
                            catch (FormatException)
                            {
                                CloseManagerFromError("You didn't program this yet.");
                                //ErrorSoIDontForgetThis
                                //Need to act like verified was "", so we just scan it as we normally would
                            }

                            

                        }
                        else if (lvtStr == "" || lvtStr == null)
                        {
                            //we immediately know it needs to be scanned for errors

                        } else if(lvtStr == "suspended")
                        {
                            //we immediately know we DON'T want to scan it for errors, and it needs to be disabled from selection
                            //if our path or LastWriteTime don't match, though, we need to ignore it
                        }



                        if (songPathsMatch && songLWTMatch && verifiedAfterLastModify)
                        { 
                            //we don't have to scan this file
                        } else if (!songPathsMatch || !songLWTMatch)
                        {
                            //our song paths don't match; user had the same name of a song in subdirectory
                            //or the song's customsongs.json LastWriteTime do not match; user updated it
                            //we want to scan this song's json for errors

                        }


                        break;//since we found a song that match Names, we don't need this ForLoop anymore, break out of it, go to next CnfgSongInfo
                    }


                    //end of for loop
                }

                //if we've gotten this far, we're done with the ForLoop. If we didn't find a match for this ConfigSong's name in our Mod folder, it
                //means that we deleted a song in our Mod folder at some point. We need to delete the entry from the App.config


               

            }

            //end of comparing Songs Lists
        }
        */


        List<string[]> CurrCustomSongsInConfig;

        /// <summary>
        /// Stores a list strings[] with all song info for each element under Customsongs in App.config
        /// </summary>
        private void GetCustomSongsInConfig()
        {
            CurrCustomSongsInConfig = new List<string[]>();


            //DeleteAllUnfoundSongsInConfig();
            

            string[][] songListScrapedFromConfig = Form1.ReadTheGODDAMNConfigFile();

            foreach (string[] sngInfo in songListScrapedFromConfig)
            {
                CurrCustomSongsInConfig.Add(sngInfo);
            }

            /*
            foreach (var endpoint in ConfigDataDaddy.Customsongs.CustomsongsList)
            {
                string[] endpointsSongInfo = { endpoint.Name, endpoint.SongInfo.Path, endpoint.SongInfo.LastWriteTime, endpoint.SongInfo.LastVerifiedTime };
                CurrCustomSongsInConfig.Add(endpointsSongInfo);
            }*/
            
        }
        /*
        private void DeleteAllUnfoundSongsInConfig()
        {
            foreach (var endpoint in ConfigDataDaddy.Customsongs.CustomsongsList.Reverse())
            {
                if (!File.Exists(endpoint.SongInfo.Path)) DeleteSongFromConfigFile(endpoint.Name);
            }
        }*/

        List<string[]> CurrentSongsInModFolder;

        /// <summary>
        /// Stores a list of strings[] with all song info for each custom song within our Mod folder with a valid customsongs.json
        /// </summary>
        private void GetCurrentCustomSongsInModFolder()
        {
            //This function fills CurrentSongsInModFolder with a SongInfo array of each CustomSong directory, as it ensures it's valid/not meant to be ignored

            //Original:
            //this returns a string that says the names of all VALID Mod selections, searching even through subdirectories for a customsongs.json
            //example of unvalid Mod is one in an _Original folder            

            CurrentSongsInModFolder = new List<string[]>();

            //we also look for the game's current customsongs.json
            if (gDir != null)
            {
                if (gDir.Exists)
                {
                    string actualJsonPath = gDir + "\\customsongs.json";
                    if (!File.Exists(actualJsonPath)) goto ModsCatcher; //we have the game linked, but we don't have a customsongs.json

                    string NameOfMod = "(game)"; //no other custom song should say this because we cant have parentheses in a folder
                    string PathOfMod = "(gamedir)";
                    
                    DateTime mLWTF = File.GetLastWriteTime(actualJsonPath);
                    string modLWT = mLWTF.ToString("MM/dd/yyyy HH:mm:ss");//example: 03/29/2023 22:45:01

                    string[] songInfo = { NameOfMod, PathOfMod, modLWT };

                    //songInfo = CheckToRenameDuplicateSongs(songInfo); //we shouldn't need this
                    //if (songInfo == null) return; this was for CheckToRename. It'll only return null if it went through 100 rename possibilities and they're all taken
                    CurrentSongsInModFolder.Add(songInfo);
                }
            }

            //we've skipped here if we didn't have game linked, or the game's customsongs.json doesn't exist
            ModsCatcher:
            
            

            string modListString = "";
            if (modDi == null)
            {
                //No Mods folder!
                //MessageBox.Show("No Mod folder found, skipping this");
                return;
            }
            if (modDi.Exists)
            {
                //searches through the Mods folder and returns an array of strings with the full path/filename of ANY customsongs.json
                string[] allJSONPaths = Directory.GetFiles(modDi.ToString(), "customsongs.json", SearchOption.AllDirectories);
                //(we later make sure it's a "valid" entry, like ensuring it's not in a folder named _Original

                foreach (string path in allJSONPaths)
                {
                    string[] foldersInPath = path.Split('\\'); //splits our string into an array of string of directory names; the \\'s that were once there are now gone
                    if (foldersInPath[foldersInPath.Length - 2] == "_Original")
                    {
                        //we found a JSON in a folder with an Original file. We could store it later if we want a "restore" button to only appear if there's a JSON in an _Original folder
                        continue;
                    }
                    //if we got this far, we see a customsongs.json, and it's not in an "Original" folder

                    //get the name of the mod; its name is its Directory
                    string NameOfMod = foldersInPath[foldersInPath.Length - 2]; //name of directory

                    //get the path of our mod relative to our Mod folder
                    string PathOfMod = string.Join("\\", foldersInPath, 0, foldersInPath.Length-1);//gives us the full directory to the .json (no filename)
                    int lengthOfModFolderPath = modDi.ToString().Length;
                    if(lengthOfModFolderPath >= PathOfMod.Length)
                    {
                        CloseManagerFromError("GCCSMF:A1");
                    }
                    PathOfMod = PathOfMod.Substring(lengthOfModFolderPath); //remove portion of Mod folder path from the path to the .json, to shorten it

                    if (!File.Exists(path)) CloseManagerFromError("GCCSMF:A2"); //there's no way that i need this....
                    DateTime mLWTF = File.GetLastWriteTime(path);
                    string modLWT = mLWTF.ToString("MM/dd/yyyy HH:mm:ss");//example: 03/29/2023 22:45:01

                    string[] songInfo = { NameOfMod, PathOfMod, modLWT };

                    songInfo = CheckToRenameDuplicateSongs(songInfo); //will change our Mod name, Path info, and LWT if it finds a duplicate
                    if (songInfo == null) return;
                    CurrentSongsInModFolder.Add(songInfo);
                    
                }

                return;
            }
            else
            {
                //No Mods folder!
                return;
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





        /// <summary>
        /// Adds a Customsong entry to App.config, whether it existed or not
        /// </summary>
        /// <param name="sName">Name of custom song</param>
        /// <param name="sPath">Path to customsongs.json</param>
        /// <param name="sLWT">Date/Time of Last Modified for customsongs.json</param>
        /// <param name="sLVT">Date/Time when .json was last scanned for errors, or "suspended" if no error scan</param>
        public static void AddSongToConfigFile(string sName, string sPath, string sLWT, string sLVT)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            // create new node <add key="Region" value="Canterbury" />
            var nodeSong = xmlDoc.CreateElement("add");
            nodeSong.SetAttribute("name", sName);
            nodeSong.SetAttribute("path", sPath);
            nodeSong.SetAttribute("lwt", sLWT);
            nodeSong.SetAttribute("lvt", sLVT);

            xmlDoc.SelectSingleNode("//CustomSongsConfig/Customsongs").AppendChild(nodeSong);
            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("CustomSongsConfig/Customsongs");
        }

        /// <summary>
        /// Edits a Customsong from App.config, currently crashes if it can't find one
        /// </summary>
        /// <param name="sName">Name of custom song</param>
        /// <param name="sPath">Path to customsongs.json</param>
        /// <param name="sLWT">Date/Time of Last Modified for customsongs.json</param>
        /// <param name="sLVT">Date/Time when .json was last scanned for errors, or "suspended" if no error scan</param>
        public static void EditSongInConfigFile(string sName, string sPath, string sLWT, string sLVT)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            var songNode = xmlDoc.SelectSingleNode("//CustomSongsConfig/Customsongs/add[@name='" + sName + "']");

            if (songNode == null)
            {
                AddSongToConfigFile(sName, sPath, sLWT, sLVT);
                return;
            }

            songNode.Attributes["path"].Value = sPath;
            songNode.Attributes["lwt"].Value = sLWT;
            songNode.Attributes["lvt"].Value = sLVT;

            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("CustomSongsConfig/Customsongs");
        }

        /// <summary>
        /// Deletes a Customsong from App.config, currently crashes if it can't find one
        /// </summary>
        /// <param name="sName">Name of custom song</param>
        public static void DeleteSongFromConfigFile(string sName)
        {
            try
            {
                var xmlDoc = new XmlDocument();

                xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

                XmlNode nodeSongName = xmlDoc.SelectSingleNode("//CustomSongsConfig/Customsongs/add[@name='" + sName + "']");
                nodeSongName.ParentNode.RemoveChild(nodeSongName);

                xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("CustomSongsConfig/Customsongs");
            } 
            catch (Exception ex)
            {
                MessageBox.Show("Failure to remove song from config:\n"+ex.Message);
            }
        }

        private void StartUpVerify()
        {
            //totalProgressBar.Value = 0;
            GetDirectoriesFromMainForm();
            GetCurrentCustomSongsInModFolder();
            //totalProgressBar.Value = 20;
            GetCustomSongsInConfig();
            //totalProgressBar.Value = 30;
            CompareSongsBtwnConfigAndModDir();
            //totalProgressBar.Value = 40;
            GetAllSongsWithErrors();
            //SuspendAllSongsWithErrors(); we suspended them already
            




            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        

        private void SuspendAllSongsWithErrors()
        {
            /*if (SongsWithErrors.Count > 0)
            {
                if (SongsWithErrors.Count == 1)
                {
                    ReportErrors();
                }
                else
                {
                    ReportErrors();
                }
            }*/
            
            if (SongsWithErrors.Count > 0)
            {
                foreach(string[] susSongInfo in SongsWithErrors)
                {
                    EditSongInConfigFile(susSongInfo[0], susSongInfo[1], susSongInfo[2], "suspended");
                    SuspendedSongs.Add(susSongInfo);
                }
            }


        }

        private string StringShortener(string ogString, int maxLength)
        {
            if (ogString.Length > maxLength)
            {
                string shortened = ogString.Substring(0, maxLength-3) + "...";
                return shortened;
            }
            else
            {
                return ogString;
            }
        }


        /// <summary>
        /// Searches through the list of CurrentSongsInModFolder for sInfo's song name; checks to rename this or the other duplicate song
        /// </summary>
        /// <param name="sInfo"></param>
        /// <returns></returns>
        private string[] CheckToRenameDuplicateSongs(string[] sInfo)
        {
            //need to check if the duplicate already existed
            //whatever sInfo is, it has NOT been added to CurrentSongsInModFolder yet. This function is a preliminary to being added to this list.
            //sInfo will have songName at 0, song's relative directory [to mods folder] at 1

            string[] scndSngWithMatchingName = null; //the song with a matching name already in our sngsInModFolder list
            string[] dopplegangerSong = null; //the song that will identified as the new song
            bool firstSongFoundWasImposter = false; //we'll use this at the end to check if we just changed the song we're looking at, or one previously added
            //false because neither one may be an imposter :O

            for (int i = 0; i < CurrentSongsInModFolder.Count; i++)
            {
                if (sInfo[0] == CurrentSongsInModFolder[i][0])
                {
                    //the song we just looked at in our Mods folder matches another song's name in mod folder (one's probably in a sub directory)
                    scndSngWithMatchingName = CurrentSongsInModFolder[i];
                    goto FoundDuplicate;
                }
            }
            //if we didn't find a duplicate, just return the song back
            return sInfo;

        FoundDuplicate:
            //we found a duplicate in our mods list. we need to check if one entry already exists in our config folder
            //(this is why we flipped the order of grabbing songs from mod folder and grabbing songs from config)

            //we're going to check if the first song actually existed in our config
            //we'll check its path
            for (int i = 0; i < CurrCustomSongsInConfig.Count; i++)
            {
                //if the song['s path] we found in mods folder mathes the song['s path] in config, then this should not be renamed
                if (scndSngWithMatchingName[1] == CurrCustomSongsInConfig[i][1])
                {
                    //the song we already added is good, and this song we're looking at now is the doppleganger (the new song)
                    dopplegangerSong = sInfo;
                    goto FoundDoppleganger;
                }
            }

            //if we're here, the song we found in mods folder first wasn't in config (subdirectory possibly lower in alphabet order than songname)
            //let's check if this song was in config
            for (int i = 0; i < CurrCustomSongsInConfig.Count; i++)
            {
                //if the song['s path] we found in mods folder mathes the song['s path] in config, then this should not be renamed
                if (sInfo[1] == CurrCustomSongsInConfig[i][1])
                {
                    //the song we're looking at now is the real Spiderman, previous one we knew was the doppleganger (the new song)
                    dopplegangerSong = scndSngWithMatchingName;
                    firstSongFoundWasImposter = true;
                    goto FoundDoppleganger;
                }
            }

            //if we got this far, neither one of these were in the mods folder before. We'll just rename this we're looking at.
            dopplegangerSong = sInfo;



        FoundDoppleganger:



            //explainLabel.Text = "Duplicate Mod name found (" + sInfo[0] + ")," + Environment.NewLine +
            //"Renaming directory...";

            int currentCheck = 1;
            int maximumChecks = 99;
            string newPath = "";

            while (currentCheck <= maximumChecks)
            {
                //we're looking for a folder that DOESN'T exist, or doesn't have files
                //it's going go from DuHast, to DuHast2, or DuHast22, because I didn't program this right. 
                //if it sees DuHast2 twice, it's just going to try to rename to "DuHast2" + "2"
                //oh well.....

                currentCheck++;//our currentCheck starts at 2 because of this, which we want
                string pathToCheck = modDi + dopplegangerSong[1] + currentCheck;

                //if th
                if (!Directory.Exists(@pathToCheck))
                {
                    newPath = pathToCheck;
                    if (checkIfWeCanRenameFile(dopplegangerSong, currentCheck.ToString()))
                    {
                        goto GotNewPathName;
                    } else
                    {
                        System.Windows.Forms.Application.Exit();
                        return null;
                    }

                }

                //if we got this far, the directory exists; check if it has no files
                DirectoryInfo DirPath = new DirectoryInfo(@pathToCheck);
                int fileCount = DirPath.GetFiles("*", SearchOption.AllDirectories).Length;
                if (fileCount == 0)
                {
                    //this directory has no files in it. we're going to delete it so this can take its place

                    if (checkIfWeCanRenameFile(dopplegangerSong, currentCheck.ToString()))
                    {
                        Directory.Delete(pathToCheck, true);
                        newPath = pathToCheck;
                        goto GotNewPathName;
                    }
                    else
                    {
                        System.Windows.Forms.Application.Exit();
                        return null;
                    }
                }

            }

            CloseManagerFromError("mduprn-01");
            return sInfo;

        GotNewPathName:

            string ogPath = modDi + dopplegangerSong[1];
            try
            {
                Directory.Move(ogPath, newPath);
            }
            catch
            {
                CloseManagerFromError("DirMovErr-01");
                return sInfo;
            }

            string renamedSongDir = newPath.Replace(modDi.ToString(), ""); //get the relative path from our changed song
            //MessageBox.Show("RenamedSongDir: " + renamedSongDir + "\nShould have proper / or \\ as config");
            string newSongName = System.IO.Path.GetFileName(newPath);
            DateTime newLWTF = File.GetLastWriteTime(newPath); //newpath should have customsongs.json

            if (firstSongFoundWasImposter)
            {
                //the first song we added was an imposter, meaning a duplicate song that's new to us. we've already changed its directory, this song's fine
                return sInfo; //return the original info given to us
            }
            else
            {
                //either this song wasn't in the config, or neither song was in the config. This song got renamed.

                string[] newSInfo = { newSongName, renamedSongDir, newLWTF.ToString() };
                return newSInfo;
            }



            /*
            if(CopyFilesRecursively(originalPath, newPath, 1024))
            {
                string renamedSongDir = newPath.Replace("\\customsongs.json", "");
                string newSongName = System.IO.Path.GetFileName(renamedSongDir);

                DateTime mLWTF;
                if (!newPath.Contains("\\customsongs.json"))
                {
                    mLWTF = File.GetLastWriteTime(newPath + "\\customsongs.json");
                } else
                {
                    mLWTF = File.GetLastWriteTime(newPath);
                }

                string[] newSInfo = { newSongName, renamedSongDir, mLWTF.ToString() };
                return newSInfo;

            } else
            {
                CloseManagerFromError("mduprn-02");
                return sInfo;
            }*/


        }

        /// <summary>
        /// Politely asks the user to rename a directory. Returns true if Yes. Otherwise returns false, and: opens the problematic directory if No, or nothing else if Cancel
        /// </summary>
        /// <param name="sInfo"></param>
        /// <param name="renamedSuffix"></param>
        /// <returns></returns>
        private bool checkIfWeCanRenameFile(string[] sInfo, string renamedSuffix)
        {
            string dupeModPath = pathShortener(modDi + sInfo[1], 40);
            dupeModPath = dupeModPath.Substring(0, 1).ToUpper() + dupeModPath.Substring(1);
            string rewriteWarningTitle = "We already got one of those";
            string rewriteWarningMessage = "Duplicate song title found: " + sInfo[0] + "\n" +
                dupeModPath +
                "\n\nCan we rename it to " + sInfo[0] + renamedSuffix.ToString() + "?\n\n" +
                "Pressing Yes will rename it and continue,\n" +
                "Pressing No will cancel startup and open directory for you to rename it.\n" +
                "Pressing Cancel will cancel startup and nothing else.";
            MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
            DialogResult result = MessageBox.Show(rewriteWarningMessage, rewriteWarningTitle, buttons);
            if (result == DialogResult.Yes)
            {
                return true;
            }
            if (result == DialogResult.No)
            {
                string fullPath = modDi + sInfo[1]; //ie: M:/Here/We/Go/Mods/BryansMix/Rammstein/DuHast ... we want Rammstein folder
                string parentDir = System.IO.Directory.GetParent(fullPath).ToString();
                OpenDir(parentDir);
                return false;
            }
            else
            {
                return false;
            }
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

            }
            else
            {
                MessageBox.Show("Error: Directory could not be found.");
            }
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

                //make sure none of the directories have ridiculous file names

                if (dirs.Length > numberOfDirectoriesToShow)
                {
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

        private bool CopyFilesRecursively(string sourcePath, string targetPath, int maximumSizeMegabytes = 0)
        {
            //First we check if the folder exceeds our max file size (in bytes)
            string sPath = sourcePath.Replace("\\customsongs.json", "");
            string nsPath = targetPath.Replace("\\customsongs.json", "");

            string songName = System.IO.Path.GetFileName(sPath);
            string newSongName = System.IO.Path.GetFileName(nsPath);
            string explText1 = "Duplicate Mod name found (" + songName + "),"+Environment.NewLine;
            string explText2 = "Renaming to " + newSongName + " and verifying files...";
            //explainLabel.Text = explText1;
            long maximumSize = maximumSizeMegabytes * 1000000;

            if(maximumSize > 0)
            {
                long fileSize = 0;
                foreach (string dirPath in Directory.GetDirectories(@sPath, "*", SearchOption.AllDirectories))
                {
                    fileSize += dirPath.Length;
                }

                if(fileSize > maximumSize)
                {
                    CloseManagerFromError("mdupFs2b-02");
                    return false;
                }
            }

            if (!Directory.Exists(nsPath))
            {
                Directory.CreateDirectory(nsPath);
            }


            //Create all of the subdirectories
            foreach (string dirPath in Directory.GetDirectories(@sPath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(@dirPath.Replace(sPath, nsPath));
            }

            //Copy all the files (Replaces any files with the same name, which is why we verify no files are in folder)
            foreach (string newPath in Directory.GetFiles(@sPath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(@sPath, nsPath), true);
                //if (CheckFileHasCopied(nsPath)) continue;
            }

            Directory.Delete(sPath); //this doesn't work if it's not empty
            //explainLabel.Text = "Getting songs from Mod folder...";
            return true;
        }

        /*
        private bool CheckFileHasCopied(string FilePath)
        {
            try
            {
                if (File.Exists(FilePath))
                    using (File.OpenRead(FilePath))
                    {
                        return true;
                    }
                else
                    return false;
            }
            catch (Exception)
            {
                Thread.Sleep(100);
                return CheckFileHasCopied(FilePath);
            }

        }*/


        private string ReportErrors()
        {
            string message = "";
            string title = "Errors found in "; 
            if (SongsWithErrors.Count == 1)
            {
                title += StringShortener(SongsWithErrors[0][0], 35);

                message = "Initial scan found errors in customsongs.json for " + SongsWithErrors[0][0] + Environment.NewLine;
                message += "Would you like to pull up the Debug Panel to fix it?" + Environment.NewLine + Environment.NewLine;

                message += "You can skip this process, but to prevent crashes, doing so will suspend" + Environment.NewLine + "Metal Manager's access to this song until all errors are removed." + Environment.NewLine + Environment.NewLine; ;
            } else if(SongsWithErrors.Count > 1)
            {
                title += "multiple songs";

                message = "Initial scan found errors in customsongs.json the following songs: " + Environment.NewLine + SongsWithErrors[0][0] + Environment.NewLine;
                message += SongsWithErrors[1][0] + Environment.NewLine;
                if(SongsWithErrors.Count > 2)
                {
                    int numMoreSongsWithErrors = SongsWithErrors.Count - 2;
                    message += "(" + numMoreSongsWithErrors + " other songs)" + Environment.NewLine;
                    message += "Would you like to pull up the Debug Panel to fix it?" + Environment.NewLine + Environment.NewLine;
                    message += "You can skip this process, but to prevent crashes, doing so will suspend" + Environment.NewLine + "Metal Manager's access to these songs until all errors are removed." + Environment.NewLine + Environment.NewLine; ;
                    
                }
            }
            message += "Yes brings up Debug Panel, No continues to Metal Manager.";

            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.Yes)
            {
                //SaveLevelInfo
                //return true;
            }
            if (result == DialogResult.No)
            {
                //just switch the page
                //return true;
            }
            else
            {
                //LevelButtons[levelInt].Focus();
                //return false;

                //don't carry out the page switch
            }
            return "";
        }


        private string replaceAllTabs(string fullJson)
        {
            if (fullJson.Contains('\t'))
            {
                string newJson = fullJson.Replace("\t", "        ");
                return newJson;
            }
            else
            {
                return fullJson;
            }
        }

        #region BuggyD StopAtFirst
        public bool BuggyD_StopAtFirst(string fullJson)
        {
            List<string> linesWithErrors = new List<string>();

            if(fullJson.Replace("\r", "") == "{\n    \"customLevelMusic\" : [\n\n    ]\n}") return true;

            string noTabs = replaceAllTabs(fullJson);
            string fixedJson = noTabs;
            if (fullJson != noTabs)
            {
                linesWithErrors.Add("...:tabs");
            }

            bool fatalErrorEncountered = false;//we use this if we find a fatal error; we keep skipping lines until we get out of the level with the error


            string[] fixedJsonLines = fixedJson.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);//it's not gonna have empty entries, damnit
            
            if (fixedJsonLines.Length < 10) { return false; }

            int bmCounter = 0; int mmCounter = 0;//we will use these to make sure we don't have two MainMusics, or two BossMusics, per level

            int currPlaceInExpctdEntry = 0; //current place in expected entry
            int i = 0;
            int threshold = 0;//threshold used to prevent infinite loop
            
            while (i < fixedJsonLines.Length - 1)
            {
                //use threshold to prevent infinite loop
                #region PreventInfiniteLoop
                threshold++; //threshold is used just to verify this doesn't get stuck in an infinite loop
                if (threshold > 300)
                {
                    //string[] tooLong = { "2long" };
                    linesWithErrors.Add("(2long)");
                    return false;
                }
                #endregion PreventInfiniteLoop

                #region DebugLines
                string line_unaltered = fixedJsonLines[i]; //get the line we're checking
                line_unaltered = line_unaltered.Replace("\r", "");//these things keep fucking me
                string line_nospaces = NormalizeWhiteSpace(fixedJsonLines[i], true); //gives us us the individual line in the JSON, with no spaces whatsoever

                if (line_nospaces == null || line_nospaces == "")
                {
                    return false;
                }

                //if a fatal error has occured, keep skipping lines until we get to a line with "]" or "LevelName":


                string finalChar = line_nospaces.Substring(line_nospaces.Length - 1, 1); //we only care about the last item on the line

                if (i < 2)
                {
                    //if we're here, we're checking either the first or 2nd line
                    if (i == 0)
                    {
                        //first line, we only want {
                        if (line_nospaces.Contains("{"))
                        {
                            string unexpChars = line_nospaces.Replace("{", "");
                            if (unexpChars.Length > 0)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            //there's no { in the first line
                            //label is missing, we need to look around for something identifiable
                            //technically, they could have the first line be-> { "customLevelMusic":[ and still be correct
                            string[] firstLineError = { "Error reading Json" };
                            if (fixedJsonLines.Length < 2) return false; //catch-all in case JSON is very short

                            if (fixedJsonLines[1].Contains("{"))
                            {
                                //our { is on the next line
                                return false;
                            }
                            else if (fixedJsonLines[1].Contains("\"customLevelMusic\""))
                            {
                                //we can't find {, but customLevelMusic is on next line
                                return false;
                            }
                        }
                        i++; continue;
                    }
                    else
                    {

                        //second line, we want "customLevelMusic" : [
                        string firstLineNoSpaces = NormalizeWhiteSpace(fixedJsonLines[0], true);

                        if (line_nospaces != "\"customLevelMusic\":[")
                        {
                            string[] secondLineError = { "Error reading Json" };
                            if (fixedJsonLines.Length < 3) return false; 
                            if (fixedJsonLines[2].Contains("{"))
                            {
                                //our next line contains the next sequence, we forgot the CLM opener
                                secondLineError[0] = "2:forgotCLM";
                                return false;
                            }
                            else if (line_nospaces.Contains("{") && fixedJsonLines[2].Contains("\"customLevelMusic\":["))
                            {
                                //this line has a {, which our first line has, and our next line has the customLevelMusic thing
                                secondLineError[0] = "2:dupe";
                                return false;

                            }
                            else if (line_nospaces.Contains("{") && firstLineNoSpaces.Contains("{\"customLevelMusic\":["))
                            {
                                //the user seems to have combined { and "customLevelMusic" : [ onto one line. that works, but we don't want that
                                secondLineError[0] = "2:forgotClmFormat"; //we want it to add another
                                return false;
                            }
                            else
                            {
                                linesWithErrors.Add("2:clmF"); //customLevelMusic opening line incorrect format
                                return false;
                            }


                        }
                        i++; continue;
                    }

                }
                else if (i >= 2)
                {
                    if (i == 2)
                    {
                        //we're on our first line out of the openers, we should verify our placement

                        if (!line_unaltered.Contains(expectedFields[currPlaceInExpctdEntry]))
                        {
                            //the first line did not have a {
                            /*
                            string[] firstTwoLines = { fixedJsonLines[i], fixedJsonLines[i + 1] };
                            int backOnTrack = verifyWTFsGoingOnFirstLine(firstTwoLines);
                            if (backOnTrack == -1)
                            {
                                string[] fatalError = { (i + 1) + ":fatality" };
                                fatalErrorEncountered = true;
                                return false;
                            }
                            else
                            {
                                currPlaceInExpctdEntry = backOnTrack;
                                linesWithErrors.Add((i) + ":forgotClmFormat"); //we don't want i-1
                                return false;
                            }*/
                            return false;
                        }

                    }

                    //if we're above our length, we might be done, check for closing ]
                    if (currPlaceInExpctdEntry >= expectedFields.Length)
                    {
                        if (line_unaltered.Contains("]"))
                        {
                            goto FoundCLMEnd;
                        }
                        else
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

                    }
                    else if (currPlaceInExpctdEntry == 1)
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
                        if (fixedJsonLines[i].Contains("\"BossMusic\"") ||
                            fixedJsonLines[i].Contains("\"MainMusic\""))
                        {
                            currPlaceInExpctdEntry = 2;
                            continue;
                        }
                        else if (!hasMatchingLabel)
                        {

                        }
                    }

                    string thisLn = fixedJsonLines[i];
                    if (i + 1 > fixedJsonLines.Length) { goto FoundCLMEnd; }
                    string nxtLn = fixedJsonLines[i + 1];

                    bool LineIsClean = getLineErrors_bool(thisLn, nxtLn, currPlaceInExpctdEntry);
                    if (!LineIsClean) { return false; }



                    #region Look For Boss/MainMusic Doubles
                    //if (!hasMatchingLabel) bmCounter = 0; mmCounter = 0;
                    if (currPlaceInExpctdEntry == 2 &&
                        (fixedJsonLines[i].Contains("\"BossMusic\"") || fixedJsonLines[i].Contains("\"MainMusic\"")))
                    {
                        if (fixedJsonLines[i].Contains("\"BossMusic\""))
                        {
                            bmCounter++;
                        }
                        else if (fixedJsonLines[i].Contains("\"MainMusic\""))
                        {
                            mmCounter++;
                        }
                    }
                    else if ((currPlaceInExpctdEntry == 0 || currPlaceInExpctdEntry == 1) &&
                          hasMatchingLabel)
                    {
                        bmCounter = 0; mmCounter = 0;
                    }
                    if (bmCounter > 1)
                    {
                        //lineErrors += ("(bmdup)");
                        bmCounter = 0;
                        return false;
                    }
                    if (mmCounter > 1)
                    {
                        //lineErrors += ("(mmdup)");
                        mmCounter = 0;
                        return false;
                    }
                    #endregion Look For Boss/MainMusic Doubles




                    currPlaceInExpctdEntry++;
                }


                i++;


                #endregion DebugLines
            }

        FoundCLMEnd:
            //since we just broke out of the while loop, i should still be the line number with the ] 
            if (i < fixedJsonLines.Length)
            {
                string allFinalLines = "";
                for (int g = i; g < fixedJsonLines.Length; g++)
                {
                    allFinalLines += fixedJsonLines[g];
                }

                if (fixedJsonLines[i].Contains("]"))
                {
                    if (i < fixedJsonLines.Length - 1)
                    {
                        string combinedFinalLines = allFinalLines;
                        string combinedFinalsNS = NormalizeWhiteSpace(combinedFinalLines, true);
                        if (!combinedFinalsNS.Contains("}") || !combinedFinalsNS.Contains("]"))
                        {
                            linesWithErrors.Add(i + 1 + "+:clmClose");
                            return false;

                        }
                        else
                        {
                            string anomaliesInFinalLines = allFinalLines;
                            anomaliesInFinalLines = NormalizeWhiteSpace(anomaliesInFinalLines, true);
                            anomaliesInFinalLines = anomaliesInFinalLines.Replace("]}", "");
                            if (anomaliesInFinalLines.Length > 0)
                            {
                                linesWithErrors.Add(i + 1 + "+:unexpCharsA_" + anomaliesInFinalLines);
                                return false;
                            }
                        }

                    }
                    else
                    {
                        string finalLine = allFinalLines;
                        if (!finalLine.Contains("}") || !finalLine.Contains("]"))
                        {
                            linesWithErrors.Add(i + 1 + "+:clmClose");
                            return false;
                        }
                        else
                        {
                            string anomaliesInFinalLine = finalLine.Replace("]}", "");
                            if (anomaliesInFinalLine.Length > 0)
                            {
                                linesWithErrors.Add(i + 1 + "+:unexpCharsB_");
                                return false;
                            }

                        }
                    }
                }
                else
                {
                    //we got out but cant find a ]??
                    linesWithErrors.Add(i + 1 + "+:forgotClosers");
                    return false;
                }
            }
            else
            {
                //we hit the end without finding a ]
                linesWithErrors.Add(i + 1 + "+:noClmClose");
                return false;
            }

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
        private bool verifyFileExists(string filePath)
        {
            string correctedPath = filePath;
            if (correctedPath.Contains("\\\\"))
            {
                correctedPath = correctedPath.Replace("\\\\", "\\");
            }
            correctedPath = shaveSurroundingQuotesAndSpaces(correctedPath);
            //all that was just in case, but pretty sure we didn't need it

            if (File.Exists(correctedPath))
            {
                return true;
            }
            return false;
        }
        private bool getLineErrors_bool(string line, string lineAfter, int indexOfLabelWeWant)
        {

            //, bool quotesOnValue, string format = ""


            List<string> errorsOnLine = new List<string>();
            string lineNS = NormalizeWhiteSpace(line);
            string endingWeWant = expectedEndings[indexOfLabelWeWant];
            string labelWeWantNoQuotes = expectedFields[indexOfLabelWeWant].Replace("\"", ""); //we're not doing this because it can fuck us with Event and LowHealthBeatEvent
            string finalCharOnLine = lineNS.Substring(lineNS.Length - 1, 1); //we only care about the last item on the line

            string nextLineNS = NormalizeWhiteSpace(lineAfter);





            if (line.Contains(expectedFields[indexOfLabelWeWant]))
            {
                //line contains label without issues
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



                }

                //errorsOnLine.Add("labelmissing");
                string whatsGoingOn = verifySequenceForMissingEntry(lineNS, nextLineNS, indexOfLabelWeWant);
                if (whatsGoingOn == "grbgLine")
                {
                    return false;
                }
                else if (whatsGoingOn == "dupe")
                {
                    int prevIndx = indexOfLabelWeWant - 1; if (prevIndx < 0) prevIndx = expectedFields.Length - 1;
                    return false;
                }
                else if (whatsGoingOn == "labelInvalid")
                {
                    return false;
                }
                else if (whatsGoingOn == "forgotitem")
                {
                    //when running through the function to translate these error codes, we need to add a line here
                    //(and add numbers to our other error Line nums)
                    return false;
                }
                else if (whatsGoingOn == "fatality")
                {
                    //   x_x

                    return false;
                }


                // }
            }


        EndLabelCheck:

            if (endingWeWant.Length == 1)
            {
                if (finalCharOnLine != endingWeWant)
                {
                    errorsOnLine.Add("unexpEnd-Wanted_\"" + endingWeWant + "\"");
                    return false;
                }
            }
            else
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
                    if (nextLineNS.Contains("bankPath"))
                    {
                        //we're looking at BPM, and next line has Bankpath
                        if (finalCharOnLine != ",")
                        {
                            errorsOnLine.Add("missingcomma");
                            return false;
                        }
                    }
                    else
                    {
                        //the next line doesn't have a bankPath

                        if (Int32.TryParse(finalCharOnLine, out int yo))
                        {
                            //the last character on the line is a number

                        }
                        else
                        {
                            //the last character on the line is not a number
                            errorsOnLine.Add("unexpEndN");
                            return false;
                        }
                    }
                }
                else if (indexOfLabelWeWant == 8)
                {
                    //we're looking at BankPath

                    //ending should always be quotes

                }
                else if (indexOfLabelWeWant == 9)
                {
                    //we're looking at MainMusic close or BossMusic close
                    if (nextLineNS.Contains("BossMusic") || nextLineNS.Contains("MainMusic"))
                    {
                        //we're on the MainMusic close, opening for BossMusic next
                        //or the user copied and pasted and forgot to change MainMusic to BossMusic, which we'll handle first
                        /* Turns out, Main Music doesn't have to be first within the two! */
                        /*
                        if (nextLineNS.Contains("MainMusic"))
                        {
                            errorsOnLine.Add("NL_bossnotmain"); //next line, we want boss not main music
                            return false;
                        }*/

                        if (finalCharOnLine != ",")
                        {
                            errorsOnLine.Add("missingcomma");
                            return false;
                        }
                    }
                    else
                    {
                        //we don't have BossMusic on the next line, we're just closing out Music
                        string unexpCharacters = lineNS.Replace("}", "");
                        if (lineNS != "}")
                        {
                            errorsOnLine.Add("unexpCharsC_" + unexpCharacters);
                            return false;
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
                            return false;
                        }
                    }
                    else
                    {
                        //we don't have another level on the next line
                        //just make sure we didn't have anything else in this line
                        string unexpCharacters = lineNS.Replace("}", "");
                        if (lineNS != "}")
                        {
                            errorsOnLine.Add("unexpCharsD_" + unexpCharacters);
                            return false;
                        }
                    }


                }
            }
            // ↑Endings, ↑↑ LabelCheck, ↓ Format Errors

            bool LineFormatIsClean = getFormatErrors_bool(line, indexOfLabelWeWant); //get the format errors
            if (!LineFormatIsClean) return false;

            return true;


        }

        int[] instancesOfLevelInJson = new int[9]; //we'll use this to detect if we have more than one instance of a level in the JSON; 9 because of tutorial level

        private bool getFormatErrors_bool(string line, int indexOfLabelWeWant)
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
                string unexpCharacters = lineNS.Replace("{", "");
                if (unexpCharacters.Length > 0)
                {
                    formatErrors.Add("unexpCharsA_" + unexpCharacters);
                    return false;
                }
            }
            else if (indexOfLabelWeWant == 9 || indexOfLabelWeWant == 10)
            {
                //either a level-closing or music-closing }
                string unexpCharacters = lineNS;
                if (unexpCharacters.Substring(unexpCharacters.Length - 1, 1) == ",")
                {
                    unexpCharacters = unexpCharacters.Substring(0, unexpCharacters.Length - 1);
                }
                unexpCharacters = unexpCharacters.Replace("}", "").Replace("\r", "").Replace("\n", "");//why is this not working!?!!!
                                                                                                       //removing control characters. I'm about to throw something through my wall

                // Get the integral value of the character.

                if (!string.IsNullOrEmpty(unexpCharacters))
                {

                    formatErrors.Add("unexpCharsB_" + unexpCharacters);
                    return false;
                }

            }
            else
            {
                if (lineColonSplit.Length == 1)
                {
                    formatErrors.Add("nocolon");
                    return false;
                }
                else
                {
                    
                    bool lineWithLabelIsClean = checkFormatLineWithLabel_bool(lineColonSplit, indexOfLabelWeWant);
                    if (!lineWithLabelIsClean) return false;
                }
            }

            return true;
        }

        private bool checkFormatLineWithLabel_bool(string[] splitInfo, int indexOfLabel)
        {
            string labelStr = splitInfo[0];
            string valueStr = splitInfo[1];
            List<string> lineFormatErrors = new List<string>();


            if (splitInfo.Length == 3)
            {
                //we want 3 in splitInfo if we're on indexOfLabel 7, meaning we're looking for a bankpath
                if (indexOfLabel == 8)
                {
                    valueStr = splitInfo[1] + ":" + splitInfo[2];
                }
                else
                {
                    lineFormatErrors.Add("2manycol"); //too many colons in value
                    return false;
                }
            }
            else
            {
                //this code won't run if splitInfo length was 1; splitInfo.length must be 2
                if (indexOfLabel == 8)
                {
                    lineFormatErrors.Add("bpnocol"); //bank path no colon
                    return false;
                }
            }


            valueStr.TrimEnd();//get rid of all whitespace on the right of value/value's comma
            if (valueStr.Substring(valueStr.Length - 1, 1) == ",")
            {
                valueStr = valueStr.Substring(0, valueStr.Length - 1); //if we had a comma, it's gone now—we already checked for endings
            }
            if (valueStr.Contains(","))
            {
                lineFormatErrors.Add("tmcom"); //we just got rid of the only comma that should be there
                return false;
            }
            string labelNS = NormalizeWhiteSpace(labelStr, true);
            string valueNS = NormalizeWhiteSpace(valueStr);

            int numberOfQuotesInValue = valueNS.Split(new string[] { "\"" }, StringSplitOptions.None).Length - 1;
            int numberOfQuotesInLabel = labelNS.Split(new string[] { "\"" }, StringSplitOptions.None).Length - 1;


            //Label check; we already checked for if it has the exact label name with quotes around it

            if (indexOfLabel == 2)
            {
                string checkMainMusic = labelNS.Replace("\"MainMusic\"", "");
                string checkBossMusic = labelNS.Replace("\"BossMusic\"", ""); //doing both of these in case our user somehow put MainMusic BossMusic: {
                string unexpCharsInLabel = labelNS.Replace("\"MainMusic\"", "").Replace("\"BossMusic\"", ""); //but this will look weird if they did
                if (checkMainMusic.Length > 0 && checkBossMusic.Length > 0)
                {
                    lineFormatErrors.Add("unexpChLC_" + unexpCharsInLabel);
                    return false;
                }
            }
            else
            {
                string unexpCharsInLabel = labelNS.Replace(expectedFields[indexOfLabel], "");
                if (unexpCharsInLabel.Length > 0)
                {
                    lineFormatErrors.Add("unexpChLC_" + unexpCharsInLabel);
                    return false;
                }
            }


            if (indexOfLabel == 6 || indexOfLabel == 7 || indexOfLabel == 2) goto ValueNoQuoteCheck;

            ValueWithQuoteCheck:

            if (numberOfQuotesInValue > 2)
            {
                lineFormatErrors.Add("2mqVal");
                return false;
            }
            else if (numberOfQuotesInValue < 2)
            {
                lineFormatErrors.Add("neqVal");
                return false;
            }
            else
            {
                //we have only two quotes in the value, which is what we want
                //now we verify anything we need to regarding the values
                if (indexOfLabel == 1)
                {
                    //we're checking for a LevelName
                    for (int j = 0; j < allLevelNames.Length; j++)
                    {
                        string valueNSShaven = shaveSurroundingQuotesAndSpaces(valueNS); //should we put 'false' for spaces..?
                        string capitalizeLevelName = allLevelNames[j].Substring(0, 1).ToUpper() + allLevelNames[j].Substring(1);

                        if (valueNSShaven == capitalizeLevelName)
                        {
                            //we found the level, formatted correctly, we're fine
                            instancesOfLevelInJson[j] = instancesOfLevelInJson[j] + 1;

                            break; //break out of J's for loop, not I
                        }
                        else if (valueNSShaven.ToLower() == allLevelNames[j])
                        {
                            //we found the level, but it's not formatted correctly
                            instancesOfLevelInJson[j] = instancesOfLevelInJson[j] + 1;
                            lineFormatErrors.Add("LCap");
                            return false;

                            break; //break out of J's for loop, not I
                        }
                        if (j == allLevelNames.Length - 1)
                        {
                            //we're at the end, and we didn't find anything
                            lineFormatErrors.Add("LUr(" + valueNS + ")"); //level unrecognized
                            return false;
                        }
                    }
                }
                else if (indexOfLabel == 4 || indexOfLabel == 5)
                {
                    //we're checking an event
                    string checkEvent = shaveSurroundingQuotesAndSpaces(valueNS); //getting rid of the spaces, then quotes
                    if (checkEvent.Length != valueNS.Length - 2)
                    {
                        //the two quotes we found weren't surrounding
                        lineFormatErrors.Add("evF1");
                        return false;

                    }
                    checkEvent = checkEvent.TrimStart('{').TrimEnd('}');

                    if (checkEvent.Length != valueNS.Length - 4 || checkEvent.Length != 36)
                    {
                        //Event string does NOT have { and }, OR it does not have the full 36-digit ID
                        lineFormatErrors.Add("evF2");
                        return false;
                    }
                }
                else if (indexOfLabel == 8)
                {
                    //we're looking at a bankPath
                    bool BankPathLineIsClean = getBankPathFormatErrors_bool(valueNS);
                    if (!BankPathLineIsClean) return false;
                }

            }

            return true;

        // We've stopped if we were expecting quotes

        ValueNoQuoteCheck:
            if (numberOfQuotesInValue > 0)
            {
                lineFormatErrors.Add("unwntdq");
                return false;
            }
            else
            {
                //we don't have any quotes, which is what we want
                if (indexOfLabel == 2)
                {
                    if (valueNS.Trim() != "{")
                    {
                        lineFormatErrors.Add("unexpChRC_" + labelNS.Replace("{", ""));
                        return false;
                    }
                }
                else
                {
                    //if we're here, then we're looking at BeatInputOffset or BPM, both are number-only variables

                    if (IsValueANumber(valueNS.Trim()) == false)
                    {
                        lineFormatErrors.Add("numFormat");
                        return false;
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
                        return false;
                    }*/
                }
            }
            return true;
        }

        private void verifyBankIfNoBankPath()
        {
            //if we see that our entry does not have a bankPath entry, we want to go back and check our "Bank"

        }

        private bool getBankPathFormatErrors_bool(string bankPathValue)
        {
            string bpVal = bankPathValue;
            List<string> bpFormErrors = new List<string>();
            if (!bpVal.Contains(".bank"))
            {
                bpFormErrors.Add("nobpfile"); //bankpath points to a directory, but needs to include "[Filename].bank"
                return false;
            }

            if (bpVal.Split('/').Length - 1 != 0)
            {
                //we have / in our bankpath, when we only want \'s

                bpFormErrors.Add("bpws"); //bank path wrong slash
                                          //we'll also replace them so we can continue with the code if that's the only issue
                bpVal.Replace('/', '\\');
                return false;
            }

            string[] directoriesBtwnDblSlashes = bpVal.Split(new string[] { "\\\\" }, StringSplitOptions.None);
            string nextLine_chkIntegrity = bpVal.Replace("\\\\", "\\");
            string[] directoriesBtwnSnglSlashes = nextLine_chkIntegrity.Split(new string[] { "\\" }, StringSplitOptions.None);

            //we're going to take value for bankPath and analyze the formating, and verify a file exists
            int indexOfBankPathInfo = nextLine_chkIntegrity.IndexOf(":\\");
            if (indexOfBankPathInfo == 0) { bpFormErrors.Add("nobpdir"); return false; } //if our line looks like bankPath: ":\\Dir\\Dir2\\ and they forgot the drive letter
            indexOfBankPathInfo -= 1; //this should be the index of C:\, B:\ X:\ Etc
            if (indexOfBankPathInfo < 0)
            {
                bpFormErrors.Add("cvBP"); //can't verify bankpath value's format
                return false;
            }
            string bankPathInfo = nextLine_chkIntegrity.Substring(indexOfBankPathInfo);
            bankPathInfo = shaveSurroundingQuotesAndSpaces(bankPathInfo);


            if (bpVal.Contains("\\\\\\") || bpVal.Contains("\\\\\\\\"))
            {
                bpFormErrors.Add("2mSl"); //too many slashes
                return false;
            }
            else if (directoriesBtwnDblSlashes.Length != directoriesBtwnSnglSlashes.Length)
            {
                //if the # of our directories with double slashes does not match our # of directories after converting to single slashes, it means we don't have enough slashes somewhere er something
                bpFormErrors.Add("bPF"); //bankpath formatting
                return false;
            }
            else if (!verifyFileExists(bankPathInfo))
            {
                //bpFormErrors.Add("bpFNF"); //bankpath file not found; this should be a potentially-major error (potentially major = problem will arise later, but program will still work)
                //return false;
                //We're not longer checking for this
            }

            return true;
        }

        int[] corrPlaceOfIdableFields = { 1, 2, 2, 3, 4, 5, 6, 7, 8 }; //correlating Placements Of Identifiable Fields
        string[] identifiableFields = { "\"LevelName\"", "\"MainMusic\"", "\"BossMusic\"", "\"Bank\"", "\"Event\"", "\"LowHealthBeatEvent\"", "\"BeatInputOffset\"", "\"BPM\"", "\"bankPath\"" };
        private int verifyWTFsGoingOnFirstLine(string[] firstTwoLines)
        {
            //this function returns what placement in the sequence we're on
            //first10Lines[2] was supposed to have {, then LevelName in it


            for (int i = 0; i < identifiableFields.Length; i++)
            {
                if (firstTwoLines[0].Contains(identifiableFields[i]))
                {

                    return corrPlaceOfIdableFields[i];
                }
            }

            //if we got this far, we couldn't find anything

            for (int i = 0; i < identifiableFields.Length; i++)
            {
                if (firstTwoLines[1].Contains(identifiableFields[i]))
                {

                    return corrPlaceOfIdableFields[i] - 1;
                }
            }

            //if we got this far, then somehow the first and 2nd line after the customLevelMusic openings don't have a LevelName or Bank

            return -1;

        }

        private string verifySequenceForMissingEntry(string curLineStr, string nextLineStr, int seqPlaceMissing)
        {
            //this function is called if we had a missing label in our line, AND the line was not empty
            //we need to check what's going on with the sequence of our expected fields



            int nextSequenceWeWant = seqPlaceMissing + 1;
            if (nextSequenceWeWant >= expectedFields.Length)
            {
                nextSequenceWeWant = 0;
            }

            int prevSequenceWeWant = seqPlaceMissing - 1;
            if (prevSequenceWeWant < 0)
            {
                prevSequenceWeWant = expectedFields.Length - 1;
            }

            string expFld = expectedFields[seqPlaceMissing];
            string prevExpFld = expectedFields[prevSequenceWeWant];
            string nextExpFld = expectedFields[nextSequenceWeWant];

            if (seqPlaceMissing == 10)
            {
                //our level-closing } is missing
                //we can't go back to prevExpFld to look for a duplicate
                if (nextLineStr.Contains(expFld))
                {
                    //our next line has the } we wanted, this line is grbg to us
                    return "grbgLine";
                }
                else if (curLineStr.Contains(nextExpFld) || nextLineStr.Contains("]"))
                {
                    return "forgotitem";
                }
                else if (nextLineStr.Contains(nextExpFld))
                {
                    return "labelInvalid";
                }
                else
                {
                    return "levelclosemissing";
                }
            }

            //if the previous sequence we wanted was 8, then it was optional
            if (seqPlaceMissing == 9)
            {
                //we can't go forward to check for a {
                if (curLineStr.Contains(expectedFields[7]) || curLineStr.Contains(expectedFields[8]))
                {
                    return "dupe";
                }
                else if (nextLineStr.Contains("\"MainMusic\"") || nextLineStr.Contains("\"BossMusic\""))
                {
                    return "forgotitem";
                }
                else
                {
                    return "musicclosemissing";
                }
            }
            if (seqPlaceMissing == 7)
            {
                //if we're at 7, the next could be bankPath, or music-closing }
            }


            /*
            #region TwoPossibilities
            if (prevExpFld.Contains("|"))
            {
                string[] possibles = prevExpFld.Split('|');
                if(curLineStr.Contains(possibles[0]) || curLineStr.Contains(possibles[1]))
                {
                    return "dupe";
                }
            }
            if (nextExpFld.Contains("|"))
            {
                string[] possibles = nextExpFld.Split('|');
                if (curLineStr.Contains(possibles[0]) || curLineStr.Contains(possibles[1]))
                {
                    return "grbgLine";
                }
            }
            if (nextLineStr.Contains(expectedFields[nextSequenceWeWant]))
            {
                //still can't find the label we want, but the next line contains the NEXT label we wanted, so we want to skip this field
                //i wish we could add a line here that the user could then add something
                //we want to continue, adding to Placement and adding to i
                return "labelinvalid";
            }
            if (expFld.Contains("|"))
            {
                string[] possibles = expFld.Split('|');
                if (curLineStr.Contains(possibles[0]) || curLineStr.Contains(possibles[1]))
                {
                    return "forgotitem";
                }
            }
            #endregion TwoPossibilities*/

            //(curLineStr.Contains("\"MainMusic\"") || curLineStr.Contains("\"BossMusic\""))

            if (curLineStr.Contains(expectedFields[prevSequenceWeWant]))
            {
                //problem: this gets called if we had the music closing }, followed by the level closing }
                //maybe check one more line for { or ] for confirmation

                //does our current line have the previous expectedField in the sequence? (IE: we're looking for Event, is Bank on the line?)
                //if it does, this is an accidental duplicate label of the previous line
                //we want to continue, keeping placement and adding to i

                if (seqPlaceMissing == 10)
                {
                    return ""; //i don't think there's an issue then...?
                }
                else
                {
                    return "dupe";
                }

            }
            else if (nextLineStr.Contains(expectedFields[seqPlaceMissing]))
            {
                //next line contains the missing label for our current seqence, this line is grbg to us
                //we want to continue with the same Placement, but adding to i
                return "grbgLine";
            }
            else if (curLineStr.Contains(expectedFields[nextSequenceWeWant]))
            {
                //still can't find the label we want, but now realizing: THIS line contains the label for the next field in the sequence
                //i wish we could add a line here that the user could then add something
                //we want to continue, adding to Placement and adding to i
                return "forgotitem";
            }
            else if (nextLineStr.Contains(expectedFields[nextSequenceWeWant]))
            {
                //still can't find the label we want, but the next line contains the NEXT label we wanted, so we want to skip this field
                //we later add a line here so the user could add something
                //we want to continue, adding to Placement and adding to i
                return "labelInvalid";
            }
            else
            {

                //I REALLY hope I did these right
                if (prevExpFld.Contains("|") && (
                    curLineStr.Contains("\"MainMusic\"") || curLineStr.Contains("\"BossMusic\"")))
                {
                    return "dupe";
                }
                else if (expFld.Contains("|") && (
                    nextLineStr.Contains("\"MainMusic\"") || nextLineStr.Contains("\"BossMusic\"")))
                {
                    return "grbgLine";
                }
                else if (nextExpFld.Contains("|") && (
                    curLineStr.Contains("\"MainMusic\"") || curLineStr.Contains("\"BossMusic\"")))
                {
                    return "forgotitem";
                }
                else if (nextExpFld.Contains("|") && (
                    nextLineStr.Contains("\"MainMusic\"") || nextLineStr.Contains("\"BossMusic\"")))
                {
                    return "labelInvalid";
                }



                //if we got this far, we're completely lost regarding what's going on in the JSON
                //fatal error: debuggie's off course
                //MessageBox.Show("CurLine: " + curLineStr + "\n NextLine: " + nextLineStr + "\n seqPlaceMissing: " + seqPlaceMissing);
                return "fatality";
            }




        }

        #endregion BuggyD_StopAtFirst

        string[] allLevelNames = { "voke", "stygia", "yhelm", "incaustis", "gehenna", "nihil", "acheron", "sheol", "tutorial" };
        string[] levelNames = { "voke", "stygia", "yhelm", "incaustis", "gehenna", "nihil", "acheron", "sheol" };
        string[] expectedEndings = { "{", ",", "{", ",", ",", ",", ",", ",or ", "bnkp", "}or,", "}or," };
        //                            L   LN   MM   Ba   Ev   LH   Ofs   BPM*  BnkPth   MMcBMc  Lc
        //                                      ^-----<----------<------------<--------' if(,)

        //                          0          1                       2                    3            4                   5                        6              7            8          9    10
        string[] expectedFields = { "{", "\"LevelName\"", "\"MainMusic\"|\"BossMusic\"", "\"Bank\"", "\"Event\"", "\"LowHealthBeatEvent\"", "\"BeatInputOffset\"", "\"BPM\"", "\"bankPath\"", "}", "}" };



        private void StartupScanForm_Load(object sender, EventArgs e)
        {
            //StartUpVerify();
            explainLabel.Text = "Starting Metal Manager...";
        }

        private void StartupScanShown(object sender, EventArgs e)
        {
            //StartUpVerify();
            
            if (!BfGStartupWorker.IsBusy)
            {
                BfGStartupWorker.RunWorkerAsync();
            }

        }

        int currentProcess = -1;
        private void BfGStartupWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            for(int i=0; i < progressReports.Length; i++)
            {
                currentProcess = i;
                if (i == 0)
                {
                    GetDirectoriesFromMainForm();
                } else if(i == 1)
                {
                    GetCustomSongsInConfig();//<------------------this used to be there--v
                } else if(i == 2) 
                {
                    GetCurrentCustomSongsInModFolder();//<--and this used to be up there-^

                }
                else if (i == 3)
                {
                    CompareSongsBtwnConfigAndModDir();
                }  else if(i == 4)
                {
                    GetAllSongsWithErrors();
                }

                BfGStartupWorker.ReportProgress(i);
                if (BfGStartupWorker.CancellationPending)
                {
                    e.Cancel = true;
                    BfGStartupWorker.ReportProgress(-1);

                }
            }
        }

        string[] progressReports =
        {
            "Checking for new, edited, or deleted Custom Songs...",
            "Getting songs from Mod folder...",
            "Reading Metal Manager configuration...",
            "Comparing old configuration...",
            "Scanning new songs for errors..."
        };
        private void BfGStartupWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int sequence = e.ProgressPercentage;
            if (sequence == -1)
            {
                explainLabel.Text = "Cancelled...";
            }
            else
            {
                explainLabel.Text = progressReports[sequence];
            }

        }

        private void BfGStartupWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                CloseManagerFromError("user-cancelled");
            }
            else if(e.Error != null)
            {
                string errMsg = e.Error.Message;
                if(errMsg.Length > 1000)
                {
                    errMsg = errMsg.Substring(0, 1000) + "...";
                }
                MessageBox.Show("Metal Manager encountered a fatal error and cannot continue:\n" + e.Error.Message);
                CloseManagerFromError("totalCalamity-Z"+ currentProcess+"\n"+ errTrack);
            } else
            {
                //we successfully got through all work!
                explainLabel.Text = "Starting Metal Manager...";
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void cancelBfGWorker()
        {
            if (!BfGStartupWorker.IsBusy) return;

            BfGStartupWorker.CancelAsync();
        }

        private void WatchForCancel(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                cancelBfGWorker();
            }
        }

        /// <summary>
        /// Parsing Numbers does not work for all users, so instead, this strips down the value of a number to make sure nothing 
        /// exists after verifying one dot, one dash, and removing all numbers. Should be an exact copy from Form1
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private bool IsValueANumber(string val)
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
            }
            else
            {
                return false;
            }
        }

    }
}
