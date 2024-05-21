#if UNITY_EDITOR
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Utils
{
    [ScriptedImporter(1, "map")]
    public class SrtImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            TextAsset subAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("text", subAsset);
            ctx.SetMainObject(subAsset);
        }
    }
}
#endif