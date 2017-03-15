using UnityEngine;
using System.Collections;

public class Crosshair : MonoBehaviour
{

    public Texture2D cursorTexture;
    private Vector2 cursorHotspot;

    // initialize mouse with a new texture with the
    // hotspot set to the middle of the texture
    // (don't forget to set the texture in the inspector
    // in the editor)
    void Start()
    {
        cursorHotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }
}