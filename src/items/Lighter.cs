datablock ShapeBaseImageData(LighterImage) {
	shapeFile = "base/data/shapes/empty.dts";

	item = LighterItem;
	armReady = false;

	stateName[0] = "Ready";
	stateAllowImageChange[0] = true;
	stateTransitionOnTriggerDown[0] = "Use";

	stateName[1] = "Use";
	stateSound[1] = AdvReloadTap0Sound;
	stateTimeoutValue[1] = 0.15;
	stateWaitForTimeout[1] = false;
	stateAllowImageChange[1] = false;
	stateTransitionOnTimeout[1] = "Auto";
	stateTransitionOnTriggerUp[1] = "Ready";

	stateName[2] = "Auto";
	stateSound[2] = AdvReloadTap0Sound;
	stateTimeoutValue[2] = 0.8;
	stateWaitForTimeout[2] = false;
	stateAllowImageChange[2] = false;
	stateTransitionOnTimeout[2] = "Cycle";
	stateTransitionOnTriggerUp[2] = "Ready";

	stateName[3] = "Cycle";
	stateTimeoutValue[3] = 0.001;
	stateWaitForTimeout[3] = false;
	stateAllowImageChange[3] = false;
	stateTransitionOnTimeout[3] = "Auto";
	stateTransitionOnTriggerUp[3] = "Ready";
};

datablock ItemData(LighterItem) {
	image = LighterImage;
	shapeFile = $SMM::Path @ "res/shapes/suitcase.dts";

	uiName = "Clicking Device";
	canDrop = true;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

function LighterImage::onMount(%this, %obj, %slot) {
	%obj.playThread(1, "root");
}