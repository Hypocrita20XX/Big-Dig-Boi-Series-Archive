/*
MIT License

Copyright (c) 2026 Hypocrita20XX/MatthiosArcanus

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

//Happy modding miners, rock and stone!

using JsonToUAsset;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Channels;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.Kismet.Bytecode.Expressions;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;



//Time to make this into a fully-fledged program with.. User input!
//Note this program assumes you know what you're doing and has very little error handling
//I don't intend to release this in any meaningful sense to the public, so for internal use, it's fine
Console.WriteLine("Big Dig Boi : Big Big Holes For Big Big Bois");
Console.WriteLine("");

//Initialize a variable to store the directories of where the assets and reports should go
List<string> outputPath = new List<string>();

//An array of strings storing the names of each report
string[] reportFileNames = new string[3];

//Populate the array with the appropriate values
reportFileNames[0] = "001_Original_Files_And_Values.txt";
reportFileNames[1] = "002_Modified_Files_And_Values.txt";
reportFileNames[2] = "003_Excluded_Files.txt";

//Make an array of mineral files that need to be accounted for
string[] mineralFiles = new string[14];

//Unless GSG adds new minerals, this shouldn't need to be updated, so I'm just going to hardcode the values
//Probably not smart, but I'm doing it anyway
mineralFiles[0] = "TM_Bismor";
mineralFiles[1] = "TM_Croppa";
mineralFiles[2] = "TM_Dystrum";
mineralFiles[3] = "TM_Generic_Morkite";
mineralFiles[4] = "TM_Gold";
mineralFiles[5] = "TM_Gold_Melted";
mineralFiles[6] = "TM_Hollomite";
mineralFiles[7] = "TM_Iron";
mineralFiles[8] = "TM_Magnite";
mineralFiles[9] = "TM_Nitra";
mineralFiles[10] = "TM_OilShale";
mineralFiles[11] = "TM_Phazyonite";
mineralFiles[12] = "TM_Quantrite";
mineralFiles[13] = "TM_Umanite";

//Because I'm using a hodgepodge of assets from the base game mixed with files from three diferent mods
//Verification needs to happen, and those values need stored just in case
//So we need <key, value, value>
//Which leads me to a Dictionary with a list for its values, so one key and two entries in the list
//First entry should always be PickAxeDigSize and the second should also be HitsNeededToMine
//Stored as a float because converting float to an int is easier
Dictionary<string, List<float>> originalProperties = new Dictionary<string, List<float>>();

//For now until I can care a bit more
//Hardcode the directory of the base game files that have not been augmented
string originalFileLoc = "F:\\DRG_Modding\\Base Game Files\\FSD\\Content\\Landscape\\Materials\\";

//To streamline the process, initialize some nested Dictionaries so that we can save some data and log those to a file
//Similar to my standalone program, so it should ouput roughly the same thing
//The following reports are not generated because I realized that they are redundant:
// > Changes
// > List Of Files
//
// > Main Dictionary contains the directories
// > Secondary Dictionary contains the file names
// > List contains the lines to write to each file
Dictionary<string, Dictionary<string, List<string>>> originalFiles = new Dictionary<string, Dictionary<string, List<string>>>();
Dictionary<string, Dictionary<string, List<string>>> modifiedFiles = new Dictionary<string, Dictionary<string, List<string>>>();
Dictionary<string, Dictionary<string, List<string>>> excludedFiles = new Dictionary<string, Dictionary<string, List<string>>>();

//Initialize some variables for PickAxeDigSize and HitsNeededToMine
FloatPropertyData digSize = new FloatPropertyData();
//Populate with default value
digSize.Value = 105;

IntPropertyData hitsNeeded = new IntPropertyData();
//Populate with default value
hitsNeeded.Value = 2;

//Master switch (one switch to rule them all)
//Guide:
//Don't do anything: -1
//000_Terrain_Only_DigSize: 0
//001_Minerals_Only_DigSize: 1
//002_MineralsAndTerrain_DigSize: 2
//003_Terrain_Only_OneHit: 3
//004_Minerals_Only_OneHit: 4
//005_MineralsAndTerrain_OneHit: 5
//006_Terrain_Only_DigSize_OneHit: 6
//007_Minerals_Only_DigSize_OneHit: 7
//008_MineralsAndTerrain_DigSize_OneHit: 8
//All: 100
int whichMod;

//Master dig size variable
float masterDigSize = -1;

//Master hits needed to mine variable
int masterHitsNeeded = -1;

//Initialize a variable to store the path to the base game's files
string inputPath = "NONE";

//Just in case you need to reset the settings file to default
//Tired of commenting/uncommenting, so here
if (UserSettings.Default.BaseFilesLocation != "")
{
    Console.WriteLine("Would you like to reset your settings? (Y/N)");
    string? meh = Console.ReadLine();

    if (meh == "Y")
    {
        UserSettings.Default.Reset();

        Console.WriteLine("Settings have been reset");
        Console.WriteLine("Press any button to continue");
        Console.ReadLine();


        //Clear the console
        //.Clear on its own doesn't work for me, for whatever reason, so let's try a random thing I found online
        Console.Clear();
        Console.WriteLine("\u001bc\x1b[3J");
    }
    else
    {
        Console.WriteLine("Settings won't be reset");
        Console.WriteLine("Press any button to continue");
        Console.ReadLine();


        //Clear the console
        //.Clear on its own doesn't work for me, for whatever reason, so let's try a random thing I found online
        Console.Clear();
        Console.WriteLine("\u001bc\x1b[3J");
    }
}

//To try to manage things better, a Dictionary with a bool and string
//The bool indicates whether or not verification should run
//The string List contains the path(s) to send to the verification function
Dictionary<bool, List<string>> shouldVerify = new Dictionary<bool, List<string>>();

//The above Dictionary needs only two keys, true or false, so create them
shouldVerify.Add(true, new List<string>());
shouldVerify.Add(false, new List<string>());

GetRequiredInput();

//Make sure the nested Dictionaries have the necessary keys
//This needs to go after getting the required input because otherwise outputPath is empty
for (int i = 0; i < outputPath.Count; i++)
{
    //To simplify things, a string to store an appended outputPath
    string appendPath = outputPath[i] + "_Reports";

    //Check to see if the various nested Dictionaries have the appropriate keys
    if (!originalFiles.ContainsKey(appendPath))
        //If not, make sure they do, and initialize the Dictionary and its List
        originalFiles.Add(appendPath, new Dictionary<string, List<string>>());

    //Check to see if the various nested Dictionaries have the appropriate keys
    if (!modifiedFiles.ContainsKey(appendPath))
        //If not, make sure they do, and initialize the Dictionary and its List
        modifiedFiles.Add(appendPath, new Dictionary<string, List<string>>());

    // Check to see if the various nested Dictionaries have the appropriate keys
    if (!excludedFiles.ContainsKey(appendPath))
        //If not, make sure they do, and initialize the Dictionary and its List
        excludedFiles.Add(appendPath, new Dictionary<string, List<string>>());


    //Now check to see if the the inner Dictionary has the appropriate key
    if (!originalFiles[appendPath].ContainsKey(reportFileNames[0]))
        //If not, make sure they do
        originalFiles[appendPath].Add(reportFileNames[0], new List<string>());

    //Now check to see if the the inner Dictionary has the appropriate key
    if (!modifiedFiles[appendPath].ContainsKey(reportFileNames[1]))
        //If not, make sure they do
        modifiedFiles[appendPath].Add(reportFileNames[1], new List<string>());

    //Now check to see if the the inner Dictionary has the appropriate key
    if (!excludedFiles[appendPath].ContainsKey(reportFileNames[2]))
        //If not, make sure they do
        excludedFiles[appendPath].Add(reportFileNames[2], new List<string>());

    //Now  we can add headers to the various dictionaries
    originalFiles[appendPath][reportFileNames[0]].Add("** The Base Game Contains These Files/Values in Landscape/Materials **");
    originalFiles[appendPath][reportFileNames[0]].Add("");

    modifiedFiles[appendPath][reportFileNames[1]].Add("** The Mod Contains These Files/Values in Landscape/Materials **");
    modifiedFiles[appendPath][reportFileNames[1]].Add("");

    excludedFiles[appendPath][reportFileNames[2]].Add("** These Files Were Not Included In This Mod **");
    excludedFiles[appendPath][reportFileNames[2]].Add("** All Values Remain As Set In The Base Game **");
    excludedFiles[appendPath][reportFileNames[2]].Add("");
}

//Clear the console
Console.Clear();
Console.WriteLine("\u001bc\x1b[3J");

//Logging
Console.WriteLine("Verification process will now begin");

//Formatting
Console.WriteLine("");

//Logging
Console.WriteLine("Now each asset's data from the original game's archive will be verified and stored");

VerifyMasterArchive();

Console.WriteLine("Press enter to verify the stored data");
Console.ReadLine();

//Clear the console
Console.Clear();
Console.WriteLine("\u001bc\x1b[3J");

//Copying isn't working, and I don't care enough to figure out why
//So let's do this the dumb way: write the same data to a file in all directories
foreach (string path in outputPath)
{
    //StreamWriter time
    using (StreamWriter output = new StreamWriter(Path.Combine(path + "\\_Reports", "001_Original_Files_And_Values.txt"), false))
    {
        //Temp List
        List<string> data = new List<string>();

        Console.WriteLine("Processing is running...");

        //Go through each key
        foreach (string file in originalProperties.Keys)
        {
            //Add current file to the list
            data.Add("File: " + file);


            data.Add("> PickAxeDigSize is " + originalProperties[file][0]);
            data.Add("> HitsNeededToMine is " + originalProperties[file][1]);

            //Formatting
            data.Add("");
        }

        Console.WriteLine("Data collected, beginning write to file...");

        //Now write all the data
        foreach (string line in data)
        {
            //Write the line to the file
            output.WriteLine(line);
        }

        //Logging
        Console.WriteLine("File has been written to " + path + "\\_Reports");
    }
}

Console.WriteLine("001_Original_Files_And_Values.txt has been successfully written into the appropriate directories");


Console.WriteLine("");

Console.WriteLine("Press enter to continue");
Console.ReadLine();

//Clear the console
Console.Clear();
Console.WriteLine("\u001bc\x1b[3J");

//Meh, fine I'll integrate it properly-ish
//And because I want a switch, here
//True will prompt you to make a master
//False skips over this
bool shouldCreateMaster = false;

if (shouldCreateMaster)
{
    Console.WriteLine("Do you want to create a new master archive? (Y/N)");
    string? input = Console.ReadLine();

    if (input == "Y")
    {
        Console.WriteLine("Where would you like the new master files to be stored? (Root directory)");
        input = Console.ReadLine();

        //Set location for the new master archive
        string newMasterArchiveDir = input + "\\FSD\\Content\\Landscape\\Materials\\";

        //For making a new master archive
        Console.WriteLine("New master achive will now be created");
        Console.WriteLine("Press enter to continue");
        Console.ReadLine();

        //Clear the console
        Console.Clear();
        Console.WriteLine("\u001bc\x1b[3J");

        //Create the archive
        CreateNewMaster(newMasterArchiveDir);

        //Get out of here
        return;
    }
}


Console.WriteLine("Work will now begin on making the desired mod(s)");

//Wait for input
Console.WriteLine("Press any button to bring processing");
Console.ReadLine();

//Clear the console
Console.Clear();
Console.WriteLine("\u001bc\x1b[3J");

Console.WriteLine("Processing will now begin...");

//Create assets based on the user's selection
//Here we go...
CreateAssets();


//Prompts and other logic to get the needed user input for later processing
//As well as handling verification of the base game's files
void GetRequiredInput()
{
    //Because I want convenience, do a check to see if I've previously used this thing
    if (UserSettings.Default.BaseFilesLocation != "")
    {
        //Logging
        Console.WriteLine("Loading directories from file...");

        //Populate the neccessary variables with the values in the settings file
        inputPath = UserSettings.Default.BaseFilesLocation;

        outputPath.Add(UserSettings.Default.Terrain_DigSize);
        outputPath.Add(UserSettings.Default.Minerals_DigSize);
        outputPath.Add(UserSettings.Default.MineralsAndTerrain_DigSize);
        outputPath.Add(UserSettings.Default.Terrain_OneHit);
        outputPath.Add(UserSettings.Default.Minerals_OneHit);
        outputPath.Add(UserSettings.Default.MineralsAndTerrain_OneHit);
        outputPath.Add(UserSettings.Default.Terrain_DigSize_OneHit);
        outputPath.Add(UserSettings.Default.Minerals_DigSize_OneHit);
        outputPath.Add(UserSettings.Default.MineralsAndTerrain_DigSize_OneHit);

        //Verify
        Console.WriteLine("The base game files are located in:");
        Console.WriteLine("> " + inputPath);

        //Formatting
        Console.WriteLine("");

        //Iterate through the paths to verify them
        foreach (string path in outputPath)
        {
            //Ensure that all folders exist in the given paths
            if (!Directory.Exists(path))
            {
                //Verify
                Console.WriteLine(path + " did not exist, creating... ");

                Directory.CreateDirectory(path);

                //Log the directory
                Console.WriteLine(" > " + path);
            }
            else
                //Log the directory
                Console.WriteLine(" > " + path);

            //We need a few other directories in here
            //_Reports
            //Where reports go
            if (!Directory.Exists(path + "_Reports\\"))
            {
                //Verify
                Console.WriteLine(path + "_Reports\\" + " did not exist, creating... ");

                Directory.CreateDirectory(path + "_Reports\\");

                //Log the directory
                Console.WriteLine(" > " + path + "_Reports\\");
            }
            else
                //Log the directory
                Console.WriteLine(" > " + path + "_Reports\\");

            //_ToEdit
            //Where files go that manually edited
            if (!Directory.Exists(path + "_ToEdit\\"))
            {
                //Verify
                Console.WriteLine(path + "_ToEdit\\" + " did not exist, creating... ");

                Directory.CreateDirectory(path + "_ToEdit\\");

                //Log the directory
                Console.WriteLine(" > " + path + "_ToEdit\\");
            }
            else
                //Log the directory
                Console.WriteLine(" > " + path + "_ToEdit\\");

            //_ExcludedFiles
            //Where files that weren't processed go, for testing/further manual editing
            if (!Directory.Exists(path + "_ExcludedFiles\\"))
            {
                //Verify
                Console.WriteLine(path + "_ExcludedFiles\\" + " did not exist, creating... ");

                Directory.CreateDirectory(path + "_ExcludedFiles\\");

                //Log the directory
                Console.WriteLine(" > " + path + "_ExcludedFiles\\");
            }
            else
                //Log the directory
                Console.WriteLine(" > " + path + "_ExcludedFiles\\");

            //Add a header if this is the first element in the list
            if (path == outputPath.ElementAt(0))
            {
                //Header
                Console.WriteLine("Each mod's working directory is located in: ");
            }
        }

        //Formatting
        Console.WriteLine("");

        //Wait for input
        Console.WriteLine("Press any button to proceed");
        Console.ReadLine();
    }
    //If not, prompt for each directory and make sure it's saved for next time
    else
    {
        //State the needed input
        Console.WriteLine("* Enter the root directory where the master archive's files are located");
        //Get the input
        string? input = Console.ReadLine();

        string bgRootDir = input + "\\FSD\\Content\\Landscape\\Materials\\";

        //Store the input
        UserSettings.Default.BaseFilesLocation = bgRootDir;
        inputPath = bgRootDir;

        //We can do this better
        //Just get the root directory you silly person

        //State the needed input
        Console.WriteLine("* Enter the path where all projects should be stored");
        //Get the input
        input = Console.ReadLine();

        //Set a value to append to the given path
        string[] append = new string[9];

        //000_Terrain_Only_DigSize: 0
        append[0] = input + "\\000_Terrain_Only_DigSize\\FSD\\Content\\Landscape\\Materials\\";
        //001_Minerals_Only_DigSize: 1
        append[1] = input + "\\001_Minerals_Only_DigSize\\FSD\\Content\\Landscape\\Materials\\";
        //002_MineralsAndTerrain_DigSize: 2
        append[2] = input + "\\002_MineralsAndTerrain_DigSize\\FSD\\Content\\Landscape\\Materials\\";
        //003_Terrain_Only_OneHit: 3
        append[3] = input + "\\003_Terrain_Only_OneHit\\FSD\\Content\\Landscape\\Materials\\";
        //004_Minerals_Only_OneHit: 4
        append[4] = input + "\\004_Minerals_Only_OneHit\\FSD\\Content\\Landscape\\Materials\\";
        //005_MineralsAndTerrain_OneHit: 5
        append[5] = input + "\\005_MineralsAndTerrain_OneHit\\FSD\\Content\\Landscape\\Materials\\";
        //006_Terrain_Only_DigSize_OneHit: 6
        append[6] = input + "\\006_Terrain_Only_DigSize_OneHit\\FSD\\Content\\Landscape\\Materials\\";
        //007_Minerals_Only_DigSize_OneHit: 7
        append[7] = input + "\\007_Minerals_Only_DigSize_OneHit\\FSD\\Content\\Landscape\\Materials\\";
        //008_MineralsAndTerrain_DigSize_OneHit: 8
        append[8] = input + "\\008_MineralsAndTerrain_DigSize_OneHit\\FSD\\Content\\Landscape\\Materials\\";

        UserSettings.Default.Terrain_DigSize = append[0];
        UserSettings.Default.Minerals_DigSize = append[1];
        UserSettings.Default.MineralsAndTerrain_DigSize = append[2];
        UserSettings.Default.Terrain_OneHit = append[3];
        UserSettings.Default.Minerals_OneHit = append[4];
        UserSettings.Default.MineralsAndTerrain_OneHit = append[5];
        UserSettings.Default.Terrain_DigSize_OneHit = append[6];
        UserSettings.Default.Minerals_DigSize_OneHit = append[7];
        UserSettings.Default.MineralsAndTerrain_DigSize_OneHit = append[8];

        for (int i = 0; i < 9; i++)
        {
            //Ensure that all folders exist in the given paths
            if (!Directory.Exists(append[i]))
                Directory.CreateDirectory(append[i]);

            //We need a few other directories in here
            //_Reports
            //Where reports go
            if (!Directory.Exists(append[i] + "_Reports\\"))
                Directory.CreateDirectory(append[i] + "_Reports\\");

            //_ToEdit
            //Where files go that manually edited
            if (!Directory.Exists(append[i] + "_ToEdit\\"))
                Directory.CreateDirectory(append[i] + "_ToEdit\\");

            //_ExcludedFiles
            //Where files that weren't processed go, for testing/further manual editing
            if (!Directory.Exists(append[i] + "_ExcludedFiles\\"))
                Directory.CreateDirectory(append[i] + "_ExcludedFiles\\");
        }

        //Save the settings
        UserSettings.Default.Save();

        //Store the generated paths in the outputPath list for easier iteration later
        outputPath.Add(UserSettings.Default.Terrain_DigSize);
        outputPath.Add(UserSettings.Default.Minerals_DigSize);
        outputPath.Add(UserSettings.Default.MineralsAndTerrain_DigSize);
        outputPath.Add(UserSettings.Default.Terrain_OneHit);
        outputPath.Add(UserSettings.Default.Minerals_OneHit);
        outputPath.Add(UserSettings.Default.MineralsAndTerrain_OneHit);
        outputPath.Add(UserSettings.Default.Terrain_DigSize_OneHit);
        outputPath.Add(UserSettings.Default.Minerals_DigSize_OneHit);
        outputPath.Add(UserSettings.Default.MineralsAndTerrain_DigSize_OneHit);

        //Clear the console
        //.Clear on its own doesn't work for me, for whatever reason, so let's try a random thing I found online
        Console.Clear();
        Console.WriteLine("\u001bc\x1b[3J");

        //Verify
        Console.WriteLine("");
        Console.WriteLine("The following paths have been saved");

        foreach (string path in outputPath)
            Console.WriteLine(path);

        Console.WriteLine("The base game files are located in " + UserSettings.Default.BaseFilesLocation);

        //Formatting
        Console.WriteLine("");

        //Verify
        Console.WriteLine("");
        Console.WriteLine("The following paths now exist in the project's root directory");

        foreach (string dir in Directory.GetDirectories(input ?? "F:\\DRG_Modding\\Mods\\BigDigBoi Series\\_Main\\", "*", SearchOption.AllDirectories))
            Console.WriteLine("> " + dir);

        //Wait for input
        Console.WriteLine("Press any button to proceed");
        Console.ReadLine();
    }

    //Clear the console
    //.Clear on its own doesn't work for me, for whatever reason, so let's try a random thing I found online
    Console.Clear();
    Console.WriteLine("\u001bc\x1b[3J");

    //Find out which mod(s) to make
    Console.WriteLine("Which mod would you like to create?");
    Console.WriteLine("** The following options are available ** ");
    Console.WriteLine("> Don't do anything: -1");
    Console.WriteLine("> Terrain_Only_DigSize: 0");
    Console.WriteLine("> Minerals_Only_DigSize: 1");
    Console.WriteLine("> MineralsAndTerrain_DigSize: 2");
    Console.WriteLine("> Terrain_Only_OneHit: 3");
    Console.WriteLine("> Minerals_Only_OneHit: 4");
    Console.WriteLine("> MineralsAndTerrain_OneHit: 5");
    Console.WriteLine("> Terrain_Only_DigSize_OneHit: 6");
    Console.WriteLine("> Minerals_Only_DigSize_OneHit: 7");
    Console.WriteLine("> MineralsAndTerrain_DigSize_OneHit: 8");
    Console.WriteLine("> All the above: 100");

    //Formatting
    Console.WriteLine("");

    //Store input temporarily as a nullable string
    string? tmp = Console.ReadLine();

    //Parse the string into an int, and in case it's null, make sure it has an alternate value of -1 just in case
    whichMod = int.Parse(tmp ?? "-1");

    //Formatting
    Console.WriteLine("");

    //If the user has selected -1, just exit the program
    if (whichMod == -1)
    {
        //Logging
        Console.WriteLine("-1 has been selected, the program will now exit");

        //Verify
        Console.WriteLine("Press any button to exit");

        //Wait for input
        Console.ReadLine();

        //Exit the program
        Environment.Exit(0);
    }

    //Depending on the desired mods to be created, prompt the user for further information
    //If the user wantes to adjust dig size, then 0, 1, 2, 6, 7, 8 will be selected
    if (whichMod == 0 || whichMod == 1 || whichMod == 2 || whichMod == 6 || whichMod == 7 || whichMod == 8)
    {
        //Get the desired PickAxeDigSize
        Console.WriteLine("How big do you want PickAxeDigSize to be?");
        Console.WriteLine("* Base game default value: 105");
        Console.WriteLine("* My original value: 225");
        Console.WriteLine("* For reference, drills have a dig size of 150 (depth) and 200/200 (width/height)");

        //Store input in tmp string
        tmp = Console.ReadLine();

        //Parse the string to an int, and if it's null, set it to the game's default of 105
        masterDigSize = float.Parse(tmp ?? "105");

        //Formatting
        Console.WriteLine("");
    }
    //Otherwise, just throw the default value in there (it won't be used anyway)
    else
        masterDigSize = 105;

    //If the user wantes to adjust dig size, then 3, 4, 5, 6, 7, 8 will be selected
    if (whichMod == 3 || whichMod == 4 || whichMod == 5 || whichMod == 6 || whichMod == 7 || whichMod == 8)
    {
        //Get the desired HitsNeededToMine
        Console.WriteLine("What should the value of HitsNeededToMine be?");
        Console.WriteLine("* Base game default value: 2");

        //Store input in tmp string
        tmp = Console.ReadLine();

        //Parse the string to an int, and if it's null, set it to the game's default of 2
        masterHitsNeeded = int.Parse(tmp ?? "2");
    }
    //Otherwise, just throw the default value in there (it won't be used anyway)
    else
        masterHitsNeeded = 2;

    if (whichMod == 100)
    {
        //Get the desired PickAxeDigSize
        Console.WriteLine("How big do you want PickAxeDigSize to be?");
        Console.WriteLine("* Base game default value: 105");
        Console.WriteLine("* My original value: 225");
        Console.WriteLine("* For reference, drills have a dig size of 150 (depth) and 200/200 (width/height)");

        //Store input in tmp string
        tmp = Console.ReadLine();

        //Parse the string to an int, and if it's null, set it to the game's default of 105
        masterDigSize = float.Parse(tmp ?? "105");

        //Formatting
        Console.WriteLine("");

        //Get the desired HitsNeededToMine
        Console.WriteLine("What should the value of HitsNeededToMine be?");
        Console.WriteLine("* Base game default value: 2");

        //Store input in tmp string
        tmp = Console.ReadLine();

        //Parse the string to an int, and if it's null, set it to the game's default of 2
        masterHitsNeeded = int.Parse(tmp ?? "2");
    }

    //Verify
    Console.WriteLine("You have chosen option: " + whichMod);

    //Check and verify dig size
    if (masterDigSize != 105)
        Console.WriteLine("A dig size of " + masterDigSize + " has been chosen");
    else
        Console.WriteLine("PickAxeDigSize will remain at each file's default");

    //Check and verify hits needed
    if (masterHitsNeeded != 2)
        Console.WriteLine("Hits needed to mine has been set to " + masterHitsNeeded);
    else
        Console.WriteLine("HitsNeededToMine will remain at each file's default");

    //Formatting
    Console.WriteLine("");

    //Wait for input before continuing
    Console.WriteLine("Press any button to continue");
    Console.ReadLine();
}

//Evaulates the unaltered base game archive and stores the values for later use
void VerifyMasterArchive()
{
    //Iterate through all files in the inputPath, logging relevant information for each one
    foreach (string file in Directory.EnumerateFiles(originalFileLoc, "*.uasset", SearchOption.AllDirectories))
    {
        //Create a new UAsset for this file
        UAsset currentAsset = new UAsset(file, EngineVersion.VER_UE4_27);

        //Get the location of the desired variables
        NormalExport assetExport = (NormalExport)currentAsset.Exports[0];

        //Get the desired values from assetExport
        FloatPropertyData digSize = (FloatPropertyData)assetExport["PickAxeDigSize"];
        IntPropertyData hitsNeeded = (IntPropertyData)assetExport["HitsNeededToMine"];

        //Show which file is being analyzed
        Console.WriteLine("File " + assetExport.ObjectName + ":");

        //Add this file the originalProperties Dictionary and initialize the internal list
        originalProperties.Add(assetExport.ObjectName.ToString(), new List<float>());


        //Check the current file to see if it contains PickAxeDigSize
        if ((FloatPropertyData)assetExport["PickAxeDigSize"] != null)
        {
            //Logging
            Console.WriteLine("* Contains a PickAxeDigSize property with a value of " + digSize.Value);

            //Add to this asset's list
            originalProperties[assetExport.ObjectName.ToString()].Add(digSize.Value);
        }
        else
        {
            //Logging
            Console.WriteLine("* PickAxeDigSize is not available, the default value is 105");

            //Add to this asset's list
            originalProperties[assetExport.ObjectName.ToString()].Add(105);
        }

        //Check the current file to see if it contains HitsNeededToMine
        if ((IntPropertyData)assetExport["HitsNeededToMine"] != null)
        {
            //Logging
            Console.WriteLine("* Contains a HitsNeededToMine property with a value of " + hitsNeeded.Value);

            //Add to this asset's list
            originalProperties[assetExport.ObjectName.ToString()].Add(hitsNeeded.Value);
        }
        else
        {
            //Logging
            Console.WriteLine("* HitsNeededToMine is not available, the default value is 2");

            //Add to this asset's list
            originalProperties[assetExport.ObjectName.ToString()].Add(2);
        }

        //Because I like formatting
        //Add some fluff for easier reading
        Console.WriteLine("--------");
    }
}


//For purposes of making a new master archive
void CreateNewMaster(string newMasterArchiveDir)
{
    //Store the asset's data for writing to a file
    List<string> data = new List<string>();

    //Make sure the specific folders exist
    if (!Directory.Exists(newMasterArchiveDir))
    {
        //Verify
        Console.WriteLine("Path " + newMasterArchiveDir + " did not exist, creating... ");

        Directory.CreateDirectory(newMasterArchiveDir);

        //Log the directory
        Console.WriteLine(" > Created " + newMasterArchiveDir);
    }
    else
        //Log the directory
        Console.WriteLine("** " + newMasterArchiveDir + " already exists");

    //Iterate through all files in the inputPath, logging relevant information for each one
    foreach (string file in Directory.EnumerateFiles(UserSettings.Default.BaseFilesLocation, "*.uasset", SearchOption.AllDirectories))
    {
        //Create a new UAsset for this file
        UAsset asset = new UAsset(file, EngineVersion.VER_UE4_27);

        //Get the location of the desired variables
        NormalExport export = (NormalExport)asset.Exports[0];

        //Get the desired values from assetExport
        FloatPropertyData digSize = (FloatPropertyData)export["PickAxeDigSize"];
        IntPropertyData hitsNeeded = (IntPropertyData)export["HitsNeededToMine"];

        //Show which file is being analyzed
        data.Add("File " + export.ObjectName + ":");
        Console.WriteLine("File " + export.ObjectName + ":");

        //Check to make sure PickAxeDigSize isn't null
        if ((FloatPropertyData)export["PickAxeDigSize"] != null)
        {
            //Get the value from the originalProperties Dictionary
            float oSize = originalProperties[asset.Exports[0].ObjectName.ToString()][0];

            //Get the value from the asset in question
            FloatPropertyData nSize = (FloatPropertyData)export["PickAxeDigSize"];

            //If the new size in the given asset isn't the same, it needs reverted to the base game value
            if (nSize.Value != oSize)
            {
                //Logging
                data.Add("> This file's PickAxeDigSize is not the same as the base game's value: " + nSize.Value + " and should be " + oSize);
                Console.WriteLine("> This file's PickAxeDigSize is not the same as the base game's value: " + nSize.Value + " and should be " + oSize);

                //Fix: set the asset's PickAxeDigSize to the value it is in the base game
                nSize.Value = oSize;

                //Logging
                data.Add("** This file's PickAxeDigSize has been set to the base game's value of " + oSize);
                Console.WriteLine("** This file's PickAxeDigSize has been set to the base game's value of " + oSize);
            }
            else
            {
                //Logging
                data.Add("** This file's PickAxeDigSize is the value it should be (" + oSize + ")");
                Console.WriteLine("** This file's PickAxeDigSize is the value it should be (" + oSize + ")");
            }
        }

        //Check to make sure HitsNeededToMine isn't null
        if ((IntPropertyData)export["HitsNeededToMine"] != null)
        {
            //Get the value from the originalProperties Dictionary
            int oHits = (int)originalProperties[asset.Exports[0].ObjectName.ToString()][1];

            //Get the value from the asset in question
            IntPropertyData nHits = (IntPropertyData)export["HitsNeededToMine"];

            //If the new size in the given asset isn't the same, it needs reverted to the base game value
            if (nHits.Value != oHits)
            {
                //Logging
                data.Add("> This file's HitsNeededToMine is not the same as the base game's value: " + nHits.Value + " and should be " + oHits);
                Console.WriteLine("> This file's HitsNeededToMine is not the same as the base game's value: " + nHits.Value + " and should be " + oHits);

                //Fix: set the asset's HitsNeededToMine to the value it is in the base game
                nHits.Value = oHits;

                //Logging
                data.Add("** This file's HitsNeededToMine has been set to the base game's value of " + oHits);
                Console.WriteLine("** This file's HitsNeededToMine has been set to the base game's value of " + oHits);
            }
            else
            {
                //Logging
                data.Add("** This file's HitsNeededToMine is the value it should be (" + oHits + ")");
                Console.WriteLine("** This file's HitsNeededToMine is the value it should be (" + oHits + ")");
            }
        }

        //Formatting
        data.Add("---------");
        Console.WriteLine("---------");

        //Logging
        Console.WriteLine("Writing " + export.ObjectName + " to " + newMasterArchiveDir);

        //Write the reverted asset to the specificed location
        asset.Write(newMasterArchiveDir + asset.Exports[0].ObjectName + ".uasset");

        //Logging
        Console.WriteLine("File has been written");
    }

    //Write data to file
    using (StreamWriter output = new StreamWriter(Path.Combine(newMasterArchiveDir, "_Master_Archive_Report.txt"), false))
    {
        //Iterate through each line and write it to this file
        foreach (string line in data)
        {
            //Write the line to the file
            output.WriteLine(line);
        }
    }

}


//Because the main logic for both terrain and minerals is identical, there's no point copy/pasting all of it into both functions
//So here's a centralized location for the asset creation logic
//
//This whole thing could be simplified by offloading duplicate code into its own function
//Later maybe
//... Fine I'll do it
void CreateAssets()
{
    //Logging
    Console.WriteLine("Asset creation is now starting");

    //Check to see if all mods or one mod needs to be made
    if (whichMod != 100)
    {
        //Get all uassets in Landscape\Materials and their associated directory
        //Main directory: /Materials
        foreach (string file in Directory.EnumerateFiles(inputPath, "*.uasset", SearchOption.AllDirectories))
        {
            //Create a new UAsset for this file
            UAsset currentAsset = new UAsset(file, EngineVersion.VER_UE4_27);

            //Get the location of the desired variable
            NormalExport assetExport = (NormalExport)currentAsset.Exports[0];

            //Logging
            Console.WriteLine("Analyzing " + assetExport.ObjectName + "...");

            //Modify PickAxeDigSize for which ever files need to be changed based on whichMod
            if (whichMod == 0 || whichMod == 1 || whichMod == 2 || whichMod == 6 || whichMod == 7 || whichMod == 8)
            {
                //Modify the property
                ModifyAssetFloatValues(currentAsset, assetExport);
            }

            //Modify HitsNeededToMine for which ever files need to be changed based on whichMod
            if (whichMod == 3 || whichMod == 4 || whichMod == 5 || whichMod == 6 || whichMod == 7 || whichMod == 8)
            {
                //Modify the property
                ModifyAssetIntValues(currentAsset, assetExport);
            }

            //Modify both PickAxeDigSpeed and HitsNeededToMine for which ever files need to be changed based on whichMod
            if (whichMod == 6 || whichMod == 7 || whichMod == 8)
            {
                //Modify the property
                ModifyAssetValues(currentAsset, assetExport);
            }
        }
    }

    //Do all the things
    if (whichMod == 100)
    {
        //whichMod is 100, which is great, but breaks all of the logic I've written
        //So instead of letting it break or re-writing everything
        //A loop which sets whichMod to 0 and iterates through it and these functions to create all mods
        for (int i = 0; i < 9; i++)
        {
            //Set whichMod to i
            whichMod = i;

            foreach (string file in Directory.EnumerateFiles(inputPath, "*.uasset", SearchOption.AllDirectories))
            {
                //Create a new UAsset for this file
                UAsset currentAsset = new UAsset(file, EngineVersion.VER_UE4_27);

                //Get the location of the desired variable
                NormalExport assetExport = (NormalExport)currentAsset.Exports[0];

                //Logging
                Console.WriteLine("Analyzing " + assetExport.ObjectName + "...");

                //Modify PickAxeDigSize for which ever files need to be changed based on whichMod
                if (whichMod == 0 || whichMod == 1 || whichMod == 2 || whichMod == 6 || whichMod == 7 || whichMod == 8)
                {
                    //Modify the property
                    ModifyAssetFloatValues(currentAsset, assetExport);
                }

                //Modify HitsNeededToMine for which ever files need to be changed based on whichMod
                if (whichMod == 3 || whichMod == 4 || whichMod == 5 || whichMod == 6 || whichMod == 7 || whichMod == 8)
                {
                    //Modify the property
                    ModifyAssetIntValues(currentAsset, assetExport);
                }

                //Modify both PickAxeDigSpeed and HitsNeededToMine for which ever files need to be changed based on whichMod
                if (whichMod == 6 || whichMod == 7 || whichMod == 8)
                {
                    //Modify the property
                    ModifyAssetValues(currentAsset, assetExport);
                }
            }
        }
    }

    //Logging
    Console.WriteLine("All files have been modified");
    Console.WriteLine("All relevant information will now be written to the appropriate file(s)");
    Console.WriteLine("");
    Console.WriteLine("Please press any button to proceed");

    //Wait for input
    Console.ReadLine();

    //Clear the console
    Console.Clear();
    Console.WriteLine("\u001bc\x1b[3J");

    //Logging
    Console.WriteLine("Beginning file writing now...");

    //Write all modifications to a file
    WriteToFile(reportFileNames[1], modifiedFiles);

    //Write all exlusions to a file
    WriteToFile(reportFileNames[2], excludedFiles);
}

//Function that takes in a UAsset, NormalExport, string for the property data name
//Specifically for PickAxeDigSize because it's a float
//For now, this doesn't need to be generic, so yeah
void ModifyAssetFloatValues(UAsset asset, NormalExport export)
{
    //Get the desired value from export
    digSize = (FloatPropertyData)export["PickAxeDigSize"];

    //Terrain_DigSize
    if (whichMod == 0)
    {
        //Check to see if the current file is for a mineral and needs to be skipped
        if (mineralFiles.Contains(export.ObjectName.ToString()))
        {
            //Logging
            excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
            Console.WriteLine("* This file is for a mineral and will be excluded");

            //Get out of here
            return;
        }

        //Because of the archive being used, we need to make sure this file's HitsNeededToMine is the intended value
        //Check to make sure it has the value
        //Needs to be here to make sure that any reversions to HitsNeededToMine are preserved
        if ((IntPropertyData)export["HitsNeededToMine"] != null)
        {
            //Get the value from the originalProperties Dictionary
            int oHits = (int)originalProperties[asset.Exports[0].ObjectName.ToString()][1];

            //Get the value from the asset in question
            IntPropertyData nHits = (IntPropertyData)export["HitsNeededToMine"];

            if (nHits.Value != oHits)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededToMine is not the same as the base game's value.");
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The value should be " + oHits + " but is " + nHits);
                Console.WriteLine("This file's HitsNeededToMine is not the same as the base game's value");

                //Fix: set the asset's HitsNeededToMine to the value it is in the base game
                nHits.Value = oHits;

                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededToMine has been set to the base game's value: " + nHits.Value);
                Console.WriteLine("This file's HitsNeededToMine has been set to the base game's value");
            }
        }

        //Check to make sure digSize isn't already masterDigSize
        if ((FloatPropertyData)export["PickAxeDigSize"] != null && digSize.Value == masterDigSize)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize is already at the desired size.");
            Console.WriteLine("This file's PickAxeDigSize is already at the desired size");

            //If so, copy it as-is to the desired directory
            asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

            //Get out of here
            return;
        }

        //Check to see if digSize is bigger than masterDigSize
        if ((FloatPropertyData)export["PickAxeDigSize"] != null && digSize.Value > masterDigSize)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize is bigger than the desired size.");
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value will be preserved instead of setting it to the desired size.");
            Console.WriteLine("This file's PickAxeDigSize is bigger than the desired size");

            //If so, copy it as-is to the desired directory
            asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

            //Get out of here
            return;
        }

        //Check the current file to make sure it contains PickAxeDigSize, otherwise don't save it
        if ((FloatPropertyData)export["PickAxeDigSize"] != null)
        {
            //Logging
            //If the header for this file doesn't exist, create it
            if (!modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Contains("File: " + export.ObjectName))
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);


            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of PickAxeDigSize was " + digSize.Value + ".");
            Console.WriteLine("* The original value of PickAxeDigSize was " + digSize.Value + ".");

            //Modify the PickAxeDigSize value
            digSize.Value = masterDigSize;

            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + digSize.Value + ".");
            Console.WriteLine("* The new value is " + digSize.Value + ".");

            //Write the asset to the desired directory
            asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

            Console.WriteLine("File " + export.ObjectName + " has been saved in: " + outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");
        }

        //If PickAxeDigSize is null, add it to the exclusions list
        //Added here to make sure HitsNeededToMine is modified if needed
        if ((FloatPropertyData)export["PickAxeDigSize"] == null)
        {
            //Logging
            excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
            Console.WriteLine("* PickAxeDigSize are not readily exposed for this file and will be stored in \\_ExcludedFiles\\");

            //If the file is in a subdirectory, make sure that's preserved
            //So get the current asset's original path in the base game files
            string? originalPath = asset.FilePath;

            //This "dereference of a possibly null reference" thing is annoying, really annoying
            //originalPath ?? "stuff" didn't fix it this time, so there, disabled it in settings
            //Take that Visual Studio and your weirdness
            string parentPath = new DirectoryInfo(originalPath).Parent.Name;

            //Moving on...
            //Add the above parentPath to the outputPath and add _ExcludedFiles in there as well
            string finalPath = outputPath[whichMod] + parentPath + "\\_ExcludedFiles\\";

            //Ensure that all folders exist in the given path
            if (!Directory.Exists(finalPath))
                Directory.CreateDirectory(finalPath);

            //Write the asset to the desired directory for later editing and stuff
            asset.Write(finalPath + export.ObjectName + ".uasset");
        }
    }

    //Minerals_DigSize
    if (whichMod == 1)
    {
        //If this is a mineral file, we can proceed
        if (mineralFiles.Contains(export.ObjectName.ToString()))
        {
            //Because of the archive being used, we need to make sure this file's HitsNeededToMine is the intended value
            //Check to make sure it has the value
            //Needs to be here to make sure that any reversions to HitsNeededToMine are preserved
            if ((IntPropertyData)export["HitsNeededToMine"] != null)
            {
                //Get the value from the originalProperties Dictionary
                int oHits = (int)originalProperties[asset.Exports[0].ObjectName.ToString()][1];

                //Get the value from the asset in question
                IntPropertyData nHits = (IntPropertyData)export["HitsNeededToMine"];

                if (nHits.Value != oHits)
                {
                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededToMine is not the same as the base game's value.");
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The value should be " + oHits + " but is " + nHits);
                    Console.WriteLine("This file's HitsNeededToMine is not the same as the base game's value");

                    //Fix: set the asset's HitsNeededToMine to the value it is in the base game
                    nHits.Value = oHits;

                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededToMine has been set to the base game's value: " + nHits.Value);
                    Console.WriteLine("This file's HitsNeededToMine has been set to the base game's value");
                }
            }

            //Check to make sure digSize isn't already masterDigSize
            if ((FloatPropertyData)export["PickAxeDigSize"] != null && digSize.Value == masterDigSize)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize is already at the desired size.");
                Console.WriteLine("This file's PickAxeDigSize is already at the desired size");

                //If so, copy it as-is to the desired directory
                asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

                //Get out of here
                return;
            }

            //Check to see if digSize is bigger than masterDigSize
            if ((FloatPropertyData)export["PickAxeDigSize"] != null && digSize.Value > masterDigSize)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize is bigger than the desired size.");
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value will be preserved instead of setting it to the desired size.");
                Console.WriteLine("This file's PickAxeDigSize is bigger than the desired size");

                //If so, copy it as-is to the desired directory
                asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

                //Get out of here
                return;
            }

            //Check the current file to make sure it contains PickAxeDigSize, otherwise it needs special handling
            if ((FloatPropertyData)export["PickAxeDigSize"] != null)
            {
                //Logging
                //If the header for this file doesn't exist, create it
                if (!modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Contains("File: " + export.ObjectName))
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);

                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of PickAxeDigSize was " + digSize.Value + ".");
                Console.WriteLine("* The original value of PickAxeDigSize was " + digSize.Value + ".");

                //Modify the PickAxeDigSize value
                digSize.Value = masterDigSize;

                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + digSize.Value + ".");
                Console.WriteLine("* The new value is " + digSize.Value + ".");

                //Write the asset to the desired directory
                asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

                //Logging
                Console.WriteLine("File " + export.ObjectName + " has been saved in: " + outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");
            }
            //Otherwise, add this property to the file with the specified value
            else
            {
                //Create a temp path
                string newPath = outputPath[whichMod] + "_ToEdit\\";

                //Instead, write it to the intended directy, but in its own folder, _ToEdit
                if (!Directory.Exists(newPath))
                {
                    //Verify
                    Console.WriteLine(newPath + " did not exist, creating... ");

                    //Create it
                    Directory.CreateDirectory(newPath);

                    //Verify
                    Console.WriteLine("> " + newPath + " now exists");
                }

                //Verify
                excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
                Console.WriteLine("> " + export.ObjectName + " cannot have PickAxeDigSize added programatically at this time");
                Console.WriteLine("> it has been excluded and stored in " + newPath + "\\" + export.ObjectName + ".uasset/.uexp for manually editing");

                //Write to file
                asset.Write(newPath + asset.Exports[0].ObjectName.ToString() + ".uasset");

                //Save settings to a file for later reference
                if (!File.Exists(newPath + "_Settings.txt"))
                    //Create the StreamWriter
                    using (StreamWriter output = new StreamWriter(Path.Combine(newPath, "_Settings.txt"), false))
                        //Write the settings to the file
                        output.WriteLine("PickAxeDigSize: " + masterDigSize);
            }

            //If PickAxeDigSize is null, add it to the exclusions list
            //Added here to make sure HitsNeededToMine is modified if needed
            if ((FloatPropertyData)export["PickAxeDigSize"] == null)
            {
                //Logging
                excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
                Console.WriteLine("* PickAxeDigSize are not readily exposed for this file and will be stored in \\_ExcludedFiles\\");

                //If the file is in a subdirectory, make sure that's preserved
                //So get the current asset's original path in the base game files
                string? originalPath = asset.FilePath;

                //This "dereference of a possibly null reference" thing is annoying, really annoying
                //originalPath ?? "stuff" didn't fix it this time, so there, disabled it in settings
                //Take that Visual Studio and your weirdness
                string parentPath = new DirectoryInfo(originalPath).Parent.Name;

                //Moving on...
                //Add the above parentPath to the outputPath and add _ExcludedFiles in there as well
                string finalPath = outputPath[whichMod] + parentPath + "\\_ExcludedFiles\\";

                //Ensure that all folders exist in the given path
                if (!Directory.Exists(finalPath))
                    Directory.CreateDirectory(finalPath);

                //Write the asset to the desired directory for later editing and stuff
                asset.Write(finalPath + export.ObjectName + ".uasset");
            }
        }
        //This file is not for a mineral
        else
        {
            //Logging
            excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
            Console.WriteLine("* " + export.ObjectName + " is not a mineral and will be excluded");
        }
    }

    //MineralsAndTerrain_DigSize
    if (whichMod == 2)
    {
        //Because of the archive being used, we need to make sure this file's HitsNeededToMine is the intended value
        //Check to make sure it has the value
        //Needs to be here to make sure that any reversions to HitsNeededToMine are preserved
        if ((IntPropertyData)export["HitsNeededToMine"] != null)
        {
            //Get the value from the originalProperties Dictionary
            int oHits = (int)originalProperties[asset.Exports[0].ObjectName.ToString()][1];

            //Get the value from the asset in question
            IntPropertyData nHits = (IntPropertyData)export["HitsNeededToMine"];

            if (nHits.Value != oHits)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededToMine is not the same as the base game's value.");
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The value should be " + oHits + " but is " + nHits);
                Console.WriteLine("This file's HitsNeededToMine is not the same as the base game's value");

                //Fix: set the asset's HitsNeededToMine to the value it is in the base game
                nHits.Value = oHits;

                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededToMine has been set to the base game's value: " + nHits.Value);
                Console.WriteLine("This file's HitsNeededToMine has been set to the base game's value");
            }
        }

        //Check to make sure digSize isn't already masterDigSize
        if ((FloatPropertyData)export["PickAxeDigSize"] != null && digSize.Value == masterDigSize)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize is already at the desired size.");
            Console.WriteLine("This file's PickAxeDigSize is already at the desired size");

            //If so, copy it as-is to the desired directory
            asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

            //Get out of here
            return;
        }

        //Check to see if digSize is bigger than masterDigSize
        if ((FloatPropertyData)export["PickAxeDigSize"] != null && digSize.Value > masterDigSize)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize is bigger than the desired size.");
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value will be preserved instead of setting it to the desired size.");
            Console.WriteLine("This file's PickAxeDigSize is bigger than the desired size");

            //If so, copy it as-is to the desired directory
            asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

            //Get out of here
            return;
        }

        //If this is a mineral file, we can proceed
        if (mineralFiles.Contains(export.ObjectName.ToString()))
        {
            //Check to make sure digSize isn't already masterDigSize
            if ((FloatPropertyData)export["PickAxeDigSize"] != null && digSize.Value == masterDigSize)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize is already at the desired size.");
                Console.WriteLine("This file's PickAxeDigSize is already at the desired size");

                //If so, copy it as-is to the desired directory
                asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

                //Get out of here
                return;
            }

            //Check the current file to make sure it contains PickAxeDigSize, otherwise it needs special handling
            if ((FloatPropertyData)export["PickAxeDigSize"] != null)
            {
                //Logging
                //If the header for this file doesn't exist, create it
                if (!modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Contains("File: " + export.ObjectName))
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);

                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of PickAxeDigSize was " + digSize.Value + ".");
                Console.WriteLine("* The original value of PickAxeDigSize was " + digSize.Value + ".");

                //Modify the PickAxeDigSize value
                digSize.Value = masterDigSize;

                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + digSize.Value + ".");
                Console.WriteLine("* The new value is " + digSize.Value + ".");

                //Write the asset to the desired directory
                asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

                //Logging
                Console.WriteLine("File " + export.ObjectName + " has been saved in: " + outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");
            }
            //Otherwise, add this property to the file with the specified value
            else
            {
                //Create a temp path
                string newPath = outputPath[whichMod] + "_ToEdit\\";

                //Instead, write it to the intended directy, but in its own folder, _ToEdit
                if (!Directory.Exists(newPath))
                {
                    //Verify
                    Console.WriteLine(newPath + " did not exist, creating... ");

                    //Create it
                    Directory.CreateDirectory(newPath);

                    //Verify
                    Console.WriteLine("> " + newPath + " now exists");
                }

                //Verify
                excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
                Console.WriteLine("> " + export.ObjectName + " cannot have PickAxeDigSize added programatically at this time");
                Console.WriteLine("> it has been excluded and stored in " + newPath + "\\" + export.ObjectName + ".uasset/.uexp for manually editing");

                //Write to file
                asset.Write(newPath + asset.Exports[0].ObjectName.ToString() + ".uasset");

                //Save settings to a file for later reference
                if (!File.Exists(newPath + "_Settings.txt"))
                    //Create the StreamWriter
                    using (StreamWriter output = new StreamWriter(Path.Combine(newPath, "_Settings.txt"), false))
                        //Write the settings to the file
                        output.WriteLine("PickAxeDigSize: " + masterDigSize);
            }
        }
        //Otherwise it's a terrain file
        else
        {
            //Check to make sure digSize isn't already masterDigSize
            if ((FloatPropertyData)export["PickAxeDigSize"] != null && digSize.Value == masterDigSize)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize is already at the desired size.");
                Console.WriteLine("This file's PickAxeDigSize is already at the desired size");

                //If so, copy it as-is to the desired directory
                asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

                //Get out of here
                return;
            }

            //Check the current file to make sure it contains PickAxeDigSize, otherwise it needs excluded
            if ((FloatPropertyData)export["PickAxeDigSize"] != null)
            {
                //Logging
                //If the header for this file doesn't exist, create it
                if (!modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Contains("File: " + export.ObjectName))
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);

                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of PickAxeDigSize was " + digSize.Value + ".");
                Console.WriteLine("* The original value of PickAxeDigSize was " + digSize.Value + ".");

                //Modify the PickAxeDigSize value
                digSize.Value = masterDigSize;

                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + digSize.Value + ".");
                Console.WriteLine("* The new value is " + digSize.Value + ".");

                //Write the asset to the desired directory
                asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

                //Logging
                Console.WriteLine("File " + export.ObjectName + " has been saved in: " + outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");
            }
        }

        //If PickAxeDigSize is null, add it to the exclusions list
        //Added here to make sure HitsNeededToMine is modified if needed
        if ((FloatPropertyData)export["PickAxeDigSize"] == null)
        {
            //Logging
            excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
            Console.WriteLine("* PickAxeDigSize are not readily exposed for this file and will be stored in \\_ExcludedFiles\\");

            //If the file is in a subdirectory, make sure that's preserved
            //So get the current asset's original path in the base game files
            string? originalPath = asset.FilePath;

            //This "dereference of a possibly null reference" thing is annoying, really annoying
            //originalPath ?? "stuff" didn't fix it this time, so there, disabled it in settings
            //Take that Visual Studio and your weirdness
            string parentPath = new DirectoryInfo(originalPath).Parent.Name;

            //Moving on...
            //Add the above parentPath to the outputPath and add _ExcludedFiles in there as well
            string finalPath = outputPath[whichMod] + parentPath + "\\_ExcludedFiles\\";

            //Ensure that all folders exist in the given path
            if (!Directory.Exists(finalPath))
                Directory.CreateDirectory(finalPath);

            //Write the asset to the desired directory for later editing and stuff
            asset.Write(finalPath + export.ObjectName + ".uasset");
        }
    }
}

//Function that takes in a UAsset, NormalExport, string for the property data name
//Specifically for HitsNeededToMine because it's an int
//For now, this doesn't need to be generic, so yeah
void ModifyAssetIntValues(UAsset asset, NormalExport export)
{
    //Get the desired value from export
    hitsNeeded = (IntPropertyData)export["HitsNeededToMine"];

    //Terrain_OneHit
    if (whichMod == 3)
    {
        //First verify that this file is a mineral as determined by the mineralFiles array
        //Check to see if the current file is for a mineral and needs to be skipped
        if (mineralFiles.Contains(export.ObjectName.ToString()))
        {
            //Logging
            excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
            Console.WriteLine("* This file is for a mineral and will be excluded");

            //Get out of here
            return;
        }

        //Because of the archive being used, we need to make sure this file's PickAxeDigSize is the intended value
        //Check to make sure it has the value
        //Needs to be here to make sure that any reversions to PickAxeDigSize are preserved
        if ((FloatPropertyData)export["PickAxeDigSize"] != null)
        {
            //Get the value from the originalProperties Dictionary
            float oSize = originalProperties[asset.Exports[0].ObjectName.ToString()][0];

            //Get the value from the asset in question
            FloatPropertyData nSize = (FloatPropertyData)export["PickAxeDigSize"];

            if (nSize.Value != oSize)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize is not the same as the base game's value.");
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The value should be " + oSize + " but is " + nSize);
                Console.WriteLine("This file's PickAxeDigSize is not the same as the base game's value");

                //Fix: set the asset's PickAxeDigSize to the value it is in the base game
                nSize.Value = oSize;

                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize has been set to the base game's value: " + nSize.Value);
                Console.WriteLine("This file's PickAxeDigSize has been set to the base game's value");
            }
        }

        //Check to make sure hitsNeeded isn't already masterHitSize
        if ((IntPropertyData)export["HitsNeededToMine"] != null && hitsNeeded.Value == masterHitsNeeded)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededToMine is already at the desired hits.");
            Console.WriteLine("This file's HitsNeededToMine is already at the desired hits");

            //If so, copy it as-is to the desired directory
            asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

            //If so, get out of here
            return;
        }

        //Check to see if hitsNeeded is smaller than masterHitSize, and check to make sure hitsNeeded isn't 1
        if ((IntPropertyData)export["HitsNeededToMine"] != null && hitsNeeded.Value < masterHitsNeeded && hitsNeeded.Value != 1)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededTo is smaller than the desired hits.");
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value will be preserved instead of setting it to the desired hits.");
            Console.WriteLine("This file's HitsNeededToMine is bigger than the desired hits");

            //If so, copy it as-is to the desired directory
            asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

            //Get out of here
            return;
        }

        //Check to make sure digSize isn't already masterDigSize
        if ((IntPropertyData)export["HitsNeededToMine"] != null && hitsNeeded.Value == masterHitsNeeded)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededToMine is already at the desired hits.");
            Console.WriteLine("This file's HitsNeededToMine is already at the desired hits");

            //If so, copy it as-is to the desired directory
            asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

            //If so, get out of here
            return;
        }

        //Check the current file to make sure it contains HitsNeededToMine, otherwise don't save it
        if ((IntPropertyData)export["HitsNeededToMine"] != null)
        {
            //Logging
            //If the header for this file doesn't exist, create it
            if (!modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Contains("File: " + export.ObjectName))
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);

            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");
            Console.WriteLine("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");

            //Modify the HitsNeededToMine value
            hitsNeeded.Value = masterHitsNeeded;

            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + hitsNeeded.Value + ".");
            Console.WriteLine("* The new value is " + hitsNeeded.Value + ".");

            //Write the asset to the desired directory
            asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

            Console.WriteLine("File " + export.ObjectName + " has been saved in: " + outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");
        }

        //If HitsNeededToMine is null, add it to the exclusions list
        //Added here to make sure PickAxeDigSize is modified if needed
        if ((IntPropertyData)export["HitsNeededToMine"] == null)
        {
            //Logging
            excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
            Console.WriteLine("* HitsNeededToMine are not readily exposed for this file and will be stored in \\_ExcludedFiles\\");

            //If the file is in a subdirectory, make sure that's preserved
            //So get the current asset's original path in the base game files
            string? originalPath = asset.FilePath;

            //This "dereference of a possibly null reference" thing is annoying, really annoying
            //originalPath ?? "stuff" didn't fix it this time, so there, disabled it in settings
            //Take that Visual Studio and your weirdness
            string parentPath = new DirectoryInfo(originalPath).Parent.Name;

            //Moving on...
            //Add the above parentPath to the outputPath and add _ExcludedFiles in there as well
            string finalPath = outputPath[whichMod] + parentPath + "\\_ExcludedFiles\\";

            //Ensure that all folders exist in the given path
            if (!Directory.Exists(finalPath))
                Directory.CreateDirectory(finalPath);

            //Write the asset to the desired directory for later editing and stuff
            asset.Write(finalPath + export.ObjectName + ".uasset");
        }
    }

    //Minerals_OneHit
    if (whichMod == 4)
    {
        //If this is a mineral file, we can proceed
        if (mineralFiles.Contains(export.ObjectName.ToString()))
        {
            //Because of the archive being used, we need to make sure this file's PickAxeDigSize is the intended value
            //Check to make sure it has the value
            //Needs to be here to make sure that any reversions to PickAxeDigSize are preserved
            if ((FloatPropertyData)export["PickAxeDigSize"] != null)
            {
                //Get the value from the originalProperties Dictionary
                float oSize = originalProperties[asset.Exports[0].ObjectName.ToString()][0];

                //Get the value from the asset in question
                FloatPropertyData nSize = (FloatPropertyData)export["PickAxeDigSize"];

                if (nSize.Value != oSize)
                {
                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize is not the same as the base game's value.");
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The value should be " + oSize + " but is " + nSize);
                    Console.WriteLine("This file's PickAxeDigSize is not the same as the base game's value");

                    //Fix: set the asset's PickAxeDigSize to the value it is in the base game
                    nSize.Value = oSize;

                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize has been set to the base game's value: " + nSize.Value);
                    Console.WriteLine("This file's PickAxeDigSize has been set to the base game's value");
                }
            }

            //Check to make sure hitSize isn't already masterHitSize
            if ((IntPropertyData)export["HitsNeededToMine"] != null && hitsNeeded.Value == masterHitsNeeded)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededToMine is already at the desired hits.");
                Console.WriteLine("This file's HitsNeededToMine is already at the desired hits");

                //If so, copy it as-is to the desired directory
                asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

                //If so, get out of here
                return;
            }

            //Check to see if hitsNeeded is smaller than masterHitSize, and check to make sure hitsNeeded isn't 1
            if ((IntPropertyData)export["HitsNeededToMine"] != null && hitsNeeded.Value < masterHitsNeeded && hitsNeeded.Value != 1)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededTo is smaller than the desired hits.");
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value will be preserved instead of setting it to the desired hits.");
                Console.WriteLine("This file's HitsNeededToMine is bigger than the desired hits");

                //If so, copy it as-is to the desired directory
                asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

                //Get out of here
                return;
            }

            //Check the current file to make sure it contains HitsNeededToMine, otherwise it needs special handling
            if ((IntPropertyData)export["HitsNeededToMine"] != null)
            {
                //Logging
                //If the header for this file doesn't exist, create it
                if (!modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Contains("File: " + export.ObjectName))
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);

                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");
                Console.WriteLine("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");

                //Modify the HitsNeededToMine value
                hitsNeeded.Value = masterHitsNeeded;

                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + hitsNeeded.Value + ".");
                Console.WriteLine("* The new value is " + hitsNeeded.Value + ".");

                //Write the asset to the desired directory
                asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

                //Logging
                Console.WriteLine("File " + export.ObjectName + " has been saved in: " + outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");
            }
            //Otherwise, add this property to the file with the specified value
            else
            {
                //Create a temp path
                string newPath = outputPath[whichMod] + "_ToEdit\\";

                //Instead, write it to the intended directy, but in its own folder, _ToEdit
                if (!Directory.Exists(newPath))
                {
                    //Verify
                    Console.WriteLine(newPath + " did not exist, creating... ");

                    //Create it
                    Directory.CreateDirectory(newPath);

                    //Verify
                    Console.WriteLine("> " + newPath + " now exists");
                }

                //Verify
                excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
                Console.WriteLine("> " + export.ObjectName + " cannot have HitsNeededToMine added programatically at this time");
                Console.WriteLine("> it has been excluded and stored in " + newPath + "\\" + export.ObjectName + ".uasset/.uexp for manually editing");

                //Write to file
                asset.Write(newPath + asset.Exports[0].ObjectName.ToString() + ".uasset");

                //Save settings to a file for later reference
                if (!File.Exists(newPath + "_Settings.txt"))
                    //Create the StreamWriter
                    using (StreamWriter output = new StreamWriter(Path.Combine(newPath, "_Settings.txt"), false))
                        //Write the settings to the file
                        output.WriteLine("HitsNeededToMine: " + masterHitsNeeded);
            }

            //If HitsNeededToMine is null, add it to the exclusions list
            //Added here to make sure PickAxeDigSize is modified if needed
            if ((IntPropertyData)export["HitsNeededToMine"] == null)
            {
                //Logging
                excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
                Console.WriteLine("* HitsNeededToMine are not readily exposed for this file and will be stored in \\_ExcludedFiles\\");

                //If the file is in a subdirectory, make sure that's preserved
                //So get the current asset's original path in the base game files
                string? originalPath = asset.FilePath;

                //This "dereference of a possibly null reference" thing is annoying, really annoying
                //originalPath ?? "stuff" didn't fix it this time, so there, disabled it in settings
                //Take that Visual Studio and your weirdness
                string parentPath = new DirectoryInfo(originalPath).Parent.Name;

                //Moving on...
                //Add the above parentPath to the outputPath and add _ExcludedFiles in there as well
                string finalPath = outputPath[whichMod] + parentPath + "\\_ExcludedFiles\\";

                //Ensure that all folders exist in the given path
                if (!Directory.Exists(finalPath))
                    Directory.CreateDirectory(finalPath);

                //Write the asset to the desired directory for later editing and stuff
                asset.Write(finalPath + export.ObjectName + ".uasset");
            }
        }
    }

    //MineralsAndTerrain_OneHit
    if (whichMod == 5)
    {
        //Because of the archive being used, we need to make sure this file's PickAxeDigSize is the intended value
        //Check to make sure it has the value
        //Needs to be here to make sure that any reversions to PickAxeDigSize are preserved
        if ((FloatPropertyData)export["PickAxeDigSize"] != null)
        {
            //Get the value from the originalProperties Dictionary
            float oSize = originalProperties[asset.Exports[0].ObjectName.ToString()][0];

            //Get the value from the asset in question
            FloatPropertyData nSize = (FloatPropertyData)export["PickAxeDigSize"];

            if (nSize.Value != oSize)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize is not the same as the base game's value.");
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The value should be " + oSize + " but is " + nSize);
                Console.WriteLine("This file's PickAxeDigSize is not the same as the base game's value");

                //Fix: set the asset's PickAxeDigSize to the value it is in the base game
                nSize.Value = oSize;

                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize has been set to the base game's value: " + nSize.Value);
                Console.WriteLine("This file's PickAxeDigSize has been set to the base game's value");
            }
        }

        //Check to make sure hitsNeeded isn't already masterHitSize
        if ((IntPropertyData)export["HitsNeededToMine"] != null && hitsNeeded.Value == masterHitsNeeded)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededToMine is already at the desired hits.");
            Console.WriteLine("This file's HitsNeededToMine is already at the desired hits");

            //If so, copy it as-is to the desired directory
            asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

            //If so, get out of here
            return;
        }

        //Check to see if hitsNeeded is smaller than masterHitSize, and check to make sure hitsNeeded isn't 1
        if ((IntPropertyData)export["HitsNeededToMine"] != null && hitsNeeded.Value < masterHitsNeeded && hitsNeeded.Value != 1)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededTo is smaller than the desired hits.");
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value will be preserved instead of setting it to the desired hits.");
            Console.WriteLine("This file's HitsNeededToMine is bigger than the desired hits");

            //If so, copy it as-is to the desired directory
            asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

            //Get out of here
            return;
        }

        ///If this is a mineral file, make sure we handle it as such
        if (mineralFiles.Contains(export.ObjectName.ToString()))
        {
            //Check to make sure hitsNeeded isn't already masterHitSize
            if ((IntPropertyData)export["HitsNeededToMine"] != null && hitsNeeded.Value == masterHitsNeeded)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededToMine is already at the desired size.");
                Console.WriteLine("This file's HitsNeededToMine is already at the desired size");

                //If so, copy it as-is to the desired directory
                asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

                //If so, get out of here
                return;
            }

            //Check the current file to make sure it contains HitsNeededToMine, otherwise it needs special handling
            if ((IntPropertyData)export["HitsNeededToMine"] != null)
            {
                //Logging
                //If the header for this file doesn't exist, create it
                if (!modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Contains("File: " + export.ObjectName))
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);

                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");
                Console.WriteLine("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");

                //Modify the HitsNeededToMine value
                hitsNeeded.Value = masterHitsNeeded;

                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + hitsNeeded.Value + ".");
                Console.WriteLine("* The new value is " + hitsNeeded.Value + ".");

                //Write the asset to the desired directory
                asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

                //Logging
                Console.WriteLine("File " + export.ObjectName + " has been saved in: " + outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");
            }
            //Otherwise, add this property to the file with the specified value
            else
            {
                //Create a temp path
                string newPath = outputPath[whichMod] + "_ToEdit\\";

                //Instead, write it to the intended directy, but in its own folder, _ToEdit
                if (!Directory.Exists(newPath))
                {
                    //Verify
                    Console.WriteLine(newPath + " did not exist, creating... ");

                    //Create it
                    Directory.CreateDirectory(newPath);

                    //Verify
                    Console.WriteLine("> " + newPath + " now exists");
                }

                //Verify
                excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
                Console.WriteLine("> " + export.ObjectName + " cannot have HitsNeededToMine added programatically at this time");
                Console.WriteLine("> it has been excluded and stored in " + newPath + "\\" + export.ObjectName + ".uasset/.uexp for manually editing");

                //Write to file
                asset.Write(newPath + asset.Exports[0].ObjectName.ToString() + ".uasset");

                //Save settings to a file for later reference
                if (!File.Exists(newPath + "_Settings.txt"))
                    //Create the StreamWriter
                    using (StreamWriter output = new StreamWriter(Path.Combine(newPath, "_Settings.txt"), false))
                        //Write the settings to the file
                        output.WriteLine("HitsNeededToMine: " + masterHitsNeeded);
            }
        }
        //Otherwise, handle it as a terrain file
        else
        {
            //Check the current file to make sure it contains HitsNeededToMine, otherwise don't save it
            if ((IntPropertyData)export["HitsNeededToMine"] != null)
            {
                //Logging
                //If the header for this file doesn't exist, create it
                if (!modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Contains("File: " + export.ObjectName))
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);

                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");
                Console.WriteLine("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");

                //Modify the HitsNeededToMine value
                hitsNeeded.Value = masterHitsNeeded;

                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + hitsNeeded.Value + ".");
                Console.WriteLine("* The new value is " + hitsNeeded.Value + ".");

                //Write the asset to the desired directory
                asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

                Console.WriteLine("File " + export.ObjectName + " has been saved in: " + outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");
            }
            //Otherwise, make a note of it in a file
            else
            {
                //Logging
                excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
                Console.WriteLine("* HitsNeededToMine is not readily exposed for this file and will be stored in \\_ExcludedFiles\\");

                //If the file is in a subdirectory, make sure that's preserved
                //So get the current asset's original path in the base game files
                string? originalPath = asset.FilePath;

                //This "dereference of a possibly null reference" thing is annoying, really annoying
                //originalPath ?? "stuff" didn't fix it this time, so there, disabled it in settings
                //Take that Visual Studio and your weirdness
                string parentPath = new DirectoryInfo(originalPath).Parent.Name;

                //Moving on...
                //Add the above parentPath to the outputPath and add _ExcludedFiles in there as well
                string finalPath = outputPath[whichMod] + parentPath + "\\_ExcludedFiles\\";

                //Ensure that all folders exist in the given path
                if (!Directory.Exists(finalPath))
                    Directory.CreateDirectory(finalPath);

                //Write the asset to the desired directory for later editing and stuff
                asset.Write(finalPath + export.ObjectName + ".uasset");
            }
        }

        //If HitsNeededToMine is null, add it to the exclusions list
        //Added here to make sure PickAxeDigSize is modified if needed
        if ((IntPropertyData)export["HitsNeededToMine"] == null)
        {
            //Logging
            excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
            Console.WriteLine("* HitsNeededToMine are not readily exposed for this file and will be stored in \\_ExcludedFiles\\");

            //If the file is in a subdirectory, make sure that's preserved
            //So get the current asset's original path in the base game files
            string? originalPath = asset.FilePath;

            //This "dereference of a possibly null reference" thing is annoying, really annoying
            //originalPath ?? "stuff" didn't fix it this time, so there, disabled it in settings
            //Take that Visual Studio and your weirdness
            string parentPath = new DirectoryInfo(originalPath).Parent.Name;

            //Moving on...
            //Add the above parentPath to the outputPath and add _ExcludedFiles in there as well
            string finalPath = outputPath[whichMod] + parentPath + "\\_ExcludedFiles\\";

            //Ensure that all folders exist in the given path
            if (!Directory.Exists(finalPath))
                Directory.CreateDirectory(finalPath);

            //Write the asset to the desired directory for later editing and stuff
            asset.Write(finalPath + export.ObjectName + ".uasset");
        }
    }
}

//Same as previous ModifyAsset[num]Values, but for both floats and ints (for DigSize_OneHit variations)
void ModifyAssetValues(UAsset asset, NormalExport export)
{
    //Bool to tell us if the asset's PickAxeDigSize is bigger than masterDigSize
    bool isPBigger = false;
    //Bool to tell us if the asset's HitsNeededToMine is bigger than masterHitSize
    bool isHHigher = false;

    //Get the desired values from export
    digSize = (FloatPropertyData)export["PickAxeDigSize"];
    hitsNeeded = (IntPropertyData)export["HitsNeededToMine"];

    //Terrain_DigSize_OneHit
    if (whichMod == 6)
    {
        //Check to see if the current file is for a mineral and needs to be skipped
        if (mineralFiles.Contains(export.ObjectName.ToString()))
        {
            //Logging
            excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
            Console.WriteLine("* This file is for a mineral and will be excluded");

            //Get out of here
            return;
        }

        //If both properties are null, add it to the exclusions list
        if ((FloatPropertyData)export["PickAxeDigSize"] == null || (IntPropertyData)export["HitsNeededToMine"] == null)
        {
            //Logging
            excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
            Console.WriteLine("* PickAxeDigSize and HitsNeededToMine are not readily exposed for this file and will be stored in \\_ExcludedFiles\\");

            //If the file is in a subdirectory, make sure that's preserved
            //So get the current asset's original path in the base game files
            string? originalPath = asset.FilePath;

            //This "dereference of a possibly null reference" thing is annoying, really annoying
            //originalPath ?? "stuff" didn't fix it this time, so there, disabled it in settings
            //Take that Visual Studio and your weirdness
            string parentPath = new DirectoryInfo(originalPath).Parent.Name;

            //Moving on...
            //Add the above parentPath to the outputPath and add _ExcludedFiles in there as well
            string finalPath = outputPath[whichMod] + parentPath + "\\_ExcludedFiles\\";

            //Ensure that all folders exist in the given path
            if (!Directory.Exists(finalPath))
                Directory.CreateDirectory(finalPath);

            //Write the asset to the desired directory for later editing and stuff
            asset.Write(finalPath + export.ObjectName + ".uasset");

            //Get out of here
            return;
        }

        //Check to make sure PickAxeDigSize and HitsNeededToMine aren't already masterDigSize and masterHitsNeeded
        if (digSize != null && digSize.Value == masterDigSize && hitsNeeded != null && hitsNeeded.Value == masterHitsNeeded)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededToMine and PickAxeDigSize is already at the desired size.");
            Console.WriteLine("This file's HitsNeededToMine and PickAxeDigSize is already at the desired value");

            //If so, copy it as-is to the desired directory
            asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

            //If so, get out of here
            return;
        }

        //Check to see if digSize is bigger than masterDigSize
        if ((FloatPropertyData)export["PickAxeDigSize"] != null && digSize.Value > masterDigSize)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize is bigger than the desired size.");
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value will be preserved instead of setting it to the desired size.");
            Console.WriteLine("This file's PickAxeDigSize is bigger than the desired size");

            //If so, make sure we know this for later
            isPBigger = true;
        }

        //Check to see if hitsNeeded is smaller than masterHitSize, and check to make sure hitsNeeded isn't 1
        if ((IntPropertyData)export["HitsNeededToMine"] != null && hitsNeeded.Value < masterHitsNeeded && hitsNeeded.Value != 1)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededTo is smaller than the desired hits.");
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value will be preserved instead of setting it to the desired hits.");
            Console.WriteLine("This file's HitsNeededToMine is bigger than the desired hits");

            //If so, make sure we know this for later
            isHHigher = true;
        }

        //Check the current file to make sure it contains PickAxeDigSize
        if ((FloatPropertyData)export["PickAxeDigSize"] != null)
        {
            //If the asset's current PickAxeDigSize isn't bigger than masterDigSize, proceed as normal
            if (!isPBigger)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of PickAxeDigSize was " + digSize.Value + ".");
                Console.WriteLine("* The original value of PickAxeDigSize was " + digSize.Value + ".");

                //Modify the PickAxeDigSize value
                digSize.Value = masterDigSize;

                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + digSize.Value + ".");
                Console.WriteLine("* The new value is " + digSize.Value + ".");
            }
        }
        //Otherwise, make a note of it
        else
        {
            //Logging
            Console.WriteLine("* PickAxeDigSize is not readily exposed for this file and will be excluded");
        }

        //Check the current file to make sure it contains HitsNeededToMine
        if ((IntPropertyData)export["HitsNeededToMine"] != null)
        {
            if (!isHHigher)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");
                Console.WriteLine("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");

                //Modify the HitsNeededToMine value
                hitsNeeded.Value = masterHitsNeeded;

                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + hitsNeeded.Value + ".");
                Console.WriteLine("* The new value is " + hitsNeeded.Value + ".");
            }
        }

        //Make sure in either case that the asset is written to the desired directory
        asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

        Console.WriteLine("File " + export.ObjectName + " has been saved in: " + outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");
    }

    //Minerals_DigSize_OneHit
    if (whichMod == 7)
    {
        //If both properties are null, add it to the exclusions list
        if ((FloatPropertyData)export["PickAxeDigSize"] == null || (IntPropertyData)export["HitsNeededToMine"] == null)
        {
            //Logging
            excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
            Console.WriteLine("* PickAxeDigSize and HitsNeededToMine are not readily exposed for this file and will be stored in \\_ExcludedFiles\\");

            //If the file is in a subdirectory, make sure that's preserved
            //So get the current asset's original path in the base game files
            string? originalPath = asset.FilePath;

            //This "dereference of a possibly null reference" thing is annoying, really annoying
            //originalPath ?? "stuff" didn't fix it this time, so there, disabled it in settings
            //Take that Visual Studio and your weirdness
            string parentPath = new DirectoryInfo(originalPath).Parent.Name;

            //Moving on...
            //Add the above parentPath to the outputPath and add _ExcludedFiles in there as well
            string finalPath = outputPath[whichMod] + parentPath + "\\_ExcludedFiles\\";

            //Ensure that all folders exist in the given path
            if (!Directory.Exists(finalPath))
                Directory.CreateDirectory(finalPath);

            //Write the asset to the desired directory for later editing and stuff
            asset.Write(finalPath + export.ObjectName + ".uasset");

            //Get out of here
            return;
        }

        //If this is a mineral file, we can proceed
        if (mineralFiles.Contains(export.ObjectName.ToString()))
        {
            //Check to make sure PickAxeDigSize and HitsNeededToMine aren't already masterDigSize and masterHitsNeeded
            if (digSize != null && digSize.Value == masterDigSize && hitsNeeded != null && hitsNeeded.Value == masterHitsNeeded)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededToMine and PickAxeDigSize is already at the desired size.");
                Console.WriteLine("This file's HitsNeededToMine and PickAxeDigSize is already at the desired value");

                //If so, copy it as-is to the desired directory
                asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

                //If so, get out of here
                return;
            }

            //Check to see if digSize is bigger than masterDigSize
            if ((FloatPropertyData)export["PickAxeDigSize"] != null && digSize.Value > masterDigSize)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize is bigger than the desired size.");
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value will be preserved instead of setting it to the desired size.");
                Console.WriteLine("This file's PickAxeDigSize is bigger than the desired size");

                //If so, make sure we know this for later
                isPBigger = true;
            }

            //Check to see if hitsNeeded is smaller than masterHitSize, and check to make sure hitsNeeded isn't 1
            if ((IntPropertyData)export["HitsNeededToMine"] != null && hitsNeeded.Value < masterHitsNeeded && hitsNeeded.Value != 1)
            {
                //Logging
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededTo is smaller than the desired hits.");
                modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value will be preserved instead of setting it to the desired hits.");
                Console.WriteLine("This file's HitsNeededToMine is bigger than the desired hits");

                //If so, make sure we know this for later
                isHHigher = true;
            }

            //Check the current file to make sure it contains PickAxeDigSize, otherwise it needs special handling
            if ((FloatPropertyData)export["PickAxeDigSize"] != null)
            {
                if (!isPBigger)
                {
                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of PickAxeDigSize was " + digSize.Value + ".");
                    Console.WriteLine("* The original value of PickAxeDigSize was " + digSize.Value + ".");

                    //Modify the PickAxeDigSize value
                    digSize.Value = masterDigSize;

                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + digSize.Value + ".");
                    Console.WriteLine("* The new value is " + digSize.Value + ".");
                }
            }
            //Otherwise, add this property to the file with the specified value
            else
            {
                //Create a temp path
                string newPath = outputPath[whichMod] + "_ToEdit\\";

                //Instead, write it to the intended directy, but in its own folder, _ToEdit
                if (!Directory.Exists(newPath))
                {
                    //Verify
                    Console.WriteLine(newPath + " did not exist, creating... ");

                    //Create it
                    Directory.CreateDirectory(newPath);

                    //Verify
                    Console.WriteLine("> " + newPath + " now exists");
                }

                //Verify
                excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
                Console.WriteLine("> " + export.ObjectName + " cannot have PickAxeDigSize added programatically at this time");
                Console.WriteLine("> it has been excluded and stored in " + newPath + "\\" + export.ObjectName + ".uasset/.uexp for manually editing");

                //Write to file
                asset.Write(newPath + asset.Exports[0].ObjectName.ToString() + ".uasset");

                //Save settings to a file for later reference
                if (!File.Exists(newPath + "_Settings.txt"))
                    //Create the StreamWriter
                    using (StreamWriter output = new StreamWriter(Path.Combine(newPath, "_Settings.txt"), false))
                    {
                        //Write the settings to the file
                        output.WriteLine("PickAxeDigSize: " + masterDigSize);
                        output.WriteLine("HitsNeededToMine: " + masterHitsNeeded);
                    }
            }

            //Check the current file to make sure it contains HitsNeededToMine, otherwise it needs special handling
            if ((IntPropertyData)export["HitsNeededToMine"] != null)
            {
                if (!isHHigher)
                {
                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");
                    Console.WriteLine("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");

                    //Modify the PickAxeDigSize value
                    hitsNeeded.Value = masterHitsNeeded;

                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + hitsNeeded.Value + ".");
                    Console.WriteLine("* The new value is " + hitsNeeded.Value + ".");
                }
            }
            //Otherwise, add this property to the file with the specified value
            else
            {
                //Create a temp path
                string newPath = outputPath[whichMod] + "_ToEdit\\";

                //Instead, write it to the intended directy, but in its own folder, _ToEdit
                if (!Directory.Exists(newPath))
                {
                    //Verify
                    Console.WriteLine(newPath + " did not exist, creating... ");

                    //Create it
                    Directory.CreateDirectory(newPath);

                    //Verify
                    Console.WriteLine("> " + newPath + " now exists");
                }

                //Verify
                excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
                Console.WriteLine("> " + export.ObjectName + " cannot have PickAxeDigSize added programatically at this time");
                Console.WriteLine("> it has been excluded and stored in " + newPath + "\\" + export.ObjectName + ".uasset/.uexp for manually editing");

                //Write to file
                asset.Write(newPath + asset.Exports[0].ObjectName.ToString() + ".uasset");

                //Save settings to a file for later reference
                if (!File.Exists(newPath + "_Settings.txt"))
                    //Create the StreamWriter
                    using (StreamWriter output = new StreamWriter(Path.Combine(newPath, "_Settings.txt"), false))
                    {
                        //Write the settings to the file
                        output.WriteLine("PickAxeDigSize: " + masterDigSize);
                        output.WriteLine("HitsNeededToMine: " + masterHitsNeeded);
                    }
            }


            //Make sure that the asset is written to the desired directory
            asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

            //Logging
            Console.WriteLine("File " + export.ObjectName + " has been saved in: " + outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");
        }
    }

    //MineralsAndTerrain_DigSize_OneHit
    if (whichMod == 8)
    {
        //If both properties are null, add it to the exclusions list
        if ((FloatPropertyData)export["PickAxeDigSize"] == null || (IntPropertyData)export["HitsNeededToMine"] == null)
        {
            //Logging
            excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
            Console.WriteLine("* PickAxeDigSize and HitsNeededToMine are not readily exposed for this file and will be stored in \\_ExcludedFiles\\");

            //If the file is in a subdirectory, make sure that's preserved
            //So get the current asset's original path in the base game files
            string? originalPath = asset.FilePath;

            //This "dereference of a possibly null reference" thing is annoying, really annoying
            //originalPath ?? "stuff" didn't fix it this time, so there, disabled it in settings
            //Take that Visual Studio and your weirdness
            string parentPath = new DirectoryInfo(originalPath).Parent.Name;

            //Moving on...
            //Add the above parentPath to the outputPath and add _ExcludedFiles in there as well
            string finalPath = outputPath[whichMod] + parentPath + "\\_ExcludedFiles\\";

            //Ensure that all folders exist in the given path
            if (!Directory.Exists(finalPath))
                Directory.CreateDirectory(finalPath);

            //Write the asset to the desired directory for later editing and stuff
            asset.Write(finalPath + export.ObjectName + ".uasset");
        }

        //Check to make sure PickAxeDigSize and HitsNeededToMine aren't already masterDigSize and masterHitsNeeded
        if (digSize != null && digSize.Value == masterDigSize && hitsNeeded != null && hitsNeeded.Value == masterHitsNeeded)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededToMine and PickAxeDigSize is already at the desired size.");
            Console.WriteLine("This file's HitsNeededToMine and PickAxeDigSize is already at the desired value");

            //If so, copy it as-is to the desired directory
            asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

            //If so, get out of here
            return;
        }

        //Check to see if digSize is bigger than masterDigSize
        if ((FloatPropertyData)export["PickAxeDigSize"] != null && digSize.Value > masterDigSize)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's PickAxeDigSize is bigger than the desired size.");
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value will be preserved instead of setting it to the desired size.");
            Console.WriteLine("This file's PickAxeDigSize is bigger than the desired size");

            //If so, make sure we know this for later
            isPBigger = true;
        }

        //Check to see if hitsNeeded is smaller than masterHitSize, and check to make sure hitsNeeded isn't 1
        if ((IntPropertyData)export["HitsNeededToMine"] != null && hitsNeeded.Value < masterHitsNeeded && hitsNeeded.Value != 1)
        {
            //Logging
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* This file's HitsNeededTo is smaller than the desired hits.");
            modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value will be preserved instead of setting it to the desired hits.");
            Console.WriteLine("This file's HitsNeededToMine is bigger than the desired hits");

            //If so, make sure we know this for later
            isHHigher = true;
        }

        //If this is a mineral file, make sure we handle it as such
        if (mineralFiles.Contains(export.ObjectName.ToString()))
        {
            //Check the current file to make sure it contains PickAxeDigSize, otherwise it needs special handling
            if ((FloatPropertyData)export["PickAxeDigSize"] != null)
            {
                if (!isPBigger)
                {
                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of PickAxeDigSize was " + digSize.Value + ".");
                    Console.WriteLine("* The original value of PickAxeDigSize was " + digSize.Value + ".");

                    //Modify the PickAxeDigSize value
                    digSize.Value = masterDigSize;

                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + digSize.Value + ".");
                    Console.WriteLine("* The new value is " + digSize.Value + ".");
                }
            }
            //Otherwise, add this property to the file with the specified value
            else
            {
                //Create a temp path
                string newPath = outputPath[whichMod] + "_ToEdit\\";

                //Instead, write it to the intended directy, but in its own folder, _ToEdit
                if (!Directory.Exists(newPath))
                {
                    //Verify
                    Console.WriteLine(newPath + " did not exist, creating... ");

                    //Create it
                    Directory.CreateDirectory(newPath);

                    //Verify
                    Console.WriteLine("> " + newPath + " now exists");
                }

                //Verify
                excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
                Console.WriteLine("> " + export.ObjectName + " cannot have PickAxeDigSize added programatically at this time");
                Console.WriteLine("> it has been excluded and stored in " + newPath + "\\" + export.ObjectName + ".uasset/.uexp for manually editing");

                //Write to file
                asset.Write(newPath + asset.Exports[0].ObjectName.ToString() + ".uasset");

                //Save settings to a file for later reference
                if (!File.Exists(newPath + "_Settings.txt"))
                    //Create the StreamWriter
                    using (StreamWriter output = new StreamWriter(Path.Combine(newPath, "_Settings.txt"), false))
                    {
                        //Write the settings to the file
                        output.WriteLine("PickAxeDigSize: " + masterDigSize);
                        output.WriteLine("HitsNeededToMine: " + masterHitsNeeded);
                    }
            }

            //Check the current file to make sure it contains PickAxeDigSize, otherwise it needs special handling
            if ((IntPropertyData)export["HitsNeededToMine"] != null)
            {
                if (!isHHigher)
                {
                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");
                    Console.WriteLine("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");

                    //Modify the PickAxeDigSize value
                    hitsNeeded.Value = masterHitsNeeded;

                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + hitsNeeded.Value + ".");
                    Console.WriteLine("* The new value is " + hitsNeeded.Value + ".");
                }
            }
            //Otherwise, add this property to the file with the specified value
            else
            {
                //Create a temp path
                string newPath = outputPath[whichMod] + "_ToEdit\\";

                //Instead, write it to the intended directy, but in its own folder, _ToEdit
                if (!Directory.Exists(newPath))
                {
                    //Verify
                    Console.WriteLine(newPath + " did not exist, creating... ");

                    //Create it
                    Directory.CreateDirectory(newPath);

                    //Verify
                    Console.WriteLine("> " + newPath + " now exists");
                }

                //Verify
                excludedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[2]].Add("File " + export.ObjectName + ".uasset/.uexp");
                Console.WriteLine("> " + export.ObjectName + " cannot have HitsNeededToMine added programatically at this time");
                Console.WriteLine("> it has been excluded and stored in " + newPath + "\\" + export.ObjectName + ".uasset/.uexp for manually editing");

                //Write to file
                asset.Write(newPath + asset.Exports[0].ObjectName.ToString() + ".uasset");

                //Save settings to a file for later reference
                if (!File.Exists(newPath + "_Settings.txt"))
                    //Create the StreamWriter
                    using (StreamWriter output = new StreamWriter(Path.Combine(newPath, "_Settings.txt"), false))
                    {
                        //Write the settings to the file
                        output.WriteLine("PickAxeDigSize: " + masterDigSize);
                        output.WriteLine("HitsNeededToMine: " + masterHitsNeeded);
                    }
            }
        }
        //Otherwise, it's a terrain file and needs handled as one
        else
        {
            //Check the current file to make sure it contains PickAxeDigSize
            if ((FloatPropertyData)export["PickAxeDigSize"] != null)
            {
                if (!isPBigger)
                {
                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of PickAxeDigSize was " + digSize.Value + ".");
                    Console.WriteLine("* The original value of PickAxeDigSize was " + digSize.Value + ".");

                    //Modify the PickAxeDigSize value
                    digSize.Value = masterDigSize;

                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + digSize.Value + ".");
                    Console.WriteLine("* The new value is " + digSize.Value + ".");
                }
            }

            //Check the current file to make sure it contains HitsNeededToMine
            if ((IntPropertyData)export["HitsNeededToMine"] != null)
            {
                if (!isHHigher)
                {
                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");
                    Console.WriteLine("* The original value of HitsNeededToMine was " + hitsNeeded.Value + ".");

                    //Modify the HitsNeededToMine value
                    hitsNeeded.Value = masterHitsNeeded;

                    //Logging
                    modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + hitsNeeded.Value + ".");
                    Console.WriteLine("* The new value is " + hitsNeeded.Value + ".");
                }
            }
            //Otherwise, make a note of it
            else
            {
                //Logging
                Console.WriteLine("* HitsNeededToMine is not readily exposed for this file and will be excluded");
            }
        }

        //Write the asset to the desired directory
        asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

        //Logging
        Console.WriteLine("File " + export.ObjectName + " has been saved in: " + outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");
    }
}

//Write whatever is needed to a file
void WriteToFile(string reportName, Dictionary<string, Dictionary<string, List<string>>> linesToWrite)
{
    //Logging
    Console.WriteLine("File " + reportName + " is now being written");

    //Iterate first through the directories
    foreach (var outerkey in linesToWrite)
    {
        //Iterate through the number of reports to generate
        foreach (var innerkey in outerkey.Value)
        {
            //Create tthe StreamWriter with the outerkey's path and the innerkey's file name, making sure not to append because overwriting is desired
            using (StreamWriter output = new StreamWriter(Path.Combine(outerkey.Key, innerkey.Key), false))
            {
                //Iterate through each line and write it to this file
                foreach (string line in innerkey.Value)
                {
                    //Write the line to the file
                    output.WriteLine(line);
                }
            }
        }
    }
}

//Just in case I need this later
//Supposed to add the property to the file, but it gives a serialization error instead when I tried
/*
//Logging
modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("File: " + export.ObjectName);
modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add(export.ObjectName + " does not expose its PickAxeDigSize property in UAssetGUI and will be added");
Console.WriteLine(export.ObjectName + " does not expose its PickAxeDigSize property in UAssetGUI and will be added");

//Add the property/value to this file (hopefully)
export["PickAxeDigSpeed"] = new FloatPropertyData() { Value = masterDigSize };

//Logging
modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The original value of PickAxeDigSize was 105.");
Console.WriteLine("* The original value of PickAxeDigSize was 105.");

//Logging
modifiedFiles[outputPath[whichMod] + "_Reports"][reportFileNames[1]].Add("* The new value is " + masterDigSize + ".");
Console.WriteLine("* The new value is " + masterDigSize + ".");

//Write the asset to the desired directory
asset.Write(outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");

//Logging
Console.WriteLine("File " + export.ObjectName + " has been saved in: " + outputPath[whichMod] + asset.Exports[0].ObjectName + ".uasset");
*/