$HL2Weapons::MaxAmmo["GoldenGun"] = 12;
$HL2Weapons::AddAmmo["GoldenGun"] = 6;

datablock ProjectileData(GoldenGunProjectile : ServicePistolProjectile) {
	directDamage = 100;
};

datablock ShapeBaseImageData(GoldenGunImage : ServicePistolImage) {
	shapeFile = ServicePistolImage.shapeFile;

	doColorShift = true;
	colorShiftColor = "1 0.9 0.1 1";

	item = GoldenGunItem;
	projectile = GoldenGunProjectile;

	armReady = true;
};

datablock ItemData(GoldenGunItem : ServicePistolItem) {
	image = GoldenGunImage;
	uiName = "Golden Gun";

	doColorShift = true;
	colorShiftColor = "1 0.9 0.1 1";

	maxMag = 1;
	ammoType = "GoldenGun";
};

function GoldenGunImage::onMount(%this, %obj, %slot) {
	ServicePistolImage::onMount(%this, %obj, %slot);
}

function GoldenGunImage::onUnMount(%this, %obj, %slot) {
	ServicePistolImage::onUnMount(%this, %obj, %slot);
}

function GoldenGunImage::onAmmoCheck(%this, %obj, %slot) {
	ServicePistolImage::onAmmoCheck(%this, %obj, %slot);
}

function GoldenGunImage::onReloadStart(%this, %obj, %slot) {
	ServicePistolImage::onReloadStart(%this, %obj, %slot);
}

function GoldenGunImage::onReload(%this, %obj, %slot) {
	ServicePistolImage::onReload(%this, %obj, %slot);
}

function GoldenGunImage::onEmpty(%this, %obj, %slot) {
	ServicePistolImage::onEmpty(%this, %obj, %slot);
}

function GoldenGunImage::onFire(%this, %obj, %slot) {
	ServicePistolImage::onFire(%this, %obj, %slot);
}