using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using IImage = Microsoft.Maui.Graphics.IImage;
#if MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#else
using Microsoft.Maui.Graphics.Win2D;
#endif
using Color = Microsoft.Maui.Graphics.Color;
using System.Reflection;
using Microsoft.Maui;
using System.Net;
using Font = Microsoft.Maui.Graphics.Font;
using SizeF = Microsoft.Maui.Graphics.SizeF;
using System;

namespace SnakeGame;
public class WorldPanel : IDrawable
{
    #region Images
    private IImage wallImg;
    private IImage backgroundImg;
    //private IImage powerupsImg;

    /// <summary>
    /// Searches for the argued image name within this programs resources folder. Only a filename is required as 
    /// this method will take care of the path that leads to it.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeGame.Resources.Images";
        var service = new W2DImageLoadingService();
        return service.FromStream(assembly.GetManifestResourceStream($"{path}.{name}"));
    }

    #endregion
    private bool initializedForDrawing = false;
    private delegate void ObjectDrawer(object o, ICanvas canvas);

    #region OS Compatibility
#if MACCATALYST
    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeGame.Resources.Images";
        return PlatformImage.FromStream(assembly.GetManifestResourceStream($"{path}.{name}"));
    }
#else


#endif
    #endregion

    #region Initialization

    /// <summary>
    /// Constructs a world panel with the argued player to be centered
    /// </summary>
    /// <param name="playerId">Snake of the Player</param>
    public WorldPanel()
    {
    }

    /// <summary>
    /// Loads all images required for Draw method 
    /// and initializes the world for drawing.
    /// </summary>
    private void InitializeDrawing()
    {
        // Set images of all objects
        wallImg = loadImage("WallSprite.png");
        backgroundImg = loadImage("Background.png");
        //powerupsImg = loadImage("PowerUpFood.png");

        // Drawing has been initialized
        initializedForDrawing = true;
    }
    #endregion

    #region Drawing

    /// <summary>
    /// This runs whenever the drawing panel is invalidated and draws the game
    /// 
    /// The drawing panel should be invalidated on each frame.
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="dirtyRect"></param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Images must be loaded before they can be drawn
        if (!initializedForDrawing)
        {
            InitializeDrawing();
        }

        // undo any leftover transformations from last frame
        canvas.ResetState();

        // center the view on the player
        GameController.CalculateScreenLoc(out float x, out float y);
        canvas.Translate(x, y);

        // draw the objects in the world
        foreach (var snake in World.snakes.Values)
        {   // Draw the snakes
            DrawObjectWithTransform(canvas, snake, snake.body.Last().GetX(), snake.body.Last().GetY(),
                snake.body.Last().ToAngle(), SnakeDrawer);
        }
        foreach (var powerup in World.powerups.Values)
        {
            DrawObjectWithTransform(canvas, powerup, powerup.loc.GetX(), powerup.loc.GetY(),
                powerup.loc.ToAngle(), PowerupDrawer);
        }

        // This should only happen once
        foreach (var wall in World.walls.Values)
        {
            DrawObjectWithTransform(canvas, wall, wall.p1.GetX(), wall.p1.GetY(),
                wall.p1.ToAngle(), WallDrawer);
        }
        DrawObjectWithTransform(canvas, backgroundImg, -1000, -1000, 0, BackgroundDrawer);
    }

    /// <summary>
    /// This method performs a translation and rotation to draw an object.
    /// </summary>
    /// <param name="canvas">The canvas object for drawing onto</param>
    /// <param name="o">The object to draw</param>
    /// <param name="worldX">The X component of the object's position in world space</param>
    /// <param name="worldY">The Y component of the object's position in world space</param>
    /// <param name="angle">The orientation of the object, measured in degrees clockwise from "up"</param>
    /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
    private void DrawObjectWithTransform(ICanvas canvas, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
    {
        // "push" the current transform
        canvas.SaveState();

        canvas.Translate((float)worldX, (float)worldY);
        canvas.Rotate((float)angle);
        drawer(o, canvas);

        // "pop" the transform
        canvas.RestoreState();
    }

    /// <summary>
    /// A method that can be used as an ObjectDrawer delegate 
    /// for drawing snakes
    /// </summary>
    /// <param name="o">The snake to draw</param>
    /// <param name="canvas"></param>
    private void SnakeDrawer(object o, ICanvas canvas)
    {
        // TODO: Calculate the Snake's position
        // TODO: Calculate the snake's color

        // TODO: Draw the snake one line-by-line
    }

    /// <summary>
    /// A method that can be used as an ObjectDrawer delegate
    /// </summary>
    /// <param name="o"></param>
    /// <param name="canvas"></param>
    private void WallDrawer(object o, ICanvas canvas)
    {
        bool isForward;     // What direction the wall is being drawn in
        bool isVertical;  // What orientation the wall is being drawn in
        Wall w = o as Wall;
        int numOfWallSprites;
        double posX1 = w.p1.GetX(), posY1 = w.p1.GetY();
        double posX2 = w.p2.GetX(), posY2 = w.p2.GetY();

        // Calculate the Wall's orientation
        isVertical = posX1 == posX2;
        if (isVertical)
        {   // Vertical wall
            numOfWallSprites = (int)(posY1 - posY2) / 50;
        }
        else
        {   // Horizontal wall
            numOfWallSprites = (int)(posX1 - posX2) / 50;
        }
        // Calculate direction
        isForward = numOfWallSprites < 0;
        numOfWallSprites = Math.Abs(numOfWallSprites);

        // Draw each wall sprite
        for (int i = 0; i < numOfWallSprites; i++)
        {
            float x = 0, y = 0;     // tmp value
            // Calculate top-left of this sprite
            if (isForward)
            {
                x = (float)w.p1.GetX() + (isVertical ? 0 : (i * 50));
                y = (float)w.p1.GetY() + (isVertical ? (i * 50) : 0);
            }
            else
            {
                x = (float)w.p1.GetX() - (isVertical ? 0 : (i * 50));
                y = (float)w.p1.GetY() - (isVertical ? (i * 50) : 0);
            }
            canvas.DrawImage(wallImg, x, y, wallImg.Width, wallImg.Height);
        }
    }

    private void BackgroundDrawer(object o, ICanvas canvas)
    {
        // Calculate top-left of image
        float x = -(World.worldSize / 2);
        float y = -(World.worldSize / 2);
        // Draw the background
        canvas.DrawImage(backgroundImg, x, y, backgroundImg.Width, backgroundImg.Height);
    }

    private void PowerupDrawer(object o, ICanvas canvas)
    {
        throw new NotImplementedException();
        // TODO: Calculate the Powerup's position
        // TODO: Draw the Powerup
    }
    #endregion


}
