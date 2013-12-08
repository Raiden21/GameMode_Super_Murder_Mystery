datablock staticShapeData(RopeShapeData) {
	shapeFile = "Add-Ons/GameMode_Super_Murder_Mystery/res/shapes/1tu_cylinder.dts";
};

datablock staticShapeData(RopeCollisionShapeData) {
	shapeFile = "Add-Ons/GameMode_Super_Murder_Mystery/res/shapes/1tu_cylinder_collision.dts";
};

datablock staticShapeData(HookShapeData) {
	shapeFile = "Add-Ons/GameMode_Super_Murder_Mystery/res/shapes/hook.dts";
};

datablock ItemData(HookshotItem) {
	image = HookshotImage;
	shapeFile = $SMM::Path @ "res/shapes/hook.dts";

	uiName = "Hookshot";
	canDrop = true;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

datablock ProjectileData(HookProjectile) {
	projectileShapeName = $SMM::Path @ "res/shapes/hook.dts";

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
	shapeFile = $SMM::Path @ "res/shapes/hook.dts";

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
	shapeFile = $SMM::Path @ "res/shapes/hook.dts";

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

function createRope(%a, %b, %collision) {
	%vec1 = vectorNormalize(vectorSub(%a, %b));
	%vec2 = vectorNormalize(vectorSub(%a, %b));

	%xyz = vectorNormalize(vectorCross("1 0 0", %vec1));
	%pow = mRadToDeg(mACos(vectorDot("1 0 0", %vec1))) * -1;

	%obj = new StaticShape() {
		datablock = %collision ? RopeCollisionShapeData : RopeShapeData;
		scale = vectorDist(%a, %b) SPC 0.1 SPC 0.1;

		position = vectorScale(vectorAdd(%a, %b), 0.5);
		rotation = %xyz SPC %pow;

		a = %a;
		b = %b;

		hook1 = createHook(%a, %vec1);
		hook2 = createHook(%b, %vec2);
	};

	MissionCleanup.add(%obj);
	%obj.setNodeColor("ALL", "0.454902 0.313726 0.109804 1.000000");

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

function deleteRope(%rope) {
	%rope.hook1.delete();
	%rope.hook2.delete();

	%rope.delete();
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
			deleteRope(%this.rope);
		}

		if (!isObject(%this.rope)) {
			%this.rope = createRope(%a, %b);
		}
	}

	%this.test = %this.schedule(100, test, %pos);
}

function vectorToAxis(%vector) {
    %y = mRadToDeg(mACos(getWord(%vector, 2) / vectorLen(%vector))) % 360;
    %z = mRadToDeg(mATan(getWord(%vector, 1), getWord(%vector, 0)));

    %euler = vectorScale(0 SPC %y SPC %z, $pi / 180);
    return getWords(matrixCreateFromEuler(%euler), 3, 6);
}