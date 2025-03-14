using HugoLandEditeur;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace MapEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    struct POSITION
    {
        public int x, y;
        public POSITION(int x, int y) { this.x = x; this.y = y; }
    }

    public partial class MainWindow : Window
    {
       private Image selectedImage;//Tile 
        private string openingFilePath = null;
        private int selectedImageId;
        private List<Image> tileSet;
        private int[,] tileMap;
        CTileLibrary m_TileLibrary = null;

        /// <summary>
        /// ctor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            m_TileLibrary = new CTileLibrary();
        }

        #region ZOOM_MANAGEMENT
        /// <summary>
        /// Increase the zoom.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemZoomIn_Click(object sender, RoutedEventArgs e)
        {
            sourceGridScaleTransform.ScaleX *= 1.1;
            sourceGridScaleTransform.ScaleY *= 1.1;
        }

        /// <summary>
        /// Decrease the zoom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemZoomOut_Click(object sender, RoutedEventArgs e)
        {
            sourceGridScaleTransform.ScaleX /= 1.1;
            sourceGridScaleTransform.ScaleY /= 1.1;
        }
        /// <summary>
        /// Reset the Zoom to 100%
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemZoomReset_Click(object sender, RoutedEventArgs e)
        {
            sourceGridScaleTransform.ScaleX = 1;
            sourceGridScaleTransform.ScaleY = 1;
        }
        #endregion


        private void MakeGridMap(int m, int n, int width, int height)
        {
            ColumnDefinition[] columns = new ColumnDefinition[n];
            RowDefinition[] rows = new RowDefinition[m];
            GridTileMap.ShowGridLines = false;
           
            GridTileMap.ColumnDefinitions.Clear();
            GridTileMap.RowDefinitions.Clear();
            GridTileMap.Children.Clear();

            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new ColumnDefinition();
                columns[i].Width = new GridLength(width);
                GridTileMap.ColumnDefinitions.Add(columns[i]);
            }

            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = new RowDefinition();
                rows[i].Height = new GridLength(height);
                GridTileMap.RowDefinitions.Add(rows[i]);
            }

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Image image = new Image();
                    var value = tileMap[i, j];
                    if (value != -1)
                        image.Source = tileSet[value].Source;
                    else
                        image.Source = null;
                    var button = new Button();
                    button.Template = FindResource("TileButton0") as ControlTemplate;

                    button.MouseEnter += Button_MouseEnter;
                    button.MouseLeave += Button_MouseLeave;
                    button.Click += Button_Click1;

                    button.Width = width;
                    button.Height = height;
                    button.Content = image;
                    button.BorderThickness = new Thickness(0);
                    button.Padding = new Thickness(0.1);
                    button.Name = "TileMapButton_" + i + "_" + j;
                    button.Tag = new POSITION(i, j);

                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    GridTileMap.Children.Add(button);
                }
            }
        }

        /// <summary>
        /// Reset the status bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextStatus.Text = null;
        }

        /// <summary>
        /// On mouse over of the tiles, add information in the status bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            int x = ((POSITION)(sender as Button).Tag).x;
            int y = ((POSITION)(sender as Button).Tag).y;
            TextStatus.Text = "(" + x + "," + y + ") = " + tileMap[x, y];
        }

        /// <summary>
        /// Load m
        /// </summary>
        /// <param name="pathToTileMapFile"></param>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        //public int[,] LoadTileMap(string pathToTileMapFile, out int m, out int n)
        //{

        //    string[] lines = File.ReadAllLines(pathToTileMapFile);

        //    string[] tmp = lines[0].Split(' ');
        //    int tileSetCount = int.Parse(tmp[0]);
        //    m = int.Parse(tmp[1]);
        //    n = int.Parse(tmp[2]);
        //    int[,] result = new int[m, n];
        //    string[] tmp2 = new string[m];

        //    for (int i = 1; i < m + 1; i++)
        //    {
        //        tmp2 = lines[i].Split(' ');
        //        for (int j = 0; j < n; j++)
        //        {
        //            result[i - 1, j] = int.Parse(tmp2[j]);
        //        }
        //    }
        //    return result;
        //}

        public void SaveTileMap(string pathToTileMapFile, int[,] tileMap, int count)
        {
            string[] lines = new string[tileMap.Length + 1];

            int m = tileMap.GetLength(0);
            int n = tileMap.GetLength(1);

            lines[0] = count.ToString() + ' ' + m + ' ' + n;

            string tmp;
            for (int i = 1; i < m + 1; i++)
            {
                tmp = "";
                for (int j = 0; j < n; j++)
                {
                    tmp += tileMap[i - 1, j].ToString() + ' ';
                }
                lines[i] = tmp;
            }
        
            File.WriteAllLines(pathToTileMapFile, lines);
        }

        public List<Image> LoadTileSet(string tileSetPath, int width, int height)
        {
            WrapPanelTileSet.Children.Clear();

            ImagePreview.Source = null;
            List<Image> m_tileSet = new List<Image>();

            Bitmap inputBitmap = new Bitmap(tileSetPath);
            int bitmapWidth = inputBitmap.Width;
            int bitmapHeight = inputBitmap.Height;

            List<Bitmap> tiles = new List<Bitmap>();
            int m, n;
            if (width <= 0 && height <= 0)
                return null;
            n = bitmapWidth / width;
            m = bitmapHeight / height;
            int count = 0;
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {

                    Bitmap tmp = new Bitmap(width, height);
                    Rectangle r = new Rectangle(j * width, i * height, width, height);
                    tmp = inputBitmap.Clone(r, inputBitmap.PixelFormat);
                    var image = new Image();
                    image.Source = BitmapToImageSource(tmp);
                    m_tileSet.Add(image);
                    var button = new Button();
                    button.MouseEnter += TileSet_Button_MouseEnter;
                    button.MouseLeave += TileSet_Button_MouseLeave;
                    button.Width = width;
                    button.Height = height;
                    button.Content = image;
                    WrapPanelTileSet.Children.Add(button);
                    count++;
                    button.Name = "TileButton" + count;
                    button.Click += Button_Click;
                    button.Tag = count;
                }
            }
            selectedImage = new Image();
            selectedImage.Source = m_tileSet[0].Source;
            selectedImageId = 0;
            return m_tileSet;
        }

        private void TileSet_Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextStatus.Text = null;
        }

        private void TileSet_Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            int x = (int)(sender as Button).Tag - 1;
            TextStatus.Text = "Tile " + x;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Select " + ((Button)sender).Name);

            selectedImage = new Image();
            int id = (int)((Button)sender).Tag - 1;
            selectedImage.Source = tileSet[id].Source;
            selectedImageId = id;
            ImagePreview.Source = selectedImage.Source;
        }


        private void Button_Click1(object sender, RoutedEventArgs e)
        {

            Console.WriteLine("Click " + ((Button)sender).Name);
            if (selectedImage != null)
            {

                ((Image)((Button)sender).Content).Source = selectedImage.Source;
                POSITION p = (POSITION)((Button)sender).Tag;

                tileMap[p.x, p.y] = selectedImageId;
                Console.WriteLine("Change to " + selectedImageId);
            }

        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void MenuItem_LoadTileMap_Click(object sender, RoutedEventArgs e)
        {
            LoadWindow wd = new LoadWindow();
            wd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (wd.ShowDialog() == true)
            {
                //Chargement du monde
             
            }
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (this.WindowState == WindowState.Maximized)
            {
                // if maximised, set window border to window resize border + fixed frame border so that the window content is not chopped off
                this.BorderThickness = new Thickness(SystemParameters.WindowResizeBorderThickness.Left + SystemParameters.FixedFrameVerticalBorderWidth,
                    SystemParameters.WindowResizeBorderThickness.Top + SystemParameters.FixedFrameHorizontalBorderHeight,
                    SystemParameters.WindowResizeBorderThickness.Right + SystemParameters.FixedFrameVerticalBorderWidth,
                    SystemParameters.WindowResizeBorderThickness.Bottom + SystemParameters.FixedFrameHorizontalBorderHeight);
            }
            else
            {
                this.BorderThickness = new Thickness();
            }
        }

        /// <summary>
        /// Save the world
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            if (tileMap == null)
                return;
            if (openingFilePath == null || openingFilePath == "")
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text File|*.txt";
                saveFileDialog.AddExtension = true;
                if (saveFileDialog.ShowDialog() == true)
                {
                    openingFilePath = saveFileDialog.FileName;
                }
                else
                    return;
            }

            SaveTileMap(openingFilePath, tileMap, tileSet.Count);
            TextStatus.Text = "Saved";
        }

        /// <summary>
        /// Create a New World
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemNew_Click(object sender, RoutedEventArgs e)
        {
            CreateWindow wd = new CreateWindow();
            wd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (wd.ShowDialog() == true)
            {
                openingFilePath = null;
                //Load tileset
                tileSet = LoadTileSet(wd.TileSetFilePath, wd.CellWidth, wd.CellHeight);

                tileMap = MakeBlankTileMap(wd.WorldWidth, wd.WorldHeight);
              

                MakeGridMap(wd.WorldWidth, wd.WorldHeight, wd.CellWidth, wd.CellHeight);

            }
        }

        /// <summary>
        /// Initialize all tiles of the new world to the DEFAULT_TILE.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private int[,] MakeBlankTileMap(int m, int n)
        {
            int[,] result = new int[m, n];
        
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result[i, j] = csteApplication.DEFAULT_TILE;
                }
            }
            return result;
        }

        /// <summary>
        /// Close the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        /// <summary>
        /// Assign the default tile to erase
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemErase_Click(object sender, RoutedEventArgs e)
        {
            if (selectedImage != null)
            {
                int id = csteApplication.DEFAULT_TILE;
                selectedImage.Source = tileSet[id].Source;
                selectedImageId = id;
                ImagePreview.Source = selectedImage.Source;
            }

        }


        //private int[,] MakeTileSet(string inputImagePath, string outputImagePath, int width, int height)
        //{

        //    Bitmap inputBitmap = new Bitmap(inputImagePath);
        //    int bitmapWidth = inputBitmap.Width;
        //    int bitmapHeight = inputBitmap.Height;

        //    List<Bitmap> tiles = new List<Bitmap>();
        //    int m, n;
        //    n = bitmapWidth / width;
        //    m = bitmapHeight / height;
        //    int[,] s = new int[m, n];
        //    bool available;
        //    int last = 0;
        //    for (int i = 0; i < m; i++)
        //    {
        //        for (int j = 0; j < n; j++)
        //        {
        //            if (i == 3) System.Console.Out.Write(true);
        //            Bitmap tmp = new Bitmap(width, height);
        //            Rectangle r = new Rectangle(j * width, i * height, width, height);
        //            tmp = inputBitmap.Clone(r, PixelFormat.Format32bppArgb);
        //            available = false;
        //            for (int k = 0; k < tiles.Count; k++)
        //                if (CompareBitmapsFast(tiles[k], tmp))
        //                {
        //                    s[i, j] = k;
        //                    available = true;
        //                    break;
        //                }
        //            if (!available)
        //            {
        //                s[i, j] = last++;
        //                tiles.Add(tmp);
        //            }
        //        }
        //    }

        //    Bitmap b = new Bitmap(width * tiles.Count, height);
        //    for (int i = 0; i < tiles.Count; i++)
        //    {
        //        using (Graphics g = Graphics.FromImage(b))
        //        {
        //            g.DrawImage(tiles[i], width * i, 0);
        //        }
        //    }

        //    b.Save(outputImagePath);
        //    return s;
        //}
        
        //Source: http://csharpexamples.com/c-fast-bitmap-compare/
    //    public static bool CompareBitmapsFast(Bitmap bmp1, Bitmap bmp2)
    //    {
    //        if (bmp1 == null || bmp2 == null)
    //            return false;
    //        if (object.Equals(bmp1, bmp2))
    //            return true;
    //        if (!bmp1.Size.Equals(bmp2.Size) || !bmp1.PixelFormat.Equals(bmp2.PixelFormat))
    //            return false;

    //        int bytes = bmp1.Width * bmp1.Height * (System.Drawing.Image.GetPixelFormatSize(bmp1.PixelFormat) / 8);

    //        bool result = true;
    //        byte[] b1bytes = new byte[bytes];
    //        byte[] b2bytes = new byte[bytes];

    //        BitmapData bitmapData1 = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height), ImageLockMode.ReadOnly, bmp1.PixelFormat);
    //        BitmapData bitmapData2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, bmp2.PixelFormat);

    //        Marshal.Copy(bitmapData1.Scan0, b1bytes, 0, bytes);
    //        Marshal.Copy(bitmapData2.Scan0, b2bytes, 0, bytes);

    //        for (int n = 0; n <= bytes - 1; n++)
    //        {
    //            if (b1bytes[n] != b2bytes[n])
    //            {
    //                result = false;
    //                break;
    //            }
    //        }

    //        bmp1.UnlockBits(bitmapData1);
    //        bmp2.UnlockBits(bitmapData2);

    //        return result;
    //    }
    }
}
