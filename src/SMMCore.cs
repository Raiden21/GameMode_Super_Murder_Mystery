$SUDDEN_DEATH_KILLS_MAFIAS_ONLY = true;

function SMMCore(%miniGame) {
	if (!isObject(%miniGame)) {
		error("ERROR: Invalid mini-game specified.");
		return;
	}

	return new ScriptObject() {
		class = SMMCore;
		miniGame = %miniGame;
	};
}

function SMMCore::onAdd(%this) {
	%this.kills = 0;
	%this.start = $Sim::Time;

	%this.placeRandomItems();
	%this.resetSuddenDeath();

	%count = mCeil(%this.miniGame.numMembers / 5);

	if (isObject(DayCycle)) {
		%time = $Sim::Time / DayCycle.dayLength;
		DayCycle.setDayOffset(0 - (%time - mCeil(%time)));
	}

	for (%i = 0; %i < %this.miniGame.numMembers; %i++) {
		%this.miniGame.member[%i].isMafia = false;
		%choices = %choices @ (%i ? " " : "") @ %this.miniGame.member[%i];
	}

	for (%i = 0; %i < %count && %choices !$= ""; %i++) {
		%clientIndex = getRandom(0, getWordCount(%choices) - 1);
		%client = getWord(%choices, %clientIndex);

		%client.isMafia = true;
		%choices = trim(strReplace(setWord(%choices, %clientIndex, ""), "  ", " "));
	}

	%message[0] = "<color:66FF66>You are innocent. <color:6666FF>Bring down the mafia!";
	%message[1] = "<color:FF6666>You are a part of the mafia. <color:6666FF>Kill all the innocents without being discovered.";
	%message[2] = "<color:FF6666>Your fellow mafias are: ";

	for (%i = 0; %i < %this.miniGame.numMembers; %i++) {
		%client = %this.miniGame.member[%i];
		%client.play2D(smmRoundStartMusic);
		%client.updateSMMDisplay();

		messageClient(%client, '', %message[%client.isMafia ? 1 : 0]);

		if (%client.isMafia) {
			%list = "";

			for (%j = 0; %j < %this.miniGame.numMembers; %j++) {
				if (%j != %i && %this.miniGame.member[%j].isMafia) {
					%list = %list @ (%list $= "" ? "" : "\t") @ %this.miniGame.member[%j].getSMMName();
				}
			}

			if (%list !$= "") {
				messageClient(%client, '', %message[2] @ naturalGrammarList(%list));
			}
		}
	}
}

function SMMCore::onRemove(%this) {
	for (%i = 0; %i < %this.miniGame.numMembers; %i++) {
		%this.miniGame.member[%i].isMafia = "";
	}
}

function SMMCore::addKill(%this, %a, %b) {
	if (!isObject(%b)) {
		return;
	}

	if (!isObject(%a) || %a == %b) {
		%a = -1;
	}

	%this.kill[%this.kills] = %a SPC %b;
	%this.kills++;
}

function SMMCore::end(%this, %message, %music) {
	cancel(%this.suddenDeathSchedule);

	if (%this.ended) {
		return;
	}

	if (%music $= ""){
		%music = smmStalemateMusic;
	}

	if (%message $= "") {
		%message = "The round is over.";
	}

	%message = "<color:6666FF>" @ %message SPC "A new round will start in 10 seconds.";
	%message1 = "<color:6666FF>This round lasted" SPC getTimeString(mCeil($Sim::Time - %this.start));
	%message1 = %message1 @ ", and involved" SPC %this.kills SPC "kill" @ (%this.kills == 1 ? "" : "s") @ ".";
	%this.ended = true;

	for (%i = 0; %i < %this.miniGame.numMembers; %i++) {
		%this.miniGame.member[%i].play2D(%music);
		messageClient(%this.miniGame.member[%i], '', %message);
		messageClient(%this.miniGame.member[%i], '', %message1);

		if (%this.miniGame.member[%i].isMafia) {
			%list = %list @ (%list $= "" ? "" : "\t") @ %this.miniGame.member[%i].getSMMName();
		}
	}

	if (%list $= "") {
		%message = "<color:6666FF>There were no mafias.";
	}
	else if (getFieldCount(%list) < 2) {
		%message = "<color:FF6666>" @ %list SPC "<color:6666FF>was the mafia.";
	}
	else {
		%list = strReplace(%list, "\t", "<color:6666FF>\t<color:FF6666>");
		%message = "<color:FF6666>" @ naturalGrammarList(%list) SPC "<color:6666FF>were the mafias.";
	}

	for (%i = 0; %i < %this.miniGame.numMembers; %i++) {
		messageClient(%this.miniGame.member[%i], '', %message);
	}

	%this.miniGame.resetSchedule = %this.miniGame.schedule(10000, reset, 0);
}

function SMMCore::placeRandomItems(%this) {
	%name = "_randomItemSpawn";

	%nameCount = BrickGroup_888888.NTObjectCount[%name];
	//%spawnCount = getRandom(mCeil(%nameCount * 0.2), mCeil(%nameCount * 0.5));
	%spawnCount = %nameCount;

	for (%i = 0; %i < %nameCount; %i++) {
		BrickGroup_888888.NTObject[%name, %i].setItem(0);
		%bricks = %bricks @ (%i ? " " : "") @ BrickGroup_888888.NTObject[%name, %i];
	}

	for (%i = 0; %i < %spawnCount && %bricks !$= ""; %i++) {
		%index = getRandom(0, getWordCount(%bricks) - 1);

		%brick = getWord(%bricks, %index);
		%bricks = trim(strReplace(setWord(%bricks, %index, ""), "  ", " "));

		if (isObject(%brick)) {
			//if (getRandom() >= 0.33) {
			if (1) {
				%brick.setItem(getRandom() < 0.25 ? LockedSuitcaseItem : SuitcaseItem);
			}
			else {
				%choices = "suitcaseKeyItem papersItem paintItem AutomaticPistolItem";
				%choices = %choices SPC "MeleeCaneItem MeleeKnifeItem MeleeUmbrellaItem MeleeWrenchItem MeleePanItem";
				%choices = %choices SPC "bodybagItem taserItem MedicineItem LighterItem AmmoSupplyItem flashlightItem";
				%choice = getWord(%choices, getRandom(0, getWordCount(%choices) - 1));
				%choice = isObject(%choice) ? %choice : SuitcaseItem;
				%brick.setItem(%choice);
			}
		}
	}
}

function SMMCore::resetSuddenDeath(%this, %killed) {
	cancel(%this.suddenDeathSchedule);

	if (%this.ended) {
		return;
	}

	if (%killed && getTimeRemaining(%this.suddenDeathSchedule) >= 120000) {
		return;
	}

	%this.suddenDeathSchedule = %this.schedule(%killed ? 120000 : 600000, suddenDeathWarning);
}

function SMMCore::suddenDeathWarning(%this) {
	if (%this.ended) {
		return;
	}

	if ($SUDDEN_DEATH_KILLS_MAFIAS_ONLY) {
		messageAll('', "<color:6666FF>Sudden Death will begin in 1 minute. During Sudden Death, a random mafia will die every minute.");
	}
	else {
		messageAll('', "<color:6666FF>Sudden Death will begin in 1 minute. During Sudden Death, a random player will die every minute.");
	}

	messageAll('', "<color:6666FF>When <color:FF6666>a Mafia <color:6666FF>kills a player, the timer will go up to 3 minutes.");
	%this.suddenDeathSchedule = %this.schedule(60000, suddenDeathTick);
}

function SMMCore::suddenDeathTick(%this) {
	cancel(%this.suddenDeathSchedule);

	if (%this.ended) {
		return;
	}

	for (%i = 0; %i < %this.miniGame.numMembers; %i++) {
		%client = %this.miniGame.member[%i];
		%player = %client.player;

		if (isObject(%player) && %player.getState() !$= "Dead") {
			if (!$SUDDEN_DEATH_KILLS_MAFIAS_ONLY || %client.isMafia) {
				%choices = %choices @ (%choices $= "" ? "" : " ") @ %player;
			}
		}
	}

	if (%choices !$= "") {
		%choice = getWord(%choices, getRandom(0, getWordCount(%choices) - 1));

		if ($SUDDEN_DEATH_KILLS_MAFIAS_ONLY) {
			messageAll('', "<color:6666FF>A random mafia has found it all too tense and had a heart attack.");
		}
		else {
			messageAll('', "<color:6666FF>Somebody was too scared of the mafia and had a heart attack.");
		}

		%choice.hasBeenUnconscious = true;
		%choice.kill();
	}

	%this.suddenDeathSchedule = %this.schedule(60000, suddenDeathTick);
}