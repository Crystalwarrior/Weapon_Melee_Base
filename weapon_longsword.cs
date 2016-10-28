datablock ItemData(LongSwordItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	// Basic Item Properties
	shapeFile = "./longsword.dts";
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Longsword";
	iconName = "./icon_sword";
	doColorShift = true;
	colorShiftColor = "0.471 0.471 0.471 1.000";

	// Dynamic properties defined by the scripts
	image = LongSwordImage;
	canDrop = true;
};

////////////////
//weapon image//
////////////////
datablock ShapeBaseImageData(LongSwordImage)
{
	// Basic Item properties
	shapeFile = "./longsword.dts";
	emap = true;

	// Specify mount point & offset for 3rd person, and eye offset
	// for first person rendering.
	mountPoint = 0;
	offset = "0 0 0";

	// When firing from a point offset from the eye, muzzle correction
	// will adjust the muzzle vector to point to the eye LOS point.
	// Since this weapon doesn't actually fire from the muzzle point,
	// we need to turn this off.
	correctMuzzleVector = false;

	eyeOffset = "0 0 0";

	// Add the WeaponImage namespace as a parent, WeaponImage namespace
	// provides some hooks into the inventory system.
	className = "WeaponImage";

	// Projectile && Ammo.
	item = LongSwordItem;
	ammo = " ";
	projectile = MeleeSharpProjectile;
	projectileType = Projectile;

	//melee particles shoot from eye node for consistancy
	melee = true;

	//Special melee hitreg system
	directDamage = 30;

	meleeEnabled = true;
	meleeStances = true; //Use stance system?
	meleeCanClash = true; //If stances are enabled, can it clash?
	meleeTick = 24; //The speed of schedule loop in MS. Change this to animation FPS

	meleeRayLength = 2;

	meleeHitProjectile = MeleeSharpProjectile;
	meleeBlockedProjectile = MeleeBlockProjectile;
	meleeHitPlayerProjectile = SwordBloodProjectile;

	meleePierceTerrain = false; //If we hit terrain hitreg will still go on until it hits a player
	meleeSingleHitProjectile = false; //If pierce terrain is on, set this to true so it doesn't spam hit projectiles

	meleeBlockedVelocity = 7;
	meleeBlockedStunTime = 0.600; //Length of stun in seconds

	//raise your arm up or not
	armReady = false;

	//casing = " ";
	doColorShift = true;
	colorShiftColor = "0.471 0.471 0.471 1.000";

	// Images have a state system which controls how the animations
	// are run, which sounds are played, script callbacks, etc. This
	// state system is downloaded to the client so that clients can
	// predict state changes and animate accordingly.  The following
	// system supports basic ready->fire->reload transitions as
	// well as a no-ammo->dryfire idle state.

	// Initial start up state
	stateName[0]                     = "Activate";
	stateTimeoutValue[0]             = 0.5;
	stateTransitionOnTimeout[0]      = "Ready";
	stateSound[0]                    = SwordDrawSound;

	stateName[1]                     = "Ready";
	stateTransitionOnTriggerDown[1]  = "Fire";
	stateAllowImageChange[1]         = true;
	stateTransitionOnNotLoaded[1]      = "noAmmo";

	stateName[2]                    = "Fire";
	stateTransitionOnTimeout[2]     = "StopFire";
	stateTimeoutValue[2]            = 0.34;
	stateFire[2]                    = true;
	stateAllowImageChange[2]        = false;
	stateScript[2]                  = "onFire";
	stateWaitForTimeout[2]		       = true;

	stateName[3]                    = "StopFire";
	stateTransitionOnTriggerUp[3]   = "Ready";
	stateAllowImageChange[3]        = false;
	stateScript[3]                  = "onStopFire";

	stateName[4]                    = "noAmmo";
	stateTransitionOnLoaded[4]        = "Ready";
	stateAllowImageChange[4]        = false;
	stateScript[4]                  = "onNoAmmo";
};

function LongSwordImage::onMount(%this, %obj, %slot)
{
	%obj.playThread(2, tswing @ (%obj.swingPhase + 1) % 2 + (%obj.meleeStance ? 3 : 1));
	%obj.schedule(32, stopThread, 2);
}

function LongSwordImage::onStanceSwitch(%this, %obj, %slot)
{
	if (%obj.getImageState(%slot) !$= "Ready") return;
	%obj.meleeStance = (%obj.meleeStance + 1) % 2;
	%obj.stopMeleeHitregLoop(%obj, %slot);
	%obj.playThread(2, tswing @ (%obj.swingPhase + 1) % 2 + (%obj.meleeStance ? 3 : 1));
	%obj.schedule(0, stopThread, 2);
	if (isobject(%client=%obj.client)) %client.centerPrint("\c4Switched to " @ (%obj.meleeStance ? "horizontal" : "vertical") @ " stance.", 2);
	%obj.playThread(3, plant);
	serverPlay3D(WeaponSwitchSound, %obj.getSlotTransform(0));
}

function LongSwordImage::onFire(%this, %obj, %slot)
{
	%obj.swingPhase = (%obj.swingPhase + 1) % 2;
	%obj.playthread(2, tswing @ %obj.swingPhase + (%obj.meleeStance ? 3 : 1));
	%this.MeleeHitregLoop(%obj, %slot, 12);
}