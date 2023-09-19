using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BrcCustomCharactersLib;
using CharacterAPI;
using Reptile;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CharacterAPI_BRCCCLoader
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency("com.Viliger.CharacterAPI")]
    [BepInDependency("BrcCustomCharacters", BepInDependency.DependencyFlags.SoftDependency)]

    public class BRCCLoader : BaseUnityPlugin
    {
        public const string ModGuid = "com.Viliger.CharacterAPI_BRCCCLoader";
        public const string ModName = "CharacterAPI_BRCCCLoader";
        public const string ModVer = "1.0.5";

        private const string FOLDER_NAME = "BrcCustomCharacters";

        public static ManualLogSource logger;

        public static ConfigEntry<bool> LoadBRCCCharacters;

        public static ConfigEntry<bool> LoadBRCCPlugin;

        public void Awake()
        {
            logger = this.Logger;

            LoadBRCCCharacters = Config.Bind<bool>("BRCCustomCharacters Loader", "Load BRCCustomCharacters", true, "Loads characters made for BRCCustomCharacters as their own characters. It loads from \"BRCCustomCharacters\" folder.");
            LoadBRCCPlugin = Config.Bind<bool>("BRCCustomCharacters Loader", "Load BRCCustomCharacters from Plugin", false, "Loads characters FROM BRCCustomCharacters as their own characters. This is different from the option above. It means that any supported character loaded by BRCCustomCharacters will also recieve an independent copy. This, however, will not disable any replacements (voice, character, etc).");

            AttemptToMoveCharacters(Info);

            if (LoadBRCCCharacters.Value)
            {
                LoadBrcCCharacters(Path.Combine(CharacterAPI.CharacterAPI.NewSavePath, FOLDER_NAME));
            }

            if (LoadBRCCPlugin.Value && BRCCompat.enabled)
            {
                BRCCompat.LoadBrcCustomCharacters();
            }

        }

        private static void AttemptToMoveCharacters(PluginInfo Info)
        {
            try
            {
                string oldPathAPIFolder = Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), FOLDER_NAME);
                string oldPathLoaderFodler = Path.Combine(System.IO.Path.GetDirectoryName(CharacterAPI.CharacterAPI.DllPath), FOLDER_NAME);

                string newPath = Path.Combine(CharacterAPI.CharacterAPI.NewSavePath, FOLDER_NAME);
                Directory.CreateDirectory(newPath);
                if (Directory.Exists(oldPathAPIFolder)) 
                { 
                    foreach (string filePath in Directory.GetFiles(oldPathAPIFolder))
                    {
                        File.Move(filePath, Path.Combine(newPath, Path.GetFileName(filePath)));
                    }
                    Directory.Delete(oldPathAPIFolder, true);
                }

                if (Directory.Exists(oldPathLoaderFodler))
                {
                    foreach (string filePath in Directory.GetFiles(oldPathLoaderFodler))
                    {
                        File.Move(filePath, Path.Combine(newPath, Path.GetFileName(filePath)));
                    }
                }
            }
            catch(Exception e)
            {
                logger.LogWarning($"Exception during moving character asset bundles to new location. Exception: {e}, Message: {e.Message}.");
            }
        }

        private static void LoadBrcCCharacters(string pluginPath)
        {
            Directory.CreateDirectory(pluginPath);

            foreach (string filePath in Directory.GetFiles(pluginPath))
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
                if (bundle != null)
                {
                    CharacterDefinition definition = null;
                    GameObject[] objects = bundle.LoadAllAssets<GameObject>();
                    if(objects.Length == 0)
                    {
                        logger.LogWarning($"Asset bundle {Path.GetFileName(filePath)} doesn't have any prefabs.");
                        continue;
                    }
                    foreach (GameObject obj in objects)
                    {
                        definition = obj.GetComponent<CharacterDefinition>();
                        if (definition)
                        {
                            break;
                        }
                    }

                    if (definition)
                    {
                        CreateModdedCharacter(definition);
                    } else
                    {
                        logger.LogWarning($"Asset bundle {Path.GetFileName(filePath)} doesn't have CharacterDefinition class attached to its prefab.");
                    }
                }
            }
        }

        internal static void CreateModdedCharacter(CharacterDefinition definition)
        {
            using (var moddedCharacter = new ModdedCharacterConstructor())
            {
                moddedCharacter.characterName = definition.CharacterName;
                moddedCharacter.characterPrefab = definition.gameObject;

                foreach (Material outfit in definition.Outfits)
                {
                    moddedCharacter.AddOutfit(outfit);
                }

                if (definition.Graffiti)
                {
                    moddedCharacter.AddPersonalGraffiti(definition.GraffitiName, definition.GraffitiArtist, definition.Graffiti, definition.Graffiti.mainTexture);
                }
                else
                {
                    moddedCharacter.personalGraffitiBase = (Characters)definition.CharacterToReplace;
                }

                if (HasVoices(definition))
                {
                    LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceDie, CharacterAPI.Hooks.CoreHooks.VOICE_DIE);
                    LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceDieFall, CharacterAPI.Hooks.CoreHooks.VOICE_DIEFALL);
                    LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceTalk, CharacterAPI.Hooks.CoreHooks.VOICE_TALK);
                    LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceBoostTrick, CharacterAPI.Hooks.CoreHooks.VOICE_BOOST);
                    LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceCombo, CharacterAPI.Hooks.CoreHooks.VOICE_COMBO);
                    LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceGetHit, CharacterAPI.Hooks.CoreHooks.VOICE_GETHIT);
                    LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceJump, CharacterAPI.Hooks.CoreHooks.VOICE_JUMP);
                }
                else
                {
                    moddedCharacter.tempAudioBase = (Characters)definition.CharacterToReplace;
                }

                moddedCharacter.canBlink = definition.CanBlink;
                moddedCharacter.usesCustomShader = !definition.UseReptileShader;

                moddedCharacter.CreateModdedCharacter();
            }
        }

        internal static bool HasVoices(CharacterDefinition definition)
        {
            return definition.VoiceDie.Length != 0 || definition.VoiceDieFall.Length != 0 || definition.VoiceTalk.Length != 0 || definition.VoiceBoostTrick.Length != 0 || definition.VoiceCombo.Length != 0 || definition.VoiceGetHit.Length != 0 || definition.VoiceJump.Length != 0;
        }

        internal static void LoadVoicesFromArray(List<AudioClip> audioClips, AudioClip[] source, string type)
        {
            foreach (AudioClip clip in source)
            {
                if (clip)
                {
                    AudioClip newClip = UnityEngine.Object.Instantiate(clip);
                    newClip.name += type;
                    audioClips.Add(newClip);
                }
            }
        }
    }
}
