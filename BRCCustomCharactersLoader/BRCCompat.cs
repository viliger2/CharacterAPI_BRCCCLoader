using BrcCustomCharactersLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CharacterAPI_BRCCCLoader
{
    public static class BRCCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("BrcCustomCharacters");
                }

                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void LoadBrcCustomCharacters()
        {
            Type type = typeof(AssetDatabase);
            Dictionary<Guid, CharacterDefinition> characterObjects = (Dictionary<Guid, CharacterDefinition>)type.GetField("_characterObjects", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            //Dictionary<Guid, string> _characterBundlePaths = (Dictionary<Guid, string>)type.GetField("_characterBundlePaths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            //Dictionary<Guid, SfxCollection> _characterSfxCollections = (Dictionary<Guid, SfxCollection>)type.GetField("_characterSfxCollections", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            //Dictionary<Guid, GameObject> _characterVisuals = (Dictionary<Guid, GameObject>)type.GetField("_characterVisuals", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            //Dictionary<Characters, List<Guid>> _characterReplacementIds = (Dictionary<Characters, List<Guid>>)type.GetField("_characterReplacementIds", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

            foreach (var characterObject in characterObjects.Values)
            {
                BRCCLoader.CreateModdedCharacter(characterObject);
            }

            //_characterBundlePaths.Clear();
            //_characterSfxCollections.Clear();
            //_characterVisuals.Clear();
            //_characterReplacementIds.Clear();
            //characterObjects.Clear();
        }
    }
}
