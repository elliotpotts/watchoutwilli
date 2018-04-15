using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wow
{
    public partial class View : Form
    {
        private const int SrcTileSize = 10;
        private const int TileSize = SrcTileSize * 4;

        private List<Bitmap> Tilesets;
        private int TilesetIndex = 0;
        private Bitmap Willi;
        public Game Game;

        public View(Game g)
        {
            this.Game = g;
            DoubleBuffered = true;
            ClientSize = new Size(800, 600);
            Tilesets = Directory.EnumerateFiles("Media/Tilesets")
                .Select(f => new Bitmap(f)).ToList();
            Willi = new Bitmap("Media/Willi.png");
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            for(int x = 0; x < Game.MapWidth; x++)
            {
                for (int y = 0; y < Game.MapHeight; y++)
                {
                    Tile? t = this.Game.Map[x, y];
                    if (t.HasValue)
                    {
                        e.Graphics.DrawImage(
                            Tilesets[TilesetIndex],                            
                            new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize), //dst
                            new Rectangle((int)t * SrcTileSize, 0, SrcTileSize, SrcTileSize), //src
                            GraphicsUnit.Pixel
                        );
                    }
                }
            }
            e.Graphics.DrawImage(
                Willi, 
                new Rectangle(Game.WilliX * TileSize, Game.WilliY * TileSize, TileSize, TileSize)
            );
        }

        private void View_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case 'w': Game.WalkUp(); break;
                case 'a': Game.WalkLeft(); break;
                case 's': Game.WalkDown(); break;
                case 'd': Game.WalkRight(); break;
                case 'g': TilesetIndex++; TilesetIndex %= Tilesets.Count; break;
            }            
            Invalidate();
            if (Game.WilliDead) MessageBox.Show("Willi is dead :( ");
            if (Game.GameFinished()) MessageBox.Show("Congrats!");
        }
    }
}
