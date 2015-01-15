﻿﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using EssenceUDK.Platform.DataTypes;
using EssenceUDK.Platform.Factories;
﻿﻿using EssenceUDK.Platform.TileEngine;
﻿﻿using EssenceUDK.Platform.UtilHelpers;

namespace EssenceUDK.Platform
{
    [Flags]
    public enum UODataTypeVersion : ushort
    {
        /// <summary>
        /// SL client (experemental support)
        /// </summary>
        PreAlpha        = UODataType.UseMulFiles | UODataType.UseOldDatas,
        /// <summary>
        /// Old clients (off not support, pre AoS clients may have much more problems)
        /// </summary>
        OldClassic      = UODataType.UseMulFiles | UODataType.UseVerPatch,
        /// <summary>
        /// ML client
        /// </summary>
        Classic         = UODataType.UseMulFiles | UODataType.UseDefPatch,
        /// <summary>
        /// SA and HS up to 7.0.23.1
        /// </summary>
        NewClassic      = UODataType.UseMulFiles | UODataType.UseDefPatch | UODataType.UseNewDatas,
        /// <summary>
        /// Latest HS clients (only latest is off suported)
        /// </summary>
        LegacyClassic   = UODataType.UseUopFiles | UODataType.UseDefPatch | UODataType.UseNewDatas,
        /// <summary>
        /// not implemented yet, cant say about flags.....
        /// </summary>
        Enchanced       = 0,
        /// <summary>
        /// Custom client based on UDK (reserved for future)
        /// </summary>
        Essence         = UODataType.UseMulFiles | UODataType.UseExtFiles,
        _DataTypeMask   = PreAlpha | OldClassic | Classic | NewClassic | LegacyClassic | Enchanced | Essence
    }

    [Flags]
    public enum UODataTypeOptions : ushort
    {
        None            = 0x0000,
        /// <summary>
        /// Use separated files from HDD as DataSource.
        /// </summary>
        UseExtFiles     = UODataType.UseExtFiles,
        /// <summary>
        /// Use verdata.mul for patching data from DataSource.
        /// </summary>
        UseVerPatch     = UODataType.UseVerPatch,
        /// <summary>
        /// Use *.dif for patching data from DataSource.
        /// </summary>
        UseDifPatch     = UODataType.UseDifPatch,
        /// <summary>
        /// Use "_x" versions of map/statics
        /// </summary>
        UseExtFacet     = UODataType.UseExtFacet,
        _DataTypeMask   = UseExtFiles | UseVerPatch | UseDifPatch | UseExtFacet
    }

    [Flags]
    public enum UODataType : ushort
    {
        Inavalide   = 0x0000,

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
        /// Use *.emp files from HDD as DataSource.
        /// </summary>
        UseEmpFiles = 0x0008,

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
        /// Use "_x" versions of map/statics
        /// </summary>
        UseExtFacet = 0x0080,

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
        EssenceInfinityClient               = UseEmpFiles | UseInfDatas,

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
        ClassicAgeOfShadows                 = UseMulFiles | UseDefPatch | UseVerPatch,
        /// <summary>
        /// Classic Client (UO: Samurai Empire)
        /// Versions: from 4.0.5a up to 4.0.11c
        /// </summary>
        ClassicSamuraiEmpire                = UseMulFiles | UseDefPatch | UseVerPatch,
        /// <summary>
        /// Classic Client (UO: Mondain's Legacy)
        /// Versions: from 4.0.11d up to 6.0.14.2
        /// </summary>
        ClassicMondainsLegacy               = UseMulFiles | UseDefPatch | UseDifPatch,
        /// <summary>
        /// Classic Client (UO: Stygian Abyss)
        /// Versions: from 6.0.14.3 up to 7.0.8.2
        /// </summary>
        ClassicStygianAbyss                 = UseMulFiles | UseDefPatch | UseDifPatch,
        /// <summary>
        /// Classic Client (UO: Adventures On High Seas)
        /// Versions: from 7.0.8.44 up to 7.0.23.1
        /// </summary>
        ClassicAdventuresOnHighSeas         = UseMulFiles | UseDefPatch | UseDifPatch | UseNewDatas,
        /// <summary>
        /// Classic Client (UO: Adventures On High Seas)
        /// Versions: from 7.0.23.2 up to 7.0.X.X
        /// </summary>
        ClassicAdventuresOnHighSeasUpdated  = UseUopFiles | UseDefPatch | UseDifPatch | UseNewDatas,
    }

    public class UODataOptions
    {
        public FacetDesc[] majorFacet = new[] { FacetDesc.NewFelucca, FacetDesc.NewFelucca, FacetDesc.Ilshenar, FacetDesc.Malas, FacetDesc.Tokuno, FacetDesc.TerMur };
        public FacetDesc[] minorFacet = new[] { FacetDesc.ExtFelucca, FacetDesc.ExtTrammel, FacetDesc.Ilshenar, FacetDesc.Malas, FacetDesc.Tokuno, FacetDesc.TerMur };

        /// <summary>
        /// Optimize UODataManager work for speedup rending. Reserved for future purporse.
        /// Recomended to enable only in hard applications like UO Client or map editor, otherwise
        /// it can get reverse effect and slows work, espesialy with GDI\WPF based applications.
        /// </summary>
        public bool OptimizeForEngine = false;

        /// <summary>
        /// Use local cache for read and write extended files, otherwise manager will work with them 
        /// as they are common client files. If this option enable it make extension files virtual
        /// and hidden from user. Manager will save them in hidden temp folder using hash of client 
        /// uri addres to detect them. This option is good to enable for viewer mode to protect for
        /// creating new files and to disable for client on which you works.
        /// Requires UODataType.UseExtFiles flag option.
        /// </summary>
        public bool LocalWorkExtFiles = false;

        public UODataOptions()
        {
        }

        internal UODataOptions(FacetDesc[] facets)
        {
            majorFacet = minorFacet = facets;
        }

        internal UODataOptions(Uri uri)
        {
            // TODO: auto detect for default values ...
        }
    }

    public sealed class UODataManager : BaseSysFactory, IDisposable
    {
        public readonly UODataType DataType;
        public readonly Language   Language;
        public readonly Uri        Location;
        public readonly bool       RealTime;

        public UODataOptions DataOptions { get { return dataOptions; } }
        public IDataFactory  DataFactory { get { return dataFactory; } }
        public FacetRender   FacetRender { get { return facetRender ?? (facetRender = new FacetRender(this)); } }
        private          UODataOptions dataOptions;
        private readonly IDataFactory  dataFactory;
        private          FacetRender   facetRender;

        //public static UODataManager[] Instanes { get { return m_Instanes.Values; } }
        private static Hashtable m_Instanes = new Hashtable(2);

        /// <summary>
        /// Create and initialized UODataManager object, wich represent viewmodel and logic of UO data.
        /// For extended variant with more options see another declaration.
        /// </summary>
        /// <param name="uri">Folder path to client data or data-server address. At this momment only local path are supported.</param>
        /// <param name="version">UO Data version</param>
        /// <param name="features">UO Additional features/</param>
        /// <param name="language">Specify language that used in data files and server. If null Default Language will be used/</param>
        /// <param name="facets">Descrition of maps, use null for auto detecting.</param>
        public UODataManager(Uri uri, UODataTypeVersion version, UODataTypeOptions options = UODataTypeOptions.None, Language language = null, FacetDesc[] facets = null)
            : this(uri, (UODataType)version | (UODataType)options, language ?? Language.English, facets != null ? new UODataOptions(facets) : null)
        {
        }

        /// <summary>
        /// Create and initialized UODataManager object, wich represent viewmodel and logic of UO data.
        /// </summary>
        /// <param name="uri">Folder path to client data or data-server address. At this momment only local path are supported.</param>
        /// <param name="type">Combination of flags to specify general behavior of UOEngine. 
        /// * if you want to use _x version of maps and statics use UseExtFacet flag (only for SA and HS),
        /// * to use special abilities of UOEngine you need additional files, to do it use UseExtFiles flag.</param>
        /// <param name="language">Specify language that used in data files and server.</param>
        /// <param name="dataoptions">Additional options. Set it 'null' for autodetect.</param>
        /// <param name="realtime">If true, engine will save all data at realtime, otherwise it will caching them and save changes in local folder.</param>
        public UODataManager(Uri uri, UODataType type, Language language, UODataOptions dataoptions = null, bool realtime = true)
        {
            if (uri == null || type == 0 || language == null)
                throw new ArgumentException("Bad parametre values");
            if (m_Instanes.ContainsKey(uri))
                throw new ArgumentException("Already inited with selected Uri");

            Location = uri;
            DataType = type;
            Language = language;
            RealTime = realtime;
            m_Instanes[uri] = this;

            dataOptions = dataoptions ?? new UODataOptions(Location);
            dataFactory = (type.HasFlag(UODataType.UseMulFiles) || type.HasFlag(UODataType.UseUopFiles)) ? new ClassicFactory(this) : null;

            // Initialize data... its loading, wait, wait
            // TODO: We need separeted thread for data working
            StorageMaps = dataFactory.GetMapFacets();
            StorageLand = dataFactory.GetLandTiles();
            StorageItem = dataFactory.GetItemTiles();
            StorageGump = dataFactory.GetGumpSurfs();
            StorageAnim = dataFactory.GetAnimations();
        }

        public void Dispose()
        {
            m_Instanes.Remove(Location);
            StorageLand = null;
            StorageItem = null;
            StorageAnim = null;
        }

        // Cached storages (always using caching)
        private IMapFacet[]  StorageMaps;
        private ILandTile[]  StorageLand;
        private IItemTile[]  StorageItem;
        private IGumpEntry[] StorageGump;
        private IAnimation[] StorageAnim;
        

        #region StorageItem Interfaces

        public uint MapsCapacity
        {
            get { return (uint)StorageMaps.Length; }
        }

        public IMapFacet GetMapFacet(int id)
        {
            return StorageMaps[id];
        }

        public IMapFacet GetMapFacet(uint id)
        {
            return StorageMaps[id];
        }

        public IMapFacet GetMapFacet(short id)
        {
            return StorageMaps[id];
        }

        public IMapFacet GetMapFacet(ushort id)
        {
            return StorageMaps[id];
        }

        public IMapFacet GetMapFacet(byte id)
        {
            return StorageMaps[id];
        }

        public uint ItemCapacity 
        {
            get { return (uint)StorageItem.Length; }
        }

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
            return new ObservableCollection<ModelItemData>(StorageItem.Select(t => new ModelItemData(t)));
        }

        public IEnumerable<ModelItemData> GetItemTile(TileFlag flags = TileFlag.None, bool valid = true)
        {
            return new ObservableCollection<ModelItemData>(StorageItem.Where(t => t.IsValid == valid && t.Flags.HasFlag(flags)).Select(t => new ModelItemData(t)));
        }

        public uint LandCapacity
        {
            get { return (uint)StorageLand.Length; }
        }

        public ILandTile GetLandTile(int id)
        {
            return StorageLand[id];
        }

        public ILandTile GetLandTile(uint id)
        {
            return StorageLand[id];
        }

        public ILandTile GetLandTile(short id)
        {
            return StorageLand[id];
        }

        public ILandTile GetLandTile(ushort id)
        {
            return StorageLand[id];
        }

        public IEnumerable<ModelLandData> GetLandTile()
        {
            return new ObservableCollection<ModelLandData>(StorageLand.Select(t => new ModelLandData(t)));
        }

        public IEnumerable<ModelLandData> GetLandTile(TileFlag flags = TileFlag.None, bool valid = true)
        {
            return new ObservableCollection<ModelLandData>(StorageLand.Where(t => t.IsValid == valid && t.Flags.HasFlag(flags)).Select(t => new ModelLandData(t)));
        }

        public uint GumpCapacity
        {
            get { return (uint)StorageGump.Length; }
        }

        public IGumpEntry GetGumpSurf(int id)
        {
            return StorageGump[id];
        }

        public IGumpEntry GetGumpSurf(uint id)
        {
            return StorageGump[id];
        }

        public IGumpEntry GetGumpSurf(short id)
        {
            return StorageGump[id];
        }

        public IGumpEntry GetGumpSurf(ushort id)
        {
            return StorageGump[id];
        }

        public IEnumerable<ModelGumpSurf> GetGumpSurf()
        {
            return new ObservableCollection<ModelGumpSurf>(StorageGump.Select(t => new ModelGumpSurf(t)));
        }

        public IEnumerable<ModelGumpSurf> GetGumpSurf(bool valid = true)
        {
            return new ObservableCollection<ModelGumpSurf>(StorageGump.Where(t => t.IsValid == valid).Select(t => new ModelGumpSurf(t)));

        }

        #endregion


     }
}
