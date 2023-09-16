using BepInEx;
using BepInEx.Configuration;
using BrcCustomCharactersLib;
using CharacterAPI;
using Reptile;
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
        public const string ModVer = "1.0.3";

        public static ConfigEntry<bool> LoadBRCCCharacters;

        public static ConfigEntry<bool> LoadBRCCPlugin;

        internal const string VOICE_BOOST = "_boost";
        internal const string VOICE_COMBO = "_combo";
        internal const string VOICE_DIE = "_die";
        internal const string VOICE_DIEFALL = "_falldamage";
        internal const string VOICE_GETHIT = "_gethit";
        internal const string VOICE_JUMP = "_jump";
        internal const string VOICE_TALK = "_talk";

        public void Awake()
        {
            LoadBRCCCharacters = Config.Bind<bool>("BRCCustomCharacters Loader", "Load BRCCustomCharacters", true, "Loads characters made for BRCCustomCharacters as their own characters. It loads from \"BRCCustomCharacters\" folder.");
            LoadBRCCPlugin = Config.Bind<bool>("BRCCustomCharacters Loader", "Load BRCCustomCharacters from Plugin", false, "Loads characters FROM BRCCustomCharacters as their own characters. This is different from the option above. It means that any supported character loaded by BRCCustomCharacters will also recieve an independent copy. This, however, will not disable any replacements (voice, character, etc).");

            if (LoadBRCCCharacters.Value)
            {
                LoadBrcCCharacters(Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "BrcCustomCharacters"));
                LoadBrcCCharacters(Path.Combine(System.IO.Path.GetDirectoryName(CharacterAPI.CharacterAPI.DllPath), "BrcCustomCharacters"));
            }

            if (LoadBRCCPlugin.Value && BRCCompat.enabled)
            {
                BRCCompat.LoadBrcCustomCharacters();
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
                    LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceDie, VOICE_DIE);
                    LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceDieFall, VOICE_DIEFALL);
                    LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceTalk, VOICE_TALK);
                    LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceBoostTrick, VOICE_BOOST);
                    LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceCombo, VOICE_COMBO);
                    LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceGetHit, VOICE_GETHIT);
                    LoadVoicesFromArray(moddedCharacter.audioClips, definition.VoiceJump, VOICE_JUMP);
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
