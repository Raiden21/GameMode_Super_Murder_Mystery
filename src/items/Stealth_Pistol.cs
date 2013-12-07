$HL2Weapons::MaxAmmo["StealthPistol"] = 24;
$HL2Weapons::AddAmmo["StealthPistol"] = 12;

datablock ProjectileData(StealthPistolProjectile : ServicePistolProjectile) {
	directDamage = 50;
	headshotMultiplier = 2;
};

datablock ShapeBaseImageData(StealthPistolImage : ServicePistolImage) {
	shapeFile = "base/data/shapes/empty.dts";

	item = StealthPistolItem;
	projectile = StealthPistolProjectile;

	armReady = false;
	ignore = true;

	melee = true;
};

datablock ItemData(StealthPistolItem : ServicePistolItem) {
	image = StealthPistolImage;
	uiName = "Stealth Pistol";

	maxMag = 1;
	ammoType = "StealthPistol";
};

function StealthPistolImage::onMount(%this, %obj, %slot) {
	ServicePistolImage::onMount(%this, %obj, %slot);
	%obj.playThread(1, "root");
}

function StealthPistolImage::onUnMount(%this, %obj, %slot) {
	ServicePistolImage::onUnMount(%this, %obj, %slot);
}

function StealthPistolImage::onAmmoCheck(%this, %obj, %slot) {
	ServicePistolImage::onAmmoCheck(%this, %obj, %slot);
}

function StealthPistolImage::onReloadStart(%this, %obj, %slot) {
	ServicePistolImage::onReloadStart(%this, %obj, %slot);
}

function StealthPistolImage::onReload(%this, %obj, %slot) {
	ServicePistolImage::onReload(%this, %obj, %slot);
}

function StealthPistolImage::onEmpty(%this, %obj, %slot) {
	ServicePistolImage::onEmpty(%this, %obj, %slot);
}

function StealthPistolImage::onFire(%this, %obj, %slot) {
	%obj.playThread(1, "armReadyRight");
	ServicePistolImage::onFire(%this, %obj, %slot);
	%obj.schedule(100, playThread, 1, "root");
}

function StealthPistolProjectile::damage(%this, %obj, %col, %fade, %pos, %normal) {
	%hasBeenUnconscious = %col.hasBeenUnconscious;
	%col.hasBeenUnconscious = true;

	ServicePistolProjectile::damage(%this, %obj, %col, %fade, %pos, %normal);
	%col.hasBeenUnconscious = %hasBeenUnconscious;
}