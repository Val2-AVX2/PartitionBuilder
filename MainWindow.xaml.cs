using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace PartitionBuilder
{
    public partial class MainWindow : Window
    {
        private BitmapSource finalImage;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Afficher_Click(object sender, RoutedEventArgs e)
        {
            string input = InputField.Text.Trim();

            if (string.IsNullOrEmpty(input) || !input.Contains(","))
                return;

            string[] notes = input.ToLower().Split(',');

            RequetesList.Items.Add(input);

            finalImage = BuildImage(notes);
            PartitionImage.Source = finalImage;
        }

        private BitmapSource BuildImage(string[] notes)
        {
            var images = new BitmapImage[notes.Length];
            int totalWidth = 0;
            int maxHeight = 0;

            for (int i = 0; i < notes.Length; i++)
            {
                string note = notes[i].Trim();
                if (note == "_") continue;

                string file = FindImageFile(note);
                if (file == null) continue;

                var img = new BitmapImage(new Uri(file));
                images[i] = img;

                totalWidth += img.PixelWidth;
                maxHeight = Math.Max(maxHeight, img.PixelHeight);
            }

            if (totalWidth == 0 || maxHeight == 0)
                return null;

            var drawing = new DrawingVisual();
            using (var context = drawing.RenderOpen())
            {
                int x = 0;
                foreach (var img in images)
                {
                    if (img != null)
                    {
                        context.DrawImage(img, new Rect(x, 0, img.PixelWidth, img.PixelHeight));
                        x += img.PixelWidth;
                    }
                }
            }

            var result = new RenderTargetBitmap(totalWidth, maxHeight, 96, 96, PixelFormats.Pbgra32);
            result.Render(drawing);
            return result;
        }

        private string FindImageFile(string note)
        {
            string folder = Directory.GetCurrentDirectory();
            string target = note + ".jpg";

            foreach (var file in Directory.GetFiles(folder, "*.jpg"))
            {
                if (Path.GetFileName(file).ToLower() == target)
                    return file;
            }

            return null;
        }

        private void Copier_Click(object sender, RoutedEventArgs e)
        {
            if (finalImage != null)
                Clipboard.SetImage(finalImage);
        }

        private void InputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Afficher_Click(null, null);
                e.Handled = true;
            }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Copier_Click(null, null);
                e.Handled = true;
            }
        }
    }
}
