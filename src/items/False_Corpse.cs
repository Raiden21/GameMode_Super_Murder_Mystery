datablock ShapeBaseImageData(FalseCorpseImage) {
	shapeFile = "base/data/shapes/printGun.dts";

	item = FalseCorpseItem;
	armReady = true;

	stateName[0] = "Ready";
	stateAllowImageChange[0] = true;
	stateTransitionOnTriggerDown[0] = "Use";

	stateName[1] = "Use";
	stateScript[1] = "onUse";
	stateAllowImageChange[1] = false;
	stateTransitionOnTriggerUp[1] = "Ready";
};

datablock ItemData(FalseCorpseItem) {
	image = FalseCorpseImage;
	shapeFile = "base/data/shapes/printGun.dts";

	uiName = "False Corpse";
	canDrop = true;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

function FalseCorpseImage::onUse(%this, %obj, %slot) {
	if (!isObject(%obj.client)) {
		return;
	}

	if (isObject(%obj.client.falseCorpse)) {
		%obj.client.centerPrint("\c6You already have a false corpse.", 2);
		return;
	}

	%corpse = %obj.client.falseCorpse = new AIPlayer() {
		dataBlock = %obj.getDataBlock();

		isCorpse = true;
		isFalseCorpse = true;
		originalClient = %obj.client;
	};

	if (isObject(%corpse)) {
		MissionCleanup.add(%corpse);

		%vehicle = %corpse.corpseVehicle = new WheeledVehicle() {
			dataBlock = CorpseVehicle;

			isCorpseVehicle = true;
			corpse = %this.corpse;
		};

		MissionCleanup.add(%vehicle);

		%pos = vectorAdd(%obj.getHackPosition(), vectorScale(%obj.getForwardVector(), 2));
		%rot = getWords(%obj.getTransform(), 3, 6);

		%vehicle.setTransform(%pos SPC %rot);
		%vehicle.setVelocity(vectorScale(%obj.getForwardVector(), 4));

		%vehicle.mountObject(%corpse, 0);
		%player = %obj.client.player;
		
		%obj.client.player = %corpse;
		%obj.client.applyBodyParts();
		%obj.client.applyBodyColors();

		%obj.client.player = %player;
		%obj.client.centerPrint("\c6You have created a false corpse.", 3);
	}
	else {
		%obj.client.centerPrint("\c6A false corpse could not be created.", 2);
	}
}