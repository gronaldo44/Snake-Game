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
    // TODO: private IImage powerupsImg;
    #endregion
    #region Drawing Properties
    // TODO: private World theWorld
    private bool initializedForDrawing = false;
    private delegate void ObjectDrawer(object o, ICanvas canvas);
    // TODO: private int mapSize;

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
    /// Construct a world pannel 
    /// </summary>
    public WorldPanel()
    {
    }

    /// <summary>
    /// Loads all images required for Draw method 
    /// and initializes the world for drawing.
    /// </summary>
    private void InitializeDrawing()
    {
        // TODO: Set images of all objects
        wallImg = loadImage("WallSprite.png");
        backgroundImg = loadImage("Background.png");

        // TODO: Draw Walls and Background
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
        throw new NotImplementedException();
        // Images must be loaded before they can be drawn
        //if (!initializedForDrawing)
        //{
        //    InitializeDrawing();
        //}

        // TODO: undo any leftover transformations from last frame
        //canvas.ResetState();

        // TODO: center the view on the player

        // TODO: draw the objects in the world
    }

    private void DrawInitialWorld()
    {
        // TODO: draw the background
        // TODO: draw the walls
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
        // TODO: Calculate the Wall's position
        // TODO: Draw the wall
    }

    private void PowerupDrawer(object o, ICanvas canvas)
    {
        throw new NotImplementedException();
        // TODO: Calculate the Powerup's position
        // TODO: Draw the Powerup
    }
    #endregion
}
