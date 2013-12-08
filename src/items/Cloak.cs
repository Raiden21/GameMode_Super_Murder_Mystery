$HL2Weapons::AddAmmo["Cloak Energy"] = 2000;
$HL2Weapons::MaxAmmo["Cloak Energy"] = 2000;

datablock ShapeBaseImageData(CloakImage) {
	shapeFile = "base/data/shapes/empty.dts";

	item = CloakItem;
	armReady = false;
};

datablock ItemData(CloakItem) {
	image = CloakImage;
	shapeFile = $SMM::Path @ "res/shapes/suitcase.dts";

	uiName = "Cloak";
	canDrop = true;

	reload = true;
	ammoType = "Cloak Energy";
	maxMag = 2000;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

function CloakImage::onMount(%this, %obj, %slot) {
	%obj.playThread(1, "root");

	if (!isObject(%obj.client)) {
		return;
	}

	hl2AmmoCheck(%this, %obj, %slot);
	// %obj.player.playAudio(0, CloakLoopSound);

	if (!isEventPending(%obj.cloakUpdateTick)) {
		%obj.schedule(500, "cloakUpdateTick", %slot);
	}
}

function CloakImage::onUnMount(%this, %obj, %slot) {
	hl2DisplayAmmo(%this, %obj, %slot, -1);
}

function Player::cloakUpdateTick(%this, %slot) {
	cancel(%this.cloakUpdateTick);
	%shouldCloak = vectorLen(%this.getVelocity()) < 0.01;

	if (%this.getMountedImage(0) != nameToID("CloakImage")) {
		if (%this.isCloaked) {
			%shouldCloak = false;
		}
		else {
			return;
		}
	}
	else if (!%this.toolMag[%this.currTool]) {
		if (%this.isCloaked) {
			%shouldCloak = false;
		}
		else {
			return;
		}
	}

	if (%shouldCloak && !%this.isCloaked) {
		%this.startCloakSchedule = %this.schedule(250, hideNode, "ALL");

		%this.setCloaked(1);
		%this.startFade(0, 0, 1);
	}

	if (!%shouldCloak && %this.isCloaked) {
		cancel(%this.startCloakSchedule);
		
		%this.client.unHideNode("ALL");
		%this.client.applyBodyParts();
		%this.client.applyBodyColors();

		%this.setCloaked(0);
		%this.startFade(0, 0, 0);
	}

	if (%shouldCloak && %this.tool[%this.currTool] == nameToID("CloakItem") && %this.toolMag[%this.currTool]) {
		%this.toolMag[%this.currTool]--;

		if (isObject(%this.client)) {
			%bars = mFloatLength(50 * mClampF(%this.toolMag[%this.currTool] / 2000, 0, 1), 0);
			%message = "<just:center><font:impact:24><color:66FF66>";

			for (%i = 0; %i < %bars; %i++) {
				%message = %message @ "|";
			}

			%message = %message @ "<color:666666>";

			for (%i = 0; %i < 50 - %bars; %i++) {
				%message = %message @ "|";
			}

			%this.client.bottomPrint(%message @ "\n", 0.1, 1);
		}
	}

	%this.isCloaked = %shouldCloak;
	%this.cloakUpdateTick = %this.schedule(50, cloakUpdateTick, %slot);
}