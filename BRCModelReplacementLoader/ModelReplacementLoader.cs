using BepInEx;
using CharacterAPI;
using Reptile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModelReplacementLoader
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency("com.Viliger.CharacterAPI")]
    public class ModelReplacementLoader : BaseUnityPlugin
    {
        public const string ModGuid = "com.Viliger.CharacterAPI_ModelReplacementLoader";
        public const string ModName = "CharacterAPI_ModelReplacementLoader";
        public const string ModVer = "1.0.0";

        public void Awake()
        {
            string modelFolder = Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "ModelReplacements");
            Directory.CreateDirectory(modelFolder);

            var referenceAssetBundle = AssetBundle.LoadFromFile(Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "beat"));
            if (!referenceAssetBundle)
            {
                Logger.LogError("Reference asset bundle is not present.");
                return;
            }

            var referenceGameObject = referenceAssetBundle.LoadAsset<GameObject>("beat_no_blades");
            if(!referenceGameObject)
            {
                Logger.LogError("Reference asset bundle has been replaced.");
                return;
            }

            foreach (string folder in Directory.GetDirectories(modelFolder))
            {
                string characterName = Path.GetFileName(folder);

                string[] configs = Directory.GetFiles(folder, "*.cfg");
                string[] assetBundles = Directory.GetFiles(folder, "*.asset").Concat(Directory.GetFiles(folder, "*.")).ToArray();

                if(configs.Length == 0)
                {
                    Logger.LogMessage($"Directory {folder} contains no config file. Default options will be applied and new config file will be created.");
                }

                string configName = configs.Length > 0 ? configs[0] : string.Concat(Path.Combine(folder, characterName), ".cfg");

                if(assetBundles.Length == 0)
                {
                    Logger.LogWarning($"Directory {folder} contains no asset bundles. Skipping...");
                    continue;
                }

                CharacterConfig characterConfig = new CharacterConfig();
                characterConfig.Init(new BepInEx.Configuration.ConfigFile(configName, true));

                AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundles[0]);
                string assetBundleName = Path.GetFileName(assetBundles[0]);
                if (!assetBundle)
                {
                    Logger.LogMessage($"In directory {folder} asset bundle {assetBundleName} is not actually asset bundle. Skipping...");
                    continue;
                }

                GameObject characterPrefab = assetBundle.LoadAsset<GameObject>("Chara");
                if (!characterPrefab)
                {
                    Logger.LogMessage($"In directory {folder} asset bundle {assetBundleName} has no prefab named \"Chara\". Skipping...");
                    continue;
                }

                using(var moddedCharacter = new ModdedCharacterConstructor())
                {
                    moddedCharacter.characterName = characterName;
                    moddedCharacter.characterPrefab = FixModel(referenceGameObject, characterPrefab);

                    moddedCharacter.AddOutfit(characterPrefab.GetComponentInChildren<SkinnedMeshRenderer>().material);

                    moddedCharacter.personalGraffitiBase = (Characters)characterConfig.charaToReplace.Value;
                    moddedCharacter.tempAudioBase = (Characters)characterConfig.charaToReplace.Value;

                    moddedCharacter.usesCustomShader = characterConfig.shaderOverwritten.Value;

                    if (!characterConfig.inlineSkatesDir.Value.Equals((Vector3)characterConfig.inlineSkatesDir.DefaultValue) 
                        && characterConfig.inlineSkatesDirL.Value.Equals(characterConfig.inlineSkatesDirL.DefaultValue) 
                        && characterConfig.inlineSkatesDirR.Value.Equals(characterConfig.inlineSkatesDirR.DefaultValue))
                    {
                        moddedCharacter.AddSkatePosition(ModdedCharacterConstructor.Skate.Left, characterConfig.inlineSkatesPosL.Value, characterConfig.inlineSkatesDir.Value, characterConfig.inlineSkatesScaleL.Value);
                        moddedCharacter.AddSkatePosition(ModdedCharacterConstructor.Skate.Right, characterConfig.inlineSkatesPosR.Value, characterConfig.inlineSkatesDir.Value, characterConfig.inlineSkatesScaleR.Value);
                    } else
                    {
                        moddedCharacter.AddSkatePosition(ModdedCharacterConstructor.Skate.Left, characterConfig.inlineSkatesPosL.Value, characterConfig.inlineSkatesDirL.Value, characterConfig.inlineSkatesScaleL.Value);
                        moddedCharacter.AddSkatePosition(ModdedCharacterConstructor.Skate.Right, characterConfig.inlineSkatesPosR.Value, characterConfig.inlineSkatesDirR.Value, characterConfig.inlineSkatesScaleR.Value);
                    }

                    moddedCharacter.CreateModdedCharacter();
                    AssetBundle.UnloadAllAssetBundles(false);
                }
            }
        }

        // Copied directly from ModelReplacement
        static GameObject FixModel(GameObject intendedReturnValue, GameObject prefab)
        {
            // Get the Chara prefab from our assetbundle
            Transform baseTransform = UnityEngine.Object.Instantiate(prefab).transform; //SavedVariables.charaPrefab.transform;

            // Get the transform from the character the game actually wants to show
            Transform intendedGameObjectTransform = intendedReturnValue.transform;

            // Compare the gameobjects, if theres an object we dont have in our prefab, we add it for compatibility
            foreach (Transform child in intendedGameObjectTransform.GetAllChildren())
            {
                if (baseTransform.Find(child.name) == null && child.name != "root")
                {
                    child.parent = baseTransform;
                }
            }

            // Set the animator to the intended character's
            baseTransform.GetComponent<Animator>().runtimeAnimatorController = intendedReturnValue.GetComponent<Animator>().runtimeAnimatorController;

            // Spooky ghosts begone
            GameObject.Destroy(intendedReturnValue);

            return baseTransform.gameObject;
        }
    }
}
