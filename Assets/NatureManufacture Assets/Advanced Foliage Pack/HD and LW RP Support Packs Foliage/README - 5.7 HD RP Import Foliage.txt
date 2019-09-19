BEFORE YOU START:
- you need Unity 2019.1 
- you need HD SRP pipline 5.7, if you use higher it could be broken but it doesn't have to. We use HD RP basic shaders so should be fine anyway.
- wind setup is gone in 5.7 but materials work. It will back just be patient.
- there is wind prefab which will manage wind at your scene
Be patient this tech is so fluid... we coudn't fallow ever beta version
Make note that hd rp doesnt have grass system at terrain yet at all so demo with terrain is not possible.

Snow on grass at hd rp will be supported in the future.

Step 1 - Import our HD RP 5.7 Unity 2019.1 Advanced Foliage Pack compatibility pack.
Step 2 - Setup Shadows and other render setups.

Find File "HDRenderPipelineAsset" and find Material section, drag and drop our SSS settings diffusion profiles for foliage into Diffusion profile list:
NM_SSSSettings_Skin_Foliage
Without this foliage materials will not become affected by scattering.


