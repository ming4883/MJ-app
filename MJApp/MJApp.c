#include "../Crender/example/Common.h"
#include "../Crender/example/Mesh.h"
#include "../Crender/example/Material.h"
#include "../Crender/example/Pvr.h"
#include "Logo.h"
#include "WaterNormalMap.h"
#include "WaterFlowMap.h"

#include "../Crender/lib/crender/Mem.h"
#include "../Crender/lib/crender/Texture.h"

AppContext* app = nullptr;

Mesh* floorMesh = nullptr;
Mesh* waterMesh = nullptr;
Mesh* bgMesh = nullptr;

Material* sceneMtl = nullptr;
Material* waterMtl = nullptr;
Material* bgMtl = nullptr;
CrTexture* texture = nullptr;
CrTexture* waterNormalMap = nullptr;
CrTexture* waterFlowMap = nullptr;
CrTexture* refractTex = nullptr;
CrTexture* rttDepth = nullptr;

float elapsedTime = 0;
float deltaTime = 0;

const CrVec3 waterN = {0.0f, 1.0f, 0.0f};
const CrVec3 waterP = {0.0f, 0.0f, 0.0f};
const float waterSize = 5.0f;

typedef struct Input
{
	CrBool isDown;
	int x;
	int y;
	
} Input;

Input input = {0};

void drawBackground()
{
	/*
	static const CrVec4 c[] = {
		{0.57f, 0.85f, 1.0f, 1.0f},
		{0.145f, 0.31f, 0.405f, 1.0f},
		{0.57f, 0.85f, 1.0f, 1.0f},
		{0.57f, 0.85f, 1.0f, 1.0f},
	};
	*/
	
	static const CrVec4 c[] = {
		//{0xf0 / 255.f,  0x5a / 255.f, 0x77 / 255.f, 1.0f}, // logo color
		//{0x93 / 255.f, 0x4e / 255.f, 0x83 / 255.f, 1.0f}, // lower-left
		{0xe0 / 255.f, 0xe0 / 255.f, 0xe0 / 255.f, 1.0f}, // lower-left
		{0xff / 255.f, 0xff / 255.f, 0xff / 255.f, 1.0f}, // lower-right
		{0xff / 255.f, 0xff / 255.f, 0xff / 255.f, 1.0f}, // upper-left
		{0xff / 255.f, 0xff / 255.f, 0xff / 255.f, 1.0f}, // upper-right
	};
	/**/
	CrGpuState* gpuState = &crContext()->gpuState;

	gpuState->depthTest = CrFalse;
	gpuState->cull = CrTrue;
	crContextApplyGpuState(crContext());

	crGpuProgramPreRender(bgMtl->program);
	crGpuProgramUniform4fv(bgMtl->program, CrHash("u_colors"), 4, (const float*)c);

	meshPreRender(bgMesh, bgMtl->program);
	meshRenderTriangles(bgMesh);
}

void drawScene(CrMat44 viewMtx, CrMat44 projMtx, CrMat44 viewProjMtx, CrVec3 camPos)
{
	CrGpuProgram* prog = sceneMtl->program;
	CrGpuState* gpuState = &crContext()->gpuState;

	gpuState->cull = CrFalse;
	gpuState->depthTest = CrTrue;
	gpuState->depthWrite = CrFalse;
	gpuState->blend = CrTrue;
	gpuState->blendFactorSrcRGB = CrGpuState_BlendFactor_SrcAlpha;
	gpuState->blendFactorSrcA = CrGpuState_BlendFactor_SrcAlpha;
	gpuState->blendFactorDestRGB = CrGpuState_BlendFactor_OneMinusSrcAlpha;
	gpuState->blendFactorDestA = CrGpuState_BlendFactor_OneMinusSrcAlpha;
	crContextApplyGpuState(crContext());

	crGpuProgramPreRender(prog);
	{ CrSampler sampler = {CrSamplerFilter_MagMin_Linear_Mip_None,  CrSamplerAddress_Wrap, CrSamplerAddress_Wrap};
	crGpuProgramUniformTexture(prog, CrHash("u_tex"), texture, &sampler);
	}

	crGpuProgramUniform3fv(prog, CrHash("u_camPos"), 1, camPos.v);

	// draw wall
	{ CrVec3 v = {0, 1.125f, 0.0f};
	CrMat44 m;
	crMat44SetIdentity(&m);
	crMat44MakeRotation(&m, CrVec3_c010(), elapsedTime * 20.0f);
	crMat44SetTranslation(&m, &v);
	
	app->shaderContext.worldMtx = m;
	crMat44Mult(&app->shaderContext.worldViewMtx, &viewMtx, &m);
	crMat44Mult(&app->shaderContext.worldViewProjMtx, &viewProjMtx, &m);
	}

	appShaderContextPreRender(app, sceneMtl);

	meshPreRender(floorMesh, prog);
	meshRenderTriangles(floorMesh);
}

#define CYCLE 0.15f
#define HALF_CYCLE CYCLE * 0.5f

void drawWater(CrMat44 viewMtx, CrMat44 projMtx, CrMat44 viewProjMtx, CrVec3 camPos)
{
	CrGpuState* gpuState = &crContext()->gpuState;
	Material* mtl = waterMtl;
	CrGpuProgram* prog = mtl->program;
	
	gpuState->cull = CrTrue;
	gpuState->depthTest = CrTrue;
	gpuState->depthWrite = CrTrue;
	gpuState->blend = CrFalse;
	crContextApplyGpuState(crContext());

	crGpuProgramPreRender(mtl->program);

	{ CrSampler sampler = {CrSamplerFilter_MagMin_Linear_Mip_None,  CrSamplerAddress_Clamp, CrSamplerAddress_Clamp};
	crGpuProgramUniformTexture(prog, CrHash("u_refract"), refractTex, &sampler);
	}

	{ CrSampler sampler = {CrSamplerFilter_MagMin_Linear_Mip_None,  CrSamplerAddress_Wrap, CrSamplerAddress_Wrap};
	crGpuProgramUniformTexture(prog, CrHash("u_water"), waterNormalMap, &sampler);
	}
	
	{ CrSampler sampler = {CrSamplerFilter_MagMin_Linear_Mip_None,  CrSamplerAddress_Clamp, CrSamplerAddress_Clamp};
	crGpuProgramUniformTexture(prog, CrHash("u_flow"), waterFlowMap, &sampler);
	}

	{ float t = deltaTime * 0.05f;
	static float p0 = 0;
	static float p1 = HALF_CYCLE;
	float val[] = {16.0f / refractTex->width, HALF_CYCLE, 0, 0};
	p0 += t; if(p0 >= CYCLE) p0 = 0;
	p1 += t; if(p1 >= CYCLE) p1 = 0;
	val[2] = p0;
	val[3] = p1;

	crGpuProgramUniform4fv(prog, CrHash("u_refractionMapParam"), 1, val);
	}

	crGpuProgramUniform3fv(prog, CrHash("u_camPos"), 1, camPos.v);

	// draw water plane
	app->shaderContext.matDiffuse = crVec4(0xe0 / 255.f, 0x96 / 255.f, 0x89 / 255.f, 1.0f);
	//app->shaderContext.matDiffuse = crVec4(0x94 / 255.f, 0x63 / 255.f, 0x5a / 255.f, 1.0f);
	app->shaderContext.matSpecular = crVec4(1.0f, 1.0f, 1.0f, 1);
	app->shaderContext.matShininess = 64;
	{ CrMat44 m;
	crMat44MakeRotation(&m, CrVec3_c100(), -90);

	app->shaderContext.worldMtx = m;
	crMat44Mult(&app->shaderContext.worldViewMtx, &viewMtx, &m);
	crMat44Mult(&app->shaderContext.worldViewProjMtx, &viewProjMtx, &m);
	}
	appShaderContextPreRender(app, mtl);

	meshPreRender(waterMesh, prog);
	meshRenderTriangles(waterMesh);
}

void crAppUpdate(unsigned int elapsedMilliseconds)
{
	deltaTime = elapsedMilliseconds / 1000.0f;
	elapsedTime += deltaTime;
}

void crAppHandleMouse(int x, int y, int action)
{
	if(CrApp_MouseDown == action) {
		input.x = x;
		input.y = y;
		input.isDown = CrTrue;
	}
	else if(CrApp_MouseUp == action) {
		input.x = x;
		input.y = y;
		input.isDown = CrFalse;
	}
	else if((CrApp_MouseMove == action) && (CrTrue == input.isDown)) {
		input.x = x;
		input.y = y;
	}
}

void crAppRender()
{
	CrVec3 eyeAt = crVec3(0, 2.0f, 4.0f);
	CrVec3 lookAt = crVec3(0, 0, 0);
	CrVec3 eyeUp = *CrVec3_c010();
	CrMat44 viewMtx;
	CrMat44 projMtx;
	CrMat44 viewProjMtx;
	
	crMat44CameraLookAt(&viewMtx, &eyeAt, &lookAt, &eyeUp);
	crMat44Prespective(&projMtx, 60.0f, app->aspect.width / app->aspect.height, 0.1f, 30.0f);
	crMat44AdjustToAPIDepthRange(&projMtx);
	crMat44Mult(&viewProjMtx, &projMtx, &viewMtx);

	// render to refractTex
	{ CrTexture* bufs[] = {refractTex, nullptr};
	crContextPreRTT(crContext(), bufs, rttDepth);
	}
	crContextSetViewport(crContext(), 0, 0, (float)refractTex->width, (float)refractTex->height, -1, 1);

	crContextClearDepth(crContext(), 1);
	crContextClearColor(crContext(), 1, 1, 1, 1);
	//drawBackground();
	{ CrMat44 r, v, vp;
	crMat44PlanarReflect(&r, &waterN, &waterP);
	crMat44Mult(&v, &viewMtx, &r);
	crMat44Mult(&vp, &projMtx, &v);
	drawScene(v, projMtx, vp, eyeAt);
	}

	crContextPostRTT(crContext());
	crContextSetViewport(crContext(), 0, 0, (float)crContext()->xres, (float)crContext()->yres, -1, 1);

	// render to screen
	crContextClearDepth(crContext(), 1);
	crContextClearColor(crContext(), 1, 1, 1, 1);
	//drawBackground();
	drawWater(viewMtx, projMtx, viewProjMtx, eyeAt);
	drawScene(viewMtx, projMtx, viewProjMtx, eyeAt);
}

void crAppConfig()
{
	crAppContext.appName = "Reflection";
	crAppContext.context->xres = 480;
	crAppContext.context->yres = 854;
}

void crAppFinalize()
{
	meshFree(floorMesh);
	meshFree(waterMesh);
	meshFree(bgMesh);
	materialFree(waterMtl);
	materialFree(sceneMtl);
	materialFree(bgMtl);
	crTextureFree(texture);
	crTextureFree(waterNormalMap);
	crTextureFree(waterFlowMap);
	crTextureFree(refractTex);
	crTextureFree(rttDepth);
	appFree(app);
}

CrBool crAppInitialize()
{
	app = appAlloc();
	appInit(app);

	// materials
	{ const char* directives[]  = {nullptr};
	appLoadMaterialBegin(app, directives);

	sceneMtl = appLoadMaterial(
		"MJApp.Scene.Vertex",
		"MJApp.Scene.Fragment",
		nullptr, nullptr, nullptr);

	waterMtl = appLoadMaterial(
		"MJApp.SceneWater.Vertex",
		"MJApp.SceneWater.Fragment",
		nullptr, nullptr, nullptr);
	
	bgMtl = appLoadMaterial(
		"Common.Bg.Vertex.20",
		"Common.Bg.Fragment.20",
		nullptr, nullptr, nullptr);
	
	appLoadMaterialEnd(app);
	}

	// textures
	texture = Pvr_createTexture(Logo);
	waterNormalMap = Pvr_createTexture(WaterNormalMap);
	waterFlowMap = Pvr_createTexture(WaterFlowMap);

	crDbgStr("create scene color buffers\n");
	refractTex = crTextureAlloc();
	crTextureInitRtt(refractTex, 512, 512, 0, 1, CrGpuFormat_UnormR8G8B8A8);
	
	crDbgStr("create scene depth buffers\n");
	rttDepth = crTextureAlloc();
	crTextureInitRtt(rttDepth, 512, 512, 0, 1, CrGpuFormat_Depth16);

	// floor
	{ CrVec3 offset = crVec3(-1.0f, -1.0f, 0);
	CrVec2 uvs = crVec2(1.0f, -1.0f);
	floorMesh = meshAlloc();
	meshInitWithQuad(floorMesh, 2, 2, &offset, &uvs, 1);
	}

	// water
	{ CrVec3 offset = crVec3(-waterSize, -waterSize, 0);
	CrVec2 uvs = crVec2(1.0f, 1.0f);
	waterMesh = meshAlloc();
	meshInitWithQuad(waterMesh, waterSize*2, waterSize*2, &offset, &uvs, 1);
	}

	// bg
	bgMesh = meshAlloc();
	meshInitWithScreenQuad(bgMesh);

	crDbgStr("MJApp started\n");

	return CrTrue;
}
