using BepInEx.Configuration;
using UnityEngine;

namespace ModelReplacementLoader
{
    public class CharacterConfig
    {
        public ConfigEntry<int> charaToReplace;

        public ConfigEntry<Vector3> inlineSkatesDir;

        public ConfigEntry<Vector3> inlineSkatesDirL;
        public ConfigEntry<Vector3> inlineSkatesPosL;
        public ConfigEntry<Vector3> inlineSkatesScaleL;

        public ConfigEntry<Vector3> inlineSkatesDirR;
        public ConfigEntry<Vector3> inlineSkatesPosR;
        public ConfigEntry<Vector3> inlineSkatesScaleR;

        public ConfigEntry<bool> shaderOverwritten;

        public void Init(ConfigFile config) 
        {
            charaToReplace = config.Bind<int>("General", "charaToReplace", 0, "Which character to replace, taken from the 'Characters' enum. See this image (The numbers are one digit more than they should be, so what would be 2 in this image is actually 1, 3 is 2, 4 is 3, and so on): https://files.catbox.moe/vhda8a.png");

            inlineSkatesDir = config.Bind<Vector3>("General", "inlineSkatesDir", Vector3.zero, "The rotation of inline skates in angles. Modify this to make your skates look correct");

            inlineSkatesDirL = config.Bind<Vector3>("General", "inlineSkatesDirL", Vector3.zero, "The rotation of the left inline skate in angles. Modify this to make your left skate look correct");
            inlineSkatesPosL = config.Bind<Vector3>("General", "inlineSkatesPosL", Vector3.zero, "The Position of the left inline skate relative to the leg bone. Modify this to make your left skate look correct");
            inlineSkatesScaleL = config.Bind<Vector3>("General", "inlineSkatesScaleL", Vector3.one, "The scale of the left inline skate relative to the leg bone. Modify this to make your left skate look correct");

            inlineSkatesDirR = config.Bind<Vector3>("General", "inlineSkatesDirR", Vector3.zero, "The rotation of the right inline skate in angles. Modify this to make your right skate look correct");
            inlineSkatesPosR = config.Bind<Vector3>("General", "inlineSkatesPosR", Vector3.zero, "The Position of the right inline skate relative to the leg bone. Modify this to make your left skate look correct");
            inlineSkatesScaleR = config.Bind<Vector3>("General", "inlineSkatesScaleR", Vector3.one, "The scale of the right inline skate relative to the leg bone.  Modify this to make your left skate look correct");

            shaderOverwritten = config.Bind<bool>("General", "shaderOverwritten", false, "Whether or not we prioritize the shader you set to your material in the Unity editor or the base shader the game uses for outlines and cel-shading");
        }
    }
}
