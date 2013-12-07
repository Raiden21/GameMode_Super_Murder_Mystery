function loadSMMNames() {
	%pattern = $SMM::Path @ "res/names/*.txt";
	%fp = new FileObject();

	for (%file = findFirstFile(%pattern); %file !$= ""; %File = findNextFile(%pattern)) {
		%name = fileBase(%file);

		$SMM::NameCount[%name] = 0;
		%fp.openForRead(%file);

		while (!%fp.isEOF()) {
			$SMM::Name[%name, $SMM::NameCount[%name]] = %fp.readLine();
			$SMM::NameCount[%name]++;
		}
	}
}

function getSMMName(%name) {
	return $SMM::Name[%name, getRandom(0, $SMM::NameCount[%name] - 1)];
}

function GameConnection::giveSMMName(%this) {
	if (getRandom() < 0.15) {
		%this.smmFirst = "";
	}
	else {
		%this.smmFirst = getSMMName(%this.smmGender ? "first_female" : "first_male");
	}

	%this.smmLast = getSMMName("last");
}

function GameConnection::getSMMName(%this, %looped) {
	if (!%looped && isObject(%this.smmDisguiseTarget) && %this.smmDisguiseTarget != %this) {
		return %this.smmDisguiseTarget.getSMMName(true);
	}

	if (%this.smmFirst !$= "") {
		%name = %this.smmFirst;
	}

	if (%this.smmLast !$= "") {
		if (%name $= "") {
			%name = %this.smmGender ? "Mrs." : "Mr.";
		}

		%name = %name SPC %this.smmLast;
	}

	if (%name !$= "") {
		return %name;
	}

	return %this.getPlayerName();
}

loadSMMNames();