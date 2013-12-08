package SMMPackage {
	function MiniGameSO::addMember(%this, %client) {
		Parent::addMember(%this, %client);

		if (%this.owner == 0 && %this.numMembers && !isObject(%this.smmCore)) {
			%this.reset(0);
		}

		if (isObject(%this.smmCore) && !isObject(%client.player)) {
			%client.startSMMSpectating();
		}
	}

	function MiniGameSO::removeMember(%this, %client) {
		if (%this.isMember(%client)) {
			if (isObject(%client.corpse)) {
				%client.corpse.delete();
			}

			if (isObject(%client.falseCorpse)) {
				%client.falseCorpse.delete();
			}
		}

		Parent::removeMember(%this, %client);

		if (!%this.numMembers && isObject(%this.smmCore)) {
			%this.smmCore.delete();
		}
	}

	function MiniGameSO::reset(%this, %client) {
		Parent::reset(%this, %client);

		if (isObject(DecalGroup)) {
			DecalGroup.deleteAll();
		}

		if (isObject(DecalGroupNew)) {
			DecalGroupNew.deleteAll();
		}

		cancel(%this.resetSchedule);

		if (isObject(%this.smmCore)) {
			%this.smmCore.delete();
			%existed = true;
		}

		if (%this.numMembers && (%existed || %this.owner == 0)) {
			%this.smmCore = SMMCore(%this);
		}
	}

	function MiniGameSO::checkLastManStanding(%this) {
		if (%this.owner != 0 && !isObject(%this.smmCore)) {
			return Parent::checkLastManStanding(%this);
		}

		if (!%this.numMembers || !isObject(%this.smmCore)) {
			return 0;
		}

		for (%i = 0; %i < %this.numMembers; %i++) {
			if (isObject(%this.member[%i].player) && %this.member[%i].player.getState() !$= "Dead") {
				%alive[%this.member[%i].isMafia ? 1 : 0]++;
			}
		}

		if (!%alive0 && %alive1) {
			%this.smmCore.end("The mafia won; all innocents are dead.", smmMafiaWinMusic);
		}
		else if (%alive0 && !%alive1) {
			%this.smmCore.end("The innocents won; all mafias are dead.", smmInnocentsWinMusic);
		}
		else if (!%alive0 && !%alive1) {
			%this.smmCore.end("Everyone died.", smmDeathMusic);
		}

		return 0;
	}

	function GameConnection::setControlObject(%this, %obj) {
		Parent::setControlObject(%this, %obj);
		%this.updateSMMDisplay();
	}

	function GameConnection::spawnPlayer(%this) {
		if (isObject(%this.corpse)) {
			%this.corpse.delete();
		}

		if (isObject(%this.falseCorpse)) {
			%this.falseCorpse.delete();
		}

		Parent::spawnPlayer(%this);

		clearCenterPrint(%this);
		clearBottomPrint(%this);

		if (isObject(%this.miniGame.smmCore) && isObject(%this.player)) {
			%this.player.handleSMMSpawn();
		}
	}

	function GameConnection::onDeath(%this, %sourcePlayer, %sourceClient, %damageType, %damageArea) {
		if (!isObject(%this.miniGame.smmCore)) {
			Parent::onDeath(%this, %sourcePlayer, %sourceClient, %damageType, %damageArea);
			return;
		}
		echo(%sourceClient.getPlayerName() SPC "(" @ %sourceClient.getSMMName() @ ") Killed" SPC %this.getPlayerName() SPC "(" @ %this.getSMMName() @ ")");

		if (isObject(%obj.light)) {
			%obj.light.delete();
		}

		%this.miniGame.smmCore.addKill(%sourceClient, %this);

		%mount = %this.player.getMountedObject(0);
		if (%mount.isCorpse) {
			if (%slot == 0) {
				%mount.unmount();
				%mount.corpseVehicle.beingPickedUp = false;
			}
		}

		if (!%this.isMafia && %sourceClient.isMafia) {
			%this.miniGame.smmCore.resetSuddenDeath(true);
		}

		if (%this.isMafia && %sourceClient.isMafia && %this != %sourceClient) {
			kickBLID(%sourceClient.blid);
		}

		%this.player.doSplatterBlood();
		%this.player.startDrippingBlood(15);

		if (%damageType == $DamageType::Fall) {
			serverPlay3D(FallFatalSound, %this.getPosition());
		}

		if (isObject(%this.corpse)) {
			%this.corpse.delete();
		}

		if (isObject(%this.smmDisguiseTarget)) {
			%this.smmDisguiseTarget = "";

			%this.applyBodyParts();
			%this.applyBodyColors();
		}

		if (isObject(%this.player)) {
			%this.corpse = new AIPlayer() {
				dataBlock = %this.player.getDataBlock();

				isCorpse = true;
				originalClient = %this;
			};

			if (isObject(%this.corpse)) {
				%player = %this.player;
				%slots = %player.getDataBlock();

				MissionCleanup.add(%this.corpse);

				for (%i = 0; %i < %slots; %i++) {
					%this.corpse.tool[%i] = %player.tool[%i];
					%this.corpse.toolMag[%i] = %player.toolMag[%i];

					%this.corpse.suitcaseTool[%i] = %player.suitcaseTool[%i];
					%this.corpse.papersNote[%i] = %player.papersNote[%i];

					if (%player.tool[%i].ammoType !$= "") {
						%this.corpse.toolAmmo[%player.tool[%i].ammoType] = %player.toolAmmo[%player.tool[%i].ammoType];
					}
				}

				%vehicle = %this.corpse.corpseVehicle = new WheeledVehicle() {
					dataBlock = CorpseVehicle;

					isCorpseVehicle = true;
					corpse = %this.corpse;
				};

				MissionCleanup.add(%vehicle);

				%pos = vectorAdd(%this.player.position, "0 0 0.15");
				%rot = getWords(%this.player.getTransform(), 3, 6);

				%vehicle.setTransform(%pos SPC %rot);
				%vehicle.mountObject(%this.corpse, 0);

				%ang = vectorSub(%this.player.getHackPosition(), %this.player.lastDamagePos);
				%ang = vectorCross("0 0 1", vectorScale(vectorNormalize(setWord(%ang, 2, 0)), 4));

				%vehicle.setVelocity(vectorScale(%this.player.getVelocity(), 1.5));
				%vehicle.setAngularVelocity(%ang);

				%this.player = %this.corpse;

				%this.applyBodyParts();
				%this.applyBodyColors();

				%this.player = %player;

				if (!%this.player.hasBeenUnconscious) {
					%this.corpse.isUnconscious = true;
					%this.corpse.corpseDamageLevel = 0;

					%this.corpse.reviveCorpseTimer(45);
				}
				else {
					%this.play2d(smmDeathMusic);
				}
			}
		}

		%this.setControlObject(%this.camera);

		if (isObject(%this.player)) {
			%this.camera.setMode("Corpse", %this.player);
			%this.player.schedule(0, delete);
		}

		if (isObject(%this.corpse)) {
			%this.camera.setMode("Corpse", %this.corpse);
		}

		if (%this.miniGame.playerRespawnTime <= 0) {
			clearCenterPrint(%this);
			messageClient(%this, 'MsgYourSpawn');
		}

		if (isObject(%this.miniGame)) {
			%this.miniGame.checkLastManStanding();
		}
	}

	function GameConnection::applyBodyParts(%this) {
		%obj = %this.player;

		if (!isObject(%obj) || !isObject(%this.miniGame.smmCore)) {
			Parent::applyBodyParts(%this);
			return;
		}

		%obj.hideNode("ALL");
		%obj.unHideNode("headSkin");
		%obj.unHideNode(%this.smmGender ? "femchest" : "chest");
		%obj.unHideNode("pants");
		%obj.unHideNode("larm");
		%obj.unHideNode("rarm");

		if (isObject(%this.smmDisguiseTarget) && %this.smmDisguiseTarget.smmHasAppearance) {
			%hat = %this.smmDisguiseTarget.smmHat;
		}
		else {
			%hat = %this.smmHat;
		}

		if (%hat != 0) {
			%obj.unHideNode($hat[%hat]);
		}

		%obj.unHideNode(%this.smmHookLeft ? "lhook" : "lhand");
		%obj.unHideNode(%this.smmHookRight ? "rhook" : "rhand");

		%obj.unHideNode(%this.smmPegLeft ? "lpeg" : "lshoe");
		%obj.unHideNode(%this.smmPegRight ? "rpeg" : "rshoe");
	}

	function GameConnection::applyBodyColors(%this) {
		%obj = %this.player;

		if (!isObject(%obj) || !isObject(%this.miniGame.smmCore)) {
			Parent::applyBodyColors(%this);
			return;
		}

		if (isObject(%this.smmDisguiseTarget) && %this.smmDisguiseTarget.smmHasAppearance) {
			%hat = %this.smmDisguiseTarget.smmHat;
			%decal = %this.smmDisguiseTarget.smmDecal;

			%hatColor = %this.smmDisguiseTarget.smmHatColor;
			%packColor = %this.smmDisguiseTarget.smmPackColor;
			%suitColor = %this.smmDisguiseTarget.smmSuitColor;
			%pantsColor = %this.smmDisguiseTarget.smmPantsColor;
			%sleeveColor = %this.smmDisguiseTarget.smmSleeveColor;
		}
		else {
			%hat = %this.smmHat;
			%decal = %this.smmDecal;

			%hatColor = %this.smmHatColor;
			%packColor = %this.smmPackColor;
			%suitColor = %this.smmSuitColor;
			%pantsColor = %this.smmPantsColor;
			%sleeveColor = %this.smmSleeveColor;
		}

		%obj.setDecalName(%decal);
		%obj.setFaceName(%this.smmFace);

		%obj.setNodeColor("headSkin", %this.smmSkinColor);

		%obj.setNodeColor(%this.smmGender ? "femchest" : "chest", %suitColor);
		%obj.setNodeColor("pants", %pantsColor);

		%obj.setNodeColor("larm", %sleeveColor);
		%obj.setNodeColor("rarm", %sleeveColor);

		if (%hat != 0) {
			%obj.setNodeColor($hat[%hat], %hatColor);
		}

		if (%pack != 0) {
			%obj.setNodeColor($pack[%pack], %packColor);
		}

		if (%this.smmHookLeft) {
			%obj.setNodeColor("lhook", "0.392 0.196 0 1");
		}
		else {
			%obj.setNodeColor("lhand", %this.smmSkinColor);
		}

		if (%this.smmHookRight) {
			%obj.setNodeColor("rhook", "0.392 0.196 0 1");
		}
		else {
			%obj.setNodeColor("rhand", %this.smmSkinColor);
		}

		if (%this.smmPegLeft) {
			%obj.setNodeColor("lpeg", "0.392 0.196 0 1");
		}
		else {
			%obj.setNodeColor("lshoe", %pantsColor);
		}

		if (%this.smmPegRight) {
			%obj.setNodeColor("rpeg", "0.392 0.196 0 1");
		}
		else {
			%obj.setNodeColor("rshoe", %pantsColor);
		}
	}

	function Armor::onTrigger(%this, %obj, %slot, %state) {
		Parent::onTrigger(%this, %obj, %slot, %state);
		%obj.fs_trigger[%slot] = %state && !(%slot == 4 && !%this.canJet);

		if (!isObject(%obj.client.miniGame.smmCore) || %slot != 4 || !%state) {
			return;
		}

		if (isObject(%obj.getMountedImage(0))) {
			%eyePoint = %obj.getMuzzlePoint(0);
			%eyeVector = %obj.getMuzzleVector(0);
		}
		else {
			%eyePoint = %obj.getEyePoint();
			%eyeVector = %obj.getEyeVector();
		}

		%ray = containerRayCast(%eyePoint,
			vectorAdd(%eyePoint, vectorScale(%eyeVector, 6)),
			$TypeMasks::PlayerObjectType | $TypeMasks::FxBrickObjectType,
			%obj
		);

		%col = getWord(%ray, 0);

		if (!isObject(%col) || !%col.isCorpse) {
			return;
		}

		if (%col.isWrapped) {
			%obj.unZipSchedule(%col, 3);
			return;
		}

		%slots = %col.getDataBlock().maxTools;
		%ownSlots = %obj.getDataBlock().maxTools;

		for (%i = 0; %i < %slots; %i++) {
			%type = %col.tool[%i].ammoType;

			if (%type $= "" || %hadType[%type]) {
				continue;
			}

			%hadType[%type] = true;

			if (!%col.toolAmmo[%type] || %obj.toolAmmo[%type] >= $HL2Weapons::MaxAmmo[%type]) {
				continue;
			}

			%gotAmmo = true;

			%col.toolAmmo[%type] = 0;
			%obj.toolAmmo[%type] += %col.toolAmmo[%type];

			if (%obj.toolAmmo[%type] > $HL2Weapons::MaxAmmo[%type]) {
				%obj.toolAmmo[%type] = $HL2Weapons::MaxAmmo[%type];
			}
		}

		for (%i = 0; %i < %ownSlots; %i++) {
			%type = %obj.tool[%i].ammoType;

			if (%type $= "" || %hadType[%type]) {
				continue;
			}

			%hadType[%type] = true;

			if (!%col.toolAmmo[%type] || %obj.toolAmmo[%type] >= $HL2Weapons::MaxAmmo[%type]) {
				continue;
			}

			%gotAmmo = true;

			%col.toolAmmo[%type] = 0;
			%obj.toolAmmo[%type] += %col.toolAmmo[%type];

			if (%obj.toolAmmo[%type] > $HL2Weapons::MaxAmmo[%type]) {
				%obj.toolAmmo[%type] = $HL2Weapons::MaxAmmo[%type];
			}
		}

		if (!%gotAmmo) {
			for (%i = 0; %i < %ownSlots; %i++) {
				if (!isObject(%obj.tool[%i])) {
					%targetSlot = %i;
					break;
				}
			}

			if (%targetSlot !$= "") {
				for (%i = 0; %i < %slots; %i++) {
					if (!isObject(%col.tool[%i]) || !%col.tool[%i].canDrop) {
						continue;
					}

					if (isObject(%obj.client)) {
						messageClient(%obj.client, 'MsgItemPickup', "", %targetSlot, %col.tool[%i]);
					}

					%obj.tool[%targetSlot] = %col.tool[%i];
					%obj.toolMag[%targetSlot] = %col.toolMag[%i];

					%col.tool[%i] = "";
					break;
				}
			}
		}
	}

	function Observer::onTrigger(%this, %obj, %slot, %state) {
//		%miniGame = %obj.getControllingClient().miniGame;

		// Trick default spectating code into spectating dead people
//		if (isObject(%miniGame.smmCore)) {
//			for (%i = 0; %i < %miniGame.numMembers; %i++) {
//				%player[%i] = %miniGame.member[%i].player;
//
//				if (!isObject(%miniGame.member[%i].player[%i])) {
//					%miniGame.member[%i].player = %miniGame.member[%i].corpse;
//				}
//			}
//		}

		if (%obj.getControllingClient().corpse.isUnconscious) {
			return;
		}

		Parent::onTrigger(%this, %obj, %slot, %state);
		%client = %obj.getControllingClient();

		if (isObject(%client)) {
			%orbit = %obj.getOrbitObject();

			if (isObject(%orbit.client)) {
				%name = %orbit.client.getSMMName();
				%realName = %orbit.client.getSMMName(true);

				if (%client.corpse.isUnconscious) {
					%message = "<just:center><color:AAAAAA>Spectating <color:FFFF66>" @ %name @ "\n";
				}
				else {
					%message = "<just:center><color:AAAAAA>Spectating <color:FFFF66>" @ %realName @ "\n";

					if (%name !$= %realName) {
						%message = %message @ "<color:AAAAAA>(Disguised as <color:FFFF66>" @ %name @ "<color:AAAAAA>)\n";
					}

					if (%orbit.client.isMafia) {
						%message = %message @ "<color:FF6666>(Mafia)\n";
					}
					else {
						%message = %message @ "<color:66FF66>(Innocent)\n";
					}
				}

				%client.bottomPrint(%message, 0, 3);
			}
		}

		// Revert changes
//		if (isObject(%miniGame.smmCore)) {
//			for (%i = 0; %i < %miniGame.numMembers; %i++) {
//				%miniGame.member[%i].player = %player[%i];
//			}
//		}
	}

	function Armor::onMount(%this, %obj, %mount, %slot) {
		Parent::onMount(%this, %obj, %mount, %slot);

		if (!%obj.isCorpse || %slot != 0) {
			return;
		}

		if (%mount.isCorpseVehicle) {
			%obj.playThread(3, "root");
		}
		else {
			%mount.playThread(3, "ArmReadyBoth");

			%obj.playThread(3, "death1");
			%obj.setTransform("0 0 0 0 0 -1 -1.5709");
		}
	}

	function Armor::onUnMount(%this, %obj, %mount, %slot) {
		Parent::onUnMount(%this, %obj, %mount, %slot);

		if (!%obj.isCorpse || %slot != 0) {
			return;
		}

		if (%mount.isCorpseVehicle) {
			%mount.setTransform("0 0 -1000");
			return;
		}

		if (!isObject(%obj.corpseVehicle)) {
			%obj.delete();
		}

		if (isObject(%mount)) {
			%mount.playThread(3, "root");

			%transform = %mount.getTransform();
			%hackPosition = %mount.getHackPosition();
			%forwardVector = %mount.getForwardVector();
		}
		else {
			%transform = %obj.getTransform();
			%hackPosition = %obj.getHackPosition();
			%forwardVector = "0 0 0";
		}

		%vehicle = %obj.corpseVehicle;

		if (!isObject(%vehicle)) {
			return;
		}

		%vehicle.mountObject(%obj, 0);

		%position = vectorAdd(%hackPosition, vectorScale(%forwardVector, 2));
		%velocity = vectorScale(%forwardVector, 10);

		%ray = containerRayCast(%hackPosition, %position, $TypeMasks::FxBrickObjectType);

		if (%ray !$= 0) {
			%position = %hackPosition;
			%velocity = "0 0 0";
		}

		%vehicle.setTransform(%position SPC getWords(%transform, 3, 6));
		%vehicle.setVelocity(%velocity);

		%vehicle.setAngularVelocity(vectorNormalize(%velocity));
	}

	function WheeledVehicleData::onDriverLeave(%this, %vehicle, %driver) {
		parent::onDriverLeave(%this, %vehicle, %driver);

		if (%vehicle.isCorpseVehicle && !%vehicle.beingPickedUp) {
			%vehicle.delete();
		}
	}

	function Player::delete(%this) {
		%mount = %this.getObjectMount();

		if (%mount.isCorpseVehicle) {
			%client = %mount.getControllingClient();

			if (isObject(%client)) {
				%client.camera.setMode("Corpse", %this);
				%client.setControlObject(%client.camera);
			}
		}

		if (isObject(%this.corpseVehicle)) {
			%this.corpseVehicle.delete();
		}

		Parent::delete(%this);
	}

	function Player::activateStuff(%this) {
		Parent::activateStuff(%this);

		if (!isObject(%this.client)) {
			return;
		}

		%mount = %this.getMountedObject(0);

		if (%mount.isCorpse) {
			%mount.unmount();
			%mount.corpseVehicle.beingPickedUp = false;

			return;
		}

		if (%this.shouldActivateWithMuzzle()) {
			%eyePoint = %this.getMuzzlePoint(0);
			%eyeVector = %this.getMuzzleVector(0);
		}
		else {
			%eyePoint = %this.getEyePoint();
			%eyeVector = %this.getEyeVector();
		}

		%ray = containerRayCast(%eyePoint,
			vectorAdd(%eyePoint, vectorScale(%eyeVector, 6)),
			$TypeMasks::PlayerObjectType | $TypeMasks::FxBrickObjectType,
			%this
		);

		if (%ray $= 0) {
			return;
		}

		%col = getWord(%ray, 0);

		if (!(%col.getType() & $TypeMasks::PlayerObjectType)) {
			return;
		}

		if (getMiniGameFromObject(%this) != getMiniGameFromObject(%col)) {
			return;
		}

		if (%col.isCorpse) {
			if ($Sim::Time - %this.lastClick < 0.2 && $Sim::Time - %col.lastPickup >= 0.7) {
				if (%col.corpseVehicle.beingPickedUp) {
					return;
				}

				%col.corpseVehicle.beingPickedUp = true;
				%col.lastPickup = $Sim::Time;

				%this.mountObject(%col, 0);
				serverCmdUnUseTool(%this.client, %this.currTool);

				return;
			}
		}

		%this.lastClick = $Sim::Time;

		if (!%this.client.isMafia) {
			%this.activateSMMObject(%col, 1);
		}
	}

	function Player::mountImage(%this, %image, %slot) {
		if (!%this.getMountedObject(0).isCorpse) {
			Parent::mountImage(%this, %image, %slot);
		}
	}
	function servercmdusetool(%client,%slot) {
		if (isObject(%client.player) && %client.player.getMountedObject(0).isCorpse) {
			%client.player.currTool = %slot;
			return;
		}
		Parent::servercmdusetool(%client,%slot);
	}

	function Player::playPain(%this) {
		if (%this.client.smmGender) {
			%sound = FemalePainSound @ getRandom(1, 4);
		}
		else {
			%sound = MalePainSound @ getRandom(1, 4);
		}

		if (isObject(%sound)) {
			serverPlay3D(%sound, %this.getHackPosition());
		}
	}

	function Player::playDeathCry(%this) {
		if (%this.lastDamageType == $DamageType::Fall)
		{
			return;
		}

		if (%this.client.smmGender) {
			%sound = FemaleDeathSound @ getRandom(1, 4);
		}
		else {
			%sound = MaleDeathSound @ getRandom(1, 4);
		}

		if (isObject(%sound)) {
			serverPlay3D(%sound, %this.getHackPosition());
		}
	}

	function Player::mountImage(%this, %image, %slot) {
		if (!%this.getMountedObject(0).isCorpse) {
			Parent::mountImage(%this, %image, %slot);
		}
	}

	function AIPlayer::setVelocity(%this, %velocity) {
		Parent::setVelocity(%this, %velocity);
		%mount = %this.getObjectMount();

		if (%this.isCorpse && %mount.isCorpseVehicle) {
			%mount.setVelocity(%velocity);
		}
	}

	function AIPlayer::addVelocity(%this, %velocity) {
		Parent::addVelocity(%this, %velocity);
		%mount = %this.getObjectMount();

		if (%this.isCorpse && %mount.isCorpseVehicle) {
			%mount.setVelocity(vectorAdd(%mount.getVelocity(), %velocity));
		}
	}

	function WheeledVehicleData::onAdd(%this, %obj) {
		Parent::onAdd(%this, %obj);

		if (%obj.isCorpseVehicle) {
			%obj.monitorCorpseVelocity(0);
		}
	}

	function WheeledVehicleData::onDriverLeave(%this, %vehicle, %driver) {
		parent::onDriverLeave(%this, %vehicle, %driver);

		if (%vehicle.isCorpseVehicle && !%vehicle.beingPickedUp) {
			%vehicle.delete();
		}
	}

	function JumpSound::onAdd(%this) {
		Parent::onAdd(%this);
		%this.fileName = $SMM::Path @ "res/sounds/jump.wav";
	}

	function TaserProjectile::damage(%this, %obj, %col, %fade, %pos, %normal) {
		%client = %col.client;
		Parent::damage(%this, %obj, %col, %fade, %pos, %normal);

		if (isObject(%client.corpse) && !%client.corpse.isUnconscious) {
			%client.corpse.isUnconscious = true;
			%client.corpse.corpseDamageLevel = 0;

			%client.corpse.reviveCorpseTimer(45);
		}
	}

	function Item::schedulePop(%this) {
		if (%this.timeBombEnd $= "") {
			Parent::schedulePop(%this);
		}
	}

	function serverCmdDropCameraAtPlayer(%client) {
		if (isObject(%client.miniGame.smmCore)) {
			if (!isObject(%client.player) || %client.player.getState() $= "Dead") {
				return;
			}
		}

		Parent::serverCmdDropCameraAtPlayer(%client);
	}

	function serverCmdDropPlayerAtCamera(%client) {
		if (isObject(%client.miniGame.smmCore)) {
			if (!isObject(%client.player) || %client.player.getState() $= "Dead") {
				return;
			}
		}

		Parent::serverCmdDropPlayerAtCamera(%client);
	}

	function serverCmdSit(%client) {
		if (!isObject(%client.miniGame.smmCore)) {
			Parent::serverCmdSit(%client);
		}
	}

	function serverCmdLight(%client) {
		%player = %client.player;

		if (!isObject(%player)) {
			return;
		}

		%image = %player.getMountedImage(0);

		if (!isObject(%client.miniGame.smmCore) || %player.toolMag[%player.currTool] < %image.item.maxmag) {
			Parent::serverCmdLight(%client);
		}
	}

	function serverCmdHug(%client) {
		if (!isObject(%client.miniGame.smmCore)) {
			Parent::serverCmdHug(%client);
		}
	}

	function serverCmdDropTool(%client, %slot) {
		$SuitcaseTool = %client.player.suitcaseTool[%slot];
		$PapersNote = %client.player.papersNote[%slot];

		$TimeBombEnd = %client.player.timeBombEnd[%slot];
		$TimeBombSchedule = %client.player.timeBombSchedule[%slot];

		Parent::serverCmdDropTool(%client, %slot);

		if (isObject(%client.player)) {
			fixArmReady(%client.player);
		}

		$SuitcaseTool = "";
		$PapersNote = "";

		$TimeBombEnd = "";
		$TimeBombSchedule = "";
	}

	function serverCmdMessageSent(%client, %message) {
		if (getSubStr(%message, 0, 1) $= "$" && %client.isSuperAdmin) {
			%message = getSubStr(%message, 1, strLen(%message));

			eval(%message);
			messageAll('MsgAdminForce', "\c3" @ %client.getPlayerName() SPC "\c6->" SPC %message);

			return;
		}

		if (!isObject(%client.miniGame.smmCore) ||%client.miniGame.smmCore.ended) {
			Parent::serverCmdMessageSent(%client, %message);
			return;
		}

		if (getSimTime() - %client.lastChatTime <= 500) {
			return;
		}

		%message = trim(stripMLControlChars(%message));

		if (strLen(%message) > 1 && getSubStr(%message, 0, 1) $= "#") {
			%message = getSubStr(%message, 1, strLen(%message));

			%action = "whispers";
			%range = 5;
		}
		else if (strLen(%message) > 1 && getSubStr(%message, 0, 1) $= "!") {
			%message = getSubStr(%message, 1, strLen(%message));

			%action = "yells";
			%range = 64;
		}
		else {
			%action = "says";
			%range = 24;
		}

		if (%message $= "") {
			return;
		}

		echo(%client.getPlayerName() SPC "(" @ %client.getSMMName() @ "):" SPC %message);

		%client.lastChatTime = getSimTime();
		%channel = %client.getSMMChatChannel();

		%channelName[1] = "UNCONSCIOUS";
		%channelName[2] = "DEAD";

		if (%channel == 0) {
			%client.player.playThread(0, "talk");
			%client.player.schedule(strLen(%Message) * 35, playThread, 0, "root");
			%message = "<color:FFFF66>" @ %client.getSMMName() @ " \c6" @ %action @ ", '" @ %message @ "'";
			emitSMMMessage(%message, %client.miniGame, %client.player.position, %range);
		}
		else
		if (%channel == 3 ) {
			%action = "mumbles";
			%range = 10;
			%message = "<color:FFFF66>Unknown \c6" @ %action @ ", '" @ muffleSpeech(%message) @ "'";
			emitSMMMessage(%message, %client.miniGame, %client.corpse.position, %range);
		}
		else {
			%realName = %client.getPlayerName();
			%smmName = %client.getSMMName(true);

			if (%realName $= %smmName) {
				%name = %realName;
			}
			else {
				%name = %realName SPC "(" @ %smmName @ ")";
			}

			%message = "<color:AAAAAA>[" @ %channelName[%channel] @ "]" SPC %name @ ":" SPC %message;

			for (%i = 0; %i < ClientGroup.getCount(); %i++) {
				%current = ClientGroup.getObject(%i);

				if (%current.getSMMChatChannel() >= %channel && !%current.corpse.revive) {
					messageClient(%current, '', %message);
				}
			}
		}
	}

	function Item::fadeOut(%this) {
		Parent::fadeOut(%this);

		if (isObject(%this.spawnBrick) && %this.spawnBrick.getName() $= "_randomItemSpawn") {
			%this.spawnBrick.schedule(0, setItem, 0);
		}
	}

	function serverCmdTeamMessageSent(%client, %message) {
		if (!isObject(%client.miniGame.smmCore)) {
			Parent::serverCmdTeamMessageSent(%client, %message);
		}
	}

	function serverCmdStartTalking(%client) {
		if (!isObject(%client.miniGame.smmCore)) {
			Parent::serverCmdStartTalking(%client);
		}
	}

	function hl2DisplayAmmo(%this, %obj, %slot, %delay) {
		Parent::hl2DisplayAmmo(%this, %obj, %slot, %delay);
	}

	function GameConnection::bottomPrint(%a,%b,%c,%d,%e) {
		Parent::bottomPrint(%a,%b,%c,%d,%e);
	}

	function commandToClient(%client, %command, %a, %b, %c, %d, %e, %f, %g, %h,
		%i, %j, %k, %l, %m, %n, %o, %p, %q, %r, %s, %t, %u, %v, %w, %x, %y, %z
	) {
		Parent::commandToClient(%client, %command, %a, %b, %c, %d, %e, %f, %g, %h,
			%i, %j, %k, %l, %m, %n, %o, %p, %q, %r, %s, %t, %u, %v, %w, %x, %y, %z
		);
	}
};

activatePackage("SMMPackage");

function hl2DisplayAmmo(%this, %obj, %slot, %delay) {
	if (!$HL2Weapons::ShowAmmo) {
		return;
	}

	if (%delay == -1) {
		%obj.ammoString = "";

		if (isObject(%obj.client)) {
			%obj.client.updateSMMDisplay();
		}

		return;
	}

	if (%obj.toolAltMag[%this.item.altAmmoType] !$= "") {
		%str = %obj.toolAltMag[%this.item.altAmmoType] @ " ALT        ";
	}

	if ($HL2Weapons::Ammo == 0) {
		%str = %str @ %obj.toolMag[%obj.currTool] @ "/" @ %this.item.maxMag;
	}
	else {
		%str = %str @ %obj.toolMag[%obj.currTool] @ "/" @ %obj.toolAmmo[%this.item.ammoType];
	}

	%obj.ammoString = %str SPC "AMMO";

	if (isObject(%obj.client)) {
		%obj.client.updateSMMDisplay();
	}
}