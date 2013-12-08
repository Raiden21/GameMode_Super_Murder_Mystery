datablock staticShapeData(RopeShapeData) {
	shapeFile = "Add-Ons/GameMode_Super_Murder_Mystery/res/shapes/rope.dts";
};

datablock ItemData(HookshotItem) {
	image = HookshotImage;
	shapeFile = $SMM::Path @ "res/shapes/hookshotImage1.dts";

	uiName = "Hookshot";
	canDrop = true;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

datablock ProjectileData(HookProjectile) {
	projectileShapeName = $SMM::Path @ "res/shapes/hookshotImage1.dts";

	directDamage = 0;
	radiusDamage = 0;

	explodeOnPlayerImpact = 0;
	explodeOnDeath = 0;

	lifetime = 5000;
	fadeDelay = 4000;

	isBallistic = 1;
	gravityMod = 0.35;

	muzzleVelocity = 60;
	velInheritFactor = 1;
};

datablock ShapeBaseImageData(HookshotImage) {
	shapeFile = $SMM::Path @ "res/shapes/hookImage1.dts";

	item = HookshotItem;
	armReady = true;
	projectile = HookshotProjectile;

	stateName[0] = "Ready";
	stateAllowImageChange[0] = 1;
	stateTransitionOnTriggerDown[0] = "Fire";

	stateName[1] = "Fire";
	stateFire[1] = 1;
	stateScript[1] = "onFire";
	stateAllowImageChange[1] = 0;
	stateTransitionOnTriggerUp[1] = "Fired";

	stateName[2] = "Fired";
	stateAllowImageChange[2] = 1;
};

datablock ShapeBaseImageData(HookshotHoldingImage) {
	shapeFile = $SMM::Path @ "res/shapes/hookImage2.dts";

	item = HookshotItem;
	armReady = true;

	stateName[0] = "Ready";
	stateAllowImageChange[0] = 0;
	stateTransitionOnTriggerDown[0] = "Use";

	stateName[1] = "Use";
	stateScript[1] = "onUse";
	stateAllowImageChange[1] = 0;
	stateTransitionOnTriggerUp[1] = "Used";

	stateName[2] = "Used";
	stateAllowImageChange[2] = 1;
};

function HookshotImage::onFire(%this, %obj, %slot) {
	if (%obj.tool[%obj.currTool] != %this.item.getID()) {
		return;
	}

	%projectile = Parent::onFire(%this, %obj, %slot);
	%projectile.dump();
}

function HookshotHoldingImage::onUse(%this, %obj, %slot) {
	// foo
}

function HookshotProjectile::onCollision(%this, %obj, %col, %pos, %vel) {
	Parent::onCollision(%this, %obj, %col, %pos, %vel);
	echo(col);
}

function createRope(%a, %b) {
	%size = 0.1;

	%offset = vectorSub(%a, %b);
	%normal = vectorNormalize(%offset);

	%xyz = vectorNormalize(vectorCross("1 0 0", %normal));
	%pow = mRadToDeg(mACos(vectorDot("1 0 0", %normal))) * -1;

	%obj = new StaticShape() {
		datablock = RopeShapeData;
		scale = vectorScale(vectorLen(%offset) SPC %size SPC %size, 1);

		position = vectorScale(vectorAdd(%a, %b), 0.5);
		rotation = %xyz SPC %pow;
	};

	echo(%obj.scale);

	MissionCleanup.add(%obj);
	%obj.setNodeColor("ALL", "0 0 0 1");

	return %obj;
}

function findRopePoint(%pos, %vec) {
	%vec = vectorNormalize(%vec);

	if (getWord(%vec, 2) < 0) {
		%vec = setWord(%vec, 2, 0);
	}

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

		if(isobject(%this.r1))%this.r1.delete();
		if(isobject(%this.r2))%this.r2.delete();
		if(isobject(%this.r3))%this.r3.delete();

		
		%this.r1=createRope(%a2, %b2);
		%this.r2=createRope(%a1, %a2);
		%this.r3=createRope(%b1, %b2);
	}

	%this.test=%this.schedule(200,test,%pos);
}

function markPoint(%a,%t) {
	//%o = new projectile(){datablock=pongprojectile;initialposition=%a;initialvelocity="0 0 0";scale="0.75 0.75 0.75";};
	%o = new projectile(){datablock=pongprojectile;initialposition=%a;initialvelocity="0 0 0";scale="1.25 1.25 1.25";};
	missioncleanup.add(%o);
	%o.schedule(%t $= "" ? 10000 : %t, delete);
}