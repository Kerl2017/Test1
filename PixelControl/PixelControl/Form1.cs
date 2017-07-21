using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace PixelControl
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);

        public static Color GetColorAt(int x, int y)
        {
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);
            int a = (int)GetPixel(dc, x, y);
            ReleaseDC(desk, dc);
            return Color.FromArgb(255, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = GetColorAt( 100, 100).ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            b = new Bitmap(pictureBox1.Image);
            b2 = new Bitmap(pictureBox2.Image);

            label3.Text = "Breite: " + pictureBox1.Image.Width;
            label2.Text = "Höhe: " + pictureBox1.Image.Height;
            label6.Text = "Erster Pixel: " + b.GetPixel(1, 1);




            label4.Text = "Breite: " + pictureBox2.Image.Width;
            label5.Text = "Höhe: " + pictureBox2.Image.Height;
            label7.Text = "Erster Pixel: " + b2.GetPixel(1, 1); ;
            
        }

        string pixel;
        string pixel2;
        Bitmap b;
        Bitmap b2;

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            for (int i = 1; i < pictureBox1.Image.Height; i++)
            {
                for (int j = 1; j < pictureBox1.Image.Width; j++)
                {
                    //listBox1.Items.Add(b.GetPixel(i, j));
                    pixel = b.GetPixel(j, i).Name;

                }

            }

            for (int i = 1; i < pictureBox2.Image.Height; i++)
            {
                for (int j = 1; j < pictureBox2.Image.Width; j++)
                {
                    //listBox1.Items.Add(b.GetPixel(i, j));
                    pixel = b2.GetPixel(j, i).Name;

                }

            }

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            if (e.ProgressPercentage == 1)
            {
                listBox1.Items.Add(pixel);
                label8.Text = listBox1.Items.Count.ToString();

                listBox2.Items.Add(pixel);
                label9.Text = listBox2.Items.Count.ToString();
            }

            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            findPicture picfind = new findPicture(pictureBox1.Image);
            Point p = picfind.findPictureCoords(pictureBox2.Image);
            
            label9.Text = p.ToString();
            //Graphics g = new Graphics();
            Bitmap b = new Bitmap(pictureBox1.Image);
            b.SetPixel(p.X, p.Y, Color.Black);
            b.SetPixel(p.X+1, p.Y+1, Color.Black);
            b.SetPixel(p.X + 2, p.Y + 2, Color.Black);
            b.SetPixel(p.X + 3, p.Y + 3, Color.Black);
            b.SetPixel(p.X + 4, p.Y + 4, Color.Black);
            b.SetPixel(p.X + 5, p.Y + 5, Color.Black);
            b.SetPixel(p.X + 6, p.Y + 6, Color.Black);
            //mMouseMove("Form1", p.X, p.Y);
            pictureBox1.Image = b;
        }

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        private const int WM_MOUSEMOVE = 0x0200;

        public int MakeLParam(int LoWord, int HiWord)
        {
            return (int)((HiWord << 16) | (LoWord & 0xFFFF));
        }

        public void mMouseMove(string name, int y, int x)
        {
            IntPtr pl = FindWindowByCaption(IntPtr.Zero, name);

            PostMessage(pl, WM_MOUSEMOVE, 1, MakeLParam(x, y));
        }
    }

    class findPicture
    {
        private Image searchIn;
        private List<List<Color>> list_searchIn;
        private List<List<Color>> list_searchedFor;
        private List<Point> maybe_points = new List<Point>();
        private List<int> hits_list = new List<int>();

        public findPicture(Image searchIn)
        {
            this.searchIn = searchIn;
            list_searchIn = imageToList(searchIn);
        }

        public Point findPictureCoords(Image searchedFor)
        {
            list_searchedFor = imageToList(searchedFor);
            int allPixels = searchedFor.Width * searchedFor.Height;

            if (searchIn.Size.Width > searchedFor.Width && searchIn.Height > searchedFor.Size.Height)
            {
                for (int line = 0; line < searchIn.Height - searchedFor.Height; line++)
                {
                    for (int col = 0; col < searchIn.Width - searchedFor.Width; col++)
                    {
                        if (list_searchIn[col][line] == list_searchedFor[0][0] && list_searchIn[col + searchedFor.Width - 1][line] == list_searchedFor[searchedFor.Width - 1][0])
                        {
                            maybe_points.Add(new Point(col, line));
                            int hits = 0;
                            for (int y = 0; y < searchedFor.Height; y++)
                            {
                                for (int x = 0; x < searchedFor.Width; x++)
                                {
                                    if (list_searchIn[x + col][y + line] == list_searchedFor[x][y])
                                    {
                                        hits++;
                                    }
                                }
                            }
                            hits_list.Add(hits);
                        }
                    }
                }
            }

            int highest = 0;
            int i = 0;
            while (i < hits_list.Count - 1)
            {
                if (hits_list[i] > highest)
                {
                    highest = i;
                }
                i++;
            }

            if (maybe_points[0] != null)
            {
                return maybe_points[highest];
            }
            else
            {
                return new Point(-1, -1);
            }
        }

        public List<List<Color>> imageToList(Image img)
        {
            List<List<Color>> pixelList = new List<List<Color>>();
            Bitmap bmp_pixels = new Bitmap(img);
            for (int col = 0; col < img.Width; col++)
            {
                List<Color> lines = new List<Color>();
                for (int line = 0; line < img.Height; line++)
                {
                    lines.Add(bmp_pixels.GetPixel(col, line));
                }
                pixelList.Add(lines);
            }
            return pixelList;
        }
    }

}
