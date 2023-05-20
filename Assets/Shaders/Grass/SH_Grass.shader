Shader "Unlit/SH_Grass"
{
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

			StructuredBuffer<MeshProperties> _Properties;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v, uint instanceID: SV_InstanceID)
            {
                v2f o;
				float height = v.vertex.y;
				float4 pos = mul(_Properties[instanceID].mat, v.vertex);
				pos.x += sin(_Time * 10) * height;
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
