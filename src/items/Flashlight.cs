if ($Pref::Server::FlashlightRange $= "") {
	$Pref::Server::FlashlightRange = 50;
}

if ($Pref::Server::FlashlightRate $= "") {
	$Pref::Server::FlashlightRate = 50;
}

if ($Pref::Server::FlashlightSpeed $= "") {
	$Pref::Server::FlashlightSpeed = 0.54;
}

datablock AudioProfile(FlashlightToggleSound) {
   fileName = $SMM::Path @ "res/sounds/toggle.wav";
   description = AudioClosest3d;
   preload = true;
};

datablock FxLightData(PlayerFlashlight : PlayerLight) {
	uiName = "";
	flareOn = 0;

	radius = 16;
	brightness = 3;
};

datablock ItemData(FlashlightItem) {
	image = FlashlightImage;
	shapeFile = $SMM::Path @ "res/shapes/flashlight.dts";

	uiName = "Flashlight";
	canDrop = true;

	doColorShift = true;
	colorShiftColor = "0.3 0.3 0.35 1";

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

datablock ShapeBaseImageData(FlashlightImage) {
	shapeFile = $SMM::Path @ "res/shapes/flashlight.dts";
	hasLight = true;

	emap = true;
	offset = "0 0 0";
	mountPoint = 0;
	armReady = false;

	doColorShift = true;
	colorShiftColor = "0.3 0.3 0.35 1";

	stateName[0] = "Ready";
	stateAllowImageChange[0] = true;
	stateTransitionOnTriggerDown[0] = "Use";

	stateName[1] = "Use";
	stateScript[1] = "onUse";
	stateAllowImageChange[1] = false;
	stateTransitionOnTriggerUp[1] = "Ready";
};

datablock ShapeBaseImageData(FlashlightMountedImage : FlashLightImage) {
	mountPoint = 1;
};


function flashLightImage::onUse(%this, %obj, %slot){
	if (!isObject(%obj) || %obj.getState() $= "Dead") {
		return;
	}

	if (getSimTime() - %obj.lastLightTime < 250) {
		return;
	}

	%obj.lastLightTime = getSimTime();
	serverPlay3D(FlashlightToggleSound, %obj.getHackPosition());

	if (isObject(%obj.light)) {
		%obj.light.delete();

		if (%obj.getMountedImage(1) == nameToID("FlashlightMountedImage")) {
			%obj.unMountImage(1);
		}
	}
	else {
		%obj.light = new FxLight() {
			datablock = playerFlashlight;
			obj = %obj;

			iconSize = 1;
			enable = 1;
		};

		missionCleanup.add(%obj.light);
		%obj.light.setTransform(%obj.getTransform());

		if (!isEventPending(%obj.flashlightTick)) {
			%obj.flashlightTick();
		}
	}
}

function Player::flashlightTick(%this) {
	cancel(%this.flashlightTick);

	if (%this.getState() $= "Dead" || !isObject(%this.light)) {
		return;
	}

	%start = %this.getMuzzlePoint(1);
	%vector = %this.getEyeVector();

	%range = $Pref::Server::FlashlightRange;

	if ($EnvGuiServer::VisibleDistance !$= "") {
		%limit = $EnvGuiServer::VisibleDistance / 2;

		if (%range > %limit) {
			%range = %limit;
		}
	}

	%end = vectorAdd(%start, vectorScale(%vector, %range));
	%end = vectorAdd(%end, %this.getVelocity());

	%mask = 0;

	%mask |= $TypeMasks::StaticShapeObjectType;
	%mask |= $TypeMasks::FxBrickObjectType;
	%mask |= $TypeMasks::VehicleObjectType;
	%mask |= $TypeMasks::TerrainObjectType;
	%mask |= $TypeMasks::PlayerObjectType;

	%ray = containerRayCast(%start, %end, %mask, %this);

	if (%ray $= "0") {
		%pos = %end;
	}
	else {
		%pos = vectorSub(getWords(%ray, 1, 3), %vector);
	}

	%path = vectorSub(%pos, %this.light.position);
	%length = vectorLen(%path);

	if (%length < $Pref::Server::FlashlightSpeed) {
		%pos = %pos;
	}
	else {
		%moved = vectorScale(%path, $Pref::Server::FlashlightSpeed);
		%pos = vectorAdd(%this.light.position, %moved);
	}

	%this.light.setTransform(%pos);
	%this.light.reset();

	%this.flashlightTick = %this.schedule($Pref::Server::FlashlightRate, "flashlightTick");
}

function flashLightImage::onUnMount(%this, %obj, %slot) {
	%obj.playThread(1, "root");
	if (!isObject(%obj.getMountedImage(1)) && isObject(%obj.light)) {
		%obj.mountImage("flashLightMountedImage", 1);
	}
}

function flashLightImage::onMount(%this, %obj, %slot) {
	if (isObject(%obj.getMountedImage(1)) && isObject(%obj.light)) {
		%obj.unMountImage(1);
	}
}