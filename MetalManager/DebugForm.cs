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
//using System.Configuration;
using System.Text.RegularExpressions; //Regex is regular expressions!??!!! WHAT a country!!

namespace MetalManager
{

    public partial class DebugForm : Form
    {

        string summoner = "";

        public DebugForm(string whatSummonsMe)
        {
            InitializeComponent();
            summoner = whatSummonsMe;
        }

        public Form1 MyParentForm;

        public string DebuggedSongPath
        {
            get { return debugSongPath.ToString(); }
        }
        public string CleanedJson
        {
            get { return cleanedUpJson.ToString(); }
        }

        string cleanedUpJson = null;
        private void SaveAndCloseDebugger(object sender, EventArgs e)
        {
            cleanedUpJson = TurnEditingLinesIntoString();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void closeDebugger(object sender, EventArgs e)
        {
            this.Close();
        }

        /*
        public string TheValue
        {

            get { return debugTextbox.Text; }
        }*/

        private void fillJsonDataGrid(string fullJson)
        {
            string[] fullJsonLines = fullJson.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in fullJsonLines)
            {
                string lineBURNEVERYTHING = line.Replace("\r", ""); //GOD I HATE THESE THINGS
                JsonLinesBind.Add(new JsonLineList(lineBURNEVERYTHING));
                //JsonLinesBind[JsonLinesBind.Count - 1][0] = 1;
            }

            dgJsonEditor.DataSource = JsonLinesBind;
            dgJsonEditor.Columns[0].Width = 500;
            //dgJsonEditor.Columns[1].Width = 400;
        }
        BindingList<JsonLineList> JsonLinesBind = new BindingList<JsonLineList>();

        /*
        private void fillJsonLines(string fullJson)
        {
            //fills the editor on the right of the error list
            string[] jsonLines = fullJson.Split('\n');
            int lineNum = 1;//we don't want to start at line 0
            foreach(string line in jsonLines)
            {
                string[] row = { lineNum.ToString(), line };
                ListViewItem lineWithLineNum = new ListViewItem(row);
                lineWithLineNum.SubItems.Add("Color");
                lineWithLineNum.SubItems[0].ForeColor = Color.Gray;
                lineWithLineNum.UseItemStyleForSubItems = false; //we'll have to set each individusal SubItem's back color now
                debuggyJsonEditor.Items.Add(lineWithLineNum);
                lineNum++;
                //debuggyJsonEditor.Items.Add(line); //this was before we put in the LineNum column
            }
        }*/

        private string[] getErrorsAlreadyInList()
        {

            List<string> ErrorsInList = new List<string>();
            for(int i=0; i < jsonAnomalyList.Items.Count; i++)
            {
                ErrorsInList.Add(jsonAnomalyList.Items[i].SubItems[0].Text.ToString() + ":" + jsonAnomalyList.Items[i].SubItems[1].Text.ToString());
            }
            return ErrorsInList.ToArray();
        }

        Color JsonEditorColor1 = Color.White;
        Color JsonEditorColor2 = Color.FromArgb(255, 240, 240, 240);//can't i just do alpha?

        Color JsonEditorErrC1 = Color.FromArgb(255, 255, 50, 50);
        Color JsonEditorErrC2 = Color.FromArgb(255, 235, 0, 0);

        Color JsonEditorNrmSlctd = SystemColors.Highlight;
        Color JsonEditorErrSlctd = Color.FromArgb(255, 70, 0, 0);

        private void setDGEditorLinesBGColrs(string[] errorList, bool addNewLinesIfForgot = true, string[] originalErrorList = null)
        {
            //there's a way to do this where I don't need two foreach loops
            //.... somewhere out there....
            int lineCounter = 1;
            foreach (DataGridViewRow row in dgJsonEditor.Rows)
            {
                if (lineCounter % 2 == 1)
                {
                    row.DefaultCellStyle.BackColor = JsonEditorColor1;
                    row.HeaderCell.Style.BackColor = JsonEditorColor1;
                } else
                {
                    row.DefaultCellStyle.BackColor = JsonEditorColor2;
                    row.HeaderCell.Style.BackColor = JsonEditorColor2;
                }
                lineCounter++;

            }

            string[] errList = errorList;
            if(originalErrorList != null)
            {
                List<string> fullErrorList = new List<string>();
                fullErrorList.AddRange(originalErrorList);
                fullErrorList.AddRange(errorList);
                errList = fullErrorList.ToArray();
            }


            int addedLines = 0;
            List<int> entries2Delete = new List<int>();
            foreach (string error in errList)
            {
                string[] errorInfo = error.Split(':');
                if (errorInfo[0].Contains("+")){
                    SetAllBGsAtAndAbove(errorInfo[0]);
                }
                if (errorInfo[0].Contains("–"))
                {
                    GetRangeAndSetBGs(errorInfo[0]);
                }
                else
                if (error.Contains("BackOnTrack"))
                {
                    if (Int32.TryParse(errorInfo[0], out int yo))
                    {
                        int[] unwantedEntries = setProperFatalityRange(Int32.Parse(errorInfo[0]), errList);//this also sets our numbers in jsonAnomalyList, and sets bg's
                        //entries2Delete.AddRange(unwantedEntries);
                    }
                } else
                if (error.Contains("forgot") && addNewLinesIfForgot)
                {
                    if (Int32.TryParse(errorInfo[0], out int yo))
                    {
                        int lineInt = Int32.Parse(errorInfo[0]) - 1;
                        lineInt += addedLines;

                        JsonLinesBind.Insert(lineInt, new JsonLineList(""));
                        setJsonEditorLineBG(lineInt + 1, true); //we want 1-based linenum
                        addedLines++;
                    }
                } else
                if (errorInfo.Length == 2)
                {
                    string lineNumStr = errorInfo[0];
                    if (Int32.TryParse(lineNumStr, out int yo))
                    {
                        //the last character on the line is a number
                        int lineNum = Int32.Parse(lineNumStr);
                        lineNum += addedLines;
                        lineNum -= 1; //why do I need to do this
                        //dgJsonEditor.Rows[lineNum].DefaultCellStyle.BackColor = JsonEditorErrC1;
                        setJsonEditorLineBG(lineNum + 1, true); //we want 1-based linenum
                        //dgJsonEditor.Rows[lineNum].DefaultCellStyle.Font = new Font(dgJsonEditor.Rows[lineNum].DefaultCellStyle.Font, FontStyle.Bold); Well this doesn't work
                        dgJsonEditor.Rows[lineNum].DefaultCellStyle.ForeColor = Color.Black;
                    }
                    //errorCode = errorInfo[1];
                }
            }

            if(entries2Delete.Count > 0)
            {
                foreach(int entryIndex in entries2Delete)
                {
                    if (entryIndex < 0) continue;
                    jsonAnomalyList.Items[entryIndex].Remove();
                }
            }
        }

        private void SetAllBGsAtAndAbove(string oneBsdStartLine)
        {
            if (!oneBsdStartLine.Contains("+")){
                //donno how we got here
                return;
            }
            string startingNumStr = oneBsdStartLine.Replace("+", "");
            int startInt = -1;
            if (Int32.TryParse(startingNumStr, out int myeyeshurt))
            {
                startInt = Int32.Parse(startingNumStr);
            } else
            {
                return;
            }

            for (int i = startInt; i <= JsonLinesBind.Count; i++)
            {
                setJsonEditorLineBG(i, true);
            }
            
            
        }

        private void GetRangeAndSetBGs(string rangeString)
        {
            string[] errorRangeStr = rangeString.Split('–');
            string rngStartStr = errorRangeStr[0];
            string rngLastStr = errorRangeStr[1];
            int rngStart = -1; int rngLast = -1;
            if (Int32.TryParse(rngStartStr, out int sonic))
            {
                rngStart = Int32.Parse(rngStartStr);
            }
            if (Int32.TryParse(rngLastStr, out int tails))
            {
                rngLast = Int32.Parse(rngLastStr);
            }
            if (rngStart == -1 || rngLast == -1) return;

            for(int i = rngStart; i <= rngLast; i++)
            {
                setJsonEditorLineBG(i, true);
            }
        }

        private int[] setProperFatalityRange(int lineNumBoT, string[] errorList)
        {
            //this function looks for the highest range that's less than errorList
            //lineNumBoT: line number where we're back on track
            List<int> entriesToRemove = new List<int>();

            int[] bunkRet = { -1 };

            int rangeEnd = lineNumBoT - 1;
            if (rangeEnd == -1) return bunkRet;
            int closestWeCanGet = -1;
            //we'll look through our errorList for the closest, highest numer before the line num where we're back on track
            //while we're here, we'll get rid of the "BackOnTrack" entry that's in the JSON Anomaly list
            for (int i = 0; i < errorList.Length; i++)
            {
                if (errorList[i].Contains(':'))
                {
                    //error usually looks like 4:missinglabel,
                    string[] errorInfo = errorList[i].Split(':'); ;
                    if (!errorInfo[1].Contains("fatal")) continue;
                    if (Int32.TryParse(errorInfo[0], out int yo))
                    {
                        int currentErrorLnNum = Int32.Parse(errorInfo[0]);
                        if (currentErrorLnNum >= lineNumBoT)
                        {
                            //we got past our line where we're back on Track
                            break;
                        }


                        if (currentErrorLnNum > closestWeCanGet)
                        {
                            closestWeCanGet = currentErrorLnNum;
                        }
                    }
                }
            }

            if (closestWeCanGet == -1) return bunkRet;


            //if we're here, we have a number

            int startingLine = -1;
            int endingLine = rangeEnd;
            int indexOfAnomalyInList = -1; //we'll use this to store what entry is being changed
            //we're going to add "-##" at the end of our LineNum for Fatality, so it looks like ie: 3-11
            //while we're here, we'll get rid of the BackOnTrack entry in error list
            for (int i = 0; i < jsonAnomalyList.Items.Count; i++)
            {
                if (jsonAnomalyList.Items[i].SubItems[0].Text == closestWeCanGet.ToString() &&
                    jsonAnomalyList.Items[i].SubItems[1].Text.Contains("fatal"))
                {
                    //we found it!
                    string oldNum = jsonAnomalyList.Items[i].SubItems[0].Text;
                    if (!Int32.TryParse(oldNum, out int yee)) continue;
                    if (rangeEnd < Int32.Parse(oldNum)) continue;

                    startingLine = Int32.Parse(oldNum);
                    endingLine = rangeEnd;

                    jsonAnomalyList.Items[i].SubItems[0].Text += "–" + rangeEnd;
                    indexOfAnomalyInList = i; //this is used for checktoCombineErrors
                    if (jsonAnomalyList.Items[i + 1].SubItems[1].Text.Contains("BackOnTrack"))
                    {
                        //make sure the line in front of us is the BackOnTrack line, then delete it. if it isn't, then.. it's still going to be there, I guess
                        //entriesToRemove.Add(i + 1);
                        jsonAnomalyList.Items[i+1].Remove();
                    }
                    //if(jsonAnomalyList.Items[i].SubItems[0].Contains())
                }
            }

            if (startingLine == -1) return bunkRet; if (endingLine == -1) return bunkRet; //pretty sure we don't need these

            checkToCombineErrors(startingLine + "–" + endingLine, indexOfAnomalyInList);

            //if we're this far, we should be good to reset the backgrounds of everything with a fatal error
            for (int i = startingLine; i <= endingLine; i++)
            {
                setJsonEditorLineBG(i, true); //sets our background of each affected entry in DataGrid to red
            }

            return entriesToRemove.ToArray();

        }



        /* We use DataGridView instead now
        private void setEditorLinesBGColors(string[] errorList)
        {
            foreach(string error in errorList)
            {
                
                string[] errorInfo = error.Split(':');
                if (errorInfo.Length == 2)
                {
                    string lineNumStr = errorInfo[0];
                    if (Int32.TryParse(lineNumStr, out int yo))
                    {
                        //the last character on the line is a number
                        int lineNum = Int32.Parse(lineNumStr);
                        lineNum -= 1; //why do I need to do this
                        debuggyJsonEditor.Items[lineNum].SubItems[0].BackColor = Color.Red;
                        debuggyJsonEditor.Items[lineNum].SubItems[1].BackColor = Color.Red;
                        debuggyJsonEditor.Items[lineNum].SubItems[0].ForeColor = Color.DarkGray;
                        debuggyJsonEditor.Items[lineNum].SubItems[1].ForeColor = Color.Black;
                    }
                    //errorCode = errorInfo[1];
                }
            }
        }*/

        private void checkToCombineErrors(string errorRange, int ignoreIndex)
        {
            //this function runs through the jsonAnomalyList, and sees if we have errors that intersect
            if (!errorRange.Contains("–")) return;
            string[] errorRangeStr = errorRange.Split('–');
            string rngStartStr = errorRangeStr[0];
            string rngLastStr = errorRangeStr[1];
            int rngStart = -1; int rngLast = -1;
            if (Int32.TryParse(rngStartStr, out int candy))
            {
                rngStart = Int32.Parse(rngStartStr);
            }
            if (Int32.TryParse(rngLastStr, out int canes))
            {
                rngLast = Int32.Parse(rngLastStr);
            }
            if (rngStart == -1 || rngLast == -1) return;

            List<int> anomaliesToPurge = new List<int>(); //stores the zero-based index that we'll get rid of when we're done with for loop

            for (int i=0; i<jsonAnomalyList.Items.Count; i++)
            {
                if (i == ignoreIndex) continue;//we use this to ensure we don't destroy the range we're inquring about
                string lineNumStr = jsonAnomalyList.Items[i].SubItems[0].Text.ToString();
                if (lineNumStr.Contains("–"))
                {
                    //we found another range (which only happens for other fatal errors)

                    string[] oldRangeStr = lineNumStr.Split('–');
                    string oldRngStartStr = oldRangeStr[0];
                    string oldRngLastStr = oldRangeStr[1];
                    int oldRngStart = -1; int oldRngLast = -1;
                    if (Int32.TryParse(oldRngStartStr, out int princess))
                    {
                        oldRngStart = Int32.Parse(oldRngStartStr);
                    }
                    if (Int32.TryParse(oldRngLastStr, out int zelda))
                    {
                        oldRngLast = Int32.Parse(oldRngLastStr);
                    }
                    if (oldRngStart == -1 || oldRngLast == -1) continue;

                    bool rangesIntersect = false;
                    if (rngStart >= oldRngStart && rngStart <= oldRngLast)
                    {
                        //MessageBox.Show("A1: OldLast: " + oldRngStart + ", RngStart: " + rngStart + ", last: " + rngLast);
                        rangesIntersect = true;
                    }
                    if (rngLast >= oldRngStart && rngLast <= oldRngLast)
                    {
                        //MessageBox.Show("B2: OldLast: " + oldRngLast + ", RngStart: " + rngStart + ", last: "+rngLast);
                        rangesIntersect = true;
                    }

                    if (rangesIntersect)
                    {
                        if (rngStart == oldRngLast)
                        {
                            //ie: our old entry said 18-25, but our new entry is 25-30
                            //we need to combine these instead of just deleting it
                            jsonAnomalyList.Items[ignoreIndex].SubItems[0].Text = oldRngStart + "–" + rngLast;
                            return; //I'm not sure if we let this go if it'll fuck up, since we just changed what was in our error range
                            //anything that has a fatal error shouldn't have any other error associated with it, though
                        }
                        else
                        {
                           
                            anomaliesToPurge.Add(i);
                        }
                    }



                } else
                {
                    //whatever error is here is a single-line error
                    //does the entry actually have a number?
                    if (Int32.TryParse(lineNumStr, out int gumdrop))
                    {
                        
                        int lineNum = Int32.Parse(lineNumStr);

                        if (jsonAnomalyList.Items[i].SubItems[1].Text.ToString().Contains("fatality"))
                        {
                            //our entry was a single fatal error
                            //see if our entry is touching our given range(if it's 18, and the range was 12-17, make it 12-18)
                            if (lineNum >= rngStart-1 && lineNum <= rngLast+1)
                            {
                                //since we're updating our errorRange, we're going to back out of this with return
                                //wait, can't we just change errorRange variable and keep going? Or make another variable to store it, then change that?
                                //anomaliesToPurge.Add(i);
                                int newStart = rngStart; int newLast = rngLast;
                                if(lineNum == rngStart - 1)
                                {
                                    newStart = lineNum;
                                } else if(lineNum == rngLast + 1)
                                {
                                    newLast = lineNum;
                                }

                                jsonAnomalyList.Items[ignoreIndex].SubItems[0].Text = newStart + "–" + newLast;
                                jsonAnomalyList.Items.RemoveAt(i);
                                return;

                                
                            }

                        } else
                        {
                            //our entry was anything except a fatal error
                            //see if our entry falls within our given range
                            if (lineNum >= rngStart && lineNum <= rngLast)
                            {
                                //MessageBox.Show("B");
                                anomaliesToPurge.Add(i); //it does, we're going to remove it
                            }
                        }

                        
                    }


                }


            }

            if (anomaliesToPurge.Count > 0)
            {
                foreach (int anomalyIndex in anomaliesToPurge)
                {
                    //MessageBox.Show("Purging: " + anomalyIndex);
                    jsonAnomalyList.Items.RemoveAt(anomalyIndex);
                }
            }


        }

        private void fillErrorList(string[] errorList, bool addIfForgot = true)
        {
            int addedLines = 0;
            foreach (string error in errorList)
            {
                string lineNum = "";
                string errorCode = "";
                string[] errorInfo = error.Split(':');

                if (errorInfo.Length == 1)
                {
                    lineNum = "...";
                    errorCode = error;
                } else
                {
                    lineNum = errorInfo[0];
                    errorCode = errorInfo[1];
                }


                //if we have an error code saying something was forgot, we need to add an extra number to our lineNum
                /*
                if (Int32.TryParse(lineNum, out int yo))
                {
                    int ln = Int32.Parse(lineNum);
                    ln += addedLines;
                    lineNum = ln.ToString();
                }*/

                string[] row = { lineNum, errorCode };
                ListViewItem newItem = new ListViewItem(row);
                jsonAnomalyList.Items.Add(newItem);

                //we only want to add to the line # AFTER we've already added a line

                if (errorCode.Contains("forgot") && addIfForgot)
                {
                    addedLines++;
                }
            }
        }

        List<string> JsonLines = new List<string>();

        private void DoubleClickJsonLine(ListViewItem selection)
        {
            //debuggyJsonEditor.Items[0].SubItems[1].Beg.BeginEdit();
        }

        string[] allLevelNames = { "voke", "stygia", "yhelm", "incaustis", "gehenna", "nihil", "acheron", "sheol", "tutorial" };

        string[] levelNames = { "voke", "stygia", "yhelm", "incaustis", "gehenna", "nihil", "acheron", "sheol" };

        private void debuggyFatality(int oneBasedFirstLine, int oneBasedLastLine, int jsonAnomalyEntryIndex)
        {
            //we use this after we edited something in a range marked "fatal error"; we're checking if the errors are gone
            //unlike debuggy(), this analyzes the lines in the JSON editor(really its source); debuggy() analyzes the original JSON

            //we don't care if they changed anything except the first (we could do last, but then we'd have to rewrite code)
            int rangeLength = oneBasedLastLine - oneBasedFirstLine + 1; //+1 because we need to include the first line

            string lineBeforeRange = JsonLinesBind[oneBasedFirstLine - 2].ListItem.ToString(); //we'll use this to get our placement
            int placement = getPlacementInSequence(lineBeforeRange, oneBasedFirstLine - 2);//this line num wants a zero-based index;
            if (placement == -1) return;
            else placement++; //as long as we found a placement, add one to it as we go into the next line
            //if placement -1, that means the next line is actually a valid line, too (or at least has a label)
            if (placement > expectedFields.Length - 1) placement = 0;

            

            int fixedLines = 0; //we'll use this to know what to subtract from our range if we fixed anything (er really, what to add to the Start #)
            int i = 0;
            while (i < rangeLength)
            {
                int bindIndex = oneBasedFirstLine - 1 + i; //bind index is zero based
                string currentLn = JsonLinesBind[bindIndex].ListItem.ToString();
                if (i + 1 > JsonLinesBind.Count - 1) return; if (JsonLinesBind[bindIndex + 1] == null) return;//make sure next line isn't blank
                string nextLn = JsonLinesBind[bindIndex + 1].ListItem.ToString();
                if (nextLn == "" || nextLn == null) return;
                string lineErrors = getLineErrors(currentLn, nextLn, placement);
                //MessageBox.Show("Get Line Errors for: \n" + currentLn + "\n" + nextLn + "\n" + placement + "\nErrors: \n" + lineErrors + "\n(Length: \n" + lineErrors.Length);
                //need to know what to do if we encounter a duplicate, forgotten line, etc.

                //Alright, if our line contains dupe, forgot, labelinvalid, or fatality, it means we threw an error for a line that doesn't have a valid label
                //we need a valid label to know where we are in the loop (forgot could be an incorrect call if we're missing a few lines and trying to fix them)
                //as long as we don't have those label-reading errors, we were able to detect what we wanted in the next line, so we add to "FixedLines"
                //if we actually encounter another error, we need to add in the new error, while keeping the background red
                

                if (lineErrors.Contains("dupe") || lineErrors.Contains("fatality") || lineErrors.Contains("forgot") || lineErrors.Contains("labelInvalid"))
                {
                    goto SetNewRange;
                    //our line isn't fixed
                    //this breaks out of the for loop too
                } else
                {
                    fixedLines++;
                }

                if (lineErrors == null || lineErrors == "")
                {
                    //if the returned no errors, take away these
                    setJsonEditorLineBG(bindIndex + 1);
                } else
                {
                    //we encountered an error, but it wasn't a fatal error/label-reading error; we'll add it now
                    //We could probably insert it instead, so it comes before?
                    string[] row = { (bindIndex+1).ToString(), lineErrors };
                    ListViewItem newItem = new ListViewItem(row);
                    jsonAnomalyList.Items.Add(newItem);
                }

                placement++;
                i++;

                //the following fixes our placement for the next loop
                //at this point, our placement is what will be there during next loop. nextLn will be our currentLn in said loop
                if (placement == 8 && !nextLn.Contains("\"bankPath\"") && nextLn.Contains("}")) placement = 9;
                if(placement== 10 && nextLn.Contains("{") && !nextLn.Contains("}"))
                {
                    placement = 2;
                }

            }

            SetNewRange:
            if(rangeLength <= fixedLines)
            {
                //we fixed all of the lines in our range!
                //remove it from the JSON anomaly list
                jsonAnomalyList.Items[jsonAnomalyEntryIndex].Remove();
            } else
            {
                //rangeLength is more than the number of lines we fixed, we didn't fix all of them
                //update its numbers
                int newRangeStart = oneBasedFirstLine + fixedLines;
                string newRange = newRangeStart + "–" + oneBasedLastLine;
                jsonAnomalyList.Items[jsonAnomalyEntryIndex].SubItems[0].Text = newRange;
            }

           
            //JsonLinesBind.Skip(oneBasedFirstLine - 1).Take(rangeLength); //OH CRAP this works!!

        }


        private string[] debuggyBurst(int oneBasedLineNum, int extentOfLinesToCheck = 2, int overridePlacement = -1)
        {
            //this is used if we just deleted a line, and didn't have an error there before. it's unlikely there's no errors now

            //we use this to analye a set range of lines that are already in our JSON Editor
            //unlike debuggy(), this analyzes the lines already in the JSON editor(really its source); debuggy() analyzes the original JSON

            List<string> linesWithErrors = new List<string>();

            string lineInitiant = JsonLinesBind[oneBasedLineNum - 1].ListItem.ToString(); 
            string lineBefore = JsonLinesBind[oneBasedLineNum - 2].ListItem.ToString();//we'll use these to get our placement
            int placement = getPlacementInSequence(lineBefore, oneBasedLineNum - 2);//this line num wants a zero-based index;
            if (overridePlacement != -1) placement = overridePlacement;
            if (placement == -1) { linesWithErrors.Add(oneBasedLineNum+":errorReadingLine"); return linesWithErrors.ToArray(); }


            //placement++; //we're about to check the line in our debuggyBurst's perameters
            //if (placement > expectedFields.Length - 1) placement = 0; //fix it if we went too far
            //   ^-->we do this at the beginning of the while loop instead. It only happens if i is >= 2, meaning on the third line at least

            bool fatalErrorEncountered = false;
            int i = oneBasedLineNum-1;
            int linesChecked = 0;
            int threshold = 0; //prevent infintie loop
            while (i < JsonLinesBind.Count && linesChecked < extentOfLinesToCheck)
            {
                //use threshold to prevent infinite loop
                #region PreventInfiniteLoop
                threshold++; //threshold is used just to verify this doesn't get stuck in an infinite loop
                if (threshold > 300)
                {
                    //string[] tooLong = { "2long" };
                    linesWithErrors.Add("(2long)");
                    return linesWithErrors.ToArray();
                }
                #endregion PreventInfiniteLoop
                linesChecked++; //this should be okay to hit extentOfLinesToCheck and still finish the loop
                #region DebugLines
                string line_unaltered = JsonLinesBind[i].ListItem.ToString(); //get the line we're checking
                line_unaltered = line_unaltered.Replace("\r", "");//these things keep fucking me
                string line_nospaces = NormalizeWhiteSpace(line_unaltered, true); //gives us us the individual line, with no spaces whatsoever


                /*
                if (line_nospaces == null || line_nospaces == "")
                {

                    linesWithErrors.Add(i + 1 + ":empty");
                    i++;
                    placement++;
                    continue;
                }*/

                //if a fatal error has occured, keep skipping lines until we get to a line with "]" or "LevelName":
                #region FatalErrorHandler
                if (fatalErrorEncountered)
                {
                    if ((line_nospaces.Contains("\"LevelName\"") && JsonLinesBind[i - 1].ListItem.ToString().Contains("{") )
                        || line_nospaces.Contains("]"))
                    {
                        if (line_nospaces.Contains("\"LevelName\"") && JsonLinesBind[i - 1].ListItem.ToString().Contains("{"))
                        {
                            if (JsonLinesBind[i - 1].ListItem.ToString().Contains("{"))
                            {
                                //this line has "LevelName", and the line before it has "{"
                                i--;//we'll go back and start verifying again on the line before this
                                placement = 0;
                                linesWithErrors.Add(i + 1 + ":BackOnTrack"); //this isn't an error, but we'll check for it, and if we have it, we're out of the fatal error
                            }
                            /*
                            else
                            {
                                //this line has "LevelName" but we don't have a "{" in the line before it
                                if (i < oneBasedLineNum+1) { i++; continue; } //we most likely just deleted the previous item
                                linesWithErrors.Add(i + 1 + ":BackOnTrack"); //this isn't an error, but we'll check for it, and if we have it, we're out of the fatal error
                                placement = 1;
                            }*/
                        }
                        else if (line_nospaces.Contains("]"))
                        {
                            placement = expectedFields.Length; //when we reset this, it will recheck this line and activate what we do if we see the closing ] bracket
                            linesWithErrors.Add(i + 1 + ":BackOnTrack");
                        }
                        fatalErrorEncountered = false;
                        linesChecked += 500;
                        continue;
                    }
                    else
                    {
                        linesChecked--; //make this keep going
                        i++;
                        continue;
                    }

                }
                #endregion FatalErrorHandler


                

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
                                linesWithErrors.Add("1:unexpChars_" + unexpChars);
                            }

                        }
                        else
                        {
                            //there's no { in the first line
                            //label is missing, we need to look around for something identifiable
                            //technically, they could have the first line be-> { "customLevelMusic":[ and still be correct
                            string[] firstLineError = { "wtf?" };
                            if (JsonLinesBind.Count < 2) return firstLineError; //catch-all in case JSON is very short

                            if (JsonLinesBind[1].ListItem.ToString().Contains("{"))
                            {
                                //our { is on the next line

                                linesWithErrors.Add("1:grbgLine");


                            }
                            else if (JsonLinesBind[1].ListItem.ToString().Contains("\"customLevelMusic\""))
                            {
                                //we can't find {, but customLevelMusic is on next line

                                linesWithErrors.Add("1:forgot1stLn");
                            }
                        }
                        i++; continue;
                    }
                    else
                    {

                        //second line, we want "customLevelMusic" : [
                        string firstLineNoSpaces = NormalizeWhiteSpace(JsonLinesBind[0].ListItem.ToString(), true);

                        if (line_nospaces != "\"customLevelMusic\":[")
                        {
                            string[] secondLineError = { "wtf?" };
                            if (JsonLinesBind.Count < 3) return secondLineError;

                            if (JsonLinesBind[2].ListItem.ToString().Contains("{"))
                            {
                                //our next line contains the next sequence, we forgot the CLM opener
                                secondLineError[0] = "2:forgotCLM";
                                return secondLineError;
                            }
                            else if (line_nospaces.Contains("{") && JsonLinesBind[2].ListItem.ToString().Contains("\"customLevelMusic\":["))
                            {
                                //this line has a {, which our first line has, and our next line has the customLevelMusic thing
                                secondLineError[0] = "2:dupe";

                            }
                            else if (line_nospaces.Contains("{") && firstLineNoSpaces.Contains("{\"customLevelMusic\":["))
                            {
                                //the user seems to have combined { and "customLevelMusic" : [ onto one line. that works, but we don't want that
                                secondLineError[0] = "2:forgotClmFormat"; //we want it to add another
                            }
                            else
                            {
                                linesWithErrors.Add("2:clmF"); //customLevelMusic opening line incorrect format
                            }


                        }
                        i++; continue;
                    }

                }
                else if (i >= 2)
                {
                    placement++; //we're going to do this at the beginning, instead of at the end
                    if (i == 2)
                    {
                        //we're on our first line out of the openers, we should verify our placement

                        if (!line_unaltered.Contains(expectedFields[placement]))
                        {
                            //the first line did not have a {
                            string[] firstTwoLines = { line_unaltered, JsonLinesBind[i+1].ListItem.ToString() };
                            int backOnTrack = verifyWTFsGoingOnFirstLine(firstTwoLines);
                            if (backOnTrack == -1)
                            {
                                string[] fatalError = { (i + 1) + ":fatality" };
                                fatalErrorEncountered = true;
                                return fatalError;
                            }
                            else
                            {
                                placement = backOnTrack;
                                linesWithErrors.Add((i) + ":forgotClmFormat"); //we don't want i-1
                                continue;
                            }

                        }

                    }

                    //if we're above our length, we might be done, check for closing ]
                    if (placement >= expectedFields.Length)
                    {
                        if (line_unaltered.Contains("]"))
                        {
                            linesWithErrors.Add("Found]");
                            break;
                        }
                        else
                        {
                            placement = 0;
                        }
                    }

                    bool hasMatchingLabel = line_nospaces.Contains(expectedFields[placement]); //labelMatches is true if our expectedField matches with whatever's on the line

                    if (placement == 2)
                    {
                        //"MainMusic" : {
                        bool hasMainMusic = line_nospaces.Contains("\"MainMusic\""); //labelMatches is true if our expectedField matches with whatever's on the line
                        bool hasBossMusic = line_nospaces.Contains("\"BossMusic\""); //labelMatches is true if our expectedField matches with whatever's on the line
                        if (hasMainMusic || hasBossMusic)
                        {
                            hasMatchingLabel = true;
                        }


                    }
                    else if (placement == 8)
                    {
                        //"bankPath": "R:\\SteamLibrary\\steamapps\\common\\Metal Hellsinger\\MODS\\Unstoppable\\Unstoppable_All.bank"
                        if (!hasMatchingLabel)
                        {
                            if (line_unaltered.Contains("}"))
                            {
                                //linesWithErrors.Add("(" + i + ":nobp)");
                                placement++; //move our current place forward
                                //i--; //move our current line back to recheck this line this isn't a for loop, we don't auto add
                                continue; //cancel all other calculations, we're rechecking this line

                            }
                        }
                    }
                    else if (placement == 10)
                    {
                        //Level Closing }, or BossMusic
                        if (line_unaltered.Contains("\"BossMusic\"") || line_unaltered.Contains("\"MainMusic\""))
                        {
                            placement = 2;
                            continue;
                        }
                        else if (!hasMatchingLabel)
                        {

                        }
                    }

                    string lineErrors = getLineErrors(line_unaltered, JsonLinesBind[i + 1].ListItem.ToString(), placement);

                    

                    
                    if (lineErrors.Contains("dupe"))
                    {
                        placement--;
                        //if we can see there was a duplicate entry in our line, we need to set our currPlaceInExpectedEntry back;
                        //we'll be back on track after the rest of the for loop runs
                        //this is getting called if our line was blank, in which case it catches up with where it wants to be, but throws a false error

                    }

                    if (lineErrors.Contains("forgot"))
                    {
                        //placement++;
                        //if we forgot something and we see it at the beginning, we'll add a line
                        //if we changed something to trigger debuggyBurst, we don't want to prevent the user from being able to add a line
                        //(as in, if we just kept placement++ like debuggy(), it'll keep putting the line back in if the user is trying to get rid of it
                        //lineErrors = lineErrors.Replace("forgot", "fatality"); //we're going to treat it as a fatal error
                        linesWithErrors.Add(i + 1 + ":" + lineErrors);
                        fatalErrorEncountered = true;
                        i++;
                        continue;
                    }

                    if (lineErrors.Contains("fatal"))
                    {
                        fatalErrorEncountered = true;
                        linesWithErrors.Add(i + 1 + ":" + lineErrors);
                        i++;
                        continue;
                    }
                    
                    if (lineErrors.Length > 0)
                    {
                        linesWithErrors.Add(i + 1 + ":" + lineErrors); //i+1 because we don't start on line 0

                    }
                }


                i++;


                #endregion DebugLines
            }

            if (linesWithErrors.Count > 0)
            {
                string[] errorList = linesWithErrors.ToArray();
                return errorList;
            }
            else
            {
                string[] noErrors = { "none" };
                return noErrors;
            }
        }

        private string replaceAllTabs(string fullJson)
        {
            if (fullJson.Contains('\t'))
            {
                string newJson = fullJson.Replace("\t", "        ");
                return newJson;
            } else
            {
                return fullJson;
            }
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
                    }
                    else if (fixedLine.Contains("]") && i != fixedJsonLines.Length - 2)
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
                            }
                            else if (z >= 2 && z <= 3)
                            {
                                //BossMusic or MainMusic
                                rewrittenLine = "            \"" + lineLabel + " {\n";
                            }
                            else
                            {
                                //one of the labels
                                if (uniqueKeywords[z] == "\"BeatInputOffset\"" || uniqueKeywords[z] == "\"BPM\"")
                                {

                                    //we don't want to add quotes to info
                                    rewrittenLine = "                " + lineLabel + " " + lineInfo;
                                }
                                else
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
                                    }
                                    else
                                    {
                                        //no bankPath detected after this line
                                        rewrittenLine += "\n";
                                    }
                                }

                            }
                            //MessageBox.Show("Found: " + uniqueKeywords[z]+ "\n Rewritten: " + rewrittenLine);

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
                                }
                                else if (fixedJsonLines[i + 1].Contains("}"))
                                {
                                    //the line underneath us is closing out the level, don't put a comma
                                    fixedJson += "            }\n";

                                    continue;
                                }
                            }
                            else if (fixedJsonLines[i - 1].Contains("}"))
                            {
                                //the line before this is closing out the music, we need to close out the level
                                rewrittenLine = "        }";
                                if (fixedJsonLines[i + 1].Contains("{"))
                                {
                                    //the next line opens up to another level, so put a comma
                                    rewrittenLine += ",\n";
                                }
                                else
                                {
                                    rewrittenLine += "\n";
                                }
                                fixedJson += rewrittenLine;
                                continue;


                            }
                            else
                            {
                                return "Error - there was an error trying to fix the horizontal tabs";
                            }
                        }

                    }



                }
                else
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

        private string ReplaceErrorWShortDescrptn(string errorCodes)
        {
            string errorsShortDiscrptn = "";
            string errorsEnclsdErrCode = "";
            if (errorCodes.Contains("tabs"))
            {
                errorsShortDiscrptn += "Horizontal tabs/indentation replaced with spaces.";
                errorsEnclsdErrCode += "(tabs)";
            }
            if (errorCodes.Contains("2short"))
            {
                errorsShortDiscrptn += "Json file too short. ";
                errorsEnclsdErrCode += "(2short)";
            }
            if (errorCodes.Contains("2long"))
            {
                errorsShortDiscrptn += "Json file way too long. ";
                errorsEnclsdErrCode += "(2long)";
            }
            if (errorCodes.Contains("empty"))
            {
                errorsShortDiscrptn += "Empty line. ";
                errorsEnclsdErrCode += "(empty)";
            }
            if (errorCodes.Contains("BackOnTrack"))
            {
                errorsShortDiscrptn += "";
                errorsEnclsdErrCode += "(BackOnTrack)";
            }
            if (errorCodes.Contains("unexpChars_"))
            {
                //string 

                errorsShortDiscrptn += "Unexpected anomalies. ";
                errorsEnclsdErrCode += "(unexpChars)";
            }
            if (errorCodes.Contains("unexpChLC_"))
            {

                errorsShortDiscrptn += "Unexpected anomalies near label. ";
                errorsEnclsdErrCode += "(unexpChLC)";
            }
            if (errorCodes.Contains("grbgLine"))
            {

                errorsShortDiscrptn += "Unneeded Line. ";
                errorsEnclsdErrCode += "(grbgLine)";
            }
            if (errorCodes.Contains("erj"))
            {

                errorsShortDiscrptn += "Error reading first few lines. ";
                errorsEnclsdErrCode += "(erj)";
            }
            if (errorCodes.Contains("forgot1stLn"))
            {

                errorsShortDiscrptn += "First line missing. ";
                errorsEnclsdErrCode += "(forgot1stLn)";
            }
            if (errorCodes.Contains("forgotCLM"))
            {

                errorsShortDiscrptn += "customLevelMusic opener missing. ";
                errorsEnclsdErrCode += "(forgotCLM)";
            }
            if (errorCodes.Contains("dupe"))
            {
                string dupedItem = getNextItemInQuotes(errorCodes, "dupe_");


                errorsShortDiscrptn += "Possible duplicate ";
                if (dupedItem.Length > 0) errorsShortDiscrptn += "of " + dupedItem + ". ";
                errorsEnclsdErrCode += "(dupe)";
            }
            if (errorCodes.Contains("forgotClmFormat"))
            {
                errorsShortDiscrptn += "Technically correct, but please separate { to its own line. ";
                errorsEnclsdErrCode += "(forgotClmFormat)";
            }
            if (errorCodes.Contains("clmF"))
            {

                errorsShortDiscrptn += "customLevelMusic opener missing. ";
                errorsEnclsdErrCode += "(forgotCLM)";
            }
            if (errorCodes.Contains("fatality"))
            {

                errorsShortDiscrptn += "Fatal error reading Json. ";
                errorsEnclsdErrCode += "(fatality)";
            }
            if (errorCodes.Contains("fatal"))
            {

                errorsShortDiscrptn += "Fatal error reading Json. ";
                errorsEnclsdErrCode += "(fatal)";
            }
            if (errorCodes.Contains("forgot_"))
            {
                //need to get 2nd part of this that says what was forgotten
                string forgottenItem = getNextItemInQuotes(errorCodes, "forgot_");


                if (forgottenItem.Length > 0) errorsShortDiscrptn += forgottenItem + " skipped. ";
                else errorsShortDiscrptn += "Label skipped. ";

                errorsEnclsdErrCode += "(forgot)";
            }
            if (errorCodes.Contains("clmClose"))
            {

                errorsShortDiscrptn += "Closing ] and/or } missing. ";
                errorsEnclsdErrCode += "(fatal)";
            }
            if (errorCodes.Contains("clmClsUnexpCh"))
            {
                //need to get 2nd part that says the anomalies
                //not going to

                errorsShortDiscrptn += "Unexpected anomalies. ";
                errorsEnclsdErrCode += "(clmClsUnexpCh)";
            }
            if (errorCodes.Contains("forgotClosers"))
            {
                errorsShortDiscrptn += "Closing ] and/or } missing. ";
                errorsEnclsdErrCode += "(forgotClosers)";
            }
            if (errorCodes.Contains("noClmClose"))
            {
                errorsShortDiscrptn += "Closing ] and/or } missing. ";
                errorsEnclsdErrCode += "(forgotClosers)";
            }
            if (errorCodes.Contains("bunkLine"))
            {
                errorsShortDiscrptn += "Unneeded Line. ";
                errorsEnclsdErrCode += "(bunkLine)";
            }
            if (errorCodes.Contains("labelInvalid"))
            {
                errorsShortDiscrptn += "Unrecognized label, possibly misspelled or missing quotes. ";
                errorsEnclsdErrCode += "(labelInvalid)";
            }
            if (errorCodes.Contains("unexpEnd-"))
            {
                //need to get the 2nd part
                if(errorCodes.Contains("unexpEnd-Wanted_\",\""))
                {
                    errorsShortDiscrptn += "Unexpected end of line, possibly missing comma. ";
                } else
                {
                    errorsShortDiscrptn += "Unexpected end of line. ";
                }

                errorsEnclsdErrCode += "(unexpEnd-)";
            }
            if (errorCodes.Contains("unexpEndN"))
            {
                errorsShortDiscrptn += "Unexpected end, expecting number with no comma. ";
                errorsEnclsdErrCode += "(unexpEndN)";
            }
            if (errorCodes.Contains("missingcomma"))
            {
                errorsShortDiscrptn += "Unexpected comma at the end of line. ";
                errorsEnclsdErrCode += "(missingcomma)";
            }
            if (errorCodes.Contains("unexpChars"))
            {
                //need to get 2nd part

                errorsShortDiscrptn += "Unexpected anomalies found. ";
                errorsEnclsdErrCode += "(unexpChars)";
            }
            if (errorCodes.Contains("nocolon"))
            {
                errorsShortDiscrptn += "No colon ( : ) to separate label from value. ";
                errorsEnclsdErrCode += "(nocolon)";
            }
            if (errorCodes.Contains("2manycol"))
            {
                errorsShortDiscrptn += "There was more than one colon ( : ). ";
                errorsEnclsdErrCode += "(2manycol)";
            }
            if (errorCodes.Contains("bpnocol"))
            {
                errorsShortDiscrptn += "Expecting absolute file location in bankPath. ";
                errorsEnclsdErrCode += "(bpnocol)";
            }
            if (errorCodes.Contains("tmcom"))
            {
                errorsShortDiscrptn += "Possible extra or misplaced comma. ";
                errorsEnclsdErrCode += "(tmcom)";
            }
            if (errorCodes.Contains("neqVal"))
            {
                errorsShortDiscrptn += "Missing quotes around value. ";
                errorsEnclsdErrCode += "(neqVal)";
            }
            if (errorCodes.Contains("LCap"))
            {
                errorsShortDiscrptn += "Level capitalization invalid. ";
                errorsEnclsdErrCode += "(LCap)";
            }
            if (errorCodes.Contains("LUr"))
            {
                //need to get 2nd part

                errorsShortDiscrptn += "Level unrecognized. ";
                errorsEnclsdErrCode += "(LUr)";
            }
            if (errorCodes.Contains("evF1"))
            {
                errorsShortDiscrptn += "Event format invalid. ";
                errorsEnclsdErrCode += "(evF1)";
            }
            if (errorCodes.Contains("evF2"))
            {
                errorsShortDiscrptn += "Event format invalid. ";
                errorsEnclsdErrCode += "(evF2)";
            }
            if (errorCodes.Contains("unwntdq"))
            {
                errorsShortDiscrptn += "Unexpected quotes around value. ";
                errorsEnclsdErrCode += "(unwntdq)";
            }
            if (errorCodes.Contains("unexpChRC_"))
            {
                //need to get 2nd part


                errorsShortDiscrptn += "Unexpected characters near value. ";
                errorsEnclsdErrCode += "(unexpChRC)";
            }
            if (errorCodes.Contains("numFormat"))
            {
                errorsShortDiscrptn += "Value must be a number. ";
                errorsEnclsdErrCode += "(numFormat)";
            }
            if (errorCodes.Contains("nobpfile"))
            {
                errorsShortDiscrptn += "No .bank file in bankPath. ";
                errorsEnclsdErrCode += "(nobpfile)";
            }
            if (errorCodes.Contains("bpws"))
            {
                errorsShortDiscrptn += "Wrong slashes in bankPath. ";
                errorsEnclsdErrCode += "(bpws)";
            }
            if (errorCodes.Contains("nobpdir"))
            {
                errorsShortDiscrptn += "Error looking for directory in bankPath. ";
                errorsEnclsdErrCode += "(nobpdir)";
            }
            if (errorCodes.Contains("cvBP"))
            {
                errorsShortDiscrptn += "Can't verify bankPath's format. ";
                errorsEnclsdErrCode += "(cvBP)";
            }
            if (errorCodes.Contains("2mSl"))
            {
                errorsShortDiscrptn += "Too many slashes in bankPath. ";
                errorsEnclsdErrCode += "(2mSl)";
            }
            if (errorCodes.Contains("bPF"))
            {
                //NEED TO MAKE SURE NONE OF THE BANKFORMATTING ERRORS WILL OVERLAP

                errorsShortDiscrptn += "bankPath formatting off. "; //number of \s might be off
                errorsEnclsdErrCode += "(bPF)";
            }
            if (errorCodes.Contains("bpFNF"))
            {
                errorsShortDiscrptn += "File in bankPath doesn't exist. ";
                errorsEnclsdErrCode += "(bpFNF)";
            }
            if (errorCodes.Contains("NL_bossnotmain"))
            {
                errorsShortDiscrptn += "Next line, MM cannot handle Boss music before Main music. ";
                errorsEnclsdErrCode += "(bossmain)";
            }
            if (errorCodes.Contains("bmdup"))
            {
                errorsShortDiscrptn += "Level contains two Boss Musics. ";
                errorsEnclsdErrCode += "(bmdup)";
            }
            if (errorCodes.Contains("mmdup"))
            {
                errorsShortDiscrptn += "Level contains two Main Musics. ";
                errorsEnclsdErrCode += "(mmdup)";
            }

            int numberOfParentheses = errorsEnclsdErrCode.Split('(').Length - 1;
            string multiErrorString = "";
            if(numberOfParentheses > 1)
            {
                multiErrorString = "(" + numberOfParentheses + ")";
            }

            string returnString = multiErrorString + errorsShortDiscrptn + errorsEnclsdErrCode;

            return returnString;

        }

        int numberOfCritErrors;
        int numberOfPotntlErrors;
        private void ReplaceAllErrorsWithDescriptions()
        {
            //We'll also handle Number of Critical Errors here
            numberOfCritErrors = 0;
            numberOfPotntlErrors = 0;

            for (int i=0; i<jsonAnomalyList.Items.Count; i++)
            {
                string errorCode = jsonAnomalyList.Items[i].SubItems[1].Text;
                string desc = ReplaceErrorWShortDescrptn(errorCode);

                jsonAnomalyList.Items[i].SubItems[1].Text = desc;

                int numberOfParentheses = desc.Split('(').Length - 1;
                if(numberOfParentheses == 0)
                {
                    //group 3 says "no errors remaining"
                    if (jsonAnomalyList.Items.Count == 1)
                    {
                        jsonAnomalyList.Items[i].Group = jsonAnomalyList.Groups[3];
                        jsonAnomalyList.Items[i].SubItems[0].Text = "";
                        jsonAnomalyList.Items[i].SubItems[1].Text = "";
                    } else
                    {
                        //this will come up if we had an unknown error (and we had another error. we're fucked if it was the only error)
                        jsonAnomalyList.Items[i].SubItems[0].Text = "...";
                        jsonAnomalyList.Items[i].SubItems[1].Text = "An unknown error broke me, please contact admin. :C";
                    }

                }
                else if (numberOfParentheses == 1) {
                    //we only have one error
                    if (jsonAnomalyList.Items[i].SubItems[1].Text.Contains("(bpFNF)"))
                    {
                        //group 2 is Potential Issues
                        jsonAnomalyList.Items[i].Group = jsonAnomalyList.Groups[2];
                        numberOfPotntlErrors++;
                    } else if (jsonAnomalyList.Items[i].SubItems[1].Text.Contains("(tabs)"))
                    {
                        //group 1 is Minor Issues
                        jsonAnomalyList.Items[i].Group = jsonAnomalyList.Groups[1];
                    } else
                    {
                        //group 0 is Critical Errors
                        
                        jsonAnomalyList.Items[i].Group = jsonAnomalyList.Groups[0];
                        numberOfCritErrors++;
                        
                    }
                } else
                {
                    jsonAnomalyList.Items[i].Group = jsonAnomalyList.Groups[0];
                    numberOfCritErrors++;
                }

            }
        }


        private string getNextItemInQuotes(string fullString, string labelWeWant)
        {
            string returnString = "";

            int indexOfLabel = fullString.IndexOf(labelWeWant);
            if (indexOfLabel == -1) return returnString;

            int indexOfFirstQuote = fullString.IndexOf("\"", indexOfLabel);
            int afterFirstQuote = indexOfFirstQuote + 1;
            if (indexOfFirstQuote == -1) return returnString;
            int indexOf2ndQuote = fullString.IndexOf("\"", indexOfFirstQuote + 1);
            if (indexOf2ndQuote == -1) return returnString;

            int itemLength = indexOf2ndQuote - afterFirstQuote;
            if (itemLength < 1) return returnString;

            returnString = fullString.Substring(afterFirstQuote, itemLength);



            return returnString;
        }



        private string emptyLineHandler(string[] allLines, int zbLineNum, int currExpPlcmnt)
        {
            //we got an empty line
            //let's figure out what the hell's going on

            //zbLineNum = zerobased line number
            string previousLine = null;
            //string currentLine is blank....
            string nextLine = null;
            if(zbLineNum > 0)
            {
                previousLine = allLines[zbLineNum - 1];
            }
            if (zbLineNum < allLines.Length-1)
            {
                nextLine = allLines[zbLineNum - 1];
            }
            int prevPlcmt = currExpPlcmnt - 1; if (prevPlcmt < 0) prevPlcmt = 10;
            int nextPlcmt = currExpPlcmnt + 1; if (prevPlcmt >= expectedFields.Length) prevPlcmt = 0; //i think it'd be -= expectedFields.Length but screw it

            bool previousLineHasItsValidLabel = false; //declare these
            bool nextLineHasItsValidLabel = false; //we'll check and make sure the lines can be read, and then detect if they have labels we want
            if(previousLine != null) previousLineHasItsValidLabel = previousLine.Contains(expectedFields[prevPlcmt]);
            if (nextLine != null) nextLineHasItsValidLabel = nextLine.Contains(expectedFields[nextPlcmt]);

            if(previousLineHasItsValidLabel && nextLineHasItsValidLabel)
            {
                //we can immediately tell what's going on: our blank line falls in line with our current placement
            }

            if (previousLineHasItsValidLabel)
            {
                //we can find our previously wanted label in the place before, there's no fatal issues behind us
            }

            if (nextLineHasItsValidLabel)
            {
                //we can look ahead and see the label we want is ahead of us
            }

            return "";
        }

        private string[] BuggyD_BindList(int omit = -1)
        {

            List<string> linesWithErrors = new List<string>();
            bool fatalErrorEncountered = false;//we use this if we find a fatal error; we keep skipping lines until we get out of the level with the error
            
            if (JsonLinesBind.Count < 10) { string[] tooshort = { "2short" }; return tooshort; }

            int bmCounter = 0; int mmCounter = 0;
            int currPlaceInExpctdEntry = 0; //current place in expected entry
            int i = 0;
            int threshold = 0;//threshold used to prevent infinite loop
            while (i < JsonLinesBind.Count-1)
            {
                if(i==omit) { i++; continue; }

                //use threshold to prevent infinite loop
                #region PreventInfiniteLoop
                threshold++; //threshold is used just to verify this doesn't get stuck in an infinite loop
                if (threshold > 300)
                {
                    //string[] tooLong = { "2long" };
                    linesWithErrors.Add("(2long)");
                    return linesWithErrors.ToArray();
                }
                #endregion PreventInfiniteLoop

                #region DebugLines
                string line_unaltered = JsonLinesBind[i].ListItem.ToString(); //get the line we're checking
                line_unaltered = line_unaltered.Replace("\r", "");//these things keep fucking me
                string line_nospaces = NormalizeWhiteSpace(JsonLinesBind[i].ListItem.ToString(), true); //gives us us the individual line in the JSON, with no spaces whatsoever

                if (line_nospaces == null || line_nospaces == "")
                {

                    linesWithErrors.Add(i + 1 + ":empty");
                    i++;
                    if (i >= 2) currPlaceInExpctdEntry++;
                    continue;
                }

                //if a fatal error has occured, keep skipping lines until we get to a line with "]" or "LevelName":
                #region FatalErrorHandler
                if (fatalErrorEncountered)
                {
                    bmCounter = 0; mmCounter = 0;
                    if (line_nospaces.Contains("LevelName") || line_nospaces.Contains("]"))
                    {
                        if (line_nospaces.Contains("LevelName"))
                        {
                            if (JsonLinesBind[i-1].ListItem.ToString().Contains("{"))
                            {
                                //this line has "LevelName", and the line before it has "{"
                                i--;//we'll go back and start verifying again on the line before this
                                currPlaceInExpctdEntry = 0;
                                linesWithErrors.Add(i + 1 + ":BackOnTrack"); //this isn't an error, but we'll check for it, and if we have it, we're out of the fatal error
                            }
                            else
                            {
                                //this line has "LevelName" but we don't have a "{" in the line before it
                                linesWithErrors.Add(i + 1 + ":BackOnTrack"); //this isn't an error, but we'll check for it, and if we have it, we're out of the fatal error
                                currPlaceInExpctdEntry = 1;
                            }
                        }
                        else if (line_nospaces.Contains("]"))
                        {
                            currPlaceInExpctdEntry = expectedFields.Length; //when we reset this, it will recheck this line and activate what we do if we see the closing ] bracket
                            linesWithErrors.Add(i + 1 + ":BackOnTrack");
                        }
                        fatalErrorEncountered = false;
                        continue;
                    }
                    else
                    {
                        i++;
                        continue;
                    }

                }
                #endregion FatalErrorHandler


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
                                linesWithErrors.Add("1:unexpChars_"+unexpChars);
                            }
                        }
                        else
                        {
                            //there's no { in the first line
                            //label is missing, we need to look around for something identifiable
                            //technically, they could have the first line be-> { "customLevelMusic":[ and still be correct
                            string[] firstLineError = { "erj" };
                            if (JsonLinesBind.Count < 2) return firstLineError; //catch-all in case JSON is very short

                            if (JsonLinesBind[1].ListItem.ToString().Contains("{"))
                            {
                                //our { is on the next line

                                linesWithErrors.Add("1:grbgLine");


                            }
                            else if (JsonLinesBind[1].ListItem.ToString().Contains("\"customLevelMusic\""))
                            {
                                //we can't find {, but customLevelMusic is on next line

                                linesWithErrors.Add("1:forgot1stLn");
                            }
                        }
                        i++; continue;
                    }
                    else
                    {

                        //second line, we want "customLevelMusic" : [
                        string firstLineNoSpaces = NormalizeWhiteSpace(JsonLinesBind[0].ListItem.ToString(), true);

                        if (line_nospaces != "\"customLevelMusic\":[")
                        {
                            string[] secondLineError = { "erj" };
                            if (JsonLinesBind.Count < 3) return secondLineError;

                            if (JsonLinesBind[2].ListItem.ToString().Contains("{"))
                            {
                                //our next line contains the next sequence, we forgot the CLM opener
                                secondLineError[0] = "2:forgotCLM";
                                return secondLineError;
                            }
                            else if (line_nospaces.Contains("{") && JsonLinesBind[2].ListItem.ToString().Contains("\"customLevelMusic\":["))
                            {
                                //this line has a {, which our first line has, and our next line has the customLevelMusic thing
                                secondLineError[0] = "2:dupe";
                                return secondLineError;
                            }
                            else if (line_nospaces.Contains("{") && firstLineNoSpaces.Contains("{\"customLevelMusic\":["))
                            {
                                //the user seems to have combined { and "customLevelMusic" : [ onto one line. that works, but we don't want that
                                secondLineError[0] = "2:forgotClmFormat"; //we want it to add another
                            }
                            else
                            {
                                linesWithErrors.Add("2:clmF"); //customLevelMusic opening line incorrect format
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
                            string[] firstTwoLines = { JsonLinesBind[i].ListItem.ToString(), JsonLinesBind[i+1].ListItem.ToString() };
                            int backOnTrack = verifyWTFsGoingOnFirstLine(firstTwoLines);
                            if (backOnTrack == -1)
                            {
                                string[] fatalError = { (i + 1) + ":fatality" };
                                fatalErrorEncountered = true;
                                return fatalError;
                            }
                            else
                            {
                                currPlaceInExpctdEntry = backOnTrack;
                                linesWithErrors.Add((i) + ":forgotClmFormat"); //we don't want i-1
                                continue;
                            }

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
                            if (JsonLinesBind[i].ListItem.ToString().Contains("}"))
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
                        if (JsonLinesBind[i].ListItem.ToString().Contains("\"BossMusic\"") ||
                            JsonLinesBind[i].ListItem.ToString().Contains("\"MainMusic\""))
                        {
                            currPlaceInExpctdEntry = 2;
                            continue;
                        }
                        else if (!hasMatchingLabel)
                        {

                        }
                    }
                    string thisLn = JsonLinesBind[i].ListItem.ToString();
                    if (i + 1 > JsonLinesBind.Count) { goto FoundCLMEnd; }
                    string nxtLn = JsonLinesBind[i + 1].ListItem.ToString();
                    

                    string lineErrors = getLineErrors(thisLn, nxtLn, currPlaceInExpctdEntry);

                    if (lineErrors.Contains("fatal"))
                    {
                        fatalErrorEncountered = true;
                        linesWithErrors.Add(i + 1 + ":" + lineErrors);
                        i++;
                        continue;
                    }

                    if (lineErrors.Contains("dupe"))
                    {
                        currPlaceInExpctdEntry--;
                        //if we can see there was a duplicate entry in our line, we need to set our currPlaceInExpectedEntry back;
                        //we'll be back on track after the rest of the for loop runs
                        //this is getting called if our line was blank, in which case it catches up with where it wants to be, but throws a false error

                    }
                    #region Look For Boss/MainMusic Doubles
                    
                    if (currPlaceInExpctdEntry == 2 &&
                        (JsonLinesBind[i].ListItem.ToString().Contains("\"BossMusic\"") || JsonLinesBind[i].ListItem.ToString().Contains("\"MainMusic\"")))
                    {
                        if (JsonLinesBind[i].ListItem.ToString().Contains("\"BossMusic\""))
                        {
                            bmCounter++;
                        }
                        else if (JsonLinesBind[i].ListItem.ToString().Contains("\"MainMusic\""))
                        {
                            mmCounter++;
                        }
                    }
                    else if (currPlaceInExpctdEntry == 0 || currPlaceInExpctdEntry == 1)
                    {
                        bmCounter = 0; mmCounter = 0;
                    }
                    if (bmCounter > 1)
                    {
                        lineErrors += ("(bmdup)");
                        bmCounter = 0; //reset it so it doesn't keep going
                    }
                    if (mmCounter > 1)
                    {
                        lineErrors += ("(mmdup)");
                        mmCounter = 0; //reset it so it doesn't keep going
                    }
                    #endregion Look For Boss/MainMusic Doubles

                    if (lineErrors.Contains("forgot"))
                    {
                        currPlaceInExpctdEntry++;
                    }

                    if (lineErrors.Length > 0)
                    {
                        linesWithErrors.Add(i + 1 + ":" + lineErrors); //i+1 because we don't start on line 0

                    }

                    currPlaceInExpctdEntry++;
                }


                i++;


                #endregion DebugLines
            }

        FoundCLMEnd:
            //since we just broke out of the while loop, i should still be the line number with the ] 
            #region Final Lines
            if (i < JsonLinesBind.Count)
            {
                string allFinalLines = "";
                for (int g = i; g < JsonLinesBind.Count; g++)
                {
                    allFinalLines += JsonLinesBind[g].ListItem.ToString();
                }

                if (JsonLinesBind[i].ListItem.ToString().Contains("]"))
                {
                    if (i < JsonLinesBind.Count - 1)
                    {
                        //as long as we're not about to break it
                        //just check both lines, i don't care
                        string combinedFinalLines = allFinalLines;
                        string combinedFinalsNS = NormalizeWhiteSpace(combinedFinalLines, true);
                        if(!combinedFinalsNS.Contains("}") || !combinedFinalsNS.Contains("]"))
                        {
                            linesWithErrors.Add(i + 1 + "+:clmClose");

                        } else
                        {
                            string anomaliesInFinalLines = combinedFinalsNS.Replace("]}", "");
                            if (anomaliesInFinalLines.Length > 0)
                            {
                                linesWithErrors.Add(i + 1 + "+:clmClsUnexpCh_");
                            }
                            
                        }
                        
                    } else
                    {
                        string finalLine = allFinalLines;
                        if (!finalLine.Contains("}") || !finalLine.Contains("]"))
                        {
                            linesWithErrors.Add(i + 1 + "+:clmClose");

                        }
                        else
                        {
                            string anomaliesInFinalLines = NormalizeWhiteSpace(finalLine, true);
                            anomaliesInFinalLines = finalLine.Replace("]}", "");
                            if (anomaliesInFinalLines.Length > 0)
                            {
                                linesWithErrors.Add(i + 1 + ":clmClsUnexpCh_");
                            }
                        }
                    }
                } else
                {
                    //we got out but cant find a ]??
                    linesWithErrors.Add(i + 1 + "+:forgotClosers");
                }
            } else
            {
                //we hit the end without finding a ]
                linesWithErrors.Add(i + 1 + "+:noClmClose");

            }
            #endregion FinalLines;

            #region PostFinalLines
            #endregion PostFinalLines

            if (linesWithErrors.Count > 0)
            {
                string[] errorList = linesWithErrors.ToArray();
                return errorList;
            }
            else
            {
                string[] noErrors = { "none" };
                return noErrors;
            }


        }
        
        /// <summary>
        /// The Debugger
        /// </summary>
        /// <param name="fullJson"></param>
        /// <returns></returns>
        public string[] BuggyD(string fullJson)
        {
            List<string> linesWithErrors = new List<string>();

            string noTabs = replaceAllTabs(fullJson);
            string fixedJson = noTabs;
            if(fullJson != noTabs)
            {
                linesWithErrors.Add("...:tabs");
            }

            
            bool fatalErrorEncountered = false;//we use this if we find a fatal error; we keep skipping lines until we get out of the level with the error

            string[] fixedJsonLinesWithEmpties = fixedJson.Split('\n'); //we're going to keep our empty lines, to be sure what line we're on


            string[] fixedJsonLines = fixedJson.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);//it's not gonna have empty entries, damnit
            if (fixedJsonLines.Length < 10) { string[] tooshort = { "2short" }; return tooshort; }

            int bmCounter = 0; int mmCounter = 0;//we will use these to make sure we don't have two MainMusics, or two BossMusics, per level

            int currPlaceInExpctdEntry = 0; //current place in expected entry
            int i = 0;
            int threshold = 0;//threshold used to prevent infinite loop
            while (i < fixedJsonLines.Length-1)
            {
                //use threshold to prevent infinite loop
                #region PreventInfiniteLoop
                threshold++; //threshold is used just to verify this doesn't get stuck in an infinite loop
                if (threshold > 300)
                {
                    //string[] tooLong = { "2long" };
                    linesWithErrors.Add("(2long)");
                    return linesWithErrors.ToArray();
                }
                #endregion PreventInfiniteLoop

                #region DebugLines
                string line_unaltered = fixedJsonLines[i]; //get the line we're checking
                line_unaltered = line_unaltered.Replace("\r", "");//these things keep fucking me
                string line_nospaces = NormalizeWhiteSpace(fixedJsonLines[i], true); //gives us us the individual line in the JSON, with no spaces whatsoever
                
                if (line_nospaces == null || line_nospaces == "")
                {

                    linesWithErrors.Add(i + 1 + ":empty");
                    i++;
                    if (i >= 2) currPlaceInExpctdEntry++;
                    continue;
                }

                //if a fatal error has occured, keep skipping lines until we get to a line with "]" or "LevelName":
                #region FatalErrorHandler
                if (fatalErrorEncountered)
                {
                    bmCounter = 0; mmCounter = 0; //first, reset these

                    if (line_nospaces.Contains("LevelName") || line_nospaces.Contains("]"))
                    {
                        if (line_nospaces.Contains("LevelName"))
                        {
                            if (fixedJsonLines[i - 1].Contains("{"))
                            {
                                //this line has "LevelName", and the line before it has "{"
                                i--;//we'll go back and start verifying again on the line before this
                                currPlaceInExpctdEntry = 0;
                                linesWithErrors.Add(i + 1 + ":BackOnTrack"); //this isn't an error, but we'll check for it, and if we have it, we're out of the fatal error
                            } else
                            {
                                //this line has "LevelName" but we don't have a "{" in the line before it
                                linesWithErrors.Add(i + 1 + ":BackOnTrack"); //this isn't an error, but we'll check for it, and if we have it, we're out of the fatal error
                                currPlaceInExpctdEntry = 1;
                            }
                        } else if (line_nospaces.Contains("]")) {
                            currPlaceInExpctdEntry = expectedFields.Length; //when we reset this, it will recheck this line and activate what we do if we see the closing ] bracket
                            linesWithErrors.Add(i + 1 + ":BackOnTrack");
                        }
                        fatalErrorEncountered = false;
                        continue;
                    } else
                    {
                        i++;
                        continue;
                    }

                }
                #endregion FatalErrorHandler


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
                                linesWithErrors.Add("1:unexpChars_" + unexpChars);
                            }
                        } else
                        {
                            //there's no { in the first line
                            //label is missing, we need to look around for something identifiable
                            //technically, they could have the first line be-> { "customLevelMusic":[ and still be correct
                            string[] firstLineError = { "Error reading Json" };
                            if (fixedJsonLines.Length < 2) return firstLineError; //catch-all in case JSON is very short

                            if (fixedJsonLines[1].Contains("{"))
                            {
                                //our { is on the next line

                                linesWithErrors.Add("1:grbgLine");


                            } else if (fixedJsonLines[1].Contains("\"customLevelMusic\""))
                            {
                                //we can't find {, but customLevelMusic is on next line

                                linesWithErrors.Add("1:forgot1stLn");
                            }
                        }
                        i++; continue;
                    } else
                    {

                        //second line, we want "customLevelMusic" : [
                        string firstLineNoSpaces = NormalizeWhiteSpace(fixedJsonLines[0], true);

                        if (line_nospaces != "\"customLevelMusic\":[")
                        {
                            string[] secondLineError = { "Error reading Json" };
                            if (fixedJsonLines.Length < 3) return secondLineError;

                            if (fixedJsonLines[2].Contains("{"))
                            {
                                //our next line contains the next sequence, we forgot the CLM opener
                                secondLineError[0] = "2:forgotCLM";
                                return secondLineError;
                            } else if (line_nospaces.Contains("{") && fixedJsonLines[2].Contains("\"customLevelMusic\":["))
                            {
                                //this line has a {, which our first line has, and our next line has the customLevelMusic thing
                                secondLineError[0] = "2:dupe";

                            }
                            else if (line_nospaces.Contains("{") && firstLineNoSpaces.Contains("{\"customLevelMusic\":["))
                            {
                                //the user seems to have combined { and "customLevelMusic" : [ onto one line. that works, but we don't want that
                                secondLineError[0] = "2:forgotClmFormat"; //we want it to add another
                            } else
                            {
                                linesWithErrors.Add("2:clmF"); //customLevelMusic opening line incorrect format
                            }


                        }
                        i++; continue;
                    }

                } else if (i >= 2)
                {
                    if (i == 2)
                    {
                        //we're on our first line out of the openers, we should verify our placement

                        if (!line_unaltered.Contains(expectedFields[currPlaceInExpctdEntry]))
                        {
                            //the first line did not have a {
                            string[] firstTwoLines = { fixedJsonLines[i], fixedJsonLines[i + 1] };
                            int backOnTrack = verifyWTFsGoingOnFirstLine(firstTwoLines);
                            if (backOnTrack == -1)
                            {
                                string[] fatalError = { (i + 1) + ":fatality" };
                                fatalErrorEncountered = true;
                                return fatalError;
                            } else
                            {
                                currPlaceInExpctdEntry = backOnTrack;
                                linesWithErrors.Add((i) + ":forgotClmFormat"); //we don't want i-1
                                continue;
                            }

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
                    string nxtLn = fixedJsonLines[i+1];

                    string lineErrors = getLineErrors(thisLn, nxtLn, currPlaceInExpctdEntry);

                    if (lineErrors.Contains("fatal"))
                    {
                        fatalErrorEncountered = true;
                        linesWithErrors.Add(i + 1 + ":" + lineErrors);
                        i++;
                        continue;
                    }

                    if (lineErrors.Contains("dupe"))
                    {
                        currPlaceInExpctdEntry--;
                        //if we can see there was a duplicate entry in our line, we need to set our currPlaceInExpectedEntry back;
                        //we'll be back on track after the rest of the for loop runs
                        //this is getting called if our line was blank, in which case it catches up with where it wants to be, but throws a false error

                    }

                    if (lineErrors.Contains("forgot"))
                    {
                        currPlaceInExpctdEntry++;
                    }

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
                    if(bmCounter > 1)
                    {
                        lineErrors += ("(bmdup)");
                        bmCounter = 0;
                    }
                    if (mmCounter > 1)
                    {
                        lineErrors += ("(mmdup)");
                        mmCounter = 0;
                    }
                    #endregion Look For Boss/MainMusic Doubles

                    if (lineErrors.Length > 0)
                    {
                        linesWithErrors.Add(i + 1 + ":" + lineErrors); //i+1 because we don't start on line 0

                    }

                    


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

                        }
                        else
                        {
                            string anomaliesInFinalLines = allFinalLines;
                            anomaliesInFinalLines = NormalizeWhiteSpace(anomaliesInFinalLines, true);
                            anomaliesInFinalLines = anomaliesInFinalLines.Replace("]}", "");
                            if (anomaliesInFinalLines.Length > 0)
                            {
                                linesWithErrors.Add(i + 1 + "+:unexpCharsA_"+anomaliesInFinalLines);
                            }
                        }

                    }
                    else
                    {
                        string finalLine = allFinalLines;
                        if (!finalLine.Contains("}") || !finalLine.Contains("]"))
                        {
                            linesWithErrors.Add(i + 1 + "+:clmClose");

                        }
                        else
                        {
                            string anomaliesInFinalLine = finalLine.Replace("]}", "");
                            if (anomaliesInFinalLine.Length > 0)
                            {
                                linesWithErrors.Add(i + 1 + "+:unexpCharsB_");
                            }
                            
                        }
                    }
                }
                else
                {
                    //we got out but cant find a ]??
                    linesWithErrors.Add(i + 1 + "+:forgotClosers");
                }
            }
            else
            {
                //we hit the end without finding a ]
                linesWithErrors.Add(i + 1 + "+:noClmClose");
                
            }

            if (linesWithErrors.Count > 0)
            {
                string[] errorList = linesWithErrors.ToArray();
                return errorList;
            }
            else
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
                    return "bunkLine";
                } else if (whatsGoingOn == "dupe")
                {
                    int prevIndx = indexOfLabelWeWant - 1; if (prevIndx < 0) prevIndx = expectedFields.Length - 1;
                    return "dupe_" + expectedFields[prevIndx];
                }
                else if (whatsGoingOn == "labelInvalid")
                {
                    return "labelInvalid_" + expectedFields[indexOfLabelWeWant];
                }
                else if (whatsGoingOn == "forgotitem")
                {
                    //when running through the function to translate these error codes, we need to add a line here
                    //(and add numbers to our other error Line nums)
                    return "forgot_" + expectedFields[indexOfLabelWeWant];
                } else if (whatsGoingOn == "fatality")
                {
                    //   x_x

                    return "fatality";
                }


                // }
            }


        EndLabelCheck:

            if (endingWeWant.Length == 1)
            {
                if (finalCharOnLine != endingWeWant)
                {
                    errorsOnLine.Add("unexpEnd-Wanted_\"" + endingWeWant + "\"");
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
                        /* Turns out, Main Music doesn't have to be first within the two!
                         
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
                        string unexpCharacters = lineNS.Replace("}", "");
                        if (lineNS != "}")
                        {
                            errorsOnLine.Add("unexpCharsC_" + unexpCharacters);
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
                    }
                    else
                    {
                        //we don't have another level on the next line
                        //just make sure we didn't have anything else in this line
                        string unexpCharacters = lineNS.Replace("}", "");
                        if (lineNS != "}")
                        {
                            errorsOnLine.Add("unexpCharsD_" + unexpCharacters);
                        }
                    }


                }
            }
            // ↑Endings, ↑↑ LabelCheck, ↓ Format Errors

            string[] LineFormatErrors = getFormatErrors(line, indexOfLabelWeWant); //get the format errors
            errorsOnLine.AddRange(LineFormatErrors); //add them to our line errors


            string errorReportString = "";
            foreach (string error in errorsOnLine)
            {
                errorReportString += "(" + error + ")"; //for each error, we're making a string that says "1:(errorCode)(errorC1)(errorCd5)
            }


            return errorReportString;



        }

        int[] instancesOfLevelInJson = new int[9]; //we'll use this to detect if we have more than one instance of a level in the JSON; 9 because of tutorial level

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
                string unexpCharacters = lineNS.Replace("{", "");
                if (unexpCharacters.Length > 0)
                {
                    formatErrors.Add("unexpCharsA_" + unexpCharacters);
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
                }

            }
            else
            {
                if (lineColonSplit.Length == 1)
                {
                    formatErrors.Add("nocolon");
                }
                else
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


            if (splitInfo.Length == 3) {
                //we want 3 in splitInfo if we're on indexOfLabel 7, meaning we're looking for a bankpath
                if (indexOfLabel == 8)
                {
                    valueStr = splitInfo[1] + ":" + splitInfo[2];
                } else
                {
                    lineFormatErrors.Add("2manycol"); //too many colons in value
                    return lineFormatErrors.ToArray();
                }
            } else
            {
                //this code won't run if splitInfo length was 1; splitInfo.length must be 2
                if (indexOfLabel == 8)
                {
                    lineFormatErrors.Add("bpnocol"); //bank path no colon
                    return lineFormatErrors.ToArray();
                }
            }


            valueStr.TrimEnd();//get rid of all whitespace on the right of value/value's comma
            if (valueStr.Substring(valueStr.Length - 1, 1) == ",")
            {
                valueStr = valueStr.Substring(0, valueStr.Length - 1); //if we had a comma, it's gone now—we already checked for endings
            }
            if (valueStr.Contains(",")) lineFormatErrors.Add("tmcom"); //we just got rid of the only comma that should be there

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
                }
            }
            else
            {
                string unexpCharsInLabel = labelNS.Replace(expectedFields[indexOfLabel], "");
                if (unexpCharsInLabel.Length > 0)
                {
                    lineFormatErrors.Add("unexpChLC_" + unexpCharsInLabel);
                }
            }


            if (indexOfLabel == 6 || indexOfLabel == 7 || indexOfLabel == 2) goto ValueNoQuoteCheck;

            ValueWithQuoteCheck:

            if (numberOfQuotesInValue > 2)
            {
                lineFormatErrors.Add("2mqVal");
            }
            else if (numberOfQuotesInValue < 2)
            {
                lineFormatErrors.Add("neqVal");
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

                            break; //break out of J's for loop, not I
                        }
                        if (j == allLevelNames.Length - 1)
                        {
                            //we're at the end, and we didn't find anything
                            lineFormatErrors.Add("LUr(" + valueNS + ")"); //level unrecognized
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
                        return lineFormatErrors.ToArray();

                    }
                    checkEvent = checkEvent.TrimStart('{').TrimEnd('}');

                    if (checkEvent.Length != valueNS.Length - 4 || checkEvent.Length != 36)
                    {
                        //Event string does NOT have { and }, OR it does not have the full 36-digit ID
                        lineFormatErrors.Add("evF2");
                        return lineFormatErrors.ToArray();
                    }
                } else if (indexOfLabel == 8)
                {
                    //we're looking at a bankPath
                    string[] bankFormatErrors = getBankPathFormatErrors(valueNS);
                    lineFormatErrors.AddRange(bankFormatErrors);
                }

            }

            return lineFormatErrors.ToArray();

        // We've stopped if we were expecting quotes

        ValueNoQuoteCheck:
            if (numberOfQuotesInValue > 0)
            {
                lineFormatErrors.Add("unwntdq");
            }
            else
            {
                //we don't have any quotes, which is what we want
                if (indexOfLabel == 2)
                {
                    if (valueNS.Trim() != "{")
                    {
                        lineFormatErrors.Add("unexpChRC_" + labelNS.Replace("{", ""));
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

        private void verifyBankIfNoBankPath()
        {
            //if we see that our entry does not have a bankPath entry, we want to go back and check our "Bank"

        }

        private string[] getBankPathFormatErrors(string bankPathValue)
        {
            string bpVal = bankPathValue;
            List<string> bpFormErrors = new List<string>();
            if (!bpVal.Contains(".bank"))
            {
                bpFormErrors.Add("nobpfile"); //bankpath points to a directory, but needs to include "[Filename].bank"
            }

            if (bpVal.Split('/').Length - 1 != 0)
            {
                //we have / in our bankpath, when we only want \'s
                bpFormErrors.Add("bpws"); //bank path wrong slash
                                          //we'll also replace them so we can continue with the code if that's the only issue
                bpVal.Replace('/', '\\');
            }

            string[] directoriesBtwnDblSlashes = bpVal.Split(new string[] { "\\\\" }, StringSplitOptions.None);
            string nextLine_chkIntegrity = bpVal.Replace("\\\\", "\\");
            string[] directoriesBtwnSnglSlashes = nextLine_chkIntegrity.Split(new string[] { "\\" }, StringSplitOptions.None);

            //we're going to take value for bankPath and analyze the formating, and verify a file exists
            int indexOfBankPathInfo = nextLine_chkIntegrity.IndexOf(":\\");
            if (indexOfBankPathInfo == 0) { bpFormErrors.Add("nobpdir"); return bpFormErrors.ToArray(); } //if our line looks like bankPath: ":\\Dir\\Dir2\\ and they forgot the drive letter
            indexOfBankPathInfo -= 1; //this should be the index of C:\, B:\ X:\ Etc
            if (indexOfBankPathInfo < 0)
            {
                bpFormErrors.Add("cvBP"); //can't verify bankpath value's format
            }
            string bankPathInfo = nextLine_chkIntegrity.Substring(indexOfBankPathInfo);
            bankPathInfo = shaveSurroundingQuotesAndSpaces(bankPathInfo);


            if (bpVal.Contains("\\\\\\") || bpVal.Contains("\\\\\\\\"))
            {
                bpFormErrors.Add("2mSl"); //too many slashes
            }
            else if (directoriesBtwnDblSlashes.Length != directoriesBtwnSnglSlashes.Length)
            {
                //if the # of our directories with double slashes does not match our # of directories after converting to single slashes, it means we don't have enough slashes somewhere er something
                bpFormErrors.Add("bPF"); //bankpath formatting
            }
            else if (!verifyFileExists(bankPathInfo))
            {
                //bpFormErrors.Add("bpFNF"); //bankpath file not found; this should be a potentially-major error (potentially major = problem will arise later, but program will still work)
                //we're no longer doing this
            }

            return bpFormErrors.ToArray();
        }

        //spaces before "customLevelMusic", and its closing ], is 4

        string[] expectedEndings = { "{", ",", "{", ",", ",", ",", ",", ",or ", "bnkp", "}or,", "}or," };
        //                            L   LN   MM   Ba   Ev   LH   Ofs   BPM*  BnkPth   MMcBMc  Lc
        //                                      ^-----<----------<------------<--------' if(,)

        //                          0          1                       2                    3            4                   5                        6              7            8          9    10
        string[] expectedFields = { "{", "\"LevelName\"", "\"MainMusic\"|\"BossMusic\"", "\"Bank\"", "\"Event\"", "\"LowHealthBeatEvent\"", "\"BeatInputOffset\"", "\"BPM\"", "\"bankPath\"", "}", "}" };

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


        //this function draws the LineNum string onto our DataGridView box using the RowPostPaint event
        private void dgJsonEditor_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.GrayText, headerBounds, centerFormat);
        }

        private void Debug_TesterBoxValue()
        {
            string originalJson = ((Form1)MyParentForm).TesterBoxValue;
            string[] errors = BuggyD(originalJson);
            fillErrorList(errors); //fills our left ListViewBox, which tells us what errors we have and what lines they're on
            //fillJsonLines(originalJson);//fills the right ListView, which has our JSON in its entirety, NOT editable line-by-line
            //setEditorLinesBGColors(errors); //sets the bg color of each line to red if it has an error
            fillJsonDataGrid(replaceAllTabs(originalJson));//fills the right DataGrid, which has our JSON in its entirety, editable line-by-line
            setDGEditorLinesBGColrs(errors);//sets the bg color of each line to red if it has an error
            verifyAllDuplicates();
            ReplaceAllErrorsWithDescriptions(); //this also finds our Crit Errors and adds to numberOfCritErrors
            setALLProperSpacePrefixes();//sets the spaces indentations for the json. we only want to do this for initial load
        }

        private void Debug_PastedSong(string textInPasteBox)
        {
            string songJson = textInPasteBox;
            string[] errors = BuggyD(songJson);
            fillErrorList(errors); //fills our left ListViewBox, which tells us what errors we have and what lines they're on
            fillJsonDataGrid(replaceAllTabs(songJson));//fills the right DataGrid, which has our JSON in its entirety, editable line-by-line
            setDGEditorLinesBGColrs(errors);//sets the bg color of each line to red if it has an error
            verifyAllDuplicates();
            ReplaceAllErrorsWithDescriptions();//this also finds our Crit Errors and adds to numberOfCritErrors
            setALLProperSpacePrefixes();//sets the spaces indentations for the json. we only want to do this for initial load(we do it individually afterwards)
        }

        private void Debug_GivenSong(string songName, string pathOfJson)
        {
            songTitleLabel.Text = "Analyzing Song: " + songName;
            string songPath = pathOfJson;
            string displayPath = pathShortener(songPath, 50, 4);
            displayPath = displayPath.Substring(0, 1).ToUpper() + displayPath.Substring(1);
            pathLabel.Text = displayPath;
            string songJson = getCustomsongsJson(new string[] { songName, pathOfJson }, false, true);
            string[] errors = BuggyD(songJson);
            fillErrorList(errors); //fills our left ListViewBox, which tells us what errors we have and what lines they're on
            fillJsonDataGrid(replaceAllTabs(songJson));//fills the right DataGrid, which has our JSON in its entirety, editable line-by-line
            setDGEditorLinesBGColrs(errors);//sets the bg color of each line to red if it has an error
            verifyAllDuplicates();
            ReplaceAllErrorsWithDescriptions();//this also finds our Crit Errors and adds to numberOfCritErrors
            setALLProperSpacePrefixes();//sets the spaces indentations for the json. we only want to do this for initial load
            enableSaveButton(numberOfCritErrors, numberOfPotntlErrors);
        }



        private string TurnEditingLinesIntoString()
        {
            string fullJson = "";
            foreach (var lt in JsonLinesBind)
            {
                fullJson += lt.ListItem.ToString() + "\r\n";
            }
            return fullJson;
        }

        private void CopyAllLinesBtn_Click(object sender, EventArgs e)
        {
            CopyAllLinesToClipboard();
        }

        private void CopyAllLinesToClipboard()
        {
            string fullJson = TurnEditingLinesIntoString();
            Clipboard.SetText(fullJson);
            MessageBox.Show("All lines copied to clipboard!");
        }

        string[][] SongsToFix = new string[0][];
        private void Debug_Mandatory()
        {
            SongsToFix = ((Form1)MyParentForm).SongsRequireMandatoryDebug;
            int NumOfSongsToFix = SongsToFix.Count();

            if (NumOfSongsToFix > 0)
            {
                
                songTitleLabel.Text = "Analyzing Song: " + SongsToFix[0][0];
                string songPath = mDir.ToString() + SongsToFix[0][1] + "\\customsongs.json";
                pathLabel.Text = pathShortener(songPath, 50, 4);


                string firstJson = getCustomsongsJson(SongsToFix[0]);
                string[] errors = BuggyD(firstJson);
                fillErrorList(errors); //fills our left ListViewBox, which tells us what errors we have and what lines they're on
                fillJsonDataGrid(replaceAllTabs(firstJson));//fills the right DataGrid, which has our JSON in its entirety, editable line-by-line
                setDGEditorLinesBGColrs(errors);//sets the bg color of each line to red if it has an error
                verifyAllDuplicates();
                ReplaceAllErrorsWithDescriptions();//this also finds our Crit Errors and adds to numberOfCritErrors
                setALLProperSpacePrefixes();//sets the spaces indentations for the json. we only want to do this for initial load
                enableSaveButton(numberOfCritErrors, numberOfPotntlErrors);
            } else
            {
                //MessageBox.Show("No errors found");
                //we didn't have any songs to be told to fix. Does the user want to bring this up for some reason?
            }

        }

        DirectoryInfo mDir;
        DirectoryInfo gDir;
        private void getModAndGameDir()
        {
            mDir = ((Form1)MyParentForm).di;
            gDir = ((Form1)MyParentForm).gameDir;
        }


        private string getCustomsongsJson(string[] songInfo, bool noLineReturns = false, bool absolutePath = false)
        {

            string pathToJson = "";

            if (absolutePath) { pathToJson = songInfo[1]; goto SkipPathCreator; }

            if(songInfo[0] == "(Game)")
            {
                pathToJson = gDir.ToString() + "\\customsongs.json";
            } else
            {
                pathToJson = mDir.ToString() + songInfo[1] + "\\customsongs.json";
            }
           
            SkipPathCreator:
            
            if (!File.Exists(pathToJson.ToString())) return "-1";

            using (StreamReader sr = File.OpenText(@pathToJson))
            {
                string s = "";

                string fullText = sr.ReadToEnd();
                if (noLineReturns)
                {
                    string trimmedLine = NormalizeWhiteSpace(fullText);
                    s = trimmedLine;
                }
                else
                {
                    s = fullText;
                }

                return s;
            }




        }

        string debugSongPath = null;
        private void debug_DropDownSelect()
        {
            string sName = ((ListItem)debugSongSelectCombo.SelectedItem).Name;
            string sPath = ((ListItem)debugSongSelectCombo.SelectedItem).Path;
            debugSongPath = sPath; //we'll send this back to Form1 if we save anything



            songTitleLabel.Text = "Analyzing Song: " + sName; //Debug_GivenSong already does this, why do we do it again
            string songPath = sPath;
            pathLabel.Text = pathShortener(songPath, 50, 4);

            string[] sInfo = { sName, sPath};
            string firstJson = getCustomsongsJson(sInfo);

            
            debugPastebox.Visible = false;
            debugPasteBoxPanel.Visible = false;
            debugSongSelectCombo.Visible = false;
            debugCopyAllLines.Visible = false;
            jsonAnomalyList.Enabled = true;

            suspendSongButton.Visible = true;
            DebugSaveJsonBtn.Visible = true;
            debugSJBtnPanel.Visible = true;

            Debug_GivenSong(sName, sPath);
            GoToFirstError();
        }


        private void debug_PasteBox()
        {
            string pasteboxJson = debugPastebox.Text;
            debugPastebox.Visible = false;
            debugPasteBoxPanel.Visible = false;
            debugSongSelectCombo.Visible = false;
            songTitleLabel.Text = "Analyzing Song: User-pasted info";
            pathLabel.Text = "No file path";
            debugCopyAllLines.Visible = true;
            jsonAnomalyList.Enabled = true;

            Debug_PastedSong(pasteboxJson);
            setJsonAnomTextAfterScan();
            GoToFirstError();
        }
        private void setJsonAnomTextAfterScan()
        {
            if (jsonAnomalyList.Items.Count == 0)
            {
                debug_undrJsonAnomLbl.Text = "No formatting errors or anomalies can be found in pasted customsongs.json.";
            }
            else
            {
                debug_undrJsonAnomLbl.Text = "Formatting errors and/or anomalies found in pasted customsongs.json.";
            }
        }

        private void debuggerPasteBox(object sender, EventArgs e)
        {
            debug_PasteBox();
        }

        ListItem[] modsList;
        private void getModsListFromMain()
        {
            modsList = ((Form1)MyParentForm).getSongsList().ToArray();
        }

        private void fillComboBoxWithCustomSongs()
        {
            debugSongSelectCombo.Items.Clear();

            getModsListFromMain();
            if(modsList.Length == 0)
            {
                debugSongSelectCombo.Text = "No custom songs found.";
                debugSongSelectCombo.Enabled = false;
            }

            /* When we did this, Debug was changing the name of this "Current customsongs.json" to the name we were trying to put here. Forget it..
            if(modsList[0].Name == "Current customsongs.json")
            {
                modsList[0].Name = "Game's current customsongs.json";
            }*/

            debugSongSelectCombo.Items.AddRange(modsList);
            


            /* This doesn't work, because it doesn't update
            foreach (var endpoint in ConfigDataDaddy.Customsongs.CustomsongsList)
            {
                if (endpoint.Name == "(game)")
                    debugSongSelectCombo.Items.Insert(0, new ListItem { Name = "Game's current customsongs.json", Path = gDir + "\\customsongs.json" });
                else
                    debugSongSelectCombo.Items.Add(new ListItem { Name = endpoint.Name, Path = mDir.ToString() + endpoint.SongInfo.Path + "\\customsongs.json" });
            }*/
        }


        private void DebugFormLoad(object sender, EventArgs e)
        {
            getModAndGameDir();

            if (summoner == "user")
            {
                //the user summoned us
                suspendSongButton.Visible = false;
                susSongExplLabel.Visible = false;
                DebugSaveJsonBtn.Visible = false;
                debugSJBtnPanel.Visible = false;

                jsonAnomalyList.Enabled = false;
                debugPastebox.Visible = true;
                debugPasteBoxPanel.Visible = true;
                debugSongSelectCombo.Visible = true;
                debugCopyAllLines.Visible = false;

                debug_undrJsonAnomLbl.Text = "Awaiting input. Please select a song or Copy+Paste your customsongs.json into the textbox.";

                fillComboBoxWithCustomSongs();


            }
            else if (summoner.Contains("(.saveAttmpt.)"))
            {
                //Organizer's save attempt summoned us
                suspendSongButton.Visible = false;
                susSongExplLabel.Visible = false;
                DebugSaveJsonBtn.Visible = false;
                debugSJBtnPanel.Visible = false;

                


                string[] sNameSplit = summoner.Split('|');
                debug_AttemptedSave(sNameSplit[1]);

            }
            else if (summoner.Contains("|") && summoner.Contains(":"))
            {
                //we were summoned by a song
                //it looks like this: song:Unstoppable|M:/Path/To/customsongs.json

                suspendSongButton.Visible = true;
                //susSongExplLabel.Visible = true;
                DebugSaveJsonBtn.Visible = true;
                debugSJBtnPanel.Visible = true;

                jsonAnomalyList.Enabled = true;
                debugPastebox.Visible = false;
                debugPasteBoxPanel.Visible = false;
                debugSongSelectCombo.Visible = false;
                debugCopyAllLines.Visible = false;

                string[] sInfo = summoner.Split('|');
                string[] sNameSplit = sInfo[0].Split(':');
                Debug_GivenSong(sNameSplit[1], sInfo[1]);
                debugSongPath = sInfo[1];
                GoToFirstError();


            }





        }

        private void debug_AttemptedSave(string attemptedSave)
        {
            
            debugPastebox.Visible = false;
            debugPasteBoxPanel.Visible = false;
            debugSongSelectCombo.Visible = false;
            songTitleLabel.Text = "Analyzing: Attempted Save";
            pathLabel.Text = "No file path";
            debugCopyAllLines.Visible = true;
            jsonAnomalyList.Enabled = true;

            Debug_PastedSong(attemptedSave);
            setJsonAnomTextAfterScan();
            GoToFirstError();
        }

        private void CheckForNewErrors(int oneBsedLineNum)
        {
            //as of now, this is just used if we removed an entry
            //similar to checkToRemoveFatlErrors, we'll 
            string[] newErrors = debuggyBurst(3, 300, 0);
            fillErrorList(newErrors, false); //this only adds to our list, we need to check to remove duplicate ranges
            string[] ogErrors = getErrorsAlreadyInList();
            setDGEditorLinesBGColrs(newErrors, false, ogErrors);
        }

        private void CheckToRemoveFatalError(string rangeString, int oneBsdLineNum, int indexOfRangeOnAnomalyList)
        {
            //if there's a series of fatal errors in our list, we'll check if our line falls within its range
            //rangeString will look like -> 12-19
            //if this is true, it will trigger us to check the entire fatality block for fixes


            //first check if we're within the range

            if (!rangeString.Contains("–")) return; //this is not a dash, it's Alt+0150
            string[] rangeNums = rangeString.Split('–'); //split our string to get the two numbers
            if (rangeNums.Length != 2) return;
            string rngStart = rangeNums[0]; //get string of start
            string rngEnd = rangeNums[1]; //and of end
            int intRngStart = -1; //declare ints
            int intRngEnd = -1;

            if (Int32.TryParse(rngStart, out int tra))
            {
                intRngStart = Int32.Parse(rngStart); //make sure it's a number, then set our Starting Point
            }
            if (Int32.TryParse(rngEnd, out int lala))
            {
                intRngEnd = Int32.Parse(rngEnd); //set our End
            }
            if (intRngStart == -1 || intRngEnd == -1) return; //we didn't have a number in one of the options

            //if we got this far, we have the information for our range

            if (oneBsdLineNum < intRngStart && oneBsdLineNum > intRngEnd) return; //we're not within the range
            //if we got this far, the line we changed is within our range; RangeStart <= OneBasedLineNumber <= RangeEnd
            //we have the green light to check the range if it's been fixed

            debuggyFatality(intRngStart, intRngEnd, indexOfRangeOnAnomalyList); //indexOfRangeOnAnomalyList is used if we need to update/remove the range





        }

        //used to be deletedLineHadError

        private bool lineHasError(int zbLineIndex)
        {
            //this just looks at our colors and sees if red is red, and says true if red
            if (zbLineIndex >= JsonLinesBind.Count) return true; //returning true stops whatever we were about to do
            int greenAmt = dgJsonEditor.Rows[zbLineIndex].DefaultCellStyle.BackColor.G;
            if(greenAmt > 120)
            {
                return false;
            }
            return true;
        }

        private void justCheckEntireJsonListAgain(int omit)
        {
            //what garbage...
            string[] errors = BuggyD_BindList(omit);
            
            jsonAnomalyList.Items.Clear();
            fillErrorList(errors, false);
            setDGEditorLinesBGColrs(errors, false);
            ReplaceAllErrorsWithDescriptions();
            enableSaveButton(numberOfCritErrors, numberOfPotntlErrors);
        }
        

        private void adjustErrorsListRemoval(int OneBsdRemovedLineNum, bool lineHadError)
        {
            string[] errors = BuggyD_BindList();
            
            jsonAnomalyList.Items.Clear();
            fillErrorList(errors, false);
            //or it's going to see duplicates and not do anything about it because i told it not to. was that stupid?
            setDGEditorLinesBGColrs(errors);

            /*
            //we just removed an entry, and need to check through our error list
            if (lineHadError)
            {
                
                //we either got rid of the error, or there's still an error
                //clearly if we delete the line with an error, the error's gone, but it could be a fatal error where it couldn't read these lines anyways
                var errorListEntries = jsonAnomalyList.Items;
                for (int i = 0; i < errorListEntries.Count; i++)
                {
                    
                    //(OneBsdRemovedLineNum-1) because we're DELETING a line
                    if (errorListEntries[i].SubItems[0].Text.ToString() == (OneBsdRemovedLineNum-1).ToString())
                    {
                        string[] errors = BuggyD_BindList();
                        MessageBox.Show("Errors: " + errors[0]);
                        jsonAnomalyList.Items.Clear();
                        fillErrorList(errors, false);
                        //or it's going to see duplicates and not do anything about it because i told it not to. was that stupid?
                        setDGEditorLinesBGColrs(errors);

                        return; //why return?
                    }
                    else if (errorListEntries[i].SubItems[0].Text.ToString().Contains("–"))
                    {
                        //our error entry has a range. CheckToRemoveFatal will see if it's in the range, then act accordingly
                        //(the range should have already been updated via HandleLineNums)
                        //– is not a -, it's Alt+0150
                        CheckToRemoveFatalError(errorListEntries[i].SubItems[0].Text.ToString(), OneBsdRemovedLineNum, i);
                        //we're taking i to be able up update the range in the JsonAnomalyList if need be
                    }

                }


            } else
            {
                //line didn't have an error
                //we either removed an optional entry (only bankPath), or we're deleting valid info we don't want
                if(OneBsdRemovedLineNum < 2) { return; }
                string lineBefore = JsonLinesBind[OneBsdRemovedLineNum - 2].ListItem.ToString();
                string lineBeforeNS = NormalizeWhiteSpace(lineBefore, true);
                if (lineBefore.Contains("\"BPM:\"")){
                    //our previous line was "BPM", we don't need this line, there's no error
                } else
                {
                    //our previous line was not BPM, and there was no error, we just deleted precious info
                    string[] errors = BuggyD_BindList();
                    jsonAnomalyList.Items.Clear();
                    fillErrorList(errors, false);
                    setDGEditorLinesBGColrs(errors);
                    //CheckForNewErrors(OneBsdRemovedLineNum); //this is hopefully going to run once and find nothing; or it's going to see fatal errors
                }

                

            }
            */

        }

        private void adjustErrorsList(string newErrors, int changedLineNum)
        {
            //changedLineNum is one-based
            var errorListEntries = jsonAnomalyList.Items;

            for (int i = 0; i < errorListEntries.Count; i++)
            {
                if (errorListEntries[i].SubItems[0].Text.ToString() == changedLineNum.ToString())
                {
                    //we already had an entry for this line, which we found! Update it!
                    if (newErrors == "" || newErrors == null)
                    {
                        //we got rid of any errors we had, we'll remove this from our error list
                        jsonAnomalyList.Items[i].Remove();
                        //if we just cleared the errors on a line, we need to set its BG to not be red
                        setJsonEditorLineBG(changedLineNum);//sets our BG to the white/offwhite tones; changedLineNum is one-based
                    }
                    else
                    {
                        //we had errors here before, but we still have errors; update the new errors
                        jsonAnomalyList.Items[i].SubItems[1].Text = newErrors;

                    }

                    return;
                } else if (errorListEntries[i].SubItems[0].Text.ToString().Contains("–"))
                {
                    //– is not a -, it's Alt+0150
                    CheckToRemoveFatalError(errorListEntries[i].SubItems[0].Text.ToString(), changedLineNum, i);
                    //we're taking i to be able up update the range in the JsonAnomalyList if need be
                }

            }

            //if we got this far, it's possible we're trying to add something not in the list, but it still might be blank
            if (newErrors == "" || newErrors == null)
            {
                return;
            }

            //if we got this far, we did not add adjust anything yet; we have a new error we need to add
            string[] row = { changedLineNum.ToString(), newErrors };
            ListViewItem newItem = new ListViewItem(row);
            jsonAnomalyList.Items.Add(newItem);
            setJsonEditorLineBG(changedLineNum, true);

            verifyAllDuplicates(); //screw it, just do this when we're done
        }

        private void setJsonEditorLineBG(int LineNumOneBased, bool hasError = false)
        {
            if (!hasError)
            {
                if (LineNumOneBased % 2 == 1)
                {
                    dgJsonEditor.Rows[LineNumOneBased - 1].DefaultCellStyle.BackColor = JsonEditorColor1;
                    dgJsonEditor.Rows[LineNumOneBased - 1].HeaderCell.Style.BackColor = JsonEditorColor1;
                }
                else
                {
                    dgJsonEditor.Rows[LineNumOneBased - 1].DefaultCellStyle.BackColor = JsonEditorColor2;
                    dgJsonEditor.Rows[LineNumOneBased - 1].HeaderCell.Style.BackColor = JsonEditorColor2;
                }
                dgJsonEditor.Rows[LineNumOneBased - 1].HeaderCell.Style.SelectionBackColor = JsonEditorNrmSlctd;
                dgJsonEditor.Rows[LineNumOneBased - 1].DefaultCellStyle.SelectionBackColor = JsonEditorNrmSlctd;
            } else
            {
                if (LineNumOneBased % 2 == 1)
                {
                    dgJsonEditor.Rows[LineNumOneBased - 1].DefaultCellStyle.BackColor = JsonEditorErrC1;
                    //dgJsonEditor.Rows[LineNumOneBased - 1].HeaderCell.Style.BackColor = JsonEditorColor1; We don't need to do this, they were already set
                }
                else
                {
                    dgJsonEditor.Rows[LineNumOneBased - 1].DefaultCellStyle.BackColor = JsonEditorErrC2;
                    //dgJsonEditor.Rows[LineNumOneBased - 1].HeaderCell.Style.BackColor = JsonEditorColor2;
                }
                dgJsonEditor.Rows[LineNumOneBased - 1].DefaultCellStyle.SelectionBackColor = JsonEditorErrSlctd;

                //dgJsonEditor.Rows[LineNumOneBased - 1].HeaderCell.Style.BackColor = JsonEditorErrC1;
            }

        }

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
                } else if (curLineStr.Contains(nextExpFld) || nextLineStr.Contains("]"))
                {
                    return "forgotitem";
                } else if (nextLineStr.Contains(nextExpFld))
                {
                    return "labelInvalid";
                } else
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
                } else
                {
                    return "dupe";
                }

            } else if (nextLineStr.Contains(expectedFields[seqPlaceMissing]))
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

        private int getPlacementInSequence(string newLine, int lineNum)
        {
            //this is CheckEditorLine, but just returns our sequence
            //lineNum wants zerobased index
            int placementInField = -1;

            string lineLabel = "";
            string lineValue = "";

            string prevLine = "";
            string nextLine = "";
            if (lineNum - 1 > 0)
            {
                prevLine = JsonLinesBind[lineNum - 1].ListItem.ToString(); //making sure this won't crash if we're on the first line
            }
            if (lineNum + 1 < JsonLinesBind.Count)
            {
                nextLine = JsonLinesBind[lineNum + 1].ListItem.ToString(); //making sure this won't crash if we're on the last line
            }

            string[] newLineSplit = newLine.Split(':');
            if (newLineSplit.Length == 1)
            {
                //there's no colon in our line, possibly supposed to be { or }
                //if it's {, there should absolutely be "LevelName"
                //if it's }, it can have BPM or bankPath above it, OR it can have another } above it, where { would be underneath it



                if (newLine.Contains("{"))
                {
                    if (nextLine.Contains("LevelName"))
                    {
                        //found line
                        placementInField = 0;
                    }
                }
                else if (newLine.Contains("}"))
                {

                    string prevLineNS = NormalizeWhiteSpace(prevLine, true);
                    if (prevLineNS.Contains("\"BPM\":") || prevLineNS.Contains("\"bankPath\":"))
                    {
                        //previous line had BPM or bankPath, found position
                        placementInField = 9;
                    }
                    else if (prevLine.Contains("}") &&
                      (nextLine.Contains("{") || (nextLine.Contains("]"))))
                    {
                        //we're surrounded by a music-closing }, and either a Level-opening { OR the end of the JSON
                        placementInField = 10;
                    }
                }

            }
            else
            {
                //we have a colon!
                //there might be more than 1, but we'll handle that with the lineErrors
                //strip the label of spaces, and the value of spaces and possible comma
                //we didn't need to that, we only need the label
                lineLabel = newLineSplit[0];
                lineLabel = lineLabel.Trim();
                lineValue = newLineSplit[1];
                lineValue = lineValue.Trim();
                if (lineValue.Last() == ',')
                {
                    lineValue = lineValue.Substring(0, lineValue.Length - 1);
                }

            }


            //if we already found our placement our field sequence, skip the for loop
            if (placementInField != -1)
            {
                goto PlacementIdentified;
            }


            for (int i = 0; i < identifiableFields.Length; i++)
            {
                if (newLine.Contains(identifiableFields[i]))
                {

                    placementInField = corrPlaceOfIdableFields[i];
                    break;
                }
            }

        PlacementIdentified:
            return placementInField;

        }



        int[] corrPlaceOfIdableFields = { 1, 2, 2, 3, 4, 5, 6, 7, 8 }; //correlating Placements Of Identifiable Fields
        string[] identifiableFields = { "\"LevelName\"", "\"MainMusic\"", "\"BossMusic\"", "\"Bank\"", "\"Event\"", "\"LowHealthBeatEvent\"", "\"BeatInputOffset\"", "\"BPM\"", "\"bankPath\"" };
        private string CheckEditorLine(string newLine, int lineNum)
        {
            //check what's below us
            //if it contains a { or a }, and not both, check what's above us
            //check if it's fixed

            int placementInField = -1;

            /* Were going to do the colon instead
            string[] newLineSplit = newLine.Split('\"');
            //if we have 1, there's no quotes; 2, only one quotes; 3 one thing has quotes around it; 4 = 3 quotes; 5 means 2 things have quotes around them
            if(newLineSplit.Length == 1)
            {
                //there's no quotes in our line, possibly supposed to be { or }
            } else if(newLineSplit.Length == 3)
            {
                //we only have one SET of quotes in our line

            } else if (newLineSplit.Length == 5)
            {

            }*/

            string lineLabel = "";
            string lineValue = "";

            string prevLine = "";
            string nextLine = "";
            if (lineNum - 1 > 0)
            {
                if (JsonLinesBind[lineNum - 1].ListItem == null || JsonLinesBind[lineNum - 1].ListItem.ToString() == "") return "Cannot handle blank lines(A)";
                prevLine = JsonLinesBind[lineNum - 1].ListItem.ToString(); //making sure this won't crash if we're on the first line
            }
            if (lineNum + 1 < JsonLinesBind.Count)
            {
                if (JsonLinesBind[lineNum + 1].ListItem == null || JsonLinesBind[lineNum + 1].ListItem.ToString() == "") return "Cannot handle blank lines(B)";
                nextLine = JsonLinesBind[lineNum + 1].ListItem.ToString(); //making sure this won't crash if we're on the last line
            }

            /* :(
            //we'll do this to actually find the real next line
            if (nextLine == "" || nextLine == null)
            {
                int j = lineNum;
                while (j < JsonLinesBind.Count)
                {
                    nextLine = JsonLinesBind[lineNum + 1].ListItem.ToString();
                    if (nextLine != "" || nextLine != null)
                    {
                        j += 500;
                    }
                    j++;
                }
            }*/

            string[] newLineSplit = newLine.Split(':');
            if (newLineSplit.Length == 1)
            {
                //there's no colon in our line, possibly supposed to be { or }
                //if it's {, there should absolutely be "LevelName"
                //if it's }, it can have BPM or bankPath above it, OR it can have another } above it, where { would be underneath it



                if (newLine.Contains("{"))
                {
                    if (nextLine.Contains("LevelName"))
                    {
                        //found line
                        placementInField = 0;
                    }
                }
                else if (newLine.Contains("}"))
                {

                    string prevLineNS = NormalizeWhiteSpace(prevLine, true);
                    if (prevLineNS.Contains("\"BPM\":") || prevLineNS.Contains("\"bankPath\":"))
                    {
                        //previous line had BPM or bankPath, found position
                        placementInField = 9;
                    }
                    else if (prevLine.Contains("}") &&
                      (nextLine.Contains("{") || (nextLine.Contains("]"))))
                    {
                        //we're surrounded by a music-closing }, and either a Level-opening { OR the end of the JSON
                        placementInField = 10;
                    }
                }

            }
            else
            {
                //we have a colon!
                //there might be more than 1, but we'll handle that with the lineErrors
                //strip the label of spaces, and the value of spaces and possible comma
                //we didn't need to that, we only need the label
                lineLabel = newLineSplit[0];
                lineLabel = lineLabel.Trim();
                lineValue = newLineSplit[1];
                lineValue = lineValue.Trim();
                if (lineValue.Last() == ',')
                {
                    lineValue = lineValue.Substring(0, lineValue.Length - 1);
                }

            }


            //if we already found our placement our field sequence, skip the for loop
            if (placementInField != -1)
            {
                goto PlacementIdentified;
            }


            for (int i = 0; i < identifiableFields.Length; i++)
            {
                if (newLine.Contains(identifiableFields[i]))
                {

                    placementInField = corrPlaceOfIdableFields[i];
                    break;
                }
            }


            if (placementInField == -1)
            {
                //whatever line we just changed in the Json editor, we can't find where we are, return error;
                return "???";
            }

        PlacementIdentified:

            string lineErrors = getLineErrors(newLine, nextLine, placementInField);

            return lineErrors;
        }

        private void GoToFirstError()
        {
            if (jsonAnomalyList.Items.Count == 0) return;

            string firstErrorLineNumStr = jsonAnomalyList.Items[0].SubItems[0].Text;
            if (firstErrorLineNumStr == "..." || firstErrorLineNumStr == "") return;

            if (firstErrorLineNumStr.Contains("+"))
            {
                //ie: 1-10

                firstErrorLineNumStr = firstErrorLineNumStr.Replace("+", "");
            }

            if (firstErrorLineNumStr.Contains("–"))
            {
                //ie: 1-10
                string[] lineNumSplit = firstErrorLineNumStr.Split('–');
                firstErrorLineNumStr = lineNumSplit[0];
            }

            if (Int32.TryParse(firstErrorLineNumStr, out int ihaveaheadache))
            {
                //it's a real number! yay!
                int lineNum = Int32.Parse(firstErrorLineNumStr);
                lineNum -= 1; //why do I need to do this
                jsonAnomalyList.Items[0].Selected = true;
                dgJsonEditor.Rows[lineNum].Selected = true; //this just makes us select the row
                dgJsonEditor.CurrentCell = dgJsonEditor.Rows[lineNum].Cells[0];//this makes us scroll to the selection
            }
        }

        private void GoToJsonLine()
        {
            string lineNumStr = jsonAnomalyList.SelectedItems[0].SubItems[0].Text;

            if (lineNumStr.Contains("+"))
            {
                //ie: 1-10

                lineNumStr = lineNumStr.Replace("+", "");
            }

            if (lineNumStr.Contains("–"))
            {
                //ie: 1-10
                string[] lineNumSplit = lineNumStr.Split('–');
                lineNumStr = lineNumSplit[0];
            }

            if (Int32.TryParse(lineNumStr, out int yo))
            {
                //the last character on the line is a number
                int lineNum = Int32.Parse(lineNumStr);
                lineNum -= 1; //why do I need to do this
                dgJsonEditor.Rows[lineNum].Selected = true; //this just makes us select the row
                dgJsonEditor.CurrentCell = dgJsonEditor.Rows[lineNum].Cells[0];//this makes us scroll to the selection
            }
        }

        private void ErrorListClick(object sender, EventArgs e)
        {
            GoToJsonLine();
        }

        int[] spacesBeforeField = { 8, 12, 12, 16, 16, 16, 16, 16, 16, 12, 8 }; //these are the spaces before Level opening {, to level closing }
        private string setProperSpacePrefix(string lineStr, int lineNum)
        {
            //this just rewrites our lines at the beginning of our label
            //we should only call this after this debugger confirms that an edited line in the JSON has the label it wants
            #region Handle Openers and Closers

            int lastLineIndxOfJson = JsonLinesBind.Count - 1;

            if (lineNum == 0)
            {
                int indxOfLbl = lineStr.IndexOf("{");
                if (indxOfLbl == -1) return lineStr;
                string allB4Label = lineStr.Substring(0, indxOfLbl);
                string allAftrLabelIndex = lineStr.Substring(indxOfLbl);
                allB4Label = NormalizeWhiteSpace(allB4Label, true);
                string fixedLine = allB4Label + allAftrLabelIndex;
                return fixedLine;
            }
            else if (lineNum == 1)
            {
                //we're on the 2nd line
                int indxOfLbl = lineStr.IndexOf("\"customLevelMusic\"");
                if (indxOfLbl == -1) return lineStr;
                string allB4Label = lineStr.Substring(0, indxOfLbl);
                string allAftrLabelIndex = lineStr.Substring(indxOfLbl);
                allB4Label = NormalizeWhiteSpace(allB4Label, true);
                string fixedLine = "    " + allB4Label + allAftrLabelIndex;
                return fixedLine;
            }
            else if (lineNum == lastLineIndxOfJson - 1)
            {
                //we're on the 2nd to last line, looking for ]
                int indxOfLbl = lineStr.IndexOf("[");
                if (indxOfLbl == -1) return lineStr;
                string allB4Label = lineStr.Substring(0, indxOfLbl);
                string allAftrLabelIndex = lineStr.Substring(indxOfLbl);
                allB4Label = NormalizeWhiteSpace(allB4Label, true);
                string fixedLine = "    " + allB4Label + allAftrLabelIndex;
                return fixedLine;
            }
            else if (lineNum == lastLineIndxOfJson)
            {
                int indxOfLbl = lineStr.IndexOf("}");
                if (indxOfLbl == -1) return lineStr;
                string allB4Label = lineStr.Substring(0, indxOfLbl);
                string allAftrLabelIndex = lineStr.Substring(indxOfLbl);
                allB4Label = NormalizeWhiteSpace(allB4Label, true);
                string fixedLine = allB4Label + allAftrLabelIndex;
                return fixedLine;
            }


            #endregion Handle Openers and Closers

            int placement = getPlacementInSequence(lineStr, lineNum); //use our line's information and its line # to figure out our placement
            if (placement == -1) return lineStr;

            int indexOfLabel = lineStr.IndexOf(expectedFields[placement]);
            #region HandleDoublePossibilities
            if (placement == 2)
            {
                //our expectedField is MainMusic or BossMusic
                indexOfLabel = lineStr.IndexOf("\"MainMusic\""); //first we look for MainMusic
                if (indexOfLabel == -1) indexOfLabel = lineStr.IndexOf("\"BossMusic\"");  //if that didn't work, look for BossMusic
            }
            #endregion HandleDoublePossibilities
            
            if (indexOfLabel == -1) return lineStr;

            //we'll cut the line in half, with the intention of setting the proper spaces at the beginning of the line
            //if our user did->    hi    "Bank" : , it'll turn into->       hi"Bank":

            string allBeforeLabel = lineStr.Substring(0, indexOfLabel);
            string allAfterLabelIndex = lineStr.Substring(indexOfLabel);
            allAfterLabelIndex = allAfterLabelIndex.TrimEnd();
            allBeforeLabel = NormalizeWhiteSpace(allBeforeLabel, true);

            string spaces = "";
            for (int i=0; i < spacesBeforeField[placement]; i++)
            {
                spaces += " ";
            }
            

            string editedLine = spaces + allBeforeLabel + allAfterLabelIndex;

            if(lineStr != editedLine)
            {
                //it's about to get updated, block the update call
                //managerFixingLine = true; this doesn't work
            }

            return editedLine;
        }
        private void setALLProperSpacePrefixes()
        {
            //runs through our binding list, line by line. If line has no error we reset the spacing

            int closeBracketLine = -1; //we'll use this to know if we found our closing ]

            int verifiedPlacement = 0;
            for (int i=0; i<JsonLinesBind.Count; i++)
            {
                if (lineHasError(i)) { verifiedPlacement = -1; continue; }//line had an error, don't do anything

                string spacesPrefix = "";
                string lineString = JsonLinesBind[i].ListItem.ToString();
                int indexOfLabel = -1;
                if(i == 0)
                {
                    //first line
                    indexOfLabel = lineString.IndexOf("{"); //there's no errors here, so this should be fine
                    spacesPrefix = "";//0
                    goto HaveSpacesAndIndex;
                } else if (i == 1)
                {
                    //2nd line
                    indexOfLabel = lineString.IndexOf("\"custom"); //there's no errors here, so this should be fine
                    spacesPrefix = "    ";//4
                    goto HaveSpacesAndIndex;
                } else if(closeBracketLine == -1)
                {
                    //all lines after first 2 lines


                    #region Verify placement isn't lost
                    if(verifiedPlacement == -1)
                    {
                        //we lost our verified Placement because there was an error, we'll reset it
                        for (int z = 0; z < identifiableFields.Length; z++)
                        {
                            if (lineString.Contains(identifiableFields[z]))
                            {

                                verifiedPlacement = corrPlaceOfIdableFields[z];
                                break;
                            }
                        }

                        //if verified placement is still -1, it wasn't any of those
                        if (verifiedPlacement == -1)
                        {
                            if (lineString.Contains("{"))
                            {
                                // SPACE COUNT IS 8, level opener
                                spacesPrefix = "        ";//8
                            } else if (lineString.Contains("]")){
                                //space count is 4
                                spacesPrefix = "    ";//4
                                closeBracketLine = i; //set our closeBracketLine so we start setting everything else
                                indexOfLabel = lineString.IndexOf("]");
                                goto HaveSpacesAndIndex;
                            } else if (lineString.Contains("}"))
                            {
                                //could be Music closer, or Level closer
                                if (!lineHasError(i - 1))
                                {
                                    if (JsonLinesBind[i - 1].ListItem.ToString().Contains("}"))
                                    {
                                        //previous line had a music closer
                                        //space count is 8
                                        spacesPrefix = "        ";//8
                                    } else if (JsonLinesBind[i - 1].ListItem.ToString().Contains("BPM") ||
                                        JsonLinesBind[i - 1].ListItem.ToString().Contains("bankPath"))
                                    {
                                        //previous line had BPM or bankPath in it
                                        //space count is 12
                                        spacesPrefix = "            ";//12
                                    }
                                } else if (!lineHasError(i + 1))
                                {
                                    if (JsonLinesBind[i + 1].ListItem.ToString().Contains("}"))
                                    {
                                        //previous line had a music closer
                                        //space count is 8
                                        spacesPrefix = "        ";//8
                                    }
                                    else if (JsonLinesBind[i + 1].ListItem.ToString().Contains("{") ||
                                        JsonLinesBind[i + 1].ListItem.ToString().Contains("]"))
                                    {
                                        //next line has a level opener, or the closing ]
                                        //space count is 12
                                        spacesPrefix = "            ";//12
                                    }
                                } else
                                {
                                    //could not find spacing, just return the line
                                    continue;//continue will stop line from being altered
                                }
                            }
                        }
                    }
                    #endregion Verify placement isn't lost

                    if (verifiedPlacement >= expectedFields.Length)
                    {
                        if (lineString.Contains("]"))
                        {
                            indexOfLabel = lineString.IndexOf("]");
                            spacesPrefix = "    ";//4
                            goto HaveSpacesAndIndex;
                        } else
                        {
                            verifiedPlacement = 0;
                        }
                    } else if(verifiedPlacement == 8 && !lineString.Contains("bankPath"))
                    {
                        verifiedPlacement = 9;
                    } else if(verifiedPlacement == 10 && lineString.Contains("{"))
                    {
                        verifiedPlacement = 2;
                    }

                    //There are NO errors if we're here....


                    if(verifiedPlacement == 0)
                    {
                        spacesPrefix = "        ";//8
                    } else if(verifiedPlacement == 1)
                    {
                        spacesPrefix = "            ";//12
                    }
                    else if(verifiedPlacement == 2)
                    {
                        spacesPrefix = "            ";//12

                    } else if(verifiedPlacement <= 3 && verifiedPlacement >= 8)
                    {
                        //i donno how to do this in a switch statement
                        spacesPrefix = "                ";//16
                    }
                    else if (verifiedPlacement == 9)
                    {
                        spacesPrefix = "            ";//12
                    }
                    else if (verifiedPlacement == 10)
                    {
                        spacesPrefix = "        ";//8
                    } else
                    {
                        continue;
                    }


                } else
                {
                    //we found a ], everything else will be no spaces
                    indexOfLabel = -2;
                }

                indexOfLabel = lineString.IndexOf(expectedFields[verifiedPlacement]);

                #region HandleDoublePossibilities
                if (verifiedPlacement == 2)
                {
                    //our expectedField is MainMusic or BossMusic
                    indexOfLabel = lineString.IndexOf("\"MainMusic\""); //first we look for MainMusic
                    if (indexOfLabel == -1) indexOfLabel = lineString.IndexOf("\"BossMusic\"");  //if that didn't work, look for BossMusic
                }
                #endregion HandleDoublePossibilities

            HaveSpacesAndIndex:

                if (indexOfLabel == -1) continue;//we can't find the label we want, something went wrong
                if(indexOfLabel == -2)
                {
                    //we're handling a line that's after the closing ]
                    string allTextOnLine = JsonLinesBind[i].ListItem.ToString();
                    allTextOnLine = NormalizeWhiteSpace(allTextOnLine, true);
                    JsonLinesBind[i].ListItem = allTextOnLine;
                }

                string allBeforeLabel = lineString.Substring(0, indexOfLabel);
                string allAfterLabelIndex = lineString.Substring(indexOfLabel);
                allBeforeLabel = NormalizeWhiteSpace(allBeforeLabel, true);
                allAfterLabelIndex = allAfterLabelIndex.TrimEnd();

                string fixedLine = spacesPrefix + allBeforeLabel + allAfterLabelIndex;
                JsonLinesBind[i].ListItem = fixedLine;

                verifiedPlacement++;
                //end of for loop
            }

        }

        private void HandleLineNums(int indexOfAddedRow, bool subtract = false)
        {
            //the index of the added row is the zero-based line number that got added
            //when we add, what we were selecting moves downward(increases line num), and the line we were selecting is now blank
            var errorListEntries = jsonAnomalyList.Items;

            for (int i = 0; i < errorListEntries.Count; i++)
            {
                
                string lineNumStr = errorListEntries[i].SubItems[0].Text.ToString();
                if (lineNumStr.Contains("–"))
                {
                    
                    //we have a range in the box, like 14–25
                    string[] lineRange = lineNumStr.Split('–');
                    string rangeStartStr = lineRange[0];
                    string rangeEndStr = lineRange[1];
                    int rangeStart = -1;
                    int rangeEnd = -1;
                    if (Int32.TryParse(lineRange[0], out int onebanana))
                    {
                        rangeStart = Int32.Parse(lineRange[0]);
                    }
                    if (Int32.TryParse(lineRange[1], out int twobanana))
                    {
                        rangeEnd = Int32.Parse(lineRange[1]);
                    }
                    if (rangeStart == -1) continue; if (rangeEnd == -1) continue;
                    

                    int OneBsdAddedRowNum = indexOfAddedRow + 1;

                    int newRangeStart = rangeStart;
                    int newRangeEnd = rangeEnd;

                    if (OneBsdAddedRowNum < rangeStart)
                    {
                        // !!!!!!!!
                    
                        if (subtract) newRangeStart--;
                        else newRangeStart++;
                    }
                    if (OneBsdAddedRowNum <= rangeEnd)
                    {
                    
                        if (subtract) newRangeEnd--;
                        else newRangeEnd++;
                    }
                    
                    errorListEntries[i].SubItems[0].Text = newRangeStart + "–" + newRangeEnd;
                    


                    bool inRangeOfFatality = (OneBsdAddedRowNum >= rangeStart) && (OneBsdAddedRowNum <= rangeEnd);
                    //I think it actually needs to be > range start, not >=
                    if (inRangeOfFatality)
                    {
                        //set background to red
                        setJsonEditorLineBG(OneBsdAddedRowNum, true);
                    }


                    /*
                     * Need to check individually if the line we're adding is above the Start, then is Above the End
                     * If it is, we add to these numbers, and SET them in the JsonAnomalyList
                     * 
                     * We still need to add what we do if we only have one number in the box--same thing without the split sequence
                     * 
                     * */




                }
                else
                {
                    //we just have one number in the box
                    int lineNum = -1;
                    if (Int32.TryParse(lineNumStr, out int threebanana))
                    {
                        lineNum = Int32.Parse(lineNumStr);
                    }
                    if (lineNum == -1) continue;
                    int newLineNum = lineNum;
                    int OneBsdAddedRowNum = indexOfAddedRow + 1;
                    if (lineNum >= OneBsdAddedRowNum)
                    {
                        //our line number in the JSON anomaly list entry is at or above the one we're changing, so we need to change ours
                        if (subtract) newLineNum--;
                        else newLineNum++;
                    }
                    errorListEntries[i].SubItems[0].Text = newLineNum.ToString();

                }
                    
                
            }
        }


        private void AddLineToEditor(bool AddBelowSelected = false)
        {
            //Adds a line to our editor; it adds it AT our index, meaning it pushes our selection down, unless addBelowSelected = true
            var selectedRows = dgJsonEditor.SelectedRows;
            if(selectedRows.Count > 1)
            {
                MessageBox.Show("Please select only one row when adding a line");
            } else if(selectedRows.Count == 0)
            {
                MessageBox.Show("No lines selected in .Json editor");
            } else
            {
                int selectedIndex = dgJsonEditor.SelectedRows[0].Index;
                if (AddBelowSelected) { selectedIndex += 1; }//if we want to add below, add to our selectedIndex
                JsonLinesBind.Insert(selectedIndex, new JsonLineList(""));
                HandleLineNums(selectedIndex); //this changes our numbers in our JSON anomaly list based on our added number
            }
        }
        private void AddLineClick(object sender, EventArgs e)
        {
            AddLineToEditor();
        }
        private void AddLineBelowClick(object sender, EventArgs e)
        {
            AddLineToEditor(true);
        }


        private void verifyAllDuplicates()
        {
            //this runs through the JSON anomaly list->if it sees that there was a "dupe" error, it verifies it's an actual duplicate
            List<int> anomaliesToDestroy = new List<int>(); //stores the zero-based index that we'll get rid of when we're done with for loop
            for (int i = 0; i < jsonAnomalyList.Items.Count; i++)
            {
                if (jsonAnomalyList.Items[i].SubItems[1].Text.Contains("dupe"))
                {
                    //we found a dupe error!
                    string lnNumString = jsonAnomalyList.Items[i].SubItems[0].Text;
                    int lnNum = -1;
                    if (Int32.TryParse(lnNumString, out int ya))
                    {
                        lnNum = Int32.Parse(lnNumString); //make sure it's a number
                    }
                    if (lnNum == -1) return;

                    if(verifyDuplicates(lnNum-1) == false)
                    {
                        //we just verified that there's no duplicate
                        anomaliesToDestroy.Add(i);
                        setJsonEditorLineBG(lnNum);
                    }
                    
                }
            }

            if(anomaliesToDestroy.Count > 0)
            {
                foreach(int anomalyIndex in anomaliesToDestroy)
                {
                    jsonAnomalyList.Items.RemoveAt(anomalyIndex);
                }
            }

        }

        private bool verifyDuplicates(int zbLineNum)
        {
            //I'm just going to have this run through the list AFTER everything's done


            if (zbLineNum < 1) return false; //we can't verify it
            string line = JsonLinesBind[zbLineNum].ListItem.ToString();

            int prevLineNum = zbLineNum - 1;
            string prevJsonLine = JsonLinesBind[prevLineNum].ListItem.ToString();


            for (int i=0; i<identifiableFields.Length; i++)
            {
                if (line.Contains(identifiableFields[i]) && prevJsonLine.Contains(identifiableFields[i]))
                {
                    return true;
                }
            }
            //

            //we haven't found anything yet
            //we'll check for individual instances of { and } (they should never be on the same line unless they were an Event format)
            if (line.Contains("{") && prevJsonLine.Contains("{") &&
                !line.Contains("}") && prevJsonLine.Contains("}"))
            {
                return true;
            }

            if (line.Contains("}") && prevJsonLine.Contains("}") &&
                !line.Contains("{") && prevJsonLine.Contains("{"))
            {
                return true;
            }



            return false;
        }



        //i might need to put this in Form1
        //this function checks to see if it has a verification date AFTER the Json's last modified date
        private void CheckIfJsonClean()
        {



        }




        bool managerFixingLine = false;
        private void JsonCellUpdate(object sender, DataGridViewCellEventArgs e)
        {

            //DataGridViewCell dgc = sender as DataGridViewCell;
            //string lineString = dgc.Value.ToString();
            //e.
            /*
            if (managerFixingLine)
            {
                managerFixingLine = false; return; //we use this if WE updated the cell, and want to block it from rechecking, such as prefix space fix
                
            }*/

            int lineNumZeroBased = e.RowIndex;
            string editedLine = "";

            if (JsonLinesBind[lineNumZeroBased].ListItem != null)
            {
                string fixedEntry = setProperSpacePrefix(JsonLinesBind[lineNumZeroBased].ListItem.ToString(), lineNumZeroBased);
                JsonLinesBind[lineNumZeroBased].ListItem = fixedEntry;
                justCheckEntireJsonListAgain(-1);
                verifyAllDuplicates();
            } else
            {
                JsonLinesBind.RemoveAt(lineNumZeroBased);
                justCheckEntireJsonListAgain(-1);
                verifyAllDuplicates();
                HandleLineNums(lineNumZeroBased, true);
            }

            

            /*
            if (JsonLinesBind[lineNumZeroBased].ListItem != null)
            {
                editedLine = JsonLinesBind[lineNumZeroBased].ListItem.ToString();
                //editedLine = setProperSpacePrefix(editedLine, lineNumZeroBased);
                string errorCode = CheckEditorLine(editedLine, lineNumZeroBased); //this is a less intricate version of the debuggy. It fails if --
                                                                                  //-- we can't figure out where we are in the sequence, returning a "???"
                adjustErrorsList(errorCode, lineNumZeroBased + 1); //this runs through our JSON anomaly list, and removes, edits, or adds error entries
            } else
            {
                //our line we edited is now blank
                bool lineIsRed = deletedLineHadError(lineNumZeroBased);
                JsonLinesBind.RemoveAt(lineNumZeroBased);
                HandleLineNums(lineNumZeroBased, true);
                adjustErrorsListRemoval(lineNumZeroBased + 1, lineIsRed);
                
                //return;
            }*/


        }



        private void enableSaveButton(int NumOfCritErrors, int NumOfPotntlErrors)
        {
            //MessageBox.Show("CritErrors: " + NumOfCritErrors + "; PotErrs: " + NumOfPotntlErrors);
            if(NumOfCritErrors == 0 && NumOfPotntlErrors == 0)
            { 
                DebugSaveJsonBtn.ForeColor = Color.FromArgb(255, 255, 255, 255);
                DebugSaveJsonBtn.BackColor = Color.FromArgb(255, 0, 64, 0);
                DebugSaveJsonBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 0, 77, 0);
                DebugSaveJsonBtn.FlatAppearance.MouseDownBackColor = Color.FromArgb(255, 0, 40, 0);
                DebugSaveJsonBtn.FlatAppearance.BorderColor = Color.FromArgb(255, 160, 160, 160);
                DebugSaveJsonBtn.Enabled = true;
                Image enabledCheck = MetalManager.Properties.Resources.check;
                DebugSaveJsonBtn.Image = enabledCheck;
                SaCBtnLabel.Text = "";
                debug_undrJsonAnomLbl.Text = "All formating errors and anomalies have been purged from this customsongs.json.";


                suspendSongButton.ForeColor = Color.FromArgb(255, 200, 160, 160);
                suspendSongButton.BackColor = Color.FromArgb(255, 70, 50, 50);
                suspendSongButton.FlatAppearance.BorderColor = Color.FromArgb(255, 255, 128, 128);

            }
            if (NumOfCritErrors == 0 && NumOfPotntlErrors > 0)
            {
                //no critical errors, but we have potential issues
                DebugSaveJsonBtn.ForeColor = Color.FromArgb(255, 255, 255, 255);
                DebugSaveJsonBtn.BackColor = Color.FromArgb(255, 129, 64, 0);
                DebugSaveJsonBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 145, 75, 0);
                DebugSaveJsonBtn.FlatAppearance.MouseDownBackColor = Color.FromArgb(255, 84, 42, 0);
                DebugSaveJsonBtn.FlatAppearance.BorderColor = Color.FromArgb(255, 255, 0, 0);
                DebugSaveJsonBtn.Enabled = true;
                Image enabledCheck = MetalManager.Properties.Resources.check;
                DebugSaveJsonBtn.Image = enabledCheck;

                SaCBtnLabel.Width = 200;
                SaCBtnLabel.Left = 713;
                SaCBtnLabel.Text = "Potential Issues remain. Only continue if you know what you're doing.";
                debug_undrJsonAnomLbl.Text = "No formating errors can be found, but Debug Panel found issues that can cause in-game crashes.";

                suspendSongButton.ForeColor = Color.FromArgb(255, 255, 255, 255);
                suspendSongButton.BackColor = Color.FromArgb(255, 90, 0, 0);
                suspendSongButton.FlatAppearance.BorderColor = Color.FromArgb(255, 255, 128, 128);
            }
            else if(NumOfCritErrors > 0)
            {
                DebugSaveJsonBtn.ForeColor = Color.FromArgb(255, 160, 160, 160);
                DebugSaveJsonBtn.BackColor = Color.FromArgb(255, 128, 128, 128);
                //DebugSaveJsonBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 0, 77, 0);
                //DebugSaveJsonBtn.FlatAppearance.MouseDownBackColor = Color.FromArgb(255, 0, 40, 0); these don't matter, it's disabled
                DebugSaveJsonBtn.FlatAppearance.BorderColor = Color.FromArgb(255, 128, 128, 128);
                DebugSaveJsonBtn.Enabled = false;
                Image disabledCheck = MetalManager.Properties.Resources.check_stillframe;
                DebugSaveJsonBtn.Image = disabledCheck;
                debug_undrJsonAnomLbl.Text = "This customsongs.json contains errors that must be addressed before Metal Manager can select it.";

                suspendSongButton.ForeColor = Color.FromArgb(255, 255, 255, 255);
                suspendSongButton.BackColor = Color.FromArgb(255, 90, 0, 0);
                suspendSongButton.FlatAppearance.BorderColor = Color.FromArgb(255, 255, 128, 128);
                

                SaCBtnLabel.Width = 171;
                SaCBtnLabel.Left = 742;
                SaCBtnLabel.Text = "All Critical Errors must be fixed before .json can be saved.";
            }
        }

        int maxSongAnalysisWidth = 413;
        private void StringFitToWidth(Label Lbl, string stringToSqueezeIn)
        {
            Lbl.Text = stringToSqueezeIn;
            int strLength = stringToSqueezeIn.Length;
            int verymaxStringLmt = 130; //the maximum limit. the label can only fit 130 i's for example
            if(strLength>verymaxStringLmt) strLength = verymaxStringLmt;

            while(Lbl.Width > 413)
            {
                string shrtnd = StringShortener(stringToSqueezeIn, strLength);
                strLength -= 5;
            }
        }

        private string StringShortener(string ogString, int maxLength)
        {
            if (ogString.Length > maxLength)
            {
                string shortened = ogString.Substring(0, maxLength-2) + "...";
                return shortened;
            } else
            {
                return ogString;
            }
        }

        private string pathShortener(string originalPath, int maxCharacters, int directoriesToShow)
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
                int numberOfDirectoriesToShow = directoriesToShow;

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
                else
                {
                   // MessageBox.Show("Original path: " + originalPath);
                }

            }
            return returnString;
        }

        private void SaC_MouseOver(object sender, MouseEventArgs e)
        {
            SaCBtnLabel.Visible = true;
        }

        private void SaC_MouseOut(object sender, EventArgs e)
        {
            SaCBtnLabel.Visible = false;
        }

        private void suspendBtn_mouseOver(object sender, MouseEventArgs e)
        {
            susSongExplLabel.Visible = true;
        }

        private void suspendBtn_mouseOut(object sender, EventArgs e)
        {
            susSongExplLabel.Visible = false;
        }

        private void RmvSlctdLineBtn_click(object sender, EventArgs e)
        {
            var selectedRows = dgJsonEditor.SelectedRows;
            if(selectedRows.Count > 1)
            {
                MessageBox.Show("Please select one line at a time when deleting.");
                return;
            } else if(selectedRows.Count == 0)
            {
                MessageBox.Show("No row selected.");
            }

            int selectedIndex = dgJsonEditor.SelectedRows[0].Index;
            JsonLinesBind.RemoveAt(selectedIndex);
            justCheckEntireJsonListAgain(-1);
            verifyAllDuplicates();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
        private void RescanJsonBtn_Click(object sender, EventArgs e)
        {
            justCheckEntireJsonListAgain(-1);
            verifyAllDuplicates();
            DisableRescanBtnForFewSeconds();

        }
        System.Windows.Forms.Timer rescanTimer = new System.Windows.Forms.Timer();
        private void DisableRescanBtnForFewSeconds() 
        {
            rescanTimer.Interval = 2000; // here time in milliseconds
            rescanTimer.Tick += timer_Tick;
            rescanTimer.Start();
            RescanJsonLinesBtn.Text = "Rescan Complete";
            RescanJsonLinesBtn.Enabled = false;
        }

        void timer_Tick(object sender, System.EventArgs e)
        {
            RescanJsonLinesBtn.Enabled = true;
            RescanJsonLinesBtn.Text = "Rescan for Errors";
            rescanTimer.Stop();
        }


        string initialDebugPBText = null; //we'll set this the first time we enter the box
        private void debugPasteBox_enter(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (initialDebugPBText == null) initialDebugPBText = tb.Text;

            if (tb.Text == initialDebugPBText)
            {
                tb.Text = "";
            }
        }
        private void debugPasteBox_unfocus(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.Text == "")
            {
                tb.Text = initialDebugPBText;
            }
        }

        private void clearDebugPastebox(object sender, EventArgs e)
        {
            debugPastebox.Clear();
        }


        private void debugChoseDDSlct(object sender, EventArgs e)
        {
            if(debugSongSelectCombo.SelectedIndex > -1)
            {
                debug_DropDownSelect();
            }
        }

        
    }

    public class JsonLineList
    {
        private string Itemname;

        public JsonLineList(string _ListItem)
        {
            ListItem = _ListItem;
        }
        public string ListItem
        {
            get { return Itemname; }
            set { Itemname = value; }

        }


    }

    //DataGridView looks for properties of containing objects. For string there is just one property: length. So, you need a wrapper for a string like this
    public class StringValue
    {
        public StringValue(string s)
        {
            _value = s;
        }
        public string Value { get { return _value; } set { _value = value; } }
        string _value;
    }
}
