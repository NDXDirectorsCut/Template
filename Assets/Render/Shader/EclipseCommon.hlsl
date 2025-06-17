#ifndef ECLIPSE_COMMON_INCLUDED
#define ECLIPSE_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

float4x4 unity_ObjectToWorld;
float4x4 unity_WorldToObject;
float4 unity_LODFade;
real4 unity_WorldTransformParams;

float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 unity_MatrixInvV;
float4x4 unity_prev_MatrixM;
float4x4 unity_prev_MatrixIM;
float4x4 glstate_matrix_projection;
float3 _WorldSpaceCameraPos;

#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_I_V unity_MatrixInvV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_PREV_MATRIX_M unity_prev_MatrixM
#define UNITY_PREV_MATRIX_I_M unity_prev_MatrixIM
#define UNITY_MATRIX_P glstate_matrix_projection

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
//#define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)

float _EnvironmentLighting;
float _EnvironmentReflection;

#define MAX_DIRECTIONAL_LIGHTS 32
#define MAX_DIRECTIONAL_SHADOWS 4
#define MAX_CASCADES 4
#define MAX_OTHER_LIGHTS 128
#define MAX_OTHER_SHADOWS 32

int _DirectionalLightCount;
float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHTS];
float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHTS];
float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHTS];
int _CascadeCount;
float4 _CascadeCullingSpheres[MAX_CASCADES];
float4 _CascadeData[MAX_CASCADES];
float4x4 _DirectionalShadowMatrices[MAX_DIRECTIONAL_SHADOWS * MAX_CASCADES];
float4x4 _DirectionalShadowInvMatrices[MAX_DIRECTIONAL_SHADOWS * MAX_CASCADES];
float4x4 _DirectionalShadowInverseMatrices[MAX_DIRECTIONAL_SHADOWS * MAX_CASCADES];

int _OtherLightCount;
float4 _OtherLightColors[MAX_OTHER_LIGHTS];
float4 _OtherLightPositions[MAX_OTHER_LIGHTS];
float4 _OtherLightDirections[MAX_OTHER_LIGHTS];
float4 _OtherLightSpotAngles[MAX_OTHER_LIGHTS];
float4 _OtherLightShadowData[MAX_OTHER_LIGHTS];
float4x4 _OtherShadowMatrices[MAX_OTHER_SHADOWS];

static const float3 pointShadowPlanes[6] = {
	float3(-1.0, 0.0, 0.0),
	float3(1.0, 0.0, 0.0),
	float3(0.0, -1.0, 0.0),
	float3(0.0, 1.0, 0.0),
	float3(0.0, 0.0, -1.0),
	float3(0.0, 0.0, 1.0)
};

static const float2 poissonDisk[64] = {
	float2(0.0617981, 0.07294159),
	float2(0.6470215, 0.7474022),
	float2(-0.5987766, -0.7512833),
	float2(-0.693034, 0.6913887),
	float2(0.6987045, -0.6843052),
	float2(-0.9402866, 0.04474335),
	float2(0.8934509, 0.07369385),
	float2(0.1592735, -0.9686295),
	float2(-0.05664673, 0.995282),
	float2(-0.1203411, -0.1301079),
	float2(0.1741608, -0.1682285),
	float2(-0.09369049, 0.3196758),
	float2(0.185363, 0.3213367),
	float2(-0.1493771, -0.3147511),
	float2(0.4452095, 0.2580113),
	float2(-0.1080467, -0.5329178),
	float2(0.1604507, 0.5460774),
	float2(-0.4037193, -0.2611179),
	float2(0.5947998, -0.2146744),
	float2(0.3276062, 0.9244621),
	float2(-0.6518704, -0.2503952),
	float2(-0.3580975, 0.2806469),
	float2(0.8587891, 0.4838005),
	float2(-0.1596546, -0.8791054),
	float2(-0.3096867, 0.5588146),
	float2(-0.5128918, 0.1448544),
	float2(0.8581337, -0.424046),
	float2(0.1562584, -0.5610626),
	float2(-0.7647934, 0.2709858),
	float2(-0.3090832, 0.9020988),
	float2(0.3935608, 0.4609676),
	float2(0.3929337, -0.5010948),
	float2(-0.8682281, -0.1990303),
	float2(-0.01973724, 0.6478714),
	float2(-0.3897587, -0.4665619),
	float2(-0.7416366, -0.4377831),
	float2(-0.5523247, 0.4272514),
	float2(-0.5325066, 0.8410385),
	float2(0.3085465, -0.7842533),
	float2(0.8400612, -0.200119),
	float2(0.6632416, 0.3067062),
	float2(-0.4462856, -0.04265022),
	float2(0.06892014, 0.812484),
	float2(0.5149567, -0.7502338),
	float2(0.6464897, -0.4666451),
	float2(-0.159861, 0.1038342),
	float2(0.6455986, 0.04419327),
	float2(-0.7445076, 0.5035095),
	float2(0.9430245, 0.3139912),
	float2(0.0349884, -0.7968109),
	float2(-0.9517487, 0.2963554),
	float2(-0.7304786, -0.01006928),
	float2(-0.5862702, -0.5531025),
	float2(0.3029106, 0.09497032),
	float2(0.09025345, -0.3503742),
	float2(0.4356628, -0.0710125),
	float2(0.4112572, 0.7500054),
	float2(0.3401214, -0.3047142),
	float2(-0.2192158, -0.6911137),
	float2(-0.4676369, 0.6570358),
	float2(0.6295372, 0.5629555),
	float2(0.1253822, 0.9892166),
	float2(-0.1154335, 0.8248222),
	float2(-0.4230408, -0.7129914),
};

static const float3 poissonSphere[256] = {
	float3(1.3789724, 1.0897330, 0.0262126),
	float3(1.3764753, 1.1004026, 0.2164218),
	float3(1.2656203, 1.1988343, 0.0661480),
	float3(1.4352847, 0.9943186, 0.1241790),
	float3(1.2532302, 1.0194877, 0.0128521),
	float3(1.2660272, 1.1003327, 0.0865168),
	float3(1.2325377, 1.2332759, 0.1593739),
	float3(1.3054420, 0.9371646, 0.0527322),
	float3(1.4083655, 0.9057239, 0.0313406),
	float3(1.1282994, 1.1821223, 0.1847234),
	float3(1.0673409, 1.2522345, 0.1343266),
	float3(0.9647787, 1.1337120, 0.2205408),
	float3(0.9405693, 1.0079031, 0.0825700),
	float3(1.1964961, 1.0544797, 0.1861273),
	float3(1.1946976, 0.7903378, 0.0643499),
	float3(0.8044865, 1.0239153, 0.1286415),
	float3(1.0239868, 0.9609498, 0.1715500),
	float3(1.0156267, 1.1549351, 0.0857485),
	float3(1.1857619, 0.6739400, 0.1653859),
	float3(1.1160736, 1.2716896, 0.2995168),
	float3(1.3013388, 0.9839621, 0.2306617),
	float3(1.2047829, 1.1744234, 0.3525776),
	float3(1.5136148, 0.8692689, 0.1926239),
	float3(1.4832442, 1.0721753, 0.0691908),
	float3(0.9480732, 1.0449759, 0.1762901),
	float3(1.3160280, 0.8103755, 0.1115805),
	float3(0.6241510, 1.0363618, 0.1402974),
	float3(1.4616187, 1.1626876, 0.1431212),
	float3(1.1732524, 0.7971741, 0.2164438),
	float3(1.0836235, 0.8396675, 0.1295094),
	float3(1.5487143, 0.9685897, 0.0118869),
	float3(0.8670130, 0.8739852, 0.1369553),
	float3(0.9447869, 1.1034451, 0.3491659),
	float3(1.1808682, 1.1489989, 0.0189782),
	float3(1.3480227, 1.3712875, 0.0124804),
	float3(1.5313528, 1.0112045, 0.1919210),
	float3(1.1912988, 0.9118379, 0.2032559),
	float3(1.0137276, 1.2380060, 0.3121556),
	float3(1.0114077, 0.9401102, 0.4130066),
	float3(1.0068368, 0.7330366, 0.0433429),
	float3(0.9406977, 1.2696491, 0.2258019),
	float3(1.1709993, 1.3143003, 0.0516663),
	float3(0.8673209, 1.1847075, 0.2845032),
	float3(1.2730204, 0.6447891, 0.0407536),
	float3(0.8476751, 1.3000333, 0.1715184),
	float3(1.3489977, 1.1968318, 0.2858992),
	float3(1.6441842, 0.8651023, 0.0251082),
	float3(1.2391382, 1.1417211, 0.2566460),
	float3(0.8499539, 1.3995869, 0.1188811),
	float3(1.7455579, 1.0210502, 0.0388821),
	float3(1.3599926, 0.8898403, 0.1901462),
	float3(1.1835733, 1.3282844, 0.1757986),
	float3(0.7088306, 0.9871765, 0.2085508),
	float3(1.0836392, 0.8634744, 0.0265378),
	float3(1.4887372, 0.8231234, 0.0374104),
	float3(0.9156641, 0.9692925, 0.4626438),
	float3(1.1652784, 0.9096948, 0.0629535),
	float3(1.2756621, 1.3006223, 0.3609592),
	float3(1.4393740, 1.2709645, 0.0009293),
	float3(1.2803927, 0.7129788, 0.2243858),
	float3(1.0680066, 1.0630673, 0.1450856),
	float3(1.3395194, 1.1848575, 0.1553826),
	float3(1.5665854, 1.1055434, 0.0079956),
	float3(0.9280857, 0.8202947, 0.0358319),
	float3(1.6676302, 0.7816518, 0.1867293),
	float3(1.4140262, 1.2899219, 0.2945020),
	float3(1.4306333, 1.3705336, 0.0899030),
	float3(1.2709751, 1.3158199, 0.2426404),
	float3(1.0603263, 1.2632845, 0.3974745),
	float3(1.0836405, 1.3598040, 0.0969364),
	float3(0.9135609, 1.1525093, 0.1291289),
	float3(1.5650150, 1.1897656, 0.1812228),
	float3(0.8143968, 1.0120971, 0.2867177),
	float3(1.1232212, 1.1249450, 0.2736847),
	float3(1.1624836, 0.6852423, 0.0294446),
	float3(1.6146328, 0.9710186, 0.1096552),
	float3(1.2103734, 0.9790816, 0.3389570),
	float3(1.1862070, 1.4368019, 0.1094891),
	float3(1.4409014, 1.1949628, 0.3464018),
	float3(1.0395254, 0.9607661, 0.0698040),
	float3(0.9438821, 0.9766291, 0.2972044),
	float3(1.6955385, 1.0551729, 0.1738530),
	float3(0.9567344, 0.8253130, 0.2206792),
	float3(0.8538063, 1.1565289, 0.0353584),
	float3(1.1251925, 1.0083462, 0.0236847),
	float3(1.3522753, 1.2044026, 0.0018808),
	float3(1.5622763, 0.7054682, 0.0331742),
	float3(1.8049128, 0.8871669, 0.0163431),
	float3(1.3379821, 1.0176305, 0.1411405),
	float3(1.0466369, 1.0497120, 0.3074660),
	float3(0.7631629, 1.1668833, 0.1870831),
	float3(1.0388824, 1.1027893, 0.0014408),
	float3(1.4027726, 1.2595874, 0.1014614),
	float3(1.4441414, 1.0189406, 0.2559451),
	float3(1.0374509, 0.9295061, 0.2868673),
	float3(1.5623592, 1.2446317, 0.0855615),
	float3(1.0072857, 1.3466033, 0.2733920),
	float3(0.8349641, 0.9193662, 0.3800624),
	float3(1.1243059, 0.9845012, 0.1816686),
	float3(0.7195344, 1.1020983, 0.0599981),
	float3(1.2477936, 1.4041795, 0.4378456),
	float3(1.3727863, 1.4590338, 0.4006326),
	float3(1.1803242, 1.3986028, 0.3134601),
	float3(1.0243633, 1.2776506, 0.0104080),
	float3(1.5335743, 1.2036439, 0.2822112),
	float3(0.6857050, 0.8740384, 0.4219424),
	float3(0.9055330, 1.2406728, 0.4568817),
	float3(1.2272895, 0.8176639, 0.3639854),
	float3(0.9451710, 1.3760613, 0.0799754),
	float3(1.3023317, 1.0919702, 0.3289000),
	float3(1.2805666, 1.4568760, 0.1604689),
	float3(1.6701831, 1.1096732, 0.0100664),
	float3(1.0265827, 0.5959314, 0.1750677),
	float3(1.7171931, 0.9562313, 0.2497334),
	float3(0.8165764, 1.2991990, 0.2886534),
	float3(0.6901732, 0.9624445, 0.0733688),
	float3(0.7103990, 0.9314675, 0.3256934),
	float3(1.7919946, 0.9882070, 0.1261077),
	float3(0.9358983, 0.7293183, 0.1598855),
	float3(1.0953767, 0.8737152, 0.2254372),
	float3(1.3229911, 0.9400002, 0.4652717),
	float3(1.8386988, 1.1603755, 0.0342323),
	float3(1.3777741, 1.0600019, 0.5776741),
	float3(1.7427407, 0.7731165, 0.2865161),
	float3(0.7619922, 1.2221659, 0.4972507),
	float3(1.4887023, 1.4489951, 0.2027416),
	float3(0.8034240, 1.2142303, 0.3596345),
	float3(0.9783989, 1.4009055, 0.1725911),
	float3(0.6517428, 1.0635571, 0.3181603),
	float3(0.8431008, 1.2627195, 0.0739263),
	float3(0.8818952, 0.8486578, 0.3240872),
	float3(1.8325235, 1.2499591, 0.0952740),
	float3(1.0952758, 1.3952626, 0.4378420),
	float3(1.3614452, 1.3601553, 0.2397568),
	float3(1.5931076, 0.7053994, 0.2689663),
	float3(0.7760214, 0.8976592, 0.2434824),
	float3(1.3431811, 0.5577296, 0.1949664),
	float3(0.7157507, 1.1357976, 0.2828455),
	float3(1.1233112, 0.6593310, 0.3946832),
	float3(1.6830400, 1.1937126, 0.1723427),
	float3(1.5174495, 0.9346254, 0.2685411),
	float3(1.0914784, 0.8768878, 0.4653681),
	float3(1.0024079, 1.5322372, 0.0864187),
	float3(0.7968623, 0.9201568, 0.0555131),
	float3(0.9352663, 0.6428657, 0.2459954),
	float3(1.8772687, 0.8056124, 0.1326169),
	float3(1.1215779, 1.5394273, 0.0975744),
	float3(1.2026024, 0.7431661, 0.5128119),
	float3(1.9723501, 0.9263985, 0.0834052),
	float3(1.4855651, 1.1009059, 0.6917358),
	float3(1.0025993, 1.3824064, 0.3715190),
	float3(1.4560694, 1.1713309, 0.0363933),
	float3(1.7376053, 1.1339744, 0.3307758),
	float3(1.6714617, 1.2080148, 0.0467597),
	float3(0.8107012, 0.7999678, 0.3986649),
	float3(1.1983811, 1.2805097, 0.4487674),
	float3(0.8583086, 1.3282479, 0.5005960),
	float3(1.3756807, 0.9237151, 0.5795208),
	float3(1.5093644, 1.3326931, 0.1579631),
	float3(1.3924868, 1.3490163, 0.4299167),
	float3(1.4955004, 0.7920670, 0.2745725),
	float3(1.3757197, 1.0018891, 0.3894882),
	float3(1.4662575, 1.4479085, 0.0101617),
	float3(1.0668198, 0.5871429, 0.0539277),
	float3(1.7078367, 0.7048972, 0.0613060),
	float3(0.7257415, 1.4053097, 0.1782373),
	float3(1.6438275, 0.6195967, 0.1310176),
	float3(1.1443241, 0.7065736, 0.2548443),
	float3(1.3501323, 0.8195000, 0.0071378),
	float3(0.9596590, 1.2802741, 0.6127138),
	float3(0.8563013, 1.3772695, 0.3692181),
	float3(1.4752295, 1.4765593, 0.3530454),
	float3(1.2399022, 1.0175702, 0.5062431),
	float3(1.4437662, 1.0023578, 0.0082231),
	float3(1.4051702, 1.0469046, 0.7466736),
	float3(1.1224302, 1.3347899, 0.5308862),
	float3(1.5723797, 1.5883758, 0.2498240),
	float3(0.8709258, 1.4776234, 0.2317063),
	float3(0.9381164, 1.0988370, 0.0402145),
	float3(1.6059205, 1.0843653, 0.2419711),
	float3(1.6327574, 0.6973669, 0.3883091),
	float3(1.0668729, 1.0264842, 0.4361088),
	float3(1.1286842, 0.5209909, 0.1004564),
	float3(1.2563164, 0.5764027, 0.1408122),
	float3(1.5079313, 1.7051777, 0.3316915),
	float3(1.3159192, 1.3890309, 0.5632374),
	float3(1.3703225, 0.8519527, 0.3980677),
	float3(0.6156183, 0.8533884, 0.0523038),
	float3(1.4295144, 0.7603991, 0.3856456),
	float3(0.6189515, 1.2526137, 0.5132098),
	float3(0.5767085, 0.9836387, 0.2864804),
	float3(0.6840127, 1.2443351, 0.0981214),
	float3(1.6810772, 0.8806254, 0.1373350),
	float3(1.3984827, 0.6805930, 0.0298809),
	float3(1.5700375, 0.8402289, 0.3257666),
	float3(1.0576036, 0.8249148, 0.3147359),
	float3(0.5804724, 0.9319664, 0.1099580),
	float3(1.0438352, 0.4264798, 0.0343467),
	float3(1.7570735, 1.1376545, 0.2167215),
	float3(1.3859551, 1.1688591, 0.4522456),
	float3(1.6057256, 1.6488353, 0.4040293),
	float3(1.0192833, 1.5131596, 0.5412533),
	float3(1.3575413, 1.4652013, 0.6770958),
	float3(1.3053993, 0.8140755, 0.2359097),
	float3(1.9758180, 1.2394163, 0.0656394),
	float3(1.1945256, 1.4937765, 0.6187823),
	float3(1.8882731, 1.0114083, 0.1061062),
	float3(1.1648930, 0.4486086, 0.0044512),
	float3(1.4366596, 0.7135484, 0.2037144),
	float3(1.6432357, 1.4863148, 0.1633045),
	float3(1.2448421, 0.6445364, 0.4015158),
	float3(1.6578731, 0.9641360, 0.3634944),
	float3(0.6606859, 1.2323360, 0.1990774),
	float3(1.4836464, 1.0922947, 0.3863218),
	float3(0.6676188, 1.2273193, 0.6487024),
	float3(1.5003979, 1.3279991, 0.3355582),
	float3(1.7668505, 0.6777712, 0.1822049),
	float3(1.5292129, 1.3569541, 0.0258357),
	float3(0.8995036, 0.4208995, 0.1491747),
	float3(1.2543183, 0.8950969, 0.2843576),
	float3(1.7925489, 1.3712729, 0.0606804),
	float3(1.0920283, 1.1658404, 0.3872327),
	float3(1.3073301, 1.4289602, 0.3291066),
	float3(0.5975780, 1.1518815, 0.0892904),
	float3(0.5496650, 1.1706637, 0.6523395),
	float3(0.9452741, 0.6722182, 0.3539441),
	float3(1.2107315, 0.3515099, 0.0764933),
	float3(0.8860896, 0.9312140, 0.2255427),
	float3(0.9691020, 0.8615017, 0.1278550),
	float3(1.2158976, 1.5720576, 0.4344215),
	float3(0.9722576, 0.8327771, 0.3903312),
	float3(1.1465213, 1.4934268, 0.5002484),
	float3(0.6705609, 1.1434302, 0.4448620),
	float3(1.2758859, 0.5878034, 0.2834880),
	float3(1.6321107, 0.5509108, 0.0030331),
	float3(1.4818441, 0.9832026, 0.5392763),
	float3(1.6583684, 0.5455071, 0.2953207),
	float3(0.7347917, 0.9708628, 0.4516979),
	float3(1.7748829, 0.7831086, 0.1053361),
	float3(1.2889261, 1.2796966, 0.5423667),
	float3(1.5974023, 1.0197151, 0.4961374),
	float3(1.4137954, 0.8148176, 0.1436401),
	float3(1.1750185, 1.3153795, 0.6867573),
	float3(1.2443297, 1.3738479, 0.8357205),
	float3(1.7128038, 0.8426840, 0.3550744),
	float3(1.3168870, 1.4519077, 0.8204653),
	float3(1.1215888, 0.8089036, 0.3906484),
	float3(1.7200123, 1.6599416, 0.4539743),
	float3(0.5142192, 1.2443730, 0.0458358),
	float3(1.0553400, 0.7776117, 0.2210858),
	float3(1.5071592, 0.8987220, 0.3871748),
	float3(0.8863030, 0.7429935, 0.4505780),
	float3(0.7309132, 1.3685831, 0.4511081),
	float3(0.7388173, 1.3339430, 0.0172966),
	float3(1.5539545, 1.2814438, 0.4709274),
	float3(1.5442573, 0.5130372, 0.0381531)
};

uint HashR(uint s)
{
    s ^= 2747636419u;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    return s;
}

float Random(uint seed)
{
    return float(HashR(seed)) / 4294967295.0 * 0.004; // 2^32-1
}

TextureCubeArray _ReflectionProbeArray;
SAMPLER(sampler_ReflectionProbeArray);

struct SurfaceData
{
    float3 diffuse;
    float alpha;
    float3 normal;
    float3 emission;
    float3 specularity;
    float metalness;
    float roughness;
    float3 sheen;

    float3 viewDir;
    float3 position;
};

float4x4 Move4x4(float4x4 m, float3 v)
{
    float x = v.x, y = v.y, z = v.z;
    m[0][3] += x;
    m[1][3] += y;
    m[2][3] += z;
    return m;
}

float4x4 AxisAngle4x4(float3 axis, float angle)
{
    angle = radians(angle);
    float s = sin(angle);
    float c = cos(angle);
    float one_minus_c = 1.0 - c;

    axis = normalize(axis);
    float4x4 rotMatrix = 
    {   one_minus_c * axis.x * axis.x + c, one_minus_c * axis.x * axis.y - axis.z * s, one_minus_c * axis.z * axis.x + axis.y * s, 0,
        one_minus_c * axis.x * axis.y + axis.z * s, one_minus_c * axis.y * axis.y + c, one_minus_c * axis.y * axis.z - axis.x * s, 0,
        one_minus_c * axis.z * axis.x - axis.y * s, one_minus_c * axis.y * axis.z + axis.x * s, one_minus_c * axis.z * axis.z + c, 0,
        0                                         , 0                                         , 0                                , 1,
    };

    // float4x4 finalRot = float4x4(
    //     float rotMatrix[0][0], rotMatrix[0][1], rotMatrix[0][2], 0, 
    //     float rotMatrix[1][0], rotMatrix[1][1], rotMatrix[1][2], 0, 
    //     float rotMatrix[2][0], rotMatrix[2][1], rotMatrix[2][2], 0, 
    //     0              , 0        , 0        , 1
    // );

    return rotMatrix;
}

float3x3 AxisAngle3x3(float3 axis, float angle)
{
    angle = radians(angle);
    float s = sin(angle);
    float c = cos(angle);
    float one_minus_c = 1.0 - c;

    axis = normalize(axis);
    float3x3 rotMatrix = 
    {   one_minus_c * axis.x * axis.x + c, one_minus_c * axis.x * axis.y - axis.z * s, one_minus_c * axis.z * axis.x + axis.y * s,
        one_minus_c * axis.x * axis.y + axis.z * s, one_minus_c * axis.y * axis.y + c, one_minus_c * axis.y * axis.z - axis.x * s,
        one_minus_c * axis.z * axis.x - axis.y * s, one_minus_c * axis.y * axis.z + axis.x * s, one_minus_c * axis.z * axis.z + c
    };

    // float4x4 finalRot = float4x4(
    //     float rotMatrix[0][0], rotMatrix[0][1], rotMatrix[0][2], 0, 
    //     float rotMatrix[1][0], rotMatrix[1][1], rotMatrix[1][2], 0, 
    //     float rotMatrix[2][0], rotMatrix[2][1], rotMatrix[2][2], 0, 
    //     0              , 0        , 0        , 1
    // );

    return rotMatrix;
}

float4x4 Scale4x4(float4x4 m, float3 v)
{
    float x = v.x, y = v.y, z = v.z;
    m[0][0] *= x;
    m[1][1] *= y;
    m[2][2] *= z;
    return m;
}

float DistanceSquared(float3 pA, float3 pB)
{
	return dot(pA - pB, pA - pB);
}

float Distance(float3 pA, float3 pB)
{
    return length(pA-pB);
}

float3 DecodeNormal (float4 sample, float strength) {
	#if defined(UNITY_NO_DXT5nm)
	    return normalize(UnpackNormalRGB(sample, strength));
	#else
	    return normalize(UnpackNormalmapRGorAG(sample, strength));
	#endif
}

float3 NormalTangentToWorld (float3 normalTS, float3 normalWS, float4 tangentWS) {
	float3x3 tangentToWorld = CreateTangentToWorld(normalWS, tangentWS.xyz, tangentWS.w);
	return TransformTangentToWorld(normalTS, tangentToWorld);
}

float Square (float x) 
{
	return x * x;
}

float3 Inversion(float3 value, float inversion)
{
    float3 activeValue = value;
    if(inversion < 0)
    {
        activeValue = 1 - value;
    }

    return activeValue * abs(inversion);
}

#endif