using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextAnimator))]
public class AnimatorEditor : Editor
{
    int currentNum;
    int oldNum;
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        TextAnimator textAni = (TextAnimator)target;
        EditorUtility.SetDirty(textAni);

        EditorGUILayout.LabelField("Spawn Effect", EditorStyles.boldLabel);
        textAni.listIDInit = EditorGUILayout.Popup(textAni.listIDInit, textAni.initList.ToArray());

        EditorGUI.indentLevel++;
        switch (textAni.listIDInit)
        {
            case 1:
                if (textAni.typewriter == null)
                {
                    textAni.SetUp();
                }
                textAni.typewriter.showParameters = EditorGUILayout.Foldout(textAni.typewriter.showParameters, "Parameters");
                if (textAni.typewriter.showParameters)
                {
                    EditorGUI.indentLevel++;
                    textAni.typewriter.typeSpeed = EditorGUILayout.Slider("Type Speed", textAni.typewriter.typeSpeed, 0.001f, 1);
                    EditorGUI.indentLevel--;
                }
                break;
            default:
                break;
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Number of Effects", EditorStyles.boldLabel);

        textAni.numbOfEffects = EditorGUILayout.IntSlider(textAni.numbOfEffects, 0, 6);
        currentNum = textAni.numbOfEffects;
        if (currentNum > textAni.listID.Count)
        {
            textAni.listID.Add(0);
        }
        if (currentNum < textAni.listID.Count)
        {
            textAni.listID.RemoveAt(textAni.listID.Count - 1);
        }

        EditorGUI.indentLevel++;

        for (int i = 0; i < textAni.numbOfEffects; i++)
        {
            EditorGUILayout.LabelField("Effect " + (i + 1), EditorStyles.boldLabel);
            textAni.listID[i] = EditorGUILayout.Popup(textAni.listID[i], textAni.textEffectList.ToArray());
            EditorGUI.indentLevel++;

            switch (textAni.listID[i])
            {
                case 1:
                    if (textAni.wobble == null)
                    {
                        textAni.SetUp();
                    }
                    textAni.wobble.showParameters = EditorGUILayout.Foldout(textAni.wobble.showParameters, "Parameters");
                    if (textAni.wobble.showParameters)
                    {
                        EditorGUI.indentLevel++;
                        textAni.wobble.wobbleSpeed = EditorGUILayout.Slider("Wobble Speed", textAni.wobble.wobbleSpeed, 0, 10);
                        textAni.wobble.horizontalWobbleAmount = EditorGUILayout.Slider("Horizontal Wobble Amount", textAni.wobble.horizontalWobbleAmount, 0, 5);           
                        textAni.wobble.verticalWobbleAmount = EditorGUILayout.Slider("Vertical Wobble Amount", textAni.wobble.verticalWobbleAmount, 0, 5);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.Space();
                    textAni.wobble.listID = EditorGUILayout.Popup(" ",textAni.wobble.listID, textAni.includeExcludeList.ToArray());
                    textAni.wobble.stringToAffect = EditorGUILayout.TextField("Word/s to Affect", textAni.wobble.stringToAffect);
                    break;

                case 2:
                    if (textAni.floaty == null)
                    {
                        textAni.SetUp();
                    }
                    textAni.floaty.showParameters = EditorGUILayout.Foldout(textAni.floaty.showParameters, "Parameters");
                    if (textAni.floaty.showParameters)
                    {
                        EditorGUI.indentLevel++;
                        textAni.floaty.floatSpeed = EditorGUILayout.FloatField("Float Speed", textAni.floaty.floatSpeed);
                        textAni.floaty.floatHeight = EditorGUILayout.Slider("Float Height", textAni.floaty.floatHeight, 0, 5);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.Space();
                    textAni.floaty.listID = EditorGUILayout.Popup(" ", textAni.floaty.listID, textAni.includeExcludeList.ToArray());
                    textAni.floaty.stringToAffect = EditorGUILayout.TextField("Word/s to Affect", textAni.floaty.stringToAffect);
                    break;

                case 3:
                    if (textAni.swing == null)
                    {
                        textAni.SetUp();
                    }
                    textAni.swing.showParameters = EditorGUILayout.Foldout(textAni.swing.showParameters, "Parameters");
                    if (textAni.swing.showParameters)
                    {
                        EditorGUI.indentLevel++;
                        textAni.swing.swingSpeed = EditorGUILayout.Slider("Swing Speed", textAni.swing.swingSpeed, 0, 1);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.Space();
                    textAni.swing.listID = EditorGUILayout.Popup(" ", textAni.swing.listID, textAni.includeExcludeList.ToArray());
                    textAni.swing.stringToAffect = EditorGUILayout.TextField("Word/s to Affect", textAni.swing.stringToAffect);
                    break;

                case 4:
                    if (textAni.shake == null)
                    {
                        textAni.SetUp();
                    }
                    textAni.shake.showParameters = EditorGUILayout.Foldout(textAni.shake.showParameters, "Parameters");
                    if (textAni.shake.showParameters)
                    {
                        EditorGUI.indentLevel++;
                        textAni.shake.shakeRange = EditorGUILayout.FloatField("Shake Range", textAni.shake.shakeRange);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.Space();
                    textAni.shake.listID = EditorGUILayout.Popup(" ", textAni.shake.listID, textAni.includeExcludeList.ToArray());
                    textAni.shake.stringToAffect = EditorGUILayout.TextField("Word/s to Affect", textAni.shake.stringToAffect);
                    break;

                case 5:
                    if (textAni.rainbow == null)
                    {
                        textAni.SetUp();
                    }
                    textAni.rainbow.showParameters = EditorGUILayout.Foldout(textAni.rainbow.showParameters, "Parameters");
                    if (textAni.rainbow.showParameters)
                    {
                        EditorGUI.indentLevel++;
                        textAni.rainbow.speed = EditorGUILayout.Slider("Speed", textAni.rainbow.speed, 0, 0.5f);
                        textAni.rainbow.opacity = EditorGUILayout.Slider("Opacity", textAni.rainbow.opacity, 0, 1);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.Space();
                    textAni.rainbow.listID = EditorGUILayout.Popup(" ", textAni.rainbow.listID, textAni.includeExcludeList.ToArray());
                    textAni.rainbow.stringToAffect = EditorGUILayout.TextField("Word/s to Affect", textAni.rainbow.stringToAffect);
                    break;

                case 6:
                    if (textAni.colourGrad == null)
                    {
                        textAni.SetUp();
                    }
                    textAni.colourGrad.topLeftCorner = EditorGUILayout.ColorField("Top Left Corner", textAni.colourGrad.topLeftCorner);
                    textAni.colourGrad.topRightCorner = EditorGUILayout.ColorField("Top Right Corner", textAni.colourGrad.topRightCorner);
                    textAni.colourGrad.bottomLeftCorner = EditorGUILayout.ColorField("Bottom Left Corner", textAni.colourGrad.bottomLeftCorner);
                    textAni.colourGrad.bottomRightCorner = EditorGUILayout.ColorField("Bottom Right Corner", textAni.colourGrad.bottomRightCorner);
                    EditorGUILayout.Space();
                    textAni.colourGrad.listID = EditorGUILayout.Popup(" ", textAni.colourGrad.listID, textAni.includeExcludeList.ToArray());
                    textAni.colourGrad.stringToAffect = EditorGUILayout.TextField("Word/s to Affect", textAni.colourGrad.stringToAffect);
                    break;

                case 7:
                    if (textAni.fading == null)
                    {
                        textAni.SetUp();
                    }
                    textAni.fading.showParameters = EditorGUILayout.Foldout(textAni.fading.showParameters, "Parameters");
                    if (textAni.fading.showParameters)
                    {
                        EditorGUI.indentLevel++;
                        textAni.fading.fadeSpeed = EditorGUILayout.FloatField("Fade Speed", textAni.fading.fadeSpeed);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.Space();
                    textAni.fading.listID = EditorGUILayout.Popup(" ", textAni.fading.listID, textAni.includeExcludeList.ToArray());
                    textAni.fading.stringToAffect = EditorGUILayout.TextField("Word/s to Affect", textAni.fading.stringToAffect);
                    break;

                default:
                    break;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }     
    }
}
