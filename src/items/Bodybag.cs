datablock ShapeBaseImageData(BodybagFullImage) {
	shapeFile = $SMM::Path @ "res/shapes/bodybag.dts";
	mountPoint = 2;
	armReady = false;

	doColorShift = true;
	colorShiftColor = "0.25 0.25 0.3 1";
};

datablock ShapeBaseImageData(BodybagImage) {
	shapeFile = $SMM::Path @ "res/shapes/bodybag_item.dts";

	item = BodybagItem;
	armReady = true;

	doColorShift = true;
	colorShiftColor = "0.25 0.25 0.3 1";

	stateName[0] = "Ready";
	stateAllowImageChange[0] = true;
	stateTransitionOnTriggerDown[0] = "Use";

	stateName[1] = "Use";
	stateScript[1] = "onUse";
	stateAllowImageChange[1] = true;
	stateTransitionOnTriggerUp[1] = "Ready";
};

datablock ItemData(BodybagItem) {
	image = BodybagImage;
	shapeFile = $SMM::Path @ "res/shapes/bodybag_item.dts";

	uiName = "Bodybag";
	canDrop = true;

	doColorShift = true;
	colorShiftColor = "0.25 0.25 0.3 1";

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

function BodybagImage::onMount(%this, %obj, %slot) {
	%obj.playThread(1, "armReadyBoth");
}

function BodybagImage::onUnMount(%this, %obj, %slot) {
	// %obj.playThread(1, "root");
}

function BodybagImage::onUse(%this, %obj, %slot) {
	if($Sim::Time - %obj.lastBodybagUse <= 0.5) {
		return;
	}
	%point = %obj.getEyePoint();
	%vector = %obj.getEyeVector();

	%ray = containerRayCast(%point,
		vectorAdd(%point, vectorScale(%vector, 7)),
		$TypeMasks::PlayerObjectType,
		%obj
	);

	if (isObject(%col = getWord(%ray, 0))) {
		if(!%col.isCorpse || !isObject(%col.originalClient)) {
			return;
		}
		if(!%col.isWrapped) {
			%col.mountImage(BodybagFullImage, 2);
			%col.isWrapped = true;
			%col.hideNode("ALL");
			serverPlay3d(bodybagZipSound, %col.getHackPosition());
			// %obj.tool[%obj.currTool] = "";
			// %obj.weaponCount--;
			// %obj.unMountImage(%obj.currTool);
		}
		else {
			%col.unMountImage(2);
			%col.isWrapped = false;
			%col.unHideNode("ALL");
			%player = %col.originalClient.player;
			%col.originalClient.player = %col;
			%col.originalClient.applyBodyParts();
			%col.originalClient.applyBodyColors();
			%col.originalClient.player = %player;
			serverPlay3d(bodybagUnzipSound, %col.getHackPosition());
		}
		%obj.lastBodybagUse = $Sim::Time;
	}
}