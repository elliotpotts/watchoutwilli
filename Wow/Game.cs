using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Wow
{
    public enum Tile
    {
        Dirt = 0,
        Rock,
        Food,
        Wall
    }

    /// <summary>
    /// Models a Watch Out Willi! game.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Width of map in tiles
        /// </summary>
        public const int MapWidth = 20;

        /// <summary>
        /// Height of map in tiles
        /// </summary>
        public const int MapHeight = 15;

        /// <summary>
        /// Map tiles
        /// </summary>
        public Tile?[,] Map { get; private set; } = new Tile?[MapWidth, MapHeight];
              
        /// <summary>
        /// x coordinate of Willi on the map
        /// </summary>
        public int WilliX { get; private set; } = -1;

        /// <summary>
        /// y coordinate of Willi on the map
        /// </summary>
        public int WilliY { get; private set; } = -1;

        /// <summary>
        /// Has Willi been crushed by a rock yet?
        /// </summary>
        public bool WilliDead { get; private set; } = false;
        
        /// <summary>
        /// Create a new game by reading a map from a text file
        /// </summary>
        /// <param name="filename">The filepath of the map file</param>
        public Game(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    for (int x = 0; x < MapWidth; x++)
                    {
                        char TileChar = Convert.ToChar(reader.Read());
                        if (TileChar == '\r') TileChar = Convert.ToChar(reader.Read());
                        if (TileChar == '\n') TileChar = Convert.ToChar(reader.Read());
                        switch (TileChar)
                        {
                            case 'd': Map[x, y] = Tile.Dirt; break;
                            case 'r': Map[x, y] = Tile.Rock; break;
                            case 'f': Map[x, y] = Tile.Food; break;
                            case 'w': Map[x, y] = Tile.Wall; break;
                            case ' ': Map[x, y] = null; break;
                            case 's':
                                Map[x, y] = null;
                                WilliX = x;
                                WilliY = y;
                                break;
                            default:
                                throw new InvalidDataException("Unrecognised character '" + TileChar + "' in map.");
                        }
                    }
                }
                if (WilliX == -1)
                {
                    throw new InvalidDataException("No Willi starting position given. Use 's'.");
                }                
            }
        }

        /// <summary>
        /// If a tile is solid, the player cannot walk through it
        /// </summary>
        /// <param name="x">x coordinate to query</param>
        /// <param name="y">y coordinate to query</param>
        /// <returns>whether the player can step into this location</returns>
        private bool IsSolid(int x, int y)
        { 
            return x < 0 || y < 0
                || x >= MapWidth || y >= MapHeight
                || Map[x, y] == Tile.Rock
                || Map[x, y] == Tile.Wall;               
        }

        /// <summary>
        /// If a tile is smooth, a rock can fall through it
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsEmpty(int x, int y)
        {
            return Map[x, y] == null && (WilliX != x || WilliY != y);
        }

        private void FallRock(int x, int y)
        {
            int drop = 0;
            while (y != MapHeight && IsEmpty(x, y + drop + 1))
            {
                drop++;
            }
            Map[x, y] = null; // Remove the rock from it's current location
            Map[x, y + drop] = Tile.Rock; // Place it as far down as it can go
            if(drop >= 1 && WilliY == y + drop && WilliX == x)
            {
                WilliDead = true;
            }
        }
        
        private void FallColumn(int x)
        {
            for (int y = MapHeight - 1; y >= 0; y--)
            {
                if (Map[x, y] == Tile.Rock) FallRock(x, y);
            }
        }

        /// <summary>
        /// Determines whether the player has won the game
        /// </summary>
        /// <returns>True if the player has won the game</returns>
        public bool GameWon()
        {
            for(int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    if (Map[x, y] == Tile.Food) return false;
                }
            }
            return true; 
        }

        /// <summary>
        /// Tries to step into the given coordinates, swallowing food, 
        /// removing dirt and updating the game state.
        /// </summary>
        /// <param name="x">The x coordinate to step into</param>
        /// <param name="y">The y coordinate to step into</param>
        private void TryStep(int x, int y)
        {
            if (IsSolid(x, y))
            {
                return;
            }
            int oldx = WilliX;
            WilliX = x;
            WilliY = y;
            Map[x, y] = null; // This turns any food/dirt into an empty space - i.e. he has eaten it 
            FallColumn(oldx);
        }

        public void WalkUp()    { TryStep(WilliX, WilliY - 1); }
        public void WalkRight() { TryStep(WilliX + 1, WilliY); }
        public void WalkDown()  { TryStep(WilliX, WilliY + 1); }
        public void WalkLeft()  { TryStep(WilliX - 1, WilliY); }
    }
}
