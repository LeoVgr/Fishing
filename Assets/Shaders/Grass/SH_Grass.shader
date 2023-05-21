Shader "Unlit/SH_Grass"
{
    Properties
    {
        _WindFrequency ("WindFrequency", Float) = 0.5
        _WindAmplitude ("WindAmplitude", Float) = 0.5
        _WindSinFrequency ("WindSinFrequency", Float) = 0.5
        _WindSinAmplitude ("WindSinAmplitude", Float) = 0.5
        _WindHeightFactor ("WindHeightFactor", Float) = 0.5
        _WindNoiseTexture ("WindTexture", 2D) = "white"{}
    }
    SubShader
    {
        Tags {"RenderType"="Opaque"}
        LOD 200
		Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				fixed4 color : COLOR;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
            };

			struct MeshProperties 
			{
				float4x4 mat;
				float4 color;
			};

            float _WindFrequency;
            float _WindAmplitude;
            float _WindSinAmplitude;
            float _WindSinFrequency;
            float _WindHeightFactor;
            sampler2D _WindNoiseTexture;
			StructuredBuffer<MeshProperties> _Properties;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v, uint instanceID: SV_InstanceID)
            {
                v2f o;

                // Register the height in object space to know which height is the vertex
                // (used to not move bottom vertex in wind animation)
				float height = v.vertex.y;

                // Wind animation
				float4 pos = mul(_Properties[instanceID].mat, v.vertex);
                
                float posX = _Properties[instanceID].mat[0][0] * _Time * _WindFrequency;
                float posZ = _Properties[instanceID].mat[0][2] * _Time * _WindFrequency;
                float4 tex = tex2Dlod (_WindNoiseTexture, float4(posX,posZ,0,0));
				pos.xz += (height * pow(2,_WindHeightFactor)) * ((tex.r+tex.g+tex.b) * _WindAmplitude);
				pos.xz += height * sin((_Properties[instanceID].mat[0][0] + _Properties[instanceID].mat[0][2]) + _Time * _WindSinFrequency) * _WindSinAmplitude;

                o.vertex = UnityObjectToClipPos(pos);
                o.color = _Properties[instanceID].color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = i.color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
