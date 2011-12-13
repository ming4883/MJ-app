-- http://docs.autodesk.com/3DSMAX/14/ENU/MAXScript%20Help%202012/index.html?url=files/GUID-A97DF4E3-04A7-4A91-AE7C-3C75A8BAA39-1071.htm,topicNumber=d28e359820

fn exportMesh obj out_path =
(
	local tmesh = snapshotAsMesh obj
	local out_file = createfile out_path
	
	-- export vertex
	vert_cnt = meshop.getNumVerts tmesh
	format "% vertex\n" vert_cnt to:out_file
	
	for v = 1 to vert_cnt do
	(
		vert = getVert tmesh v
		format "%\n" vert to:out_file
	)
	
	-- export faces
	face_cnt = meshop.getNumFaces tmesh
	format "% face\n" face_cnt to:out_file
	
	for f = 1 to face_cnt do
	(
		face = getFace tmesh f
		rnormals = meshop.getFaceRNormals tmesh f
		mtl = getFaceMatID tmesh f
		format "% % %\n" face rnormals mtl to:out_file
	)
	
	-- export mapping info
	map_cnt = meshop.getNumMaps tmesh
	
	for map_ch = 1 to map_cnt do
	(
		if (meshop.getMapSupport tmesh map_ch) then
		(
			format "% map\n" map_ch to:out_file
			
			vert_cnt = meshop.getNumMapVerts tmesh map_ch
			format "% map vertex\n" vert_cnt to:out_file
			
			for v = 1 to vert_cnt do
			(
				vert = meshop.getMapVert tmesh map_ch v
				format "%\n" vert to:out_file
			)
			
			face_cnt = meshop.getNumMapFaces tmesh map_ch
			format "% map face\n" face_cnt to:out_file
			
			for f = 1 to face_cnt do
			(
				face = meshop.getMapFace tmesh map_ch f
				format "%\n" face to:out_file
			)
		)
	)
	
	close out_file
	delete tmesh
)

if selection.count > 0 then
(
	op = ((GetDir #export)+"/testmesh.txt")
	exportMesh selection[1] op
	edit op
)