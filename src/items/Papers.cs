datablock ShapeBaseImageData(PapersImage) {
	shapeFile = $SMM::Path @ "res/shapes/paper.dts";

	item = PapersItem;
	armReady = true;
};

datablock ItemData(PapersItem) {
	image = PapersImage;
	shapeFile = $SMM::Path @ "res/shapes/paper_item.dts";

	uiName = "Papers";
	canDrop = true;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

function PapersImage::onMount(%this, %obj, %slot) {
	if (isObject(%obj.client)) {
		if (%obj.papersNote[%obj.currTool] $= "") {
			if (%obj.client.isMafia) {
				%obj.papersNote[%obj.currTool] = "Remember your call; you're with the mafia, not them.";
			}
			else {
				%miniGame = %obj.client.miniGame;

				for (%i = 0; %i < %miniGame.numMembers; %i++) {
					%client = %miniGame.member[%i];

					if (!%client.isMafia && %client != %obj.client) {
						%choices = %choices @ (%choices $= "" ? "" : "\t") @ %client.getSMMName();
					}
				}

				if (%choices !$= "") {
					%choice = getField(%choices, getRandom(0, getFieldCount(%choices) - 1));
					%obj.papersNote[%obj.currTool] = %choice SPC "is innocent!";
				}
				else {
					%obj.papersNote[%obj.currTool] = "Nothing to see here..";
				}
			}
		}

		if (%obj.papersNote[%obj.currTool] !$= "") {
			%obj.client.centerPrint("\c6" @ %obj.papersNote[%obj.currTool]);
		}
	}
}

function PapersImage::onUnMount(%this, %obj, %slot) {
	if (isObject(%obj.client)) {
		commandToClient(%obj.client, 'ClearCenterPrint');
	}
}

function PapersItem::onAdd(%this, %obj) {
	Parent::onAdd(%this, %obj);

	%obj.papersNote = $PapersNote;
	$PapersNote = "";
}

function PapersItem::onPickup(%this, %obj, %player) {
	%note = %obj.papersNote;

	if (%note $= "") {
		return Parent::onPickup(%this, %obj, %player);
	}

	%slots = %player.getDataBlock().maxTools;

	for (%i = 0; %i < %slots; %i++) {
		if (%player.tool[%i] == %this) {
			%ignore[%i] = true;
		}
	}

	%parent = Parent::onPickup(%this, %obj, %player);

	if (%parent) {
		for (%i = 0; %i < %slots; %i++) {
			if (!%ignore[%i] && %player.tool[%i] == %this) {
				%player.papersNote[%i] = %note;
				break;
			}
		}
	}

	return %parent;
}