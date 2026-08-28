## Overview

*I discovered in trying to make the Big Dig Boi Series of mods that the information to do what I wanted was a bit sparse. A person by the name of Von on the Practical DRG Discord server got me going in the right direction though, using UAssetAPI as the base for the program I wrote to make all mods in the series.*

**Disclaimer: I'm not a professional programmer, and the code that is in this repository is provided as-is. It's full of jank, bad practices, and even worse comments.**

*That being said, I nevertheless wanted to share what I did because those that have done something similar have left no record (that I know of) as to how they managed to make their versions.*

*With that out of the way, let's get into it.*

## Scope Of This Documentation
*I assume you have a basic understanding of how to make mods manually using tools such as UAssetGUI. I also assume that you have a basic grasp of packing/unpacking files. Furthermore, some level of programming experience is required (I used C# for this, but it most likely works in other languages, for example Von uses Python for their mods).*

**This documentation is not intended, and will not ever intend, to be an all-encompassing beginner's guide to mod creation. There will be minimal hand-holding, and you assume all responsiblity and risk in implementing anything mentioned here or elsewhere.**

**This is meant only as a reference, a jumping off point to hopefully make your mod creation journey just a little easier.**

### Index
- [Required Tools](#the-required-tools)
- [Required Files](#the-required-files)
- [Overview](#overview)
- [The Master Archive](#the-master-archive)
- [Creating The Archive](#creating-the-archive)
- [Creating The Mods](#creating-the-mods)
- [Diving Into Mass Asset Creation](#diving-into-mass-asset-creation)
- [Automation](#automation)
- [Conclusion](#conclusion)
- [Credits](#credits)
- [No AI Use Disclaimer](#no-ai-use-disclaimer)
- [License](#license)

### The Required Tools

- [Visual Studio Community 2026](https://visualstudio.microsoft.com/vs/community/ "Visual Studio Community 2026")
- [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0 ".NET 10.0 SDK")
- [UAssetAPI](https://github.com/atenfyr/UAssetAPI "UAsset API")
- [UAssetGUI](https://github.com/atenfyr/UAssetGUI "UAssetGUI")
- [DRG Packer](https://github.com/DRG-Modding/tools/blob/9bacb72561a5ce43d84138999bd972158b2b34a0/loose-files/DRGPacker4.27.zip "DRG Packer")

### The Required Files

- [The Master Archive Located In /Files](Files/Master_Archive "The Master Archive Located in /Files")
- A Copy Of The Unpacked Original Game Files

## Overview
Broadly, what my program does is take the user's input for their desired PickAxeDigSize and/or HitsNeededToMine (based on which mod(s) they want to make) and then uses that to read a modified version of the base game's files so that the desired changes are reflected across all relevant assets contained in that directory. Some files are intentionally excluded because their data structure differed from the mods I wanted to alter. More details are located in the Master Archive's [readme](Files/MasterArchive/Readme.md).

### The Master Archive

The Master Archive is the lynchpin to make this work. Because of its importance, let me reiterate what that linked readme says about it:
> I ran into a serialization issue with UAssetAPI that prevented me from adding PickAxeDigSize and/or HitsNeededToMine to any file that doesn't have that property exposed in UAssetGUI. The solution is what's present in this archive: a compilation of files that have all the necessary properties added to them, at the base game's values, so that they can be modified as needed to contain what value is desired for those two properties.

Without these modified files, I could not have changed all relevant files in the game due to the properties not being readily available in the file through UAssetAPI. These properties are present in the file, and can be viewed within Unreal Engine's Editor, if you happen to have all the requirements to load the game's assets into the editor. I haven't gone through that process, so my only viable option was compiling this archive.

Because this Master Archive was achieved through combining the base game's files and the work of three other people, I want to take a moment to give proper credit to these authors and their mods:
- [Better Digging](https://mod.io/g/drg/m/better-digging) by [Dreyda](https://mod.io/g/drg/u/dreyda)
- [Better Pickaxe](https://mod.io/g/drg/m/better-pickaxe2) by [-Toki-Shak-](https://mod.io/g/drg/u/dedmraz)
- [Even Better Mining V2](https://mod.io/g/drg/m/even-better-mining-v2) by [Ratchet7x5](https://mod.io/g/drg/u/ratchet7x5)

Without their work, The Big Dig Boi series of mods would have been much smaller in scope. So thank you, your work is greatly appreciated!

### Creating The Archive

Because my original Master Archive was a hodgepodge of modded files and base game files, this meant that their values were all over the place, and could not be relied on to make my mods with only the intended changes.

This is where a small but crucial part of my program comes into play: creating a new Master Archive with only default values. Most of the code to do this is located in VerifyMasterArchive() and CreateNewMaster().

This code does a few things: 
- Analyze all relevant files from the base game's unaltered, unedited files, straight from the game's FSD-WindowsNoEditor.pak and extracted using DRG Packer
- Store the relevant data for each file: 
    - File name
    - PickAxeDigSize value
    - HitsNeededToMine value
- Analyze all relevant files from the first iteration of the Master Archive, containing a mix of values that differ from default in most cases
- Compare the data from the base game's files with those of the first Master Archive, and if that Master Archive differs, write a file with the original default values instead
- Lastly, if a file does not differ between these two archives, that file is written as-is to the new Master Archive

Doing this ensures that I have a Master Archive that has all default values for every file, and that all files have both necessary properties so that I can properly change them programmatically with UAssetAPI.

I feel like I should clarify something: none of the Big Dig Boi series contains any original files from the base game or from the mentioned mods.
I used them as a reference only, and my files are newly generated assets with the desired changes.

### Creating The Mods

Once I was able to trust my source material, I could safely move on to creating all mods in the series. The code itself is of questionable quality, but I've been able to verify the results in UAssetGUI and in-game, so the code at least works as intended within a narrow set of circumstances. 

*As an aside, the code, as of time of writing, has no exception handling, and contains essentially no checks for invalid input. There's two do/while loops that were halfheartedly added and then I just decided not to implement them elsewhere. I hate while loops anyways, we never get along, so it's probably for the best. This is why I say that the code works within narrow cirumstances. It does the job so long as you know exactly what to input and don't fat-finger anything.
It's only intended for internal use, and should not be used as-is by anyone (unless you want to risk deleting the entirety of your C drive or something else equally horrific).*

Now with more disclaimers out of the way, let's get into some specifics.

The core of any part of the asset creation process is straightforward. The following code comes from the early version of the program that made the initail version of the first three mods in the series, and shows the process well enough:


```c#
//Get all uassets in Landscape\Materials and their associated directory
foreach (string file in Directory.EnumerateFiles(inputPath, "*.uasset", SearchOption.AllDirectories))
{
    //Create a new UAsset for this file
    UAsset currentAsset = new UAsset(file, EngineVersion.VER_UE4_27);

    //Get the location of the desired variable
    NormalExport assetExport = (NormalExport)currentAsset.Exports[0];

    //Check the current file to make sure it contains PickAxeDigSize, otherwise don't save it
    if ((FloatPropertyData)assetExport["PickAxeDigSize"] != null)
    {
        //Get the desired value from assetExport
        FloatPropertyData digSize = (FloatPropertyData)assetExport["PickAxeDigSize"];

        //Modify the PickAxeDigSize value
        digSize.Value = 225;

        //Write the asset to the desired directory
        currentAsset.Write(outputPath + currentAsset.Exports[0].ObjectName + ".uasset");
    }
}
```
To summarize: 
- Iterate through the directory containing the files
- Get the desired value from the given property contained without Exports (you can find this info in UAssetGUI)
- Modify that value
- Write a new uasset file

The above code will get the job done, but it's limited in what you can do. So the current version of the program I use expands on this functionality with a bunch of wrapper logic that dictates which mod gets which values, and which files, as well as several logging features to verify what was modified and what was excluded.

Some mods needed to only contain minerals files, others, only terrain files. Still others needed both. Then there's the variation in which mods get which modified property, or if they get both. This is why I landed on nine mods for the series. It's also why my code has grown in size from a few dozen lines to nearly 3,000.

### Diving Into Mass Asset Creation

I would like to dive into one function that handles creating the first three mods in the series. The logic is virtually identical for the other six, so going over this one should give you a decent understanding of the other two functions.

Let's look at ModifyAssetFloatValues(UAsset uasset, NormalExport export) which handles the PickAxeDigSize changes for those three mods. Because this function is several hundred lines long, I'll instead provide some psuedocode to illustrate its logic.
```c#
get DigSize for currentAsset;

if (whichMod == 0)
	Mineral file? > return;

	if (HitsNeededToMine exists)
		Get HitsNeeded;

		Does HitsNeeded = BaseGameValue?
			if (HitsNeeded != BaseGameValue)
				set currentAssetHitsValue to BaseGameValue;
		
		if (DigSize == DesiredValue)
			write unmodified asset > return;
		else
			set currentAssetDig to DesiredDigSize;

		If (DigSize != null)
			set currentAssetDig to DesiredDigSize;
			write modified asset;
			
if (whichMod == 1)
	if (Mineral file)
		if (HitsNeededToMine exists)
			Get HitsNeeded;

			Does HitsNeeded = BaseGameValue?
				if (HitsNeeded != BaseGameValue)
					set currentAssetHitsValue to BaseGameValue;
		
		if (DigSize == DesiredValue)
			write unmodified asset > return;
		else
			set currentAssetDig to DesiredDigSize;

		If (DigSize !=  null)
			set currentAssetDig to DesiredDigSize;
			write modified asset;

if (whichMod == 2)
	if (HitsNeededToMine exists)
		Get HitsNeeded;

		Does HitsNeeded = BaseGameValue?
			if (HitsNeeded != BaseGameValue)
				set currentAssetHitsValue to BaseGameValue;

	if (Mineral file)
		if (DigSize == DesiredValue)
			write unmodified asset > return;
		else
			set currentAssetDig to DesiredDigSize;

		If (DigSize != null)
			set currentAssetDig to DesiredDigSize;

	if (Terrain file)
		if (DigSize == DesiredValue)
			write unmodified asset > return;
		else
			set currentAssetDig to DesiredDigSize;

		If (DigSize != null)
			set currentAssetDig to DesiredDigSize;

		write modified asset;
```
Note: none of the actual code that handles this logic is optimized, some of it is probably redundant/unneccessary, and again, it's pretty jank (reviewing the code to write this documentation has made all of this quite obvious. Will I do anything about it? Probably not).

Anyways, like I said, the logic of this function carries over to the other functions that handle file modification, ModifyIntValues() (for HitsNeededToMine) and ModifyAssetValues() (for both properties).

It's probably worth mentioning that all three of these functions run in a few loops that iterate through every file in the Master Archive, with logic to handle which mod(s) the user wants to make. It should be noted that the current version of the program only provides two main options: single mod, or all mods. You can't choose to make only two mods at once, for instance (again, will I fix this? Doubtful).
The result is, as of the current version of the program, a main directory for all mods containing 2,727 files (included 27 reports I have it write for verification) within 72 different folders.

As an aside, if you want to view the raw files, you can check them out [here](Files\Working_Archive). I wanted this repo to be as complete as possible, so that in the future someone will hopefully have everything they need to do something similar for their own project.

## Automation

Tangentially related to the main purpose of the program itself, I've made a few automation-focused helper functions because I got tired of manually packing and zipping 9 files. It's probably a little too late for me to make much use out of this, because unless something breaks, I'm most likely finished uploading new versions of the mods. As of 8/27/2026, I'm reasonably confident that they're all feature-complete, fully functional, and work as intended.
Never-the-less, I felt that this was a worthwhile addition for anyone else that might need this information.

With any asset creation operation, at the end, all files will automatically be packed and then stored in their own zip file. The core of this is handled in three new functions: PakFiles(), ZipFiles(), and MoveToFolder(). These are located right at the very end of Program.cs.
The main logic of PakFiles() is as follows (in psuedocode):
```c#
get mainPath = directory of main project folder;
create command prompt process and assign it to \\repak.exe;

if (whichMod != 100)
	mainPath = outputPath[whichMod];
	fullPath = "\"" + outputPath[whichMod];
	pass argument to command prompt: " pack " + fullPath;
	start command prompt;
	get name of mod;

if (whichMod == 100)
	iterate through each path in outputPath
		mainPath = outputPath[whichMod];
		fullPath = "\"" + outputPath[whichMod];
		pass argument to command prompt: " pack " + fullPath;
		start command prompt;
		get name of mod;
```
To make this function work, I had to use [trumank's](https://github.com/trumank) [repak](https://github.com/trumank/repak) tool instead of DRG Packer and interface with its executable through the command prompt. I really tried to get it to work with DRG packer, but in the end, it just would not cooperate. Because the code to do this is integral to the function working as intended, I'll share a stripped-down version of the actual code.
```c#
 //Get the executable located in _Main
 process.StartInfo.FileName = mainPathP5 + "\\repak.exe";
 //Best I can figure, this makes it so that everything happens in the main window
 process.StartInfo.RedirectStandardOutput = true;
 //Make sure the repak.exe runs in an elevated prompt
 //I don't know why this is necessary on my system, but it is
 process.StartInfo.Verb = "runas";

 //Assign the outputPath to a new variable with formatting for command prompt usage
 fullPath = "\"" + outputPath[whichMod];

 //Create the argument needed to make this mod's pak file
 process.StartInfo.Arguments = " pack " + fullPath;

 //Start the process
 process.Start();

 //Throws the command text output into a string
 output = process.StandardOutput.ReadToEnd();

 //Wait until stuff finishes
 process.WaitForExit();

```
It's worth mentioning that to make this work, I put repak.exe in my root mod folder for this project. You can do whatever you want, but I found that doing this made things easier for me.
There are two things that I should highlight from the above code: the first is *process.StartInfo.Verb = "runas"* and the second is *fullPath = "\"" + outputPath[whichMod]*. The first makes it so that the command prompt is run as an administrator. For whatever reason, I could not get the thing to run an executable until I did this. The second is something I'm not sure I can explain fully, but is essential to making this work. 
The translation of this basically reads as "F:\DRG_Modding\Mods\BigDigBoi Series\_Main\000__BDB__Dig_Bigger_Holes__TE if you're trying to make mod 0.
This confuses me because in all other cases where I've needed to pass a path to the command prompt, I've had to make sure it was formatting as "path to thing/path to thing" when the path contains spaces. Notice the last quotation mark in that example. As far as I can tell, the code ultimately omits that final quotation mark in its output, and as you can see, my path has a space in it. How does it work? I honestly couldn't tell you. But again, it has to have that, or this doesn't work.
Coding is weird. There's an explanation somewhere, but I couldn't find it.
Long story short, just add "\"" + [path] to your string that you pass to the argument and you should be fine, somehow.

The next piece of the automation puzzle is ZipFiles(). Unlike the above, the logic in this function is straightforward enough, and without any enigmas found so far in the code. The logic goes like this:
- Accept an input for your mainPath and a list of pak files
- Iterate through those pak files found in your mainPath
- Create a new zip file containing only that pak file
- Repeat until all pak files are contained in its own zip file

The only other interesting bit about this is that I've decided to move away from traditional versioning for these zip files, instead appending the local date and time to the end of the file name. I couldn't be bothered to figure out a way to integrate that easily while still maintaining the level of automation I wanted (perhaps query the public mod page for the live file's version? Mint does it, so it's certainly doable - but nothing I care to investigate and implement).

The last function is MoveToFolder() which, if I'm being honest, only caters to my particular need for specificity and organization. It just moves the created paks and zips into their own subdirectory, /_Paks and /_Zips, respectively. The only interesting bit of code in there (in my opinion) is how I handle deleting old archives. Because of that whole aforementioned date-appending thing, finding old zips by name is... Tricky. You can go in there, look for files that contain a certain part of the name, or just look for files in general, or... A fair few things, really. None are as simple as just deleting the thing and starting over. The code to this is as follows, and is short enough that I have no issue sharing it here:
```c#
if (!Directory.Exists(path + subDir))
{
	//Create the directory
	Directory.CreateDirectory(path + subDir);
}
else
{
	//Delete the directory
	Directory.Delete(path + subDir + "\\", recursive: true);

	//Create the directory
	Directory.CreateDirectory(path + subDir + "\\");
}
```
The trick to making it work is the recursive flag. As far as I understand, without that, Directory.Delete can only delete an empty folder, and throws an exception if something is in there. With that flag though, it iterates through all files and folders, deleting as it goes, before finally deleting the main folder itself.
Quick, simple, effective. I love it.

I can't imagine I'll be adding much more code to this program anymore, so I feel pretty confident with what I've covered, that it's as complete of an overview as I can manage.


## Conclusion

It's worth noting that I haven't gone over every bit of code in the program, only what is most relevant to writing modified assets. If you would like, you can check out the full source [here](Source/Program.cs).

From time to time, I may modify the program either for optimization purposes, to add in new features, or reduce the jank to more reasonable levels. If so, I'll try to make sure this documentation stays-up-to-date with anything that's relevant.

It is my hope that what I've detailed here can help you better understand how to programmatically alter UAsset files. The methods used here, as far as I'm aware, can be applied to any property in a UAsset file (that can be seen in UAssetGUI - unless that serialization issue is fixed), not just PickAxeDigSize and HitsNeededToMine.


Happy modding miners, rock and stone!

![Big Dig Boi Logo](/Assets/bigdigboi_logo_var1_nobg.png)

## Credits

*First, I want to thank Von for pointing me in the right direction to use [UAssetAPI](https://github.com/atenfyr/UAssetAPI "UAssetAPI"), as well as Enn, 67, and Mitgobla who assisted me further on the Practical DRG Discord server. [UAssetGUI](https://github.com/atenfyr/UAssetGUI "UAssetGUI") was essential to reference/verify the needed properties/values. [DRG Packer](https://github.com/DRG-Modding/tools/blob/9bacb72561a5ce43d84138999bd972158b2b34a0/loose-files/DRGPacker4.27.zip "DRG Packer") is also instrumental in mod creation, and wouldn't be possible without it. To get the files from various mods to make the Master Archive, I used [Modio Direct](https://github.com/Therootexec/ModioDirect "Modio Direct").*

*Thank you again to the aforementioned authors for the mods that helped create the Master Archive (as well as for inspiring me to create this project), and to [Atenfyr](https://github.com/atenfyr "Atenfyr"), [Buckminsterfullerene02](https://github.com/Buckminsterfullerene02 "Buckminsterfullerene02"), [Henri J. Norden](https://github.com/Henri-J-Norden "Henri J. Norden"), [Samamstar](https://github.com/samamstar "Samamstar"), [Trumank](https://github.com/trumank) and [Therootexec](https://github.com/Therootexec "Therootexec") for making/maintaining the tools that make my mod, and many others, possible.*

*Thank you as well to [Pandao](https://github.com/pandao "Pandao") for their [Editor.md project](https://pandao.github.io/editor.md/index.html "Editor.md project") which helped me greatly in making this documentation.*

*Thank you Ghost Ship Games for Deep Rock Galactic.*

*I'm sure I've forgotten to mention a number of people, so my apologies. Your work is also greatly appreciated!*

*Lastly, thanks to my best friend, who taught me a long time ago the basics of C#. Without that, I wouldn't have been able to make this mod. Also, he's a great dude.*

## Shameless Self-Promotion
*You can find my various mods and tools on [Nexus Mods](https://www.nexusmods.com/profile/MatthiosArcanus "Nexus Mods") and [Mod io](https://mod.io/u/hypocrita20xx/ "Mod io"). I also have various videos, such as demonstrations and tutorials for my mods/tools, that you can check out on [Youtube](https://www.youtube.com/@hypocritaafterdark).*

## No AI Use Disclaimer
*No part of these mods, the program, or related assets were made in part or in whole, from concept to completion, with the use of AI. In theory, it's just another tool, but in practice the implementation of AI has serious moral, ethical, and ecological failings, and I will do my best to abstain from participating in the AI scene as much as possible until those issues are rectified in a satisfactory way. All code was created without AI assistance. All images are actual images either taken in-game or made by myself in Photoshop. All text related to this program and these was written without AI assistance. All videos were recorded by myself of myself playing the game using OBS, without the use of AI filters or AI editing.*

## License
*This project and all associated files are licensed under [MIT]([tree/main?tab=MIT-1-ov-file "MIT"](https://github.com/Hypocrita20XX/Big-Dig-Boi-Series-Archive?tab=MIT-1-ov-file)) because I deeply feel that knowledge should be free, and freely shared. If anything I've done for this series of mods has helped you make your own mods, consider throwing a thanks my way, if you get the time.*
