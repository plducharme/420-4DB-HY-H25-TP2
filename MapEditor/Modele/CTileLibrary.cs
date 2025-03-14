using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace HugoLandEditeur
{
    /// <summary>
    /// Summary description for CTileLibrary.
    /// </summary>
    public class CTileLibrary
    {
        private int m_Count;            // number of tiles
        private Bitmap m_TileSource;        // to be loaded from external File or resource...
        private int m_Width;
        private int m_Height;
        private Dictionary<int, Tile> _ObjMonde = new Dictionary<int, Tile>();

        public Dictionary<int, Tile> ObjMonde
        {
            get { return _ObjMonde; }
            set { _ObjMonde = value; }
        }

        // Count
        public int Count
        {
            get
            {
                return m_Count;
            }
            set
            {
                m_Count = value;
            }
        }

        //Width
        public int Width
        {
            get
            {
                return m_Width; //m_TileSource.Width;;
            }
        }

        //Height
        public int Height
        {
            get
            {
                return m_Height; //m_TileSource.Height;;
            }
        }


        public CTileLibrary()
        {
            string sPath = AppDomain.CurrentDomain.BaseDirectory + "GameData\\AllTiles.bmp";

            m_TileSource = new Bitmap(sPath);

            m_Width = (m_TileSource.Width / csteApplication.TILE_WIDTH_IN_IMAGE) + 1;
            m_Height = (m_TileSource.Height / csteApplication.TILE_HEIGHT_IN_IMAGE) + 1;

            readTileDefinitions(@"gamedata\AllTilesLookups.csv");
        }

        public void GetSourceRect(Rectangle sourcerect, int ID)
        {
            sourcerect.X = ID % csteApplication.TILE_WIDTH_IN_LIBRARY;
            sourcerect.Y = ID / csteApplication.TILE_HEIGHT_IN_LIBRARY;
            sourcerect.Width = csteApplication.TILE_WIDTH_IN_IMAGE;
            sourcerect.Height = csteApplication.TILE_HEIGHT_IN_IMAGE;
        }

        /// <summary>
        /// Hugo St-Louis : Cette fonction permet de retourner l'index 
        /// du dessin par rapport à cet objet.
        /// </summary>
        /// <param name="xindex"></param>
        /// <param name="yindex"></param>
        /// <returns></returns>
        public int TileToTileID(int xindex, int yindex)
        {
            if (xindex > m_Width)
                xindex = m_Width;
            if (yindex > m_Height)
                yindex = m_Height;
            return (yindex * 10 + xindex);
        }

        public void PointToBoundingRect(int x, int y, ref Rectangle bounding)
        {
            x = x / csteApplication.TILE_WIDTH_IN_IMAGE;
            y = y / csteApplication.TILE_HEIGHT_IN_IMAGE;
            bounding.Size = new Size(csteApplication.TILE_WIDTH_IN_IMAGE + 6, csteApplication.TILE_HEIGHT_IN_IMAGE + 6);
            bounding.X = (x * csteApplication.TILE_WIDTH_IN_IMAGE) - 3;
            bounding.Y = (y * csteApplication.TILE_HEIGHT_IN_IMAGE) - 3;
        }

        /// <summary>
        ///  Each line contains a comma delimited tile definition that the tile constructor understands.
        /// </summary>
        /// <param name="tileDescriptionFile"></param>
        private void readTileDefinitions(string tileDescriptionFile)
        {
            using (StreamReader stream = new StreamReader(tileDescriptionFile))
            {
                string line;
                int id;
                while ((line = stream.ReadLine()) != null)
                {
                    //separate out the elements of the 
                    string[] elements = line.Split(',');

                    Tile objMonde;
                    objMonde = new Tile(elements);
                    //Index dans le tableau
                    id = TileToTileID(objMonde.X_Image, objMonde.Y_Image);
                    Console.WriteLine("ID = " + id.ToString());
                    _ObjMonde.Add(id, objMonde);
                }
            }
        }
    }
}
