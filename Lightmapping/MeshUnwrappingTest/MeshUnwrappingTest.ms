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

struct FaceProp (material, smoothing)

fn addToMeshBuilder obj builder faceprops =
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
			
			local mtlid = getFaceMatID tmesh f
			local smgid = getFaceSmoothGroup tmesh f
			append faceprops (FaceProp mtlid smgid)
			
			builder.FaceProp (faceprops.count)
			builder.AddFace()
		)
		
		delete tmesh
	)
	catch
	(
		format "failed to process %\n" obj.name
	)
)

fn copyUV msh buf mapCh =
(
	meshop.setMapSupport msh mapCh true
	meshop.setNumMapVerts msh mapCh buf.Count
	meshop.defaultMapFaces msh mapCh
	
	local iter = buf.GetEnumerator()
	local vcnt = 0
	while iter.MoveNext() do
	(
		local x = iter.current.X
		local y = iter.current.Y
		
		vcnt = vcnt + 1
		
		meshop.setMapVert msh mapCh vcnt [x, y, 0]
	)
)

fn toMaxMesh m faceprops mname =
(
	local vertArray = #()
	local faceArray = #()
	
	local iter
	local vcnt
	
	iter = m.Positions.Raw.GetEnumerator()
	vcnt = 0
	
	while iter.MoveNext() do
	(
		local x = iter.current.X
		local y = iter.current.Y
		local z = iter.current.Z
		
		append vertArray [x, y, z]
		vcnt = vcnt + 1
		
		if vcnt == 3 then
		(
			append faceArray [vertArray.count - 2, vertArray.count - 1, vertArray.count]
			vcnt = 0
		)
	)
	
	meshMax = mesh vertices:vertArray faces:faceArray
	meshMax.name = mname
	meshop.setNumMaps meshMax 3 keep:false
	
	copyUV meshMax m.Texcrds0.Raw 1
	copyUV meshMax m.Texcrds1.Raw 2
	
	for f = 1 to faceArray.count do
	(
		local fprop = faceprops[m.FaceProps.Item(f-1)]
		setFaceMatID meshMax f fprop.material
		setFaceSmoothGroup meshMax f fprop.smoothing
	)
	
	update meshMax
	
	Unwrapper = Unwrap_UVW name:"UVW"
	Unwrapper.setApplyToWholeObject true
	Unwrapper.setMapChannel 2
	addModifier meshMax Unwrapper
	
	return meshMax
)

fn test1 = 
(
	format "% % %\n" units.DisplayType units.MetricType units.SystemScale
	
	if selection.count == 0 then
	(
		format "selection is empty, quit\n"
		return false
	)
	format "performing test1()...\n"
	
	local unwrapper = (dotNetObject "MCD.GroupedFaceUnwrapper")
	local meshbuilder = (dotNetObject "MCD.MeshBuilder")
	local faceprops = #()
	
	for i = 1 to selection.count do
	(
		addToMeshBuilder selection[i] meshbuilder faceprops
	)
	
	local fmesh = meshbuilder.ToMesh()
	format "final mesh:%f %v\n" fmesh.FaceCount fmesh.VertexCount
	
	-- local MESH_UTIL = dotNetClass "MCD.MeshUtil"
	-- MESH_UTIL.Save fmesh ((GetDir #export)+"/testmesh.txt")
	
	format "perform packing...\n"
	local packed = unwrapper.Unwrap fmesh 512 (1.0 / (2.0 * units.SystemScale))
	format "output % meshes\n" packed.Count
	
	format "saving results...\n"
	
	for i = 1 to packed.Count do
	(
		local mname = "PackResult"+(i as string)
		local mmesh = toMaxMesh (packed.Item (i-1)) faceprops mname
		
		mmesh.material = merged.material
	)
	
	return true
)

fn test2 = 
(
	format "% % %\n" units.DisplayType units.MetricType units.SystemScale
	local sel = selection as array
	
	if sel.count == 0 then
	(
		format "selection is empty, quit\n"
		return false
	)
	format "performing test2()...\n"
	
	local target = undefined
	
	for i = 1 to sel.count do
	(
		format "processing %\n" sel[i]
		
		if canConvertTo sel[i] Editable_Mesh then
		(
			local curr = snapshot sel[i]
			if undefined == target then
			(
				target = curr
				target.name = "Merged"
			)
			else
			(
				meshop.attach target curr attachMat:#IDToMat condenseMat:true
			)
		)
	)
	
	-- if undefined != target then
	--	delete target
	
	return true
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
	
	Unwrapper = Unwrap_UVW name:"UVW"
	Unwrapper.setApplyToWholeObject true
	Unwrapper.setMapChannel 2
	addModifier meshMax Unwrapper
	
	return meshMax
)

fn test3 = 
(
	local sel = selection as array
	
	if sel.count == 0 then
	(
		format "selection is empty, quit\n"
		return false
	)
	
	format "performing test3()...\n"
	
	local dlg = dotNetObject "MCD.PackInputDialog"
	if false == dlg.DoModal() then
	(
		format "user aborted!\n"
		return false
	)
	
	local console = dotNetClass "System.Console"
	local consoleView = dotNetObject "MCD.ConsoleViewer"
	consoleView.Display()
	
	local mapSize = dlg.MapSize
	local pixRatio = (units.decodeValue dlg.PixelSizeWithUnits)
	console.Write "pixRatio={0}\n" (pixRatio as string)
	
	local merged = undefined
	
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
	
	if undefined == merged then
	(
		console.Write "Merged is undefined, aborted!\n"
		return false
	)
	
	console.Write "{0} {1}\n" (merged.max as string) (merged.min as string)
	
	local unwrapper = (dotNetObject "MCD.GroupedFaceUnwrapper")
	local mbuilder = (dotNetObject "MCD.MeshBuilder")
	
	addToMeshBuilder2 merged mbuilder
	
	local fmesh = mbuilder.ToMesh()
	console.Write "final mesh:f{0} v{1}\n" (fmesh.FaceCount as string) (fmesh.VertexCount as string)
	
	local MESH_UTIL = dotNetClass "MCD.MeshUtil"
	MESH_UTIL.Save fmesh ((GetDir #export)+"/testmesh.txt")
	
	console.Write "perform packing...\n"
	local packed = unwrapper.Unwrap fmesh mapSize (1.0 / pixRatio)
	console.Write "output {0} meshes\n" (packed.Count as string)
	
	console.Write "saving results...\n"
	
	for i = 1 to packed.Count do
	(
		local mname = "PackResult"+(i as string)
		local mmesh = toMaxMesh2 (packed.Item (i-1)) merged mname
		
		mmesh.material = merged.material
	)
	
	-- merged is no longer needed
	delete merged
	
	console.Write "done!\n"

	return true
)


if dllLoaded then
(
	--test1()
	--test2()
	test3()
)