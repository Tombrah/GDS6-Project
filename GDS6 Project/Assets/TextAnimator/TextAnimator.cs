using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextAnimator : MonoBehaviour
{ 
    public TMP_Text textComponent;

    public int numbOfEffects = 0;
    public List<int> listID = new List<int>();
    public List<string> textEffectList = new List<string>(new string[] { "Nothing" , "Wobbly Text", "Floaty Text", "Swinging Text", "Shaky Text", "Rainbow Text", "Colour Gradient", "Fading Text" });

    public int listIDInit = 0;
    public List<string> initList = new List<string>(new string[] { "Nothing", "TypeWriter" });

    public List<string> includeExcludeList = new List<string>(new string[] { "Include" , "Exclude" });


    public WobblyText wobble;
    public FloatyText floaty;
    public SwingingText swing;
    public ShakyText shake;
    public ColourGradient colourGrad;
    public RainbowText rainbow;
    public FadingText fading;

    public TypewriterSpawn typewriter;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        swing.AssignStructValues();
    }

    public void SetUp()
    {
        wobble = new WobblyText();
        floaty = new FloatyText();
        swing = new SwingingText();
        shake = new ShakyText();
        rainbow = new RainbowText();
        colourGrad = new ColourGradient();
        fading = new FadingText();

        typewriter = new TypewriterSpawn();
    }

    private void Start()
    {
        //SpawnText();
    }

    private void FixedUpdate()
    {
        textComponent.ForceMeshUpdate();
        for (int i = 0; i < numbOfEffects; i++)
        {
            switch (listID[i])
            {
                case 1:
                    wobble.AnimateText(textComponent);
                    break;

                case 2:
                    floaty.AnimateText(textComponent);
                    break;

                case 3:
                    swing.AnimateText(textComponent);
                    break;
                case 4:
                    shake.AnimateText(textComponent);
                    break;

                case 5:
                    rainbow.AnimateText(textComponent);
                    break;

                case 6:
                    colourGrad.AnimateText(textComponent);
                    break;

                case 7:
                    fading.AnimateText(textComponent);
                    break;

                default:
                    break;

            }
        }
        UpdateMesh();
    }

    public void SpawnText()
    {
        textComponent.ForceMeshUpdate();
        switch (listIDInit)
        {
            case 1:
                StartCoroutine(typewriter.AnimateText(textComponent));
                break;

            default:
                break;

        }
    }

    public void UpdateMesh()
    {
        for (int i = 0; i < textComponent.textInfo.meshInfo.Length; i++)
        {
            TMP_MeshInfo meshInfo = textComponent.textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            meshInfo.mesh.colors32 = meshInfo.colors32;
            textComponent.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
