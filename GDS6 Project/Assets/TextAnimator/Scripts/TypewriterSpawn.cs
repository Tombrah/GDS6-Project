using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class TypewriterSpawn
{
    public bool showParameters;
    public float typeSpeed = 0.05f;

    public IEnumerator AnimateText(TMP_Text textComponent)
    {
        //textComponent.ForceMeshUpdate();

        int totalChars = textComponent.textInfo.characterCount;
        int counter = 0;
        int visibleCount;

        while (counter < totalChars + 1)
        {
            visibleCount = counter % (totalChars + 1);

            textComponent.maxVisibleCharacters = visibleCount;

            counter++;

            yield return new WaitForSeconds(typeSpeed);
        }     

    }
    
}
