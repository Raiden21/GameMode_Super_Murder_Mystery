package SMMDamagePackage {
	function Armor::damage(%this, %obj, %src, %pos, %damage, %type) {
		%obj.lastDamagePos = %pos;
		%obj.lastDamageType = %type;
		createBloodExplosion(%pos, %obj.getVelocity(), %obj.getScale());

		if (%obj.isCorpse) {
			%obj.doDripBlood();

			for (%i = 0; %i < $Blood::CheckpointCount; %i++) {
				%threshold = (%obj.corpseDamageLevel / $Blood::CheckpointCount) * %i;
				if (%obj.corpseDamageLevel >= %threshold) continue;
				if (%obj.corpseDamageLevel + %damage < %threshold) break;
				%obj.doSplatterBlood("", 2);
				echo(%threshold);
			}
			%obj.corpseDamageLevel += %damage;

			if (%obj.isUnconscious) {
				if (%obj.corpseDamageLevel >= %this.maxDamage * 5) {
					%obj.isUnconscious = "";
					cancel(%obj.reviveCorpseTimer);

					if (isObject(%obj.originalClient)) {
						clearCenterPrint(%obj.originalClient);
					}
				}
				else {
					%factor = %obj.corpseDamageLevel / (%this.maxDamage * 5);
					%obj.reviveCorpseTimer(45 + mCeil(mClampF(%factor, 0, 1) * 45));
				}
			}

			if (isObject(%obj.client)) {
				%obj.client.updateSMMDisplay();
			}

			return;
		}
		
		if(%damage > %this.maxDamage) {
			%fatal = true;
		}

		if (%type == $DamageType::Fall) {
			if(%obj.isCrouched())
			{
				%damage = %damage * 3;
			}
			if(%fatal) {
				serverPlay3D(FallFatalSound, %obj.getPosition());
			}
			else {
				serverPlay3D(FallInjurySound, %obj.getPosition());
			}
			%obj.doDripBlood(1);
		}

		if(%fatal) {
			if(%obj.hasBeenUnconscious) {
				%obj.doSplatterBlood("", $Blood::CheckpointCount / 3);
			}
			else {
				%obj.doSplatterBlood("", getRandom(2, 4));
			}
		}

		Parent::damage(%this, %obj, %src, %pos, %damage, %type);

		%time = %obj.getDamagePercent() * 10;
		%time = mClampF(%time, 0, 10);

		%obj.startDrippingBlood(%time);

		if (isObject(%obj.client)) {
			%obj.client.updateSMMDisplay();
		}
	}
};

activatePackage("SMMDamagePackage");