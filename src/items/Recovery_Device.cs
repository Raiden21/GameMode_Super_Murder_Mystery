datablock ShapeBaseImageData(RecoveryDeviceImage) {
	shapeFile = $SMM::Path @ "res/shapes/suitcase.dts";

	item = ReceoveryDeviceItem;
	armReady = true;
};

datablock ItemData(RecoveryDeviceItem) {
	image = RecoveryDeviceImage;
	shapeFile = $SMM::Path @ "res/shapes/suitcase.dts";

	uiName = "Recovery Device";
	canDrop = true;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

function RecoveryDeviceImage::onMount(%this, %obj, %slot) {
	%obj.playThread(1, "armReadyBoth");

	if (isObject(%obj.client)) {
		%obj.client.centerPrint("\c6This is a one-use passive item that will revive you 5 times as fast\nWhen knocked unconscious.");
	}
}

function RecoveryDisguiseImage::onUnMount(%this, %obj, %slot) {
	%obj.playThread(1, "root");
	if (isObject(%obj.client)) {
		clearCenterPrint(%obj.client);
	}
}