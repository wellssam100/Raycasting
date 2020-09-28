Shader "Unlit/RayMarch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            #define MAX_STEPS 100
            #define MAX_DIST 100
            #define SURF_DIST .001

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            //data structure that gets sent from the vertex shadder to fragment shader
            //vertex to fragment = v2f
    // type of data to send name of var : tell gpu which register to use to store data
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                
                float3 ro : TEXTCOORD1;
                float3 hitPos : TEXTCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;


            //Vertex Shader
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //use mul and unity_WorldtoObject or unity_ObjectToWorld to switch between those representations
                //worldspace camera pos is 3d but it needs to be 4d or the wrong translation will occur
                //TODO: What is unity_WorldToObject?
                o.ro = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1)); //originally in in world space

                o.hitPos = v.vertex; //in object space(local)
                return o;
            }

            float GetDist(float3 p) 
            {
                //building a sphere
                float r = .29;
                float d = length(p)-r;
                //tarus
                d =length(float2( length(p.xz) - r,p.y))-.2;

                return d;
            }

            float3 GetNormal(float3 p) 
            {
                //e is epsilon, to check the space directly around p
                float2 e = float2 (.01, 0);
                float3 n = GetDist(p) - float3(
                        GetDist(p - e.xyy),
                        GetDist(p - e.yxy),
                        GetDist(p - e.yyx)
                    );
                return normalize(n);
            }

            float Raymarch(float3 ro, float3 rd)
            {
                //distance from the origin that has been marched
                //start at camera position and march till scene
                float dO = 0; 
                //distance from scene (distance from surface)
                float dS;
                for (int i = 0; i < MAX_STEPS; i++) 
                {
                    //moving along the ray rd from rO in steps of dO
                    float3 p = ro + dO * rd;
                    dS = GetDist(p);
                    dO += dS;
                    if (dS<SURF_DIST || dO>MAX_DIST) 
                    {
                        break;
                    }
                }
                return dO;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                //uv shows us how the texture is mapped to the object
                //the subtractyion is to move the origin of this mapping to the center
                float2 uv = i.uv-.5;
                //ro = ray origin of camera
                float3 ro = i.ro;
                //rd = raydirection 
                float3 rd = normalize(i.hitPos-ro);
                float d = Raymarch(ro, rd);



                fixed4 col = tex2D(_MainTex, i.uv);

                //if hit
                if (d < MAX_DIST)
                {
                    float3 p = ro + rd * d;
                    float3 n = GetNormal(p);
                    col.rgb = n;
                }
                //if not
               // else 
               //{
                    //tells teh shader not to render the pixel
                //    discard;
                //}
                return col;
            }
            ENDCG
        }
    }
}
