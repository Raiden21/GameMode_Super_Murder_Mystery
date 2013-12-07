datablock ShapeBaseImageData(SuitcaseKeyImage) {
	shapeFile = "Add-Ons/Item_Key/keya.dts";

	doColorShift = true;
	colorShiftColor = "0.75 0.75 0.75 1";

	item = SuitcaseKeyItem;
	armReady = true;
};

datablock ItemData(SuitcaseKeyItem) {
	image = SuitcaseKeyImage;
	shapeFile = "Add-Ons/Item_Key/keya.dts";

	doColorShift = true;
	colorShiftColor = "0.75 0.75 0.75 1";

	uiName = "Suitcase Key";
	iconName = "Add-Ons/Item_Key/Icon_KeyA";
	canDrop = true;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	isSuitcaseKey = true;
};