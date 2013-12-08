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

	doColorShift = 1;
	colorShiftColor = "0.3 0.3 0.3 1.000000";

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

datablock ProjectileData(HookProjectile) {
	projectileShapeName = $SMM::Path @ "res/shapes/hook_proj.dts";

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
	armReady = false;

	projectile = HookProjectile;
	projectileType = Projectile;

	doColorShift = HookshotItem.doColorShift;
	colorShiftColor = HookshotItem.colorShiftColor;

	stateName[0] = "Ready";
	stateAllowImageChange[0] = 1;
	stateTransitionOnTriggerDown[0] = "Fire";

	stateName[1] = "Fire";
	stateScript[1] = "onFire";
	stateAllowImageChange[1] = 1;
	stateTransitionOnTriggerUp[1] = "Fired";

	stateName[2] = "Fired";
	stateAllowImageChange[2] = 1;
};

datablock ShapeBaseImageData(HookshotLeftImage) {
	shapeFile = $SMM::Path @ "res/shapes/hook.dts";
	mountPoint = 1;

	item = HookshotItem;
	armReady = false;

	doColorShift = HookshotItem.doColorShift;
	colorShiftColor = HookshotItem.colorShiftColor;
};

package HookshotPackage {
	function Armor::onTrigger(%this, %obj, %slot, %state) {
		Parent::onTrigger(%this, %obj, %slot, %state);

		if (!isEventPending(%obj.tempRopeLoop)) {
			return;
		}

		if (%slot == 0 && %state && isObject(%obj.tempRope)) {
			if (isObject(%obj.client)) {
				%obj.client.centerPrint("\c3Rope placed.", 1);
			}

			%obj.rope = createRope(%obj.tempRope.a, %obj.tempRope.b, 1);

			cancel(%obj.tempRopeLoop);
			deleteRope(%obj.tempRope);
		}

		if (%slot == 4 && %state) {
			cancel(%obj.tempRopeLoop);

			if (isObject(%obj.tempRope)) {
				deleteRope(%obj.tempRope);
			}
		}
	}
};

activatePackage("HookshotPackage");

function HookshotImage::onMount(%this, %obj, %slot) {
	%obj.mountImage(HookshotLeftImage, 1);
}

function HookshotImage::onUnMount(%this, %obj, %slot) {
	if (%obj.getMountedImage(1) == nameToID("HookshotLeftImage")) {
		%obj.unmountImage(1);
	}
}

function HookshotImage::onFire(%this, %obj, %slot) {
	Parent::onFire(%this, %obj, %slot);
	%obj.unmountImage(%slot);
}

function HookProjectile::onCollision(%this, %obj, %col, %fade, %pos, %normal, %vel) {
	Parent::onCollision(%this, %obj, %col, %fade, %pos, %normal, %vel);

	if (isObject(%obj.sourceObject)) {
		%obj.sourceObject.tempRopeLoop(%pos);
	}
}

function Player::tempRopeLoop(%this, %point) {
	cancel(%this.tempRopeLoop);

	if (%this.getState() $= "Dead") {
		if (isObject(%this.tempRope)) {
			deleteRope(%this.tempRope);
		}

		return;
	}

	%a = calculateHookLatch(%this.position, %point);
	%b = calculateHookLatch(%point, %this.position);

	%va = %a !$= -1 && %a !$= -2;
	%vb = %b !$= -1 && %b !$= -2;

	if (%va && %vb) {
		%a = vectorAdd(%a, "0 0 0.05");
		%b = vectorAdd(%b, "0 0 0.05");

		if (isObject(%this.tempRope)) {
			if (%this.tempRope.a !$= %a || %this.tempRope.b !$= %b) {
				deleteRope(%this.tempRope);
			}
		}

		if (!isObject(%this.tempRope)) {
			%this.tempRope = createRope(%a, %b);
		}

		%message = "\c2Left click to attach rope.\n\c3Right click to cancel.";
	}
	else {
		if (isObject(%this.tempRope)) {
			deleteRope(%this.tempRope);
		}

		if (%a $= -1) %ea = "Cannot find a wall from your side toward the target";
		if (%a $= -2) %ea = "Cannot find top of the wall from your side toward the target";

		if (%b $= -1) %eb = "Cannot find a wall from the target to you";
		if (%b $= -2) %eb = "Cannot find top of the wall from the target to you";

		%message = "\c0No connection can be made." NL %ea NL %eb NL "\c3Right click to cancel.";
	}

	if (isObject(%this.client) && %message !$= "") {
		%this.client.centerPrint(%message, 0.25);
	}

	%this.tempRopeLoop = %this.schedule(100, tempRopeLoop, %point);
}

function createRope(%a, %b, %collision) {
	%vec1 = vectorNormalize(vectorSub(%a, %b));
	%vec2 = vectorNormalize(vectorSub(%b, %a));

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
	%vector = setWord(%vector, 2, 0);

	%obj = new StaticShape() {
		datablock = HookShapeData;
	};

	MissionCleanup.add(%obj);

	%obj.setTransform(%position SPC vectorToAxis(%vector));
	%obj.setNodeColor("ALL", HookshotItem.colorShiftColor);

	return %obj;
}

function deleteRope(%rope) {
	%rope.hook1.delete();
	%rope.hook2.delete();

	%rope.delete();
}

function calculateHookLatch(%pos, %src) {
	%vector = setWord(vectorNormalize(vectorSub(%pos, %src)), 2, 0);
	%ray = containerRayCast(%pos, vectorSub(%pos, vectorScale(%vector, 24)), $TypeMasks::FxBrickObjectType);

	if (%ray $= 0) {
		return -1;
	}

	%pos = vectorSub(getWords(%ray, 1, 3), vectorScale(%vector, 0.499));
	%ray = containerRayCast(vectorAdd(%pos, "0 0 10"), %pos, $TypeMasks::FxBrickObjectType);

	if (%ray $= 0) {
		return -2;
	}

	return getWords(%ray, 1, 3);
}

function vectorToAxis(%vector) {
    %y = mRadToDeg(mACos(getWord(%vector, 2) / vectorLen(%vector))) % 360;
    %z = mRadToDeg(mATan(getWord(%vector, 1), getWord(%vector, 0)));

    %euler = vectorScale(0 SPC %y SPC %z, $pi / 180);
    return getWords(matrixCreateFromEuler(%euler), 3, 6);
}