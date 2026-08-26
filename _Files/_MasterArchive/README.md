# Master Archive Files Required To Make This Work

This is an archive that contains an amalgamation of base game files and modded files that is needed to make this series of mods.

I ran into a serialization issue with UAssetAPI that prevented me from adding PickAxeDigSize and/or HitsNeededToMine to any file that doesn't that property exposed in UAssetGUI. The solution is what's present in this archive: a compilation of files that have all the necessary properties added to them, at the base game's values, so that they can be modified as needed to contain what value is desired for those two properties.

Most of the files are sourced from the game itself while other files are sourced from the following mods:
1. [Better Digging](https://mod.io/g/drg/m/better-digging) by [Dreyda](https://mod.io/g/drg/u/dreyda)
2. [Better Pickaxe](https://mod.io/g/drg/m/better-pickaxe2) by [-Toki-Shak-](https://mod.io/g/drg/u/dedmraz)
3. [Even Better Mining V2](https://mod.io/g/drg/m/even-better-mining-v2) by [Ratchet7x5](https://mod.io/g/drg/u/ratchet7x5)

Note: The files sourced from those mods had to programmatically altered to ensure base game values. Otherwise, you end up with files having non-default dig sizes and hits needed, and it's just a mess to sort through. The code to do this is in the main project, within CreateNewMaster().

Huge thanks to these authors, without their work I couldn't have made the Big Dig Boi series of mods as all-encompassing as they are now.

It's worth noting that a handful of files aren't in this archive because their data suggests that they aren't directly interactable by the player, and so those properties would most likely be useless if added. I removed them because I don't want to deal with them within the program. Much easier to just remove them than filter them out, in my opinion.

The excluded files are as follows:
1. TerrainMaterialCollection .uasset/.uexp
2. CTM_Burned .uasset/.uexp
3. CTM_CarveError .uasset/.uexp
4. CTM_CarvePlaceholder .uasset/.uexp
5. CTM_CarveSolid .uasset/.uexp
6. CTM_Empty .uasset/.uexp
7. DBR_Roots .uasset/.uexp

Lastly, the unaltered archive compilation is also stored here in .zip form. I don't want to get rid of it entirely, but I need it to go somewhere else that isn't in my root mod creation folder.
