using TMProOld;
using UnityEngine;
using UnityEngine.UI;

namespace FsmTool;

public class StateTitle
{
    GameObject baseObject;
    private TMProOld.TextMeshPro superText;

    public StateTitle()
    {
        baseObject = new GameObject("State Title");
        {
            var largeTextObject = new GameObject("Super");
            var transform = largeTextObject.AddComponent<RectTransform>();
            transform.anchoredPosition = new Vector2(0f, 1.09f);
            transform.anchorMax = new Vector2(0.5f,0.5f);
            transform.anchorMin = new Vector2(0.5f,0.5f);
            transform.sizeDelta = new Vector2(19f, 4f);
            var meshRenderer = largeTextObject.AddComponent<MeshRenderer>();
            var meshFilter = largeTextObject.AddComponent<MeshFilter>();
            var container = largeTextObject.AddComponent<TextContainer>();
            container.anchorPosition = TextContainerAnchors.Middle;
            container.pivot = new Vector2(0.5f, 0.5f);
            superText = largeTextObject.AddComponent<TextMeshPro>();
            superText.font = GameResources.TrajanProBold;
            superText.fontSize = 42;
            superText.alignment = TextAlignmentOptions.Center;
            superText.fontMaterial = GameResources.TrajanProBoldMaterial;
            superText.fontStyle = FontStyles.SmallCaps;
        }
    }

}