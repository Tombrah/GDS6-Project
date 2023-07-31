using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class RainbowText
{
    public bool showParameters;
    public float speed = 0.1f;
    public float opacity = 1;
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

            c0 = Gradient().Evaluate(Mathf.Abs(Time.time * speed - vertexIndex - 10) * 100/7 % 1);
            c1 = Gradient().Evaluate(Mathf.Abs(Time.time * speed - vertexIndex - 10.5f) * 100/7 % 1);

            vertices[vertexIndex + 0] = c0;
            vertices[vertexIndex + 1] = c0;
            vertices[vertexIndex + 2] = c1;
            vertices[vertexIndex + 3] = c1;
            
        }
    }

    Gradient Gradient()
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colourKey = new GradientColorKey[8];
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];

        colourKey[0].color = new Color32(255, 70, 70, 255);
        colourKey[0].time = 0;
        colourKey[1].color = new Color32(255, 152, 51, 255);
        colourKey[1].time = 0.14285f;
        colourKey[2].color = new Color32(255, 255, 83, 255);
        colourKey[2].time = 0.2857f;
        colourKey[3].color = new Color32(73, 255, 73, 255);
        colourKey[3].time = 0.42855f;
        colourKey[4].color = new Color32(71, 71, 255, 255);
        colourKey[4].time = 0.5714f;
        colourKey[5].color = new Color32(162, 73, 255, 255);
        colourKey[5].time = 0.71425f;
        colourKey[6].color = new Color32(255, 80, 255, 255);
        colourKey[6].time = 0.8571f;
        colourKey[7].color = new Color32(255, 76, 76, 255);
        colourKey[7].time = 1;

        alphaKey[0].alpha = opacity;
        alphaKey[0].time = 0;
        alphaKey[1].alpha = opacity;
        alphaKey[1].time = 1;

        gradient.SetKeys(colourKey, alphaKey);
        gradient.mode = GradientMode.Blend;

        return gradient;
    }
}
