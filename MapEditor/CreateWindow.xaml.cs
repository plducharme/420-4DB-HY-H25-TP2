using HugoLandEditeur;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapEditor
{
    /// <summary>
    /// Interaction logic for CreateWindow.xaml
    /// </summary>
    public partial class CreateWindow : Window
    {
        public CreateWindow()
        {
            InitializeComponent();
            txtWorldWidth.Text = csteApplication.DEFAULT_WORLD_WIDTH.ToString();
            txtWorldHeight.Text = csteApplication.DEFAULT_WORLD_HEIGHT.ToString();
        }
        public int CellHeight { set; get; }
        public int CellWidth { set; get; }
        public int WorldWidth { set; get; }
        public int WorldHeight { set; get; }
        public string TileSetFilePath { set; get; }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            CellHeight = csteApplication.TILE_WIDTH_IN_IMAGE;
            CellWidth = csteApplication.TILE_HEIGHT_IN_IMAGE;
            WorldHeight = int.Parse(txtWorldHeight.Text);
            WorldWidth = int.Parse(txtWorldWidth.Text);

            TileSetFilePath = System.AppDomain.CurrentDomain.BaseDirectory + csteApplication.TILE_FILE_PATH;
            if (File.Exists(TileSetFilePath))
                DialogResult = true;
            else
                MessageBox.Show("Please enter a valid file path", "Invalid", MessageBoxButton.OK, MessageBoxImage.Error);
        }

     
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValid(((TextBox)sender).Text + e.Text);
        }

        public static bool IsValid(string str)
        {
            int i;
            return int.TryParse(str, out i) && i >= 1 && i <= csteApplication.MAP_MAX_SIZE;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
