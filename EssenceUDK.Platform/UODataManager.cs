using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using EssenceUDK.Platform.DataTypes;
using EssenceUDK.Platform.Factories;

namespace EssenceUDK.Platform
{
    [Flags]
    public enum UODataType : ushort
    {
        /// <summary>
        /// Use separated files from HDD as DataSource.
        /// </summary>
        UseExtFiles = 0x0001,
        /// <summary>
        /// Use *.mul files from HDD as DataSource.
        /// </summary>
        UseMulFiles = 0x0002,
        /// <summary>
        /// Use *.uop files from HDD as DataSource.
        /// </summary>
        UseUopFiles = 0x0004,
        /// <summary>
        /// Use *.qup files from HDD as DataSource.
        /// </summary>
        UseQupFiles = 0x0008,

        /// <summary>
        /// Use verdata.mul for patching data from DataSource.
        /// </summary>
        UseVerPatch = 0x0010,
        /// <summary>
        /// Use *.def tables for redeclaration data from DataSource.
        /// </summary>
        UseDefPatch = 0x0020,
        /// <summary>
        /// Use *.dif for patching data from DataSource.
        /// </summary>
        UseDifPatch = 0x0040,
        /// <summary>
        /// Reserved for future porporse
        /// </summary>
        UseReserve1 = 0x0080,

        /// <summary>
        /// Sets use of an old format of the files art.mul, artidx.mul, staidx#.mul, statics#.mul. 
        /// And also disconnects use of texidx.mul, texmaps.mul. This data format was used only in early pre-alpha the client.
        /// </summary>
        UsePreDatas = 0x0100,
        /// <summary>
        /// Sets use of the old TileData.mul format. This format is used by the majority of clients, except the last.
        /// </summary>
        UseOldDatas = 0x0200,
        /// <summary>
        /// Sets use of the new TileData.mul format entered in a buster of "High Seas".
        /// </summary>
        UseNewDatas = 0x0400,
        /// <summary>
        /// Sets use of special data format which is used by Essence Infinity Client
        /// </summary>
        UseInfDatas = 0x0800,

        /// <summary>
        /// Just for classic clients based on uo-ext
        /// see http://code.google.com/p/uo-ext/
        /// </summary>
        UseExtHooks = 0x1000,
        /// <summary>
        /// Reserved for future porporse
        /// </summary>
        UseReserve2 = 0x2000,
        /// <summary>
        /// Reserved for future porporse
        /// </summary>
        UseReserve3 = 0x4000,
        /// <summary>
        /// Reserved for future porporse
        /// </summary>
        UseReserve4 = 0x8000,
        
        /// <summary>
        /// For future purporse....
        /// </summary>
        EssenceInfinityClient               = UseQupFiles | UseInfDatas,

        /// <summary>
        /// Classic Client (UO: Pre Alpha Client)
        /// Versions: ---
        /// </summary>
        ClassicPreAlphaVersion              = UseMulFiles | UseOldDatas,
        /// <summary>
        /// Classic Client (UO: Shattered Legacy)
        /// Versions: from 1.23.27 up to 1.26.4j
        /// </summary>
        ClassicShatteredLegacy              = UseMulFiles,
        /// <summary>
        /// Classic Client (UO: Renaissance)
        /// Versions: from 2.0.0 up to 2.0.9a
        /// </summary>
        ClassicRenaissance                  = UseMulFiles | UseVerPatch,
        /// <summary>
        /// Classic Client (UO: Third Dawn)
        /// Versions: from 3.0.0 up to 3.0.7a
        /// </summary>
        ClassicThirdDawn                    = UseMulFiles | UseVerPatch,
        /// <summary>
        /// Classic Client (UO: Blackthorn's Revenge)
        /// Versions: from 3.0.7b up to 3.0.8r
        /// </summary>
        ClassicLordBlackthornsRevenge       = UseMulFiles | UseVerPatch,
        /// <summary>
        /// Classic Client (UO: Age of Shadows)
        /// Versions: from 3.0.8z up to 4.0.4b2
        /// </summary>
        ClassicAgeOfShadows                 = UseMulFiles | UseVerPatch,
        /// <summary>
        /// Classic Client (UO: Samurai Empire)
        /// Versions: from 4.0.5a up to 4.0.11c
        /// </summary>
        ClassicSamuraiEmpire                = UseMulFiles | UseVerPatch,
        /// <summary>
        /// Classic Client (UO: Mondain's Legacy)
        /// Versions: from 4.0.11d up to 6.0.14.2
        /// </summary>
        ClassicMondainsLegacy               = UseMulFiles | UseDifPatch,
        /// <summary>
        /// Classic Client (UO: Stygian Abyss)
        /// Versions: from 6.0.14.3 up to 7.0.8.2
        /// </summary>
        ClassicStygianAbyss                 = UseMulFiles | UseDifPatch,
        /// <summary>
        /// Classic Client (UO: Adventures On High Seas)
        /// Versions: from 7.0.8.44 up to 7.0.23.1
        /// </summary>
        ClassicAdventuresOnHighSeas         = UseMulFiles | UseDifPatch | UseNewDatas,
        /// <summary>
        /// Classic Client (UO: Adventures On High Seas)
        /// Versions: from 7.0.23.2 up to 7.0.X.X
        /// </summary>
        ClassicAdventuresOnHighSeasUpdated  = UseUopFiles | UseDifPatch | UseNewDatas,
    }

    public class UODataManager
    {
        public readonly UODataType DataType;
        public readonly Uri        Location;
        public readonly bool       RealTime;

        private readonly IDataFactory dataFactory;

        //public static UODataManager[] Instanes { get { return m_Instanes.Values; } }
        private static Hashtable m_Instanes = new Hashtable(2);

        public UODataManager(Uri uri, UODataType type, bool realtime = true)
        {
            if (uri == null || type == 0)
                throw new ArgumentException("Bad parametre values");
            if (m_Instanes.ContainsKey(uri))
                throw new ArgumentException("Already inited with selected Uri");

            Location = uri;
            DataType = type;
            RealTime = realtime;
            m_Instanes[uri] = this;

            dataFactory = type.HasFlag(UODataType.UseMulFiles) || type.HasFlag(UODataType.UseUopFiles) ? new ClassicFactory(uri, type, realtime) : null;

            // Initialize data... its loading, wait, wait
            // TODO: We need separeted thread for data working
            StorageLand = dataFactory.GetLandTiles();
            StorageItem = dataFactory.GetItemTiles();
        }

        public UODataManager()
            : this(new Uri(@"C:\Ultima\Clients\ML"), UODataType.ClassicAdventuresOnHighSeas, false)
        {
            
        }

        // Cached storages (always using caching)
        private ILandTile[] StorageLand;
        private IItemTile[] StorageItem;
        

        #region StorageItem Interfaces

        public IItemTile GetItemTile(int id)
        {
            return StorageItem[id];
        }

        public IItemTile GetItemTile(uint id)
        {
            return StorageItem[id];
        }

        public IItemTile GetItemTile(short id)
        {
            return StorageItem[id];
        }

        public IItemTile GetItemTile(ushort id)
        {
            return StorageItem[id];
        }

        public IEnumerable<ModelItemData> GetItemTile()
        {

            //var arr = new ItemTile[5000];
            //var arr = new ItemTile[25000];
            var arr = new ItemTile[1000];
            Array.Copy(StorageItem, arr, arr.Length);
            //Array.Copy(StorageItem, 5000, arr, 0, arr.Length);
            //Array.Copy(StorageItem, 0xE000, arr, 0, arr.Length);
            return new ObservableCollection<ModelItemData>(arr.Select(t => new ModelItemData(t)));
            //return new ObservableCollection<ModelItemData>(StorageItem.Select(t => new ModelItemData(t)));
        }

        //public IEnumerable<ModelItemData> GetItemTile(TileFlag flags = TileFlag.None)
        //{
        //    return new ObservableCollection<ModelItemData>(StorageItem.Where(t => t.Flags.HasFlag(flags)).Select(t => new ModelItemData(t)));
        //}

        #endregion


     }
}
