#ifndef LIGHTING_CEL_SHADED_INCLUDED
#define LIGHTING_CEL_SHADED_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
struct EdgeConstants 
{
   float ambient;
   float diffuse;
   float specular;
   float specularOffset;
   float distanceAttenuation;
   float shadowAttenuation;
   float rim;
   float rimOffset;
};

struct SurfaceVariables 
{
   float3 normal;
   float3 view;
   float smoothness;
   float shininess;
   float rimThreshold;
   EdgeConstants ec;
};

float3 CalculateCelShading(Light l, SurfaceVariables s)
{
   float shadowAttenuationSmoothstepped = smoothstep(0.0f, s.ec.shadowAttenuation, l.shadowAttenuation);
   float distanceAttenuationSmoothstepped = smoothstep(0.0f, s.ec.distanceAttenuation, l.distanceAttenuation);
   float attenuation = shadowAttenuationSmoothstepped * distanceAttenuationSmoothstepped;

   float diffuse = saturate(dot(s.normal, l.direction));
   diffuse *= attenuation;

   float3 h = SafeNormalize(l.direction + s.view);
   float specular = saturate(dot(s.normal, h));
   specular = pow(specular, s.shininess);
   specular *= diffuse * s.smoothness;

   float rim = 1 - dot(s.view, s.normal);
   rim *= pow(diffuse, s.rimThreshold);

   diffuse = smoothstep(0.0f, s.ec.diffuse, diffuse);
   specular = s.smoothness * smoothstep((1 - s.smoothness) * s.ec.specular + s.ec.specularOffset, s.ec.specular + s.ec.specularOffset, specular);
   rim = s.smoothness * smoothstep(s.ec.rim - 0.5f * s.ec.rimOffset, s.ec.rim + 0.5f * s.ec.rimOffset, rim);

   return s.ec.ambient + l.color * (diffuse + max(specular, rim));
}
#endif

void LightingCelShaded_float(float Smoothness, float RimThreshold, float3 Position, float3 Normal, float3 View, float EdgeDiffuse, float EdgeSpecular, float EdgeSpecularOffset,
float EdgeDistanceAttenuation, float EdgeShadowAttenuation, float EdgeRim, float EdgeRimOffset, float Ambient, out float3 Color)
{
#if defined(SHADERGRAPH_PREVIEW)
   Color = float3(0.0f, 0.0f, 0.0f);
#else
   // Initialize edge constants
   EdgeConstants ec;
   ec.diffuse = EdgeDiffuse;
   ec.specular = EdgeSpecular;
   ec.specularOffset = EdgeSpecularOffset;
   ec.distanceAttenuation = EdgeDistanceAttenuation;
   ec.shadowAttenuation = EdgeShadowAttenuation;
   ec.rim = EdgeRim;
   ec.rimOffset = EdgeRimOffset;
   ec.ambient = Ambient;

   // Initialize surface variable
   SurfaceVariables s;
   s.normal = normalize(Normal);
   s.view = SafeNormalize(View);
   s.smoothness = Smoothness;
   s.shininess = exp2(10 * Smoothness + 1);
   s.rimThreshold = RimThreshold;
   s.ec = ec;

   #if SHADOW_SCREEN
      float4 clipPos = TransformWorldToHClip(Position);
      float4 shadowCoord = ComputeScreenPos(clipPos);
   #else
      float4 shadowCoord = TransformWorldToShadowCoord(Position);
   #endif

   Light light = GetMainLight(shadowCoord);
   Color = CalculateCelShading(light, s);

   int pixelLightCount = GetAdditionalLightsCount();
   for (int i = 0; i < pixelLightCount; i++)
   {
      light = GetAdditionalLight(i, Position, 1);
      Color += CalculateCelShading(light, s);
   }

#endif
}

#endif