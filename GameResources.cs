using TMPro;
using UnityEngine;

namespace FsmTool;

public static class GameResources
{
    public static TMProOld.TMP_FontAsset? TrajanProBold { get; private set; }
    public static Material? TrajanProBoldMaterial { get; private set; }
    public static TMProOld.TMP_FontAsset? TrajanProRegular { get; private set; }
    
    static GameResources()
    {
        ReloadResources();
    }

    static void ReloadResources()
    {
        foreach (var font in Resources.FindObjectsOfTypeAll<TMProOld.TMP_FontAsset>())
        {
            if (font != null) switch (font.name)
            {
                case "trajan_bold_tmpro":
                    TrajanProBold = font;
                    break;
            }
        }
        
        foreach (var material in Resources.FindObjectsOfTypeAll<Material>())
        {
            if (material != null) switch (material.name)
            {
                case "trajan_bold_tmpro Material (Instance)":
                    TrajanProBoldMaterial = material;
                    break;
            }
        }
    }

}