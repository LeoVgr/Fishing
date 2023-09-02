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

        _TopColor ("TopColor", Color) = (1,1,1,1)
        _BotColor ("BotColor", Color) = (1,1,1,1)

        _PlayerPos ("PlayerPos", Vector) = (0,0,0,0)
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
                float4 texCoord : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 texCoord : TEXCOORD0;
            };

			struct MeshProperties 
			{
				float4x4 mat;
			};

            float _WindFrequency;
            float _WindAmplitude;
            float _WindSinAmplitude;
            float _WindSinFrequency;
            float _WindHeightFactor;
            fixed4 _TopColor;
            fixed4 _BotColor;
            sampler2D _WindNoiseTexture;
			StructuredBuffer<MeshProperties> _Properties;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _PlayerPos;

            v2f vert (appdata v, uint instanceID: SV_InstanceID)
            {
                v2f o;

                // Register the height in object space to know which height is the vertex
                // (used to not move bottom vertex in wind animation)
				float height = v.vertex.y;

                // Wind animation
				float4 pos = mul(_Properties[instanceID].mat, v.vertex);
                float3 bendVector = float3(pos.x,0,pos.z) - float3(_PlayerPos.x,0,_PlayerPos.z);
                float distFromPlayer = length(bendVector);
                float finalBend = 2 * clamp(3 - distFromPlayer ,0, distFromPlayer);
                
                float freq = _Time * _WindFrequency;
                float posX = _Properties[instanceID].mat[0][0] * freq;
                float posZ = _Properties[instanceID].mat[0][2] * freq;
                float4 tex = tex2Dlod (_WindNoiseTexture, float4(posX,posZ,0,0));
				pos.xz += (height * pow(2,_WindHeightFactor)) * ((tex.r+tex.g+tex.b) * _WindAmplitude);
				pos.xz += height * sin((_Properties[instanceID].mat[0][0] + _Properties[instanceID].mat[0][2]) + _Time * _WindSinFrequency) * _WindSinAmplitude;
                //pos.xz += bendVector * finalBend * height;
                pos.y *= (clamp(distFromPlayer, 0, 3) / 3);

                o.vertex = UnityObjectToClipPos(pos);
                o.texCoord = v.texCoord;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = lerp(_BotColor, _TopColor, i.texCoord.y);
                col.a = 1;
                return col;
            }
            ENDCG
        }
    }
}
