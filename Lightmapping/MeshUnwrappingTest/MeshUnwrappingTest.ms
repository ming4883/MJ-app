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

fn addToBuilder obj builder =
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
			builder.FaceProp mtlid
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

fn toMaxMesh m mname =
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
	
	Unwrapper = Unwrap_UVW name:"UVW"
	Unwrapper.setApplyToWholeObject true
	Unwrapper.setMapChannel 2
	addModifier meshMax Unwrapper
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
	
	for i = 1 to selection.count do
	(
		addToBuilder selection[i] meshbuilder
	)
	
	local fmesh = meshbuilder.ToMesh()
	format "final mesh:%f %v\n" fmesh.FaceCount fmesh.VertexCount
	
	-- local MESH_UTIL = dotNetClass "MCD.MeshUtil"
	-- MESH_UTIL.Save fmesh ((GetDir #export)+"/testmesh.txt")
	
	format "perform packing...\n"
	local pmesh = unwrapper.Unwrap fmesh 512 (1.0 / (2.0 * units.SystemScale))
	format "output % meshes\n" pmesh.Count
	
	format "saving results...\n"
	
	local iter = pmesh.GetEnumerator()
	i = 1
	while iter.MoveNext() do
	(
		local mname = "PackResult"+(i as string)
		toMaxMesh iter.current mname
		i = i + 1
	)
	
)

fn test2 = 
(
	format "% % %\n" units.DisplayType units.MetricType units.SystemScale
	
	if selection.count == 0 then
	(
		format "selection is empty, quit\n"
		return false
	)
	format "performing test2()...\n"
	
	tmesh = editable_mesh()
	tmesh.name = "Merged"
	tmesh.material = multimaterial name:"MergedMaterial"
	
	for i = 1 to selection.count do
	(
		local s = snapshotAsMesh selection[i]
		meshop.attach tmesh s attachMat:#IDToMat
		delete s
	)
	
)

if dllLoaded then
(
	--test1()
	test2()
)