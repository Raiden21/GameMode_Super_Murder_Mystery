function GameConnection::updateSMMDisplay(%this) {
	cancel(%this.updateSMMDisplay);
	%time = "";

	if (!isObject(%this.miniGame.smmCore)) {// || %this.getControlObject().getClassName() $= "Camera") {
		commandToClient(%this, 'SetVignette', $EnvGuiServer::VignetteMultiply, $EnvGuiServer::VignetteColor);
		return;
	}

	%text = "";
	%color = $EnvGuiServer::VignetteColor;

	if (isObject(%this.player) && %this.player.getState() !$= "Dead") {
		%color = blendRGBA(%color, "1 0 0" SPC %this.player.getDamagePercent() / 2);
		%text = "<color:FFFF66>" @ %this.getSMMName() @ " ";

		if (%this.isMafia) {
			%text = %text @ "(<color:FF6666>Mafia<color:FFFF66>)";
		}
		else {
			%text = %text @ "(<color:66FF66>Innocent<color:FFFF66>)";
		}

		if (%this.player.ammoString !$= "") {
			%text = %text @ "<just:right><color:6666FF>" @ %this.player.ammoString @ " ";
		}

		if (%this.isMafia) {
			%this.player.doMafiaLongRangeDisplay();
			%time = 200;
		}
		else {
			%time = 500;
		}
	}
	else if (%this.corpse.inUnconscious) {
		%factor = %this.corpse.corpseDamageLevel / (%this.getDataBlock().maxDamage * 5);
		%color = blendRGBA(%color, "0 0 0" SPC %factor);
	}

	commandToClient(%this, 'SetVignette', $EnvGuiServer::VignetteMultiply, %color);
	commandToClient(%this, 'BottomPrint', %text @ "\n", 0, 1);

	if (%time !$= "") {
		%this.updateSMMDisplay = %this.schedule(%time, updateSMMDisplay);
	}
}

function Player::activateSMMObject(%this, %col, %stuff) {
	if (!isObject(%this.client)) {
		return;
	}

	if (%col.isCorpse) {
		if (%col.isWrapped) {
			%message = "\c6There's a body in the bodybag. \c3[Rightclick to unzip it]\n";
		}
		else if (%col.isUnconscious) {
			if (isObject(%col.originalClient)) {
				%message = "\c6This is \c3" @ %col.originalClient.getSMMName() @ "\c6.\n";
			}

			%message = %message @ "\c6They're unconscious.";

			if (%this.client.isMafia) {
				if (%col.originalClient.isMafia) {
					%message = %message @ "\n<color:FF6666>(Fellow Mafia)";
				}
				else {
					%message = %message @ "\n<color:66FF66>(Innocent)";
				}
			}
		}
		else {
			%message = "\c6They're dead.";
		}
	}
	else {
		if (isObject(%col.client)) {
			%message = "\c6This is \c3" @ %col.client.getSMMName() @ "\c6.";
		}

		%mount = %col.getMountedImage(0);

		if (isObject(%mount) && %mount.item.image.getID() == %mount && %mount.item.uiName !$= "" && !%mount.ignore) {
			if (%message !$= "") {
				%message = %message @ "\n";
			}

			if (%mount.item.smmReplaceUIName $= "") {
				%uiName = %mount.item.uiName;
			}
			else {
				%uiName = %mount.item.smmReplaceUIName;
			}

			%message = %message @ "\c6They are wielding a(n) \c3" @ %uiName @ "\c6.";
		}

		if (%this.client.isMafia) {
			if (%col.client.isMafia) {
				%message = %message @ "\n<color:FF6666>(Fellow Mafia)";
			}
			else {
				%message = %message @ "\n<color:66FF66>(Innocent)";
			}
		}
	}

	if (%message !$= "") {
		%this.client.centerPrint(%message, %stuff ? 3 : 0.4);
	}
}

function Player::doMafiaLongRangeDisplay(%this) {
	if (%this.shouldActivateWithMuzzle()) {
		%eyePoint = %this.getMuzzlePoint(0);
		%eyeVector = %this.getMuzzleVector(0);
	}
	else {
		%eyePoint = %this.getEyePoint();
		%eyeVector = %this.getEyeVector();
	}

	%ray = containerRayCast(%eyePoint,
		vectorAdd(%eyePoint, vectorScale(%eyeVector, 64)),
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

	%this.activateSMMObject(%col);
}

function Player::shouldActivateWithMuzzle(%this) {
	return %this.isFirstPerson() && isObject(%this.getMountedImage(0));
}