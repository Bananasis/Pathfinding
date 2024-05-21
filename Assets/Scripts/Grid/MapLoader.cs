using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PathUtils;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Grid
{
    public class MapLoader
    {
        private Dictionary<string, List<string>> maps = new Dictionary<string, List<string>>();
        public List<string> mapTypes => maps.Keys.ToList();
        private static readonly List<string> dirs = new List<string>()
            { "dao-map", "maze-map", "random-map", "room-map" };

        public List<string> GetMapOptions(int type) => maps[dirs[type]];
        public MapLoader()
        {
            for (var i = 0; i < dirs.Count; i++)
            {
                string dir = dirs[i];
                maps[dir] = ResourcesExtension.GetPathsRecursively(dir);
            }
        }

        public IMapData GetMap(string path)
        {
            TextAsset mapAsset = Resources.Load<TextAsset>(path);
            var text = mapAsset.text;
            var heightPattern = @"height\s+(\d+)";
            var widthPattern = @"width\s+(\d+)";
            var mapPattern = @"map\s*";
            var heightMatch = Regex.Match(text, heightPattern);
            var height = heightMatch.Success ? int.Parse(heightMatch.Groups[1].Value) : 0;
            var widthMatch = Regex.Match(text, widthPattern);
            var width = widthMatch.Success ? int.Parse(widthMatch.Groups[1].Value) : 0;
            Match mapMatch = Regex.Match(text, mapPattern, RegexOptions.Singleline);
            int mapStartIndex = mapMatch.Success ? mapMatch.Index + mapMatch.Length - mapMatch.Groups[1].Length : 0;
            IMapData data = new MapData(new Vector2Int(width, height));
            for (int i = 0; i < text.Length - mapStartIndex; i++)
            {
                if (text[i + mapStartIndex] == '@' || text[i + mapStartIndex] == 'T')
                    data[new Vector2Int((i - i / (width + 1)) % width, i / (width + 1))] = false;
            }

            return data;
        }

        public IMapData GetMap(int type, int option)
        {
            return GetMap(maps[dirs[type]][option]);
        }
    }
}