datablock staticShapeData(RopeShapeData) {
	shapeFile = "Add-Ons/GameMode_Super_Murder_Mystery/res/shapes/1tu_cylinder.dts";
};

datablock staticShapeData(RopeCollisionShapeData) {
	shapeFile = "Add-Ons/GameMode_Super_Murder_Mystery/res/shapes/1tu_cylinder_collision.dts";
};

datablock staticShapeData(HookShapeData) {
	shapeFile = "Add-Ons/GameMode_Super_Murder_Mystery/res/shapes/hook.dts";
};

datablock staticShapeData(HookCollisionShapeData) {
	shapeFile = "Add-Ons/GameMode_Super_Murder_Mystery/res/shapes/hookCol.dts";
};

datablock audioProfile(hookHitSound) {
	fileName = $SMM::Path @ "res/sounds/physics/hookHit.wav";
	description = AudioClosest3D;
	preload = true;
};
datablock audioProfile(hookTossSound) {
	fileName = $SMM::Path @ "res/sounds/physics/hookToss.wav";
	description = AudioClosest3D;
	preload = true;
};
datablock audioProfile(hookOffSound) {
	fileName = $SMM::Path @ "res/sounds/physics/hookOff.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock audioProfile(ropeSound1) {
	fileName = $SMM::Path @ "res/sounds/physics/ropeSound1.wav";
	description = AudioClosest3D;
	preload = true;
};
datablock audioProfile(ropeSound2) {
	fileName = $SMM::Path @ "res/sounds/physics/ropeSound2.wav";
	description = AudioClosest3D;
	preload = true;
};
datablock audioProfile(ropeSound3) {
	fileName = $SMM::Path @ "res/sounds/physics/ropeSound3.wav";
	description = AudioClosest3D;
	preload = true;
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
	projectileShapeName = $SMM::Path @ "res/shapes/hookProj.dts";

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

	doColorShift = HookshotItem.doColorShift;
	colorShiftColor = HookshotItem.colorShiftColor;

	stateName[0] = "Activate";
	stateTimeoutValue[0] = 0.1;
	stateSound[0] = weaponSwitchSound;
	stateTransitionOnTimeout[0] = "Ready";

	stateName[1] = "Ready";
	stateAllowImageChange[1] = 1;
	stateTransitionOnTriggerDown[1] = "Fire";

	stateName[2] = "Fire";
	stateScript[2] = "onFire";
	stateAllowImageChange[2] = 1;
	stateTransitionOnTriggerUp[2] = "Fired";

	stateName[3] = "Fired";
	stateAllowImageChange[3] = 0;
	stateTransitionOnAmmo[3] = "Ready";
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

			serverPlay3d(ropeSound @ getRandom(1, 3), %obj.tempRope.a);
			serverPlay3d(ropeSound @ getRandom(1, 3), %obj.tempRope.b);
			%obj.rope = createRope(%obj.tempRope.a, %obj.tempRope.b, 1);
			cancel(%obj.tempRopeLoop);
			deleteRope(%obj.tempRope);
			%obj.playThread(2, spearThrow);
			%obj.playThread(1, root);
			if (%obj.getMountedImage(0) == nameToId("HookshotImage")) {
				%obj.unmountImage(0);
			}
			for (%i = 0; %i < %obj.getDataBlock().maxTools; %i++) {
				if( %obj.tool[%i] == nameToID("hookshotItem")) {
					%obj.tool[%i] = 0;
					%obj.weaponCount--;
					messageClient(%obj.client, 'MsgItemPickup', '', %i, 0);
					break;
				}
			}
		}

		if (%slot == 4 && %state) {
			cancel(%obj.tempRopeLoop);
			%obj.mountImage(HookshotLeftImage, 1);
			if (%obj.getMountedImage(0) == nameToId("HookshotImage")) {
				%obj.setImageAmmo(0, 1);
				// %obj.unmountImage(0);
			}

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
	%obj.playThread(2, leftRecoil);
	serverPlay3d(hookTossSound, %obj.getMuzzlePoint(0));
	%obj.unmountImage(1);
	%obj.setImageAmmo(%slot, 0);
}

function HookProjectile::onCollision(%this, %obj, %col, %fade, %pos, %normal, %vel) {
	Parent::onCollision(%this, %obj, %col, %fade, %pos, %normal, %vel);
	serverPlay3d(hookHitSound, %pos);
	if (isObject(%obj.sourceObject)) {
		%obj.sourceObject.tempRopeLoop(%pos);
	}
}

function Player::tempRopeLoop(%this, %point) {
	cancel(%this.tempRopeLoop);

	if (%this.getState() $= "Dead" || %this.getMountedImage(0) != nameToID("hookshotImage")) {
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

		hook1 = createHook(%a, %vec1, %collision);
		hook2 = createHook(%b, %vec2, %collision);
	};
	%obj.hook1.rope = %obj;
	%obj.hook2.rope = %obj;
	if (!isObject(RopeGroup)) {
		MissionCleanup.add(new SimGroup(RopeGroup));
	}
	MissionCleanup.add(%obj);
	RopeGroup.add(%obj);
	%obj.setNodeColor("ALL", "0.454902 0.313726 0.109804 1.0" ); // SPC %collision ? 1 : 0.9);

	return %obj;
}

function createHook(%position, %vector, %collision) {
	%vector = setWord(%vector, 2, 0);

	%obj = new StaticShape() {
		datablock = %collision ? hookCollisionShapeData : HookShapeData;
	};

	if (!isObject(RopeGroup)) {
		MissionCleanup.add(new SimGroup(RopeGroup));
	}
	RopeGroup.add(%obj);
	MissionCleanup.add(%obj);

	%obj.setTransform(%position SPC vectorToAxis(%vector));
	%obj.setNodeColor("ALL", getWords(HookshotItem.colorShiftColor, 0, 3) SPC 1);//SPC %collision ? 1 : 0.9);

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