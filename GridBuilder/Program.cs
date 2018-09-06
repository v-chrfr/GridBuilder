using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace GridBuilder
{
    public class MakeImageGrid
    {
        private const int bit_map_size = 256;
        private const string BingMapsKey = "API_KEY_HERE";
        private const string subdomain = "t0";
        private const string URL = "http://ecn.{0}.tiles.virtualearth.net/tiles/hs0203232101212100{1}{2}?g=6616&key={3}";

        private const int master_size = (bit_map_size * 4) - 2;

        public void Print_Grid(string save_file_name)
        {
            // see https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap?view=netframework-4.7.1#examples
            Bitmap cube_image = new Bitmap(master_size, master_size);
            Bitmap tmp_image;

            for (int face_id = 0; face_id < 2; face_id++)
            {
                for (int tile_id = 0; tile_id < ((face_id == 0) ? 4 : 3); tile_id++)
                {
                    
                    tmp_image = GetCubeSide(face_id, tile_id);
                    UpdateCube(ref cube_image, tmp_image, face_id, tile_id);
                }
            }

            cube_image.Save(save_file_name, ImageFormat.Jpeg);

        }

        private void UpdateCube(ref Bitmap cube_image, Bitmap tmp_image, int face_id, int tile_id)
        {
            int x = 0;
            int y = 0;
            int x_var;
            int y_var;
            Color color;

            int move_len = bit_map_size - 2; // due to 2 pixel overlap

            if (face_id == 0)
            {
                y = move_len;
                x = move_len * (tile_id);
            }
            else if (face_id == 1)
            {
                switch(tile_id)
                {
                    case 1:
                        y = 0;
                        break;
                    case 0:
                        y = move_len;
                        break;
                    case 2:
                        default:
                        y = 2 * move_len;
                        break;
                }
            }
                
            for (int i = 0; i < bit_map_size; i++)
            {
                for (int j = 0; j < bit_map_size; j++)
                {
                    color = tmp_image.GetPixel(i, j);
                    x_var = Math.Min(master_size-1, x + i);
                    y_var = Math.Min(master_size-1, y + j);
                    cube_image.SetPixel(x_var, y_var, color);
                }
            }

            Graphics g = Graphics.FromImage(cube_image);
            using (Font font1 = new Font("Times New Roman", 24, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                PointF pointF1 = new PointF(x, y);
                g.DrawString($"{face_id}-{tile_id}", font1, Brushes.IndianRed, pointF1);
                g.Flush();
            }
        }

        private Bitmap GetCubeSide(int face_id, int tile_id)
        {
            Bitmap bp;
            //Bitmap bp = new BitmapImage();
            using (WebClient webClient = new WebClient())
            {
                Uri fin_url = new Uri(string.Format(URL, subdomain, face_id, tile_id, BingMapsKey));
                Console.WriteLine("http: {0}", fin_url);
                byte[] data = webClient.DownloadData(fin_url);
                using (var stream = new MemoryStream(data))
                {
                    bp = new Bitmap(stream);
                }
            }

            return bp;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var save_file = "test.jpeg";
            MakeImageGrid image = new MakeImageGrid();
            image.Print_Grid(save_file);
        }
    }
    
}
