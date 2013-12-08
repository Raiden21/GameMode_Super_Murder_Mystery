datablock staticShapeData(RopeShapeData) {
	shapeFile = "Add-Ons/GameMode_Super_Murder_Mystery/res/shapes/rope.dts";
};

datablock staticShapeData(HookShapeData) {
	shapeFile = "Add-Ons/GameMode_Super_Murder_Mystery/res/shapes/hook2.dts";
};

datablock ItemData(HookshotItem) {
	image = HookshotImage;
	shapeFile = $SMM::Path @ "res/shapes/hookImage1.dts";

	uiName = "Hookshot";
	canDrop = true;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

datablock ProjectileData(HookProjectile) {
	projectileShapeName = $SMM::Path @ "res/shapes/hook2.dts";

	directDamage = 0;
	radiusDamage = 0;

	explodeOnPlayerImpact = 0;
	explodeOnDeath = 0;

	lifetime = 5000;
	fadeDelay = 4000;

	isBallistic = 1;
	gravityMod = 1;

	muzzleVelocity = 20;
	velInheritFactor = 1;
};

datablock ShapeBaseImageData(HookshotImage) {
	className = "WeaponImage";
	shapeFile = $SMM::Path @ "res/shapes/hookImage1.dts";

	item = HookshotItem;
	armReady = true;

	projectile = HookProjectile;
	projectileType = Projectile;

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
}

function HookshotHoldingImage::onUse(%this, %obj, %slot) {
	// foo
}

function HookProjectile::onCollision(%this, %obj, %col, %fade, %pos, %normal, %vel) {
	Parent::onCollision(%this, %obj, %col, %fade, %pos, %normal, %vel);

	if (!isObject(%obj.sourceObject)) {
		return;
	}

	talk("Player" SPC %obj.sourceObject SPC "hit a hookshot at" SPC %pos SPC "with velocity" SPC %vel);
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

		a = %a;
		b = %b;
	};

	MissionCleanup.add(%obj);
	%obj.setNodeColor("ALL", "0 0 0 1");

	return %obj;
}

function createHook(%position, %vector) {
	%obj = new StaticShape() {
		datablock = HookShapeData;
		scale = "0.5 0.5 0.5";

		_position = %position;
		_vector = %vector;
	};

	MissionCleanup.add(%obj);

	%obj.setTransform(%position SPC vectorToAxis(%vector));
	%obj.setNodeColor("ALL", "0.3 0.3 0.3 1");

	return %obj;
}

function findRopePoint(%pos, %vec) {
	%vec = vectorNormalize(%vec);

	if (getWord(%vec, 2) < 0) {
		%vec = setWord(%vec, 2, 0);
	}

	%ray = containerRayCast(%pos, vectorAdd(%pos, vectorScale(%vec, 10)), $TypeMasks::FxBrickObjectType);

	if (%ray $= 0) {
		return "";
	}

	%pos = getWords(%ray, 1, 3);
	%pos = vectorAdd(%pos, vectorScale(%vec, 0.5));

	%ray = containerRayCast(
		vectorAdd(%pos, "0 0 5"),
		vectorSub(%pos, "0 0 0"),
		$TypeMasks::FxBrickObjectType
	);

	if (%ray $= 0) {
		return "";
	}

	return getWords(%ray, 1, 3);
}

function player::test(%this,%pos) {
	cancel(%this.test);

	%vec1 = vectorNormalize(vectorSub(%this.position, %pos));
	%vec2 = vectorNormalize(vectorSub(%pos, %this.position));

	%a = findRopePoint(%pos, %vec1);
	%b = findRopePoint(%this.position, %vec2);

	if (%a !$= "" && %b !$= "") {
		%a = vectorAdd(%a, "0 0 0.05");
		%b = vectorAdd(%b, "0 0 0.05");

		if (isObject(%this.rope) && (%this.rope.a !$= %a || %this.rope.b !$= %b)) {
			%this.rope.delete();
		}

		if (!isObject(%this.rope)) {
			%this.rope = createRope(%a, %b);
		}

		if (isObject(%this.hook1)) {
			if (%this.hook1._position !$= %a || %this.hook1._vector !$= %vec2) {
				%this.hook1._position = %a;
				%this.hook1._vector = %vec2;

				%this.hook1.setTransform(%a SPC vectorToAxis(%vec2));
			}
		}
		else {
			%this.hook1 = createRope(%a, %vec2);
		}

		if (isObject(%this.hook2)) {
			if (%this.hook2._position !$= %b || %this.hook2._vector !$= %vec1) {
				%this.hook2._position = %b;
				%this.hook2._vector = %vec1;

				%this.hook2.setTransform(%b SPC vectorToAxis(%vec1));
			}
		}
		else {
			%this.hook2 = createRope(%b, %vec1);
		}
	}

	%this.test=%this.schedule(50,test,%pos);
}

function markPoint(%a,%t) {
	//%o = new projectile(){datablock=pongprojectile;initialposition=%a;initialvelocity="0 0 0";scale="0.75 0.75 0.75";};
	%o = new projectile(){datablock=pongprojectile;initialposition=%a;initialvelocity="0 0 0";scale="1.25 1.25 1.25";};
	missioncleanup.add(%o);
	%o.schedule(%t $= "" ? 10000 : %t, delete);
}

function vectorToAxis(%vector) {
    %y = mRadToDeg(mACos(getWord(%vector, 2) / vectorLen(%vector))) % 360;
    %z = mRadToDeg(mATan(getWord(%vector, 1), getWord(%vector, 0)));

    %euler = vectorScale(0 SPC %y SPC %z, $pi / 180);
    return getWords(matrixCreateFromEuler(%euler), 3, 6);
}