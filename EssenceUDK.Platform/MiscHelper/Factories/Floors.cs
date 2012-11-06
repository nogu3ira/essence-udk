using System.Linq;
using System.IO;
using EssenceUDK.Platform;
using EssenceUDK.TilesInfo.Components;
using EssenceUDK.TilesInfo.Components.Tiles;
using EssenceUDK.TilesInfo.Interfaces;

namespace EssenceUDK.TilesInfo.Factories
{
    public class Floors : Factory , IFactory
    {

        public Floors(UODataManager location) : base(location)
        {
           
        }

        public override void Populate()
        {
            var txtFileLines = File.ReadAllLines(DataManager.Location.LocalPath + "floors.txt");
            var typeNames = txtFileLines[1].Split(Separators);

            for (int i = 2; i < txtFileLines.Length; i++)
            {
                var infos = txtFileLines[i].Split('\t');
                var category = new TileCategory();
                category.Name = infos.Last();
                
                var style = new TileStyle();
                category.AddStyle(style);

                for (int j = 1; j < typeNames.Length-2; j++)
                {
                    if(infos[j]!="0")
                    {
                        var tile = new TileFloor {Id = uint.Parse(infos[j])};
                        style.AddTile(tile);
                        tile.Position= j;
                    }
                }
                Categories.Add(category);
            }
        }
    }
}
