Shader "Unlit/BoardShader"
{
    Properties
    {
        _CellsX("# of Cells X", Float) = 9.0
        _CellsY("# of Cells Y", Float) = 9.0
        _CellColor("Cell Color", Color) = (0.5, 0.5, 0.5)
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #define M_PI 3.1415926535897932384626433832795

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            // sampler2D _MainTex;
            // float4 _MainTex_ST;
            float _CellsX;
            float _CellsY;
            float4 _CellColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }


            float grid(float x, float cells, float weight) {
                return smoothstep(weight, 0.01 + weight, abs(sin(cells*x*M_PI)));
            }

            float Dot(float2 uv, float x, float y) {
                return smoothstep(.009, .0, length(uv - float2(x, y)) - 0.02);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                // fixed4 col = tex2D(_MainTex, i.uv);
                // float2 uv = (i.uv - (iResolution.xy - vec2(dim) )* .5)/dim;
                float2 uv = i.uv;

                float k = grid(uv.x, _CellsX, 0.09) * grid(uv.y, _CellsY, 0.09);
                float3 col = _CellColor * float3(k,k,k);
                
                col *= grid(uv.x, (_CellsX/ 3), 0.06) * grid(uv.y, (_CellsY/3), 0.06);
                
                col -= smoothstep(.5, 0.501, abs(uv.x - .5));

                // Pretty cool ball
                // float iTime = _Time.y;
                // float dot = Dot(uv, (fmod(iTime, 1.)),  (fmod(iTime, 1.)));
                // col = lerp(col, float3(1.,0.,0.), dot);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col.xyzx;
            }
            ENDCG
        }
    }
}
