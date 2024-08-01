Shader "Custom/contrastmod"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ModTex ("Texture", 2DArray) = "" {}
        _NumSlices ("Slices in TextureArray", Int) = 1
        _FPS ("Texture frames per second", Int) = 1
        _xscale ("x scale factor (mod width/screen width)", float) = 1
        _yscale ("y scale factor", float) = 1

    }
    SubShader
    {

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma require 2darray
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            UNITY_DECLARE_TEX2DARRAY(_ModTex);
            float3 mod_uv;
            int _NumSlices;
            int _FPS;
            float _xscale;
            float _yscale;

            fixed4 frag (v2f_img input) : COLOR
            {
                float slice = floor((_Time[1] * _FPS) % _NumSlices);
                mod_uv = float3(input.uv.x * _xscale, input.uv.y * _yscale, slice);
                
                fixed4 image = tex2D(_MainTex, input.uv);
                fixed4 mod = UNITY_SAMPLE_TEX2DARRAY(_ModTex, mod_uv);

                mod = mod - 0.5;
                mod = mod * image;
                mod = mod + 0.5;

                return mod;
            }


            ENDCG
        }
    }
}
