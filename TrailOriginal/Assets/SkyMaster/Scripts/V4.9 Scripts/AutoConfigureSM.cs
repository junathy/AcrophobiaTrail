using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.SKYMASTER
{
    [ExecuteAlways]
    public class AutoConfigureSM : MonoBehaviour
    {

        public SkyMasterManager SkyManager;
        // Start is called before the first frame update
        void Start()
        {
            
        }
        public bool configure = false;
        public bool enableWater = true;
        public bool configured = false;

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            int stage = GameObjectTypeLoggingSM.postStageInformation(this.gameObject);
            if (stage == 4)
            {
                Debug.Log("Grass is in prefab edit mode");
                return;
            }
#endif
            //configure and close

            if (configure && !configured && SkyManager != null)
            {
#if UNITY_EDITOR
                LayerMaskCreateSM.CreateLayer("Background");
                LayerMaskCreateSM.CreateLayer("Conductor");
#endif
                if (Camera.main == null)
                {
                    //add main camera
                    GameObject cameraMain = new GameObject();
                    cameraMain.tag = "MainCamera";
                    cameraMain.AddComponent<Camera>();
                }

                if (Camera.main != null)
                {                      
                    //v4.8.4
                    Camera.main.farClipPlane = 30000;
                    Camera.main.transform.position = Camera.main.transform.position + new Vector3(0,10,0);
                    SkyManager.Current_Time = 10.3f;

                    //ADD CLOUDS
                    FullVolumeCloudsSkyMaster cloudsScript = Camera.main.gameObject.GetComponent<FullVolumeCloudsSkyMaster>();
                    if (cloudsScript != null)
                    {
                        cloudsScript.Sun = SkyManager.SunObj.transform;
                        cloudsScript.SkyManager = SkyManager;                        
                        cloudsScript.initVariablesScatter();
                        SkyManager.volumeClouds = cloudsScript;                                                                      
                    }
                    else
                    {                        
                        cloudsScript = Camera.main.gameObject.AddComponent<FullVolumeCloudsSkyMaster>();
                        cloudsScript.Sun = SkyManager.SunObj.transform; 
                        cloudsScript.SkyManager = SkyManager;
                        cloudsScript.initVariablesA();
                        cloudsScript.initVariablesScatter();
                        SkyManager.volumeClouds = cloudsScript;                         
                    }

                    //Use Temporal AA to lower cloud raycast steps
                    if (Camera.main.gameObject.GetComponent<TemporalReprojection>() == null)
                    {
                        Camera.main.gameObject.AddComponent<TemporalReprojection>();
                    }

                    //ADD REFLECTIONS if water active
                    FullVolumeCloudsSkyMaster CloudsScript = SkyManager.volumeClouds;
                    int layerToCheck = LayerMask.NameToLayer("Background");

                    if (CloudsScript != null)
                    {
                        if (SkyManager.water != null)
                        { //v4.8.5
                            if (CloudsScript.reflectClouds == null) //v4.8.5
                            {

                                //WATER enable
                                if (enableWater)
                                {
                                    SkyManager.water.transform.parent.gameObject.SetActive(true);
                                    SkyManager.water.GetComponent<PlanarReflectionSM>().enabled = true; /////////enable reflect
                                    //if (GUILayout.Button(new GUIContent("Add Underwater blur"), GUILayout.Width(150)))
                                    //{
                                        if (Camera.main != null && Camera.main.gameObject.GetComponent<UnderWaterImageEffect>() == null)
                                        {
                                            Camera.main.gameObject.AddComponent<UnderWaterImageEffect>();
                                        }
                                        else
                                        {
                                            Debug.Log("Add a main camera first");
                                        }

                                        //LEFT-RIGHT VR CAMERAS
                                        //if (SkyManager.Mesh_Terrain_controller != null)
                                        //{
                                        //    if (TerrainManager.LeftCam != null && TerrainManager.LeftCam.GetComponent<UnderWaterImageEffect>() == null)
                                        //    {
                                        //        TerrainManager.LeftCam.AddComponent<UnderWaterImageEffect>();
                                        //    }
                                        //    if (TerrainManager.RightCam != null && TerrainManager.RightCam.GetComponent<UnderWaterImageEffect>() == null)
                                        //    {
                                        //        TerrainManager.RightCam.AddComponent<UnderWaterImageEffect>();
                                        //    }
                                        //}
                                    //}
                                }

                                //check if volume script already on reflect camera
                                if (SkyManager.water.GetComponent<PlanarReflectionSM>().m_ReflectionCameraOut != null
                                    && SkyManager.water.GetComponent<PlanarReflectionSM>().m_ReflectionCameraOut.GetComponent<FullVolumeCloudsSkyMaster>() != null)
                                {
                                    //clouds exist already, handle this here
                                    FullVolumeCloudsSkyMaster Rclouds = SkyManager.water.GetComponent<PlanarReflectionSM>().m_ReflectionCameraOut.GetComponent<FullVolumeCloudsSkyMaster>();
                                    CloudsScript.updateReflectionCamera = true;
                                    CloudsScript.reflectClouds = Rclouds;
                                    Debug.Log("Cloud script found on reflection camera, adding auto update based on main clouds system");

                                    CloudsScript.reflectClouds.startDistance = 10000000000; //v4.8.5
                                    CloudsScript.reflectClouds.Sun = CloudsScript.Sun;//v4.8.5
                                }
                                else
                                {
                                    if (SkyManager.water.GetComponent<PlanarReflectionSM>().m_ReflectionCameraOut != null) //v4.8.5
                                    {
                                        CloudsScript.updateReflectionCamera = true;
                                        CloudsScript.updateReflections();

                                        //script.CloudsScript.updateReflectionCamera
                                        CloudsScript.reflectClouds._HorizonYAdjust = -500;
                                        CloudsScript.reflectClouds._FarDist = CloudsScript._FarDist / 2;

                                        //v4.8
                                        CloudsScript.reflectClouds.isForReflections = true;
                                        //remove back layer from refections
                                        //int layerToCheck = LayerMask.NameToLayer("Background");
                                        //backgroundCam.cullingMask = 1 << layerToCheck;
                                        SkyManager.water.GetComponent<PlanarReflectionSM>().m_ReflectionCameraOut.cullingMask &= ~(1 << layerToCheck);
                                        SkyManager.water.GetComponent<PlanarReflectionSM>().reflectionMask &= ~(1 << layerToCheck);

                                        Debug.Log("Created clouds");
                                        
                                        CloudsScript.reflectClouds.startDistance = 10000000000; //v4.8.5
                                        CloudsScript.reflectClouds.Sun = CloudsScript.Sun;//v4.8.5
                                    }
                                    else
                                    {
                                        Debug.Log("No reflection camera in scene, please enable the water and Planer reflection script components.");
                                    }
                                }
                            }
                            else
                            {
                                Debug.Log("Reflection cloud script already setup on reflect camera");

                                //v4.8
                                CloudsScript.updateReflectionCamera = true;
                                CloudsScript.updateReflections();
                                CloudsScript.reflectClouds.isForReflections = true;
                                //remove back layer from refections
                               
                                //backgroundCam.cullingMask = 1 << layerToCheck;
                                SkyManager.water.GetComponent<PlanarReflectionSM>().m_ReflectionCameraOut.cullingMask &= ~(1 << layerToCheck);
                                SkyManager.water.GetComponent<PlanarReflectionSM>().reflectionMask &= ~(1 << layerToCheck);

                                CloudsScript.reflectClouds.startDistance = 10000000000; //v4.8.5
                                CloudsScript.reflectClouds.Sun = CloudsScript.Sun;//v4.8.5
                            }                            
                        }
                        else
                        {
                            Debug.Log("No Water in scene, please add water component first in Water section");
                        }

                        //ADD SHADOWS
                        //FullVolumeCloudsSkyMaster CloudsScript = script.SkyManager.volumeClouds;
                        if (CloudsScript != null && CloudsScript.shadowDome == null)
                        {
                            CloudsScript.setupShadows = true;
                            CloudsScript.createShadowDome();
                            CloudsScript.shadowsUpdate();
                        }
                        else
                        {
                            Debug.Log("Shadows already setup");
                        }

                        //SET BACKLAYER
                        //FullVolumeCloudsSkyMaster CloudsScript = script.SkyManager.volumeClouds;

                        //v4.8.5
                        //var layerToCheck = LayerMask.NameToLayer("Background");
                        if (layerToCheck > -1)
                        {
                            if (CloudsScript != null && CloudsScript.backgroundCam == null)
                            {
                                CloudsScript.setupDepth = true;
                                CloudsScript.createDepthSetup();
                                CloudsScript.setupDepth = true;
                                CloudsScript.blendBackground = true;
                            }
                            else
                            {
                                Debug.Log("Depth camera already setup");
                            }
                        }
                        else
                        {
                            Debug.Log("Please add the Background layer to proceed with the setup");
                        }

                        //LIGHTNING
                        //FullVolumeCloudsSkyMaster CloudsScript = SkyManager.volumeClouds;
                        if (CloudsScript != null && CloudsScript.LightningBox == null)
                        {
                            CloudsScript.setupLightning = true;
                            CloudsScript.createLightningBox();
                            //script.CloudsScript.shadowsUpdate ();
                            CloudsScript.EnableLightning = true;
                            CloudsScript.lightning_every = 5;
                            CloudsScript.max_lightning_time = 9;
                        }
                        else
                        {
                            Debug.Log("Lightning components already setup");
                        }


                    }
                    else
                    {
                        Debug.Log("No Clouds");
                    }


                    //VOLUME FOG
                    SeasonalTerrainSKYMASTER TerrainManager = SkyManager.gameObject.GetComponent<SeasonalTerrainSKYMASTER>();
                    //if (GUILayout.Button(new GUIContent("Add volumetric fog"), GUILayout.Width(150)))
                    //{
                        if (Camera.main != null && Camera.main.gameObject.GetComponent<GlobalFogSkyMaster>() == null)
                        {
                            Camera.main.gameObject.AddComponent<GlobalFogSkyMaster>();
                            Camera.main.gameObject.GetComponent<GlobalFogSkyMaster>().SkyManager = SkyManager;
                            Camera.main.gameObject.GetComponent<GlobalFogSkyMaster>().Sun = SkyManager.SUN_LIGHT.transform;
                            //Debug.Log("CAMERA FOUND");
                            TerrainManager.Lerp_gradient = true;
                            TerrainManager.ImageEffectFog = true;
                            TerrainManager.FogHeightByTerrain = true;
                        }
                        else
                        {
                            if (Camera.main == null)
                            {
                                Debug.Log("Add a main camera first");
                            }
                            if (Camera.main.gameObject.GetComponent<GlobalFogSkyMaster>() != null)
                            {
                                //setup existing
                                Camera.main.gameObject.GetComponent<GlobalFogSkyMaster>().SkyManager = SkyManager;
                                Camera.main.gameObject.GetComponent<GlobalFogSkyMaster>().Sun = SkyManager.SUN_LIGHT.transform;
                                TerrainManager.Lerp_gradient = true;
                                TerrainManager.ImageEffectFog = true;
                                TerrainManager.FogHeightByTerrain = true;
                            }
                        }
                        //v4.8
                        TerrainManager.setVFogCurvesPresetE();
                    //}
                    //END FOG

                    //SHAFTS
                    if (Camera.main != null && Camera.main.gameObject.GetComponent<SunShaftsSkyMaster>() == null)
                    {
                        Camera.main.gameObject.AddComponent<SunShaftsSkyMaster>();
                        Camera.main.gameObject.GetComponent<SunShaftsSkyMaster>().sunTransform = SkyManager.SunObj.transform;
                        //Debug.Log("CAMERA FOUND");
                        TerrainManager.ImageEffectShafts = true;
                    }
                    else
                    {
                        if (Camera.main == null)
                        {
                            Debug.Log("Add a main camera first");
                        }
                        if (Camera.main.gameObject.GetComponent<SunShaftsSkyMaster>() != null)
                        {
                            Camera.main.gameObject.GetComponent<SunShaftsSkyMaster>().sunTransform = SkyManager.SunObj.transform;
                            TerrainManager.ImageEffectShafts = true;
                        }
                    }
                    //END SHAFTS
                }
                else //find if camera in scene
                {

                }

                configured = true;
                this.enabled = false;
            }

        }
    }
}