Shader "Unlit/FpsMeter"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_CurrentFPS("Digits", float) = 0.0

		_highFpsColor("High Fps", Color) = (0.2, 1.0, 0.2, 0.5)
		_midFpsColor("Mid Fps", Color) = (1.0, 1.0, 0.2, 0.5)
		_lowFpsColor("Low Fps", Color) = (1.0, 0.2, 0.2, 0.5)

		// _FpsRect: this refers to the portion of the quad
		// which is occupied by the FPS value to display.

		_FpsRectStartX("FpsRectStartX", float) = 0.72
		_FpsRectStartY("FpsRectStartY", float) = 0.2

		_FpsRectEndX("FpsRectEndX", float) = 0.98
		_FpsRectEndY("FpsRectEndY", float) = 0.5
	}
		SubShader
		{
			Tags { "RenderType" = "Transparent" }
			LOD 100
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				#define HISTORY_LENGTH 60
				#define FPS_HIGH 42
				#define FPS_LOW 30

				uniform float _FpsArray[HISTORY_LENGTH];

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float3 packedDigits : TEXCOORD1;
					float2 digitsToShow : TEXCOORD2;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _CurrentFPS;
				float _FpsRectStartX;
				float _FpsRectStartY;
				float _FpsRectEndX;
				float _FpsRectEndY;
				float _Opacity;

				fixed4 _highFpsColor;
				fixed4 _midFpsColor;
				fixed4 _lowFpsColor;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);

					// Break down the number to display into its
					// constituent digits. Note that "ones" may
					// actually contain a number above 9. But this
					// is fine, because the texture containing the
					// individual digits from 0-9 in a row wraps
					// around, so the UV value will still work.

					float hundreds = 0.0;
					float throwAway = modf(_CurrentFPS / 100.0, hundreds);
					float tens = 0.0;
					float ones = modf(_CurrentFPS / 10.0, tens) * 10.0;


					o.packedDigits = float3(hundreds, tens, ones);

					o.digitsToShow = float2(step(99.01, _CurrentFPS), step(9.01, _CurrentFPS));

					return o;
				}

				fixed4 GetFpsNumberColor(v2f i)
				{
					float dontShowBackground = step(_FpsRectStartX, i.uv.x) * (1 - step(_FpsRectEndX, i.uv.x));
					dontShowBackground *= (step(_FpsRectStartY, i.uv.y) * (1 - step(_FpsRectEndY, i.uv.y)));

					i.uv.x = (i.uv.x - _FpsRectStartX) / (_FpsRectEndX - _FpsRectStartX);
					i.uv.y = (i.uv.y - _FpsRectStartY) / (_FpsRectEndY - _FpsRectStartY);

					float scaledU = i.uv.x * 3;
					float perDigitU = frac(scaledU);
					float digitPlace = scaledU - perDigitU;

					float isZerothDigit = 1.0 - step(0.01, digitPlace);
					float isSecondDigit = step(1.01, digitPlace);
					float isFirstDigit = (1.0 - isZerothDigit) * (1.0 - isSecondDigit);

					float startDigit = isZerothDigit * i.packedDigits.x +
						isFirstDigit * i.packedDigits.y +
						isSecondDigit * i.packedDigits.z;

					float uvx = 0.1 * (startDigit + perDigitU);

					float showDigit = isZerothDigit * i.digitsToShow.x +
						isFirstDigit * i.digitsToShow.y +
						isSecondDigit;

					showDigit *= dontShowBackground;

					fixed4 digitCol = tex2D(_MainTex, float2(uvx, i.uv.y));
					return lerp(fixed4(0, 0, 0, 0), digitCol, showDigit);
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 fpsTextColor = GetFpsNumberColor(i);

					float index = i.uv.x * (HISTORY_LENGTH - 1);
					float fps = _FpsArray[index];

					float isHigh = step(FPS_HIGH, fps);
					float isMid = step(FPS_LOW, fps);
					float isLow = 1.0f - isMid;

					isMid = isMid * (isMid - isHigh - isLow);

					fixed4 fpsBarColor = _highFpsColor * isHigh + _midFpsColor * isMid + _lowFpsColor * isLow;

					// "Bars" of the Fps meter should also indicate performance by height,
					// not only by colour.

					float relativeFps = fps / 60.0;
					float yRange = 0.75 * relativeFps + 0.25;
					fpsBarColor.a *= step(0.0, yRange - i.uv.y);

					// If we have a non-transparent pixel from the fpsText, then show that one,
					// otherwise show the bar's colour.

					return lerp(fpsBarColor, fpsTextColor, step(0.25, fpsTextColor.w));
				}

				ENDCG
			}
		}
}
