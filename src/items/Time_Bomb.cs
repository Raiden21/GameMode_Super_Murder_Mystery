datablock ShapeBaseImageData(TimeBombImage) {
	shapeFile = $SMM::Path @ "res/shapes/suitcase.dts";

	item = TimeBombItem;
	armReady = false;

	stateName[0] = "Ready";
	stateAllowImageChange[0] = true;
	stateTransitionOnTriggerDown[0] = "StartFuse";

	stateName[1] = "StartFuse";
	stateScript[1] = "onStartFuse";
	stateAllowImageChange[1] = false;
	stateTransitionOnTriggerUp[1] = "EndFuse";

	stateName[2] = "EndFuse";
	stateScript[2] = "onEndFuse";
	stateTimeoutValue[2] = 0.1;
	stateAllowImageChange[2] = false;
	stateTransitionOnTimeout[2] = "Ready";
};

datablock ItemData(TimeBombItem) {
	image = TimeBombImage;
	shapeFile = $SMM::Path @ "res/shapes/suitcase.dts";

	uiName = "Time Bomb";
	smmReplaceUIName = "Locked Suitcase";

	canDrop = true;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

function TimeBombImage::onMount(%this, %obj, %slot) {
	%obj.playThread(1, "armReadyBoth");
}

function TimeBombImage::onUnMount(%this, %obj, %slot) {
	%obj.playThread(1, "root");
}

function TimeBombImage::onStartFuse(%this, %obj, %slot) {
	%obj.timeBombStartFuse[%obj.currTool] = $Sim::Time;
	%obj.displayTimeBombFuse();
}

function TimeBombImage::onEndFuse(%this, %obj, %slot) {
	cancel(%obj.displayTimeBombFuse);
	%time = 30 + mClampF(($Sim::Time - %obj.timeBombStartFuse[%obj.currTool]) * 5, 0, 90);

	if (isObject(%obj.client)) {
		%message = "<font:impact:24><color:FF6666>Fuse set!";
		%message = %message NL "<font:impact:32>\c6" @ mFloatLength(%time, 0) @ " seconds";

		%obj.client.centerPrint(%message, 2);
	}

	%obj.timeBombStartFuse[%obj.currTool] = "";
	%obj.tool[%obj.currTool] = nameToID("LockedSuitcaseItem");

	%obj.timeBombEnd[%obj.currTool] = $Sim::Time + %time;
	%obj.timeBombSchedule[%obj.currTool] = %obj.schedule(%time * 1000 - 4000, timeBombExplode, 0, %slot);

	if (isObject(%obj.client)) {
		serverCmdUseTool(%obj.client, %obj.currTool);
	}
}

function Player::displayTimeBombFuse(%this) {
	cancel(%this.displayTimeBombFuse);

	if (!isObject(%this.client) || %this.timeBombStartFuse[%this.currTool] $= "") {
		return;
	}

	%time = 30 + mClampF(($Sim::Time - %this.timeBombStartFuse[%this.currTool]) * 5, 0, 90);
	%time = mFloatLength(%time, 0);

	%message = "<font:impact:24>\c3Setting fuse";
	%message = %message NL "<font:impact:32>\c6" @ %time @ " seconds";
	//%message = %message NL "\n<font:impact:16>\c6(press \c3Jet \c6to add 10 seconds)";

	%this.client.centerPrint(%message, 0.5);
	%this.displayTimeBombFuse = %this.schedule(150, displayTimeBombFuse);
}

function Player::startTimeBombSchedule(%this, %time, %slot) {
	%time *= 1000;

	if (%time < 1000) {
		%this.timeBombSchedule[%slot] = %this.schedule(%time, timeBombExplode, 3, %slot);
	}
	else if (%time < 4000) {
		%stage = mCeil((4000 - %time) / 1000);
		%delay = %time - 1000 * mFloor(%time / 1000);

		%this.timeBombSchedule[%slot] = %this.schedule(%delay, timeBombExplode, %stage, %slot);
	}
	else {
		%this.timeBombSchedule[%slot] = %this.schedule(%time - 4000, timeBombExplode, 0, %slot);
	}
}

function Player::timeBombExplode(%this, %stage, %slot) {
	cancel(%this.timeBombSchedule[%slot]);

	if (%stage > 3) {
		%this.hasBeenUnconscious = true;
		%this.spawnExplosion(RocketLauncherProjectile, 2);
	}
	else {
		serverPlay3D(AdvReloadTap0Sound, %this.getHackPosition());
		%this.timeBombSchedule[%slot] = %this.schedule(1000, timeBombExplode, %stage++, %slot);
	}
}

function Item::startTimeBombSchedule(%this, %time) {
	%time *= 1000;

	if (%time < 0) {
		%this.delete();
		return;
	}
	else if (%time < 1000) {
		%this.timeBombSchedule[%slot] = %this.schedule(%time, timeBombExplode, 3);
	}
	else if (%time < 4000) {
		%stage = mCeil((4000 - %time) / 1000);
		%delay = %time - 1000 * mFloor(%time / 1000);

		%this.timeBombSchedule[%slot] = %this.schedule(%delay, timeBombExplode, %stage);
	}
	else {
		%this.timeBombSchedule[%slot] = %this.schedule(%time - 4000, timeBombExplode, 0);
	}
}

function Item::timeBombExplode(%this, %stage) {
	cancel(%this.timeBombSchedule);

	if (%stage >= 3) {
		%obj = new Projectile() {
			datablock = RocketLauncherProjectile;

			initialPosition = %this.position;
			initialVelocity = "0 0 0";

			client = $DefaultMiniGame.member0;
		};

		MissionCleanup.add(%obj);

		%obj.setScale("2 2 2");
		%obj.explode();

		%this.delete();
	}
	else {
		serverPlay3D(AdvReloadTap0Sound, %this.position);
		%this.timeBombSchedule = %this.schedule(1000, timeBombExplode, %stage++);
	}
}