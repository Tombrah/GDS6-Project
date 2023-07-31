using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class ColourGradient
{
    public Color32 topLeftCorner = new Color32(99, 27, 82, 255);
    public Color32 topRightCorner = new Color32(190, 56, 159, 255);
    public Color32 bottomLeftCorner = new Color32(190, 56, 159, 255);
    public Color32 bottomRightCorner = new Color32(0, 255, 138, 255);
    public int listID = 0;
    public string stringToAffect;

    private int startAt;
    private int endAt;

    public void CheckText(TMP_Text textComponent)
    {
        string mainText = textComponent.text;
        string[] separator = { stringToAffect };

        if (mainText.Contains(stringToAffect) && stringToAffect != "")
        {
            startAt = mainText.IndexOf(stringToAffect);
            endAt = startAt + stringToAffect.Length - 1;
        }
        else
        {
            startAt = 0;
            endAt = mainText.Length - 1;
        }
    }
    private bool InBetween(int checkValue, int start, int end)
    {
        return (checkValue >= start && checkValue <= end);
    }

    public void AnimateText(TMP_Text textComponent)
    {
        CheckText(textComponent);
        // Update the mesh and store texInfo in a variable
        TMP_TextInfo textInfo = textComponent.textInfo;

        //Loop through each character
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            Color32[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32;
            int vertexIndex = charInfo.vertexIndex;

            //Check if the character is visible. if it isn't then skip to the next iteration in the loop
            if (!charInfo.isVisible)
            {
                continue;
            }
            switch (listID)
            {
                case 0:
                    if (!InBetween(i, startAt, endAt))
                    {
                        continue;
                    }
                    break;

                case 1:
                    if (InBetween(i, startAt, endAt) && stringToAffect != "")
                    {
                        continue;
                    }
                    break;
            }
            //Apply the colour to the different vertices
            vertices[vertexIndex + 0] = bottomLeftCorner;
            vertices[vertexIndex + 1] = topLeftCorner;
            vertices[vertexIndex + 2] = topRightCorner;
            vertices[vertexIndex + 3] = bottomRightCorner;

        }
    }
}
