#include "../Crender/example/Common.h"
#include "../Crender/example/Mesh.h"
#include "../Crender/example/Material.h"
#include "../Crender/example/Pvr.h"
#include "Logo.h"
#include "Cloud.h"
#include "Rainbow.h"
#include "WaterNormalMap.h"
#include "WaterFlowMap.h"

#include "../Crender/lib/crender/Mem.h"
#include "../Crender/lib/crender/Texture.h"

AppContext* app = nullptr;

Mesh* sceneMesh = nullptr;
Mesh* waterMesh = nullptr;
Mesh* bgMesh = nullptr;

Material* sceneMtl = nullptr;
Material* waterMtl = nullptr;
Material* bgMtl = nullptr;
CrTexture* logo = nullptr;
CrTexture* rainbow = nullptr;
CrTexture* cloud = nullptr;
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

void drawBackground(void)
{
	static const CrVec4 c[] = {
		{0xff / 255.f, 0xff / 255.f, 0xff / 255.f, 1.0f}, // lower-left
		{0xff / 255.f, 0xff / 255.f, 0xff / 255.f, 1.0f}, // lower-right
		{0x64 / 255.f,  0xcd / 255.f, 0xf2 / 255.f, 1.0f}, // upper-left
		{0x64 / 255.f,  0xcd / 255.f, 0xf2 / 255.f, 1.0f}, // upper-right
	};
	CrGpuProgram* prog = bgMtl->program;
	CrGpuState* gpuState = &crContext()->gpuState;

	gpuState->depthTest = CrFalse;
	gpuState->depthWrite = CrFalse;
	gpuState->cull = CrTrue;
	crContextApplyGpuState(crContext());

	crGpuProgramPreRender(prog);
	crGpuProgramUniform4fv(prog, CrHash("u_colors"), 4, (const float*)c);

	meshPreRender(bgMesh, bgMtl->program);
	meshRenderTriangles(bgMesh);
}

void drawScene(CrMat44 viewMtx, CrMat44 projMtx, CrMat44 viewProjMtx, CrVec3 camPos)
{
	CrGpuProgram* prog = sceneMtl->program;
	CrGpuState* gpuState = &crContext()->gpuState;
	static CrVec3 cloudPos[] = { 
	{0.0f, 0.75f, -1.0f}, {2.0f, 1.5f, -1.0f}, {-2.0f, 2.5f, -1.0f}, 
	{1.5f, 3.5f, -1.0f}, {-1.5f, 3.5f, -1.0f},
	};
	size_t i;

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
	crGpuProgramUniform3fv(prog, CrHash("u_camPos"), 1, camPos.v);
	
	// draw rainbow
	{ CrSampler sampler = {CrSamplerFilter_MagMin_Linear_Mip_None,  CrSamplerAddress_Wrap, CrSamplerAddress_Wrap};
	crGpuProgramUniformTexture(prog, CrHash("u_tex"), rainbow, &sampler);
	}

	{ CrVec3 v = {0, 0.5f, 0.0f};
	CrMat44 m;
	crMat44SetIdentity(&m);
	crMat44SetTranslation(&m, &v);
	m.m00 = m.m11 = 4.0f;
	
	app->shaderContext.worldMtx = m;
	crMat44Mult(&app->shaderContext.worldViewMtx, &viewMtx, &m);
	crMat44Mult(&app->shaderContext.worldViewProjMtx, &viewProjMtx, &m);
	}
	
	appShaderContextPreRender(app, sceneMtl);

	meshPreRender(sceneMesh, prog);
	meshRenderTriangles(sceneMesh);
	
	// draw clouds
	for(i=0; i<crCountOf(cloudPos); ++i) {
		{ CrSampler sampler = {CrSamplerFilter_MagMin_Linear_Mip_None,  CrSamplerAddress_Wrap, CrSamplerAddress_Wrap};
		crGpuProgramUniformTexture(prog, CrHash("u_tex"), cloud, &sampler);
		}
		
		{ CrVec3 v = cloudPos[i];
		float a = -3; float b = 3;
		float d = b - a;
		float w;
		CrMat44 m;
		crMat44SetIdentity(&m);
		cloudPos[i].x += deltaTime * 0.25f * (rand() / (float)RAND_MAX);
		w = fmod((cloudPos[i].x - a) / d, 1.0);
		v.x = a + (b - a) * w;
		crMat44SetTranslation(&m, &v);
		m.m00 = 1.0f; m.m11 = 0.5f;
		
		app->shaderContext.worldMtx = m;
		crMat44Mult(&app->shaderContext.worldViewMtx, &viewMtx, &m);
		crMat44Mult(&app->shaderContext.worldViewProjMtx, &viewProjMtx, &m);
		}
		
		appShaderContextPreRender(app, sceneMtl);

		meshPreRender(sceneMesh, prog);
		meshRenderTriangles(sceneMesh);
	}
	
	// draw logo
	{ CrSampler sampler = {CrSamplerFilter_MagMin_Linear_Mip_None,  CrSamplerAddress_Wrap, CrSamplerAddress_Wrap};
	crGpuProgramUniformTexture(prog, CrHash("u_tex"), logo, &sampler);
	}

	{ CrVec3 v = {0, 1.0f, 1.0f};
	CrMat44 m;
	crMat44SetIdentity(&m);
	crMat44MakeRotation(&m, CrVec3_c010(), elapsedTime * 25.0f);
	crMat44SetTranslation(&m, &v);
	
	app->shaderContext.worldMtx = m;
	crMat44Mult(&app->shaderContext.worldViewMtx, &viewMtx, &m);
	crMat44Mult(&app->shaderContext.worldViewProjMtx, &viewProjMtx, &m);
	}

	appShaderContextPreRender(app, sceneMtl);

	meshPreRender(sceneMesh, prog);
	meshRenderTriangles(sceneMesh);
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
	app->shaderContext.matDiffuse = crVec4(0.875f, 0.875f, 0.875f, 1);
	app->shaderContext.matSpecular = crVec4(1.0f, 1.0f, 1.0f, 1);
	app->shaderContext.matShininess = 5;
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
	CrVec3 eyeAt = crVec3(0, 0.5f, 5.0f);
	CrVec3 lookAt = crVec3(0, 0.5f, 0);
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
	drawBackground();
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
	drawBackground();
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
	meshFree(sceneMesh);
	meshFree(waterMesh);
	meshFree(bgMesh);
	materialFree(waterMtl);
	materialFree(sceneMtl);
	materialFree(bgMtl);
	crTextureFree(logo);
	crTextureFree(rainbow);
	crTextureFree(cloud);
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
		"MJApp.Bg.Vertex",
		"MJApp.Bg.Fragment",
		nullptr, nullptr, nullptr);
	
	appLoadMaterialEnd(app);
	}

	// textures
	logo = Pvr_createTexture(Logo);
	rainbow = Pvr_createTexture(Rainbow);
	cloud = Pvr_createTexture(Cloud);
	waterNormalMap = Pvr_createTexture(WaterNormalMap);
	waterFlowMap = Pvr_createTexture(WaterFlowMap);

	crDbgStr("create scene color buffers\n");
	refractTex = crTextureAlloc();
	crTextureInitRtt(refractTex, 512, 512, 0, 1, CrGpuFormat_UnormR8G8B8A8);
	
	crDbgStr("create scene depth buffers\n");
	rttDepth = crTextureAlloc();
	crTextureInitRtt(rttDepth, 512, 512, 0, 1, CrGpuFormat_Depth16);

	// logo
	{ CrVec3 offset = crVec3(-0.5f, -0.5f, 0);
	CrVec2 uvs = crVec2(1.0f, -1.0f);
	sceneMesh = meshAlloc();
	meshInitWithQuad(sceneMesh, 1, 1, &offset, &uvs, 1);
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
