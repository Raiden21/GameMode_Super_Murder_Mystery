datablock ShapeBaseImageData(DisguiseImage) {
	shapeFile = $SMM::Path @ "res/shapes/suitcase.dts";
	allowRandomDisguise = 0;

	item = DisguiseItem;
	armReady = true;

	stateName[0] = "Ready";
	stateAllowImageChange[0] = true;
	stateTransitionOnTriggerDown[0] = "Use";

	stateName[1] = "Use";
	stateScript[1] = "onUse";
	stateAllowImageChange[1] = false;
	stateTransitionOnTriggerUp[1] = "Ready";
};

datablock ItemData(DisguiseItem) {
	image = DisguiseImage;
	shapeFile = $SMM::Path @ "res/shapes/suitcase.dts";

	uiName = "Disguise";
	canDrop = true;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

function DisguiseImage::onMount(%this, %obj, %slot) {
	%obj.playThread(1, "armReadyBoth");
}

function DisguiseImage::onUse(%this, %obj, %slot) {
	if (!isObject(%obj.client)) {
		return;
	}

	if (isObject(%obj.client.smmDisguiseTarget)) {
		%obj.client.smmDisguiseTarget = "";
		%obj.client.centerPrint("\c6You have dropped your disguise.", 2);

		%obj.client.applyBodyParts();
		%obj.client.applyBodyColors();
	}
	else {
		%eyePoint = %obj.getEyePoint();
		%eyeVector = %obj.getEyeVector();

		%ray = containerRayCast(%eyePoint,
			vectorAdd(%eyePoint, vectorScale(%eyeVector, 6)),
			$TypeMasks::PlayerObjectType | $TypeMasks::FxBrickObjectType,
			%obj
		);

		%col = getWord(%ray, 0);

		if (%ray $= 0 || !(%col.getType() & $TypeMasks::PlayerObjectType)) {
			%miniGame = %obj.client.miniGame;

			if (%this.allowRandomDisguise && isObject(%miniGame)) {
				for (%i = 0; %i < %miniGame.numMembers; %i++) {
					%client = %miniGame.member[%i];

					if (%client != %obj.client && %client.smmHasAppearance) {
						%targets = %targets @ (%targets $= "" ? "" : " ") @ %client;
					}
				}
			}

			%target = getWord(%targets, getRandom(0, getWordCount(%targets) - 1));
		}
		else {
			%target = %col.isCorpse ? %col.originalClient : %col.client;
		}

		if (!isObject(%target)) {
			%obj.client.centerPrint("\c6There is nobody to disguise as.", 2);
			return;
		}

		%obj.client.smmDisguiseTarget = %target;
		%obj.client.centerPrint("\c6You have disguised as \c3" @ %target.getSMMName() @ "\c6.", 2);

		%obj.client.applyBodyParts();
		%obj.client.applyBodyColors();
	}
}