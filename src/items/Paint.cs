$HL2Weapons::AddAmmo["Blood Paint"] = 80;
$HL2Weapons::MaxAmmo["Blood Paint"] = 80;

// Sounds
datablock audioProfile(altSpraySound)
{
	fileName =  $SMM::Path @ "res/sounds/sprayLoop.wav";
	description = AudioClosestLooping3d;
	preload = true;
};

// Standard
datablock ParticleData(paintTrailParticle : blinkPaintParticle)
{
	textureName		= $SMM::Path @ "res/shapes/blur";

	dragCoefficient			= 3.0;
	windCoefficient			= 0.0;
	gravityCoefficient		= 0.0;
	inheritedVelFactor		= 0.0;
	constantAcceleration	= 0.0;
	lifetimeMS				= 420;
	lifetimeVarianceMS		= 0;
	spinSpeed				= 0.0;
	spinRandomMin			= -50.0;
	spinRandomMax			= 50.0;
	useInvAlpha				= false;
	animateTexture			= false;

	// Interpolation variables
	colors[0]	= "0.9 0.2 0.2 1";
	colors[1]	= "0.8 0.2 0.2 1";
	colors[2]	= "0.75 0.2 0.2 0.4";

	sizes[0]	= "0.1";
	sizes[1]	= "0.5";
	sizes[2]	= "0.3";
	sizes[3]	= "0.1";

	times[0]	= "0";
	times[1]	= "0.5";
	times[2]	= "0.9";
	times[3]	= "1";
};

datablock ParticleEmitterData(paintTrailEmitter)
{
   ejectionPeriodMS = 2;
   periodVarianceMS = 0;

   ejectionVelocity = 40;
   velocityVariance = 0;

   ejectionOffset = 0;

   thetaMin         = 0;
   thetaMax         = 3;  

   phiReferenceVel = 0;
   phiVariance = 360;

   particles = paintTrailParticle;
};

datablock ItemData(PaintItem) {
	image = PaintImage;
	shapeFile = "base/data/shapes/spraycan.dts";

	doColorShift = true;
	colorShiftColor = "0.75 0.2 0.2 1";

	uiName = "Paint";
	canDrop = true;

	reload = true;
	ammoType = "Blood Paint";
	maxMag = 32;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

datablock ShapeBaseImageData(PaintImage)
{
	shapeFile = "base/data/shapes/spraycan.dts";
	emap = false;

	mountPoint = 0;
	offset = "0 0 0";
	eyeOffset = "0.7 1.2 -0.55";
	eyeRotation = "1 0 0 0";
	rotation = "1 0 0 0";

	doColorShift = true;
	colorShiftColor = "0.75 0.2 0.2 1";

	correctMuzzleVector = true;

	className = "WeaponImage";

	item = paintItem;
	ammo = " ";
	projectile = "";
	projectileType = projectile;
	melee = false;
	armReady = true;

	doColorShift = true;
	colorShiftColor = "0.75 0.2 0.2 1";

	stateName[0] = "Activate";
	stateTimeoutValue[0] = 0.4;
	stateWaitForTimeout[0] = false;
	stateAllowImageChange[0] = true;
	stateTransitionOnTimeout[0] = "CapOff";
	stateSequence[0] = "Shake";
	//stateSound[0] = "sprayActivateSound";

	stateName[1] = "Ready";
	stateTransitionOnTriggerDown[1] = "Fire";
	stateAllowImageChange[1] = true;
	stateTransitionOnNoAmmo[1] = "Empty";

	stateName[2] = "Fire";
	stateFire[2] = true;
	stateScript[2] = "onFire";
	stateTimeoutValue[2] = 0.04;
	stateWaitForTimeout[2] = true;
	stateAllowImageChange[2] = false;
	stateTransitionOnTimeout[2] = "Fire";
	stateTransitionOnTriggerUp[2] = "StopFire";
	stateSequence[2] = "fire";
	stateSound[2] = "altSpraySound";
	stateEmitterTime[2] = "0.06";
	stateEmitter[2] = "paintTrailEmitter";
	stateTransitionOnNoAmmo[2] = "Empty";

	stateName[3] = "StopFire";
	stateWaitForTimeout[3] = true;
	stateAllowImageChange[3] = false;
	stateTransitionOnTimeout[3] = "Ready";
	stateTransitionOnTriggerDown[3] = "Fire";
	stateSequence[3] = "StopFire";
	stateTransitionOnNoAmmo[3] = "Empty";

	stateName[4] = "CapOff";
	stateTimeoutValue[4] = 0.2;
	stateWaitForTimeout[4] = true;
	stateAllowImageChange[4] = true;
	stateTransitionOnTimeout[4] = "Ready";
	stateTransitionOnNoAmmo[4]	= "Empty";
	stateSequence[4] = "CapOff";

	stateName[5] = "Empty";
	stateWaitForTimeout[5] = false;
	stateAllowImageChange[5] = true;
	stateTransitionOnAmmo[5] = "Activate";
};

function PaintImage::onMount(%this, %obj, %slot) {
	hl2AmmoCheck(%this, %obj, %slot);
	hl2DisplayAmmo(%this, %obj, %slot);
}

function PaintImage::onUnMount(%this, %obj, %slot) {
	hl2DisplayAmmo(%this, %obj, %slot, -1);
}

function PaintImage::onFire(%this, %obj, %slot) {
	if (!%obj.toolMag[%obj.currTool]) {
		%obj.setImageAmmo(0, 0);
		return;
	}
	else {
		%obj.setImageAmmo(0, 1);
	}

	%point = %obj.getMuzzlePoint(%slot);
	%vector = %obj.getMuzzleVector(%slot);

	%ray = containerRayCast(%point,
		vectorAdd(%point, vectorScale(%vector, 15)),
		$TypeMasks::FxBrickObjectType,
		%obj
	);

	%decal = spawnDecalFromRayCast(BloodDecal @ getRandom(1, 2), %ray, 0.4 + getRandom() * 0.85);

	if (isObject(%decal)) {
		%decal.setNodeColor("ALL", "0.8 0.2 0.2 1");

		// if(vectorDot("0 0 -1", %decal.normal) >= 0.5 && !isEventPending(%decal.ceilingBloodSchedule)) {
		// 	if(getRandom(0, 3) == 3) 
		// 	{
		// 		%decal.ceilingBloodSchedule = schedule(getRandom(16, 500), 0, ceilingBloodLoop, %decal, getWords(%ray, 1, 3));
		// 	}
		// }
		// 2silly

		%obj.toolMag[%obj.currTool]--;
		hl2DisplayAmmo(%this, %obj, %slot);
	}
}