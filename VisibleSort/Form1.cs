using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Threading;

namespace VisibleSort
{
    public partial class Form1 : Form
    {
        int[] array;
        Rectangle[] panels;
        Brush[] digitBrushes;
        int vPadding = 50;
        int hPadding = 100;
        Action selectedSort;
        Thread threadSort;
        DoubleBuffer buffer;

        public Form1()
        {
            InitializeComponent();
        }

        private void PaintFrame(Graphics g)
        {
            Pen pen = new Pen(Color.Black);
            Brush brush = new SolidBrush(panel1.BackColor);

            g.Clear(this.BackColor);
            foreach (Rectangle r in panels)
            {
                g.DrawRectangle(new Pen(Brushes.Black), r);
            }
        }

        private void PaintArray()
        {
            try
            {
                Graphics g = buffer.Background;
                Graphics moto = panel1.CreateGraphics();
                Pen pen = new Pen(Color.BlueViolet);

                PaintFrame(g);

                for (int i = 0; i < 10; i++)
                {
                    g.DrawString(array[i].ToString(), new Font("나눔고딕", 30), digitBrushes[i], panels[i]);
                }

                moto.DrawImage(buffer.Image, 0, 0);
                moto.Dispose();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                MessageBox.Show(exc.Source);
                Console.WriteLine(exc.StackTrace);
                threadSort.Abort();
            }
        }

        private void BubbleSort()
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                for (int j = 0; j < i; j++)
                {
                    SetDigitBrushes(Brushes.Black, 0, i);
                    SetDigitBrushes(Brushes.Red, new int[] { j, j + 1 });
                    PaintArray();
                    Thread.Sleep(250);
                    if (array[j] > array[j + 1])
                    {
                        int tmp = array[j];
                        array[j] = array[j + 1];
                        array[j + 1] = tmp;
                    }
                    SetDigitBrushes(Brushes.HotPink, new int[] { j, j + 1 });
                    PaintArray();
                    Thread.Sleep(250);
                }
                SetDigitBrushes(Brushes.Blue, i);
                PaintArray();
            }
            SetDigitBrushes(Brushes.Blue);
            PaintArray();
        }

        private void SelectionSort()
        {
            int minIdx = 0;
            int j;
            bool isChanged;

            for (int i = 0; i < 9; i++)
            {
                minIdx = i;
                for (j = i + 1; j < 10; j++)
                {
                    isChanged = false;
                    SetDigitBrushes(Brushes.Red, new int[] { minIdx });
                    PaintArray();
                    if (array[j] < array[minIdx])
                    {
                        minIdx = j;
                        isChanged = true;
                    }
                    SetDigitBrushes(Brushes.Black, i + 1, 9);
                    SetDigitBrushes(Brushes.Gold, new int[] { minIdx });
                    if (!isChanged || i != 0)
                        SetDigitBrushes(Brushes.Pink, new int[] { j });
                    PaintArray();
                    Thread.Sleep(500);
                }

                int tmp = array[minIdx];
                array[minIdx] = array[i];
                array[i] = tmp;

                SetDigitBrushes(Brushes.Blue, 0, i);
                PaintArray();
            }


            SetDigitBrushes(Brushes.Blue);
            PaintArray();
        }

        private void MakeArray()
        {
            Random r = new Random();
            array = new int[10];
            bool[] check = new bool[10];

            for (int i = 0; i < 10; i++)
            {
                check[i] = false;
            }

            int j = 0;

            while (j < 10)
            {
                int tmp = r.Next(10);
                if (check[tmp] == false)
                {
                    check[tmp] = true;
                    array[j++] = tmp;
                }
            }

            string numbers = "초기 배열 : ";

            for (j = 0; j < 10; j++)
            {
                numbers += array[j];
                if (j != 9)
                {
                    numbers += ", ";
                }
            }

            textBox1.Text = numbers;
        }

        private void SetDigitBrushes(Brush brush, int[] arr)
        {
            foreach (int i in arr)
            {
                digitBrushes[i] = brush;
            }
        }

        private void SetDigitBrushes(Brush brush, int start = 0, int end = 9)
        {
            if (end >= start && start > -1 && end < 10)
            {
                int[] arr = new int[end - start + 1];
                for (int i = start; i <= end; i++)
                {
                    arr[i - start] = i;
                }

                SetDigitBrushes(brush, arr);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            buffer = DoubleBuffer.getInstance(panel1.Width, panel1.Height);
            digitBrushes = new Brush[10];

            for (int i = 0; i < 10; i++)
            {
                digitBrushes[i] = Brushes.Black;
            }

                panels = new Rectangle[10];
            int partWidth = (panel1.Width - (2 * hPadding)) / 10;

            for (int i = 0; i < 10; i++)
            {
                panels[i] = new Rectangle(hPadding + i * partWidth, vPadding, partWidth, panel1.Height - (2 * vPadding));
            }

            listBox1.SelectedIndex = 0;
            selectedSort = BubbleSort;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            ThreadStart ts = new ThreadStart(selectedSort);

            if (threadSort == null || threadSort.ThreadState == ThreadState.Stopped || threadSort.ThreadState == ThreadState.Aborted)
            {
                MakeArray();
                threadSort = new Thread(ts);
                threadSort.Start();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (threadSort != null)
            {
                threadSort.Abort();
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            lblSortName.Text = listBox1.Text;
            switch (listBox1.SelectedIndex)
            {
                case 0:
                    selectedSort = BubbleSort;
                    break;
                case 1:
                    selectedSort = SelectionSort;
                    break;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (threadSort != null)
                threadSort.Abort();
        }
    }

    public class DoubleBuffer
    {
        private static DoubleBuffer instance = null;
        private Graphics background;
        private Bitmap bitoffScreen;

        private int width;
        private int height;

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public Graphics Background { get { return background; } }
        public Bitmap Image { get { return bitoffScreen; } }

        private DoubleBuffer(int width, int height)
        {
            this.width = width;
            this.height = height;

            bitoffScreen = new Bitmap(this.width, this.height);
            background = Graphics.FromImage(bitoffScreen);
        }

        public static DoubleBuffer getInstance(int width, int height)
        {
            if (instance == null)
            {
                return (instance = new DoubleBuffer(width, height));
            }
            return instance;
        }

        public static DoubleBuffer getInstance()
        {
            if (instance != null)
                return instance;
            else
                throw new NullReferenceException();
        }
    }
}