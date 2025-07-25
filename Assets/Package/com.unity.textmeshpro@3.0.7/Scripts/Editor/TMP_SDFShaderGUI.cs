﻿using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace TMPro.EditorUtilities
{
    public class TMP_SDFShaderGUI : TMP_BaseShaderGUI
    {
        static ShaderFeature s_OutlineFeature, s_UnderlayFeature, s_BevelFeature, s_GlowFeature, s_MaskFeature;

        static bool s_Face = true, s_Outline = true, s_Outline2, s_Underlay, s_Lighting, s_Glow, s_Bevel, s_Light, s_Bump, s_Env;

        static string[]
            s_FaceUVSpeedName = { "_FaceUVSpeed" },
            s_FaceUvSpeedNames = { "_FaceUVSpeedX", "_FaceUVSpeedY" },
            s_OutlineUvSpeedNames = { "_OutlineUVSpeedX", "_OutlineUVSpeedY" };
        static TMP_SDFShaderGUI()
        {
            s_OutlineFeature = new ShaderFeature()
            {
                undoLabel = "Outline",
                keywords = new[] { "OUTLINE_ON" }
            };

            s_UnderlayFeature = new ShaderFeature()
            {
                undoLabel = "Underlay",
                keywords = new[] { "UNDERLAY_ON", "UNDERLAY_INNER" },
                label = new GUIContent("Underlay Type"),
                keywordLabels = new[]
                {
                    new GUIContent("None"), new GUIContent("Normal"), new GUIContent("Inner")
                }
            };

            s_BevelFeature = new ShaderFeature()
            {
                undoLabel = "Bevel",
                keywords = new[] { "BEVEL_ON" }
            };

            s_GlowFeature = new ShaderFeature()
            {
                undoLabel = "Glow",
                keywords = new[] { "GLOW_ON" }
            };

            s_MaskFeature = new ShaderFeature()
            {
                undoLabel = "Mask",
                keywords = new[] { "MASK_HARD", "MASK_SOFT" },
                label = new GUIContent("Mask"),
                keywordLabels = new[]
                {
                    new GUIContent("Mask Off"), new GUIContent("Mask Hard"), new GUIContent("Mask Soft")
                }
            };
        }

        protected override void DoGUI()
        {
            DoAtlasTexture();
            DoBlendMode();
            s_Face = BeginPanel("Face", s_Face);
            if (s_Face)
            {
                DoFacePanel();
            }

            EndPanel();

            s_Outline = m_Material.HasProperty(ShaderUtilities.ID_OutlineTex) ? BeginPanel("Outline", s_Outline) : BeginPanel("Outline", s_OutlineFeature, s_Outline);
            if (s_Outline)
            {
                DoOutlinePanel();
            }

            EndPanel();

            if (m_Material.HasProperty(ShaderUtilities.ID_Outline2Color))
            {
                s_Outline2 = BeginPanel("Outline 2", s_OutlineFeature, s_Outline2);
                if (s_Outline2)
                {
                    DoOutline2Panel();
                }

                EndPanel();
            }

            if (m_Material.HasProperty(ShaderUtilities.ID_UnderlayColor))
            {
                s_Underlay = BeginPanel("Underlay", s_UnderlayFeature, s_Underlay);
                if (s_Underlay)
                {
                    DoUnderlayPanel();
                }

                EndPanel();
            }

            if (m_Material.HasProperty("_SpecularColor"))
            {
                s_Lighting = BeginPanel("Lighting", s_BevelFeature, s_Lighting);
                if (s_Lighting)
                {
                    s_Bevel = BeginPanel("Bevel", s_Bevel);
                    if (s_Bevel)
                    {
                        DoBevelPanel();
                    }

                    EndPanel();

                    s_Light = BeginPanel("Local Lighting", s_Light);
                    if (s_Light)
                    {
                        DoLocalLightingPanel();
                    }

                    EndPanel();

                    s_Bump = BeginPanel("Bump Map", s_Bump);
                    if (s_Bump)
                    {
                        DoBumpMapPanel();
                    }

                    EndPanel();

                    s_Env = BeginPanel("Environment Map", s_Env);
                    if (s_Env)
                    {
                        DoEnvMapPanel();
                    }

                    EndPanel();
                }

                EndPanel();
            }
            else if (m_Material.HasProperty("_SpecColor"))
            {
                s_Bevel = BeginPanel("Bevel", s_Bevel);
                if (s_Bevel)
                {
                    DoBevelPanel();
                }

                EndPanel();

                s_Light = BeginPanel("Surface Lighting", s_Light);
                if (s_Light)
                {
                    DoSurfaceLightingPanel();
                }

                EndPanel();

                s_Bump = BeginPanel("Bump Map", s_Bump);
                if (s_Bump)
                {
                    DoBumpMapPanel();
                }

                EndPanel();

                s_Env = BeginPanel("Environment Map", s_Env);
                if (s_Env)
                {
                    DoEnvMapPanel();
                }

                EndPanel();
            }

            if (m_Material.HasProperty(ShaderUtilities.ID_GlowColor))
            {
                s_Glow = BeginPanel("Glow", s_GlowFeature, s_Glow);
                if (s_Glow)
                {
                    DoGlowPanel();
                }

                EndPanel();
            }

            s_DebugExtended = BeginPanel("Debug Settings", s_DebugExtended);
            if (s_DebugExtended)
            {
                DoDebugPanel();
            }

            EndPanel();
        }
        
        private enum BlendPreset
        {
            Opaque,
            AlphaBlend,
            Additive,
            Multiply,
            Custom
        }
        
        private static readonly GUIContent[] BlendModePresets =
        {
            new GUIContent("Opaque"),
            new GUIContent("Alpha Blend"),
            new GUIContent("Additive"),
            new GUIContent("Multiply"),
            new GUIContent("Custom")
        };
        
        private void DoBlendMode()
        {
            BlendPreset blendPreset = GetCurrentBlendPreset(m_Material);
        
            EditorGUI.BeginChangeCheck();
            blendPreset = (BlendPreset)EditorGUILayout.Popup(new GUIContent("Blend Mode"), (int)blendPreset, BlendModePresets);
        
            if (EditorGUI.EndChangeCheck())
            {
                ApplyBlendPreset(m_Material, blendPreset);
            }
        
            // 如果是自定义模式，显示详细参数
            if (blendPreset == BlendPreset.Custom)
            {
                EditorGUI.indentLevel++;
                MaterialProperty srcBlend = FindProperty("_SrcBlend", m_Properties);
                MaterialProperty dstBlend = FindProperty("_DstBlend", m_Properties);
                MaterialProperty blendOp = FindProperty("_BlendOp", m_Properties);
            
                m_Editor.ShaderProperty(srcBlend, "Source Blend");
                m_Editor.ShaderProperty(dstBlend, "Destination Blend");
                m_Editor.ShaderProperty(blendOp, "Blend Operation");
                EditorGUI.indentLevel--;
            }
        }
        
        private void ApplyBlendPreset(Material material, BlendPreset preset)
        {
            switch (preset)
            {
                case BlendPreset.Opaque:
                    material.SetFloat("_SrcBlend", (float)BlendMode.One);
                    material.SetFloat("_DstBlend", (float)BlendMode.Zero);
                    material.SetFloat("_BlendOp", (float)BlendOp.Add);
                    material.SetFloat("_ZWrite", 1);
                    material.renderQueue = (int)RenderQueue.Geometry;
                    break;
                
                case BlendPreset.AlphaBlend:
                    material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                    material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                    material.SetFloat("_BlendOp", (float)BlendOp.Add);
                    material.SetFloat("_ZWrite", 0);
                    material.renderQueue = (int)RenderQueue.Transparent;
                    break;
                
                case BlendPreset.Additive:
                    material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                    material.SetFloat("_DstBlend", (float)BlendMode.One);
                    material.SetFloat("_BlendOp", (float)BlendOp.Add);
                    material.SetFloat("_ZWrite", 0);
                    material.renderQueue = (int)RenderQueue.Transparent;
                    break;
                
                case BlendPreset.Multiply:
                    material.SetFloat("_SrcBlend", (float)BlendMode.DstColor);
                    material.SetFloat("_DstBlend", (float)BlendMode.Zero);
                    material.SetFloat("_BlendOp", (float)BlendOp.Add);
                    material.SetFloat("_ZWrite", 0);
                    material.renderQueue = (int)RenderQueue.Transparent;
                    break;
            }
        }
        
        private BlendPreset GetCurrentBlendPreset(Material material)
        {
            int srcBlend = (int)material.GetFloat("_SrcBlend");
            int dstBlend = (int)material.GetFloat("_DstBlend");
            int blendOp = (int)material.GetFloat("_BlendOp");
        
            // Opaque
            if (srcBlend == (int)BlendMode.One && dstBlend == (int)BlendMode.Zero && blendOp == (int)BlendOp.Add)
                return BlendPreset.Opaque;
        
            // Alpha Blend
            if (srcBlend == (int)BlendMode.SrcAlpha && dstBlend == (int)BlendMode.OneMinusSrcAlpha && blendOp == (int)BlendOp.Add)
                return BlendPreset.AlphaBlend;
        
            // Additive
            if (srcBlend == (int)BlendMode.SrcAlpha && dstBlend == (int)BlendMode.One && blendOp == (int)BlendOp.Add)
                return BlendPreset.Additive;
        
            // Multiply
            if (srcBlend == (int)BlendMode.DstColor && dstBlend == (int)BlendMode.Zero && blendOp == (int)BlendOp.Add)
                return BlendPreset.Multiply;
        
            return BlendPreset.Custom;
        }
        
        private void DoAtlasTexture()
        {
            DoTexture2D("_AtlasTex","Texture",false);
            DoVector("_Atlas_ST1","ST",s_ST);
            DoColor("_AtlasColor","color");
            DoFloat("_Type1", "render Type");
        }

        void DoFacePanel()
        {
            EditorGUI.indentLevel += 1;

            DoColor("_FaceColor", "Color");

            if (m_Material.HasProperty(ShaderUtilities.ID_FaceTex))
            {
                if (m_Material.HasProperty("_FaceUVSpeedX"))
                {
                    DoTexture2D("_FaceTex", "Texture", true, s_FaceUvSpeedNames);
                }
                else if (m_Material.HasProperty("_FaceUVSpeed"))
                {
                    DoTexture2D("_FaceTex", "Texture", true, s_FaceUVSpeedName);
                }
                else
                {
                    DoTexture2D("_FaceTex", "Texture", true);
                }
            }

            if (m_Material.HasProperty("_FaceSoftness"))
            {
                DoSlider("_FaceSoftness", "X", "Softness");
            }

            if (m_Material.HasProperty("_OutlineSoftness"))
            {
                DoSlider("_OutlineSoftness", "Softness");
            }

            if (m_Material.HasProperty(ShaderUtilities.ID_FaceDilate))
            {
                DoSlider("_FaceDilate", "Dilate");
                if (m_Material.HasProperty(ShaderUtilities.ID_Shininess))
                {
                    DoSlider("_FaceShininess", "Gloss");
                }
            }

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        void DoOutlinePanel()
        {
            EditorGUI.indentLevel += 1;
            DoColor("_OutlineColor", "Color");
            if (m_Material.HasProperty(ShaderUtilities.ID_OutlineTex))
            {
                if (m_Material.HasProperty("_OutlineUVSpeedX"))
                {
                    DoTexture2D("_OutlineTex", "Texture", true, s_OutlineUvSpeedNames);
                }
                else
                {
                    DoTexture2D("_OutlineTex", "Texture", true);
                }
            }

            DoSlider("_OutlineWidth", "Thickness");
            if (m_Material.HasProperty("_OutlineShininess"))
            {
                DoSlider("_OutlineShininess", "Gloss");
            }

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        void DoOutline2Panel()
        {
            EditorGUI.indentLevel += 1;
            DoColor("_Outline2Color", "Color");
            //if (m_Material.HasProperty(ShaderUtilities.ID_OutlineTex))
            //{
            //    if (m_Material.HasProperty("_OutlineUVSpeedX"))
            //    {
            //        DoTexture2D("_OutlineTex", "Texture", true, s_OutlineUvSpeedNames);
            //    }
            //    else
            //    {
            //        DoTexture2D("_OutlineTex", "Texture", true);
            //    }
            //}

            DoSlider("_Outline2Width", "Thickness");
            //if (m_Material.HasProperty("_OutlineShininess"))
            //{
            //    DoSlider("_OutlineShininess", "Gloss");
            //}

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        void DoUnderlayPanel()
        {
            EditorGUI.indentLevel += 1;
            s_UnderlayFeature.DoPopup(m_Editor, m_Material);
            DoColor("_UnderlayColor", "Color");
            DoSlider("_UnderlayOffsetX", "Offset X");
            DoSlider("_UnderlayOffsetY", "Offset Y");
            DoSlider("_UnderlayDilate", "Dilate");
            DoSlider("_UnderlaySoftness", "Softness");
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        static GUIContent[] s_BevelTypeLabels =
        {
            new GUIContent("Outer Bevel"),
            new GUIContent("Inner Bevel")
        };

        void DoBevelPanel()
        {
            EditorGUI.indentLevel += 1;
            DoPopup("_ShaderFlags", "Type", s_BevelTypeLabels);
            DoSlider("_Bevel", "Amount");
            DoSlider("_BevelOffset", "Offset");
            DoSlider("_BevelWidth", "Width");
            DoSlider("_BevelRoundness", "Roundness");
            DoSlider("_BevelClamp", "Clamp");
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        void DoLocalLightingPanel()
        {
            EditorGUI.indentLevel += 1;
            DoSlider("_LightAngle", "Light Angle");
            DoColor("_SpecularColor", "Specular Color");
            DoSlider("_SpecularPower", "Specular Power");
            DoSlider("_Reflectivity", "Reflectivity Power");
            DoSlider("_Diffuse", "Diffuse Shadow");
            DoSlider("_Ambient", "Ambient Shadow");
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        void DoSurfaceLightingPanel()
        {
            EditorGUI.indentLevel += 1;
            DoColor("_SpecColor", "Specular Color");
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        void DoBumpMapPanel()
        {
            EditorGUI.indentLevel += 1;
            DoTexture2D("_BumpMap", "Texture");
            DoSlider("_BumpFace", "Face");
            DoSlider("_BumpOutline", "Outline");
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        void DoEnvMapPanel()
        {
            EditorGUI.indentLevel += 1;
            DoColor("_ReflectFaceColor", "Face Color");
            DoColor("_ReflectOutlineColor", "Outline Color");
            DoCubeMap("_Cube", "Texture");
            DoVector3("_EnvMatrixRotation", "Rotation");
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        void DoGlowPanel()
        {
            EditorGUI.indentLevel += 1;
            DoColor("_GlowColor", "Color");
            DoSlider("_GlowOffset", "Offset");
            DoSlider("_GlowInner", "Inner");
            DoSlider("_GlowOuter", "Outer");
            DoSlider("_GlowPower", "Power");
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        void DoDebugPanel()
        {
            EditorGUI.indentLevel += 1;
            DoTexture2D("_MainTex", "Font Atlas");
            DoFloat("_GradientScale", "Gradient Scale");
            DoFloat("_TextureWidth", "Texture Width");
            DoFloat("_TextureHeight", "Texture Height");
            EditorGUILayout.Space();
            DoFloat("_ScaleX", "Scale X");
            DoFloat("_ScaleY", "Scale Y");

            if (m_Material.HasProperty(ShaderUtilities.ID_Sharpness))
                DoSlider("_Sharpness", "Sharpness");

            DoSlider("_PerspectiveFilter", "Perspective Filter");
            EditorGUILayout.Space();
            DoFloat("_VertexOffsetX", "Offset X");
            DoFloat("_VertexOffsetY", "Offset Y");

            if (m_Material.HasProperty(ShaderUtilities.ID_MaskCoord))
            {
                EditorGUILayout.Space();
                s_MaskFeature.ReadState(m_Material);
                s_MaskFeature.DoPopup(m_Editor, m_Material);
                if (s_MaskFeature.Active)
                {
                    DoMaskSubgroup();
                }

                EditorGUILayout.Space();
                DoVector("_ClipRect", "Clip Rect", s_LbrtVectorLabels);
            }
            else if (m_Material.HasProperty("_MaskTex"))
            {
                DoMaskTexSubgroup();
            }
            else if (m_Material.HasProperty(ShaderUtilities.ID_MaskSoftnessX))
            {
                EditorGUILayout.Space();
                DoFloat("_MaskSoftnessX", "Softness X");
                DoFloat("_MaskSoftnessY", "Softness Y");
                DoVector("_ClipRect", "Clip Rect", s_LbrtVectorLabels);
            }

            if (m_Material.HasProperty(ShaderUtilities.ID_StencilID))
            {
                EditorGUILayout.Space();
                DoFloat("_Stencil", "Stencil ID");
                DoFloat("_StencilComp", "Stencil Comp");
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            bool useRatios = EditorGUILayout.Toggle("Use Ratios", !m_Material.IsKeywordEnabled("RATIOS_OFF"));
            if (EditorGUI.EndChangeCheck())
            {
                m_Editor.RegisterPropertyChangeUndo("Use Ratios");
                if (useRatios)
                {
                    m_Material.DisableKeyword("RATIOS_OFF");
                }
                else
                {
                    m_Material.EnableKeyword("RATIOS_OFF");
                }
            }

            if (m_Material.HasProperty(ShaderUtilities.ShaderTag_CullMode))
            {
                EditorGUILayout.Space();
                DoPopup("_CullMode", "Cull Mode", s_CullingTypeLabels);
            }

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(true);
            DoFloat("_ScaleRatioA", "Scale Ratio A");
            DoFloat("_ScaleRatioB", "Scale Ratio B");
            DoFloat("_ScaleRatioC", "Scale Ratio C");
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        void DoMaskSubgroup()
        {
            DoVector("_MaskCoord", "Mask Bounds", s_XywhVectorLabels);
            if (Selection.activeGameObject != null)
            {
                Renderer renderer = Selection.activeGameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Rect rect = EditorGUILayout.GetControlRect();
                    rect.x += EditorGUIUtility.labelWidth;
                    rect.width -= EditorGUIUtility.labelWidth;
                    if (GUI.Button(rect, "Match Renderer Bounds"))
                    {
                        FindProperty("_MaskCoord", m_Properties).vectorValue = new Vector4(
                            0,
                            0,
                            Mathf.Round(renderer.bounds.extents.x * 1000) / 1000,
                            Mathf.Round(renderer.bounds.extents.y * 1000) / 1000
                        );
                    }
                }
            }

            if (s_MaskFeature.State == 1)
            {
                DoFloat("_MaskSoftnessX", "Softness X");
                DoFloat("_MaskSoftnessY", "Softness Y");
            }
        }

        void DoMaskTexSubgroup()
        {
            EditorGUILayout.Space();
            DoTexture2D("_MaskTex", "Mask Texture");
            DoToggle("_MaskInverse", "Inverse Mask");
            DoColor("_MaskEdgeColor", "Edge Color");
            DoSlider("_MaskEdgeSoftness", "Edge Softness");
            DoSlider("_MaskWipeControl", "Wipe Position");
            DoFloat("_MaskSoftnessX", "Softness X");
            DoFloat("_MaskSoftnessY", "Softness Y");
            DoVector("_ClipRect", "Clip Rect", s_LbrtVectorLabels);
        }
    }
}
