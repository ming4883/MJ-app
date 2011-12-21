-- http://forums.cgsociety.org/archive/index.php/t-668343.html
ASSEMBLY = dotNetClass "System.Reflection.Assembly"
IO_PATH = dotNetClass "System.IO.Path"

dllPath = IO_PATH.GetFullPath "MeshUnwrapping.dll"
dllLoaded = false

if doesFileExist dllPath then
(
	local uri = "file://"  + dllPath
	format "trying to load Assembly '%'\n" uri
	
	r = ASSEMBLY.LoadFrom uri
	dllLoaded = true
)

fn addToMeshBuilder2 obj builder =
(
	format "processing %...\n" obj.name
	
	try
	(
		if undefined != obj.material then
		(
			format "%\n" obj.material.name
		)
		else
		(
			format "no material\n"
		)
		
		local tmesh = snapshotAsMesh obj
		
		local face_cnt = meshop.getNumFaces tmesh
		format "% faces\n" face_cnt
		
		for f = 1 to face_cnt do
		(
			local face = getFace tmesh f
			local normals = meshop.getFaceRNormals tmesh f
			local vertexs = #()
			local mvertexs = #()
			
			append vertexs (getVert tmesh face[1])
			append vertexs (getVert tmesh face[2])
			append vertexs (getVert tmesh face[3])
			
			map_ch = 1
			if (meshop.getMapSupport tmesh map_ch) then
			(
				local mface = meshop.getMapFace tmesh map_ch f
				append mvertexs (meshop.getMapVert tmesh map_ch mface[1])
				append mvertexs (meshop.getMapVert tmesh map_ch mface[2])
				append mvertexs (meshop.getMapVert tmesh map_ch mface[3])
			)
			else
			(
				append mvertexs [0.0, 0.0, 0.0]
				append mvertexs [0.0, 0.0, 0.0]
				append mvertexs [0.0, 0.0, 0.0]
			)
			
			for v = 1 to 3 do
			(
				builder.Position vertexs[v].x vertexs[v].y vertexs[v].z
				builder.Normal normals[v].x normals[v].y normals[v].z
				builder.Texcrd0 mvertexs[v].x mvertexs[v].y
				builder.AddVertex()
			)
			
			builder.FaceProp f
			builder.AddFace()
		)
		
		delete tmesh
	)
	catch
	(
		format "failed to process %\n" obj.name
	)
)

fn copyUV2 msh buf mapCh =
(
	meshop.setMapSupport msh mapCh true
	
	meshop.buildMapFaces msh mapCh
	meshop.setNumMapVerts msh mapCh buf.Count
	
	local vcnt = 0
	for v = 1 to buf.Count do
	(
		local x = (buf.Item (v-1)).X
		local y = (buf.Item (v-1)).Y
		
		meshop.setMapVert msh mapCh v [x, y, 0]
		
		vcnt = vcnt + 1
		if vcnt == 3 then
		(
			meshop.setMapFace msh mapCh (v/3) [v - 2, v - 1, v]
			vcnt = 0
		)
	)
)

fn toMaxMesh2 m refm mname =
(
	local vertArray = #()
	local faceArray = #()
	
	local vcnt = meshop.getNumVerts refm
	for v = 1 to vcnt do ( append vertArray (meshop.getVert refm v) )
	
	local fcnt = m.FaceCount
	for f = 1 to fcnt do
	(
		local refid = m.FaceProps.Item(f-1)
		append faceArray (getFace refm refid)
	)
	
	meshMax = mesh vertices:vertArray faces:faceArray
	meshMax.name = mname
	meshop.setNumMaps meshMax 3 keep:false
	
	copyUV2 meshMax m.Texcrds0.Raw 1
	copyUV2 meshMax m.Texcrds1.Raw 2
	
	for f = 1 to faceArray.count do
	(
		local refid = m.FaceProps.Item(f-1)
		local mtlid = getFaceMatID refm refid
		local smgid = getFaceSmoothGroup refm refid

		setFaceMatID meshMax f mtlid
		setFaceSmoothGroup meshMax f smgid
	)
	
	update meshMax
	
	return meshMax
)

fn setupRTT tmesh mapsize = 
(
	Unwrapper = Unwrap_UVW name:"UVW"
	Unwrapper.setApplyToWholeObject true
	Unwrapper.setMapChannel 2
	addModifier tmesh Unwrapper
	
	local be = LightingMap()
	be.outputSzX = mapsize
	be.outputSzY = mapsize
	be.targetMapSlotName = "Diffuse Color"
	be.enabled = true
	
	tmesh.iNodeBakeProperties.removeAllBakeElements() 
	tmesh.INodeBakeProperties.addBakeElement be
	tmesh.INodeBakeProperties.bakeChannel = 2 --channel to bake
)

dlg = undefined

fn test3 = 
(
	fn onPack a b = 
	(
		local console = dotNetClass "System.Console"
		
		local sel = selection as array
		
		if sel.count == 0 then
		(
			messageBox "Selection is empty"
			return false
		)
		
		local mapSize = dlg.MapSize
		local pixRatio = (units.decodeValue dlg.PixelSizeWithUnits)
		console.Write "pixRatio={0}\n" (pixRatio as string)
		
		local merged = undefined
		
		undo off
		(
			for i = 1 to sel.count do
			(
				console.Write "processing {0}\n" (sel[i] as string)
				
				if canConvertTo sel[i] Editable_Mesh then
				(
					local curr = snapshot sel[i]
					if undefined == merged then
					(
						merged = curr
						merged.name = "Merged"
					)
					else
					(
						meshop.attach merged curr attachMat:#IDToMat condenseMat:true
					)
				)
			)
		)
		
		if undefined == merged then
		(
			console.Write "Merged is undefined, abort!\n"
			return false
		)
		
		console.Write "Bounding Box = min:{0} max:{1}\n" (merged.min as string) (merged.max as string)
		
		local unwrapper = (dotNetObject "MCD.GroupedFaceUnwrapper")
		local mbuilder = (dotNetObject "MCD.MeshBuilder")
		
		addToMeshBuilder2 merged mbuilder
		
		local fmesh = mbuilder.ToMesh()
		console.Write "final mesh:f{0} v{1}\n" (fmesh.FaceCount as string) (fmesh.VertexCount as string)
		
		-- local MESH_UTIL = dotNetClass "MCD.MeshUtil"
		-- MESH_UTIL.Save fmesh ((GetDir #export)+"/testmesh.txt")
		
		console.Write "perform packing...\n"
		unwrapper.Border = dlg.BorderSize
		local packed = unwrapper.Unwrap fmesh (mapSize / 2) (1.0 / pixRatio)
		
		if mapSize > unwrapper.OutputSize then ( mapSize = unwrapper.OutputSize )
		
		console.Write "output cnt={0} opt-size={1}\n" (packed.Count as string) (unwrapper.OutputSize as string)
		
		undo on
		(
			console.Write "saving results...\n"
			
			for i = 1 to packed.Count do
			(
				--local mname = dlg.OutputName +(i as string)
				local mname = dlg.OutputName
				local mmesh = toMaxMesh2 (packed.Item (i-1)) merged mname
				
				setupRTT mmesh mapSize
				
				mmesh.material = merged.material
			)
		)
		
		undo off
		(
			-- merged is no longer needed
			try
			(
				delete merged
			)
			catch
			(
			)
		)
		
		console.Write "done!\n"
	)
	
	format "performing test3()...\n"
	
	dlg = dotNetObject "MCD.PackDialog"
	dlg.Text = "Lightmap Packing"
	dotNet.addEventHandler dlg "Pack" onPack
	dlg.DoModel()
	
	return true
)

if dllLoaded then
(
	test3()
)