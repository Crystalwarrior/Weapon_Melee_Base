RTB_registerPref("Durability", "Medieval Weapons", "$MeleeWeapons::Durability", "bool 0 1", "Weapon_Melee_Base", 1, 0, 0);

exec("./debug/init.cs");
exec("./sounds.cs");
exec("./effects.cs");
exec("./support_decMaxSpeeds.cs");
exec("./itemprops.cs");

exec("./weapon_base.cs");

//Weapon types, sorted by length
//Daggers
exec("./weapon_dagger.cs");
//Swords
exec("./weapon_shortsword.cs");
exec("./weapon_longsword.cs");
exec("./weapon_claymore.cs");
exec("./weapon_zweihander.cs");
//Axes
exec("./weapon_axe.cs");
exec("./weapon_battleaxe.cs");
exec("./weapon_doubleaxe.cs");
//Polearms
exec("./weapon_swordstaff.cs");
exec("./weapon_halberd.cs");
exec("./weapon_spear.cs");
exec("./weapon_pike.cs");
//Blunt
exec("./weapon_warhammer.cs");
exec("./weapon_mace.cs");
//PEASANT
exec("./weapon_pickaxe.cs");
exec("./weapon_pitchfork.cs");
exec("./weapon_scythe.cs");
exec("./weapon_cudgel.cs");
//Weirdness
exec("./lightsaber/server.cs");

function MeleeWeaponProps::onAdd(%this)
{
	%this.durability = %this.sourceItemData.durability;
}