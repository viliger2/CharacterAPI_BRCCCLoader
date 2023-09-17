# CharacterAPI_BRCCCLoader (what a mouthful)
Loader of [BrcCustomCharacters](https://github.com/SGiygas/BrcCustomCharacters) for [CharacterAPI](https://github.com/viliger2/BRC_CharacterAPI).

## How It Works

By default loader loads all compatible asset bundles from "BepInEx\CharacterAPI\ModelReplacements" folder inside your r2modman profile folder or game folder (if you install everything manually). If character comes with multiple asset bundles (for different characters to replace), you need to put only one asset bundle inside the folder, otherwise separate character will be created for each asset bundle. Asset bundles need to be just inside the folder, **not inside any sub folder**. Anything that is not asset bundle will be ignored, so manifest and guid files are not needed. This mode can be disabled by changing "Load BRCCustomCharacters" option in the config file. This mode is enabled by default. **You don't need BrcCustomCharacters for this mode.**

Another mode loads characters directly from BrcCustomCharacters memory. HOWEVER, and this is important, **replacements will still be in place.** Let's say you have a character that replaces Red, his voice and his personal graffiti and then decide to enable this option. Then Red will be replaced by new character, his voice and graffiti also will be replaced AND you will get a new character separate from Red's replacement. In essence, you will get two characters. This mode is here mostly for compatibility and maybe SlopCrew. This mode is disabled by default and can be enabled by changing "Load BRCCustomCharacters from Plugin" option in the config file. **You need BrcCustomCharacters for this mode.**

## Credits

Plugin comes with precompiled version of [BrcCustomCharactersLib](https://github.com/SGiygas/BrcCustomCharactersLib) made by [SGiygas](https://github.com/SGiygas). It is licensed under GPL-3.0 license.