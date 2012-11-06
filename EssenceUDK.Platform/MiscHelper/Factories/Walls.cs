﻿using System;
using System.IO;
using System.Linq;
using EssenceUDK.Platform;
using EssenceUDK.TilesInfo.Components;
using EssenceUDK.TilesInfo.Components.Enums;
using EssenceUDK.TilesInfo.Components.Tiles;
using EssenceUDK.TilesInfo.Interfaces;

namespace EssenceUDK.TilesInfo.Factories
{

    public class Walls : Factory , IFactory 
    {
        #region Implementation of IFactory

        public Walls(UODataManager location) : base(location)
        {
        }

        

        public override void Populate()
        {
        
            var txtFileLines = File.ReadAllLines(DataManager.Location.LocalPath+ "walls.txt");
            var typeNames = txtFileLines[1].Split(Separators);
            TileCategory category = null;
            for (int i = 2; i < txtFileLines.Length; i++)
            {
                var infos = txtFileLines[i].Split('\t');

                if (infos[1] == "0")
                {
                    category = new TileCategory(Int32.Parse(infos[2]),TypeTile.Wall) {Name = infos.Last()};
                    Categories.Add(category);
                }
                var style = new TileStyle();
                category.AddStyle(style);
                style.Name = infos.Last();
                style.Id = Int32.Parse(infos[1]);
                for (int j = 3; j < typeNames.Length - 2; j++)
                {
                    if (infos[j] != "0")
                    {
                        var tile = new TileWall { Id = uint.Parse(infos[j]) };
                        style.AddTile(tile);
                    }
                }
                
            }
            TilesCategorySDKModule.Supp.PositionCheck(Categories);
        }

        #endregion
    }
}
