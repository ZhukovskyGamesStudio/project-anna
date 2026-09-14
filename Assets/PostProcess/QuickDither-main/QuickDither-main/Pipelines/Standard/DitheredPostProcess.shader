// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Dithered/DitheredPostProcess"
{
	Properties
	{
		_Primary("Primary", 3D) = "white" {}
		_Secondary("Secondary", 3D) = "white" {}
		_NoiseIntensity("Noise Intensity", Range( 0 , 1)) = 0
		[IntRange]_PixelScale("Pixel Scale", Range( 1 , 32)) = 3
		_Pattern("Pattern", 2D) = "white" {}
		_Intensity("Intensity", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}

	SubShader
	{
		LOD 0

		Cull Off
		ZWrite Off
		ZTest Always
		
		Pass
		{
			CGPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			
			struct ASEAttributesDefault
			{
				float3 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct ASEVaryingsDefault
			{
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float2 texcoordStereo : TEXCOORD1;
			#if STEREO_INSTANCING_ENABLED
				uint stereoTargetEyeIndex : SV_RenderTargetArrayIndex;
			#endif
			};

			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform half4 _MainTex_ST;
			
			uniform sampler3D _Primary;
			SamplerState sampler_Primary;
			uniform float _PixelScale;
			uniform float _NoiseIntensity;
			uniform sampler2D _Pattern;
			float4 _Pattern_TexelSize;
			uniform sampler3D _Secondary;
			uniform float _Intensity;

			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}

			float2 TransformTriangleVertexToUV (float2 vertex)
			{
				float2 uv = (vertex + 1.0) * 0.5;
				return uv;
			}

			ASEVaryingsDefault Vert( ASEAttributesDefault v  )
			{
				ASEVaryingsDefault o;
				o.vertex = UnityObjectToClipPos( v.vertex );
				o.texcoord = v.texcoord;

			#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0.0)
				{
					o.texcoord.y = 1.0 - o.texcoord.y;
				}
			#endif

				o.texcoordStereo = o.texcoord;

				return o;
			}

			float4 Frag (ASEVaryingsDefault i  ) : SV_Target
			{
				float4 ase_ppsScreenPosFragNorm = float4(i.texcoordStereo,0,1);

				float2 uv_MainTex = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float pixelWidth99 =  1.0f / ( _MainTex_TexelSize.z / _PixelScale );
				float pixelHeight99 = 1.0f / ( _MainTex_TexelSize.w / _PixelScale );
				half2 pixelateduv99 = half2((int)(uv_MainTex.x / pixelWidth99) * pixelWidth99, (int)(uv_MainTex.y / pixelHeight99) * pixelHeight99);
				float2 ifLocalVar150 = 0;
				if( _PixelScale == 1.0 )
				ifLocalVar150 = uv_MainTex;
				else
				ifLocalVar150 = pixelateduv99;
				float4 tex2DNode16 = tex2D( _MainTex, ifLocalVar150 );
				float4 tex3DNode21 = tex3D( _Primary, tex2DNode16.rgb );
				float simplePerlin2D55 = snoise( ifLocalVar150*400.0 );
				simplePerlin2D55 = simplePerlin2D55*0.5 + 0.5;
				float2 appendResult125 = (float2(_MainTex_TexelSize.z , _MainTex_TexelSize.w));
				float2 appendResult137 = (float2(_Pattern_TexelSize.z , _Pattern_TexelSize.w));
				float4 ifLocalVar78 = 0;
				if( ( ( 1.0 - tex3DNode21.a ) + ( ( simplePerlin2D55 - 0.5 ) * _NoiseIntensity * 0.5 ) ) >= tex2D( _Pattern, ( ( uv_MainTex * appendResult125 ) / ( appendResult137 * _PixelScale ) ) ).r )
				ifLocalVar78 = tex3DNode21;
				else
				ifLocalVar78 = tex3D( _Secondary, tex2DNode16.rgb );
				float4 lerpResult143 = lerp( tex2D( _MainTex, uv_MainTex ) , ifLocalVar78 , _Intensity);
				float4 appendResult149 = (float4(lerpResult143.rgb , tex2DNode16.a));

				float4 color = appendResult149;
				
				return color;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
}