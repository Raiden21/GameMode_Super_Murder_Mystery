datablock ShapeBaseImageData(MedicineImage) {
	shapeFile = $SMM::Path @ "res/shapes/suitcase.dts";

	item = MedicineItem;
	armReady = true;

	stateName[0] = "Ready";
	stateAllowImageChange[0] = true;
	stateTransitionOnTriggerDown[0] = "Use";

	stateName[1] = "Use";
	stateScript[1] = "onUse";
	stateAllowImageChange[1] = false;
	stateTransitionOnTriggerUp[1] = "Ready";
};

datablock ItemData(MedicineItem) {
	image = MedicineImage;
	shapeFile = $SMM::Path @ "res/shapes/suitcase.dts";

	uiName = "Medicine";
	canDrop = true;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

function MedicineImage::onMount(%this, %obj, %slot) {
	%obj.playThread(1, "armReadyBoth");
}

function MedicineImage::onUnMount(%this, %obj, %slot) {
	%obj.playThread(1, "root");
}

function MedicineImage::onUse(%this, %obj, %slot) {
	if (!isObject(%obj.client)) {
		return;
	}

	%eyePoint = %obj.getEyePoint();
	%eyeVector = %obj.getEyeVector();

	%ray = containerRayCast(%eyePoint,
		vectorAdd(%eyePoint, vectorScale(%eyeVector, 6)),
		$TypeMasks::PlayerObjectType | $TypeMasks::FxBrickObjectType,
		%obj
	);

	%col = getWord(%ray, 0);

	if (!isObject(%col) || !%col.isCorpse || !isObject(%col.originalClient)) {
		%obj.client.centerPrint("\c6Click on an unconscious player with Medicine to revive them.", 2);
		return;
	}

	if (!%col.isUnconscious) {
		%obj.client.centerPrint("\c6They're dead; there is no hope.", 2);
		return;
	}

	%corpseClient = %col.originalClient;
	%col.reviveCorpse();

	%obj.client.centerPrint("\c6You have revived \c3" @ %corpseClient.getSMMName() @ "\c6.", 3);
	%corpseClient.centerPrint("\c6You have been revived by \c3" @ %obj.client.getSMMName() @ "\c6.", 3);
}