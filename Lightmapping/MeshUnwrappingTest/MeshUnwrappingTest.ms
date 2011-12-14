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

fn test1 = 
(
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
	
	local mesh = meshbuilder.ToMesh()
	format "final mesh:%f %v\n" mesh.FaceCount mesh.VertexCount
	
	format "perform packing...\n"
	local pmesh = unwrapper.Unwrap mesh 512 (1.0 / 5.0)
	format "output % meshes\n" pmesh.Count
	
	MESHVISUALIZER = dotNetClass "MCD.MeshVisualizer"
	MESHVISUALIZER.DrawTexcrd1 pmesh ((GetDir #export)+"/test.png") 512 512
)

if dllLoaded then
(
	test1()
)