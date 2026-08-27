# Source Code And Project Files

Here is where the program and its source code lives. I'm not going to provide a ready-to-run executable, so if you want to use this as-is (and you probably shouldn't) then you'll need to make sure a few things are in place first.<br />

I say that you probably shouldn't run this as provided because this program was made for internal use. No exception handling, minimal input checks, and less-than-stellar logging all make this program kind of a pain to use if you don't know its intricacies.<br />

Nevertheless, because I know that this program works and because I know it's a lot to ask someone to make a program like this from scratch, I'm going to provide what information I can so that you can maybe muddle your way through its nuances.<br />

**Disclaimer: I won't provide support for this! It's provided as-is and it's on you to make it work, and troubleshoot when things go wrong. Please don't ask me why something isn't working (I probably won't know anyway).**

### Index
- [What I Needed To Make This Program](#what-i-needed-to-make-this-program)
- [A Few Notes](#a-few-notes)
- [Basic Operation](#basic-operation)
- [Conclusion](#conclusion)
- [Credits](#credits)
- [License](#license)

### What I Needed To Make This Program
- [Visual Studio Community 2026](https://visualstudio.microsoft.com/vs/community/ "Visual Studio Community 2026")
- [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0 ".NET 10.0 SDK")
- [UAssetAPI](https://www.nuget.org/packages/UAssetAPI/ "UAssetAPI")
- [ConfigurationManager](https://www.nuget.org/packages/System.Configuration.ConfigurationManager/11.0.0-preview.7.26381.103 "System.Configuration.ConfigurationManager")
- [The cursed CS8602 warning to be supressed in Visual Studio](https://learn.microsoft.com/en-us/visualstudio/ide/how-to-suppress-compiler-warnings?view=visualstudio "Supress That Thing Post-Haste")

### A Few Notes

Regarding UAssetAPI, I linked to where you can find it on the Nuget website because it provides the only installation command that worked for me. The one they provide in their [Basic Usage Guide](https://atenfyr.github.io/UAssetAPI/guide/basic.html "Basic Usage Guide") didn't work for me, for some reason.<br />
Just in case that website isn't working, here's the install command:
```
dotnet add package UAssetAPI --version 1.1.0
```
Whenever the API updates, just replace the version number with the latest.<br />

As for the ConfigurationManager, I needed that to make the settings file work. There are numerous options for storing user input across uses, however I found that a settings file was exactly what I needed, and works well enough for my purposes. As with UAssetAPI, here's the install command just in case:
```
dotnet add package System.Configuration.ConfigurationManager --version 11.0.0-preview.7.26381.103
```
Again, whenever it updates, you'll have to replace the version listed to the newest one.<br />

*Side-note: you might have to add your project name to the command, IE:*
```
dotnet add [YourProjectName] package System.Configuration.ConfigurationManager
```
*I don't know why sometimes it's fine without that, and other times it isn't. Visual Studio is just a weird program, I don't know.*<br />

Other than that, I got unreasonably annoyed with the CS8602 "dereference to a null reference" warning. I get it, I'm using string? to get the user's input through Console.ReadLine(), and yes I know that the value *could* be null. However 1.) I don't care and 2.) the only fix I know of (IE string? meh = "stuff I want" ?? "if null") didn't work in all cases, so I gave up and just suppressed that accursed warning. My sanity thanks me for doing that. 
Also I literally am assigning this thing a value *and* checking for null in most cases, so just shove it Visual Studio.<br />

Anyways.

### Basic Operation

I'm going to attempt to run your through operating this thing. Again, no exception handling, minimal input checks, so make sure you type in *exactly* what is being asked *exactly* as you intend.<br />

When you first run the program, it will ask you a number of things:
- Where is your Master Archive located? (You can get a copy of mine [here](/Files/Master_Archive) and more information is available [here](/Files/Master_Archive/Readme.md))
- Where are the base game's files located? (specifically, the unaltered original files unpacked directly from FSD-WindowsNoEditor.pak)
- Where would you like to store the program's generated files?

Spaces in the paths are irrelevant, I at least took that into consideration.<br />

After that, the program runs a check to make sure all necessary folders are located in your project's root folder, creating them if neccessary (and doing a poor job of logging the information to the console - no I probably won't fix that).<br />
Pressing enter (it says "press any button" but only enter seems to work) will bring you to the next series of prompts.<br />

The first asks you which mod(s) you want to make. Note that there are only two operating mods: single and all. So if you want to make 0 and 3 at once, for instance, you're out of luck. You'll have to run the program once to make 0, then again to make 3. It's not great, honestly. Also -1 is pointless because that just ends the program, and you can just close the program yourself if you don't want to do anything. I don't know why I added it as an option.<br />

The next prompt depends on your choice in the previous prompt.<br />
If you've chosen 0, 1, 2, 6, 7, or 8 you'll be asked to provide the desired PickAxeDigSize.<br />
If you've chosen 3, 4, 5, 6, 7, or 8 then you'll be asked to also provide the desired HitsNeededToMine.<br />
If you've chosen 0, 1, 2, you'll only be prompted for the dig size while hits needed will stay at the base game's default value for each file.<br />
If 3, 4, or 5 is selected, you'll only be prompted for hits needed with dig size staying at the default value for each file.<br />
100 means every mod variation will be created, so you'll be prompted to enter values for both properties.<br />
This probably could be done with a string, or some such, but I used an int specifically because I use this variable as an index in various parts of the code to iterate through paths.<br />

Once you've done that, pressing "any button" (enter) will start a verification check of the unedited base game's files. This is needed to verify the Master Archive so that you only get exactly the changes you want, and nothing you don't want. If a property is not readily available in a file (IE TM_Umanite) then it's assumed that it's values are default (In the case of Umanite, it's default for dig size is 105 and hits needed is 2).<br />

There's a lot to unpack regarding this verification check and why the Master Archive is needed. I've already covered it in the documentation though, so check it out [here](/Documentation/Readme.md#the-master-archive "documentation").<br />

Within this verification check also lives a check for each file in the Master Archive and its associated file in the base game's folder to ensure that it only contains default values for both properties. Technically, this is a holdover from before I wrote the code to make a new Master Archive, which at that time contained files from the base game and three other mods. With the new Master Archive, it's not necessary, but it's staying as-is.<br />

Following that, the report generated from the above check is made and then written to each mod's /_Reports folder. I couldn't get a copy operation to work, so I gave up and instead chose to just write a new file for each directory. Inefficient, but effective.<br />

Then, finally, mass UAsset creation begins based on what you've provided and what is contained in the Master Archive. For instance, if you want to make 0, then only PickAxeDigSize will change for only terrain files, with HitsNeededToMine staying at default, and no mineral files will be included.<br />

It's worth mentioning that a small selection of files are not contained within the Master Archive, and as such will not be included in any mod created with this program.<br />
The files are:
- TerrainMaterialCollection .uasset/.uexp
- CTM_Burned .uasset/.uexp
- CTM_CarveError .uasset/.uexp
- CTM_CarvePlaceholder .uasset/.uexp
- CTM_CarveSolid .uasset/.uexp
- CTM_Empty .uasset/.uexp
- DBR_Roots .uasset/.uexp

Looking in UAssetGUI, their data structure differs from normal terrain/mineral files, so I opted to just exclude them instead of adding properties to them that may break something.<br />

At least on my system, making all 9 mods takes between 2 and 3 minutes. You could probably do some async nonsense to run all of this parallel to each other so that you cut that time down to under a minute, but I neither know nor care enough to implement such a thing. The last time I touched async stuff was about 6 years ago and I just can't be bothered to figure it out again. If you want to though, go right ahead, more power to you.<br />

Once all files have been modified (and you've recovered from the nigh-unreadable onslaught of logging lines in the console) you'll be asked to press enter again, which will then create a modified files and excluded files report in each directory's /_Reports folder.<br />

As an aside, this process creates two other folders, _ExcludedFiles and _ToEdit.<br />
Strictly speaking, these are unncessary at this point in the program's lifetime.<br />
 _ExcludedFiles is intended to contain any file that cannot be edited programmatically, and as such was excluded from the list of modified files.<br />
_ToEdit is slightly more specific, intended to contain any file that should be modified, but can only be done so manually.<br />
Again, the current version of the program paired with the Master Archive make these irrelevant (unless/until Ghost Ship Games adds new terrain/mineral files, in which case I guess they become relevant again).<br />

Anyways, hit that enter button again to continue, and this is where my logging gets even more sloppy.<br />

You'll briefly be hit with messages telling you that the aforementioned reports have been made for each mod before it moves right along into creating pak files and moving them into a new directory (making sure that /_Paks is empty and deleting everything in there if not, to remove any old files that might be in there). There needs to be a Console.ReadLine() somewhere in there, but I forgot. Eh, it's fine.<br />

Press enter yet again and now those pak files will be packaged in their own zip file, with a new date/time-appended name so that I can keep track of when the program made something. I decided to do this instead of traditional versioning because I couldn't be bothered to figure out a way to do that automatically. You could probably query Mod io for the live version of a mod's file, then increment according to if it's a major or minor revision. Again though, I can't be bothered, so I did the date-time thing instead.<br />

Once that's done, these zip files will be moved to /_Zips, and, like with the pak files, any old files in there will be deleted.<br />

Then you'll be greeted with some lovely end-of-operation lines and the (possibly infamous) Big Dig Boi catchphrase. Because of course I had to include it.<br />

Hit enter to close the program and enjoy your fancy new mods!

## Conclusion

So if you entered everything exactly as needed exactly as you intended, every mod will have the modified files it needs with the changes you entered. Otherwise, it probably crashed and you're cursing me for making such a mess of a program (see that whole provided as-is thing I mentioned previously).<br />
There's a fair bit going on under the hood, so if you would like more information, check out the [documentation](/Documentation/Readme.md) and if you just want to poke around, you can view the [source itself](/Source/Program.cs).<br />

So long as you're mindful of this program's inadequacies, you shouldn't have too many problems getting it to work. I did the best I could, but the base of what I know regarding C# was taught to me by a friend, and everything from there has been self-taught, so you're going to get whatever you get when it comes to my programming projects (apologies for that, by the way).<br />
All current versions and variations of the mods in the series have been made with it though, so it can't be all bad, right?<br />

Anyways, I wish y'all the best in whatever project you're involved in.<br /><br />

Happy modding miners, rock and stone!

![Big Dig Boi Logo](/Assets/bigdigboi_logo_var1_nobg.png)

## Credits

*First, I want to thank Von for pointing me in the right direction to use [UAssetAPI](https://github.com/atenfyr/UAssetAPI "UAssetAPI"), as well as Enn, 67, and Mitgobla who assisted me further on the Practical DRG Discord server. [UAssetGUI](https://github.com/atenfyr/UAssetGUI "UAssetGUI") was essential to reference/verify the needed properties/values. [DRG Packer](https://github.com/DRG-Modding/tools/blob/9bacb72561a5ce43d84138999bd972158b2b34a0/loose-files/DRGPacker4.27.zip "DRG Packer") is also instrumental in mod creation, and wouldn't be possible without it. To get the files from various mods to make the Master Archive, I used [Modio Direct](https://github.com/Therootexec/ModioDirect "Modio Direct").*

*Thank you again to the aforementioned authors for the mods that helped create the Master Archive (as well as for inspiring me to create this project), and to [Atenfyr](https://github.com/atenfyr "Atenfyr"), [Buckminsterfullerene02](https://github.com/Buckminsterfullerene02 "Buckminsterfullerene02"), [Henri J. Norden](https://github.com/Henri-J-Norden "Henri J. Norden"), [Samamstar](https://github.com/samamstar "Samamstar"), [Trumank](https://github.com/trumank) and [Therootexec](https://github.com/Therootexec "Therootexec") for making/maintaining the tools that make my mod, and many others, possible.*

*Thank you as well to [Pandao](https://github.com/pandao "Pandao") for their [Editor.md project](https://pandao.github.io/editor.md/index.html "Editor.md project") which helped me greatly in making this documentation.*

*Thank you Ghost Ship Games for Deep Rock Galactic.*

*Lastly, thanks to my best friend, who taught me a long time ago the basics of C#. Without that, I wouldn't have been able to make this mod. Also, he's a great dude.*

## Shameless Self-Promotion
*You can find my various mods and tools on [Nexus Mods](https://www.nexusmods.com/profile/MatthiosArcanus "Nexus Mods") and [Mod io](https://mod.io/u/hypocrita20xx/ "Mod io"). I also have various videos, such as demonstrations and tutorials for my mods/tools, that you can check out on [Youtube](https://www.youtube.com/@hypocritaafterdark).*


## License
*This project and all associated files are licensed under [MIT](https://github.com/Hypocrita20XX/Big-Dig-Boi-Series-Archive/tree/main?tab=MIT-1-ov-file) because I deeply feel that knowledge should be free, and freely shared. If anything I've done for this series of mods has helped you make your own mods, consider throwing a thanks my way, if you get the time.*
