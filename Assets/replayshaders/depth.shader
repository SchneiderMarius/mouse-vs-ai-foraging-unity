Shader "Unlit/depth"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _DepthFactor("Depth factor", float) = 1.0
        _DepthPow("Depth Pow", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"


            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos: TEXCOORD1;
            };

            float4 _Color;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float _DepthFactor;
            float _DepthPow;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                //compute depth
                o.screenPos = ComputeScreenPos(o.vertex);
                //COMPUTE_EYEDEPTH(o.screenPos.z);

                return o;
            }

            //fixed4 frag (v2f i) : SV_Target
            //{
            //    fixed4 col = _Color;

            //    //compute depth
            //    float sceneZ = Linear01Depth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
            //    //float depth = sceneZ - i.screenPos.z;

            //    //modify color based on depth
            //    fixed depthFading = saturate((abs(pow(depth, _DepthPow))) / _DepthFactor);
            //    col *= depthFading;

            //    return col;
            //}

            half4 frag(v2f i) : SV_Target{

                //depth output of Linear01Depth is the linear distance from camera to the camera clipping plane.
                //clipping plane = 3000, which is too far for linear encoding. Everything close too black. 
                //Multiplying and saturating by some factor essentially scales the depth and clipping plane values together.

                //Set the _DepthFactor to 3000/x, where x is the maximum distance encoded. Values from 0 to 1 are scaled linearly with distance to that point.

                //return saturate(Linear01Depth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos))) * _DepthFactor);

                
                //linear eye depth reports distance (in game meters) to vertex from camera
                //logging this transforms to ln(distance) which is what we want
                //this function must return values from 0,1.
                //setting _DepthFactor to ln(3000)=0.125 normalizes the output to the maximum distance rendered by the camera.
                //(a little shorter than this distance is desirable because the farthest objects rendered don't extend to 3000. So this is decreased some.)

                return saturate(log(LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)))) * _DepthFactor);
            }
            ENDCG
        }
    }
}
