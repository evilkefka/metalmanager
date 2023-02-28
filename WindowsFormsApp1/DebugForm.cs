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
using System.Text.RegularExpressions; //Regex is regular expressions!??!!! WHAT a country!!

namespace WindowsFormsApp1
{

    public partial class DebugForm : Form
    {
        public DebugForm()
        {
            InitializeComponent();
        }

        public Form1 MyParentForm;

        public string TheValue
        {

            get { return debugTextbox.Text; }
        }

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

        Color JsonEditorColor1 = Color.White;
        Color JsonEditorColor2 = Color.FromArgb(255, 240, 240, 240);//can't i just do alpha?

        Color JsonEditorErrC1 = Color.FromArgb(255, 255, 50, 50);
        Color JsonEditorErrC2 = Color.FromArgb(255, 235, 0, 0);

        private void setDGEditorLinesBGColrs(string[] errorList)
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
            int addedLines = 0;
            foreach (string error in errorList)
            {
                string[] errorInfo = error.Split(':');
                if (error.Contains("BackOnTrack"))
                {
                    if (Int32.TryParse(errorInfo[0], out int yo))
                    {
                        setProperFatalityRange(Int32.Parse(errorInfo[0]), errorList);
                    }
                } else
                if (error.Contains("forgot"))
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
        }

        private void setProperFatalityRange(int lineNumBoT, string[] errorList)
        {
            //this function looks for the highest range that's less than errorList
            //lineNumBoT: line number where we're back on track

            int rangeEnd = lineNumBoT - 1;
            if (rangeEnd == -1) return;
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

            if (closestWeCanGet == -1) return;


            //if we're here, we have a number

            int startingLine = -1;
            int endingLine = rangeEnd;
            //we're going to add "-##" at the end of our LineNum for Fatality, so it looks like ie: 3-11
            //while we're here, we'll get rid of the BackOnTrack entry in error list
            for (int i = 0; i < jsonAnomalyList.Items.Count; i++)
            {
                if (jsonAnomalyList.Items[i].SubItems[0].Text == closestWeCanGet.ToString() &&
                    jsonAnomalyList.Items[i].SubItems[1].Text.Contains("fatal"))
                {
                    //we found it!
                    string oldNum = jsonAnomalyList.Items[i].SubItems[0].Text;
                    if (!Int32.TryParse(oldNum, out int yee)) return;
                    if (rangeEnd < Int32.Parse(oldNum)) return;

                    startingLine = Int32.Parse(oldNum);
                    endingLine = rangeEnd;

                    jsonAnomalyList.Items[i].SubItems[0].Text += "–" + rangeEnd;
                    if (jsonAnomalyList.Items[i + 1].SubItems[1].Text.Contains("BackOnTrack"))
                    {
                        //make sure the line in front of us is the BackOnTrack line, then delete it. if it isn't, then.. it's still going to be there
                        jsonAnomalyList.Items[i + 1].Remove();
                    }
                    //if(jsonAnomalyList.Items[i].SubItems[0].Contains())
                }
            }

            if (startingLine == -1) return; if (endingLine == -1) return; //pretty sure we don't need these

            //if we're this far, we should be good to reset the backgrounds of everything with a fatal error
            for (int i = startingLine; i <= endingLine; i++)
            {
                setJsonEditorLineBG(i, true); //sets our background of each affected entry in DataGrid to red
            }

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

        private void fillErrorList(string[] errorList)
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

                if (errorCode.Contains("forgot"))
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

        private void debuggyRange(int oneBasedFirstLine, int oneBasedLastLine, int jsonAnomalyEntryIndex)
        {
            //unlike debuggy(), this analyzes the lines in the JSON editor(really its source); debuggy() analyzes the original JSON

            //we don't care if they changed anything except the first (we could do last, but then we'd have to rewrite code)
            int rangeLength = oneBasedLastLine - oneBasedFirstLine + 1; //+1 because we need to include the first line

            string lineBeforeRange = JsonLinesBind[oneBasedFirstLine - 2].ListItem.ToString(); //we'll use this to get our placement
            int placement = getPlacementInSequence(lineBeforeRange, oneBasedFirstLine - 2);//this line num wants a zero-based index;
            if (placement == -1) return;
            else placement++; //as long as we found a placement, add one to it as we go into the next line
            //if placement -1, that means the next line is actually a valid line, too (or at least has a label)
            if (placement > expectedFields.Length - 1) placement = 0;

            MessageBox.Show("LineBeforeRange: " + lineBeforeRange + "\nPlacementForIt: " + placement);

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


                if (lineErrors.Contains("dupe") || lineErrors.Contains("fatality") || lineErrors.Contains("forgot") || lineErrors.Contains("labelinvalid"))
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
                    //we encountered an error, but it wasn't a fatal error/label-reading error
                    jsonAnomalyList.Items.Add(lineErrors);
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


            /*

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
            }

            if (lineErrors.Contains("forgot"))
            {
                currPlaceInExpctdEntry++;
            }

            if (lineErrors.Length > 0)
            {
                linesWithErrors.Add(i + 1 + ":" + lineErrors); //i+1 because we don't start on line 0

            }

            currPlaceInExpctdEntry++;
            */

        }



        private string[] debuggy(string fullJson)
        {
            string fixedJson = fullJson;
            List<string> linesWithErrors = new List<string>();
            bool fatalErrorEncountered = false;//we use this if we find a fatal error; we keep skipping lines until we get out of the level with the error

            string[] fixedJsonLinesWithEmpties = fixedJson.Split('\n'); //we're going to keep our empty lines, to be sure what line we're on


            string[] fixedJsonLines = fixedJson.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (fixedJsonLines.Length < 10) { string[] tooshort = { "2short" }; return tooshort; }

            int currPlaceInExpctdEntry = 0; //current place in expected entry
            int i = 0;
            int threshold = 0;
            while (i < fixedJsonLines.Length)
            {
                #region DebugLines
                string line_unaltered = fixedJsonLines[i];
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

                        } else
                        {
                            //there's no { in the first line
                            //label is missing, we need to look around for something identifiable
                            //technically, they could have the first line be-> { "customLevelMusic":[ and still be correct
                            string[] firstLineError = { "wtf?" };
                            if (fixedJsonLines.Length < 2) return firstLineError; //catch-all in case JSON is very short

                            if (fixedJsonLines[1].Contains("{"))
                            {
                                //our { is on the next line

                                linesWithErrors.Add("garbageLine");


                            } else if (fixedJsonLines[1].Contains("\"customLevelMusic\""))
                            {
                                //we can't find {, but customLevelMusic is on next line

                                linesWithErrors.Add("forgotFirstLine");
                            }
                        }
                        i++; continue;
                    } else
                    {

                        //second line, we want "customLevelMusic" : [
                        string firstLineNoSpaces = NormalizeWhiteSpace(fixedJsonLines[0], true);

                        if (line_nospaces != "\"customLevelMusic\":[")
                        {
                            string[] secondLineError = { "wtf?" };
                            if (fixedJsonLines.Length < 3) return secondLineError;

                            if (fixedJsonLines[2].Contains("{"))
                            {
                                //our next line contains the next sequence, we forgot the CLM opener
                                secondLineError[0] = "forgotCLM";
                                return secondLineError;
                            } else if (line_nospaces.Contains("{") && fixedJsonLines[2].Contains("\"customLevelMusic\":["))
                            {
                                //this line has a {, which our first line has, and our next line has the customLevelMusic thing
                                secondLineError[0] = "dupe";

                            }
                            else if (line_nospaces.Contains("{") && firstLineNoSpaces.Contains("{\"customLevelMusic\":["))
                            {
                                //the user seems to have combined { and "customLevelMusic" : [ onto one line. that works, but we don't want that
                                secondLineError[0] = "forgotCLMFormat"; //we want it to add another
                            } else
                            {
                                linesWithErrors.Add("clm"); //customLevelMusic opening line incorrect format
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
                                linesWithErrors.Add((i) + ":forgotCLMFormat"); //we don't want i-1
                                continue;
                            }

                        }

                    }

                    //if we're above our length, we might be done, check for closing ]
                    if (currPlaceInExpctdEntry >= expectedFields.Length)
                    {
                        if (line_unaltered.Contains("]"))
                        {
                            linesWithErrors.Add("Found]");
                            break;
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
                        if (fixedJsonLines[i].Contains("\"BossMusic\""))
                        {
                            currPlaceInExpctdEntry = 2;
                            continue;
                        }
                        else if (!hasMatchingLabel)
                        {

                        }
                    }

                    string lineErrors = getLineErrors(fixedJsonLines[i], fixedJsonLines[i + 1], currPlaceInExpctdEntry);

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
                    }

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

                threshold++; //threshold is used just to verify this doesn't get stuck in an infinite loop, but i isn't going up, why would this?
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
                if (whatsGoingOn == "garbageLine")
                {
                    return "bunkLine";
                } else if (whatsGoingOn == "dupe")
                {
                    int prevIndx = indexOfLabelWeWant - 1; if (prevIndx < 0) prevIndx = expectedFields.Length - 1;
                    return "dupe_" + expectedFields[prevIndx];
                }
                else if (whatsGoingOn == "labelinvalid")
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
                    errorsOnLine.Add("unexpectedEnd-Wanted_" + endingWeWant);
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
                            errorsOnLine.Add("unexpectedEnd");
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
                    }
                    else
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
            // ↑Endings, ↑↑ LabelCheck, ↓ Format Errors

            string[] LineFormatErrors = getFormatErrors(line, indexOfLabelWeWant); //get the format errors
            errorsOnLine.AddRange(LineFormatErrors); //add them to our line errors


            string errorReportString = "";
            foreach (string error in errorsOnLine)
            {
                errorReportString += error + "|"; //for each error, we're making a string that says "1:errorCode|4:errorC1|34:errorCd5|
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
                string unexpectedCharacters = lineNS.Replace("{", "");
                if (unexpectedCharacters.Length > 0)
                {
                    formatErrors.Add("unexpectedCharsA_" + unexpectedCharacters);
                }
            }
            else if (indexOfLabelWeWant == 9 || indexOfLabelWeWant == 10)
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
            string labelNS = NormalizeWhiteSpace(labelStr, true);
            string valueNS = NormalizeWhiteSpace(valueStr);

            int numberOfQuotesInValue = valueNS.Split(new string[] { "\"" }, StringSplitOptions.None).Length - 1;
            int numberOfQuotesInLabel = labelNS.Split(new string[] { "\"" }, StringSplitOptions.None).Length - 1;


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
            }
            else
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
            }
            else if (numberOfQuotesInValue < 2)
            {
                lineFormatErrors.Add("neqVal->" + valueNS);
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
                lineFormatErrors.Add("unwantedquotes(value: " + valueNS + ")");
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
            if (indexOfBankPathInfo == 0) { bpFormErrors.Add("nobpfile"); return bpFormErrors.ToArray(); } //if our line looks like bankPath: ":\\Dir\\Dir2\\ and they forgot the drive letter
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
                bpFormErrors.Add("bpF"); //bankpath formatting
            }
            else if (!verifyFileExists(bankPathInfo))
            {
                bpFormErrors.Add("bpFNF"); //bankpath file not found; this should be a potentially-major error (potentially major = problem will arise later, but program will still work)
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


        private void DebugFormLoad(object sender, EventArgs e)
        {
            string originalJson = ((Form1)MyParentForm).TesterBoxValue;
            string[] errors = debuggy(originalJson);
            fillErrorList(errors); //fills our left ListViewBox, which tells us what errors we have and what lines they're on
            //fillJsonLines(originalJson);//fills the right ListView, which has our JSON in its entirety, NOT editable line-by-line
            //setEditorLinesBGColors(errors); //sets the bg color of each line to red if it has an error
            fillJsonDataGrid(originalJson);//fills the right DataGrid, which has our JSON in its entirety, editable line-by-line
            setDGEditorLinesBGColrs(errors);//sets the bg color of each line to red if it has an error
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

            debuggyRange(intRngStart, intRngEnd, indexOfRangeOnAnomalyList); //indexOfRangeOnAnomalyList is used if we need to update/remove the range





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
                        jsonAnomalyList.Items[i].Remove();
                        //if we just cleared the errors on a line, we need to set its BG back
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
                    //our next line has the } we wanted, this line is garbage to us
                    return "garbageLine";
                } else if (curLineStr.Contains(nextExpFld) || nextLineStr.Contains("]"))
                {
                    return "forgotitem";
                } else if (nextLineStr.Contains(nextExpFld))
                {
                    return "labelinvalid";
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
                    return "garbageLine";
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
                //next line contains the missing label for our current seqence, this line is garbage to us
                //we want to continue with the same Placement, but adding to i
                return "garbageLine";
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
                return "labelinvalid";
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
                    return "garbageLine";
                }
                else if (nextExpFld.Contains("|") && (
                    curLineStr.Contains("\"MainMusic\"") || curLineStr.Contains("\"BossMusic\"")))
                {
                    return "forgotitem";
                }
                else if (nextExpFld.Contains("|") && (
                    nextLineStr.Contains("\"MainMusic\"") || nextLineStr.Contains("\"BossMusic\"")))
                {
                    return "labelinvalid";
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
                } else if (newLine.Contains("}"))
                {
                    
                    string prevLineNS = NormalizeWhiteSpace(prevLine, true);
                    if (prevLineNS.Contains("\"BPM\":") || prevLineNS.Contains("\"bankPath\":"))
                    {
                        //previous line had BPM or bankPath, found position
                        placementInField = 9;
                    } else if (prevLine.Contains("}") && 
                        (nextLine.Contains("{") || (nextLine.Contains("]"))) )
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
                if(lineValue.Last() == ',')
                {
                    lineValue = lineValue.Substring(0, lineValue.Length - 1);
                }

            }
            

            //if we already found our placement our field sequence, skip the for loop
            if(placementInField != -1)
            {
                goto PlacementIdentified;
            }


            for (int i=0; i<identifiableFields.Length; i++)
            {
                if (newLine.Contains(identifiableFields[i]))
                {
                    
                    placementInField = corrPlaceOfIdableFields[i];
                    break;
                }
            }


            if(placementInField == -1)
            {
                //whatever line we just changed in the Json editor, we can't find where we are, return error;
                return "???";
            }

        PlacementIdentified:

            string lineErrors = getLineErrors(newLine, nextLine, placementInField);

            return lineErrors;
        }

        private void GoToJsonLine()
        {
            string lineNumStr = jsonAnomalyList.SelectedItems[0].SubItems[0].Text;

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

            int placement = getPlacementInSequence(lineStr, lineNum); //use our line's information and its line # to figure out our placement
            if (placement == -1) return lineStr;

            int indexOfLabel = lineStr.IndexOf(expectedFields[placement]);
            #region HandleDoublePossibilities
            if (placement == 2)
            {
                //our expectedField is MainMusic or BossMusic
                indexOfLabel = lineStr.IndexOf("\"MainMusic\""); //first we look for MainMusic
                if(indexOfLabel == -1) indexOfLabel = lineStr.IndexOf("\"BossMusic\"");  //if that didn't work, look for BossMusic
            }
            #endregion HandleDoublePossibilities
            if (indexOfLabel == -1) return lineStr;

            //we'll cut the line in half, with the intention of setting the proper spaces at the beginning of the line
            //if our user did->    hi    "Bank" : , it'll turn into->       hi"Bank":

            string allBeforeLabel = lineStr.Substring(0, indexOfLabel);
            string allAfterLabelIndex = lineStr.Substring(indexOfLabel);
            allBeforeLabel = NormalizeWhiteSpace(allBeforeLabel, true);
            string editedLine = spacesBeforeField[placement] + allBeforeLabel + allAfterLabelIndex;

            if(lineStr != editedLine)
            {
                //it's about to get updated, block the update call
                managerFixingLine = true;
            } else
            {
                MessageBox.Show("Spacing was good!");
            }

            return editedLine;
        }

        private void HandleLineNums(int indexOfAddedRow)
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
                    if (rangeStart == -1) return; if (rangeEnd == -1) return;

                    int OneBsdAddedRowNum = indexOfAddedRow + 1;

                    int newRangeStart = rangeStart;
                    int newRangeEnd = rangeEnd;

                    if (OneBsdAddedRowNum < rangeStart)
                    {
                        newRangeStart++;
                    }
                    if (OneBsdAddedRowNum <= rangeEnd)
                    {
                        newRangeEnd++;
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




                } /*else
                {
                    //we just have one number in the box


                    
                    if (Int32.TryParse(lineNumStr, out int threebanana))
                    {
                        intRngEnd = Int32.Parse(rngEnd); //set our End
                    }

                    jsonAnomalyList.Items[i].SubItems[1].Text = newErrors;
                }
                
                if (Int32.TryParse(lineNumStr, out int hi))
                {
                    intRngEnd = Int32.Parse(rngEnd); //set our End
                }


                if (errorListEntries[i].SubItems[0].Text.ToString() == changedLineNum.ToString())
                {
                    //we already had an entry for this line, which we found! Update it!
                    if (newErrors == "" || newErrors == null)
                    {
                        jsonAnomalyList.Items[i].Remove();
                        //if we just cleared the errors on a line, we need to set its BG back
                        setJsonEditorLineBG(changedLineNum);//sets our BG to the white/offwhite tones; changedLineNum is one-based
                    }
                    else
                    {
                        //we had errors here before, but we still have errors; update the new errors
                        jsonAnomalyList.Items[i].SubItems[1].Text = newErrors;

                    }

                    return;
                }
                else if (errorListEntries[i].SubItems[0].Text.ToString().Contains("–"))
                {
                    //– is not a -, it's Alt+0150
                    CheckToRemoveFatalError(errorListEntries[i].SubItems[0].Text.ToString(), changedLineNum);
                }
                */
            }


        }

        private void AddLineToEditor(bool AddBelowSelected = false)
        {
            //Adds a line to our editor. I'm pretty sure it pushes our selection down
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
                if (AddBelowSelected) { selectedIndex += 1; }
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

        bool managerFixingLine = false;
        private void JsonCellUpdate(object sender, DataGridViewCellEventArgs e)
        {

            //DataGridViewCell dgc = sender as DataGridViewCell;
            //string lineString = dgc.Value.ToString();
            //e.
            if (managerFixingLine)
            {
                managerFixingLine = false; return; //we use this if WE updated the cell, and want to block it from rechecking
            }

            int lineNumZeroBased = e.RowIndex;
            string editedLine = "";
            if (JsonLinesBind[lineNumZeroBased].ListItem != null)
            {
                editedLine = JsonLinesBind[lineNumZeroBased].ListItem.ToString();
            }
            //editedLine = setProperSpacePrefix(editedLine, lineNumZeroBased);
            string errorCode = CheckEditorLine(editedLine, lineNumZeroBased);
            adjustErrorsList(errorCode, lineNumZeroBased+1);
            
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
