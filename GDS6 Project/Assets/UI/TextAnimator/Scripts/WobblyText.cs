using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class WobblyText
{
    public bool showParameters;
    public float wobbleSpeed = 3;
    public float horizontalWobbleAmount = 2.8f;
    public float verticalWobbleAmount = 2;
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

        //This loop applies the effect to each character
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
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

            //Apply the offset to the different vertices
            for (int j = 0; j < 4; j++)
            {
                Vector3 offset = Wobble(Time.time * wobbleSpeed + j);
                vertices[vertexIndex + j] += offset;
            }

        }
        
    }
    //Offset
    Vector2 Wobble(float time)
    {
        return new Vector2(Mathf.Sin(time * horizontalWobbleAmount), Mathf.Cos(time * verticalWobbleAmount));
    }

}
