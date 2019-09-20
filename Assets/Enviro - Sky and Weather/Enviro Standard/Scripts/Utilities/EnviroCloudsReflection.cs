using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class EnviroCloudsReflection : MonoBehaviour {
#if ENVIRO_HD
    public bool resetCameraProjection = true;
    public bool tonemapping = true;
    public EnviroVolumeCloudsQualitySettings.ReprojectionPixelSize reprojectionResolution = EnviroVolumeCloudsQualitySettings.ReprojectionPixelSize.Medium;
    public Camera myCam;

    // Volume Clouds
    private Material mat;
    private Material blitMat;
    private Material weatherMapMat;
    private RenderTexture subFrameTex;
    private RenderTexture prevFrameTex;
    private Texture2D curlMap;
    private Texture2D blueNoise;
    private Texture3D noiseTexture = null;
    private Texture3D noiseTextureHigh = null;
    private Texture3D detailNoiseTexture = null;
    private Texture3D detailNoiseTextureHigh = null;

    //Cloud Rendering Matrix
    private Matrix4x4 projection;
    private Matrix4x4 projectionSPVR;
    private Matrix4x4 inverseRotation;
    private Matrix4x4 inverseRotationSPVR;
    private Matrix4x4 rotation;
    private Matrix4x4 rotationSPVR;
    private Matrix4x4 previousRotation;
    private Matrix4x4 previousRotationSPVR;
    [HideInInspector]
    public EnviroVolumeCloudsQualitySettings.ReprojectionPixelSize currentReprojectionPixelSize;
    private int reprojectionPixelSize;

    private bool isFirstFrame;

    private int subFrameNumber;
    private int[] frameList;
    private int renderingCounter;
    private int subFrameWidth;
    private int subFrameHeight;
    private int frameWidth;
    private int frameHeight;
    private bool textureDimensionChanged;

     
    void OnEnable ()
    {
        myCam = GetComponent<Camera>();
        CreateMaterialsAndTextures();
        SetReprojectionPixelSize(reprojectionResolution);
        currentReprojectionPixelSize = reprojectionResolution;
    }

    private void CreateMaterialsAndTextures()
    {
        if (mat == null)
            mat = new Material(Shader.Find("Enviro/Standard/RaymarchClouds"));

        if (blitMat == null)
            blitMat = new Material(Shader.Find("Enviro/Standard/Blit"));

        if (curlMap == null)
            curlMap = Resources.Load("tex_enviro_curl") as Texture2D;

        if (noiseTexture == null)
            noiseTexture = Resources.Load("enviro_clouds_base_low") as Texture3D;

        if (noiseTextureHigh == null)
            noiseTextureHigh = Resources.Load("enviro_clouds_base") as Texture3D;

        if (detailNoiseTexture == null)
            detailNoiseTexture = Resources.Load("enviro_clouds_detail_low") as Texture3D;

        if (detailNoiseTextureHigh == null)
            detailNoiseTextureHigh = Resources.Load("enviro_clouds_detail_high") as Texture3D;

        if (blueNoise == null)
            blueNoise = Resources.Load("tex_enviro_blueNoise", typeof(Texture2D)) as Texture2D;
    }

    private void SetCloudProperties()
    {
        if (mat == null)
            mat = new Material(Shader.Find("Enviro/Standard/RaymarchClouds"));

        mat.SetTexture("_WeatherMap", EnviroSky.instance.weatherMap);
        mat.SetTexture("_Noise", noiseTextureHigh);
        mat.SetTexture("_NoiseLow", noiseTexture);

        if (EnviroSky.instance.cloudsSettings.cloudsQualitySettings.detailQuality == EnviroVolumeCloudsQualitySettings.CloudDetailQuality.Low)
            mat.SetTexture("_DetailNoise", detailNoiseTexture);
        else
            mat.SetTexture("_DetailNoise", detailNoiseTextureHigh);

        switch (myCam.stereoActiveEye)
        {
            case Camera.MonoOrStereoscopicEye.Mono:
                projection = myCam.projectionMatrix;
                Matrix4x4 inverseProjection = projection.inverse;
                mat.SetMatrix("_InverseProjection", inverseProjection);
                inverseRotation = myCam.cameraToWorldMatrix;
                mat.SetMatrix("_InverseRotation", inverseRotation);
                break;

            case Camera.MonoOrStereoscopicEye.Left:
                projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                Matrix4x4 inverseProjectionLeft = projection.inverse;
                mat.SetMatrix("_InverseProjection", inverseProjectionLeft);
                inverseRotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
                mat.SetMatrix("_InverseRotation", inverseRotation);

                if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
                {
                    Matrix4x4 inverseProjectionRightSP = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right).inverse;
                    mat.SetMatrix("_InverseProjection_SP", inverseProjectionRightSP);

                    inverseRotationSPVR = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
                    mat.SetMatrix("_InverseRotation_SP", inverseRotationSPVR);
                }
                break;

            case Camera.MonoOrStereoscopicEye.Right:
                projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                Matrix4x4 inverseProjectionRight = projection.inverse;
                mat.SetMatrix("_InverseProjection", inverseProjectionRight);
                inverseRotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
                mat.SetMatrix("_InverseRotation", inverseRotation);
                break;
        }

        if (EnviroSky.instance.cloudsSettings.customWeatherMap == null)
            mat.SetTexture("_WeatherMap", EnviroSky.instance.weatherMap);
        else
            mat.SetTexture("_WeatherMap", EnviroSky.instance.cloudsSettings.customWeatherMap);

        mat.SetTexture("_CurlNoise", curlMap);

        mat.SetVector("_Steps", new Vector4(EnviroSky.instance.cloudsSettings.cloudsQualitySettings.raymarchSteps * EnviroSky.instance.cloudsConfig.raymarchingScale, EnviroSky.instance.cloudsSettings.cloudsQualitySettings.raymarchSteps * EnviroSky.instance.cloudsConfig.raymarchingScale, 0.0f, 0.0f));
        mat.SetFloat("_BaseNoiseUV", EnviroSky.instance.cloudsSettings.cloudsQualitySettings.baseNoiseUV);
        mat.SetFloat("_DetailNoiseUV", EnviroSky.instance.cloudsSettings.cloudsQualitySettings.detailNoiseUV);
        mat.SetFloat("_AmbientSkyColorIntensity", EnviroSky.instance.cloudsSettings.ambientLightIntensity.Evaluate(EnviroSky.instance.GameTime.solarTime));
        mat.SetVector("_CloudsLighting", new Vector4(EnviroSky.instance.cloudsConfig.scatteringCoef, EnviroSky.instance.cloudsSettings.hgPhase, EnviroSky.instance.cloudsSettings.silverLiningIntensity, EnviroSky.instance.cloudsSettings.silverLiningSpread));

        float toneMap = tonemapping ? 0f : 1f;

        if (!Application.isPlaying && EnviroSky.instance.showVolumeCloudsInEditor)
            toneMap = 0f;

        mat.SetVector("_CloudsLightingExtended", new Vector4(EnviroSky.instance.cloudsConfig.edgeDarkness, EnviroSky.instance.cloudsConfig.ambientSkyColorIntensity, toneMap, EnviroSky.instance.cloudsSettings.cloudsExposure));

        float bottomH = EnviroSky.instance.cloudsSettings.cloudsQualitySettings.bottomCloudHeight + EnviroSky.instance.cloudsSettings.cloudsHeightMod;
        float topH = EnviroSky.instance.cloudsSettings.cloudsQualitySettings.topCloudHeight + EnviroSky.instance.cloudsSettings.cloudsHeightMod;

        if (myCam.transform.position.y < bottomH - 250)
            mat.SetVector("_CloudsParameter", new Vector4(bottomH, topH, topH - bottomH, EnviroSky.instance.cloudsSettings.cloudsWorldScale * 10));
        else
            mat.SetVector("_CloudsParameter", new Vector4(myCam.transform.position.y + 250, topH + ((myCam.transform.position.y + 250) - bottomH), (topH + ((myCam.transform.position.y + 250) - bottomH)) - (myCam.transform.position.y + 250), EnviroSky.instance.cloudsSettings.cloudsWorldScale * 10));

        mat.SetVector("_CloudDensityScale", new Vector4(EnviroSky.instance.cloudsConfig.density, EnviroSky.instance.cloudsConfig.densityLightning, 0f, 0f));
        mat.SetFloat("_CloudsType", EnviroSky.instance.cloudsConfig.cloudType);
        mat.SetVector("_CloudsCoverageSettings", new Vector4(EnviroSky.instance.cloudsConfig.coverage * EnviroSky.instance.cloudsSettings.globalCloudCoverage, EnviroSky.instance.cloudsConfig.coverageModBottom, EnviroSky.instance.cloudsConfig.coverageModTop, 0f));
        mat.SetVector("_CloudsAnimation", new Vector4(EnviroSky.instance.cloudAnim.x, EnviroSky.instance.cloudAnim.y, EnviroSky.instance.cloudsSettings.cloudsWindDirectionX, EnviroSky.instance.cloudsSettings.cloudsWindDirectionY));
        mat.SetColor("_LightColor", EnviroSky.instance.cloudsSettings.volumeCloudsColor.Evaluate(EnviroSky.instance.GameTime.solarTime));
        mat.SetColor("_MoonLightColor", EnviroSky.instance.cloudsSettings.volumeCloudsMoonColor.Evaluate(EnviroSky.instance.GameTime.lunarTime));
        mat.SetFloat("_stepsInDepth", EnviroSky.instance.cloudsSettings.cloudsQualitySettings.stepsInDepthModificator);
        mat.SetFloat("_LODDistance", EnviroSky.instance.cloudsSettings.cloudsQualitySettings.lodDistance);
        mat.SetVector("_LightDir", -EnviroSky.instance.Components.DirectLight.transform.forward);
        mat.SetFloat("_LightIntensity", EnviroSky.instance.cloudsSettings.lightIntensity.Evaluate(EnviroSky.instance.GameTime.solarTime));

        mat.SetVector("_CloudsErosionIntensity", new Vector4(1f - EnviroSky.instance.cloudsConfig.baseErosionIntensity, EnviroSky.instance.cloudsConfig.detailErosionIntensity, 0f, 0f));
        mat.SetTexture("_BlueNoise", blueNoise);
        mat.SetVector("_Randomness", new Vector4(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value));
    }
     
    public void SetBlitmaterialProperties()
    {
        Matrix4x4 inverseProjection = projection.inverse;

        blitMat.SetMatrix("_PreviousRotation", previousRotation);
        blitMat.SetMatrix("_Projection", projection);
        blitMat.SetMatrix("_InverseRotation", inverseRotation);
        blitMat.SetMatrix("_InverseProjection", inverseProjection);

        if (EnviroSky.instance.singlePassVR)
        {
            Matrix4x4 inverseProjectionSPVR = projectionSPVR.inverse;
            blitMat.SetMatrix("_PreviousRotationSPVR", previousRotationSPVR);
            blitMat.SetMatrix("_ProjectionSPVR", projectionSPVR);
            blitMat.SetMatrix("_InverseRotationSPVR", inverseRotationSPVR);
            blitMat.SetMatrix("_InverseProjectionSPVR", inverseProjectionSPVR);
        }

        blitMat.SetFloat("_FrameNumber", subFrameNumber);
        blitMat.SetFloat("_ReprojectionPixelSize", reprojectionPixelSize);
        blitMat.SetVector("_SubFrameDimension", new Vector2(subFrameWidth, subFrameHeight));
        blitMat.SetVector("_FrameDimension", new Vector2(frameWidth, frameHeight));
    }

    public void RenderClouds(RenderTexture src, RenderTexture tex)
    {
        SetCloudProperties();
        mat.SetTexture("_MainTex", src);
        Graphics.Blit(src, tex, mat);
    }

    private void CreateCloudsRenderTextures(RenderTexture source)
    {
        if (subFrameTex != null)
        {
            DestroyImmediate(subFrameTex);
            subFrameTex = null;
        }

        if (prevFrameTex != null)
        {
            DestroyImmediate(prevFrameTex);
            prevFrameTex = null;
        }

        RenderTextureFormat format = myCam.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;



        if (subFrameTex == null)
        {

#if UNITY_2017_1_OR_NEWER
            RenderTextureDescriptor desc = new RenderTextureDescriptor(subFrameWidth, subFrameHeight, format, 0);
            if (EnviroSky.instance.singlePassVR)
                desc.vrUsage = VRTextureUsage.TwoEyes;
            subFrameTex = new RenderTexture(desc);
#else
            subFrameTex = new RenderTexture(subFrameWidth, subFrameHeight, 0, format);
#endif
            subFrameTex.filterMode = FilterMode.Bilinear;
            subFrameTex.hideFlags = HideFlags.HideAndDontSave;

            isFirstFrame = true;
        }

        if (prevFrameTex == null)
        {


#if UNITY_2017_1_OR_NEWER
            RenderTextureDescriptor desc = new RenderTextureDescriptor(frameWidth, frameHeight, format, 0);
            if (EnviroSky.instance.singlePassVR)
                desc.vrUsage = VRTextureUsage.TwoEyes;
            prevFrameTex = new RenderTexture(desc);
#else
            prevFrameTex = new RenderTexture(frameWidth, frameHeight, 0, format);
#endif

            prevFrameTex.filterMode = FilterMode.Bilinear;
            prevFrameTex.hideFlags = HideFlags.HideAndDontSave;

            isFirstFrame = true;
        }
    }

    void Update ()
    {
        if (EnviroSky.instance == null)
            return;

        if (currentReprojectionPixelSize != reprojectionResolution)
        {
            currentReprojectionPixelSize = reprojectionResolution;
            SetReprojectionPixelSize(reprojectionResolution);
        }    
    }

    [ImageEffectOpaque]
    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (EnviroSky.instance == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        if (EnviroSky.instance.useVolumeClouds)
        {
            StartFrame();
             
            if (subFrameTex == null || prevFrameTex == null || textureDimensionChanged)
                CreateCloudsRenderTextures(source);

            //RenderingClouds
            RenderClouds(source, subFrameTex);

            if (isFirstFrame)
            {
                Graphics.Blit(subFrameTex, prevFrameTex);
                isFirstFrame = false;
            }

            //Blit clouds to final image
            blitMat.SetTexture("_MainTex", source);
            blitMat.SetTexture("_SubFrame", subFrameTex);
            blitMat.SetTexture("_PrevFrame", prevFrameTex);
            SetBlitmaterialProperties();

            Graphics.Blit(source, destination, blitMat);
            Graphics.Blit(subFrameTex, prevFrameTex);

            FinalizeFrame();
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    public void SetReprojectionPixelSize(EnviroVolumeCloudsQualitySettings.ReprojectionPixelSize pSize)
    {
        switch (pSize)
        {
            case EnviroVolumeCloudsQualitySettings.ReprojectionPixelSize.Off:
                reprojectionPixelSize = 1;
                break;

            case EnviroVolumeCloudsQualitySettings.ReprojectionPixelSize.Low:
                reprojectionPixelSize = 2;
                break;

            case EnviroVolumeCloudsQualitySettings.ReprojectionPixelSize.Medium:
                reprojectionPixelSize = 4;
                break;

            case EnviroVolumeCloudsQualitySettings.ReprojectionPixelSize.High:
                reprojectionPixelSize = 8;
                break;
        }

        frameList = CalculateFrames(reprojectionPixelSize);
    }
    public void StartFrame()
    {
        textureDimensionChanged = UpdateFrameDimensions();

        switch (myCam.stereoActiveEye)
        {
            case Camera.MonoOrStereoscopicEye.Mono:

                if (resetCameraProjection)
                    myCam.ResetProjectionMatrix();

                projection = myCam.projectionMatrix;
                rotation = myCam.worldToCameraMatrix;
                inverseRotation = myCam.cameraToWorldMatrix;
                break;

            case Camera.MonoOrStereoscopicEye.Left:
                projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                rotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
                inverseRotation = rotation.inverse;

                if (EnviroSky.instance.singlePassVR)
                {
                    projectionSPVR = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                    rotationSPVR = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
                    inverseRotationSPVR = rotationSPVR.inverse;
                }
                break;

            case Camera.MonoOrStereoscopicEye.Right:
                projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                rotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
                inverseRotation = rotation.inverse;
                break;
        }
    }
    public void FinalizeFrame()
    {
        renderingCounter++;

        previousRotation = rotation;
        if (EnviroSky.instance.singlePassVR)
            previousRotationSPVR = rotationSPVR;

        int reproSize = reprojectionPixelSize * reprojectionPixelSize;
        subFrameNumber = frameList[renderingCounter % reproSize];
    }
    private bool UpdateFrameDimensions()
    {
        //Add downsampling
        int newFrameWidth = myCam.pixelWidth / EnviroSky.instance.cloudsSettings.cloudsQualitySettings.cloudsRenderResolution;
        int newFrameHeight = myCam.pixelHeight / EnviroSky.instance.cloudsSettings.cloudsQualitySettings.cloudsRenderResolution;

        //Calculate new frame width and height
        while (newFrameWidth % reprojectionPixelSize != 0)
        {
            newFrameWidth++;
        }

        while (newFrameHeight % reprojectionPixelSize != 0)
        {
            newFrameHeight++;
        }

        int newSubFrameWidth = newFrameWidth / reprojectionPixelSize;
        int newSubFrameHeight = newFrameHeight / reprojectionPixelSize;

        //Check if diemensions changed
        if (newFrameWidth != frameWidth || newSubFrameWidth != subFrameWidth || newFrameHeight != frameHeight || newSubFrameHeight != subFrameHeight)
        {
            //Cache new dimensions
            frameWidth = newFrameWidth;
            frameHeight = newFrameHeight;
            subFrameWidth = newSubFrameWidth;
            subFrameHeight = newSubFrameHeight;
            return true;
        }
        else
        {
            //Cache new dimensions
            frameWidth = newFrameWidth;
            frameHeight = newFrameHeight;
            subFrameWidth = newSubFrameWidth;
            subFrameHeight = newSubFrameHeight;
            return false;
        }
    }
    private int[] CalculateFrames(int reproSize)
    {
        subFrameNumber = 0;

        int i = 0;
        int reproCount = reproSize * reproSize;
        int[] frameNumbers = new int[reproCount];

        for (i = 0; i < reproCount; i++)
        {
            frameNumbers[i] = i;
        }

        while (i-- > 0)
        {
            int frame = frameNumbers[i];
            int count = (int)(UnityEngine.Random.Range(0, 1) * 1000.0f) % reproCount;
            frameNumbers[i] = frameNumbers[count];
            frameNumbers[count] = frame;
        }

        return frameNumbers;
    }

#endif
}
