datablock staticShapeData(RopeShapeData) {
	shapeFile = "Add-Ons/GameMode_Super_Murder_Mystery/res/shapes/rope.dts";
};

function createRope(%a, %b) {
	%size = 0.1;

	%offset = vectorSub(%a, %b);
	%normal = vectorNormalize(%offset);

	%xyz = vectorNormalize(vectorCross("1 0 0", %normal));
	%pow = mRadToDeg(mACos(vectorDot("1 0 0", %normal))) * -1;

	%obj = new StaticShape() {
		datablock = RopeShapeData;
		scale = vectorScale(vectorLen(%offset) SPC %size SPC %size, 20);

		position = vectorScale(vectorAdd(%a, %b), 0.5);
		rotation = %xyz SPC %pow;
	};

	MissionCleanup.add(%obj);
	%obj.setNodeColor("ALL", "0 0 0 1");

	return %obj;
}

function findRopePoint(%pos, %vec) {
	%vec = vectorNormalize(%vec);

	%pointDownDist = 5;
	%findWallDist = 5;
	%findTopDist = 5;

	%ray = containerRayCast(%pos,
		vectorAdd(%pos, vectorScale(%vec, %findWallDist)),
		$TypeMasks::FxBrickObjectType
	);

	if (%ray $= 0) {
		return "";
	}

	%pos = vectorSub(getWords(%ray, 1, 3), vectorScale(%vec, 0.05));

	%ray = containerRayCast(
		vectorAdd(%pos, "0 0 0.1"),
		vectorSub(%pos, "0 0" SPC 0.1 + %pointDownDist),
		$TypeMasks::FxBrickObjectType
	);

	if (%ray $= 0) {
		return "";
	}

	%pos = getWords(%ray, 1, 3);

	%diff = vectorScale(%vec, 0.2);
	%test = vectorAdd(%pos, %diff);

	%ray = containerRayCast(
		vectorAdd(%test, "0 0" SPC %findTopDist),
		vectorSub(%test, "0 0 0.1"),
		$TypeMasks::FxBrickObjectType
	);

	if (%ray $= 0) {
		return "";
	}

	%wall = getWords(%ray, 1, 3);

	%wall = vectorSub(%wall, %diff);
	%wall = vectorAdd(%wall, "0 0 0.05");

	return %pos SPC %wall;
}

function player::test(%this,%pos) {
	cancel(%this.test);

	%a = findRopePoint(%pos, vectorSub(%this.position, %pos));
	%b = findRopePoint(%this.position, vectorSub(%pos, %this.position));

	if (%a !$= "" && %b !$= "") {
		%a1 = getWords(%a, 0, 2);
		%a2 = getWords(%a, 3, 5);

		%b1 = getWords(%b, 0, 2);
		%b2 = getWords(%b, 3, 5);

		createRope(%a2, %b2).schedule(200, delete);
		createRope(%a1, %a2).schedule(200, delete);
		createRope(%b1, %b2).schedule(200, delete);
	}

	%this.test=%this.schedule(200,test,%pos);
}

function markPoint(%a,%t) {
	//%o = new projectile(){datablock=pongprojectile;initialposition=%a;initialvelocity="0 0 0";scale="0.75 0.75 0.75";};
	%o = new projectile(){datablock=pongprojectile;initialposition=%a;initialvelocity="0 0 0";scale="1.25 1.25 1.25";};
	missioncleanup.add(%o);
	%o.schedule(%t $= "" ? 10000 : %t, delete);
}