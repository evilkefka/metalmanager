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

namespace MetalManager
{
    public partial class CleanUpSAForm : Form
    {
        /// <summary>
        /// banksInCSjson only want the filename, not fullpath, plus |, followed by "true" or "false" strings
        /// </summary>
        string[] banksInCSjson = new string[0];

        public CleanUpSAForm(string[] omitFromPurge)
        {
            InitializeComponent();
            if(omitFromPurge == null || omitFromPurge.Length == 0)
            {
                banksInCSjson = null;
            } else
            {
                banksInCSjson = omitFromPurge;
            }
            
        }

        public Form1 MyParentForm;

        DirectoryInfo mDir;
        DirectoryInfo gDir;
        private void getModAndGameDir()
        {
            mDir = MyParentForm.di;
            gDir = MyParentForm.gameDir;
        }

        

        private void writeAllAnomaliesToList()
        {
            string[] allCustomBanksInSA = GetAnomaliesInSA();

            foreach (string anomaly in allCustomBanksInSA)
            {
                ListViewItem anomalyListing = new ListViewItem();
                anomalyListing.Text = anomaly.Replace(gDir.ToString() + "\\", "");
                anomalyListing.SubItems.Add("...");
                anomalyListing.Checked = true;

                SAAnomalyList.Items.Add(anomalyListing);
                
            }
            SAAnomalyList.Enabled = true;
        }

        
        
        private void uncheckAllEssentials()
        {

            string[] currentlyUsingBanks = MyParentForm.getAllBanksUsedByCSJson();
            if (currentlyUsingBanks == null || currentlyUsingBanks.Length == 0) return;

            
            foreach(ListViewItem anomalyListing in SAAnomalyList.Items)
            {
                if (currentlyUsingBanks.Contains(anomalyListing.Text))
                {
                    anomalyListing.Checked = false;
                    anomalyListing.SubItems[1].Text = "In use by customsongs.json";
                    continue;
                }

                long banksFileSz = new FileInfo(gDir.ToString() + "\\" + anomalyListing.Text).Length;
                if (banksFileSz == MyParentForm.gameMBFileSize)
                {
                    //we just found the game's default Music.bank, most likely renamed as a backup by the user. Don't delete it.
                    anomalyListing.Checked = false;
                    anomalyListing.SubItems[1].Text = "Backup of game's original Music.bank";
                    continue;
                }

                anomalyListing.SubItems[1].Text = "Currently unused";
            }
        }


        private string[] GetAnomaliesInSA(bool report = false, bool deleet = false)
        {
            //this does not look for a custom Music.bank. Music.bank can be the game's, the LowHealth Library, or an unrecognized custom one
            //the following .banks are default .banks, at least for non-DLC 

            var banks = Directory.EnumerateFiles(gDir.ToString(), "*.bank", SearchOption.TopDirectoryOnly)
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

            return banks.ToArray();
        }

        /*
        private void deleeeeet()
        {
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

                            }
                            else
                            {
                                //cantBeDeleted.Add(bank.Replace(gameDir + "\\", ""));
                                undeletableMsg += bank.Replace(gameDir + "\\", "") + "\n";
                            }
                        }
                    }
                    else
                    {
                        //we want to ensure we're not deleting our original
                        long banksFileSz = new FileInfo(bank).Length;
                        if (banksFileSz == gameMBFileSize)
                        {
                            //we just found the game's default Music.bank, most likely renamed as a backup by the user. Don't delete it.
                            //foundGameDefaultBank = true; //We could just keep all of them, or comment/uncomment this to make it go once...
                            continue;
                        }
                        else
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
        }*/

        private ListViewColumnSorter lvwColumnSorter;

        private void CleanUpSAForm_Load(object sender, EventArgs e)
        {
            getModAndGameDir();
            writeAllAnomaliesToList();
            uncheckAllEssentials();

            lvwColumnSorter = new ListViewColumnSorter();
            SAAnomalyList.ListViewItemSorter = lvwColumnSorter;
        }

        private void SAAnomList_ColClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.SAAnomalyList.Sort();
        }

        private void chkUnchkAllClick(object sender, EventArgs e)
        {
            int changesMade = 0;
            foreach(ListViewItem item in SAAnomalyList.Items)
            {
                if (!item.Checked)
                {
                    item.Checked = true;
                    changesMade++;
                }
                    
            }

            if(changesMade == 0)
            {
                foreach (ListViewItem item in SAAnomalyList.Items)
                {
                    item.Checked = false;
                }
            }
        }

        private void SACleanupDeleteClick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in SAAnomalyList.Items)
            {
                if (!item.Checked) continue;

                string pathToDelete = gDir.ToString() + "\\" + item.Text; 

                try
                {
                    File.Delete(pathToDelete);
                    item.Text = "---";
                }
                catch
                {
                    //doing this because some people set their mods to be READ ONLY.. WHY!?!?!
                    FileInfo fi = new FileInfo(@pathToDelete);
                    if (fi.IsReadOnly)
                    {
                        File.SetAttributes(pathToDelete, ~FileAttributes.ReadOnly);
                        try
                        {
                            File.Delete(pathToDelete);
                            item.Text = "---";
                        }
                        catch
                        {
                            item.BackColor = Color.FromArgb(255, 255, 225, 225);
                            item.SubItems[1].Text = "Could not be deleted.";
                        }
                    }
                    else
                    {
                        
                        item.BackColor = Color.FromArgb(255, 255, 225, 225);
                        item.SubItems[1].Text = "Could not be deleted.";
                    }
                }
            }

            for(int i = SAAnomalyList.Items.Count-1; i >= 0; i--)
            {
                if(SAAnomalyList.Items[i].Text == "---")
                {
                    SAAnomalyList.Items.RemoveAt(i);
                }
            }



        }
    }
}
