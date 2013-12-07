//$Pref::Server::DecalLimit = 100;
$Pref::Server::DecalLimit = 200;
$Pref::Server::DecalTimeout = "";

function spawnDecal(%dataBlock, %position, %vector, %size, %effect) {
	if (!isObject(MissionCleanup)) {
		error("ERROR: MissionCleanup not created.");
		return -1;
	}

	if (!isObject(%dataBlock) || %dataBlock.getClassName() !$= StaticShapeData) {
		return;
	}

	if (%size $= "") {
		%size = %dataBlock.decalSize;
	}

	if (%size $= "") {
		%size = 1;
	}

	%obj = new StaticShape() {
		dataBlock = %dataBlock;
		spawnTime = $Sim::Time;
	};

	if (!isObject(%obj)) {
		error("ERROR: Unable to create object.");
		return -1;
	}

	if (!isObject(DecalGroupNew)) {
		MissionCleanup.add(new SimGroup(DecalGroupNew));
	}

	DecalGroupNew.add(%obj);

	while (DecalGroupNew.getCount() >= $Pref::Server::DecalLimit) {
		//DecalGroupNew.getObject(0).delete();

		//Port [09-10-13 10:27]: trying to fix decal limit,
		//let's reverse this - since it seems to delete the newest decals

		//DecalGroupNew.getObject(DecalGroupNew.getCount() - 1).delete();
		%count = DecalGroupNew.getCount();

		for (%i = 0; %i < %count; %i++) {
			%decal = DecalGroupNew.getObject(%i);

			if (%decal.spawnTime < %best || %best $= "") {
				%best = %decal.spawnTime;
				%oldest = %decal;
			}
		}

		if (isObject(%oldest)) {
			%oldest.delete();
		}
	}

	if (!isObject(%obj)) {
		return -1;
	}

	%obj.setTransform(%position SPC vectorToAxis(%vector));
	%obj.setScale(%size SPC %size SPC %size);
	%obj.normal = %vector;
	if (%dataBlock.doColorShift) {
		%obj.setNodeColor("ALL", %dataBlock.colorShiftColor);
	}

	if ($Pref::Server::DecalTimeout !$= "" && !%effect) {
		%obj.schedule($Pref::Server::DecalTimeout - 1000, fadeOut);
		%obj.schedule($Pref::Server::DecalTimeout, delete);
	}

	return %obj;
}

function spawnDecalFromRayCast(%dataBlock, %rayCast, %size) {
	if (%rayCast $= 0) {
		return -1;
	}

	return spawnDecal(%dataBlock, getWords(%rayCast, 1, 3), getWords(%rayCast, 4, 6), %size);
}

function sprayDecalWithRayCast(%dataBlock, %a, %b, %mask, %avoid, %size) {
	return spawnDecalFromRayCast(%dataBlock, containerRayCast(%a, %b, %mask, %avoid), %size);
}

function vectorToAxis(%vector) {
	%y = mRadToDeg(mACos(getWord(%vector, 2) / vectorLen(%vector))) % 360;
	%z = mRadToDeg(mATan(getWord(%vector, 1), getWord(%vector, 0)));

	%euler = vectorScale(0 SPC %y SPC %z, $pi / 180);
	return getWords(matrixCreateFromEuler(%euler), 3, 6);
}

function clearDecals()
{
	if (isObject(DecalGroup)) 
		DecalGroup.deleteAll();
}