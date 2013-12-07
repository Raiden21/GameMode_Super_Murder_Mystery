$FS::CheckInterval = 50;
$FS::DefaultMaterial = "hardboot_generic";

function getNumberStart( %str )
{
	%best = -1;

	for ( %i = 0 ; %i < 10 ; %i++ )
	{
		%pos = strPos( %str, %i );

		if ( %pos < 0 )
		{
			continue;
		}

		if ( %best == -1 || %pos < %best )
		{
			%best = %pos;
		}
	}

	return %best;
}

function loadFootstepSounds()
{
	%pattern = $SMM::Path @ "res/sounds/footsteps/*.wav";

	deleteVariables( "$FS::Sound*" );
	$FS::SoundNum = 0;

	for ( %file = findFirstFile( %pattern ) ; %file !$= "" ; %file = findNextFile( %pattern ) )
	{
		%base = fileBase( %file );
		%name = "footstepSound_" @ %base;

		if ( !isObject( %name ) )
		{
			datablock audioProfile( genericFootstepSound )
			{
				description = "audioClosest3D";
				fileName = %file;
				preload = true;
			};

			if ( !isObject( %obj = nameToID( "genericFootstepSound" ) ) )
			{
				continue;
			}

			%obj.setName( %name );
		}

		if ( ( %pos = getNumberStart( %base ) ) > 0 )
		{
			%pre = getSubStr( %base, 0, %pos );
			%post = getSubStr( %base, %pos, strLen( %base ) );

			if ( $FS::SoundCount[ %pre ] < %post )
			{
				$FS::SoundCount[ %pre ] = %post;
			}

			$FS::SoundName[ $FS::SoundNum ] = %pre;
			$FS::SoundIndex[ %pre ] = $FS::SoundNum;
			$FS::SoundNum++;
		}
	}
}

function playFootstep( %pos, %material )
{
	if ( !strLen( %material ) || $FS::SoundCount[ %material ] < 1 )
	{
		return;
	}

	if ( !isObject( %sound = nameToID( "footstepSound_" @ %material @ getRandom( 1, $FS::SoundCount[ %material ] ) ) ) )
	{
		return;
	}

	serverPlay3D( %sound, %pos );
}

function Player::fs_velocityCheckTick(%this) {
	cancel(%this.fs_velocityCheckTick);
	%vz = getWord(%this.getVelocity(), 2);

	if (%this.lastVZ < -6 && %vz >= -0.75) {
		serverPlay3D(LandSound, %this.getPosition());
	}

	%this.lastVZ = %vz;

	if (vectorLen(%this.getVelocity()) >= 2.62 && !isEventPending(%this.fs_playTick)) {
		%this.fs_playTick = %this.schedule(100, "fs_playTick");
	}

	%this.fs_velocityCheckTick = %this.schedule($FS::CheckInterval, "fs_velocityCheckTick");
}

function player::fs_playTick(%this) {
	cancel(%this.fs_playTick);

	if (vectorLen(%this.getVelocity()) < %this.getDatablock().maxForwardSpeed / 2.67) {
		return;
	}

	if (%this.fs_trigger[3] || %this.fs_trigger[4] || %this.isCorpse) {
		return;
	}

	if (mAbs(getWord(%this.getVelocity(), 2)) < 0.01) {
		playFootstep(%this.position, $FS::DefaultMaterial);
	}

	%this.fs_playTick = %this.schedule(290, "fs_playTick");
}

loadFootstepSounds();