datablock staticShapeData(RopeShapeData) {
	shapeFile = "Add-Ons/GameMode_Super_Murder_Mystery/res/shapes/rope.dts";
};

function createRope(%a, %b) {
	%size = 0.1;

	%offset = vectorSub(%a, %b);
	%normal = vectorNormalize(%offset);

	%xyz = vectorNormalize(vectorCross("1 0 0", %normal));
	%pow = mRadToDeg(mACos(vectorDot("1 0 0", %normal))) * -1;

	%obj = new StaticShape() {
		datablock = RopeShapeData;
		scale = vectorScale(vectorLen(%offset) SPC %size SPC %size, 20);

		position = vectorScale(vectorAdd(%a, %b), 0.5);
		rotation = %xyz SPC %pow;
	};

	MissionCleanup.add(%obj);
	%obj.setNodeColor("ALL", "0 0 0 1");

	return %obj;
}