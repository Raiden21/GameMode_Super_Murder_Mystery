datablock ShapeBaseImageData(SuitcaseImage) {
	shapeFile = $SMM::Path @ "res/shapes/suitcase.dts";

	item = SuitcaseItem;
	armReady = false;

	stateName[0] = "Ready";
	stateAllowImageChange[0] = true;
	stateTransitionOnTriggerDown[0] = "Use";

	stateName[1] = "Use";
	stateScript[1] = "onUse";
	stateAllowImageChange[1] = true;
	stateTransitionOnTriggerUp[1] = "Ready";
};

datablock ItemData(SuitcaseItem) {
	image = SuitcaseImage;
	shapeFile = $SMM::Path @ "res/shapes/suitcase.dts";

	uiName = "Suitcase";
	canDrop = true;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	isSuitcase = true;
};

datablock ItemData(LockedSuitcaseItem : SuitcaseItem) {
	uiName = "Locked Suitcase";
	isLockedSuitcase = true;
};

function SuitcaseImage::onMount(%this, %obj, %slot) {
	%obj.playThread(1, "armReadyBoth");
}

function SuitcaseImage::onUnMount(%this, %obj, %slot) {
	%obj.playThread(1, "root");
}

function SuitcaseImage::onUse(%this, %obj, %slot) {
	%tool = %obj.suitcaseTool[%obj.currTool];

	if (!%obj.tool[%obj.currTool].isSuitcase || !isObject(%tool)) {
		if (isObject(%obj.client)) {
			%obj.client.centerPrint("\c6This suitcase cannot be opened.", 2);
		}

		return;
	}

	%tool = %tool.getID();

	if (%obj.tool[%obj.currTool].isLockedSuitcase) {
		%slots = %obj.getDataBlock().maxTools;

		for (%i = 0; %i < %slots; %i++) {
			if (%obj.tool[%i].isSuitcaseKey) {
				break;
			}
		}

		if (%i == %slots) {
			if (isObject(%obj.client)) {
				%obj.client.centerPrint("\c6You need a \c3Suitcase Key \c6to open this locked suitcase.", 2);
			}

			return;
		}
	}

	%obj.tool[%obj.currTool] = %tool;
	%obj.suitcaseTool[%obj.currTool] = "";

	if (isObject(%obj.client)) {
		messageClient(%obj.client, 'MsgItemPickup', "", %obj.currTool, %tool);
		%obj.client.centerPrint("\c6You opened the suitcase to find a(n) \c3" @ %tool.uiName @ "\c6.", 3);

		serverCmdUseTool(%obj.client, %obj.currTool);
	}
}

function SuitcaseItem::onAdd(%this, %obj) {
	Parent::onAdd(%this, %obj);

	if (isObject($SuitcaseTool)) {
		%obj.suitcaseTool = $SuitcaseTool;
		$SuitcaseTool = "";
	}
	else {
		%obj.suitcaseTool = pickSuitcaseTool(%this.isLockedSuitcase);
	}
}

function SuitcaseItem::onPickup(%this, %obj, %player) {
	if (!isObject(%obj.suitcaseTool)) {
		return 0;
	}

	%tool = %obj.suitcaseTool;
	%slots = %player.getDataBlock().maxTools;

	for (%i = 0; %i < %slots; %i++) {
		if (%player.tool[%i].isSuitcase) {
			%ignore[%i] = true;
		}
	}

	%parent = Parent::onPickup(%this, %obj, %player);

	if (%parent) {
		for (%i = 0; %i < %slots; %i++) {
			if (!%ignore[%i] && %player.tool[%i].isSuitcase) {
				%player.suitcaseTool[%i] = %tool;
				break;
			}
		}
	}

	// if (isObject(%obj.spawnBrick)) {
	// 	%obj.spawnBrick.setItem(0);
	// }

	return %parent;
}

function LockedSuitcaseItem::onAdd(%this, %obj) {
	if ($TimeBombEnd !$= "") {
		%obj.timeBombEnd = $TimeBombEnd;
		%obj.startTimeBombSchedule($TimeBombEnd - $Sim::Time);

		$TimeBombEnd = "";
	}

	if (isEventPending($TimeBombSchedule)) {
		cancel($TimeBombSchedule);
		$TimeBombSchedule = "";
	}

	SuitcaseItem::onAdd(%this, %obj);

	if (%obj.timeBombEnd !$= "") {
		%obj.suitcaseTool = "";
	}
}

function LockedSuitcaseItem::onPickup(%this, %obj, %player) {
	if (%obj.timeBombEnd !$= "") {
		%end = %obj.timeBombEnd;
		%schedule = %obj.timeBombSchedule;

		%slots = %player.getDataBlock().maxTools;

		for (%i = 0; %i < %slots; %i++) {
			if (%player.tool[%i].isLockedSuitcase) {
				%ignore[%i] = true;
			}
		}

		%parent = Parent::onPickup(%this, %obj, %player);

		if (%parent) {
			cancel(%schedule);

			for (%i = 0; %i < %slots; %i++) {
				if (!%ignore[%i] && %player.tool[%i].isLockedSuitcase) {
					%player.timeBombEnd[%i] = %end;
					%player.startTimeBombSchedule(%end - $Sim::Time, %i);

					break;
				}
			}
		}

		return %parent;
	}

	return SuitcaseItem::onPickup(%this, %obj, %player);
}

function pickSuitcaseTool(%isLocked) {
	if (%isLocked) {
		%choices = "assaultRifleItem doubleShotgunItem sniperRifleItem fieldRifleItem combatRifleItem stealthPistolItem goldenGunItem";
		%choices = %choices TAB "disguiseItem medicineItem ammoSupplyItem recoveryDeviceItem suitcaseKeyItem cloakItem bodybagItem";
		%choices = %choices TAB "falseCorpseItem flashlightItem hookshotItem paintItem timeBombItem";
	}
	else {
		%choices = "carbineSMGItem gamePistolItem automaticPistolItem singleShotgunItem fieldRifleItem";
		%choices = %choices TAB "suitcaseKeyItem papersItem lighterItem bodybagItem flashlightItem taserItem paintItem hookshotItem";
	}

	%choice = getWord(%choices, getRandom(0, getWordCount(%choices) - 1));
	return isObject(%choice) ? %choice : bodybagItem;
}