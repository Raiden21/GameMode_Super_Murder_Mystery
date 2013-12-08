if ($GameModeArg !$= "Add-Ons/GameMode_Super_Murder_Mystery/gamemode.txt") {
	error("ERROR: GameMode_Super_Murder_Mystery should not be used in custom games.");
	return;
}

$SMM::Path = "Add-Ons/GameMode_Super_Murder_Mystery/";

exec("./lib/decals.cs");
exec("./src/datablocks.cs");
exec("./src/misc.cs");
exec("./src/replace.cs");

exec("./src/items/Suitcase.cs");
exec("./src/items/Suitcase_Key.cs");
exec("./src/items/Stealth_Pistol.cs");
exec("./src/items/Golden_Gun.cs");
exec("./src/items/Medicine.cs");
exec("./src/items/Disguise.cs");
exec("./src/items/Cloak.cs");
exec("./src/items/False_Corpse.cs");
exec("./src/items/Recovery_Device.cs");
exec("./src/items/Time_Bomb.cs");
exec("./src/items/Paint.cs");
exec("./src/items/Ammo_Supply.cs");
exec("./src/items/Papers.cs");
exec("./src/items/Lighter.cs");
exec("./src/items/Bodybag.cs");
exec("./src/items/Flashlight.cs");
exec("./src/items/Hookshot.cs");

exec("./src/SMMCore.cs");
exec("./src/package.cs");
exec("./src/names.cs");
exec("./src/footsteps.cs");
exec("./src/damage.cs");
exec("./src/blood.cs");
exec("./src/display.cs");
exec("./src/commands.cs");