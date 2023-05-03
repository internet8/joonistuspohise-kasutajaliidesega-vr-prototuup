namespace FunktronicLabs
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.Rendering;

    public class VolumetricCrystalMaterialEditor_HDRP : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            // if we're not using hdrp, we do not need this custom material editor 
            if (GraphicsSettings.currentRenderPipeline == null || !GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
            {
                return;
            }

            var material = (Material) materialEditor.target;

            EditorGUILayout.LabelField("RenderQueue"); 
            var newRenderQueue = (RenderQueue) EditorGUILayout.EnumPopup((RenderQueue) material.renderQueue); 
            var newRenderQueueInt = EditorGUILayout.IntField(material.renderQueue);

            if(material.renderQueue < (int) RenderQueue.Transparent && material.IsKeywordEnabled("EnableRefraction"))
            {
                EditorGUILayout.HelpBox("Cannot use refraction if rendering before the transparency queue.", MessageType.Warning);
            }

            if(GUI.changed)
            {
                Undo.RecordObject(material, "render queue");

                if(newRenderQueueInt != material.renderQueue)
                {
                    material.renderQueue = newRenderQueueInt;
                }

                else if((int) newRenderQueue != material.renderQueue)
                {
                    material.renderQueue = (int) newRenderQueue;
                }
            }

        }
    }
}