JumpSound.fileName = $SMM::Path @ "res/sounds/jump.wav";

datablock AudioDescription( AudioSilent3d : AudioClose3d )
{
	volume = 0.6;
	maxDistance = 15;
};

datablock AudioProfile(FallFatalSound) {
	fileName = $SMM::Path @ "res/sounds/fallFatal.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(FallInjurySound) {
	fileName = $SMM::Path @ "res/sounds/fallInjury.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(MaleDeathSound1) {
	fileName = $SMM::Path @ "res/sounds/vo/blockhead_death1.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(MaleDeathSound2) {
	fileName = $SMM::Path @ "res/sounds/vo/blockhead_death2.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(MaleDeathSound3) {
	fileName = $SMM::Path @ "res/sounds/vo/blockhead_death3.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(MalePainSound1) {
	fileName = $SMM::Path @ "res/sounds/vo/blockhead_pain1.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(MalePainSound2) {
	fileName = $SMM::Path @ "res/sounds/vo/blockhead_pain2.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(MalePainSound3) {
	fileName = $SMM::Path @ "res/sounds/vo/blockhead_pain3.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(MalePainSound4) {
	fileName = $SMM::Path @ "res/sounds/vo/blockhead_pain4.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock audioProfile( MaleYesSound )
{
	fileName = $SMM::Path @ "res/sounds/vo/blockhead_yes.wav";
	description = audioClosest3D;
	preload = true;
};

datablock audioProfile( MaleNoSound )
{
	fileName = $SMM::Path @ "res/sounds/vo/blockhead_no.wav";
	description = audioClosest3D;
	preload = true;
};

datablock audioProfile( MaleHelpSound )
{
	fileName = $SMM::Path @ "res/sounds/vo/blockhead_help.wav";
	description = audioClosest3D;
	preload = true;
};

datablock audioProfile( MaleThanksSound )
{
	fileName = $SMM::Path @ "res/sounds/vo/blockhead_thanks.wav";
	description = audioClosest3D;
	preload = true;
};

datablock audioProfile( MaleWtfSound1 )
{
	fileName = $SMM::Path @ "res/sounds/vo/blockhead_wtf1.wav";
	description = audioClosest3D;
	preload = true;
};

datablock audioProfile( MaleWtfSound2 )
{
	fileName = $SMM::Path @ "res/sounds/vo/blockhead_wtf2.wav";
	description = audioClosest3D;
	preload = true;
};

datablock AudioProfile(FemaleDeathSound1) {
	fileName = $SMM::Path @ "res/sounds/vo/femblockhead_death1.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(FemaleDeathSound2) {
	fileName = $SMM::Path @ "res/sounds/vo/femblockhead_death2.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(FemaleDeathSound3) {
	fileName = $SMM::Path @ "res/sounds/vo/femblockhead_death3.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(FemalePainSound1) {
	fileName = $SMM::Path @ "res/sounds/vo/femblockhead_pain1.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(FemalePainSound2) {
	fileName = $SMM::Path @ "res/sounds/vo/femblockhead_pain2.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(FemalePainSound3) {
	fileName = $SMM::Path @ "res/sounds/vo/femblockhead_pain3.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(FemalePainSound4) {
	fileName = $SMM::Path @ "res/sounds/vo/femblockhead_pain4.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock audioProfile( FemaleYesSound )
{
	fileName = $SMM::Path @ "res/sounds/vo/femblockhead_yes.wav";
	description = audioClosest3D;
	preload = true;
};

datablock audioProfile( FemaleNoSound )
{
	fileName = $SMM::Path @ "res/sounds/vo/femblockhead_no.wav";
	description = audioClosest3D;
	preload = true;
};

datablock audioProfile( FemaleHelpSound )
{
	fileName = $SMM::Path @ "res/sounds/vo/femblockhead_help.wav";
	description = audioClosest3D;
	preload = true;
};

datablock audioProfile( FemaleThanksSound )
{
	fileName = $SMM::Path @ "res/sounds/vo/femblockhead_thanks.wav";
	description = audioClosest3D;
	preload = true;
};

datablock audioProfile( FemaleWtfSound1 )
{
	fileName = $SMM::Path @ "res/sounds/vo/femblockhead_wtf1.wav";
	description = audioClosest3D;
	preload = true;
};

datablock audioProfile( FemaleWtfSound2 )
{
	fileName = $SMM::Path @ "res/sounds/vo/femblockhead_wtf2.wav";
	description = audioClosest3D;
	preload = true;
};

datablock AudioProfile(LandSound) {
	fileName = $SMM::Path @ "res/sounds/land.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(CorpseImpactSoftSound1) {
	fileName = $SMM::Path @ "res/sounds/physics/corpseImpactSoft1.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(CorpseImpactSoftSound2) {
	fileName = $SMM::Path @ "res/sounds/physics/corpseImpactSoft2.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(CorpseImpactSoftSound3) {
	fileName = $SMM::Path @ "res/sounds/physics/corpseImpactSoft3.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(CorpseImpactHardSound1) {
	fileName = $SMM::Path @ "res/sounds/physics/corpseImpactHard1.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(CorpseImpactHardSound2) {
	fileName = $SMM::Path @ "res/sounds/physics/corpseImpactHard2.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(CorpseImpactHardSound3) {
	fileName = $SMM::Path @ "res/sounds/physics/corpseImpactHard3.wav";
	description = AudioClosest3D;
	preload = true;
};

datablock AudioProfile(BloodSpillSound) {
	fileName = $SMM::Path @ "res/sounds/physics/blood_Spill.wav";
	description = AudioSilent3D;
	preload = true;
};

datablock AudioProfile(BloodDripSound1) {
	fileName = $SMM::Path @ "res/sounds/physics/blood_drip1.wav";
	description = AudioSilent3D;
	preload = true;
};
datablock AudioProfile(BloodDripSound2) {
	fileName = $SMM::Path @ "res/sounds/physics/blood_drip2.wav";
	description = AudioSilent3D;
	preload = true;
};
datablock AudioProfile(BloodDripSound3) {
	fileName = $SMM::Path @ "res/sounds/physics/blood_drip3.wav";
	description = AudioSilent3D;
	preload = true;
};

datablock audioProfile(bodyBagZipSound) {
	fileName = $SMM::Path @ "res/sounds/zip.wav";
	description = AudioClosest3D;
	preload = true;
};
datablock audioProfile(bodyBagUnzipSound) {
	fileName = $SMM::Path @ "res/sounds/unzip.wav";
	description = AudioClosest3D;
	preload = true;
};


datablock audioDescription(audioMusic2d : audioClose3D) {
	volume = 1.5;
};
datablock audioProfile(smmMafiaWinMusic) {
	fileName = $SMM::Path @ "res/sounds/music/mafiawins.ogg";
	description = audioMusic2d;
	preload = true;
};
datablock audioProfile(smmInnocentsWinMusic) {
	fileName = $SMM::Path @ "res/sounds/music/innocentswin.ogg";
	description = audioMusic2d;
	preload = true;
};
datablock audioProfile(smmRoundStartMusic) {
	fileName = $SMM::Path @ "res/sounds/music/roundstart.ogg";
	description = audioMusic2d;
	preload = true;
};
datablock audioProfile(smmDeathMusic) {
	fileName = $SMM::Path @ "res/sounds/music/dead.ogg";
	description = audioMusic2d;
	preload = true;
};
datablock audioProfile(smmStalemateMusic) {
	fileName = $SMM::Path @ "res/sounds/music/stalemate.ogg";
	description = audioMusic2d;
	preload = true;
};


datablock StaticShapeData(BulletHoleDecalData) {
	shapeFile = $SMM::Path @ "res/shapes/shot_decal.dts";

	doColorShift = true;
	colorShiftColor = "0.5 0.55 0.5 1";
};

datablock PlayerData(PlayerSMMArmor : PlayerStandardArmor) {
	uiName = "SMM Player";
	canJet = 0;

	minImpactSpeed = 19;
	speedDamageScale = 2.9;

	jumpDelay = 30;
	maxTools = 6;
	mass = 120;

	cameraTilt = 0;
	cameraMaxDist = 2;

	cameraHorizontalOffset = 0.5;
	cameraVerticalOffset = 1;

	cloakTexture = "base/data/specialfx/cloakTexture";
};

datablock PlayerData(PlayerSMMLimpingArmor : PlayerSMMArmor) {
	uiName = "";

	speedDamageScale = 3.0;

	jumpDelay = 40;
	mass = 122;

	maxForwardSpeed = 5;
	maxBackwardSpeed = 2;
	maxSideSpeed = 3;

	cloakTexture = "base/data/specialfx/cloakTexture";
};

datablock playerData(PlayerSMMFrozenArmor : PlayerSMMArmor) {
	uiName = "";
	maxForwardSpeed = 0;
	maxBackwardSpeed = 0;
	maxSideSpeed = 0;
	maxForwardCrouchSpeed = 0;
	maxSideCrouchSpeed = 0;
	maxBackwardCrouchSpeed = 0;
	jumpForce = 0;
	airControl = 0;
};

datablock WheeledVehicleData(CorpseVehicle) {
	shapeFile = "Add-Ons/Item_Skis/deathVehicle.dts";
	emap = true;

	isSled = true;
	maxDamage = 1;
	destroyedLevel = 1;
	maxSteeringAngle = 0.885;

	mass = 300;
	density = 0.5;

	massCenter = "0.0 -0.5 1.25";
	massBox = "1.25 1.25 2.65";

	drag = 0.6;
	bodyFriction = 1;
	bodyRestitution = 0.2;
	minImpactSpeed = 0;
	softImpactSpeed = 0;
	hardImpactSpeed = 0;
	integration = 4;
	collisionTol = 0.25;
	contactTol = 0.1;
	justcollided = 0;

	engineTorque = 0;
	engineBrake = 0;
	brakeTorque = 0;
	maxWheelSpeed = 0;

	forwardThrust = 0;
	reverseThrust = 0;
	lift = 0;
	maxForwardVel = 30;
	maxReverseVel = 30;
	horizontalSurfaceForce = 0;
	verticalSurfaceForce = 0; 
	rollForce = 0;
	yawForce = 0;
	pitchForce = 0;
	rotationalDrag = 5;
	stallSpeed = 0;

	runOverDamageScale = 0;
	runOverPushScale = 0;
};