using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class SwingingText
{
    public bool showParameters;
    public float swingSpeed = 0.5f;
    public int listID = 0;
    public string stringToAffect;

    private int startAt;
    private int endAt;

    private struct VertexAnim
    {
        public float angleRange;
        public float angle;
        public float speed;
    }

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


    Matrix4x4 matrix;
    VertexAnim[] vertexAnim = new VertexAnim[1024];
    public void AssignStructValues()
    {
        for (int i = 0; i < 1024; i++)
        {
            vertexAnim[i].angleRange = Random.Range(10, 25);
            vertexAnim[i].speed = Random.Range(1,5);
        }
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
            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            int vertexIndex = charInfo.vertexIndex;

            VertexAnim vertAnim = vertexAnim[i];

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
            Vector2 charMidTopPoint = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, charInfo.topRight.y);

            //Apply the offset to the different vertices
            Vector3 offset = charMidTopPoint;

            vertices[vertexIndex + 0] -= offset;
            vertices[vertexIndex + 1] -= offset;
            vertices[vertexIndex + 2] -= offset;
            vertices[vertexIndex + 3] -= offset;

            vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong((Time.time + i) * swingSpeed * vertAnim.speed, 1f));

            matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, vertexAnim[i].angle), Vector3.one);

            vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
            vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
            vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
            vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;

            vertexAnim[i] = vertAnim;
        }
    }
}
