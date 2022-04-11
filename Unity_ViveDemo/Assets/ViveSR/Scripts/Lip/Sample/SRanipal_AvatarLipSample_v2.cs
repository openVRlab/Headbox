//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
using System.Collections.Generic;
using UnityEngine;

namespace ViveSR
{
	namespace anipal
	{
		namespace Lip
		{
			public class SRanipal_AvatarLipSample_v2 : MonoBehaviour
			{
				[SerializeField] private List<LipShapeTable_v2> LipShapeTables;

				#region Headbox 42 blendshape Vive 
				//Mapping Code Reference: https://www.youtube.com/watch?v=LOIOU2v3N3w
				//*
				public const string BLENDSHAPE_PREFIX = "blendShape1.SR_";

				//Change the headboxWeight below
				public static List<LipMapping> LipMappings = new List<LipMapping> // See SRanipal_Lip_v2 Script (namespace Lip)
				{
					new LipMapping{viveName = LipShape_v2.Cheek_Puff_Left, avatarName = $"{BLENDSHAPE_PREFIX}01_Cheek_Puff_Left", headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Cheek_Puff_Right, avatarName = $"{BLENDSHAPE_PREFIX}02_Cheek_Puff_Right",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Cheek_Suck, avatarName = $"{BLENDSHAPE_PREFIX}03_Cheek_Suck",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Ape_Shape, avatarName = $"{BLENDSHAPE_PREFIX}23_Mouth_Ape_Shape",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Lower_DownLeft, avatarName = $"{BLENDSHAPE_PREFIX}24_Mouth_Lower_DownLeft" ,headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Lower_DownRight, avatarName = $"{BLENDSHAPE_PREFIX}25_Mouth_Lower_DownRight" ,headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Lower_Inside, avatarName = $"{BLENDSHAPE_PREFIX}26_Mouth_Lower_Inside",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Lower_Left, avatarName = $"{BLENDSHAPE_PREFIX}27_Mouth_Lower_Left",headboxWeight = 1.0f },
					new LipMapping{viveName = LipShape_v2.Mouth_Lower_Overlay, avatarName = $"{BLENDSHAPE_PREFIX}28_Mouth_Lower_Overlay",headboxWeight = 1.0f },
					new LipMapping{viveName = LipShape_v2.Mouth_Lower_Overturn, avatarName = $"{BLENDSHAPE_PREFIX}29_Mouth_Lower_Overturn",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Lower_Right, avatarName = $"{BLENDSHAPE_PREFIX}30_Mouth_Lower_Right" ,headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Sad_Left, avatarName = $"{BLENDSHAPE_PREFIX}32_Mouth_Sad_Left" ,headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Sad_Right, avatarName = $"{BLENDSHAPE_PREFIX}33_Mouth_Sad_Right",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Upper_Inside, avatarName = $"{BLENDSHAPE_PREFIX}36_Mouth_Upper_Inside",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Upper_Left, avatarName = $"{BLENDSHAPE_PREFIX}37_Mouth_Upper_Left" ,headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Upper_Overturn, avatarName = $"{BLENDSHAPE_PREFIX}38_Mouth_Upper_Overturn",headboxWeight = 1.0f },
					new LipMapping{viveName = LipShape_v2.Mouth_Upper_Right, avatarName = $"{BLENDSHAPE_PREFIX}39_Mouth_Upper_Right",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Upper_UpLeft, avatarName = $"{BLENDSHAPE_PREFIX}40_Mouth_Upper_UpLeft" ,headboxWeight = 0.3f},
					new LipMapping{viveName = LipShape_v2.Mouth_Upper_UpRight, avatarName = $"{BLENDSHAPE_PREFIX}41_Mouth_Upper_UpRight",headboxWeight = 0.3f},
					new LipMapping{viveName = LipShape_v2.Jaw_Forward, avatarName = $"{BLENDSHAPE_PREFIX}19_Jaw_Forward" ,headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Jaw_Left, avatarName = $"{BLENDSHAPE_PREFIX}20_Jaw_Left",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Jaw_Open, avatarName = $"{BLENDSHAPE_PREFIX}21_Jaw_Open" ,headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Jaw_Right, avatarName = $"{BLENDSHAPE_PREFIX}22_Jaw_Right" ,headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Pout, avatarName = $"{BLENDSHAPE_PREFIX}31_Mouth_Pout",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Smile_Left, avatarName = $"{BLENDSHAPE_PREFIX}34_Mouth_Smile_Left",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Mouth_Smile_Right, avatarName = $"{BLENDSHAPE_PREFIX}35_Mouth_Smile_Right" ,headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Tongue_Down, avatarName = $"{BLENDSHAPE_PREFIX}" ,headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Tongue_LongStep1, avatarName = $"{BLENDSHAPE_PREFIX}42_Tongue_LongStep1" ,headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Tongue_LongStep2, avatarName = $"{BLENDSHAPE_PREFIX}",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Tongue_DownLeft_Morph, avatarName = $"{BLENDSHAPE_PREFIX}",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Tongue_DownRight_Morph, avatarName = $"{BLENDSHAPE_PREFIX}" ,headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Tongue_Left, avatarName = $"{BLENDSHAPE_PREFIX}",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Tongue_Right, avatarName = $"{BLENDSHAPE_PREFIX}" ,headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Tongue_Roll, avatarName = $"{BLENDSHAPE_PREFIX}",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Tongue_Up, avatarName = $"{BLENDSHAPE_PREFIX}" ,headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Tongue_UpLeft_Morph, avatarName = $"{BLENDSHAPE_PREFIX}",headboxWeight = 1.0f},
					new LipMapping{viveName = LipShape_v2.Tongue_UpRight_Morph, avatarName = $"{BLENDSHAPE_PREFIX}" ,headboxWeight = 1.0f},

				};
				//*/
				
				public List<HeadboxMapped> headboxMapped = new List<HeadboxMapped>();
				private List<int> headboxIndex = new List<int>();

				#endregion

				public bool NeededToGetData = true;
				private Dictionary<LipShape_v2, float> LipWeightings;

				private void Start()
				{
					if (!SRanipal_Lip_Framework.Instance.EnableLip)
					{
						enabled = false;
						return;
					}
					SetLipShapeTables(LipShapeTables);
					#region Headbox START
					SetHeadboxMapped();
					#endregion
				}

				private void Update()
				{
					if (SRanipal_Lip_Framework.Status != SRanipal_Lip_Framework.FrameworkStatus.WORKING) return;

					if (NeededToGetData)
					{
						SRanipal_Lip_v2.GetLipWeightings(out LipWeightings);
						UpdateLipShapes(LipWeightings);
					}
				}

				public void SetLipShapeTables(List<LipShapeTable_v2> lipShapeTables)
				{
					bool valid = true;
					if (lipShapeTables == null)
					{
						valid = false;
					}
					else
					{
						for (int table = 0; table < lipShapeTables.Count; ++table)
						{
							if (lipShapeTables[table].skinnedMeshRenderer == null)
							{
								valid = false;
								break;
							}
							for (int shape = 0; shape < lipShapeTables[table].lipShapes.Length; ++shape)
							{
								LipShape_v2 lipShape = lipShapeTables[table].lipShapes[shape];
								if (lipShape > LipShape_v2.Max || lipShape < 0)
								{
									valid = false;
									break;
								}
							}
						}
					}
					if (valid)
						LipShapeTables = lipShapeTables;
				}



				#region  Set MappedHeadbox
				private void SetHeadboxMapped()
				{
					foreach (var table in LipShapeTables)
					{
						HeadboxLipMapped(table);
					}
				}

				#endregion
				public void UpdateLipShapes(Dictionary<LipShape_v2, float> lipWeightings)
				{
					foreach (var table in LipShapeTables)
						RenderModelLipShape(table, lipWeightings);
				}

				private void RenderModelLipShape(LipShapeTable_v2 lipShapeTable, Dictionary<LipShape_v2, float> weighting)
				{
					for (int i = 0; i < lipShapeTable.lipShapes.Length; i++)
					{
						int targetIndex = (int)lipShapeTable.lipShapes[i];
						if (targetIndex > (int)LipShape_v2.Max || targetIndex < 0) continue;
						//lipShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weighting[(LipShape_v2)targetIndex] * 100);

						#region Headbox Weight
						if (headboxIndex.Contains(i))
						{
							for (int j = 0; j < headboxMapped.Count; j++)
							{
								if (headboxMapped[j].headboxMappedIndex == i)
								{
									lipShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weighting[(LipShape_v2)targetIndex] * 100 * headboxMapped[j].headboxWeight);

								}

							}

						}
						else
						{
							lipShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weighting[(LipShape_v2)targetIndex] * 100);
						}



						#endregion
					}
				}

				#region HeadboxWeightMapping

				private void HeadboxLipMapped(LipShapeTable_v2 lipShapeTable)
				{
					for (int skinnedIndex = 0; skinnedIndex < lipShapeTable.skinnedMeshRenderer.sharedMesh.blendShapeCount; ++skinnedIndex)
					{
						string elementName = lipShapeTable.skinnedMeshRenderer.sharedMesh.GetBlendShapeName(skinnedIndex);

						foreach (LipMapping lipmapping in LipMappings)
						{
							if (elementName == lipmapping.avatarName)
							{
								HeadboxMapped hb = new HeadboxMapped(skinnedIndex, elementName, lipmapping.headboxWeight);
								if (!Contains(headboxMapped, hb)) // There should be a simpler way to do this here, so if have time change it
								{
									headboxMapped.Add(hb);
									headboxIndex.Add(hb.headboxMappedIndex);
								}

								//Debug.Log("run: " + elementName + skinnedIndex);
							}

						}
					}
				}

				bool Contains(List<HeadboxMapped> list, HeadboxMapped headboxMapped)
				{
					foreach (HeadboxMapped n in list)
					{
						if (n.headboxMappedIndex == headboxMapped.headboxMappedIndex && n.headboxMappedName == headboxMapped.headboxMappedName && n.headboxWeight == headboxMapped.headboxWeight)
						{ return true; }
					}
					return false;
				}
				#endregion
			}
		}
	}
}