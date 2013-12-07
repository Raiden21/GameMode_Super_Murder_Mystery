function GameConnection::startSMMSpectating(%this) {
	%miniGame = %this.miniGame;

	for (%i = 0; %i < %miniGame.numMembers; %i++) {
		%player = %miniGame.member[%i].player;

		if (isObject(%player) && %player.getState() !$= "Dead") {
			%target = %player;
			break;
		}
	}

	if (!isObject(%target)) {
		for (%i = 0; %i < %miniGame.numMembers; %i++) {
			%corpse = %miniGame.member[%i].corpse;

			if (isObject(%corpse)) {
				%target = %corpse;
				break;
			}
		}
	}

	if (isObject(%target)) {
		%this.camera.setMode("Corpse", %target);
		%this.setControlObject(%this.camera);
	}
}

function GameConnection::generateSMMAppearance(%this) {
	%this.smmHasAppearance = true;
	%this.smmDisguiseTarget = "";

	%this.smmGender = getRandom() < 0.4 ? 1 : 0;

	%this.smmSkinColor = smm_generateSkinColor();
	%this.smmHasGloves = getRandom() < (%this.smmGender ? 0.75 : 0.25);

	if (%this.smmHasGloves) {
		%this.smmGloveColor = vectorScale(getRandom() SPC getRandom() SPC getRandom(), 0.1) SPC 1;
	}

	%this.smmDecal = %this.smmGender ? "AAA-None" : smm_generateDecalName();
	%this.smmFace = smm_generateFaceName(%this.smmGender);

	%this.smmHat = getRandom() <= 0.175 ? 4 : 0;
	%this.smmPack = getRandom() < 0.175 ? 4 : 0;

	if (%this.smmHat != 0) {
		%this.smmHatColor = smm_generateGenericColor();
	}

	if (%this.smmPack != 0) {
		%this.smmPackColor = smm_generateGenericColor();
	}

	//%this.smmSuitColor = smm_generateSuitColor();
	%this.smmSuitColor = smm_generateGenericColor();
	%this.smmPantsColor = smm_generatePantsColor();

	if (getRandom() <= 0.45) {
		%r = max(getWord(%this.smmSuitColor, 0) - 0.05 - getRandom() * 0.1, 0);
		%g = max(getWord(%this.smmSuitColor, 1) - 0.05 - getRandom() * 0.1, 0);
		%b = max(getWord(%this.smmSuitColor, 2) - 0.05 - getRandom() * 0.1, 0);

		%this.smmSleeveColor = %r SPC %g SPC %b SPC 1;
	}
	else {
		%this.smmSleeveColor = %this.smmSuitColor;
	}

	%this.smmHookLeft = getRandom() < 0.1;
	%this.smmHookRight = getRandom() < (0.1 - %this.smmHookLeft * 0.25);

	%this.smmPegLeft = getRandom() < (0.1 - %this.smmHookLeft * 0.25 - %this.smmHookRight * 0.25);
	%this.smmPegRight = getRandom() < (0.1 - %this.smmHookLeft * 0.25 - %this.smmHookRight * 0.25 - %this.smmPegRight * 0.25);
}

function GameConnection::getSMMChatChannel(%this) {
	if (!isObject(%this.miniGame.smmCore)) {
		return 2;
	}

	if (isObject(%this.player) && %this.player.getState() !$= "Dead") {
		return 0;
	}

	if (%this.corpse.isWrapped && %this.corpse.revive && %this.corpse.isUnconscious) {
		return 3;
	}

	if (%this.corpse.isUnconscious) {
		return 1;
	}

	return 2;
}

function Player::handleSMMSpawn(%this) {
	%this.fs_velocityCheckTick();

	if (isObject(%this.client)) {
		%this.client.generateSMMAppearance();
		%this.client.giveSMMName();

		%this.client.applyBodyParts();
		%this.client.applyBodyColors();
	}

	%this.setShapeNameDistance(0);

	%this.schedule(0, handleSMMSpawnPhase2);
	%this.schedule(0, prepareSMMAwayKiller);
}

function Player::handleSMMSpawnPhase2(%this) {
	if (isObject(%this.client)) {
		if (%this.client.isMafia) {
			%message = "<color:FF6666>MAFIA";
		}
		else {
			%message = "<color:66FF66>INNOCENT";
		}

		%message = %message @ "\n<color:AAAAAA>You are <color:FFFF66>" @ %this.client.getSMMName() @ "<color:AAAAAA>.";
		%this.client.centerPrint("<font:impact:64><shadow:4:4><shadowcolor:666666>" @ %message, 10);

		%slots = %this.getDataBlock().maxTools;
		commandToClient(%this.client, 'PlayGui_CreateToolHud', %slots);

		for (%i = 0; %i < %slots; %i++) {
			commandToClient(%this.client, 'MsgItemPickup', "", %i, 0);
		}
	}

	%tools[0] = "AutomaticPistolItem RevolverItem CarbineSMGItem";
	%tools_maf[0] = "StealthPistolItem";
	%tools[1] = "MeleeCaneItem MeleeKnifeItem MeleeUmbrellaItem MeleeWrenchItem MeleePanItem taserItem";
	%tools[2] = "GoldenGunItem PapersItem MedicineItem PaintItem SuitcaseKeyItem LighterItem bodybagItem AmmoSupplyItem RecoveryDeviceItem flashlightItem";
	%tools_maf[2] = "DisguiseItem CloakItem falseCorpseItem timeBombItem";

	for (%slot = 0; %slot < 3; %slot++) {
		%choice = %tools[%slot];

		if (%this.client.isMafia) {
			if (getRandom(0, getWordCount(%tools[%slot])) > getWordCount(%tools[%slot]) - 2 ) {
				if(%slot != 1) {
					%choice = %tools_maf[%slot];
				}
			}
		}

		%tool = getWord(%choice, getRandom(0, getWordCount(%choice) - 1));

		if (isObject(%tool)) {
			%tool = %tool.getID();
			%this.tool[%slot] = %tool;

			if (isObject(%this.client)) {
				messageClient(%this.client, 'MsgItemPickup', "", %slot, %tool, true);
			}

			if (isFunction(%tool.getName(), "onSMMGiven")) {
				%tool.onSMMGiven(%this, %slot);
			}
		}
	}
}

function Player::prepareSMMAwayKiller(%this) {
	if (vectorLen(%this.getVelocity()) > 0) {
		%this.schedule(0, prepareSMMAwayKiller);
	}
	else {
		%this.schedule(59500, killSMMIfSameTransform, %this.getTransform());
	}
}

function Player::killSMMIfSameTransform(%this, %transform) {
	if (%transform $= %this.getTransform()) {
		%this.hasBeenUnconscious = true;
		%this.kill();
	}
	else {
		%this.schedule(120000, killSMMIfSameTransform, %this.getTransform());
	}
}

function AIPlayer::reviveCorpseTimer(%this, %seconds, %scheduled) {
	cancel(%this.reviveCorpseTimer);

	if (!isObject(%this.originalClient) || !%this.isUnconscious) {
		%this.revive = "";
		return;
	}

	if (!%scheduled) {
		if (%this.usedRecoveryDevice) {
			%seconds /= 5;
		}
		else {
			%slots = %this.getDataBlock().maxTools;

			for (%i = 0; %i < %slots; %i++) {
				if (%this.tool[%i] == nameToID("RecoveryDeviceItem")) {
					%this.tool[%i] = "";
					%this.usedRecoveryDevice = true;

					%seconds /= 5;
					break;
				}
			}
		}
	}

	%seconds = mFloor(%seconds);
	%carried = !%this.getObjectMount().isCorpseVehicle;

	if (%seconds <= 0) {
		if (%this.isWrapped) {
			%seconds = 0;
			%this.revive = true;
			%this.originalClient.centerPrint("\c6You have been revived, however, your speech is muffled and you can't move!", 1.5);
		}
		else
		if (%carried) {
			%seconds = 0;
		}
		else {
			%this.reviveCorpse();
			return;
		}
	}
	else {
		if (%this.revive) {
			%this.revive = false;
		}
	}

	if (!%carried && !%this.isWrapped || %seconds > 0) {
		%this.originalClient.centerPrint("\c6You will automatically be revived in \c3" @ %seconds SPC "seconds\c6.", 1.5);
	}
	else {
		if (!%this.isWrapped) {
			%this.originalClient.centerPrint("\c6You will automatically be revived when you are dropped.", 1.5);
		}
	}

	%this.reviveCorpseTimer = %this.schedule(1000, reviveCorpseTimer, %seconds--, true);
}

function AIPlayer::reviveCorpse(%this) {
	if (!%this.isUnconscious || !isObject(%this.originalClient)) {
		return false;
	}

	%client = %this.originalClient;

	if (!%this.getObjectMount().isCorpseVehicle) {
		return;
	}

	cancel(%this.reviveCorpseTimer);
	clearCenterPrint(%client);
	%this.revive = "";

	%this.corpseVehicle.beingPickedUp = false;
	%this.unmount();

	%client.createPlayer(%this.getTransform());

	%client.applyBodyParts();
	%client.applyBodyColors();

	%client.player.setShapeNameDistance(0);
	%client.player.fs_velocityCheckTick();

	%client.player.hasBeenUnconscious = true;
	%client.setControlObject(%client.player);

	%slots = %this.getDataBlock();

	for (%i = 0; %i < %slots; %i++) {
		%client.player.tool[%i] = %this.tool[%i];
		%client.player.toolMag[%i] = %this.toolMag[%i];

		%client.player.suitcaseTool[%i] = %this.suitcaseTool[%i];
		%client.player.papersNote[%i] = %this.papersNote[%i];

		messageClient(%client, 'MsgItemPickup', "", %i, %client.player.tool[%i], true);

		if (%this.tool[%i].ammoType !$= "") {
			%client.player.toolAmmo[%this.tool[%i].ammoType] = %this.toolAmmo[%this.tool[%i].ammoType];
		}
	}

	%this.schedule(0, delete);
}

function WheeledVehicle::monitorCorpseVelocity(%this, %lastSpeed) {
	cancel(%this.monitorCorpseVelocity);
	%speed = vectorLen(%this.getVelocity());

	if (isObject(%this.getMountedObject(0)) && %lastSpeed >= 4 && %lastSpeed - %speed >= 1.5) {
		%pos = %this.getWorldBoxCenter();
		%this.getMountedObject(0).doDripBlood(true);

		if (%lastSpeed >= 12) {
			serverPlay3D(CorpseImpactHardSound @ getRandom(1, 3), %pos);
			%damage = %lastSpeed * 4;
			%this.getMountedObject(0).damage(%this.getMountedObject(0), %pos, %damage);
		}
		else {
			serverPlay3D(CorpseImpactSoftSound @ getRandom(1, 3), %pos);
		}
	}

	%this.monitorCorpseVelocity = %this.schedule(50, "monitorCorpseVelocity", %speed);
}

function smm_generateGenericColor() {
	%color0 = "0.9 0 0";
	%color1 = "0.9 0 0";
	%color2 = "0.74902 0.180392 0.482353";
	%color3 = "0.388235 0 0.117647";
	%color4 = "0.133333 0.270588 0.270588";
	%color5 = "0 0.141176 0.333333";
	%color6 = "0.105882 0.458824 0.768627";
	%color7 = "1 1 1";
	%color8 = "0.0784314 0.0784314 0.0784314";
	%color9 = "0.92549 0.513726 0.678431";
	%color10 = "0 0.5 0.25";
	%color11 = "0.784314 0.921569 0.490196";
	%color12 = "0.541176 0.698039 0.552941";
	%color13 = "0.560784 0.929412 0.960784";
	%color14 = "0.698039 0.662745 0.905882";
	%color15 = "0.878431 0.560784 0.956863";
	%color16 = "0.667 0 0";
	%color17 = "1 0.5 0";
	%color18 = "0.99 0.96 0";
	%color19 = "0.2 0 0.8";
	%color20 = "0 0.471 0.196";
	%color21 = "0 0.2 0.64";
	%color22 = "0.596078 0.160784 0.392157";
	%color23 = "0.55 0.7 1";
	%color24 = "0.85 0.85 0.85";
	%color25 = "0.1 0.1 0.1";
	%color26 = "0.9 0.9 0.9";
	%color27 = "0.75 0.75 0.75";
	%color28 = "0.5 0.5 0.5";
	%color29 = "0.2 0.2 0.2";
	%color30 = "0.901961 0.341176 0.0784314";

	return %color[getRandom(0, 30)] SPC 1;
}

function smm_generateSkinColor() {
	%index = getRandom(0, 3);

	%color0 = "0.956863 0.878431 0.784314";
	%color1 = "1 0.878431 0.611765";
	%color2 = "1 0.603922 0.423529";
	%color3 = "0.392157 0.196078 0";

	%r = max(min(getWord(%color[%index], 0) + 0.05 - getRandom() * 0.1, 1), 0);
	%g = max(min(getWord(%color[%index], 1) + 0.05 - getRandom() * 0.1, 1), 0);
	%b = max(min(getWord(%color[%index], 2) + 0.05 - getRandom() * 0.1, 1), 0);

	return %r SPC %g SPC %b SPC 1;
}

function smm_generateSuitColor() {
	%color0 = "0.75 0.75 0.75";
	%color1 = "0.2 0.2 0.2";
	%color2 = "0.388 0 0.117";
	%color3 = "0.133 0.27 0.27";
	%color4 = "0 0.141 0.333";
	%color5 = "0.078 0.078 0.078";

	return %color[getRandom(0, 5)] SPC 1;
}

function smm_generatePantsColor() {
	return smm_generateSuitColor();
}

function smm_generateFaceName(%gender) {
	if (%gender) {
		%face0 = "smiley";
		%face1 = "smileyFemale1";

		%faceMax = 1;
	}
	else {
		%face0 = "smiley";
		%face1 = "Jamie";
		%face2 = "Male07Smiley";
		%face3 = "BrownSmiley";
		%face4 = "smileyOld";
		%face5 = "smileyEvil2";
		%face6 = "smileyEvil1";
		%face7 = "smileyCreepy";

		%faceMax = 7;
	}

	return %face[getRandom(0, %faceMax)];
}

function smm_generateDecalName() {
	%decal0 = "Mod-Suit";
	%decal1 = "Mod-Pilot";

	return %decal[getRandom(0, 1)];
}

function median(%a, %b, %c) {
	if ((%a >= %b && %a <= %b) || (%a <= %b && %a >= %b)) return %a;
	if ((%b >= %a && %b <= %c) || (%b <= %a && %b >= %c)) return %b;
	if ((%c >= %a && %c <= %b) || (%c <= %a && %c >= %b)) return %c;
}

function min(%a, %b) {
	return %a < %b ? %a : %b;
}

function max(%a, %b) {
	return %a > %b ? %a : %b;
}

function naturalGrammarList(%list) {
	%fields = getFieldCount(%list);

	if (%fields < 2) {
		return %list;
	}

	for (%i = 0; %i < %fields - 1; %i++) {
		%partial = %partial @ (%i ? ", " : "") @ getField(%list, %i);
	}

	return %partial SPC "and" SPC getField(%list, %fields - 1);
}

function vectorSpread(%vector, %spread) {
	%x = (getRandom() - 0.5) * 10 * 3.1415926 * %spread;
	%y = (getRandom() - 0.5) * 10 * 3.1415926 * %spread;
	%z = (getRandom() - 0.5) * 10 * 3.1415926 * %spread;

	%mat = matrixCreateFromEuler(%x SPC %y SPC %z);
	return vectorNormalize(matrixMulVector(%mat, %vector));
}

function blendRGBA(%bg, %fg) {
	%ba = getWord(%bg, 3);
	%fa = getWord(%fg, 3);

	%a = 1 - (1 - %fa) * (1 - %ba);
	%r = getWord(%fg, 0) * %fa / %a + getWord(%bg, 0) * %ba * (1 - %fa) / %a;
	%g = getWord(%fg, 1) * %fa / %a + getWord(%bg, 1) * %ba * (1 - %fa) / %a;
	%b = getWord(%fg, 2) * %fa / %a + getWord(%bg, 0) * %ba * (1 - %fa) / %a;

	return %r SPC %g SPC %b SPC %a;
}

function emitSMMMessage(%message, %miniGame, %pos, %range) {
	initContainerRadiusSearch(%pos, %range, $TypeMasks::PlayerObjectType);

	while (isObject(%obj = containerSearchNext())) {
		if (!%obj.isCorpse && isObject(%obj.client) && %obj.client.miniGame == %miniGame) {
			messageClient(%obj.client, '', %message);
			%seen[%obj.client] = true;
		}
	}

	%count = ClientGroup.getCount();

	for (%i = 0; %i < %count; %i++) {
		%client = ClientGroup.getObject(%i);

		if (%seen[%client]) {
			continue;
		}

		if (%client.miniGame != %miniGame || %client.getSMMChatChannel() != 0) {
			messageClient(%client, '', %message);
		}
	}
}

function serverCmdWhoIs(%client, %a, %b) {
	if (!%client.isAdmin && !%client.isSuperAdmin) {
		return;
	}

	%name = trim(%a SPC %b);
	%count = ClientGroup.getCount();
	if(%name $= "" && isObject(%client.player)) {
		%point = %client.player.getEyePoint();
		%vector = %client.player.getEyeVector();

		%ray = containerRayCast(%point,
			vectorAdd(%point, vectorScale(%vector, 7)),
			$TypeMasks::PlayerObjectType,
			%client.player
		);

		if(isObject(%col = getWord(%ray, 0))) {
			if (%col.isCorpse) {
				messageClient(%client, '', "\c6The corpse belongs to \c3" @ %col.sourceClient.getPlayerName() SPC "(" @ %col.sourceClient.getSMMName() @ ")");
			}
			else {
				messageClient(%client, '', "\c3" @ %col.client.getSMMName() SPC "\c6is \c3" @ %col.client.getPlayerName());
			}
		}
		return;
	}

	for (%i = 0; %i < %count; %i++) {
		%current = ClientGroup.getObject(%i);

		if (striPos(%current.getSMMName(), %name) != -1) {
			messageClient(%client, '', "\c3" @ %current.getSMMName() SPC "\c6is \c3" @ %current.getPlayerName());
		}
		else if (isObject(findClientByName(%name))) {
			messageClient(%client, '', "\c3" @ %current.getPlayerName() SPC "\c6is \c3" @ %current.getSMMName());
		}
	}
}

function serverCmdResetSMM(%client) {
	if (%client.isSuperAdmin) {
		if (isObject($DefaultMiniGame.smmCore)) {
			$DefaultMiniGame.smmCore.end("The round has been forcibly ended by a Super Admin.");
		}
	}
}

function Player::unZipSchedule(%this, %col, %times) {
	cancel(%this.unZipSchedule);
	if (!isObject(%col.originalClient) && !%col.isFalseCorpse) {
		return;
	}
	if (!isObject(%col) || !%col.isWrapped || vectorLen(%this.getVelocity()) > 0 || vectorLen(%col.getVelocity()) > 0) {
		%this.client.centerPrint("\c6Action aborted.", 2);
		return;
	}
	if (%times <= 0) {
		%this.client.centerPrint("\c6You have unzipped the bodybag.", 2);
		%this.playThread(2, "activate2");
		%col.unMountImage(2);
		%col.isWrapped = false;
		%player = %col.originalClient.player;
		%col.originalClient.player = %col;
		%col.originalClient.applyBodyParts();
		%col.originalClient.applyBodyColors();
		%col.originalClient.player = %player;
		serverPlay3d(bodybagUnzipSound, %col.getHackPosition());
		return;
	}
	%this.client.centerPrint("\c6Unzipping...", 1);
	%this.schedule(500, unZipSchedule, %col, %times - 1);
}

function muffleSpeech(%text) {
	return %text;
}