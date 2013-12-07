datablock ShapeBaseImageData(AmmoSupplyImage) {
	shapeFile = $SMM::Path @ "res/shapes/suitcase.dts";

	item = AmmoSupplyItem;
	armReady = true;

	stateName[0] = "Ready";
	stateAllowImageChange[0] = true;
	stateTransitionOnTriggerDown[0] = "Use";

	stateName[1] = "Use";
	stateScript[1] = "onUse";
	stateAllowImageChange[1] = false;
	stateTransitionOnTriggerUp[1] = "Ready";
};

datablock ItemData(AmmoSupplyItem) {
	image = AmmoSupplyImage;
	shapeFile = $SMM::Path @ "res/shapes/suitcase.dts";

	uiName = "Ammo Supply";
	canDrop = true;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

function AmmoSupplyImage::onMount(%this, %obj, %slot) {
	%obj.playThread(1, "armReadyBoth");
}

function AmmoSupplyImage::onUse(%this, %obj, %slot) {
	%slots = %obj.getDataBlock().maxTools;

	for (%i = 0; %i < %slots; %i++) {
		%type = %obj.tool[%i].ammoType;

		if (%type !$= "" && %obj.toolAmmo[%type] < $HL2Weapons::AddAmmo[%type]) {
			%obj.toolAmmo[%type] = $HL2Weapons::AddAmmo[%type];
		}
	}

	if (isObject(%obj.client)) {
		%obj.client.centerPrint("\c6All ammo types refilled.", 2);
	}
}