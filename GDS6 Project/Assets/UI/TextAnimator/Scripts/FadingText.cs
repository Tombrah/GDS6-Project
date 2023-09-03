using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class FadingText
{
    public bool showParameters;
    public float fadeSpeed = 0.1f;
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

        Color32 c0 = textComponent.color;
        Color32 c1 = textComponent.color;

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

            c0 = new Color32(255, 255, 255, (byte)Mathf.PingPong(Time.time * fadeSpeed, 255));

            vertices[vertexIndex + 0] = c0;
            vertices[vertexIndex + 1] = c0;
            vertices[vertexIndex + 2] = c0;
            vertices[vertexIndex + 3] = c0;
            
        }
    }
}
